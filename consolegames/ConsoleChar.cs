using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace consolegames
{
    class ConsoleChar
    {

        public char character;
        public int foreColour; // 0 - 15: https://docs.microsoft.com/en-us/dotnet/api/system.consolecolor?view=netframework-4.8#fields
        public int backColour;

        public ConsoleChar()
        {
            character = ' ';
            foreColour = 15;
            backColour = 0;
        }
        public ConsoleChar(char character_)
        {
            character = character_;
            foreColour = 15;
            backColour = 0;
        }
        public ConsoleChar(char character_, int foreColour_, int backColour_)
        {
            character = character_;
            foreColour = foreColour_;
            backColour = backColour_;
        }


        public static Drawing.CharInfo ConsoleCharToCharInfo(ConsoleChar consoleChar)
        {
            Drawing.CharInfo r = new Drawing.CharInfo();
            r.Attributes = (short)(consoleChar.backColour * 16 + consoleChar.foreColour);
            r.Char.UnicodeChar = consoleChar.character;

            byte asciiChar = Encoding.Unicode.GetBytes(new char[] { consoleChar.character })[0];
            if (consoleChar.character == '┌')       asciiChar = 218; //http://www.softwareandfinance.com/CSharp/PrintASCII.html encoding problem - probably a conversion function but i couldn't find it
            else if (consoleChar.character == '┐')  asciiChar = 191; 
            else if (consoleChar.character == '└')  asciiChar = 192; 
            else if (consoleChar.character == '┘')  asciiChar = 217; 
            else if (consoleChar.character == '─')  asciiChar = 196; 
            else if (consoleChar.character == '│')  asciiChar = 179; 


            else if (consoleChar.character == '░')  asciiChar = 176; 
            else if (consoleChar.character == '▒')  asciiChar = 177; 
            else if (consoleChar.character == '▓')  asciiChar = 178; 

            else if (consoleChar.character == '█')  asciiChar = 219; 
            else if (consoleChar.character == '▄')  asciiChar = 220; 
            else if (consoleChar.character == '▌')  asciiChar = 221; 
            else if (consoleChar.character == '▐')  asciiChar = 222; 
            else if (consoleChar.character == '▀')  asciiChar = 223; 

            else if (consoleChar.character == '♥')  asciiChar = 3;
            else if (consoleChar.character == '♦')  asciiChar = 4;
            else if (consoleChar.character == '♣')  asciiChar = 5;
            else if (consoleChar.character == '♠')  asciiChar = 6;
            r.Char.AsciiChar = asciiChar;
            
            return r;
        }
        public static Drawing.CharInfo[] ConsoleCharToCharInfo(ConsoleChar[] consoleChars)
        {
            Drawing.CharInfo[] r = new Drawing.CharInfo[consoleChars.Length];
            for (int i = 0; i <= consoleChars.Length - 1; i++)
            {
                r[i] = ConsoleCharToCharInfo(consoleChars[i]);
            }
            return r;
        }

        public static ConsoleChar[] StrToConsoleCharArr(string str)
        {
            return StrToConsoleCharArr(str, 15, 0);
        }
        public static ConsoleChar[,] StrToConsoleCharArr(string[] stringArray, char spaceChar)
        {
            return StrToConsoleCharArr(stringArray, spaceChar, 15, 0);
        }
        public static ConsoleChar[] StrToConsoleCharArr(string str, int foreColour, int backColour)
        {
            ConsoleChar[] r = new ConsoleChar[str.Length];
            for (int i = 0; i <= str.Length - 1; i++)
            {
                r[i] = new ConsoleChar(str[i], foreColour, backColour);
            }
            return r;
        }
        public static ConsoleChar[,] StrToConsoleCharArr(string[] stringArray, char spaceChar, int foreColour, int backColour)
        {
            int maxLength = stringArray.Max(s => s.Length);
            ConsoleChar[,] r = new ConsoleChar[maxLength, stringArray.Length];

            for (int y = 0; y <= stringArray.Length - 1; y++)
            {
                for (int x = 0; x <= maxLength - 1; x++)
                {
                    r[x, y] = new ConsoleChar();
                    r[x, y].foreColour = foreColour;
                    r[x, y].backColour = backColour;
                    if (x <= stringArray[y].Length - 1)
                    {
                        r[x, y].character = stringArray[y][x];
                    }
                    else
                    {
                        r[x, y].character = spaceChar;
                    }
                }
            }

            return r;
        }

        public static void CopyImage(ref ConsoleChar[,] dest, ref ConsoleChar[,] source, int destX, int destY)
        {
            for (int x = 0; x < source.GetLength(0); x++)
                for (int y = 0; y < source.GetLength(1); y++)
                    dest[destX + x, destY + y] = new ConsoleChar(source[x, y].character, source[x, y].foreColour, source[x, y].backColour);
        }
        public static void CopyImageCrop(ref ConsoleChar[,] dest, ref ConsoleChar[,] source, int destX, int destY) // copies image but allows it to go off the edge of dest
        {
            for (int x = 0; x < source.GetLength(0); x++)
                for (int y = 0; y < source.GetLength(1); y++)
                {
                    int rx = destX + x;
                    int ry = destY + y;
                    if (rx > 0  && ry > 0 && rx < dest.GetLength(0) && ry < dest.GetLength(1))
                        dest[rx, ry] = new ConsoleChar(source[x, y].character, source[x, y].foreColour, source[x, y].backColour);
                }
        }


        public static ConsoleChar[] Arr2DTo1D(ConsoleChar[,] Arr2D)
        {
            ConsoleChar[] Arr1D = new ConsoleChar[Arr2D.GetLength(0) * Arr2D.GetLength(1)];
            int i = 0;
            for (int y = 0; y <= Arr2D.GetLength(1) - 1; y++)
            {
                for (int x = 0; x <= Arr2D.GetLength(0) - 1; x++)
                {
                    Arr1D[i++] = Arr2D[x, y];
                }
            }
            return Arr1D;
        }

        public static ConsoleChar[,] Flip2DArrUpDown(ConsoleChar[,] arr)
        {
            ConsoleChar[,] r = new ConsoleChar[arr.GetLength(0), arr.GetLength(1)];
            for (int x = 0; x <= arr.GetLength(0) - 1; x++)
            {
                for (int y = 0; y <= arr.GetLength(1) - 1; y++)
                {
                    r[x, y] = arr[x, arr.GetLength(1) - 1 - y];
                }
            }
            return r;
        }

        public static ConsoleChar[,] DoubleWidth2DArr(ConsoleChar[,] arr)
        {
            ConsoleChar[,] r = new ConsoleChar[arr.GetLength(0) * 2, arr.GetLength(1)];
            for (int x = 0; x <= arr.GetLength(0) - 1; x++)
            {
                for (int y = 0; y <= arr.GetLength(1) - 1; y++)
                {
                    r[x * 2, y] = arr[x, y];
                    r[x * 2 + 1, y] = arr[x, y];
                }
            }
            return r;
        }

        public static ConsoleChar[,] InvertColours(ConsoleChar[,] arr)
        {
            ConsoleChar[,] r = new ConsoleChar[arr.GetLength(0), arr.GetLength(1)];
            for (int x = 0; x < arr.GetLength(0); x++)
            {
                for (int y = 0; y < arr.GetLength(1); y++)
                {
                    r[x, y] = arr[x, y];
                    r[x, y].backColour = 15 - r[x, y].backColour;
                    r[x, y].foreColour = 15 - r[x, y].foreColour;
                    //int[] colours = new int[] { r[x, y].foreColour, r[x, y].backColour };
                    //for (int i = 0; i < colours.Length; i++)
                    //{
                    //    if (colours[i] == (int)ConsoleColor.Black) { colours[i] = (int)ConsoleColor.White; }
                    //    else if (colours[i] == (int)ConsoleColor.DarkBlue) { colours[i] = (int)ConsoleColor.Yellow; }
                    //    else if (colours[i] == (int)ConsoleColor.DarkGreen) { colours[i] = (int)ConsoleColor.Magenta; }
                    //    else if (colours[i] == (int)ConsoleColor.DarkRed) { colours[i] = (int)ConsoleColor.Magenta; }
                    //}
                }
            }
            return r;
        }
    }

}
