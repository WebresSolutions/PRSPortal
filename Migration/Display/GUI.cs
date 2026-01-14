using System.Diagnostics;
using Terminal.Gui;

namespace Migration.Display;

/// <summary>
/// Graphical user interface for the database migration tool
/// Provides a terminal-based UI for initiating and monitoring migrations
/// </summary>
public class GUI : Window
{
    /// <summary>
    /// Initializes a new instance of the GUI class
    /// </summary>
    /// <param name="initapp">Function to initialize the migration (takes resetDatabase parameter)</param>
    /// <param name="startMigration">Action to start the migration process with progress callback</param>
    public GUI(Func<bool, bool> initapp, Action<Action<MigrationProgress>> startMigration)
    {
        Title = "PRS Database Migration [MySQL ---> PgSQL]";

        // Add the button clicked events
        _BtnInitWithoutDbReset.Clicked += () => ButtonClicked(false);
        _BtnInit.Clicked += () => ButtonClicked(true);

        // Add the buttons to the main window. 
        Add(_BtnInit);
        Add(_BtnInitWithoutDbReset);

        void ButtonClicked(bool reset)
        {
            if (initapp.Invoke(reset))
            {
                // Remove the current window
                Application.Top.Remove(this);

                _MigrationWindow.Add(_StepLabel, _StatusLabel, _ProgressBar, _PercentageLabel, _CountLabel, _BtnRestart, _BtnExit);
                Application.Top.Add(_MigrationWindow);
                // Refresh the display
                Application.Top.SetNeedsDisplay();

                // Store the reset parameter for restart
                bool shouldReset = reset;

                // Function to start the migration
                void StartMigrationProcess()
                {
                    // Reset UI
                    _StepLabel.Text = "Initializing migration...";
                    _StatusLabel.Text = "Preparing...";
                    _ProgressBar.Fraction = 0f;
                    _PercentageLabel.Text = "0%";
                    _CountLabel.Text = "0 / 0";
                    _BtnRestart.Visible = false;
                    _BtnExit.Visible = false;
                    Application.Refresh();

                    // Progress callback that updates the UI
                    void progressCallback(MigrationProgress progress)
                    {
                        Application.MainLoop.Invoke(() =>
                        {
                            _StepLabel.Text = progress.CurrentStep;
                            _StatusLabel.Text = progress.CurrentItem;

                            if (progress.TotalItems > 0)
                            {
                                double fraction = progress.Percentage / 100.0;
                                _ProgressBar.Fraction = (float)fraction;
                                _PercentageLabel.Text = $"{progress.Percentage:F1}%";
                                _CountLabel.Text = $"{progress.CurrentItemIndex} / {progress.TotalItems}";
                            }
                            else
                            {
                                _ProgressBar.Fraction = 0f;
                                _PercentageLabel.Text = "0%";
                                _CountLabel.Text = "0 / 0";
                            }

                            Application.Refresh();
                        });
                    }

                    Thread migrationThread = new(() =>
                    {
                        try
                        {
                            Stopwatch stopwatch = new();
                            stopwatch.Start();

                            // Start the actual migration.
                            startMigration.Invoke(progressCallback);

                            stopwatch.Stop();

                            Application.MainLoop.Invoke(() =>
                            {
                                _StepLabel.Text = "Migration Complete";
                                _StatusLabel.Text = $"Migration Complete. Time Elapsed: {stopwatch.Elapsed.TotalSeconds}";
                                _ProgressBar.Fraction = 1.0f;
                                _PercentageLabel.Text = "100%";

                                // Show restart and exit buttons
                                _BtnRestart.Visible = true;
                                _BtnExit.Visible = true;
                                Application.Refresh();
                            });
                        }
                        catch (Exception ex)
                        {
                            Application.MainLoop.Invoke(() =>
                            {
                                _StepLabel.Text = "Migration Failed";
                                _StatusLabel.Text = $"Error: {ex.Message}";

                                // Show restart and exit buttons even on failure
                                _BtnRestart.Visible = true;
                                _BtnExit.Visible = true;
                                Application.Refresh();
                            });
                        }
                    })
                    {
                        IsBackground = true  // Thread will terminate when main thread exits
                    };

                    migrationThread.Start();
                }

                // Button click handlers
                _BtnRestart.Clicked += () =>
                {
                    // Re-initialize if needed (only if reset was originally requested)
                    if (shouldReset)
                    {
                        if (initapp.Invoke(true))
                        {
                            StartMigrationProcess();
                        }
                    }
                    else
                    {
                        // Just restart migration without re-initializing
                        StartMigrationProcess();
                    }
                };

                _BtnExit.Clicked += () =>
                {
                    Application.RequestStop();
                };

                // Start the initial migration
                StartMigrationProcess();
            }
            else
            {
                _BtnInit.Text = "Failed to Init";
                Application.RequestStop();
            }
        }
    }

    // Create and show a new view/window for migration
    private readonly Window _MigrationWindow = new()
    {
        Title = "Migration In Progress",
        X = 0,
        Y = 0,
        Width = Dim.Fill(),
        Height = Dim.Fill()
    };

    // Add migration UI components
    private readonly Label _StepLabel = new()
    {
        Text = "Initializing migration...",
        X = 2,
        Y = 2,
        Width = Dim.Fill() - 4
    };

    private readonly Label _StatusLabel = new()
    {
        Text = "Preparing...",
        X = 2,
        Y = 4,
        Width = Dim.Fill() - 4
    };

    private readonly ProgressBar _ProgressBar = new()
    {
        X = 2,
        Y = 6,
        Width = Dim.Fill() - 4,
        Height = 1
    };

    private readonly Label _PercentageLabel = new()
    {
        Text = "0%",
        X = Pos.Center(),
        Y = 8,
        Width = 10
    };

    private readonly Label _CountLabel = new()
    {
        Text = "0 / 0",
        X = Pos.Center(),
        Y = 10,
        Width = 20
    };

    // Buttons for restart and exit (initially hidden)
    private Button _BtnRestart = new()
    {
        Text = "Restart Migration",
        X = Pos.Center() - 15,
        Y = 12,
        Width = 30,
        Visible = false
    };

    private Button _BtnExit = new()
    {
        Text = "Exit",
        X = Pos.Center() - 10,
        Y = 14,
        Width = 20,
        Visible = false
    };

    private readonly Button _BtnInit = new()
    {
        Text = "Migrate And Reset",
        X = Pos.Center(),
        Y = Pos.Center(),
        IsDefault = true
    };

    private readonly Button _BtnInitWithoutDbReset = new()
    {
        Text = "Migrate",
        X = Pos.Center(),
        Y = Pos.Center() - 4,
        IsDefault = true
    };

}