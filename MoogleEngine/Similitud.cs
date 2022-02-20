namespace MoogleEngine
{
    public class Similitud
    {
        public float[,]? similitud;
        public Similitud(Query query, Corpus corpus)
        {
            similitud = Cosin(query, corpus);
        }
        // Calcular ITF de cada palabra
        public static void ITF(Corpus corpus) 
        {
            foreach (var pair in corpus.ITFs)
            {
                int count = 0;

                for (int i = 0; i < corpus.documents.Count; i++)
                {
                    if (corpus.documents[i].ContainsKey(pair.Key))
                    {
                        count++;
                    }
                }
                
                corpus.ITFs[pair.Key] = (float)Math.Log((float)corpus.documents.Count/(count+1));
            }
        }
        
        // Llama a clacular el modulo de cada documento
        public static void Modulo(Corpus corpus)
        {
            for (int i = 0; i < corpus.documents.Count; i++)
            {
                corpus.documents[i].Modulo(corpus.ITFs);
            }
        }

        public static List<int>[] SeparateWords(List<int>[] operators)
        {
            List<int>[] a = new List<int>[2];
            a[0] = new List<int>();
            a[1] = new List<int>();

            for (int i = 0; i < operators[1].Count; i++)
            {
                if (operators[1][i] == 0)
                {
                    a[0].Add(i);
                }

                else if (operators[1][i] == 1)
                {
                    a[1].Add(i);
                }

                else
                {
                    continue;
                }
            }

            return a;
        }

        public static bool ContainsWord(Info info, List<int> index, List<string> words)
        {
            foreach (int number in index)
            {
                if (info.ContainsKey(words[number]))
                {
                    return true;
                }
            }
            return false;
        }

        // Calculo de array similitud
        public static float[,] Cosin(Query query, Corpus corpus)
        {
            float[,] similitud = new float[2, corpus.documents.Count];
            List<int>[] separatewords = SeparateWords(query.operators);
            float mq = query.info[0].modulo;
            float mr = query.info[1].modulo;

            for (int i = 25; i < similitud.GetLength(1); i++)
            {
                if (ContainsWord(corpus.documents[i], separatewords[1], query.list[0])) continue;
                if (separatewords[0].Count > 0 && !ContainsWord(corpus.documents[i], separatewords[0], query.list[0])) continue;

                float prodvectq = VectorialProduct(query, corpus.documents, i, 0);
                float prodvectr = VectorialProduct(query, corpus.documents, i, 1);
                float mi = corpus.documents[i].modulo;
                if (mi == 0) continue;
                if (mq != 0) similitud[0,i] = prodvectq / (mq * mi);
                if (mr != 0) similitud[1,i] = prodvectr / (float)3*(mr * mi);
            }
            return SortedDocuments(AuxMethods.Closeness(Prom(similitud), query, corpus));
        }

        public static float[] Prom(float[,] array)
        {
            float[] newarray = new float[array.GetLength(1)];
            for (int i = 0; i < newarray.Length; i++)
            {
                newarray[i] = array[0,i] + array[1,i];
            }
            return newarray;
        }

        // Producto vectorial
        private static float VectorialProduct(Query query, Dictionary<int, Info> documents, int a, int j)
        {
            float prodvect = 0;
            for (int i = 0; i < query.list[0].Count; i++)
            {
                if (!documents[a].weigths.ContainsKey(query.list[j][i])) continue;
                prodvect += documents[a].weigths[query.list[j][i]]*query.info[j].weigths[query.list[j][i]];
            }
            return prodvect;
        }

        // Devuelve los documentos en orden de importancia
        public static float[,] SortedDocuments(float[] score)
        {
            float[] originalscores = new float[score.Length];
            for (int i = 0; i < score.Length; i++)
            {
                originalscores[i] = score[i];
            }
            Array.Sort(score);
            Array.Reverse(score);

            int[] originalplaces = new int[score.Length];
            int[] pased = new int[score.Length];

            for (int i = 0; i < score.Length; i++)
            {
                int k = Index(originalscores, score[i]);
                while (pased[k] == 1)
                {
                    float[] scorei = new float[score.Length-k-1];

                    for (int j = 0; j < scorei.Length; j++)
                    {
                        scorei[j] = originalscores[k+j+1];
                    }

                    k += 1 + Index(scorei, score[i]);
                }   

                originalplaces[i] = k;
                pased[k] = 1;                                                                 
            }

            return MixArrays(score, originalplaces);
        }        

        // Indice de un elemento en un array
        public static int Index(float[] array, float value)
        {
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i] == value)
                {
                    return i;
                }
            }
            return 1;
        }

        // Une dos filas
        public static float[,] MixArrays(float[] a1, int[] a2)
        {
            float[,] mixarrays = new float[2, a1.Length];

            for (int i = 0; i < a1.Length; i++)
            {
                mixarrays[0,i] = a1[i];
                mixarrays[1,i] = a2[i];
            }

            return mixarrays;
        }

        // Calcula el peso total de una palabra entre todos los docs
        public static float TotalWeight(string word, Corpus corpus)
        {
            float totalweight = 0;

            for (int i = 0; i < corpus.documents.Count; i++)
            {
                totalweight += (corpus.documents[i].weigths.ContainsKey(word))? corpus.documents[i].weigths[word] : 0;
            }

            return totalweight;
        }

        public static string BestWord(List<string> list, string word, Corpus corpus)
        {
            if (list.Count == 0) return "";

            string bestword = "";

            for (int i = 0; i < list.Count; i++)
            {
                bestword = TotalWeight(bestword, corpus) < TotalWeight(list[i], corpus)? list[i] : bestword;
            }

            return bestword;
        }
    }
}