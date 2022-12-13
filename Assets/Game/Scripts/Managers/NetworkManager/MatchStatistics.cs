using Game.Managers.NetworkManager;
using Game.UI;
using Mirror;

using UnityEngine;

public class MatchStatistics : NetworkBehaviour
{
	public static MatchStatistics Instance
	{
		get
		{
			if (instance == null)
			{
				instance = GameObject.FindObjectOfType<MatchStatistics>(true);

				DontDestroyOnLoad(instance);
			}

			return instance;
		}
	}
	private static MatchStatistics instance;

	public override void OnStartServer()
	{
		NetworkServer.RegisterHandler<MatchMessage>(OnServerMessage);
	}

	public void SetCount(NetworkIdentity identity, int count)
	{
		if (count >= 3)
		{
			GameCanvas.Instance.SetCounterText($"WIN Player#{identity.connectionToClient.connectionId}");

			NetworkClient.connection.Send(new MatchMessage { serverOperationType = ServerOperationType.RestartGame });
		}
		else
		{
			GameCanvas.Instance.SetCounterText(count.ToString());
		}
	}

	private void OnServerMessage(NetworkConnectionToClient connection, MatchMessage message)
	{
		if(message.serverOperationType == ServerOperationType.RestartGame)
		{
			CustomNetworkManager.Instance.RestartGame();
		}
	}
}