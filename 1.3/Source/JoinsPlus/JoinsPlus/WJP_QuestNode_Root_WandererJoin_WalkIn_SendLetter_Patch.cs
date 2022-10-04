using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using RimWorld.QuestGen;
using Verse;

namespace WandererJoinPlus
{
//	[HarmonyPatch(typeof(QuestNode_Root_WandererJoin_WalkIn), "SendLetter")]
	[HarmonyPatch]
	public static class WJP_QuestNode_SendLetter_Patch
	{
		
		[HarmonyTargetMethods]
		public static IEnumerable<MethodBase> PatchAllPlayerMethods()
		{
			IEnumerable<Type> types = typeof(QuestNode_Root_WandererJoin).AllSubclasses();
			string l = ("WJP found " + types.Count() + " WandererJoin QuestNode types");
			List<MethodInfo> methods = new List<MethodInfo>();
			foreach (Type item in types)
            {
				l += "\nchecking " + item.Name;
				MethodInfo m = AccessTools.Method(item, "SendLetter");

				if (m != null)
				{
					l += "\nPatching " + item.Name+" "+m.Name;
					yield return m as MethodBase;
				}
			}
			if (Prefs.DevMode)
			{
				Log.Message(l);
			}
			yield break;
		}

		// change to transpiler that swaps out the vanilla ChoiceLetter_AcceptJoiner for ChoiceLetter_AcceptJoinerPlus and sets its joiner
		public static bool Prefix(QuestNode_Root_WandererJoin __instance, Quest quest, Pawn pawn)
		{
            //	Log.Message("WJP_IncidentWorker checking "+ __instance.def.defName);
            if (__instance is QuestNode_Root_WandererJoin_WalkIn walkIn)
			{
				
				TaggedString label = "LetterLabelWandererJoins".Translate(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN", true);
				TaggedString text = "LetterWandererJoins".Translate(pawn.Named("PAWN")).AdjustedFor(pawn, "PAWN", true);
				QuestNode_Root_WandererJoin_WalkIn.AppendCharityInfoToLetter("JoinerCharityInfo".Translate(pawn), ref text);
				PawnRelationUtility.TryAppendRelationsWithColonistsInfo(ref text, ref label, pawn);
				GenPersonStatText.AppendStats(ref text, pawn);
				ChoiceLetter_AcceptJoinerPlus choiceLetter_AcceptJoiner = (ChoiceLetter_AcceptJoinerPlus)LetterMaker.MakeLetter(label, text, LetterDefOf.AcceptJoiner, null, null);
				choiceLetter_AcceptJoiner.signalAccept = walkIn.signalAccept;
				choiceLetter_AcceptJoiner.signalReject = walkIn.signalReject;
				choiceLetter_AcceptJoiner.quest = quest;
				choiceLetter_AcceptJoiner.StartTimeout(60000);
				choiceLetter_AcceptJoiner.joiner = pawn;
				Find.LetterStack.ReceiveLetter(choiceLetter_AcceptJoiner, null);
				return false;
				
			}
			return true;
		}
	}
}
