using FightGame.Weapon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightGame.Character
{
    public interface IEquip
    {
        IWeapon CurWeapon { get; }
        bool ChangeWeapon(IWeapon tarWeapon);
    }
}
