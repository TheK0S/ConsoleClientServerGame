using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Weapons
{
    internal class Weapon
    {
        string? _name;
        int _healthBuff;
        int _attackBuff;
        int _deffenceBuff;

        public Weapon(string name, int healthBuff, int attackBuff, int deffenceBuff)
        {
            _name = name;
            _healthBuff = healthBuff;
            _attackBuff = attackBuff;
            _deffenceBuff = deffenceBuff;
        }

        public string? Name => _name;
        public int HealthBuff => _healthBuff;
        public int AttackBuff => _attackBuff;
        public int DeffenceBuff => _deffenceBuff;

        public override string ToString()
        {
            return Name ?? "no name";
        }
    }
}
