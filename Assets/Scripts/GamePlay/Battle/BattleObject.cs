using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FightGame.Battle.BattleCfg;

namespace FightGame.Battle
{
    public class BattleObject
    {
        public int mFaction = FactionConst.NONE;
        public event Action<BattleObject, BattleObject> OnContactOtherFaction;
        public event Action<BattleObject, BattleObject> OnBattleTriggerEnter;

        protected List<BattleObjectComponentBase> mBattleObjectComps;

        public BattleObject(int faction)
        {
            mFaction = faction;
            mBattleObjectComps = new List<BattleObjectComponentBase>();
        }

        public void AddBattleObjectComponent(BattleObjectComponentBase comp)
        {
            comp.Init(this);
            mBattleObjectComps.Add(comp);
        }

        public void RemoveBattleObjectComponent(BattleObjectComponentBase comp)
        {
            mBattleObjectComps.Remove(comp);
        }

        public void OnTriggerEnter(BattleObject otherBattleObject)
        {
            if (otherBattleObject == null) return;
            if (otherBattleObject.mFaction != mFaction)
            {
                if (OnContactOtherFaction != null)
                {
                    OnContactOtherFaction(this, otherBattleObject);
                }
            }
            if (OnBattleTriggerEnter != null)
            {
                OnBattleTriggerEnter(this, otherBattleObject);
            }
        }

        public T GetBattleObjectComponent<T>() where T : BattleObjectComponentBase
        {
            var result = default(T);
            for (int i = 0; i < mBattleObjectComps.Count; i++)
            {
                var comp = mBattleObjectComps[i];
                if (comp != null && comp.GetType() == typeof(T))
                {
                    result = comp as T;
                    break;
                }
            }
            return result;
        }
    }
}
