using System.Windows;
using System.Windows.Threading;
using ArduReader.Models;
using ArduReader.ViewModels;

namespace ArduReader;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    
    private MainViewModel _viewModels;
    
    public MainWindow()
    {
        InitializeComponent();
    
        _viewModels = new MainViewModel();
        DataContext = _viewModels;

       
    }
}

