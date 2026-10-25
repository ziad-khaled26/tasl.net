using CinemaBooking.Models;
using Microsoft.EntityFrameworkCore;
namespace CinemaBooking.Data;
public class CinemaDbContext(DbContextOptions<CinemaDbContext> options) : DbContext(options)
{
    public DbSet<Cinema> Cinemas => Set<Cinema>();
    public DbSet<Actor> Actors => Set<Actor>();
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<MovieActor> MovieActors => Set<MovieActor>();
    public DbSet<Showtime> Showtimes => Set<Showtime>();
    public DbSet<Booking> Bookings => Set<Booking>();
    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<MovieActor>().HasKey(x => new { x.MovieId, x.ActorId });
        model.Entity<MovieActor>().HasOne(x => x.Movie).WithMany(x => x.MovieActors).HasForeignKey(x => x.MovieId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<MovieActor>().HasOne(x => x.Actor).WithMany(x => x.MovieActors).HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Showtime>().HasOne(x => x.Movie).WithMany(x => x.Showtimes).HasForeignKey(x => x.MovieId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Showtime>().HasOne(x => x.Cinema).WithMany(x => x.Showtimes).HasForeignKey(x => x.CinemaId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Booking>().HasOne(x => x.Showtime).WithMany(x => x.Bookings).HasForeignKey(x => x.ShowtimeId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Showtime>().Property(x => x.RowVersion).IsRowVersion();
        model.Entity<Showtime>().Property(x => x.TicketPrice).HasPrecision(10, 2);
        model.Entity<Booking>().Property(x => x.UnitPrice).HasPrecision(10, 2);
        model.Entity<Booking>().Property(x => x.TotalAmount).HasPrecision(12, 2);
        model.Entity<Booking>().HasIndex(x => x.Code).IsUnique();
        model.Entity<Booking>().HasIndex(x => x.SubmissionKey).IsUnique();
        model.Entity<Showtime>().HasIndex(x => new { x.StartAt, x.MovieId, x.CinemaId });
        model.Entity<Movie>().ToTable(t => t.HasCheckConstraint("CK_Movie_Duration", "[DurationMinutes] BETWEEN 1 AND 600"));
        model.Entity<Showtime>().ToTable(t => {
            t.HasCheckConstraint("CK_Showtime_Capacity", "[Capacity] BETWEEN 1 AND 10000");
            t.HasCheckConstraint("CK_Showtime_Price", "[TicketPrice] BETWEEN 1 AND 100000");
        });
        model.Entity<Booking>().ToTable(t => {
            t.HasCheckConstraint("CK_Booking_Quantity", "[Quantity] BETWEEN 1 AND 10");
            t.HasCheckConstraint("CK_Booking_Total", "[UnitPrice] > 0 AND [TotalAmount] = [UnitPrice] * [Quantity]");
        });
    }
}
