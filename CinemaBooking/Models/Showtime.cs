namespace CinemaBooking.Models;
public class Showtime
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null!;
    public int CinemaId { get; set; }
    public Cinema Cinema { get; set; } = null!;
    public DateTime StartAt { get; set; }
    public decimal TicketPrice { get; set; }
    public int Capacity { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public List<Booking> Bookings { get; set; } = [];
}
