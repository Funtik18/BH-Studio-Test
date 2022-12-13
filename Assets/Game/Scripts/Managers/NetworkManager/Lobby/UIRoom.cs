using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Managers.NetworkManager
{
	public class UIRoom : MonoBehaviour
	{
		[field: SerializeField] public Transform Content { get; private set; }

		[field: SerializeField] public Button Cancel { get; private set; }
		[field: SerializeField] public Button Ready { get; private set; }
		[field: SerializeField] public Button StartGame { get; private set; }

		[SerializeField] private UIRoomItem itemPrefab;

		private List<UIRoomItem> items = new List<UIRoomItem>();
		private bool isInitialized = false;

		public void ResizeContent(PlayerDescription[] descriptions)
		{
			if (!isInitialized)
			{
				Content.DestroyChildren();
				isInitialized = true;
			}

			CollectionExtensions.Resize(descriptions, items,
			() =>
			{
				var item = Instantiate(itemPrefab);
				item.transform.SetParent(Content);
				item.transform.localScale = Vector3.one;

				return item;
			},
			() =>
			{
				return items.Last();
			});

			for (int i = 0; i < descriptions.Length; i++)
			{
				items[i].SetDescription(descriptions[i]);
			}
		}
	}
}