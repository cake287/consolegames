using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace consolegames
{
    class ConsoleChess
    {
        static int boardWidth = 8; // in tiles
        static int tileWidth = 5; // in chars

        const int BLACKTILE = (int)ConsoleColor.DarkYellow, WHITETILE = (int)ConsoleColor.Yellow,
            SELECTEDTILE = (int)ConsoleColor.Blue, TARGETEDTILE = (int)ConsoleColor.Red,
            CHECKTILE = (int)ConsoleColor.DarkRed,
            POSSIBLEMOVEBLACKTILE = (int)ConsoleColor.DarkGreen, POSSIBLEMOVEWHITETILE = (int)ConsoleColor.Green,
            BLACKPIECE = (int)ConsoleColor.Black, WHITEPIECE = (int)ConsoleColor.Gray;
        static bool invertColours = false;
        const int EMPTY = -1, PAWN = 0, KNIGHT = 1, BISHOP = 2, ROOK = 3, QUEEN = 4, KING = 5;
        static Piece[] pieces = new Piece[]
        {
            new Piece(
                PAWN,
                new string[]
                {
                    "OOO",
                    " O ",
                    "OOO",
                },
                new Point[]
                {
                    new Point(0, 1),
                    new Point(0, 2) // not used unless numberMoves == 0
                },
                new Point[]
                {
                    new Point(1, 1),
                    new Point(-1, 1),

                    // en passant taking (recorded as the position of the piece being taken, not as the position where the pawn moves to
                    new Point(1, 0),
                    new Point(-1, 0)
                },
                new Point[] { }
            ),
            new Piece(
                KNIGHT,
                new string[]
                {
                    "OOO",
                    " OO",
                    " O ",
                    "OOO",
                },
                new Point[]
                {
                    new Point(1, 2),
                    new Point(2, 1),
                    new Point(2, -1),
                    new Point(1, -2),
                    new Point(-1, -2),
                    new Point(-2, -1),
                    new Point(-2, 1),
                    new Point(-1, 2),
                },
                new Point[]
                {
                    new Point(1, 2),
                    new Point(2, 1),
                    new Point(2, -1),
                    new Point(1, -2),
                    new Point(-1, -2),
                    new Point(-2, -1),
                    new Point(-2, 1),
                    new Point(-1, 2),
                },
                new Point[] { }
            ),
            new Piece(
                BISHOP,
                new string[]
                {
                    " O ",
                    "O O",
                    " O ",
                    "OOO",
                },
                new Point[] { },
                new Point[] { },
                new Point[]
                {
                    new Point(1, 1),
                    new Point(1, -1),
                    new Point(-1, -1),
                    new Point(-1, 1),
                }
            ),
            new Piece(
                ROOK,
                new string[]
                {
                    "O O",
                    "OOO",
                    " O ",
                    "OOO",
                },
                new Point[] { },
                new Point[] { },
                new Point[]
                {
                    new Point(1, 0),
                    new Point(0, -1),
                    new Point(-1, 0),
                    new Point(0, 1),
                }
            ),
            new Piece(
                QUEEN,
                new string[]
                {
                    "OOO",
                    "OOO",
                    " O ",
                    "OOO",
                },
                new Point[] { },
                new Point[] { },
                new Point[] 
                {
                    new Point(1, 1),
                    new Point(1, -1),
                    new Point(-1, -1),
                    new Point(-1, 1),

                    new Point(1, 0),
                    new Point(0, -1),
                    new Point(-1, 0),
                    new Point(0, 1),
                }
            ),
            new Piece(
                KING,
                new string[]
                {
                    " O ",
                    "OOO",
                    " O ",
                    "OOO",
                },
                new Point[] 
                {
                    new Point(1, 0),
                    new Point(0, -1),
                    new Point(-1, 0),
                    new Point(0, 1),
                   
                    new Point(1, 1),
                    new Point(1, -1),
                    new Point(-1, -1),
                    new Point(-1, 1),
                },
                new Point[]
                {
                    new Point(1, 0),
                    new Point(0, -1),
                    new Point(-1, 0),
                    new Point(0, 1),

                    new Point(1, 1),
                    new Point(1, -1),
                    new Point(-1, -1),
                    new Point(-1, 1),
                },
                new Point[] { }
            ),
        };
        static DisplayTile[,] board = new DisplayTile[boardWidth, boardWidth];
        static ConsoleChar[,] image = new ConsoleChar[boardWidth * tileWidth * 2 + 4, boardWidth * tileWidth + 2];

        static Point selectedPos = new Point(4, 1);
        static bool showSelection = true;
        static bool showPossibleMoves = false;
        static Point targetedPos = new Point(); // red selection; appears once a piece has been selected

        static bool isWhitesMove = true;
        static bool shouldLoop = true;
        static bool stalemate = false;
        static Point enPassantablePawn = new Point(-1, 0);
        static bool[] hasKingMoved = new bool[2]; // [0] = white, [1] = black

        public void run()
        {
            Console.CursorVisible = false;

            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardWidth; y++)
                {
                    board[x, y] = new DisplayTile((x + y) % 2 != 0);
                }
            }

            // for stalemate testing
            //board[0, 0].SetPiece(KING, true);
            //board[2, 1].SetPiece(KING, false);
            //board[6, 5].SetPiece(QUEEN, false);

            // for pawn promotion testing
            //board[0, 5].SetPiece(PAWN, true);
            //board[1, 2].SetPiece(PAWN, false);
            //board[2, 1].SetPiece(PAWN, false);
            //board[5, 0].SetPiece(KING, true);
            //board[7, 1].SetPiece(KING, false);

            // for en passant testing
            //board[1, 1].SetPiece(PAWN, true);
            //board[2, 4].SetPiece(PAWN, false);
            //board[3, 6].SetPiece(PAWN, false);
            //board[4, 4].SetPiece(PAWN, true);
            //board[5, 0].SetPiece(KING, true);
            //board[7, 1].SetPiece(KING, false);

            // for castling testing
            //board[0, 0].SetPiece(ROOK, true);
            //board[7, 0].SetPiece(ROOK, true);
            //board[0, 7].SetPiece(ROOK, false);
            //board[7, 7].SetPiece(ROOK, false);
            //board[4, 0].SetPiece(KING, true);
            //board[4, 7].SetPiece(KING, false);

            InitPieces();
            DrawBoard();

            while (shouldLoop)
            {
                ConsoleKey input = Console.ReadKey(true).Key;
                if (input == ConsoleKey.LeftArrow || input == ConsoleKey.A) { MoveSelection(new Point(-1, 0)); }
                else if (input == ConsoleKey.RightArrow || input == ConsoleKey.D) { MoveSelection(new Point(1, 0)); }
                else if (input == ConsoleKey.UpArrow || input == ConsoleKey.W) { MoveSelection(new Point(0, 1)); }
                else if (input == ConsoleKey.DownArrow || input == ConsoleKey.S) { MoveSelection(new Point(0, -1)); }
                else if (input == ConsoleKey.C) { showSelection = !showSelection; }
                else if (input == ConsoleKey.Spacebar) { Select(); }
                else if (input == ConsoleKey.I) { invertColours = !invertColours; }
                else if (input == ConsoleKey.Escape) { shouldLoop = false; }
                DrawBoard();
            }
            GameOver();
            Console.ReadLine();
            Console.Clear();
        }

        static void InitPieces()
        {
            // pawns
            for (int x = 0; x < 8; x++)
            {
                board[x, 1].SetPiece(PAWN, true);
                board[x, 6].SetPiece(PAWN, false);
            }

            // rooks
            board[0, 0].SetPiece(ROOK, true);
            board[7, 0].SetPiece(ROOK, true);
            board[0, 7].SetPiece(ROOK, false);
            board[7, 7].SetPiece(ROOK, false);

            // knights
            board[1, 0].SetPiece(KNIGHT, true);
            board[6, 0].SetPiece(KNIGHT, true);
            board[1, 7].SetPiece(KNIGHT, false);
            board[6, 7].SetPiece(KNIGHT, false);

            // bishops
            board[2, 0].SetPiece(BISHOP, true);
            board[5, 0].SetPiece(BISHOP, true);
            board[2, 7].SetPiece(BISHOP, false);
            board[5, 7].SetPiece(BISHOP, false);

            // queens
            board[3, 0].SetPiece(QUEEN, true);
            board[3, 7].SetPiece(QUEEN, false);

            // kings
            board[4, 0].SetPiece(KING, true);
            board[4, 7].SetPiece(KING, false);
        }

        static void DrawBoard() // updates selected and highlighted tiles, then updates image to match board and then draws image
        {
            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardWidth; y++)
                {
                    board[x, y].isTargeted= false;
                    board[x, y].isSelected = false;
                    board[x, y].isPossibleMove = false;
                }
            }
            board[selectedPos.x, selectedPos.y].isSelected = true;
            if (showPossibleMoves)
            {
                board[targetedPos.x, targetedPos.y].isTargeted = true;
                if (board[selectedPos.x, selectedPos.y].piece != EMPTY)
                {
                    Point[] possibleMoves = pieces[board[selectedPos.x, selectedPos.y].piece].GetPossibleMoves(selectedPos, true, board);
                    foreach (Point m in possibleMoves)
                    {
                        board[m.x, m.y].isPossibleMove = true;
                    }
                }
            }            

            for (int x = 0; x < image.GetLength(0); x++)
            {
                for (int y = 0; y < image.GetLength(1); y++)
                {
                    image[x, y] = new ConsoleChar(' ', (int)ConsoleColor.White, (int)ConsoleColor.Gray);
                }
            }
            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardWidth; y++)
                {
                    ConsoleChar[,] tileChars = board[x, y].GetConsoleChars();
                    for (int tileX = 0; tileX < tileWidth * 2; tileX++)
                    {
                        for (int tileY = 0; tileY < tileWidth ; tileY++)
                        {
                            image[x * tileWidth * 2 + 2 + tileX, y * tileWidth + tileY + 1] = tileChars[tileX, tileY];
                        }
                    }
                }
            }
            if (invertColours)
            {
                image = ConsoleChar.InvertColours(image);
            }
            Drawing.draw(ConsoleChar.Flip2DArrUpDown(image));
        }
        
        static void Select()
        {
            if (showPossibleMoves)
            {
                if (board[targetedPos.x, targetedPos.y].isPossibleMove)
                {
                    enPassantablePawn = new Point(-1, 0);
                    // if selected piece is a pawn, and it is moving 2 spaces, set enPassantablePawn to its new position
                    if (board[selectedPos.x, selectedPos.y].piece == PAWN && Math.Abs(targetedPos.y - selectedPos.y) == 2)
                    {
                        enPassantablePawn = targetedPos;
                    }

                    if (board[selectedPos.x, selectedPos.y].piece == PAWN && board[targetedPos.x, targetedPos.y].piece == EMPTY && targetedPos.x != selectedPos.x)
                    { // if the pawn is moving to an empty square not in front of it, the move must be en passant and so the adjacent pawn needs to removed
                        board[targetedPos.x, selectedPos.y].UnsetPiece();
                    }

                    if (Math.Abs(selectedPos.x - targetedPos.x) == 2) // castling
                    {
                        if (targetedPos.x < selectedPos.x) // queen's side
                        {
                            board[3, targetedPos.y].SetPiece(ROOK, isWhitesMove);
                            board[0, targetedPos.y].UnsetPiece();
                        } else
                        {
                            board[5, targetedPos.y].SetPiece(ROOK, isWhitesMove);
                            board[7, targetedPos.y].UnsetPiece();
                        }
                    }
                    
                    // move piece
                    board[targetedPos.x, targetedPos.y].SetPiece(board[selectedPos.x, selectedPos.y].piece, board[selectedPos.x, selectedPos.y].isPieceWhite);
                    board[selectedPos.x, selectedPos.y].UnsetPiece();
                    
                    // pawn promotion
                    for (int x = 0; x < boardWidth; x++)
                    {
                        if ((isWhitesMove && board[x, boardWidth - 1].piece == PAWN && board[x, boardWidth - 1].isPieceWhite) ||
                            (!isWhitesMove && board[x, 0].piece == PAWN && !board[x, boardWidth - 1].isPieceWhite)
                            )
                        {
                            string newPieceStr = Program.GetStringInput("Enter piece to promote pawn to ((k)night, (b)ishop, (r)ook or (q)ueen): ", new string[] { "k", "knight", "b", "bishop", "r", "rook", "q", "queen" });
                            int newPiece = EMPTY;
                            if (newPieceStr == "k" || newPieceStr == "knight") { newPiece = KNIGHT; }
                            else if (newPieceStr == "b" || newPieceStr == "bishop") { newPiece = BISHOP; }
                            else if (newPieceStr == "r" || newPieceStr == "rook") { newPiece = ROOK; }
                            else if (newPieceStr == "q" || newPieceStr == "queen") { newPiece = QUEEN; }
                            board[x, isWhitesMove ? boardWidth - 1 : 0].piece = newPiece;
                        }
                    }

                    isWhitesMove = !isWhitesMove;

                    for (int x = 0; x < boardWidth; x++)
                    {
                        for (int y = 0; y < boardWidth; y++)
                        {
                            board[x, y].isInCheck = false;
                        }
                    }
                    if (IsInCheck(isWhitesMove, board))
                    {
                        Debug.WriteLine("check");
                        bool checkmate = true;
                        for (int x = 0; x < boardWidth; x++)
                        {
                            for (int y = 0; y < boardWidth; y++)
                            {
                                if (board[x, y].piece == KING && board[x, y].isPieceWhite == isWhitesMove)
                                {
                                    board[x, y].isInCheck = true;
                                }
                                if (board[x, y].piece != EMPTY && board[x, y].isPieceWhite == isWhitesMove) // if this tile contains a piece of the same colour as the king in check
                                {
                                    if (pieces[board[x, y].piece].GetPossibleMoves(new Point(x, y), true, board).Length != 0) // if said piece has a valid move which would remove the check, then it is not checkmate
                                    {
                                        checkmate = false;
                                    }
                                }
                            }
                        }
                        if (checkmate)
                        {
                            Debug.WriteLine("Checkmate");
                            shouldLoop = false;
                        }
                    } else // test if stalemate
                    {
                        stalemate = true;
                        for (int x = 0; x < boardWidth; x++)
                        {
                            for (int y = 0; y < boardWidth; y++)
                            {
                                if (board[x, y].piece != EMPTY && board[x, y].isPieceWhite == isWhitesMove)
                                {
                                    if (pieces[board[x, y].piece].GetPossibleMoves(new Point(x, y), true, board).Length != 0) // if the piece has a valid move, then it is not stalemate
                                    {
                                        stalemate = false;
                                    }
                                }
                            }
                        }
                        if (stalemate)
                        {
                            shouldLoop = false;
                        }
                    }
                    
                    if (board[targetedPos.x, targetedPos.y].piece == KING)
                    {
                        hasKingMoved[board[targetedPos.x, targetedPos.y].isPieceWhite ? 0 : 1] = true;
                    }
                }

                selectedPos = targetedPos;
                showPossibleMoves = false;
            } else
            {
                if (board[selectedPos.x, selectedPos.y].piece != EMPTY && board[selectedPos.x, selectedPos.y].isPieceWhite == isWhitesMove)
                {
                    showPossibleMoves = true;
                    targetedPos = selectedPos;
                }
            }
        }

        static void GameOver()
        {
            Console.WriteLine("Game over. " + 
                (stalemate ? 
                "Stalemate." : 
                (isWhitesMove ? "Black" : "White") + " wins."
                ));

            //ConsoleChar[,] newImage = new ConsoleChar[image.GetLength(0), image.GetLength(1) + 1];
            //for (int x = 0; x < image.GetLength(0); x++)
            //{
            //    for (int y = 0; y < image.GetLength(1); y++)
            //    {
            //        newImage[x, y] = new ConsoleChar(image[x, y].character, image[x, y].foreColour, image[x, y].backColour);
            //    }
            //}
            //for (int x = 0; x < image.GetLength(0); x++)
            //{
            //    newImage[x, newImage.GetLength(1) - 2] = new ConsoleChar();
            //}
            //ConsoleChar[] gameOverStr = ConsoleChar.StrToConsoleCharArr("Game over. " + (isWhitesMove ? "Black" : "White") + " wins.");
            //for (int x = 0; x < gameOverStr.Length; x++)
            //{
            //    newImage[x, newImage.GetLength(1) - 2] = gameOverStr[x];
            //}
            //Drawing.draw(ConsoleChar.Flip2DArrUpDown(image));
        }

        static void MoveSelection(Point offset)
        {
            if (!showPossibleMoves)
            {
                selectedPos = LoopBounds(selectedPos + offset);
            } else
            {
                targetedPos = LoopBounds(targetedPos + offset);
            }            
        }

        static Point LoopBounds(Point p)
        {
            Point r = p;
            while (!IsInBounds(r))
            {
                if (r.x >= boardWidth) { r.x -= boardWidth; }
                else if (r.x < 0) { r.x += boardWidth; }
                if (r.y >= boardWidth) { r.y -= boardWidth; }
                else if (r.y < 0) { r.y += boardWidth; }
            }
            return r;
        }

        static bool IsInBounds(Point p)
        {
            bool r = true;
            if (p.x < 0 || p.x >= boardWidth || p.y < 0 || p.y >= boardWidth) { r = false; }
            return r;
        }

        static bool IsInCheck(bool testAgainstWhiteKing, Tile[,] boardState)
        {
            //Debug.WriteLine("testAgainstWhiteKing = " + testAgainstWhiteKing);
            //DebugPrintBoard(boardState);
            //Debug.WriteLine("");

            bool inCheck = false;
            Point defendingKingPos = new Point(-1, 0); // to throw an error if the king is not found
            for (int x = 0; x < boardState.GetLength(0); x++)
            {
                for (int y = 0; y < boardState.GetLength(1); y++)
                {
                    if (boardState[x, y].piece == KING && boardState[x, y].isPieceWhite == testAgainstWhiteKing)
                    {
                        if (defendingKingPos.x != -1)
                        {
                            Debug.WriteLine("Chungus");
                        }
                        defendingKingPos = new Point(x, y);
                    }
                }
            }
            if (defendingKingPos.x == -1)
            {
                Debug.WriteLine("Problemo");
            }
            for (int x = 0; x < boardState.GetLength(0); x++)
            {
                for (int y = 0; y < boardState.GetLength(1); y++)
                {
                    if (boardState[x, y].isPieceWhite != testAgainstWhiteKing && boardState[x, y].piece != EMPTY)
                    {
                        Point[] possibleMoves = pieces[boardState[x, y].piece].GetPossibleMoves(new Point(x, y), false, boardState);
                        if (possibleMoves.Contains(defendingKingPos))
                        {
                            //Debug.WriteLine("inCheck = true because of " + boardState[x, y].piece + " at " + x + ", " + y);
                            //Debug.Write("Possible moves of the checking piece: ");
                            //Debug.WriteLine("defendingKingPos = " + defendingKingPos.x + ", " + defendingKingPos.y);
                            //foreach (Point m in possibleMoves)
                            //{
                            //    Debug.Write(m.x + "," + m.y + " ");
                            //}
                            //Debug.WriteLine("");
                            inCheck = true;
                        }
                    }
                }
            }
            return inCheck;
        }

        static void DebugPrintBoard(Tile[,] boardState)
        {
            for (int y = boardState.GetLength(1) -1 ; y >= 0; y--)
            {
                for (int x = 0; x < boardState.GetLength(0); x++)
                {                    
                    Debug.Write(Piece.GetChar(boardState[x, y].isPieceWhite, boardState[x, y].piece));
                }
                Debug.WriteLine("");
            }
        }

        class Tile
        {
            public int piece;
            public bool isPieceWhite;
            
            public void UnsetPiece()
            {
                piece = EMPTY;
            }

            public void SetPiece(int _piece, bool _isPieceWhite)
            {
                piece = _piece;
                isPieceWhite = _isPieceWhite;
            }

            public static Tile[,] DeepCopy(Tile[,] source)
            {
                Tile[,] copy = new Tile[source.GetLength(0), source.GetLength(1)];
                for (int x = 0; x < source.GetLength(0); x++)
                {
                    for (int y = 0; y < source.GetLength(1); y++)
                    {
                        copy[x, y] = new Tile();
                        copy[x, y].piece = source[x, y].piece;
                        copy[x, y].isPieceWhite = source[x, y].isPieceWhite;
                    }
                }
                return copy;
            }
        }

        class DisplayTile : Tile
        {
            public bool isTileWhite;
            public bool isTargeted;
            public bool isSelected;
            public bool isPossibleMove;
            public bool isInCheck;
            
            public DisplayTile(bool _isTileWhite)
            {
                piece = EMPTY;
                isPieceWhite = false;
                isTileWhite = _isTileWhite;
                isTargeted = false;
                isSelected = false;
                isPossibleMove = false;
                isInCheck = false;
            }

            public ConsoleChar[,] GetConsoleChars()
            {
                ConsoleChar[,] r = new ConsoleChar[tileWidth * 2, tileWidth];

                int backColour = isTileWhite? WHITETILE : BLACKTILE;
                if (isTargeted)
                {
                    backColour = TARGETEDTILE;
                } else if (isSelected && showSelection)
                {
                    backColour = SELECTEDTILE;
                }
                else if (isInCheck)
                {
                    backColour = CHECKTILE;
                } else if (isPossibleMove && showPossibleMoves)
                {
                    backColour = isTileWhite ? POSSIBLEMOVEWHITETILE : POSSIBLEMOVEBLACKTILE;
                }

                for (int x = 0; x < r.GetLength(0); x++)
                {
                    for (int y = 0; y < r.GetLength(1); y++)
                    {
                        r[x, y] = new ConsoleChar(' ', 15, backColour);
                    }
                }
                if (piece != EMPTY)
                {
                    for (int y = 0; y < pieces[piece].display.Length; y++)
                    {
                        for (int x = 0; x < pieces[piece].display[y].Length; x++)
                        {
                            if (pieces[piece].display[y][x] == 'O')
                            {
                                r[x * 2 + 2, pieces[piece].display.Length - 1 - y].backColour = isPieceWhite ? WHITEPIECE : BLACKPIECE;
                                r[x * 2 + 3, pieces[piece].display.Length - 1 - y].backColour = isPieceWhite ? WHITEPIECE : BLACKPIECE;
                            }
                        }
                    }
                }
                return r;
            }
        }

        class Piece
        {
            public int type;
            public string[] display;

            // "moves" are tiles relative to the piece (i.e. in piece space) that are possible moves
            Point[] singleMoves; // e.g. knight movement; does not factor in pieces between the original tile and the target tile
            Point[] singleTakeableMoves; // for pawn taking
            Point[] multipleMoves; // e.g. bishop movement (movement includes taking on multiple, unlike single); describes the 'gradient' of the line that starts at the original tile and goes through the target tiles

            public Piece(int _type, string[] _display, Point[] _singleMoves, Point[] _singleTakeableMoves, Point[] _multipleMoves)
            {
                type = _type;
                display = _display;
                singleMoves = _singleMoves;
                singleTakeableMoves = _singleTakeableMoves;
                multipleMoves = _multipleMoves;
            }
                          
            public Point[] GetPossibleMoves(Point currentPos, bool considerCheck, Tile[,] boardState)
            {
                List<Point> r = new List<Point>();

                foreach (Point offset in singleMoves)
                {
                    if (!(type == PAWN && offset == new Point(0, 2)) ||
                        (currentPos.y == 1 && boardState[currentPos.x, currentPos.y].isPieceWhite && boardState[currentPos.x, currentPos.y + 1].piece == EMPTY) ||
                        (currentPos.y == 6 && !boardState[currentPos.x, currentPos.y].isPieceWhite && boardState[currentPos.x, currentPos.y - 1].piece == EMPTY)
                        )
                    {
                        Point p = currentPos;
                        p += boardState[currentPos.x, currentPos.y].isPieceWhite ? offset : new Point(offset.x, -offset.y); // black pawns move direction -y so offsets have to be flipped
                        if (IsInBounds(p))
                        {
                            if (boardState[p.x, p.y].piece == EMPTY)
                            {
                                r.Add(p);
                            }
                        }
                    }
                }                
                foreach (Point offset in singleTakeableMoves)
                {
                    Point p = currentPos;
                    p += boardState[currentPos.x, currentPos.y].isPieceWhite ? offset : new Point(offset.x, -offset.y);
                    if (IsInBounds(p))
                    {
                        if (boardState[p.x, p.y].piece != EMPTY && boardState[p.x, p.y].isPieceWhite != boardState[currentPos.x, currentPos.y].isPieceWhite)
                        {
                            if (boardState[p.x, p.y].piece == PAWN && boardState[currentPos.x, currentPos.y].piece == PAWN && p == enPassantablePawn && offset.y == 0) // for en passant
                            {
                                p.y += boardState[currentPos.x, currentPos.y].isPieceWhite ? 1 : -1;
                                r.Add(p);
                            } else
                            {
                                r.Add(p);
                            } 
                        }
                    }
                }
                foreach (Point offset in multipleMoves)
                {
                    bool obstructed = false;
                    for (Point p = currentPos + offset; IsInBounds(p) && !obstructed; p += offset)
                    {
                        if (boardState[p.x, p.y].isPieceWhite == boardState[currentPos.x, currentPos.y].isPieceWhite && boardState[p.x, p.y].piece != EMPTY)
                        {
                            obstructed = true;
                        }
                        else if (boardState[p.x, p.y].piece != EMPTY)
                        {
                            obstructed = true;
                            r.Add(p);
                        }
                        else
                        {
                            r.Add(p);
                        }
                    }                    
                }
                
                if (considerCheck)
                {
                    // remove possible moves if they put the king in check
                    List<Point> dissallowedMoves = new List<Point>(); // removing moves in the foreach loop throws an error because the next iteration requires the list to be the same size (i think)
                    foreach (Point move in r)
                    {
                        Tile[,] newBoardState = Tile.DeepCopy(boardState);                        
                        newBoardState[move.x, move.y].SetPiece(boardState[currentPos.x, currentPos.y].piece, boardState[currentPos.x, currentPos.y].isPieceWhite);
                        newBoardState[currentPos.x, currentPos.y].UnsetPiece();
                        if (IsInCheck(isWhitesMove, newBoardState))
                        {
                            dissallowedMoves.Add(move);
                        }
                    }
                    foreach (Point dissallowedMove in dissallowedMoves)
                    {
                        r.Remove(dissallowedMove);
                    }

                    // castling
                    if (type == KING && !hasKingMoved[isWhitesMove ? 0 : 1])
                    {
                        Debug.WriteLine("king hasnt moved " + considerCheck);
                        for (int sideOffset = -1; sideOffset <= 1; sideOffset += 2)
                        {
                            bool canCastle = true;
                            if (sideOffset == -1 && !(boardState[0, currentPos.y].piece == ROOK && boardState[0, currentPos.y].isPieceWhite == boardState[currentPos.x, currentPos.y].isPieceWhite) )
                            {
                                canCastle = false;
                            } else if (sideOffset == 1 && !(boardState[7, currentPos.y].piece == ROOK && boardState[7, currentPos.y].isPieceWhite == boardState[currentPos.x, currentPos.y].isPieceWhite))
                            {
                                canCastle = false;
                            }
                            for (int x = 1; x <= 2; x++)
                            {
                                if (boardState[currentPos.x + x * sideOffset, currentPos.y].piece == EMPTY)
                                {
                                    Tile[,] newBoardState = Tile.DeepCopy(boardState);
                                    newBoardState[currentPos.x + x * sideOffset, currentPos.y].SetPiece(KING, boardState[currentPos.x, currentPos.y].isPieceWhite);
                                    newBoardState[currentPos.x, currentPos.y].UnsetPiece();
                                    if (IsInCheck(isWhitesMove, newBoardState))
                                    {
                                        canCastle = false;
                                    }
                                }
                                else
                                {
                                    canCastle = false;
                                }
                            }
                            if (canCastle)
                            {
                                r.Add(new Point(currentPos.x + 2 * sideOffset, currentPos.y));
                            }
                        }
                    }
                }

                return r.ToArray();
            }

            public static char GetChar(bool isWhite, int type)
            {
                char c = ' ';
                switch (type)
                {
                    case PAWN: c = 'P'; break;
                    case KNIGHT: c = 'N'; break;
                    case BISHOP: c = 'B'; break;
                    case ROOK: c = 'R'; break;
                    case QUEEN: c = 'Q'; break;
                    case KING: c = 'K'; break;
                }
                if (!isWhite) { c = char.ToLower(c); }
                return c;
            }
        }
    }
}
