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
		public static bool AlienRaces_Active;
		public static bool VEE_Active;
		public static bool AlphaAnimals_Active;
		public static Harmony harmony;
		static Main()
		{
			AlienRaces_Active = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "erdelf.HumanoidAlienRaces");
			VEE_Active = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "VanillaExpanded.VEE");
			AlphaAnimals_Active = ModsConfig.ActiveModsInLoadOrder.Any((ModMetaData m) => m.PackageIdPlayerFacing == "sarg.alphaanimals");
			harmony = new Harmony("com.ogliss.rimworld.mod.WandererJoinPlus");
			harmony.PatchAll(Assembly.GetExecutingAssembly());
			/*
            if (VEE_Active)
            {
				VEE_WarnderJoinsTraitorPatch();

			}
            if (AlphaAnimals_Active)
            {
				AlphaAnimals_WarnderJoinsMimePatch();

			}
			*/
			if (Prefs.DevMode) Log.Message(string.Format("WandererJoinPlus: successfully completed {0} harmony patches.", harmony.GetPatchedMethods().Select(new Func<MethodBase, Patches>(Harmony.GetPatchInfo)).SelectMany((Patches p) => p.Prefixes.Concat(p.Postfixes).Concat(p.Transpilers)).Count((Patch p) => p.owner.Contains(harmony.Id))), false);
		}

		public static void AlphaAnimals_WarnderJoinsMimePatch()
		{
			harmony.Patch(AccessTools.Method(GenTypes.GetTypeInAnyAssembly("VEE.RegularEvents.WandererJoinTraitor", "VEE.RegularEvents"), "TryExecuteWorker", null, null),
				new HarmonyMethod(typeof(Main).GetMethod("VEE_WarnderJoinsTraitorPrefix")), null);
		}

		public static bool AlphaAnimals_WarnderJoinsMimePrefix(ref bool __result, ref IncidentWorker __instance, IncidentParms parms)
		{
			Log.Message("WJP_IncidentWorker checking " + __instance.def.defName);
			if (__instance.def.defName.Contains("WandererJoin") && !__instance.def.defName.Contains("ManInBlack"))
			{
				Map map = (Map)parms.target;
				if (!map.mapPawns.AnyColonistSpawned)
				{
					return true;
				}
				IntVec3 loc;
				if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Neutral, out loc))
				{
					__result = false;
				}
				Gender? gender = null;
				if (__instance.def.pawnFixedGender != Gender.None)
				{
					gender = new Gender?(__instance.def.pawnFixedGender);
				}
				PawnKindDef pawnKind = __instance.def.pawnKind;
				Faction ofPlayer = Faction.OfPlayer;
				bool pawnMustBeCapableOfViolence = __instance.def.pawnMustBeCapableOfViolence;
				Gender? gender2 = gender;
				float num = (float)typeof(IncidentWorker_WandererJoin).GetField("RelationWithColonistWeight", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
				PawnGenerationRequest pawnGenerationRequest = new PawnGenerationRequest(pawnKind, ofPlayer, (PawnGenerationContext)2, -1, true, false, false, false, true, pawnMustBeCapableOfViolence, num, false, true, true, false, false, false, false, false);
				Pawn pawn = PawnGenerator.GeneratePawn(pawnGenerationRequest);
				TaggedString ttexta;
				TaggedString tlabela = __instance.def.letterLabel.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN", true);
				DiaNode diaNode;
				bool abasia = __instance.def.defName == "WandererJoinAbasia";
				if (abasia)
				{
					CheckHediff(__instance, pawn);
					CheckHediff2(__instance, pawn, ref ttexta);
					diaNode = new DiaNode(CreateTransportPodText(__instance, pawn));
				}
				else
				{
					ttexta = __instance.def.letterText.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN", true);
					diaNode = new DiaNode(CreateWandererText(pawn));
				}
				DiaOption diaOptionDetails = new DiaOption(Translator.Translate("ClickForMoreInfo"));
				diaOptionDetails.action = delegate ()
				{
					Find.WindowStack.Add(new Dialog_InfoCard(pawn));
				};
				diaNode.options.Add(diaOptionDetails);
				DiaOption diaOption = new DiaOption(Translator.Translate(abasia ? "WandererPodJoinAccept" : "WandererJoinAccept"));
				PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref ttexta, ref tlabela, pawn);
				GenPersonStatText.AppendStats(ref ttexta, pawn);
				diaOption.action = delegate ()
				{
					GenSpawn.Spawn(pawn, loc, map, 0);
					Find.LetterStack.ReceiveLetter(tlabela, ttexta, LetterDefOf.PositiveEvent, pawn, null, null);
				};
				diaOption.resolveTree = true;
				diaNode.options.Add(diaOption);
				DiaOption diaOption2 = new DiaOption(Translator.Translate(abasia ? "WandererPodJoinReject" : "WandererJoinReject"));
				diaOption2.action = delegate ()
				{
					Find.WorldPawns.PassToWorld(pawn, RimWorld.Planet.PawnDiscardDecideMode.KeepForever);
				};
				diaOption2.resolveTree = true;
				diaNode.options.Add(diaOption2);
				string text = TranslatorFormattedStringExtensions.Translate("WandererJoinTitle", map.Parent.Label);
				Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, true, text));
				__result = true;
				return false;
			}
			return true;
		}
		
		public static void VEE_WarnderJoinsTraitorPatch()
		{
			harmony.Patch(AccessTools.Method(GenTypes.GetTypeInAnyAssembly("VEE.RegularEvents.WandererJoinTraitor", "VEE.RegularEvents"), "TryExecuteWorker", null, null),
				new HarmonyMethod(typeof(Main).GetMethod("VEE_WarnderJoinsTraitorPrefix")), null);
		}

		public static bool VEE_WarnderJoinsTraitorPrefix(ref bool __result, ref IncidentWorker_WandererJoin __instance, IncidentParms parms)
		{
			Log.Message("WJP_IncidentWorker checking " + __instance.def.defName);
			if (__instance.def.defName.Contains("WandererJoin") && !__instance.def.defName.Contains("ManInBlack"))
			{
				Map map = (Map)parms.target;
				if (!map.mapPawns.AnyColonistSpawned)
				{
					return true;
				}
				IntVec3 loc;
				if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 c) => map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Neutral, out loc))
				{
					__result = false;
				}
				Gender? gender = null;
				if (__instance.def.pawnFixedGender != Gender.None)
				{
					gender = new Gender?(__instance.def.pawnFixedGender);
				}
				PawnKindDef pawnKind = __instance.def.pawnKind;
				Faction ofPlayer = Faction.OfPlayer;
				bool pawnMustBeCapableOfViolence = __instance.def.pawnMustBeCapableOfViolence;
				Gender? gender2 = gender;
				float num = (float)typeof(IncidentWorker_WandererJoin).GetField("RelationWithColonistWeight", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
				PawnGenerationRequest pawnGenerationRequest = new PawnGenerationRequest(pawnKind, ofPlayer, (PawnGenerationContext)2, -1, true, false, false, false, true, pawnMustBeCapableOfViolence, num, false, true, true, false, false, false, false, false);
				Pawn pawn = PawnGenerator.GeneratePawn(pawnGenerationRequest);
				TaggedString ttexta;
				TaggedString tlabela = __instance.def.letterLabel.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN", true);
				DiaNode diaNode;
				bool abasia = __instance.def.defName == "WandererJoinAbasia";
				if (abasia)
				{
					CheckHediff(__instance, pawn);
					CheckHediff2(__instance, pawn, ref ttexta);
					diaNode = new DiaNode(CreateTransportPodText(__instance, pawn));
				}
				else
				{
					ttexta = __instance.def.letterText.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN", true);
					diaNode = new DiaNode(CreateWandererText(pawn));
				}
				DiaOption diaOptionDetails = new DiaOption(Translator.Translate("ClickForMoreInfo"));
				diaOptionDetails.action = delegate ()
				{
					Find.WindowStack.Add(new Dialog_InfoCard(pawn));
				};
				diaNode.options.Add(diaOptionDetails);
				DiaOption diaOption = new DiaOption(Translator.Translate(abasia ? "WandererPodJoinAccept" : "WandererJoinAccept"));
				PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref ttexta, ref tlabela, pawn);
				GenPersonStatText.AppendStats(ref ttexta, pawn);
				diaOption.action = delegate ()
				{
					GenSpawn.Spawn(pawn, loc, map, 0);
					Find.LetterStack.ReceiveLetter(tlabela, ttexta, LetterDefOf.PositiveEvent, pawn, null, null);
				};
				diaOption.resolveTree = true;
				diaNode.options.Add(diaOption);
				DiaOption diaOption2 = new DiaOption(Translator.Translate(abasia ? "WandererPodJoinReject" : "WandererJoinReject"));
				diaOption2.action = delegate ()
				{
					Find.WorldPawns.PassToWorld(pawn, RimWorld.Planet.PawnDiscardDecideMode.KeepForever);
				};
				diaOption2.resolveTree = true;
				diaNode.options.Add(diaOption2);
				string text = TranslatorFormattedStringExtensions.Translate("WandererJoinTitle", map.Parent.Label);
				Find.WindowStack.Add(new Dialog_NodeTree(diaNode, true, true, text));
				__result = true;
				return false;
			}
			return true;
		}

		// Token: 0x0600000D RID: 13 RVA: 0x00002EE8 File Offset: 0x000010E8
		private static string CreateWandererText(Pawn wanderer)
		{
			TaggedString text = TranslatorFormattedStringExtensions.Translate("WandererJoinDesc", wanderer.NameFullColored, wanderer.NameShortColored, wanderer.story.Title.ToLower(), wanderer.ageTracker.AgeBiologicalYears.ToString(), wanderer.def.label).Formatted(wanderer.Named("PAWN")).AdjustedFor(wanderer, "PAWN", true);
			if (wanderer.gender != Gender.None)
			{
				text += " " + wanderer.gender.GetLabel();
			}
			text += ".";
			PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, wanderer);
			GenPersonStatText.AppendStats(ref text, wanderer);
			return text;
		}
		public static void CheckHediff(IncidentWorker __instance, Pawn pawn)
		{
			if (__instance.def.pawnHediff != null)
			{
				pawn.health.AddHediff(__instance.def.pawnHediff, null, null, null);
			}
		}
		public static void CheckHediff2(IncidentWorker __instance, Pawn pawn, ref TaggedString outstring)
		{
			outstring = (__instance.def.pawnHediff != null) ? __instance.def.letterText.Formatted(pawn.Named("PAWN"), __instance.def.pawnHediff.Named("HEDIFF")).AdjustedFor(pawn, "PAWN", true) : __instance.def.letterText.Formatted(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN", true);

		}
		private static string CreateTransportPodText(IncidentWorker __instance, Pawn wanderer)
		{
			TaggedString text = TranslatorFormattedStringExtensions.Translate("WandererPodJoinDesc", wanderer.NameFullColored, wanderer.NameShortColored, wanderer.story.Title.ToLower(), wanderer.ageTracker.AgeBiologicalYears.ToString(), wanderer.def.label, __instance.def.pawnHediff.label).Formatted(wanderer.Named("PAWN"), __instance.def.pawnHediff.Named("HEDIFF")).AdjustedFor(wanderer, "PAWN", true);
			if (wanderer.gender != Gender.None)
			{
				text += " " + wanderer.gender.GetLabel();
			}
			text += ".";
			PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, wanderer);
			GenPersonStatText.AppendStats(ref text, wanderer);
			return text;
		}
	}

}
