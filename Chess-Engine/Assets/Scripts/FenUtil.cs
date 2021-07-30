using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FenUtil {

	private const string INITIAL_POS = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";

	private static Dictionary<char, int> symbolToPieceType = new Dictionary<char, int>() {
		{'p', Piece.PAWN},
		{'n', Piece.KNIGHT},
		{'k', Piece.KING},
		{'b', Piece.BISHOP},
		{'r', Piece.ROOK},
		{'q', Piece.QUEEN}
	};

	public static Board LoadPositionFromFenString(string fen) {
		Board board = new Board();

		string[] sections = fen.Split(' ');

		int rank = 7;
		int file = 0;

		foreach (char current in sections[0]) {
			if (current == '/') {
				rank--;
				file = 0;
			} else if (char.IsDigit(current)) {
				file += (int)char.GetNumericValue(current);
			} else {
				char currentLowered = char.ToLower(current);
				if (!symbolToPieceType.TryGetValue(currentLowered, out int pieceType)) {
					throw new System.ArgumentException($"Character: {current} is not a valid piece");
				}

				int piece = pieceType;

				if (current != currentLowered)
					piece |= Piece.WHITE;

				board.PlacePieceOnSquare(piece, rank, file);

				file++;
			}
		}

		board.SetWhiteMovesNext(sections[1].Equals("w"));

		board.SetAllCastlingAvailability(
			sections[2].Contains("K"),
			sections[2].Contains("Q"),
			sections[2].Contains("k"),
			sections[2].Contains("q"));

		if (sections[3].Equals("-"))
			board.SetEnPassantTarget(0, 0);
		else {
			(int, int) enPassantTarget = Board.SquareNameToSquarePos(sections[3]);
			board.SetEnPassantTarget(enPassantTarget.Item1, enPassantTarget.Item2);
		}
		int fiftyMoveRuleCount = int.Parse(sections[4]);
		board.SetFiftyMoveRuleCounter(fiftyMoveRuleCount);

		int moveCount = int.Parse(sections[5]);
		board.SetMoveCounter(moveCount);

		Debug.Log($"FEN string\"{fen}\" loaded.\n" +
			$"GameState:\n" +
			$"WhiteMovesNext: {board.WhiteMovesNext()}\n" +
			$"CastlingAvailability: {board.GetAllCastlingAvailibility()}\n" +
			$"EnPassantTarget: {board.GetEnPassantTarget()}\n" +
			$"FiftyMoveRuleCount: {board.GetFiftyMoveRuleCounter()}\n" +
			$"MoveCount: {board.GetMoveCounter()}");
		return board;
	}

	public static Board LoadInitialPosition() {
		return LoadPositionFromFenString(INITIAL_POS);
	}
}
