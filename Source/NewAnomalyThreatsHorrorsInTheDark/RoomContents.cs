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
using System.Net.NetworkInformation;
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

namespace NAT
{
	public class RoomContents_CollectorLairRoom : RoomContentsWorker
	{
		public virtual float Chance => 0.15f;

		public virtual FloatRange PointsRange => new FloatRange(140f, 280f);

		public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
		{
			base.FillRoom(map, room, faction, threatPoints);
			if (!Rand.Chance(Chance))
			{
				return;
			}
			SignalAction_Sightstealers signalAction_Ambush = (SignalAction_Sightstealers)ThingMaker.MakeThing(NATDefOf.NAT_SignalAction_Sightstealers);
			signalAction_Ambush.points = PointsRange.RandomInRange;
			signalAction_Ambush.spawnAround = room.rects[0];
			if(room.rects[0].TryFindRandomCell(out var cell, (c)=> c.Standable(map)))
			{
				GenSpawn.Spawn(signalAction_Ambush, cell, map);
			}
		}
	}

	public class RoomContents_CollectorLairStorage : RoomContents_CollectorLairRoom
	{
		public override FloatRange PointsRange => new FloatRange(140f, 350f);

		public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
		{
			base.FillRoom(map, room, faction, threatPoints);
			List<IntVec3> cells = new List<IntVec3>();
			foreach(IntVec3 cell in room.rects[0])
			{
				if(cell.GetFirstBuilding(map)?.def == ThingDefOf.Shelf)
				{
					cells.Add(cell);
				}
			}
			if(cells.NullOrEmpty())
			{
				return;
			}
			ThingSetMakerParams setParms = new ThingSetMakerParams
			{
				makingFaction = Faction.OfEntities,
				tile = map.Tile
			};
			List<Thing> loot = NATHDDefOf.NAT_CollectoirLairStorage.root.Generate(setParms);
			if (loot.NullOrEmpty())
			{
				return;
			}
			foreach (Thing thing in loot)
			{
				if (cells.NullOrEmpty())
				{
					return;
				}
				IntVec3 c = cells.RandomElement();
				GenPlace.TryPlaceThing(thing, c, map, ThingPlaceMode.Near);
				if(c.GetThingList(map)?.Count > 4)
				{
					cells.Remove(c);
				}
			}
		}
	}

	public class RoomContents_CollectorLairBedroom : RoomContents_CollectorLairRoom
	{
		public override float Chance => 1f;

		public override FloatRange PointsRange => new FloatRange(280f, 490f);
	}

	public class RoomContents_CollectorLairEntrance : RoomContentsWorker
    {
        private static readonly IntRange TurretsRange = new IntRange(1, 2);

        public override void FillRoom(Map map, LayoutRoom room, Faction faction, float? threatPoints = null)
        {
            SpawnExit(map, room);
            base.FillRoom(map, room, faction, threatPoints);
        }

        private void SpawnExit(Map map, LayoutRoom room)
        {
            List<Thing> list = new List<Thing>();
            ThingDef exit = NATHDDefOf.NAT_CollectorLairExit;
            List<Thing> spawned = list;
            RoomGenUtility.FillWithPadding(exit, 1, room, map, null, null, spawned, 3);
            MapGenerator.PlayerStartSpot = list.First().Position;
        }
    }
}