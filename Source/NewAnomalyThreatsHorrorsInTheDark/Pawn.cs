using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using RimWorld;

namespace NAT
{
	public class Pawn_Unkillable : Pawn
	{
		public bool sourceDestroyed = false;

		public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
		{
			if (sourceDestroyed)
			{
				base.Kill(dinfo, exactCulprit);
			}
			
		}

		public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
		{
			if (sourceDestroyed)
			{
				base.PreApplyDamage(ref dinfo, out absorbed);
				return;
			}
			absorbed = true;
		}

		protected override void TickInterval(int delta)
		{
			base.TickInterval(delta);
			if (!sourceDestroyed && this.IsHashIntervalTick(30, delta))
			{
				health.RemoveAllHediffs();
			}
		}
	}
}
