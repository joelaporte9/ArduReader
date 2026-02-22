using System.IO.Ports;
using System.Windows;

namespace ArduReader.Models
{
    public class Connection
    {
        public SerialPort? serialPort;
        public int BaudRate {get; set;}
        public string? DeviceName {get; set;}
        public Connection()
        {
        }
        public void OpenSerialCommunication()
        {
            try
            {   
                if(DeviceName != null)
                {
                    string comport = DeviceName.Substring(0,4);
                    serialPort = new SerialPort(comport, BaudRate);
                    serialPort.Open();
                    serialPort.DiscardInBuffer();
                }
               
            }
            catch (System.Exception)
            {
                MessageBox.Show(messageBoxText:"error establishing serial connection");
            }
            
        }
        public void CloseSerialCommunication()
        {
            try
            {   
                serialPort?.Close();
               
            }
            catch (System.Exception)
            {
                MessageBox.Show(messageBoxText:"error closing serial connection");
            }
            
        }


      
    }
}
