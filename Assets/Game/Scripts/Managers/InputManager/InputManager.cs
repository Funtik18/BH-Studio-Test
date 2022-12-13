using System.Collections.Generic;
using UnityEngine;

namespace Game.Managers.InputManager
{
	public class InputManager : LazySingletonMonoDontDestroyOnLoad<InputManager>
	{
		private Dictionary<KeyType, KeyCode> KeyBindings = new Dictionary<KeyType, KeyCode>
		{
			{KeyType.Up, KeyCode.W },
			{KeyType.Down, KeyCode.S },
			{KeyType.Left, KeyCode.A },
			{KeyType.Right, KeyCode.D },
			{KeyType.LBM, KeyCode.Mouse0 },
		};

		public Vector3 GetDirection()
		{
			return new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		}

		public Vector2 GetMouseDirection()
		{
			return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
		}

		public KeyCode GetKey(KeyType keyType)
		{
			KeyBindings.TryGetValue(keyType, out KeyCode code);
			return code;
		}
	}

	public enum KeyType
	{
		None,

		Up,
		Down,
		Left,
		Right,

		LBM,
	}
}