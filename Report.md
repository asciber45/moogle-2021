# Moogle!

> Alex Sierra Alcalá. Grupo C-111.

> Proyecto de Programación I. Facultad de Matemática y Computación.
> Universidad de La Habana. Curso 2021

## Sobre la Búsqueda

Moogle! es una aplicación cuyo propósito es buscar un texto en un conjunto de documentos. Para ello usamos el Modelo de Recuperación Vectorial, en el cual a través de las palabras presentes en cada documento y sus pesos (calculados con TermFrequency-InverseDocumentFrequency), logramos crear un vector por documento para aplicarle la Similitud del Coseno con el vector relativo a la consulta. De esta forma se obtiene un sistema de ranking donde el más alto será el documento más similar a la consulta hecha, es decir, el más recomendado de acuerdo a la búsqueda del usuario.

### Operadores

El proyecto cuenta con varios operadores para mejorar la búsqueda del usuario:

* Exclusión, identificado con con un `!` delante de una palabra, (e.j., `!algoritmo`) indica que algoritmo **no debe aparecer** en ningún documento devuelto.
* Inclusión, identificado con con un `^` delante de una palabra, (e.j., `^algoritmo`) indica que algoritmo **debe aparecer** en todos los documentos devueltos.
* Mayor Relevancia, identificado por varios `*` delante de una palabra, (e.j., `*computación`) con cada * la relevancia de la palabra para la query se hace el doble.
* Cercanía identificado con un `~` entre las palabras, sin usar la tecla de espacio entre ellas, (e.j., `M~N`) indica que los documentos que contengan una ventana del texto con M y N tendrán mayor score.

### Sugerencia

Si el usuario introduce una palabra cuya información no esté almacenada, el algoritmo ofrece una sugerencia para brindar una mayor exactitud en la búsqueda.

### Resultados de la Búsqueda

Una vez asignado un `Score` a cada documento, se muestran en pantalla una lista de los resultados con su nombre y el `Snippet` donde se encontró la palabra más relevante de la consulta en el documento.

## Ejecutando el proyecto

Debes colocar los documentos en los que quieres desarrollar la búsqueda, en la carpeta Content, en formato `.txt`.

Este proyecto está desarroyado para la versión objetivo de .NET Core 6.0. Para ejecutarlo solo te debes parar en la carpeta del proyecto y ejecutar en la terminal de Linux:

```bash
make dev
```

Si estás en Windows, debes poder hacer lo mismo desde la terminal del WSL (Windows Subsystem for Linux). Si no tienes WSL ni posibilidad de instalarlo, deberías considerar seriamente instalar Linux, pero si de todas formas te empeñas en desarrollar el proyecto en Windows, el comando *ultimate* para ejecutar la aplicación es (desde la carpeta raíz del proyecto):

```bash
dotnet watch run --project MoogleServer
```

## Implementación Moogle Engine

Estructura de la biblioteca de clases `MoogleEngine`.

### Procesamineto de las palabras del Corpus

Al iniciar el servidor, se llama al método `Start` de la clase `Moogle`, el cual se encarga de leer los documentos, y crear un objeto de la clase `Info` para cada documento de la carpeta Content:

* La clase `Corpus` se encarga de procesar los textos contenidos dentro del cuerpo de documentos, separar por espacios y eliminar los signos de puntuación. Contiene cuatro diccionarios: uno del vocabulario general contra el IDF de cada una de las palabras, uno para guardar las raíces, otro para los sinónimos y otro que nos da un acceso rápido a cada uno de los objetos `Info` asociados a cada uno de los documentos. En esta clase evaluamos además la posiblidad de haber analizado anteriormente el actual cuerpo de documentos con el método `Analysis`, de forma tal que, si después de comparar las últimas fechas de modificación y los nombres de los documentos con la información guardada, no hay cambios, solo es necesario cargar los datos y no realizar el análisis de los documentos. La información se guarda con el método `SaveDocInfo` que serializa los diccionarios del objeto Corpus.

* En la clase `Info` se almacena la información de cada una de las palabras por documento en los diccionarios `Weigths` y `Words` que tienen como valor  el peso de la palabra y una lista de índices con las posiciones de la palabra en el documento (estructurando los vectores documento). Está además el método `Serialize` que perimite serializar un objeto Info y un constructor para deserializar que recibe como parámetro un string con los datos de objetos serializados.

* Una vez terminado este proceso se calcula el TF-IDF de las palabras del corpus, mediante el método `Modulo` de la clase `Similitud`. Estos datos se guardan para próximos usos en la carpeta Cache.

### Procesamiento de la Query

Cuando el usuario introduce una nueva query se crea un objeto de la clase `Query` y en dicha clase se extraen las palabras de la query:

#### Operadores

 Se identifican los operadores de búsqueda mediante el método `Operators`. Para hacer esto se guarda en una `List<int>[]` los índices de las palabras sobre las cuales estarán actuando los operadores. Para el operador de cercanía se consideró la distancia entre AB como la mínima cantidad de palabras que podamos encontrar entre A y B en un documento específico. Para hallar la menor distancia entre dos palabras en un documento (se considera que si alguna de las palabras no está en el documento, la distancia entre las palabras es el número de la longitud del vocabualario del documento) se toman las listas de índices de apariciones de ambas palabras en dicho documento se busca la menor diferencia de forma lineal mediante el método `WordDistance`:

```cs
private static int WordDistance(int i, int j, Query query, Corpus corpus)
{
    // Se cargan las dos listas de índices
    List<int> a = corpus.documents[j][query.list[0][i]];
    List<int> b = corpus.documents[j][query.list[0][i+1]];
    int x = 0; int y = 0;
    int min = Math.Abs(a[0]-b[0]);

    while (x + y < a.Count + b.Count -2) // Mientras queden elementos en alguna de las listas
    {
        if (y == b.Count-1 || (x < a.Count-1 && a[x] < b[y])) x++; // Tomamos un elemento de la lista a
        else y++; //Tomamos un elemento de la lista b
        min = Math.Min(min, Math.Abs(a[x] - b[y])); // Nos quedamos con la menor diferencia   
    }

    return min;
}
```

#### Sugerencias

 Una vez identificados los operadores se procede a comprobar la existencia de las palabras de la query en el `Corpus`, en caso contrario, se llama al método `AddSuggestion` donde se combina la `Distancia de Levensthein` con el peso de las palabras del `Corpus` y se construye la nueva query donde está incluida la palabra sugerida. En caso de no encontrar sugerencias, la palabra se obvia por completo.

#### Raíces y Sinónimos

Con el objetivo de ser más abarcador al dar los resultados de la búsqueda al usuario se identifican las palabras que posean las mismas raíces que las de la query, mediante la clase `Stemming Algorithm` que se encarga de realizar el stemming en español a través de una serie de pasos q eliminan sufijos y prefijos para obtener un lexema aproximado de la palabra. Para ello se crea un diccionario que guarda en sus llaves los lexemas encontrados y como valor la lista de palabras que tienen el mismo lexema. Este diccionario se guarda, pero sus datos nunca son eliminados de forma tal que con cada cuerpo de documentos analizados se engorda la información que en él se almacena. También se hace uso del diccionario synonymous de la clase `Corpus`, el cual contiene un diccionario de sinónimos para el español. Para cada palabra se añaden dos palabras más al Query: entre las que tienen la misma raíz o las que tienen el mismo sinónimo, las de mayor relevancia para el cuerpo de documentos, mediante los métodos `AddRoot` y `AddSynonymus`.

Luego se procede a calcular el TF-IDF de las palabras de la query.

### Resultados de la Búsqueda

Se crea un objeto `Similitud` que recibe en su constructor un objeto `Query` y un objeto `Corpus`:

#### Cómputo del Ranking

* LLegado este punto, tenemos tres listas de palabras sobre las cuales queremos encontrar ocurrencias en el cuerpo de documentos: la lista de palabras originales del queryo sus sugerencias, la lista de palabras con la misma raíz que las palabras del query, y la lista de sinónimos asociada al query. En el método `Cosin` de la clase `Similitud`, para cada documento y usando los datos guardados en el objeto `Info` relativo a él, se calcula el producto vectorial del vector documento con el vector de cada una de las tres consultas y se normaliza con el módulo de estos vectores, además se hace de forma tal que las palabras con igual raíz tengan menos peso que las originales y que los sinónimos tengan aún menos relevancia que estas primeras. Esto devuelve un `float[,]` de tres filas donde la columna `i` contiene la similitud del i-ésimo documento con cada una de las consultas, estas puntuaciones se suman con el metodo `Sum`, obteniéndose un `float[]` con las puntuaciones de cada uno de los documentos. En este método realizan su función los operadores de inclusión y exclusión, a través de los cuales al inicio del proceso se decanta si un documento cumple con las condiciones requeridas usando los métodos `SeparatedWords` y `ContainsWord`, cuyo papel no es más que evaluar si una palabra afectada por un operador está o no en el documento en cuestión.

* Luego se procede a modificar las puntuaciones en correspondencia con el operador de cercanía. Para esto, se toman los pares de palabras sobre las cuales se aplica el operador, se halla su distancia en el documento y se divide el `Score` entre la distancia hallada. Para esto se emplea la clase `AuxMethods` donde está el método `Closeness` que calcula esta distancia.

* Luego en el método `SortedDocuments` se ordenan los `Score` en orden descendiente. Este método devuelve un `float[,]` de dos filas tal que en la posición [0,i] está la puntuación del i-ésimo documento después de ordenarlos y en la posición [1,i] está el índice que teniá este documento antes de ser ordenado.

#### Snippet

Una vez asignado para cada documento su `Score`, se procede a tomar un `Snippet`:

* Se construye el `Snippet` de cada uno de los documentos a devolver mediante el método `FindSnippet` de la clase `AuxMethods`.

* Para ello obtenemos primero, de las palabra que están en el query y el documento, la que mayor relevancia tiene para el corpus. Esto se hace a través del método `BestSnippet`. 

* Después de seleccionar la palabra se llama al método `Take10Words`, el cual carga nuevamente el documento en forma de string y se devuelve una ventana random de 10 palabras en la que aparezca la palabra seleccionada. 