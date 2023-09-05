using GameServer.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Rooms
{
    public class Cave
    {
        public List<User> users = new List<User>();
        public List<Mob> mobs = new List<Mob>();
    }
}
