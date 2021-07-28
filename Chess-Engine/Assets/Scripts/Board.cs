public class Board
{
    private int[,] representation;

    private const int FILE_COUNT = 8;
    private const int RANK_COUNT = 8;

    // Bit 0 - current turn, 1 for white's turn, 0 for black
    // Bits 1-4 - castling availability,
    // white can kingside, white can queenside, black can kingside, black can queenside
    // Bits 5-7 - rank of available en passant target square (square behind pawn that just moved 2 spaces)
    // Bits 8-10 - file of available en passant target square (square behind pawn that just moved 2 spaces)
    // Bits 11-16 - half move counter for 50-move rule
    // Remaining bits - move count (starts at 1, increments after black move)
    private int GameState;

    private const int turnMask = 1;
    private const int castlingMask = 30;
    private const int enRankMask = 224;
    private const int enFileMask = 1792;
    private const int fiftyMoveCounterMask = 129024;
    private const int moveCountMask = ~131071;

    public Board() {
        this.GameState = 0b0;
        this.representation = new int[RANK_COUNT, FILE_COUNT];

        for (int file = 0; file < FILE_COUNT; file++)
            for (int rank = 0; rank < RANK_COUNT; rank++)
                this.representation[rank, file] = 0;
    }

    public int GetSquareContents(int rank, int file) {
        return representation[rank, file];
    }

    public void PlacePieceOnSquare(int piece, int rank, int file) {
        this.representation[rank, file] = piece;
    }

    public bool WhiteMovesNext() {
        return (GameState & 0b1) != 0;
    }

    public void SetWhiteMovesNext(bool whitesTurn) {
        this.GameState &= ~turnMask;

        if (whitesTurn)
            GameState |= turnMask;
    }

    public void SetCastlingAvailability(CastlingDirection direction, bool availability) {
        this.GameState &= ~1 << (int) direction;
        this.GameState |= BoolToInt(availability) << (int) direction;
    }

    public void SetAllCastlingAvailability(bool whiteKing, bool whiteQueen, bool blackKing, bool blackQueen) {
        SetCastlingAvailability(CastlingDirection.WhiteKing, whiteKing);
        SetCastlingAvailability(CastlingDirection.WhiteQueen, whiteQueen);
        SetCastlingAvailability(CastlingDirection.BlackKing, blackKing);
        SetCastlingAvailability(CastlingDirection.BlackQueen, blackQueen);
    }

    public void SetEnPassantTarget(int rank, int file) {
        this.GameState &= ~enRankMask;
        this.GameState &= ~enFileMask;

        this.GameState |= rank << 5;
        this.GameState |= file << 8;
    }

    public void SetFiftyMoveRuleCounter(int count) {
        this.GameState &= ~fiftyMoveCounterMask;

        this.GameState |= count << 11;
    }

    public void SetMoveCounter(int count) {
        this.GameState &= ~moveCountMask;

        this.GameState |= count << 17;
    }

    public static string SquarePosToSquareName(int rank, int file) {
        const string letters = "abcdefgh";

        if (file > FILE_COUNT - 1)
            throw new System.ArgumentException("Square's file cannot be greater than the board width");

        return letters[file].ToString() + (rank + 1).ToString();
    }

    public static (int, int) SquareNameToSquarePos(string name) {
        const string letters = "abcdefgh";

        if (name.Length != 2)
            throw new System.ArgumentException($"{name} is not a valid square name");
        if(!letters.Contains(name[0].ToString()))
            throw new System.ArgumentException($"{name[0]} is not a valid file for a square name");
        if(!char.IsDigit(name[1]))
            throw new System.ArgumentException($"{name[1]} is not a valid rank for a square name");

        int rank = int.Parse(name[1].ToString());

        if(rank < 0 || rank > 7)
            throw new System.ArgumentException($"{name[1]} is not a valid rank for a square name");

        int file = letters.IndexOf(name[0]);

        return (rank, file);
    }

    private int BoolToInt(bool boolean) {
        return boolean ? 1 : 0;
    }

    public enum CastlingDirection { 
        WhiteKing = 1,
        WhiteQueen = 2,
        BlackKing = 3,
        BlackQueen = 4
    }
}
