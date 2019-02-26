using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography;

namespace Server
{
    class Server
    {
        private enum CMD
        {
            GetRSA = 0
        }

        private TcpListener listener; // Listener that will accept clients
        private Dictionary<string, Client> clients = new Dictionary<string, Client>(); // The clients are available now

        public Server(int port)
        {
            listener = new TcpListener(IPAddress.Any, port); // Initialize the listener
        }

        public void Start()
        {
            listener.Start();
            Console.WriteLine("Server startetd.");

            while (true)
            {
                if (listener.Pending())
                    Task.Factory.StartNew(() => handle_client(listener.AcceptSocket())); // Accept new client
                else
                    Thread.Sleep(100); // Whait for clients
            }
        }

        private void handle_client(Socket client)
        {
            /*
             * Connection protocol (RSA key len = 2048 bits):
             *     First byte is a length if client's name.
             *     Next n bytes are the client's name (ASCII).
             *     Next 2 bytes is a length of XML string contains public RSA key.
             *     Next n bytes are the XML string of public key.
             */


            string name = read_ascii(client, read_byte(client)); // Get name
            Console.WriteLine($"New Client '{name}'. IP: {((IPEndPoint)client.RemoteEndPoint).Address.ToString()}");

            byte[] rsa_public_xml = read_bytes(client, read_ushort(client)); // Get XML public key
            Client client_data = new Client(rsa_public_xml, name, client); // Create Client
            clients.Add(name, client_data); // Add this client to the dictionary

            while (is_connected(client))
            {
                if (client.Available > 0) // Something to read
                {
                    byte cmd = read_byte(client);
                    switch (cmd) // Witch command?
                    {
                        case (byte)CMD.GetRSA: /*
                                                * Request for someone's public key.
                                                * 
                                                * Request public key protocol:
                                                *     First byte is a length of a user's name (whouse key if requested)
                                                *     Next n bytes if a name of user.
                                                * 
                                                * Response protocol:
                                                *     First byte is a length of user's publick key (0 if the user doesn't exists).
                                                *     Next n bytes are the public key.
                                                */
                            
                            string u_name = read_ascii(client, read_byte(client)); // Get users'name
                            if (clients.ContainsKey(u_name)) // User is exists
                            {
                                Client u_client = clients[u_name]; // Find a user


                                send_ushort(client, (ushort)u_client.Public_Key_Xml.Length); // Send the length of key
                                client.Send(u_client.Public_Key_Xml); // Send the publick key
                            }
                            else // User does not exists
                                send_ushort(client, 0);
                            break;
                    }
                }
                else
                    Thread.Sleep(100);
            }

            client.Disconnect(false);
            clients.Remove(name);

            Console.WriteLine($"Client '{name}' disconnected. IP: {((IPEndPoint)client.RemoteEndPoint).Address.ToString()}");
        }

        private byte read_byte(Socket sock)
        {
            byte[] buff = new byte[1];
            sock.Receive(buff);
            return buff[0];
        }
        private byte[] read_bytes(Socket sock, uint len)
        {
            byte[] buff = new byte[len];
            sock.Receive(buff);
            return buff;
        }
        private ushort read_ushort(Socket sock)
        {
            byte[] buff = new byte[2];
            sock.Receive(buff);
            return BitConverter.ToUInt16(buff, 0);
        }
        private string read_ascii(Socket sock, uint len)
        {
            byte[] buff = new byte[len];
            sock.Receive(buff);
            return Encoding.ASCII.GetString(buff);
        }

        private void send_ushort(Socket sock, ushort val)
        {
            byte[] buff = BitConverter.GetBytes(val);
            sock.Send(buff);
        }

        bool is_connected(Socket sock)
        {
            return !(sock.Poll(1000, SelectMode.SelectRead) && sock.Available == 0);
        }
    }
}
