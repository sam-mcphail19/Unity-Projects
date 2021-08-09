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

		foreach (int pawn in pawns) {
			int target = this.whiteMovesNext ? pawn + 8 : pawn - 8;
			// This is temporary while promotion is not implemented
			if (target < 0 || target > 63)
				continue;
			(int, int) targetPos = Board.IndexToSquarePos(target);
			(int, int) pawnPos = Board.IndexToSquarePos(pawn);
			int targetPiece = board.GetSquareContents(targetPos);

			if (board.GetSquareContents(targetPos) == 0) {
				AddPawnMoveIfLegal(targetPiece, pawn, targetPos, false);

				if ((this.whiteMovesNext && pawnPos.Item1 == 1) || (!this.whiteMovesNext && pawnPos.Item1 == 6)) {
					(int, int) doubleMoveTargetPos = Board.IndexToSquarePos(this.whiteMovesNext ? pawn + 16 : pawn - 16);
					AddPawnMoveIfLegal(board.GetSquareContents(doubleMoveTargetPos), pawn, doubleMoveTargetPos, false);
				}
			}

			target = this.whiteMovesNext ? pawn + 7 : pawn - 7;
			targetPiece = board.GetSquareContents(Board.IndexToSquarePos(target));
			if (Math.Abs(pawnPos.Item1 - Board.IndexToSquarePos(target).Item1) == 1)
				AddPawnMoveIfLegal(targetPiece, pawn, Board.IndexToSquarePos(target), true);

			target = this.whiteMovesNext ? pawn + 9 : pawn - 9;
			targetPiece = board.GetSquareContents(Board.IndexToSquarePos(target));
			if (Math.Abs(pawnPos.Item1 - Board.IndexToSquarePos(target).Item1) == 1)
				AddPawnMoveIfLegal(targetPiece, pawn, Board.IndexToSquarePos(target), true);

		}
	}

	private void GenerateKnightMoves() {
		List<int> knights = board.GetPieceTypes(this.whiteMovesNext, Piece.PieceType.Knight);

		foreach (int knight in knights) {
			foreach (int target in KNIGHT_TARGETS) {
				int targetIndex = knight + target;
				if (targetIndex < 0 || targetIndex > 63)
					continue;

				(int, int) knightSquare = Board.IndexToSquarePos(knight);
				(int, int) targetSquare = Board.IndexToSquarePos(targetIndex);
				if (Math.Abs(knightSquare.Item1 - targetSquare.Item1) + Math.Abs(knightSquare.Item2 - targetSquare.Item2) != 3)
					continue;

				AddMoveIfLegal(board.GetSquareContents(targetSquare), knight, targetSquare);
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

		int king = kings[0];

		foreach (int direction in KING_MOVES) {
			int target = king + direction;
			if (target < 0 || target > 63)
				continue;

			(int, int) kingPos = Board.IndexToSquarePos(king);
			(int, int) targetPos = Board.IndexToSquarePos(target);
			int targetContents = board.GetSquareContents(targetPos);

			if (Math.Abs(kingPos.Item1 - targetPos.Item1) + Math.Abs(kingPos.Item2 - targetPos.Item2) > 2)
				continue;

			AddMoveIfLegal(targetContents, king, targetPos);
		}
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
				(int, int) targetPos = Board.IndexToSquarePos(i);

				if (BoardUI.IsSquareLight(targetPos) != BoardUI.IsSquareLight(Board.IndexToSquarePos(piece)))
					break;

				int targetContents = board.GetSquareContents(targetPos);

				AddMoveIfLegal(targetContents, piece, targetPos);

				if (targetContents != 0)
					break;
			}
		}
	}

	private void GenerateStraightMovesForPiece(int piece) {
		foreach (int direction in STRAIGHT_MOVES) {
			for (int i = piece + direction; i < 63 && i > 0; i += direction) {
				(int, int) targetPos = Board.IndexToSquarePos(i);

				if (Math.Abs(direction) == 1 && targetPos.Item1 != Board.IndexToSquarePos(piece).Item1)
					break;
				if (Math.Abs(direction) == 8 && targetPos.Item2 != Board.IndexToSquarePos(piece).Item2)
					break;

				int targetContents = board.GetSquareContents(targetPos);

				AddMoveIfLegal(targetContents, piece, targetPos);

				if (targetContents != 0)
					break;
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

		if (move.GetStartSquare() == king) {
			for (int i = 0; i < movesAtNextDepth.Count; i++)
				if (movesAtNextDepth[i].GetTargetSquare() == move.GetTargetSquare())
					return true;
		} else {
			for (int i = 0; i < movesAtNextDepth.Count; i++)
				if (movesAtNextDepth[i].GetTargetSquare() == king)
					return true;
		}



		return false;
	}

	private void AddMoveIfLegal(int targetPiece, int movingPiece, (int, int) targetPos) {
		if (targetPiece == 0 || Piece.IsWhite(targetPiece) != board.WhiteMovesNext()) {
			Move move = new Move(movingPiece, Board.SquarePosToIndex(targetPos));
			if (!MoveAllowsKingToBeTaken(move))
				moves.Add(move);
		}
	}

	private void AddPawnMoveIfLegal(int targetPiece, int movingPawn, (int, int) targetPos, bool isAttackingMove) {
		if (isAttackingMove != (targetPiece == 0))
			AddMoveIfLegal(targetPiece, movingPawn, targetPos);
	}
}
