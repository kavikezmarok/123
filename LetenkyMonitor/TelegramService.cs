namespace LetenkyMonitor;

public static class TelegramService
{
    public static async Task SendAsync(AppSettings settings, string text)
    {
        if (string.IsNullOrWhiteSpace(settings.TelegramBotToken))
            throw new InvalidOperationException("Chýba Telegram bot token.");

        if (string.IsNullOrWhiteSpace(settings.TelegramChatId))
            throw new InvalidOperationException("Chýba Telegram Chat ID.");

        using var http = new HttpClient();
        var url = $"https://api.telegram.org/bot{settings.TelegramBotToken}/sendMessage";

        using var response = await http.PostAsJsonAsync(url, new
        {
            chat_id = settings.TelegramChatId,
            text,
            disable_web_page_preview = true
        });

        response.EnsureSuccessStatusCode();
    }
}
