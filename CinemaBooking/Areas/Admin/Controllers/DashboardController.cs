using CinemaBooking.Data;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CinemaBooking.Areas.Admin.Controllers;
public class DashboardController(CinemaDbContext db) : AdminController
{
    public async Task<IActionResult> Index() => View(new AdminDashboard {
        Cinemas = await db.Cinemas.CountAsync(), Movies = await db.Movies.CountAsync(), Actors = await db.Actors.CountAsync(),
        Bookings = await db.Bookings.CountAsync(), Tickets = await db.Bookings.SumAsync(x => x.Quantity), Revenue = await db.Bookings.SumAsync(x => x.TotalAmount),
        Recent = await db.Bookings.AsNoTracking().Include(x => x.Showtime).ThenInclude(x => x.Movie).OrderByDescending(x => x.CreatedUtc).Take(8).ToListAsync()
    });
}
