using System;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace WandererJoinPlus
{
	public static class GenPersonStatText
	{
		public static void AppendStats(ref TaggedString text, Pawn pawn, bool raceInfo = false)
		{
			StringBuilder sb = new StringBuilder(text);
			GenPersonStatText.AppendDisabledWorkTags(sb, pawn.story.DisabledWorkTagsBackstoryAndTraits);
			GenPersonStatText.AppendTraits(sb, pawn.story.traits.allTraits);
			GenPersonStatText.AppendPassions(sb, pawn.skills.skills);
            if (raceInfo)
			{
				GenPersonStatText.AppendRaceInfo(sb, pawn);
			}
			sb.AppendLine();
			sb.AppendLine();
			text = sb.ToString();
		}

		public static void AppendRaceInfo(StringBuilder sb, Pawn pawn)
		{
			sb.AppendLine();
			sb.AppendLine();
			sb.AppendLine(TranslatorFormattedStringExtensions.Translate("RaceInfo", pawn.def.LabelCap));
		}

		private static void AppendDisabledWorkTags(StringBuilder sb, WorkTags disabledWorkTags)
		{
			if (disabledWorkTags == WorkTags.None)
			{
				return;
			}
			sb.AppendLine();
			sb.AppendLine();
			sb.Append(GenPersonStatText.CreateTitle(Translator.Translate("IncapableOf")));
			sb.AppendLine();
			int num = 0;
			bool flag = true;
			foreach (object obj in Enum.GetValues(typeof(WorkTags)))
			{
				WorkTags workTags = (WorkTags)obj;
				if (workTags != WorkTags.None && (disabledWorkTags & workTags) == workTags)
				{
					if (num > 0)
					{
						sb.Append(", ");
					}
					if (!flag)
					{
						sb.Append(WorkTypeDefsUtility.LabelTranslated(workTags).ToLower());
					}
					else
					{
						sb.Append(WorkTypeDefsUtility.LabelTranslated(workTags));
					}
					num++;
					flag = false;
				}
			}
		}

		private static void AppendTraits(StringBuilder sb, List<Trait> traits)
		{
			sb.AppendLine();
			sb.AppendLine();
			sb.Append(GenPersonStatText.CreateTitle(Translator.Translate("Traits")));
			if (traits.Count == 0)
			{
				sb.AppendLine();
				sb.Append("(" + Translator.Translate("NoneLower") + ")");
				return;
			}
			foreach (Trait trait in traits)
			{
				sb.AppendLine();
				sb.Append(trait.LabelCap);
			}
		}

		private static void AppendPassions(StringBuilder sb, List<SkillRecord> skills)
		{
			if (skills.Count == 0)
			{
				return;
			}
			sb.AppendLine();
			sb.AppendLine();
			sb.Append(GenPersonStatText.CreateTitle(Translator.Translate("PassionateFor")));
			sb.AppendLine();
			int num = 0;
			foreach (SkillRecord skillRecord in skills)
			{
				if (skillRecord.passion > 0)
				{
					if (num > 0)
					{
						sb.Append(", ");
					}
					string str = "PassionateMajor".Translate();

					sb.Append(skillRecord.def.skillLabel + ((skillRecord.passion == (Passion)2) ? str : ""));
					num++;
				}
			}
		}

		private static string CreateTitle(string title)
		{
			return string.Concat(new object[]
			{
				"<b><size=",
				TITLE_SIZE,
				">",
				title,
				"</size></b>"
			});
		}

		private const int TITLE_SIZE = 16;
	}
}
