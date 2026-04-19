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

namespace NAT
{
	public class JobDriver_MoveIgnoreAnything : JobDriver
	{
		public LocalTargetInfo Target => job.GetTarget(TargetIndex.A);

		private float movingCostLeft = 0;

		private float movingCostTotal = 1;

		private IntVec3 nextCell;

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref movingCostLeft, "movingCostLeft", 0);
			Scribe_Values.Look(ref movingCostTotal, "movingCostTotal", 1);
			Scribe_Values.Look(ref nextCell, "nextCell");
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return true;
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			Toil toil = ToilMaker.MakeToil("MoveIgnoreAnything");
			toil.initAction = delegate
			{
				nextCell = toil.actor.Position + FromAngleFlat((Target.Cell - toil.actor.Position).AngleFlat);
				movingCostTotal = TicksToMove(pawn, nextCell);
				movingCostLeft = movingCostTotal;
			};
			toil.tickAction = delegate
			{
				toil.actor.rotationTracker.FaceCell(Target.Cell);
				if (toil.actor.Drawer.renderer.CurAnimation != NATHDDefOf.NAT_Phasepasser)
				{
					pawn.Drawer.renderer.SetAnimation(NATHDDefOf.NAT_Phasepasser);
				}
				if (movingCostLeft > 0f)
				{
					movingCostLeft -= CostToMoveThisTick(toil.actor);
				}
				if (movingCostLeft <= 0f)
				{
					TryEnterNextPathCell(toil.actor);
				}
			};
			toil.handlingFacing = true;
			toil.defaultCompleteMode = ToilCompleteMode.Never;
			yield return toil;
		}

		private float CostToMoveThisTick(Pawn pawn)
		{
			float num = 1f;
			if (Staggerable && pawn.stances.stagger.Staggered)
			{
				num *= pawn.stances.stagger.StaggerMoveSpeedFactor;
			}
			if (num < movingCostTotal / 450f)
			{
				num = movingCostTotal / 450f;
			}
			return num;
		}

		protected virtual bool Staggerable => true;

		private float TicksToMove(Pawn pawn, IntVec3 c)
		{
			bool diagonal = (c.x != pawn.Position.x && c.z != pawn.Position.z);
			float num = MoveSpeed(pawn);
			float num2 = num / 60f;
			float num3;
			if (num2 == 0f)
			{
				num3 = 450f;
			}
			else
			{
				num3 = 1f / num2;
				if (diagonal)
				{
					num3 *= 1.41421f;
				}
			}
			num3 = Mathf.Clamp(num3, 1f, 450f);
			if (pawn.CurJob != null)
			{
				switch (pawn.jobs.curJob.locomotionUrgency)
				{
					case LocomotionUrgency.Amble:
						num3 *= 3f;
						if (num3 < 60f)
						{
							num3 = 60f;
						}
						break;
					case LocomotionUrgency.Walk:
						num3 *= 2f;
						if (num3 < 50f)
						{
							num3 = 50f;
						}
						break;
					case LocomotionUrgency.Jog:
						num3 *= 1f;
						break;
					case LocomotionUrgency.Sprint:
						num3 = Mathf.RoundToInt(num * 0.75f);
						break;
				}
			}
			return num3;
		}

		protected virtual float MoveSpeed(Pawn p) => p.GetStatValue(StatDefOf.MoveSpeed);

		protected virtual void TryEnterNextPathCell(Pawn pawn)
		{
			pawn.Position = nextCell;
			pawn.filth.Notify_EnteredNewCell();
			float angle = (Target.Cell - pawn.Position).AngleFlat;
			nextCell = pawn.Position + FromAngleFlat(angle);
			movingCostTotal = TicksToMove(pawn, nextCell);
			movingCostLeft = movingCostTotal;
			if (Target.Cell == pawn.Position)
			{
				pawn.jobs.curDriver.ReadyForNextToil();
			}
		}

		public static IntVec3 FromAngleFlat(float angle)
		{
			angle = GenMath.PositiveMod(angle, 360f);
			if (angle < 22.5f)
			{
				return IntVec3.North;
			}
			if (angle < 67.5f)
			{
				return IntVec3.NorthEast;
			}
			if (angle < 112.5f)
			{
				return IntVec3.East;
			}
			if (angle < 157.5f)
			{
				return IntVec3.SouthEast;
			}
			if (angle < 202.5f)
			{
				return IntVec3.South;
			}
			if (angle < 247.5f)
			{
				return IntVec3.SouthWest;
			}
			if (angle < 292.5f)
			{
				return IntVec3.West;
			}
			if (angle < 337.5f)
			{
				return IntVec3.NorthWest;
			}
			return IntVec3.North;
		}
	}

	public class JobDriver_PhasepasserAttack : JobDriver_MoveIgnoreAnything
	{
		protected override IEnumerable<Toil> MakeNewToils()
		{
			foreach(Toil toil in base.MakeNewToils())
			{
				toil.FailOnDespawnedOrNull(TargetIndex.A);
				/*toil.tickIntervalAction = (Action<int>)Delegate.Combine(toil.tickIntervalAction, (Action<int>)delegate (int delta)
				{
					Job curJob = toil.actor.jobs.curJob;
					if (Rand.MTBEventOccurs(180f, 1f, delta))
					{
						curJob.SetTarget(TargetIndex.A, GetClosestTargetInRadius(pawn, 15f) ?? curJob.GetTarget(TargetIndex.A));
						pawn.mindState.enemyTarget = curJob.GetTarget(TargetIndex.A).Pawn;
					}
					if (curJob.targetA == null)
					{
						EndJobWith(JobCondition.Incompletable);
					}
				});*/
				yield return toil;
			}
			Toil attack = ToilMaker.MakeToil("PhasepasserAttack");
			attack.FailOnDestroyedOrNull(TargetIndex.A);
			attack.initAction = delegate
			{
				attack.actor.GetComp<CompPhasepasser>().Detect();
				DisablePather(true);
			};
			attack.tickAction = delegate
			{
				Thing t = Target.Thing;
				//Pawn p = Target.Pawn;
				if (t == null || attack.actor.Position.DistanceTo(t.PositionHeld) > 8f)
				{
					attack.actor.jobs?.curDriver?.ReadyForNextToil();
					return;
				}
				if (!t.Spawned)
				{
					Thing thing = t.SpawnedParentOrMe;
					if(thing != null)
					{
						DisablePather(false);
						attack.actor.jobs?.curJob.SetTarget(TargetIndex.A, thing);
						DisablePather(true);
						return;
					}
					else
					{
						attack.actor.jobs?.curDriver?.ReadyForNextToil();
						return;
					}
				}
				if (!attack.actor.GetComp<CompPhasepasser>().KeepAttackTick(t))
				{
					attack.actor.jobs?.curDriver?.ReadyForNextToil();
					DisablePather(false);
				}
			};
			attack.AddFinishAction(delegate
			{
				DisablePather(false);
				attack?.actor?.GetComp<CompPhasepasser>()?.RemoveMote();
			});
			attack.defaultCompleteMode = ToilCompleteMode.Never;
			yield return attack;
		}

		protected override void TryEnterNextPathCell(Pawn pawn)
		{
			base.TryEnterNextPathCell(pawn);
			if(Target.Cell != pawn.Position && pawn.Position.DistanceTo(Target.Cell) <= 7 && GenSight.LineOfSight(pawn.Position, Target.Cell, pawn.Map, false))
			{
				pawn.jobs.curDriver.ReadyForNextToil();
			}
		}

		public static Pawn GetClosestTargetInRadius(Pawn pawn, float radius)
		{
			List<Thing> list = pawn.Map.listerThings.ThingsInGroup(ThingRequestGroup.Pawn);
			float num = float.MaxValue;
			Pawn result = null;
			foreach (Pawn item in list)
			{
				if (ValidTarget(item, pawn) && pawn.Position.InHorDistOf(item.Position, radius) && (float)item.Position.DistanceToSquared(pawn.Position) < num)
				{
					num = item.Position.DistanceToSquared(pawn.Position);
					result = item;
				}
			}
			return result;
		}

		public static bool ValidTarget(IAttackTarget target, Pawn attacker)
		{
			if((target as Thing) == null) return false;
			Pawn pawn = target as Pawn;
			if (pawn?.IsPsychologicallyInvisible() == false && target.ThreatDisabled(attacker))
			{
				return false;
			}
			if (!AttackTargetFinder.IsAutoTargetable(target))
			{
				return false;
			}
			return true;
		}

		private void DisablePather(bool flag)
		{
			if(job != null && Target.Pawn?.pather != null)
			{
				Target.Pawn.pather.debugDisabled = flag;
			}
		}
	}

	public class JobDriver_PhaseGoToAbility : JobDriver_MoveIgnoreAnything
	{
		protected override IEnumerable<Toil> MakeNewToils()
		{
			foreach (Toil toil in base.MakeNewToils())
			{
				yield return toil;
				toil.AddFinishAction(delegate
				{
					if (toil.actor.Drawer.renderer.CurAnimation == NATHDDefOf.NAT_Phasepasser)
					{
						pawn.Drawer.renderer.SetAnimation(null);
					}
				});
			}
			Toil end = ToilMaker.MakeToil("EndAnimation");
			end.initAction = delegate
			{
				if (end.actor.Drawer.renderer.CurAnimation == NATHDDefOf.NAT_Phasepasser)
				{
					pawn.Drawer.renderer.SetAnimation(null);
				}
				end.actor.Position.GetDoor(end.actor.Map)?.StartManualOpenBy(end.actor);
			};
			end.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return end;
		}

		protected override float MoveSpeed(Pawn p) => 9f;

		protected override bool Staggerable => false;
	}
}