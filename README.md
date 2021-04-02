# Projet C#

## Résolution de sudokus avec SWARM


### Introduction

Nous nous sommes inspiré du repo https://github.com/MyIntelligenceAgency/ECE-2021-FIN-E-Ing4-Finance-Gr02-IA1/tree/main/Sudoku.SwarmInt pour intégrer le code de résolution de sudoku avec le solveur SWARM


####  Adaptation pour prise en compte d'un fichier csv
##### 1 - Recuperation des arguments
```c#
            var limitRows = int.Parse(args[0]);
            var coreCount = args[1];
            var nodeCount = args[2];
```

##### 2 - Creation et configuration de la session spark
```c#
           SparkSession spark = SparkSession
                .Builder()
                .AppName("Sentiment Analysis using .NET for Apache Spark")
                .Config("spark.executor.cores", coreCount)
                .Config("spark.executor.instances", nodeCount)
                .GetOrCreate();
```

##### 3 - Lecture de fichier csv avec un limit de nombre de ligne (limitRows)
```c#      
    DataFrame df = spark.Read().Option("header", true).Option("inferSchema", true)
                .Csv("C:\\Users\\abdel\\Desktop\\ESGF\\5ESGF-BD-2021\\ESGF.Sudoku.Spark.RecursiveSearch\\sudoku.csv");
            var limitedDf = df.Limit(limitRows);
```
##### 4 - Enregistrement de la methode de solving dans spark
Rq: on a jouté le caractere 'a' au sudoku pour qu'elle soit interpreté comme string  
```c# 
        spark.Udf().Register<string, string>("MLudf", (text) => Solve(text.Trim(new Char[] { ' ', '"', 'a' })));
```
##### 5 - Parsing du sudoku en strin to grid 
```c# 
public static int[,] TextToGrid(string sudokuText)
        {
            var initial_grid = new int[9, 9];//matrice à 2 dimension;
            var colindex = 0;//Variable de boucle de colonne;
            var rowindex = 0;//Variable de boucle de ligne；
            foreach (var c in sudokuText)
            {
                if (colindex >= 9)
                {
                    colindex = 0;
                    rowindex++;
                }
                if (rowindex >= 9)
                {
                    rowindex = 0;
                }
                initial_grid[rowindex, colindex] = int.Parse(c.ToString());
                colindex++;
            }
            return initial_grid;
        }
```


##### 6 - Methode de solving 
```c# 
  public static string Solve(string sudokuText)
        {
            const int numOrganisms = 200; 
            const int maxEpochs = 10000;
            const int maxRestarts = 150;
            var solver = new SudokuSolver();
            var solvedSudoku = solver.Solve(Sudoku.New(TextToGrid(sudokuText)), numOrganisms, maxEpochs, maxRestarts);
            return solvedSudoku.ToString();
        }
```

#### Build et publication du projet 
    dotnet publish -f netcoreapp3.1

#### Execution du program avec un jeux de parametre different (limitRows,coreCount,nodeCount ) et comparaison des resultats  
    spark-submit --class org.apache.spark.deploy.dotnet.DotnetRunner --master local microsoft-spark-3-0_2.12-1.1.1.jar ESGF.Sudoku.Spark.Swarm.exe [limitRows] [coreCount] [nodeCount]

#### RESULTATS
--------------------+--------------------+
|             quizzes|      MLudf(quizzes)|
+--------------------+--------------------+
|00430020900500900...| 8 6 4  3 7 1  2 ...|
|04010005010700396...| 3 4 6  1 7 9  2 ...|
|60012038400845907...| 6 9 5  1 2 7  3 ...|
|49720000010040000...| 4 9 7  2 5 8  3 ...|
|00591030800940306...| 4 6 5  9 1 2  3 ...|
|10000500738090000...| 1 9 4  6 8 5  2 ...|
|00906543000700080...| 2 8 9  7 6 5  4 ...|
|00000065770240010...| 8 9 4  2 3 1  6 ...|
|50307019000000675...| 5 6 3  4 7 2  1 ...|
|06072090808400300...| 1 6 3  7 2 5  9 ...|
|00408300205100430...| 9 7 4  1 8 3  6 ...|
|00006028070900100...| 4 3 1  5 6 7  2 ...|
|00430000089020067...| 2 5 4  3 6 7  8 ...|
|00807010012009005...| 9 5 8  2 7 4  1 ...|
|06537000200000137...| 8 6 5  3 7 9  4 ...|
|00571032900036280...| 8 6 5  7 1 4  3 ...|
|20000530000007385...| 2 6 8  4 9 5  3 ...|
|04080050008076009...| 9 4 7  8 1 2  5 ...|
|05008301700010040...| 6 5 2  4 8 3  9 ...|
|70008400530070102...| 7 1 2  9 8 4  3 ...|
+--------------------+--------------------+
only showing top 20 rows

limitRows :1000
coreCount :2
nodeCount :2
ElapsedMilliseconds : 191781

|             quizzes|      MLudf(quizzes)|
+--------------------+--------------------+
|00430020900500900...| 8 6 4  3 7 1  2 ...|
|04010005010700396...| 3 4 6  1 7 9  2 ...|
|60012038400845907...| 6 9 5  1 2 7  3 ...|
|49720000010040000...| 4 9 7  2 5 8  3 ...|
|00591030800940306...| 4 6 5  9 1 2  3 ...|
|10000500738090000...| 1 9 4  6 8 5  2 ...|
|00906543000700080...| 2 8 9  7 6 5  4 ...|
|00000065770240010...| 8 9 4  2 3 1  6 ...|
|50307019000000675...| 5 6 3  4 7 2  1 ...|
|06072090808400300...| 1 6 3  7 2 5  9 ...|
|00408300205100430...| 9 7 4  1 8 3  6 ...|
|00006028070900100...| 4 3 1  5 6 7  2 ...|
|00430000089020067...| 2 5 4  3 6 7  8 ...|
|00807010012009005...| 9 5 8  2 7 4  1 ...|
|06537000200000137...| 8 6 5  3 7 9  4 ...|
|00571032900036280...| 8 6 5  7 1 4  3 ...|
|20000530000007385...| 2 6 8  4 9 5  3 ...|
|04080050008076009...| 9 4 7  8 1 2  5 ...|
|05008301700010040...| 6 5 2  4 8 3  9 ...|
|70008400530070102...| 7 1 2  9 8 4  3 ...|
+--------------------+--------------------+
only showing top 20 rows

limitRows :1000
coreCount :1
nodeCount :1
ElapsedMilliseconds : 233632
