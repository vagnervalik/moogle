namespace MoogleEngine;
using System.IO;


public class Moogle{
    public DataBase Docs;
    public Matrix M;
    public Moogle(string address){
        this.Docs = new DataBase(address);
        this.M = new Matrix(this.Docs);
    }
    public SearchResult Query(string query) {
        // Modifique este método para responder a la búsqueda
        Vector qry = new Vector(query, this.Docs.Vocabulary());
        var scores = this.M.GetScores(qry);

        List<SearchItem> Items = new List<SearchItem>();
        for(int i = 0; i < scores.score.Length; i++){
            if(scores.score[i] == 0){
                break;
            }
            Items.Add(new SearchItem(scores.name[i], scores.snippet[i], scores.score[i]));
        }
        string suggestion = this.M.GetSuggestion(query);
        Console.WriteLine(this.M.GetSuggestion(query));
        SearchItem[] items = Items.ToArray();
        return new SearchResult(items, suggestion);
    }
}
