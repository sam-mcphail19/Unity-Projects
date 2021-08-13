using System;
using System.Collections.Generic;
using UnityEngine;

public class MoveGenerator {

	private static readonly int[] KNIGHT_TARGETS = { -17, -15, -10, -6, 6, 10, 15, 17 };
	private static readonly int[] STRAIGHT_MOVES = { 1, -8, -1, 8 };
	private static readonly int[] DIAGONAL_MOVES = { 9, -7, -9, 7 };
	private static readonly int[] KING_MOVES = { 1, -7, -8, -9, -1, 7, 8, 9 };

	List<Move> moves;
	Board board;

	bool whiteMovesNext;
	bool checkNextDepth;

	public List<Move> GenerateMoves(Board board, bool checkNextDepth = true) {
		this.board = board;
		this.checkNextDepth = checkNextDepth;
		Init();

		GenerateKingMoves();
		GenerateKnightMoves();
		GenerateQueenMoves();
		GenerateRookMoves();
		GenerateBishopMoves();
		GeneratePawnMoves();

		return moves;
	}

	private void Init() {
		moves = new List<Move>(64);
		whiteMovesNext = board.WhiteMovesNext();
	}

	private void GeneratePawnMoves() {
		List<int> pawns = board.GetPieceTypes(this.whiteMovesNext, Piece.PieceType.Pawn);

		foreach (int pawnIndex in pawns) {
			int targetIndex = this.whiteMovesNext ? pawnIndex + 8 : pawnIndex - 8;

			Coord target = new Coord(targetIndex);
			Coord pawn = new Coord(pawnIndex);
			int targetPiece = board.GetSquareContents(target);

			if (targetPiece == 0) {
				AddPawnMoveIfLegal(new Move(pawn, target));

				if ((this.whiteMovesNext && pawn.GetRank() == 1) || (!this.whiteMovesNext && pawn.GetRank() == 6)) {
					target = new Coord(this.whiteMovesNext ? pawnIndex + 16 : pawnIndex - 16);
					if (board.GetSquareContents(target) == 0)
						AddPawnMoveIfLegal(new Move(pawn, target, Move.Flag.PawnTwoForward));
				}
			}

			target = new Coord(this.whiteMovesNext ? pawnIndex + 7 : pawnIndex - 7);
			targetPiece = board.GetSquareContents(target);

			if (Math.Abs(pawn.GetFile() - target.GetFile()) == 1 &&
				targetPiece != 0 && Piece.IsWhite(targetPiece) != board.WhiteMovesNext()) {
				AddPawnMoveIfLegal(new Move(pawn, target));
			}

			target = new Coord(this.whiteMovesNext ? pawnIndex + 9 : pawnIndex - 9);
			targetPiece = board.GetSquareContents(target);

			if (Math.Abs(pawn.GetFile() - target.GetFile()) == 1 &&
				targetPiece != 0 && Piece.IsWhite(targetPiece) != board.WhiteMovesNext()) {
				AddPawnMoveIfLegal(new Move(pawn, target));
			}

			if (board.GetEnPassantTarget() != 0 && pawn.GetRank() == (whiteMovesNext ? 4 : 3)) {
				Coord enPassantTarget = new Coord(whiteMovesNext ? 5 : 2, board.GetEnPassantTarget() - 1);

				if (pawn.GetFile() - 1 == enPassantTarget.GetFile() || pawn.GetFile() + 1 == enPassantTarget.GetFile())
					AddPawnMoveIfLegal(new Move(pawn, enPassantTarget, Move.Flag.EnPassantCapture));
			}
		}
	}

	private void GenerateKnightMoves() {
		List<int> knights = board.GetPieceTypes(this.whiteMovesNext, Piece.PieceType.Knight);

		foreach (int knightIndex in knights) {
			foreach (int knightTarget in KNIGHT_TARGETS) {
				int targetIndex = knightIndex + knightTarget;
				if (targetIndex < 0 || targetIndex > 63)
					continue;

				Coord knight = new Coord(knightIndex);
				Coord target = new Coord(targetIndex);
				if (Math.Abs(knight.GetRank() - target.GetRank()) + Math.Abs(knight.GetFile() - target.GetFile()) != 3)
					continue;

				AddMoveIfLegal(knight, target);
			}
		}
	}

	private void GenerateKingMoves() {
		List<int> kings = board.GetPieceTypes(this.whiteMovesNext, Piece.PieceType.King);

		// This is temporary while checkmate is not implemented
		if (kings.Count < 1)
			return;

		int kingIndex = kings[0];

		foreach (int direction in KING_MOVES) {
			int targetIndex = kingIndex + direction;
			if (targetIndex < 0 || targetIndex > 63)
				continue;

			Coord king = new Coord(kingIndex);
			Coord target = new Coord(targetIndex);

			if (Math.Abs(king.GetRank() - target.GetRank()) > 1 || Math.Abs(king.GetFile() - target.GetFile()) > 1)
				continue;

			AddMoveIfLegal(king, target);
		}

		GenerateCastlingMoves(kingIndex);
	}

	private void GenerateQueenMoves() {
		List<int> queens = board.GetPieceTypes(this.whiteMovesNext, Piece.PieceType.Queen);

		foreach (int queen in queens) {
			GenerateStraightMovesForPiece(queen);
			GenerateDiagonalMovesForPiece(queen);
		}
	}

	private void GenerateRookMoves() {
		List<int> rooks = board.GetPieceTypes(this.whiteMovesNext, Piece.PieceType.Rook);

		foreach (int rook in rooks)
			GenerateStraightMovesForPiece(rook);
	}

	private void GenerateBishopMoves() {
		List<int> bishops = board.GetPieceTypes(this.whiteMovesNext, Piece.PieceType.Bishop);

		foreach (int bishop in bishops)
			GenerateDiagonalMovesForPiece(bishop);
	}

	private void GenerateDiagonalMovesForPiece(int pieceIndex) {
		foreach (int direction in DIAGONAL_MOVES) {
			for (int i = pieceIndex + direction; i < 63 && i > 0; i += direction) {
				Coord target = new Coord(i);

				if (BoardUI.IsSquareLight(target) != BoardUI.IsSquareLight(new Coord(pieceIndex)))
					break;

				int targetContents = board.GetSquareContents(target);

				AddMoveIfLegal(new Coord(pieceIndex), target);

				if (targetContents != 0)
					break;
			}
		}
	}

	private void GenerateStraightMovesForPiece(int pieceIndex) {
		foreach (int direction in STRAIGHT_MOVES) {
			for (int i = pieceIndex + direction; i < 63 && i > 0; i += direction) {
				Coord target = new Coord(i);

				if (Math.Abs(direction) == 1 && target.GetRank() != new Coord(pieceIndex).GetRank())
					break;
				if (Math.Abs(direction) == 8 && target.GetFile() != new Coord(pieceIndex).GetFile())
					break;

				int targetContents = board.GetSquareContents(target);

				AddMoveIfLegal(new Coord(pieceIndex), target);

				if (targetContents != 0)
					break;
			}
		}
	}

	private void GenerateCastlingMoves(int kingIndex) {
		bool[] castlingRights = board.GetAllCastlingAvailibility();

		int i = whiteMovesNext ? 0 : 2;
		int rank = whiteMovesNext ? 0 : 7;

		// King side castling
		if (castlingRights[i]) {
			// Make sure inbetween square and result square are empty and not attacked
			if (board.GetSquareContents(new Coord(rank, 7)) == (int)Piece.PieceType.Rook &&
				board.GetSquareContents(new Coord(rank, 5)) == 0 &&
				board.GetSquareContents(new Coord(rank, 6)) == 0 &&
				!MoveAllowsKingToBeTaken(new Move(kingIndex, kingIndex + 1)) &&
				!MoveAllowsKingToBeTaken(new Move(kingIndex, kingIndex + 2))) {
				moves.Add(new Move(kingIndex, kingIndex + 2, Move.Flag.Castling));
			}
		}

		// Queen side castling
		if (castlingRights[i + 1]) {
			// Make sure inbetween squares and result square are empty and not attacked
			if (board.GetSquareContents(new Coord(rank, 0)) == (int)Piece.PieceType.Rook &&
				board.GetSquareContents(new Coord(rank, 1)) == 0 &&
				board.GetSquareContents(new Coord(rank, 2)) == 0 &&
				!MoveAllowsKingToBeTaken(new Move(kingIndex, kingIndex - 1)) &&
				!MoveAllowsKingToBeTaken(new Move(kingIndex, kingIndex - 2))) {
				moves.Add(new Move(kingIndex, kingIndex - 2, Move.Flag.Castling));
			}
		}
	}

	private bool MoveAllowsKingToBeTaken(Move move) {
		if (!checkNextDepth)
			return false;

		List<int> kings = board.GetPieceTypes(this.whiteMovesNext, Piece.PieceType.King);

		//TODO: Remove when checkmate is implemented
		if (kings.Count < 1)
			return true;

		int king = kings[0];

		Board testBoard = board.Clone();

		testBoard.MakeMove(move);
		MoveGenerator moveGenerator = new MoveGenerator();
		List<Move> movesAtNextDepth = moveGenerator.GenerateMoves(testBoard, false);

		if (move.GetStartSquareIndex() == king) {
			for (int i = 0; i < movesAtNextDepth.Count; i++)
				if (movesAtNextDepth[i].GetTargetSquareIndex() == move.GetTargetSquareIndex())
					return true;
		} else {
			for (int i = 0; i < movesAtNextDepth.Count; i++)
				if (movesAtNextDepth[i].GetTargetSquareIndex() == king)
					return true;
		}

		return false;
	}

	private void AddMoveIfLegal(Coord startSquare, Coord targetSquare) {
		int targetPiece = board.GetSquareContents(targetSquare);
		if (targetPiece == 0 || Piece.IsWhite(targetPiece) != board.WhiteMovesNext()) {
			Move move = new Move(startSquare, targetSquare);
			if (!MoveAllowsKingToBeTaken(move))
				moves.Add(move);
		}
	}

	private void AddPawnMoveIfLegal(Move move) {
		if (MoveAllowsKingToBeTaken(move))
			return;

		Coord startSquare = new Coord(move.GetStartSquareIndex());
		Coord targetSquare = new Coord(move.GetTargetSquareIndex());

		if (targetSquare.GetRank() == (this.whiteMovesNext ? 7 : 0)) {
			moves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToQueen));
			moves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToKnight));
			moves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToRook));
			moves.Add(new Move(startSquare, targetSquare, Move.Flag.PromoteToBishop));
		} else {
			moves.Add(move);
		}


	}

	private void LogMoves() {
		string movesString = "Generated Moves: ";
		foreach (Move move in moves) {
			movesString += $"{move.ToString(board.GetSquareContents(new Coord(move.GetStartSquareIndex())))}, ";
		}
		Debug.Log(movesString.Trim(' ').Trim(','));
	}
}
