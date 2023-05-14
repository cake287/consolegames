using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace consolegames
{
    class Console2048
    {
        int tileWidth = 4; // metric is characters 
        int boardWidth = 4;
        int[,] board; // int value refers to the log2 of the displayed number
        int score = 0;
        int[] colourInts = { 0, 8, 7, 4, 12, 13, 5, 3, 11, 9, 1, 15  }; //https://docs.microsoft.com/en-us/dotnet/api/system.consolecolor?view=netframework-4.8
        bool[] shouldInvertForeCol = { false, false, true, false, false, false, false, false, true, false, false, true };
        ConsoleChar[,] drawingBoard;

        public void run(bool showHelp, bool chooseGameParameters)
        {

            if (showHelp)
            {
                Console.WriteLine("Move tiles with the arrow keys. Combine equal tiles to create a larger tile. Try and create a 2048 tile.");
                Console.WriteLine("Combine equal tiles to create a larger tile.");
                Console.WriteLine("Try and create a 2048 tile.");
                Console.ReadLine();
                Console.Clear();
            }

            if (chooseGameParameters)
            {
                tileWidth = Program.GetIntInput("Tile width (characters) (default: 4) : ", 100, 1);
                boardWidth = Program.GetIntInput("Board width (tiles) (default: 4) : ", int.MaxValue, 2);
                Console.Clear();
            }
            board = new int[boardWidth, boardWidth];
            drawingBoard = new ConsoleChar[(tileWidth + 2) * boardWidth, 3 * boardWidth + 1];
            drawInit();

            createNewValues();
            createNewValues();

            Console.CursorVisible = false;
            //board[0, 0] = 1;
            //board[0, 1] = 2;
            //board[0, 2] = 3;
            //board[0, 3] = 4;
            //board[1, 0] = 5;
            //board[1, 1] = 6;
            //board[1, 2] = 7;
            //board[1, 3] = 8;
            //board[2, 0] = 9;
            //board[2, 1] = 10;
            //board[2, 2] = 11;
            //board[2, 3] = 12;
            //board[3, 0] = 13;
            //board[3, 1] = 14;
            //board[3, 2] = 15;
            //board[3, 3] = 16;

            draw();

            bool shouldLoop = true;
            do
            {
                int dir = -1;
                ConsoleKey input = Console.ReadKey(true).Key;

                if (input == ConsoleKey.DownArrow) { dir = 0; }
                else if (input == ConsoleKey.UpArrow) { dir = 1; }
                else if (input == ConsoleKey.RightArrow) { dir = 2; }
                else if (input == ConsoleKey.LeftArrow) { dir = 3; }
                else if (input == ConsoleKey.Escape)
                {
                    shouldLoop = false;
                }

                if (dir != -1)
                {
                    if (testLost())
                    {
                        shouldLoop = false;
                    }
                    else if (doMoves(dir))
                    {
                        createNewValues();
                        draw();
                    }
                    if (testLost())
                    {
                        shouldLoop = false;
                    }
                }
            } while (shouldLoop);
            Console.WriteLine("You lose!");
            Console.ReadLine();
        }

        int[] translateDir(int dir)
        {
            int[] r = new int[2]; // dir[0] is x, dir[1] is y
            if (dir == 0) { r[0] = 0; r[1] = 1; }
            else if (dir == 1) { r[0] = 0; r[1] = -1; }
            else if (dir == 2) { r[0] = 1; r[1] = 0; }
            else if (dir == 3) { r[0] = -1; r[1] = 0; }
            return r;
        }

        bool checkBounds(int x, int y) //true means coords are within board bounds, coords are 0 based
        {
            bool r = true;
            if (x < 0 || x > boardWidth - 1) { r = false; }
            if (y < 0 || y > boardWidth - 1) { r = false; }
            return r;
        }

        bool doMoves(int dir_)
        {
            bool hasMoved = false;
            int[] dir = translateDir(dir_);
            if (dir[0] == 0) // up or down, do x then y loops
            {
                for (int x = 0; x <= boardWidth - 1; x++)
                {
                    bool[] hasMerged = new bool[boardWidth]; // refers to each tile in the column. if two tiles have merged into that tile, it can no longer be merged into for the rest of this move
                    for (int y = dir[1] > 0 ? boardWidth - 1 : 0; dir[1] > 0 ? y >= 0: y <= boardWidth - 1; y -= dir[1])
                    {
                        for (int y_ = y; dir[1] > 0 ? y_ <= boardWidth - 1 : y_ >= 0; y_ += dir[1])
                        {
                            if (board[x, y_] != 0)
                            {
                                int newY = y_ + dir[1];
                                if (checkBounds(x, newY))
                                {
                                    if (board[x, newY] == 0)
                                    {
                                        board[x, newY] = board[x, y_];
                                        board[x, y_] = 0;
                                        hasMerged[newY] = hasMerged[y_];
                                        hasMerged[y_] = false;
                                        hasMoved = true;
                                    } else if (board[x, newY] == board[x, y_] && !hasMerged[newY] && !hasMerged[y_])
                                    {
                                        board[x, newY] += 1;
                                        board[x, y_] = 0;
                                        hasMerged[newY] = true;
                                        score += Power2(board[x, newY]);
                                        hasMoved = true;
                                    }
                                }
                            }
                        }
                    }
                }
            } 
            else // right or left, do y then x loops
            {
                for (int y = 0; y <= boardWidth - 1; y++)
                {
                    bool[] hasMerged = new bool[boardWidth]; 
                    for (int x = dir[0] > 0 ? boardWidth - 1 : 0; dir[0] > 0 ? x >= 0 : x <= boardWidth - 1; x -= dir[0])
                    {
                        for (int x_ = x; dir[0] > 0 ? x_ <= boardWidth - 1 : x_ >= 0; x_ += dir[0])
                        {
                            if (board[x_, y] != 0)
                            {
                                int newX = x_ + dir[0];
                                if (checkBounds(newX, y))
                                {
                                    if (board[newX, y] == 0)
                                    {
                                        board[newX, y] = board[x_, y];
                                        board[x_, y] = 0;
                                        hasMerged[newX] = hasMerged[x_];
                                        hasMerged[x_] = false;
                                        hasMoved = true;
                                    }
                                    else if (board[newX, y] == board[x_, y] && !hasMerged[newX] && !hasMerged[x_])
                                    {
                                        board[newX, y] += 1;
                                        board[x_, y] = 0;
                                        hasMerged[newX] = true;
                                        score += Power2(board[newX, y]);
                                        hasMoved = true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return hasMoved;
        }

        bool testLost()
        {
            bool r = false;
            bool isFull = true;
            for (int x = 0; x <= boardWidth - 1; x++)
            {
                for (int y = 0; y <= boardWidth - 1; y++)
                {
                    if (board[x, y] == 0)
                    {
                        isFull = false;
                    }
                }
            }
            if (isFull)
            {
                bool hasAnyTwoAdjacent = false;
                // loop through all and see if they have an adjacent tile of the same value
                for (int x = 0; x <= boardWidth - 1 && hasAnyTwoAdjacent == false; x++)
                {
                    for (int y = 0; y <= boardWidth - 1 && hasAnyTwoAdjacent == false; y++)
                    {
                        for (int locX = -1; locX <= 1 && hasAnyTwoAdjacent == false; locX += 2)
                        {
                            if (checkBounds(x + locX, y))
                            {
                                if (board[x, y] == board[x + locX, y])
                                {
                                    hasAnyTwoAdjacent = true;
                                }
                            }
                        }
                        for (int locY = -1; locY <= 1 && hasAnyTwoAdjacent == false; locY += 2)
                        {
                            if (checkBounds(x, y + locY))
                            {
                                if (board[x, y] == board[x, y + locY])
                                {
                                    hasAnyTwoAdjacent = true;
                                }
                            }
                        }
                    }
                }
                if (!hasAnyTwoAdjacent) { r = true; }
            }
            return r;
        }

        void createNewValues()
        {
            List<int> emptyTilesX = new List<int>();
            List<int> emptyTilesY = new List<int>();
            for (int x = 0; x <= boardWidth - 1; x++)
            {
                for (int y = 0; y <= boardWidth - 1; y++)
                {
                    if (board[x, y] == 0)
                    {
                        emptyTilesX.Add(x);
                        emptyTilesY.Add(y);
                    }
                }
            }
            if (emptyTilesX.Count != 0)
            {
                Random random = new Random();
                int randIndex = random.Next(emptyTilesX.Count); // Random.Next upperbound is exclusive, but List.Count is 1 based (+ 1 - 1 = +0 to 'fix')
                int newVal = random.Next(9);
                newVal = newVal < 8 ? 1 : 2;
                board[emptyTilesX[randIndex], emptyTilesY[randIndex]] = newVal;
            }
        }

        void draw()
        {
            for (int boardY = 0; boardY <= boardWidth - 1; boardY++)
            {
                for (int boardX = 0; boardX <= boardWidth - 1; boardX++)
                {
                    int val = board[boardX, boardY];
                    string numStr = Power2(val).ToString();
                    if (int.Parse(numStr) <= 1) { numStr = ""; }
                    numStr = numStr.PadLeft(tileWidth, ' ');

                    val = val > 11 ? 0 : val; // greater than 2048?: not enough colours to colour them in differently, so it looks the same as no value. 
                                              // greater than 8192 (or different depending on board size) will look weird on tileSize 4, but somehow i 
                                              //    doubt anyone will ever get there on this program

                    int foreColour = shouldInvertForeCol[val] ? 0 : 15;
                    ConsoleChar[] consoleChars = ConsoleChar.StrToConsoleCharArr(numStr, foreColour, colourInts[val]);

                    for (int tileX = 0; tileX <= tileWidth - 1; tileX++)
                    {
                        drawingBoard[boardX * (tileWidth + 2) + tileX + 1, boardY * 3 + 1] = consoleChars[tileX];
                    }
                }
            }
            addScoreToDrawingBoard();
            Drawing.draw(drawingBoard);
        }

        void drawInit()
        {
            for (int boardY = 0; boardY <= boardWidth - 1; boardY++)
            {
                for (int boardX = 0; boardX <= boardWidth - 1; boardX++)
                {
                    drawingBoard[boardX * (tileWidth + 2), boardY * 3] = new ConsoleChar('┌');
                    drawingBoard[boardX * (tileWidth + 2) + tileWidth + 1, boardY * 3] = new ConsoleChar('┐');
                    drawingBoard[boardX * (tileWidth + 2), boardY * 3 + 2] = new ConsoleChar('└');
                    drawingBoard[boardX * (tileWidth + 2) + tileWidth + 1, boardY * 3 + 2] = new ConsoleChar('┘');

                    for (int tileX = 0; tileX <= tileWidth + 1; tileX += tileWidth + 1)
                    {
                        drawingBoard[boardX * (tileWidth + 2) + tileX, boardY * 3 + 1] = new ConsoleChar('│');
                    }

                    for (int tileX = 0; tileX <= tileWidth - 1; tileX++)
                    {
                        for (int tileY = 0; tileY <= 2; tileY += 2)
                        {
                            drawingBoard[boardX * (tileWidth + 2) + tileX + 1, boardY * 3 + tileY] = new ConsoleChar('─');
                        }
                        drawingBoard[boardX * (tileWidth + 2) + tileX + 1, boardY * 3 + 1] = new ConsoleChar(' ');
                    }
                }
            }
            addScoreToDrawingBoard();
            Drawing.draw(drawingBoard);
        }

        void addScoreToDrawingBoard()
        {
            string scoreStr = ("Score: " + score).PadRight(drawingBoard.GetLength(0), ' ');
            ConsoleChar[] scoreChars = ConsoleChar.StrToConsoleCharArr(scoreStr);
            for (int x = 0; x <= scoreChars.Length - 1; x++)
            {
                drawingBoard[x, drawingBoard.GetLength(1) - 1] = scoreChars[x];
            }
        }

        int Power2(int index)
        {
            int r = 1;
            for (int i = 0; i < index; i++)
            {
                r *= 2;
            }
            return r;
        }
    }
}
