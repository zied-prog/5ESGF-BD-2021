using Microsoft.Spark.Sql;
using Sudoku.Core;
using System;
using System.Diagnostics;
using System.Globalization;

namespace ESGF.Sudoku.Spark.Swarm
{
    class Program
    {
        private static void Main(string[] args)
        {

            var limitRows = int.Parse(args[0]);
            var coreCount = args[1];
            var nodeCount = args[2];

            Console.WriteLine("Begin solving Sudoku using combinatorial evolution");
            SparkSession spark = SparkSession
                .Builder()
                .AppName("Sentiment Analysis using .NET for Apache Spark")
                .Config("spark.executor.cores", coreCount)
                .Config("spark.executor.instances", nodeCount)
                .GetOrCreate();

            DataFrame df = spark.Read().Option("header", true).Option("inferSchema", true)
                .Csv("C:\\Users\\abdel\\Desktop\\ESGF\\5ESGF-BD-2021\\ESGF.Sudoku.Spark.RecursiveSearch\\sudoku.csv");
            var limitedDf = df.Limit(limitRows);
            // Use ML.NET in a UDF to evaluate each review 
            spark.Udf().Register<string, string>(
                "MLudf",
                  (text) => Solve(text.Trim(new Char[] { ' ', '"', 'a' })));

            var watch = Stopwatch.StartNew();
            limitedDf.CreateOrReplaceTempView("Sudokus");
            limitedDf.Show();
            DataFrame sqlDf = spark.Sql("SELECT quizzes, MLudf(quizzes) FROM Sudokus");
            sqlDf.Show();
            watch.Stop();
            Console.WriteLine("limitRows :" + limitRows);
            Console.WriteLine("coreCount :" + coreCount);
            Console.WriteLine("nodeCount :" + nodeCount);
            Console.WriteLine("ElapsedMilliseconds : " + watch.ElapsedMilliseconds);
            spark.Stop();
        }


        public static string Solve(string sudokuText)
        {
            const int numOrganisms = 200;
            const int maxEpochs = 10000;
            const int maxRestarts = 150;
            var solver = new SudokuSolver();
            var solvedSudoku = solver.Solve(Sudoku.New(TextToGrid(sudokuText)), numOrganisms, maxEpochs, maxRestarts);
            return solvedSudoku.ToString();
        }



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
    }
}
//quizzes