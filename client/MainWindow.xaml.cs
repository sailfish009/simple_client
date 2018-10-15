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
using System.Threading;

namespace client
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

        private void connect_Click(object sender, RoutedEventArgs e)
        {
            string ipaddress = "192.168.0.100";
            int port = 5555;
            AsyncClient.connect(ipaddress, port);
        }


        private void send1_Click(object sender, RoutedEventArgs e)
        {
            byte[] byteData = Encoding.ASCII.GetBytes("This is a test 1<EOF>");
            AsyncClient.send(ref byteData);
        }

        private void send2_Click(object sender, RoutedEventArgs e)
        {
            byte[] byteData = Encoding.ASCII.GetBytes("This is a test 2<EOF>");
            AsyncClient.send(ref byteData);
        }

        private void close_Click(object sender, RoutedEventArgs e)
        {
            AsyncClient.close();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            AsyncClient.init();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
        }


    }
}
