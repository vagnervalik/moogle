namespace MoogleEngine;

public class SearchItem
{
    public SearchItem(string title, string snippet, float score, string[] matches)
    {
        this.Title = title;
        this.Snippet = snippet;
        this.Score = score;
        this.Matches = matches;
    }

    public string Title { get; private set; }

    public string Snippet { get; private set; }

    public float Score { get; private set; }

    public string[] Matches { get; private set; }
}
