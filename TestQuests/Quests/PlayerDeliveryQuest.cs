using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using TaleWorlds.SaveSystem;

namespace TestQuests.Quests
{
    internal class PlayerDeliveryQuest : QuestBase
    {
        [SaveableField(1)] public int defaultreward;
        [SaveableField(2)] public Hero _target;
        [SaveableField(3)] public bool _questrunning;
        public PlayerDeliveryQuest(string questId, Hero questGiver, CampaignTime duration, int rewardGold) : base("SimpleDeliveryQuest", questGiver, duration, rewardGold)
        {
            defaultreward = rewardGold;
        }

        public override TextObject Title => throw new NotImplementedException();

        public override bool IsRemainingTimeHidden => throw new NotImplementedException();

        protected override void InitializeQuestOnGameLoad()
        {
            throw new NotImplementedException();
        }

        protected override void SetDialogs()
        {
            throw new NotImplementedException();
        }
    }
}
