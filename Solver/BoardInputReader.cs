using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solver
{
    public static class BoardInputReader
    {

        public static string Read()
        {
            StringBuilder boardData = new StringBuilder();

            string? boardLine;
            while (true)
            {
                boardLine = Console.In.ReadLine();
                if (boardLine == null)
                    break;

                boardData.AppendLine(boardLine);
            }

            return boardData.ToString();
        }
    }
}
