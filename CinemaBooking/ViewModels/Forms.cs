using System.ComponentModel.DataAnnotations;
using CinemaBooking.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace CinemaBooking.ViewModels;
public abstract class ImageForm
{
    public int Id { get; set; }
    [Display(Name = "Image (JPEG, PNG or WebP, maximum 3 MB)")] public IFormFile? Image { get; set; }
    public string? ExistingImage { get; set; }
}
public class CinemaForm : ImageForm
{
    [Required, StringLength(120, MinimumLength = 2)] public string Name { get; set; } = "";
    [Required, StringLength(250, MinimumLength = 5)] public string Address { get; set; } = "";
    [Required, StringLength(3000, MinimumLength = 10)] public string Description { get; set; } = "";
}
public class ActorForm : ImageForm
{
    [Required, StringLength(120, MinimumLength = 2)] public string Name { get; set; } = "";
    [Required, StringLength(3000, MinimumLength = 10)] public string Biography { get; set; } = "";
}
public class MovieForm : ImageForm
{
    [Required, StringLength(160, MinimumLength = 2)] public string Title { get; set; } = "";
    [Required, StringLength(4000, MinimumLength = 10)] public string Description { get; set; } = "";
    [Range(1, 600), Display(Name = "Duration (minutes)")] public int DurationMinutes { get; set; } = 100;
    [Required, StringLength(50, MinimumLength = 2)] public string Genre { get; set; } = "";
    [Display(Name = "Cast")] public List<int> ActorIds { get; set; } = [];
    public List<SelectListItem> Actors { get; set; } = [];
}
public class ShowtimeForm
{
    public int Id { get; set; }
    [Range(1, int.MaxValue), Display(Name = "Movie")] public int MovieId { get; set; }
    [Range(1, int.MaxValue), Display(Name = "Cinema")] public int CinemaId { get; set; }
    [Required, Display(Name = "Screening time (Cairo)")] public DateTime? StartAt { get; set; }
    [Range(typeof(decimal), "1", "100000"), Display(Name = "Ticket price (EGP)")]
    [RegularExpression(@"\d+(\.\d{1,2})?", ErrorMessage = "Use a price with at most two decimal places.")] public decimal TicketPrice { get; set; } = 150;
    [Range(1, 10000)] public int Capacity { get; set; } = 100;
    public string? RowVersion { get; set; }
    public int BookedTickets { get; set; }
    public List<SelectListItem> Movies { get; set; } = [];
    public List<SelectListItem> Cinemas { get; set; } = [];
}
public class BookingForm
{
    [Range(1, int.MaxValue)] public int ShowtimeId { get; set; }
    [Required] public string SubmissionToken { get; set; } = "";
    [Required, StringLength(100, MinimumLength = 2), Display(Name = "Customer name")] public string CustomerName { get; set; } = "";
    [Required, StringLength(25), RegularExpression(@"\+?(?=(?:[ ()-]*[0-9]){8,15}[ ()-]*$)[0-9][0-9 ()-]*[0-9]", ErrorMessage = "Enter 8-15 digits with an optional +, spaces, parentheses or hyphens.")]
    public string Phone { get; set; } = "";
    [Range(1, 10, ErrorMessage = "Book between 1 and 10 tickets.")] public int Quantity { get; set; } = 1;
    public ScreeningCard? Screening { get; set; }
}
public class ScreeningCard
{
    public int Id { get; set; }
    public string MovieTitle { get; set; } = "";
    public string CinemaName { get; set; } = "";
    public string Address { get; set; } = "";
    public DateTime StartAt { get; set; }
    public decimal Price { get; set; }
    public int Available { get; set; }
}
public class HomePage
{
    public int Cinemas { get; set; }
    public int Movies { get; set; }
    public int Actors { get; set; }
    public List<Movie> Featured { get; set; } = [];
}
public class MovieBrowse
{
    [StringLength(160)] public string? Search { get; set; }
    public string? Genre { get; set; }
    public int? CinemaId { get; set; }
    public List<Movie> Movies { get; set; } = [];
    public List<SelectListItem> Cinemas { get; set; } = [];
    public List<string> Genres { get; set; } = [];
}
public class MovieDetails
{
    public Movie Movie { get; set; } = null!;
    public List<ScreeningCard> Screenings { get; set; } = [];
}
public class CinemaDetails
{
    public Cinema Cinema { get; set; } = null!;
    public List<ScreeningCard> Screenings { get; set; } = [];
}
public class AdminDashboard
{
    public int Cinemas { get; set; }
    public int Movies { get; set; }
    public int Actors { get; set; }
    public int Bookings { get; set; }
    public int Tickets { get; set; }
    public decimal Revenue { get; set; }
    public List<Booking> Recent { get; set; } = [];
}
public class DeletePage
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? BlockReason { get; set; }
}
public class BookingSearch
{
    public string? Search { get; set; }
    public int? CinemaId { get; set; }
    public int? ShowtimeId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int Total { get; set; }
    public List<Booking> Bookings { get; set; } = [];
    public List<SelectListItem> Cinemas { get; set; } = [];
}
