using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace consolegames
{
    class ConsoleTetris
    {
        static Point visibleBoardSize = new Point(10, 20);
        static Point realBoardSize;
        static int extraCellWidth = 9; // width (including borders) of hold and next cells
        static ConsoleChar[,] image;
        static Tile[,] board;
        static int timerSpeed = 500; // time between timer calls in milliseconds
        static Point piecePos; // bottom left corner of bounding box of active piece
        static bool timerRunning = true;
        static Timer t;
        static List<int> bag = new List<int>(); // ints reference the shapes[] array; bag contains up to 2 sets of 7 pieces and is refilled at 7 pieces
        static int currentShape = 0;
        static int currentState = 0;
        static int heldShape = -1;
        static bool hasHeld = false; // can only swap hold/active piece once before it is locked
        static bool hardDropping = false;
        static int animationSpeed = 50; // ms between frames in animations (i.e. lines clearing and moving down)
        private static Random rng = new Random();
        static bool displayGhostPiece = true;
        static int comboCleared = 0; // number of lines cleared since last 0-lines clear lock; additional score is multiplied by (1 + comboCleared / 2) before being added to score, excluding the first clear
        static int score = 0;
        static int level = 1; // determines timer speed. increments every 10 lines
        static int lines = 0; // number of lines cleared
        static bool paused = false;
        static bool shouldDelayLock = true;
        static bool delayingLock = false;
        static bool skipLockDelay = false; // when lockDelayTimer has ticked but the piece has been rotated/translated so that it's no longer obstructed, lockDelay is skipped when it reaches the ground, as to prevent 'infinity' (https://tetris.fandom.com/wiki/Infinity)
        static Timer lockDelayT;
        static int transformsDuringLockDelay = 0; // number of rotations or translations during lock delay for this piece (timer is reset on each one; at 16 the lock delay ends; multiple lock delays are possible if the timer does not run out)

        // shapes[0] = I;  1 = J;  2 = L;  3 = O;  4 = S;  5 = T;  6 = Z
        static Shape[] shapes = new Shape[]
        {
            new Shape( // I
                (int)ConsoleColor.Cyan,
                new Point(visibleBoardSize.x / 2 - 2, visibleBoardSize.y - 3),
                new string[][] 
                { 
                    new string[] {
                    "----",
                    "0000",
                    "----",
                    "----"
                    },
                    new string[] {
                    "--0-",
                    "--0-",
                    "--0-",
                    "--0-"
                    },
                    new string[] {
                    "----",
                    "----",
                    "0000",
                    "----"
                    },
                    new string[] {
                    "-0--",
                    "-0--",
                    "-0--",
                    "-0--"
                    },
                }
            ),
            new Shape( // J
                (int)ConsoleColor.Blue,
                new Point(visibleBoardSize.x / 2 - 2, visibleBoardSize.y - 2),
                new string[][]
                {
                    new string[] {
                    "0--",
                    "000",
                    "---",
                    },
                    new string[] {
                    "-00",
                    "-0-",
                    "-0-",
                    },
                    new string[] {
                    "---",
                    "000",
                    "--0",
                    },
                    new string[] {
                    "-0-",
                    "-0-",
                    "00-",
                    },
                }
            ),
            new Shape( // L
                (int)ConsoleColor.DarkYellow,
                new Point(visibleBoardSize.x / 2 - 2, visibleBoardSize.y - 2),
                new string[][]
                {
                    new string[] {
                    "--0",
                    "000",
                    "---",
                    },
                    new string[] {
                    "-0-",
                    "-0-",
                    "-00",
                    },
                    new string[] {
                    "---",
                    "000",
                    "0--",
                    },
                    new string[] {
                    "00-",
                    "-0-",
                    "-0-",
                    },
                }
            ),
            new Shape( // O
                (int)ConsoleColor.Yellow,
                new Point(visibleBoardSize.x / 2 - 2, visibleBoardSize.y - 3),
                new string[][]
                {
                    new string[] {
                    "-00-",
                    "-00-",
                    "----",
                    "----",
                    },
                    new string[] {
                    "-00-",
                    "-00-",
                    "----",
                    "----",
                    },
                    new string[] {
                    "-00-",
                    "-00-",
                    "----",
                    "----",
                    },
                    new string[] {
                    "-00-",
                    "-00-",
                    "----",
                    "----",
                    },
                }
            ),
            new Shape( // S
                (int)ConsoleColor.Green,
                new Point(visibleBoardSize.x / 2 - 2, visibleBoardSize.y - 2),
                new string[][]
                {
                    new string[] {
                    "-00",
                    "00-",
                    "---",
                    },
                    new string[] {
                    "-0-",
                    "-00",
                    "--0",
                    },
                    new string[] {
                    "---",
                    "-00",
                    "00-",
                    },
                    new string[] {
                    "0--",
                    "00-",
                    "-0-",
                    },
                }
            ),
            new Shape( // T
                (int)ConsoleColor.Magenta,
                new Point(visibleBoardSize.x / 2 - 2, visibleBoardSize.y - 2),
                new string[][]
                {
                    new string[] {
                    "-0-",
                    "000",
                    "---",
                    },
                    new string[] {
                    "-0-",
                    "-00",
                    "-0-",
                    },
                    new string[] {
                    "---",
                    "000",
                    "-0-",
                    },
                    new string[] {
                    "-0-",
                    "00-",
                    "-0-",
                    },
                }
            ),
            new Shape( // Z
                (int)ConsoleColor.Red,
                new Point(visibleBoardSize.x / 2 - 1, visibleBoardSize.y - 2),
                new string[][]
                {
                    new string[] {
                    "00-",
                    "-00",
                    "---",
                    },
                    new string[] {
                    "--0",
                    "-00",
                    "-0-",
                    },
                    new string[] {
                    "---",
                    "00-",
                    "-00",
                    },
                    new string[] {
                    "-0-",
                    "00-",
                    "0--",
                    },
                }
            ),
        };

        // see https://tetris.fandom.com/wiki/SRS#Wall_Kicks
        static Point[][][] IKickTestTransforms = // [clockwise or anticlockwise][starting state][test]
        {
            new Point[][] // clockwise
            {
                new Point[] // starting state 0, target state 1
                {
                    new Point(),
                    new Point(-2, 0),
                    new Point( 1, 0),
                    new Point(-2, -1),
                    new Point( 1, 2),
                },
                new Point[] // starting state 1, target state 2
                {
                    new Point(),
                    new Point(-1, 0),
                    new Point( 2, 0),
                    new Point(-1, 2),
                    new Point( 2,-1),
                },
                new Point[] // starting state 2, target state 3
                {
                    new Point(),
                    new Point( 2, 0),
                    new Point(-1, 0),
                    new Point( 2, 1),
                    new Point(-1,-2),
                },
                new Point[] // starting state 3, target state 0
                {
                    new Point(),
                    new Point( 1, 0),
                    new Point(-2, 0),
                    new Point( 1,-2),
                    new Point(-2, 1),
                },
            },
            new Point[][] // anticlockwise
            {
                new Point[] // starting state 0, target state 3
                {
                    new Point(),
                    new Point(-1, 0),
                    new Point( 2, 0),
                    new Point(-1, 2),
                    new Point( 2,-1),
                },
                new Point[] // starting state 1, target state 0
                {
                    new Point(),
                    new Point( 2, 0),
                    new Point(-1, 0),
                    new Point( 2, 1),
                    new Point(-1,-2),
                },
                new Point[] // starting state 2, target state 1
                {
                    new Point(),
                    new Point( 1, 0),
                    new Point(-2, 0),
                    new Point( 1,-2),
                    new Point(-2, 1),
                },
                new Point[] // starting state 3, target state 2
                {
                    new Point(),
                    new Point(-2, 0),
                    new Point( 1, 0),
                    new Point(-2,-1),
                    new Point( 1, 2),
                },
            }
        };
        static Point[][][] OtherKickTestTransforms = // for J, L, S, T, Z pieces
        {
            new Point[][] // clockwise
            {
                new Point[] // starting state 0, target state 1
                {
                    new Point(),
                    new Point(-1, 0),
                    new Point(-1, 1),
                    new Point( 0,-2),
                    new Point(-1,-2),
                },
                new Point[] // starting state 1, target state 2
                {
                    new Point(),
                    new Point( 1, 0),
                    new Point( 1,-1),
                    new Point( 0, 2),
                    new Point( 1, 2),
                },
                new Point[] // starting state 2, target state 3
                {
                    new Point(),
                    new Point( 1, 0),
                    new Point( 1, 1),
                    new Point( 0,-2),
                    new Point( 1,-2),
                },
                new Point[] // starting state 3, target state 0
                {
                    new Point(),
                    new Point(-1, 0),
                    new Point(-1,-1),
                    new Point( 0, 2),
                    new Point(-1, 2),
                },
            },
            new Point[][] // anticlockwise
            {
                new Point[] // starting state 0, target state 3
                {
                    new Point(),
                    new Point( 1, 0),
                    new Point( 1, 1),
                    new Point( 0,-2),
                    new Point( 1,-2),
                },
                new Point[] // starting state 1, target state 0
                {
                    new Point(),
                    new Point( 1, 0),
                    new Point( 1,-1),
                    new Point( 0, 2),
                    new Point( 1, 2),
                },
                new Point[] // starting state 2, target state 1
                {
                    new Point(),
                    new Point(-1, 0),
                    new Point(-1, 1),
                    new Point( 0,-2),
                    new Point(-1,-2),
                },
                new Point[] // starting state 3, target state 2
                {
                    new Point(),
                    new Point(-1, 0),
                    new Point(-1,-1),
                    new Point( 0, 2),
                    new Point(-1, 2),
                },
            }
        };

        public void run(bool showHelp, bool chooseGameParameters)
        {
            if (showHelp)
            {
                Console.WriteLine("Move and rotate the tetromino to fill rows of the board. Full rows are cleared.");
                Console.WriteLine("Points are awarded for cleared rows (+ extra for successive cleared rows), hard drops and soft drops.");
                Console.WriteLine("Level up by clearing 10 rows.");
                Console.WriteLine("Tetrominos move faster the higher level you are at.");
                Console.WriteLine();
                Console.WriteLine("Left/right arrow keys - move tetromino");
                Console.WriteLine("Up arrow/z keys - rotate tetromino");
                Console.WriteLine("Down arrow key - soft drop");
                Console.WriteLine("Space key - hard drop");
                Console.WriteLine("C key - hold the tetromino (only possible once until another tetromino spawns)");
                Console.WriteLine("P - pause");
                Console.WriteLine("Escape - end game");
                Console.ReadLine();
                Console.Clear();
            }

            if (chooseGameParameters)
            {
                visibleBoardSize.x = Program.GetIntInput("Board width (default: 10) : ", 1000, 1);
                visibleBoardSize.y = Program.GetIntInput("Board height (default: 20) : ", 1000, 1);
                displayGhostPiece = Program.GetBoolInput("Display ghost piece? (y/n) (default y) : ");
                shouldDelayLock = Program.GetBoolInput("Delay lock? (y/n) (default y) : ");
                Console.Clear();
            }

            Console.CursorVisible = false;

            realBoardSize = new Point(visibleBoardSize.x, visibleBoardSize.y + 4);
            image = new ConsoleChar[visibleBoardSize.x + 2 + 2 * extraCellWidth, visibleBoardSize.y + 2];
            board = new Tile[realBoardSize.x, realBoardSize.y];

            for (int x = 0; x < image.GetLength(0); x++)
            {
                for (int y = 0; y < image.GetLength(1); y++)
                {
                    image[x, y] = new ConsoleChar();
                }
            }
            drawBorders();
            
            for (int x = 0; x < realBoardSize.x; x++)
            {
                for (int y = 0; y < realBoardSize.y; y++)
                {
                    board[x, y] = new Tile();
                }
            }

            RefillBag();
            SpawnPiece();
            UpdateImage();
            
            t = new Timer(TimerCallback, null, 0, timerSpeed);
            lockDelayT = new Timer(LockDelayTimerCallback, null, Timeout.Infinite, Timeout.Infinite);
            
            while (timerRunning)
            {
                ConsoleKey input = Console.ReadKey(true).Key;
                if (BoardContainsActivePiece())
                {
                    if (input == ConsoleKey.P)
                    {
                        paused = !paused;
                        if (paused) { t.Change(Timeout.Infinite, Timeout.Infinite); }
                        else { t.Change(timerSpeed, timerSpeed); }
                    } else if (input == ConsoleKey.Escape)
                    {
                        paused = true;
                        GameOver();
                    }
                    if (!paused)
                    {
                        bool hasPieceBeenTransformed = false;
                        if (input == ConsoleKey.RightArrow || input == ConsoleKey.D) { TranslatePiece(1); hasPieceBeenTransformed = true; }
                        else if (input == ConsoleKey.LeftArrow || input == ConsoleKey.A) { TranslatePiece(-1); hasPieceBeenTransformed = true; }
                        else if (input == ConsoleKey.UpArrow || input == ConsoleKey.W) { RotatePiece(1); hasPieceBeenTransformed = true; }
                        else if (input == ConsoleKey.Z) { RotatePiece(-1); hasPieceBeenTransformed = true; }
                        else if (input == ConsoleKey.DownArrow || input == ConsoleKey.S)
                        {
                            ApplyGravity();
                            if (BoardContainsActivePiece()) { score++; }
                        }
                        else if (input == ConsoleKey.Spacebar)
                        {
                            hardDropping = true;
                            ApplyGravity();
                        }
                        else if (input == ConsoleKey.C) { Hold(); }

                        if (hasPieceBeenTransformed && delayingLock)
                        {
                            transformsDuringLockDelay++;
                            if (transformsDuringLockDelay >= 16)
                            {
                                lockDelayT.Change(0, Timeout.Infinite);
                            }
                            else
                            {
                                lockDelayT.Change(timerSpeed, Timeout.Infinite);
                            }
                        }
                    }
                    UpdateImage();
                }
            }
            t.Dispose();
            Console.ReadLine();
        }

        private static void TimerCallback(object o) // https://stackoverflow.com/a/7865126 timer code
        {
            if (timerRunning && BoardContainsActivePiece())
            {
                //Debug.WriteLine("In TimerCallback: " + DateTime.Now);
                ApplyGravity();
                UpdateImage();
            }
            //else
            //{
            //    Debug.WriteLine("Timer called but not runnning " + timerRunning + " " + BoardContainsActivePiece());
            //}
        }

        private static void LockDelayTimerCallback(object o)
        {
            if (!CanPieceMoveDown())
            {
                LockPiece();
            } else
            {
                delayingLock = false;
                skipLockDelay = true;
            }
        }

        static void ApplyGravity()
        {            
            if (CanPieceMoveDown())
            {
                for (int x = 0; x < realBoardSize.x; x++)
                {
                    for (int y = 1; y < realBoardSize.y; y++) // tiles at y = 0 are not moved down, so loop starts at y = 1
                    {
                        if (board[x, y].hasPiece && !board[x, y].locked)
                        {
                            board[x, y - 1].Set(shapes[currentShape].colour);
                            board[x, y].Unset();
                        }
                    }
                }
                piecePos.y--;
                if (!CanPieceMoveDown() && !skipLockDelay && shouldDelayLock) // set delayingLock as true so that the piece is displayed as semi-transparent as soon as it touches the floor
                {
                    delayingLock = true;
                }
                if (hardDropping)
                {
                    score += 2;
                    ApplyGravity();
                }
            } else
            {
                if (!skipLockDelay && !hardDropping && shouldDelayLock)
                {
                    delayingLock = true;
                    lockDelayT.Change(timerSpeed, Timeout.Infinite);
                } else
                {
                    LockPiece();
                }
            }
        }

        static bool CanPieceMoveDown()
        {
            bool canMoveDown = true;
            for (int x = 0; x < realBoardSize.x; x++)
            {
                for (int y = 0; y < realBoardSize.y; y++)
                {
                    if (board[x, y].hasPiece && !board[x, y].locked) // tile contains moving tetromino piece
                    {
                        if (y == 0) // the tetramino has reached the bottom of the board
                        {
                            canMoveDown = false;
                        }
                        else if (board[x, y - 1].hasPiece && board[x, y - 1].locked) // the tetramino is blocked by a locked tile
                        {
                            canMoveDown = false;
                        }
                    }
                }
            }
            return canMoveDown;
        }

        static void LockPiece()
        {
            for (int x = 0; x < realBoardSize.x; x++)
            {
                for (int y = 0; y < realBoardSize.y; y++)
                {
                    if (board[x, y].hasPiece && !board[x, y].locked)
                    {
                        board[x, y].locked = true;
                    }
                }
            }
            transformsDuringLockDelay = 0;
            delayingLock = false;
            skipLockDelay = false;
            hardDropping = false;
            hasHeld = false;
            ClearFullLines();
            UpdateImage();
            SpawnPiece();
        }
        
        static void SpawnPiece()
        {
            if (bag.Count <= 7) // bag needs to be refilled (done at 7 pieces instead of 0 so that there are always 3 pieces to be displayed as 'next')
            {
                RefillBag();
            }
            SpawnPiece(bag[0]);
            bag.RemoveAt(0);
        }

        static void SpawnPiece(int shape)
        {
            currentShape = shape;
            currentState = 0;

            piecePos = shapes[currentShape].startingPos;

            // check if the new piece has space to spawn in
            bool canSpawn = true;
            for (int x = 0; x < shapes[currentShape].boundingSize.x; x++) 
            {
                for (int y = 0; y < shapes[currentShape].boundingSize.y; y++)
                {
                    if (shapes[currentShape].states[currentState][x, y] && board[piecePos.x + x, piecePos.y + y].hasPiece && board[piecePos.x + x, piecePos.y + y].locked) // if there is a piece where the new piece spawns in
                    {
                        canSpawn = false;                        
                    }
                }
            }
            if (canSpawn)
            {
                for (int x = 0; x < shapes[currentShape].boundingSize.x; x++)
                {
                    for (int y = 0; y < shapes[currentShape].boundingSize.y; y++)
                    {
                        if (shapes[currentShape].states[currentState][x, y])
                        {
                            board[piecePos.x + x, piecePos.y + y].Set(shapes[currentShape].colour);
                        }
                    }
                }
            } else
            {
                GameOver();
            }
        }

        static void RefillBag()
        {
            List<int> newBag = new List<int>();
            for (int i = 0; i < shapes.Length; i++)
            {
                newBag.Add(i);
            }
            List<int> shuffledNewBag = newBag.OrderBy(a => rng.Next()).ToList();
            bag.AddRange(shuffledNewBag);
        }

        static void GameOver()
        {
            Debug.WriteLine("Game over");
            timerRunning = false;
        }

        static void ClearFullLines()
        {
            List<int> clearedLines = new List<int>(); // y value of cleared lines
            for (int y = 0; y < realBoardSize.y; y++)
            {
                bool fullLine = true;
                for (int x = 0; x < realBoardSize.x; x++)
                {
                    if (!(board[x, y].hasPiece && board[x, y].locked))
                    {
                        fullLine = false;
                    }
                }
                if (fullLine)
                {
                    clearedLines.Add(y);
                }
            }
            if (clearedLines.Count != 0)
            {
                t.Change(Timeout.Infinite, Timeout.Infinite); // pause timer
                //timerRunning = false;

                // clear lines from the centre in an animation
                for (int x = 0; x < realBoardSize.x / 2; x++)
                {
                    foreach (int y in clearedLines)
                    {
                        board[realBoardSize.x / 2 + x, y].Unset();
                        board[(realBoardSize.x - 1) / 2 - x, y].Unset();
                    }
                    UpdateImage();
                    Thread.Sleep(animationSpeed);
                }
                if (realBoardSize.x % 2 != 0) // if the board width is odd, the two edge columns are not cleared by previous ode
                {
                    foreach (int y in clearedLines)
                    {
                        board[0, y].Unset();
                        board[realBoardSize.x - 1, y].Unset();
                    }
                    UpdateImage();
                    Thread.Sleep(animationSpeed);
                }

                // move other lines down to fill space
                for (int i = clearedLines.Count - 1; i >= 0; i--)
                {
                    //Debug.Write(clearedLines[i] + " ");
                    for (int y = clearedLines[i] + 1; y < realBoardSize.y; y++) // move all lines above this cleared line down 1
                    {
                        for (int x = 0; x < realBoardSize.x; x++)
                        {
                            if (board[x, y].hasPiece)
                            {
                                board[x, y - 1].Set(board[x, y].colour);
                                board[x, y].Unset();
                            }
                            if (board[x, y].locked)
                            {
                                board[x, y - 1].locked = true;
                            }
                        }
                    }
                    UpdateImage();
                    Thread.Sleep(animationSpeed);
                }

                lines += clearedLines.Count;
                level = (int)Math.Floor(lines / 10m + 1);
                timerSpeed = Math.Max(500 - (level - 1) * 40, 40); // decreases timer interval by 40ms every level, and stops at 40ms

                int additionalScore = 0;
                if (clearedLines.Count == 1) { additionalScore = 100; }
                else if (clearedLines.Count == 2) { additionalScore = 300; }
                else if (clearedLines.Count == 3) { additionalScore = 500; }
                else if (clearedLines.Count == 4) { additionalScore = 800; }
                additionalScore += comboCleared / 2 * additionalScore;
                score += additionalScore * level;

                comboCleared += clearedLines.Count;


                //timerRunning = true;
                //Debug.WriteLine("timerRunning = " + timerRunning);
                //Thread.Sleep(timerSpeed);
                t.Change(timerSpeed, timerSpeed); // resume timer
            } else
            {
                comboCleared = 0;
            }
        }

        static void RotatePiece(int dir) // dir = 1 is clockwise; -1 is anti-clockwise
        {
            int newState = currentState - dir;
            if (newState == -1) { newState = 3; } // loop round states
            else if (newState == 4) { newState = 0; }

            Point newPiecePos;

            int kickTestIndex = -1; // gets incremented at the beginning of loop
            int clockwise = dir == 1 ? 0 : 1; // clockwise = 0 if dir is clockwise; clockwise = 1 if dir is anticlockwise (for referencing kick transform arrays)
            bool newStateOK = true;
            do
            {
                kickTestIndex++;
                newStateOK = true;
                newPiecePos = new Point(piecePos.x, piecePos.y);  // reset newPiecePos before applying transform; "= new Point(x, y)" instead of "= piecePos" for deep copy instead of reference
                if (currentShape == 0) // I piece
                {
                    newPiecePos += IKickTestTransforms[clockwise][currentState][kickTestIndex];
                } else
                {
                    newPiecePos += OtherKickTestTransforms[clockwise][currentState][kickTestIndex];
                }
                // test if the new state does not intersect with a locked piece or go off the edge of the board
                for (int x = 0; x < shapes[currentShape].boundingSize.x; x++)
                {
                    for (int y = 0; y < shapes[currentShape].boundingSize.y; y++)
                    {
                        if (shapes[currentShape].states[newState][x, y])
                        {
                            if (!ClipsBounds(new Point(x + newPiecePos.x, y + newPiecePos.y), realBoardSize))
                            {
                                if (board[x + newPiecePos.x, y + newPiecePos.y].hasPiece && board[x + newPiecePos.x, y + newPiecePos.y].locked)
                                {
                                    newStateOK = false;
                                }
                            }
                            else
                            {
                                newStateOK = false;
                            }
                        }
                    }
                }
            } while (!newStateOK && kickTestIndex < 4);

            if (newStateOK)
            {
                //if (kickTestIndex != 0)
                //{
                //    Debug.WriteLine(kickTestIndex);
                //}
                currentState = newState;
                piecePos = new Point(newPiecePos.x, newPiecePos.y);
                // clear active piece from board
                for (int x = 0; x < realBoardSize.x; x++)
                {
                    for (int y = 0; y < realBoardSize.y; y++)
                    {
                        if (board[x, y].hasPiece && !board[x, y].locked)
                        {
                            board[x, y].Unset();
                        }
                    }
                }
                for (int x = 0; x < shapes[currentShape].boundingSize.x; x++)
                {
                    for (int y = 0; y < shapes[currentShape].boundingSize.y; y++)
                    {
                        if (shapes[currentShape].states[currentState][x, y])
                        {
                            board[x + piecePos.x, y + piecePos.y].Set(shapes[currentShape].colour);
                        }
                    }
                }
            }
        }

        static void TranslatePiece(int xDir)
        {
            Point[] clippedPieceBounds = new Point[] // the board-space bounding-box of the active piece, with the edges clipped if necessary to fit within the edges of the board
            {
                new Point(Math.Max(0, piecePos.x), Math.Max(0, piecePos.y)), // bottom-left corner (inclusive)
                new Point(Math.Min(realBoardSize.x, shapes[currentShape].boundingSize.x + piecePos.x), Math.Min(realBoardSize.y, shapes[currentShape].boundingSize.y + piecePos.y)) // top-right corner (exclusive)
            };
            
            bool canMove = true;
            for (int x = 0; x < realBoardSize.x; x++)
            {
                for (int y = 0; y < realBoardSize.y; y++)
                {
                    if (board[x, y].hasPiece && !board[x, y].locked)
                    {
                        int newTileX = x + xDir;
                        if (newTileX < 0 || newTileX > realBoardSize.x - 1) // piece would go off the edge of the board if moved
                        {
                            canMove = false;
                        } else if (board[newTileX, y].hasPiece && board[newTileX, y].locked) // piece would clip into a locked piece
                        {
                            canMove = false;
                        }
                    }
                }
            }
            if (canMove)
            {
                // if moving to the right (xDir is positive), loop from right to left, otherwise loop left to right, so that pieces are not overwritten
                if (xDir > 0)
                {
                    for (int x = realBoardSize.x - 1; x >= 0; x--)
                    {
                        for (int y = 0; y < realBoardSize.y; y++)
                        {
                            if (board[x, y].hasPiece && !board[x, y].locked)
                            {
                                board[x + xDir, y].Set(shapes[currentShape].colour);
                                board[x, y].Unset();
                            }
                        }
                    }
                } else
                {
                    for (int x = 0; x < realBoardSize.x; x++)
                    {
                        for (int y = 0; y < realBoardSize.y; y++)
                        {
                            if (board[x, y].hasPiece && !board[x, y].locked)
                            {
                                board[x + xDir, y].Set(shapes[currentShape].colour);
                                board[x, y].Unset();
                            }
                        }
                    }
                }
                piecePos.x += xDir;
            }
        }

        static void Hold()
        {
            if (!hasHeld)
            {
                // remove active piece from board
                for (int x = 0; x < realBoardSize.x; x++)
                {
                    for (int y = 0; y < realBoardSize.y; y++)
                    {
                        if (board[x, y].hasPiece && !board[x, y].locked)
                        {
                            board[x, y].Unset();
                        }
                    }
                }

                int newHeldShape = currentShape;
                if (heldShape == -1)
                {
                    SpawnPiece();
                } else
                {
                    SpawnPiece(heldShape);
                }
                heldShape = newHeldShape;
                hasHeld = true;
                UpdateImage();
            }
        }

        static Point[] GetPreviewPositions()
        {
            Point[] r = new Point[4];
            int index = 0;
            for (int x = 0; x < realBoardSize.x; x++)
            {
                for (int y = 0; y < realBoardSize.y; y++)
                {
                    if (board[x, y].hasPiece && !board[x, y].locked)
                    {
                        if (index < 4) // sometimes index goes above the size of the array, no idea why
                        {
                            r[index] = new Point(x, y);
                            index++;
                        }
                    }
                }
            }
            bool canMoveDown = true;
            while (canMoveDown)
            {
                for (int i = 0; i < r.Length; i++)
                {
                    if (r[i].y == 0) // the tetramino has reached the bottom of the board
                    {
                        canMoveDown = false;
                    }
                    else if (board[r[i].x, r[i].y - 1].hasPiece && board[r[i].x, r[i].y - 1].locked) // the tetramino is blocked by a locked tile
                    {
                        canMoveDown = false;
                    }
                }
                if (canMoveDown)
                {
                    for (int i = 0; i < r.Length; i++)
                    {
                        r[i] = new Point(r[i].x, r[i].y - 1);
                    }
                }
            }
            return r;
        }

        static void UpdateImage() // updates image to match the board 
        {
            if (!paused)
            {
                Point[] previewTiles = GetPreviewPositions();
                for (int x = 0; x < visibleBoardSize.x; x++)
                {
                    for (int y = 0; y < visibleBoardSize.y; y++)
                    {
                        image[x + extraCellWidth + 1, y + 1].backColour = board[x, y].colour;
                        image[x + extraCellWidth + 1, y + 1].foreColour = 0;
                        if (board[x, y].hasPiece && !board[x, y].locked && delayingLock)
                        {
                            image[x + extraCellWidth + 1, y + 1].backColour = 0; // normally backColour is set since the ' ' character (space) displays only backColour
                            image[x + extraCellWidth + 1, y + 1].foreColour = board[x, y].colour;
                            image[x + extraCellWidth + 1, y + 1].character = '▓';
                        } else
                        {
                            image[x + extraCellWidth + 1, y + 1].character = ' ';
                        }


                        if (displayGhostPiece && BoardContainsActivePiece() && !delayingLock) // && !delayingLock so that the lock-delayed piece is not overwritten by the ghost piece
                        {
                            for (int i = 0; i < previewTiles.Length; i++)
                            {
                                if (!ClipsBounds(previewTiles[i], visibleBoardSize))
                                {
                                    image[previewTiles[i].x + extraCellWidth + 1, previewTiles[i].y + 1].foreColour = shapes[currentShape].colour;
                                    image[previewTiles[i].x + extraCellWidth + 1, previewTiles[i].y + 1].character = '▒';
                                }
                            }
                        }

                        if (false) // displays tiles for which hasPiece = true and colour = 0
                        {
                            if (/*board[x, y].colour == 0 &&*/ board[x, y].hasPiece == true)
                            {
                                if (board[x, y].locked == true)
                                {
                                    image[x + extraCellWidth + 1, y + 1].backColour = (int)ConsoleColor.DarkGray;
                                }
                                else
                                {
                                    image[x + extraCellWidth + 1, y + 1].backColour = (int)ConsoleColor.Gray;
                                }
                            }
                        }
                        if (false) // displays bounding box
                        {
                            if (image[x + extraCellWidth + 1, y + 1].backColour == 0 && x >= piecePos.x && x < piecePos.x + shapes[currentShape].boundingSize.x && y >= piecePos.y && y < piecePos.y + shapes[currentShape].boundingSize.y)
                            {
                                image[x + extraCellWidth + 1, y + 1].backColour = (int)ConsoleColor.DarkGray;
                            }
                        }
                    }
                }
            }

            UpdateHoldNext();

            ConsoleChar[,] doubleWidthImage = ConsoleChar.DoubleWidth2DArr(image);
            // add text after doubling width
            ConsoleChar[] nextStr = ConsoleChar.StrToConsoleCharArr("Next");
            for (int i = 0; i < nextStr.Length; i++)
            {
                doubleWidthImage[2 * (visibleBoardSize.x + extraCellWidth + 6) + i, visibleBoardSize.y + 1] = nextStr[i];
            }
            ConsoleChar[] holdStr = ConsoleChar.StrToConsoleCharArr("Hold");
            for (int i = 0; i < holdStr.Length; i++)
            {
                doubleWidthImage[6 + i, visibleBoardSize.y + 1] = holdStr[i];
            }

            UpdateScoreboard(ref doubleWidthImage);

            if (paused)
            {
                ConsoleChar[] pausedStr = ConsoleChar.StrToConsoleCharArr("Paused");
                for (int i = 0; i < pausedStr.Length; i++)
                {
                    doubleWidthImage[2 * (extraCellWidth + 1 + visibleBoardSize.x / 2) - pausedStr.Length / 2 + i, visibleBoardSize.y / 2 + 1] = pausedStr[i];
                }
            }

            Drawing.draw(ConsoleChar.Flip2DArrUpDown(doubleWidthImage));
        }

        static void UpdateHoldNext() // updates image so the hold cell contains the held piece and the next cell contains the next 3 pieces
        {
            // hold
            if (heldShape != -1)
            {
                // clear cell
                for (int x = 0; x < 6; x++)
                {
                    for (int y = 0; y < 4; y++)
                    {
                        image[2 + x, visibleBoardSize.y - 2 - y].backColour = 0;
                    }
                }
                // put in held piece
                for (int x = 0; x < shapes[heldShape].boundingSize.x; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        if (shapes[heldShape].states[0][x, y]) // only the top 2 tiles are used in state[0]
                        {
                            image[2 + x, visibleBoardSize.y - 3 + y].backColour = shapes[heldShape].colour;
                        }
                    }
                }
            }

            // next
            // clear cell
            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 10; y++)
                {
                    image[extraCellWidth + visibleBoardSize.x + 4 + x, visibleBoardSize.y - 2 - y].backColour = 0;
                }
            }
            // put in next pieces
            for (int i = 0; i < 3; i++)
            {
                for (int x = 0; x < shapes[bag[i]].boundingSize.x; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        if (shapes[bag[i]].states[0][x, y]) // only the top 2 tiles are used in state[0]
                        {
                            image[extraCellWidth + visibleBoardSize.x + 5 + x, visibleBoardSize.y - 3 - i * 3 + y].backColour = shapes[bag[i]].colour;
                        }                        
                    }
                }
            }
        }

        static void UpdateScoreboard(ref ConsoleChar[,] img)
        {
            ConsoleChar[][] strs = new ConsoleChar[][] 
            {
                ConsoleChar.StrToConsoleCharArr("Score: " + score),
                ConsoleChar.StrToConsoleCharArr("Level: " + level),
                ConsoleChar.StrToConsoleCharArr("Lines: " + lines),
            };
            for (int i = 0; i < strs.Length; i++)
            {
                for (int j = 0; j < strs[i].Length; j++)
                {
                    img[2 + j, 5 - i] = strs[i][j];
                }
            }
        }

        static bool BoardContainsActivePiece()
        {
            bool r = false;
            for (int x = 0; x < realBoardSize.x; x++)
            {
                for (int y = 0; y < realBoardSize.y; y++)
                {
                    if (board[x, y].hasPiece && !board[x, y].locked)
                    {
                        r = true;
                    }
                }
            }
            return r;
        }

        static bool ClipsBounds(Point p, Point bounds) // upperbounds are exclusive 
        {
            bool r = true;
            if (p.x >= 0 && p.x < bounds.x && p.y >= 0 && p.y < bounds.y)
            {
                r = false;
            }
            return r;
        }

        void drawBorders()
        {
            // hold cell
            for (int x = 0; x < 8; x++)
            {
                image[x, visibleBoardSize.y + 1 - 1].character = '▒';
                image[x, visibleBoardSize.y + 1 - 1 - 5].character = '▒';
            }
            for (int y = 0; y < 5; y++)
            {
                image[0, visibleBoardSize.y + 1 - 2 - y].character = '▒';
                image[extraCellWidth - 2, visibleBoardSize.y + 1 - 2 - y].character = '▒';
            }

            // main board cell
            for (int x = 0; x < visibleBoardSize.x + 2; x++)
            {
                image[x + 9, 0].character = '▒';
                image[x + 9, visibleBoardSize.y + 1].character = '▒';
            }
            for (int y = 0; y < visibleBoardSize.y + 2; y++)
            {
                image[0 + extraCellWidth, y].character = '▒';
                image[visibleBoardSize.x + extraCellWidth + 1, y].character = '▒';
            }
            
            // next cell
            for (int x = 0; x < 8; x++)
            {
                image[visibleBoardSize.x + extraCellWidth + 3 + x, visibleBoardSize.y + 1 - 1].character = '▒';
                image[visibleBoardSize.x + extraCellWidth + 3 + x, visibleBoardSize.y + 1 - 1 - 11].character = '▒';
            }
            for (int y = 0; y < 11; y++)
            {
                image[visibleBoardSize.x + extraCellWidth + 3, visibleBoardSize.y + 1 - 2 - y].character = '▒';
                image[visibleBoardSize.x + extraCellWidth + 3 + 7, visibleBoardSize.y + 1 - 2 - y].character = '▒';
            }

            
        }

        struct Tile
        {
            public bool hasPiece;
            public bool locked; // true if the tile contains a moving tetramino
            public int colour; // display colour of the tile (if it contains a piece)  https://docs.microsoft.com/en-us/dotnet/api/system.consolecolor?view=netframework-4.8#fields
            public Tile(bool _hasPiece, bool _locked, int _colour)
            {
                hasPiece = _hasPiece;
                locked = _locked;
                colour = _colour;
            }

            public void Set(int _colour)
            {
                hasPiece = true;
                locked = false;
                colour = _colour;
            }

            public void Unset()
            {
                hasPiece = false;
                colour = 0;
            }
        }

        struct Shape
        {
            public bool[][,] states; // [rotation][x, y]; true is where the tile contains part of the tetramino; see https://tetris.fandom.com/wiki/SRS
            public Point startingPos; // bottom left corner of bounding box
            public Point boundingSize;
            public int colour;

            public Shape(int _colour, Point _startingPos, string[][] _states) // _states: '0' = true; '-' = false
            {
                states = new bool[_states.Length][,];
                for (int r = 0; r < _states.Length; r++)
                {
                    states[r] = new bool[_states[0][0].Length, _states[0].Length];
                    for (int x = 0; x < _states[0].Length; x++)
                    {
                        for (int y = 0; y < _states[0][0].Length; y++)
                        {
                            states[r][x, y] = _states[r][y][x] == '0'; // [y][x] as opposed to [x][y] since each string represents a row, and each char within that string represents a column
                        }
                    }
                }
                startingPos = _startingPos;
                boundingSize = new Point(states[0].GetLength(0), states[0].GetLength(1));
                colour = _colour;
            }
        }
    }
}
