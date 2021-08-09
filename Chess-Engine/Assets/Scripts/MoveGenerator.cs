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
			// This is temporary while promotion is not implemented
			if (targetIndex < 0 || targetIndex > 63)
				continue;

			Coord target = new Coord(targetIndex);
			Coord pawn = new Coord(pawnIndex);
			int targetPiece = board.GetSquareContents(target);

			if (targetPiece == 0) {
				Move move = new Move(pawnIndex, targetIndex);
				if (!MoveAllowsKingToBeTaken(move))
					moves.Add(move);

				if ((this.whiteMovesNext && pawn.GetRank() == 1) || (!this.whiteMovesNext && pawn.GetRank() == 6)) {
					Coord doubleMoveTarget = new Coord(this.whiteMovesNext ? pawnIndex + 16 : pawnIndex - 16);
					move = new Move(pawnIndex, doubleMoveTarget.GetIndex(), (int)Move.Flag.PawnTwoForward);
					if (board.GetSquareContents(doubleMoveTarget) == 0 && !MoveAllowsKingToBeTaken(move))
						moves.Add(move);
				}
			}

			targetIndex = this.whiteMovesNext ? pawnIndex + 7 : pawnIndex - 7;
			target = new Coord(targetIndex);
			targetPiece = board.GetSquareContents(target);

			if (Math.Abs(pawn.GetFile() - target.GetFile()) == 1 &&
				targetPiece != 0 && Piece.IsWhite(targetPiece) != board.WhiteMovesNext()) {
				Move move = new Move(pawnIndex, targetIndex);
				if (!MoveAllowsKingToBeTaken(move))
					moves.Add(move);
			}

			targetIndex = this.whiteMovesNext ? pawnIndex + 9 : pawnIndex - 9;
			target = new Coord(targetIndex);
			targetPiece = board.GetSquareContents(target);

			if (Math.Abs(pawn.GetFile() - target.GetFile()) == 1 &&
				targetPiece != 0 && Piece.IsWhite(targetPiece) != board.WhiteMovesNext()) {
				Move move = new Move(pawnIndex, targetIndex);
				if (!MoveAllowsKingToBeTaken(move))
					moves.Add(move);
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

				AddMoveIfLegal(board.GetSquareContents(target), knightIndex, target);
			}
		}
	}

	private void GenerateKingMoves() {
		List<int> kings = board.GetPieceTypes(this.whiteMovesNext, Piece.PieceType.King);
		if (kings.Count < 1) {
			string movingSide = whiteMovesNext ? "White" : "Black";
			//Debug.LogError($"No Kings on the board for {movingSide}");
			return;
		}

		int kingIndex = kings[0];

		foreach (int direction in KING_MOVES) {
			int targetIndex = kingIndex + direction;
			if (targetIndex < 0 || targetIndex > 63)
				continue;

			Coord king = new Coord(kingIndex);
			Coord target = new Coord(kingIndex);
			int targetContents = board.GetSquareContents(target);

			if (Math.Abs(king.GetRank() - target.GetRank()) + Math.Abs(king.GetFile() - target.GetFile()) > 2)
				continue;

			AddMoveIfLegal(targetContents, kingIndex, target);
		}

		//GenerateCastlingMoves(king);
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

	private void GenerateDiagonalMovesForPiece(int piece) {
		foreach (int direction in DIAGONAL_MOVES) {
			for (int i = piece + direction; i < 63 && i > 0; i += direction) {
				Coord target = new Coord(i);

				if (BoardUI.IsSquareLight(target) != BoardUI.IsSquareLight(new Coord(piece)))
					break;

				int targetContents = board.GetSquareContents(target);

				AddMoveIfLegal(targetContents, piece, target);

				if (targetContents != 0)
					break;
			}
		}
	}

	private void GenerateStraightMovesForPiece(int piece) {
		foreach (int direction in STRAIGHT_MOVES) {
			for (int i = piece + direction; i < 63 && i > 0; i += direction) {
				Coord target = new Coord(i);

				if (Math.Abs(direction) == 1 && target.GetRank() != new Coord(piece).GetRank())
					break;
				if (Math.Abs(direction) == 8 && target.GetFile() != new Coord(piece).GetFile())
					break;

				int targetContents = board.GetSquareContents(target);

				AddMoveIfLegal(targetContents, piece, target);

				if (targetContents != 0)
					break;
			}
		}
	}

	private void GenerateCastlingMoves(int king) {
		bool[] castlingRights = board.GetAllCastlingAvailibility();

		for (int i = whiteMovesNext ? 0 : 2; i < (whiteMovesNext ? 2 : 4); i++) {
			if (castlingRights[i]) {
				// king side castling
				if (i % 2 == 1) {
					//if (!MoveAllowsKingToBeTaken(new Move()))
				}
			}
		}
	}

	private bool MoveAllowsKingToBeTaken(Move move) {
		if (!checkNextDepth)
			return false;

		List<int> kings = board.GetPieceTypes(this.whiteMovesNext, Piece.PieceType.King);
		if (kings.Count < 1) {
			string movingSide = whiteMovesNext ? "White" : "Black";
			//Debug.LogError($"No Kings on the board for {movingSide}");
			return true;
		}

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

	private void AddMoveIfLegal(int targetPiece, int movingPiece, Coord target) {
		if (targetPiece == 0 || Piece.IsWhite(targetPiece) != board.WhiteMovesNext()) {
			Move move = new Move(movingPiece, target.GetIndex());
			if (!MoveAllowsKingToBeTaken(move))
				moves.Add(move);
		}
	}
}
