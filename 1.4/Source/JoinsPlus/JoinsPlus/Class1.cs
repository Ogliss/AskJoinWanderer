using RimWorld;
using System.Collections.Generic;
using Verse;

namespace WandererJoinPlus
{
	// WandererJoinPlus.ChoiceLetter_AcceptJoinerPlus
	class ChoiceLetter_AcceptJoinerPlus : ChoiceLetter_AcceptJoiner
    {
		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_References.Look<Pawn>(ref this.joiner, "joiner", false);
		}
		public Pawn joiner;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
				Pawn pawn = joiner;
				if (pawn != null)
				{
					DiaOption diaOptionDetails = new DiaOption(Translator.Translate("ClickForMoreInfo") + " about " + pawn.NameShortColored);
					diaOptionDetails.action = delegate ()
					{
						Find.WindowStack.Add(new Dialog_InfoCard(pawn));
					};
					yield return diaOptionDetails;
				}
				foreach (var item in base.Choices)
				{
					yield return item;
				}
			}
        }
    }
}
