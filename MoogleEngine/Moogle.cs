namespace MoogleEngine
{
    public class Moogle
    {
        public static Corpus? corpus;
        /// <summary> Metodo que inicia el procesado de los documentos </summary> ///
        public static void Start()
        {
            corpus = new Corpus(@"..//Content"); // Inicializa el corpus
        } 

        /// <summary> Metodo que devuelve la busqueda deseada </summary> ///
        public static SearchResult Query(string query) 
        {
            return new SearchResult(query, corpus!);
        }
    }   
}