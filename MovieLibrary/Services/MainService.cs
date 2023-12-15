

using Microsoft.Extensions.Logging;
using MovieLibrary.Dao;
using MovieLibraryEntities.Dao;

namespace MovieLibrary.Services
{
    public class MainService : IMainService
    {
        private readonly ILogger<MainService> _logger;
        private readonly IMenuService _menuService;
        private readonly IMovieRepository _movieRepository;


        public MainService(ILogger<MainService> logger, IMenuService menuService, IMovieRepository movieRepository)
        {
            _logger = logger;
            _menuService = menuService;
            _movieRepository = movieRepository;
        }

        public void Invoke()
        {
            MenuOptions selection;
            do
            {
                selection = _menuService.RunMenu();

                switch (selection) 
                {
                    case MenuOptions.SearchMovie:
                        _logger.LogInformation("\nSearching for a Movie");
                        _movieRepository.SearchMovie();
                        break;
                    case MenuOptions.AddMovie:
                        _logger.LogInformation("\nAdding a new Movie");
                        _movieRepository.AddMovie();
                        break;
                    case MenuOptions.ListMovies:
                        _logger.LogInformation("\nListing Movies from database");
                        _movieRepository.ListMovies();
                        break;
                    case MenuOptions.UpdateMovie:
                        _logger.LogInformation("\nUpdating an existing Movie");
                        _movieRepository.UpdateMovie();
                        break;
                    case MenuOptions.DeleteMovie:
                        _logger.LogInformation("\nDeleting a Movie");
                        _movieRepository.DeleteMovie();
                        break;
                    case MenuOptions.AddUser:
                        _logger.LogInformation("\nAdding a User");
                        _movieRepository.AddUser();
                        break;
                    case MenuOptions.DisplayUser:
                        _logger.LogInformation("\nDisplaying a User");
                        _movieRepository.DisplayUser();
                        break;
                    case MenuOptions.UserRate:
                        _logger.LogInformation("\nSubmitting a User Rating");
                        _movieRepository.UserRate();
                        break;
                    case MenuOptions.ListTopRated:
                        _logger.LogInformation("\nListing a Top Rated Movie");
                        _movieRepository.ListTopRated();
                        break;
                }
            } while (selection != MenuOptions.Exit);

            _logger.LogInformation("\nUser has exited the program.\nGoodbye.");

        }
    }
}
