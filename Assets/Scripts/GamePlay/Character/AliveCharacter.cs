using FightGame.Battle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace FightGame.Character
{
    public class AliveCharacter : CharacterBase, IAlive
    {
        protected bool mIsAlive = true;
        protected int mCurHp = 0;
        public bool IsAlive { get { return mIsAlive; } }
        public int CurHp { get => mCurHp; }

        public AliveCharacter(CharacterAttribute baseAttr, CharacterBaseInfo baseInfo, int faction) : base(baseAttr, baseInfo, faction)
        {
            mIsAlive = true;
            mCurHp = baseAttr.Hp;
            AliveBattleComponent aliveComp = new AliveBattleComponent(this);
            mBattleObject.AddBattleObjectComponent(aliveComp);
        }

        public void OnHurt(int damage)
        {
            if (!IsAlive)
            {
                return;
            }
            mCurHp -= damage;
            mCurHp = Math.Max(mCurHp, 0);
            Console.WriteLine($"{mBaseInfo.Name}：受到伤害{damage}点，当前生命值{mCurHp}");
            if (mCurHp <= 0)
            {
                OnDead();
            }
        }

        public void OnDead()
        {
            if (!IsAlive)
            {
                return;
            }
            mIsAlive = false;
            Console.WriteLine($"{mBaseInfo.Name}已阵亡");
        }
    }
}
