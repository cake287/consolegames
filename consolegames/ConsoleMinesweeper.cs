using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace consolegames
{
    class ConsoleMinesweeper
    {
        bool[,] board;
        int boardWidth = 30;
        int boardHeight = 20;
        float mineFreq = 0.1f; // proportion of tiles which have mines behind them

        public void run()
        {
            Random r = new Random();
            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {

                }  
            }
        }

        //public void PopulateBoard()
    }
}
