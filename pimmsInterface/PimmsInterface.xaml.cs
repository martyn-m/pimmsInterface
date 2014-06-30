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

namespace pimmsInterface
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TrigChk_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if(TrigChk.IsChecked == true)
            {
                // open a connection to the pimms server
                Console.WriteLine("Connecting Trigger");
            }
            else 
            {
                // disconnect from the pimms server
                Console.WriteLine("Disconnecting Trigger");
            }
        }

        private void TrigPollBtn_Click(object sender, RoutedEventArgs e)
        {
            // Send a poll response message to the pimms server
            // check if we're connected first
            Console.WriteLine("Sending Trigger Poll Response");
        }

        private void TrigEventBtn_Click(object sender, RoutedEventArgs e)
        {
            // Send a Trigger event message to the pimms server
            // if we are connected
            Console.WriteLine("Sending Trigger 1 Event");
        }

        private void TrigT1Btn_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Sending Train 1 ride start event");
        }

        private void TrigT2Btn_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Sending Train 2 ride start event");
        }

        private void ControllerChk_CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (ControllerChk.IsChecked == true)
            {
                // Open a connection to the PiMMS server using the controller IP address
                Console.WriteLine("Connecting Controller");
            }
            else
            {
                // Disconnect the controller 
                Console.WriteLine("Disconnecting Controller");
            }
        }

        private void ConLogOnBtn_Click(object sender, RoutedEventArgs e)
        {
            // If the controller is connected, send a log on message to the PiMMS server
            Console.WriteLine("Logging On");
        }
    }
}
