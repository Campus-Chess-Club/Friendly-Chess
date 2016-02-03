using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Friendly_Chess
{
    public partial class frmChess : Form
    {

        public struct valueRange
        {
            public float minValue;
            public float maxValue;

            public valueRange(float value)
            {
                minValue = value;
                maxValue = value;
            }

            public static valueRange operator -(valueRange x)
            {
                valueRange result;
                result.minValue = -x.maxValue;
                result.maxValue = -x.minValue;
                return result;
            }
        }

        public class NodeData
        {
            public float value;
            public float alpha;
            public float beta;
            public float depth;
            public long[] sortedMoves;
            public float[] moveValues;
            //public string[] names;
            public GameState state;
        }

        public class Game
        {
            public int[] board;
            public int[] pieces;
            public int pieceCount;
            public int playerOnTurn;
            public int ply;
            public int moves50;

            public Game Clone()
            {
                Game newGame = new Game();
                newGame.pieceCount = pieceCount;
                newGame.playerOnTurn = playerOnTurn;
                newGame.ply = ply;
                newGame.moves50 = moves50;
                newGame.pieces = new int[pieceCount + 2];
                newGame.board = new int[129];
                for (int i = 1; i <= pieceCount; i++)
                {
                    newGame.pieces[i] = pieces[i];
                    newGame.board[pieces[i] >> squareOffset] = i;
                }
                newGame.board[128] = board[128];
                return newGame;
            }

            public int[] Serialize()
            {
                int[] result = new int[pieceCount + 2];
                result[0] = -1;
                result[pieceCount + 1] = -1;
                for (int i = 1; i <= pieceCount; i++)
                {
                    result[i] = pieces[i];
                }
                Array.Sort(result);
                result[1] = board[128];
                /*
                if (epData == 0)
                {
                    result[1] = 0;
                }
                else
                {
                    int epPieceIndex = epData & 63;
                    int epCard = pieces[epPieceIndex];
                    int epSquare = epData >> squareOffset;
                    int epNewPieceIndex = Array.BinarySearch(result, epCard);
                    result[1] = (epNewPieceIndex | (epSquare << squareOffset));
                }
                 */
                result[0] = playerOnTurn;
                return result;
            }

            public Game(int[] serializedData)
            {
                pieceCount = serializedData.Length - 2;
                pieces = new int[pieceCount + 1];
                board = new int[129];
                playerOnTurn = serializedData[0];
                board[128] = serializedData[1];
                for (int i = 1; i <= pieceCount; i++)
                {
                    pieces[i] = serializedData[i + 1];
                    board[pieces[i] >> squareOffset] = i;
                }
            }

            public Game(string FEN)
            {
                string[] fenParts = FEN.Split(' ');

                string boardPart = fenParts[0];
                string turnPart = fenParts[1];
                string castlePart = fenParts[2];
                string epPart = fenParts[3];
                string moves50Part = fenParts[4];
                string halfplyPart = fenParts[5];
                pieces = new int[33];
                board = new int[129];

                bool[] deflowered = new bool[128];

                this.moves50 = Convert.ToInt32(moves50Part);

                int fullMoves = Convert.ToInt32(halfplyPart);

                if (turnPart == "w")
                {
                    playerOnTurn = 8;
                    ply = 2 * fullMoves - 2;
                }
                else
                {
                    playerOnTurn = 16;
                    ply = 2 * fullMoves - 1;
                }

                if (epPart != "-")
                {
                    board[128] = LookupSquare(epPart);
                }

                deflowered[0] = true;
                deflowered[4] = true;
                deflowered[7] = true;
                deflowered[112] = true;
                deflowered[116] = true;
                deflowered[119] = true;

                if (castlePart != "-")
                {
                    foreach (char right in castlePart)
                    {
                        switch (right)
                        {
                            case 'K':
                                deflowered[4] = false;
                                deflowered[7] = false;
                                break;
                            case 'Q':
                                deflowered[4] = false;
                                deflowered[0] = false;
                                break;
                            case 'k':
                                deflowered[116] = false;
                                deflowered[119] = false;
                                break;
                            case 'q':
                                deflowered[116] = false;
                                deflowered[112] = false;
                                break;
                            default:
                                break;
                        }
                    }
                }

                string[] rows = boardPart.Split('/');
                for (int i = 0; i < rows.Length; i++)
                {
                    int rank = 7 - i;
                    int file = 0;
                    foreach (char symbol in rows[i])
                    {
                        int pieceType = LookupPiece(symbol);
                        {
                            if ((pieceType & 15) == pieceType)
                            {
                                int square = (rank << 4) | file;
                                if ((pieceType&8)==0)
                                {
                                    pieceType = pieceType | 16;
                                }
                                if (!deflowered[square])
                                {
                                    switch (pieceType&7)
                                    {
                                        case 1:
                                            if (rank==1)
                                            {
                                                pieceType = pieceType | 32;
                                            }
                                            break;
                                        case 2:
                                            if (rank==6)
                                            {
                                                pieceType = pieceType | 32;
                                            }
                                            break;
                                        default:
                                            pieceType = pieceType | 32;
                                            break;
                                    }
                                }
                                pieceCount++;
                                pieces[pieceCount] = pieceType | (square << squareOffset);
                                board[square] = pieceCount;
                                file++;
                            }
                            else if ((pieceType & 32) == 0)
                            {
                                file += (pieceType - 16);
                            }
                            
                        }

                    }
                }

            }
            
            public string FEN()
            {
 
                // Board part
                StringBuilder sb = new StringBuilder();
                int square = 112;
                int spaces = 0;
                while ((square & 136) == 0)
                {
                    if (square != 112 && ((square & 7) == 0))
                    {
                        sb.Append("/");
                    }
                    int pieceType = pieces[board[square]] & 15;
                    if (pieceType == 0)
                    {
                        spaces++;
                    }
                    else
                    {
                        if (spaces > 0)
                        {
                            sb.Append(spaces.ToString());
                            spaces = 0;
                        }
                        sb.Append(plainSymbols[pieceType]);
                    }
                    square++;
                    if ((square & 8) != 0)
                    {
                        square -= 24;
                        if (spaces > 0)
                        {
                            sb.Append(spaces.ToString());
                            spaces = 0;
                        }

                    }
                }

                // Turn Part
                if (playerOnTurn==8)
                {
                    sb.Append(" w");
                }
                else
                {
                    sb.Append(" b");
                }

                // Castle Part

                sb.Append(" ");
                bool castleOK = false;

                if ((pieces[board[4]]&63) == 44)
                {
                    if ((pieces[board[7]]&63) == 46)
                    {
                        sb.Append("K");
                        castleOK = true;
                    }
                    if ((pieces[board[0]] & 63) == 46)
                    {
                        sb.Append("Q");
                        castleOK = true;
                    }
                }
                if ((pieces[board[116]] & 63) == 52)
                {
                    if ((pieces[board[119]] & 63) == 54)
                    {
                        sb.Append("k");
                        castleOK = true;
                    }
                    if ((pieces[board[112]] & 63) == 54)
                    {
                        sb.Append("q");
                        castleOK = true;
                    }
                }
                if (!castleOK)
                {
                    sb.Append ("-");
                }

                // EP Part
                square = board[128];
                if (square == 0)
                {
                    sb.Append (" -");
                }
                else
                {
                    string squareName = SquareName(square);
                    sb.Append(" " + squareName);
                }

                // Moves50 Part
                sb.Append(" " + moves50.ToString());

                // Full Moves Part
                sb.Append(" " + (1 + ply / 2).ToString());

                return sb.ToString();
            }
            
            public Game()
            {

            }
        }

        // I have to do this because C# is stupid and won't allow me to use an int array as a native key for a dictionary
        public class MyEqualityComparer : IEqualityComparer<int[]>
        {
            public bool Equals(int[] x, int[] y)
            {
                if (x.Length != y.Length)
                {
                    return false;
                }
                for (int i = 0; i < x.Length; i++)
                {
                    if (x[i] != y[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            public int GetHashCode(int[] obj)
            {
                int result = 17;
                for (int i = 0; i < obj.Length; i++)
                {
                    unchecked
                    {
                        result = result * 23 + obj[i];
                    }
                }
                return result;
            }
        }

        public string ArrayToString(int[] array)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append(array[0].ToString());
            for (int i = 1; i < array.Length; i++)
            {
                sb.Append(",");
                sb.Append(array[i].ToString());
            }
            sb.Append("}");
            return sb.ToString();
        }

        public enum GameState { Ongoing, Illegal, WhiteWin, Draw, BlackWin, Uninitialized }

        RadioButton[] radiobuttons = new RadioButton[0];

        const int white = 8;
        const int black = 16;
        const int squareOffset = 6;
        const int move1Offset = 14;
        const int move1SquareOffset = move1Offset + squareOffset;
        const int move2Offset = 28;
        const int move2SquareOffset = move2Offset + squareOffset;
        const int move3Offset = 42;
        const int move3SquareOffset = move1Offset + squareOffset;
        const int epOffset = 56;


        //const string symbols = "-?pnkbrq?P?NKBRQ";
        const string symbols = " ?♟♞♚♝♜♛?♙?♘♔♗♖♕012345678";
        const string plainSymbols = " ?pnkbrq?P?NKBRQ012345678";
        //const string shortSymbols = "?  NKBRQ";
        Game curGame = new Game();
        int[] directionVectors = { -16, -15, -17, 0, 1, 16, 0, 1, 16, 15, 17, 0, 14, 18, 31, 33, 0 };
        int[] directory = { 7, -1, 11, 6, 8, 3, 6 };
        float[] pieceValues = { 0, -1, -1, -3, -20, -3, -5, -9, 0, 1, 1, 3, 20, 3, 5, 9 };
        Int64[] moves = null;
        string[] names = null;
        int legalMoves = 0;
        string pendingMove = "";
        Random r = new Random();
        int globalIndexer = 0;
        Dictionary<int[], NodeData> transpositionTable = new Dictionary<int[], NodeData>(new MyEqualityComparer());
        Dictionary<int[], long> perftTable = new Dictionary<int[], long>(new MyEqualityComparer());

        long[] movesTemp = new Int64[400];
        string[] namesTemp = new String[400];
        int[] promotionChoices = { 7, 6, 5, 3 };
        long[] moveQueue = new long[4];
        string[] nameQueue = new string[4];

        //int[][] directions = { new int[] { 1, -1, 16, -16 }, new int[] { 15, 17 }, new int[] { -15, -17 }, new int[] { 14, 18, 31, 33, -14, -18, -31, -33 } };
        int[] directions = { 1, -1, 16, -16, 15, 17, -15, -17, 14, 18, 31, 33, -14, -18, -31, -33 };
        int[] directionStyles = { 0, 0, 0, 0, 1, 1, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3 };
        int[,] shortRange = { { 4, 0 }, { 4, 2 }, { 4, 1 }, { 3, 0 } };
        int[,] longRange = { { 6, 7 }, { 5, 7 }, { 5, 7 }, { 0, 0 } };

        int[] checkDirectionIndices = new int[16];
        int checkDirectionCount = 0;

        //bool[] threatMap = new bool[128];

        Label[] visBoard = new Label[128];

        public frmChess()
        {
            InitializeComponent();
        }

        private string BoardRepresentation(Game game)
        {
            StringBuilder sb = new StringBuilder();
            int square = 112;
            while ((square & 136) == 0)
            {
                if (square != 112 && ((square & 7) == 0))
                {
                    sb.Append("\r\n");
                }
                sb.Append(symbols[game.pieces[game.board[square]] & 15]);
                //square = (square + 9) & (~8);
                square++;
                if ((square & 8) != 0)
                {
                    square -= 24;
                }
            }
            return sb.ToString();
        }

        public static string SquareName(int square)
        {
            int rank = (square >> 4);
            int file = (square & 7);
            string fileNames = "abcdefgh";
            string rankNames = "12345678";
            return (fileNames[file].ToString() + rankNames[rank].ToString());
        }

        public static int LookupPiece(char piece)
        {
            for (int i = 0; i < plainSymbols.Length; i++)
            {
                if (plainSymbols[i] == piece)
                {
                    return i;
                }
            }
            return -1;
        }

        public static int LookupSquare(string squareName)
        {
            int square = 0;
            while ((square & 136) == 0)
            {
                if (SquareName(square) == squareName)
                {
                    return square;
                }
                square = (square + 9) & 247;
            }
            return -1;
        }

        public string MoveName(long move)
        {
            long bitmap = (1 << move1Offset) - 1;

            int sourceCard1 = (int)(move & bitmap);
            int destCard1 = (int)((move >> move1Offset) & bitmap);

            int sourceSquare;
            int destSquare;

            sourceSquare = sourceCard1 >> squareOffset;
            destSquare = destCard1 >> squareOffset;

            return SquareName(sourceSquare) + SquareName(destSquare);
        }

        private GameState GenerateMoves(Game game, out long[] moves, bool justCheckingCheck, int qsSquare = -1)
        {
            moves = null;
            if (!justCheckingCheck && game.moves50 >= 75)
            {
                moves = new long[0];
                //names = new string[0];
                return GameState.Draw;
            }
            int[] board = game.board;
            int[] pieces = game.pieces;
            int pieceCount = game.pieceCount;
            int playerOnTurn = game.playerOnTurn;
            int legalMoves = 0;
            long[] dummyMoves = null;
            //string[] dummyNames = null;


            Game tempGame = game.Clone();
            tempGame.playerOnTurn = 24 ^ tempGame.playerOnTurn;

            bool isInCheck = false;
            if (!justCheckingCheck)
            {
                isInCheck = (GenerateMoves(tempGame, out dummyMoves, true) == GameState.Illegal);
                CheckThreats(game);
            }


            for (int i = 1; i <= pieceCount; i++)
            {
                int piece = pieces[i];
                int initialSquare = (piece >> squareOffset);
                if ((piece & playerOnTurn) != 0)
                {
                    int pieceType = piece & 7;
                    bool isPawn = (pieceType < 3);
                    int directionIndex = directory[pieceType - 1] + 1;
                    int direction = directionVectors[directionIndex];
                    bool breaking = false;
                    while (!breaking)
                    {
                        bool breakInner = false;
                        int square = initialSquare;
                        bool firstMove = true;
                        while (!breakInner)
                        {
                            square += direction;
                            if ((square & 136) != 0)
                            {
                                breakInner = true;
                            }
                            else
                            {
                                int targetPieceIndex = board[square];
                                int targetPiece = pieces[targetPieceIndex];
                                bool capture = (((targetPiece & 24) ^ playerOnTurn) == 24);
                                bool obstruct = (((targetPiece & 24) & playerOnTurn) != 0);

                                if (justCheckingCheck)
                                {
                                    if ((targetPiece & 7) == 4 && capture && !(isPawn && ((direction & 1) == 0)))
                                    {
                                        return GameState.Illegal;
                                    }
                                    if (obstruct || capture || pieceType < 5)
                                    {
                                        breakInner = true;
                                    }
                                }
                                else
                                {
                                    int epSquare = board[128];
                                    if (square == epSquare && epSquare != 0 && isPawn)
                                    {
                                        int epCaptureSquare = epSquare + (((epSquare & 64) == 0) ? 16 : -16);
                                        targetPieceIndex = board[epCaptureSquare];
                                        targetPiece = pieces[targetPieceIndex];
                                        capture = true;
                                       
                                    }
                                    if (obstruct)
                                    {
                                        breakInner = true;
                                    }
                                    else if (isPawn && (((direction & 1) != 0) != capture))
                                    {
                                        breakInner = true;
                                    }
                                    else if (pieceType == 4 && !firstMove && capture)
                                    {
                                        breakInner = true;
                                    }
                                    else
                                    {
                                        int promotedPiece;
                                        bool promotionChoice = isPawn && ((square >> 4) == 0 || (square >> 4) == 7);
                                        promotedPiece = (pieceType | playerOnTurn);

                                        //Int64 move = -1;
                                        //string name = "";
                                        if (capture)
                                        {
                                            if (!promotionChoice)
                                            {
                                                long move1 = (long)(piece | (promotedPiece << move1Offset) | (square << move1SquareOffset));
                                                long move2 = (long)(targetPiece);
                                                moveQueue[0] = ((move2 << move2Offset) | move1);
                                                
                                                //moveQueue[0] = ((long)(targetPieceIndex | (i << move1Offset) | (square << move1SquareOffset))) | ((long)promotedPiece << move3Offset);
                                                //name = shortSymbols[pieceType]+ SquareName(initialSquare) + "x" + SquareName(square);
                                                //nameQueue[0] = SquareName(initialSquare) + SquareName(square);
                                                breakInner = true;
                                            }
                                            else
                                            {
                                                int virtualDirection = (pieceType == 1) ? -16 : 16;
                                                int virtualSquare = square;
                                                for (int j = 0; j < promotionChoices.Length; j++)
                                                {
                                                    promotedPiece = (promotionChoices[j] | playerOnTurn);
                                                    long move1 = (long)(piece | (promotedPiece << move1Offset) | (square << move1SquareOffset));
                                                    long move2 = (long)(targetPiece | (square << squareOffset));
                                                    moveQueue[j] = ((move2 << move2Offset) | move1);
                                                    
                                                    //moveQueue[j] = ((long)(targetPieceIndex | (i << move1Offset) | (square << move1SquareOffset))) | ((long)promotedPiece << move3Offset);
                                                    //name = shortSymbols[pieceType]+ SquareName(initialSquare) + "x" + SquareName(square);
                                                    //nameQueue[j] = SquareName(initialSquare) + SquareName(virtualSquare);
                                                    virtualSquare += virtualDirection;
                                                }
                                            }

                                        }
                                        else
                                        {
                                            if (pieceType == 4 && !firstMove)
                                            {
                                                int scanSquare = square;
                                                int scannedPieceIndex = 0;
                                                while ((scanSquare & 136) == 0 && scannedPieceIndex == 0)
                                                {
                                                    scanSquare += direction;
                                                    if (scanSquare >= 0)
                                                    {
                                                        scannedPieceIndex = board[scanSquare];
                                                    }
                                                }
                                                int scannedPiece = pieces[scannedPieceIndex] & 63;
                                                if ((scannedPiece & 39) == 38) // Virgin rook of any color
                                                {
                                                    long move1 = (long)(piece | (promotedPiece << move1Offset) | (square << move1SquareOffset));
                                                    long move2 = (long)(scannedPiece | (scanSquare << squareOffset) | ((scannedPiece & 31) << move1Offset) | ((square - direction) << move1SquareOffset));
                                                    moveQueue[0] = ((move2 << move2Offset) | move1);

                                                    //moveQueue[0] = (i << move1Offset) | (square << move1SquareOffset) | ((long)scannedPieceIndex << move2Offset) | ((long)(square - direction) << move2SquareOffset) | ((long)promotedPiece << move3Offset);
                                                    //nameQueue[0] = SquareName(initialSquare) + SquareName(square);
                                                }
                                                else
                                                {
                                                    moveQueue[0] = -1;
                                                }
                                            }
                                            else
                                            {
                                                if (!promotionChoice)
                                                {
                                                    long move1 = (long)(piece | (promotedPiece << move1Offset) | (square << move1SquareOffset));
                                                    moveQueue[0] = move1;

                                                    //moveQueue[0] = (i << move1Offset) | (square << move1SquareOffset) | ((long)promotedPiece << move3Offset);
                                                    //string name = shortSymbols[pieceType]+SquareName(initialSquare) + "-" + SquareName(square);
                                                    //nameQueue[0] = SquareName(initialSquare) + SquareName(square);
                                                }
                                                else
                                                {
                                                    int virtualDirection = (pieceType == 1) ? -16 : 16;
                                                    int virtualSquare = square;
                                                    for (int j = 0; j < promotionChoices.Length; j++)
                                                    {
                                                        promotedPiece = (promotionChoices[j] | playerOnTurn);
                                                        long move1 = (long)(piece | (promotedPiece << move1Offset) | (square << move1SquareOffset));
                                                        moveQueue[j] = move1;

                                                        //moveQueue[j] = (i << move1Offset) | (square << move1SquareOffset) | ((long)promotedPiece << move3Offset);
                                                        //name = shortSymbols[pieceType]+ SquareName(initialSquare) + "x" + SquareName(square);
                                                        //nameQueue[j] = SquareName(initialSquare) + SquareName(virtualSquare);
                                                        virtualSquare += virtualDirection;
                                                    }

                                                }
                                            }
                                        }
                                        bool legalPosition = true;
                                        if (moveQueue[0] == -1)
                                        {
                                            legalPosition = false;
                                        }
                                        else
                                        {
                                            tempGame = ExecuteMove(game, moveQueue[0]);
                                            //legalPosition = (GenerateMoves(tempGame, ref dummyMoves, ref dummyNames, true) != GameState.Illegal);
                                            legalPosition = (!CheckCheck(tempGame, !(pieceType == 4 | (isPawn && epSquare==square))));
                                        }
                                        if (legalPosition)
                                        {
                                            if (!promotionChoice)
                                            {
                                                movesTemp[legalMoves] = moveQueue[0] | (((long)epSquare) << epOffset);
                                                //namesTemp[legalMoves] = nameQueue[0];
                                                legalMoves++;
                                            }
                                            else
                                            {
                                                for (int j = 0; j < promotionChoices.Length; j++)
                                                {
                                                    movesTemp[legalMoves] = moveQueue[j] | (((long)epSquare) << epOffset);
                                                    //namesTemp[legalMoves] = nameQueue[j];
                                                    legalMoves++;
                                                }
                                            }
                                        }


                                        if (pieceType < 5)
                                        {
                                            if ((piece & 32) == 0)
                                            {
                                                breakInner = true;
                                            }
                                            else
                                            {
                                                bool continueCastleCheck = (pieceType == 4 && (direction == 1 || direction == -1) && !isInCheck && legalPosition);
                                                if ((!isPawn && !continueCastleCheck) || !firstMove || ((piece & 32) == 0))
                                                {
                                                    breakInner = true;
                                                }
                                            }
                                        }
                                    }
                                    firstMove = false;
                                }
                            }
                        }
                        if (direction > 0 && pieceType > 2)
                        {
                            direction = -direction;
                        }
                        else
                        {
                            directionIndex++;
                            direction = directionVectors[directionIndex];
                            if (direction == 0)
                            {
                                breaking = true;
                            }
                        }
                    }
                }
            }
            if (justCheckingCheck)
            {
                return GameState.Ongoing;
            }
            moves = new long[legalMoves];
            //names = new string[legalMoves];
            if (legalMoves > 0)
            {
                for (int i = 0; i < legalMoves; i++)
                {
                    moves[i] = movesTemp[i];
                    //names[i] = namesTemp[i];
                }
                return GameState.Ongoing;
            }
            else
            {
                if (isInCheck)
                {
                    if (playerOnTurn == white)
                    {
                        return GameState.BlackWin;
                    }
                    else
                    {
                        return GameState.WhiteWin;
                    }
                }
                else
                {
                    return GameState.Draw;
                }
            }

        }

        public void PositionLabels()
        {
            int wid = 60;
            int hgt = 60;
            for (int i = 0; i < visBoard.Length; i++)
            {
                if ((i & 136) == 0)
                {
                    Label curLabel = visBoard[i];
                    int file = (i & 7);
                    int rank = (i >> 4);
                    if (chkRotate.Checked)
                    {
                        file = 7 - file;
                    }
                    else
                    {
                        rank = 7 - rank;
                    }
                    curLabel.Left = 10 + wid * file;
                    curLabel.Top = 100 + hgt * rank;
                    curLabel.Width = wid;
                    curLabel.Height = hgt;
                    curLabel.Font = new Font("Chess Merida Unicode", 42);
                }
            }

        }

        private void frmChess_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < 128; i++)
            {
                if ((i & 136) == 0)
                {
                    Label curLabel = new Label();
                    visBoard[i] = curLabel;
                    curLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
                    curLabel.Name = i.ToString();
                    curLabel.Click += visBoard_Click;
                    //curLabel.MouseDown += visBoard_MouseDown;
                    bool black = (((i & 1) ^ ((i >> 4) & 1)) == 0);
                    if (black)
                    {
                        curLabel.BackColor = Color.LightGray;
                    }
                    this.Controls.Add(curLabel);
                }
            }
            PositionLabels();
            StartGame();
            RefreshBoard();
            ActiveComputerPlay();
        }

        public void StartGame()
        {
            /*
            curGame = new Game();
            int[] initialSet = { 6, 3, 5, 7, 4, 5, 3, 6 };
            curGame.pieceCount = 0;
            curGame.pieces = new int[33];
            curGame.board = new int[129];
            int[] squareList = new int[32];
            int[] pieceTypeList = new int[32];
            for (int i = 0; i < 8; i++)
            {
                squareList[i] = i;
                squareList[i | 8] = 16 | i;
                squareList[i | 16] = 96 | i;
                squareList[i | 24] = 112 | i;
                pieceTypeList[i] = (32 | white | initialSet[i]);
                pieceTypeList[i | 8] = white | 33;
                pieceTypeList[i | 16] = black | 34;
                pieceTypeList[i | 24] = (32 | black | initialSet[i]);
            }

            for (int i = 0; i < 32; i++)
            {
                int square = squareList[i];
                int pieceType = pieceTypeList[i];
                curGame.pieceCount++;
                curGame.pieces[curGame.pieceCount] = ((square << squareOffset) | pieceType);
                curGame.board[square] = curGame.pieceCount;
            }
            curGame.playerOnTurn = white;
             */

            string FEN;

            //Starting Position
            //int[] data = { 8, 0, 46, 299, 557, 815, 1068, 1325, 1579, 1838, 4137, 4393, 4649, 4905, 5161, 5417, 5673, 5929, 24626, 24882, 25138, 25394, 25650, 25906, 26162, 26418, 28726, 28979, 29237, 29495, 29748, 30005, 30259, 30518 };
            FEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

            //Pawn W
            //int[] data = { 8,0,21257, 21769, 25097, 25609, 26121, 13324, 21524 };
            //FEN = "8/2P1P1P1/3PkP2/8/4K3/8/8/8 w - - 0 1";
            
            // 4-move mate problem
            //int[] data = { 8, 0, 1292, 8971, 14093, 9230, 1812, 9494, 5394, 5906 };
            //FEN = "8/8/8/8/7B/3NRr2/5p1p/5K1k w - - 0 1";

            // Rank mate
            //int[] data = { 8, 0, 524, 16654, 20494, 26388 };
            //FEN = "8/7k/R7/1R6/8/8/8/2K5 w - - 0 1";

            //int[] data = { 16, 0, 46, 815, 1294, 1548, 4137, 4393, 4649, 5417, 5673, 5929, 9483, 12306, 12813, 13065, 13321, 16651, 18194, 20499, 21522, 21773, 25138, 25394, 28726, 29237, 29495, 29716 };
            //int[] data = { 8, 0, 46, 299, 557, 815, 1068, 1325, 1838, 4137, 4393, 4649, 4905, 5417, 5673, 5929, 17419, 17673, 21010, 21779, 24626, 24882, 25394, 26162, 26418, 28726, 28979, 29237, 29495, 29748, 30005, 30518 };
            //int[] data = { 8, 21260, 14, 1548, 5133, 5417, 8713, 9737, 9999, 12553, 13065, 14093, 17170, 17419, 21011, 21522, 24882, 25138, 26391, 29237, 29748, 30005, 30486 };
            
            curGame = new Game(FEN);
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            StartGame();
            RefreshBoard();
            this.Refresh();
            ActiveComputerPlay();
        }

        public void RefreshBoard()
        {
            //lblBoard.Text = BoardRepresentation(curGame);

            for (int i = 0; i < 128; i++)
            {
                if ((i & 136) == 0)
                {
                    visBoard[i].Text = symbols[curGame.pieces[curGame.board[i]] & 15].ToString();
                }
            }

            lblOnTurn.Text = (curGame.playerOnTurn == white) ? "White" : "Black";
            lblPly.Text = curGame.ply.ToString();
            //lblValue.Text = Value(curGame, 2).ToString();

            /*
            foreach (RadioButton radio in radiobuttons)
            {
                this.Controls.Remove(radio);
            }
             */

            GenerateMoves(curGame, out moves, ref names, false);
            lblMoves.Text = "";
            foreach (string name in names)
            {
                lblMoves.Text += name + "\r\n";
            }
            legalMoves = moves.Length;

            /*
            radiobuttons = new RadioButton[legalMoves];
            for (int i = 0; i < legalMoves; i++)
            {
                RadioButton curRadio = new RadioButton();
                radiobuttons[i] = curRadio;
                this.Controls.Add(curRadio);
                curRadio.Left = 400;
                curRadio.Top = 30 + 20 * i;
                curRadio.Text = names[i];
                curRadio.Font = lblMoves.Font;
            }
             */
        }

        private void btnMove_Click(object sender, EventArgs e)
        {

            /*
            int moveIndex = -1;
            for (int i = 0; i < legalMoves; i++)
            {
                if (radiobuttons[i].Checked)
                {
                    moveIndex = i;
                    break;
                }
            }
            if (moveIndex == -1)
            {
                MessageBox.Show("No move selected");
            }
            else
            {
                //MessageBox.Show(moveIndex.ToString() + ", " + moves[moveIndex].ToString() + ", " + names[moveIndex]);
                curGame = ExecuteMove(curGame, moves[moveIndex]);
                RefreshBoard();
            }
             * */

            SubmitMove(Convert.ToInt64(txtMove.Text));
        }

        public Game ExecuteMoveOld(Game game, long move)
        {
            Game newGame = game.Clone();

            newGame.moves50++;

            int capturedPiece = (int)(move & 63);
            int movedPiece = (int)((move >> move1Offset) & 63);
            int movedPiece2 = (int)((move >> move2Offset) & 63);
            int promotion = (int)((move >> move3Offset) & 63);

            newGame.board[128] = 0;

            if (capturedPiece != 0)
            {
                newGame.board[newGame.pieces[capturedPiece] >> squareOffset] = 0;
                if (capturedPiece != newGame.pieceCount)
                {
                    newGame.pieces[capturedPiece] = newGame.pieces[newGame.pieceCount];
                    newGame.board[newGame.pieces[capturedPiece] >> squareOffset] = capturedPiece;
                    if (movedPiece == newGame.pieceCount)
                    {
                        movedPiece = capturedPiece;
                    }
                }

                newGame.pieceCount--;
                newGame.moves50 = 0;
            }
            if (movedPiece != 0)
            {
                int movedPieceCard = (newGame.pieces[movedPiece]);
                int sourceSquare = (movedPieceCard >> squareOffset);
                int movedSquare = (int)((move >> move1SquareOffset) & 127);
                if ((movedPieceCard & 7) < 3)
                {
                    newGame.moves50 = 0;
                    if ((movedPieceCard & 32) != 0)
                    {
                        int squareDelta = sourceSquare - movedSquare;
                        if (squareDelta == 32 | squareDelta == -32)
                        {
                            int epSquare = (sourceSquare + movedSquare) / 2;
                            newGame.board[128] = (movedPiece | (epSquare << squareOffset));
                        }
                    }
                }

                newGame.board[sourceSquare] = 0;
                newGame.board[movedSquare] = movedPiece;
                newGame.pieces[movedPiece] = (promotion | (movedSquare << squareOffset));

            }
            if (movedPiece2 != 0)
            {
                newGame.board[newGame.pieces[movedPiece2] >> squareOffset] = 0;
                int movedSquare = (int)((move >> move2SquareOffset) & 127);
                newGame.board[movedSquare] = movedPiece2;
                newGame.pieces[movedPiece2] = ((newGame.pieces[movedPiece2] & 31) | (movedSquare << squareOffset));
            }
            newGame.playerOnTurn = 24 ^ newGame.playerOnTurn;
            newGame.ply++;
            return newGame;
        }

        public Game ExecuteMove(Game game, long move, bool inPlace=false)
        {
            Game newGame;
            if (inPlace)
            {
                newGame = game;
            }
            else
            {
                newGame = game.Clone();
            }

            newGame.moves50++;

            long bitmap = (1 << move1Offset) - 1;

            int sourceCard1 = (int)(move & bitmap);
            int destCard1 = (int)((move >> move1Offset) & bitmap);
            int sourceCard2 = (int)((move >> move2Offset) & bitmap);
            int destCard2 = (int)((move >> move3Offset) & bitmap);

            int sourceSquare;
            int destSquare;
            int pieceIndex;
            if (sourceCard2 != 0)
            {
                sourceSquare = sourceCard2 >> squareOffset;
                pieceIndex = newGame.board[sourceSquare];
                if (destCard2 == 0)
                {
                    int subPieceIndex = newGame.pieceCount;
                    int subPieceCard = newGame.pieces[subPieceIndex];
                    int subSquare = subPieceCard >> squareOffset;
                    newGame.pieces[pieceIndex] = subPieceCard;
                    newGame.board[subSquare] = pieceIndex;
                    newGame.board[sourceSquare] = 0;
                    newGame.pieceCount--;
                }
                else
                {
                    destSquare = destCard2 >> squareOffset;
                    newGame.pieces[pieceIndex] = destCard2;
                    newGame.board[sourceSquare] = 0;
                    newGame.board[destSquare] = pieceIndex;
                }
            }

            sourceSquare = sourceCard1 >> squareOffset;
            pieceIndex = newGame.board[sourceSquare];
            destSquare = destCard1 >> squareOffset;
            newGame.pieces[pieceIndex] = destCard1;
            newGame.board[sourceSquare] = 0;
            newGame.board[destSquare] = pieceIndex;

            newGame.board[128] = 0;

            if ((sourceCard1 & 7) < 3)
            {
                newGame.moves50 = 0;
                if ((sourceCard1 & 32) != 0)
                {
                    int squareDelta = sourceSquare - destSquare;
                    if (squareDelta == 32 | squareDelta == -32)
                    {
                        int epSquare = (sourceSquare + destSquare) >> 1;
                        newGame.board[128] = epSquare;
                    }
                }
            }

            newGame.playerOnTurn = 24 ^ newGame.playerOnTurn;
            newGame.ply++;
            return newGame;
        }

        public Game ReverseMove(Game game, long move, bool inPlace = false)
        {
            Game newGame;
            if (inPlace)
            {
                newGame = game;
            }
            else
            {
                newGame = game.Clone();
            }

            if (newGame.moves50 <= 0)
            {
                newGame.moves50 = 0;
            }
            else
            {
                newGame.moves50--;
            }

            long bitmap = (1 << move1Offset) - 1;

            int sourceCard1 = (int)(move & bitmap);
            int destCard1 = (int)((move >> move1Offset) & bitmap);
            int sourceCard2 = (int)((move >> move2Offset) & bitmap);
            int destCard2 = (int)((move >> move3Offset) & bitmap);

            int sourceSquare;
            int destSquare;
            int pieceIndex;

            sourceSquare = sourceCard1 >> squareOffset;
            destSquare = destCard1 >> squareOffset;
            pieceIndex = newGame.board[destSquare];
            newGame.pieces[pieceIndex] = sourceCard1;
            newGame.board[destSquare] = 0;
            newGame.board[sourceSquare] = pieceIndex;

            if (sourceCard2 != 0)
            {
                sourceSquare = sourceCard2 >> squareOffset;
                pieceIndex = newGame.board[sourceSquare];
                if (destCard2 == 0)
                {
                    newGame.pieceCount++;
                    newGame.board[sourceSquare] = newGame.pieceCount;
                    newGame.pieces[newGame.pieceCount] = sourceCard2;
                }
                else
                {
                    destSquare = destCard2 >> squareOffset;
                    newGame.pieces[pieceIndex] = sourceCard2;
                    newGame.board[destSquare] = 0;
                    newGame.board[sourceSquare] = pieceIndex;
                }
            }
            
            newGame.board[128] = (int)(move >> epOffset);
            
            newGame.playerOnTurn = 24 ^ newGame.playerOnTurn;
            newGame.ply--;
            return newGame;
        }

        public long IterativeDeepening(Game game, float depth, out float value)
        {
            int maxDepth = Convert.ToInt32(Math.Floor(depth));
            int minDepth = maxDepth; // maxDepth; //1;
            int nodeCount = 0;
            DateTime startTime = DateTime.Now;
            for (int i = minDepth; i <= maxDepth; i++)
            {
                float curDepth = i + depth - Convert.ToInt32(Math.Floor(depth));
                Value(curGame, curDepth, ref nodeCount, float.NegativeInfinity, float.PositiveInfinity);
            }
            DateTime endTime = DateTime.Now;
            double elapsedSec = endTime.Subtract(startTime).TotalSeconds;
            NodeData nodeData = transpositionTable[curGame.Serialize()];
            int candidateMovesCount = 1;
            float bestValue = nodeData.moveValues[0];
            while (candidateMovesCount < nodeData.sortedMoves.Length && nodeData.moveValues[candidateMovesCount] > bestValue - 0.05)
            {
                candidateMovesCount++;
            }
            value = nodeData.value;
            AddDebug(value.ToString("0.00"), depth.ToString(), nodeCount.ToString(), elapsedSec.ToString());
            return nodeData.sortedMoves[r.Next(candidateMovesCount)];
        }

        private void ComputerPlay(bool silent = false)
        {
            if (silent || curGame.playerOnTurn == white && chkWhite.Checked || curGame.playerOnTurn == black && chkBlack.Checked)
            {
                GenerateMoves(curGame, ref moves, ref names, false);
                if (moves.Length > 0)
                {
                    int depth = (curGame.playerOnTurn == white) ? tbrDepthWhite.Value : tbrDepthBlack.Value;
                    long computerMove = -1;
                    if (depth == 0)
                    {
                        //computerMove = moves[r.Next(moves.Length)];
                        computerMove = moves[globalIndexer % moves.Length];
                        globalIndexer += 13;
                    }
                    else
                    {
                        float value;
                        computerMove = IterativeDeepening(curGame, (float)depth + (float)0.01, out value);
                        lblValue.Text = value.ToString();
                    }
                    SubmitMove(computerMove, silent: silent);
                }
                else
                {
                    //RefreshBoard();
                    //this.Refresh();
                }
            }
        }

        private void ActiveComputerPlay()
        {
            ComputerPlay();
            RefreshBoard();
            this.Refresh();
        }

        private void SubmitMove(long move, bool active = false, bool silent = false)
        {
            pendingMove = "";
            curGame = ExecuteMove(curGame, move);
            AddDebug(move.ToString());
            GameState state = GenerateMoves(curGame, ref moves, ref names, false);
            if (state == GameState.Ongoing)
            {
                if ((active || (curGame.ply & 0) == 0) && (!silent))
                {
                    RefreshBoard();
                    this.Refresh();
                }
                ComputerPlay(silent);
            }
            else
            {
                if (!silent)
                {
                    RefreshBoard();
                    this.Refresh();
                    MessageBox.Show(state.ToString());
                }
            }

        }

        /*
        public float Value(Game game, float depth, long nextMove=-1, bool straight=true, float alpha = float.NegativeInfinity, float beta = float.PositiveInfinity)
        {
            float value = Value(game, depth, alpha, beta,nextMove);
            return (straight && game.playerOnTurn==black)?-value:value;
        }
         */

        public long Perft(Game game, int depth, bool useTable = false)
        {
            if (depth == 0)
            {
                return 1;
            }
            int[] serializedGame = null;
            if (useTable)
            {
                serializedGame = game.Serialize();
                serializedGame[0] = serializedGame[0] | ((game.ply) << 5);
                long data;
                bool inTable = perftTable.TryGetValue(serializedGame, out data);
                if (inTable)
                {
                    return data;
                }
            }
            long result;
            long[] moves = null;
            string[] names = null;
            GenerateMoves(game, ref moves, ref names, false);
            if (depth == 1)
            {
                result = moves.Length;
            }
            else
            {
                long totalNodes = 0;
                foreach (long move in moves)
                {
                    
                    //Game newGame = ExecuteMove(game, move);
                    //totalNodes += Perft(newGame, depth - 1);
                    
                    ExecuteMove(game, move, true);
                    totalNodes += Perft(game, depth - 1);
                    ReverseMove(game, move, true);
                }
                result = totalNodes;
            }
            if (useTable)
            {
                perftTable.Add(serializedGame, result);
            }
            return result;
        }

        public void CheckThreats(Game game)
        {
            int turn = 24 ^ game.playerOnTurn;
            int square = FindKingSquare(game, 24 ^ turn);
            checkDirectionCount = 0;

            int curSquare;
            /*
            for (int i = 0; i < directions.Length; i++)
            {
                int[] curDirections = directions[i];

                foreach (int direction in curDirections)
             * */

            for (int j = 0; j < directions.Length; j++)
            {

                int direction = directions[j];
                int i = directionStyles[j];
                curSquare = square;
                bool breaking = false;
                bool allowShortRange = true;
                int block = 0;
                while (!breaking)
                {
                    curSquare += direction;
                    if ((curSquare & 136) != 0)
                    {
                        breaking = true;
                    }
                    else
                    {
                        int pieceIndex = game.board[curSquare];
                        if (pieceIndex > 0)
                        {
                            int pieceCard = game.pieces[pieceIndex];
                            if ((pieceCard & 24) == turn)
                            {
                                breaking = true;
                                int pieceType = pieceCard & 7;
                                if (pieceType == longRange[i, 0] || pieceType == longRange[i, 1])
                                {
                                    checkDirectionIndices[checkDirectionCount] = j;
                                    checkDirectionCount++;
                                }
                                else if (allowShortRange && (pieceType == shortRange[i, 0] || pieceType == shortRange[i, 1]))
                                {
                                    checkDirectionIndices[checkDirectionCount] = j;
                                    checkDirectionCount++;
                                }
                            }
                            else
                            {
                                block++;
                                if (block == 2)
                                {
                                    breaking = true;
                                }

                            }
                        }
                    }
                    allowShortRange = false;
                    // }
                }
            }
        }

        public int FindKingSquare(Game game, int side)
        {
            int king = 4 | side;
            int pieceCard = 0;
            for (int i = 1; i <= game.pieceCount; i++)
            {
                pieceCard = game.pieces[i];
                if ((pieceCard & 31) == king)
                {
                    break;
                }
            }
            return (pieceCard >> squareOffset);
        }

        public bool CheckCheck(Game game, bool useMap = true)
        {
            int turn;
            turn = game.playerOnTurn;
            int square = FindKingSquare(game, 24 ^ turn);

            int curSquare;
            /*
            for (int i = 0; i < directions.Length; i++)
            {
                int[] curDirections = directions[i];

                foreach (int direction in curDirections)
             * */

            int maxIndex;
            if (useMap)
            {
                maxIndex = checkDirectionCount;
            }
            else
            {
                maxIndex = directions.Length;
            }

            for (int j = 0; j < maxIndex; j++)
            {
                int mainDirectionIndex;
                if (useMap)
                {
                    mainDirectionIndex = checkDirectionIndices[j];
                }
                else
                {
                    mainDirectionIndex = j;
                }
                int direction = directions[mainDirectionIndex];
                int i = directionStyles[mainDirectionIndex];
                curSquare = square;
                bool breaking = false;
                bool allowShortRange = true;
                while (!breaking)
                {
                    curSquare += direction;
                    if ((curSquare & 136) != 0)
                    {
                        breaking = true;
                    }
                    else
                    {
                        int pieceIndex = game.board[curSquare];
                        if (pieceIndex > 0)
                        {
                            breaking = true;
                            int pieceCard = game.pieces[pieceIndex];
                            if ((pieceCard & 24) == turn)
                            {
                                int pieceType = pieceCard & 7;
                                if (pieceType == longRange[i, 0] || pieceType == longRange[i, 1])
                                {
                                    return true;
                                }
                                else if (allowShortRange && (pieceType == shortRange[i, 0] || pieceType == shortRange[i, 1]))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                    allowShortRange = false;
                    // }
                }
            }
            return false;
        }

        public void GenerateThreats(Game game, int square, out float[] attackValues, out float[] defendValues)
        {
            float[] tempAttack = new float[16];
            float[] tempDefend = new float[16];
            int attackCount = 0;
            int defendCount = 0;
            //directions = new int[] { 1, -1, 16, -16 };
            int curSquare;
            /*
            for (int i = 0; i < directions.Length; i++)
            {
                int[] curDirections = directions;//[i];
                foreach (int direction in curDirections)
             * */
            for (int j = 0; j < directions.Length; j++)
            {
                int direction = directions[j];
                int i = directionStyles[j];

                curSquare = square;
                bool breaking = false;
                bool allowShortRange = true;
                while (!breaking)
                {
                    curSquare += direction;
                    if ((curSquare & 136) != 0)
                    {
                        breaking = true;
                    }
                    else
                    {
                        int pieceIndex = game.board[curSquare];
                        if (pieceIndex > 0)
                        {
                            int pieceCard = game.pieces[pieceIndex];
                            int pieceType = pieceCard & 7;
                            bool goodPiece = false;
                            if (pieceType == longRange[i, 0] || pieceType == longRange[i, 1])
                            {
                                goodPiece = true;
                            }
                            else
                            {
                                goodPiece = (allowShortRange && (pieceType == shortRange[i, 0] || pieceType == shortRange[i, 1]));
                            }
                            if (goodPiece)
                            {
                                if (game.playerOnTurn == (pieceCard & 24))
                                {
                                    tempAttack[attackCount] = pieceValues[8 | pieceType];
                                    attackCount++;
                                }
                                else
                                {
                                    tempDefend[defendCount] = pieceValues[8 | pieceType];
                                    defendCount++;
                                }
                            }
                            breaking = true;
                        }
                        //int pieceType = 
                        //if (game.pieces[game.board[curSquare]]&7<6)
                    }
                    allowShortRange = false;
                    //}
                }
            }

            attackValues = new float[attackCount];
            defendValues = new float[defendCount];
            for (int i = 0; i < attackCount; i++)
            {
                attackValues[i] = tempAttack[i];
            }
            for (int i = 0; i < defendCount; i++)
            {
                defendValues[i] = tempDefend[i];
            }
        }

        public float TerminalValue(Game game, long nextMove = -1)
        {
            bool switchValue = (game.playerOnTurn == black);
            float bestValue = 0;


            for (int i = 1; i <= game.pieceCount; i++)
            {
                bestValue += pieceValues[(game.pieces[i] & 15)];
            }

            if (nextMove != -1)
            {

                int minusPiece1 = (int)(nextMove & 15);
                int minusPiece2 = (int)((nextMove>>move2Offset)  & 15);
                int plusPiece1 = (int)((nextMove >> move1Offset) & 15);
                int plusPiece2 = (int)((nextMove >> move3Offset) & 15);


                bestValue += pieceValues[plusPiece1] + pieceValues[plusPiece2] - (pieceValues[minusPiece1] + pieceValues[minusPiece2]);
                //bestValue += (-pieceValues[(game.pieces[capturedPiece] & 15)] + pieceValues[promotion] - pieceValues[(game.pieces[movedPiece] & 15)]);
                switchValue = !switchValue;

                if ((plusPiece2 == 0) && (minusPiece2!=0))
                {
                    float[] attackValues;
                    float[] defendValues;
                    int qsSquare = (int)((nextMove >> move1SquareOffset) & 127);
                    //int qsSquare = (int)((nextMove >> move1SquareOffset) & 127);
                    Game tempGame = ExecuteMove(game, nextMove);
                    GenerateThreats(tempGame, qsSquare, out attackValues, out defendValues);

                    Array.Sort(attackValues);
                    Array.Sort(defendValues);

                    bool attacker = true;
                    int choiceArraySize;
                    if (attackValues.Length > 0)
                    {
                        if (attackValues.Length > defendValues.Length)
                        {
                            choiceArraySize = (defendValues.Length << 1) + 2;
                        }
                        else
                        {
                            choiceArraySize = (attackValues.Length << 1) + 1;
                        }
                        float[] choiceArray = new float[choiceArraySize];
                        choiceArray[1] = pieceValues[(8 | (plusPiece1 & 7))];
                        int index = 2;
                        for (int i = 0; i < (choiceArraySize - 1) >> 1; i++)
                        {
                            choiceArray[index] = choiceArray[index - 1] - attackValues[i];
                            index++;
                            if ((i + 1) < attackValues.Length)
                            {
                                choiceArray[index] = choiceArray[index - 1] + defendValues[i];
                                index++;
                            }
                        }
                        float value = choiceArray[choiceArraySize - 1];
                        for (int i = choiceArraySize - 2; i >= 0; i--)
                        {
                            if ((i & 1) == 0)
                            {
                                if (value < choiceArray[i])
                                {
                                    value = choiceArray[i];
                                }
                            }
                            else
                            {
                                if (value > choiceArray[i])
                                {
                                    value = choiceArray[i];
                                }
                            }
                        }
                        bestValue += (switchValue) ? -value : value;

                    }



                }


            }

            return (switchValue) ? -bestValue : bestValue;

        }

        public float Value(Game game, float depth, ref int nodeCount, float alpha = float.NegativeInfinity, float beta = float.PositiveInfinity, long nextMove = -1)
        {
            nodeCount++;
            NodeData pastData = null;
            if (depth < 1)
            {
                return TerminalValue(game, nextMove);
            }
            float value = 0;

            Game newGame;

            if (nextMove != -1)
            {
                newGame = ExecuteMove(game, nextMove);
                //                if (ArrayToString(game.Serialize())=="{8,0,1292,1812,5394,5906,8971,9494,9741,22286}")
                //{
                //value = 0;
                //}
            }
            else
            {
                newGame = game;
            }

            int[] serializedGame = newGame.Serialize();
            bool inTable = transpositionTable.TryGetValue(serializedGame, out pastData);
            //int[] serializedGame = null;
            //bool inTable = false;

            if (inTable)
            {
                if (false)
                {
                    if (pastData.depth >= depth)
                    {
                        if (pastData.alpha < pastData.value && pastData.value < pastData.beta)
                        {
                            return pastData.value;
                        }
                        else if (pastData.alpha == pastData.value && pastData.alpha < alpha)
                        {
                            return alpha;
                        }
                        else if (pastData.beta == pastData.value && pastData.beta > beta)
                        {
                            return beta;
                        }
                    }
                }
            }
            else
            {
                pastData = new NodeData();
                transpositionTable.Add(serializedGame, pastData);
            }
            pastData.depth = depth;



            long[] moves = pastData.sortedMoves;
            //string[] names = pastData.names;
            float[] moveValues = pastData.moveValues;
            string[] names = null;
            GameState state = pastData.state;

            if (moves == null)
            {
                state = GenerateMoves(newGame, ref moves, ref names, false);
                moveValues = new float[moves.Length];
            }
            if (state == GameState.Ongoing)
            {
                float bestValue = float.NegativeInfinity;
                if (depth == 4)
                {
                    txtLog.Text = transpositionTable.Count.ToString() + "\r\n";
                }
                for (int i = 0; i < moves.Length; i++)
                {
                    long move = moves[i];
                    //string name = names[i];
                    //Game tempGame = ExecuteMove(game, move);
                    float curValue = -Value(newGame, depth - 1, ref nodeCount, -beta, -alpha, move);
                    if (curValue <= alpha)
                    {
                        moveValues[i] = curValue - 0.2f;
                    }
                    else
                    {
                        moveValues[i] = curValue;
                    }
                    if (depth == 4)
                    {
                        //txtLog.Text += name + " " + curValue.ToString() + "\r\n";
                    }
                    if (curValue >= beta)
                    {
                        bestValue = beta;// +0.1f;
                        break;
                    }
                    if (curValue > bestValue)
                    {
                        bestValue = curValue;
                    }
                    if (curValue > alpha)
                    {
                        alpha = curValue;
                    }
                }
                if (bestValue < alpha)
                {
                    bestValue = alpha;
                }
                if (bestValue > 100)
                {
                    bestValue--;
                }
                else if (bestValue < -100)
                {
                    bestValue++;
                }
                value = bestValue;
            }
            else
            {
                pastData.depth = float.PositiveInfinity;
                pastData.alpha = alpha;
                pastData.beta = beta;
                float winValue = (float)(999);
                switch (state)
                {
                    case GameState.Ongoing:
                        break;
                    case GameState.Illegal:
                        break;
                    case GameState.WhiteWin:
                        value = (newGame.playerOnTurn == white) ? winValue : -winValue;
                        break;
                    case GameState.Draw:
                        value = 0;
                        break;
                    case GameState.BlackWin:
                        value = (newGame.playerOnTurn == white) ? -winValue : winValue;
                        break;
                    case GameState.Uninitialized:
                        break;
                    default:
                        break;
                }
            }
            pastData.state = state;
            int[] indices = Enumerable.Range(0, moves.Length).ToArray();
            var sortedIndices = indices.OrderBy(j => moveValues[j]);
            int k = moves.Length;
            pastData.moveValues = new float[moves.Length];
            //pastData.names = new string[moves.Length];
            pastData.sortedMoves = new long[moves.Length];
            foreach (int j in sortedIndices)
            {
                k--;
                pastData.moveValues[k] = moveValues[j];
                //pastData.names[k] = names[j];
                pastData.sortedMoves[k] = moves[j];

            }

            pastData.value = value;
            return value;
        }

        public string Lines(Game game, float depth)
        {
            if (depth < 1)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            int nodeCount = 0;
            DateTime startTime = DateTime.Now;
            Value(game, depth, ref nodeCount);
            DateTime endTime = DateTime.Now;
            double elapsedSec = endTime.Subtract(startTime).TotalSeconds;

            NodeData nodeData = transpositionTable[game.Serialize()];
            for (int i = 0; i < nodeData.sortedMoves.Length; i++)
            {
                if (i != 0)
                {
                    sb.Append("\r\n");
                }
                long move = nodeData.sortedMoves[i];
                sb.Append(nodeData.moveValues[i].ToString("0.00") + " " + SeekMove(game, move) + " ");
                sb.Append(Line(ExecuteMove(game, move), depth - 1));

            }
            sb.Append("\r\n" + depth.ToString() + ", " + nodeCount.ToString() + ", " + elapsedSec.ToString());

            return sb.ToString();
        }

        public string Line(Game game, float depth)
        {
            if (depth < 1)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            int nodeCount = 0;
            Value(game, depth, ref nodeCount);
            NodeData nodeData = transpositionTable[game.Serialize()];
            if (nodeData.sortedMoves.Length == 0)
            {
                return "";
            }
            long move = nodeData.sortedMoves[0];
            sb.Append(SeekMove(game, move) + " ");
            sb.Append(Line(ExecuteMove(game, move), depth - 1));
            return sb.ToString();
        }

        public long SeekMove(Game game, string name)
        {
            long[] moves = null;
            string[] names = null;
            GenerateMoves(game, ref moves, ref names, false);
            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == name)
                {
                    return moves[i];
                }
            }
            return -1;
        }

        public string SeekMove(Game game, long move)
        {
            long[] moves = null;
            string[] names = null;
            GenerateMoves(game, ref moves, ref names, false);
            for (int i = 0; i < names.Length; i++)
            {
                if (moves[i] == move)
                {
                    return names[i];
                }
            }
            return "";
        }

        private void visBoard_Click(object sender, EventArgs e)
        {
            Label labelSender = (Label)sender;
            int square = Convert.ToInt32(labelSender.Name);
            pendingMove += SquareName(square);
            if (pendingMove.Length == 4)
            {
                int moveIndex = -1;
                for (int i = 0; i < legalMoves; i++)
                {
                    if (names[i] == pendingMove)
                    {
                        moveIndex = i;
                        break;
                    }
                }
                if (moveIndex != -1)
                {
                    SubmitMove(moves[moveIndex], true);

                }
                else
                {
                    pendingMove = "";
                }
            }
            lblPending.Text = pendingMove;
            RefreshBoard();

            //MessageBox.Show(((Label)sender).Name);
            //labelSender.DoDragDrop(labelSender.Text,0);
        }

        private void visBoard_MouseDown(object sender, EventArgs e)
        {
            Label labelSender = (Label)sender;
            //MessageBox.Show(((Label)sender).Name);
            labelSender.DoDragDrop(labelSender.Text, DragDropEffects.Copy | DragDropEffects.Move);
        }

        private void lblMoves_Click(object sender, EventArgs e)
        {

        }

        private void lblMoves_DragDrop(object sender, DragEventArgs e)
        {
            MessageBox.Show("Dragged!");
        }

        private void chkWhite_CheckedChanged(object sender, EventArgs e)
        {
            ActiveComputerPlay();
        }

        private void chkBlack_CheckedChanged(object sender, EventArgs e)
        {
            ActiveComputerPlay();
        }

        private void btnMoves_Click(object sender, EventArgs e)
        {
            MessageBox.Show(GenerateMoves(curGame, ref moves, ref names, false).ToString());
        }

        private void chkRotate_CheckedChanged(object sender, EventArgs e)
        {
            PositionLabels();
        }

        private void tbrDepthWhite_Scroll(object sender, EventArgs e)
        {
            tbrDepthBlack.Value = tbrDepthWhite.Value;
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            DateTime then = DateTime.Now;
            int gameNum = 20;
            for (int i = 0; i < gameNum; i++)
            {
                StartGame();
                ComputerPlay(true);
            }
            DateTime now = DateTime.Now;
            MessageBox.Show((gameNum / now.Subtract(then).TotalSeconds).ToString());
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int a = 1;
        }

        private void btnLines_Click(object sender, EventArgs e)
        {
            txtLog.Text = Lines(curGame, tbrDepthWhite.Value);
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            transpositionTable = new Dictionary<int[], NodeData>(new MyEqualityComparer());
            perftTable = new Dictionary<int[], long>(new MyEqualityComparer());
        }

        private void txtLog_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnPerft_Click(object sender, EventArgs e)
        {

            int depth = 0;
            // depth = tbrDepthWhite.Value

            while (true)
            {
                perftTable = new Dictionary<int[], long>(new MyEqualityComparer());
                DateTime startTime = DateTime.Now;
                long perft = Perft(curGame, depth);
                DateTime endTime = DateTime.Now;
                double interval = endTime.Subtract(startTime).TotalSeconds;
                txtLog.Text += depth.ToString() + ", " + perft.ToString() + ", " + interval.ToString("0.000") + "\r\n";
                depth++;
                this.Refresh();
            }


        }

        private void AddDebug(params string[] fields)
        {
            String appendText = "";
            for (int i = 0; i < fields.Length; i++)
            {
                if (i > 0)
                {
                    appendText += ", ";
                }
                appendText += fields[i];
            }
            txtLog.Text += (appendText + "\r\n");
        }

        private void PerftEnumerate(int depth)
        {
            long[] moves = null;
            string[] names = null;
            GenerateMoves(curGame, ref moves, ref names, false);
            for (int i = 0; i < moves.Length; i++)
            {
                Game newGame = ExecuteMove(curGame, moves[i]);
                AddDebug(names[i], Perft(newGame, depth - 1).ToString());

            }


        }

        private void btnPerftLines_Click(object sender, EventArgs e)
        {
            PerftEnumerate(tbrDepthWhite.Value);
        }

        private void btnClearLog_Click(object sender, EventArgs e)
        {
            txtLog.Text = "";
        }

        private void btnImportFEN_Click(object sender, EventArgs e)
        {
            curGame = new Game(txtFen.Text);
            RefreshBoard();
            this.Refresh();
            ActiveComputerPlay();
        }

        private void btnExportFEN_Click(object sender, EventArgs e)
        {
            txtFen.Text = curGame.FEN();
        }

        private void btnReverse_Click(object sender, EventArgs e)
        {
            pendingMove = "";
            curGame = ReverseMove(curGame, Convert.ToInt64(txtMove.Text));
            RefreshBoard();
            this.Refresh();
        }
    }
}
