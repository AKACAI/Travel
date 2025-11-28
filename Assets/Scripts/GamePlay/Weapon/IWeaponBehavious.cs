using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightGame.Weapon
{
    public interface IWeaponBehavious
    {
        void LightAttack();
        void HeavyAttack();
        void WeaponSkill();
        void OnEquip();
        void OnUnequip();
    }
}
