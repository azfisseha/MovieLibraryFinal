using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieLibrary.Services.IO
{
    public interface IInputService
    {
        public char Read();
        public string ReadLine();

        public ConsoleKeyInfo ReadKey();
    }
}
