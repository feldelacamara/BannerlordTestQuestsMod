using TestQuests.Behaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace TestQuests
{
    internal class MySubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            try
            {
                base.OnGameStart(game, gameStarterObject);
                if (!(game.GameType is Campaign))
                {
                    return;
                }
                //AddBehaviors(gameStarterObject as CampaignGameStarter);
            }
            catch { }
        }

        private void AddBehaviors(CampaignGameStarter gameStarterObject)
        {
            if(gameStarterObject != null)
            {
                gameStarterObject.AddBehavior(new SimpleDeliveryQuest());
            }
        }
    }

}
