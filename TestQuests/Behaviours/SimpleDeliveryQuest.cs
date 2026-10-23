using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace TestQuests.Behaviours
{
    internal class SimpleDeliveryQuest : CampaignBehaviorBase
    {
        public void OnSessionLaunched(CampaignGameStarter campaignGameStarter)
        {
            //new PlayerDeliveryQuest(Hero.MainHero).StartQuest();
        }

        public override void RegisterEvents()
        {
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnSessionLaunched));
        }

        public override void SyncData(IDataStore dataStore)
        {
            throw new NotImplementedException();
        }
    }
}
