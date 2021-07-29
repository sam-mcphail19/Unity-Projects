using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardUI : MonoBehaviour {
	MeshRenderer[,] squareRenderers;
	SpriteRenderer[,] pieceRenderers;
	Shader squareShader;

	Board board;

	private const int FILE_COUNT = 8;
	private const int RANK_COUNT = 8;
	private const int SQUARE_SIZE = 1;

	public Color lightColor;
	public Color darkColor;

	public float pieceScale = 1f;

	public PieceManager pieceManager;

	void Awake() {
		this.board = FenUtil.LoadInitialPosition();
		CreateBoard();
	}

	private void Update() {
		ResetSquareColors();
		UpdatePosition(this.board);
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
		square.transform.position = this.transform.position + new Vector3(file * SQUARE_SIZE, rank * SQUARE_SIZE, 0);

		Material material = new Material(squareShader);

		squareRenderers[file, rank] = square.GetComponent<MeshRenderer>();
		squareRenderers[file, rank].material = material;

		SpriteRenderer piece = new GameObject("Piece").AddComponent<SpriteRenderer>();
		piece.transform.parent = square.transform;
		piece.transform.position = this.transform.position + new Vector3(file * SQUARE_SIZE, rank * SQUARE_SIZE, 0);
		piece.transform.localScale = Vector3.one * pieceScale;
		pieceRenderers[file, rank] = piece;
	}

	public void UpdatePosition(Board board) {
		for (int rank = 0; rank < 8; rank++) {
			for (int file = 0; file < 8; file++) {
				int piece = board.GetSquareContents(rank, file);
				if (piece != 0) {
					pieceRenderers[file, rank].sprite = pieceManager.getPieceSprite(piece);
					pieceRenderers[file, rank].transform.position = this.transform.position + new Vector3(file * SQUARE_SIZE, rank * SQUARE_SIZE, 0);
				}
			}
		}
	}

	private void ResetSquareColors() {
		for (int rank = 0; rank < FILE_COUNT; rank++)
			for (int file = 0; file < RANK_COUNT; file++)
				SetSquareColor(rank, file, IsSquareLight(rank, file) ? lightColor : darkColor);
	}

	private void SetSquareColor(int rank, int file, Color color) {
		squareRenderers[file, rank].material.color = color;
	}

	private bool IsSquareLight(int rank, int file) {
		return (rank + file) % 2 == 1;
	}
}
