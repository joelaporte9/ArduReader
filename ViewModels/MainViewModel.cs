using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Windows.Threading;
using ArduReader.Models;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using ArduReader.Windows;
using OxyPlot;

namespace ArduReader.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {   
        DispatcherTimer? dispatcherTimer;
        int intervalCount = 1;
        public int [] baudrate {get; set;} = new [] {75, 150, 300, 600, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400};
        public bool isConnected;
        public Connection Connection { get; set; } = new Connection();
        public List<string> devicenames {get; set; }
        public ICommand? ConnectCommand { get; }
        public ICommand? CloseCommand { get; }
        public ICommand? DataCommand { get; }
        public ICommand? ExportCommand { get; }
        public ICommand? ImportCommand { get; }
        public ICommand? HelpCommand { get; }
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
        public ObservableCollection<string> Data { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<DataPoint> DataPointList1 {get; set;} = new ObservableCollection<DataPoint>();
        public ObservableCollection<DataPoint> DataPointList2 {get; set;} = new ObservableCollection<DataPoint>();
        public ObservableCollection<DataPoint> DataPointList3 {get; set;} = new ObservableCollection<DataPoint>();
        public event PropertyChangedEventHandler? PropertyChanged;

        public MainViewModel()
        {
            GetDeviceData getDeviceData = new GetDeviceData();
            devicenames = getDeviceData.GetDeviceNames();

            ConnectCommand = new RelayCommand(Connect);
            DataCommand = new RelayCommand(ReadData);
            ExportCommand = new RelayCommand(ExportData);
            ImportCommand = new RelayCommand(ImportData);
            CloseCommand = new RelayCommand(CloseConnection);
            HelpCommand = new RelayCommand(OpenHelpWindow);

        }

        
        public void Connect()
        {            
            try
            {
                if(Connection.DeviceName != null)
                {
                    Connection.OpenSerialCommunication();
                    Message = $"Connected - {Connection.DeviceName} At: {DateTime.Now}";
                    isConnected = true;
                }
                else
                {
                    Message = $"Unable to Connect. Last Attempt: {DateTime.Now}";
                }
                
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
                MessageBox.Show(messageBoxText:"error:" + e);
            }
        }

        public void CloseConnection()
        {            
            try
            {
                if(isConnected == true)
                {
                    Connection.CloseSerialCommunication();
                    Message = $"Disconnected At: {DateTime.Now} - Export Data ->";
                    isConnected = false;
                }
                else
                {
                    Message = $"Please Connect to Device";
                    MessageBox.Show(messageBoxText:"Must Be Connected Before Disconnecting.");
                }
               
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
                MessageBox.Show(messageBoxText:"Error:" +e.Message);
            }
        }
        public void ReadData()
        {            
            try
            {   
                if(Connection.DeviceName != null && isConnected == true)
                {
                    if(Connection.DeviceName.Contains("Arduino"))
                    {
                        dispatcherTimer = new DispatcherTimer();
                        dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
                        dispatcherTimer.Interval = TimeSpan.FromSeconds(intervalCount);
                        dispatcherTimer.Start();

                        MessageBox.Show(messageBoxText:"Reading Data!");
                    }
                    else
                    {
                        MessageBox.Show(messageBoxText:"Please select the proper device.");
                    }
                }
            }
            catch (System.Exception e)
            {
                Console.WriteLine(e);
                MessageBox.Show(messageBoxText:"error:" +e.Message);
            }
        }
        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            string pattern = @"[+-]?([0-9]*[.])?[0-9]+"; 
            bool haveData = false;
            string? data = String.Empty;
            var model = new PlotModel { Title = "My Plot" };

            try
            {
                while(!haveData && isConnected == true)
                {
                    data = Connection.serialPort?.ReadLine();
                    if(!string.IsNullOrWhiteSpace(data))
                    {
                        haveData = true;
                    }
                }
                if(haveData && data != null)
                {
                    MatchCollection matches = Regex.Matches(data, pattern);
                    for (int i = 0; i < matches.Count; i++)
                    {
                        double value = i; 
                        DataPointList1.Add(new DataPoint(value,value));
                    }
                    Data.Add(data);
                }

            }
            catch (System.Exception )
            {
                Console.WriteLine(e);
                MessageBox.Show(messageBoxText:"error:" + e);
            }
        }

        private void ExportData()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            
            saveFileDialog.FileName = "ArduData";
            saveFileDialog.DefaultExt = ".txt";
            saveFileDialog.Filter = "Text documents (.txt)|*.txt";
            try
            {
                bool? result = saveFileDialog.ShowDialog();

                if (result == true)
                {
                    string filename = saveFileDialog.FileName;
                    if(Data != null && filename != null)
                    {
                        using (System.IO.StreamWriter SaveFile = new System.IO.StreamWriter(filename))
                        {
                            foreach(var item in Data)
                            {
                                SaveFile.WriteLine(item.ToString());
                            }
                        }
                        MessageBox.Show(messageBoxText:$"Data saved to {filename}");
                    }
                }
                
            }
            catch (System.Exception e)
            {
                
                Console.WriteLine(e);
                MessageBox.Show(messageBoxText:"error:" + e);
            }
        }
        private void ImportData()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            try
            {
                Data.Clear();
                bool? response = openFileDialog.ShowDialog();
                if(response != null)
                {
                    string filepath = openFileDialog.FileName;
                    var fileContents = File.ReadLines(filepath);
                    
                    foreach(var items in fileContents)
                    {
                        Data.Add(items);
                    }
                    Message = $"Viewing Imported Data - {DateTime.Now}";
                }
            }
            catch (System.Exception e)
            {
                
                Console.WriteLine(e);
                MessageBox.Show(messageBoxText:"error:" + e);
            }
        }
        private void OpenHelpWindow()
        {
            try
            {
                HelpWindow helpWindow = new HelpWindow();
                helpWindow.ShowDialog();
            }
            catch (System.Exception e)
            {
                
                Console.WriteLine(e);
                MessageBox.Show(messageBoxText:"error:" + e);
            }
        }
        
    }
}


