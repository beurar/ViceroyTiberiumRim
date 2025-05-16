using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace TiberiumRim
{
    public class EventLetter : StandardLetter
    {
        private List<EventDef> events = new List<EventDef>();


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref events, "events");
        }

        public override void Received()
        {
            base.Received();
        }

        public void AddEvents(List<EventDef> events)
        {
            this.events.AddRange(events);
        }

        public void AddEvent(EventDef eventDef)
        {
            events.Add(eventDef);
        }

        private void CloseLetter()
        {
            if (Find.LetterStack.LettersListForReading.Contains(this))
                Find.LetterStack.RemoveLetter(this);
            else
                Log.Warning("[TiberiumRim] Tried to remove EventLetter that was not in stack.");
        }


        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                foreach (var choice in base.Choices)
                    yield return choice;

                // Check safely before assuming
                var validEvent = events?.FirstOrDefault(e => !e.unlocksResearch.NullOrEmpty());
                if (validEvent?.unlocksResearch != null && validEvent.unlocksResearch.Count > 0)
                {
                    yield return new DiaOption("TR_OpenTab".Translate())
                    {
                        action = delegate
                        {
                            Find.MainTabsRoot.SetCurrentTab(TiberiumDefOf.TiberiumTab);
                            var researchWindow = (MainTabWindow_TibResearch)Find.MainTabsRoot.OpenTab.TabWindow;
                            researchWindow.SelTab = ResearchTabOption.Projects;
                            researchWindow.SelProject = validEvent.unlocksResearch.First();

                            var manager = TRUtils.ResearchManager();
                            if (!manager.IsOpen(validEvent.unlocksResearch.First().ParentGroup))
                                manager.OpenClose(validEvent.unlocksResearch.First().ParentGroup);

                            // TODO: Select event
                        }
                    };
                }
                else
                {
                    // Optional fallback so the dialog doesn't look empty
                    yield return new DiaOption("OK".Translate()) { action = () => CloseLetter() };
                    Log.Warning("[TiberiumRim] EventLetter: No valid research project found to unlock.");
                }
            }
        }
    }
}
