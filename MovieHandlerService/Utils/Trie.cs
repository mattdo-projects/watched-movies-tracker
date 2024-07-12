using DatabaseService.Utils;

namespace MovieHandlerService.Utils;

public class Trie
{
    private readonly int _limit = Convert.ToInt32(
        Environment.GetEnvironmentVariable("TRIE_RETURN_LIMIT"));
    private readonly TrieNode _root = new();

    public bool IsPopulated()
    {
        return _root.Children.Count > 0;
    }

    public void Insert(string title)
    {
        var normalizedTitle = SanitizeTitle.Sanitize(title);

        for (var start = 0; start < normalizedTitle.Length; ++start)
        {
            var node = _root;
            for (var i = start; i < normalizedTitle.Length; ++i)
            {
                var ch = normalizedTitle[i];
                if (!node.Children.TryGetValue(ch, out var value))
                {
                    value = new TrieNode();
                    node.Children[ch] = value;
                }

                node = value;
            }

            node.IsWordTerminated = true;
            node.Titles.Add(title);
        }
    }

    public IEnumerable<string> Search(string query)
    {
        var node = _root;
        query = SanitizeTitle.Sanitize(query);

        foreach (var ch in query)
        {
            if (!node.Children.TryGetValue(ch, out var child)) return [];

            node = child;
        }

        return Traverse(node, []);
    }

    private List<string> Traverse(TrieNode node, HashSet<string> results)
    {
        if (results.Count >= _limit)
        {
            return [..results];
        }

        if (node.IsWordTerminated)
        {
            foreach (var title in node.Titles.TakeWhile(
                         _ => results.Count < _limit))
            {
                results.Add(title);
            }
        }

        foreach (var child in node.Children.TakeWhile(
                     _ => results.Count < _limit))
        {
            Traverse(child.Value, results);
        }

        return [..results];
    }
}