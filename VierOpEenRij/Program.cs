using System;
using System.Collections.Generic;
using System.Linq;

namespace VierOpEenRij
{
    abstract class Node {
        public static Boolean debug = false;

        public bool TurnPlayer1 { get; protected set; }

        public abstract List<Node> PossibleMoves();

        public float Utility() {
            if (TurnPlayer1) {
                return MaximinValue();
            } else {
                 return MinimaxValue();
            }
        }

        public float MaximinValue() {
            if (Node.debug)
                Log();
            Boolean[] terminalTest = TerminalTest();
            if (terminalTest[0]) {
                // NOTE: Why return -1 when it is the AI's turn?
                // If the turn is the AI's, it has not made its move yet. Yet it encounters a terminal state, which means the other player beat us and no decision can reverse this
                if (Node.debug) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("WINNER!!!");
                    Console.ResetColor();
                }
                return terminalTest[1] ? +1 : -1;
            } else if (GameFinishedWithoutWinner()) {
                if (Node.debug) {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("TERMINAL");
                    Console.ResetColor();
                }
                return 0;
            } else {
                return PossibleMoves().Select(possibleMove => possibleMove.MinimaxValue()).Max();
            }
        }

        public float MinimaxValue() {
            if (Node.debug)
                Log();
            Boolean[] terminalTest = TerminalTest();
            if (terminalTest[0]) {
                if (Node.debug) {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("WINNER!!!");
                    Console.ResetColor();
                }
                return terminalTest[1] ? +1 : -1;
            } else if (GameFinishedWithoutWinner()) {
                if (Node.debug){
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("TERMINAL");
                    Console.ResetColor();
                }
                return 0;
            } else {
                return PossibleMoves().Select(possibleMove => possibleMove.MaximinValue()).Min();
            }
        }

        public abstract bool[] TerminalTest();
        public abstract bool GameFinishedWithoutWinner();

        public abstract void Log();

    }

    class GameBoard : Node {
        #region
        public const int ROWS = 4;
        public const int COLS = 4;
        public const string EMPTY = " "; 
        public const string PLAYER1 = "X"; 

        public const string PLAYER2 = "O"; 

        public string[,] Board { get; }

        public int Layer { get; set; }

        public int Move { get; set; }

        public int[] PlayerBoardMove { get; set; }
        #endregion

        public GameBoard(string[,] initialBoard, bool turnPlayer1, int layer = 0, int move = 0) {
            Board = initialBoard;
            TurnPlayer1 = turnPlayer1;
            Layer = layer;
            Move = move;
        }

        public override List<Node> PossibleMoves() {
            List<Node> possibleBoards = new List<Node>();
            
            int move = 0;

            for (int row = Board.GetLength(0) - 1; row >= 0 ; row--) {
                for (int col = Board.GetLength(1) - 1; col >= 0; col--) {
                    if (Board[row, col].Equals(EMPTY) && row == Board.GetLength(0) - 1) {
                        string[,] newBoard = Duplicate();
                        newBoard[row, col] = TurnPlayer1 ? PLAYER1 : PLAYER2;

                        possibleBoards.Add(new GameBoard(newBoard, !TurnPlayer1, this.Layer + 1, move));
                        move++;                        
                    }
                    else if (Board[row, col].Equals(EMPTY) && row < Board.GetLength(0) -1 && row >= 0) {
                        if (!Board[row + 1, col].Equals(EMPTY)) {
                            string[,] newBoard = Duplicate();
                            newBoard[row, col] = TurnPlayer1 ? PLAYER1 : PLAYER2;
                            
                            possibleBoards.Add(new GameBoard(newBoard, !TurnPlayer1, this.Layer + 1, move));
                            move++;
                        }
                    }
                }
            }
            return possibleBoards;
        }

        public string[,] Duplicate() {
            string[,] newGame = new string[Board.GetLength(0), Board.GetLength(1)];
            for (int row = 0; row < Board.GetLength(0); row ++) {
                for (int col = 0; col < Board.GetLength(1); col++) {
                    newGame[row, col] = Board[row,col];
                }
            }
            return newGame;
        }

        public bool PlayerMove (int col, bool turnPlayer1) {
            if (Board[0, col] == GameBoard.EMPTY) {
                for (int y = 1; y < Board.GetLength(0); y++) {
                    if (Board[y, col] != GameBoard.EMPTY) {
                        Board[y - 1, col] = turnPlayer1 ? GameBoard.PLAYER1 : GameBoard.PLAYER2;  
                        PlayerBoardMove = new int[] { y - 1, col };
                        TurnPlayer1 = turnPlayer1 ? false : true;
                        return true;
                    }
                }
            }
            Console.WriteLine("Move not possible");
            return false;
        }

        public override bool[] TerminalTest() {

            for (int row = 0; row < Board.GetLength(0); row++) {
                string rowToCheck = "";
                for (int col = 0; col < Board.GetLength(1); col++)
                    rowToCheck += Board[row, col];
                if (rowToCheck.Contains("XXXX"))
                    return new Boolean[] { true, true };
                else if (rowToCheck.Contains("OOOO"))
                    return new Boolean[] { true, false };
            }

            for (int col = 0; col < Board.GetLength(1); col++) {
                string colToCheck = "";
                for (int row = 0; row < Board.GetLength(0); row++)
                    colToCheck += Board[row, col];
                if (colToCheck.Contains("XXXX"))
                    return new Boolean[] { true, true };
                else if (colToCheck.Contains("OOOO"))
                    return new Boolean[] { true, false };
            }
            
            for (int d = 0; d < Board.GetLength(0) + Board.GetLength(1) - 1; d++) {
                string diagonalToCheck = "";

                int rowStart = d >= Board.GetLength(1) ? d - Board.GetLength(1) + 1 : 0;
                int colStart = d < Board.GetLength(1) ? Board.GetLength(1) - d - 1 : 0; 

                for (int row = rowStart, col = colStart; row < Board.GetLength(0) && col < Board.GetLength(1); row++, col++)
                {
                    diagonalToCheck += Board[row, col];
                }

                if (diagonalToCheck.Contains("XXXX"))
                    return new Boolean[] { true, true };
                else if (diagonalToCheck.Contains("OOOO"))
                    return new Boolean[] { true, false };
            }

            for (int d = 0; d < Board.GetLength(0) + Board.GetLength(1) - 1; d++)
            {
                string diagonalToCheck = "";

                int rowStart = d < Board.GetLength(0) ? d : Board.GetLength(0) - 1;
                int colStart = d >= Board.GetLength(0) ? d - Board.GetLength(0) + 1 : 0;

                for (int row = rowStart, col = colStart; row >= 0 && col < Board.GetLength(1); row--, col++)
                {
                    diagonalToCheck += Board[row, col];
                }

                if (diagonalToCheck.Contains("XXXX"))
                    return new Boolean[] { true, true };
                else if (diagonalToCheck.Contains("OOOO"))
                    return new Boolean[] { true, false };
            }

            return new Boolean[] { false, false };
        }

        public override bool GameFinishedWithoutWinner() {
            if (!(bool) TerminalTest().GetValue(0)) {
                for (int row = 0; row < Board.GetLength(0); row ++) {
                    for (int col = 0; col < Board.GetLength(1); col ++) {
                        if (Board[row, col] == EMPTY) {
                            return false; // Game not finished
                        }
                    }
                }
                return true;
            }
            return false;
        }

        public void SetPlayerBoardMove(Node previousState) {
            for (int row = 0; row < Board.GetLength(0); row++) {
                for (int col = 0; col < Board.GetLength(1); col++) {
                    if (Board[row, col] != ((GameBoard) previousState).Board[row, col])
                        PlayerBoardMove = new int[] { row, col };
                }
            }
        }

        public override void Log() {
            if (Node.debug)
                System.Console.WriteLine($"Layer: {this.Layer}, Move: {this.Move}");
            Console.WriteLine("---");
            for (int row = 0; row < Board.GetLength(0); row ++) {
                for (int col = 0; col < Board.GetLength(1); col++) {
                    if (PlayerBoardMove != null && row == PlayerBoardMove[0] && col == PlayerBoardMove[1]) {
                        if (Board[PlayerBoardMove[0], PlayerBoardMove[1]] == "X")
                            Console.ForegroundColor = ConsoleColor.Red;
                        else
                            Console.ForegroundColor = ConsoleColor.Blue;
                    }
                    Console.Write(Board[row,col]);
                    Console.ResetColor();
                    Console.Write(",");
                }
                Console.WriteLine();
            }
            Console.WriteLine("---\n");
        }
    }

    class MinimaxSearch {
        public Node MinimaxDecision(List<Node> possibleMoves) {
            // Choose node with highest possible min value, because that choice will minimize chance of losing
            return possibleMoves.OrderBy(possibleMove => possibleMove.Utility()).Last();
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            Node.debug = false;
            MinimaxSearch algorithm = new MinimaxSearch();

            #region 
            // const int ROWS = 6;
            // const int COLS = 7;

            // string[,] initialBoard = new string[ROWS, COLS];
            // for (int i = 0; i < ROWS; i++) {
            //     for (int j = 0; j <  COLS; j++) {
            //      initialBoard[i, j] = GameBoard.EMPTY;
            //     }
            // }
            #endregion

            string[,] initialBoard = new string[,] {
                { GameBoard.EMPTY, GameBoard.PLAYER2, GameBoard.EMPTY, GameBoard.EMPTY, GameBoard.EMPTY, GameBoard.EMPTY, GameBoard.PLAYER1 },
                { GameBoard.PLAYER1, GameBoard.PLAYER1, GameBoard.EMPTY, GameBoard.EMPTY, GameBoard.EMPTY, GameBoard.PLAYER2, GameBoard.PLAYER2 },
                { GameBoard.PLAYER2, GameBoard.PLAYER1, GameBoard.EMPTY, GameBoard.EMPTY, GameBoard.PLAYER2, GameBoard.PLAYER1, GameBoard.PLAYER2 },
                { GameBoard.PLAYER1, GameBoard.PLAYER1, GameBoard.EMPTY, GameBoard.EMPTY, GameBoard.PLAYER1, GameBoard.PLAYER2, GameBoard.PLAYER2 },
                { GameBoard.PLAYER2, GameBoard.PLAYER2, GameBoard.PLAYER1, GameBoard.PLAYER2, GameBoard.PLAYER1, GameBoard.PLAYER1, GameBoard.PLAYER1 },
                { GameBoard.PLAYER2, GameBoard.PLAYER1, GameBoard.PLAYER2, GameBoard.PLAYER1, GameBoard.PLAYER1, GameBoard.PLAYER2, GameBoard.PLAYER2 }
            };

            // We assume the AI starts first and tries to maximize its value. This is actually maximin instead of minimax, but the gist is the same
            Node currState = new GameBoard(initialBoard, true); // Turn player 1
            currState.Log();

            while (!(bool) currState.TerminalTest().GetValue(0) && !currState.GameFinishedWithoutWinner()) {

                GameBoard testBoard = new GameBoard(initialBoard, true);
                if (Node.debug) {
                    testBoard.Log();
                    float ut = testBoard.Utility();
                    Console.WriteLine("CALCULATION DONE");
                }

                List<Node> possibleMoves = currState.PossibleMoves();
                if (Node.debug) {
                    foreach (Node move in possibleMoves)
                    {
                        move.Log();
                    }
                }

                Node previousState = currState;
                currState = algorithm.MinimaxDecision(possibleMoves);
                ((GameBoard) currState).SetPlayerBoardMove(previousState);
                
                if(Node.debug)
                    Console.WriteLine("CALCULATION DONE");
                
                currState.Log();

                if ((bool) currState.TerminalTest().GetValue(0) || currState.GameFinishedWithoutWinner()) {
                    break;
                }

                bool moveIsPossible = false;
                while (!moveIsPossible) {
                    // Wait for player input
                    Console.WriteLine("Give row");
                    int input = Convert.ToInt32(Console.ReadLine());
                    Console.Write("\n");
                    moveIsPossible = ((GameBoard) currState).PlayerMove(input, false);
                    currState.Log();
                }
            }

            Console.WriteLine("Game done");
            if (currState.GameFinishedWithoutWinner())
                Console.WriteLine("TIE");
            else if((bool) currState.TerminalTest().GetValue(1))
                Console.WriteLine("AI WINS");
            else
                Console.WriteLine("YOU WIN");
            Console.ReadLine();
        }
    }
}