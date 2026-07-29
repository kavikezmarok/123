namespace LetenkyMonitor;

public partial class SettingsWindow : Window
{
    private readonly AppSettings settings;

    public SettingsWindow()
    {
        InitializeComponent();

        settings = SettingsService.Load();
        TokenBox.Password = settings.TelegramBotToken;
        ChatIdBox.Text = settings.TelegramChatId;
        MorningBox.Text = settings.MorningTime;
        EveningBox.Text = settings.EveningTime;
        HeadlessBox.IsChecked = settings.Headless;
    }

    private async void TestTelegram_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var temporary = new AppSettings
            {
                TelegramBotToken = TokenBox.Password.Trim(),
                TelegramChatId = ChatIdBox.Text.Trim()
            };

            await TelegramService.SendAsync(
                temporary,
                "✅ Test Telegramu z aplikácie Letenky Monitor funguje."
            );

            MessageBox.Show("Testovacia správa bola odoslaná.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Chyba");
        }
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        settings.TelegramBotToken = TokenBox.Password.Trim();
        settings.TelegramChatId = ChatIdBox.Text.Trim();
        settings.MorningTime = MorningBox.Text.Trim();
        settings.EveningTime = EveningBox.Text.Trim();
        settings.Headless = HeadlessBox.IsChecked == true;

        SettingsService.Save(settings);
        DialogResult = true;
    }
}
