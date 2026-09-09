using EntrevistaDesktop.Models;
using EntrevistaDesktop.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;

namespace EntrevistaDesktop.ViewModels;

public class UsuarioViewModel : INotifyPropertyChanged
{
    private readonly UsuarioApiService _usuarioApiService;

        private Usuario? _usuarioSeleccionado;

    public ObservableCollection<Usuario> Usuarios { get; set; }

    public ICommand CargarCommand { get; }

    public ICommand NuevoCommand { get; }

    public ICommand EditarCommand { get; }

    public UsuarioViewModel()
    {
        _usuarioApiService = new UsuarioApiService();
        Usuarios = new ObservableCollection<Usuario>();
        CargarCommand = new RelayCommand(async () => await CargarUsuariosAsync());
        NuevoCommand = new RelayCommand(NuevoUsuarioAsync);
        EditarCommand = new RelayCommand(EditarUsuarioAsync);
    }

    public Usuario? UsuarioSeleccionado
    {
        get => _usuarioSeleccionado;
        set
        {
            _usuarioSeleccionado = value;
            OnPropertyChanged();
        }
    }

    public async Task CargarUsuariosAsync()
    {
        var usuarios = await _usuarioApiService.GetUsuariosAsync();

        Usuarios.Clear();

        foreach (var usuario in usuarios)
        {
            Usuarios.Add(usuario);
        }

        OnPropertyChanged(nameof(Usuarios));
    }

    private Task NuevoUsuarioAsync()
    {
        var ventana = new NuevoUsuarioWindow();

        if (ventana.ShowDialog() == true &&
            ventana.UsuarioCreado != null)
        {
            Usuarios.Add(ventana.UsuarioCreado);
        }

        return Task.CompletedTask;
    }

    private async Task EditarUsuarioAsync()
{
    if (UsuarioSeleccionado == null)
    {
        MessageBox.Show("Selecciona un usuario.");
        return;
    }

    var usuarioActualizado = await _usuarioApiService.ActualizarUsuarioAsync(
        UsuarioSeleccionado.Id,
        UsuarioSeleccionado);

    if (usuarioActualizado != null)
    {
        var index = Usuarios.IndexOf(UsuarioSeleccionado);

        if (index >= 0)
        {
            Usuarios[index] = usuarioActualizado;
        }

        MessageBox.Show("Usuario actualizado correctamente.");
    }
}

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged(
        [CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }
}