using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace consolegames
{
    class Program
    {
        static bool showHelp = false;
        static bool chooseGameParameters = false;

        static void Main(string[] args) // To do: snake gets very slow on high timer speed and after eating a lot - probable memory leak
        {
            //ConsoleChar[,] image = new ConsoleChar[16, 2];
            //for (int x = 0; x < image.GetLength(0); x++)
            //{
            //    image[x, 0] = new ConsoleChar(' ', 15, x);
            //    image[x, 1] = new ConsoleChar(' ', 15, x);
            //}
            //Drawing.draw(ConsoleChar.DoubleWidth2DArr(image));
            //Console.ReadKey();

            //Drawing.CharInfo[] buffer = new Drawing.CharInfo[256];
            //for (int i = 0; i < 256; i++)
            //{
            //    buffer[i] = new Drawing.CharInfo();
            //    buffer[i].Attributes = (short)(15 * 16 + 0);
            //    buffer[i].Char.AsciiChar = (byte)i;
            //    buffer[i].Char.UnicodeChar = (char)i;

            //}
            //Drawing.draw(buffer);
            //Console.ReadKey();


            Menu();
        }

        static void Menu()
        {
            //Maximize();
            //ConsoleSolitaire solitaire = new ConsoleSolitaire();
            //solitaire.run();


            //return;
            Console.Clear();
            Console.WriteLine(" -- Console games by Eddie Smith 2019 -- ");
            Console.WriteLine();

            Console.WriteLine("0 - Options");
            Console.WriteLine("1 - 2048");
            Console.WriteLine("2 - Picross");
            Console.WriteLine("3 - Snake");
            Console.WriteLine("4 - Tetris");
            Console.WriteLine("5 - Chess");
            Console.WriteLine("6 - Solitaire");
            Console.WriteLine("E - exit");
            string input = Console.ReadLine();
            Console.Clear();


            if (input == "0")
            {
                OptionsMenu();
            }
            else if (input == "1")
            {              
                Console2048 _2048 = new Console2048();
                _2048.run(showHelp, chooseGameParameters);
            }
            else if (input == "2")
            {
                ConsolePicross picross = new ConsolePicross();
                picross.run(showHelp, chooseGameParameters);
            }
            else if (input == "3")
            {
                ConsoleSnake snake = new ConsoleSnake();
                snake.run(showHelp, chooseGameParameters);
            }
            else if (input == "4")
            {
                ConsoleTetris tetris = new ConsoleTetris();
                tetris.run(showHelp, chooseGameParameters);
                tetris = null;
            }
            else if (input == "5")
            {
                Maximize();
                ConsoleChess chess = new ConsoleChess();
                chess.run();
                chess = null;
            } else if (input == "6")
            {
                Maximize();
                ConsoleSolitaire solitaire = new ConsoleSolitaire();
                solitaire.run(showHelp, chooseGameParameters);
            }
            if (input.ToLower() != "e")
            {
                Menu();
            }
        }

        static void OptionsMenu()
        {
            Console.WriteLine("Options");
            Console.WriteLine("1 - toggle show help on game start. Currently " + (showHelp ? "on." : "off."));
            Console.WriteLine("2 - toggle choosing game parameters. Currently " + (chooseGameParameters ? "on." : "off.") + " May result in crashes.");
            string input = Console.ReadLine();
            if (input.Contains('1'))
            {
                showHelp = !showHelp;
            } else if (input.Contains('2'))
            {
                chooseGameParameters = !chooseGameParameters;
            }
            Menu();
        }

        public static string GetStringInput(string prompt, string[] possibleAnswers)
        {
            string r = "";
            bool shouldLoop = true;
            do
            {
                Console.Write(prompt);
                r = Console.ReadLine().ToLower();
                if (possibleAnswers.Contains(r))
                {
                    shouldLoop = false;
                }
            } while (shouldLoop);
            return r;
        }
        public static int GetIntInput(string prompt, int max, int min) // inclusive lower and upper
        {
            int r = int.MinValue;
            bool shouldLoop = true;
            do
            {
                Console.Clear();
                Console.Write(prompt);
                string input = Console.ReadLine();
                shouldLoop = !int.TryParse(input, out r);
                if (r > max || r < min)
                {
                    shouldLoop = true;
                }
            } while (shouldLoop);
            return r;
        }
        public static bool GetBoolInput(string prompt)
        {
            bool r = false;
            bool shouldLoop = true;
            do
            {
                Console.Clear();
                Console.Write(prompt);
                string input = Console.ReadLine().ToLower();
                shouldLoop = false;
                if (input == "y" || input == "yes")
                {
                    r = true;
                }
                else if (input == "n" || input == "no")
                {
                    r = false;
                } else
                {
                    shouldLoop = true;
                }
            } while (shouldLoop);
            return r;
        }
               
        [DllImport("user32.dll")]
        public static extern bool ShowWindow(System.IntPtr hWnd, int cmdShow);
        private static void Maximize() //https://stackoverflow.com/a/22053200
        {
            Process p = Process.GetCurrentProcess();
            ShowWindow(p.MainWindowHandle, 3); //SW_MAXIMIZE = 3
        }
    }

    struct Point
    {
        public int x;
        public int y;

        public Point(int x_, int y_)
        {
            x = x_;
            y = y_;
        }

        public static bool operator ==(Point a, Point b)
        {
            return a.x == b.x && a.y == b.y;
        }
        public static bool operator !=(Point a, Point b)
        {
            return a.x != b.x || a.y != b.y;
        }
        public static Point operator +(Point a, Point b)
        {
            return new Point(a.x + b.x, a.y + b.y);
        }
        public static Point operator -(Point a, Point b)
        {
            return new Point(a.x - b.x, a.y - b.y);
        }
    }

    struct Vector2
    {
        public float x;
        public float y;

        public Vector2(float x_, float y_)
        {
            x = x_;
            y = y_;
        }

        public static bool operator ==(Vector2 a, Vector2 b)
        {
            return a.x == b.x && a.y == b.y;
        }
        public static bool operator !=(Vector2 a, Vector2 b)
        {
            return a.x != b.x || a.y != b.y;
        }
        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x + b.x, a.y + b.y);
        }
        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.x - b.x, a.y - b.y);
        }
    }
}
