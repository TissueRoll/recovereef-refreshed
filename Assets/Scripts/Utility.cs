using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class Utility
{
	private Vector3Int[,] hexNeighbors = new Vector3Int[,] {
		{new Vector3Int(1,0,0), new Vector3Int(0,-1,0), new Vector3Int(-1,-1,0), new Vector3Int(-1,0,0), new Vector3Int(-1,1,0), new Vector3Int(0,1,0)},
		{new Vector3Int(1,0,0), new Vector3Int(1,-1,0), new Vector3Int(0,-1,0), new Vector3Int(-1,0,0), new Vector3Int(0,1,0), new Vector3Int(1,1,0)}
	};
	public readonly Color progressNone = new Color(43f / 255f, 90f / 255f, 147f / 255f, 1f);
	public readonly Color progressZero = new Color(247f / 255f, 104f / 255f, 9f / 255f, 1f); // new Color(69f/255f,129f/255f,201f/255f,1f);
	public readonly Color progressIsDone = new Color(249f / 255f, 237f / 255f, 6f / 255f, 1f); // new Color(0f,1f,17f/255f,1f); // new Color(0f,1f,176f/255f,1f);
	public readonly Color progressDefinitelyDone = new Color(0f, 1f, 17f / 255f, 1f); // new Color(1f, 200f/255f, 102f/255f, 1f);
	public readonly Color gold = new Color(255f / 255f, 198f / 255f, 39f / 255f, 1f);
	public readonly Color green = new Color(73f / 255f, 196f / 255f, 114f / 255f, 1f);
	public readonly Color red = new Color(255f / 255f, 69f / 255f, 69f / 255f, 1f);

	public Utility()
	{

	}

	public string ConvertTimetoMS(float rawTime)
	{
		int minutes = Mathf.FloorToInt(rawTime / 60f);
		int seconds = Mathf.FloorToInt(rawTime - minutes * 60);
		return string.Format("{0:00}:{1:00}", minutes, seconds);
	}
	public HashSet<Vector3Int> Spread(Vector3Int position, int level)
	{
		HashSet<Vector3Int> result = new HashSet<Vector3Int>
		{
			position
		};
		if (level > 0)
		{
			for (int i = 0; i < 6; i++)
			{
				Vector3Int posNeighbor = position + hexNeighbors[position.y & 1, i];
				result.UnionWith(Spread(posNeighbor, level - 1));
			}
		}
		return result;
	}
	public bool WithinBoardBounds(Vector3Int position, int bound)
	{
		bool ok = (Math.Abs(position.x) <= bound && Math.Abs(position.y) <= bound);
		return ok;
	}
}
