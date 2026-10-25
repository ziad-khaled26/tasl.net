using CinemaBooking.Data;
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CinemaBooking.Areas.Admin.Controllers;
public class CinemasController(CinemaDbContext db, ImageService images) : AdminController
{
    public async Task<IActionResult> Index() => View(await db.Cinemas.AsNoTracking().OrderBy(x => x.Name).ToListAsync());
    public IActionResult Create() => View("Form", new CinemaForm());
    [HttpPost]
    public async Task<IActionResult> Create(CinemaForm form)
    {
        form.Id = 0;
        form.ExistingImage = null;
        if (!ModelState.IsValid) return View("Form", form);
        var path = await Upload(images, form);
        if (!ModelState.IsValid) return View("Form", form);
        db.Cinemas.Add(new Cinema { Name = form.Name.Trim(), Address = form.Address.Trim(), Description = form.Description.Trim(), ImagePath = path ?? "/images/cinema-1.png" });
        await db.SaveChangesAsync();
        return Saved("Cinema created.");
    }
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await db.Cinemas.FindAsync(id);
        return entity is null ? NotFound() : View("Form", new CinemaForm { Id = id, Name = entity.Name, Address = entity.Address, Description = entity.Description, ExistingImage = entity.ImagePath });
    }
    [HttpPost]
    public async Task<IActionResult> Edit(int id, CinemaForm form)
    {
        if (id != form.Id) return BadRequest();
        var entity = await db.Cinemas.FindAsync(id);
        if (entity is null) return NotFound();
        form.ExistingImage = entity.ImagePath;
        if (!ModelState.IsValid) return View("Form", form);
        var path = await Upload(images, form);
        if (!ModelState.IsValid) return View("Form", form);
        entity.Name = form.Name.Trim(); entity.Address = form.Address.Trim(); entity.Description = form.Description.Trim(); entity.ImagePath = path ?? entity.ImagePath;
        try { await db.SaveChangesAsync(); } catch (DbUpdateConcurrencyException) { ModelState.AddModelError("", "This cinema was deleted. Return to the list."); return View("Form", form); }
        return Saved("Cinema updated.");
    }
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.Cinemas.FindAsync(id);
        if (entity is null) return NotFound();
        return View("Delete", new DeletePage { Id = id, Name = entity.Name, BlockReason = await db.Showtimes.AnyAsync(x => x.CinemaId == id) ? "This cinema has screenings. Remove unbooked screenings first; booked screenings must be preserved." : null });
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await db.Cinemas.FindAsync(id);
        if (entity is null) return NotFound();
        if (await db.Showtimes.AnyAsync(x => x.CinemaId == id)) return View("Delete", new DeletePage { Id = id, Name = entity.Name, BlockReason = "A cinema with screenings cannot be deleted." });
        db.Cinemas.Remove(entity);
        try { await db.SaveChangesAsync(); } catch (DbUpdateException) { return View("Delete", new DeletePage { Id = id, Name = entity.Name, BlockReason = "The cinema is now referenced or has changed. Refresh the page." }); }
        return Saved("Cinema deleted.");
    }
}
