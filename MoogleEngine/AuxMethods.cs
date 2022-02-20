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
        public static float Distance(int i, int j, bool a, bool b, Query query, Corpus corpus)
        {
            if (a && b)
            {
                int c = WordDistance(i, j, query, corpus);
                return c;
            }
            else
            {
                return int.MaxValue; 
            }
        }

        // Devuelve cantidad minima de palabras entre dos palabras en un documento
        public static int WordDistance(int i, int j, Query query, Corpus corpus)
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
            int indexq = 0;
            while (!corpus.documents[i].ContainsKey(query.list[0][indexq]) && !corpus.documents[i].ContainsKey(query.list[1][indexq])) indexq++;
            int indexr = indexq;

            for (int j = indexq; j < query.list[0].Count; j++)
            {
                if (corpus.documents[i].ContainsKey(query.list[0][indexq]) && corpus.documents[i].ContainsKey(query.list[0][j]))
                    indexq = (corpus.documents[i].weigths[query.list[0][indexq]] > corpus.documents[i].weigths[query.list[0][j]])? indexq : j;
                if (corpus.documents[i].ContainsKey(query.list[1][indexr]) && corpus.documents[i].ContainsKey(query.list[1][j]))
                    indexr = (corpus.documents[i].weigths[query.list[1][indexr]] > corpus.documents[i].weigths[query.list[1][j]])? indexr : j;
            }

            string q = query.list[0][indexq];
            string r = query.list[1][indexr];
            string word = "";

            if (corpus.documents[i].ContainsKey(q) && corpus.documents[i].ContainsKey(r))
                word = Similitud.TotalWeight(q, corpus) < Similitud.TotalWeight(r, corpus)? r : q;
            else if (corpus.documents[i].ContainsKey(q)) word = q;
            else word = r;

            return Take10Words(i, word, corpus, path);
        }
        public static string Take10Words(int i, string word, Corpus corpus, string path)
        {
            string[] words = File.ReadAllText(Directory.GetFiles(path, "*.txt")[i]).Split(' ');
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
            // string snippet = File.ReadAllText(Directory.GetFiles(path, "*.txt")[i]).ToLower();
            // int index = snippet.IndexOf(" " + word + " ");

            // if (index > 20 && index < snippet.Length-30)
            // {
            //     snippet = snippet.Substring(index-20, 50);
            //     return snippet;
            // }
            // if (index <= 20 && index < snippet.Length-40)
            // {
            //     snippet = snippet.Substring(0, index+50);
            //     return snippet;
            // }
            // if (index > 20 && index >= snippet.Length-50)
            // {
            //     snippet = snippet.Substring(index-20);
            //     return snippet;
            // }
            // else
            // {
            //     return snippet;
            // } 
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