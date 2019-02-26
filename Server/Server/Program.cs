using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            int port;

            if (args.Length == 1)
                port = int.Parse(args[0]);
            else if (args.Length == 0)
            {
                Console.Write("Enter port: ");
                port = int.Parse(Console.ReadLine());
            }
            else
            {
                Console.WriteLine("Incorrect parameters.");
                Console.WriteLine("Usage:");
                Console.WriteLine("  Server.exe [port]");
                return;
            }

            Server server = new Server(port);
            server.Start();
        }
    }
}
