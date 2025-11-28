using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightGame.Character
{
    public class CharacterAttribute
    {
        public int Hp { get; set; }
        public int Mp { get; set; }
        public int Energy { get; set; }
        public int BaseSpeed { get; set; }

        public CharacterAttribute(int hp, int mp, int energy, int baseSpeed)
        {
            Hp = hp;
            Mp = mp;
            Energy = energy;
            BaseSpeed = baseSpeed;
        }
    }
}
