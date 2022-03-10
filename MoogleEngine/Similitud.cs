namespace MoogleEngine
{
    public class Similitud
    {
        public float[,]? similitud;
        public Similitud(Query query, Corpus corpus)
        {
            similitud = Cosin(query, corpus);
        }
        // Calcular IDF de cada palabra
        public static void IDF(Dictionary<int, Info> documents, Dictionary<string, float> IDFs) 
        {
            foreach (var pair in IDFs)
            {
                int count = 0;

                for (int i = 0; i < documents.Count; i++)
                {
                    if (documents[i].ContainsKey(pair.Key))
                    {
                        count++;
                    }
                }
                
                if (count == 0) return; // Retorna si la palabara no está en ningún documento
                IDFs[pair.Key] = (float)Math.Log((float)documents.Count/(count));
            }

            Similitud.Modulo(documents, IDFs); // Calclula el peso de cada palabra y el modulo de los documentos
        }
        
        // Llama a clacular el modulo de cada documento
        public static void Modulo(Dictionary<int, Info> documents, Dictionary<string, float> IDFs)
        {
            for (int i = 0; i < documents.Count; i++)
            {
                documents[i].Modulo(IDFs);
            }
        }

        // Guarda los indices de las palabras asociadas a operadores de inclusion y exclusion
        private static List<int>[] SeparateWords(List<int>[] operators)
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

        private static bool ContainsWord(Info info, List<int> index, List<string> words)
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
        private static float[,] Cosin(Query query, Corpus corpus)
        {
            float[,] similitud = new float[3, corpus.documents.Count];
            List<int>[] separatewords = SeparateWords(query.operators);
            float mq = query.info[0].modulo;
            float mr = query.info[1].modulo;
            float ms = query.info[2].modulo;

            for (int i = 0; i < similitud.GetLength(1); i++)
            {
                if (ContainsWord(corpus.documents[i], separatewords[1], query.list[0])) continue;
                if (separatewords[0].Count > 0 && !ContainsWord(corpus.documents[i], separatewords[0], query.list[0])) continue;

                float prodvectq = VectorialProduct(query, corpus.documents, i, 0);
                float prodvectr = VectorialProduct(query, corpus.documents, i, 1);
                float prodvects = VectorialProduct(query, corpus.documents, i, 2);
                float mi = corpus.documents[i].modulo;
                if (mi == 0) continue;
                if (mq != 0) similitud[0,i] = prodvectq / (mq * mi);
                if (mr != 0) similitud[1,i] = prodvectr / (float)(Math.E * mr * mi);
                if (ms != 0) similitud[2,i] = prodvects / (float)(Math.Pow(Math.E, 2) * ms * mi);
            }
            return SortedDocuments(AuxMethods.Closeness(Sum(similitud), query, corpus));
        }

        // Suma todos los elementos de cada columna de un array bidimensional y devuelve una sola fila de elementos
        private static float[] Sum(float[,] array)
        {
            float[] newarray = new float[array.GetLength(1)];
            for (int i = 0; i < newarray.Length; i++)
            {
                newarray[i] = array[0,i] + array[1,i] + array[2,i];
            }
            return newarray;
        }

        // Calcula el producto vectorial
        private static float VectorialProduct(Query query, Dictionary<int, Info> documents, int a, int j)
        {
            float prodvect = 0;
            for (int i = 0; i < query.list[j].Count; i++)
            {
                if (!documents[a].weigths.ContainsKey(query.list[j][i])) continue;
                prodvect += documents[a].weigths[query.list[j][i]]*query.info[j].weigths[query.list[j][i]];
            }
            return prodvect;
        }

        // Devuelve los documentos en orden de importancia
        private static float[,] SortedDocuments(float[] score)
        {
            float[] originalscores = new float[score.Length];
            for (int i = 0; i < score.Length; i++)
            {
                originalscores[i] = score[i];
            }

            // Ordenar el array de scores
            Array.Sort(score);
            Array.Reverse(score);

            // Este array deberá guardar las posiciones iniciales de los docuementos antes de ordenarlos
            int[] originalplaces = new int[score.Length];
            bool[] pased = new bool[score.Length];

            for (int i = 0; i < score.Length; i++)
            {
                int k = Index(originalscores, score[i]);
                while (pased[k])
                {
                    float[] scorei = new float[score.Length-k-1];

                    for (int j = 0; j < scorei.Length; j++)
                    {
                        scorei[j] = originalscores[k+j+1];
                    }

                    k += 1 + Index(scorei, score[i]);
                }   

                originalplaces[i] = k;
                pased[k] = true;                                                                 
            }

            return MixArrays(score, originalplaces);
        }        

        // Indice de un elemento en un array
        private static int Index(float[] array, float value)
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
        private static float[,] MixArrays(float[] a1, int[] a2)
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

        // En una lista de palabras seleciiona la mejor relativa al cuerpo de docuementos
        public static string BestWord(List<string> list, Corpus corpus)
        {
            if (list.Count == 0) return "";

            string bestword = "";

            for (int i = 0; i < list.Count; i++)
            {
                if (!corpus.IDFs.ContainsKey(list[i])) continue;
                bestword = TotalWeight(bestword, corpus) < TotalWeight(list[i], corpus)? list[i] : bestword;
            }

            return bestword;
        }
    }
}