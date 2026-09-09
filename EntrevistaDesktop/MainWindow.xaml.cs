using EntrevistaDesktop.ViewModels;
using System.Windows;

namespace EntrevistaDesktop;

public partial class MainWindow : Window
{
    private readonly UsuarioViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();

        _viewModel = new UsuarioViewModel();

        DataContext = _viewModel;
    }
}