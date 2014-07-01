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
        NetworkStream stream;
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
                    // Create a TCPClient bound to the specified local interface
                    Console.WriteLine("Creating new TcpClient bound to {0}", ipLocalEndPoint.ToString());
                    pimmsClient = new TcpClient(ipLocalEndPoint);

                    // Connect to the remote PiMMS server
                    Console.WriteLine("Connecting to {0}:{1}", sServerAddress, iServerPort);
                    pimmsClient.Connect(IPAddress.Parse(sServerAddress), iServerPort);

                    // Get a client stream for reading and writing
                    stream = pimmsClient.GetStream();

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

        // Send a trigger message
        public void SendTriggerMessage(byte bOpcode)
        {
            try
            {
                // trigger data as raw hex, never varies
                Byte[] data = { 0x23, 0x21, 0x00, 0x00, 0x00, 0x0f, 0x00, 0x00, 0x00, 0x0c,
                                  0x00, 0x00, 0x00, 0x00, 0x02, 0x02, 0x00, 0x00, 0x00, 0x00, 0x0f };
                data[16] = bOpcode;

                // Write the data to the network stream
                stream.Write(data, 0, data.Length);
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
 
        // Send a train start message
        public void SendTrainStartMessage(int iTrainID, int iControllerID)
        {
            try
            {
                // Base data string
                Byte[] data = { 0x23, 0x21, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00,
                                0x00, 0x00, 0x05, 0x02, 0x01, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x00, 0x12 };
                // train ID
                data[17] = Encoding.ASCII.GetBytes(iTrainID.ToString())[0];
                // controller ID
                data[19] = Encoding.ASCII.GetBytes(iControllerID.ToString())[0];

                // Write the data to the network stream
                stream.Write(data, 0, data.Length);
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

        // Send a controller log on message
        public void SendLogOnMessage()
        {
            try
            {
                // raw hex data
                byte[] data = {
                                	0x23, 0x21, 0x00, 0x00, 0x00, 0xB1, 0x00, 0x00, 0x00, 0x1C, 0x00, 0x00,
   	                                0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x04, 0x00, 0x4F, 0x00, 0x4B, 0x00,
                                	0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x08, 0x00, 0x53, 0x00,
                                	0x53, 0x00, 0x49, 0x00, 0x44, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x00, 0x00,
	                                0x00, 0x16, 0x00, 0x54, 0x00, 0x50, 0x00, 0x54, 0x00, 0x53, 0x00, 0x2D,
	                                0x00, 0x56, 0x00, 0x69, 0x00, 0x64, 0x00, 0x65, 0x00, 0x6F, 0x00, 0x32,
	                                0x00, 0x00, 0x00, 0x14, 0x00, 0x5F, 0x00, 0x5F, 0x00, 0x4D, 0x00, 0x53,
	                                0x00, 0x47, 0x00, 0x5F, 0x00, 0x54, 0x00, 0x59, 0x00, 0x50, 0x00, 0x45,
	                                0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00,
	                                0x18, 0x00, 0x5F, 0x00, 0x5F, 0x00, 0x4D, 0x00, 0x53, 0x00, 0x47, 0x00,
	                                0x5F, 0x00, 0x53, 0x00, 0x54, 0x00, 0x52, 0x00, 0x49, 0x00, 0x4E, 0x00,
	                                0x47, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	                                0x00, 0x18, 0x00, 0x5F, 0x00, 0x5F, 0x00, 0x4D, 0x00, 0x53, 0x00, 0x47,
	                                0x00, 0x5F, 0x00, 0x4F, 0x00, 0x50, 0x00, 0x43, 0x00, 0x4F, 0x00, 0x44,
	                                0x00, 0x45, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	                                0x00, 0x00, 0xB1
                };
                
                // Write the data to the network stream
                stream.Write(data, 0, data.Length);
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
