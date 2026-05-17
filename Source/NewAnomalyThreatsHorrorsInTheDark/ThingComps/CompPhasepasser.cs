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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
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
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.Scripting.GarbageCollector;

namespace NAT
{
	public class CompProperties_Phasepasser : CompProperties
	{
		public int attackCooldown;

		public FloatRange armorPenetrationRange = new FloatRange(0, 1);

		public FloatRange damageRange = new FloatRange(3, 5);

		public FloatRange headAngleRange = new FloatRange(0, 1);

		public EffecterDef attackEffecter;

		public CompProperties_Phasepasser()
		{
			compClass = typeof(CompPhasepasser);
		}
	}
	public class CompPhasepasser : ThingComp
	{
		public CompProperties_Phasepasser Props => (CompProperties_Phasepasser)props;

		[Unsaved(false)]
		public HediffComp_Invisibility invisibility;

		private int lastDetectedTick = -99999;

		private static float lastNotified = -99999f;

		private float headAngle;

		public int ticksSinceLastAttack;

		public override void PostExposeData()
		{
			base.PostExposeData();
			Scribe_Values.Look(ref headAngle, "headAngle", 0);
			Scribe_Values.Look(ref lastDetectedTick, "lastDetectedTick", 0);
			Scribe_Values.Look(ref ticksSinceLastAttack, "ticksSinceLastAttack", 0);
		}

		private Pawn Phasepasser => (Pawn)parent;

		public HediffComp_Invisibility Invisibility => invisibility ?? (invisibility = Phasepasser.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.HoraxianInvisibility)?.TryGetComp<HediffComp_Invisibility>());

		public float HeadAngle
		{
			get
			{
				float num = headAngle;
				int rot = parent.Rotation.AsInt;
				if (rot > 1)
				{
					num *= -1f;
				}
				if(rot == 1 || rot == 3)
				{
					num *= 0.5f;
				}
				return num;
			}
		}

		public bool KeepAttackTick(Thing target)
		{
			if (Phasepasser.DeadOrDowned || target.Map != parent.Map || parent.Position.DistanceTo(target.Position) > 1.5f)
			{
				return false;
			}
			Pawn pawn = target as Pawn;
			if (pawn.DeadOrDowned)
			{
				return false;
			}
			if (Phasepasser.stances.stunner?.Stunned == true || Phasepasser.stances.curStance is Stance_Cooldown)
			{
				return true;
			}
			target.TakeDamage(new DamageInfo(NATHDDefOf.NAT_Distortion, Props.damageRange.RandomInRange, Props.armorPenetrationRange.RandomInRange, -1, Phasepasser));
			Phasepasser.stances.SetStance(new Stance_Cooldown(Props.attackCooldown, target, null));
			Props.attackEffecter?.SpawnAttached(target, parent.Map);
			ticksSinceLastAttack = 0;
			if ((pawn != null && pawn.DeadOrDowned) || target.Destroyed)
			{
				return false;
			}
			return true;
		}

        public override string CompInspectStringExtra()
        {
			string s = null;
			if(DebugSettings.ShowDevGizmos && Phasepasser.CurJob != null)
            {
				s = Phasepasser.CurJob.def.defName;
			}
			return s;
        }

        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
			if (DebugSettings.ShowDevGizmos && Phasepasser.CurJob != null)
			{
				GenDraw.DrawCircleOutline(Phasepasser.Position.ToVector3(), 0.7f, SimpleColor.Orange);
				if (Phasepasser.CurJob.targetA.IsValid)
				{
					GenDraw.DrawCircleOutline(Phasepasser.CurJob.targetA.CenterVector3, 0.7f, SimpleColor.Red);
					GenDraw.DrawLineBetween(parent.TrueCenter(), Phasepasser.CurJob.targetA.CenterVector3, SimpleColor.Red);
				}
				if (Phasepasser.CurJob.targetB.IsValid)
				{
					GenDraw.DrawCircleOutline(Phasepasser.CurJob.targetB.CenterVector3, 0.7f, SimpleColor.Blue);
					GenDraw.DrawLineBetween(parent.TrueCenter(), Phasepasser.CurJob.targetB.CenterVector3, SimpleColor.Blue);
				}
			}
        }

        public override void CompTick()
		{
			ticksSinceLastAttack++;
			base.CompTick();
			if (Invisibility == null)
			{
				Phasepasser.health.AddHediff(HediffDefOf.HoraxianInvisibility);
			}
			if (!Phasepasser.Spawned)
			{
				return;
			}
			if (parent.IsHashIntervalTick(7))
			{
				if (Find.TickManager.TicksGame > lastDetectedTick + 3500)
				{
					CheckDetected();
				}
				if (Find.TickManager.TicksGame > lastDetectedTick + 3500)
				{
					Invisibility.BecomeInvisible();
				}
			}
		}


		private void CheckDetected()
		{
			foreach (Pawn item in Phasepasser.Map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn))
			{
				if (PawnCanDetect(item))
				{
					Detect();
				}
			}
		}

		public void Detect()
		{
			if (!Invisibility.PsychologicallyVisible)
			{
				Invisibility.BecomeVisible();
			}
			lastDetectedTick = Find.TickManager.TicksGame;
		}

		private bool PawnCanDetect(Pawn pawn)
		{
			if (pawn.Faction == Faction.OfEntities || pawn.Faction == Faction.OfMechanoids || pawn.Downed || !pawn.Awake())
			{
				return false;
			}
			if (pawn.IsAnimal)
			{
				return false;
			}
			if (!Phasepasser.Position.InHorDistOf(pawn.Position, GetPawnSightRadius(pawn, Phasepasser)))
			{
				return false;
			}
			return GenSight.LineOfSightToThing(pawn.Position, Phasepasser, parent.Map);
		}

		private static float GetPawnSightRadius(Pawn pawn, Pawn phasepasser)
		{
			float num = 14f;
			if (pawn.genes == null || pawn.genes.AffectedByDarkness)
			{
				float t = phasepasser.Map.glowGrid.GroundGlowAt(phasepasser.Position);
				num *= Mathf.Lerp(0.33f, 1f, t);
			}
			return num * pawn.health.capacities.GetLevel(PawnCapacityDefOf.Sight);
		}

		public override void Notify_BecameVisible()
		{
			//SoundDefOf.Pawn_Sightstealer_Howl.PlayOneShotOnCamera();
			foreach (Pawn item in Phasepasser.MapHeld.listerThings.ThingsInGroup(ThingRequestGroup.Pawn))
			{
				if (item.kindDef == Phasepasser.kindDef && item != Phasepasser && item.Position.InHorDistOf(Phasepasser.Position, 30f) && !item.IsPsychologicallyInvisible() && GenSight.LineOfSight(item.Position, Phasepasser.Position, item.Map))
				{
					return;
				}
			}
			if (RealTime.LastRealTime > lastNotified + 60f)
			{
				Find.LetterStack.ReceiveLetter("LetterLabelSightstealerRevealed".Translate(), "LetterSightstealerRevealed".Translate(), LetterDefOf.ThreatBig, Phasepasser, null, null, null, null, 6);
			}
			else
			{
				Messages.Message("MessageSightstealerRevealed".Translate(), Phasepasser, MessageTypeDefOf.ThreatBig);
			}
			lastNotified = RealTime.LastRealTime;
			lastDetectedTick = Find.TickManager.TicksGame;
		}

        public override void Notify_UsedVerb(Pawn pawn, Verb verb)
        {
            base.Notify_UsedVerb(pawn, verb);
			Invisibility.BecomeVisible();
			lastDetectedTick = Find.TickManager.TicksGame;
		}

		public override void PostPostMake()
		{
			base.PostPostMake();
			headAngle = Props.headAngleRange.RandomInRange;
		}

		public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
		{
			base.PostPostApplyDamage(dinfo, totalDamageDealt);
			if(dinfo.Instigator != null && dinfo.Instigator.Position.DistanceTo(parent.Position) < 7f && Phasepasser.jobs != null && Phasepasser.jobs.curDriver?.CurToilString != "PhasepasserAttack")
			{
				Job job = JobMaker.MakeJob(NATHDDefOf.NAT_PhasepasserAttack, dinfo.Instigator);
				job.checkOverrideOnExpire = true;
				job.expireRequiresEnemiesNearby = true;
				job.collideWithPawns = false;
				Phasepasser.jobs.StartJob(job, JobCondition.InterruptForced);
			}
		}
	}
}