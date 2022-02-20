/*Un objeto del tipo Query analiza la consulta y obtiene de ella toda la informacion necesaria*/

namespace MoogleEngine
{
    public class Query
    {
        // Queda guardada la posicion inicial de los operadores antes de ser eliminados
        public List<int>[] operators = new List<int>[2];
        // Dos listas, una con las palabras de la consulta, otras con palabras relacionadas por la misma raiz
        public List<string>[] list = new List<string>[2];
        // Un objeto Info por cada una de las listas anteriores
        public Info[] info = new Info[2];

        ///<summary> Constructor que recibe la consulta en forma de string y el corpus de docuementos </summary>///
        public Query(string query, Corpus corpus)
        {
            list[0] = new List<string>(); // Inicializa lista de palabras buscadas
            list[1] = new List<string>(); // Inicializa lista de raices buscadas
            ProcQuery(query, corpus);
        }

        ///<summary> Metodo que analiza cada palabra de la consulta </summary>///
        public void ProcQuery(string query, Corpus corpus)
        {
            info[0] = new Info(); // Inicializa Info de las palabras de la consulta
            info[1] = new Info(); // Inicilaiza Info de las raices de las palabras
            operators = Operators(query);
            List<string> words = query.Split(new char[] { ' ', '~', '!', '^' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            for (int i = 0; i < words.Count; i++)
            {
                double counter = 0;
                string word = words[i].ToLower();

                if (word[0] == '*') // Cuenta la cantidad de * antes de la palabra
                {
                    while (word[0] == '*')
                    {
                        word = word.Substring(1);
                        counter++;
                    }
                    counter = Math.Pow(2, counter);
                }

                if (corpus.ITFs.ContainsKey(word) && corpus.ITFs[word] <= 0) // Exluye palabras nada relevantes
                {
                    words.RemoveAt(i);
                    i--;
                    continue;
                }

                word = AddSuggestion(word, corpus); // Sugiere una palabra que pertenezca al vocabulario

                AddRoot(word, corpus, i); // Guarda la raiz de la palabra

                if (!corpus.ITFs.ContainsKey(word)) // Guarda las palabras nuevas con itf = 0
                {
                    corpus.ITFs.Add(word, 0);
                }

                while (counter >= 0)
                {
                    info[0][word].Add(i);
                    counter--;
                }
            }
            info[0].Modulo(corpus.ITFs);
            info[1].Modulo(corpus.ITFs);
        }

        private string AddSuggestion(string word, Corpus corpus)
        {
            string newword = Suggestion(word, corpus);

            if (newword != "")
            {
                word = newword;
            }

            if (!info[0].ContainsKey(word))
            {
                info[0].Add(word);
            }
            list[0].Add(word);
            return word;
        }

        private void AddRoot(string word, Corpus corpus, int i)
        {
            if (corpus.roots.ContainsKey(word.Stemmer()))
            {
                string root = (operators[1][i] == 1) ? "" : Similitud.BestWord(corpus.roots[word.Stemmer()], word, corpus);

                if (!info[1].ContainsKey(word))
                {
                    info[1].Add(root);
                }
                info[1][root].Add(1);
                list[1].Add(root);
            }
            else list[1].Add("");
        }

        ///<summary> Recibe la consulta y devuelve las posiciones de los operadores ~, ^, ! </summary>///
        private static List<int>[] Operators(string query)
        {
            List<int>[] operators = new List<int>[2];
            // Guarda 0 para los espacios, 1 para operador de cercania ~
            operators[0] = new List<int>(); 
            // Guarda 2 si no hay operadores, 0 para operador de inclusion, 1 para operador de exclusion
            operators[1] = new List<int>(); 
            
            if (query == "") return operators;

            if (query[0] == '^') operators[1].Add(0);
            else if (query[0] == '!') operators[1].Add(1);
            else operators[1].Add(2);

            for (int i = 1; i < query.Length; i++)
            {
                bool changed = false;

                if (query[i] == '~')
                {
                    operators[0].Add(1);
                    changed = true;
                }

                if (query[i] == ' ')
                {
                    operators[0].Add(0);
                    changed = true;
                }

                if (changed)
                {
                    if (i + 1 == query.Length || query[i + 1] == ' ') continue;

                    if (query[i + 1] == '^') operators[1].Add(0);

                    else if (query[i + 1] == '!') operators[1].Add(1);

                    else operators[1].Add(2);
                }
            }
            return operators;
        }

        ///<summary> Recorre el corpus en busca de palabras semejantes </summary>///
        private static string Suggestion(string word, Corpus corpus)
        {
            string suggestion = "";

            if (!corpus.ITFs.ContainsKey(word))
            {
                foreach (var pair in corpus.ITFs)
                {
                    // if (LevenshteinDistance(word, pair.Key) <= word.Length/3)
                    if (LevenshteinDistance(word, pair.Key) <= 1)
                    {
                        suggestion = Compare(suggestion, pair.Key, word, corpus);
                    }
                }
            }

            return suggestion;
        }

        ///<summary> Entre dos palabras devuelve la mas relevante
        private static string Compare(string suggestion, string newword, string word, Corpus corpus)
        {
            if (suggestion == "") suggestion = newword;

            if (Similitud.TotalWeight(suggestion, corpus) < Similitud.TotalWeight(newword, corpus)) suggestion = newword;

            return suggestion;
        }

        ///<summary> Devuelve la distancia minima entre dos palabras
        private static int LevenshteinDistance(string s, string t)
        {
            int costo = 0;
            int m = s.Length;
            int n = t.Length;
            int[,] d = new int[m + 1, n + 1];

            if (n == 0) return m;
            if (m == 0) return n;

            for (int i = 0; i <= m; d[i, 0] = i++) ;
            for (int j = 0; j <= n; d[0, j] = j++) ;

            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    costo = (s[i - 1] == t[j - 1]) ? 0 : 1;
                    d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + costo);
                }
            }

            return d[m, n];
        }
    }
}