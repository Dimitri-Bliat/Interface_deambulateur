
using System.Diagnostics;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SciChart.Charting.Visuals;
using ExtendedSerialPort_NS;
using System.IO.Ports;
using static SciChart.Drawing.Utility.PointUtil;


namespace Interface_deambulateur
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ExtendedSerialPort serialPort1;
        Stopwatch sw = new Stopwatch();
        public MainWindow()
        {
            // Set this code once in App.xaml.cs or application startup
            SciChartSurface.SetRuntimeLicenseKey("Fq4JIi+pb9+SIiCgmQXdENKU34fPypzSG8b+ilReUsSk3iehNc/rwLGlnXF2LRSzyC8ZggBuCMEpe4RngbGAIJtAHwmCwU7tPe+hnOeZ6TxqAxF0/b8Az9H6rXjnEsTc0MbS5VzYb8aCGDS42Ck7v0UZkpzOQIDl7+ZBZYmpQKp7vBUovKVbyXPzXQiSDG2FmZQKiJKqYug19TwdKTrr5lDaPbZaFc42DD6ovUAZY+muOzixt5jycavflt2xkM+1SzlRrkIUrZk1BnLEGoXUHKsSL5YedNxr+ltHxmiYUL13mH3JCzkH22EnFhBE4B/pcp4Jd+8XJXfWBHrr52RVdQmhcSpm/f8YwXyXqrP5KmVjyzQyn7PQU9NuAcQEcT0IYeoStmL0y9WO7jejQfzKls7Tzknyw5TgxeKtmF84hQ4kouCRpw9BrSgwssCzoOfxL1MUcs0i+Z9nnyoamZpkSm28aB2+k9cDcBBVygSDavhnzY9IeMcKfqb7z1Dbtnv06a6DRZw+");

            InitializeComponent();

            serialPort1 = new ExtendedSerialPort("COM8", 115200, Parity.None, 8, StopBits.One);
            serialPort1.DataReceived += SerialPort1_DataReceived;
            serialPort1.Open();

            oscilloSpeed.AddOrUpdateLine(0, 200, "CPT1");
            oscilloSpeed.ChangeLineColor(0, Color.FromRgb(255, 0, 0));
            oscilloSpeed.AddOrUpdateLine(1, 200, "CPT2");
            oscilloSpeed.ChangeLineColor(1, Color.FromRgb(0, 255, 0));
            oscilloSpeed.AddOrUpdateLine(2, 200, "CPT3");
            oscilloSpeed.ChangeLineColor(2, Color.FromRgb(0, 0, 255));

            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            aTimer.Interval = 100; // ~ 5 seconds
            aTimer.Enabled = true;
            aTimer.Start();

            sw.Start();
        }

        Random random = new Random();
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            oscilloSpeed.AddPointToLine(0, sw.ElapsedMilliseconds / 1000.0, random.NextDouble() * 3);
            oscilloSpeed.AddPointToLine(1, sw.ElapsedMilliseconds / 1000.0, random.NextDouble() * 5);
            oscilloSpeed.AddPointToLine(2, sw.ElapsedMilliseconds / 1000.0, random.NextDouble() * 1);

            
        }

        public void SerialPort1_DataReceived(object sender, DataReceivedArgs e)
        {
            for (int i = 0; i < e.Data.Length; i++)
            {
                DecodeMessage(e.Data[i]);
                //robot.byteListReceived.Enqueue(e.Data[i]);
            }
            //robot.receivedText += Encoding.ASCII.GetString(e.Data, 0, e.Data.Length);
        }

        public byte CalculateChecksum(Int16 msgFunction, int msgPayloadLength, byte[] msgPayload)
        {

                
            byte checksum = 0xFE;
            checksum = (byte)(msgFunction ^ checksum);

            for (int j = 0; j < msgPayloadLength; j++)
            {
                checksum = (byte)(checksum ^ msgPayload[j]);

            }
            return checksum;
        }

        void UartEncodeAndSendMessage(Int16 msgFunction, Int16 msgPayloadLength, byte[] msgPayload)
        {
            byte start = 0xFE;
            byte[] checksum = { CalculateChecksum((byte)msgFunction, msgPayloadLength, msgPayload) };
            byte[] command = new byte[] { start };
            byte[] codeFunction = BitConverter.GetBytes(msgFunction);
            /*Array.Reverse(codeFunction);*/
            byte[] PayloadLength = BitConverter.GetBytes(msgPayloadLength);
            //Array.Reverse(PayloadLength);
            serialPort1.Write(command, 0, command.Length);
            serialPort1.Write(codeFunction, 0, codeFunction.Length);
            serialPort1.Write(PayloadLength, 0, PayloadLength.Length);
            serialPort1.Write(msgPayload, 0, msgPayload.Length);
            serialPort1.Write(checksum, 0, checksum.Length);
        }

        public enum StateReception
        {
            Waiting,
            FunctionMSB,
            FunctionLSB,
            PayloadLengthMSB,
            PayloadLengthLSB,
            Payload,
            CheckSum
        }

        StateReception rcvState = StateReception.Waiting;
        int msgDecodedFunction = 0;
        int msgDecodedPayloadLength = 0;
        byte[] msgDecodedPayload = { };
        int msgDecodedPayloadIndex = 0;


    private void DecodeMessage(byte c)
        {
            switch (rcvState)
            {
                case StateReception.Waiting:
                    if (c == 0xFE)
                    {
                        rcvState = StateReception.FunctionLSB;
                        
                    }
                    break;
                case StateReception.FunctionMSB:
                    msgDecodedFunction += c;
                    rcvState = StateReception.PayloadLengthLSB;
                    
                    break;
                case StateReception.FunctionLSB:
                    msgDecodedFunction += c;
                    rcvState = StateReception.FunctionMSB;
                    
                    break;
                case StateReception.PayloadLengthMSB:
                    msgDecodedPayloadLength += c;
                    rcvState = StateReception.Payload;
                    Array.Resize(ref msgDecodedPayload, msgDecodedPayload.Length + msgDecodedPayloadLength);
                    break;
                case StateReception.PayloadLengthLSB:
                    msgDecodedPayloadLength += c;
                    rcvState = StateReception.PayloadLengthMSB;
                    break;
                case StateReception.Payload:
                    
                    msgDecodedPayloadIndex += 1;
                    if (msgDecodedPayloadIndex == msgDecodedPayloadLength)
                    {
                        rcvState = StateReception.CheckSum;
                    }
                    msgDecodedPayload[msgDecodedPayloadIndex - 1] = c;

                    break;
                case StateReception.CheckSum:

                    byte calculatedChecksum = CalculateChecksum((byte)msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload);
                    
                    byte receivedChecksum = c;
                    if (calculatedChecksum == receivedChecksum)
                    {
                        
                        rcvState = StateReception.Waiting;

                    }
                    else
                    {
                        rcvState = StateReception.Waiting;
                    }
                    msgDecodedFunction = 0;
                    msgDecodedPayloadLength = 0;
                    msgDecodedPayloadIndex = 0;
                    for (int i = 0; i < msgDecodedPayloadLength; i++)
                    {
                        msgDecodedPayload[i] = 0;
                    }
                    break;
                default:
                    rcvState = StateReception.Waiting;
                    break;
            }
        }
    }
}