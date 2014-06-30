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

        public PimmsTCPClient(String sLocalAddress)
        {
            try
            {
                Console.WriteLine("Creating new TcpClient bound to {0}", sLocalAddress);
                // Create a new TcpClient bound to the specified local address
                IPEndPoint ipLocalEndPoint = new IPEndPoint(IPAddress.Parse(sLocalAddress), 57343);
                this.pimmsClient = new TcpClient(ipLocalEndPoint);
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

        public bool IsConnected()
        {
            if (this.pimmsClient.Connected == true)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Connect(String sServerAddress, int iServerPort)
        {
            try 
            {
                // connect to the remote server
                this.pimmsClient.Connect(IPAddress.Parse(sServerAddress), iServerPort);  
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
                // Close the connection
                this.pimmsClient.Close();
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

    }
}
