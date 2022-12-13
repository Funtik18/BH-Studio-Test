using System.Collections.Generic;

namespace Game.Managers.SpawnManager
{
	public class SpawnManager : LazySingletonMono<SpawnManager>
	{
		public List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
	}
}