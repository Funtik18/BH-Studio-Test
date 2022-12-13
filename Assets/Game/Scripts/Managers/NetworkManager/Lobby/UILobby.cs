using UnityEngine;
using UnityEngine.UI;

namespace Game.Managers.NetworkManager
{
	public class UILobby : LazySingletonMono<UILobby>
	{
		[field: SerializeField] public Button Join { get; private set; }
		[field: SerializeField] public Button Host { get; private set; }

		[field: SerializeField] public TMPro.TMP_InputField JoinIp { get; private set; }
		[field: SerializeField] public Button Connect { get; private set; }

		[field: SerializeField] public UIRoom Room { get; private set; }

		private void Start()
		{
			Room.gameObject.SetActive(false);
			Room.StartGame.gameObject.SetActive(false);

			JoinIp.text = "localhost";

			JoinIp.gameObject.SetActive(false);
			Connect.gameObject.SetActive(false);
		}

		private void OnDestroy()
		{
			Join.onClick.RemoveAllListeners();
			Host.onClick.RemoveAllListeners();
			Connect.onClick.RemoveAllListeners();

			Room.Cancel.onClick.RemoveAllListeners();
			Room.Ready.onClick.RemoveAllListeners();
			Room.StartGame.onClick.RemoveAllListeners();
		}
	}
}