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
	// Bits 5-8 - file of available en passant target square
	// (starting at 1, 0 means no en passant)
	// (en passant is always on the 3rd or 6th rank depending on which player just moved
	// Bits 9-11 - what piece type was just taken
	// Bits 12-17 - half move counter for 50-move rule
	// Remaining bits - move count (starts at 1, increments after black move)
	public int GameState;
	Stack<int> gameStateHistory;

	private const int turnMask = 1;
	private const int castlingMask = 30;
	private const int enFileMask = 480;
	private const int pieceTypeMask = 3584;
	private const int fiftyMoveCounterMask = 520192;
	private const int moveCountMask = ~262143;

	public Board() {
		this.GameState = 0;
		this.gameStateHistory = new Stack<int>();
		this.representation = new int[RANK_COUNT, FILE_COUNT];

		for (int file = 0; file < FILE_COUNT; file++)
			for (int rank = 0; rank < RANK_COUNT; rank++)
				PlacePieceOnSquare(0, new Coord(rank, file));
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

		Coord startSquare = new Coord(move.GetStartSquareIndex());
		Coord targetSquare = new Coord(move.GetTargetSquareIndex());

		int pieceType = (int)Piece.GetPieceType(GetSquareContents(targetSquare));
		SetTakenPieceType(pieceType);

		if (Piece.GetPieceType(GetSquareContents(startSquare)) == Piece.PieceType.King) {
			if (WhiteMovesNext()) {
				SetCastlingAvailability(CastlingDirection.WhiteKing, false);
				SetCastlingAvailability(CastlingDirection.WhiteQueen, false);
			} else {
				SetCastlingAvailability(CastlingDirection.BlackKing, false);
				SetCastlingAvailability(CastlingDirection.BlackQueen, false);
			}
		}

		if (Piece.GetPieceType(GetSquareContents(startSquare)) == Piece.PieceType.Rook) {
			if (WhiteMovesNext()) {
				if (startSquare.GetRank() == 0)
					SetCastlingAvailability(CastlingDirection.WhiteQueen, false);
				else if (startSquare.GetRank() == 7)
					SetCastlingAvailability(CastlingDirection.WhiteKing, false);
			} else {
				if (startSquare.GetRank() == 0)
					SetCastlingAvailability(CastlingDirection.BlackQueen, false);
				else if (startSquare.GetRank() == 7)
					SetCastlingAvailability(CastlingDirection.BlackKing, false);
			}
		}

		if (move.GetFlag() == (int)Move.Flag.Castling) {
			PlacePieceOnSquare(GetSquareContents(startSquare), targetSquare);
			Coord rookSquare = null;
			Coord rookTargetSquare = null;

			// Queen side castle
			if (targetSquare.GetFile() == 2) {
				rookSquare = new Coord(move.GetTargetSquareIndex() - 2);
				rookTargetSquare = new Coord(move.GetTargetSquareIndex() + 1);
			}
			// King side castle
			if (targetSquare.GetFile() == 6) {
				rookSquare = new Coord(move.GetTargetSquareIndex() + 1);
				rookTargetSquare = new Coord(move.GetTargetSquareIndex() - 1);
			}

			PlacePieceOnSquare(GetSquareContents(rookSquare), rookTargetSquare);
			PlacePieceOnSquare(0, rookSquare);
		} else if (move.GetFlag() == (int)Move.Flag.PawnTwoForward) {
			SetEnPassantTarget(targetSquare.GetFile() + 1);
			PlacePieceOnSquare(GetSquareContents(startSquare), targetSquare);
		} else if (move.IsPromotion()) {
			Piece.PieceType promotedPieceType = Piece.PieceType.None;
			int flag = move.GetFlag();

			if (flag == (int)Move.Flag.PromoteToQueen)
				promotedPieceType = Piece.PieceType.Queen;
			else if (flag == (int)Move.Flag.PromoteToKnight)
				promotedPieceType = Piece.PieceType.Knight;
			else if (flag == (int)Move.Flag.PromoteToRook)
				promotedPieceType = Piece.PieceType.Rook;
			else if (flag == (int)Move.Flag.PromoteToBishop)
				promotedPieceType = Piece.PieceType.Bishop;

			Piece.Color color = WhiteMovesNext() ? Piece.Color.White : Piece.Color.Black;
			PlacePieceOnSquare(Piece.NewPiece(promotedPieceType, color), targetSquare);
		} else if (move.GetFlag() == (int)Move.Flag.EnPassantCapture) {
			PlacePieceOnSquare(0, GetEnPassantTargetCoord());
			PlacePieceOnSquare(GetSquareContents(startSquare), targetSquare);
		} else {
			PlacePieceOnSquare(GetSquareContents(startSquare), targetSquare);
		}

		PlacePieceOnSquare(0, startSquare);

		if (move.GetFlag() != (int)Move.Flag.PawnTwoForward)
			SetEnPassantTarget(0);

		SetWhiteMovesNext(!WhiteMovesNext());

		SetFiftyMoveRuleCounter(GetFiftyMoveRuleCounter() + 1);
		if (WhiteMovesNext())
			SetMoveCounter(GetMoveCounter() + 1);
	}

	public void UnmakeMove(Move move) {
		int previousState = gameStateHistory.Pop();

		Coord startSquare = new Coord(move.GetStartSquareIndex());
		Coord targetSquare = new Coord(move.GetTargetSquareIndex());

		PlacePieceOnSquare(GetSquareContents(targetSquare), startSquare);
		PlacePieceOnSquare(GetTakenPieceType(), targetSquare);

		this.GameState = previousState;
	}

	public int GetSquareContents(Coord coord) {
		return representation[coord.GetRank(), coord.GetFile()];
	}

	public void PlacePieceOnSquare(int piece, Coord coord) {
		this.representation[coord.GetRank(), coord.GetFile()] = piece;
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
	public bool[] GetAllCastlingAvailibility() {
		int availibility = (GameState & castlingMask) >> 1;
		return new bool[] { availibility % 2 > 0, availibility % 4 > 1, availibility % 8 > 3, availibility % 16 > 7 };
	}

	public void SetEnPassantTarget(int file) {
		this.GameState &= ~enFileMask;

		this.GameState |= file << 5;
	}

	public int GetEnPassantTarget() {
		return (GameState & enFileMask) >> 5;
	}

	public Coord GetEnPassantTargetCoord() {
		// The target piece is on rank 4 or 5, while the target square is on rank 3 or 6
		if (GetEnPassantTarget() == 0)
			return null;
		return new Coord(WhiteMovesNext() ? 4 : 3, GetEnPassantTarget()-1);
	}

	public string GetEnPassantTargetName() {
		return GetEnPassantTarget() == 0 ? "-" : new Coord(WhiteMovesNext() ? 5 : 2, GetEnPassantTarget() + 1).ToString();
	}

	public void SetTakenPieceType(int pieceType) {
		this.GameState &= ~pieceTypeMask;

		this.GameState |= pieceType << 9;
	}

	public int GetTakenPieceType() {
		return (GameState & pieceTypeMask) >> 9;
	}

	public void SetFiftyMoveRuleCounter(int count) {
		this.GameState &= ~fiftyMoveCounterMask;

		this.GameState |= count << 12;
	}

	public int GetFiftyMoveRuleCounter() {
		return (GameState & fiftyMoveCounterMask) >> 12;
	}

	public int GetMoveCounter() {
		return (GameState & moveCountMask) >> 18;
	}

	public void SetMoveCounter(int count) {
		this.GameState &= ~moveCountMask;

		this.GameState |= count << 18;
	}

	// Iterate over board and return all instances of the given PieceType of the given color
	// eg. GetPieceTypes(false, Piece.PieceType.Pawn) will return all black pawns on the board
	public List<int> GetPieceTypes(bool getWhitePieces, Piece.PieceType pieceType) {
		List<int> toReturn = new List<int>();
		for (int rank = 0; rank < RANK_COUNT; rank++)
			for (int file = 0; file < FILE_COUNT; file++) {
				Coord coord = new Coord(rank, file);
				int piece = GetSquareContents(coord);
				if (piece != 0 && Piece.IsWhite(piece) == getWhitePieces && Piece.GetPieceType(piece) == pieceType)
					toReturn.Add(coord.GetIndex());
			}
		return toReturn;
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
