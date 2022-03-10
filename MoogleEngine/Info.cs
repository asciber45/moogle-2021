/*Un objeto de tipo Info guarda para cada documento los datos especificos que necesitamos
como su modulo, nombre y el peso de cada palabra en el docuemnto*/

using System.Runtime.Serialization;
using System.Text.Json;


namespace MoogleEngine
{
    public class Info
    {
        // Guarda para cada palabra los indices en los que aparece en el documento
        public  Dictionary<string, List<int>> words = new Dictionary<string, List<int>>();
        public string name = ""; // Nombre del documento
        // Guarda para cada palabra el peso que tiene en el docuemnto
        public Dictionary<string, float> weigths = new Dictionary<string, float>();
        public float modulo = 0; // Modulo del documento
        public DateTime lastmodification; // Fecha de la ultima modificacion

        ///<summary> Constructor que recibe la direccion del documento </summary>///
        public Info(string path, int i)
        {
            string adress = Directory.GetFiles(path, "*.txt")[i];

            name = new FileInfo(adress).Name;
            lastmodification = File.GetLastWriteTime(adress);
        }

        /// <summary> Constructor que no necesita de una direccion </summary>///
        public Info(){}

        /// <summary> Constructor que recibe un string serializado </summary> ///
        public Info(string serializedstring)
        {
            Dictionary<string, string> docinfo = JsonSerializer.Deserialize<Dictionary<string, string>>(serializedstring)!;
            words = JsonSerializer.Deserialize<Dictionary<string, List<int>>>(docinfo["wd"])!;
            weigths = JsonSerializer.Deserialize<Dictionary<string, float>>(docinfo["wg"])!;
            modulo = JsonSerializer.Deserialize<float>(docinfo["m"])!;
            name = JsonSerializer.Deserialize<string>(docinfo["n"])!;
            lastmodification = JsonSerializer.Deserialize<DateTime>(docinfo["lm"])!;
        }

        /// <summary> Transforma en un string serializado todas las propiedades de esta instancia </summary> ///
        public string Serialize()
        {
            Dictionary<string, string> docinfo = new Dictionary<string, string>();
            string words = JsonSerializer.Serialize(this.words);
            docinfo.Add("wd", words);
            string weigths = JsonSerializer.Serialize(this.weigths);
            docinfo.Add("wg", weigths);
            string modulo = JsonSerializer.Serialize(this.modulo);
            docinfo.Add("m", modulo);
            string lastmodification = JsonSerializer.Serialize(this.lastmodification);
            docinfo.Add("lm", lastmodification);
            string name = JsonSerializer.Serialize(this.name);
            docinfo.Add("n", name);

            return JsonSerializer.Serialize(docinfo);
        }

        /// <summary> Devuelve ConstainsKey aplicado al diccionario words </summary>///
        public bool ContainsKey(string word)
        {
            return words.ContainsKey(word);
        }

        /// <summary> Agrega elementos al diccionario words </summary>///
        public void Add(string word)
        {
            if ( word.Length < 1) return;
            List<int> newlist = new List<int>();
            words.Add(word, newlist);
        }

        // Indexador en diccionario words
        public List<int> this[string word]
        {
            get
            {
              return words[word.ToLower()];
            }
            set{words[word] = value;}
        }

        ///<summary> Metodo que devuelve el numero de ocurrencias de la palabra mas comun del docuemento </summary>///
        private int FindMax(Dictionary<string, float> itfs)
        {
            int max = 0;
            foreach (var word in words)
            {
                max = Math.Max(max, word.Value.Count());
            }
            return max;
        } 

        ///<summary> Devuelve el modulo de un dcouemnto y calcula los pesos de cada palabra </summary>///
        public void Modulo(Dictionary<string, float> idfs)
        {
            int max = FindMax(idfs);
            float sum = 0;

            foreach (var pair in words)
            {
                if (pair.Key == "") continue;
                float tf = (float)words[pair.Key].Count/max;
                float itf = idfs[pair.Key];
                float weigth = tf*itf; // Calcula el peso de cada palabra
                weigths.Add(pair.Key, weigth); // Guarda los pesos en el diccionario weigths
                sum += (float)Math.Pow(weigth, 2);
            }

            modulo = (float)Math.Sqrt(sum);
        }
    } 
}