using System.ComponentModel.DataAnnotations;
namespace CinemaBooking.Models;
public class Actor
{
    public int Id { get; set; }
    [MaxLength(120)] public string Name { get; set; } = "";
    [MaxLength(3000)] public string Biography { get; set; } = "";
    [MaxLength(250)] public string ImagePath { get; set; } = "/images/actor-1.png";
    public List<MovieActor> MovieActors { get; set; } = [];
}
