using UnityEngine;

public class PieceManager : MonoBehaviour {
	public PieceSprites pieces;

	public Sprite getPieceSprite(int piece) {
		if (Piece.IsWhite(piece)) {
			return (piece & Piece.pieceTypeMask) switch
			{
				1 => pieces.w_pawn,
				2 => pieces.w_knight,
				3 => pieces.w_king,
				5 => pieces.w_bishop,
				6 => pieces.w_rook,
				7 => pieces.w_queen,
				_ => throw new System.ArgumentException($"Piece: {piece} is not a valid piece type"),
			};
		}

		return (piece & Piece.pieceTypeMask) switch
		{
			1 => pieces.b_pawn,
			2 => pieces.b_knight,
			3 => pieces.b_king,
			5 => pieces.b_bishop,
			6 => pieces.b_rook,
			7 => pieces.b_queen,
			_ => throw new System.ArgumentException($"Piece: {piece} is not a valid piece type"),
		};
	}

	[System.Serializable]
	public class PieceSprites {
		public Sprite w_pawn, w_rook, w_knight, w_bishop, w_queen, w_king;
		public Sprite b_pawn, b_rook, b_knight, b_bishop, b_queen, b_king;

		public Sprite this[int i] {
			get {
				return new Sprite[] { w_pawn, w_rook, w_knight, w_bishop, w_queen, w_king,
									  b_pawn, b_rook, b_knight, b_bishop, b_queen, b_king}[i];
			}
		}
	}
}
