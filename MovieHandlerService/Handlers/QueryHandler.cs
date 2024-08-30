using System.Globalization;
using DatabaseService;
using MovieHandlerService.Utils;

namespace MovieHandlerService.Handlers;

public sealed class QueryHandler
{
    private readonly string[] _dateFormats = ["MM/dd/yyyy", "dd/MM/yyyy"];
    private readonly Trie _trie = new();
    private readonly SheetsHandler _sheets = new();

    public QueryHandler()
    {
        PrepopulateData();
    }

    /// <summary>
    /// Returns a value in string type, giving either the date, multiple matches,
    /// or no matches, given a title query.
    /// </summary>
    /// <param name="query">The query to do a date lookup.</param>
    /// <param name="explicitSearch">Whether to utilize explicit term search.</param>
    /// <returns>An array of either a date, a list of matches, or nothing.</returns>
    public string[]? MovieNameFindLastWatchedDate(string query, bool explicitSearch)
    {
        // Explicit search should return one value -- per DbHandler spec.
        if (explicitSearch)
        {
            var lastWatchedDate = DbHandler.GetMovieLastWatchedDate(query);
            if (lastWatchedDate == DateTime.MinValue)
            {
                return null;
            }

            return [lastWatchedDate.ToString("dd MMMM, yyyy", CultureInfo.InvariantCulture)];
        }

        var trieSearchMatches = _trie.Search(query);
        var enumerable = trieSearchMatches as string[] ?? trieSearchMatches.ToArray();
        var size = enumerable.Length;

        switch (size)
        {
            case 1:
                var lastWatchedDate = DbHandler.GetMovieLastWatchedDate(enumerable.First());

                if (lastWatchedDate == DateTime.MinValue)
                {
                    return null;
                }
                return [lastWatchedDate.ToString("dd MMMM, yyyy", CultureInfo.InvariantCulture)];
            case > 1:
                //  TODO should we do checks on whether our autocomplete DS contain within DB?
                //  - Functionally, this should never happen, but you never know...
                //  - Low priority.
                return enumerable;
            case < 1:
                return null;
        }
    }

    /// <summary>
    /// Initialization method to prepopulate data on call.
    /// </summary>
    private void PrepopulateData()
    {
        var datesDdmmRange2022 = _sheets.GetDataInRange("C2", "D123");
        var datesMmddRange2022 = _sheets.GetDataInRange("C124", "D522");
        var datesMmddRange2023 = _sheets.GetDataInRange("C523", "D3361");
        var datesMmddRange2024 = _sheets.GetDataInRange("C3362", "D4521");
        // and more

        InsertMovies(datesDdmmRange2022, 2022);
        InsertMovies(datesMmddRange2022, 2022);
        InsertMovies(datesMmddRange2023, 2023);
        InsertMovies(datesMmddRange2024, 2024);
    }

    /// <summary>
    /// Method that handles the different date formats in the spreadsheet,
    /// and performs insertion of data into memory and database.
    ///
    /// The spreadsheet itself does not contain the year so sections were
    /// manually drawn for this fact.
    /// </summary>
    /// <param name="dateRanges">The data collected from our spreadsheet.</param>
    /// <param name="year">The year in which the section was watched.</param>
    private void InsertMovies(IList<IList<object>> dateRanges, int year)
    {
        foreach (var row in dateRanges)
        {
            if (row.Count <= 1) continue;

            var dateWatched = row[0].ToString()?.Trim() ?? string.Empty;

            if (dateWatched == "skipped" || string.IsNullOrWhiteSpace(dateWatched)) continue;

            var movieWatched = row[1].ToString()?.Trim();

            dateWatched += $"/{year}";

            if (string.IsNullOrWhiteSpace(movieWatched)) continue;

            try
            {
                DateTime.TryParseExact(dateWatched, _dateFormats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var parsedDate);
                DbHandler.InsertOrUpdateWatchedMovie(movieWatched, parsedDate);
                _trie.Insert(movieWatched);
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Error parsing date {dateWatched}: {ex.Message}");
            }
        }
    }
}