using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

namespace Assets.Scripts
{
	[ExecuteInEditMode()]
	public class AutoGenerateSubstrataBorders : MonoBehaviour
	{
		[SerializeField] private Tilemap tilemap;
		[SerializeField] private TileBase[] edge_tilebases;
		public int border = 15;
		private int prev_border = -1;
		public bool resetBorder = false;

		// Use this for initialization
		void Start()
		{
			ClearBorders();
		}

		// Update is called once per frame
		void Update()
		{
			if (prev_border != border || resetBorder)
			{
				ClearBorders();
				FillBorders();
				prev_border = border;
				resetBorder = false;
			}
		}

		void ClearBorders()
		{
			foreach (Vector3Int pos in tilemap.cellBounds.allPositionsWithin)
			{
				if (edge_tilebases.Contains(tilemap.GetTile(pos))) 
				{
					tilemap.SetTile(pos, null);
				}
			}
		}

		void FillBorders()
		{
			// assigning the borders of the level
			for (int i = border + 1; i <= border + 5; i++)
			{
				for (int j = -border - 5; j <= border + 5; j++)
				{
					int temp;

					temp = UnityEngine.Random.Range(0, edge_tilebases.Length + 1);
					if (temp < edge_tilebases.Length)
						tilemap.SetTile(new Vector3Int(j, i, 0), edge_tilebases[temp]);

					temp = UnityEngine.Random.Range(0, edge_tilebases.Length + 1);
					if (temp < edge_tilebases.Length)
						tilemap.SetTile(new Vector3Int(j, -i, 0), edge_tilebases[temp]);

					temp = UnityEngine.Random.Range(0, edge_tilebases.Length + 1);
					if (temp < edge_tilebases.Length)
						tilemap.SetTile(new Vector3Int(i, j, 0), edge_tilebases[temp]);

					temp = UnityEngine.Random.Range(0, edge_tilebases.Length + 1);
					if (temp < edge_tilebases.Length)
						tilemap.SetTile(new Vector3Int(-i, j, 0), edge_tilebases[temp]);
				}
			}
		}
	}
}