using MovieHandlerService.Handlers;

class Program
{
    static void Main()
    {
        // Initialize the QueryHandler which also prepopulates data
        var queryHandler = new QueryHandler();

        // Test 1: Broad case - searching for a broad set of movie title
        Console.WriteLine("Test 1: Exact match");
        var resultExact = queryHandler.MovieNameFindLastWatchedDate("batman", false);
        PrintResults(resultExact);

        // Test 2: No matches - searching for a non-existent title
        Console.WriteLine("\nTest 2: No match");
        var resultNone = queryHandler.MovieNameFindLastWatchedDate("loool", false);
        PrintResults(resultNone);

        // Test 3: Return a date of a recorded movie title
        Console.WriteLine("\nTest 3: Get a date");
        var result3 = queryHandler.MovieNameFindLastWatchedDate("spirited aw", false); // spirited away
        PrintResults(result3);

        // Test 4: Explicit search return
        Console.WriteLine("\nTest 4: Explicit search");
        var result4 = queryHandler.MovieNameFindLastWatchedDate("ponyo", true);
        PrintResults(result4);

        // Test 5: Explicit search null return
        Console.WriteLine("\nTest 5: Null explicit search");
        var result5 = queryHandler.MovieNameFindLastWatchedDate("movie name is not real", true);
        PrintResults(result5);

        // Test 6: Limited generic search to 15
        Console.WriteLine("\nTest 6: Generic queries only return at most 15 values");
        var result6 = queryHandler.MovieNameFindLastWatchedDate("a", false);
        Console.WriteLine(result6.Length);
    }

    private static void PrintResults(string[]? results)
    {
        if (results == null) {
            Console.WriteLine("debug null returned");
            return;
        }

        if (results.Length == 0)
        {
            Console.WriteLine("No results found.");
            return;
        }

        foreach (var result in results)
        {
            Console.WriteLine(result);
        }
    }
}