using System;
using System.Collections.Generic;
using System.IO.Ports;
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
using ExtendedSerialPort;
using System.Windows.Threading;

namespace RobotInterface_GONON_MOREAU
{
    public partial class MainWindow : Window
    {
        string receveidText = "";
        ReliableSerialPort serialPort1;
        //string var1;

        public MainWindow()
        {
            InitializeComponent();
            serialPort1 = new ReliableSerialPort("COM15", 115200, Parity.None, 8, StopBits.One);
            serialPort1.OnDataReceivedEvent += SerialPort1_DataReceived;
            serialPort1.Open();
        }

        public void SerialPort1_DataReceived(object sender, DataReceivedArgs e)
        {
            receveidText += Encoding.UTF8.GetString(e.Data, 0, e.Data.Length);
        }

        private void ButtonEnvoyer_Click(object sender, RoutedEventArgs e)
        {
        }

        private void TextBoxEmission_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage(textBoxEmission.Text);
                textBoxEmission.Text = "";
            }
        }

        public void SendMessage(string mess)
        {
            serialPort1.WriteLine("Reçu: " + mess);
            Console.WriteLine("toto");
        }
    }
}