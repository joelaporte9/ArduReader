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
using OxyPlot.Wpf;

namespace ArduReader.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {   
        DispatcherTimer? dispatcherTimer;
        int intervalCount = 1;
        public int [] baudrate {get; set;} = [75, 150, 300, 600, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400];
        public bool isConnected;
        public Connection Connection { get; set; } = new Connection();
        public PlotView PlotView { get; set; } = new PlotView();
        public List<string> devicenames {get; set; }
        public ICommand? ConnectCommand { get; }
        public ICommand? CloseCommand { get; }
        public ICommand? DataCommand { get; }
        public ICommand? ExportCommand { get; }
        public ICommand? ImportCommand { get; }
        public ICommand? HelpCommand { get; }

        public ICommand? LogCommand { get; }

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

        public ObservableCollection<string> Data { get; set; } = [];
        public ObservableCollection<string> Logs { get; set; } = [];
        public ObservableCollection<DataPoint> DataPointList1 {get; set;}= [];
        public ObservableCollection<DataPoint> DataPointList2 {get; set;} = [];
        public ObservableCollection<DataPoint> DataPointList3 {get; set;} = [];
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
            LogCommand = new RelayCommand(OpenLogWindow);


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
                    Logs.Add($"Connected: {isConnected} -- {DateTime.Now}");

                }
                else
                {
                    Message = $"Unable to Connect. Last Attempt: {DateTime.Now}";
                }
                
            }
            catch (System.Exception e)
            {
                Logs.Add(e.ToString());
                MessageBox.Show(messageBoxText:"error connecting");
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
                    Logs.Add($"Connected: {isConnected} -- {DateTime.Now}");
                }
                else
                {
                    Message = $"Please Connect to Device";
                    MessageBox.Show(messageBoxText:"Must Be Connected Before Disconnecting.");
                }
               
            }
            catch (System.Exception e)
            {
                Logs.Add(e.ToString());
                MessageBox.Show(messageBoxText:"error disconnecting");
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
                        Logs.Add($"Reading Data! -- {DateTime.Now}");

                    }
                    else
                    {
                        MessageBox.Show(messageBoxText:"Please select the proper device.");
                    }
                }
            }
            catch (System.Exception e)
            {
                Logs.Add(e.ToString());
                MessageBox.Show(messageBoxText:"error with dispatch timer");
            }
        }
        
        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            string pattern = @"[+-]?([0-9]*[.])?[0-9]+"; 
            bool haveData = false;
            string? data = String.Empty;

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
                    MatchCollection dataList = Regex.Matches(data, pattern);
                    CloseConnection();
                    var resultsList = Regex.Matches(data,pattern)
                       .Cast<Match>()
                       .Select(m => m.Value)
                       .ToList();

                    for (int i = 0; i < resultsList.Count; i++)
                    {   
                        DataPointList1.Add(new DataPoint(Convert.ToDouble(i),Convert.ToDouble(resultsList[0])));
                        DataPointList2.Add(new DataPoint(Convert.ToDouble(i),Convert.ToDouble(resultsList[1])));  
                        DataPointList3.Add(new DataPoint(Convert.ToDouble(i),Convert.ToDouble(resultsList[2])));
                    }
                    Data.Add(data);
                    if(DataPointList1 != null && DataPointList2 != null && DataPointList3 != null)
                    {
                        PlotView.InvalidatePlot(true);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Logs.Add(ex.ToString());
                MessageBox.Show(messageBoxText:"error reading device data");
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
                        Logs.Add($"Data saved to {filename}");
                    }
                }
                
            }
            catch (System.Exception e)
            {
                Logs.Add(e.ToString());
                MessageBox.Show(messageBoxText:"error exporting data");
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
                    Logs.Add($"Viewing Imported Data - {DateTime.Now}");

                }
            }
            catch (System.Exception)
            {
                MessageBox.Show(messageBoxText:"error importing data");
            }
        }
        private void OpenHelpWindow()
        {
            try
            {
                HelpWindow helpWindow = new();
                helpWindow.ShowDialog();
            }
            catch (System.Exception e)
            {
                Logs.Add($"{e.ToString()} -- {DateTime.Now}");
                MessageBox.Show(messageBoxText:"error opening help page");
            }
        }
        private void OpenLogWindow()
        {
            try
            {
                LogWindow logWindow = new()
                {
                    DataContext = this
                };
                logWindow.ShowDialog();
            }
            catch (System.Exception)
            {
                MessageBox.Show(messageBoxText:"error opening logs");
            }
        }
        
    }
}