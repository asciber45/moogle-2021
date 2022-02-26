namespace MoogleEngine
{
    public class Moogle
    {
        public static Corpus? corpus;
        /// <summary> Metodo que inicia el procesado de los documentos </summary> ///
        public static void Start(string adress)
        {
            corpus = new Corpus(adress); // Inicializa el corpus
            Similitud.IDF(corpus); // Halla el ITF de cada palabra
            Similitud.Modulo(corpus); // Calclula el peso de cada palabra y el modulo de los documentos
        } 

        /// <summary> Metodo que devuelve la busqueda deseada </summary> ///
        public static SearchResult Query(string query) 
        {
            return new SearchResult(query, corpus!);
        }
    }   
}