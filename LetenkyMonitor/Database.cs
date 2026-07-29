using Microsoft.Data.Sqlite;

namespace LetenkyMonitor;

public static class Database
{
    private static string ConnectionString => $"Data Source={AppPaths.DbPath}";

    public static void Initialize()
    {
        using var con = new SqliteConnection(ConnectionString);
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = """
        CREATE TABLE IF NOT EXISTS flights (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            name TEXT NOT NULL,
            origin TEXT NOT NULL,
            destination TEXT NOT NULL,
            departure_date TEXT NOT NULL,
            return_date TEXT NOT NULL,
            preferred_airline TEXT NOT NULL DEFAULT '',
            adults INTEGER NOT NULL DEFAULT 2,
            tolerance_days INTEGER NOT NULL DEFAULT 1,
            enabled INTEGER NOT NULL DEFAULT 1
        );

        CREATE TABLE IF NOT EXISTS prices (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            flight_id INTEGER NOT NULL,
            checked_at TEXT NOT NULL,
            departure_date TEXT NOT NULL,
            return_date TEXT NOT NULL,
            total_price REAL NULL,
            source TEXT NOT NULL,
            note TEXT NOT NULL DEFAULT '',
            FOREIGN KEY(flight_id) REFERENCES flights(id)
        );
        """;
        cmd.ExecuteNonQuery();

        using var countCmd = con.CreateCommand();
        countCmd.CommandText = "SELECT COUNT(*) FROM flights";
        var count = Convert.ToInt32(countCmd.ExecuteScalar());

        if (count == 0)
        {
            InsertFlight(new FlightItem
            {
                Name = "Wizz Air KRK–VLC",
                Origin = "KRK",
                Destination = "VLC",
                DepartureDate = "2026-09-11",
                ReturnDate = "2026-09-17",
                PreferredAirline = "Wizz Air"
            });
            InsertFlight(new FlightItem
            {
                Name = "Ryanair KRK–VLC",
                Origin = "KRK",
                Destination = "VLC",
                DepartureDate = "2026-09-10",
                ReturnDate = "2026-09-17",
                PreferredAirline = "Ryanair"
            });
            InsertFlight(new FlightItem
            {
                Name = "Ryanair VIE–VLC",
                Origin = "VIE",
                Destination = "VLC",
                DepartureDate = "2026-09-11",
                ReturnDate = "2026-09-17",
                PreferredAirline = "Ryanair"
            });
        }
    }

    public static List<FlightItem> GetFlights()
    {
        var result = new List<FlightItem>();
        using var con = new SqliteConnection(ConnectionString);
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = """
        SELECT id, name, origin, destination, departure_date, return_date,
               preferred_airline, adults, tolerance_days, enabled
        FROM flights
        ORDER BY id
        """;

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            result.Add(new FlightItem
            {
                Id = reader.GetInt64(0),
                Name = reader.GetString(1),
                Origin = reader.GetString(2),
                Destination = reader.GetString(3),
                DepartureDate = reader.GetString(4),
                ReturnDate = reader.GetString(5),
                PreferredAirline = reader.GetString(6),
                Adults = reader.GetInt32(7),
                ToleranceDays = reader.GetInt32(8),
                Enabled = reader.GetInt32(9) == 1
            });
        }

        return result;
    }

    public static void InsertFlight(FlightItem flight)
    {
        using var con = new SqliteConnection(ConnectionString);
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = """
        INSERT INTO flights
        (name, origin, destination, departure_date, return_date, preferred_airline, adults, tolerance_days, enabled)
        VALUES
        ($name, $origin, $destination, $departure, $return, $airline, $adults, $tolerance, $enabled)
        """;
        BindFlight(cmd, flight);
        cmd.ExecuteNonQuery();
    }

    public static void UpdateFlight(FlightItem flight)
    {
        using var con = new SqliteConnection(ConnectionString);
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = """
        UPDATE flights
        SET name=$name,
            origin=$origin,
            destination=$destination,
            departure_date=$departure,
            return_date=$return,
            preferred_airline=$airline,
            adults=$adults,
            tolerance_days=$tolerance,
            enabled=$enabled
        WHERE id=$id
        """;
        BindFlight(cmd, flight);
        cmd.Parameters.AddWithValue("$id", flight.Id);
        cmd.ExecuteNonQuery();
    }

    public static void DeleteFlight(long id)
    {
        using var con = new SqliteConnection(ConnectionString);
        con.Open();

        using var transaction = con.BeginTransaction();

        using (var prices = con.CreateCommand())
        {
            prices.Transaction = transaction;
            prices.CommandText = "DELETE FROM prices WHERE flight_id=$id";
            prices.Parameters.AddWithValue("$id", id);
            prices.ExecuteNonQuery();
        }

        using (var flight = con.CreateCommand())
        {
            flight.Transaction = transaction;
            flight.CommandText = "DELETE FROM flights WHERE id=$id";
            flight.Parameters.AddWithValue("$id", id);
            flight.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    public static void SavePrice(long flightId, string departure, string returnDate,
        decimal? totalPrice, string source, string note)
    {
        using var con = new SqliteConnection(ConnectionString);
        con.Open();

        using var cmd = con.CreateCommand();
        cmd.CommandText = """
        INSERT INTO prices
        (flight_id, checked_at, departure_date, return_date, total_price, source, note)
        VALUES
        ($flightId, $checkedAt, $departure, $return, $price, $source, $note)
        """;
        cmd.Parameters.AddWithValue("$flightId", flightId);
        cmd.Parameters.AddWithValue("$checkedAt", DateTime.Now.ToString("s"));
        cmd.Parameters.AddWithValue("$departure", departure);
        cmd.Parameters.AddWithValue("$return", returnDate);
        cmd.Parameters.AddWithValue("$price", totalPrice is null ? DBNull.Value : totalPrice.Value);
        cmd.Parameters.AddWithValue("$source", source);
        cmd.Parameters.AddWithValue("$note", note);
        cmd.ExecuteNonQuery();
    }

    private static void BindFlight(SqliteCommand cmd, FlightItem flight)
    {
        cmd.Parameters.AddWithValue("$name", flight.Name);
        cmd.Parameters.AddWithValue("$origin", flight.Origin);
        cmd.Parameters.AddWithValue("$destination", flight.Destination);
        cmd.Parameters.AddWithValue("$departure", flight.DepartureDate);
        cmd.Parameters.AddWithValue("$return", flight.ReturnDate);
        cmd.Parameters.AddWithValue("$airline", flight.PreferredAirline);
        cmd.Parameters.AddWithValue("$adults", flight.Adults);
        cmd.Parameters.AddWithValue("$tolerance", flight.ToleranceDays);
        cmd.Parameters.AddWithValue("$enabled", flight.Enabled ? 1 : 0);
    }
}
