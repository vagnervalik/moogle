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

    //CONSTRUCTOR AID FOR THE FIELD Idfs.
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
    //CONSTRUCTOR AID FOR THE FIELD matrix.
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
                Dict[this.voc[i]].Add(j, (this.matrix[j].GetVal(i), this.matrix[j].GetTf(i)));
            }
        }
        return Dict;
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
             matchesRel.Add(x.matchesRel);
        }
        for(int i = 0; i < this.matrix.Length; i++){
            snippets.Add(this.GetSnippet(this.Docs.GetAllText()[i], Allmatches[i], matchesRel[i]));
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
    string GetSnippet(string text, string[] words, float[] relevance){
        if (words.Length == 0){
            return "";
        }
        if(text.Length < 100){
            return text;
        }
        float maxCount = 0;
        int maxStartIndex = 0;
        for (int i = 0; i < text.Length - 100; i++){
            float count = 0;
            int k = 1;
            for (int j = 0; j < words.Length; j++){
                int[] matches = GetAllMatches(text.Substring(i, 100), words[j]);
                if (matches.Length > 0){
                    count += Convert.ToSingle(Math.Log(matches.Length + 1, 1.5)) * relevance[j] * k;
                    k++;
                }
            }
            if (count > maxCount){
                maxCount = count;
                maxStartIndex = i;
            }
        }
        foreach(string word in words){
            Console.Write(word + maxStartIndex);
        }
        Console.WriteLine();
        int index = maxStartIndex;
        int len = 100;
        if (text.LastIndexOf(' ', maxStartIndex) != -1){
            index = text.LastIndexOf(' ' , maxStartIndex) + 1;
        }
        if (text.IndexOf(' ', index + 100) != -1){
            len = (text.IndexOf(' ', index + 100)) - index;
        }
        return text.Substring(index, len);
    }

    public int[] GetAllMatches(string text, string word){
        List<int> indexes = new List<int>();
        MatchCollection matches = Regex.Matches(text, @$"\b{word}\b");
        foreach(Match match in matches){
            indexes.Add(match.Index);
        }
        return indexes.ToArray();
    }

    public string GetSuggestion(string qry){
        string[] words = Regex.Split(qry.ToLower(), "[^a-zA-Z]+").Where(x => !string.IsNullOrEmpty(x)).ToArray();
        List<(string wrong, string correct)> tup = new List<(string wrong, string correct)>();
        foreach(string word in words){
            if(!Array.Exists(this.voc, x => x == word)){
                tup.Add((word, this.GetCorrection(word)));
                Console.WriteLine(this.GetCorrection(word));
            }
        }
        string q = qry;
        foreach((string wrong, string correct) x in tup){
            q = q.Replace(x.wrong, x.correct);
        }
        return q;
    }

    public string GetCorrection(string word){
        if(Array.Exists(this.voc, x => x == word)){
            return word;
        }
        int place = Array.BinarySearch(this.voc, word);
        int min = int.MaxValue;
        int minPos = place;
        for (int i = 0; i < this.voc.Length; i++){
            int cur = this.GetLevanstheins(word, this.voc[i]);
            if(cur < min){
                min = cur;
                minPos = i;
            }
        }
        return this.voc[minPos];
    }

    public int GetLevanstheins(string s, string t){
    int n = s.Length;
    int m = t.Length;
    int[,] d = new int[n + 1, m + 1];
    // Verificar argumentos
    if (n == 0){
        return m;
    }
    if (m == 0){
        return n;
    }

    // Inicializar matriz
    for (int i = 0; i <= n; d[i, 0] = i++){
    }

    for (int j = 0; j <= m; d[0, j] = j++){
    }

    // Comenzar iteraciones
    for (int i = 1; i <= n; i++){
        for (int j = 1; j <= m; j++){
            // Calcular costo
            int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

            d[i, j] = Math.Min(
                Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                d[i - 1, j - 1] + cost);
        }
    }
    
    // Devolver costo
    return d[n, m];
    }

    public (string word, int pos) GetMin(string text, string[] words, int pos){
        if (pos > text.Length || pos < 0){
            throw new ArgumentException("Index was out of range!!!");
        }
        int min = int.MaxValue;
        string word = "";
        for(int i = 0; i < words.Length; i++){
            int cur = text.IndexOf(words[i], pos);
            if (cur == -1) continue;
            if(cur < min){
                min = cur;
                word = words[i];
            }
        }
        if(min == int.MaxValue){
            return (null, -1);
        }
        return (word, min);
    }

    public int Module(int x){
        if (x > 0){
            return x;
        }
        return x * -1;
    }
}