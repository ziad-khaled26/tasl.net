using CinemaBooking.Data;
using CinemaBooking.Services;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CinemaBooking.Controllers;
public class CinemasController(CinemaDbContext db, ScreeningQueries screenings) : Controller
{
    public async Task<IActionResult> Index() => View(await db.Cinemas.AsNoTracking().OrderBy(x => x.Name).ToListAsync());
    public async Task<IActionResult> Details(int id)
    {
        var cinema = await db.Cinemas.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (cinema is null) return NotFound();
        return View(new CinemaDetails { Cinema = cinema, Screenings = await screenings.Cards(db.Showtimes.Where(x => x.CinemaId == id && x.StartAt > CairoClock.Now)).ToListAsync() });
    }
}
