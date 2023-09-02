using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Weapons
{
    internal interface IWeapon
    {
        string? Name { get; }
        int HealthBuff { get; set; }
        int AttackBuff { get; set; }
        int DeffenceBuff { get; set; }

        public string ToString()
        {
            return Name ?? "no name";
        }
    }
}
