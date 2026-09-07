using MeetingBooking.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace MeetingBooking.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.UserId).IsRequired().HasMaxLength(450); 
        builder.Property(b => b.Date).IsRequired();
        builder.Property(b => b.CreatedAtUtc).IsRequired();

        builder.HasIndex(b => new { b.ResourceId, b.TimeSlotId, b.Date })
            .IsUnique()
            .HasDatabaseName("IX_Bookings_Resource_TimeSlot_Date_Unique");

        builder.HasOne<Resource>()
            .WithMany()
            .HasForeignKey(b => b.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<TimeSlot>()
            .WithMany()
            .HasForeignKey(b => b.TimeSlotId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}