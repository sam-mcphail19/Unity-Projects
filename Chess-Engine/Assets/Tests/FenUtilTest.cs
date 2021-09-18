using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class FenUtilTest {

	readonly int[,] STARTING_POS_BOARD = new int[8, 8] {
		{6, 2, 5, 7, 3, 5, 2, 6},
		{1, 1, 1, 1, 1, 1, 1, 1},
		{0, 0, 0, 0, 0, 0, 0, 0},
		{0, 0, 0, 0, 0, 0, 0, 0},
		{0, 0, 0, 0, 0, 0, 0, 0},
		{0, 0, 0, 0, 0, 0, 0, 0},
		{9, 9, 9, 9, 9, 9, 9, 9},
		{14, 10, 13, 15, 11, 13, 10, 14}
	};

	[Test]
	public void LoadInitialPos_LoadsInitialPos() {
		Assert.AreEqual(FenUtil.LoadInitialPosition().representation, STARTING_POS_BOARD);
	}
}
