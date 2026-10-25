namespace CinemaBooking.Services;
public static class CairoClock
{
    public static TimeZoneInfo Zone { get; } = TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");
    public static DateTime Now => DateTime.SpecifyKind(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zone), DateTimeKind.Unspecified);
    public static bool IsValidScreeningTime(DateTime time) => !Zone.IsInvalidTime(DateTime.SpecifyKind(time, DateTimeKind.Unspecified)) && !Zone.IsAmbiguousTime(DateTime.SpecifyKind(time, DateTimeKind.Unspecified));
    public static string Display(DateTime time) => time.ToString("ddd, dd MMM yyyy | hh:mm tt", System.Globalization.CultureInfo.InvariantCulture);
}
