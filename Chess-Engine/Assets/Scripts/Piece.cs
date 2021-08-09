public static class Piece {
	public const int WHITE = 8;

	public const int PAWN = 1;
	public const int KNIGHT = 2;
	public const int KING = 3;
	public const int BISHOP = 5;
	public const int ROOK = 6;
	public const int QUEEN = 7;

	public static int pieceTypeMask = 7;

	public static int NewPiece(PieceType pieceType, Color color) {
		return (int)pieceType | (int)color;
	}

	public static PieceType GetPieceType(int piece) {
		return (piece & pieceTypeMask) switch
		{
			0 => PieceType.None,
			1 => PieceType.Pawn,
			2 => PieceType.Knight,
			3 => PieceType.King,
			5 => PieceType.Bishop,
			6 => PieceType.Rook,
			7 => PieceType.Queen,
			_ => throw new System.InvalidOperationException($"Piece: {piece} is not a valid piece type"),
		};
	}

	public static bool IsWhite(int piece) {
		return (piece & WHITE) != 0;
	}

	public static bool CanSlide(int piece) {
		return (piece & 0b100) != 0;
	}

	public static bool CanSlideDiagonal(int piece) {
		return (piece & 0b101) != 0;
	}

	public static bool CanSlideStraight(int piece) {
		return (piece & 0b110) != 0;
	}

	public enum Color {
		White = 8,
		Black = 0
	}

	public enum PieceType {
		Pawn,
		Knight,
		Bishop,
		Rook,
		Queen,
		King,
		None
	}

	public static string GetPieceTypeAbbreviation(PieceType type) {
		return (type) switch
		{
			PieceType.Pawn => "",
			PieceType.Knight => "N",
			PieceType.Bishop => "B",
			PieceType.Rook => "R",
			PieceType.Queen => "Q",
			PieceType.King => "K",
			_ => throw new System.ArgumentException($"Piece: {type} is not a valid piece type"),
		};
	}

	public static string GetPieceTypeAbbreviation(int type) {
		return GetPieceTypeAbbreviation(GetPieceType(type));
	}
}
