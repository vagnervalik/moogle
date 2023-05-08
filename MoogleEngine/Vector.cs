using System.Linq;
using System.Text.RegularExpressions;
namespace MoogleEngine;

public class Vector{
    private string[] words; //WORDS TO VECTORIZE.
    private string[] voc;   //VOCABULARY
    private float[]? idfs;  //VOCABULARY IDFS
    private float[]? tfs;   //WORDS'S TF'S
    private float[] vector; //WORDS'S TF-IDFS

    //CONSTRUCTOR FOR THE DOCUMENT VECTOR
    public Vector(string[] words, string[] vocabulary, float[] Idfs){
        this.words = words;
        this.voc = vocabulary;
        this.idfs = Idfs;
        var x = this.Vectorize();
        this.tfs = x.tf;
        this.vector = x.tfidf;
    }

    //CONSTRUCTOR FOR THE QUERY VECTOR
    public Vector(string words, string[] vocabulary){
        this.words = this.GetWords(words);
        this.voc = vocabulary;
        this.vector = this.GetQueryVector();
    }

    //CONSTRUCTOR AID FOR FIELD WORDS IN THE QUERYVECTOR. SPLITS THE QUERY INTO WORDS.
    public string[] GetWords(string words){
        string[] W = Regex.Split(words.ToLower(), "[^a-zA-Z]+").Where(x => !string.IsNullOrEmpty(x)).ToArray();
        return W;
    }

    //CONSTRUCTOR AID FOR THE VECTOR FIELD IN QUERYVECTOR. 
    public float[] GetQueryVector(){
        float[] query = new float[this.voc.Length];
        int val = 0;
        for(int i = 0; i < this.voc.Length; i++){
            val = (Array.Exists(this.words, x => x == this.voc[i])) ? 1 : 0;
            query[i] = val;
        }
        return query;
    }

    //CONSTRUCTOR AID FOR THE FIELD VECTOR OF THE DOCUMENT VECTOR. GETS THE TF-IDFS OF ALL THE WORDS.
    public (float[] tfidf, float[] tf) Vectorize(){
        float[] tfidf = new float[this.Count()];
        float[] tf = new float[this.Count()];
        float count = 0f;
        for(int i = 0; i < this.Count(); i++){
            count = this.words.Count(x => x == this.voc[i]);
            tfidf[i] = (count/Convert.ToSingle(this.words.Length)) * this.GetIdf(i);
            tf[i] = (count/Convert.ToSingle(this.words.Length));
        }
        return (tfidf, tf);
    } 
    public float GetTf(int pos){
        return this.tfs[pos];
    }
    public float GetIdf(int pos){
        return this.idfs[pos];
    }
    public float GetVal(int pos){
        return this.vector[pos];
    }
    public int Size(){
        return this.vector.Length;
    }
    public int Count(){
        return this.idfs.Length;
    }

    //MULTYPLY VECTORS METHOD
    public (float score, string[] matches, float[] matchesRel) Multiply(Vector V){
        if (this.Size() != V.Size()){
            throw new ArgumentException($"Vector must be the same size");
        }
        float count = 0f;
        List<string> matches = new List<string>();
        List<float> relevance = new List<float>();
        float actual = 0;
        for(int i = 0; i < this.Size(); i++){
            actual = this.GetVal(i) * V.GetVal(i);
            count += actual;
            if(actual != 0){
                matches.Add(this.voc[i]);
                relevance.Add(actual);
            }
        }
        string[] m = matches.ToArray();
        float[] f = relevance.ToArray();
        Array.Sort(f, m);
        Array.Reverse(m);
        Array.Reverse(f);
        return (count, m, f);
    }
}