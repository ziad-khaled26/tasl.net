using CinemaBooking.Data;
using CinemaBooking.Services;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CinemaBooking.Controllers;
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public class BookingsController(CinemaDbContext db, BookingService bookings, BookingTokens tokens, ScreeningQueries screenings) : Controller
{
    public async Task<IActionResult> Create(int id)
    {
        var screening = await screenings.Find(id);
        if (screening is null) return NotFound();
        return View(new BookingForm { ShowtimeId = id, SubmissionToken = tokens.Create(id), Screening = screening });
    }
    [HttpPost]
    public async Task<IActionResult> Create(int id, BookingForm form)
    {
        if (id != form.ShowtimeId) return BadRequest();
        form.Screening = await screenings.Find(id);
        if (form.Screening is null) return NotFound();
        if (ModelState.IsValid)
        {
            var result = await bookings.Book(form);
            if (result.Code is not null) return RedirectToAction(nameof(Confirmation), new { code = result.Code });
            ModelState.AddModelError("", result.Error!);
            form.Screening = await screenings.Find(id);
        }
        return View(form);
    }
    public async Task<IActionResult> Confirmation(string code)
    {
        if (string.IsNullOrEmpty(code) || code.Length != 32 || !code.All(Uri.IsHexDigit)) return NotFound();
        var booking = await db.Bookings.AsNoTracking().Include(x => x.Showtime).ThenInclude(x => x.Movie).Include(x => x.Showtime).ThenInclude(x => x.Cinema).SingleOrDefaultAsync(x => x.Code == code);
        return booking is null ? NotFound() : View(booking);
    }
}
