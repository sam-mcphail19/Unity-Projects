using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceManager : MonoBehaviour
{
	public PieceSprites pieces;

    public Sprite getPieceSprite(int piece) {

		return pieces.w_pawn;        
    }

	[System.Serializable]
	public class PieceSprites
	{
		public Sprite w_pawn, w_rook, w_knight, w_bishop, w_queen, w_king;
		public Sprite b_pawn, b_rook, b_knight, b_bishop, b_queen, b_king;

		public Sprite this[int i]
		{
			get
			{
				return new Sprite[] { w_pawn, w_rook, w_knight, w_bishop, w_queen, w_king,
									  b_pawn, b_rook, b_knight, b_bishop, b_queen, b_king}[i];
			}
		}
	}
}
