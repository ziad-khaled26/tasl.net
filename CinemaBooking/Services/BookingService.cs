using System.Security.Cryptography;
using CinemaBooking.Data;
using CinemaBooking.Models;
using CinemaBooking.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
namespace CinemaBooking.Services;
public record BookingResult(string? Code, string? Error);
public class BookingService(CinemaDbContext db, BookingTokens tokens)
{
    public async Task<BookingResult> Book(BookingForm form)
    {
        var key = tokens.Read(form.SubmissionToken, form.ShowtimeId);
        if (key is null) return new(null, "This booking form expired or is invalid. Reload the page and try again.");
        if (form.Quantity is < 1 or > 10) return new(null, "Book between 1 and 10 tickets.");
        try
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            var show = await db.Showtimes.FromSqlInterpolated($"SELECT * FROM [Showtimes] WITH (UPDLOCK, HOLDLOCK) WHERE [Id] = {form.ShowtimeId}").SingleOrDefaultAsync();
            if (show is null) return new(null, "This screening no longer exists.");
            var existing = await db.Bookings.SingleOrDefaultAsync(x => x.SubmissionKey == key.Value);
            if (existing is not null)
            {
                if (existing.ShowtimeId != form.ShowtimeId || existing.CustomerName != form.CustomerName.Trim() || existing.Phone != form.Phone.Trim() || existing.Quantity != form.Quantity)
                    return new(null, "This form has already been used. Start a new booking to change the details.");
                await transaction.CommitAsync();
                return new(existing.Code, null);
            }
            if (show.StartAt <= CairoClock.Now) return new(null, "This screening has already started. Choose an upcoming screening.");
            var booked = await db.Bookings.Where(x => x.ShowtimeId == show.Id).SumAsync(x => x.Quantity);
            if (form.Quantity > show.Capacity - booked) return new(null, $"Only {show.Capacity - booked} tickets are available. Choose a smaller quantity or another screening.");
            var booking = new Booking {
                Code = Convert.ToHexString(RandomNumberGenerator.GetBytes(16)), SubmissionKey = key.Value,
                ShowtimeId = show.Id, CustomerName = form.CustomerName.Trim(), Phone = form.Phone.Trim(), Quantity = form.Quantity,
                UnitPrice = show.TicketPrice, TotalAmount = show.TicketPrice * form.Quantity, CreatedUtc = DateTime.UtcNow
            };
            db.Bookings.Add(booking);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return new(booking.Code, null);
        }
        catch (Exception ex) when (ex is DbUpdateConcurrencyException || ex is SqlException { Number: 1205 or -2 } || ex is DbUpdateException { InnerException: SqlException { Number: 1205 or 2601 or 2627 } })
        {
            db.ChangeTracker.Clear();
            return new(null, "Another request updated this screening. Please submit again; your form safely prevents duplicate bookings.");
        }
    }
}
