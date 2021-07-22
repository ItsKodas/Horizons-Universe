using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Torch.Mod;
using Torch.Mod.Messages;

namespace Spawn
{
    public class LobbyDirect
    {
        //IP address of target server
        private string IP = "51.161.132.141:27016";


        public void LobbySpawn(ref List<long> ContainedPlayers, string CustomData = null)
        {
            foreach(var player in ContainedPlayers)
            {
                ulong Player = MySession.Static.Players.TryGetSteamId(player);

                //Invalid player check
                if (Player == 0L)
                    continue;

                ModCommunication.SendMessageTo(new JoinServerMessage(IP), Player);
            }

            //Clear all characters so we dont let the rest of nexus code happen
            ContainedPlayers.Clear();
        }

    }
}
