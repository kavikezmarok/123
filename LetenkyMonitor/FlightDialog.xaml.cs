namespace LetenkyMonitor;

public partial class FlightDialog : Window
{
    public FlightItem Flight { get; }

    public FlightDialog(FlightItem? source = null)
    {
        InitializeComponent();

        Flight = source is null
            ? new FlightItem()
            : new FlightItem
            {
                Id = source.Id,
                Name = source.Name,
                Origin = source.Origin,
                Destination = source.Destination,
                DepartureDate = source.DepartureDate,
                ReturnDate = source.ReturnDate,
                PreferredAirline = source.PreferredAirline,
                Adults = source.Adults,
                ToleranceDays = source.ToleranceDays,
                Enabled = source.Enabled
            };

        NameBox.Text = Flight.Name;
        OriginBox.Text = Flight.Origin;
        DestinationBox.Text = Flight.Destination;
        DepartureBox.Text = Flight.DepartureDate;
        ReturnBox.Text = Flight.ReturnDate;
        AirlineBox.Text = Flight.PreferredAirline;
        AdultsBox.Text = Flight.Adults.ToString(CultureInfo.InvariantCulture);
        ToleranceBox.Text = Flight.ToleranceDays.ToString(CultureInfo.InvariantCulture);
        EnabledBox.IsChecked = Flight.Enabled;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(OriginBox.Text) ||
            string.IsNullOrWhiteSpace(DestinationBox.Text))
        {
            MessageBox.Show("Zadaj odletové a cieľové letisko.");
            return;
        }

        if (!DateOnly.TryParseExact(
                DepartureBox.Text.Trim(),
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var departure))
        {
            MessageBox.Show("Dátum odletu musí byť vo formáte RRRR-MM-DD.");
            return;
        }

        if (!DateOnly.TryParseExact(
                ReturnBox.Text.Trim(),
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var returnDate))
        {
            MessageBox.Show("Dátum návratu musí byť vo formáte RRRR-MM-DD.");
            return;
        }

        if (returnDate <= departure)
        {
            MessageBox.Show("Dátum návratu musí byť neskôr ako dátum odletu.");
            return;
        }

        if (!int.TryParse(AdultsBox.Text, out var adults) || adults < 1 || adults > 9)
        {
            MessageBox.Show("Počet dospelých musí byť od 1 do 9.");
            return;
        }

        if (!int.TryParse(ToleranceBox.Text, out var tolerance) || tolerance < 0 || tolerance > 7)
        {
            MessageBox.Show("Tolerancia musí byť od 0 do 7 dní.");
            return;
        }

        Flight.Name = string.IsNullOrWhiteSpace(NameBox.Text)
            ? $"{OriginBox.Text.Trim().ToUpperInvariant()}–{DestinationBox.Text.Trim().ToUpperInvariant()}"
            : NameBox.Text.Trim();

        Flight.Origin = OriginBox.Text.Trim().ToUpperInvariant();
        Flight.Destination = DestinationBox.Text.Trim().ToUpperInvariant();
        Flight.DepartureDate = departure.ToString("yyyy-MM-dd");
        Flight.ReturnDate = returnDate.ToString("yyyy-MM-dd");
        Flight.PreferredAirline = AirlineBox.Text.Trim();
        Flight.Adults = adults;
        Flight.ToleranceDays = tolerance;
        Flight.Enabled = EnabledBox.IsChecked == true;

        DialogResult = true;
    }
}
