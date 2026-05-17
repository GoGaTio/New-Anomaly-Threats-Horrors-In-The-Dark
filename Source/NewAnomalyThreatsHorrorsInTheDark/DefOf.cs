using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using RimWorld.BaseGen;
using RimWorld.IO;
using RimWorld.Planet;
using RimWorld.QuestGen;
using RimWorld.SketchGen;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using Verse.Grammar;
using Verse.Noise;
using Verse.Profile;
using Verse.Sound;
using Verse.Steam;
using UnityEngine;
using System.Diagnostics;

namespace NAT
{
    [DefOf]
    public static class NATHDDefOf
    {
		public static AnimationDef NAT_Phasepasser;

		public static ThingDef NAT_CollectorNotes;

		public static ThingDef NAT_CollectorGlassCase;

		public static ThingDef NAT_CollectorLairExit;

		public static JobDef NAT_CollectorStealPawn;

		public static JobDef NAT_CollectorStealThing;

		public static JobDef NAT_CollectorWait;

		public static JobDef NAT_CollectorEscape;

		public static JobDef NAT_PhasepasserMoveTo;

		public static JobDef NAT_PhasepasserAttack;

		public static JobDef NAT_PhaseGoToAbility;

		public static HediffDef NAT_CollectorHypnosis;

		public static PawnKindDef NAT_Collector;

		public static LayoutRoomDef NAT_CollectionRoom;

		public static QuestScriptDef NAT_CollectorScript;

		public static ThingSetMakerDef NAT_CollectoirLairCase;

		public static ThingSetMakerDef NAT_CollectoirLairStorage;

		public static PawnGroupKindDef NAT_Phasepassers;

		public static DamageDef NAT_Distortion;

		public static EffecterDef NAT_FlyingFlare;

		public static SoundDef NAT_FlareLaunch;
	}
}
