using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace TestClient
{
    class Server
    {
        private enum CMD
        {
            GetRSA = 0
        }

        private TcpClient client;
        private NetworkStream ns;

        private RSACryptoServiceProvider rsa;

        public Server(string ip, int port)
        {
            client = new TcpClient(ip, port); // Init TcpClient
            ns = client.GetStream();

            rsa = new RSACryptoServiceProvider(2048); // Generate RSA keys
        }
        
        public void send_short_ascii(string str) // Send ascii string that are less than 257 symbols
        {
            ns.WriteByte((byte)str.Length);
            byte[] buff = Encoding.ASCII.GetBytes(str);
            ns.Write(buff, 0, buff.Length);
        }

        public void send_rsa() // Send RSA public key
        {
            string key = rsa.ToXmlString(false);

            byte[] buff = BitConverter.GetBytes((ushort)key.Length);
            ns.Write(buff, 0, buff.Length);

            buff = Encoding.ASCII.GetBytes(key);
            ns.Write(buff, 0, buff.Length);
        }

        public RSACryptoServiceProvider GetRSA(string user) // Get someones public RSA
        {
            ns.WriteByte((byte)CMD.GetRSA);
            send_short_ascii(user); // Send user's name

            ushort key_len = read_ushort();
            if (key_len == 0)
                return null;
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(2048); // Restore RSA
            rsa.FromXmlString(read_ascii(key_len));

            return rsa;
        }

        public NetworkStream GetStream() // Return network stream
        {
            return ns;
        }
        public RSACryptoServiceProvider GetRSA() // Return RSA
        {
            return rsa;
        }

        public void Close()
        {
            ns.Close();
            client.Close();
        }

        private ushort read_ushort()
        {
            byte[] buff = new byte[2];
            ns.Read(buff, 0, 2);
            return BitConverter.ToUInt16(buff, 0);
        }
        private string read_ascii(uint len)
        {
            byte[] buff = new byte[len];
            ns.Read(buff, 0, (int)len);
            return Encoding.ASCII.GetString(buff);
        }
    }
}
