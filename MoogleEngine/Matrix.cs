namespace MoogleEngine;
using System.Text.RegularExpressions;

public class Matrix{
    private DataBase Docs;
    private float[] Idfs;
    private string[] voc;
    private Vector[] matrix;

    public Matrix(DataBase Docs){
        this.Docs = Docs;
        this.voc = this.Docs.Vocabulary();
        this.Idfs = this.GetIdfs();
        this.matrix = this.GetMatrix();
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

    /// <summary>
    /// The function takes a query vector and returns an array of document names, scores, matched terms,
    /// relevance scores, and snippets sorted by relevance to the query.
    /// </summary>
    /// <param name="Vector">The "Vector" parameter is an input vector used to calculate the scores for
    /// each document in the collection. It is likely a vector representation of the user's
    /// query.</param>
    /// <returns>
    /// The method is returning a tuple containing five arrays: 
    /// - an array of file names
    /// - an array of scores
    /// - a 2D array of matches for each file
    /// - a 2D array of relevance scores for each match
    /// - an array of snippets for each file
    /// </returns>
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

    /// <summary>
    /// The function returns a snippet of text that contains the most relevant words based on their
    /// relevance scores.
    /// </summary>
    /// <param name="text">The text to search for snippets in.</param>
    /// <param name="words">An array of strings representing the words to search for in the
    /// text.</param>
    /// <param name="relevance">An array of floats representing the relevance/importance of each word in
    /// the search query. The order of the relevance values should correspond to the order of the words
    /// in the words array.</param>
    /// <returns>
    /// The method is returning a string, which is a snippet of text from the input text that contains
    /// the highest relevance score for the given words. The snippet is determined by searching for the
    /// highest count of matches for the given words within a 200-character window of the input text,
    /// and then returning a 200-character substring centered around the highest-scoring match. The
    /// substring is then adjusted to start and end
    /// </returns>
    public string GetSnippet(string text, string[] words, float[] relevance){
        if (words.Length == 0){
            return "";
        }
        if(text.Length < 100){
            return text;
        }
        float maxCount = 0;
        int maxStartIndex = 0;
        for (int i = 0; i < text.Length - 200; i++){
            float count = 0;
            int k = 10;
            for (int j = 0; j < words.Length; j++){
                int[] matches = GetAllMatches(text.Substring(i, 200), words[j]);
                if (matches.Length > 0){
                    count += Convert.ToSingle(matches.Length) * relevance[j] * k;
                    k += 1000;
                }
            }
            if (count > maxCount){
                maxCount = count;
                maxStartIndex = i;
            }
        }
        int x = this.Correct(text.Substring(maxStartIndex, 200), words);
        int index = this.RejectNegative(x + maxStartIndex);
        int len = 200;
        if (text.LastIndexOf(' ', index) != -1){
            index = text.LastIndexOf(' ' , index) + 1;
        }
        if (text.IndexOf(' ', index + 200) != -1){
            len = (text.IndexOf(' ', index + 200)) - index;
        }
        return text.Substring(index, len);
    }

    /// <summary>
    /// The function returns 0 if the input is negative, otherwise it returns the input.
    /// </summary>
    /// <param name="x">an integer value that may or may not be negative.</param>
    /// <returns>
    /// If the input integer `x` is less than 0, then the method returns 0. Otherwise, it returns the
    /// input integer `x`.
    /// </returns>
    public int RejectNegative(int x){
        if(x < 0 )return 0;
        return x;
    }
    /// <summary>
    /// The function calculates the correction needed for a given text based on the position of
    /// specified words within it.
    /// </summary>
    /// <param name="text">a string representing the text to be corrected</param>
    /// <param name="words">An array of strings containing the words to be searched for in the text
    /// parameter.</param>
    /// <returns>
    /// The method is returning an integer value that represents the correction needed to adjust the
    /// position of a string of words within a given text.
    /// </returns>
    public int Correct(string text, string[] words){
        int min = int.MaxValue;
        int max = int.MinValue;
        for(int i = 0; i < words.Length; i++){
            MatchCollection matches = Regex.Matches(text,  @$"(?<![a-zA-Z0-9]){words[i]}(?![a-zA-Z0-9])", RegexOptions.IgnoreCase);
            if (matches.Count == 0) continue;
            int cur1 = matches[0].Index;
            int cur2 = matches[matches.Count - 1].Index;
            if(cur1 < min){
                min = cur1;
            }
            if(cur2 > max){
                max = cur2;
            }
        }
        if(min == max){
            return 100;
        }
        int correction = 0;
        int sum = min + max;
        if(sum >= 180 && sum <= 200){
        } else if(sum < 180){
            correction = (sum - 180)/2;
        } else if(sum > 200){
            correction = (sum - 200)/2;
        }
        return correction;
    }
    /// <summary>
    /// The function returns an array of indexes where a given word appears in a given text, using
    /// regular expressions to match the word.
    /// </summary>
    /// <param name="text">The text in which we want to find all occurrences of the word.</param>
    /// <param name="word">The word to search for in the text.</param>
    /// <returns>
    /// An array of integers representing the indexes of all matches of the given word in the given
    /// text.
    /// </returns>
    public int[] GetAllMatches(string text, string word){
        List<int> indexes = new List<int>();
        // MatchCollection matches = Regex.Matches(text,  @$"(?<!\w)(_)?\b{word}\b(_)?(?!\w)", RegexOptions.IgnoreCase);
        MatchCollection matches = Regex.Matches(text, @$"(?<![a-zA-Z0-9]){word}(?![a-zA-Z0-9])", RegexOptions.IgnoreCase);
        foreach(Match match in matches){
            indexes.Add(match.Index);
        }
        return indexes.ToArray();
    }

   /// <summary>
   /// The function takes a string query, identifies incorrect words in it, suggests corrections for
   /// those words, and returns the corrected query.
   /// </summary>
   /// <param name="qry">The input query string that needs to be checked for spelling errors and
   /// suggestions.</param>
   /// <returns>
   /// The method `GetSuggestion` returns a string that is the input query `qry` with any misspelled
   /// words corrected based on the vocabulary `voc` provided to the object.
   /// </returns>
    public string GetSuggestion(string qry){
        string[] words = Regex.Split(qry.ToLower(), "[^a-zA-Z]+").Where(x => !string.IsNullOrEmpty(x)).ToArray();
        List<(string wrong, string correct)> tup = new List<(string wrong, string correct)>();
        foreach(string word in words){
            if(!Array.Exists(this.voc, x => x == word)){
                tup.Add((word, this.GetCorrection(word)));
            }
        }
        string q = qry;
        foreach((string wrong, string correct) x in tup){
            q = q.Replace(x.wrong, x.correct);
        }
        return q;
    }

    /// <summary>
    /// The function takes a word and returns either the word itself if it exists in a given vocabulary
    /// array or the closest matching word based on Levenshtein distance.
    /// </summary>
    /// <param name="word">The word that needs to be corrected.</param>
    /// <returns>
    /// The method is returning a string, which is the closest matching word to the input word based on
    /// the Levenshtein distance algorithm.
    /// </returns>
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

    /// <summary>
    /// The function calculates the Levenshtein distance between two strings using dynamic programming.
    /// </summary>
    /// <param name="s">a string representing the first word or phrase</param>
    /// <param name="t">The second string input for which we want to calculate the Levenshtein
    /// distance.</param>
    /// <returns>
    /// an integer value which represents the Levenshtein distance between two input strings 's' and
    /// 't'.
    /// </returns>
    public int GetLevanstheins(string s, string t){
        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];
        if (n == 0){
            return m;
        }
        if (m == 0){
            return n;
        }

        for (int i = 0; i <= n; d[i, 0] = i++){
        }

        for (int j = 0; j <= m; d[0, j] = j++){
        }

        for (int i = 1; i <= n; i++){
            for (int j = 1; j <= m; j++){
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }

    
    /// <summary>
    /// The function returns the minimum occurrence of a word from a given list of words in a given text
    /// starting from a specified position.
    /// </summary>
    /// <param name="text">The input text string to search for the minimum occurrence of a word from the
    /// given array of words.</param>
    /// <param name="words">An array of strings containing the words to search for in the text.</param>
    /// <param name="pos">The starting position in the text where the search for the minimum word
    /// occurrence should begin.</param>
    /// <returns>
    /// A tuple containing a string and an integer value. The string represents the word found in the
    /// text that matches one of the words in the given array, and the integer represents the position
    /// of the first character of the found word in the text. If no match is found, the tuple contains a
    /// null string and a value of -1.
    /// </returns>
    public (string word, int pos) GetMin(string text, string[] words, int pos){
        if (pos > text.Length || pos < 0){
            throw new ArgumentException("Index was out of range!!!");
        }
        int min = int.MaxValue;
        string word = "";
        for(int i = 0; i < words.Length; i++){
            Match match = Regex.Match(text.Substring(pos), @$"(?<![a-zA-Z0-9]){words[i]}(?![a-zA-Z0-9])", RegexOptions.IgnoreCase);
            if (!match.Success) continue;
            int cur = match.Index + pos;
            if(cur < min){
                min = cur;
                word = text.Substring(cur, words[i].Length);
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