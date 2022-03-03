/*Un objeto de tipo Corpus guarda todo lo que necesitamos de las palabras del vocabulario
y nos da una acceso de formaa sencilla a la informacion guardada de cada documento*/

using System.Text.Json;

namespace MoogleEngine
{
    public class Corpus
    {
        public Dictionary<string, List<string>> roots = new Dictionary<string, List<string>>();
        public Dictionary<int, Info> documents = new Dictionary<int, Info>();
        public Dictionary<string, float> IDFs = new Dictionary<string, float>();
        public Dictionary<string, List<string>> synonymus = new Dictionary<string, List<string>>();
        public string path = "";
        
        ///<summary> Constructor que recibe la direccion donde estan los documentos </summary>///
        public Corpus(string path)
        {
            this.path = path;
            string synjsonstring = File.ReadAllText("..//Synonymus\\synonymus.json");
            synonymus = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(synjsonstring)!;
            int  numberofdocuments = Directory.GetFiles(path, "*.txt").Length;
            bool stemmdone = Directory.GetFiles("..//Cache").Contains("..//Cache\\stemming.json");

            // Preguntamos si el json con las raices ya esta guardado
            if (stemmdone)
            {
                string jsonstring = File.ReadAllText("..//Cache\\stemming.json");
                roots = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(jsonstring)!;
            }

            // Iteramos sobre los documentos procesandolos cada uno
            for (int i = 0; i < numberofdocuments; i++)
            {
               ProcDoc(i, stemmdone);                                 
            }
        }
        
        /// <summary> Metodo que procesa los docuementos </summary> ///
        private void ProcDoc(int i, bool stemmdone)
        {
            Info info = new Info(path, i); // Inicializamos un objeto info para guardar todo lo necesario
            string text = File.ReadAllText(Directory.GetFiles(path, "*.txt")[i]).ToLower();

            if (text == null) return; // Revisamos si el documento esta vacio
            string[] words = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            
            for (int j = 0; j < words.Length; j++)
            {
                string word = words[j];
                // Filtramos los signos de puntuacion
                if (word.Length == 1 && Char.IsPunctuation(word[0])) 
                {
                    continue;
                }

                while (word.Length >= 2 && (char.IsPunctuation(word[0]) || char.IsPunctuation(word[word.Length - 1])))
                {
                    if (char.IsPunctuation(word[word.Length - 1])) word = word.Substring(0, word.Length - 1);
                    if (char.IsPunctuation(word[0])) word = word.Substring(1);
                }

                AddWord(word, info, stemmdone); // Guardamos los datos necesarios de cada palabra

                info[word].Add(j);
            }

            documents.Add(i, info); // Agregamos el objeto info al diccionario documents
            // Serializamos el json de raices para proximos usos
            if (!stemmdone) File.WriteAllText("..//Cache\\stemming.json", JsonSerializer.Serialize(roots));
        }

        /// <summary> Metodo que procesa cada palabra </summary> ///
        private void AddWord(string word, Info info, bool stemmdone)
        {
            if (!info.ContainsKey(word)) // Preguntamos si es una palabra nueva en el docuemento
            {
                info.Add(word);

                if (!IDFs.ContainsKey(word)) // Preguntamos si es una palabra nueva en el vocabulario
                {
                    IDFs.Add(word, 0);

                    if (!stemmdone) // Aca solo entramos si el json de las raices no ha sido creado
                    {
                        string root = word.Stemmer();
                        if (!roots.ContainsKey(root)) // Preguntamos si ya tenemos esa raiz guardada
                        {
                            roots.Add(root, new List<string>());
                            roots[root].Add(word);
                        }
                        else roots[root].Add(word);
                    }
                }
            }
        }
    }
}