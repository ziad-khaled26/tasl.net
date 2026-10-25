using CinemaBooking.Data;
using CinemaBooking.Services;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
namespace CinemaBooking.Controllers;
public class MoviesController(CinemaDbContext db, ScreeningQueries screenings) : Controller
{
    public async Task<IActionResult> Index(MovieBrowse model)
    {
        var query = db.Movies.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(model.Search)) query = query.Where(x => x.Title.Contains(model.Search.Trim()));
        if (!string.IsNullOrWhiteSpace(model.Genre)) query = query.Where(x => x.Genre == model.Genre);
        if (model.CinemaId.HasValue) query = query.Where(x => x.Showtimes.Any(s => s.CinemaId == model.CinemaId && s.StartAt > CairoClock.Now));
        model.Movies = await query.OrderBy(x => x.Title).ToListAsync();
        model.Genres = await db.Movies.Select(x => x.Genre).Distinct().OrderBy(x => x).ToListAsync();
        model.Cinemas = await db.Cinemas.OrderBy(x => x.Name).Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToListAsync();
        return View(model);
    }
    public async Task<IActionResult> Details(int id)
    {
        var movie = await db.Movies.AsNoTracking().Include(x => x.MovieActors).ThenInclude(x => x.Actor).SingleOrDefaultAsync(x => x.Id == id);
        if (movie is null) return NotFound();
        return View(new MovieDetails { Movie = movie, Screenings = await screenings.Cards(db.Showtimes.Where(x => x.MovieId == id && x.StartAt > CairoClock.Now)).ToListAsync() });
    }
}
