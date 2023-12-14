using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MovieLibraryEntities.Models;

namespace MovieLibraryEntities.Context
{
    public class MovieContext : DbContext
    {
        private readonly ILogger<MovieContext> _logger;
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieGenre> MovieGenres { get; set; }
        public DbSet<Occupation> Occupations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<UserMovie> UserMovies { get; set; }

        
        public MovieContext(ILogger<MovieContext> logger)
        {
            _logger = logger;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            optionsBuilder
                .LogTo(action => _logger.LogInformation(action), LogLevel.Information)
                .UseLazyLoadingProxies()
                .UseSqlServer(configuration.GetConnectionString("MovieContext"));
        }
    }
}