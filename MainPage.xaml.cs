using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SachaApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void NavigateToGif_Clicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(AppShell.GifPageRoute);
    }
}