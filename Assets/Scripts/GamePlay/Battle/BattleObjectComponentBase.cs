using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightGame.Battle
{
    public class BattleObjectComponentBase
    {
        protected BattleObject mBattleObject;

        public void Init(BattleObject battleObject)
        {
            if (mBattleObject != null)
            {
                return;
            }
            mBattleObject = battleObject;
            OnInit(battleObject);
        }

        protected virtual void OnInit(BattleObject battleObject)
        {

        }
    }
}
