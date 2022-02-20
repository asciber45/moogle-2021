/*Un objeto de tipo Corpus guarda todo lo que necesitamos de las palabras del vocabulario
y nos da una acceso de formaa sencilla a la informacion guardada de cada documento*/

using System.Text.Json;

namespace MoogleEngine
{
    public class Corpus
    {
        public Dictionary<string, List<string>> roots = new Dictionary<string, List<string>>();
        public Dictionary<int, Info> documents = new Dictionary<int, Info>();
        public Dictionary<string, float> ITFs = new Dictionary<string, float>();
        public string path = "";
        
        ///<summary> Constructor que recibe la direccion donde estan los documentos <summary>//
        public Corpus(string path)
        {
            this.path = path;
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
               ProcDoc(path, i, stemmdone);                                 
            }
        }
        
        /// <summary> Metodo que procesa los docuementos </summary> ///
        private void ProcDoc(string path, int i, bool stemmdone)
        {
            Info info = new Info(path, i); // Inicializamos un objeto info para guardar todo lo necesario
            string text = File.ReadAllText(Directory.GetFiles(path, "*.txt")[i]).ToLower();
            int count = 0; // Este contador sera un indice de las palabras en el documento

            if (text == null) return; // Revisamos si el documento esta vacio
            string[] words = text.Split(' ');
            
            for (int j = 0; j < words.Length; j++)
            {
                string word = words[j];
                // Filtramos los signos de puntuacion y lineas vacias
                if (word == "" || (word.Length == 1 && Char.IsPunctuation(word[0]))) 
                {
                    count++;
                    continue;
                }

                if (char.IsPunctuation(word[word.Length-1])) word = word.Substring(0, word.Length-1);

                AddWord(word, info, stemmdone); // Guardamos los datos necesarios de cada palabra

                info[word].Add(count);
                count++;
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

                if (!ITFs.ContainsKey(word)) // Preguntamos si es una palabra nueva en el vocabulario
                {
                    ITFs.Add(word, 0);

                    if (!stemmdone) // Aca solo entramos si el json de las raices no ha sido creado
                    {
                        string root = word.Stemmer();
                        if (!roots.ContainsKey(root)) // Preguntamos si ya tenemos esa raiz guardada
                        {
                            List<string> newlist = new List<string>();
                            roots.Add(root, newlist);
                            roots[root].Add(word);
                        }
                        else roots[root].Add(word);
                    }
                }
            }
        }
    }
}