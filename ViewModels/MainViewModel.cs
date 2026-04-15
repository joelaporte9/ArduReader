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
using System.Data;

namespace ArduReader.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {   
        DispatcherTimer? dispatcherTimer;
        int intervalCount = 1;
        public int [] baudrate {get; set;} = [75, 150, 300, 600, 1200, 2400, 4800, 9600, 19200, 38400, 57600, 115200, 230400];
        private bool isConnected;
        private bool haveColumns;

        public Connection Connection { get; set; } = new Connection();
        public DataTable dataTable { get; set; } = new DataTable();
        public List<string> devicenames {get; set; }
        public ICommand? ConnectCommand { get; }
        public ICommand? CloseCommand { get; }
        public ICommand? DataCommand { get; }
        public ICommand? ExportCommand { get; }
        public ICommand? ImportCommand { get; }
        public ICommand? HelpCommand { get; }
        public ICommand? LogCommand { get; }
        public ICommand? VizCommand { get; }

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
        public ObservableCollection<string> DataGridData { get; set; } = [];
        public ObservableCollection<string> Logs { get; set; } = [];
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
            VizCommand = new RelayCommand(OpenVizWindow);

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
                    Logs.Add($"Unable to Connect. Last Attempt: -- {DateTime.Now}");
                }
            }
            catch (System.Exception)
            {
                MessageBox.Show(messageBoxText:"error connecting");
                Logs.Add($"error connecting -- {DateTime.Now}");
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
                    Logs.Add($"Must Be Connected Before Disconnecting. -- {DateTime.Now}");
                }
               
            }
            catch (System.Exception)
            {
                MessageBox.Show(messageBoxText:"error disconnecting");
                Logs.Add($"error disconnecting -- {DateTime.Now}");
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
                        Logs.Add($"Please select the proper device. -- {DateTime.Now}");

                    }
                }
            }
            catch (System.Exception)
            {
                MessageBox.Show(messageBoxText:"error with dispatch timer");
                Logs.Add($"error with dispatch timer. -- {DateTime.Now}");
            }
        }
        
        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            string valuePattern = @"[+-]?([0-9]*[.])?[0-9]+"; 
            string colPattern = @"(\w+)(?=\s*[>:])";
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
                    MatchCollection dataList = Regex.Matches(data, valuePattern);
                    var resultsList = Regex.Matches(data,valuePattern)
                       .Cast<Match>()
                       .Select(m => m.Value)
                       .ToList();

                    MatchCollection columnList = Regex.Matches(data, colPattern);
                    var colList = Regex.Matches(data,colPattern)
                       .Cast<Match>()
                       .Select(m => m.Value)
                       .ToList();
                                    
                    Data.Add(data);

                    if(!haveColumns)
                    {
                        foreach (var item1 in colList)
                        {
                            dataTable.Columns.Add(item1);

                        }
                        haveColumns = true;
                    }
                    if(haveColumns)
                    {
                        dataTable.Rows.Add(resultsList.ToArray());
                    }
                }
            }
            catch (System.Exception)
            {
                MessageBox.Show(messageBoxText:"error reading device data");
                Logs.Add($"error reading device data. -- {DateTime.Now}");
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
            catch (System.Exception)
            {
                MessageBox.Show(messageBoxText:"error exporting data");
                Logs.Add($"error exporting data -- {DateTime.Now}");
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
                Logs.Add($"error importing data. -- {DateTime.Now}");
            }
        }
        private void OpenHelpWindow()
        {
            try
            {
                HelpWindow helpWindow = new();
                helpWindow.ShowDialog();
            }
            catch (System.Exception)
            {
                MessageBox.Show(messageBoxText:"error opening help page");
                Logs.Add($"error opening help page. -- {DateTime.Now}");
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
                logWindow.Show();
            }
            catch (System.Exception)
            {
                MessageBox.Show(messageBoxText:"error opening logs");
            }
        }
        private void OpenVizWindow()
        {
            try
            {
                VizWindow vizWindow = new()
                {
                    DataContext = this
                };
                vizWindow.Show();
            }
            catch (System.Exception)
            {
                MessageBox.Show(messageBoxText:"error opening visualization page");
            }
        }
    }
}