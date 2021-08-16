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

	public void RunSuite() {
		RunTests();
	}

	public void RunTests() {
		Test[] tests = GetSuiteTests(perftTestSuite);
		Debug.Log($"Test Suite Loaded. {tests.Length} tests loaded");
		int failedTests = 0;
		foreach (Test test in tests)
			if (!RunTest(test))
				failedTests++;

		Debug.Log($"Test Suite Completed. {tests.Length} tests ran. " +
			$"{failedTests} tests failed. " +
			$"{(((float)failedTests)/tests.Length * 100).ToString("F2")}% of tests failed.");
	}

	public bool RunTest(Test test) {
		board = FenUtil.LoadPositionFromFenString(test.fen);

		for (int i = 0; i < test.depth; i++) {
			int nodesFound = Search(i+1);
			if (nodesFound != test.expectedNodeCounts[i]) {
				Debug.Log($"At depth: {i+1}\n" +
					$"Nodes Found: {nodesFound}, Nodes Expected: {test.expectedNodeCounts[i]}. " +
					$"Test Failed for position: {FenUtil.CurrentBoardPositionToFenString(board)}");
				return false;
			}
		}

		return true;
	}

	int Search(int depth) {
		var moves = moveGenerator.GenerateMoves(board);

		if (depth == 1) {
			return moves.Count;
		}

		int numLocalNodes = 0;

		for (int i = 0; i < moves.Count; i++) {
			board.MakeMove(moves[i]);
			numLocalNodes += Search(depth - 1);
			board.UnmakeMove(moves[i]);
		}
		return numLocalNodes;
	}

	public Test[] GetSuiteTests(TextAsset suiteFile) {
		var testList = new List<Test>();

		string[] testStrings = suiteFile.text.Split('\n');

		for (int i = 0; i < testStrings.Length - 1; i++) {
			string[] sections = testStrings[i].Split(',');

			// Only go to depth 1 for the full suite
			// Allows us to see exactly what positions we are failing for
			int depth = suiteFile.name.Contains("TestSuiteFull") ? 1 : sections.Length - 1;
			int[] expectedNodeCounts = new int[depth];

			for (int j = 0; j < depth; j++) {
				expectedNodeCounts[j] = int.Parse(sections[j + 1]);
			}

			Test test = new Test() {
				depth = depth,
				expectedNodeCounts = expectedNodeCounts,
				fen = sections[0]
			};
			testList.Add(test);
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
