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

            if (!Analysis()) CreateCorpus();

            else
            {
                string idfsjsonstring = File.ReadAllText("..//Cache\\IDFs.json");
                IDFs = JsonSerializer.Deserialize<Dictionary<string, float>>(idfsjsonstring)!;
            }
        }

        /// <summary> Constructor para cuando no se ha analizado este cuerpo de documentos </summary> /// 
        private void CreateCorpus()
        {
            int  numberofdocuments = Directory.GetFiles(path, "*.txt").Length;
            bool stemmdone = Directory.GetFiles("..//Cache").Contains("..//Cache\\stemming.json");

            // Preguntamos si el json con las raices ya esta guardado
            if (stemmdone)
            {
                string rootjsonstring = File.ReadAllText("..//Cache\\stemming.json");
                roots = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(rootjsonstring)!;
                File.Delete("..//Cache\\stemming.json");
            }

            // Iteramos sobre los documentos procesandolos cada uno
            for (int i = 0; i < numberofdocuments; i++)
            {
               ProcDoc(i);                                 
            }

            Similitud.IDF(documents, IDFs); // Halla el IDF de cada palabras

            SaveDocsInfo();
        }

        /// <summary> Guarda la informacion necesaria para proximos usos </summary> ///
        private void SaveDocsInfo()
        {
            Dictionary<int, string> information = Serialize();

            File.WriteAllText("..//Cache\\documents.json", JsonSerializer.Serialize(information));

            File.WriteAllText("..//Cache\\IDFs.json", JsonSerializer.Serialize(IDFs));

            File.WriteAllText("..//Cache\\stemming.json", JsonSerializer.Serialize(roots));

        }

        /// <summary> Serializa cada uno de los obejtos Info de cada documento </summary> ///
        private Dictionary<int, string> Serialize()
        {
            Dictionary<int, string> information = new Dictionary<int, string>();

            for (int i = 0; i < documents.Count; i++)
            {
                information.Add(i, documents[i].Serialize());
            }

            return information;
        }

        /// <summary> Deserializa los diccionarios </summary> ///
        private void GetDocsInfo()
        {
            string docsjsonstring = File.ReadAllText("..//Cache\\documents.json");
            Dictionary<int, string> docs = JsonSerializer.Deserialize<Dictionary<int, string>>(docsjsonstring)!;

            for (int i = 0; i < docs.Count; i++)
            {
                documents.Add(i, new Info(docs[i]));
            }
        }

        /// <summary> Comprueba si los documentos han sido analizados anteriormente </summary> ///
        private bool Analysis()
        {
            if (Directory.GetFiles("..//Cache").Contains("..//Cache\\documents.json"))
            {
                GetDocsInfo();

                for (int i = 0; i < Directory.GetFiles(path, "*.txt").Length; i++)
                {
                    string name = Directory.GetFiles(path, "*.txt")[i];
                    // Comprobamos si se ha hecho alguna modificacion al cuerpo de documentos
                    if (new FileInfo(name).Name != documents[i].name || File.GetLastWriteTime(name) != documents[i].lastmodification)
                    {
                        RemoveInfo(); // Eliminamos los datos guardados para rehacerlos
                        documents = new Dictionary<int, Info>();
                        return false;
                    }
                }

                return true; // Si llegamos a aqui no ha habido cambios
            }

            return false;
        }
        
        /// <summary> Elimina toda la informacion guardada sobre los documentos </summary> ///
        private void RemoveInfo()
        {
            File.Delete("..//Cache\\docuemnts.json");
            File.Delete("..//Cache\\IDFs.json");
        }
        
        /// <summary> Metodo que procesa los docuementos </summary> ///
        private void ProcDoc(int i)
        {
            Info info = new Info(path, i); // Inicializamos un objeto Info para guardar todo lo necesario
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

                AddWord(word, info); // Guardamos los datos necesarios de cada palabra

                if (word != "") info[word].Add(j);
            }
            
            documents.Add(i, info); // Agregamos el objeto Info al diccionario documents
        }

        /// <summary> Metodo que procesa cada palabra </summary> ///
        private void AddWord(string word, Info info)
        {
            if ( word.Length < 1) return;

            if (!info.ContainsKey(word)) // Preguntamos si es una palabra nueva en el docuemento
            {
                info.Add(word);

                if (!IDFs.ContainsKey(word)) // Preguntamos si es una palabra nueva en el vocabulario
                {
                    IDFs.Add(word, 0);

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