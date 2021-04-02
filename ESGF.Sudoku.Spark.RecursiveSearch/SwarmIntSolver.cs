using System;
using System.Collections.Generic;
using System.Linq;

namespace ESGF.Sudoku.Spark.Swarm
{
    public class SudokuSolver
    {
        public const int SIZE = 9;
        public const int BLOCK_SIZE = 3;
        private Random _rnd;

        public SudokuSolver()
        {
            _rnd = new Random();
        }

        public Sudoku Solve(Sudoku sudoku, int numOrganisms, int maxEpochs, int maxRestarts)
        {
            var error = int.MaxValue;
            Sudoku bestSolution = null;
            var attempt = 0;
            while (error != 0 && attempt < maxRestarts)
            {
                //Console.WriteLine($"Attempt: {attempt}");
                _rnd = new Random(attempt);
                bestSolution = SolveInternal(sudoku, numOrganisms, maxEpochs);
                error = bestSolution.Error;
                ++attempt;
            }

            return bestSolution;
        }

        private Sudoku SolveInternal(Sudoku sudoku, int numOrganisms, int maxEpochs)
        {
            var numberOfWorkers = (int)(numOrganisms * 0.90);
            var hive = new Organism[numOrganisms];

            var bestError = int.MaxValue;
            Sudoku bestSolution = null;

            for (var i = 0; i < numOrganisms; ++i)
            {
                var organismType = i < numberOfWorkers
                  ? OrganismType.Worker
                  : OrganismType.Explorer;

                var randomSudoku = Sudoku.New(MatrixHelper.RandomMatrix(_rnd, sudoku.CellValues));
                var err = randomSudoku.Error;

                hive[i] = new Organism(organismType, randomSudoku.CellValues, err, 0);

                if (err >= bestError) continue;
                bestError = err;
                bestSolution = Sudoku.New(randomSudoku);
            }

            var epoch = 0;
            while (epoch < maxEpochs)
            {
                // if (epoch % 1000 == 0)
                  //  Console.WriteLine($"Epoch: {epoch}, Best error: {bestError}");

                if (bestError == 0)
                    break;

                for (var i = 0; i < numOrganisms; ++i)
                {
                    if (hive[i].Type == OrganismType.Worker)
                    {
                        var neighbor = MatrixHelper.NeighborMatrix(_rnd, sudoku.CellValues, hive[i].Matrix);
                        var neighborSudoku = Sudoku.New(neighbor);
                        var neighborError = neighborSudoku.Error;

                        var p = _rnd.NextDouble();
                        if (neighborError < hive[i].Error || p < 0.001)
                        {
                            hive[i].Matrix = MatrixHelper.DuplicateMatrix(neighbor);
                            hive[i].Error = neighborError;
                            if (neighborError < hive[i].Error) hive[i].Age = 0;

                            if (neighborError >= bestError) continue;
                            bestError = neighborError;
                            bestSolution = neighborSudoku;
                        }
                        else
                        {
                            hive[i].Age++;
                            if (hive[i].Age <= 1000) continue;
                            var randomSudoku = Sudoku.New(MatrixHelper.RandomMatrix(_rnd, sudoku.CellValues));
                            hive[i] = new Organism(0, randomSudoku.CellValues, randomSudoku.Error, 0);
                        }
                    }
                    else
                    {
                        var randomSudoku = Sudoku.New(MatrixHelper.RandomMatrix(_rnd, sudoku.CellValues));
                        hive[i].Matrix = MatrixHelper.DuplicateMatrix(randomSudoku.CellValues);
                        hive[i].Error = randomSudoku.Error;

                        if (hive[i].Error >= bestError) continue;
                        bestError = hive[i].Error;
                        bestSolution = randomSudoku;
                    }
                }

                // merge best worker with best explorer into worst worker
                var bestWorkerIndex = 0;
                var smallestWorkerError = hive[0].Error;
                for (var i = 0; i < numberOfWorkers; ++i)
                {
                    if (hive[i].Error >= smallestWorkerError) continue;
                    smallestWorkerError = hive[i].Error;
                    bestWorkerIndex = i;
                }

                var bestExplorerIndex = numberOfWorkers;
                var smallestExplorerError = hive[numberOfWorkers].Error;
                for (var i = numberOfWorkers; i < numOrganisms; ++i)
                {
                    if (hive[i].Error >= smallestExplorerError) continue;
                    smallestExplorerError = hive[i].Error;
                    bestExplorerIndex = i;
                }

                var worstWorkerIndex = 0;
                var largestWorkerError = hive[0].Error;
                for (var i = 0; i < numberOfWorkers; ++i)
                {
                    if (hive[i].Error <= largestWorkerError) continue;
                    largestWorkerError = hive[i].Error;
                    worstWorkerIndex = i;
                }

                var merged = MatrixHelper.MergeMatrices(_rnd, hive[bestWorkerIndex].Matrix, hive[bestExplorerIndex].Matrix);
                var mergedSudoku = Sudoku.New(merged);

                hive[worstWorkerIndex] = new Organism(0, merged, mergedSudoku.Error, 0);
                if (hive[worstWorkerIndex].Error < bestError)
                {
                    bestError = hive[worstWorkerIndex].Error;
                    bestSolution = mergedSudoku;
                }

                ++epoch;
            }

            return bestSolution;
        }
    }


    public static class MatrixHelper
    {
        public const int SIZE = 9;
        public const int BLOCK_SIZE = 3;

        public static int[,] CreateMatrix(int m, int n)
        {
            var result = new int[m, n];
            return result;
        }

        public static (int row, int column) Corner(int block)
        {
            int r = -1, c = -1;

            if (block == 0 || block == 1 || block == 2)
                r = 0;
            else if (block == 3 || block == 4 || block == 5)
                r = 3;
            else if (block == 6 || block == 7 || block == 8)
                r = 6;

            if (block == 0 || block == 3 || block == 6)
                c = 0;
            else if (block == 1 || block == 4 || block == 7)
                c = 3;
            else if (block == 2 || block == 5 || block == 8)
                c = 6;

            return (r, c);
        }

        public static int Block(int r, int c)
        {
            if (r >= 0 && r <= 2 && c >= 0 && c <= 2)
                return 0;
            if (r >= 0 && r <= 2 && c >= 3 && c <= 5)
                return 1;
            if (r >= 0 && r <= 2 && c >= 6 && c <= 8)
                return 2;
            if (r >= 3 && r <= 5 && c >= 0 && c <= 2)
                return 3;
            if (r >= 3 && r <= 5 && c >= 3 && c <= 5)
                return 4;
            if (r >= 3 && r <= 5 && c >= 6 && c <= 8)
                return 5;
            if (r >= 6 && r <= 8 && c >= 0 && c <= 2)
                return 6;
            if (r >= 6 && r <= 8 && c >= 3 && c <= 5)
                return 7;
            if (r >= 6 && r <= 8 && c >= 6 && c <= 8)
                return 8;

            throw new Exception("Unable to find Block()");
        }

        public static int[,] RandomMatrix(Random rnd, int[,] problem)
        {
            var result = DuplicateMatrix(problem);

            for (var block = 0; block < SIZE; ++block)
            {
                var corner = Corner(block);
                var values = Enumerable.Range(1, SIZE).ToList();

                for (var k = 0; k < values.Count; ++k)
                {
                    var ri = rnd.Next(k, values.Count);
                    var tmp = values[k];
                    values[k] = values[ri];
                    values[ri] = tmp;
                }

                var r = corner.row;
                var c = corner.column;
                for (var i = r; i < r + BLOCK_SIZE; ++i)
                {
                    for (var j = c; j < c + BLOCK_SIZE; ++j)
                    {
                        var value = problem[i, j];
                        if (value != 0)
                            values.Remove(value);
                    }
                }

                var pointer = 0;
                for (var i = r; i < r + BLOCK_SIZE; ++i)
                {
                    for (var j = c; j < c + BLOCK_SIZE; ++j)
                    {
                        if (result[i, j] != 0) continue;
                        var value = values[pointer];
                        result[i, j] = value;
                        ++pointer;
                    }
                }
            }

            return result;
        }


        public static int[,] DuplicateMatrix(int[,] matrix)
        {
            var m = matrix.GetLength(0);
            var n = matrix.GetLength(1);
            var result = CreateMatrix(m, n);
            for (var i = 0; i < m; ++i)
                for (var j = 0; j < n; ++j)
                    result[i, j] = matrix[i, j];

            return result;
        }

        public static int[,] MergeMatrices(Random rnd, int[,] m1, int[,] m2)
        {
            var result = DuplicateMatrix(m1);

            for (var block = 0; block < 9; ++block)
            {
                var pr = rnd.NextDouble();
                if (!(pr < 0.50)) continue;
                var corner = Corner(block);
                for (var i = corner.row; i < corner.row + BLOCK_SIZE; ++i)
                    for (var j = corner.column; j < corner.column + BLOCK_SIZE; ++j)
                        result[i, j] = m2[i, j];
            }

            return result;
        }

        public static int[,] NeighborMatrix(Random rnd, int[,] problem, int[,] matrix)
        {
            // pick a random 3x3 block,
            // pick two random cells in block
            // swap values
            var result = DuplicateMatrix(matrix);

            var block = rnd.Next(0, SIZE); // [0,8]
            var corner = Corner(block);
            var cells = new List<int[]>();
            for (var i = corner.row; i < corner.row + BLOCK_SIZE; ++i)
            {
                for (var j = corner.column; j < corner.column + BLOCK_SIZE; ++j)
                {
                    if (problem[i, j] == 0)
                        cells.Add(new[] { i, j });
                }
            }

            if (cells.Count < 2)
                throw new Exception($"Block {block} doesn't have two values to swap!");

            // pick two. suppose there are 4 possible cells 0,1,2,3
            var k1 = rnd.Next(0, cells.Count); // 0,1,2,3
            var inc = rnd.Next(1, cells.Count); // 1,2,3
            var k2 = (k1 + inc) % cells.Count;

            var r1 = cells[k1][0];
            var c1 = cells[k1][1];
            var r2 = cells[k2][0];
            var c2 = cells[k2][1];

            var tmp = result[r1, c1];
            result[r1, c1] = result[r2, c2];
            result[r2, c2] = tmp;

            return result;
        }
    }

}