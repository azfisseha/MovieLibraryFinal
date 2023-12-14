
using MovieLibrary.Services;
using MovieLibrary.Services.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MovieLibraryEntities.Dao;
using MovieLibrary.Dao;
using MovieLibraryEntities.Context;

namespace MovieLibrary
{
    internal class Startup
    {
        public IServiceProvider ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddLogging(builder =>
            {
                //builder.AddConsole();
                builder.AddFile("app.log");
            });


            services.AddTransient<IMainService, MainService>();
            services.AddTransient<IMenuService, MenuService>();
            services.AddTransient<IRepository, Repository>();
            services.AddTransient<IMovieRepository, MovieRepository>();

            services.AddTransient<IOutputService, ConsoleOutputService>();
            services.AddTransient<IInputService, ConsoleInputService>();

            services.AddDbContextFactory<MovieContext>();

            return services.BuildServiceProvider();
        }
    }
}
