/*Un objeto de tipo Info guarda para cada documento los datos especificos que necesitamos
como su modulo, nombre y el peso de cada palabra en el docuemnto*/


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

        ///<summary> Constructor que recibe la direccion del documento </summary>///
        public Info(string path, int i)
        {
            name = Name(path, i);      
        }

        /// <summary> Constructor que no necesita de una direccion </summary>///
        public Info(){}

        /// <summary> Devuelve ConstainsKey aplicado al diccionario words </summary>///
        public bool ContainsKey(string word)
        {
            return words.ContainsKey(word);
        }

        /// <summary> Agrega elementos al diccionario words </summary>///
        public void Add(string word)
        {
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

        public string Name(string adress, int i)
        {
            return new FileInfo((Directory.GetFiles(adress, "*.txt"))[i]).Name;
        }

        ///<summary> Metodo que devuelve el numero de ocurrencias de la palabra mas comun del docuemento </summary>///
        public int FindMax(Dictionary<string, float> itfs)
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