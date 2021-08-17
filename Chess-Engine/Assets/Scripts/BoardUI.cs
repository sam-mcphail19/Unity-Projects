using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardUI : MonoBehaviour {
	MeshRenderer[,] squareRenderers;
	SpriteRenderer[,] pieceRenderers;
	Shader squareShader;

	Move lastMoveMade;

	private const int FILE_COUNT = 8;
	private const int RANK_COUNT = 8;
	private const float pieceDragDepth = -0.2f;

	public Color lightColor;
	public Color darkColor;
	public Color lightColorSelected;
	public Color darkColorSelected;
	public Color lightColorLegalMove;
	public Color darkColorLegalMove;
	public Color lightColorLastMove;
	public Color darkColorLastMove;

	public float pieceScale = 1f;

	public PieceManager pieceManager;

	void Awake() {
		CreateBoard();
	}

	public void OnMoveMade(Board board, Move move) {
		UpdatePosition(board);
		ResetSquareColors();
		this.lastMoveMade = move;
		HighlightLastMoveMade();
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
		Coord coord = new Coord(rank, file);
		square.transform.name = coord.ToString();
		square.transform.parent = this.transform;
		square.transform.position = GetSquarePosition(coord);

		Material material = new Material(squareShader);

		squareRenderers[rank, file] = square.GetComponent<MeshRenderer>();
		squareRenderers[rank, file].material = material;

		SpriteRenderer piece = new GameObject("Piece").AddComponent<SpriteRenderer>();
		piece.transform.parent = square.transform;
		piece.transform.position = GetSquarePosition(coord);
		piece.transform.localScale = Vector3.one * pieceScale;
		pieceRenderers[rank, file] = piece;
	}

	public void UpdatePosition(Board board) {
		for (int rank = 0; rank < 8; rank++) {
			for (int file = 0; file < 8; file++) {
				Coord coord = new Coord(rank, file);
				int piece = board.GetSquareContents(coord);
				pieceRenderers[rank, file].sprite = pieceManager.getPieceSprite(piece);
				pieceRenderers[rank, file].transform.position = GetSquarePosition(coord);
			}
		}
	}

	public void HighlightLegalMoves(List<Move> moves, Coord fromSquare) {
		for (int i = 0; i < moves.Count; i++) {
			Move move = moves[i];
			if (move.GetStartSquareIndex() == fromSquare.GetIndex()) {
				Coord target = new Coord(move.GetTargetSquareIndex());
				SetSquareColor(target, IsSquareLight(target) ? lightColorLegalMove : darkColorLegalMove);
			}
		}
	}

	public void HighlightLastMoveMade() {
		Coord start = new Coord(this.lastMoveMade.GetStartSquareIndex());
		Coord target = new Coord(this.lastMoveMade.GetTargetSquareIndex());
		SetSquareColor(start, IsSquareLight(start) ? lightColorLastMove : darkColorLastMove);
		SetSquareColor(target, IsSquareLight(target) ? lightColorLastMove : darkColorLastMove);
	}

	public void DragPiece(Coord coord, Vector2 mousePos) {
		pieceRenderers[coord.GetRank(), coord.GetFile()].transform.position = new Vector3(mousePos.x, mousePos.y, pieceDragDepth);
	}

	public void SelectSquare(Coord coord) {
		SetSquareColor(coord, IsSquareLight(coord) ? lightColorSelected : darkColorSelected);
	}

	public void DeselectSquare() {
		ResetSquareColors();
	}

	public void ResetPiecePosition(Coord coord) {
		pieceRenderers[coord.GetRank(), coord.GetFile()].transform.position = GetSquarePosition(coord);
	}

	public bool TryGetSquareUnderMouse(Vector2 mouseWorld, out Coord coord) {
		coord = new Coord((int)(mouseWorld.y + 4), (int)(mouseWorld.x + 4));

		return coord.GetRank() >= 0 && coord.GetRank() < 8 && coord.GetFile() >= 0 && coord.GetFile() < 8;
	}

	public void ResetSquareColors() {
		for (int rank = 0; rank < FILE_COUNT; rank++)
			for (int file = 0; file < RANK_COUNT; file++) {
				Coord coord = new Coord(rank, file);
				SetSquareColor(coord, IsSquareLight(coord) ? lightColor : darkColor);
			}
	}

	private void SetSquareColor(Coord coord, Color color) {
		squareRenderers[coord.GetRank(), coord.GetFile()].material.color = color;
	}

	public static bool IsSquareLight(Coord coord) {
		return (coord.GetRank() + coord.GetFile()) % 2 == 1;
	}

	private Vector3 GetSquarePosition(Coord coord, int depth = 0) {
		return new Vector3(-3.5f + coord.GetFile(), -3.5f + coord.GetRank(), depth);
	}
}
