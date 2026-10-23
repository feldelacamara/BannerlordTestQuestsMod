/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;*/
using TestQuests.Quests;
using TaleWorlds.SaveSystem;

namespace TestQuests
{
    internal class TestQuestsSaveableTypeDefiner : SaveableTypeDefiner
    {
        public TestQuestsSaveableTypeDefiner() : base(5634132) //make sure base number is unique from other mods
        {
        }

        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(PlayerDeliveryQuest), 5634132);
        }
    }
}
