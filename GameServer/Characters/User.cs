using GameServer.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Characters
{
    internal class User
    {
        int _id;
        string? _name;
        int _health;
        int _attack;
        int _defense;
        int _experiance;
        int _level;
        Weapon? _weapon;

        public User(int id, Socket socket, string? name = null)
        {
            this._id = id;
            Name = name;
            _health = 5;
            _attack = 1;
            _defense = 1;
            _experiance = 0;
            _level = 1;
            _weapon = null;
            UserSocket = socket;
        }

        public int Id => _id;
        public string? Name
        {
            get => _name;
            set { _name = value != null? value : $"player{_id}"; }
        }
        
        public int Health => _health;
        public int Attack => _attack;
        public int Deffence => _defense;
        public int Experiance {
            get => _experiance;
            set
            {
                if(_experiance + value >= 10)
                {
                    _experiance = _experiance + value - 10;
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
                    _health++;
                    _attack++;
                    _defense++;
                }

            } 
        }
        public Weapon? Weapon
        { 
            get => _weapon;
            set
            {
                if(value != null)
                {
                    _weapon = value;
                    _health += _weapon.HealthBuff;
                    _attack += _weapon.AttackBuff;
                    _defense += _weapon.DeffenceBuff;
                }
                else
                {
                    if(_weapon != null)
                    {
                        _health -= _weapon.HealthBuff;
                        _attack -= _weapon.AttackBuff;
                        _defense -= _weapon.DeffenceBuff;
                        _weapon = null;
                    }
                }
            }                
        }

        public Socket UserSocket { get; set; }

        public override string ToString()
        {
            return $"Player: {Name}\t\tlvl: {Level}  Health: {Health}  Att: {Attack}  Deff: {Deffence}  Weapon: {Weapon?.ToString()?? "no weapon"}";
        }
    }
}
