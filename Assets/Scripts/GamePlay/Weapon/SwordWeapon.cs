using FightGame.Battle;
using FightGame.Character;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightGame.Weapon
{
    public class SwordWeapon : IWeapon
    {
        private int mWeaponId = 0;
        private string mWeaponName = String.Empty;
        private CharacterBase mHolder = null;
        private WeaponAttribute mBaseAttribute = null;
        private WeaponAttribute? mExtraAttribute = null;

        public int WeaponId => mWeaponId;
        public string WeaponName => mWeaponName;
        public CharacterBase Holder => mHolder;
        public WeaponAttribute BaseAttribute => mBaseAttribute;
        public WeaponAttribute ExtraAttribute => mExtraAttribute;


        public SwordWeapon(int id, string name, CharacterBase holder, WeaponAttribute baseAttr, WeaponAttribute extraAttr = null)
        {
            mWeaponId = id;
            mWeaponName = name;
            mHolder = holder;
            mBaseAttribute = baseAttr;
            mExtraAttribute = extraAttr;
        }

        void IWeaponBehavious.HeavyAttack()
        {
            BattleObject battleObject = new BattleObject(Holder.Faction);
            DamageBattleComponent damageBattleComponent = new DamageBattleComponent(mBaseAttribute.Attack * 2);
            battleObject.AddBattleObjectComponent(damageBattleComponent);
            battleObject.OnTriggerEnter(Test.MoodObject.CharacterBattleObject);
        }

        void IWeaponBehavious.LightAttack()
        {
            BattleObject battleObject = new BattleObject(Holder.Faction);
            DamageBattleComponent damageBattleComponent = new DamageBattleComponent(mBaseAttribute.Attack);
            battleObject.AddBattleObjectComponent(damageBattleComponent);
            battleObject.OnTriggerEnter(Test.MoodObject.CharacterBattleObject);
        }

        void IWeaponBehavious.OnEquip()
        {
            Console.WriteLine("剑来");
        }

        void IWeaponBehavious.OnUnequip()
        {

        }

        void IWeaponBehavious.WeaponSkill()
        {
            BattleObject battleObject = new BattleObject(Holder.Faction);
            DamageBattleComponent damageBattleComponent = new DamageBattleComponent(mBaseAttribute.Attack * 5);
            battleObject.AddBattleObjectComponent(damageBattleComponent);
            battleObject.OnTriggerEnter(Test.MoodObject.CharacterBattleObject);
        }
    }
}
