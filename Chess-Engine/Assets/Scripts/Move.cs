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

	public Move(int startSquare, int targetSquare, int flag) {
		this.representation = (ushort)(startSquare | targetSquare << 6 | flag << 12);
	}

	public int GetStartSquare() {
		return fromSquareMask & representation;
	}

	public int GetTargetSquare() {
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
}
