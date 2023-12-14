namespace MovieLibraryEntities.Models
{
    public class Movie
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public DateTime ReleaseDate { get; set; }


        public virtual ICollection<MovieGenre> MovieGenres { get; set; }
        public virtual ICollection<UserMovie> UserMovies { get; set; }

        public override string ToString()
        {
            return $"{this.Id,5} | {this.Title,-40} | {this.ReleaseDate.ToString("MM/dd/yyyy")}";
        }
    }
}
