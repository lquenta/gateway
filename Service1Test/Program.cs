using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Service1Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var service = new Service1();
            service.Run(args);
        }
    }
}
