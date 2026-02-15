using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows;
using System.Windows.Threading;

namespace ArduReader.Models
{
    public class Connection
    {
        public SerialPort? serialPort;
        public string? ComPort {get; set;}
        public int BaudRate {get; set;}
        public Connection()
        {
        }
        public void OpenSerialCommunication()
        {
            try
            {
                serialPort = new SerialPort(ComPort, BaudRate);
                serialPort.Open();
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
                MessageBox.Show(messageBoxText:"error:" +e.Message);
            }
            
        }
      
    }
}
