using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightGame.Weapon
{
    public class WeaponAttribute
    {
        public int Attack { get; set; }
        public int CostEnergy { get; set; }
        public int Weight { get; set; }
        public int CriticalRate { get; set; }


        public WeaponAttribute(int attack, int costEnergy, int weight, int criticalRate)
        {
            Attack = attack;
            CostEnergy = costEnergy;
            Weight = weight;
            CriticalRate = criticalRate;
        }

    }
}
