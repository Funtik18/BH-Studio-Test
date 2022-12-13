using Game.Entities;
using Game.Managers.InputManager;

using UnityEngine;

namespace Game.Systems.NavigationSystem
{
	public class CharacterController3D : MonoBehaviour
	{
		private Character character;

		private float baseSpeed = 200f;

		private float turnSmoothTime = 0.1f;
		private float smoothVelocity;

		public void FixedTick()
		{
			Vector3 direction = InputManager.Instance.GetDirection().normalized;

			if (direction.magnitude > 0.1f)
			{
				float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + character.CameraVision.transform.eulerAngles.y;
				float angle = Mathf.SmoothDampAngle(character.Model.eulerAngles.y, targetAngle, ref smoothVelocity, turnSmoothTime);

				if (character.isCanRotate)
				{
					character.Model.rotation = Quaternion.Euler(0, angle, 0);
				}

				if (character.isCanMove)
				{
					Vector3 velocity = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
					character.Rigidbody.velocity = velocity.normalized * Time.fixedDeltaTime * baseSpeed;
				}
			}
		}

		public void SetCharacter(Character character)
		{
			this.character = character;
		}

		public float GetVelocityAmount()
		{
			return Mathf.InverseLerp(0, 1f, character.Rigidbody.velocity.magnitude);
		}
	}
}