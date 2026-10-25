using CinemaBooking.Services;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Mvc;
namespace CinemaBooking.Areas.Admin.Controllers;
[Area("Admin")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public abstract class AdminController : Controller
{
    protected async Task<string?> Upload(ImageService images, ImageForm form)
    {
        try { return await images.Save(form.Image); }
        catch (InvalidDataException ex) { ModelState.AddModelError(nameof(form.Image), ex.Message); return null; }
    }
    protected IActionResult Saved(string message)
    {
        TempData["Success"] = message;
        return RedirectToAction("Index");
    }
}
