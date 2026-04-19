using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;
using DelaunatorSharp;
using Gilzoide.ManagedJobs;
using Ionic.Crc;
using Ionic.Zlib;
using JetBrains.Annotations;
using KTrie;
using LudeonTK;
using NVorbis.NAudioSupport;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.IO;
using RimWorld.Planet;
using RimWorld.QuestGen;
using RimWorld.SketchGen;
using RimWorld.Utility;
using RuntimeAudioClipLoader;
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

namespace NAT
{
	public class CompProperties_AbilityCollectorHowl : CompProperties_AbilityEffect
	{
		public SimpleCurve sightstealersPointsFromPointsCurve = new SimpleCurve();

		public CompProperties_AbilityCollectorHowl()
		{
			compClass = typeof(CompAbilityEffect_CollectorHowl);
		}
	}
	public class CompAbilityEffect_CollectorHowl : CompAbilityEffect
	{
		public new CompProperties_AbilityCollectorHowl Props => (CompProperties_AbilityCollectorHowl)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
			Map map = parent.pawn.Map;
			PawnGroupMakerParms pawnGroupMakerParms = new PawnGroupMakerParms
			{
				groupKind = PawnGroupKindDefOf.Sightstealers,
				tile = map.Tile,
				faction = Faction.OfEntities,
				points = Props.sightstealersPointsFromPointsCurve.Evaluate(StorytellerUtility.DefaultThreatPointsNow(map))
			};
			pawnGroupMakerParms.points = Mathf.Max(pawnGroupMakerParms.points, Faction.OfEntities.def.MinPointsToGeneratePawnGroup(pawnGroupMakerParms.groupKind) * 1.05f);
			List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(pawnGroupMakerParms).ToList();
			List<IntVec3> cells = parent.pawn.OccupiedRect().ExpandedBy(15).ClipInsideMap(map).Cells.ToList();
			List<IntVec3> spawnCells = new List<IntVec3>();
			foreach (Pawn p in list)
			{
				if(cells.TryRandomElement((IntVec3 c) => c.Standable(map) && c.Walkable(map) && GenSight.LineOfSight(parent.pawn.Position, c, map), out var result))
                {
					spawnCells.Add(result);
				}
			}
			SpawnRequest spawnRequest = new SpawnRequest(list.Cast<Thing>().ToList(), spawnCells, 1, 1f);
			spawnRequest.spawnSound = SoundDefOf.Pawn_Sightstealer_Howl;
			spawnRequest.preSpawnEffecterOffsetTicks = -40;
			spawnRequest.initialDelay = 180;
			spawnRequest.lord = LordMaker.MakeNewLord(Faction.OfEntities, new LordJob_SightstealerAssault(), map);
			Find.CurrentMap.deferredSpawner.AddRequest(spawnRequest);
		}
	}

	public class CompProperties_AbilityPhaseTo : CompProperties_AbilityEffect
	{
		public CompProperties_AbilityPhaseTo()
		{
			compClass = typeof(CompAbilityEffect_PhaseTo);
		}
	}
	public class CompAbilityEffect_PhaseTo : CompAbilityEffect
	{
		public new CompProperties_AbilityPhaseTo Props => (CompProperties_AbilityPhaseTo)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			base.Apply(target, dest);
			Map map = parent.pawn.Map;
			Job job = JobMaker.MakeJob(NATHDDefOf.NAT_PhaseGoToAbility, target.Cell);
			job.playerForced = true;
			parent.pawn.jobs.StartJob(job, JobCondition.InterruptForced, cancelBusyStances: true);
		}

		public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
		{
			Map map = parent.pawn.Map;
			if(!target.Cell.InBounds(map) || target.Cell.Fogged(map))
			{
				return false;
			}
			if(!target.Cell.StandableBy(map, parent.pawn))
			{
				return false;
			}
			return base.Valid(target, throwMessages);
		}
	}
}