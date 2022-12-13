using Mirror;
using UnityEngine;

namespace Game.Managers.NetworkManager
{
    public class RendererLink : NetworkBehaviour
	{
		[SerializeField] private Renderer renderer;

		[SyncVar(hook = nameof(HandleColorChange))]
		public Color playerColor = Color.white;

		private Material playerMaterialClone;

		public void SetColor(Color color)
		{
			playerColor = color;
		}

		private void HandleColorChange(Color old, Color newValue)
		{
			playerMaterialClone = new Material(renderer.material);
			playerMaterialClone.color = newValue;
			renderer.material = playerMaterialClone;
		}
	}
}