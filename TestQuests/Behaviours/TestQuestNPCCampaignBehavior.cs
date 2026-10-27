using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Conversation;
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
        private const int GiftCost = 5;

        public override void RegisterEvents()
        {
            //Event listener for all the things that need to be done as soon as the game loads
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new System.Action<CampaignGameStarter>(this.OnSessionLaunched));
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

        private void OnSessionLaunched(CampaignGameStarter gameStarter)
        {
            //Function for checking if the NPC is spawned / spawning if not
            SpawnEvent(gameStarter);
            //Adds all of the npc's dialogue to the game's list
            AddDialogue(gameStarter);
        }
        private void SpawnEvent(CampaignGameStarter gameStarter)
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

        private void EnterScene(Dictionary<string, int> unusedUsablePointCount)
        {
            EnsureInTavern();
        }

        private void OnGameMenuOpened(TaleWorlds.CampaignSystem.GameMenus.MenuCallbackArgs args)
        {
            EnsureInTavern();
        }
        //The code that moves the NPC into the town's tavern
        private bool EnsureInTavern()
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

        private void AddDialogue(CampaignGameStarter campaignGameStarter)
        {
            campaignGameStarter.AddDialogLine("testQuestNPC_start_talk", "start", "testQuestNPC_answer1", "{?PLAYER.GENDER}M'lady{?}M'lord{\\?}, I ask you to pay me no mind. I'm afraid my troubles of late weigh too heavy on my mind to provide ye with good company.", new ConversationSentence.OnConditionDelegate(this.IsConversationAgentTestQuestNPC), null, 200, null);
            campaignGameStarter.AddPlayerLine("testQuestNPC_player_response1", "testQuestNPC_answer1", "testQuestNPC_Exit_Nice", "I understand too well friend. Here, have a drink on me. ({GOLD_GIFT} {GOLD_ICON})", new ConversationSentence.OnConditionDelegate(this.CanGiveNPCADrink), new ConversationSentence.OnConsequenceDelegate(this.GiveNPCADrink), 100, new ConversationSentence.OnClickableConditionDelegate(this.DoesPlayerHaveGold));
            campaignGameStarter.AddPlayerLine("testQuestNPC_player_response2", "testQuestNPC_answer1", "testQuestNPC_Exit_Rude", "Believe me churl, I want nothing of you or your paltry issues.", null, null, 100, null);
            campaignGameStarter.AddPlayerLine("testQuestNPC_player_response3", "testQuestNPC_answer1", "close_window", "I see, carry on then.", null, null, 100, null);
            campaignGameStarter.AddDialogLine("testQuestNPC_dialog1_nice", "testQuestNPC_Exit_Nice", "close_window", "You're too kind. May the heavens bless you {?PLAYER.GENDER}my lady{?}my lord{\\?}.", null, null, 100, null);
            campaignGameStarter.AddDialogLine("testQuestNPC_dialog1_rude", "testQuestNPC_Exit_Rude", "close_window", "(He mumbles under his breath) Foul {?PLAYER.GENDER}shrew{?}cur{\\?}.", null, null, 100, null);
        }
        private bool IsConversationAgentTestQuestNPC()
        {
            //needed for when the npc is not spawned and the character being talked to is not a hero
            return (_spawnedNPC != null) && (_spawnedNPC == Hero.OneToOneConversationHero);
        }
        private bool CanGiveNPCADrink()
        {
            MBTextManager.SetTextVariable("GOLD_GIFT", GiftCost);
            return true;
        }
        private void GiveNPCADrink()
        {
            GiveGoldAction.ApplyBetweenCharacters(Hero.MainHero, _spawnedNPC, GiftCost, false);
        }
        private bool DoesPlayerHaveGold(out TextObject explanation)
        {
            if (Hero.MainHero.Gold < GiftCost)
            {
                explanation = new TextObject("Broke ass", null);
                return false;
            }
            explanation = null;
            return true;
        }
    }

}
