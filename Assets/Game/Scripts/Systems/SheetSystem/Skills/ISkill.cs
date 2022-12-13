using UnityEngine;
using Game.Entities;

namespace Game.Systems.SheetSystem.Skills
{
	public interface ISkill { }

	public abstract class PassiveSkill : ISkill { }

	public abstract class ActiveSkill : MonoBehaviour, ISkill
	{
		protected Character character;
		
		public virtual void SetCharacter(Character character)
		{
			this.character = character;
		}

		public abstract void Execute();

		public abstract void Refresh();
	}
}