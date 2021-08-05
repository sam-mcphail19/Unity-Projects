using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * https://www.chessprogramming.org/Perft
 * https://www.chessprogramming.org/Perft_Results
 */

public class Perft : MonoBehaviour {

	[Header("Suite Test")]
	public TextAsset perftTestSuite;

	MoveGenerator moveGenerator;
	Board board;

	void Start() {
		board = new Board();
		moveGenerator = new MoveGenerator();
	}

	// Update is called once per frame
	void Update() {

	}

	public void RunTests() {
		Test[] tests = GetSuiteTests(perftTestSuite);
		Debug.Log($"Test Suite Loaded. {tests.Length} tests loaded");
		foreach (Test test in tests)
			RunTest(test);
	}

	public void RunTest(Test test) {
		board = FenUtil.LoadPositionFromFenString(test.fen);

		for (int i = 1; i < test.depth; i++) {
			int nodesFound = Search(i);
			if(nodesFound == test.expectedNodeCounts[i])
				Debug.Log($"At depth: {i}\n" +
					$"Nodes Found: {nodesFound}, Nodes Expected: {test.expectedNodeCounts[i]}\n" +
					$"Test Succeeded for depth {i}");
			else
				Debug.Log($"At depth: {i}\n" +
					$"Nodes Found: {nodesFound}, Nodes Expected: {test.expectedNodeCounts[i]}\n" +
					$"Test Failed for depth {i}");
		}
	}

	int Search(int depth) {
		var moves = moveGenerator.GenerateMoves(board);

		if (depth == 1) {
			return moves.Count;
		}

		int numLocalNodes = 0;

		for (int i = 0; i < moves.Count; i++) {
			board.MakeMove(moves[i]);
			int numNodesFromThisPosition = Search(depth - 1);
			numLocalNodes += numNodesFromThisPosition;
			board.UnmakeMove(moves[i]);
		}
		return numLocalNodes;
	}

	public Test[] GetSuiteTests(TextAsset suiteFile) {
		var testList = new List<Test>();

		string suiteText = suiteFile.text.Split('{')[1].Split('}')[0];
		string[] testStrings = suiteText.Split('\n');

		for (int i = 0; i < testStrings.Length; i++) {
			string testString = testStrings[i].Trim();
			string[] sections = testString.Split('_');
			if (sections.Length == 3) {
				int depth = int.Parse(sections[0]);
				string[] nodeCountSection = sections[1].Trim('[').Trim(']').Split(',');
				int[] expectedNodeCounts = new int[depth];
				for (int j = 0; j < depth; j++)
					expectedNodeCounts[j] = int.Parse(nodeCountSection[j]);

				var test = new Test() {
					depth = depth,
					expectedNodeCounts = expectedNodeCounts,
					fen = sections[2]
				};
				testList.Add(test);
			}
		}
		return testList.ToArray();
	}

	[System.Serializable]
	public struct Test {
		public string fen;
		public int depth;
		public int[] expectedNodeCounts;
	}
}
