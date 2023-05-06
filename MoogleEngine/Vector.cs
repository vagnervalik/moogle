using System.Linq;
using System.Text.RegularExpressions;
namespace MoogleEngine;

public class Vector{
    private string[] words;
    private string[] voc;
    private float[]? idfs;
    private float[]? tfs;
    private float[] vector;

    public Vector(string[] words, string[] vocabulary, float[] Idfs){
        this.words = words;
        this.voc = vocabulary;
        this.idfs = Idfs;
        var result = this.Vectorize();
        this.vector = result.Item1;
        this.tfs = result.Item2;
    }

    public Vector(string words, string[] vocabulary){
        this.words = this.GetWords(words);
        this.voc = vocabulary;
        this.vector = this.GetQueryVector();
    }

    public string[] GetWords(string words){
        string[] W = Regex.Split(words.ToLower(), "[^a-zA-Z]+").Where(x => !string.IsNullOrEmpty(x)).ToArray();
        return W;
    }

    public float[] GetQueryVector(){
        float[] query = new float[this.voc.Length];
        int val = 0;
        for(int i = 0; i < this.voc.Length; i++){
            val = (Array.Exists(this.words, x => x == this.voc[i])) ? 1 : 0;
            query[i] = val;
        }
        return query;
    }

    public (float[],float[]) Vectorize(){
        float[] tfidf = new float[this.idfs.Length];
        float[] tf = new float[this.idfs.Length];
        float count = 0f;
        for(int i = 0; i < this.idfs.Length; i++){
            count = this.words.Count(x => x == this.voc[i]);
            tfidf[i] = (count/Convert.ToSingle(this.words.Length)) * this.idfs[i];
            tf[i] = (count/Convert.ToSingle(this.words.Length));
        }
        return (tfidf, tf);
    } 

    public float[] GetTfidf(){
        return this.vector;
    }
    public float[] GetTf(){
        return this.tfs;
    }
    public void Display(){
        for(int i = 0; i < 10; i++){
            Console.Write(this.vector[i] + ",    ");
        }
        Console.WriteLine();
    }
    public int Size(){
        return this.vector.Length;
    }
    public (float score, string[] matches, float[] matchesScores) Multiply(Vector V){
        if (this.Size() != V.Size()){
            throw new ArgumentException($"Vector must be the same size");
        }
        float count = 0f;
        List<string> matches = new List<string>();
        List<float> relevance = new List<float>();
        float actual = 0;
        for(int i = 0; i < this.Size(); i++){
            actual = this.vector[i] * V.GetTfidf()[i];
            count += actual;
            if(actual != 0){
                matches.Add(this.voc[i]);
                relevance.Add(actual);
            }
        }
        string[] m = matches.ToArray();
        float[] f = relevance.ToArray();
        Array.Sort(f, m);
        Array.Reverse(f);
        Array.Reverse(m);
        return (count, m, f);
    }
}