using CinemaBooking.Models;
using CinemaBooking.Services;
using Microsoft.EntityFrameworkCore;
namespace CinemaBooking.Data;
public class SeedData(CinemaDbContext db)
{
    public async Task Initialize()
    {
        if ((await db.Database.GetPendingMigrationsAsync()).Any()) throw new InvalidOperationException("Apply the supplied migration first: dotnet ef database update --project CinemaBooking --startup-project CinemaBooking --context CinemaDbContext");
        await using var transaction = await db.Database.BeginTransactionAsync();
        await db.Database.ExecuteSqlRawAsync("DECLARE @result int; EXEC @result = sp_getapplock @Resource = 'CinemaBooking_Seed', @LockMode = 'Exclusive', @LockOwner = 'Transaction', @LockTimeout = 15000; IF @result < 0 THROW 51000, 'Could not obtain seed lock.', 1;");
        var cinemaNames = new[] { "Nile Screen", "City Lights Cinema", "Alexandria Picture House" };
        var addresses = new[] { "18 Corniche El Nil, Garden City, Cairo", "45 El Tahrir Street, Dokki, Giza", "12 El Geish Road, Alexandria" };
        for (var i = 0; i < cinemaNames.Length; i++)
            if (!await db.Cinemas.AnyAsync(x => x.Name == cinemaNames[i]))
                db.Cinemas.Add(new Cinema { Name = cinemaNames[i], Address = addresses[i], Description = "A fictional demonstration cinema with comfortable screening rooms and a welcoming atmosphere. Arrive 15 minutes before your screening and show your booking code at the entrance.", ImagePath = $"/images/cinema-{i + 1}.png" });
        var actors = new[] { "Adam Farid", "Layla Nasser", "Omar Selim", "Maya Adel", "Youssef Amin", "Nour Khalil", "Karim Hassan", "Salma Fathi" };
        for (var i = 0; i < actors.Length; i++)
            if (!await db.Actors.AnyAsync(x => x.Name == actors[i]))
                db.Actors.Add(new Actor { Name = actors[i], Biography = "A fictional performer created for this educational cinema catalogue. The illustrated portrait is a placeholder, not a photograph of a real person.", ImagePath = $"/images/actor-{i + 1}.png" });
        await db.SaveChangesAsync();
        var titles = new[] { "Beyond the Nile", "Orbit Nine", "The Last Tram", "Summer in Alex", "Hidden Frequency", "Paper Lanterns" };
        var genres = new[] { "Adventure", "Science Fiction", "Drama", "Comedy", "Thriller", "Animation" };
        var descriptions = new[] {
            "Two explorers follow a forgotten river map through sunlit valleys, discovering that the greatest journey is the one that brings them home.",
            "On a quiet research station above a distant world, a young engineer receives a signal that could change the future of human exploration.",
            "During the final night of an old tram line, strangers share stories, small kindnesses and an unexpected chance to begin again.",
            "A family reunion by the Mediterranean becomes a joyful adventure of mistaken plans, old friendships and a summer nobody will forget.",
            "A radio presenter hears a message hidden between stations. As midnight approaches, every clue leads closer to the heart of the city.",
            "A young inventor and a curious fox build a flying lantern to find the missing stars in this colourful animated family adventure."
        };
        var cast = await db.Actors.Where(x => actors.Contains(x.Name)).OrderBy(x => x.Id).ToListAsync();
        for (var i = 0; i < titles.Length; i++)
        {
            var movie = await db.Movies.Include(x => x.MovieActors).SingleOrDefaultAsync(x => x.Title == titles[i]);
            if (movie is null)
            {
                movie = new Movie { Title = titles[i], Genre = genres[i], DurationMinutes = 95 + i * 7, Description = descriptions[i], PosterPath = $"/images/movie-{i + 1}.png" };
                movie.MovieActors.Add(new MovieActor { ActorId = cast[i % cast.Count].Id });
                movie.MovieActors.Add(new MovieActor { ActorId = cast[(i + 2) % cast.Count].Id });
                db.Movies.Add(movie);
            }
        }
        await db.SaveChangesAsync();
        var cinemas = await db.Cinemas.Where(x => cinemaNames.Contains(x.Name)).OrderBy(x => x.Id).ToListAsync();
        var movies = await db.Movies.Where(x => titles.Contains(x.Title)).OrderBy(x => x.Id).ToListAsync();
        var now = CairoClock.Now;
        for (var i = 0; i < movies.Count; i++)
        {
            var movieId = movies[i].Id;
            if (await db.Showtimes.AnyAsync(x => x.MovieId == movieId && x.StartAt > now)) continue;
            for (var day = 1; day <= 3; day++)
            {
                var start = now.Date.AddDays(day).AddHours(16 + i % 3 * 2);
                db.Showtimes.Add(new Showtime { MovieId = movieId, CinemaId = cinemas[(i + day - 1) % cinemas.Count].Id, StartAt = start, Capacity = 60 + i * 10, TicketPrice = 120 + i * 15 });
            }
        }
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
    }
}
