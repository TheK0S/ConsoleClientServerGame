using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Characters;

namespace GameServer
{
    public class Message
    {
        public List<User> users = new List<User>();
        public List<Mob> mobs = new List<Mob>();
        public List<string> gameActions = new List<string>();
        public string? ouner;
        public int targetId;
        public string? action;
    }
}
