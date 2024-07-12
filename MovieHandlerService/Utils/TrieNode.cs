namespace MovieHandlerService.Utils;

public class TrieNode
{
    public Dictionary<char, TrieNode> Children { get; set; } = new();
    public bool IsWordTerminated { get; set; }
    public List<string> Titles { get; set; } = [];
}