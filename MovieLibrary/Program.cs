using System;
using MovieLibrary.Services;
using Microsoft.Extensions.DependencyInjection;

namespace MovieLibrary
{
    class Program
    {
        static void Main(string[] args)
        {
            try 
            {
                var startup = new Startup();
                var serviceProvider = startup.ConfigureServices();
                var service = serviceProvider.GetService<IMainService>();

                service?.Invoke();
            }
            catch (Exception ex) 
            {
                Console.Error.WriteLine(ex);
            }

        }
    }
}
