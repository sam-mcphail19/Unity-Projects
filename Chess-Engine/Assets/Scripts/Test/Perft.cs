using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.Linq;
using System.IO;

/*
 * https://www.chessprogramming.org/Perft
 * https://www.chessprogramming.org/Perft_Results
 */

public class Perft : MonoBehaviour {

	[Header("Suite Test")]
	public TextAsset perftTestSuite;

	void Start() { }

	void Update() { }

	public void RunSuite() {
		StartCoroutine(RunTests());
	}

	public IEnumerator RunTests() {
		Test[] tests = GetSuiteTests(perftTestSuite);
		int[] testResults = new int[tests.Length];
		for (int i = 0; i < testResults.Length; i++)
			testResults[i] = -1;

		Debug.Log($"Test Suite Loaded. {tests.Length} tests loaded");
		for (int i = 0; i < tests.Length; i++) {
			yield return new WaitForEndOfFrame();
			RunTest(testResults, i, tests[i]);
		}

		float failedTests = testResults.Count(n => n == 0);

		Debug.Log($"Test Suite Completed. {tests.Length} tests ran. " +
					$"{(int)failedTests} tests failed. " +
					$"{(failedTests / tests.Length * 100).ToString("F2")}% of tests failed.");
	}

	/*
	// With these tests:
	// rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1,20,400,8902
	// r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq -,48,2039
	// 8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - -,14,191,2812,43238,674624
	// r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1,6,264,9467
	// rnbq1k1r/pp1Pbppp/2p5/8/2B5/8/PPP1NnPP/RNBQK2R w KQ - 1 8,44,1486,62379
	// r4rk1/1pp1qppp/p1np1n2/2b1p1B1/2B1P1b1/P1NP1N2/1PP1QPPP/R4RK1 w - - 0 10,46,2079
	// Test completion took:
	// With threading: 1:31
	// Without threading: 1:21
	// With coroutines: 1:18
	public void RunSuite() {
		Test[] tests = GetSuiteTests(perftTestSuite);
		int[] testResults = new int[tests.Length];
		for (int i = 0; i < testResults.Length; i++)
			testResults[i] = -1;

		Debug.Log($"Test Suite Loaded. {tests.Length} tests loaded");
		for (int i = 0; i < tests.Length; i++) {
			int index = i;
			RunTest(testResults, index, tests[index]);
		}

		float failedTests = testResults.Count(n => n == 0);

		Debug.Log($"Test Suite Completed. {tests.Length} tests ran. " +
					$"{(int)failedTests} tests failed. " +
					$"{(failedTests / tests.Length * 100).ToString("F2")}% of tests failed.");
	}

	
	public void RunSuite() {
		Test[] tests = GetSuiteTests(perftTestSuite);
		// -1 for undefined, 0 for failed, 1 for passed
		int[] testResults = new int[tests.Length];
		Thread[] threads = new Thread[tests.Length];
		for (int i = 0; i < testResults.Length; i++)
			testResults[i] = -1;

		Debug.Log($"Test Suite Loaded. {tests.Length} tests loaded");
		for (int i = 0; i < tests.Length; i++) {
			int index = i;
			threads[i] = new Thread(() => RunTest(testResults, index, tests[index]));
		}

		foreach (Thread thread in threads)
			thread.Start();

		foreach (Thread thread in threads)
			thread.Join();

		float failedTests = testResults.Count(n => n == 0);

		Debug.Log($"Test Suite Completed. {tests.Length} tests ran. " +
					$"{(int)failedTests} tests failed. " +
					$"{(failedTests / tests.Length * 100).ToString("F2")}% of tests failed.");
	}*/

	public void RunTest(int[] testResults, int index, Test test) {
		Thread.CurrentThread.IsBackground = true;
		testResults[index] = test.RunTest() ? 1 : 0;
		Debug.Log($"Test {index} for position: {test.fen} completed. Waiting for other tests to complete");
	}

	public Test[] GetSuiteTests(TextAsset suiteFile) {
		var testList = new List<Test>();

		string[] testStrings = suiteFile.text.Split('\n');

		for (int i = 0; i < testStrings.Length; i++) {
			string[] sections = testStrings[i].Split(',');

			// Only go to depth 1 for the full suite
			// Allows us to see exactly what positions we are failing for
			int depth = suiteFile.name.Contains("TestSuiteFull") ? 1 : sections.Length - 1;
			int[] expectedNodeCounts = new int[depth];

			for (int j = 0; j < depth; j++) {
				expectedNodeCounts[j] = int.Parse(sections[j + 1]);
			}

			testList.Add(new Test(sections[0], depth, expectedNodeCounts));
		}
		return testList.ToArray();
	}
}
