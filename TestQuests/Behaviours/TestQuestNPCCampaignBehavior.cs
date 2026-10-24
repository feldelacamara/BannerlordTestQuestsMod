using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;

namespace TestQuests.Behaviours
{
    internal class TestQuestNPCCampaignBehavior : CampaignBehaviorBase
    {
        private Hero _spawnedNPC;

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new System.Action<CampaignGameStarter>(this.SpawnEvent));
            CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, new System.Action<Dictionary<string, int>>(this.PlaceInTavern));
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData<Hero>("TestQuestSpawnedNPC1", ref this._spawnedNPC);
        }

        public void SpawnEvent(CampaignGameStarter gameStarter)
        {
            if (_spawnedNPC != null) if (_spawnedNPC.IsAlive) return;
            InformationManager.DisplayMessage(new InformationMessage("The bun is in your mind"));
            CharacterObject _spawnedNPCTemplate = MBObjectManager.Instance.GetObject<CharacterObject>("testquest_npc1");
            if(_spawnedNPCTemplate == null)
            {
                InformationManager.DisplayMessage(new InformationMessage("Error loading NPC template, check if testquestnpcs.xml is loaded correctly"));
                return;
            }
            Town randomTown = Town.AllTowns.GetRandomElementWithPredicate((Town x) => x.Culture == _spawnedNPCTemplate.Culture);
            //Settlement birthTown = (randomTown != null) ? randomTown.Settlement : null;
            //if (birthTown == null) { birthTown = Town.AllTowns.GetRandomElement<Town>().Settlement; }
            Settlement birthTown = (randomTown != null) ? randomTown.Settlement : Town.AllTowns.GetRandomElement<Town>().Settlement;
            birthTown = Settlement.Find("town_ES3");
            _spawnedNPC = HeroCreator.CreateSpecialHero(_spawnedNPCTemplate, birthTown, null, null, -1);
            _spawnedNPC.SetName(new TextObject("{=TQNPC1Name}Claude Lastname"), new TextObject("{=TQNPC1First}Claude"));
            _spawnedNPC.ChangeState(Hero.CharacterStates.Active);
            EnterSettlementAction.ApplyForCharacterOnly(_spawnedNPC, birthTown);
            InformationManager.DisplayMessage(new InformationMessage($"{_spawnedNPC.GetName()} is at {birthTown.GetName()}"));
        }

        public void PlaceInTavern(Dictionary<string, int> unusedUsablePointCount)
        {
            Settlement settlement = PlayerEncounter.LocationEncounter.Settlement;
            if(_spawnedNPC != null && settlement.IsTown && settlement.StringId == _spawnedNPC.CurrentSettlement.StringId && CampaignMission.Current != null)
            {
                Location location = CampaignMission.Current.Location;
                if (location != null && location.StringId == "tavern")
                {
                    int num;
                    if (unusedUsablePointCount.TryGetValue("npc_common", out num) && num > 0)
                    {
                        Location heroLoc = settlement.LocationComplex.GetLocationOfCharacter(_spawnedNPC);
                        if (heroLoc != null && heroLoc.StringId != "tavern")
                        {
                            LocationCharacter locationCharacterOfHero = settlement.LocationComplex.GetLocationCharacterOfHero(_spawnedNPC);
                            settlement.LocationComplex.ChangeLocation(locationCharacterOfHero, heroLoc, location);
                            InformationManager.DisplayMessage(new InformationMessage("Frylock and I'm on top"));
                        }
                    }
                }
            }
        }
    }
}
