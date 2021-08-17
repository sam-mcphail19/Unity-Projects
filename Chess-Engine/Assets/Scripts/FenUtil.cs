using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FenUtil {

	//private const string INITIAL_POS = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
	private const string INITIAL_POS = "8/4k1B1/1p2p2r/1N3b2/pP3P1P/3R4/5QB1/1N4KR b  b3 0 0";

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

				board.PlacePieceOnSquare(piece, new Coord(rank, file));

				file++;
			}
		}

		board.SetWhiteMovesNext(sections[1].Equals("w"));

		board.SetAllCastlingAvailability(
			sections[2].Contains("K"),
			sections[2].Contains("Q"),
			sections[2].Contains("k"),
			sections[2].Contains("q"));

		if (sections.Length > 3) {
			if (!sections[3].Equals("-"))
				board.SetEnPassantTarget(Coord.SquareNameToSquarePos(sections[3]).GetFile() + 1);
		}

		if (sections.Length > 4)
			board.SetFiftyMoveRuleCounter(int.Parse(sections[4]));

		if (sections.Length > 5)
			board.SetMoveCounter(int.Parse(sections[5]));

		return board;
	}

	public static string CurrentBoardPositionToFenString(Board board) {
		string fen = "";

		for (int rank = 7; rank >= 0; rank--) {
			int emptySpaces = 0;
			for (int file = 0; file < 8; file++) {
				Coord currentSquare = new Coord(rank, file);
				int piece = board.GetSquareContents(currentSquare);
				if (piece == 0) {
					emptySpaces++;
					continue;
				}

				if (emptySpaces > 0) {
					fen += emptySpaces;
					emptySpaces = 0;
				}

				char pieceChar = ' ';
				switch (Piece.GetPieceType(piece)) {
					case Piece.PieceType.Pawn:
						pieceChar = 'p';
						break;

					case Piece.PieceType.Knight:
						pieceChar = 'n';
						break;

					case Piece.PieceType.King:
						pieceChar = 'k';
						break;

					case Piece.PieceType.Bishop:
						pieceChar = 'b';
						break;

					case Piece.PieceType.Rook:
						pieceChar = 'r';
						break;

					case Piece.PieceType.Queen:
						pieceChar = 'q';
						break;
				}
				fen += Piece.IsWhite(piece) ? Char.ToUpper(pieceChar) : pieceChar;
			}

			if (emptySpaces > 0)
				fen += emptySpaces;

			if (rank != 0)
				fen += "/";
		}

		fen += " " + (board.WhiteMovesNext() ? "w" : "b") + " ";

		bool[] castlingRights = board.GetAllCastlingAvailibility();
		fen += castlingRights[0] ? "K" : "";
		fen += castlingRights[1] ? "Q" : "";
		fen += castlingRights[2] ? "k" : "";
		fen += castlingRights[3] ? "q" : "";

		fen += " " + board.GetEnPassantTargetName();

		fen += " " + board.GetFiftyMoveRuleCounter();

		fen += " " + board.GetMoveCounter();

		return fen;
	}

	public static Board LoadInitialPosition() {
		return LoadPositionFromFenString(INITIAL_POS);
	}
}
