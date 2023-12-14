using MovieLibraryEntities.Models;

namespace MovieLibraryEntities.Dao
{
    public interface IRepository
    {
        IEnumerable<Movie> GetAllMovies();
        IEnumerable<Movie> Search(string searchString);

        public Movie GetMovieById(long id);
        public long AddMovie(string title, IEnumerable<string> genres, DateTime releaseDate);

        public User GetUserByID(long id);
        public long AddUser(long age, string gender, string zipcode, Occupation occupation);

        public long AddUserRating(User user, Movie movie, long rating, DateTime ratingTime);

        public IEnumerable<Genre> GetAllGenres();
        public IEnumerable<Occupation> GetAllOccupations();
        public void UpdateMovie(Movie movie, string title, IEnumerable<string> genresToRemove, IEnumerable<string> genresToAdd, DateTime releaseDate);

        public void DeleteMovie(Movie movie);

        public IEnumerable<UserMovie> GetUserMovies();
    }
}
