using LudeonTK;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.IO;
using RimWorld.Planet;
using RimWorld.QuestGen;
using RimWorld.SketchGen;
using RimWorld.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Grammar;
using Verse.Noise;
using Verse.Profile;
using Verse.Sound;
using Verse.Steam;
using static Unity.IO.LowLevel.Unsafe.AsyncReadManagerMetrics;

namespace NAT
{
	public class IncidentWorker_Collector : IncidentWorker
	{
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			Slate slate = new Slate();
			slate.Set("map", map);
			QuestUtility.GenerateQuestAndMakeAvailable(NATHDDefOf.NAT_CollectorScript, slate);
			return true;
		}
	}

	public class IncidentWorker_PhasepassersAttack : IncidentWorker
	{
		protected override bool TryExecuteWorker(IncidentParms parms)
		{
			Map map = (Map)parms.target;
			parms.faction = Faction.OfEntities;
			PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(NATHDDefOf.NAT_Phasepassers, parms);
			float num = Faction.OfEntities.def.MinPointsToGeneratePawnGroup(NATHDDefOf.NAT_Phasepassers) * 3f;
			if (parms.points < num)
			{
				parms.points = num;
			}
			List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms).ToList();
			if (AnomalyIncidentUtility.IncidentShardChance(parms.points))
			{
				AnomalyIncidentUtility.PawnShardOnDeath(list.RandomElement());
			}
			for (int i = 0; i < list.Count; i++)
			{
				IntVec3 result = CellRect.WholeMap(map).EdgeCells.RandomElement();
				list[i].Position = result;
				GenSpawn.Spawn(list[i], result, map);
			}
			return true;
		}
	}
}