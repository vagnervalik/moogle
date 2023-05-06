namespace MoogleEngine;
using System.Text.RegularExpressions;

public class Matrix{
    private DataBase Docs;
    private float[] Idfs;
    private string[] voc;
    private Vector[] matrix;
    private Dictionary<string, Dictionary<int, (float tf_idf, float tf)>> Dict;

    public Matrix(DataBase Docs){
        this.Docs = Docs;
        this.voc = this.Docs.Vocabulary();
        this.Idfs = this.GetIdfs();
        this.matrix = this.GetMatrix();
        this.Dict = this.GetDictionary();
    }

    public float[] GetIdfs(){
        float size = Convert.ToSingle(this.Docs.Count());
        float[] idfs = new float[this.Docs.NumOfWords()];
        float count = 0f;
        for(int i = 0; i < this.Docs.NumOfWords(); i++){
            count = 0;
            for(int j = 0; j < size; j++){
                if(Array.Exists(this.Docs.Get(j), element => element == this.Docs.Vocabulary()[i])){
                    count ++;
                }
            }
            idfs[i] = Convert.ToSingle(Math.Log(size/count));
        }
        return idfs;
    }

    private Vector[] GetMatrix(){
        Vector[] All = new Vector[this.Docs.Count()];
        for(int i = 0; i < this.Docs.Count(); i++){
            All[i] = new Vector(this.Docs.Get(i), this.voc, this.Idfs);
        }
        return All;
    }

    public Vector[] Get(){
        return this.matrix;
    }

    public Dictionary<string, Dictionary<int, (float tf_idf, float tf) >> GetDictionary(){
        Dictionary<string, Dictionary<int, (float tf_idf, float tf)>> Dict = new Dictionary<string, Dictionary<int, (float tf_idf, float tf)>>();
        for(int i = 0; i < this.voc.Length; i++){
            Dict.Add(this.voc[i], new Dictionary<int, (float tf_idf, float tf)>());
            for(int j = 0; j < this.Docs.Count(); j++){
                Dict[this.voc[i]].Add(j, (this.matrix[j].GetTfidf()[i], this.matrix[j].GetTf()[i]));
            }
        }
        return Dict;
    }        
    
    public float[] score(string query){
        float[] scores = new float[this.Docs.Count()];
        for(int i = 0; i < this.Docs.Count(); i++){
            scores[i] = this.Dict[query][i].tf_idf;
        }
        return scores;
    }

    public (string[] name, float[] score, string[][] matches, float[][] matchesScores, string[] snippet)  GetScores(Vector query){
        float[] scores = new float[this.matrix.Length];
        string[] files = new string[this.Docs.Count()];
        List<string[]> Allmatches = new List<string[]>();
        List<float[]> matchesRel = new List<float[]>();
        List<string> snippets = new List<string>();
        for(int i = 0; i < this.Docs.Count(); i++){
            files[i] = this.Docs.FileNames()[i];
        }
        for(int i = 0; i < this.matrix.Length; i++){
             var x = query.Multiply(this.matrix[i]);
             scores[i] = x.score;
             Allmatches.Add(x.matches);
             matchesRel.Add(x.matchesScores);
        }
        for(int i = 0; i < this.matrix.Length; i++){
            snippets.Add(this.GetSnippet(this.Docs.GetAllText()[i], Allmatches[i]));
        }
        string[] allSnippets = snippets.ToArray();
        string[][] AllMatchArray = Allmatches.ToArray();
        float[][] MatchRelevance = matchesRel.ToArray();
        float[] scoresTemp = new float[scores.Length];
        float[] scoresTemp2 = new float[scores.Length];
        float[] scoresTemp3 = new float[scores.Length];
        for(int i = 0; i < scores.Length; i++){
            scoresTemp[i] = scores[i];
            scoresTemp2[i] = scores[i];
            scoresTemp3[i] = scores[i];
        }
        Array.Sort(scoresTemp3, allSnippets);
        Array.Sort(scoresTemp, AllMatchArray);
        Array.Sort(scores, files);
        Array.Sort(scoresTemp2, MatchRelevance);
        Array.Reverse(allSnippets);
        Array.Reverse(scores);
        Array.Reverse(files);
        Array.Reverse(AllMatchArray);
        Array.Reverse(MatchRelevance);
        return (files, scores, AllMatchArray, MatchRelevance, allSnippets);
    }

    public string GetSnippet(string text, string[] words){
        // if (words.Length == 0){
        //     return "";
        // }  
        // string m = words[0];
        // string separators = $"[\b{m}\b]";
        // Match match = Regex.Match(separators, text);
        // int index = match.Index;
        // int start = text.IndexOf(" ", index - 50);
        // int length = text.IndexOf(".", index) - start;
        // return text.Substring(start, length);
        return "lorem ipsum";
    }

    public string PrintVector(int pos){
        string vector = "";
        for(int i = 0; i < this.voc.Length; i++){
            if(i != this.voc.Length - 1){
                vector = vector + this.matrix[pos].GetTfidf()[i].ToString() + " ";
            } else{
                vector = vector + this.matrix[pos].GetTfidf()[i].ToString();
            }
        }
        return vector;
    }

    public int Module(int x){
        if (x > 0){
            return x;
        }
        return x * -1;
    }
}