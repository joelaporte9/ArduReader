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
    public class DataGridViewModel : MainViewModel
    {   
        public new ICommand? ConnectCommand { get; }
        public new ICommand? CloseCommand { get; }
        public new ICommand? DataCommand { get; }
        public new ICommand ExportCommand { get; }
        public new DataTable dataTable { get; set; } = new DataTable();

        public DataGridViewModel()
        {
           
            ExportCommand = new RelayCommand(ExportData);           

        }

        private void ExportData()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            
            saveFileDialog.FileName = "ArduDataCSV";
            saveFileDialog.DefaultExt = ".csv";
            saveFileDialog.Filter = "CSV file (*.csv)|*.csv";
            try
            {
                bool? result = saveFileDialog.ShowDialog();

                if (result == true)
                {
                    string filename = saveFileDialog.FileName;
                    if(dataTable != null && filename != null)
                    {
                        using (System.IO.StreamWriter SaveFile = new System.IO.StreamWriter(filename))
                        {
                            foreach(var item in DataGridData)
                            {
                                SaveFile.WriteLine(item.ToString());
                            }
                        }
                        MessageBox.Show(messageBoxText:$"Data saved to {filename}");
                    }
                }
            }
            catch (System.Exception)
            {
                MessageBox.Show(messageBoxText:"error exporting data");
                Logs.Add($"error exporting data -- {DateTime.Now}");
            }
        }
        
       
      
    }
}