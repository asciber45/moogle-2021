# Moogle!

> Alex Sierra Alcalá. Grupo C-111.

> Proyecto de Programación I. Facultad de Matemática y Computación.
> Universidad de La Habana. Curso 2021

## Sobre la Búsqueda

Moogle! es una aplicación cuyo propósito es buscar un texto en un conjunto de documentos. Para ello usamos el Modelo de Recuperación Vectorial, en el cual a través de las palabras que forman cada documento y sus pesos (calculados con TF-IDF), logramos crear un vector por documento para aplicarle la Similitud del Coseno con el vector relativo a la consulta. De esta forma se obtiene un sistema de ranking donde el más alto será el documento más similar a la consulta hecha, es decir, el más recomendado de acuerdo a la búsqueda del usuario.

### Operadores

El proyecto cuenta con varios operadores para mejorar la búsqueda del usuario:

* Exclusión, identificado con con un `!` delante de una palabra, (e.j., `!algoritmo`) indica que computación **no debe aparecer** en ningún documento devuelto.
* Inclusión, identificado con con un `^` delante de una palabra, (e.j., `^algoritmo`) indica que computación **debe aparecer** en todos los documentos devueltos.
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

Estructura de la biblioteca de clases MoogleEngine.

### Procesamineto de las palabras del Corpus

Al iniciar el servidor, se llama al método `Start` de la clase `Moogle`, el cual se encarga de leer los documentos, y crear un objeto de la clase `Info` para cada documento de la carpeta Content:

* La clase `Corpus` se encarga de procesar los textos contenidos dentro del cuerpo de documentos, separar por espacios y eliminar los signos de puntuación. Contiene cuatro diccionarios: uno del vocabulario total contra el IDF de cada una de las palabras, uno para guardar las raíces, otro para los sinónimos y otro q nos da un acceso rápido a cada uno de los objetos Info asociados a cada uno de los documentos.

* En la clase `Info` se almacena la información de cada una de las palabras por documento en los diccionarios `Weigths` y `Words` que tienen como valor  el peso de la palabra y una lista de índices con las posiciones de la palabra en el documento (estructurando los vectores documento).

* Una vez terminado este proceso se calcula el TF-IDF de las palabras del corpus, mediante el método `Modulo` de la clase `Similitud`.

### Procesamiento de la Query

Cuando el usuario introduce una nueva query se crea un objeto de la clase `Query` y en dicha clase se extraen las palabras de la query:

* Se identifican los operadores de búsqueda mediante el método `Operators`. Para el operador de cercanía se consideró la distancia entre AB como la mínima cantidad de palabras que podamos encontrar entre A y B en un documento específico.

* Una vez identificados los operadores se procede a comprobar la existencia de las palabras de la query en el `Corpus`, en caso contrario, se llama al método `AddSuggestion` donde se combina la `Distancia de Levensthein` con el peso de las palabras del `Corpus` y se construye la nueva query donde está incluida la palabra sugerida.

* Con el objetivo de ser más abarcador al dar los resultados de la búsqueda al usuario se identifican las palabras que posean las mismas raíces o el mismo significado que las de la query, mediante la clase `Stemming Algorithm` que se encarga de realizar el stemming en español (se recomienda eliminar el contenido de la carpeta Cache que contiene los datos de las palabras y sus familias si se incorporan nuevos documentos para procesar la información nueva en el Corpus) y el diccionario synonymous de la clase `Corpus`, la cual contiene un diccionario de sinónimos para el español.

* Se procede a calcular el TF-IDF de las palabras de la query.

### Resultados de la Búsqueda

Se crea un objeto `Similitud` que recibe en su constructor un objeto `Query` y un objeto `Corpus`:

* Se comparan los datos de los vectores documento almacenados en el objeto `Info` relativo a cada uno de ellos, con el vector consulta mediante el método Cosin de la clase `Similitud` y se calcula el `Score` de cada documento teniendo en cuenta la influencia de los operadores mediante el método `SeparatedWords`.

* Para tener en cuenta las condiciones del operador de cercanía, se emplea la clase `AuxMethods` donde está el método `Closeness` que devuelve la mínima distancia entre una lista de índices de las palabras en un determinado documento.

* Se construye el `Snippet` de cada uno de los documentos a devolver mediante el método `FindSnippet` de la clase `AuxMethods`. Se define un tamaño máximo de 10 palabras para la línea, luego entre las palabras del query que están en el documento, se selcciona la que más peso tenga y se devuelve una línea random en la que aparezca. 