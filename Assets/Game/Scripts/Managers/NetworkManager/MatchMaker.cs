using Mirror;
using Mirror.Examples.MultipleMatch;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Managers.NetworkManager
{
	public partial class MatchMaker : LazySingletonMono<MatchMaker>
	{
		private CustomNetworkManager networkManager;

		private UILobby instanceLobby;
		private Guid selectedMatch = Guid.Empty;
		private bool isCreated = false;

		private void Start()
		{
			networkManager = CustomNetworkManager.Instance;

			instanceLobby = UILobby.Instance;

			instanceLobby.Join.onClick.AddListener(OnJoinClicked);
			instanceLobby.Host.onClick.AddListener(OnHostClicked);
			instanceLobby.Connect.onClick.AddListener(OnConnectClicked);

			instanceLobby.Room.Cancel.onClick.AddListener(OnCancelClick);
			instanceLobby.Room.Ready.onClick.AddListener(OnReadyClick);
			instanceLobby.Room.StartGame.onClick.AddListener(OnStartClick);
		}

		public void OnServerReady(NetworkConnectionToClient conn)
		{
			isCreated = true;
			playerDescriptions.Add(conn, new PlayerDescription { isReady = false });
			NetworkClient.connection.Send(new ServerMessage { serverOperationType = ServerOperationType.CreateRoom });
		}

		public void OnClientConnect()
		{
			if (!isCreated)
			{
				isCreated = true;
				playerDescriptions.Add(NetworkClient.connection, new PlayerDescription { isReady = false });
				NetworkClient.connection.Send(new ServerMessage { serverOperationType = ServerOperationType.JoinRoom, matchId = selectedMatch });
			}
		}

		public void OnServerDisconect(NetworkConnectionToClient conn)
		{
			Guid matchId = matchConnections.First().Key;

			foreach (NetworkConnectionToClient playerConn in matchConnections[matchId])
			{
				var description = playerDescriptions[playerConn];
				description.isLeader = false;
				description.isReady = false;
				description.matchId = Guid.Empty;
				playerDescriptions[playerConn] = description;

				playerConn.Send(new ClientMatchMessage { clientMatchOperation = ClientMatchOperation.Departed });
			}

			foreach (var kvp in matchConnections)
			{
				kvp.Value.Remove(conn);
			}
		}


		private void OnJoinClicked()
		{
			instanceLobby.Host.gameObject.SetActive(false);
			instanceLobby.Join.gameObject.SetActive(false);

			instanceLobby.JoinIp.gameObject.SetActive(true);
			instanceLobby.Connect.gameObject.SetActive(true);
		}

		private void OnHostClicked()
		{
			//Host.enabled = false;
			networkManager.StartHost();

			NetworkServer.RegisterHandler<ServerMessage>(OnServerMessage);
			NetworkClient.RegisterHandler<ClientMessage>(OnClientMessage);
		}

		private void OnConnectClicked()
		{
			if (instanceLobby.JoinIp.text.IsEmpty()) return;

			networkManager.networkAddress = instanceLobby.JoinIp.text;
			networkManager.StartClient();

			NetworkClient.RegisterHandler<ClientMessage>(OnClientMessage);
		}

		private void OnCancelClick()
		{
			NetworkClient.connection.Send(new ServerMessage { serverOperationType = ServerOperationType.LeaveRoom });
		}

		private void OnReadyClick()
		{
			NetworkClient.connection.Send(new ServerMessage { serverOperationType = ServerOperationType.Ready });
		}

		private void OnStartClick()
		{
			NetworkClient.connection.Send(new ServerMessage { serverOperationType = ServerOperationType.StartGame });
		}
	}

	public partial class MatchMaker
	{
		public Dictionary<Guid, HashSet<NetworkConnectionToClient>> matchConnections = new Dictionary<Guid, HashSet<NetworkConnectionToClient>>();
		public Dictionary<Guid, MatchDescription> openMatches = new Dictionary<Guid, MatchDescription>();
		public Dictionary<NetworkConnection, PlayerDescription> playerDescriptions = new Dictionary<NetworkConnection, PlayerDescription>();

		private void OnServerMessage(NetworkConnectionToClient connection, ServerMessage message)
		{
			switch (message.serverOperationType)
			{
				case ServerOperationType.CreateRoom:
				{
					//Add new match
					Guid newMatchId = Guid.NewGuid();
					matchConnections.Add(newMatchId, new HashSet<NetworkConnectionToClient>());
					matchConnections[newMatchId].Add(connection);

					//Update owner
					var description = playerDescriptions[connection];
					description.isLeader = true;
					description.isReady = false;
					description.playerName = $"Leader";
					description.matchId = newMatchId;
					playerDescriptions[connection] = description;

					//Send Client
					var infos = GetDescriptions(newMatchId);
					connection.Send(new ClientMessage { clientOperationType = ClientOperationType.RoomCreated, matchId = newMatchId, playerDescriptions = infos });
					break;
				}
				case ServerOperationType.JoinRoom:
				{
					Guid matchId = matchConnections.First().Key;//cached message.matchId

					if (matchConnections[matchId].Contains(connection)) return;
					matchConnections[matchId].Add(connection);

					//Update Player
					var description = playerDescriptions[connection];
					description.isLeader = false;
					description.isReady = false;
					description.playerName = $"Player";
					description.matchId = matchId;
					playerDescriptions[connection] = description;


					//Send
					connection.Send(new ClientMessage { clientOperationType = ClientOperationType.JoinedToRoom, matchId = matchId, playerDescriptions = GetDescriptions(matchId) });
					SendRefreshRoom(matchId);
					break;
				}
				case ServerOperationType.LeaveRoom:
				{
					Guid matchId = matchConnections.First().Key;

					var description = playerDescriptions[connection];
					description.isLeader = false;
					description.isReady = false;
					description.matchId = Guid.Empty;
					playerDescriptions[connection] = description;


					foreach (var kvp in matchConnections)
					{
						kvp.Value.Remove(connection);
					}
					SendRefreshRoom(matchId);
					connection.Send(new ClientMessage { clientOperationType = ClientOperationType.Departed });
					break;
				}


				case ServerOperationType.Ready:
				{
					Guid matchId = matchConnections.First().Key;

					var description = playerDescriptions[connection];
					description.isReady = !description.isReady;
					playerDescriptions[connection] = description;

					bool isAllReady = playerDescriptions.All((x) => x.Value.isReady);
					bool isLeader = description.isLeader;

					instanceLobby.Room.StartGame.gameObject.SetActive(isLeader && isAllReady);

					SendRefreshRoom(matchId);
					break;
				}

				case ServerOperationType.StartGame:
				{
					Guid matchId = matchConnections.First().Key;

					networkManager.ServerChangeScene(networkManager.gameScene);

					var infos = GetDescriptions(matchId);
					foreach (NetworkConnectionToClient playerConn in matchConnections[matchId])
					{
						playerConn.Send(new ClientMessage { clientOperationType = ClientOperationType.StartGame, playerDescriptions = infos });
					}
					break;
				}
			}

			PlayerDescription[] GetDescriptions(Guid matchId)
			{
				return matchConnections[matchId].Select((connection) => playerDescriptions[connection]).ToArray();
			}

			void SendRefreshRoom(Guid matchId)
			{
				var infos = GetDescriptions(matchId);
				foreach (NetworkConnectionToClient playerConn in matchConnections[matchId])
				{
					playerConn.Send(new ClientMessage { clientOperationType = ClientOperationType.RefreshRoom, playerDescriptions = infos });
				}
			}
		}

		private void OnClientMessage(ClientMessage message)
		{
			switch (message.clientOperationType)
			{
				case ClientOperationType.RoomCreated:
				{
					instanceLobby.Room.ResizeContent(message.playerDescriptions);
					instanceLobby.Room.gameObject.SetActive(true);

					break;
				}
				case ClientOperationType.JoinedToRoom:
				{
					instanceLobby.Room.ResizeContent(message.playerDescriptions);
					instanceLobby.Room.gameObject.SetActive(true);
					break;
				}

				case ClientOperationType.Canceled:
				case ClientOperationType.Departed:
				{
					networkManager.StopClient();
					instanceLobby.Room.ResizeContent(message.playerDescriptions);
					break;
				}

				case ClientOperationType.RefreshRoom:
				{
					instanceLobby.Room.ResizeContent(message.playerDescriptions);

					//bool isAllReady = playerDescriptions.All((x) => x.Value.isReady);
					//bool isLeader = networkConnection != null ? playerDescriptions[networkConnection].isLeader : false;
					//instanceLobby.Room.StartGame.gameObject.SetActive(isLeader && isAllReady);
					break;
				}

				case ClientOperationType.StartGame:
				{
					instanceLobby.gameObject.SetActive(false);
					break;
				}
			}
		}
	}
}