using UnityEngine;

namespace Game.Entities
{
	public class CharacterAnimatorController : MonoBehaviour
	{
		public bool IsIdle { get; private set; }

		[SerializeField] private Animator animator;

		protected int isIdleHash;
		protected int forwardSpeedHash;

		private Character character;

		private void Start()
		{
			forwardSpeedHash = Animator.StringToHash("ForwardSpeed");
			isIdleHash = Animator.StringToHash("IsIdle");
		}

		public void Tick()
		{
			UpdateMovement();
		}

		public void SetCharacter(Character character)
		{
			this.character = character;
		}

		private void UpdateMovement()
		{
			float amount = character.Controller.GetVelocityAmount();

			IsIdle = amount <= 0.05f;

			animator.SetFloat(forwardSpeedHash, amount);
			animator.SetBool(isIdleHash, IsIdle);
		}
	}
}