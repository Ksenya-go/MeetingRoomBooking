using FluentValidation;
using Mediator;
using FluentResults;
using System.Reflection;

namespace MeetingBooking.Application.Common.Behaviors;

public class ValidationBehavior<TMessage, TResponse> : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
    private readonly IEnumerable<IValidator<TMessage>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TMessage>> validators)
    {
        _validators = validators;
    }

    public async ValueTask<TResponse> Handle(
        TMessage message, MessageHandlerDelegate<TMessage, TResponse> next, CancellationToken 
        cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TMessage>(message);
            var failures = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, 
                cancellationToken))))
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                var errorMessage = string.Join("; ",
                    failures
                        .Select(f => f.ErrorMessage)
                        .Where(msg => !string.IsNullOrWhiteSpace(msg)));

                return BuildFailResult(errorMessage);
            }
        }

        return await next(message, cancellationToken);
    }

    private static TResponse BuildFailResult(string errorMessage)
    {
        if (typeof(TResponse) == typeof(Result))
        {
            return (TResponse)(object)Result.Fail(errorMessage);
        }
        var innerType = typeof(TResponse).GetGenericArguments()[0];
        var failMethod = typeof(Result)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(m => m.Name == nameof(Result.Fail)
                         && m.IsGenericMethodDefinition
                         && m.GetParameters().Length == 1
                         && m.GetParameters()[0].ParameterType == typeof(string));

        var closedMethod = failMethod.MakeGenericMethod(innerType);
        return (TResponse)closedMethod.Invoke(null, new object[] { errorMessage })!;
    }
}