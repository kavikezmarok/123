namespace LetenkyMonitor;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        RefreshFlights();
    }

    private FlightItem? SelectedFlight => FlightsGrid.SelectedItem as FlightItem;

    private void RefreshFlights()
    {
        FlightsGrid.ItemsSource = Database.GetFlights();
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new FlightDialog { Owner = this };

        if (dialog.ShowDialog() == true)
        {
            Database.InsertFlight(dialog.Flight);
            RefreshFlights();
        }
    }

    private void Edit_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedFlight is null)
        {
            MessageBox.Show("Najprv vyber let.");
            return;
        }

        var dialog = new FlightDialog(SelectedFlight) { Owner = this };

        if (dialog.ShowDialog() == true)
        {
            Database.UpdateFlight(dialog.Flight);
            RefreshFlights();
        }
    }

    private void Delete_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedFlight is null)
        {
            MessageBox.Show("Najprv vyber let.");
            return;
        }

        var answer = MessageBox.Show(
            "Naozaj odstrániť vybraný let?",
            "Potvrdenie",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question
        );

        if (answer == MessageBoxResult.Yes)
        {
            Database.DeleteFlight(SelectedFlight.Id);
            RefreshFlights();
        }
    }

    private void Toggle_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedFlight is null)
        {
            MessageBox.Show("Najprv vyber let.");
            return;
        }

        SelectedFlight.Enabled = !SelectedFlight.Enabled;
        Database.UpdateFlight(SelectedFlight);
        RefreshFlights();
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        new SettingsWindow { Owner = this }.ShowDialog();
    }

    private async void Run_Click(object sender, RoutedEventArgs e)
    {
        StatusText.Text = "Kontrola prebieha...";

        try
        {
            var monitor = new FlightMonitorService();
            var result = await monitor.RunAsync(
                Database.GetFlights(),
                SettingsService.Load()
            );

            StatusText.Text = "Kontrola dokončená";
            MessageBox.Show(result, "Výsledok kontroly");
        }
        catch (Exception ex)
        {
            StatusText.Text = "Kontrola skončila chybou";
            MessageBox.Show(ex.ToString(), "Chyba");
        }
    }
}
