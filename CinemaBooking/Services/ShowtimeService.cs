using CinemaBooking.Data;
using CinemaBooking.Models;
using CinemaBooking.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
namespace CinemaBooking.Services;
public class ShowtimeService(CinemaDbContext db)
{
    public async Task<string?> Save(ShowtimeForm form)
    {
        if (form.StartAt is null || !CairoClock.IsValidScreeningTime(form.StartAt.Value)) return "Choose an unambiguous, valid Cairo date and time (outside the daylight saving transition).";
        if (form.Capacity is < 1 or > 10000 || form.TicketPrice is < 1 or > 100000 || decimal.Round(form.TicketPrice, 2) != form.TicketPrice) return "Enter a valid capacity and price with at most two decimal places.";
        if (!await db.Movies.AnyAsync(x => x.Id == form.MovieId) || !await db.Cinemas.AnyAsync(x => x.Id == form.CinemaId)) return "Select an existing movie and cinema.";
        try
        {
            await using var tx = await db.Database.BeginTransactionAsync();
            Showtime show;
            if (form.Id == 0)
            {
                show = new Showtime();
                db.Showtimes.Add(show);
            }
            else
            {
                var current = await db.Showtimes.FromSqlInterpolated($"SELECT * FROM [Showtimes] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {form.Id}").SingleOrDefaultAsync();
                if (current is null) return "The screening was deleted.";
                show = current;
                if (form.RowVersion != Convert.ToBase64String(show.RowVersion)) return "This screening was edited by another request. Reload the page before editing again.";
                var booked = await db.Bookings.Where(x => x.ShowtimeId == show.Id).SumAsync(x => x.Quantity);
                if (booked > 0 && (show.MovieId != form.MovieId || show.CinemaId != form.CinemaId || show.StartAt != form.StartAt || show.TicketPrice != form.TicketPrice))
                    return "This screening has bookings. Its movie, cinema, time and price cannot change.";
                if (form.Capacity < booked) return $"Capacity cannot be lower than the {booked} booked tickets.";
            }
            if (form.StartAt <= CairoClock.Now && (form.Id == 0 || show.StartAt != form.StartAt)) return "New or rescheduled screenings must be in the future (Cairo time).";
            show.MovieId = form.MovieId;
            show.CinemaId = form.CinemaId;
            show.StartAt = DateTime.SpecifyKind(form.StartAt.Value, DateTimeKind.Unspecified);
            show.TicketPrice = form.TicketPrice;
            show.Capacity = form.Capacity;
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return null;
        }
        catch (DbUpdateException) { db.ChangeTracker.Clear(); return "The screening changed or a related record was removed. Reload the page and try again."; }
        catch (SqlException ex) when (ex.Number is 1205 or -2) { return "Another request is using this screening. Please try again."; }
    }
    public async Task<string?> Delete(int id)
    {
        try
        {
            await using var tx = await db.Database.BeginTransactionAsync();
            var show = await db.Showtimes.FromSqlInterpolated($"SELECT * FROM [Showtimes] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {id}").SingleOrDefaultAsync();
            if (show is null) return "Screening not found.";
            if (await db.Bookings.AnyAsync(x => x.ShowtimeId == id)) return "This screening has bookings and must be kept for ticket history.";
            db.Showtimes.Remove(show);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return null;
        }
        catch (DbUpdateException) { return "This screening is referenced or has changed. Refresh and try again."; }
        catch (SqlException ex) when (ex.Number is 1205 or -2) { return "Another request is using this screening. Please try again."; }
    }
}
