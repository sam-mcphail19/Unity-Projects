using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

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
		Test[] tests = GetSuiteTests(perftTestSuite);
		Debug.Log($"Test Suite Loaded. {tests.Length} tests loaded");
		int failedTests = 0;
		foreach (Test test in tests) {
			bool testPassed = false;
			Thread thread = new Thread(
				 () => {
					 testPassed = test.RunTest();
				 }
			);
			thread.Start();
			thread.Join();
			if (!testPassed)
				failedTests++;
		}

		Debug.Log($"Test Suite Completed. {tests.Length} tests ran. " +
					$"{failedTests} tests failed. " +
					$"{(((float)failedTests) / tests.Length * 100).ToString("F2")}% of tests failed.");
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
