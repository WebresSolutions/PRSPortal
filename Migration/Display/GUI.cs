using Terminal.Gui;

namespace Migration.Display;

public class GUI : Window
{
    public GUI(Func<bool, bool> initapp, Action<Action<MigrationProgress>> startMigration)
    {
        Title = "PRS Database Migration [MySQL ---> PgSQL]";

        Button btnInit = new()
        {
            Text = "Start Database Migration W reset.",
            X = Pos.Center(),
            Y = Pos.Center(),
            IsDefault = true
        };

        Button btnInitWithoutDbReset = new()
        {
            Text = "Start Database Migration WO reset.",
            X = Pos.Center(),
            Y = Pos.Center() - 4,
            IsDefault = true
        };

        void ButtonClicked(bool reset)
        {
            if (initapp.Invoke(reset))
            {
                // Remove the current window
                Application.Top.Remove(this);

                // Create and show a new view/window for migration
                Window migrationWindow = new()
                {
                    Title = "Migration Progress",
                    X = 0,
                    Y = 0,
                    Width = Dim.Fill(),
                    Height = Dim.Fill()
                };

                // Add migration UI components
                Label stepLabel = new()
                {
                    Text = "Initializing migration...",
                    X = 2,
                    Y = 2,
                    Width = Dim.Fill() - 4
                };

                Label statusLabel = new()
                {
                    Text = "Preparing...",
                    X = 2,
                    Y = 4,
                    Width = Dim.Fill() - 4
                };

                ProgressBar progressBar = new()
                {
                    X = 2,
                    Y = 6,
                    Width = Dim.Fill() - 4,
                    Height = 1
                };

                Label percentageLabel = new()
                {
                    Text = "0%",
                    X = Pos.Center(),
                    Y = 8,
                    Width = 10
                };

                Label countLabel = new()
                {
                    Text = "0 / 0",
                    X = Pos.Center(),
                    Y = 10,
                    Width = 20
                };

                migrationWindow.Add(stepLabel, statusLabel, progressBar, percentageLabel, countLabel);
                Application.Top.Add(migrationWindow);
                // Refresh the display
                Application.Top.SetNeedsDisplay();

                // Progress callback that updates the UI
                void progressCallback(MigrationProgress progress)
                {
                    Application.MainLoop.Invoke(() =>
                    {
                        stepLabel.Text = progress.CurrentStep;
                        statusLabel.Text = progress.CurrentItem;

                        if (progress.TotalItems > 0)
                        {
                            double fraction = progress.Percentage / 100.0;
                            progressBar.Fraction = (float)fraction;
                            percentageLabel.Text = $"{progress.Percentage:F1}%";
                            countLabel.Text = $"{progress.CurrentItemIndex} / {progress.TotalItems}";
                        }
                        else
                        {
                            progressBar.Fraction = 0f;
                            percentageLabel.Text = "0%";
                            countLabel.Text = "0 / 0";
                        }

                        Application.Refresh();
                    });
                }

                // Start the migration process in a background task
                Task.Run(() =>
                {
                    try
                    {
                        startMigration.Invoke(progressCallback);

                        Application.MainLoop.Invoke(() =>
                        {
                            stepLabel.Text = "Migration Complete";
                            statusLabel.Text = "Database Migration Complete!";
                            progressBar.Fraction = 1.0f;
                            percentageLabel.Text = "100%";
                            Application.Refresh();
                            
                            // Use a timer to shutdown after a brief delay to allow UI to update
                            Application.MainLoop.AddTimeout(TimeSpan.FromSeconds(2), (mainLoop) =>
                            {
                                Application.RequestStop();
                                return false;
                            });
                        });
                    }
                    catch (Exception ex)
                    {
                        Application.MainLoop.Invoke(() =>
                        {
                            stepLabel.Text = "Migration Failed";
                            statusLabel.Text = $"Error: {ex.Message}";
                            Application.Refresh();
                        });
                    }
                });
            }
            else
            {
                btnInit.Text = "Failed to Init";
                Application.RequestStop();
            }
        }
        btnInitWithoutDbReset.Clicked += () => ButtonClicked(false);
        btnInit.Clicked += () => ButtonClicked(true);

        Add(btnInit);
        Add(btnInitWithoutDbReset);
    }
}