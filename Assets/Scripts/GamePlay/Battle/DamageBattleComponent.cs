using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightGame.Battle
{
    public class DamageBattleComponent : BattleObjectComponentBase
    {
        public int toEnemyDamage;

        public DamageBattleComponent(int toEnemyDamage)
        {
            this.toEnemyDamage = toEnemyDamage;
        }

        protected override void OnInit(BattleObject battleObject)
        {
            base.OnInit(battleObject);
            battleObject.OnContactOtherFaction += OnContackOtherFactionCb;
        }

        private void OnContackOtherFactionCb(BattleObject sender, BattleObject other)
        {
            var aliveComp = other.GetBattleObjectComponent<AliveBattleComponent>();
            if (aliveComp != null)
            {
                aliveComp.OnSufferAttack(sender, this);
            }
        }
    }
}
