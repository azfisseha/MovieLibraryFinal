using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieLibrary.Services.IO
{
    public interface IOutputService
    {
        public void Write(string message);

        public void WriteLine(string message);
    }
}
