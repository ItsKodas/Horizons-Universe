using ProtoBuf;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexusSyncMod
{
    [ProtoContract]
    public class ServerSpawnMessage
    {
        public const ushort NETWORK_ID = 2937;


        [ProtoMember(1)]
        public int ToServerID;

        [ProtoMember(2)]
        public string ShipPrefabName;

        [ProtoMember(3)]
        public string ScriptName;

        [ProtoMember(4)]
        public List<ulong> ContainedPlayers = new List<ulong>();

        [ProtoMember(5)]
        public string CustomData;

        public ServerSpawnMessage() { }

        public void SendMessageToServer()
        {
            byte[] Data = MyAPIGateway.Utilities.SerializeToBinary(this);
            MyAPIGateway.Multiplayer.SendMessageToServer(NETWORK_ID, Data);
        }

    }
}
