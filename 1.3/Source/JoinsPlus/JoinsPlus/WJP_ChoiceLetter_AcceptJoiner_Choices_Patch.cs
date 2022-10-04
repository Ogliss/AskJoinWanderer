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
//	[HarmonyPatch(typeof(ChoiceLetter_AcceptJoiner), "get_Choices")]
	public static class WJP_ChoiceLetter_AcceptJoiner_Choices_Patch
	{
		public static IEnumerable<DiaOption> Postfix(IEnumerable<DiaOption> __result, ChoiceLetter_AcceptJoiner __instance)
		{
			if (__instance.ArchivedOnly)
			{
				yield return __instance.Option_Close;
			}
			else
			{
				Pawn pawn = __instance.lookTargets?.PrimaryTarget.Thing as Pawn;
				if (pawn != null)
				{
					DiaOption diaOptionDetails = new DiaOption(Translator.Translate("ClickForMoreInfo") + " about " + pawn.NameShortColored);
					diaOptionDetails.action = delegate ()
					{
						Find.WindowStack.Add(new Dialog_InfoCard(pawn));
					};
					yield return diaOptionDetails;
				}
				foreach (var item in __result)
				{
					yield return item;
				}
			}
			yield break;
		}

	}
}
