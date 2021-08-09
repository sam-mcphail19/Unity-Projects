using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coord
{
    private const string letters = "abcdefgh";
    private const int RANK_COUNT = 8;
    private const int FILE_COUNT = 8;

    private int rank;
    private int file;
    private int index;

    public Coord(int rank, int file) {
        this.rank = rank;
        this.file = file;
        this.index = rank * FILE_COUNT + file;
    }

    public Coord(int index) {
        this.rank = (int)Math.Floor((float)index / RANK_COUNT);
        this.file = index % FILE_COUNT;
        this.index = rank * FILE_COUNT + file;
    }

    public int GetRank() {
        return rank;
    }

    public int GetFile() {
        return file;
    }

    public int GetIndex() {
        return index;
    }

    public override bool Equals(object obj) {
        Coord otherCoord = (Coord)obj;
        return this.file == otherCoord.GetFile() && this.rank == otherCoord.GetRank();
    }

    public override string ToString() {
        if (file > FILE_COUNT - 1)
            throw new System.ArgumentException("Square's file cannot be greater than the board width");

        return letters[file].ToString() + (rank + 1).ToString();
    }

    public static Coord SquareNameToSquarePos(string name) {
        const string letters = "abcdefgh";

        if (name.Length != 2)
            throw new System.ArgumentException($"{name} is not a valid square name");
        if (!letters.Contains(name[0].ToString()))
            throw new System.ArgumentException($"{name[0]} is not a valid file for a square name");
        if (!char.IsDigit(name[1]))
            throw new System.ArgumentException($"{name[1]} is not a valid rank for a square name");

        int rank = int.Parse(name[1].ToString());

        if (rank < 0 || rank > 7)
            throw new System.ArgumentException($"{name[1]} is not a valid rank for a square name");

        int file = letters.IndexOf(name[0]);

        return new Coord(rank, file);
    }
}
