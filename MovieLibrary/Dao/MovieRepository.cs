using Castle.Core.Internal;
using Microsoft.Extensions.Logging;
using MovieLibrary.Services.IO;
using MovieLibraryEntities.Dao;
using System.Text;

namespace MovieLibrary.Dao
{
    /// <summary>
    /// A business logic implementation layer used to hide potential complexities from the database layer.
    /// Also takes away user interaction from the database layer.
    /// 
    /// All "get" operations on the datalayer result in either instanced entity objects, or Lists.
    /// 
    /// This class became bogged down with business logic. My next goal would be to look for ways to isolate and remove some of that to preserve Single Responsibility.
    /// I was held back by the idea of not having addtional services that also had access to the data layer - I couldn't find a way to move, i.e. the TopRatedByX logic into a "IRatingsService"
    /// without that service also needing access to the datalayer.  In the interest of not allowing that, I let this class grow into the monstrosity that you see before you.
    /// 
    /// Additionally, my input validation went out of control and should definitely not be as lengthy as it is...
    /// </summary>
    public class MovieRepository : IMovieRepository
    {
        private readonly ILogger<IMovieRepository> _logger;
        private readonly IOutputService _outputService;
        private readonly IInputService _inputService;
        private readonly IRepository _repository;

        public MovieRepository(ILogger<IMovieRepository> logger, IOutputService outputService, IInputService inputService, IRepository repository)
        {
            _logger = logger;
            _outputService = outputService;
            _inputService = inputService;
            _repository = repository;
        }

        public void SearchMovie()
        {
            _logger.LogInformation("\nSearching...");
            _outputService.WriteLine("Enter the title substring to search for (case-insensitive)");

            var searchString = _inputService.ReadLine();
            _logger.LogInformation("\nUser submitted searchString: \"{searchString}\"", searchString);

            var movies = _repository.Search(searchString);
            _logger.LogInformation("\nSearching Library...");

            if (!movies.Any())
            {
                _logger.LogError("\nNo movie found with searchString {searchString}", searchString);
                _outputService.WriteLine("No movie found.");
            }
            else
            {
                _logger.LogInformation("\n{movies} Movies found with this search.\nPrinting...", movies.Count());

                _outputService.WriteLine($"Your movies are: ");
                movies.ToList().ForEach(x => _outputService.WriteLine($"{x.ToString()}"));
            }
            _logger.LogInformation("\nReturning to Menu...");

        }
        public void AddMovie()
        {
            _logger.LogInformation("\nAdding a Movie...");

            string title = "";
            do
            {
                title = GetTitleFromUser();
            } while (title == "");



            var releaseDate = GetReleaseDateFromUser("Enter Release Date (MM/DD/YYYY): ");
            var releaseDateString = releaseDate.ToString("MM/dd/yyyy");
            _logger.LogInformation("\nUser submitted release date: \"{releaseDate}\"", releaseDate);


            if (releaseDate > DateTime.UtcNow)
            {
                _logger.LogError("\nUser submitted an invalid release date: \"{releaseDate}\"", releaseDate);
                _logger.LogInformation("\nReturning to Menu...");
                _outputService.WriteLine("Returning to Menu...");
                return;
            }

            _logger.LogInformation("\nGetting Genres to Add...");


            var validGenres = _repository.GetAllGenres();
            var validGenresString = string.Join(", ", validGenres.Select(x => x.Name));
            _logger.LogInformation("\nValid Genres: {validGenresString}\n", validGenresString);
            _outputService.WriteLine("Genres: " + string.Join(", ", validGenres.Select(x => x.Name)) + "\n");

            var genres = new List<String>();
            var addtlGenres = true;
            do
            {
                _outputService.Write("Enter Genre (X when done): ");

                var input = _inputService.ReadLine();
                if (input.ToUpper() == "X")
                {
                    if (genres.Count == 0)
                    {
                        _logger.LogError("\nUser attempted to create movie with no genres");
                        _outputService.WriteLine("Enter at least one Genre to continue");
                    }
                    else
                    {
                        addtlGenres = false;
                    }
                }
                else if (validGenres.SingleOrDefault(x => x.Name == input) == null)
                {
                    _logger.LogError("\nUser attempted to add a genre that does not exist to this movie: {input}", input);
                    _outputService.WriteLine($"{input} does not match an existing Genre.");
                }
                else
                {
                    _logger.LogInformation("\nUser added {input} to the Genres of this Movie.", input);
                    genres.Add(input);
                }
            } while (addtlGenres);

            _logger.LogInformation("\nSending Movie to Library...");
            var id = _repository.AddMovie(title, genres, releaseDate);

            _logger.LogInformation("\nAdded {title} to the Library.  ID#:{id}", title, id);
            _outputService.WriteLine($"Added {title} to the Library.  ID#:{id}");

            _logger.LogInformation("\nReturning to Menu...");
        }
        public void ListMovies()
        {
            _logger.LogInformation("\nListing Movies...");
            _outputService.WriteLine($"{"Id",-5} | {"Title",-40} | {"Released",-10} | Genres ");
            var entry = 0;

            var movies = _repository.GetAllMovies();

            foreach (var movie in movies)
            {
                var next10 = movies.Skip(entry).Take(10);

                foreach (var m in next10)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append($"{m.ToString()} | ")
                      .Append(string.Join(", ", m.MovieGenres.Select(x => x.Genre.Name)));

                    _outputService.WriteLine(sb.ToString());
                    entry++;
                }
                next10 = movies.Skip(entry).Take(10);

                _logger.LogInformation("\nDisplayed {entry} entries...", entry);

                if (movies.Count() < entry + 10 || !ContinueDisplaying())
                {
                    if (movies.Count() < entry + 10)
                        _logger.LogInformation("\nReached end of Library.");
                    else
                        _logger.LogInformation("\nUser chose to stop displaying Movies");

                    _logger.LogInformation("\nReturning to Menu...");
                    break;
                }
            }
        }
        public void UpdateMovie()
        {
            _logger.LogInformation("\nUpdating a Movie...");

            var id = GetIDFromUser("movie", "Enter Movie ID to update: ");
            _logger.LogInformation("\nUser submitted movieID: {movieID}", id);

            _logger.LogInformation($"Searching Library...");
            var movie = _repository.GetMovieById(id);

            if (movie == null)
            {
                _logger.LogError("\nNo movie found with ID: {movieID}", id);
                _outputService.WriteLine($"No movie found with ID: {id}");

                _logger.LogInformation("\nReturning to Menu...");
                return;
            }

            _logger.LogInformation("\nFound {movie}", movie);
            _outputService.WriteLine(movie.ToString());

            string title = "";
            do
            {
                _outputService.Write("Enter New Title: ");
                title = _inputService.ReadLine();
                _logger.LogInformation("\nUser submitted Title: \"{title}\"", title);

            } while (title == "");


            var releaseDate = GetReleaseDateFromUser("Update Release Date (MM/DD/YYYY): ");
            var releaseDateString = releaseDate.ToString("MM/dd/yyyy");
            _logger.LogInformation("\nUser submitted ReleaseDate: {releaseDateString}", releaseDateString);
            if (releaseDate > DateTime.UtcNow)
            {
                releaseDate = movie.ReleaseDate;
                _logger.LogError("\nInvalid ReleaseDate. Defaulting to original: {releaseDateString}", releaseDateString);
            }

            _logger.LogInformation("\nPulling list of Genres associated with this Movie");
            var genres = movie.MovieGenres.Select(x => x.Genre).ToList();
            var genresString = string.Join(", ", genres.Select(x => x.Name));

            _outputService.Write("Genres: ");
            _outputService.WriteLine(genresString);
            _logger.LogInformation(genresString);

            _logger.LogInformation("\nPulling list of all Genres in the Library");
            var validGenres = _repository.GetAllGenres().ToList();


            var genresToAdd = new List<string>();
            var genresToRemove = new List<string>();



            _outputService.Write("Enter Y to remove a Genre: ");
            var input = _inputService.ReadLine();
            while (input != "" && input.ToLower().Substring(0, 1) == "y")
            {
                _outputService.Write("Enter the Genre to remove: ");
                do
                {
                    input = _inputService.ReadLine();
                    if (genres.Where(x => x.Name == input).IsNullOrEmpty())
                    {
                        _logger.LogError("\nUser tried to remove an invalid Genre: {input}", input);
                        _outputService.WriteLine($"{input} is not a valid genre associated with {movie.Title}");
                    }
                    else
                    {
                        _logger.LogInformation("\nGenre added to list for removal from this Movie: {input}", input);
                        genresToRemove.Add(input);
                    }
                } while (genres.Where(x => x.Name == input).IsNullOrEmpty());

                _outputService.Write("Enter Y to remove another Genre: ");
                input = _inputService.ReadLine();
            }

            var genresAvailableToAdd = validGenres.Except(genres);
            var genresAvailableString = string.Join(", ", genresAvailableToAdd.Select(x => x.Name));
            _outputService.WriteLine($"Other genres: {genresAvailableString}");
            _logger.LogInformation("\nGenres available to add: {genresAvailableString}", genresAvailableString);

            _outputService.Write("Enter Y to add a Genre: ");
            input = _inputService.ReadLine();
            while (input != "" && input.ToLower().Substring(0, 1) == "y")
            {
                _outputService.Write("Enter the Genre to add: ");
                do
                {
                    input = _inputService.ReadLine();
                    if (genresAvailableToAdd.Where(x => x.Name == input).IsNullOrEmpty())
                    {
                        _logger.LogError("\nUser tried to add an invalid Genre: {input}", input);
                        _outputService.WriteLine($"{input} is not a valid genre");
                    }
                    else
                    {
                        _logger.LogInformation("\nGenre added to list to be added from this Movie: {input}", input);
                        genresToAdd.Add(input);
                    }
                } while (genresAvailableToAdd.Where(x => x.Name == input).IsNullOrEmpty());

                _outputService.Write("Enter Y to add another Genre: ");
                input = _inputService.ReadLine();
            }

            _logger.LogInformation("\nSending updated Movie details to the Library...");
            _repository.UpdateMovie(movie, title, genresToRemove, genresToAdd, releaseDate);

            _outputService.WriteLine($"Movie updated successfully");
            _logger.LogInformation($"Movie updated successfully");

            _logger.LogInformation("\nReturning to Menu...");
        }
        public void DeleteMovie()
        {
            _logger.LogInformation("\nDeleting a Movie...");

            var id = GetIDFromUser("movie", "Enter Movie ID to delete: ");
            _logger.LogInformation("\nUser submitted movieID: {movieID}", id);

            _logger.LogInformation($"Searching Library...");
            var movie = _repository.GetMovieById(id);

            if (movie == null)
            {
                _logger.LogError("\nNo movie found with ID: {movieID}", id);
                _outputService.WriteLine($"No movie found with ID: {id}");

                _logger.LogInformation("\nReturning to Menu...");
                return;
            }

            _outputService.WriteLine(movie.ToString());
            _outputService.WriteLine("Type Y to confirm deletion. Any other response will cancel this attempt.");
            var input = _inputService.ReadLine();

            if (input != "" && input.ToLower() == "y")
            {
                _logger.LogInformation("\nUser confirmed delete request.\nDeleting...");
                _repository.DeleteMovie(movie);

                _outputService.WriteLine($"Deleted Movie# {id}");
                _logger.LogInformation("\nSuccessfully Deleted Movie#{movieID}", id);
            }
            else
            {
                _logger.LogInformation("\nUser did not confirm deletion.\nMovie#{movieID} will not be deleted.", id);
            }
            _logger.LogInformation("\nReturning to Menu...");
        }
        public void AddUser()
        {
            _logger.LogInformation("\nAdding a User...");

            var age = GetAgeFromUser();
            _logger.LogInformation("\nUser submitted Age: {age}", age);


            var gender = GetGenderCodeFromUser();
            _logger.LogInformation("\nUser submitted Gender: {gender}", gender);

            string zipcode = "";
            int foo;
            do
            {
                _outputService.Write("Enter Zip Code (5 digits): ");
                zipcode = _inputService.ReadLine();
                if (zipcode.Length != 5 && Int32.TryParse(zipcode, out foo))
                {
                    _logger.LogError("\nUser submitted invalid Zipcode: {zipcode}", zipcode);
                    _outputService.WriteLine($"{zipcode} is invalid");
                }
            } while (zipcode.Length != 5 && Int32.TryParse(zipcode, out foo));
            _logger.LogInformation("\nUser submitted ZipCode: {zipcode}", zipcode);

            var occupations = _repository.GetAllOccupations();
            var validOccsString = string.Join(", ", occupations.Select(x => x.Name));
            _logger.LogInformation("\nValid Occupations: {validOccsString}\n", validOccsString);
            _outputService.WriteLine($"Occupations: {validOccsString}\n");

            string userOccupation = "";
            _outputService.Write("Enter the Users Occupation: ");
            userOccupation = _inputService.ReadLine();
            var occupation = occupations.Where(x => x.Name == userOccupation).SingleOrDefault();
            if (occupation == null) {
                _logger.LogError("\nUser submitted invalid Occupation: {userOccupation}.\nDefaulting to None.", userOccupation);
                occupation = occupations.Where(x => x.Name == "None").Single(); 
            }

            _logger.LogInformation("\nUser submitted Occupation: {userOccupation}", userOccupation);

            var id = _repository.AddUser(age, gender, zipcode, occupation);
            _logger.LogInformation("\nAdded new user. ID: {id}", id);
            _outputService.WriteLine($"Added new user. ID: {id}");

            _logger.LogInformation("\nReturning to Menu...");
        }
        public void DisplayUser()
        {
            _logger.LogInformation("\nDisplaying a User");

            var id = GetIDFromUser("user", "Enter User ID to display: ");
            _logger.LogInformation("\nUser submitted UserID: {id}", id);

            _logger.LogInformation("\nSearching Library...");
            var user = _repository.GetUserByID(id);
            if (user == null)
            {
                _logger.LogError("\nNo user found with UserId: {id}", id);
                _outputService.WriteLine($"No user found with UserId: {id}");
            }
            else
            {
                _logger.LogInformation("\nUser found. Displaying details...");
                _outputService.WriteLine($"User found. Details: \nID: {user.Id} | Age: {user.Age} | Gender: {user.Gender} | ZipCode: {user.ZipCode} | Occupation: | {user.Occupation.Name}");

                var count = user.UserMovies != null ? user.UserMovies.ToList().Count() : 0;
                _outputService.WriteLine($"\t{count} review(s) associated with this user.");
            }

            _logger.LogInformation("\nReturning to Menu...");
        }
        public void UserRate()
        {
            _logger.LogInformation("\nCreating a new Rating by a User");

            var id = GetIDFromUser("user", "Enter ID of the User adding a new Rating: ");
            _logger.LogInformation("\nUser submitted UserID: {movieID}", id);

            _logger.LogInformation("\nSearching Library...");
            var user = _repository.GetUserByID(id);
            if (user == null)
            {
                _outputService.WriteLine($"No User found with ID: {id}");
                _logger.LogError("\nNo User found with ID: {movieID}", id);
                _logger.LogInformation("\nReturning to Menu...");
                return;
            }
            _logger.LogInformation("\nFound User with ID: {id}", id);


            id = GetIDFromUser("movie", "Enter ID of the existing Movie being Rated: ");
            _logger.LogInformation("\nUser submitted MovieID: {movieID}", id);

            _logger.LogInformation("\nSearching Library...");
            var movie = _repository.GetMovieById(id);
            if (movie == null)
            {
                _outputService.WriteLine($"No Movie found with ID: {id}");
                _logger.LogError("\nNo Movie found with ID: {movieID}", id);
                _logger.LogInformation("\nReturning to Menu...");
                return;
            }
            _logger.LogInformation("\nFound Movie with ID: {movieID}", id);

            string input = "";
            long rating = 0;
            do
            {
                _outputService.Write("Enter the Rating (1-5): ");
                input = _inputService.ReadLine();

            } while (long.TryParse(input, out rating) && (rating < 1 || rating > 5));
            _logger.LogInformation("\nUser submitted Rating: {rating}", rating);

            DateTime ratingTime = DateTime.UtcNow;
            _logger.LogInformation("\nRatedAt: {ratingTime}", ratingTime);

            _logger.LogInformation("\nSending UserRating to Library...");
            var ratingId = _repository.AddUserRating(user, movie, rating, ratingTime);
            _logger.LogInformation("\nAdded UserRating to the Library.  ID#:{ratingId}", ratingId);

            _outputService.WriteLine($"Added UserRating to the Library.");
            _outputService.WriteLine($"\tUser: {user.Id}\n\tMovie: {movie.Id} | {movie.Title} \n\tRating: {rating} | RatingID: {ratingId}");

            _logger.LogInformation("\nReturning to Menu...");
        }
        public void ListTopRated()
        {
            _logger.LogInformation("\nPreparing to search for Top Rated Movies by a category");

            string input;
            do
            {
                _outputService.WriteLine("Top Rated by:");
                _outputService.WriteLine("1) Age Bracket");
                _outputService.WriteLine("2) Occupation");
                _outputService.WriteLine("3) Year Reviewed");
                _outputService.WriteLine("4) Year Released");
                _outputService.WriteLine("X) Return to Main Menu");
                input = _inputService.ReadLine();

                switch (input.ToLower())
                {
                    case "1":
                        _logger.LogInformation("\nUser selected Top Rated Movies by Age Bracket");
                        TopRatedByAgeBracket();
                        return;
                    case "2":
                        _logger.LogInformation("\nUser selected Top Rated Movies by Occupation");
                        TopRatedByOccupation();
                        return;
                    case "3":
                        _logger.LogInformation("\nUser selected Top Rated Movies Reviewed in a Given Year");
                        TopRatedByYearReviewed();
                        return;
                    case "4":
                        _logger.LogInformation("\nUser selected Top Rated Movies Released in a Given Year");
                        TopRatedByYearReleased();
                        return;


                }
            } while (input != "x");

            _logger.LogInformation("\nReturning to Menu...");
        }


        private void TopRatedByAgeBracket()
        {
            _logger.LogInformation("\nSearching for Top Rated Movie for each Age Bracket");

            int[] ageBracketBounds = new int[]
            {
                18, 27, 36, 45, 54, 63, 72, 125
            };
            int brackets = ageBracketBounds.Length;

            _logger.LogInformation("\nGathering the list of all UserMovie records from the Library...");
            var userMovies = _repository.GetUserMovies();

            int lowerBound = 0;
            for (int i = 0; i < brackets; i++)
            {
                int upperBound = ageBracketBounds[i];
                if (lowerBound == 0)
                    _outputService.WriteLine($"\nUsers {upperBound} and under: ");
                else if (i == brackets - 1)
                    _outputService.WriteLine($"\nUsers over {lowerBound}: ");
                else
                    _outputService.WriteLine($"\nUsers {lowerBound} - {upperBound}: ");

                var topUserMoviebyBracket = userMovies
                    .Where(x => x.User.Age >= lowerBound && x.User.Age < upperBound)
                    .OrderByDescending(x => x.Rating)
                    .ThenBy(x => x.Movie.Title)
                    .FirstOrDefault();

                if (topUserMoviebyBracket == null)
                {
                    _logger.LogInformation("\nNo movies found for age bracket {lowerBound} - {upperBound}", lowerBound, upperBound);
                    _outputService.WriteLine("No movies found for this age bracket.");
                }
                else
                {
                    _outputService.WriteLine($"Highest Rating: {topUserMoviebyBracket.Rating}");
                    var movieID = topUserMoviebyBracket.Movie.Id;
                    _outputService.WriteLine($"First Result:\n\tID: {movieID} | Title: {topUserMoviebyBracket.Movie.Title}\n");
                    _logger.LogInformation("\n{lowerBound} - {upperBound} | Highest Rated Movie found: ID:{movieID}", lowerBound, upperBound, movieID);
                }


                lowerBound = upperBound;
                if (i < brackets - 1)
                {
                    _outputService.WriteLine("See next Age Bracket?");
                    if (!ContinueDisplaying()) return;
                }
            }

            _logger.LogInformation("\nFinished searching for Top Rated Movie for each Age Bracket");
        }
        private void TopRatedByOccupation()
        {
            _logger.LogInformation("\nSearching for Top Rated Movie for each Occupation");

            _logger.LogInformation("\nGathering the list of all UserMovie records from the Library...");
            var userRatings = _repository.GetUserMovies();

            _logger.LogInformation("\nGathering the list of all Occupation records from the Library...");
            var occupations = _repository.GetAllOccupations();

            foreach (var occupation in occupations)
            {
                _outputService.WriteLine($"\nTop Rated Movie by Occupation: {occupation.Name}");

                var topUserMovieByOccupation = userRatings
                    .Where(x => x.User.Occupation.Id == occupation.Id)
                    .OrderByDescending(x => x.Rating)
                    .ThenBy(x => x.Movie.Title)
                    .FirstOrDefault();


                var occName = occupation.Name;
                if (topUserMovieByOccupation == null)
                {
                    _logger.LogError("\nNo movies found for Occupation: {occName}", occName);
                    _outputService.WriteLine("No movies found reviewed by users of this occupation.");
                }
                else
                {

                    var movieId = topUserMovieByOccupation.Movie.Id;
                    _outputService.WriteLine($"Highest Rating: {topUserMovieByOccupation.Rating}");
                    _outputService.WriteLine($"First Result:\n\tID: {movieId} | Title: {topUserMovieByOccupation.Movie.Title}\n");
                    _logger.LogInformation("\nOccupation: {occName} | Highest Rated Movie found: ID:{movieID}", occName, movieId);
                }

                _outputService.WriteLine("See next Occupation?");
                if (!ContinueDisplaying()) return;
            }
            _logger.LogInformation("\nFinished searching for Top Rated Movie for each Occupation");
        }
        private void TopRatedByYearReviewed()
        {
            _logger.LogInformation("\nSearching for Top Rated Movie by Year");

            var year = GetYearFromUser();
            _logger.LogInformation("\nUser submitted Year: {year}", year);

            _logger.LogInformation("\nGathering the list of all UserMovie records from the Library...");
            var userRatings = _repository.GetUserMovies();


            var topRatedOfTheYear = userRatings
                .Where(x => x.RatedAt.Year == year)
                .OrderByDescending(x => x.Rating)
                .ThenBy(x => x.Movie.Title)
                .FirstOrDefault();

            if (topRatedOfTheYear == null)
            {
                _logger.LogError("\nNo user ratings found from {year}", year);
                _outputService.WriteLine($"No user ratings found from {year}");
            }
            else
            {
                var movieId = topRatedOfTheYear.Movie.Id;
                _outputService.WriteLine($"Highest Rating of {year}: {topRatedOfTheYear.Rating}");
                _outputService.WriteLine($"First Result:\n\tID: {movieId} | Title: {topRatedOfTheYear.Movie.Title}\n");
                _logger.LogInformation("\nYear: {year}s | Highest Rated Movie found: ID:{movieID}", year, movieId);
            }
        }
        private void TopRatedByYearReleased()
        {
            _logger.LogInformation("\nSearching for Top Rated Movie by Year");

            var year = GetYearFromUser();
            _logger.LogInformation("\nUser submitted Year: {year}", year);

            _logger.LogInformation("\nGathering the list of all UserMovie records from the Library...");
            var userRatings = _repository.GetUserMovies();

            var topRatedOfTheYear = userRatings
                .Where(x => x.Movie.ReleaseDate.Year == year)
                .OrderByDescending(x => x.Rating)
                .ThenBy(x => x.Movie.Title)
                .FirstOrDefault();

            if (topRatedOfTheYear == null)
            {
                _logger.LogError("\nNo user ratings found for movies released in {year}", year);
                _outputService.WriteLine($"No user ratings found for movies released in {year}");
            }
            else
            {
                var movieId = topRatedOfTheYear.Movie.Id;
                _outputService.WriteLine($"Highest Rating of {year}: {topRatedOfTheYear.Rating}");
                _outputService.WriteLine($"First Result:\n\tID: {movieId} | Title: {topRatedOfTheYear.Movie.Title}\n");
                _logger.LogInformation("\nYear: {year}s | Highest Rated Movie found: ID:{movieID}", year, movieId);
            }
        }
        private int GetYearFromUser()
        {
            string input;
            int year = -1;
            do
            {
                _outputService.WriteLine("Choose a valid year.");
                input = _inputService.ReadLine();
            } while (!int.TryParse(input, out year) || !DateTime.TryParse($"{year}-01-01", out _));

            return year;
        }
        private long GetAgeFromUser()
        {
            long age = 0;
            string input;
            do
            {
                _outputService.Write("Enter User Age: ");
                input = _inputService.ReadLine();
                try
                {
                    age = long.Parse(input);
                }
                catch (Exception ex)
                {

                }
                if (age < 1 || age > 125)
                {
                    _logger.LogError("\nUser submitted invalid Age: {input}", input);
                    _outputService.WriteLine($"{input} is invalid. (1-125)");
                }
            } while (age < 1 || age > 125);
            return age;
        }
        private string GetGenderCodeFromUser()
        {
            string input;
            bool valid = false;
            do
            {
                _outputService.Write("Enter User Gender(M/F): ");
                input = _inputService.ReadLine();

                if (input != null &&
                    (input.Substring(0, 1).ToUpper() == "M"
                    || input.Substring(0, 1).ToUpper() == "F"))
                {
                    valid = true;
                }
                else
                {
                    _logger.LogError("\nUser submitted invalid Gender code {input}", input);
                    _outputService.WriteLine($"{input} is not valid in this library.");
                }
            } while (!valid);

            return input.Substring(0, 1).ToUpper();
        }
        private string GetTitleFromUser()
        {
            _outputService.Write("Enter Title: ");
            var title = _inputService.ReadLine();
            _logger.LogInformation("\nUser submitted Title: \"{title}\"", title);

            _logger.LogInformation("\nConfirming this Title does not exist yet...");
            var searchResults = _repository.Search(title);
            var duplicateEntries = searchResults.Where(x => x.Title == title);
            if (duplicateEntries.Any())
            {
                //Error - duplicate entry
                _logger.LogError("\nInvalid Title - identical to existing entry");
                _logger.LogError(duplicateEntries.First().ToString());

                _outputService.WriteLine("Invalid Title - identical to existing entry");
                _outputService.WriteLine(duplicateEntries.First().ToString());
                return "";
            }

            return title;
        }
        private bool ContinueDisplaying()
        {
            _outputService.WriteLine("Hit Enter to continue or ESC to cancel");
            var input = _inputService.ReadKey();
            while (input.Key != ConsoleKey.Enter && input.Key != ConsoleKey.Escape)
            {
                input = _inputService.ReadKey();
                _outputService.WriteLine("Hit Enter to continue or ESC to cancel");
            }

            if (input.Key == ConsoleKey.Escape)
            {
                return false;
            }

            return true;
        }
        private DateTime GetReleaseDateFromUser(string prompt)
        {
            _logger.LogInformation("\nRequesting ReleaseDate from User...");
            _outputService.Write(prompt);
            var input = _inputService.ReadLine();
            var exit = false;
            var releaseDate = DateTime.MaxValue;
            do
            {
                if (DateTime.TryParseExact(input, "MM/dd/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime result))
                {
                    releaseDate = result;
                    exit = true;
                }
                else
                {
                    _logger.LogError("\nUser submitted invalid release date format:  \"{input}\"", input);
                    _outputService.WriteLine($"{input} is invalid.");
                    _outputService.Write("Please try again using the MM/DD/YYYY format, or press q to cancel: ");
                    input = _inputService.ReadLine();
                    if (input.ToLower() == "q")
                    {
                        exit = true;
                    }
                }
            } while (!exit);
            return releaseDate;
        }
        private int GetIDFromUser(string type, string prompt)
        {
            _logger.LogInformation("\nRequesting {type}ID from User...", type);
            var id = -1;
            string input;
            do
            {
                _outputService.Write(prompt);
                input = _inputService.ReadLine();
                try
                {
                    id = Int32.Parse(input);
                }
                catch (Exception e)
                {

                }
                if (id < 0)
                {
                    _logger.LogError("\nUser submitted invalid {type}ID: {input}", type, input);
                    _outputService.WriteLine($"{input} is invalid.");
                }
            } while (id < 0);

            return id;
        }
    }
}
