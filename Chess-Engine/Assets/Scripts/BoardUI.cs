using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardUI : MonoBehaviour {
	MeshRenderer[,] squareRenderers;
	SpriteRenderer[,] pieceRenderers;
	Shader squareShader;

	private const int FILE_COUNT = 8;
	private const int RANK_COUNT = 8;
	private const int SQUARE_SIZE = 1;
	private const float pieceDragDepth = -0.2f;

	public Color lightColor;
	public Color darkColor;
	public Color lightColorSelected;
	public Color darkColorSelected;
	public Color lightColorLegalMove;
	public Color darkColorLegalMove;

	public float pieceScale = 1f;

	public PieceManager pieceManager;

	void Awake() {
		CreateBoard();
	}

	public void OnMoveMade(Board board, Move move) {
		UpdatePosition(board);
		ResetSquareColors();
		//HighlightMove(move); // To be implemeted - highlight the new position of the last piece moved, and its old position
	}

	void CreateBoard() {
		squareRenderers = new MeshRenderer[FILE_COUNT, RANK_COUNT];
		pieceRenderers = new SpriteRenderer[FILE_COUNT, RANK_COUNT];
		squareShader = Shader.Find("Unlit/Color");

		for (int rank = 0; rank < FILE_COUNT; rank++)
			for (int file = 0; file < RANK_COUNT; file++)
				CreateSquare(rank, file);

		ResetSquareColors();
	}

	// How many "meters" the square's width and height is
	// The file, and the rank the square
	private void CreateSquare(int rank, int file) {
		GameObject square = GameObject.CreatePrimitive(PrimitiveType.Quad);
		square.transform.name = Board.SquarePosToSquareName(rank, file);
		square.transform.parent = this.transform;
		square.transform.position = this.transform.position + GetSquarePosition(rank, file);

		Material material = new Material(squareShader);

		squareRenderers[file, rank] = square.GetComponent<MeshRenderer>();
		squareRenderers[file, rank].material = material;

		SpriteRenderer piece = new GameObject("Piece").AddComponent<SpriteRenderer>();
		piece.transform.parent = square.transform;
		piece.transform.position = this.transform.position + GetSquarePosition(rank, file);
		piece.transform.localScale = Vector3.one * pieceScale;
		pieceRenderers[file, rank] = piece;
	}

	public void UpdatePosition(Board board) {
		for (int rank = 0; rank < 8; rank++) {
			for (int file = 0; file < 8; file++) {
				int piece = board.GetSquareContents(rank, file);
				if (piece != 0) {
					pieceRenderers[file, rank].sprite = pieceManager.getPieceSprite(piece);
					pieceRenderers[file, rank].transform.position = this.transform.position + GetSquarePosition(rank, file);
				}
			}
		}
	}

	public void HighlightLegalMoves(List<Move> moves, (int, int) fromSquare) {
		for (int i = 0; i < moves.Count; i++) {
			Move move = moves[i];
			if (move.GetStartSquare() == Board.SquarePosToIndex(fromSquare)) {
				(int, int) targetSquare = Board.IndexToSquarePos(move.GetTargetSquare());
				SetSquareColor(targetSquare, IsSquareLight(targetSquare) ? lightColorLegalMove : darkColorLegalMove);
			}
		}
	}

	public void DragPiece(int rank, int file, Vector2 mousePos) {
		Debug.Log($"Dragging piece to mousePos: {mousePos}");
		pieceRenderers[rank, file].transform.position = new Vector3(mousePos.x, mousePos.y, pieceDragDepth);
	}

	public void SelectSquare(int rank, int file) {
		SetSquareColor(rank, file, IsSquareLight(rank, file) ? lightColorSelected : darkColorSelected);
	}

	public void SelectSquare((int, int) squarePos) {
		SelectSquare(squarePos.Item1, squarePos.Item2);
	}

	public void DeselectSquare() {
		ResetSquareColors();
	}

	public void ResetPiecePosition(int rank, int file) {
		Vector3 pos = GetSquarePosition(rank, file);
		pieceRenderers[file, rank].transform.position = pos;
	}

	public void ResetPiecePosition((int, int) squarePos) {
		ResetPiecePosition(squarePos.Item1, squarePos.Item2);
	}

	public bool TryGetSquareUnderMouse(Vector2 mouseWorld, out (int, int) squarePos) {
		squarePos.Item1 = (int)(mouseWorld.y + 4);
		squarePos.Item2 = (int)(mouseWorld.x + 4);

		return squarePos.Item2 >= 0 && squarePos.Item2 < 8 && squarePos.Item1 >= 0 && squarePos.Item1 < 8;
	}

	public void ResetSquareColors() {
		for (int rank = 0; rank < FILE_COUNT; rank++)
			for (int file = 0; file < RANK_COUNT; file++)
				SetSquareColor(rank, file, IsSquareLight(rank, file) ? lightColor : darkColor);
	}

	private void SetSquareColor(int rank, int file, Color color) {
		squareRenderers[file, rank].material.color = color;
	}

	private void SetSquareColor((int, int) squarePos, Color color) {
		SetSquareColor(squarePos.Item1, squarePos.Item2, color);
	}

	private bool IsSquareLight(int rank, int file) {
		return (rank + file) % 2 == 1;
	}

	private bool IsSquareLight((int, int) squarePos) {
		return IsSquareLight(squarePos.Item1, squarePos.Item2);
	}

	private Vector3 GetSquarePosition(int rank, int file, int depth = 0) {
		return new Vector3(file * SQUARE_SIZE, rank * SQUARE_SIZE, depth);
	}
}
