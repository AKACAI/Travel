using FightGame.Battle;
using FightGame.Engine;
using FightGame.Weapon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightGame.Character
{
    public class CharacterBase
    {
        protected int mFaction;
        protected CharacterAttribute mBaseAttribute;
        protected CharacterBaseInfo mBaseInfo;

        protected BattleObject mBattleObject;
        protected Animator mAnimator;

        public int Faction { get; }
        public BattleObject CharacterBattleObject { get { return mBattleObject; } }

        public CharacterBase(CharacterAttribute baseAttr, CharacterBaseInfo baseInfo, int faction)
        {
            mFaction = mFaction;
            mBaseAttribute = baseAttr;
            mBaseInfo = baseInfo;
            mBattleObject = new BattleObject(faction);
        }

        public virtual void Update(float dt)
        {

        }
    }
}
