using Game.Entities;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
	public class GameCanvas : LazySingletonMono<GameCanvas>
	{
		[field: SerializeField] public Transform Content { get; private set; }
		[field: SerializeField] public TMPro.TextMeshProUGUI Counter { get; private set; }

		[SerializeField] private GameObject prefab;

		private void Start()
		{
			Content.DestroyChildren();
		}

		public void SetCounterText(string text)
		{
			Counter.text = text;
		}

		public void UpdateCharacterList(List<Character> characters)
		{
			Content.DestroyChildren();

			for (int i = 0; i < characters.Count; i++)
			{
				var go = Instantiate(prefab);
				go.transform.SetParent(Content);
			}
		}
	}
}