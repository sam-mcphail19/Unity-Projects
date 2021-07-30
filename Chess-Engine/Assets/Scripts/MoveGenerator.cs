using System;
using System.Collections.Generic;
using UnityEngine;

public class MoveGenerator {

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
		//GenerateKnightMoves();
		GeneratePawnMoves();

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
			moves.Add(new Move(pawn, pawn+8));
			moves.Add(new Move(pawn, pawn+16));
		}
	}

	private void GenerateKnightMoves() {
		throw new NotImplementedException();
	}

	private void GenerateSlidingMoves() {
		throw new NotImplementedException();
	}

	private void GenerateKingMoves() {
		throw new NotImplementedException();
	}

	private void CalculateAttackData() {
		throw new NotImplementedException();
	}
}
