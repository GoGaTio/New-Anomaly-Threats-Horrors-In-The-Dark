using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using UnityEngine;

namespace NAT
{
	public class Shadow : ThingWithComps, IAttackTarget, ILoadReferenceable, IAlwaysTargetable
	{
		public Vector3 exactPos;

		public LocalTargetInfo enemyTarget;

		public int meleeCooldownTicksLeft;

		Thing IAttackTarget.Thing => this;

		public float TargetPriorityFactor => 1f;

		public LocalTargetInfo TargetCurrentlyAimingAt => LocalTargetInfo.Invalid;

		public float Damage => 10f;

		public float ArmorPenetration => 1f;

		public override Vector3 DrawPos => exactPos;

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
			if(Faction != Faction.OfEntities)
			{
				this.SetFaction(Faction.OfEntities);
			}
			base.SpawnSetup(map, respawningAfterLoad);
		}

		protected override void Tick()
		{
			base.Tick();
			if(meleeCooldownTicksLeft > 0)
			{
				meleeCooldownTicksLeft--;
			}
		}

		/*public bool TryFindNewTarget()
		{
			float num = float.MaxValue;
			Thing thing = null;
			List<IAttackTarget> potentialTargetsFor = Map.attackTargetsCache.GetPotentialTargetsFor(this);
			for (int i = 0; i < potentialTargetsFor.Count; i++)
			{
				IAttackTarget attackTarget = potentialTargetsFor[i];
				if (!attackTarget.ThreatDisabled(pawn) && AttackTargetFinder.IsAutoTargetable(attackTarget) && (!humanlikesOnly || !(attackTarget is Pawn pawn2) || pawn2.RaceProps.Humanlike) && (!(attackTarget.Thing is Pawn pawn3) || pawn3.IsCombatant() || (!ignoreNonCombatants && GenSight.LineOfSightToThing(pawn.Position, pawn3, pawn.Map))) && (pawn.Faction == null || !pawn.Faction.IsPlayer || !attackTarget.Thing.Position.Fogged(pawn.Map)))
				{
					Thing thing2 = (Thing)attackTarget;
					int num2 = thing2.Position.DistanceToSquared(pawn.Position);
					if ((float)num2 < num && pawn.CanReach(thing2, PathEndMode.OnCell, Danger.Deadly))
					{
						num = num2;
						thing = thing2;
					}
				}
			}
			if (thing != null)
			{
				if (thing.PositionHeld == pawn.PositionHeld || pawn.CanReachImmediate(thing, PathEndMode.Touch))
				{
					return null;
				}
				Job job = JobMaker.MakeJob(JobDefOf.Goto, thing);
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
				job.collideWithPawns = true;
				return job;
			}
			return null;
		}

		public void Attack(LocalTargetInfo target)
		{
			if(target.Thing == null)
			{
				return;
			}
			DamageInfo damageInfo = new DamageInfo(DamageDefOf.Scratch, Damage, ArmorPenetration, -1f, this, null, null, DamageInfo.SourceCategory.ThingOrUnknown);
			damageInfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
			damageInfo.SetAngle((target.Thing.Position - Position).ToVector3());
			target.Thing.TakeDamage(damageInfo);
			if (target.Thing is Pawn pawn)
			{
				if (pawn.kindDef.canMeleeAttack && pawn.CurJob.def == JobDefOf.Wait_Combat)
				{
					pawn.meleeVerbs.TryMeleeAttack(this);
					pawn.jobs.curDriver.collideWithPawns = true;
				}
			}
			meleeCooldownTicksLeft = 120;
		}

		public override void DrawExtraSelectionOverlays()
		{
			if(meleeCooldownTicksLeft > 0)
			{
				float radius = Mathf.Min(0.5f, (float)meleeCooldownTicksLeft * 0.002f);
				GenDraw.DrawCooldownCircle(DrawPos, radius);
			}
		}*/

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref meleeCooldownTicksLeft, "meleeCooldownTicksLeft");
			Scribe_Values.Look(ref exactPos, "exactPos");
			Scribe_Values.Look(ref enemyTarget, "enemyTarget");
		}
	}
}
