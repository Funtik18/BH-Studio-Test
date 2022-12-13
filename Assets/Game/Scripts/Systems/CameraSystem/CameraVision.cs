using Game.Managers.InputManager;
using UnityEngine;

namespace Game.Systems.CameraSystem
{
	public class CameraVision : MonoBehaviour
	{
		public bool IsEnable { get; private set; }

		[SerializeField] private Camera camera;
		[SerializeField] private Transform targetFollow;
		[Space]
		[SerializeField] private float mouseSensitivity = 2.5f;
		[SerializeField] private Vector3 offset;
		[SerializeField] private float distance = 1f;
		[SerializeField] private Vector2 rotationXMinMax = new Vector2(-40, 40);

		private Vector3 currentRotation;
		private Vector3 smoothVelocity = Vector3.zero;
		private float smoothTime = 0.2f;

		private float rotationY;
		private float rotationX;

		public void Tick()
		{
			if (targetFollow == null) return;
			if (!IsEnable) return;

			Vector3 mouse = InputManager.Instance.GetMouseDirection();

			rotationY += mouse.x * mouseSensitivity;
			rotationX += mouse.y * mouseSensitivity;

			rotationX = Mathf.Clamp(rotationX, rotationXMinMax.x, rotationXMinMax.y);
			Vector3 nextRotation = new Vector3(rotationX, rotationY);

			currentRotation = Vector3.SmoothDamp(currentRotation, nextRotation, ref smoothVelocity, smoothTime);
			transform.localEulerAngles = currentRotation;

			transform.position = (targetFollow.position + offset) - (transform.forward) * distance;
		}

		public void Enable(bool trigger)
		{
			camera.enabled = trigger;
			IsEnable = trigger;
		}
	}
}