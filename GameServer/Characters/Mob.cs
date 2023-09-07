using GameServer.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Characters
{
    public class Mob
    {
        int _id;
        string? _name;
        int _health;
        int _attack;
        int _defense;
        bool _isAlive;

        public Mob(int id, string? name, int health, int attack, int deffence)
        {
            Id = id;
            Name = name;
            Health = health;
            Attack = attack;
            Deffence = deffence;
            _isAlive = true;
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
        public bool IsAlive { get => _isAlive; set { _isAlive = value; } }


        public override string ToString()
        {
            return $"{Name}\t\t  Health: {Health}  Att: {Attack}  Deff: {Deffence}";
        }

        public string AttackTheUser(User user)
        {
            if (user == null || !user.IsAlive) return $"Игрок {user?.Name} уже мертв";

            int damage = Attack - user.Deffence;

            if (damage > 0)
                user.Health -= damage;
            else
                return $"{Name} не смог пробить броню игрока {user.Name}";

            if (user.Health > 0) return $"{Name} нанес {damage} урона игроку {user.Name}";

            user.IsAlive = false;
            return $"{Name} уничтожил игрока {user.Name}";
        }
    }
}
