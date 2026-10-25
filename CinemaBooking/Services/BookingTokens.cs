using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
namespace CinemaBooking.Services;
public class BookingTokens(IDataProtectionProvider provider)
{
    private readonly IDataProtector protector = provider.CreateProtector("CinemaBooking.BookingForm.v1");
    public string Create(int showtimeId) => protector.Protect($"{showtimeId}|{Guid.NewGuid():N}|{DateTime.UtcNow.AddMinutes(30).Ticks}");
    public Guid? Read(string token, int showtimeId)
    {
        try
        {
            var parts = protector.Unprotect(token).Split('|');
            if (parts.Length != 3 || !int.TryParse(parts[0], out var id) || id != showtimeId || !long.TryParse(parts[2], out var expires) || expires < DateTime.UtcNow.Ticks || !Guid.TryParseExact(parts[1], "N", out var key)) return null;
            return key;
        }
        catch (Exception ex) when (ex is CryptographicException or ArgumentException) { return null; }
    }
}
