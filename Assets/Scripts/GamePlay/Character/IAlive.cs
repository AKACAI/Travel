using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightGame.Character
{
    public interface IAlive
    {
        bool IsAlive { get; }
        int CurHp { get; }
        void OnHurt(int damage);
        void OnDead();
    }
}
