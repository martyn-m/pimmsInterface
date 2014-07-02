using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;

namespace pimmsInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // PiMMS Server Details
        String sServerIpAddress = pimmsInterface.Properties.Settings.Default.sServerIpAddress;
        int iServerPort = pimmsInterface.Properties.Settings.Default.iServerPort;

        // Local IPs to use for PiMMS comms
        String sTriggerIpAddress = pimmsInterface.Properties.Settings.Default.sTriggerIpAddress;
        String sController1IpAddress = pimmsInterface.Properties.Settings.Default.sController1IpAddress;
        String sController2IpAddress = pimmsInterface.Properties.Settings.Default.sController2IpAddress;

        // Paths to files
        String sBasePath = pimmsInterface.Properties.Settings.Default.sBasePath;
        String sCameraFolder = pimmsInterface.Properties.Settings.Default.sCameraFolder;
        String sControllerFolder = pimmsInterface.Properties.Settings.Default.sControllerFolder;
        String sSourceFilePath = pimmsInterface.Properties.Settings.Default.sSourceFilePath;
        String sBatteryIniFile = pimmsInterface.Properties.Settings.Default.sBatteryIniFile;
        String sVideoFile = pimmsInterface.Properties.Settings.Default.sVideoFile;
        String sVideoInfFile = pimmsInterface.Properties.Settings.Default.sVideoInfFile;
        
        // Number of Cameras and Controllers to initialise
        int iNumCameras = pimmsInterface.Properties.Settings.Default.iNumberOfCameras;
        int iNumControllers = pimmsInterface.Properties.Settings.Default.iNumberOfControllers;
        
        // Declare PimmsTCPClient objects    
        PimmsTCPClient pimmsTrigger;
        PimmsTCPClient pimmsController1;
        PimmsTCPClient pimmsController2;

        // Create a timer to periodically send trigger poll responses
        Timer triggerTimer;
        

        public MainWindow()
        {
            InitializeComponent();
            
            pimmsTrigger = new PimmsTCPClient(sTriggerIpAddress); 
            pimmsController1 = new PimmsTCPClient(sController1IpAddress);
            pimmsController2 = new PimmsTCPClient(sController2IpAddress);

            // Set up the timer for periodic trigger poll responses, but leave it disabled
            triggerTimer = new Timer();
            triggerTimer.Elapsed += new ElapsedEventHandler(OnTriggerTimer);
            triggerTimer.Interval = 45000;
            triggerTimer.Enabled = false;
        }

        private void OnTriggerTimer(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Sending periodic trigger poll response");
            pimmsTrigger.SendTriggerMessage(0x07);
        }

        private void TrigChk_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if(TrigChk.IsChecked == true)
            {
                // open a connection to the pimms server
                Console.WriteLine("Connecting Trigger");
                pimmsTrigger.Connect(sServerIpAddress, iServerPort);

                // enable the poll response timer
                triggerTimer.Enabled = true;
            }
            else 
            {
                // disable the poll response timer
                triggerTimer.Enabled = false;

                // disconnect from the pimms server
                Console.WriteLine("Disconnecting Trigger");
                pimmsTrigger.Close();
            }
        }

        private void TrigPollBtn_Click(object sender, RoutedEventArgs e)
        {
            // Send a poll response message to the pimms server
            if (pimmsTrigger.Connected)
            {
                Console.WriteLine("Sending Trigger Poll Response");
                //pimmsTrigger.SendTriggerPollResponse();
                pimmsTrigger.SendTriggerMessage(0x07);
            }
            else
            {
                Console.WriteLine("Poll response failed: No open connection");
            }
        }

        private void TrigEventBtn_Click(object sender, RoutedEventArgs e)
        {
            // Send a Trigger event message to the pimms server
            if (pimmsTrigger.Connected)
            {
                Console.WriteLine("Sending Trigger 1 Event");
                //pimmsTrigger.SendTriggerEvent();
                pimmsTrigger.SendTriggerMessage(0x02);
            }
            else
            {
                Console.WriteLine("Trigger 1 event failed: No open connection");
            }
        }

        private void TrigT1Btn_Click(object sender, RoutedEventArgs e)
        {
            if (pimmsTrigger.Connected)
            {
                Console.WriteLine("Sending Train 1 ride start event");
                pimmsTrigger.SendTrainStartMessage(0, 0);
            }
            else
            {
                Console.WriteLine("Train 1 start failed: No open connection");
            }
        }

        private void TrigT2Btn_Click(object sender, RoutedEventArgs e)
        {
            if (pimmsTrigger.Connected)
            {
                Console.WriteLine("Sending Train 2 ride start event");
                pimmsTrigger.SendTrainStartMessage(1, 0);
            }
            else
            {
                Console.WriteLine("Train 2 start failed: No open connection");
            }
        }

        private void Controller1Chk_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (Controller1Chk.IsChecked == true)
            {
                // Open a connection to the PiMMS server using the controller IP address
                Console.WriteLine("Connecting Controller 1");
                pimmsController1.Connect(sServerIpAddress, iServerPort);
            }
            else
            {
                // Disconnect the controller 
                Console.WriteLine("Disconnecting Controller 1");
                pimmsController1.Close();
            }
        }

        private void Controller1LogOnBtn_Click(object sender, RoutedEventArgs e)
        {
            // Send a log on message to the PiMMS server
            if (pimmsController1.Connected)
            {
                Console.WriteLine("Logging On (1)");
                pimmsController1.SendLogOnMessage();
            }
            else
            {
                Console.WriteLine("Log on (1) failed: no open connection");
            }
            
        }
        private void Controller2Chk_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (Controller2Chk.IsChecked == true)
            {
                // Open a connection to the PiMMS server using the controller IP address
                Console.WriteLine("Connecting Controller 2");
                pimmsController2.Connect(sServerIpAddress, iServerPort);
            }
            else
            {
                // Disconnect the controller 
                Console.WriteLine("Disconnecting Controller 2");
                pimmsController2.Close();
            }
        }

        private void Controller2LogOnBtn_Click(object sender, RoutedEventArgs e)
        {
            // Send a log on message to the PiMMS server
            if (pimmsController2.Connected)
            {
                Console.WriteLine("Logging On (2)");
                pimmsController2.SendLogOnMessage();
            }
            else
            {
                Console.WriteLine("Log on (2) failed: no open connection");
            }
            
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            // Re-create all the dummy files in the FTP folder structure

            String sSourceBatteryIniFile = System.IO.Path.Combine(sSourceFilePath, "logs", sBatteryIniFile);
            String sSourceVideoFile = System.IO.Path.Combine(sSourceFilePath,sVideoFile);
            String sSourceVideoInfFile = System.IO.Path.Combine(sSourceFilePath,sVideoInfFile);

            for (int i = 1; i <= iNumControllers; i++)
            {
                // Copy battery.ini into controller logs folder
                String sThisController = sControllerFolder + i.ToString();
                String sDestBatteryIniPath = System.IO.Path.Combine(sBasePath, sThisController, "logs");
                String sDestBatteryIniFile = System.IO.Path.Combine(sDestBatteryIniPath, sBatteryIniFile);
                try
                {
                    if (!System.IO.Directory.Exists(sDestBatteryIniPath))
                    {
                        System.IO.Directory.CreateDirectory(sDestBatteryIniPath);
                    }

                    Console.WriteLine("Copying {0} to {1}", sSourceBatteryIniFile, sDestBatteryIniFile);
                    System.IO.File.Copy(sSourceBatteryIniFile, sDestBatteryIniFile, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: {0}", ex);
                }
            }
            for (int i = 1; i <= iNumCameras; i++)
            {
                // Copy battery.ini into camera logs folder
                String sThisCamera = sCameraFolder + i.ToString();
                String sDestBatteryIniPath = System.IO.Path.Combine(sBasePath, sThisCamera, "logs");
                String sDestBatteryIniFile = System.IO.Path.Combine(sDestBatteryIniPath, sBatteryIniFile);
                
                try
                {
                    if (!System.IO.Directory.Exists(sDestBatteryIniPath))
                    {
                        System.IO.Directory.CreateDirectory(sDestBatteryIniPath);
                    }

                    Console.WriteLine("Copying {0} to {1}", sSourceBatteryIniFile, sDestBatteryIniFile);
                    System.IO.File.Copy(sSourceBatteryIniFile, sDestBatteryIniFile, true);

                    // Copy dummy video and inf into camera folder
                    String sDestVideoFile = System.IO.Path.Combine(sBasePath, sThisCamera, sVideoFile);
                    String sDestVideoInfFile = System.IO.Path.Combine(sBasePath, sThisCamera, sVideoInfFile);
                    Console.WriteLine("Copying {0} to {1}", sSourceVideoFile, sDestVideoFile);
                    System.IO.File.Copy(sSourceVideoFile, sDestVideoFile, true);
                    System.IO.File.Copy(sSourceVideoInfFile, sDestVideoInfFile, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: {0}", ex);
                }
            }          
        }
    }
}
