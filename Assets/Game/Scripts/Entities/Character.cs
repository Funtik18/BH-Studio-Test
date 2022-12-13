using Mirror;
using Game.Systems.SheetSystem.Skills;
using UnityEngine;
using Game.Systems.CameraSystem;
using Game.Systems.NavigationSystem;
using Game.Managers.InputManager;
using Game.Managers.NetworkManager;

namespace Game.Entities
{
	public class Character : NetworkBehaviour
	{
		public Transform Model => model;
		public CameraVision CameraVision => cameraVision;
		public CharacterController3D Controller => controller3D;
		public CharacterAnimatorController Animator => animator;
		public Rigidbody Rigidbody => rigidbody;
		public PhysicsLink PhysicsLink => physicsLink;
		public RendererLink RendererLink => rendererLink;
		public NetworkIdentity NetworkIdentity => identity;

		[Header("Components")]
		[SerializeField] private Transform model;
		[SerializeField] private CameraVision cameraVision;
		[SerializeField] private CharacterController3D controller3D;
		[SerializeField] private CharacterAnimatorController animator;
		[SerializeField] private Rigidbody rigidbody;
		[SerializeField] private PhysicsLink physicsLink;
		[SerializeField] private RendererLink rendererLink;
		[SerializeField] private NetworkIdentity identity;

		[SerializeField] private ActiveSkill dashSkill;

		public int Score { get; private set; }

		[SyncVar] public bool isImmortal = false;
		[SyncVar] public bool isCanMove;
		[SyncVar] public bool isCanRotate;


		private void Start()
		{
			if (!isOwned) return;

			isCanMove = true;
			isCanRotate = true;

			dashSkill.SetCharacter(this);

			controller3D.SetCharacter(this);
			animator.SetCharacter(this);
		}

		public override void OnStartAuthority()
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Confined;

			cameraVision.Enable(true);
		}

		private void Update()
		{
			if (!isOwned) return;

			cameraVision.Tick();
			animator.Tick();

			if (IsKeyDown(KeyType.LBM))
			{
				dashSkill.Execute();
			}
		}

		private void FixedUpdate()
		{
			if (!isOwned) return;

			controller3D.FixedTick();
		}

		public void Frezze(bool trigger, bool withKinematic = false)
		{
			isCanMove = !trigger;
			isCanRotate = !trigger;

			if (withKinematic)
			{
				rigidbody.isKinematic = trigger;
			}
		}

		public void SetScore(int score)
		{
			Score = score;
			MatchStatistics.Instance.SetCount(identity, Score);
		}

		public void Refresh()
		{
			isImmortal = false;
			Score = 0;
			dashSkill.Refresh();
			Frezze(false, true);
		}

		protected bool IsKeyDown(KeyType keyType)
		{
			return Input.GetKeyDown(InputManager.Instance.GetKey(keyType));
		}
	}
}