using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Move {
	public enum Flag {
		None = 0,
		EnPassantCapture = 1,
		Castling = 2,
		PromoteToQueen = 3,
		PromoteToKnight = 4,
		PromoteToRook = 5,
		PromoteToBishop = 6,
		PawnTwoForward = 7
	}

	// bit 0-5: from square(0 to 63)
	// bit 6-11: to square(0 to 63)
	// bit 12-15: flag

	private const int fromSquareMask = 63;
	private const int toSquareMask = 4032;
	private const int flagMask = 61440;

	private ushort representation;

	public Move() {
		this.representation = 0;
	}

	public Move(ushort move) {
		this.representation = move;
	}

	public Move(int startSquare, int targetSquare) {
		this.representation = (ushort)(startSquare | targetSquare << 6);
	}

	public Move(Coord startSquare, Coord targetSquare) {
		this.representation = (ushort)(startSquare.GetIndex() | targetSquare.GetIndex() << 6);
	}

	public Move(int startSquare, int targetSquare, Flag flag) {
		this.representation = (ushort)(startSquare | targetSquare << 6 | (int)flag << 12);
	}

	public Move(Coord startSquare, Coord targetSquare, Flag flag) {
		this.representation = (ushort)(startSquare.GetIndex() | targetSquare.GetIndex() << 6 | (int)flag << 12);
	}

	public int GetStartSquareIndex() {
		return fromSquareMask & representation;
	}

	public int GetTargetSquareIndex() {
		return (toSquareMask & representation) >> 6;
	}

	public int GetFlag() {
		return (flagMask & representation) >> 12;
	}

	public bool IsPromotion() {
		int flag = GetFlag();
		return flag == (int)Flag.PromoteToBishop ||
			flag == (int)Flag.PromoteToKnight ||
			flag == (int)Flag.PromoteToRook ||
			flag == (int)Flag.PromoteToQueen;
	}

	public override string ToString() {
		return $"From {GetStartSquareIndex()} to {GetTargetSquareIndex()}";
	}

	public string ToString(Board board) {
		if (GetFlag() == (int)Flag.Castling)
			return new Coord(GetTargetSquareIndex()).GetFile() == 2 ? "O-O-O" : "O-O";

		Coord startSquare = new Coord(GetStartSquareIndex());
		Coord targetSquare = new Coord(GetTargetSquareIndex());
		if ((board.GetSquareContents(targetSquare) != 0)) {
			string pieceType = Piece.GetPieceTypeAbbreviation(board.GetSquareContents(startSquare));
			return $"{(pieceType == "" ? startSquare.GetFileName() : pieceType)}x{targetSquare}";
		}
		return $"{Piece.GetPieceTypeAbbreviation(board.GetSquareContents(new Coord(GetStartSquareIndex())))}{targetSquare}";
	}
}
