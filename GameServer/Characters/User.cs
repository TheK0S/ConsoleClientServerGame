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
        IWeapon? _weapon;

        public User(int id, string? name)
        {
            this._id = id;
            this._name = name;
            _health = 5;
            _attack = 1;
            _defense = 1;
            _experiance = 0;
            _level = 1;
            _weapon = null;
        }

        public int Id { get; set; }
        public string? Name { get; set; }
        public int Health { get; set; }
        public int Attack { get; set; }
        public int Deffence { get; set; }
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
                    Health++;
                    Attack++;
                    Deffence++;
                }

            } 
        }
        public IWeapon? Wapon
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
                    }
                }
            }                
        }

        public Socket? UserSocket { get; set; }

        public override string ToString()
        {
            return $"Player: {Name}\tlvl: {Level}\nHealth: {Health}\nAtt: {Attack}\nDeff: {Deffence}";
        }
    }
}
