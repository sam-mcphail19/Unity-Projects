
using System.Collections.Generic;
using UnityEngine;

public class Test {
	public string fen;
	public int depth;
	public int[] expectedNodeCounts;

	private Board board;

	public Test(string fen, int depth, int[] expectedNodeCounts) {
		this.fen = fen;
		this.depth = depth;
		this.expectedNodeCounts = expectedNodeCounts;
	}

	public bool RunTest() {
		board = FenUtil.LoadPositionFromFenString(fen);
		Debug.Log($"Loaded position: ${FenUtil.CurrentBoardPositionToFenString(board)}");

		for (int i = 0; i < depth; i++) {
			int nodesFound = Search(i + 1);
			if (nodesFound != expectedNodeCounts[i]) {
				Debug.Log($"At depth: {i + 1}\n" +
					$"Nodes Found: {nodesFound}, Nodes Expected: {expectedNodeCounts[i]}. " +
					$"Test Failed for position: {fen}");
				return false;
			}
		}
		return true;
	}

	int Search(int depth) {
		List<Move> moves = new MoveGenerator().GenerateMoves(board);

		if (depth == 1) {
			return moves.Count;
		}

		int numLocalNodes = 0;

		for (int i = 0; i < moves.Count; i++) {
			board.MakeMove(moves[i]);
			numLocalNodes += Search(depth - 1);
			board.UnmakeMove();
		}
		return numLocalNodes;
	}
}
