using CinemaBooking.Data;
using CinemaBooking.Services;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
namespace CinemaBooking.Areas.Admin.Controllers;
public class ShowtimesController(CinemaDbContext db, ShowtimeService service) : AdminController
{
    public async Task<IActionResult> Index() => View(await db.Showtimes.AsNoTracking().Include(x => x.Movie).Include(x => x.Cinema).Include(x => x.Bookings).OrderByDescending(x => x.StartAt).ToListAsync());
    private async Task Populate(ShowtimeForm form)
    {
        form.Movies = await db.Movies.OrderBy(x => x.Title).Select(x => new SelectListItem(x.Title, x.Id.ToString())).ToListAsync();
        form.Cinemas = await db.Cinemas.OrderBy(x => x.Name).Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToListAsync();
        form.BookedTickets = await db.Bookings.Where(x => x.ShowtimeId == form.Id).SumAsync(x => x.Quantity);
    }
    public async Task<IActionResult> Create() { var form = new ShowtimeForm { StartAt = CairoClock.Now.Date.AddDays(1).AddHours(19) }; await Populate(form); return View("Form", form); }
    [HttpPost]
    public async Task<IActionResult> Create(ShowtimeForm form)
    {
        form.Id = 0;
        if (ModelState.IsValid) { var error = await service.Save(form); if (error is null) return Saved("Screening created."); ModelState.AddModelError("", error); }
        await Populate(form); return View("Form", form);
    }
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await db.Showtimes.FindAsync(id);
        if (entity is null) return NotFound();
        var form = new ShowtimeForm { Id = id, MovieId = entity.MovieId, CinemaId = entity.CinemaId, StartAt = entity.StartAt, TicketPrice = entity.TicketPrice, Capacity = entity.Capacity, RowVersion = Convert.ToBase64String(entity.RowVersion) };
        await Populate(form); return View("Form", form);
    }
    [HttpPost]
    public async Task<IActionResult> Edit(int id, ShowtimeForm form)
    {
        if (id != form.Id) return BadRequest();
        if (!await db.Showtimes.AnyAsync(x => x.Id == id)) return NotFound();
        if (ModelState.IsValid) { var error = await service.Save(form); if (error is null) return Saved("Screening updated."); ModelState.AddModelError("", error); }
        await Populate(form); return View("Form", form);
    }
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.Showtimes.Include(x => x.Movie).SingleOrDefaultAsync(x => x.Id == id);
        return entity is null ? NotFound() : View("Delete", new DeletePage { Id = id, Name = entity.Movie.Title + " | " + CairoClock.Display(entity.StartAt), BlockReason = await db.Bookings.AnyAsync(x => x.ShowtimeId == id) ? "This screening has bookings. Historical tickets cannot be deleted." : null });
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var error = await service.Delete(id);
        if (error is null) return Saved("Screening deleted.");
        return View("Delete", new DeletePage { Id = id, Name = "Screening", BlockReason = error });
    }
}
