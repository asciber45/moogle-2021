namespace MoogleEngine;

public class SearchResult
{
    private List<SearchItem> items;

    public SearchResult(List<SearchItem> items, string suggestion="")
    {
        if (items == null) {
            throw new ArgumentNullException("items");
        }

        this.items = items;
        this.Suggestion = suggestion;
    }

    public SearchResult() : this(new List<SearchItem>()) {

    }
    
    ///<summary> Constructor que devuelve una lista de los documentos ordenados por su score </summary>///
    public SearchResult(string query, Corpus corpus)
    {
        Query newquery = new Query(query, corpus);
        Similitud scores = new Similitud(newquery, corpus);
        List<SearchItem> items = new List<SearchItem>();

        for (int i = 0; scores.similitud![0, i] != 0; i++)
        {
            string name = corpus.documents[(int)scores.similitud[1, i]].name;
            name = name.Substring(0, name.Length - 4);
            string snippet = AuxMethods.FindSnippet((int)scores.similitud[1, i], newquery, corpus, corpus.path);
            float score = scores.similitud[0, i];

            SearchItem newitem = new SearchItem(name, snippet, score);
            items.Add(newitem);
        }

        query = (newquery.list[0].Count == 0)? query : AuxMethods.ToString(newquery.list[0]);

        this.items = items;
        this.Suggestion = query;
    }

    public string Suggestion { get; private set; }

    public IEnumerable<SearchItem> Items() {
        return this.items;
    }

    public int Count { get { return this.items.Count; } }

}
