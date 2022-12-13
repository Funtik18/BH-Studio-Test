using Game.Entities;

using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace Game.Systems.SheetSystem.Skills
{
	public class DashSkill : ActiveSkill
	{
		public bool IsDashing { get; private set; }

		[SerializeField] private DashSettings dashSettings;

		private Coroutine dashCoroutine;

		public override void SetCharacter(Character character)
		{
			base.SetCharacter(character);

			IsDashing = false;
		}

		public override void Execute()
		{
			if (!IsDashing)
			{
				if (dashCoroutine != null)
				{
					StopCoroutine(dashCoroutine);
				}
				dashCoroutine = StartCoroutine(Dash());
			}
		}

		public override void Refresh()
		{
			if (dashCoroutine != null)
			{
				StopCoroutine(dashCoroutine);
				dashCoroutine = null;
			}
		}

		private IEnumerator Dash()
		{
			Character[] targets;

			character.Frezze(true);
			IsDashing = true;

			Vector3 dir = character.CameraVision.transform.forward;
			dir.y = 0;
			yield return RotateTo(dir);

			float t = 0;
			Vector3 startPosition = transform.position;
			Vector3 endPosition = transform.position + (character.Model.forward * dashSettings.distance);

			if(Physics.CapsuleCast(transform.position, transform.position + dashSettings.capsulePoint, dashSettings.capusleRadius, character.Model.forward, out RaycastHit hit))
			{
				endPosition = (endPosition - startPosition).magnitude > (hit.point - startPosition).magnitude ? hit.point : endPosition;
			}

			endPosition.y = startPosition.y;

			yield return new WaitForFixedUpdate();

			while (t <= dashSettings.time)
			{
				character.Rigidbody.MovePosition(Vector3.Lerp(startPosition, endPosition, dashSettings.accelaration.Evaluate(t / dashSettings.time)));
				t += Time.deltaTime;
				yield return new WaitForFixedUpdate();
			}

			dashCoroutine = null;

			Ray centerRay = new Ray(Vector3.Lerp(transform.position, transform.position + dashSettings.capsulePoint, 0.5f), character.Model.forward);
			var rayCasts = Physics.SphereCastAll(centerRay, dashSettings.skillRadius, 1, dashSettings.layerMask);
			if (rayCasts.Length > 0)
			{
				for (int i = 0; i < rayCasts.Length; i++)
				{
					var target = rayCasts[i].collider.GetComponentInParent<Character>();
					if (target != null && target != character)
					{
						if (!target.isImmortal)
						{
							character.SetScore(character.Score + 1);
							target.PhysicsLink.ApplyForce(Vector3.Lerp(character.transform.up, character.Model.forward, 0.5f) * 10, ForceMode.VelocityChange);
							target.StartCoroutine(RendererTimer(target));
						}
						else
						{
							Debug.LogError("IsImmortal");
						}
					}
				}
			}

			yield return new WaitForFixedUpdate();

			IsDashing = false;
			character.Frezze(false);
		}

		private IEnumerator RotateTo(Vector3 direction, float time = 0.2f)
		{
			float t = 0;
			var lookRotation = Quaternion.LookRotation(direction);
			while (t < time)
			{
				character.Model.rotation = Quaternion.Slerp(character.Model.rotation, lookRotation, t / time);
				t += Time.deltaTime;
				yield return null;
			}
		}

		private IEnumerator RendererTimer(Character character)
		{
			character.isImmortal = true;
			character.RendererLink.SetColor(Color.red);
			yield return new WaitForSeconds(dashSettings.duration);
			character.RendererLink.SetColor(Color.white);
			character.isImmortal = false;
		}

#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			if (!dashSettings.debug) return;

			Gizmos.color = Color.red;
			Gizmos.DrawSphere(transform.position + dashSettings.capsulePoint, 0.08f);

			Handles.color = Color.red;
			Vector3 point1 = transform.position + (transform.forward * dashSettings.distance);
			Handles.DrawSolidDisc(point1, transform.up, 0.1f);

			Vector3 point2 = Vector3.Lerp(transform.position, transform.position + dashSettings.capsulePoint, 0.5f);
			Handles.DrawWireDisc(point2, transform.up, dashSettings.capusleRadius);
			Gizmos.DrawWireSphere(point2, dashSettings.skillRadius);

			Gizmos.color = Color.blue;
			point1.y = point2.y;
			Gizmos.DrawLine(point1, point2);
		}
#endif
	}

	[System.Serializable]
	public class DashSettings
	{
		[Min(1f)]
		public float duration = 1f;
		[Min(1f)]
		public float distance = 5f;
		[Min(0.01f)]
		public float time = 0.5f;
		public AnimationCurve accelaration;
		[Space]
		public Vector3 capsulePoint;
		public float capusleRadius;
		public float skillRadius;
		[Space]
		public LayerMask layerMask;

		public bool debug = true;
	}
}