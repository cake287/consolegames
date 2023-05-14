using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace consolegames
{
    class Drawing
    {
        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern SafeFileHandle CreateFile(
string fileName,
[MarshalAs(UnmanagedType.U4)] uint fileAccess,
[MarshalAs(UnmanagedType.U4)] uint fileShare,
IntPtr securityAttributes,
[MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
[MarshalAs(UnmanagedType.U4)] int flags,
IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteConsoleOutput(
          SafeFileHandle hConsoleOutput,
          CharInfo[] lpBuffer,
          Coord dwBufferSize,
          Coord dwBufferCoord,
          ref SmallRect lpWriteRegion);

        [StructLayout(LayoutKind.Sequential)]
        public struct Coord
        {
            public short X;
            public short Y;

            public Coord(short X, short Y)
            {
                this.X = X;
                this.Y = Y;
            }
        };

        [StructLayout(LayoutKind.Explicit)]
        public struct CharUnion
        {
            [FieldOffset(0)] public char UnicodeChar;
            [FieldOffset(0)] public byte AsciiChar;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct CharInfo
        {
            [FieldOffset(0)] public CharUnion Char;
            [FieldOffset(2)] public short Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SmallRect
        {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }


        static SafeFileHandle h;
        static bool hasInited = false;

        [STAThread]
        public static void draw(ConsoleChar[,] chars)
        {
            if (!hasInited)
            {
                h = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
                hasInited = true;
            }

            CharInfo[] buf = ConsoleChar.ConsoleCharToCharInfo(ConsoleChar.Arr2DTo1D(chars));
            int width = chars.GetLength(0);
            int height = chars.GetLength(1);

            if (!h.IsInvalid)
            {
                SmallRect rect = new SmallRect() { Left = 0, Top = 0, Right = (short)width, Bottom = (short)height };
                bool b = WriteConsoleOutput(h, buf,
                    new Coord() { X = (short)width, Y = (short)height },
                    new Coord() { X = 0, Y = 0 },
                    ref rect);
            }
        }


        [STAThread]
        public static void draw(CharInfo[] buf) // for testing only
        {
            if (!hasInited)
            {
                h = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
                hasInited = true;
            }


            if (!h.IsInvalid)
            {
                SmallRect rect = new SmallRect() { Left = 0, Top = 0, Right = 100, Bottom = 100 };
                bool b = WriteConsoleOutput(h, buf,
                    new Coord() { X = 100, Y = 100 },
                    new Coord() { X = 0, Y = 0 },
                    ref rect);
            }
        }
    }
}
