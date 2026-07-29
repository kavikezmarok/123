namespace LetenkyMonitor;

public sealed class FlightItem
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string Origin { get; set; } = "";
    public string Destination { get; set; } = "";
    public string DepartureDate { get; set; } = "";
    public string ReturnDate { get; set; } = "";
    public string PreferredAirline { get; set; } = "";
    public int Adults { get; set; } = 2;
    public int ToleranceDays { get; set; } = 1;
    public bool Enabled { get; set; } = true;
}

public sealed class AppSettings
{
    public string TelegramBotToken { get; set; } = "";
    public string TelegramChatId { get; set; } = "";
    public string MorningTime { get; set; } = "08:00";
    public string EveningTime { get; set; } = "20:00";
    public bool Headless { get; set; } = false;
}
