using cinema_app_back.Models;
using System;

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; }  // e.g., "Avengers: Endgame"
    public string Genre { get; set; }
    public string Image { get; set; } // URL to the movie poster
    public string Description { get; set; }
    public string AgeRating { get; set; }
    public int DurationInMinutes { get; set; } // Duration of the movie in minutes
    public string Director { get; set; }
    public string Language { get; set; }
    public DateTime? ReleaseDate { get; set; } // Дата прем'єри фільму
    public virtual ICollection<Showtime> Showtimes { get; set; } // A movie can have many showtimes
}
