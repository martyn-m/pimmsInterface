using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace pimmsInterface
{
    /// <summary>
    /// PiMMSTCPClient
    /// Provides an interface for TCP communications with a PiMMS server for trigger and controller messaging.
    /// A separate TCPClient instance must be created for each trigger/controller function, 
    /// each bound to a separate local network interface/IP address.
    ///     one trigger per system
    ///     one controller per train
    /// </summary>
    class PimmsTCPClient
    {
        private TcpClient pimmsClient;
        public bool Connected = false;
        private IPEndPoint ipLocalEndPoint;
        private NetworkStream stream;
        private String sLocalAddress;
        private int iPort;

        /// <summary>
        /// PimmsTCPClient Constructor
        /// </summary>
        /// <param name="sLocalIpAddress">Local IP address to bind this PimmsTCPClient object to</param>
        public PimmsTCPClient(String sLocalIpAddress)
        {
            // Use local port 12345 for the first connection
            iPort = 12345;
            sLocalAddress = sLocalIpAddress;
        }

        /// <summary>
        /// Open the connection to the PiMMS server
        /// </summary>
        /// <param name="sServerAddress">IP address of the remote PiMMS Server</param>
        /// <param name="iServerPort">Remote port of the PiMMS server, must match the value configured in PiMMS in SYSTEM_PARAMETERS/iTcpIpPort (default 57343)</param>
        public void Connect(String sServerAddress, int iServerPort)
        {
            try 
            {
                if (!Connected)
                {
                    // Create an IP endpoint for the local IP/port to use for this connection
                    ipLocalEndPoint = new IPEndPoint(IPAddress.Parse(sLocalAddress), iPort);

                    // Increment the local port number for the next connection to avoid issues with the port staying in TIME_WAIT after a disconnect
                    iPort++;

                    // Create a new TcpClient bound to the specified local endpoint
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
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
        }

        /// <summary>
        /// Sends a message of (PiMMS) type MSG_TRIGGER
        /// Can be either a trigger event message or a trigger poll response message
        /// </summary>
        /// <param name="bOpcode">Single byte message OpCode, 0x02 for trigger event, 0x07 for poll response</param>
        public void SendTriggerMessage(byte bOpcode)
        {
            try
            {
                // trigger message data as raw hex bytes, never needs to vary in Vio system (apart from opcode byte)
                // __MSG_TYPE = 02
                // __MSG_OPCODE = bOpcode
                Byte[] data = { 0x23, 0x21, 0x00, 0x00, 0x00, 0x0f, 0x00, 0x00, 0x00, 0x0c,
                                  0x00, 0x00, 0x00, 0x00, 0x02, 0x02, 0x00, 0x00, 0x00, 0x00, 0x0f };
                // Set the OpCode byte
                data[16] = bOpcode;

                // Write the data to the network stream
                stream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
        }
 
        /// <summary>
        /// Sends a trigger 'train N started' message
        /// The train N started message must be followed within 5s by a trigger event message PimmsTCPClient.SendTriggerMessage(0x02)
        /// Note that train and controller IDs start counting from 0 internally in PiMMS, i.e. for train N send iTrainID (N-1)
        /// </summary>
        /// <param name="iTrainID">integer ID of the train that has just started the ride</param>
        /// <param name="iControllerID">integer ID of the controller seen by the trigger. Can always use 0 in Vio system</param>
        public void SendTrainStartMessage(int iTrainID, int iControllerID)
        {
            try
            {
                // message data as raw hex
                // __MSG_TYPE = 02
                // __MSG_OPCODE = 01
                // train,controller "0,0"
                Byte[] data = { 0x23, 0x21, 0x00, 0x00, 0x00, 0x12, 0x00, 0x00, 0x00, 0x0C, 0x00, 0x00,
                                0x00, 0x00, 0x05, 0x02, 0x01, 0x00, 0x2C, 0x00, 0x00, 0x00, 0x00, 0x12 };
                // set train ID
                data[17] = Encoding.ASCII.GetBytes(iTrainID.ToString())[0];
                // set controller ID
                data[19] = Encoding.ASCII.GetBytes(iControllerID.ToString())[0];

                // Write the data to the network stream
                stream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
        }

        /// <summary>
        /// Send a controller log on message at the end of ride (once all video clips are ready for transfer to
        /// the PiMMS server).
        /// The PiMMS server determines which train the message refers to by the source IP address of the message.
        /// The message content can be a fixed value for the Vio system as the content refers to server-train functionality
        /// not used in the Vio system
        /// </summary>
        public void SendLogOnMessage()
        {
            try
            {
                // log on message data as raw hex
                // __MSG_TYPE = 04
                // __MSG_OPCODE = 00
                // SSID = VioCamera
                // OK = true
                // TEST = false
                // __MSG_STRING = ""
                byte[] data = {
	                0x23, 0x21, 0x00, 0x00, 0x00, 0xBF, 0x00, 0x00, 0x00, 0x1C, 0x00, 0x00,
	                0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x04, 0x00, 0x4F, 0x00, 0x4B, 0x00,
	                0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x00, 0x00, 0x08, 0x00, 0x53, 0x00,
	                0x53, 0x00, 0x49, 0x00, 0x44, 0x00, 0x00, 0x00, 0x0A, 0x00, 0x00, 0x00,
	                0x00, 0x12, 0x00, 0x56, 0x00, 0x69, 0x00, 0x6F, 0x00, 0x43, 0x00, 0x61,
	                0x00, 0x6D, 0x00, 0x65, 0x00, 0x72, 0x00, 0x61, 0x00, 0x00, 0x00, 0x14,
	                0x00, 0x5F, 0x00, 0x5F, 0x00, 0x4D, 0x00, 0x53, 0x00, 0x47, 0x00, 0x5F,
	                0x00, 0x54, 0x00, 0x59, 0x00, 0x50, 0x00, 0x45, 0x00, 0x00, 0x00, 0x02,
	                0x00, 0x00, 0x00, 0x00, 0x04, 0x00, 0x00, 0x00, 0x18, 0x00, 0x5F, 0x00,
	                0x5F, 0x00, 0x4D, 0x00, 0x53, 0x00, 0x47, 0x00, 0x5F, 0x00, 0x53, 0x00,
	                0x54, 0x00, 0x52, 0x00, 0x49, 0x00, 0x4E, 0x00, 0x47, 0x00, 0x00, 0x00,
	                0x0A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x18, 0x00, 0x5F,
	                0x00, 0x5F, 0x00, 0x4D, 0x00, 0x53, 0x00, 0x47, 0x00, 0x5F, 0x00, 0x4F,
	                0x00, 0x50, 0x00, 0x43, 0x00, 0x4F, 0x00, 0x44, 0x00, 0x45, 0x00, 0x00,
	                0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00,
	                0x54, 0x00, 0x45, 0x00, 0x53, 0x00, 0x54, 0x00, 0x00, 0x00, 0x01, 0x00,
	                0x00, 0x00, 0x00, 0x00, 0xBF
                };
                
                // Write the data to the network stream
                stream.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }
        }

        /// <summary>
        /// Close the TCP connection
        /// </summary>
        public void Close()
        {
            try
            {
                if (Connected)
                {
                    // Close the connection
                    Console.WriteLine("Closing connection");
                    stream.Close();
                    pimmsClient.Close();
                    
                    Connected = false;
                }
                else
                {
                    Console.WriteLine("Unable to close connection, none open");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e);
            }                 
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~PimmsTCPClient()
        {
            Console.WriteLine("Aaaargh, I'm being eaten!");
        }

    }
}
