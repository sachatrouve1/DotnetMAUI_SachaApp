namespace SachaApp.View;

public partial class AppShell : Shell
{
    public const string GifPageRoute = "gif";

    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(GifPageRoute, typeof(GifPage));
    }
}
