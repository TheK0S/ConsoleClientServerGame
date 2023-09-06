using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    internal class GameAction
    {
        public int ounerId;
        public int targetId;
        public bool isAttack;

        public GameAction(int ounerId, int targetId, bool isAttack)
        {
            this.ounerId = ounerId;
            this.targetId = targetId;
            this.isAttack = isAttack;
        }
    }
}
