using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Server s = new Server("109.120.151.26", 1234);
            //Server s = new Server("127.0.0.1", 1234);
            //NetworkStream ns = s.GetStream();

            Console.Write("Enter the name: ");
            string name = Console.ReadLine();

            s.send_short_ascii(name); // Send the name
            s.send_rsa(); // Send rsa public key

            Console.Write("Write to: ");
            name = Console.ReadLine();

            RSACryptoServiceProvider rsa = s.GetRSA(name);
            if (rsa == null)
                Console.WriteLine("no user");
            else
                Console.WriteLine(rsa.ToXmlString(false));

            s.Close();
        }
    }
}
