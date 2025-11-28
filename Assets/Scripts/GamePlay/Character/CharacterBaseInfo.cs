using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FightGame.Character
{
    public class CharacterBaseInfo
    {
        public int Id;
        public int Sex;
        public int JobType;
        public string Name;

        public CharacterBaseInfo(int id, int sex, int jobType, string name)
        {
            Id = id;
            Sex = sex;
            JobType = jobType;
            Name = name;
        }
    }
}
