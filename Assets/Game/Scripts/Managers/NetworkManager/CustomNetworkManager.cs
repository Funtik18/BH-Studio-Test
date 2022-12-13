using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;
using System;
using System.Linq;
using System.Collections;
using Game.Entities;

/*
	Documentation: https://mirror-networking.gitbook.io/docs/components/network-manager
	API Reference: https://mirror-networking.com/docs/api/Mirror.NetworkManager.html
*/

namespace Game.Managers.NetworkManager
{
    public class CustomNetworkManager : Mirror.NetworkManager
    {
		public static CustomNetworkManager Instance
		{
			get
			{
				if (instance == null)
				{
					instance = GameObject.FindObjectOfType<CustomNetworkManager>(true);
				}

				return instance;
			}
		}
		private static CustomNetworkManager instance;

		[Header("Custom")]
		[SerializeField] private MatchMaker matchMaker;

		[Scene]
		public string gameScene;
		private bool sceneLoaded = false;

		public override void ServerChangeScene(string newSceneName)
		{
			var first = matchMaker.matchConnections.First();

			if (SceneManager.sceneCount > 1)
			{
				StartCoroutine(ServerReloadGameScene());
			}
			else
			{
				foreach (var item in first.Value)
				{
					StartCoroutine(OnServerAddPlayerDelayed(item));
				}

				StartCoroutine(ServerLoadGameScene());
			}
		}

		public void RestartGame()
		{
			ServerChangeScene(gameScene);
		}

		public override void OnStartClient()
		{
			Debug.Log("Starting client...");
		}
		public override void OnClientConnect()
		{
			Debug.Log("Client connected.");
			base.OnClientConnect();
			matchMaker.OnClientConnect();
		}
		public override void OnClientDisconnect()
		{
			Debug.Log("Client disconnected.");
		}

		public override void OnServerReady(NetworkConnectionToClient conn)
		{
			base.OnServerReady(conn);
			matchMaker.OnServerReady(conn);
		}
		public override void OnServerConnect(NetworkConnectionToClient conn)
		{
			Debug.Log("Connecting to server...");
			if (numPlayers >= maxConnections)
			{
				Debug.Log("Too many players. Disconnecting user.");
				conn.Disconnect();
				return;
			}
			if (SceneManager.GetActiveScene().name != "TitleScreen") 
			{
				Debug.Log("Player did not load from correct scene. Disconnecting user. Player loaded from scene: " + SceneManager.GetActiveScene().name);
				conn.Disconnect();
				return;
			}

			Debug.Log("Server Connected");
		}
		public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            Debug.LogError("Disconect Server " + conn.connectionId);
			matchMaker.OnServerDisconect(conn);
			base.OnServerDisconnect(conn);
		}

		private IEnumerator OnServerAddPlayerDelayed(NetworkConnectionToClient conn)
		{
			yield return new WaitWhile(() => !sceneLoaded);

			conn.Send(new SceneMessage { sceneName = gameScene, sceneOperation = SceneOperation.LoadAdditive });

			yield return new WaitForEndOfFrame();

			Transform startPos = GetStartPosition();
			GameObject player = startPos != null
				? Instantiate(playerPrefab, startPos.position, startPos.rotation)
				: Instantiate(playerPrefab);

			player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
			NetworkServer.AddPlayerForConnection(conn, player);

			SceneManager.MoveGameObjectToScene(conn.identity.gameObject, SceneManager.GetSceneByName(gameScene));
		}

		public override Transform GetStartPosition()
		{
			var list = SpawnManager.SpawnManager.Instance.spawnPoints;

			if (list.Count == 0) return null;
			var point = list[UnityEngine.Random.Range(0, list.Count)];

			SpawnManager.SpawnManager.Instance.spawnPoints.ForEach((x) =>
			{
				x.Enable(false);
			});

			return point.transform;
		}

		private IEnumerator ServerLoadGameScene()
		{
			sceneLoaded = false;
			yield return SceneManager.LoadSceneAsync(gameScene, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive });
			sceneLoaded = true;
		}

		private IEnumerator ServerReloadGameScene()
		{
			sceneLoaded = false;

			//Frezze
			foreach (var item in NetworkServer.connections)
			{
				var character = item.Value.identity.gameObject.GetComponent<Character>();
				character.Frezze(true, true);
			}
			Debug.LogError("Wait 5Sec");
			yield return new WaitForSeconds(5f);
			Debug.LogError("Wait 5Sec End");
			Debug.LogError("Reload scene");

			//UnLoad
			Scene scene = SceneManager.GetActiveScene();
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				if(SceneManager.GetSceneAt(i).name == "Game")
				{
					scene = SceneManager.GetSceneAt(i);
				}
			}
			foreach (var item in NetworkServer.connections)
			{
				item.Value.Send(new SceneMessage { sceneName = scene.name, sceneOperation = SceneOperation.UnloadAdditive });
			}
			yield return SceneManager.UnloadSceneAsync(scene);

			//Load
			foreach (var item in NetworkServer.connections)
			{
				item.Value.Send(new SceneMessage { sceneName = gameScene, sceneOperation = SceneOperation.LoadAdditive });
			}
			yield return SceneManager.LoadSceneAsync(gameScene, new LoadSceneParameters { loadSceneMode = LoadSceneMode.Additive });

			//Refresh
			foreach (var item in NetworkServer.connections)
			{
				var character = item.Value.identity.gameObject.GetComponent<Character>();
				character.transform.position = GetStartPosition().position;
				character.Refresh();
			}
			
			sceneLoaded = true;
		}
	}


	[System.Serializable]
	public struct PlayerDescription
	{
		public string playerName;
		public bool isLeader;
		public bool isReady;
		public Guid matchId;
	}

	[System.Serializable]
	public struct MatchDescription
	{

	}

	public struct ServerMessage : NetworkMessage
	{
		public ServerOperationType serverOperationType;
		public Guid matchId;
	}

	public struct ClientMessage : NetworkMessage
	{
		public ClientOperationType clientOperationType;
		public Guid matchId;
		public PlayerDescription[] playerDescriptions;
	}

	public struct MatchMessage : NetworkMessage
	{
		public ServerOperationType serverOperationType;
	}

	public enum ServerOperationType
	{
		CreateRoom,
		JoinRoom,
		LeaveRoom,
		Cancel,

		Ready,
		StartGame,
		RestartGame,
	}

	public enum ClientOperationType
	{
		RoomCreated,
		JoinedToRoom,
		
		Canceled,
		Departed,

		RefreshRoom,

		StartGame,
	}
}