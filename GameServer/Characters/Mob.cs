using GameServer.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Characters
{
    internal class Mob
    {
        int _id;
        string? _name;
        int _health;
        int _attack;
        int _defense;

        public Mob(int id, string? name, int health, int attack, int deffence)
        {
            Id = id;
            Name = name;
            Health = health;
            Attack = attack;
            Deffence = deffence;            
        }

        public int Id { get => _id; set { _id = value; } }
        public string? Name
        {
            get => _name;
            set { _name = value != null ? value : $"Enemy{Id}"; }
        }

        public int Health { get => _health; set { _health = value; } }
        public int Attack { get => _attack; set { _attack = value; } }
        public int Deffence { get => _defense; set { _defense = value; } }


        public override string ToString()
        {
            return $"{Name}\t\t  Health: {Health}  Att: {Attack}  Deff: {Deffence}";
        }
    }
}
