using GameServer.Weapons;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace GameServer.Characters
{
    public class User
    {
        int _id;
        string? _name;
        int _health;
        int _attack;
        int _defense;
        bool _isAlive;
        int _experiance;
        int _level;
        int _targetId;
        Weapon? _weapon;

        public User(int id, Socket socket, string? name = null)
        {
            this._id = id;
            Name = name;
            _health = 100;
            _attack = 10;
            _defense = 1;
            _experiance = 0;
            _level = 1;
            _weapon = null;
            UserSocket = socket;
            _isAlive = true;
        }

        public int Id => _id;
        public string? Name
        {
            get => _name;
            set { _name = value != null? value : $"player{_id}"; }
        }
        
        public int Health { get => _health; set { _health = value; } }
        public int Attack { get => _attack; set { _attack = value; } }
        public int Deffence { get => _defense; set { _defense = value; } }
        public bool IsAlive { get => _isAlive; set { _isAlive = value; } }
        public int Experiance {
            get => _experiance;
            set
            {
                if(_experiance + value >= 50)
                {
                    _experiance = _experiance + value - 50;
                    Level++;
                }
                else
                {
                    _experiance += value;
                }
                
            }
        }
        public int Level 
        {
            get => _level;
            set
            {
                if(_level > 0)
                {
                    _health += 10;
                    _attack += 10;
                    _defense += 10;
                }

            } 
        }

        public int TargetId { get => _targetId; set => _targetId = value; }

        public Weapon? Weapon
        { 
            get => _weapon;
            set
            {
                if(value != null)
                {
                    _weapon = value;
                    Health += _weapon.HealthBuff;
                    Attack += _weapon.AttackBuff;
                    Deffence += _weapon.DeffenceBuff;
                }
                else
                {
                    if(_weapon != null)
                    {
                        Health -= _weapon.HealthBuff;
                        Attack -= _weapon.AttackBuff;
                        Deffence -= _weapon.DeffenceBuff;
                        _weapon = null;
                    }
                }
            }                
        }

        [JsonIgnore]
        public Socket UserSocket { get; set; }

        public override string ToString()
        {
            return $"{Name}\tlvl:{Level}  Health:{Health}  Att:{Attack}  Deff:{Deffence}  Weapon:{Weapon?.ToString()?? "no weapon"}";
        }

        public string AttackTheEnemy(Mob mob)
        {
            if (mob == null || !mob.IsAlive) return $"Соперник {mob?.Name} уже мертв";

            int damage = Attack - mob.Deffence;            

            if (damage > 0)
            {
                mob.Health -= damage;
                Experiance += damage;
            }                
            else return $"{Name} не смог пробить броню {mob.Name}";

            if (mob.Health > 0) return $"{Name} нанес {damage} урона {mob.Name}";

            mob.IsAlive = false;
            return $"{Name} уничтожил {mob.Name}";
        }

        public string DropWeapon(User user)
        {
            if(Weapon == null) return $"{Name} нет оружия для передачи";

            if (user == null || !user.IsAlive) return $"{Name} ошибся при выборе адресата передачи оружия";

            user.Weapon = Weapon;
            Weapon = null;

            return $"{Name} передал {user.Weapon.Name} игроку {user.Name}";
        }
    }
}
