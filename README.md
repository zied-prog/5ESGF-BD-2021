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
##### 5 - Parsing du sudoku en string to grid 
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

limitRows :1000
coreCount :2
nodeCount :2
ElapsedMilliseconds : 191781

limitRows :1000
coreCount :1
nodeCount :1
ElapsedMilliseconds : 233632

limitRows :1000
coreCount :4
nodeCount :4
ElapsedMilliseconds : 173687

limitRows :10000
coreCount :2
nodeCount :2
ElapsedMilliseconds : 913071
