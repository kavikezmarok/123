using Microsoft.Playwright;

namespace LetenkyMonitor;

public sealed class FlightMonitorService
{
    private static readonly Regex PriceRegex = new(
        @"(?:€\s?(\d{1,4}(?:[.,]\d{2})?)|(\d{1,4}(?:[.,]\d{2})?)\s?€)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant
    );

    public async Task<string> RunAsync(IEnumerable<FlightItem> flights, AppSettings settings)
    {
        var activeFlights = flights.Where(x => x.Enabled).ToList();
        if (activeFlights.Count == 0)
            return "Nie sú nastavené žiadne aktívne lety.";

        var report = new List<string>
        {
            $"✈️ Kontrola cien – {DateTime.Now:dd.MM.yyyy HH:mm}",
            ""
        };

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = settings.Headless
        });

        var context = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            Locale = "sk-SK",
            ViewportSize = new ViewportSize { Width = 1440, Height = 1000 }
        });

        var page = await context.NewPageAsync();

        foreach (var flight in activeFlights)
        {
            report.Add($"🔹 {flight.Name}");
            report.Add($"   {flight.Origin} → {flight.Destination}, {flight.Adults} osoby");

            var baseDeparture = DateOnly.ParseExact(flight.DepartureDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var baseReturn = DateOnly.ParseExact(flight.ReturnDate, "yyyy-MM-dd", CultureInfo.InvariantCulture);

            decimal? bestPrice = null;
            string bestDates = "";

            for (var departureShift = -flight.ToleranceDays; departureShift <= flight.ToleranceDays; departureShift++)
            {
                for (var returnShift = -flight.ToleranceDays; returnShift <= flight.ToleranceDays; returnShift++)
                {
                    var departure = baseDeparture.AddDays(departureShift);
                    var returnDate = baseReturn.AddDays(returnShift);

                    if (returnDate <= departure)
                        continue;

                    var url = BuildUrl(flight, departure, returnDate);
                    decimal? price = null;
                    var note = "";

                    try
                    {
                        await page.GotoAsync(url, new PageGotoOptions
                        {
                            WaitUntil = WaitUntilState.DOMContentLoaded,
                            Timeout = 90000
                        });

                        await page.WaitForTimeoutAsync(12000);
                        var body = await page.Locator("body").InnerTextAsync();

                        if (ContainsBlockingMessage(body))
                        {
                            note = "Skyscanner požaduje CAPTCHA alebo blokuje automatické načítanie.";
                        }
                        else
                        {
                            var prices = ExtractPrices(body);
                            if (prices.Count > 0)
                            {
                                price = prices.Min();
                                note = "Orientačná cena načítaná zo stránky Skyscanner.";
                            }
                            else
                            {
                                note = "Cena sa nedala spoľahlivo rozpoznať.";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        note = ex.Message;
                        File.AppendAllText(
                            Path.Combine(AppPaths.LogsDir, "errors.log"),
                            $"{DateTime.Now:s} | {flight.Name} | {ex}{Environment.NewLine}"
                        );
                    }

                    Database.SavePrice(
                        flight.Id,
                        departure.ToString("yyyy-MM-dd"),
                        returnDate.ToString("yyyy-MM-dd"),
                        price,
                        "Skyscanner",
                        note
                    );

                    if (price is not null)
                    {
                        report.Add($"   • {departure:yyyy-MM-dd} – {returnDate:yyyy-MM-dd}: {price:0.00} € spolu");

                        if (bestPrice is null || price.Value < bestPrice.Value)
                        {
                            bestPrice = price;
                            bestDates = $"{departure:yyyy-MM-dd} – {returnDate:yyyy-MM-dd}";
                        }
                    }
                }
            }

            if (bestPrice is null)
            {
                report.Add("   ⚠️ Presnú cenu sa nepodarilo načítať.");
            }
            else
            {
                report.Add(
                    $"   ✅ Najlepšie: {bestDates}, {bestPrice:0.00} € spolu / " +
                    $"{bestPrice.Value / flight.Adults:0.00} € na osobu"
                );
            }

            report.Add("");
        }

        report.Add("Pred nákupom cenu vždy potvrď priamo u predajcu.");
        var text = string.Join(Environment.NewLine, report);

        if (!string.IsNullOrWhiteSpace(settings.TelegramBotToken) &&
            !string.IsNullOrWhiteSpace(settings.TelegramChatId))
        {
            await TelegramService.SendAsync(settings, text);
        }

        return text;
    }

    private static string BuildUrl(FlightItem flight, DateOnly departure, DateOnly returnDate)
    {
        return
            $"https://www.skyscanner.net/transport/flights/" +
            $"{flight.Origin.ToLowerInvariant()}/" +
            $"{flight.Destination.ToLowerInvariant()}/" +
            $"{departure:yyMMdd}/" +
            $"{returnDate:yyMMdd}/" +
            $"?adultsv2={flight.Adults}&cabinclass=economy&currency=EUR&rtn=1";
    }

    private static bool ContainsBlockingMessage(string body)
    {
        return body.Contains("captcha", StringComparison.OrdinalIgnoreCase)
            || body.Contains("verify you are human", StringComparison.OrdinalIgnoreCase)
            || body.Contains("unusual traffic", StringComparison.OrdinalIgnoreCase)
            || body.Contains("access denied", StringComparison.OrdinalIgnoreCase);
    }

    private static List<decimal> ExtractPrices(string text)
    {
        var result = new List<decimal>();

        foreach (Match match in PriceRegex.Matches(text))
        {
            var raw = !string.IsNullOrWhiteSpace(match.Groups[1].Value)
                ? match.Groups[1].Value
                : match.Groups[2].Value;

            if (decimal.TryParse(
                    raw.Replace(',', '.'),
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out var value)
                && value >= 20
                && value <= 5000)
            {
                result.Add(value);
            }
        }

        return result;
    }
}
