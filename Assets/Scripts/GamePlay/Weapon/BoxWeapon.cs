using FightGame.Battle;
using FightGame.Character;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightGame.Weapon
{
    public class BoxWeapon : IWeapon
    {
        private int mWeaponId = 0;
        private string mWeaponName = String.Empty;
        private CharacterBase mHolder = null;
        private WeaponAttribute mBaseAttribute = null;
        private WeaponAttribute? mExtraAttribute = null;

        public int WeaponId => mWeaponId;
        public string WeaponName => mWeaponName;
        public WeaponAttribute BaseAttribute => mBaseAttribute;
        public WeaponAttribute ExtraAttribute => mExtraAttribute;
        public CharacterBase Holder => mHolder;

        public BoxWeapon(int id, string name, CharacterBase holder, WeaponAttribute baseAttr, WeaponAttribute extraAttr = null)
        {
            mWeaponId = id;
            mWeaponName = name;
            mHolder = holder;
            mBaseAttribute = baseAttr;
            mExtraAttribute = extraAttr;
        }

        void IWeaponBehavious.HeavyAttack()
        {
            Console.WriteLine("未解锁重击");
        }

        void IWeaponBehavious.LightAttack()
        {
            if (Holder == null)
            {
                return;
            }
            BattleObject battleObject = new BattleObject(Holder.Faction);
            DamageBattleComponent damageBattleComponent = new DamageBattleComponent(mBaseAttribute.Attack);
            battleObject.AddBattleObjectComponent(damageBattleComponent);
            battleObject.OnTriggerEnter(Test.MoodObject.CharacterBattleObject);
        }

        void IWeaponBehavious.OnEquip()
        {
            Console.WriteLine("赤手空拳");
        }

        void IWeaponBehavious.OnUnequip()
        {

        }

        void IWeaponBehavious.WeaponSkill()
        {
            BattleObject battleObject = new BattleObject(Holder.Faction);
            DamageBattleComponent damageBattleComponent = new DamageBattleComponent(mBaseAttribute.Attack * 3);
            battleObject.AddBattleObjectComponent(damageBattleComponent);
            battleObject.OnTriggerEnter(Test.MoodObject.CharacterBattleObject);
        }
    }
}
