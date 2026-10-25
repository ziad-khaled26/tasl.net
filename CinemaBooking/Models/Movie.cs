using System.ComponentModel.DataAnnotations;
namespace CinemaBooking.Models;
public class Movie
{
    public int Id { get; set; }
    [MaxLength(160)] public string Title { get; set; } = "";
    [MaxLength(4000)] public string Description { get; set; } = "";
    public int DurationMinutes { get; set; }
    [MaxLength(50)] public string Genre { get; set; } = "";
    [MaxLength(250)] public string PosterPath { get; set; } = "/images/movie-1.png";
    public List<MovieActor> MovieActors { get; set; } = [];
    public List<Showtime> Showtimes { get; set; } = [];
}
