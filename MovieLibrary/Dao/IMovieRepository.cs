
namespace MovieLibrary.Dao
{
    public interface IMovieRepository
    {
        void SearchMovie();
        void AddMovie();
        void ListMovies();
        void UpdateMovie();
        void DeleteMovie();
        void AddUser();
        void DisplayUser();
        void UserRate();
        void ListTopRated();
    }
}
