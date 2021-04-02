using System;
using System.Collections.Generic;
using System.Text;

namespace Sudoku.Core
{
    public interface ISudokuSolver
    {

        void Solve(GrilleSudoku s);
        
    }
}
