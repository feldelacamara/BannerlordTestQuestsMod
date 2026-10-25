using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Party;
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
            //Event listener for checking if the NPC is spawned / spawning if not
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new System.Action<CampaignGameStarter>(this.SpawnEvent));
            //Event listener that moves the NPC into the tavern list of the town instead of the keep list the game wants it to spawn in. Called every time an overworld menu is loaded
            //Replaced OnMissionEnded/OnGameLoaded because the game’s own refresh would run after ours and undo the move (newest listener runs first) 
            CampaignEvents.GameMenuOpened.AddNonSerializedListener(this, new Action<TaleWorlds.CampaignSystem.GameMenus.MenuCallbackArgs>(this.OnGameMenuOpened));
            //Safety net to ensure NPC is moved to tavern when the player enters a scene in town, in case the last event failed
            CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, new System.Action<Dictionary<string, int>>(this.EnterScene));
        }

        public override void SyncData(IDataStore dataStore)
        {
            dataStore.SyncData<Hero>("TestQuestSpawnedNPC1", ref this._spawnedNPC);
        }

        public void SpawnEvent(CampaignGameStarter gameStarter)
        {
            if (_spawnedNPC != null) if (_spawnedNPC.IsAlive) return; //If NPC is dead, spawn a new one
            CharacterObject spawnedNPCTemplate = MBObjectManager.Instance.GetObject<CharacterObject>("testquest_npc1");
            if(spawnedNPCTemplate == null)
            {
                InformationManager.DisplayMessage(new InformationMessage("Error loading NPC template, check if testquestnpcs.xml is loaded correctly"));
                return;
            }
            Town randomTown = Town.AllTowns.GetRandomElementWithPredicate((Town x) => x.Culture == spawnedNPCTemplate.Culture);
            Settlement birthTown = (randomTown != null) ? randomTown.Settlement : Town.AllTowns.GetRandomElement<Town>().Settlement;
            //Fixed town for testing, to be replaced when NPC wandering mechanic is added
            birthTown = Settlement.Find("town_ES3");
            if(birthTown == null)
            {
                InformationManager.DisplayMessage(new InformationMessage("Error: town_ES3 not found, aborting TestQuests spawn event"));
                return;
            }
            _spawnedNPC = HeroCreator.CreateSpecialHero(spawnedNPCTemplate, birthTown, null, null, -1);
            _spawnedNPC.SetName(new TextObject("{=TQNPC1Name}Claude Lastname"), new TextObject("{=TQNPC1First}Claude"));
            _spawnedNPC.ChangeState(Hero.CharacterStates.Active);
            EnterSettlementAction.ApplyForCharacterOnly(_spawnedNPC, birthTown);
            //Temporary debug line to ensure the NPC spawn feature works correctly. To be removed
#if DEBUG
            InformationManager.DisplayMessage(new InformationMessage($"{_spawnedNPC.GetName()} is at {birthTown.GetName()}"));
#endif
        }

        public void EnterScene(Dictionary<string, int> unusedUsablePointCount)
        {
            EnsureInTavern();
        }

        public void OnGameMenuOpened(TaleWorlds.CampaignSystem.GameMenus.MenuCallbackArgs args)
        {
            EnsureInTavern();
        }
        //The code that moves the NPC into the town's tavern
        public bool EnsureInTavern()
        {
            Settlement settlement = Settlement.CurrentSettlement;
            if (settlement == null || settlement.LocationComplex != LocationComplex.Current) return false; //LocationComplex stores the towns different locations, as well as the NPCs in each location
            if (_spawnedNPC != null && _spawnedNPC.IsAlive && settlement.IsTown && settlement == _spawnedNPC.CurrentSettlement)
            {
                Location tavernLoc = settlement.LocationComplex.GetLocationWithId("tavern");
                LocationCharacter locationCharacterOfHero = settlement.LocationComplex.GetLocationCharacterOfHero(_spawnedNPC);//The location character is the version of the NPC that spawns in the scene
                if (locationCharacterOfHero == null) return false;
                locationCharacterOfHero.SpecialTargetTag = "npc_common"; //Makes sure the NPC will spawn where a regular commoner would spawn
                if (tavernLoc != null)
                {
                    Location heroLoc = settlement.LocationComplex.GetLocationOfCharacter(_spawnedNPC);
                    if (heroLoc != null && heroLoc.StringId != "tavern")
                    {
                        settlement.LocationComplex.ChangeLocation(locationCharacterOfHero, heroLoc, tavernLoc);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
