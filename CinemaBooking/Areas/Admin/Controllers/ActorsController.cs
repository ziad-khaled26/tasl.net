using CinemaBooking.Data;
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CinemaBooking.Areas.Admin.Controllers;
public class ActorsController(CinemaDbContext db, ImageService images) : AdminController
{
    public async Task<IActionResult> Index() => View(await db.Actors.AsNoTracking().OrderBy(x => x.Name).ToListAsync());
    public IActionResult Create() => View("Form", new ActorForm());
    [HttpPost]
    public async Task<IActionResult> Create(ActorForm form)
    {
        form.Id = 0; form.ExistingImage = null;
        if (!ModelState.IsValid) return View("Form", form);
        var path = await Upload(images, form);
        if (!ModelState.IsValid) return View("Form", form);
        db.Actors.Add(new Actor { Name = form.Name.Trim(), Biography = form.Biography.Trim(), ImagePath = path ?? "/images/actor-1.png" });
        await db.SaveChangesAsync(); return Saved("Actor created.");
    }
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await db.Actors.FindAsync(id);
        return entity is null ? NotFound() : View("Form", new ActorForm { Id = id, Name = entity.Name, Biography = entity.Biography, ExistingImage = entity.ImagePath });
    }
    [HttpPost]
    public async Task<IActionResult> Edit(int id, ActorForm form)
    {
        if (id != form.Id) return BadRequest();
        var entity = await db.Actors.FindAsync(id);
        if (entity is null) return NotFound();
        form.ExistingImage = entity.ImagePath;
        if (!ModelState.IsValid) return View("Form", form);
        var path = await Upload(images, form);
        if (!ModelState.IsValid) return View("Form", form);
        entity.Name = form.Name.Trim(); entity.Biography = form.Biography.Trim(); entity.ImagePath = path ?? entity.ImagePath;
        try { await db.SaveChangesAsync(); } catch (DbUpdateConcurrencyException) { ModelState.AddModelError("", "This actor was deleted. Return to the list."); return View("Form", form); }
        return Saved("Actor updated.");
    }
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.Actors.FindAsync(id);
        return entity is null ? NotFound() : View("Delete", new DeletePage { Id = id, Name = entity.Name, BlockReason = await db.MovieActors.AnyAsync(x => x.ActorId == id) ? "This actor belongs to a movie cast. Remove the cast assignment before deleting." : null });
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await db.Actors.FindAsync(id);
        if (entity is null) return NotFound();
        if (await db.MovieActors.AnyAsync(x => x.ActorId == id)) return View("Delete", new DeletePage { Id = id, Name = entity.Name, BlockReason = "Remove this actor from all movie casts before deleting." });
        db.Actors.Remove(entity);
        try { await db.SaveChangesAsync(); } catch (DbUpdateException) { return View("Delete", new DeletePage { Id = id, Name = entity.Name, BlockReason = "The actor is now referenced or has changed. Refresh the page." }); }
        return Saved("Actor deleted.");
    }
}
