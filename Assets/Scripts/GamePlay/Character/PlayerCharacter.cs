using FightGame.Battle;
using FightGame.Weapon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightGame.Character
{
    public class PlayerCharacter : AliveCharacter, IEquip
    {
        protected IWeapon mCurWeapon;
        protected BoxWeapon mBoxWeapon;
        protected SwordWeapon mSwordWeapon;

        public IWeapon CurWeapon => mCurWeapon;

        public PlayerCharacter(CharacterAttribute baseAttr, CharacterBaseInfo baseInfo) : base(baseAttr, baseInfo, BattleCfg.FactionConst.PLAYER)
        {
            WeaponAttribute boxAttr = new WeaponAttribute(2, 1, 0, 0);
            mBoxWeapon = new BoxWeapon(1, "拳头", this, boxAttr);
            WeaponAttribute swordAttr = new WeaponAttribute(4, 5, 2, 0);
            mSwordWeapon = new SwordWeapon(2, "长剑", this, swordAttr);
            this.ChangeWeapon(mBoxWeapon);
        }

        public override void Update(float dt)
        {
            this.HandleInput();
        }

        protected void HandleInput()
        {
            ConsoleKeyInfo info = Console.ReadKey(true);
            if (info.KeyChar == ' ')
            {
                if (CurWeapon != null)
                {
                    CurWeapon.LightAttack();
                }
            }
            else if (info.KeyChar == '1')
            {
                ChangeWeapon(mBoxWeapon);
            }
            else if (info.KeyChar == '2')
            {
                ChangeWeapon(mSwordWeapon);
            }
        }


        public bool ChangeWeapon(IWeapon weapon)
        {
            // 卸载当前武器
            if (mCurWeapon != null)
            {
                if (mCurWeapon.WeaponId == weapon.WeaponId)
                {
                    return false;
                }
                mCurWeapon.OnUnequip();
            }

            // 装备新武器
            mCurWeapon = weapon;
            mCurWeapon.OnEquip();
            return true;
        }

        bool IEquip.ChangeWeapon(IWeapon tarWeapon)
        {
            throw new NotImplementedException();
        }
    }
}
