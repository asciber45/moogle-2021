namespace MoogleEngine
{
    public static class AuxMethods
    {
        #region Cercania
         // Modifica el score de un documento en dependencia de la cercania buscada
        public static float[] Closeness(float[] score, Query query, Corpus corpus)
        {
            for (int i = 0; i < query.operators[0].Count; i++)
            {
                if (query.operators[0][i] == 1)
                {
                    for (int j = 0; j < corpus.documents.Count-1; j++)
                    {
                        bool a = corpus.documents[j].ContainsKey(query.list[0][i]);
                        bool b = corpus.documents[j].ContainsKey(query.list[0][i+1]);
                        float c = Distance(i, j, a, b, query, corpus);
                        score[j] = score[j]/c;                         
                    }
                }
            }
            return score;
        }

        // Devuelve la distancia entre dos palabras en un documento
        private static float Distance(int i, int j, bool a, bool b, Query query, Corpus corpus)
        {
            if (a && b)
            {
                int distance = WordDistance(i, j, query, corpus);
                return distance;
            }
            else
            {
                return corpus.documents[j].words.Count; 
            }
        }

        // Devuelve cantidad minima de palabras entre dos palabras en un documento
        private static int WordDistance(int i, int j, Query query, Corpus corpus)
        {
            List<int> a = corpus.documents[j][query.list[0][i]];
            List<int> b = corpus.documents[j][query.list[0][i+1]];
            int x = 0; int y = 0;
            int min = Math.Abs(a[0]-b[0]);

            while (x + y < a.Count + b.Count -2)
            {
                if (y == b.Count-1 || (x < a.Count-1 && a[x] < b[y])) x++;
                else y++;
                min = Math.Min(min, Math.Abs(a[x] - b[y]));        
            }

            return min;
        }
        #endregion

        #region Snippet
        // Encontrar snippet
        public static string FindSnippet(int i, Query query, Corpus corpus, string path)
        {
            string word = BestSnippet(query, corpus, i);

            return Take10Words(i, word, corpus, path);
        }

        // Entre palabras originales, raices y sinonimos selecciona la de mayor importancia en el documento
        private static string BestSnippet(Query query, Corpus corpus, int k)
        {
            List<string>[] list = new List<string>[3];

            for (int i = 0; i < 3; i++)
            {
                list[i] = new List<string>();
                // Para un documento se guardan las listas de las palabras del query qu están en él
                for (int j = 0; j < query.list[i].Count; j++)
                {
                    if (!corpus.documents[k].ContainsKey(query.list[i][j])) continue;
                    list[i].Add(query.list[i][j]);
                }

                // Nos quedamos con la palabra más relevante
                list[i] = new List<string>{Similitud.BestWord(list[i], corpus)}; 
            }

            if (list[0][0] != "") return list[0][0]; // Mejor palabra original
            else if (list[1][0] != "") return list[1][0]; // Mejor raíz
            else return list[2][0]; // Mejor sinónimo
        }

        // Selccionada una palabra en un documento, toma una ocurrencia random y printea 5 palabras para cada lado
        private static string Take10Words(int i, string word, Corpus corpus, string path)
        {
            string[] words = File.ReadAllText(Directory.GetFiles(path, "*.txt")[i]).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string snippet = "";
            int randomnumber = new Random().Next(0, corpus.documents[i][word].Count);
            int a = 0;
            int b = 10;

            if (corpus.documents[i][word][randomnumber] < 5) a = 5-corpus.documents[i][word][randomnumber];
            if (corpus.documents[i][word][randomnumber] > words.Length-5) b = words.Length-corpus.documents[i][word][randomnumber];

            for (int j = a; j < b; j++)
            {
                snippet += " " + words[corpus.documents[i][word][randomnumber]+j-5];
            }

            return snippet;
        }

        #endregion

        public static string ToString(List<string> list)
        {
            string query = list[0];
            List<string> pased = new List<string>{query};
            for (int i = 1; i < list.Count; i++)
            {
                if (pased.IndexOf(list[i]) != -1) continue;
                query += " " + list[i];
                pased.Add(list[i]);
            }
            return query;
        }
    }
}