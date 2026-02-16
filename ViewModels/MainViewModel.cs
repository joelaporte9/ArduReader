using System.ComponentModel;
using System.IO.Ports;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using ArduReader.Models;
using CommunityToolkit.Mvvm.Input;

namespace ArduReader.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {   
        DispatcherTimer? dispatcherTimer;
        int intervalCount = 1;
        public string[] ports {get; set;} = SerialPort.GetPortNames();
        public int [] baudrate {get; set;} = new [] {75, 150, 300, 600, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400};
        public Connection Connection { get; set; } = new Connection();
        public List<string> devicenames {get; set; }
        public ICommand? ConnectCommand { get; }
        public ICommand? DataCommand { get; }
        private void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public  string? message;
        public string? Message
        {
            get => message;
            set
            {
                message = value;
                OnPropertyChanged(nameof(Message));
            }
        }
        public  string? data;
        public string? Data
        {
            get => data;
            set
            {
                data = value;
                OnPropertyChanged(nameof(Data));
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            GetDeviceData getDeviceData = new GetDeviceData();
            devicenames = getDeviceData.GetDeviceNames();
            ConnectCommand = new RelayCommand(Connect);
            DataCommand = new RelayCommand(ReadData);

        }

        public void Connect()
        {            
            try
            {
                Connection.OpenSerialCommunication();
                Message = $"Connected to: {Connection.DeviceName} at:{DateTime.Now}";
            }
            catch (System.Exception)
            {
                Message = $"Unable to connect to COM port. Last attempt: {DateTime.Now}";
            }
        }
        public void ReadData()
        {            
            try
            {
                if(Connection.DeviceName != null)
                {
                    if(Connection.DeviceName.Contains("Arduino"))
                    {
                        dispatcherTimer = new DispatcherTimer();
                        dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                        dispatcherTimer.Interval = TimeSpan.FromSeconds(intervalCount);
                        dispatcherTimer.Start();

                        MessageBox.Show(messageBoxText:"Reading Data!");
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
                MessageBox.Show(messageBoxText:"error:" +e.Message);
            }
        }
        private void dispatcherTimer_Tick(object? sender, EventArgs e)
        {
            bool haveData = false;
            data = String.Empty;
            try
            {
                while(!haveData)
                {
                    data = Connection.serialPort?.ReadLine();
                    if(!string.IsNullOrEmpty(data))
                    {
                        haveData = true;
                    }
                }
                if(haveData)
                {
                    Data += Environment.NewLine;
                }
            }
            catch (System.Exception)
            {
                Console.WriteLine(e);
                MessageBox.Show(messageBoxText:"error:" + e);
            }
        }
        
    }
}
