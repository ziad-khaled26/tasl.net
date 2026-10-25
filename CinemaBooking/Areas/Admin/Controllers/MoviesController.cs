using CinemaBooking.Data;
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
namespace CinemaBooking.Areas.Admin.Controllers;
public class MoviesController(CinemaDbContext db, ImageService images) : AdminController
{
    public async Task<IActionResult> Index() => View(await db.Movies.AsNoTracking().Include(x => x.MovieActors).OrderBy(x => x.Title).ToListAsync());
    private async Task Populate(MovieForm form) => form.Actors = await db.Actors.OrderBy(x => x.Name).Select(x => new SelectListItem(x.Name, x.Id.ToString())).ToListAsync();
    private async Task ValidateCast(MovieForm form)
    {
        form.ActorIds = form.ActorIds.Distinct().ToList();
        if (await db.Actors.CountAsync(x => form.ActorIds.Contains(x.Id)) != form.ActorIds.Count) ModelState.AddModelError(nameof(form.ActorIds), "Select existing actors only.");
    }
    public async Task<IActionResult> Create() { var form = new MovieForm(); await Populate(form); return View("Form", form); }
    [HttpPost]
    public async Task<IActionResult> Create(MovieForm form)
    {
        form.Id = 0; form.ExistingImage = null;
        await Populate(form); await ValidateCast(form);
        if (!ModelState.IsValid) return View("Form", form);
        var path = await Upload(images, form);
        if (!ModelState.IsValid) return View("Form", form);
        var movie = new Movie { Title = form.Title.Trim(), Description = form.Description.Trim(), Genre = form.Genre.Trim(), DurationMinutes = form.DurationMinutes, PosterPath = path ?? "/images/movie-1.png", MovieActors = form.ActorIds.Select(x => new MovieActor { ActorId = x }).ToList() };
        db.Movies.Add(movie);
        try { await db.SaveChangesAsync(); } catch (DbUpdateException) { ModelState.AddModelError("", "A selected actor was removed. Review the cast and try again."); return View("Form", form); }
        return Saved("Movie created.");
    }
    public async Task<IActionResult> Edit(int id)
    {
        var entity = await db.Movies.Include(x => x.MovieActors).SingleOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();
        var form = new MovieForm { Id = id, Title = entity.Title, Description = entity.Description, Genre = entity.Genre, DurationMinutes = entity.DurationMinutes, ExistingImage = entity.PosterPath, ActorIds = entity.MovieActors.Select(x => x.ActorId).ToList() };
        await Populate(form); return View("Form", form);
    }
    [HttpPost]
    public async Task<IActionResult> Edit(int id, MovieForm form)
    {
        if (id != form.Id) return BadRequest();
        var entity = await db.Movies.Include(x => x.MovieActors).SingleOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();
        form.ExistingImage = entity.PosterPath;
        await Populate(form); await ValidateCast(form);
        if (!ModelState.IsValid) return View("Form", form);
        var path = await Upload(images, form);
        if (!ModelState.IsValid) return View("Form", form);
        entity.Title = form.Title.Trim(); entity.Description = form.Description.Trim(); entity.Genre = form.Genre.Trim(); entity.DurationMinutes = form.DurationMinutes; entity.PosterPath = path ?? entity.PosterPath;
        db.MovieActors.RemoveRange(entity.MovieActors.Where(x => !form.ActorIds.Contains(x.ActorId)));
        foreach (var actorId in form.ActorIds.Where(x => entity.MovieActors.All(a => a.ActorId != x))) entity.MovieActors.Add(new MovieActor { ActorId = actorId });
        try { await db.SaveChangesAsync(); } catch (DbUpdateException) { ModelState.AddModelError("", "The movie or selected cast changed. Reload the page and try again."); return View("Form", form); }
        return Saved("Movie updated.");
    }
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await db.Movies.FindAsync(id);
        return entity is null ? NotFound() : View("Delete", new DeletePage { Id = id, Name = entity.Title, BlockReason = await db.Showtimes.AnyAsync(x => x.MovieId == id) ? "This movie has screenings and cannot be deleted. Booked screenings must be preserved." : null });
    }
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var entity = await db.Movies.Include(x => x.MovieActors).SingleOrDefaultAsync(x => x.Id == id);
        if (entity is null) return NotFound();
        if (await db.Showtimes.AnyAsync(x => x.MovieId == id)) return View("Delete", new DeletePage { Id = id, Name = entity.Title, BlockReason = "A movie with screenings cannot be deleted." });
        db.MovieActors.RemoveRange(entity.MovieActors); db.Movies.Remove(entity);
        try { await db.SaveChangesAsync(); } catch (DbUpdateException) { return View("Delete", new DeletePage { Id = id, Name = entity.Title, BlockReason = "The movie is now referenced or has changed. Refresh the page." }); }
        return Saved("Movie deleted.");
    }
}
