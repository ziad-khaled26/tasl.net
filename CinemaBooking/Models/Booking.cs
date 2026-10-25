using System.ComponentModel.DataAnnotations;
namespace CinemaBooking.Models;
public class Booking
{
    public int Id { get; set; }
    [MaxLength(32)] public string Code { get; set; } = "";
    public Guid SubmissionKey { get; set; }
    public int ShowtimeId { get; set; }
    public Showtime Showtime { get; set; } = null!;
    [MaxLength(100)] public string CustomerName { get; set; } = "";
    [MaxLength(25)] public string Phone { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedUtc { get; set; }
}
