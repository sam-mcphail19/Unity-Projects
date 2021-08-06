using System;
using System.Collections.Generic;
using UnityEngine;

public class Board {
	private int[,] representation;

	private const int FILE_COUNT = 8;
	private const int RANK_COUNT = 8;

	// Bit 0 - current turn, 1 for white's turn, 0 for black
	// Bits 1-4 - castling availability,
	// white can kingside, white can queenside, black can kingside, black can queenside
	// Bits 5-7 - file of available en passant target square (square behind pawn that just moved 2 spaces)
	// (starting at 1, 0 means no en passant)
	// (en passant is always on the 3rd or 6th rank depending on which player just moved
	// Bits 8-10 - what piece type was just taken
	// Bits 11-16 - half move counter for 50-move rule
	// Remaining bits - move count (starts at 1, increments after black move)
	public int GameState;
	Stack<int> gameStateHistory;

	private const int turnMask = 1;
	private const int castlingMask = 30;
	private const int enFileMask = 224;
	private const int pieceTypeMask = 1792;
	private const int fiftyMoveCounterMask = 129024;
	private const int moveCountMask = ~131071;

	public Board() {
		this.GameState = 0;
		this.gameStateHistory = new Stack<int>();
		this.representation = new int[RANK_COUNT, FILE_COUNT];

		for (int file = 0; file < FILE_COUNT; file++)
			for (int rank = 0; rank < RANK_COUNT; rank++)
				PlacePieceOnSquare(0, rank, file);
	}

	public Board Clone() {
		Board board = new Board();
		board.representation = (int[,])this.representation.Clone();
		board.GameState = this.GameState;
		board.gameStateHistory = new Stack<int>(new Stack<int>(this.gameStateHistory));

		return board;
	}

	public void MakeMove(Move move) {
		this.gameStateHistory.Push(GameState);

		(int, int) startSquarePos = IndexToSquarePos(move.GetStartSquare());
		(int, int) targetSquarePos = IndexToSquarePos(move.GetTargetSquare());

		int pieceType = (int)Piece.GetPieceType(GetSquareContents(targetSquarePos));
		SetTakenPieceType(pieceType);

		SetWhiteMovesNext(!WhiteMovesNext());
		SetFiftyMoveRuleCounter(GetFiftyMoveRuleCounter() + 1);
		if (WhiteMovesNext())
			SetMoveCounter(GetMoveCounter() + 1);

		PlacePieceOnSquare(GetSquareContents(startSquarePos), targetSquarePos.Item1, targetSquarePos.Item2);
		PlacePieceOnSquare(0, startSquarePos.Item1, startSquarePos.Item2);
	}

	public void UnmakeMove(Move move) {
		int previousState = gameStateHistory.Pop();

		(int, int) startSquarePos = IndexToSquarePos(move.GetStartSquare());
		(int, int) targetSquarePos = IndexToSquarePos(move.GetTargetSquare());

		this.representation[startSquarePos.Item1, startSquarePos.Item2] = this.representation[targetSquarePos.Item1, targetSquarePos.Item2];
		this.representation[targetSquarePos.Item1, targetSquarePos.Item2] = GetTakenPieceType();

		this.GameState = previousState;
	}

	public int GetSquareContents(int rank, int file) {
		return representation[rank, file];
	}

	public int GetSquareContents((int, int) squarePos) {
		return GetSquareContents(squarePos.Item1, squarePos.Item2);
	}
	public int GetSquareContentsAtIndex(int index) {
		return GetSquareContents(IndexToSquarePos(index));
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
		this.GameState &= ~(1 << (int)direction);
		this.GameState |= BoolToInt(availability) << (int)direction;
	}

	public void SetAllCastlingAvailability(bool whiteKing, bool whiteQueen, bool blackKing, bool blackQueen) {
		SetCastlingAvailability(CastlingDirection.WhiteKing, whiteKing);
		SetCastlingAvailability(CastlingDirection.WhiteQueen, whiteQueen);
		SetCastlingAvailability(CastlingDirection.BlackKing, blackKing);
		SetCastlingAvailability(CastlingDirection.BlackQueen, blackQueen);
	}

	//first bit: mod 2 > 0
	//second bit: mod 4 > 1
	//third bit: mod 8 > 3
	//fourth bit: mod 16 > 7
	public (bool, bool, bool, bool) GetAllCastlingAvailibility() {
		int availibility = (GameState & castlingMask) >> 1;
		return (availibility % 2 > 0, availibility % 4 > 1, availibility % 8 > 3, availibility % 16 > 7);
	}

	public void SetEnPassantTarget(int file) {
		this.GameState &= ~enFileMask;

		this.GameState |= file << 5;
	}

	public int GetEnPassantTarget() {
		return (GameState & enFileMask) >> 5;
	}

	public string GetEnPassantTargetName() {
		int file = GetEnPassantTarget();
		int rank = WhiteMovesNext() ? 6 : 3;
		return SquarePosToIndex(rank, file) == 0 ? "-" : SquarePosToSquareName(rank, file);
	}

	public void SetTakenPieceType(int pieceType) {
		this.GameState &= ~pieceTypeMask;

		this.GameState |= pieceType << 5;
	}

	public int GetTakenPieceType() {
		return (GameState & pieceTypeMask) >> 8;
	}

	public void SetFiftyMoveRuleCounter(int count) {
		this.GameState &= ~fiftyMoveCounterMask;

		this.GameState |= count << 11;
	}

	public int GetFiftyMoveRuleCounter() {
		return (GameState & fiftyMoveCounterMask) >> 11;
	}

	public int GetMoveCounter() {
		return (GameState & moveCountMask) >> 17;
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

	public static string SquarePosToSquareName((int, int) squarePos) {
		return SquarePosToSquareName(squarePos.Item1, squarePos.Item2);
	}

	public static (int, int) SquareNameToSquarePos(string name) {
		const string letters = "abcdefgh";

		if (name.Length != 2)
			throw new System.ArgumentException($"{name} is not a valid square name");
		if (!letters.Contains(name[0].ToString()))
			throw new System.ArgumentException($"{name[0]} is not a valid file for a square name");
		if (!char.IsDigit(name[1]))
			throw new System.ArgumentException($"{name[1]} is not a valid rank for a square name");

		int rank = int.Parse(name[1].ToString());

		if (rank < 0 || rank > 7)
			throw new System.ArgumentException($"{name[1]} is not a valid rank for a square name");

		int file = letters.IndexOf(name[0]);

		return (rank, file);
	}

	// Iterate over board and return all instances of the given PieceType of the given color
	// eg. GetPieceTypes(false, Piece.PieceType.Pawn) will return all black pawns on the board
	public List<int> GetPieceTypes(bool getWhitePieces, Piece.PieceType pieceType) {
		List<int> toReturn = new List<int>();
		for (int rank = 0; rank < RANK_COUNT; rank++)
			for (int file = 0; file < FILE_COUNT; file++) {
				int piece = GetSquareContents(rank, file);
				if (piece != 0 && Piece.IsWhite(piece) == getWhitePieces && Piece.GetPieceType(piece) == pieceType)
					toReturn.Add(SquarePosToIndex(rank, file));
			}
		return toReturn;
	}

	// a1 = 0, h1 = 7 h8 = 63
	public static int SquarePosToIndex(int rank, int file) {
		return rank * 8 + file;
	}

	public static int SquarePosToIndex((int, int) squarePos) {
		return SquarePosToIndex(squarePos.Item1, squarePos.Item2);
	}

	public static (int, int) IndexToSquarePos(int index) {
		return ((int)Math.Floor((float)index / RANK_COUNT), index % FILE_COUNT);
	}

	public static string IndexToSquareName(int index) {
		return SquarePosToSquareName(IndexToSquarePos(index));
	}

	public static bool IsIndexInBounds(int index) {
		return !(index < 0) && !(index > 63);
	}

	private static int BoolToInt(bool boolean) {
		return boolean ? 1 : 0;
	}

	public enum CastlingDirection {
		WhiteKing = 1,
		WhiteQueen = 2,
		BlackKing = 3,
		BlackQueen = 4
	}
}
