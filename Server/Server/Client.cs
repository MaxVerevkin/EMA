using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Client
    {
        public string Name { get; set; }
        public RSACryptoServiceProvider RSA { get; set; }
        public byte[] Public_Key_Xml { get; set; }
        public Socket Socket { get; set; }

        public Client(byte[] public_key_xml, string name, Socket socket)
        {
            Public_Key_Xml = public_key_xml;

            RSA = new RSACryptoServiceProvider(2048); // Restore RSA from public key
            RSA.FromXmlString(Encoding.ASCII.GetString(public_key_xml));

            Name = name;
            Socket = socket;
        }
    }
}
