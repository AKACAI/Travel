using FightGame.Character;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightGame.Battle
{
    public class AliveBattleComponent : BattleObjectComponentBase
    {
        private IAlive aliveUnit;

        public AliveBattleComponent(IAlive aliveUnit)
        {
            this.aliveUnit = aliveUnit;
        }

        protected override void OnInit(BattleObject battleObject)
        {
            base.OnInit(battleObject);
        }

        public void OnSufferAttack(BattleObject other, DamageBattleComponent damegeComp)
        {
            if (aliveUnit != null)
            {
                aliveUnit.OnHurt(damegeComp.toEnemyDamage);
            }
        }
    }
}
