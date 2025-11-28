using FightGame.Character;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightGame.Weapon
{
    public interface IWeapon : IWeaponBehavious
    {
        int WeaponId { get; }
        string WeaponName { get; }
        WeaponAttribute BaseAttribute { get; }
        WeaponAttribute ExtraAttribute { get; }
        CharacterBase Holder { get; }
        //GameObject WeaponPrefab { get; }
    }
}
