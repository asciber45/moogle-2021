namespace MoogleEngine
{
    public static class Stemming
    {
        public static string Stemmer(this string word)
        {
            word = Preffixes(word);
            char[] charword = Step0(word.ToCharArray(), RV(word));
            charword = Step1(charword);
            charword = Step3(charword);
            return ToString(charword);
        }

        static string Preffixes(string word)
        {
            List<string> preffixes = new List<string>{"extra", "infra", "inter", "omni", "retro", "sobre", "sub", "supra",
            "tele", "ultra", "macro", "micro", "mono", "multi", "nano", "pluri", "poli", "tri", "archi", "cuasi", "hiper",
            "hipo", "semi", "super", "vice", "anti", "contra", "em", "post", "cubre", "fusi", "en"};

            for (int i = 0; i < preffixes.Count; i++)
            {
                if (word.Length <= preffixes[i].Length + 4) continue;
                if (word.StartsWith(preffixes[i])) word = word.Substring(preffixes[i].Length);
            }

            if (word.Length <= 3) return word;

            if (word.Substring(0, 2) == "rr") word = word.Substring(1);

            return word;
        }

        static string ToString(char[] word)
        {
            string a = "";
            for (int i = 0; i < word.Length; i++)
            {
                a += word[i];
            }
            return a;
        }

        static char[] Step0(char[] word, char[] rv)
        {
            string[] suffixes = {"melo", "me", "se", "sela", "selo", "selas", "mela", "melos", "melas", "telo", "tela", "telos",
    "selos", "la", "le", "te", "lo", "las", "les", "los", "nos", "nosla", "noslo", "noslas", "noslos", "telas", "tenos"};
            suffixes = suffixes.OrderBy(t => t.Length).ToArray();

            for (int i = suffixes.Length - 1; i > -1; i--)
            {
                if (rv.Length < suffixes[i].Length) continue;
                if (EndOfWord(rv, suffixes[i]))
                {
                    rv = Delete(rv, suffixes[i]);
                    string[] list_a = { "éte", "éndo", "iéndo", "ína", "ándo", "ár", "ér", "ír" };
                    string[] list_b = { "ando", "iendo", "ar", "er", "ir" };
                    string list_c = "yendo";

                    for (int j = 0; j < list_a.Length; j++)
                    {
                        if (EndOfWord(rv, list_a[j]))
                        {
                            word = Delete(word, suffixes[i]);
                            ReplaceAccentVowels(word);
                            return word;
                        }
                    }

                    for (int j = 0; j < list_b.Length; j++)
                    {
                        if (EndOfWord(rv, list_b[j]))
                        {
                            return Delete(word, suffixes[i]);
                        }
                    }

                    if (EndOfWord(rv, list_c) && word[word.Length - list_c.Length - suffixes[i].Length - 1] == 'u')
                    {
                        return Delete(word, suffixes[i]);
                    }
                }
            }

            return word;
        }

        static char[] Step1(char[] word)
        {
            List<string>[] suffixes = new List<string>[12];
            suffixes[0] = new List<string>{"anza", "anzas", "ico", "ica", "icos", "icas", "ismo", "ismos", "able", "ables", "eta",
            "etas", "ible", "ibles", "ista", "istas", "oso", "osa", "ita", "ito", "itas", "itos", "cionarias", "ual", "uales",
            "osos", "osas", "amiento", "amientos", "imiento", "imientos", "cionario", "cionaria", "cionarios", "ario", "aria",
            "arios", "arias", "al", "ales", "eral", "erales", "icia", "icias", "eza", "ezas", "ierto", "ierta", "iertos", "iertas",
            "sición", "siciones", "ernal", "ernales", "ierno", "erior", "eriores", "ísima", "ísimo", "ísimas", "ísimos",
            "adura", "aduras", "grafía", "grafías", "ero", "era", "eros", "eras"};
            suffixes[1] = new List<string>{"adora", "ador", "ación", "adoras", "adores", "aciones", "or", "ora", "ores", "ización", "izaciones",
            "ante", "antes", "ancia", "ancias", "ada", "adas", "ado", "ados", "adería", "aderías", "itor", "itora", "itores", "sitor", "sitora",
            "sitores", "erío", "ería", "eríos", "erías"};
            suffixes[2] = new List<string> { "logía", "logías" };
            suffixes[3] = new List<string> { "ución", "uciones" };
            suffixes[4] = new List<string> { "encia", "encias" };
            suffixes[5] = new List<string> { "amente" };
            suffixes[6] = new List<string> { "mente" };
            suffixes[7] = new List<string> { "idad", "idades" };
            suffixes[8] = new List<string> { "iva", "ivo", "ivas", "ivos" };
            suffixes[9] = new List<string> { "era", "ero", "eras", "eros" };
            suffixes[10] = new List<string> { "cción", "cciones" };
            suffixes[11] = new List<string> { "ición", "iciones" };

            for (int i = 0; i < suffixes.Length; i++)
            {
                suffixes[i] = suffixes[i].OrderBy(t => t.Length).ToList();

                if (i == 5)
                {
                    if (word.Length - suffixes[i][0].Length <= 2) continue;
                    if (word.Length <= suffixes[i][0].Length) continue;
                    if (EndOfWord(word, suffixes[i][0]))
                    {
                        word = Delete(word, suffixes[i][0]);
                        string a = ToString(word).Substring(word.Length - 2);

                        if (a == "iv" || a == "os" || a == "ic" || a == "ad")
                        {
                            word = Delete(word, a);
                            if (a == "iv" && EndOfWord(word, "at")) word = Delete(word, "at");
                        }

                        word = Replace(word, "", "a");
                        i = -1;
                        continue;
                    }
                }

                for (int j = suffixes[i].Count - 1; j > -1; j--)
                {
                    if (word.Length - suffixes[i][j].Length <= 2) continue;
                    if (EndOfWord(word, suffixes[i][j]))
                    {
                        if (i == 0)
                        {
                            word = Delete(word, suffixes[i][j]);
                            if (EndOfWord(word, "gu")) word = Delete(word, "u");
                            if (EndOfWord(word, "qu")) word = Replace(word, "qu", "c");
                            if (EndOfWord(word, "ur")) word = Replace(word, "ur", "or");
                            return word;
                        }

                        if (i == 1)
                        {
                            word = Delete(word, suffixes[i][j]);
                            if (EndOfWord(word, "ic")) word = Delete(word, "ic");
                            if (EndOfWord(word, "sc")) word = Replace(word, "sc", "z");
                            return word;
                        }

                        if (i == 2 || i == 3 || i == 4 || i == 10 || i == 11)
                        {
                            string replaced = "";

                            switch (i)
                            {
                                case 2:
                                    replaced = "log";
                                    break;
                                case 3:
                                    replaced = "u";
                                    break;
                                case 4:
                                    replaced = "ente";
                                    break;
                                case 10:
                                    replaced = "ct";
                                    break;
                                case 11:
                                    replaced = "i";
                                    break;
                                default:
                                    break;
                            }

                            return Replace(word, suffixes[i][j], replaced);
                        }

                        if (i == 6)
                        {
                            if (word.Length - 2 <= suffixes[i][j].Length) continue;
                            word = Delete(word, suffixes[i][j]);

                            if (EndOfWord(word, "ante")) word = Delete(word, "ante");
                            if (EndOfWord(word, "able")) word = Delete(word, "able");
                            if (EndOfWord(word, "ible")) word = Delete(word, "ible");
                            j = -1;
                            i = -1;
                            continue;
                        }

                        if (i == 7)
                        {
                            word = Delete(word, suffixes[i][j]);

                            if (EndOfWord(word, "abil")) word = Delete(word, "abil");
                            if (EndOfWord(word, "ic")) word = Delete(word, "ic");
                            if (EndOfWord(word, "iv")) word = Delete(word, "iv");
                            i = -1;
                            j = -1;
                            continue;
                        }

                        if (i == 8)
                        {
                            word = Delete(word, suffixes[i][j]);

                            if (EndOfWord(word, "at")) word = Delete(word, "at");
                            if (EndOfWord(word, "ct")) word = Replace(word, "ct", "t");
                            return word;
                        }

                        if (i == 9)
                        {
                            word = Delete(word, suffixes[i][j]);
                            if (EndOfWord(word, "ec")) word = Replace(word, "c", "z");
                            if (EndOfWord(word, "et")) word = Delete(word, "et");
                            return word;
                        }
                    }
                }
            }

            return Step2a(word);
        }

        static char[] Step2a(char[] word)
        {
            List<string> list = new List<string> { "ya", "ye", "yan", "yen", "yeron", "yendo", "yo", "yó", "yas", "yes", "yais", "yamos" };
            list = list.OrderBy(t => t.Length).ToList();

            for (int i = list.Count - 1; i > -1; i--)
            {
                if (word.Length < list[i].Length) continue;
                if (EndOfWord(word, list[i]) && EndOfWord(Delete(word, list[i]), "u")) return Delete(word, list[i]);
            }

            return Step2b(word);
        }

        static char[] Step2b(char[] word)
        {
            List<string> list_a = new List<string> { "en", "es", "éis", "emos" }.OrderBy(t => t.Length).ToList();
            List<string> list_b = new List<string>{"arían", "arías", "arán", "arás", "aríais", "aría", "aréis", "aríamos",
            "aremos", "ará", "aré", "erían", "eríaserán", "erás", "eríais", "ería", "eréis", "eríamos", "eremos", "erá", "eré",
            "irían", "irías", "irán", "irás", "iríais", "iría", "iréis", "iríamos", "iremos", "irá ", "iré", "aba", "ada", "ida",
            "ía", "ara", "iera", "ad", "ed", "id", "ase", "iese", "aran", "ieran", "asen", "iesen", "aron", "ieron", "ado",
            "ido", "ando", "iendo", "ió", "ar", "er", "ir", "as", "abas", "adas", "idas", "ías", "aras", "ieras", "ases",
            "ís", "áis", "abais", "íais", "arais", "ierais", "aseis", "ieseis", "asteis", "isteis", "ados", "dos", "amos",
            "ábamos", "íamos", "imos", "áramos", "iéramos", "iésemos", "ieses", "ásemos", "izar", "ner"}.OrderBy(t => t.Length).ToList();

            for (int i = list_a.Count - 1; i > -1; i--)
            {
                if (word.Length < list_a[i].Length) continue;
                if (EndOfWord(word, list_a[i]))
                {
                    word = Delete(word, list_a[i]);
                    if (EndOfWord(word, "gu")) word = Delete(word, "u");
                    if (EndOfWord(word, "c")) word = Replace(word, "c", "z");
                    return word;
                }
            }

            for (int i = list_b.Count - 1; i > -1; i--)
            {
                if (word.Length <= list_b[i].Length) continue;
                if (EndOfWord(word, list_b[i])) return Delete(word, list_b[i]);
            }

            return word;
        }

        static char[] Step3(char[] word)
        {
            List<string> list_a = new List<string> { "os", "a", "o", "á", "í", "ó" }.OrderBy(t => t.Length).ToList();
            List<string> list_b = new List<string> { "e", "é" }.OrderBy(t => t.Length).ToList();

            for (int i = 0; i < list_a.Count; i++)
            {
                if (EndOfWord(word, list_a[i])) word = Delete(word, list_a[i]);
            }

            for (int i = 0; i < list_b.Count; i++)
            {
                if (EndOfWord(word, list_b[i]))
                {
                    word = Delete(word, list_a[i]);
                    if (EndOfWord(word, "gu")) word = Replace(word, "gu", "u");
                }
            }

            ReplaceAccentVowels(word);
            return word;
        }

        static char[] Replace(char[] word, string s1, string s2)
        {
            char[] a = new char[word.Length - s1.Length + s2.Length];

            for (int i = 0; i < a.Length; i++)
            {
                if (i < a.Length - s2.Length) a[i] = word[i];
                else a[i] = s2[i - word.Length + s1.Length];
            }

            return a;
        }

        static char[] Delete(char[] word1, string word2)
        {
            char[] newword = new char[word1.Length - word2.Length];

            for (int i = 0; i < word1.Length - word2.Length; i++)
            {
                newword[i] = word1[i];
            }

            return newword;
        }

        static bool EndOfWord(char[] word, string suffixe)
        {
            if (word.Length < suffixe.Length) return false;
            if (ToString(word).Substring(word.Length - suffixe.Length) == suffixe) return true;
            return false;
        }

        static void ReplaceAccentVowels(char[] word)
        {
            for (int i = 0; i < word.Length; i++)
            {
                switch (word[i])
                {
                    case 'á':
                        word[i] = 'a';
                        break;
                    case 'é':
                        word[i] = 'e';
                        break;
                    case 'í':
                        word[i] = 'i';
                        break;
                    case 'ó':
                        word[i] = 'o';
                        break;
                    case 'ú':
                        word[i] = 'u';
                        break;
                    default:
                        break;
                }
            }

            return;
        }

        static bool IsVowel(char c)
        {
            char[] vowels = { 'a', 'e', 'i', 'o', 'u', 'á', 'é', 'í', 'ó', 'ú', 'ü' };

            foreach (char vowel in vowels)
            {
                if (vowel == c) return true;
            }

            return false;
        }

        static char[] RV(string word)
        {
            if (word.Length <= 3) return word[word.Length - 1].ToString().ToCharArray();
            if (!IsVowel(word[1]))
            {
                int count = 2;
                while (count < word.Length && !IsVowel(word[count])) count++;
                if (count == word.Length) return word.ToCharArray();
                return word.Substring(count).ToCharArray();
            }

            else if (IsVowel(word[0]) && IsVowel(word[1]))
            {
                for (int i = 2; i < word.Length; i++)
                {
                    if (!IsVowel(word[i])) return word.Substring(i + 1).ToCharArray();
                }
            }

            else
            {
                return word.Substring(3).ToCharArray();
            }

            return "".ToCharArray();
        }
    }
}
