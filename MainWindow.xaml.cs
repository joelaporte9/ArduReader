using System.Windows;
using ArduReader.ViewModels;

namespace ArduReader;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private MainViewModel viewModels;
    public MainWindow()
    {
        InitializeComponent();
        viewModels = new MainViewModel();
        DataContext = viewModels;
    }
}

