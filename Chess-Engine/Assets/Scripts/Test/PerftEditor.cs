using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Perft))]
public class PerftEditor : Editor {

	Perft perft;

	public override void OnInspectorGUI() {
		DrawDefaultInspector();
		GUILayout.Space(10);
		if (GUILayout.Button("Run Suite")) {
			perft.RunSuite();
		}
	}

	void OnEnable() {
		perft = (Perft)target;
	}
}
