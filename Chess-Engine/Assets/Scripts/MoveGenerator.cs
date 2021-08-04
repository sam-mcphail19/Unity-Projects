using System;
using System.Collections.Generic;
using UnityEngine;

public class MoveGenerator {

	private const int FILE_COUNT = 8;
	private const int RANK_COUNT = 8;

	private static readonly int[] KNIGHT_TARGETS = { -17, -15, -10, -6, 6, 10, 15, 17 };

	List<Move> moves;
	Board board;

	bool inCheck;
	bool inDoubleCheck;
	bool whiteMovesNext;

	public List<Move> GenerateMoves(Board board) {
		this.board = board;
		Init();

		//CalculateAttackData();
		//GenerateKingMoves();

		// Only king moves are valid in a double check position, so can return early.
		/*
		if (inDoubleCheck) {
			return moves;
		}
		*/
		//GenerateSlidingMoves();
		GenerateKnightMoves();
		//GeneratePawnMoves();

		return moves;
	}

	private void Init() {
		moves = new List<Move>(64);
		inCheck = false;
		inDoubleCheck = false;
		whiteMovesNext = board.WhiteMovesNext();
	}

	private void GeneratePawnMoves() {
		List<int> pawns = board.GetPieceTypes(this.whiteMovesNext, Piece.PieceType.Pawn);

		foreach (int pawn in pawns) {
			//moves.Add(new Move(pawn, pawn + 8));
			//moves.Add(new Move(pawn, pawn + 16));
		}
	}

	private void GenerateKnightMoves() {
		List<int> knights = board.GetPieceTypes(this.whiteMovesNext, Piece.PieceType.Knight);

		foreach (int knight in knights) {
			foreach (int target in KNIGHT_TARGETS) {
				int targetIndex = knight + target;

				(int, int) knightSquare = Board.IndexToSquarePos(knight);
				(int, int) targetSquare = Board.IndexToSquarePos(targetIndex);
				if (Math.Abs(knightSquare.Item1 - targetSquare.Item1) + Math.Abs(knightSquare.Item2 - targetSquare.Item2) != 3)
					continue;

				if (Board.IsIndexInBounds(targetIndex)) {
					int targetSquareContent = board.GetSquareContents(targetSquare);
					if (Piece.GetPieceType(targetSquareContent) != Piece.PieceType.None)
						if (Piece.IsWhite(targetSquareContent) == this.whiteMovesNext)
							continue;
					moves.Add(new Move(knight, targetIndex));
				}
			}
		}
	}

	private void GenerateSlidingMoves() {
		GenerateQueenMoves();
		GenerateRookMoves();
		GenerateBishopMoves();
	}

	private void GenerateKingMoves() {
		throw new NotImplementedException();
	}

	private void GenerateAttackData() {
		throw new NotImplementedException();
	}

	private void GenerateQueenMoves() {
		throw new NotImplementedException();
	}

	private void GenerateRookMoves() {
		throw new NotImplementedException();
	}

	private void GenerateBishopMoves() {
		throw new NotImplementedException();
	}
}
