using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace pimmsInterface
{
    class PimmsTCPClient
    {
        protected TcpClient pimmsClient;
        public bool Connected = false;
        IPEndPoint ipLocalEndPoint;

        public PimmsTCPClient(String sLocalAddress)
        {
            try
            {
                // Create a new IP end point using the supplied local address
                ipLocalEndPoint = new IPEndPoint(IPAddress.Parse(sLocalAddress), 57343);   
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            } 
        }

        public void Connect(String sServerAddress, int iServerPort)
        {
            try 
            {
                if (!Connected)
                {
                    // connect to the remote server
                    Console.WriteLine("Creating new TcpClient bound to {0}", ipLocalEndPoint.ToString());
                    pimmsClient = new TcpClient(ipLocalEndPoint);
                    Console.WriteLine("Connecting to {0}:{1}", sServerAddress, iServerPort);
                    pimmsClient.Connect(IPAddress.Parse(sServerAddress), iServerPort);
                    Connected = true;
                }
                else
                {
                    Console.WriteLine("Connection attempt to {0}:{1} recieved when already connected", sServerAddress, iServerPort);
                }
                
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }

        public void Close()
        {
            try
            {
                if (Connected)
                {
                    // Close the connection
                    Console.WriteLine("Closing connection");
                    pimmsClient.Close();
                    Connected = false;
                }
                else
                {
                    Console.WriteLine("Unable to close connection, none open");
                }
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            
        }
        ~PimmsTCPClient()
        {
            Console.WriteLine("Aaaargh, I'm closing!");
        }

    }
}
