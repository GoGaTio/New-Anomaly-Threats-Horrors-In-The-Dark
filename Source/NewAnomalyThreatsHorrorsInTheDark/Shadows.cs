using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace NAT
{
	public class Shadow : Thing, IAttackTarget, ILoadReferenceable
	{
		Thing IAttackTarget.Thing => this;

		public float TargetPriorityFactor => 0.4f;

		public LocalTargetInfo TargetCurrentlyAimingAt => LocalTargetInfo.Invalid;

		public bool ThreatDisabled(IAttackTargetSearcher disabledFor)
		{
			if (!base.Spawned)
			{
				return true;
			}
			return false;
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
		}

		protected override void TickInterval(int delta)
		{
			base.TickInterval(delta);
		}
	}
}
