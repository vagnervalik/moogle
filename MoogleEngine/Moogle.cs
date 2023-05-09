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
            Items.Add(new SearchItem(scores.name[i], scores.snippet[i], scores.score[i], scores.matches[i]));
        }
        Console.WriteLine(this.M.GetSuggestion(query));
        SearchItem[] items = Items.ToArray();
        return new SearchResult(items, query);
    }
}

// int Min(int[] array, int p){
//   if (p==array.Length) return int.MaxValue;
//   return Math.Min(array[p], Min(array, p+1))
//}