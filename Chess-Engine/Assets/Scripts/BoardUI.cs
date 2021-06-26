using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardUI : MonoBehaviour
{
    MeshRenderer[,] squareRenderers;
    SpriteRenderer[,] pieceRenderers;
    Shader squareShader;

    private const int FILE_COUNT = 8;
    private const int RANK_COUNT = 8;
    private const int SQUARE_SIZE = 1;

    public Color lightColor;
    public Color darkColor;

    public PieceManager pieceManager;

    void Awake()
    {
        CreateBoard();
    }

    private void Update()
    {
        ResetSquareColors();
        UpdatePosition();
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
    private void CreateSquare(int rank, int file)
    {
        GameObject square = GameObject.CreatePrimitive(PrimitiveType.Quad);
        square.transform.name = SquarePosToSquareName(rank, file);
        square.transform.parent = this.transform;
        square.transform.position = this.transform.position + new Vector3(file * SQUARE_SIZE, rank * SQUARE_SIZE, 0);

        Material material = new Material(squareShader);

        squareRenderers[file, rank] = square.GetComponent<MeshRenderer>();
        squareRenderers[file, rank].material = material;

        SpriteRenderer piece = new GameObject("Piece").AddComponent<SpriteRenderer>();
        piece.transform.parent = square.transform;
        piece.transform.position = this.transform.position + new Vector3(file * SQUARE_SIZE, rank * SQUARE_SIZE, 0);
        piece.transform.localScale = Vector3.one * 0.5f;
        pieceRenderers[file, rank] = piece;
    }

    //public void UpdatePosition(Board board)
    public void UpdatePosition()
    {
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                //Coord coord = new Coord(file, rank);
                //int piece = board.Square[BoardRepresentation.IndexFromCoord(coord.fileIndex, coord.rankIndex)];
                pieceRenderers[file, rank].sprite = pieceManager.getPieceSprite(1);
                pieceRenderers[file, rank].transform.position = this.transform.position + new Vector3(file * SQUARE_SIZE, rank * SQUARE_SIZE, 0);
            }
        }

    }

    private string SquarePosToSquareName(int rank, int file) {
        const string letters = "abcdefgh";

        if (file > FILE_COUNT - 1)
            throw new ArgumentException("Square's file cannot be greater than the board width");

        return letters[file].ToString() + (rank+1).ToString();
    }

    private void ResetSquareColors() {
        for (int rank = 0; rank < FILE_COUNT; rank++)
            for (int file = 0; file < RANK_COUNT; file++)
                SetSquareColor(rank, file, isSquareLight(rank, file) ? lightColor : darkColor);
    }

    private void SetSquareColor(int rank, int file, Color color) {
        squareRenderers[file, rank].material.color = color;
    }

    private bool isSquareLight(int rank, int file) {
        return (rank + file) % 2 == 1;
    }
}
