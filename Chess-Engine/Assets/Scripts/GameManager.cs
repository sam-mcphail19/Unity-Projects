using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public event System.Action<Move> onMoveMade;

	Player whitePlayer;
	Player blackPlayer;
	Player playerToMove;

	Board board;
	BoardUI boardUi;

	// Start is called before the first frame update
	void Start() {
		boardUi = FindObjectOfType<BoardUI>();
		NewGame();
	}

	// Update is called once per frame
	void Update() {
		playerToMove.Update();
		if (board.unmadeMove) {
			boardUi.UpdatePosition(board);
			board.unmadeMove = false;
		}
	}

	void NewGame() {
		board = FenUtil.LoadInitialPosition();
		CreatePlayer(ref whitePlayer, PlayerType.Human);
		CreatePlayer(ref blackPlayer, PlayerType.Human);
		playerToMove = board.WhiteMovesNext() ? whitePlayer : blackPlayer;

		boardUi.UpdatePosition(board);
	}

	void OnMoveChosen(Move move) {
		board.MakeMove(move);
		onMoveMade?.Invoke(move);
		boardUi.OnMoveMade(board, move);
		playerToMove = board.WhiteMovesNext() ? whitePlayer : blackPlayer;
	}

	void CreatePlayer(ref Player player, PlayerType playerType) {
		if (player != null) {
			player.onMoveChosen -= OnMoveChosen;
		}

		if (playerType == PlayerType.Human) {
			player = new HumanPlayer(board);
		}
		player.onMoveChosen += OnMoveChosen;
	}

	public enum PlayerType {
		Human,
		AI
	}
}
