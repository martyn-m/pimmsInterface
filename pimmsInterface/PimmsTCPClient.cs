using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Windows;

namespace PimmsInterface
{
    /// <summary>
    /// PimmsTcpClient
    /// Provides an interface for TCP communications with a PiMMS server for trigger and controller messaging.
    /// </summary>
    class PimmsTcpClient
    {
        private IPEndPoint ipRemoteEndPoint;
        private int iStartPort;
        private int iEndPort;
        private int iCurrentLocalPort;

        /// <summary>
        /// Create a new PimmsTcpClient object for communication with a PiMMS server
        /// </summary>
        /// <param name="sServerAddress">The IP Address of the PiMMS server</param>
        /// <param name="iServerPort">The port the PiMMS server listens on (default 57343)</param>
        public PimmsTcpClient(String sServerAddress, int iServerPort)
        {
            // Use local port 44000 for the first connection, and increment up to iEndPort before re-using local ports.
            iCurrentLocalPort = iStartPort = 44000;
            iEndPort = 44100;

            // set the remote endpoint to connect to the PiMMS Server
            if (!SetRemoteEndpoint(sServerAddress, iServerPort))
            {
                // something went wrong, should prompt user to check settings and exit.
                MessageBoxResult result = MessageBox.Show(
                    "Unable to set the PiMMS Server IP/Port, please check the settings and try again.",
                    "Warning");
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Sets the remote IPEndpoint for use by all connections made by this PimmsTcpClient
        /// </summary>
        /// <param name="ipAddress">The destination IP Address to connect to</param>
        /// <param name="iPort">The destination port</param>
        /// <returns>true if OK, false otherwise</returns>
        private bool SetRemoteEndpoint(String ipAddress, int iPort)
        {
            try
            {
                ipRemoteEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), iPort);
                return true;
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("WARNING: ArgumentNullException while setting server IP address");
                return false;
            }
            catch (ArgumentOutOfRangeException e){
                Console.WriteLine("WARNING: ArgumentOutOfRangeException while setting server port");
                return false;
            }
            
        }

        /// <summary>
        /// Informs the PiMMS server that the specified train has started a ride
        /// </summary>
        /// <param name="iTrain">The train ID (1 to N)</param>
        /// <param name="sIpAddress">The local IP address configured for this train</param>
        /// <returns>true if OK, false otherwise</returns>
        public bool StartRide(int iTrain, String sIpAddress)
        {
            bool result = false;
            try
            {
                using (TcpClient client = Connect(sIpAddress))
                {
                    NetworkStream stream = client.GetStream();
                    SendTrainStartMessage((iTrain - 1), 0, stream);
                    SendTriggerMessage(0x02, stream);
                    return true;
                }
                /* 
                TcpClient client = Connect(sIpAddress);
                SendTrainStartMessage((iTrain - 1), 0, client);
                client = Connect(sIpAddress);
                SendTriggerMessage(0x02, client);
                client.Close();
                return true;
                 */
            }
            catch (Exception e)
            {
                // Something went wrong
                // TO DO: flesh out this exception handler
                Console.WriteLine("WARNING: failed to send ride start message (StartRide)");
                Console.WriteLine("{0}", e);
                return false;
            }
            
        }

        /// <summary>
        /// Informs the PiMMS server that a train has completed the ride and files are ready to download
        /// </summary>
        /// <param name="sIpAddress">The local IP address configured for this train</param>
        /// <returns>true if OK, false otherwise</returns>
        public bool LogOn(String sIpAddress)
        {
            try
            {
                using (TcpClient client = Connect(sIpAddress))
                {
                    NetworkStream stream = client.GetStream();
                    SendLogOnMessage(stream);
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to send log on message (LogOn)");
                Console.WriteLine("{0}", e);
            }
            
            return false;
        }



        /// <summary>
        /// Creates a new TCP connection to the PiMMS server
        /// </summary>
        /// <param name="sServerAddress">The local IP address to use for this connection</param>
        /// <returns>A TcpClient object using the specified local endpoint if successful, null if not</returns>
        private TcpClient Connect(String sIpAddress)
        {
            TcpClient client;
            try 
            {
                // Create an IP endpoint for the local IP/port to use for this connection
                IPEndPoint ipLocalEndPoint = new IPEndPoint(IPAddress.Parse(sIpAddress), iCurrentLocalPort);

                // Increment the local port number for the next connection to avoid issues with the port staying in TIME_WAIT after a disconnect
                if (iCurrentLocalPort >= iEndPort)
                {
                    // Reset back to the start port
                    iCurrentLocalPort = iStartPort;
                }
                else
                {
                    iCurrentLocalPort++;
                }

                // Create a new TcpClient bound to the specified local endpoint
                Console.WriteLine("Creating new TcpClient bound to {0}", ipLocalEndPoint.ToString());
                client = new TcpClient(ipLocalEndPoint);
                    
                // Connect to the remote PiMMS server
                Console.WriteLine("Connecting to PiMMS server {0}", ipRemoteEndPoint.ToString());
                client.Connect(ipRemoteEndPoint);

            }
            catch (Exception e)
            {
                // TO DO: flesh out this exception handler
                Console.WriteLine("Failed to open connection to server");
                Console.WriteLine("Exception: {0}", e);
                client = null;
            }
            return client;
        }

        /// <summary>
        /// Sends a message of (PiMMS) type MSG_TRIGGER
        /// Can be either a trigger event message or a trigger poll response message
        /// </summary>
        /// <param name="bOpcode">Single byte message OpCode, 0x02 for trigger event, 0x07 for poll response</param>
        /// <param name="stream">A NetworkStream object to send the data on</param>
        /// <returns>true if OK, false otherwise</returns>
        private bool SendTriggerMessage(byte bOpcode, NetworkStream stream)
        {
            bool result = false;
            // trigger message data as raw hex bytes, never needs to vary in Vio system (apart from opcode byte)
            // __MSG_TYPE = 02
            // __MSG_OPCODE = bOpcode
            Byte[] data = { 0x23, 0x21, 0x00, 0x00, 0x00, 0x0f, 0x00, 0x00, 0x00, 0x0c,
                                  0x00, 0x00, 0x00, 0x00, 0x02, 0x02, 0x00, 0x00, 0x00, 0x00, 0x0f };
            // Set the OpCode byte
            data[16] = bOpcode;

            try
            {
                // Write the data to the network stream
                stream.Write(data, 0, data.Length);
                result = true;            
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to send trigger message");
                Console.WriteLine("Exception: {0}", e);
            }
            return result;
        }
 
        /// <summary>
        /// Sends a trigger 'train N started' message
        /// The train N started message must be followed within 5s by a trigger event message PimmsTCPClient.SendTriggerMessage(0x02)
        /// Note that train and controller IDs start counting from 0 internally in PiMMS, i.e. for train N send iTrainID (N-1)
        /// </summary>
        /// <param name="iTrainID">integer ID of the train that has just started the ride</param>
        /// <param name="iControllerID">integer ID of the controller seen by the trigger. Can always use 0 in Vio system</param>
        /// <param name="client">A TcpClient object to send the message with</param>
        /// <returns>true if OK, false otherwise</returns>
        private bool SendTrainStartMessage(int iTrainID, int iControllerID, NetworkStream stream)
        {
            bool result = false;
            
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
            
            try
            {
                // Write the data to the network stream
                stream.Write(data, 0, data.Length);             
                result = true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to send train start message");
                Console.WriteLine("Exception: {0}", e);
            }
            return result;
        }

        /// <summary>
        /// Send a controller log on message at the end of ride (once all video clips are ready for transfer to
        /// the PiMMS server).
        /// The PiMMS server determines which train the message refers to by the source IP address of the message.
        /// The message content can be a fixed value for the Vio system as the content refers to server-train functionality
        /// not used in the Vio system
        /// </summary>
        /// <param name="client">A TcpClient object to send the message with</param>
        /// <returns>true if OK, false otherwise</returns>
        private bool SendLogOnMessage(NetworkStream stream)
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
            
            try
            {
                // Write the data to the network stream
                stream.Write(data, 0, data.Length);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to send log on message");
                Console.WriteLine("Exception: {0}", e);
                return false;
            }
            
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~PimmsTcpClient()
        {
            Console.WriteLine("Aaaargh, I'm being eaten!");
        }

    }
}
