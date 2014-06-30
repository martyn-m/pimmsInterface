using System;
using System.Collections.Generic;
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
using System.Net;
using System.Net.Sockets;

namespace pimmsInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Initialise endpoint to connect to PiMMS Server
        String sServerIpAddress = "192.168.0.106";
        int iServerPort = 57343;

        // Local IP to use for trigger comms
        String sTriggerIpAddress = "192.168.0.221";
        String sControllerIpAddress = "192.168.0.222";
        PimmsTCPClient pimmsTrigger;
        PimmsTCPClient pimmsController;

        public MainWindow()
        {
            InitializeComponent();
            pimmsTrigger = new PimmsTCPClient(sTriggerIpAddress);
            pimmsController = new PimmsTCPClient(sControllerIpAddress);
        }

        private void TrigChk_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if(TrigChk.IsChecked == true)
            {
                // open a connection to the pimms server
                Console.WriteLine("Connecting Trigger");
                pimmsTrigger.Connect(sServerIpAddress, iServerPort);
            }
            else 
            {
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
            }
            else
            {
                Console.WriteLine("Train 2 start failed: No open connection");
            }
        }

        private void ControllerChk_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (ControllerChk.IsChecked == true)
            {
                // Open a connection to the PiMMS server using the controller IP address
                Console.WriteLine("Connecting Controller");
                pimmsController.Connect(sServerIpAddress, iServerPort);
            }
            else
            {
                // Disconnect the controller 
                Console.WriteLine("Disconnecting Controller");
                pimmsController.Close();
            }
        }

        private void ConLogOnBtn_Click(object sender, RoutedEventArgs e)
        {
            // Send a log on message to the PiMMS server
            if (pimmsController.Connected)
            {
                Console.WriteLine("Logging On");
            }
            else
            {
                Console.WriteLine("Log on failed: no open connection");
            }
            
        }
    }
}
