using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace NAT
{
	public class WeatherEvent_Flare : WeatherEvent
	{
		private static readonly SimpleCurve LitCurve = new SimpleCurve
		{
			new CurvePoint(0f, 0f),
			new CurvePoint(180f, 0.3f),
			new CurvePoint(5340f, 0.3f),
			new CurvePoint(5400f, 0f)
		};

		public bool expired;

		public int fuelLeft;

		public bool darkness;

		private static readonly SkyColorSet LightningFlashColors = new SkyColorSet(new Color(1f, 0.9f, 0.9f), new Color(1f, 0.9f, 0.9f), new Color(1f, 0.9f, 0.9f), 1.15f);

		public override bool Expired => expired;

		public override SkyTarget SkyTarget => new SkyTarget(1f, LightningFlashColors, 1f, 1f);

		public override float SkyTargetLerpFactor => LitCurve.Evaluate(fuelLeft);

		public WeatherEvent_Flare(Map map)
			: base(map)
		{
		}

		public override void FireEvent()
		{
			
		}

		public override void WeatherEventTick()
		{
			
		}
	}
}
