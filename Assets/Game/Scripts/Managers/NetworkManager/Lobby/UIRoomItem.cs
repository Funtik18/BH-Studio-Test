using System;

using UnityEngine;
using UnityEngine.UI;

namespace Game.Managers.NetworkManager
{
	public class UIRoomItem : MonoBehaviour
	{
		[field: SerializeField] public Image Star { get; private set; }
		[field: SerializeField] public TMPro.TextMeshProUGUI Name { get; private set; }
		[field: SerializeField] public TMPro.TextMeshProUGUI Ready { get; private set; }

		private Guid matchId;

		public void SetMatch()
		{

		}

		public void SetDescription(PlayerDescription description)
		{
			Name.text = description.playerName;
			Star.enabled = description.isLeader;
			SetReadyState(description.isReady);
		}

		public void SetReadyState(bool trigger)
		{
			if (trigger)
			{
				Ready.text = "Ready";
				Ready.color = Color.green;
			}
			else
			{
				Ready.text = "Not Ready";
				Ready.color = Color.red;
			}
		}
	}
}