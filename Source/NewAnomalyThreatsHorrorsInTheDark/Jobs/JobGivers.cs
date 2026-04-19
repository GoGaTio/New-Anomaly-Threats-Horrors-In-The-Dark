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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;
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
using static System.Net.WebRequestMethods;

namespace NAT
{
	public class JobGiver_AIPhaseAttack : ThinkNode_JobGiver
	{
		private bool ignoreNonCombatants;

		private bool humanlikesOnly;

		private int overrideExpiryInterval = -1;

		public override ThinkNode DeepCopy(bool resolve = true)
		{
			JobGiver_AIPhaseAttack obj = (JobGiver_AIPhaseAttack)base.DeepCopy(resolve);
			obj.ignoreNonCombatants = ignoreNonCombatants;
			obj.humanlikesOnly = humanlikesOnly;
			obj.overrideExpiryInterval = overrideExpiryInterval;
			return obj;
		}

		protected override Job TryGiveJob(Pawn pawn)
		{
			float num = float.MaxValue;
			Thing thing = null;
			List<IAttackTarget> potentialTargetsFor = pawn.Map.attackTargetsCache.GetPotentialTargetsFor(pawn);
			for (int i = 0; i < potentialTargetsFor.Count; i++)
			{
				IAttackTarget attackTarget = potentialTargetsFor[i];
				if (JobDriver_PhasepasserAttack.ValidTarget(attackTarget, pawn))
				{
					Thing thing2 = (Thing)attackTarget;
					int num2 = thing2.Position.DistanceToSquared(pawn.Position);
					if ((float)num2 < num)
					{
						num = num2;
						thing = thing2;
					}
				}
			}
			if (thing != null)
			{
				if (thing.PositionHeld == pawn.PositionHeld)
				{
					return null;
				}
				Job job = JobMaker.MakeJob(NATHDDefOf.NAT_PhasepasserAttack, thing);
				if (overrideExpiryInterval > 0)
				{
					job.expiryInterval = overrideExpiryInterval;
				}
				else
				{
					job.intervalScalingTarget = TargetIndex.A;
				}
				job.checkOverrideOnExpire = true;
				job.expireRequiresEnemiesNearby = true;
				job.collideWithPawns = false;
				return job;
			}
			return null;
		}
	}

	public class JobGiver_PhaseWander : ThinkNode_JobGiver
	{
		protected override Job TryGiveJob(Pawn pawn)
		{
			Job job = null;
			IntVec3 dest = RCellFinder.RandomWanderDestFor(pawn, pawn.Position, 7f, null, Danger.Deadly, false);
			if(dest == pawn.Position)
			{
				job = JobMaker.MakeJob(JobDefOf.Wait);
				job.expiryInterval = 180;
				job.checkOverrideOnExpire = true;
				return job;
			}
			job = JobMaker.MakeJob(NATHDDefOf.NAT_PhasepasserMoveTo, dest);
			job.expiryInterval = 180;
			job.checkOverrideOnExpire = true;
			job.reportStringOverride = reportStringOverride;
			return job;
		}
	}
}