using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPlayer : Player {
	public enum InputState {
		None,
		PieceSelected,
		DraggingPiece
	}

	InputState currentState;

	BoardUI boardUI;
	Camera cam;
	Board board;
	MoveGenerator moveGenerator;

	Coord selectedPieceSquare;

	public HumanPlayer(Board board) {
		boardUI = GameObject.FindObjectOfType<BoardUI>();
		cam = Camera.main;
		this.board = board;
		moveGenerator = new MoveGenerator();
	}

	public override void Update() {
		HandleInput();
	}

	void HandleInput() {
		Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

		if (currentState == InputState.None) {
			HandlePieceSelection(mousePos);
		} else if (currentState == InputState.DraggingPiece) {
			HandleDragMovement(mousePos);
		} else if (currentState == InputState.PieceSelected) {
			HandlePointAndClickMovement(mousePos);
		}

		if (Input.GetMouseButtonDown(1)) {
			CancelPieceSelection();
		}
	}

	void HandlePointAndClickMovement(Vector2 mousePos) {
		if (Input.GetMouseButton(0)) {
			HandlePiecePlacement(mousePos);
		}
	}

	void HandleDragMovement(Vector2 mousePos) {
		boardUI.DragPiece(selectedPieceSquare, mousePos);
		if (Input.GetMouseButtonUp(0)) {
			HandlePiecePlacement(mousePos);
		}
	}

	void HandlePiecePlacement(Vector2 mousePos) {
		if (boardUI.TryGetSquareUnderMouse(mousePos, out Coord targetSquare)) {
			// Place the piece back on its original square
			if (targetSquare.Equals(selectedPieceSquare)) {
				boardUI.ResetPiecePosition(selectedPieceSquare);
				if (currentState == InputState.DraggingPiece) {
					currentState = InputState.PieceSelected;
				} else {
					currentState = InputState.None;
					boardUI.DeselectSquare();
				}
			} else {
				int targetPiece = board.GetSquareContents(targetSquare);
				if (targetPiece != 0 && Piece.IsWhite(targetPiece) == board.WhiteMovesNext()) {
					CancelPieceSelection();
					HandlePieceSelection(mousePos);
				} else {
					TryMakeMove(selectedPieceSquare, targetSquare);
				}
			}
		} else {
			CancelPieceSelection();
		}

	}

	void HandlePieceSelection(Vector2 mousePos) {
		if (Input.GetMouseButton(0)) {
			if (boardUI.TryGetSquareUnderMouse(mousePos, out selectedPieceSquare)) {
				int piece = board.GetSquareContents(selectedPieceSquare);

				if (piece != 0 && Piece.IsWhite(piece) == board.WhiteMovesNext()) {
					boardUI.HighlightLegalMoves(moveGenerator.GenerateMoves(board), selectedPieceSquare);
					boardUI.SelectSquare(selectedPieceSquare);
					currentState = InputState.DraggingPiece;
				}
			}
		}
	}

	void CancelPieceSelection() {
		if (currentState != InputState.None) {
			currentState = InputState.None;
			boardUI.DeselectSquare();
			boardUI.ResetPiecePosition(selectedPieceSquare);
		}
	}

	void TryMakeMove(Coord startSquare, Coord targetSquare) {
		bool moveIsLegal = false;
		Move chosenMove = new Move();

		MoveGenerator moveGenerator = new MoveGenerator();
		bool wantsKnightPromotion = Input.GetKey(KeyCode.LeftAlt);

		Debug.Log($"{moveGenerator.GenerateMoves(board).Count} legal moves found");

		foreach (Move move in moveGenerator.GenerateMoves(board)) {
			if (move.GetStartSquareIndex() == startSquare.GetIndex() && move.GetTargetSquareIndex() == targetSquare.GetIndex()) {
				if (move.IsPromotion()) {
					if (move.GetFlag() == (int)Move.Flag.PromoteToQueen && wantsKnightPromotion)
						continue;
					if (move.GetFlag() != (int)Move.Flag.PromoteToQueen && !wantsKnightPromotion)
						continue;
				}
				moveIsLegal = true;
				chosenMove = move;
				break;
			}
		}

		if (moveIsLegal) {
			Debug.Log("Move made: " +
				$"{chosenMove.ToString(board.GetSquareContents(startSquare))}");
			ChoseMove(chosenMove);
			currentState = InputState.None;
		} else {
			Debug.Log("Illegal move. Could not make move: " +
				$"{new Move(startSquare, targetSquare).ToString(board.GetSquareContents(startSquare))}");
			CancelPieceSelection();
		}
	}
}
