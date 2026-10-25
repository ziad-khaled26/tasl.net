using System.Diagnostics;
using CinemaBooking.Data;
using CinemaBooking.Models;
using CinemaBooking.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace CinemaBooking.Controllers;
public class HomeController(CinemaDbContext db) : Controller
{
    public async Task<IActionResult> Index() => View(new HomePage {
        Cinemas = await db.Cinemas.CountAsync(), Movies = await db.Movies.CountAsync(), Actors = await db.Actors.CountAsync(),
        Featured = await db.Movies.AsNoTracking().OrderBy(x => x.Id).Take(6).ToListAsync()
    });
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
