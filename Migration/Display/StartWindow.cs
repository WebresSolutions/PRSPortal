using Terminal.Gui;

namespace Migration.Display;

public class StartWindow : Window
{
    public StartWindow(Func<bool> initapp)
    {
        Title = "PRS Database Migration [MySQL ---> PgSQL]";

        Button btnInit = new()
        {
            Text = "Init",
            X = Pos.Center(),
            IsDefault = true,
            Y = Pos.Center()
        };

        btnInit.Clicked += () =>
        {
            if (initapp.Invoke())
            {
                // Remove the current window
                Application.Top.Remove(this);

                // Create and show a new view/window
                Window migrationWindow = new()
                {
                    Title = "Migration Progress",
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(),
                    Height = Dim.Fill()
                };

                // Add your migration UI components here
                Label statusLabel = new()
                {
                    Text = "Migration initialized successfully!",
                    X = Pos.Center(),
                    Y = Pos.Center()
                };

                migrationWindow.Add(statusLabel);
                Application.Top.Add(migrationWindow);

                // Refresh the display
                Application.Top.SetNeedsDisplay();
            }
            else
            {
                btnInit.Text = "Failed to Init";
                Application.Shutdown();
            }
        };

        Add(btnInit);
    }
}

public class MigrationWindow : View
{

}
