using System.Data;
using Npgsql;
using DatabaseService.Utils;

namespace DatabaseService;

public static class DbHandler
{
    private static readonly Lazy<NpgsqlConnection> Lazy;
    private static int _activeTransactions;

    static DbHandler()
    {
        const string connectionString = "Host=postgresdb;" +
                                        "Port=5432;" +
                                        "Username=dlc-db;" +
                                        "Password=dlcdb-nopass;" +
                                        "Database=watched-dlc";
        Lazy = new Lazy<NpgsqlConnection>(() => new NpgsqlConnection(connectionString));

        const string testDropTableQuery = "DROP TABLE IF EXISTS watched_movies";

        ExecuteNonQuery(testDropTableQuery);

        const string createTableQuery = """
                                        CREATE TABLE IF NOT EXISTS watched_movies (
                                            id SERIAL PRIMARY KEY,
                                            short_name VARCHAR(255),
                                            movie_name VARCHAR(255),
                                            last_watched DATE
                                        );
                                        """;

        ExecuteNonQuery(createTableQuery);
    }

    private static NpgsqlConnection Connection => Lazy.Value;

    public static void TestConnection()
    {
        Console.WriteLine("Testing Postgres connection...");
        try
        {
            Connection.Open();
            Console.WriteLine("Connection to Postgres db successful!");
        }
        finally
        {
            Connection.Close();
        }
    }


    private static int ExecuteNonQuery(string query)
    {
        using var conn = new NpgsqlConnection(Connection.ConnectionString);
        conn.Open();
        using var cmd = new NpgsqlCommand(query, conn);
        return cmd.ExecuteNonQuery();
    }

    public static void InsertOrUpdateWatchedMovie(string movie, DateTime lastSeen)
    {
        const string insertQuery = """
                                   INSERT INTO watched_movies (short_name, 
                                                               movie_name, 
                                                               last_watched) 
                                   VALUES (@short, 
                                           @full, 
                                           @lastSeen) 
                                   ON CONFLICT (short_name)
                                   DO UPDATE SET last_watched = EXCLUDED.last_watched
                                   """;

        var lower = SanitizeTitle.Sanitize(movie);

        using var conn = new NpgsqlConnection(Connection.ConnectionString);
        conn.Open();

        Interlocked.Increment(ref _activeTransactions);

        try
        {
            using var cmd = new NpgsqlCommand(insertQuery, conn);
            cmd.Parameters.AddWithValue("@short", lower);
            cmd.Parameters.AddWithValue("@full", movie);
            cmd.Parameters.AddWithValue("@lastSeen", lastSeen);

            cmd.ExecuteScalar();
        }
        finally
        {
            Interlocked.Decrement(ref _activeTransactions);
            conn.Close();
        }
    }

    public static void RemoveDateWatchedMovie(string normalizedTitle)
    {
        const string removeQuery = """
                                   UPDATE watched_movies
                                   SET last_watched = NULL
                                   WHERE short_name = @short
                                   """;

        using var conn = new NpgsqlConnection(Connection.ConnectionString);
        conn.Open();

        Interlocked.Increment(ref _activeTransactions);

        try
        {
            using var cmd = new NpgsqlCommand(removeQuery, conn);
            cmd.Parameters.AddWithValue("@short", normalizedTitle);
            cmd.ExecuteScalar();
        }
        finally
        {
            Interlocked.Decrement(ref _activeTransactions);
            conn.Close();
        }
    }

    public static void DeleteWatchedMovieRecord(string normalizedTitle)
    {
        const string deleteQuery = """
                                   DELETE FROM watched_movies
                                   WHERE short_name = @short
                                   """;

        using var conn = new NpgsqlConnection(Connection.ConnectionString);
        conn.Open();

        Interlocked.Increment(ref _activeTransactions);

        try
        {
            using var cmd = new NpgsqlCommand(deleteQuery, conn);
            cmd.Parameters.AddWithValue("@short", normalizedTitle);
            cmd.ExecuteScalar();
        }
        finally
        {
            Interlocked.Decrement(ref _activeTransactions);
            conn.Close();
        }
    }

    /// <summary>
    /// Returns the date of when an existing queried film was last seen,
    /// otherwise return Unix T-0.
    ///
    /// The number of values is capped.
    /// </summary>
    /// <param name="normalizedTitle">The title to lookup.</param>
    /// <returns>The existing last watched date, or Unix T-0 if not.</returns>
    public static DateTime GetMovieLastWatchedDate(string normalizedTitle)
    {
        const string searchQuery = """
                                   SELECT last_watched 
                                   FROM watched_movies
                                   WHERE short_name = @short
                                   """;

        using var conn = new NpgsqlConnection(Connection.ConnectionString);
        conn.Open();

        Interlocked.Increment(ref _activeTransactions);

        try
        {
            normalizedTitle = SanitizeTitle.Sanitize(normalizedTitle);
            using var cmd = new NpgsqlCommand(searchQuery, conn);
            cmd.Parameters.AddWithValue("@short", normalizedTitle);

            var result = cmd.ExecuteScalar();

            if (result == DBNull.Value || result == null)
            {
                return DateTime.MinValue;
            }

            return (DateTime) result;
        }
        finally
        {
            Interlocked.Decrement(ref _activeTransactions);
            conn.Close();
        }
    }

    public static string? GetFirstMovies()
    {
        const string searchQuery = """
                                   SELECT movie_name
                                   FROM watched_movies
                                   LIMIT 1;
                                   """;

        using var conn = new NpgsqlConnection(Connection.ConnectionString);
        conn.Open();

        Interlocked.Increment(ref _activeTransactions);
        try
        {
            using var cmd = new NpgsqlCommand(searchQuery, conn);
            using var reader = cmd.ExecuteReader();

            return reader.Read() ? reader["movie_name"].ToString() : null;
        }
        finally
        {
            Interlocked.Decrement(ref _activeTransactions);
            conn.Close();
        }
    }

    public static void DisconnectDatabase()
    {
        Console.WriteLine("Initiating db shutdown...");

        while (_activeTransactions > 0)
        {
            Console.WriteLine($"Waiting for {_activeTransactions} transaction(s) to complete...");
            Thread.Sleep(1000);
        }

        if (Connection.State != ConnectionState.Closed)
        {
            Connection.Close();
            Console.WriteLine("Database connection closed.");
        }

        Console.WriteLine("Database shutdown complete.");
    }
}
