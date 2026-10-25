using CinemaBooking.Data;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
namespace CinemaBooking.Areas.Admin.Controllers;
public class BookingsController(CinemaDbContext db) : AdminController
{
    public async Task<IActionResult> Index(BookingSearch model)
    {
        var query = db.Bookings.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(model.Search)) { var term = model.Search.Trim(); query = query.Where(x => x.Code.Contains(term) || x.CustomerName.Contains(term) || x.Phone.Contains(term) || x.Showtime.Movie.Title.Contains(term)); }
        if (model.CinemaId.HasValue) query = query.Where(x => x.Showtime.CinemaId == model.CinemaId);
        if (model.ShowtimeId.HasValue) query = query.Where(x => x.ShowtimeId == model.ShowtimeId);
        if (model.From.HasValue) query = query.Where(x => x.Showtime.StartAt >= model.From.Value.Date);
        if (model.To.HasValue) { var end = model.To.Value.Date; query = query.Where(x => x.Showtime.StartAt.Date <= end); }
        model.Total = await query.CountAsync(); model.Page = Math.Clamp(model.Page, 1, Math.Max(1, (model.Total + 19) / 20));
        model.Bookings = await query.Include(x => x.Showtime).ThenInclude(x => x.Movie).Include(x => x.Showtime).ThenInclude(x => x.Cinema).OrderByDescending(x => x.CreatedUtc).Skip((model.Page - 1) * 20).Take(20).ToListAsync();
        model.Cinemas = await db.Cinemas.OrderBy(x => x.Name).Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToListAsync();
        return View(model);
    }
    public async Task<IActionResult> Details(int id)
    {
        var booking = await db.Bookings.AsNoTracking().Include(x => x.Showtime).ThenInclude(x => x.Movie).Include(x => x.Showtime).ThenInclude(x => x.Cinema).SingleOrDefaultAsync(x => x.Id == id);
        return booking is null ? NotFound() : View(booking);
    }
}
