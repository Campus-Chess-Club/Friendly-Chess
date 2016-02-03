#define useTable
#define useTableValues

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

namespace Friendly_Chess
{
    public partial class frmChess : Form
    {

        public struct TestPoint
        {
            public string FEN;
            public int depth;
            public long perft;
        }

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

        public struct NodeData
        {
            public int lowerBound;
            public int upperBound;
            public float lowerDepth;
            public float upperDepth;
            public long[] moves;
            public int bestMoveIndex;
            public int owner;
            //public string[] names;
            public GameState state;
        }

        public struct ShortSerial
        {
            public int hash1;
            public long hash2;

            /*
            public static bool operator ==(ShortSerial)
            {

            }
             */
        }

        public class Game
        {
            public int[] board;
            public int[] pieces;
            public int pieceCount;
            public int playerOnTurn;
            public int ply;
            public int moves50;
            public int zobristHash;
            public long zobristHash2;

            private void CalculateZobrist()
            {
                zobristHash = 0;
                zobristHash2 = 0;
                for (int i = 1; i <= pieceCount; i++)
                {
                    zobristHash = zobristHash ^ zobristKeys[pieces[i]];
                    zobristHash2 = zobristHash2 ^ zobristKeys2[pieces[i]];
                }
            }

            public Game Clone()
            {
                Game newGame = new Game();
                newGame.pieceCount = pieceCount;
                newGame.playerOnTurn = playerOnTurn;
                newGame.ply = ply;
                newGame.moves50 = moves50;
                newGame.pieces = new int[pieceCount + 2];
                newGame.board = new int[129];
                newGame.zobristHash = zobristHash;
                newGame.zobristHash2 = zobristHash2;
                for (int i = 1; i <= pieceCount; i++)
                {
                    newGame.pieces[i] = pieces[i];
                    newGame.board[pieces[i] >> squareOffset] = i;
                }
                newGame.board[128] = board[128];
                return newGame;
            }

            public bool Compare(Game game2)
            {
                bool equal = true;
                equal = equal && (game2.pieceCount == pieceCount);
                equal = equal && (game2.playerOnTurn == playerOnTurn);
                equal = equal && (game2.ply == ply);
                for (int i = 0; i < 129; i++)
                {
                    equal = equal && (game2.pieces[game2.board[i]] == pieces[board[i]]);
                }
                return equal;
            }

            public ShortSerial ShortSerialize(int depth = 0)
            {
                ShortSerial result = new ShortSerial();
                result.hash2 = zobristHash2;
                result.hash1 = (zobristHash ^ playerOnTurn ^ (board[128] << 5) ^ (ply << 12) ^ (depth << 22));
                return result;
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
                CalculateZobrist();
            }

            public Game(string FEN)
            {
                string[] fenParts = FEN.Split(' ');

                string boardPart = fenParts[0];
                string turnPart = (fenParts.Length > 1) ? fenParts[1] : "w";
                string castlePart = (fenParts.Length > 2) ? fenParts[2] : "-";
                string epPart = (fenParts.Length > 3) ? fenParts[3] : "-";
                string moves50Part = (fenParts.Length > 4) ? fenParts[4] : "0";
                string halfplyPart = (fenParts.Length > 5) ? fenParts[5] : "1";


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
                                if ((pieceType & 8) == 0)
                                {
                                    pieceType = pieceType | 16;
                                }
                                if (!deflowered[square])
                                {
                                    switch (pieceType & 7)
                                    {
                                        case 1:
                                            if (rank == 1)
                                            {
                                                pieceType = pieceType | 32;
                                            }
                                            break;
                                        case 2:
                                            if (rank == 6)
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
                CalculateZobrist();

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
                if (playerOnTurn == 8)
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

                if ((pieces[board[4]] & 63) == 44)
                {
                    if ((pieces[board[7]] & 63) == 46)
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
                    sb.Append("-");
                }

                // EP Part
                square = board[128];
                if (square == 0)
                {
                    sb.Append(" -");
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
        public class ArrayEqualityComparer : IEqualityComparer<int[]>
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

        public class GameEqualityComparer : IEqualityComparer<ShortSerial>
        {
            public bool Equals(ShortSerial x, ShortSerial y)
            {
                //return (x.zobristHash == y.zobristHash && x.zobristHash2 == y.zobristHash2 && x.playerOnTurn == y.playerOnTurn && x.board[128] == y.board[128] && x.ply == y.ply);
                return (x.hash1 == y.hash1 && x.hash2 == y.hash2);
            }

            public int GetHashCode(ShortSerial obj)
            {
                //return (obj.zobristHash ^ obj.playerOnTurn ^ (obj.board[128] << 5) ^ (obj.ply << 12));
                return obj.hash1;
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
        const int cardFlagMap = (1 << move1Offset) - 1;


        //const string symbols = "-?pnkbrq?P?NKBRQ";
        const string symbols = " ?♟♞♚♝♜♛?♙?♘♔♗♖♕012345678";
        const string plainSymbols = " ?pnkbrq?P?NKBRQ012345678";
        //const string shortSymbols = "?  NKBRQ";
        Game curGame = new Game();
        int[] directionVectors = { -16, -15, -17, 0, 1, 16, 0, 1, 16, 15, 17, 0, 14, 18, 31, 33, 0 };
        int[] directory = { 7, -1, 11, 6, 8, 3, 6 };
        int[] basicPieceValues = { 0, -1000, -1000, -3000, -20000, -3000, -5000, -9000, 0, 1000, 1000, 3000, 20000, 3000, 5000, 9000 };
        int[] extendedPieceValues;
        Int64[] moves = null;
        //string[] names = null;
        int legalMoves = 0;
        string pendingMove = "";
        Random r = new Random();
        int globalIndexer = 0;
        //Dictionary<int[], NodeData> transpositionTable = new Dictionary<int[], NodeData>(new ArrayEqualityComparer());
        Dictionary<ShortSerial, NodeData> transpositionTable = new Dictionary<ShortSerial, NodeData>(new GameEqualityComparer());
        Dictionary<ShortSerial, long> perftTable = new Dictionary<ShortSerial, long>(new GameEqualityComparer());
        Dictionary<ShortSerial, long> perftTableCore = new Dictionary<ShortSerial, long>(new GameEqualityComparer());
        Dictionary<int[], long> perftTable2 = new Dictionary<int[], long>(new ArrayEqualityComparer());

        long[] movesTemp = new Int64[400];
        string[] namesTemp = new String[400];
        int[] moveValuesTemp = new int[400];
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

        const int zobristSize = 16384;
        static int[] zobristKeys = new int[zobristSize];
        static long[] zobristKeys2 = new long[zobristSize];

        const int coreHeight = 4;
        const int perftMaxEntries = 9000000;
        const int mainMaxEntries = 2000000;

        const int positiveInfinity = 2000000000;
        const int negativeInfinity = -2000000000;

        int prevValue = 0;

        Stopwatch sw1 = new Stopwatch();
        Stopwatch sw2 = new Stopwatch();
        Stopwatch sw3 = new Stopwatch();

        Stopwatch swComputerTime = new Stopwatch();

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

            string promotion = ((sourceCard1 & 7) == (destCard1 & 7)) ? "" : plainSymbols[8 | (destCard1 & 7)].ToString(); // Add symbol of promoted piece

            return SquareName(sourceSquare) + SquareName(destSquare) + promotion;
        }

        private GameState GenerateMoves(Game game, out long[] moves, bool justCheckingCheck, int qsSquare = -1)
        {
            moves = null;
            //int[] Garbage;
            if (!justCheckingCheck && game.moves50 >= 150)
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

            bool isInCheck = false;
            Game tempGame;
            if (!justCheckingCheck)
            {
                CheckThreats(game);
            }
            /*
            if (!justCheckingCheck)
            {
                game.playerOnTurn = game.playerOnTurn ^ 24;
                isInCheck = (GenerateMoves(game, out dummyMoves, true) == GameState.Illegal);
                game.playerOnTurn = game.playerOnTurn ^ 24;
            }
             */


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
                                        int promotedPieceCard;
                                        bool promotionChoice = isPawn && ((square >> 4) == 0 || (square >> 4) == 7);
                                        promotedPieceCard = (pieceType | playerOnTurn | (square << squareOffset));

                                        //Int64 move = -1;
                                        //string name = "";
                                        if (capture)
                                        {
                                            if (!promotionChoice)
                                            {
                                                long move1 = (long)(piece | (promotedPieceCard << move1Offset));
                                                long move2 = (long)(targetPiece);
                                                moveQueue[0] = ((move2 << move2Offset) | move1);

                                                //moveQueue[0] = ((long)(targetPieceIndex | (i << move1Offset) | (square << move1SquareOffset))) | ((long)promotedPiece << move3Offset);
                                                //name = shortSymbols[pieceType]+ SquareName(initialSquare) + "x" + SquareName(square);
                                                //nameQueue[0] = SquareName(initialSquare) + SquareName(square);
                                                breakInner = true;
                                            }
                                            else
                                            {
                                                //int virtualDirection = (pieceType == 1) ? -16 : 16;
                                                //int virtualSquare = square;
                                                for (int j = 0; j < promotionChoices.Length; j++)
                                                {
                                                    promotedPieceCard = (promotionChoices[j] | playerOnTurn | (square << squareOffset));
                                                    long move1 = (long)(piece | (promotedPieceCard << move1Offset) );
                                                    long move2 = (long)(targetPiece | (square << squareOffset));
                                                    moveQueue[j] = ((move2 << move2Offset) | move1);

                                                    //moveQueue[j] = ((long)(targetPieceIndex | (i << move1Offset) | (square << move1SquareOffset))) | ((long)promotedPiece << move3Offset);
                                                    //name = shortSymbols[pieceType]+ SquareName(initialSquare) + "x" + SquareName(square);
                                                    //nameQueue[j] = SquareName(initialSquare) + SquareName(virtualSquare);
                                                    //virtualSquare += virtualDirection;
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
                                                    game.playerOnTurn = game.playerOnTurn ^ 24;
                                                    isInCheck = (GenerateMoves(game, out dummyMoves, true) == GameState.Illegal);
                                                    game.playerOnTurn = game.playerOnTurn ^ 24;

                                                    if (!isInCheck)
                                                    {
                                                        long move1 = (long)(piece | (promotedPieceCard << move1Offset));
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
                                                    moveQueue[0] = -1;
                                                }
                                            }
                                            else
                                            {
                                                if (!promotionChoice)
                                                {
                                                    long move1 = (long)(piece | (promotedPieceCard << move1Offset));
                                                    moveQueue[0] = move1;

                                                    //moveQueue[0] = (i << move1Offset) | (square << move1SquareOffset) | ((long)promotedPiece << move3Offset);
                                                    //string name = shortSymbols[pieceType]+SquareName(initialSquare) + "-" + SquareName(square);
                                                    //nameQueue[0] = SquareName(initialSquare) + SquareName(square);
                                                }
                                                else
                                                {
                                                    //int virtualDirection = (pieceType == 1) ? -16 : 16;
                                                    //int virtualSquare = square;
                                                    for (int j = 0; j < promotionChoices.Length; j++)
                                                    {
                                                        promotedPieceCard = (promotionChoices[j] | playerOnTurn | (square << squareOffset));
                                                        long move1 = (long)(piece | (promotedPieceCard << move1Offset));
                                                        moveQueue[j] = move1;

                                                        //moveQueue[j] = (i << move1Offset) | (square << move1SquareOffset) | ((long)promotedPiece << move3Offset);
                                                        //name = shortSymbols[pieceType]+ SquareName(initialSquare) + "x" + SquareName(square);
                                                        //nameQueue[j] = SquareName(initialSquare) + SquareName(virtualSquare);
                                                        //virtualSquare += virtualDirection;
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
                                            moveQueue[0] = moveQueue[0] | (((long)epSquare) << epOffset);
                                            //tempGame = ExecuteMove(game, moveQueue[0]);
                                            //int[] pre = game.Serialize();
                                            ExecuteMove(game, moveQueue[0], true);
                                            legalPosition = (!CheckCheck(game, !(pieceType == 4 | (isPawn && epSquare == square))));
                                            ReverseMove(game, moveQueue[0], true, targetPieceIndex);

                                            //int[] post = game.Serialize();
                                            /*
                                            bool legalPosition2 = (GenerateMoves(tempGame, out dummyMoves, true) != GameState.Illegal);
                                            legalPosition = legalPosition2;
                                            for (int l = 0; l < pre.Length; l++)
                                            {
                                                if (pre[l]!=post[l] || pre.Length != post.Length || legalPosition != legalPosition2)
                                                {
                                                    pre = pre;
                                                }
                                            }
                                             */

                                        }
                                        if (legalPosition)
                                        {
                                            if (!promotionChoice)
                                            {
                                                movesTemp[legalMoves] = moveQueue[0];
                                                if (capture)
                                                {
                                                    moveValuesTemp[legalMoves] = extendedPieceValues[targetPiece] + (extendedPieceValues[piece] >> 5);
                                                    if (playerOnTurn == black) moveValuesTemp[legalMoves] = -moveValuesTemp[legalMoves];
                                                }
                                                else
                                                {
                                                    moveValuesTemp[legalMoves] = 0;
                                                }
                                                //namesTemp[legalMoves] = nameQueue[0];
                                                legalMoves++;
                                            }
                                            else
                                            {
                                                for (int j = 0; j < promotionChoices.Length; j++)
                                                {
                                                    movesTemp[legalMoves] = moveQueue[j] | (((long)epSquare) << epOffset);
                                                    moveValuesTemp[legalMoves] = extendedPieceValues[targetPiece] + (extendedPieceValues[piece] >> 5);
                                                    if (playerOnTurn == black) moveValuesTemp[legalMoves] = -moveValuesTemp[legalMoves];
                                                    moveValuesTemp[legalMoves] -= (basicPieceValues[promotionChoices[j]] >> 5);
                                                    if (capture)
                                                    {
                                                        moveValuesTemp[legalMoves] = extendedPieceValues[targetPiece] - (extendedPieceValues[piece] >> 5);
                                                    }
                                                    else
                                                    {
                                                        //moveValuesTemp[legalMoves] = 0;
                                                    }
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
                                                bool continueCastleCheck = (pieceType == 4 && (direction == 1 || direction == -1) && legalPosition);
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
                Array.Sort(moveValuesTemp, moves, 0, legalMoves); // MVV/LVA
                return GameState.Ongoing;
            }
            else
            {
                game.playerOnTurn = game.playerOnTurn ^ 24;
                isInCheck = (GenerateMoves(game, out dummyMoves, true) == GameState.Illegal);
                game.playerOnTurn = game.playerOnTurn ^ 24;
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
            InitializePieceValues();
            Random zr = new Random(455468055);
            for (int i = 1; i < zobristSize; i++)
            {
                zobristKeys[i] = zr.Next(int.MinValue, int.MaxValue);
                int part1 = zr.Next(int.MinValue, int.MaxValue);
                int part2 = zr.Next(int.MinValue, int.MaxValue);
                zobristKeys2[i] = ((((long)part1) << 32) | ((long)part2));
            }

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

            swComputerTime.Reset();
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

            GenerateMoves(curGame.Clone(), out moves, false);
            lblMoves.Text = "";
            foreach (long move in moves)
            {
                lblMoves.Text += MoveName(move) + "\r\n";
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

            lblClock.Text = swComputerTime.Elapsed.Minutes.ToString("00") + ":" + swComputerTime.Elapsed.Seconds.ToString("00");
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

        public Game ExecuteMove(Game game, long move, bool inPlace = false)
        {
            if (game.pieces[0] != 0)
            {
                game = game;
            }
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
                    newGame.moves50 = 0;
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

            newGame.zobristHash = newGame.zobristHash ^ zobristKeys[sourceCard1] ^ zobristKeys[sourceCard2] ^ zobristKeys[destCard1] ^ zobristKeys[destCard2];
            newGame.zobristHash2 = newGame.zobristHash2 ^ zobristKeys2[sourceCard1] ^ zobristKeys2[sourceCard2] ^ zobristKeys2[destCard1] ^ zobristKeys2[destCard2];

            newGame.playerOnTurn = 24 ^ newGame.playerOnTurn;
            newGame.ply++;

            if (newGame.pieces[0] != 0)
            {
                game = game;
            }

            return newGame;
        }

        public Game ReverseMove(Game game, long move, bool inPlace = false, int resurrectIndex = -1)
        {
            Game newGame;

            if (game.pieces[0] != 0)
            {
                game = game;
            }


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
                if (destCard2 == 0)
                {
                    newGame.pieceCount++;
                    if (resurrectIndex == -1)
                    {
                        newGame.board[sourceSquare] = newGame.pieceCount;
                        newGame.pieces[newGame.pieceCount] = sourceCard2;
                    }
                    else
                    {
                        newGame.pieces[newGame.pieceCount] = newGame.pieces[resurrectIndex];
                        newGame.board[newGame.pieces[resurrectIndex] >> squareOffset] = newGame.pieceCount;
                        newGame.board[sourceSquare] = resurrectIndex;
                        newGame.pieces[resurrectIndex] = sourceCard2;
                    }
                }
                else
                {
                    destSquare = destCard2 >> squareOffset;
                    pieceIndex = newGame.board[destSquare];
                    newGame.pieces[pieceIndex] = sourceCard2;
                    newGame.board[destSquare] = 0;
                    newGame.board[sourceSquare] = pieceIndex;
                }
            }

            newGame.board[128] = (int)(move >> epOffset);

            newGame.zobristHash = newGame.zobristHash ^ zobristKeys[sourceCard1] ^ zobristKeys[sourceCard2] ^ zobristKeys[destCard1] ^ zobristKeys[destCard2];
            newGame.zobristHash2 = newGame.zobristHash2 ^ zobristKeys2[sourceCard1] ^ zobristKeys2[sourceCard2] ^ zobristKeys2[destCard1] ^ zobristKeys2[destCard2];

            newGame.playerOnTurn = 24 ^ newGame.playerOnTurn;
            newGame.ply--;

            if (newGame.pieces[0] != 0)
            {
                game = game;
            }

            return newGame;
        }

        public long IterativeDeepening(Game game, float depth, out int value, bool silent = false)
        {
            game = game.Clone();
            swComputerTime.Start();
            int maxDepth = Convert.ToInt32(Math.Floor(depth));
            int minDepth = 1; // maxDepth; //1;
            int nodeCount = 0;
            DateTime startTime = DateTime.Now;
            int threshold = 1;
            transpositionTable.Clear();
            for (int i = minDepth; i <= maxDepth; i++)
            {
                float curDepth = i + depth - Convert.ToInt32(Math.Floor(depth));
                Value(game, curDepth, ref nodeCount, negativeInfinity, positiveInfinity, -1, threshold,(int)depth);
            }
            DateTime endTime = DateTime.Now;
            double elapsedSec = endTime.Subtract(startTime).TotalSeconds;
            NodeData nodeData = transpositionTable[game.ShortSerialize()];
            int candidateMovesCount = 0;
            long[] candidateMoves = new long[nodeData.moves.Length];
            //candidateMoves[0] = nodeData.moves[0];
            value = nodeData.lowerBound;
            Debug.Assert(nodeData.lowerBound == nodeData.upperBound);
            foreach (long move in nodeData.moves)
            {
                float curValue = -Value(game, depth - 1, ref nodeCount, value - threshold-1, value + threshold, move, 0,(int)depth);
                if (curValue >= value - threshold)
                {
                    candidateMoves[candidateMovesCount] = move;
                    candidateMovesCount++;
                }
            }
            Array.Sort(candidateMoves, 0, candidateMovesCount);
            int chosenMoveIndex = r.Next(candidateMovesCount);
            long chosenMove = candidateMoves[chosenMoveIndex];
            Debug.Assert(-Value(game, depth - 1, ref nodeCount, value - threshold - 1, value + threshold, chosenMove, 0, (int)depth) > value - threshold);
            Game newGame = ExecuteMove(game, chosenMove, false);
            bool continuing = transpositionTable.TryGetValue(newGame.ShortSerialize(), out nodeData);
            StringBuilder sb = new StringBuilder(MoveName(chosenMove));
            int j = 0;
            swComputerTime.Stop();
            while (continuing && j < 10 && nodeData.moves.Length > 0)
            {
                sb.Append(" " + MoveName(nodeData.moves[nodeData.bestMoveIndex]));
                ExecuteMove(newGame, nodeData.moves[nodeData.bestMoveIndex], true);
                continuing = transpositionTable.TryGetValue(newGame.ShortSerialize(), out nodeData);
                j++;
            }
            if (!silent)
            {
                AddDebug(value.ToString("0.00"), depth.ToString(), nodeCount.ToString(), elapsedSec.ToString());
                AddDebug(sb.ToString());
            }

            return chosenMove;
        }

        private void ComputerPlay(bool silent = false)
        {
            if (silent || curGame.playerOnTurn == white && chkWhite.Checked || curGame.playerOnTurn == black && chkBlack.Checked)
            {
                GenerateMoves(curGame.Clone(), out moves, false);
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
                        int value;
                        computerMove = IterativeDeepening(curGame, (float)depth + (float)0.01, out value, silent);
                        lblValue.Text = FormatScore(value);
                        if (Math.Abs(Math.Abs(value) - Math.Abs(prevValue)) > 2500)
                        {
                            //chkBlack.Checked = false;
                            //chkWhite.Checked = false;
                        }
                        prevValue = value;
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
            if (!silent)
            {
                AddDebug(move.ToString());
                AddDebug(curGame.FEN());
            }
            GameState state = GenerateMoves(curGame.Clone(), out moves, false);
            if (state == GameState.Ongoing)
            {
                if ((active || (curGame.ply & 0) == 0) && (!silent))
                {
                    RefreshBoard();
                    this.Refresh();
                    Application.DoEvents();
                    //Thread.Sleep(1000);
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

        public long Perft(Game game, int depth, int height, bool useTable = true)
        {
            if (depth == 0)
            {
                return 1;
            }
            ShortSerial serializedGame = new ShortSerial();
            //int[] serializedGame = null;
            if (useTable)
            {
                long data;
                //serializedGame = game.Serialize();
                //serializedGame[0] = serializedGame[0] | ((game.ply) << 5);
                serializedGame = game.ShortSerialize(depth);
                //if (serializedGame.Equals((new Game("8/8/8/8/2k5/8/3P2K1/8 w - - 0 3")).ShortSerialize(2)))
                if (serializedGame.hash1 == -1358628805 && serializedGame.hash2 == -7396787305713997022)
                //if (game.FEN()=="8/8/8/8/2k5/8/3P2K1/8 w - - 0 3")
                {
                    game = game;
                }
                //serializedGame.hash1 = serializedGame.hash1 ^ (depth << 25);
                bool inTable;
                if (height <= coreHeight)
                {
                    inTable = perftTableCore.TryGetValue(serializedGame, out data);
                }
                else
                {
                    inTable = perftTable.TryGetValue(serializedGame, out data);
                }
                if (inTable)
                {
                    return data;
                }
                else
                {
                    //perftTable.Add(serializedGame, 0);
                }
            }
            long result;
            long[] moves;
            sw1.Start();
            GenerateMoves(game, out moves, false);
            sw1.Stop();
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

                    sw2.Start();
                    game = ExecuteMove(game, move, false);
                    sw2.Stop();
                    totalNodes += Perft(game, depth - 1, height + 1);
                    sw3.Start();
                    game = ReverseMove(game, move, false);
                    sw3.Stop();

                    /*
                    for (int i = 0; i < pre.Length; i++)
                    {
                        if (pre[i] != post[i])
                        {
                            pre = pre;
                        }
                    }
                     */

                }
                result = totalNodes;
            }
            if (useTable)
            {
                if (height <= coreHeight)
                {
                    perftTableCore.Add(serializedGame, result);
                }
                else
                {
                    perftTable.Add(serializedGame, result);
                    if (perftTable.Count == perftMaxEntries)
                    {
                        perftTable.Clear();
                        AddDebug("Refresh");
                        this.Refresh();
                    }
                }

                //perftTable.Add(serializedGame, result);


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

        public void GenerateThreats(Game game, int square, out int[] attackValues, out int[] defendValues)
        {
            int[] tempAttack = new int[16];
            int[] tempDefend = new int[16];
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
                                    tempAttack[attackCount] = extendedPieceValues[8 | pieceCard];
                                    attackCount++;
                                }
                                else
                                {
                                    tempDefend[defendCount] = extendedPieceValues[8 | pieceCard];
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

            attackValues = new int[attackCount];
            defendValues = new int[defendCount];
            for (int i = 0; i < attackCount; i++)
            {
                attackValues[i] = tempAttack[i];
            }
            for (int i = 0; i < defendCount; i++)
            {
                defendValues[i] = tempDefend[i];
            }
        }

        public int TerminalValue(Game game, long nextMove = -1)
        {
            bool switchValue = (game.playerOnTurn == black);
            int bestValue = 0;


            for (int i = 1; i <= game.pieceCount; i++)
            {
                bestValue += extendedPieceValues[game.pieces[i]];
                /*
                if ((game.pieces[i] & 7) < 3)
                {
                    bestValue += 10 * ((game.pieces[i] >> (squareOffset + 4)));
                    //bestValue -= 0.001f * ((game.pieces[i] >> (squareOffset + 3)));
                }
                 */
            }

            if (nextMove != -1)
            {

                int minusPiece1 = (int)(nextMove & cardFlagMap);
                int minusPiece2 = (int)((nextMove >> move2Offset) & cardFlagMap);
                int plusPiece1 = (int)((nextMove >> move1Offset) & cardFlagMap);
                int plusPiece2 = (int)((nextMove >> move3Offset) & cardFlagMap);


                bestValue += extendedPieceValues[plusPiece1] + extendedPieceValues[plusPiece2] - (extendedPieceValues[minusPiece1] + extendedPieceValues[minusPiece2]);
                //bestValue += (-pieceValues[(game.pieces[capturedPiece] & 15)] + pieceValues[promotion] - pieceValues[(game.pieces[movedPiece] & 15)]);
                switchValue = !switchValue;

                if ((plusPiece2 == 0) && (minusPiece2 != 0))
                {
                    int[] attackValues;
                    int[] defendValues;
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
                        int[] choiceArray = new int[choiceArraySize];
                        choiceArray[1] = extendedPieceValues[(8 | plusPiece1)];
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
                        int value = choiceArray[choiceArraySize - 1];
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

        public int Value (Game game, float depth, long nextMove=-1, int owner=0)
        {
            int nodeCount = 0;
            return Value(game, depth, ref nodeCount, negativeInfinity, positiveInfinity, nextMove, 0, owner);
        }

        public int Value(Game game, float depth, ref int nodeCount, int alpha = negativeInfinity, int beta = positiveInfinity, long nextMove = -1, int threshold = 0, int owner=0)
        {
            nodeCount++;
            int inputAlpha = alpha;
            int inputBeta = beta;
            if (depth < 1)
            {
                return TerminalValue(game, nextMove);
            }
            int value = 0;

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

            NodeData pastData;
            bool inTable = false;
            //int[] serializedGame;
            ShortSerial serializedGame;
#if (useTable)
            {
                //serializedGame = newGame.Serialize();
                serializedGame = newGame.ShortSerialize();
                //pastData = null;
                inTable = transpositionTable.TryGetValue(serializedGame, out pastData);
            }
#endif

            //int[] serializedGame = null;
            //bool inTable = false;

#if useTable && useTableValues
            {
                if (inTable)
                {
                    if (pastData.owner == owner)
                    {
                        if (pastData.lowerDepth >= depth)
                        {
                            if (pastData.lowerBound >= beta)
                            {
                                return pastData.lowerBound;
                            }
                            if (pastData.lowerBound > alpha)
                            {
                                alpha = pastData.lowerBound;
                            }
                        }

                        if (pastData.upperDepth >= depth)
                        {
                            if (pastData.upperBound <= alpha)
                            {
                                return pastData.upperBound;
                            }
                            if (pastData.upperBound < beta)
                            {
                                beta = pastData.upperBound;
                            }
                        }
                    }
                }
                /*
                    if (pastData.upperDepth >= depth)
                    {
                        if (pastData.upperBound <= alpha)
                        {
                            return pastData.upperBound;
                        }
                    }
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
                 */
            }
#endif

            //pastData.depth = depth;
            if (alpha == beta)
            {
                return alpha;
            }



            long[] moves;
            GameState state;
            int bestMoveIndex;

#if useTable
            if (inTable)
            {
                {
                    bestMoveIndex = pastData.bestMoveIndex;
                    moves = (long[])pastData.moves.Clone();
                    state = pastData.state;
                }
            }
            else
            {
                state = GenerateMoves(newGame, out moves, false);
                bestMoveIndex = 0;
                pastData = new NodeData();
            }
#else
            {
            state = GenerateMoves(newGame, out moves, false);
                bestMoveIndex = 0;
            }

                
#endif

            //float[] moveValues = new float[moves.Length];
            int originalAlpha = alpha;

            if (state == GameState.Ongoing)
            {
                int bestValue = negativeInfinity;
                int originalBestMoveIndex = bestMoveIndex;
                for (int i = 0; i < moves.Length; i++)
                {

                    //Search best move first
                    int moveIndex;
                    if (i == 0)
                    {
                        moveIndex = originalBestMoveIndex;
                    }
                    else if (i == originalBestMoveIndex)
                    {
                        moveIndex = 0;
                    }
                    else
                    {
                        moveIndex = i;
                    }

                    long move = moves[moveIndex];
                    //string name = names[i];
                    //Game tempGame = ExecuteMove(game, move);
                    int curValue;

                    if (i != 0) // Principal variation search
                    {
                        curValue = -Value(newGame, depth - 1, ref nodeCount, -alpha - 1, -alpha + threshold, move, 0, owner);
                        if (curValue >= alpha + 1)
                        {
                            curValue = -Value(newGame, depth - 1, ref nodeCount, -beta, -alpha + threshold, move, 0, owner);
                        }
                    }
                    else
                    {
                        curValue = -Value(newGame, depth - 1, ref nodeCount, -beta, -alpha + threshold, move, 0, owner);
                    }

                    //moveValues[i] = curValue;

                    if (curValue > bestValue)
                    {
                        bestValue = curValue;
                        bestMoveIndex = moveIndex;
                    }
                    if (curValue >= beta) break;
                    if (curValue > alpha) alpha = curValue;
                }
                if (bestValue > 50000)
                {
                    bestValue -= 10;
                }
                else if (bestValue < -50000)
                {
                    bestValue += 10;
                }
                value = bestValue;
                /*
                if (bestValue < originalAlpha && bestValue > inputAlpha)
                {
                    value = inputAlpha;
                }
                else
                {
                    value = bestValue;
                }
                 */

#if useTable
                if (!inTable || pastData.owner!=owner) // If new owner of transposition table, erase data
                {
                    pastData.lowerBound = negativeInfinity;
                    pastData.upperDepth = positiveInfinity;
                    pastData.lowerDepth = 0;
                    pastData.upperDepth = 0;
                    pastData.moves = moves;
                    pastData.state = state;
                    pastData.bestMoveIndex = 0;
                    pastData.owner = owner;
                }

                if ((value > beta && value < inputBeta) || (value < originalAlpha && value > inputAlpha))
                {
                    value = value;
                }

                if (value < beta) //Value is upper bound
                {
                    if (depth > pastData.upperDepth) // Replace immediately with higher-depth bound
                    {
                        pastData.upperDepth = depth;
                        pastData.upperBound = value;
                        pastData.bestMoveIndex = bestMoveIndex;
                    }
                    else if (depth == pastData.upperDepth)
                    {
                        if (value < pastData.upperBound) // Replace with tighter bound of equal depth
                        {
                            pastData.upperBound = value;
                            pastData.bestMoveIndex = bestMoveIndex;
                        }
                    }
                }
                if (value > originalAlpha) // Value is lower bound
                {
                    if (depth > pastData.lowerDepth) // Replace immediately with higher-depth bound
                    {
                        pastData.lowerDepth = depth;
                        pastData.lowerBound = value;
                        pastData.bestMoveIndex = bestMoveIndex;
                    }
                    else if (depth == pastData.lowerDepth)
                    {
                        if (value > pastData.lowerBound) // Replace with tighter bound of equal depth
                        {
                            pastData.lowerBound = value;
                            pastData.bestMoveIndex = bestMoveIndex;
                        }
                    }
                }
#endif
            }
            else
            {
                //pastData.moveValues = moveValues;
                int winValue = 99990;
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
#if useTable
                {

                    pastData.moves = moves;
                    pastData.lowerBound = value;
                    pastData.upperBound = value;
                    pastData.lowerDepth = depth;
                    pastData.upperDepth = depth;
                    pastData.state = state;
                    pastData.owner = owner;
                }
#endif

            }
            //if (best)

            /*
            int[] indices = Enumerable.Range(0, moves.Length).ToArray();
            var sortedIndices = indices.OrderBy(j => moveValues[j]);
            int k = moves.Length;
            pastData.moveValues = new float[moves.Length];
            //pastData.names = new string[moves.Length];
            pastData.moves = new long[moves.Length];
            foreach (int j in sortedIndices)
            {
                k--;
                pastData.moveValues[k] = moveValues[j];
                //pastData.names[k] = names[j];
                pastData.moves[k] = moves[j];

            }

            pastData.value = value;
             */



#if useTable
            if (inTable)
            {
                transpositionTable[serializedGame] = pastData;
            }
            else
            {
                transpositionTable.Add(serializedGame, pastData);
                if (transpositionTable.Count == mainMaxEntries)
                {
                    transpositionTable.Clear();
                }
            }
#endif

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
            Value(game, 1, -1,(int)depth);
            DateTime endTime = DateTime.Now;
            double elapsedSec1 = endTime.Subtract(startTime).TotalSeconds;
            startTime = DateTime.Now;
            NodeData nodeData = transpositionTable[game.ShortSerialize()];
            long[] moves = (long[])nodeData.moves.Clone();
            string[] lines = new string[moves.Length];
            int[] moveValues = new int[moves.Length];
            for (int i = 0; i < moves.Length; i++)
            {
                lines[i] = Line(game, depth, moves[i]);
                moveValues[i] = Value(game, depth-1, moves[i], (int)depth);
            }
            Array.Sort(moveValues, lines);
            foreach (string line in lines)
            {
                sb.Append(line + "\r\n");
            }
            /*
            long bestMove = moves[nodeData.bestMoveIndex];
            sb.Append(Line(game, depth, bestMove));
            for (int i = 0; i < moves.Length; i++)
            {
                //if (i != 0)
                //{
                //sb.Append("\r\n");
                //}
                if (i != nodeData.bestMoveIndex)
                {
                    sb.Append("\r\n");
                    long move = moves[i];
                    //sb.Append(nodeData.moveValues[i].ToString("0.00") + " " + SeekMove(game, move) + " ");
                    sb.Append(Line(game, depth , move));
                }

            }
             */
            endTime = DateTime.Now;
            double elapsedSec2 = endTime.Subtract(startTime).TotalSeconds;
            sb.Append(depth.ToString() + ", " + nodeCount.ToString() + ", " + elapsedSec1.ToString() + ", " + elapsedSec2.ToString()+"\r\n");

            return sb.ToString();
        }

        public string Line(Game game, float depth, long nextMove = -1)
        {
            int owner = (int)depth;
            if (depth < 1)
            {
                return TerminalValue(game).ToString();
            }
            int nodeCount = 0;
            int value = 0;
            if (nextMove == -1)
            {
                for (int i = 1; i <= depth; i++)
                {
                    value = Value(game, i, -1, owner);
                }

                nextMove = transpositionTable[game.ShortSerialize()].moves[transpositionTable[game.ShortSerialize()].bestMoveIndex];
            }
            else
            {
                for (int i = 0; i <= depth - 1; i++)
                {
                    value = -Value(game, i, nextMove, owner);
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(FormatScore(value) + " " + MoveName(nextMove) + " ");
            game = game.Clone();
            ExecuteMove(game, nextMove, true);
            depth--;
            while (depth > 0)
            {
                Value(game, depth, -1, owner);
                NodeData nodeData = transpositionTable[game.ShortSerialize()];
                if (nodeData.moves.Length == 0)
                {
                    return sb.ToString();
                }

                long move = nodeData.moves[nodeData.bestMoveIndex];
                sb.Append(SeekMove(game, move) + " ");
                ExecuteMove(game, move, true);
                depth--;
            }
            return sb.ToString();
        }

        public long SeekMove(Game game, string name)
        {
            long[] moves = null;
            GenerateMoves(game.Clone(), out moves, false);
            for (int i = 0; i < moves.Length; i++)
            {
                if (MoveName(moves[i]) == name)
                {
                    return moves[i];
                }
            }
            return -1;
        }

        public string SeekMove(Game game, long move)
        {
            long[] moves = null;
            GenerateMoves(game.Clone(), out moves, false);
            for (int i = 0; i < moves.Length; i++)
            {
                if (moves[i] == move)
                {
                    return MoveName(move);
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
                long move = SeekMove(curGame, pendingMove);
                if (move == -1)
                {
                    string promotionSymbol = "";
                    if (optQ.Checked==true)
                    {
                        promotionSymbol = "Q";
                    }
                    else if (optR.Checked == true)
                    {
                        promotionSymbol = "R";
                    }
                    else if (optB.Checked == true)
                    {
                        promotionSymbol = "B";
                    }
                    else if (optN.Checked == true)
                    {
                        promotionSymbol = "N";
                    }
                    move = SeekMove(curGame, pendingMove + promotionSymbol);
                }
                if (move != -1)
                {
                    SubmitMove(move, true);

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
            MessageBox.Show(GenerateMoves(curGame.Clone(), out moves, false).ToString());
        }

        private void chkRotate_CheckedChanged(object sender, EventArgs e)
        {
            PositionLabels();
        }

        private void tbrDepthWhite_Scroll(object sender, EventArgs e)
        {
            tbrDepthBlack.Value = tbrDepthWhite.Value;
            lblDepth.Text = tbrDepthWhite.Value.ToString();
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

        public void ClearTables()
        {
            //transpositionTable = new Dictionary<int[], NodeData>(new ArrayEqualityComparer());
            transpositionTable.Clear();
            perftTable.Clear();
            perftTableCore.Clear();
            perftTable2 = new Dictionary<int[], long>(new ArrayEqualityComparer());
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearTables();
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
                perftTable.Clear();
                perftTableCore.Clear();
                perftTable2 = new Dictionary<int[], long>(new ArrayEqualityComparer());
                DateTime startTime = DateTime.Now;
                sw1.Reset();
                sw2.Reset();
                sw3.Reset();
                long perft = Perft(curGame, depth, 0);
                DateTime endTime = DateTime.Now;
                double interval = endTime.Subtract(startTime).TotalSeconds;
                //AddDebug( depth.ToString(), perft.ToString(),interval.ToString("0.000")) ;
                AddDebug(depth.ToString(), perft.ToString(), interval.ToString("0.000"), sw1.ElapsedMilliseconds.ToString("0"), sw2.ElapsedMilliseconds.ToString("0"), sw3.ElapsedMilliseconds.ToString("0"));
                depth++;
                AddDebug(perftTable.Count.ToString());
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
            GenerateMoves(curGame, out moves, false);
            long totalPerft = 0;
            for (int i = 0; i < moves.Length; i++)
            {
                Game newGame = ExecuteMove(curGame, moves[i]);
                long curPerft = Perft(newGame, depth - 1, 0);
                AddDebug(MoveName(moves[i]), curPerft.ToString());
                totalPerft += curPerft;
            }
            long rootPerft = Perft(curGame, depth, 0);
            AddDebug("Total: " + totalPerft.ToString(), "Root: " + rootPerft.ToString());


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

        private void btnStopWatch_Click(object sender, EventArgs e)
        {
            Stopwatch sw = new Stopwatch();
            MessageBox.Show(Stopwatch.IsHighResolution.ToString());
            MessageBox.Show(Stopwatch.Frequency.ToString());
        }

        private void btnZobrist_Click(object sender, EventArgs e)
        {
            AddDebug(curGame.ShortSerialize().hash1.ToString(), curGame.ShortSerialize().hash2.ToString());
        }

        private void btnQPerft_Click(object sender, EventArgs e)
        {
            MessageBox.Show(QPerft(curGame, tbrDepthWhite.Value).ToString());
        }

        public long QPerft(Game game, int depth)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "C:\\Users\\Meni\\Documents\\Visual Studio 2013\\Projects\\Friendly Chess\\Friendly Chess\\perft.exe",
                    Arguments = depth.ToString() + " H25" + " \"" + game.FEN() + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            while (!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                if (line.StartsWith("perft( " + depth.ToString() + ")="))
                {
                    line = line.Substring(12, line.Length - 10 - 14);
                    return (Convert.ToInt64(line));
                }
            }
            return -1;
        }

        private void btnEstimate_Click(object sender, EventArgs e)
        {
            int targetDepth = 14;
            int pathdepth = 8;
            double totalWeight = 0;
            double totalWeight2 = 0;
            double totalPerft = 0;
            double totalPerft2 = 0;
            long weight;
            long[][] moves = new long[pathdepth][];

            double basePerft = Convert.ToDouble(QPerft(curGame, pathdepth));
            double mean;
            double sd;
            double error;


            //long[] chosen
            for (int i = 0; i < pathdepth; i++)
            {
                moves[i] = null;
            }
            int time = 10 * 1000;
            Random r = new Random();
            int swTarget = 1000;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < int.MaxValue; i++)
            {
                weight = 1;
                Game tempGame = curGame.Clone();
                for (int j = 0; j < pathdepth; j++)
                {
                    GenerateMoves(tempGame, out moves[j], false);
                    int possibleMoves = moves[j].Length;
                    weight *= possibleMoves;
                    if (possibleMoves == 0) break;
                    int moveIndex = r.Next(moves[j].Length);
                    tempGame = ExecuteMove(tempGame, moves[j][moveIndex]);
                }
                if (weight != 0)
                {
                    double perft = Convert.ToDouble(QPerft(tempGame, targetDepth - pathdepth));
                    double dweight = Convert.ToDouble(weight);
                    double weightPerft = dweight * perft;
                    totalWeight += dweight;
                    totalWeight2 += dweight * dweight;
                    totalPerft += weightPerft;
                    totalPerft2 += perft * weightPerft;
                }
                if (sw.ElapsedMilliseconds > swTarget)
                {
                    swTarget += 1000;
                    mean = basePerft * totalPerft / totalWeight;
                    sd = Math.Sqrt(basePerft * basePerft * (totalPerft2 / totalWeight) - mean * mean);
                    error = sd * Math.Sqrt(totalWeight2) / (totalWeight);
                    AddDebug(mean.ToString(), sd.ToString(), error.ToString());
                    this.Refresh();
                }
                if (sw.ElapsedMilliseconds > time)
                {
                    break;
                }
            }
            mean = basePerft * totalPerft / totalWeight;
            sd = Math.Sqrt(basePerft * basePerft * (totalPerft2 / totalWeight) - mean * mean);
            error = sd * Math.Sqrt(totalWeight2) / (totalWeight);
            AddDebug(mean.ToString(), sd.ToString(), error.ToString());
        }

        public string FormatScore(int value)
        {
            return (((double)value) / 1000).ToString("0.00");
        }

        private void btnTerminal_Click(object sender, EventArgs e)
        {
            MessageBox.Show(TerminalValue(curGame).ToString());
        }

        private void btnGame_Click(object sender, EventArgs e)
        {
            ClearTables();
            r = new Random(-1170798812);
            int i = 0;
            int iterations = 5;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            while (!chkPause.Checked && i < iterations)
            {
                StartGame();
                ComputerPlay(true);
                RefreshBoard();
                this.Refresh();
                Application.DoEvents();
                i++;
            }
            sw.Stop();
            AddDebug("Finished " + i.ToString() + " games in " + sw.Elapsed.TotalSeconds + " seconds.");
        }

        public TestPoint[] TestPoints()
        {
            String[] strings = 
            {
                "3k4/3p4/8/K1P4r/8/8/8/8 b - - 0 1",
                "8/8/8/8/k1p4R/8/3P4/3K4 w - - 0 1",
                "8/8/4k3/8/2p5/8/B2P2K1/8 w - - 0 1",
                "8/b2p2k1/8/2P5/8/4K3/8/8 b - - 0 1",
                "8/8/1k6/2b5/2pP4/8/5K2/8 b - d3 0 1",
                "8/5k2/8/2Pp4/2B5/1K6/8/8 w - d6 0 1",
                "5k2/8/8/8/8/8/8/4K2R w K - 0 1",
                "4k2r/8/8/8/8/8/8/5K2 b k - 0 1",
                "3k4/8/8/8/8/8/8/R3K3 w Q - 0 1",
                "r3k3/8/8/8/8/8/8/3K4 b q - 0 1",
                "r3k2r/1b4bq/8/8/8/8/7B/R3K2R w KQkq - 0 1",
                "r3k2r/7b/8/8/8/8/1B4BQ/R3K2R b KQkq - 0 1",
                "r3k2r/8/3Q4/8/8/5q2/8/R3K2R b KQkq - 0 1",
                "r3k2r/8/5Q2/8/8/3q4/8/R3K2R w KQkq - 0 1",
                "2K2r2/4P3/8/8/8/8/8/3k4 w - - 0 1",
                "3K4/8/8/8/8/8/4p3/2k2R2 b - - 0 1",
                "8/8/1P2K3/8/2n5/1q6/8/5k2 b - - 0 1",
                "5K2/8/1Q6/2N5/8/1p2k3/8/8 w - - 0 1",
                "4k3/1P6/8/8/8/8/K7/8 w - - 0 1",
                "8/k7/8/8/8/8/1p6/4K3 b - - 0 1",
                "8/P1k5/K7/8/8/8/8/8 w - - 0 1",
                "8/8/8/8/8/k7/p1K5/8 b - - 0 1",
                "K1k5/8/P7/8/8/8/8/8 w - - 0 1",
                "8/8/8/8/8/p7/8/k1K5 b - - 0 1",
                "8/k1P5/8/1K6/8/8/8/8 w - - 0 1",
                "8/8/8/8/1k6/8/K1p5/8 b - - 0 1",
                "8/8/2k5/5q2/5n2/8/5K2/8 b - - 0 1",
                "8/5k2/8/5N2/5Q2/2K5/8/8 w - - 0 1",
                "1k6/1b6/8/8/7R/8/8/4K2R b K - 0 1",
                "4k2r/8/8/7r/8/8/1B6/1K6 w k - 0 1",
                "1k6/8/8/8/R7/1n6/8/R3K3 b Q - 0 1",
                "r3k3/8/1N6/r7/8/8/8/1K6 w q - 0 1",
                "r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -",
                "rnbqkb1r/pp1p1ppp/2p5/4P3/2B5/8/PPP1NnPP/RNBQK2R w KQkq - 0 6",
                "8/7p/p5pb/4k3/P1pPn3/8/P5PP/1rB2RK1 b - d3 0 28",
            };
            int[] depths = { 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 4, 4, 4, 4, 6, 6, 5, 5, 6, 6, 6, 6, 6, 6, 7, 7, 4, 4, 5, 5, 5, 5, 4, 3, 4 };
            long[] perfts = { 1134888, 1134888, 1015133, 1015133, 1440467, 1440467, 661072, 661072, 803711, 803711, 1274206, 1274206, 1720476, 1720476, 3821001, 3821001, 1004658, 1004658, 217342, 217342, 92683, 92683, 2217, 2217, 567584, 567584, 23527, 23527, 1063513, 1063513, 346695, 346695, 4085603, 53392, 67197 };
            TestPoint[] data = new TestPoint[strings.Length];
            for (int i = 0; i < strings.Length; i++)
            {
                data[i] = new TestPoint();
                data[i].FEN = strings[i];
                data[i].depth = depths[i];
                data[i].perft = perfts[i];
            }
            return data;

        }

        private void btnPerftSuite_Click(object sender, EventArgs e)
        {
            TestPoint[] testData = TestPoints();
            bool allClear = true;
            for (int i = 0; i < testData.Length; i++)
            {
                //perftTable.Clear();
                //perftTableCore.Clear();
                TestPoint point = testData[i];
                AddDebug("Testing " + point.FEN,"depth "+point.depth);
                long computed = Perft(new Game(point.FEN), point.depth, 0);
                long expected = point.perft;
                AddDebug((computed == expected) ? "GOOD" : "ERROR", "Computed: " + computed.ToString(), "Expected: " + expected.ToString());
                allClear = (allClear && (computed == expected));
                this.Refresh();
            }
            AddDebug(allClear ? "ALL CLEAR" : "There is an error.");
            //curGame = new Game(TestPoints()[10].FEN);
            //RefreshBoard();
        }

        public void InitializePieceValues()
        {
            int[] basicPieceValues = { 0, 1000, 1000, 3000, 20000, 3000, 5000, 9000 };
            extendedPieceValues = new int[zobristSize];
            for (int card = 0; card < zobristSize; card++)
            {
                int pieceType = (card & 7);
                int pieceColor = (card & 24);
                int square = (card >> squareOffset);
                int rank = (square >> 4);
                int file = (square & 7);
                int value = basicPieceValues[pieceType];
                if (pieceType==1)
                {
                    value += 10 * (rank - 1);
                }
                else if (pieceType==2)
                {
                    value += 10 * (6 - rank);
                }
                if ((pieceColor & white) == 0)
                {
                    value = -value;
                }
                extendedPieceValues[card] = value;
            }
        }

        private void btnLine_Click(object sender, EventArgs e)
        {
            AddDebug(Line(curGame, tbrDepthWhite.Value));
        }
    }
}
