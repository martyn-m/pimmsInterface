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

namespace PimmsInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // PiMMS Server Details
        String sServerIpAddress = PimmsInterface.Properties.Settings.Default.sServerIpAddress;
        int iServerPort = PimmsInterface.Properties.Settings.Default.iServerPort;

        // Local IPs to use for PiMMS comms
        String sTriggerIpAddress = PimmsInterface.Properties.Settings.Default.sTriggerIpAddress;
        String sController1IpAddress = PimmsInterface.Properties.Settings.Default.sController1IpAddress;
        String sController2IpAddress = PimmsInterface.Properties.Settings.Default.sController2IpAddress;
        String sController3IpAddress = PimmsInterface.Properties.Settings.Default.sController3IpAddress;

        // Paths to files
        String sBasePath = PimmsInterface.Properties.Settings.Default.sBasePath;
        String sCameraFolder = PimmsInterface.Properties.Settings.Default.sCameraFolder;
        String sControllerFolder = PimmsInterface.Properties.Settings.Default.sControllerFolder;
        String sSourceFilePath = PimmsInterface.Properties.Settings.Default.sSourceFilePath;
        String sBatteryIniFile = PimmsInterface.Properties.Settings.Default.sBatteryIniFile;
        String sVideoFile = PimmsInterface.Properties.Settings.Default.sVideoFile;
        String sVideoInfFile = PimmsInterface.Properties.Settings.Default.sVideoInfFile;
        
        // Number of Cameras and Controllers to initialise
        int iNumCameras = PimmsInterface.Properties.Settings.Default.iNumberOfCameras;
        int iNumControllers = PimmsInterface.Properties.Settings.Default.iNumberOfControllers;
        int iNumRows = PimmsInterface.Properties.Settings.Default.iNumberOfRows;
        
        // Declare PimmsTCPClient objects    
        PimmsTcpClient pimmsClient;
        

        public MainWindow()
        {
            InitializeComponent();
            
            // Create a new PimmsTCPCLient object to manage communications from various components to a PiMMS server
            pimmsClient = new PimmsTcpClient(sServerIpAddress, iServerPort);
            
        }

        /// <summary>
        /// Re-initialise the camera/controller hot folders after the completion of a 'ride'
        /// 
        /// For each camera hot folder:
        /// Copies a dummy clip0.inf video information file from a source location to the hot folder, 
        ///     must be implemented or the PiMMS server will report a failed video download and abort.
        /// 
        /// Copies a dummy \logs\battery.ini file from a source location to the hot folder,
        ///     not strictly necessary for system functionality, but prevents error mesages in PiMMS
        /// 
        /// Copies in a fake video clip (clip0.mpg) for testing only.
        /// 
        /// For each controller hot folder:
        /// Copies a dummy \logs\battery.ini file from a source location to the hot folder,
        ///     not strictly necessary for system functionality, but prevents error mesages in PiMMS
        ///     
        /// TO DO: put all of this functionality into a class rather than directly in the button click method.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
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
                for (int j = 1; j <= iNumRows; j++)
                {
                    string[] positions = {"a","b"};
                    foreach (string position in positions)
                    {
                        // Copy battery.ini into camera logs folder
                        String sThisCamera = i.ToString() + "-" + j.ToString() + position;
                        String sDestCamBatteryIniPath = System.IO.Path.Combine(sBasePath, sThisCamera, "logs");
                        String sDestCamBatteryIniFile = System.IO.Path.Combine(sDestCamBatteryIniPath, sBatteryIniFile);
                        String sDownloadedVideoFileName = null;

                        try
                        {
                            if (!System.IO.Directory.Exists(sDestCamBatteryIniPath))
                            {
                                System.IO.Directory.CreateDirectory(sDestCamBatteryIniPath);
                            }

                            Console.WriteLine("Copying {0} to {1}", sSourceBatteryIniFile, sDestCamBatteryIniFile);
                            System.IO.File.Copy(sSourceBatteryIniFile, sDestCamBatteryIniFile, true);

                            // find the most recent mp4 file (the newest video) in the folder
                            try
                            {
                                var directory = new DirectoryInfo(System.IO.Path.Combine(sBasePath, sThisCamera));
                                var videoName = directory.GetFiles("*.mp4").OrderByDescending(f => f.LastWriteTime).First();
                                sDownloadedVideoFileName = System.IO.Path.Combine(sBasePath, sThisCamera, videoName.Name);
                            }
                            catch (Exception ex)
                            {
                                // There aren't any mp4 files
                                //Console.WriteLine("Exception: {0}", ex);
                             
                            }
                            
                            // Copy dummy video and inf into camera folder
                            String sDestVideoFile = System.IO.Path.Combine(sBasePath, sThisCamera, sVideoFile);
                            String sDestVideoInfFile = System.IO.Path.Combine(sBasePath, sThisCamera, sVideoInfFile);

                            // Delete clip0.mpg
                            if (DeleteClip0CheckBox.IsChecked == true)
                            {
                                Console.WriteLine("Deleting {0}", sDestVideoFile);
                                System.IO.File.Delete(sDestVideoFile);
                            }

                            if (SampleVideoCheckBox.IsChecked == true)
                            {
                                // Use a dummy source video file
                                Console.WriteLine("Copying {0} to {1}", sSourceVideoFile, sDestVideoFile);
                                System.IO.File.Copy(sSourceVideoFile, sDestVideoFile, true);
                            }
                            else
                            {
                                // Attempt to copy the newest mp4 file in the hot folder to clip0.mpg
                                if (!String.IsNullOrEmpty(sDownloadedVideoFileName))
                                {
                                    Console.WriteLine("Copying {0} to {1}", sDownloadedVideoFileName, sDestVideoFile);
                                    System.IO.File.Copy(sDownloadedVideoFileName, sDestVideoFile, true);

                                    // Delete the old stream video file
                                    if (DeleteStreamCheckBox.IsChecked == true)
                                    {
                                        Console.WriteLine("Deleting {0}", sDownloadedVideoFileName);
                                        System.IO.File.Delete(sDownloadedVideoFileName);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("No mp4 files to copy in {0}", sThisCamera);
                                }
                            }
                            Console.WriteLine("Copying {0} to {1}", sSourceVideoInfFile, sDestVideoInfFile);
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

        private void StartBtn1_Click(object sender, RoutedEventArgs e)
        {
            
            // Send the train start message for train 1
            Console.WriteLine("Sending Train 1 ride start event");
            pimmsClient.StartRide(1, sTriggerIpAddress);
        }

        private void LogOnBtn1_Click(object sender, RoutedEventArgs e)
        {
            // Log On
            Console.WriteLine("Logging On (1)");
            pimmsClient.LogOn(sController1IpAddress);
        }

        private void StartBtn2_Click(object sender, RoutedEventArgs e)
        {
            // Send the train start message for train 1
            Console.WriteLine("Sending Train 2 ride start event");
            pimmsClient.StartRide(2, sTriggerIpAddress);
        }

        private void LogOnBtn2_Click(object sender, RoutedEventArgs e)
        {
            // Log On
            Console.WriteLine("Logging On (2)");
            pimmsClient.LogOn(sController2IpAddress);
        }

        private void StartBtn3_Click(object sender, RoutedEventArgs e)
        {
            // Send the train start message for train 3
            Console.WriteLine("Sending Train 3 ride start event");
            pimmsClient.StartRide(3, sTriggerIpAddress);
        }

        private void LogOnBtn3_Click(object sender, RoutedEventArgs e)
        {
            // Log On
            Console.WriteLine("Logging On (1)");
            pimmsClient.LogOn(sController2IpAddress);
        }
    }
}
