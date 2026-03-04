using System.Collections.Generic;
using UnityEngine;

namespace flanne
{
	public class WorldScroller : MonoBehaviour
	{
		private enum Quadrant
		{
			TopLeft = 0,
			TopRight = 1,
			BotLeft = 2,
			BotRight = 3
		}

		[SerializeField]
		private float tileSize = 32f;

		[SerializeField]
		private List<Transform> tiles = new List<Transform>(4);

		private Transform player;

		private Quadrant _currentQuadrant;

		private Transform _currentTile;

		private void Start()
		{
			player = PlayerController.Instance.transform;
			_currentTile = GetCurrentTile();
		}

		private void FixedUpdate()
		{
			if (!IsPlayerOnTile(_currentTile))
			{
				_currentTile = GetCurrentTile();
			}
			Quadrant quadrant = GetQuadrant(_currentTile);
			if (_currentQuadrant != quadrant)
			{
				_currentQuadrant = quadrant;
				List<Transform> list = new List<Transform>(tiles);
				list.Remove(_currentTile);
				switch (quadrant)
				{
				case Quadrant.TopLeft:
					list[0].position = new Vector3(_currentTile.position.x - tileSize, _currentTile.position.y, 0f);
					list[1].position = new Vector3(_currentTile.position.x, _currentTile.position.y + tileSize, 0f);
					list[2].position = new Vector3(_currentTile.position.x - tileSize, _currentTile.position.y + tileSize, 0f);
					break;
				case Quadrant.TopRight:
					list[0].position = new Vector3(_currentTile.position.x + tileSize, _currentTile.position.y, 0f);
					list[1].position = new Vector3(_currentTile.position.x, _currentTile.position.y + tileSize, 0f);
					list[2].position = new Vector3(_currentTile.position.x + tileSize, _currentTile.position.y + tileSize, 0f);
					break;
				case Quadrant.BotLeft:
					list[0].position = new Vector3(_currentTile.position.x - tileSize, _currentTile.position.y, 0f);
					list[1].position = new Vector3(_currentTile.position.x, _currentTile.position.y - tileSize, 0f);
					list[2].position = new Vector3(_currentTile.position.x - tileSize, _currentTile.position.y - tileSize, 0f);
					break;
				case Quadrant.BotRight:
					list[0].position = new Vector3(_currentTile.position.x + tileSize, _currentTile.position.y, 0f);
					list[1].position = new Vector3(_currentTile.position.x, _currentTile.position.y - tileSize, 0f);
					list[2].position = new Vector3(_currentTile.position.x + tileSize, _currentTile.position.y - tileSize, 0f);
					break;
				}
			}
		}

		private Quadrant GetQuadrant(Transform tile)
		{
			Vector3 position = player.position;
			Vector3 position2 = tile.position;
			if (position.x < position2.x && position.y < position2.y)
			{
				return Quadrant.BotLeft;
			}
			if (position.x < position2.x && position.y >= position2.y)
			{
				return Quadrant.TopLeft;
			}
			if (position.x >= position2.x && position.y < position2.y)
			{
				return Quadrant.BotRight;
			}
			return Quadrant.TopRight;
		}

		private bool IsPlayerOnTile(Transform tile)
		{
			Vector3 position = player.position;
			Vector3 position2 = tile.position;
			if (position2.x - tileSize / 2f < position.x && position2.x + tileSize / 2f > position.x && position2.y - tileSize / 2f < position.y)
			{
				return position2.y + tileSize / 2f > position.y;
			}
			return false;
		}

		private Transform GetCurrentTile()
		{
			for (int i = 0; i < tiles.Count; i++)
			{
				if (IsPlayerOnTile(tiles[i]))
				{
					return tiles[i];
				}
			}
			Debug.LogError("Player is not on any current tile");
			return tiles[0];
		}
	}
}
