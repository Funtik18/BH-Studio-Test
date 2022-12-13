using Mirror;
using UnityEngine;

namespace Game.Managers.NetworkManager
{
	public class PhysicsLink : NetworkBehaviour
	{
		[SerializeField] private Rigidbody rb;

		public void ApplyForce(Vector3 force, ForceMode forceMode)
		{
			if (isServer)
			{
				RpcApplyForce(force, forceMode);
			}

			if (isClientOnly)
			{
				CmdApplyForce(force, forceMode);
			}
		}

		[ClientRpc]
		public void RpcApplyForce(Vector3 force, ForceMode FMode)
		{
			rb.AddForce(force, FMode);
		}


		[Command]
		public void CmdApplyForce(Vector3 force, ForceMode FMode)
		{
			RpcApplyForce(force, FMode);
		}
	}
}