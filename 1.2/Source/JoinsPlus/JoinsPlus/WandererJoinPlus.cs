using System;
using System.Collections.Generic;
using Verse;
using RimWorld;
using HarmonyLib;
using RimWorld.Planet;
using System.Reflection;
using System.Linq;

namespace WandererJoinPlus
{
	[StaticConstructorOnStartup]
	public class Main
	{
		public static Harmony harmony;
		static Main()
		{
			harmony = new Harmony("com.ogliss.rimworld.mod.WandererJoinPlus");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
			if (Prefs.DevMode) Log.Message(string.Format("WandererJoinPlus: successfully completed {0} harmony patches.", harmony.GetPatchedMethods().Select(new Func<MethodBase, Patches>(Harmony.GetPatchInfo)).SelectMany((Patches p) => p.Prefixes.Concat(p.Postfixes).Concat(p.Transpilers)).Count((Patch p) => p.owner.Contains(harmony.Id))), false);
		}

	}

}
