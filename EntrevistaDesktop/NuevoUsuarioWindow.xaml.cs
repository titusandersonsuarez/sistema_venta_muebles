using EntrevistaDesktop.Models;
using EntrevistaDesktop.Services;
using System.Windows;

namespace EntrevistaDesktop;

public partial class NuevoUsuarioWindow : Window
{
    private readonly UsuarioApiService _usuarioApiService;

    public Usuario? UsuarioCreado { get; private set; }

    public NuevoUsuarioWindow()
    {
        InitializeComponent();
        Owner = Application.Current?.MainWindow;
        _usuarioApiService = new UsuarioApiService();
    }

    private async void Guardar_Click(object sender, RoutedEventArgs e)
    {
        var usuario = new Usuario
        {
            Name = txtNombre.Text,
            Email = txtEmail.Text,
            Activo = chkActivo.IsChecked == true
        };

        try
        {
            UsuarioCreado = await _usuarioApiService.CrearUsuarioAsync(usuario);

            DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error al crear usuario: {ex.Message}",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
    }
}