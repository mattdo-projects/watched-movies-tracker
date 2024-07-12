using StackExchange.Redis;
using DatabaseService;
using DatabaseService.Utils;

namespace MovieHandlerService.Utils;

// TODO: FIX. UNTESTED CODE.
public static class RedisTrie
{
    private const string RootPrefix = "trieRoot";
    private const string TitleSetSuffix = ":titles";
    private const string ExistsHashField = "exists";
    private const string TerminatingKeySuffix = ":term";

    public static void Insert(string title)
    {
        var normalizedTitle = SanitizeTitle.Sanitize(title);
        var db = RedisHandler.Database;
        var batch = db.CreateBatch();

        foreach (var start in Enumerable.Range(0, normalizedTitle.Length))
        {
            var currentPrefix = RootPrefix;
            foreach (var i in Enumerable.Range(start, normalizedTitle.Length - start))
            {
                currentPrefix += ":" + normalizedTitle[i];
                batch.StringSetAsync(currentPrefix + ExistsHashField, 1);
            }

            // terminator + title suffix
            batch.StringSetAsync(currentPrefix + TerminatingKeySuffix, 1);
            batch.SetAddAsync(currentPrefix + TitleSetSuffix, title);
        }

        batch.Execute();
    }

    public static IEnumerable<string> Search(string query)
    {
        query = SanitizeTitle.Sanitize(query);
        var db = RedisHandler.Database;
        var currentPrefix = BuildKeyForQuery(query);

        return !db.KeyExists(currentPrefix + ExistsHashField) ?
            [] : CollectAllTitles(db, currentPrefix);
    }

    private static HashSet<string> CollectAllTitles(IDatabase db, string baseKey)
    {
        return [..db.SetMembers(baseKey + TitleSetSuffix).Select(v => (string)v!)];
    }

    private static string BuildKeyForQuery(string query)
    {
        return $"{RootPrefix}:{string.Join(":", query.Select(c => c))}";
    }
}