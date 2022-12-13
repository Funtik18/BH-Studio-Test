using UnityEngine;

namespace Game.Managers.SpawnManager
{
	[ExecuteInEditMode]
	public class SpawnPoint : MonoBehaviour
	{
		[SerializeField] private Mesh mesh;
		[SerializeField] private Material material;

		public bool IsEnable { get; private set; }

		private void Start()
		{
			IsEnable = false;
		}

		private void Update()
		{
			if (Application.isPlaying)
			{
				if (!IsEnable) return;
			}

			DrawSilhouette();
		}

		public void Enable(bool trigger)
		{
			IsEnable = trigger;
		}


		private void DrawSilhouette()
		{
			if (mesh == null || material == null) return;

			Graphics.DrawMesh(mesh, transform.position + new Vector3(0, 0.05f, 0), transform.rotation * Quaternion.Euler(-90f, 0, 0), material, gameObject.layer);
		}
	}
}