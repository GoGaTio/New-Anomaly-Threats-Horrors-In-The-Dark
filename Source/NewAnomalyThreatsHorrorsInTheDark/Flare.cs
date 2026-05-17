using RimWorld;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Noise;
using Verse.Sound;

namespace NAT
{
	public class FlyingFlare : ThingWithComps, IThingGlower
	{
		public WeatherEvent_Flare weatherEvent;

		public int ageTicks;

		public int fuelLeft;

		private Effecter flareEffecter;

		private Vector3 direction;

		public bool ShouldBeLitNow()
		{
			return true;
		}

		private static readonly SimpleCurve HeightCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(60f, 8f),
			new CurvePoint(120f, 15f),
			new CurvePoint(600f, 12f),
			new CurvePoint(6000f, 0f)
		};

		private static readonly SimpleCurve OffsetCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(120f, 0.5f),
			new CurvePoint(6000f, 1f)
		};

		public override Vector3 DrawPos => Position.ToVector3() + DrawOffset;

		public Vector3 DrawOffset
		{
			get
			{
				Vector3 vec = direction * OffsetCurve.Evaluate(ageTicks);
				vec.z = HeightCurve.Evaluate(ageTicks);
				return vec;
			}
		}

		public override void PostMake()
		{
			base.PostMake();
			fuelLeft = 5400;
			float angle = new FloatRange(-60, 60).RandomInRange;
			if (Rand.Bool)
			{
				angle += 180f;
			}
			direction = Vector3Utility.FromAngleFlat(angle).Yto0().normalized * 2f;
		}

		protected override void Tick()
		{
			if(weatherEvent == null)
			{
				SpawnWeatherEvent();
			}
			ageTicks++;
			if(flareEffecter == null)
			{
				flareEffecter = NATHDDefOf.NAT_FlyingFlare.Spawn(base.Position, base.Map);
			}
			flareEffecter.offset = DrawOffset;
			flareEffecter.EffectTick(this, this);
			base.Tick();
			fuelLeft -= weatherEvent.darkness ? 5 : 1;
			weatherEvent.fuelLeft = fuelLeft;
			if (fuelLeft <= 0)
			{
				Destroy();
			}
		}

		public void SpawnWeatherEvent()
		{
			weatherEvent = new WeatherEvent_Flare(Map);
			weatherEvent.darkness = GameCondition_UnnaturalDarkness.UnnaturalDarknessOnMap(Map);
			weatherEvent.fuelLeft = fuelLeft;
			Map.weatherManager.eventHandler.AddEvent(weatherEvent);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			if (weatherEvent == null || weatherEvent.Expired)
			{
				SpawnWeatherEvent();
			}
			NATHDDefOf.NAT_FlareLaunch.PlayOneShot(this);
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			base.DeSpawn(mode);
			if (weatherEvent != null)
			{
				weatherEvent.expired = true;
			}
		}
	}
}
