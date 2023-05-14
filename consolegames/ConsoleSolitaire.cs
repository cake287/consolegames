using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace consolegames
{
    class ConsoleSolitaire
    {
        const int DIAMONDS = 0, CLUBS = 1, HEARTS = 2, SPADES = 3;
        readonly int[] SUITCOL = new int[] { 12, 0, 12, 0 };
        readonly char[] SUITCHAR = new char[] { '♦', '♣', '♥', '♠' };
        readonly string[][] SUITIMGS = new string[][]
        {
            new string[] {
                "   █   ",
                "  ███  ",
                " █████ ",
                " █████ ",
                "  ███  ",
                "   █   ",
            },
            new string[] {
                "  ▄█▄  ",
                " █████ ",
                " ▄███▄ ",
                "███████",
                "▀█████▀",
                "  ▄█▄  ",
            },
            new string[] {
                "▄██▄██▄",
                "███████",
                "███████",
                " █████ ",
                "  ███  ",
                "   █   ",
            },
            new string[] {
                "   █   ",
                "  ███  ",
                "▄█████▄",
                "███████",
                "▀█████▀",
                "  ▄█▄  ",
            }
        };
        ConsoleChar[,] CARDBACK;
        ConsoleChar[,] EMPTYSLOT;

        readonly string[] NUMCHARS = new string[] { "A ", "2 ", "3 ", "4 ", "5 ", "6 ", "7 ", "8 ", "9 ", "10", "J ", "Q ", "K "}; // fuck 10 why tf do u need to have 2 CHARACTERS YOU FUCKING SPECIAL PRICK


        const int CARDBACKCOL = 15;

        int STACKCOUNT = 7;
        int CARDWIDTH =  13;
        int CARDHEIGHT = 10;
        const float CARDLAYERPROPORTION = 0.15f;

        const int CURSORCOLOUR = 11;
        const int SELECTEDCOLOUR = 4;
        const int FOUNDATIONIMGCOLOUR = 8;

        const int BORDERFORECOL = 15;
        const int BORDERBACKCOL = 0;

        public void run(bool showHelp, bool chooseGameParameters)
        {
            if (showHelp)
            {
                Console.WriteLine("Use arrow keys or WASD to move the cursor");
                Console.WriteLine("Use space to select a card");
                Console.WriteLine("Move cards of an opposite colour onto cards of number 1 higher");
                Console.WriteLine("Move cards into the foundations on the right from ace to king to complete the game");
                Console.ReadLine();
                Console.Clear();
            }

            bool completedStacks = false;
            if (chooseGameParameters)
            {
                STACKCOUNT = Program.GetIntInput("Stack count (default: 7) : ", 100, 0);
                CARDWIDTH = Program.GetIntInput("Card width (default: 13) : ", 1000, 1);
                CARDHEIGHT = Program.GetIntInput("Card height (default: 10) : ", 1000, 1);
                completedStacks = Program.GetBoolInput("Complete stacks at beginning? (y/n) (default n): ");
                Console.Clear();
            }


            PopulateCardBack();


            List<Card> deck = new List<Card>();
            List<Card> waste = new List<Card>();
            List<Card>[] foundations = new List<Card>[SUITCOL.Length];
            List<Card>[] stacks = new List<Card>[STACKCOUNT];
            List<bool>[] showCards = new List<bool>[STACKCOUNT];

            (int x, int y) cursor = (1, 0);
            (int x, int y) selection = (0, 0);

            if (!completedStacks)
            {
                for (int n = 0; n < NUMCHARS.Length; n++)
                    for (int s = 0; s < SUITCOL.Length; s++)
                        deck.Add(new Card(n, s));

                for (int i = 0; i < foundations.Length; i++)
                    foundations[i] = new List<Card>();

                Random r = new Random();
                deck = deck.OrderBy(i => r.Next()).ToList();

                for (int i = 0; i < STACKCOUNT; i++)
                {
                    stacks[i] = new List<Card>();
                    showCards[i] = new List<bool>();
                    for (int j = 0; j <= i; j++)
                    {
                        if (deck.Any())
                        {
                            stacks[i].Add(deck[deck.Count - 1]);
                            deck.RemoveAt(deck.Count - 1);
                            showCards[i].Add(j == i);
                        }
                        else if (showCards[i].Any())
                            showCards[i][showCards[i].Count - 1] = true;
                    }
                }
            }
            else
            {
                for (int i = 0; i < foundations.Length; i++)
                    foundations[i] = new List<Card>();

                for (int i = 0; i < STACKCOUNT; i++)
                {
                    stacks[i] = new List<Card>();
                    showCards[i] = new List<bool>();
                }

                waste.Add(new Card(0, 0));
                for (int n = 0; n < NUMCHARS.Length; n++)
                    for (int s = 0; s < SUITCOL.Length; s++)
                    {
                        int suit = s;
                        if (n % 2 != 0)
                            suit++;
                        if (suit > SUITCOL.Length - 1)
                            suit = 0;

                        stacks[s].Add(new Card(NUMCHARS.Length - 1 - n, suit));
                        showCards[s].Add(true);
                    }
                stacks[0].RemoveAt(stacks[0].Count - 1);
                cursor = (1, -(stacks[0].Count - 1));
                selection = (0, -1);
            }


            ConsoleChar[,] screen = new ConsoleChar[
                Console.WindowWidth, //Math.Max((STACKCOUNT + 2) * (CARDWIDTH + 1) + 10, Console.WindowWidth), 
                Console.WindowHeight]; //Math.Max(CARDHEIGHT * 5, Console.WindowHeight)];
            DrawScreen(screen, deck, waste, stacks, showCards, foundations, cursor, selection);
            Drawing.draw(screen);

            bool shouldLoop = true;
            bool won = false;
            while (shouldLoop && !won)
            {
                won = hasWon(ref foundations);

                (int x , int y) dir = (0, 0);
                ConsoleKey input = Console.ReadKey(true).Key;

                int keyX = -1;

                if (input == ConsoleKey.DownArrow || input == ConsoleKey.S) dir.y = -1;
                else if (input == ConsoleKey.UpArrow || input == ConsoleKey.W) dir.y = 1;
                else if (input == ConsoleKey.RightArrow || input == ConsoleKey.D) dir.x = 1;
                else if (input == ConsoleKey.LeftArrow || input == ConsoleKey.A) dir.x = -1;
                else if (input >= ConsoleKey.D0 && input <= ConsoleKey.D9)
                    keyX = input - ConsoleKey.D0;
                else if (input >= ConsoleKey.NumPad0 && input <= ConsoleKey.NumPad9)
                    keyX = input - ConsoleKey.NumPad0;
                else if (input == ConsoleKey.Spacebar)
                    PressCursor(ref deck, ref waste, ref stacks, ref showCards, ref foundations, ref cursor, ref selection);
                else if (input == ConsoleKey.Escape)
                    shouldLoop = false;

                if (keyX != -1)
                {
                    cursor.x = keyX % (STACKCOUNT + 2);
                    if (cursor.x == 0 || cursor.x == STACKCOUNT + 1)
                        cursor.y = 0;
                    else if (cursor.x > 0 && cursor.x < STACKCOUNT + 1)
                        cursor.y = -(stacks[cursor.x - 1].Count - 1);
                }


                if (dir.x != 0 || dir.y != 0)
                    MoveCursor(ref deck, ref waste, ref stacks, ref showCards, ref foundations, ref cursor, dir);

                DrawScreen(screen, deck, waste, stacks, showCards, foundations, cursor, selection);
                Drawing.draw(screen);

                if (!deck.Any() && !waste.Any())
                {
                    bool stacksComplete = true;
                    foreach (List<bool> s in showCards)
                        if (s.Contains(false))
                            stacksComplete = false;

                    if (stacksComplete)
                    {
                        for (int num = 0; num < NUMCHARS.Length; num++)
                        {
                            for (int i = 0; i < stacks.Length; i++)
                            {
                                if (stacks[i].Any())
                                    if (stacks[i][stacks[i].Count - 1].number == num)
                                    {
                                        MoveToFoundation(ref stacks, ref showCards, ref foundations,
                                            (i + 1, -(stacks[i].Count - 1)));
                                        DrawScreen(screen, deck, waste, stacks, showCards, foundations, cursor, selection);
                                        Drawing.draw(screen);
                                        System.Threading.Thread.Sleep(20);
                                    }
                            }
                        }
                        won = true;
                    }
                }
            }
            if (won)
            {
                EndingAnimation(ref screen, ref foundations);
                Console.WriteLine("You win! Press enter to continue...");

            }
            else
                Console.WriteLine("You lost. Press enter to continue...");


            Console.ReadLine();
        }


        void EndingAnimation(ref ConsoleChar[,] screen, ref List<Card>[] foundations)
        {
            // probs shouldve made a struct but oh well lol i really like tuples :pleading:
            List<PhysicsCard> cards =
                new List<PhysicsCard>();
            Random r = new Random();
            (float min, float max) vxBounds = (1, 7);

            while (foundations[0].Any())
            {
                for (int s = 0; s < SUITCHAR.Length; s++)
                {
                    float vx = (float)r.NextDouble() * (vxBounds.max - vxBounds.min) + vxBounds.min;
                    int vxPositive = r.Next(2); // 0 if negative, 1 if positive
                    vx *= vxPositive == 0 ? 1 : -1;

                    cards.Add(new PhysicsCard(foundations[s][foundations[s].Count - 1].number,
                        s,
                        (6 + CARDWIDTH + (CARDWIDTH + 1) * STACKCOUNT + 4, CARDHEIGHT * (s + 1) + 1),
                        (vx, 0)
                        ));
                    foundations[s].RemoveAt(foundations[s].Count - 1);
                }
            }

            float g = 1f;
            float dt = 0.4f;
            float startRestitution = 0.92f;
            int sleepTime = 10;
            bool inAnimation = true;
            int nextCard = 0;
            for (int frame = 0; inAnimation; frame++)
            {
                if (frame % 20 == 0 && nextCard < cards.Count) // release a new card every 20th frame
                {
                    cards[nextCard].moving = true;
                    nextCard++;
                }


                foreach (PhysicsCard c in cards)
                {
                    if (c.moving)
                    {
                        c.pos.x += c.v.x * dt;

                        c.v.y += g * dt;
                        c.pos.y += c.v.y * dt;
                        if (c.pos.y >= screen.GetLength(1) - 1) // bottom of the physics world is actually the top of the screen because it's drawn top to bottom
                        {
                            c.bounce++;
                            c.v.y *= -(float)Math.Pow(startRestitution, c.bounce);
                            c.pos.y = -Math.Abs(screen.GetLength(1) - 1 - c.pos.y) + screen.GetLength(1) - 1;
                        }

                        if (c.pos.x < -CARDWIDTH || c.pos.x > (screen.GetLength(0) + CARDWIDTH))
                        {
                            // side bouncing
                            //c.v.x *= -1; 
                            //c.pos.x += c.v.x * dt * 2;

                            c.moving = false;
                        }

                        ConsoleChar.CopyImageCrop(ref screen, ref c.img, (int)c.pos.x, (int)c.pos.y - CARDHEIGHT);
                    }
                }


                inAnimation = false;
                foreach (PhysicsCard c in cards)
                {
                    if (c.moving)
                        inAnimation = true;
                }
                foreach(List<Card> f in foundations)
                {
                    if (f.Any())
                        inAnimation = true;
                }

                Drawing.draw(screen);

                System.Threading.Thread.Sleep(sleepTime);
            }


            return;

            // bouncing physics test
            (float x, float y) pos = (3, 20);
            (float x, float y) v = (3, 0);
            int bounce = 0;
            for (float t = 0; pos.x >= 0 && pos.x < screen.GetLength(0); t += dt)
            {
                if (pos.y > 0 && pos.y < screen.GetLength(1))
                    screen[(int)pos.x, screen.GetLength(1) - 1 - (int)pos.y].backColour = SELECTEDCOLOUR;
                Drawing.draw(screen);
                System.Threading.Thread.Sleep(sleepTime);

                pos.x += v.x * dt;

                v.y += g * dt;
                pos.y += v.y * dt;
                if (pos.y < 0)
                {
                    bounce++;
                    pos.y = -pos.y;
                    v.y *= -(float)Math.Pow(startRestitution, bounce);
                }
            }
        }

        bool hasWon(ref List<Card>[] foundations)
        {
            bool won = true;
            foreach (List<Card> f in foundations)
            {
                if (f.Count != NUMCHARS.Length)
                    won = false;
            }
            return won;
        }

        void PressCursor(ref List<Card> deck, ref List<Card> waste, ref List<Card>[] stacks, ref List<bool>[] showCards, ref List<Card>[] foundations, ref (int x, int y) cursor, ref (int x, int y) selection)
        {
            if (selection == (0, 0))
            {
                if (cursor == (0, 0))
                    AddWaste(ref deck, ref waste);
                else if (cursor == (0, -1))
                {
                    selection = cursor;
                }
                else if (cursor.x > 0 && cursor.x < STACKCOUNT + 1)
                {
                    if (stacks[cursor.x - 1].Any())
                        selection = cursor;
                }
            } else if (cursor.x == 0)
            {
                selection = (0, 0);
            } else
            {
                if (cursor.x < STACKCOUNT + 1)
                {
                    if (selection == (0, -1))
                    {
                        if (!stacks[cursor.x - 1].Any())
                        {
                            MoveFromWaste(ref waste, ref stacks, ref showCards, cursor);
                        }
                        else if (CanStackCard(waste[waste.Count - 1], stacks[cursor.x - 1][stacks[cursor.x - 1].Count - 1]) || !stacks[cursor.x - 1].Any())
                        {
                            MoveFromWaste(ref waste, ref stacks, ref showCards, cursor);
                        }
                    }
                    else if (selection.x > 0 && selection.y < STACKCOUNT + 1)
                    {
                        if (!stacks[cursor.x - 1].Any())
                        {
                            MoveCards(ref stacks, ref showCards, cursor, selection);
                        }
                        else if (CanStackCard(stacks[selection.x - 1][-selection.y], stacks[cursor.x - 1][stacks[cursor.x - 1].Count - 1]))
                        {
                            MoveCards(ref stacks, ref showCards, cursor, selection);
                        }
                    }
                    selection = (0, 0);
                    cursor.y = -(stacks[cursor.x - 1].Count - 1);
                } else
                {
                    if (selection.x > 0)
                    {
                        if (-selection.y == stacks[selection.x - 1].Count - 1) // card must be the bottom of the stack to move to foundation
                        {
                            if (foundations[stacks[selection.x - 1][-selection.y].suit].Any())
                            {
                                if (stacks[selection.x - 1][-selection.y].number == 
                                    foundations[stacks[selection.x - 1][-selection.y].suit][foundations[stacks[selection.x - 1][-selection.y].suit].Count - 1].number + 1)
                                {
                                    MoveToFoundation(ref stacks, ref showCards, ref foundations, selection);
                                }
                            } else if (stacks[selection.x - 1][-selection.y].number == 0)
                            {
                                MoveToFoundation(ref stacks, ref showCards, ref foundations, selection);
                            }

                        } 
                    }

                    selection = (0, 0);
                }
            }
        }

        void MoveToFoundation(ref List<Card>[] stacks, ref List<bool>[] showCards, ref List<Card>[] foundations, (int x, int y) selection)
        {
            foundations[stacks[selection.x - 1][-selection.y].suit].Add(stacks[selection.x - 1][-selection.y]);
            if (selection.y != 0)
                showCards[selection.x - 1][-selection.y - 1] = true;
            stacks[selection.x - 1].RemoveAt(-selection.y);
            showCards[selection.x - 1].RemoveAt(-selection.y);
        }
        void MoveFromWaste(ref List<Card> waste, ref List<Card>[] stacks, ref List<bool>[] showCards, (int x, int y) cursor)
        {
            stacks[cursor.x - 1].Add(waste[waste.Count - 1]);
            showCards[cursor.x - 1].Add(true);
            waste.RemoveAt(waste.Count - 1);
        }

        void MoveCards(ref List<Card>[] stacks, ref List<bool>[] showCards, (int x, int y) cursor, (int x, int y) selection)
        {
            for (int i = -selection.y; i < stacks[selection.x - 1].Count; i++)
            {
                stacks[cursor.x - 1].Add(stacks[selection.x - 1][i]);
                showCards[cursor.x - 1].Add(true);
            }
            stacks[selection.x - 1].RemoveRange(-selection.y, stacks[selection.x - 1].Count + selection.y);
            showCards[selection.x - 1].RemoveRange(-selection.y, stacks[selection.x - 1].Count + selection.y);
            //stacks[selection.x - 1].RemoveAt(-selection.y);
            //showCards[selection.x - 1].RemoveAt(-selection.y);
            cursor.y = -(stacks[cursor.x - 1].Count - 1);
            if (selection.y != 0)
                showCards[selection.x - 1][-selection.y - 1] = true;

        }

        void MoveCursor(ref List<Card> deck, ref List<Card> waste, ref List<Card>[] stacks, ref List<bool>[] showCards, ref List<Card>[] foundations, ref (int x, int y) cursor, (int x, int y) dir)
        {
            if (dir.x != 0)
            {
                if (cursor.x + dir.x >= 1 && cursor.x + dir.x <= STACKCOUNT)
                {
                    cursor.x += dir.x;

                    // move cursor to the nearest possible y value (not used)
                    //int bottomY = -(stacks[cursor.x - 1].Count - 1);
                    //if (cursor.y < bottomY)
                    //    cursor.y = bottomY;

                    //int topY = 0;
                    //for (int i = stacks[cursor.x - 1].Count - 1; i >= 0; i--)
                    //{
                    //    if (showCards[cursor.x - 1][i])
                    //        topY = -i;
                    //}
                    //if (cursor.y > topY)
                    //    cursor.y = topY;

                    // move cursor to the bottom of the stack
                    cursor.y = -(stacks[cursor.x - 1].Count - 1);
                }

                else if (dir.x == -1 && cursor.x == 1)
                {
                    cursor.x += dir.x;
                    cursor.y = 0;
                }
                else if (cursor.x + dir.x == STACKCOUNT + 1)
                {
                    cursor.x += dir.x;
                    cursor.y = 0;
                }
                else if (cursor.x + dir.x == STACKCOUNT + 2) // loop round to deck
                {
                    cursor.x = 0;
                    cursor.y = 0;
                } else if (cursor.x == 0 && dir.x == -1)
                {
                    cursor.x = STACKCOUNT + 1;
                    cursor.y = 0;
                }
            }
            else
            {
                if (cursor.x == 0)
                {
                    if (!(cursor.y == 0 && !waste.Any()))
                    {
                        cursor.y += dir.y;
                        cursor.y = -(Math.Abs(cursor.y) % 2);
                    }
                }
                else if (cursor.x >= 1 && cursor.x <= STACKCOUNT)
                {
                    int bottomY = -(stacks[cursor.x - 1].Count - 1);
                    int topY = 0;
                    for (int i = stacks[cursor.x - 1].Count - 1; i >= 0; i--)
                    {
                        if (showCards[cursor.x - 1][i])
                            topY = -i;
                    }
                    cursor.y += dir.y;
                    if (cursor.y + dir.y > topY + 1) cursor.y = bottomY;
                    else if (cursor.y + dir.y < bottomY - 1) cursor.y = topY;
                } else if (cursor.x == STACKCOUNT + 1)
                {
                    cursor.y += dir.y;
                    if (cursor.y > 0) cursor.y = -(foundations.Length - 1);
                    else if (cursor.y <= -foundations.Length) cursor.y = 0;
                }
            }
        }

        void DrawScreen(ConsoleChar[,] screen, List<Card> deck, List<Card> waste, List<Card>[] stacks, List<bool>[] showCards, List<Card>[] foundations, (int x, int y) cursor, (int x, int y) selection)
        {
            for (int x = 0; x < screen.GetLength(0); x++)
                for (int y = 0; y < screen.GetLength(1); y++)
                    screen[x, y] = new ConsoleChar(' ', 15, 0);
            

            {
                int col = BORDERBACKCOL;
                if (cursor == (0, 0))
                    col = CURSORCOLOUR;
                ConsoleChar[,] c;
                if (deck.Any())
                    c = DrawCardBack(BORDERFORECOL, col);
                else
                    c = DrawEmptySlot(BORDERFORECOL, col);
                
                ConsoleChar.CopyImageCrop(ref screen, ref c, 1, 1);
            }
            
            if (waste.Any())
            {
                int cardsToDisplay = Math.Min(3, waste.Count);
                for (int i = 0; i < cardsToDisplay; i++)
                {
                    int col = BORDERBACKCOL;
                    if (cursor == (0, -1) && cardsToDisplay - i == 1)
                        col = CURSORCOLOUR;
                    else if (selection == (0, -1) && cardsToDisplay - i == 1)
                        col = SELECTEDCOLOUR;
                    ConsoleChar[,] c = DrawCard(waste[waste.Count - cardsToDisplay + i], BORDERFORECOL, col);
                    ConsoleChar.CopyImageCrop(ref screen, ref c, 1, CARDHEIGHT + 1 + (int)(CARDHEIGHT * CARDLAYERPROPORTION + 1) * i);
                }
            }

            for (int i = 0; i < stacks.Length; i++)
            {
                for (int j = 0; j < stacks[i].Count; j++)
                {
                    int col = BORDERBACKCOL;
                    if (cursor.x - 1 == i && cursor.y == -j)
                        col = CURSORCOLOUR;
                    else if (selection.x - 1 == i && selection.y == -j)
                        col = SELECTEDCOLOUR;
                    ConsoleChar[,] c = showCards[i][j] ? DrawCard(stacks[i][j], BORDERFORECOL, col) : DrawCardBack();
                    ConsoleChar.CopyImageCrop(ref screen, ref c, 6 + CARDWIDTH + (CARDWIDTH + 1) * i, 1 + (int)(CARDHEIGHT * CARDLAYERPROPORTION + 1) * j);
                }
                if (!stacks[i].Any())
                {
                    int col = BORDERBACKCOL;
                    if (cursor.x - 1 == i)
                        col = CURSORCOLOUR;
                    ConsoleChar[,] c = DrawEmptySlot(BORDERFORECOL, col);
                    ConsoleChar.CopyImageCrop(ref screen, ref c, 6 + CARDWIDTH + (CARDWIDTH + 1) * i, 1);
                }
            }

            for (int i = 0; i < foundations.Length; i++)
            {
                if (!foundations[i].Any())
                {
                    int col = BORDERBACKCOL;
                    if (cursor.x == STACKCOUNT + 1 && -cursor.y == i)
                        col = CURSORCOLOUR;
                    ConsoleChar[,] c = DrawEmptySlot(i, BORDERFORECOL, col);
                    ConsoleChar.CopyImageCrop(ref screen, ref c, 6 + CARDWIDTH + (CARDWIDTH + 1) * STACKCOUNT + 4, 1 + CARDHEIGHT * i);
                } else
                {
                    int col = BORDERBACKCOL;
                    if (cursor.x == STACKCOUNT + 1 && -cursor.y == i)
                        col = CURSORCOLOUR;
                    ConsoleChar[,] c = DrawCard(foundations[i][foundations[i].Count - 1], BORDERFORECOL, col);
                    ConsoleChar.CopyImageCrop(ref screen, ref c, 6 + CARDWIDTH + (CARDWIDTH + 1) * STACKCOUNT + 4, 1 + CARDHEIGHT * i);
                }
            }
            //return Task.CompletedTask;
        }


        void AddWaste(ref List<Card> deck, ref List<Card> waste) // moves a card from the top of the deck to the waste
        {
            if (deck.Any())
            {
                waste.Add(deck[deck.Count - 1]);
                deck.RemoveAt(deck.Count - 1);
            } else
                while (waste.Any())
                {
                    deck.Add(waste[waste.Count - 1]);
                    waste.RemoveAt(waste.Count - 1);
                }
        }

        bool CanStackCard(Card bottom, Card top)
        {
            bool r = true;
            r = top.number == bottom.number + 1;
            r = bottom.suit % 2 != top.suit % 2 ? r : false ; // red suits are odd, black suits are even
            return r;
        }

        ConsoleChar[,] DrawCardBack()
        {
            return CARDBACK;
        }
        ConsoleChar[,] DrawCardBack(int borderForeColour, int borderBackColour)
        {
            ConsoleChar[,] r = new ConsoleChar[CARDBACK.GetLength(0), CARDBACK.GetLength(1)];
            for (int x = 0; x < CARDBACK.GetLength(0); x++)
                for (int y = 0; y < CARDBACK.GetLength(1); y++)
                {     
                    if (x == 0 || x == CARDBACK.GetLength(0) - 1 || y == 0 || y == CARDBACK.GetLength(1) - 1)
                        r[x, y] = new ConsoleChar(CARDBACK[x, y].character, borderForeColour, borderBackColour);
                    else
                        r[x, y] = new ConsoleChar(CARDBACK[x, y].character, CARDBACK[x, y].foreColour, CARDBACK[x, y].backColour);
                }
            return r;
        }
        ConsoleChar[,] DrawEmptySlot()
        {
            return EMPTYSLOT;
        }
        ConsoleChar[,] DrawEmptySlot(int borderForeColour, int borderBackColour)
        {
            ConsoleChar[,] r = new ConsoleChar[EMPTYSLOT.GetLength(0), EMPTYSLOT.GetLength(1)];
            for (int x = 0; x < EMPTYSLOT.GetLength(0); x++)
                for (int y = 0; y < EMPTYSLOT.GetLength(1); y++)
                {
                    if (x == 0 || x == EMPTYSLOT.GetLength(0) - 1 || y == 0 || y == EMPTYSLOT.GetLength(1) - 1)
                        r[x, y] = new ConsoleChar(EMPTYSLOT[x, y].character, borderForeColour, borderBackColour);
                    else
                        r[x, y] = new ConsoleChar(EMPTYSLOT[x, y].character, EMPTYSLOT[x, y].foreColour, EMPTYSLOT[x, y].backColour);
                }
            return r;
        }
        ConsoleChar[,] DrawEmptySlot(int suit, int borderForeColour, int borderBackColour)
        {
            ConsoleChar[,] r = new ConsoleChar[EMPTYSLOT.GetLength(0), EMPTYSLOT.GetLength(1)];
            for (int x = 0; x < EMPTYSLOT.GetLength(0); x++)
                for (int y = 0; y < EMPTYSLOT.GetLength(1); y++)
                {
                    if (x == 0 || x == EMPTYSLOT.GetLength(0) - 1 || y == 0 || y == EMPTYSLOT.GetLength(1) - 1)
                        r[x, y] = new ConsoleChar(EMPTYSLOT[x, y].character, borderForeColour, borderBackColour);
                    else
                        r[x, y] = new ConsoleChar(EMPTYSLOT[x, y].character, FOUNDATIONIMGCOLOUR, EMPTYSLOT[x, y].backColour);
                }

            int imageTop = CARDHEIGHT - 2 - SUITIMGS[suit].Length;
            int imageLeft = (CARDWIDTH - SUITIMGS[suit][0].Length) / 2;
            for (int x = 0; x < SUITIMGS[suit][0].Length; x++)
                for (int y = 0; y < SUITIMGS[suit].Length; y++)
                    r[imageLeft + x, imageTop + y].character = SUITIMGS[suit][y][x];

            return r;
        }



        ConsoleChar[,] DrawCard(Card c)
        {
            return DrawCard(c, 15, 0);
        }
        ConsoleChar[,] DrawCard(Card c, int borderForeColour, int borderBackColour)
        {
            ConsoleChar[,] r = new ConsoleChar[CARDWIDTH, CARDHEIGHT];
            for (int x = 0; x < CARDWIDTH; x++)
            {
                r[x, 0] = new ConsoleChar('─', borderForeColour, borderBackColour);
                r[x, CARDHEIGHT - 1] = new ConsoleChar('─', borderForeColour, borderBackColour);
            }
            for (int y = 0; y < CARDHEIGHT; y++)
            {
                r[0, y] = new ConsoleChar('│', borderForeColour, borderBackColour);
                r[CARDWIDTH - 1, y] = new ConsoleChar('│', borderForeColour, borderBackColour);
            }
            r[0, 0] = new ConsoleChar('┌', borderForeColour, borderBackColour);
            r[CARDWIDTH - 1, 0] = new ConsoleChar('┐', borderForeColour, borderBackColour);
            r[0, CARDHEIGHT - 1] = new ConsoleChar('└', borderForeColour, borderBackColour);
            r[CARDWIDTH - 1, CARDHEIGHT - 1] = new ConsoleChar('┘', borderForeColour, borderBackColour);

            for (int x = 1; x < CARDWIDTH - 1; x++)
            {
                for (int y = 1; y < CARDHEIGHT - 1; y++)
                {
                    r[x, y] = new ConsoleChar(' ', SUITCOL[c.suit], CARDBACKCOL);
                }
            }

            r[1, 1].character = NUMCHARS[c.number][0];
            r[2, 1].character = NUMCHARS[c.number][1];
            if (NUMCHARS[c.number][1] == ' ')
                r[CARDWIDTH - 2, CARDHEIGHT - 2].character = NUMCHARS[c.number][0];
            else
            {
                r[CARDWIDTH - 3, CARDHEIGHT - 2].character = NUMCHARS[c.number][0];
                r[CARDWIDTH - 2, CARDHEIGHT - 2].character = NUMCHARS[c.number][1];
            }
            r[1, CARDHEIGHT - 2].character = SUITCHAR[c.suit];
            r[CARDWIDTH - 2, 1].character = SUITCHAR[c.suit];

            int imageTop = CARDHEIGHT - 2 - SUITIMGS[c.suit].Length;
            int imageLeft = (CARDWIDTH - SUITIMGS[c.suit][0].Length) / 2;
            for (int x = 0; x < SUITIMGS[c.suit][0].Length; x++)
                for (int y = 0; y < SUITIMGS[c.suit].Length; y++)
                    r[imageLeft + x, imageTop + y].character = SUITIMGS[c.suit][y][x];

            return r;
        }

        void PopulateCardBack()
        {
            CARDBACK = new ConsoleChar[CARDWIDTH, CARDHEIGHT];
            for (int x = 0; x < CARDWIDTH; x++)
            {
                CARDBACK[x, 0] = new ConsoleChar('─');
                CARDBACK[x, CARDHEIGHT - 1] = new ConsoleChar('─');
            }
            for (int y = 0; y < CARDHEIGHT; y++)
            {
                CARDBACK[0, y] = new ConsoleChar('│');
                CARDBACK[CARDWIDTH - 1, y] = new ConsoleChar('│');
            }
            CARDBACK[0, 0] = new ConsoleChar('┌');
            CARDBACK[CARDWIDTH - 1, 0] = new ConsoleChar('┐');
            CARDBACK[0, CARDHEIGHT - 1] = new ConsoleChar('└');
            CARDBACK[CARDWIDTH - 1, CARDHEIGHT - 1] = new ConsoleChar('┘');

            for (int x = 1; x < CARDWIDTH - 1; x++)
                for (int y = 1; y < CARDHEIGHT - 1; y++)
                    CARDBACK[x, y] = new ConsoleChar(' ', 15, 9);

            for (int y = 2; y < CARDHEIGHT - 2; y++)
                for (int x = 2 + (y % 2); x < CARDWIDTH - 2; x += 2) // start at 3 on every other row
                {
                    CARDBACK[x, y].character = 'X';
                }


            EMPTYSLOT = new ConsoleChar[CARDWIDTH, CARDHEIGHT];
            for (int x = 0; x < CARDWIDTH; x++)
            {
                EMPTYSLOT[x, 0] = new ConsoleChar('─');
                EMPTYSLOT[x, CARDHEIGHT - 1] = new ConsoleChar('─');
            }
            for (int y = 0; y < CARDHEIGHT; y++)
            {
                EMPTYSLOT[0, y] = new ConsoleChar('│');
                EMPTYSLOT[CARDWIDTH - 1, y] = new ConsoleChar('│');
            }
            EMPTYSLOT[0, 0] = new ConsoleChar('┌');
            EMPTYSLOT[CARDWIDTH - 1, 0] = new ConsoleChar('┐');
            EMPTYSLOT[0, CARDHEIGHT - 1] = new ConsoleChar('└');
            EMPTYSLOT[CARDWIDTH - 1, CARDHEIGHT - 1] = new ConsoleChar('┘');
            for (int x = 1; x < CARDWIDTH - 1; x++)
                for (int y = 1; y < CARDHEIGHT - 1; y++)
                    EMPTYSLOT[x, y] = new ConsoleChar(' ', 15, 0);

        }

        // originally i had a card member variable in PhysicsCard but i wanted to flex inheritence so here we are
        class PhysicsCard : Card
        {
            public (float x, float y) pos;
            public (float x, float y) v;
            public int bounce;
            public bool moving;
            public ConsoleChar[,] img;

            public PhysicsCard(int _number, int _suit, (float x, float y) _pos, (float x, float y) _v) : base(_number, _suit)
            {
                number = _number;
                suit = _suit;
                pos = _pos;
                v = _v;
                bounce = 0;
                moving = false;
                img = new ConsoleSolitaire().DrawCard(new Card(number, suit));
            }
        }
        class Card
        {
            public int number; // 1-13
            public int suit; // 0-3

            public Card(int _number, int _suit)
            {
                number = _number;
                suit = _suit;
            }
        }


    }
}
