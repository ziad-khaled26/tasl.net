using System.ComponentModel.DataAnnotations;
namespace CinemaBooking.Models;
public class Cinema
{
    public int Id { get; set; }
    [MaxLength(120)] public string Name { get; set; } = "";
    [MaxLength(250)] public string Address { get; set; } = "";
    [MaxLength(3000)] public string Description { get; set; } = "";
    [MaxLength(250)] public string ImagePath { get; set; } = "/images/cinema-1.png";
    public List<Showtime> Showtimes { get; set; } = [];
}
