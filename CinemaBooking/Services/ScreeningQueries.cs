using CinemaBooking.Data;
using CinemaBooking.Models;
using CinemaBooking.ViewModels;
using Microsoft.EntityFrameworkCore;
namespace CinemaBooking.Services;
public class ScreeningQueries(CinemaDbContext db)
{
    public IQueryable<ScreeningCard> Cards(IQueryable<Showtime> query) => query.AsNoTracking().OrderBy(x => x.StartAt).Select(x => new ScreeningCard {
        Id = x.Id, MovieTitle = x.Movie.Title, CinemaName = x.Cinema.Name, Address = x.Cinema.Address,
        StartAt = x.StartAt, Price = x.TicketPrice, Available = x.Capacity - x.Bookings.Sum(b => b.Quantity)
    });
    public Task<ScreeningCard?> Find(int id) => Cards(db.Showtimes.Where(x => x.Id == id)).SingleOrDefaultAsync();
}
