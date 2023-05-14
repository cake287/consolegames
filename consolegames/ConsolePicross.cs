using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace consolegames
{
    class ConsolePicross
    {
        Point boardSize = new Point(10, 10);
        Tile[,] board;
        Point selectedPos;
        Point previousSelectedPos;
        static int mistakes = 0;
        bool hasWon = false;
        string[] sideLabels;
        List<int>[] bottomLabels; 
        ConsoleChar[,] drawingBoard;

        public void run(bool showHelp, bool chooseGameParameters)
        {
            mistakes = 0;

            if (showHelp)
            {
                Console.WriteLine("Use the arrow keys to navigate the board. Use WASD to move larger steps.");
                Console.WriteLine("Mark a tile as 'on' (blue) with X, mark a tile as 'off' (grey) with C.");
                Console.WriteLine("Use the labels, which show the size and order of groups of 'on' tiles, to try to mark all the tiles.");
                Console.ReadLine();
                Console.Clear();
            }

            if (chooseGameParameters)
            {
                boardSize.x = Program.GetIntInput("Board width (default: 10) : ", 100, 2);
                boardSize.y = Program.GetIntInput("Board height (default: 10) : ", 100, 2);
                Console.Clear();
            }
            Console.CursorVisible = false;
            board = new Tile[boardSize.x, boardSize.y];
            sideLabels = new string[boardSize.y];
            bottomLabels = new List<int>[boardSize.x];
            selectedPos = new Point((int)Math.Round((double)(boardSize.x / 2)), (int)Math.Round((double)(boardSize.y / 2)));

            fillBoard();
            createLabels();
            
            drawingBoard = new ConsoleChar[Math.Max(boardSize.x * 3 + sideLabels.Max(sl => sl.Length), ("You win! Mistakes: " + (boardSize.x * boardSize.y)).ToString().Length), 
                boardSize.y + bottomLabels.Max(bl => bl.Count) + 1];
            drawInit();

            Console.CursorVisible = false;
            bool shouldLoop = true;
            bool shouldDraw = true;
            do
            {
                if (shouldDraw)
                {
                    draw();
                } else
                {
                    shouldDraw = true;
                }

                ConsoleKey input = Console.ReadKey(true).Key;
                int inputType = 0;
                Point dir = new Point();

                if (input == ConsoleKey.DownArrow) { dir.y = 1; }
                else if (input == ConsoleKey.UpArrow) { dir.y = -1; }
                else if (input == ConsoleKey.RightArrow) { dir.x = 1; }
                else if (input == ConsoleKey.LeftArrow) { dir.x = -1; }
                else if (input == ConsoleKey.S) { dir.y = 5; }
                else if (input == ConsoleKey.W) { dir.y = -5; }
                else if (input == ConsoleKey.D) { dir.x = 5; }
                else if (input == ConsoleKey.A) { dir.x = -5; }
                else if (input == ConsoleKey.X) { inputType = 1; }
                else if (input == ConsoleKey.C) { inputType = 2; }
                else if (input == ConsoleKey.Escape)
                {
                    shouldLoop = false;
                }

                if (dir.x != 0 || dir.y != 0)
                {
                    previousSelectedPos = selectedPos;
                    Point newSelectedPos = selectedPos + dir;
                    if (!checkBounds(newSelectedPos))
                    {
                        if (newSelectedPos.x < 0) { newSelectedPos.x = boardSize.x - 1; }
                        else if (newSelectedPos.x > boardSize.x - 1) { newSelectedPos.x = 0; }

                        if (newSelectedPos.y < 0) { newSelectedPos.y = boardSize.y - 1; }
                        else if (newSelectedPos.y > boardSize.y - 1) { newSelectedPos.y = 0; }
                    }
                    selectedPos = newSelectedPos;
                }
                if (inputType == 1)
                {
                    board[selectedPos.x, selectedPos.y].Reveal(true);
                } else if (inputType == 2)
                {
                    board[selectedPos.x, selectedPos.y].Reveal(false);
                }
                if (checkWin()) {
                    shouldLoop = false;
                    hasWon = true;
                }
            } while (shouldLoop == true);
            if (hasWon)
            {
                draw();
                Console.ReadLine();
            }
        }

        void fillBoard()
        {
            Random random = new Random();
            for (int x = 0; x <= boardSize.x - 1; x++)
            {
                for (int y = 0; y <= boardSize.y - 1; y++)
                {
                    board[x, y] = new Tile(random.Next(2) > 0);
                    //board[x, y].isRevealed = true;
                }
            }
        }

        void createLabels()
        {
            // side labels
            int onCount = 0;
            for (int y = 0; y <= boardSize.y - 1; y++)
            {
                sideLabels[y] = "";
                for (int x = 0; x <= boardSize.x - 1; x++)
                {
                    if (board[x, y].isOn)
                    {
                        onCount++;
                    }
                    if ((board[x, y].isOn == false && onCount > 0) || (board[x, y].isOn && x == boardSize.x - 1)) // if the previous tile was on (end of current set of ons) or if reached the end
                    {
                        sideLabels[y] += " " + onCount;
                        onCount = 0;
                    }
                }
            }

            // bottom labels
            onCount = 0;
            for (int x = 0; x <= boardSize.x - 1; x++)
            {
                bottomLabels[x] = new List<int>();
                for (int y = 0; y <= boardSize.y - 1; y++)
                {
                    if (board[x, y].isOn)
                    {
                        onCount++;
                    }
                    if ((board[x, y].isOn == false && onCount > 0) || (board[x, y].isOn && y == boardSize.y - 1)) // if the previous tile was on (end of current set of ons) or if reached the end
                    {
                        bottomLabels[x].Add(onCount);
                        onCount = 0;
                    }
                }
            }
        }

        bool checkWin()
        {
            bool allDone = true;
            for (int x = 0; x <= boardSize.x - 1; x++)
            {
                for (int y = 0; y <= boardSize.y - 1; y++)
                {
                    if (!board[x, y].isRevealed && board[x, y].isOn)
                    {
                        allDone = false;
                    }
                }
            }
            return allDone;
        }

        void draw()
        {
            string mistakesStr = "Mistakes: " + mistakes;
            if (hasWon)
            {
                mistakesStr = "You win! " + mistakesStr;
            }
            ConsoleChar[] mistakesChars = ConsoleChar.StrToConsoleCharArr(mistakesStr);
            for (int x = 0; x <= mistakesChars.Length - 1; x++)
            {
                drawingBoard[x, 0] = mistakesChars[x];
            }


            if (board[selectedPos.x, selectedPos.y].isRevealed)
            {
                if (board[selectedPos.x, selectedPos.y].isOn)
                {
                    changeCol(selectedPos, 15, 10);
                } else
                {
                    changeCol(selectedPos, 15, 6);
                }
            } else
            {
                changeCol(selectedPos, 0, 14);
            }
            
            if (board[previousSelectedPos.x, previousSelectedPos.y].isRevealed)
            {
                if (board[previousSelectedPos.x, previousSelectedPos.y].isOn)
                {
                    changeCol(previousSelectedPos, 15, 9);
                }
                else
                {
                    changeCol(previousSelectedPos, 15, 8);
                }
            }
            else
            {
                changeCol(previousSelectedPos, 15, 0);
            }
            Drawing.draw(drawingBoard);
        }

        void drawInit()
        {
            for (int y = 0; y <= drawingBoard.GetLength(1) - 1; y++)
            {
                for (int x = 0; x <= drawingBoard.GetLength(0) - 1; x++)
                {
                    drawingBoard[x, y] = new ConsoleChar(' ');
                }
            }

            ConsoleChar[] mistakesChars = ConsoleChar.StrToConsoleCharArr(("Mistakes: " + mistakes).PadRight(drawingBoard.GetLength(0)));
            for (int x = 0; x <= mistakesChars.Length - 1; x++) 
            {
                drawingBoard[x, 0] = mistakesChars[x];
            }

            for (int y = 0; y <= boardSize.y - 1; y++)
            {
                for (int x = 0; x <= boardSize.x - 1; x++)
                {
                    drawingBoard[x * 3, y + 1] = new ConsoleChar('[');
                    drawingBoard[x * 3 + 1, y + 1] = new ConsoleChar(']');
                    drawingBoard[x * 3 + 2, y + 1] = new ConsoleChar(' ');
                }
            }

            ConsoleChar[,] sideLabelChars = ConsoleChar.StrToConsoleCharArr(sideLabels, ' ');
            for (int y = 0; y <= sideLabelChars.GetLength(1) - 1; y++)
            {
                for (int x = 0; x <= sideLabelChars.GetLength(0) - 1; x++)
                {
                    drawingBoard[3 * boardSize.x + x, y + 1] = sideLabelChars[x, y];
                }
            }
            
            for (int x = 0; x <= bottomLabels.Length - 1; x++)
            {
                for (int y = 0; y <= bottomLabels[x].Count - 1; y++)
                {
                    for (int i = 0; i <= 2; i++)
                    {
                        string str = bottomLabels[x][y].ToString();
                        if (i <= str.Length - 1)
                        {
                            drawingBoard[x * 3, boardSize.y + 1 + y].character = str[i];
                        }
                    }
                }
            }

            Drawing.draw(drawingBoard);
        }

        bool checkBounds(Point pos)
        {
            bool r = true;

            if (pos.x > boardSize.x - 1 || pos.x < 0) { r = false; }
            if (pos.y > boardSize.y - 1 || pos.y < 0) { r = false; }

            return r;
        }

        void changeCol(Point pos, int foreCol, int backCol)
        {
            for (int i = 0; i <= 1; i++)
            {
                drawingBoard[pos.x * 3 + i, pos.y + 1].foreColour = foreCol;
                drawingBoard[pos.x * 3 + i, pos.y + 1].backColour = backCol;
            }
        }

        struct Tile
        {
            public bool isOn; // true, originally hidden value
            public bool isRevealed;

            public Tile(bool isOn_)
            {
                isOn = isOn_;
                isRevealed = false;
            }

            public void Reveal(bool isInputOn)
            {
                if (!isRevealed)
                {
                    if (isInputOn != isOn)
                    {
                        mistakes++;
                    }
                    isRevealed = true;
                }
            }
        }

    }
}