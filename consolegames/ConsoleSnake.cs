using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace consolegames
{
    class ConsoleSnake
    {
        Timer timer = new Timer();
        int boardSize = 20;
        List<Point> snakeTiles; // snakeTiles[0] contains the head
        Point dir = new Point(0, 1);
        bool canInput = true;
        bool isTorus = true;
        bool shouldLoop = true;
        Point fruitPos;
        int score = 0;
        bool hasLost = false;
        bool shouldChangeFruit = false;
        
        public void run(bool showHelp, bool chooseGameParameters)
        {
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
            timer.Interval = 100;

            if (showHelp)
            {
                Console.WriteLine("Use the arrow keys to turn the snake.");
                Console.WriteLine("Move the snake to the fruit.");
                Console.WriteLine("Eat the fruit to grow the snake.");
                Console.WriteLine("Try to get as large as positive.");
                Console.ReadLine();
                Console.Clear();
            }
            if (chooseGameParameters)
            {
                timer.Interval = Program.GetIntInput("Timer speed (ms) (default: 100): ", int.MaxValue, 1);
                boardSize = Program.GetIntInput("Board size (default: 20): ", int.MaxValue, 8);
                isTorus = Program.GetBoolInput("Is the board a torus (loop off the edge of the board) (y/n) (default: y): ");
                Console.Clear();
            }

            Console.CursorVisible = false;

            snakeTiles = new List<Point>();
            for (int y = 6; y >= 0; y--)
            {
                snakeTiles.Add(new Point(1, y + 1));
            }

            changeFruitPos();

            timer.Enabled = true;
            do
            {
                Point newDir = new Point();
                ConsoleKey input = Console.ReadKey(true).Key;
                if (input == ConsoleKey.DownArrow) { newDir.y = 1; }
                else if (input == ConsoleKey.UpArrow) { newDir.y = -1; }
                else if (input == ConsoleKey.RightArrow) { newDir.x = 1; }
                else if (input == ConsoleKey.LeftArrow) { newDir.x = -1; }
                else if (input == ConsoleKey.Escape)
                {
                    shouldLoop = false;
                }
                if (newDir != new Point() && canInput)
                {
                    if (Math.Abs(newDir.x) != Math.Abs(dir.x) && Math.Abs(newDir.y) != Math.Abs(dir.y))
                    {
                        dir = newDir;
                        canInput = false;
                    } else
                    {
                        canInput = true;
                    }
                } else
                {
                    canInput = true;
                }
            } while (shouldLoop && hasLost == false);
            Console.ReadLine();
        }

        void draw()
        {
            if (shouldChangeFruit)
            {
                changeFruitPos();
                shouldChangeFruit = false;
            }

            string[] lines = new string[boardSize + 3];

            for (int x = 0; x <= boardSize + 1; x++)
            {
                lines[0] += "░░";
                lines[boardSize + 1] += "░░";
            }

            for (int y = 0; y <= boardSize - 1; y++)
            {
                lines[y + 1] += "░░";
                for (int x = 0; x <= boardSize - 1; x++)
                {
                    if (fruitPos == new Point(x, y))
                    {
                        lines[y + 1] += "()";
                    }
                    else if (snakeTiles.Contains(new Point(x, y)))
                    {
                        if (snakeTiles[0] == new Point(x, y))
                        {
                            if (dir.y == 0)
                            {
                                if (dir.x == -1)
                                {
                                    lines[y + 1] += ":[";
                                }
                                else if (dir.x == 1)
                                {
                                    lines[y + 1] += "]:";
                                }
                            }
                            else
                            {
                                lines[y + 1] += "..";
                            }
                        }
                        else
                        {
                            lines[y + 1] += "[]";
                        }
                    } 
                    else
                    {
                        lines[y + 1] += "  ";
                    }
                }
                lines[y + 1] += "░░";
            }

            if (hasLost)
            {
                lines[boardSize + 2] = "You lose! ";    
            }
            lines[boardSize + 2] += "Score: " + score;
            Drawing.draw(ConsoleChar.StrToConsoleCharArr(lines, ' '));
        }

        bool checkBounds(Point p)
        {
            bool r = true;
            if (p.x > boardSize - 1 || p.x < 0) { r = false; }
            if (p.y > boardSize - 1 || p.y < 0) { r = false; }
            return r;
        }

        void changeFruitPos()
        {
            Random r = new Random();

            bool isSnakeTile = false;
            Point newSnakePos;
            do
            {
                newSnakePos = new Point(r.Next(boardSize), r.Next(boardSize));
                for (int snakeIndex = 0; snakeIndex <= snakeTiles.Count - 1; snakeIndex++) 
                {
                    if (snakeTiles[snakeIndex] == newSnakePos)
                    {
                        isSnakeTile = true;
                    }
                }
            } while (isSnakeTile);
            fruitPos = newSnakePos;

        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            canInput = true;
            Point[] oldSnakeTiles = snakeTiles.ToArray();

            Point newHeadPos = new Point(snakeTiles[0].x + dir.x, snakeTiles[0].y + dir.y);
            if (!checkBounds(newHeadPos))
            {
                if (isTorus)
                {
                    if (newHeadPos.x > boardSize - 1) { newHeadPos.x = 0; }
                    else if (newHeadPos.x < 0) { newHeadPos.x = boardSize - 1; }
                    if (newHeadPos.y > boardSize - 1) { newHeadPos.y = 0; }
                    else if (newHeadPos.y < 0) { newHeadPos.y = boardSize - 1; }
                } else
                {
                    shouldLoop = false;
                }
            }

            if (snakeTiles.Contains(newHeadPos))
            {
                shouldLoop = false;
            }

            if (shouldLoop)
            {
                if (newHeadPos == fruitPos)
                {
                    score++;
                    shouldChangeFruit = true;
                }

                snakeTiles[0] = newHeadPos;
                

                for (int i = 1; i <= snakeTiles.Count - 1; i++)
                {
                    snakeTiles[i] = oldSnakeTiles[i - 1];
                }

                if (shouldChangeFruit)
                {
                    snakeTiles.Add(oldSnakeTiles[oldSnakeTiles.Length - 1]);
                    changeFruitPos();
                }

                draw();
            } else
            {
                hasLost = true;
                draw();
                timer.Stop();
            }
        }
    }
}
