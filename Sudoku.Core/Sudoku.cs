using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;

namespace Sudoku.Core
{
    /// <summary>
    /// Class that represents a GrilleSudoku, fully or partially completed
    /// Holds a list of 81 int for cells, with 0 for empty cells
    /// Can parse strings and files from most common formats and displays the sudoku in an easy to read format
    /// </summary>
    public class GrilleSudoku:ICloneable
    {



        /// <summary>
        /// The list of cells is used many times and thus stored for quicker access.
        /// </summary>
        public static readonly ReadOnlyCollection<int> IndicesCellules = new ReadOnlyCollection<int>(Enumerable.Range(0, 81).ToList());

        /// <summary>
        /// The list of row indexes is used many times and thus stored for quicker access.
        /// </summary>
        public static readonly ReadOnlyCollection<int> IndicesVoisinages = new ReadOnlyCollection<int>(Enumerable.Range(0, 9).ToList());

        private static readonly List<List<int>> _voisinagesLignes =
            IndicesCellules.GroupBy(x => x / 9).Select(g => g.ToList()).ToList();

        private static readonly List<List<int>> _voisinagesColonnes =
            IndicesCellules.GroupBy(x => x % 9).Select(g => g.ToList()).ToList();

        private static readonly List<List<int>> _voisinagesBoites =
            IndicesCellules.GroupBy(x => x / 27 * 27 + x % 9 / 3 * 3).Select(g => g.ToList()).ToList();

        public static readonly List<List<int>> TousLesVoisinages =
            _voisinagesLignes.Concat(_voisinagesColonnes).Concat(_voisinagesBoites).ToList();



        public static readonly List<List<int>> VoisinagesParCellule;
        

        static GrilleSudoku()
        {
            VoisinagesParCellule = new List<List<int>>();
            foreach (var indicesCellule in IndicesCellules)
            {
                var cellVoisinage = new List<int>();
                VoisinagesParCellule.Add(cellVoisinage);
                foreach (var voisinage in TousLesVoisinages)
                {
                    if (voisinage.Contains(indicesCellule))
                    {
                        foreach (var voisin in voisinage)
                        {
                            if (!cellVoisinage.Contains(voisin))
                            {
                                cellVoisinage.Add(voisin);
                            }
                        }
                    }
                }
            }
        }



        /// <summary>
        /// constructor that initializes the board with 81 cells
        /// </summary>
        /// <param name="cells"></param>
        public GrilleSudoku(IEnumerable<int> cells)
        {
            var enumerable = cells.ToList();
            if (enumerable.Count != 81)
            {
                throw new ArgumentException("GrilleSudoku should have exactly 81 cells", nameof(cells));
            }
            Cellules = new List<int>(enumerable);
        }

        public GrilleSudoku()
        {
        }



        // The List property makes it easier to manipulate cells,
        public List<int> Cellules { get; set; } = Enumerable.Repeat(0, 81).ToList();

        /// <summary>
        /// Easy access by row and column number
        /// </summary>
        /// <param name="x">row number (between 0 and 8)</param>
        /// <param name="y">column number (between 0 and 8)</param>
        /// <returns>value of the cell</returns>
        public int GetCellule(int x, int y)
        {
            return Cellules[(9 * x) + y];
        }

        /// <summary>
        /// Easy setter by row and column number
        /// </summary>
        /// <param name="x">row number (between 0 and 8)</param>
        /// <param name="y">column number (between 0 and 8)</param>
        /// <param name="value">value of the cell to set</param>
        public void SetCell(int x, int y, int value)
        {
            Cellules[(9 * x) + y] = value;
        }

        /// <summary>
        /// Displays a GrilleSudoku in an easy-to-read format
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var lineSep = new string('-', 31);
            var blankSep = new string(' ', 8);

            var output = new StringBuilder();
            output.Append(lineSep);
            output.AppendLine();

            for (int row = 1; row <= 9; row++)
            {
                output.Append("| ");
                for (int column = 1; column <= 9; column++)
                {

                    var value = Cellules[(row - 1) * 9 + (column - 1)];

                    output.Append(value);
                    if (column % 3 == 0)
                    {
                        output.Append(" | ");
                    }
                    else
                    {
                        output.Append("  ");
                    }
                }

                output.AppendLine();
                if (row % 3 == 0)
                {
                    output.Append(lineSep);
                }
                else
                {
                    output.Append("| ");
                    for (int i = 0; i < 3; i++)
                    {
                        output.Append(blankSep);
                        output.Append("| ");
                    }
                }
                output.AppendLine();
            }

            return output.ToString();
        }

        public List<List<int>> GetVoisins()
        {
            return (TousLesVoisinages);
        }



        public int[] GetPossibilities(int x, int y)
        {
            if (x < 0 || x >= 9 || y < 0 || y >= 9)
            {
                throw new ApplicationException("Invalid Coodrindates");
            }

            bool[] used = new bool[9];
            for (int i = 0; i < 9; i++)
            {
                used[i] = false;
            }

            for (int ix = 0; ix < 9; ix++)
            {
                if (GetCellule(ix, y) != 0)
                {
                    used[GetCellule(ix, y) - 1] = true;
                }
            }

            for (int iy = 0; iy < 9; iy++)
            {
                if (GetCellule(x, iy) != 0)
                {
                    used[GetCellule(x, iy) - 1] = true;
                }
            }

            int sx = (x / 3) * 3;
            int sy = (y / 3) * 3;

            for (int ix = 0; ix < 3; ix++)
            {
                for (int iy = 0; iy < 3; iy++)
                {
                    if (GetCellule(sx + ix, sy + iy) != 0)
                    {
                        used[GetCellule(sx + ix, sy + iy) - 1] = true;
                    }
                }
            }

            List<int> res = new List<int>();

            for (int i = 0; i < 9; i++)
            {
                if (used[i] == false)
                {
                    res.Add(i + 1);
                }
            }

            return res.ToArray();
        }



        /// <summary>
        /// Parses a single GrilleSudoku
        /// </summary>
        /// <param name="sudokuAsString">the string representing the sudoku</param>
        /// <returns>the parsed sudoku</returns>
        public static GrilleSudoku Parse(string sudokuAsString)
        {
            return ParseMulti(new[] { sudokuAsString })[0];
        }


        /// <summary>
        /// Parses a file with one or several sudokus
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>the list of parsed Sudokus</returns>
        public static List<GrilleSudoku> ParseFile(string fileName)
        {
            return ParseMulti(File.ReadAllLines(fileName));
        }

        /// <summary>
        /// Parses a list of lines into a list of sudoku, accounting for most cases usually encountered
        /// </summary>
        /// <param name="lines">the lines of string to parse</param>
        /// <returns>the list of parsed Sudokus</returns>
        public static List<GrilleSudoku> ParseMulti(string[] lines)
        {
            var toReturn = new List<GrilleSudoku>();
            var cells = new List<int>(81);
            // we ignore lines not starting with a sudoku character
            foreach (var line in lines.Where(l => l.Length > 0
                                                 && IsSudokuChar(l[0])))
            {
                foreach (char c in line)
                {
                    //we ignore lines not starting with cell chars
                    if (IsSudokuChar(c))
                    {
                        if (char.IsDigit(c))
                        {
                            // if char is a digit, we add it to a cell
                            cells.Add((int)Char.GetNumericValue(c));
                        }
                        else
                        {
                            // if char represents an empty cell, we add 0
                            cells.Add(0);
                        }
                    }
                    // when 81 cells are entered, we create a sudoku and start collecting cells again.
                    if (cells.Count == 81)
                    {
                        toReturn.Add(new GrilleSudoku() { Cellules = new List<int>(cells) });
                        // we empty the current cell collector to start building a new GrilleSudoku
                        cells.Clear();
                    }

                }
            }

            return toReturn;
        }


        /// <summary>
        /// identifies characters to be parsed into sudoku cells
        /// </summary>
        /// <param name="c">a character to test</param>
        /// <returns>true if the character is a cell's char</returns>
        private static bool IsSudokuChar(char c)
        {
            return char.IsDigit(c) || c == '.' || c == 'X' || c == '-';
        }

        public object Clone()
        {
            return CloneSudoku();
        }

        public Core.GrilleSudoku CloneSudoku()
        {
            return new GrilleSudoku(new List<int>(this.Cellules));
        }


        private static IList<ISudokuSolver> _CachedSolvers;

        public static IList<ISudokuSolver> GetSolvers()
        {
            if (_CachedSolvers == null)
            {
            var solvers = new List<ISudokuSolver>();

                
            foreach (var file in Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory))
            {
                if (file.EndsWith("dll") && !(Path.GetFileName(file).StartsWith("libz3")))
                {
                    try
                    {
                        var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
                        foreach (var type in assembly.GetTypes())
                        {
                            if (typeof(ISudokuSolver).IsAssignableFrom(type) && !(typeof(ISudokuSolver) == type))
                            {
                                var solver = (ISudokuSolver)Activator.CreateInstance(type);
                                solvers.Add(solver);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e);
                    }

                }

            }

                _CachedSolvers = solvers;
        }

            return _CachedSolvers;
        }


        public int NbErrors(GrilleSudoku originalPuzzle)
        {
            // We use a large lambda expression to count duplicates in rows, columns and boxes
            var toReturn = GrilleSudoku.TousLesVoisinages.Select(n => n.Select(nx => this.Cellules[nx])).Sum(n => n.GroupBy(x => x).Select(g => g.Count() - 1).Sum());
            var cellsToTest = this.Cellules.Select((c, i) => new { index = i, cell = c }).ToList();
            toReturn += cellsToTest.Count(x => originalPuzzle.Cellules[x.index] > 0 && originalPuzzle.Cellules[x.index] != x.cell); // Mask
            return toReturn;
        }

        public bool IsValid(GrilleSudoku originalPuzzle)
        {
            return NbErrors(originalPuzzle) == 0;
        }

        public void setSudoku(int[][] tab)  //Attribue des val au sudoku 
        {
      
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Cellules[(9 * i) + j] = tab[i][j];
                }
            }
        }
    }
}
