namespace MeetingBooking.Domain;
/// <summary>
/// A meeting room, e.g. a bookable resource. Name and capacity invariants
/// are enforced at construction/update time.
/// </summary>
public class Resource
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int Capacity { get; private set; }
    public bool IsActive { get; private set; }

    private Resource() { } // EF Core

    public Resource(string name, string? description, int capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Resource name is required.", nameof(name));
        }
            
        if (capacity <= 0)
        {
            throw new ArgumentException("Capacity must be positive.", nameof(capacity));
        }
    
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        Capacity = capacity;
        IsActive = true;
    }

    public void Update(string name, string? description, int capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Resource name is required.", nameof(name));
        }
        
        if (capacity <= 0)
        {
            throw new ArgumentException("Capacity must be positive.", nameof(capacity));
        }
          
        Name = name;
        Description = description;
        Capacity = capacity;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
}