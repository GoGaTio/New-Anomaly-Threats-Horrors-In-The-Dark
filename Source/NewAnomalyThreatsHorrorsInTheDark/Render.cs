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
	public class PawnRenderNodeWorker_PhasepasserHead : PawnRenderNodeWorker
	{
		public override Quaternion RotationFor(PawnRenderNode node, PawnDrawParms parms)
		{
			if (!parms.flags.FlagSet(PawnRenderFlags.Portrait) && parms.pawn.TryGetComp<CompPhasepasser>(out var comp))
			{
				return Quaternion.AngleAxis(comp.HeadAngle + node.DebugAngleOffset, Vector3.up);
			}
			return base.RotationFor(node, parms);
		}
	}
}
