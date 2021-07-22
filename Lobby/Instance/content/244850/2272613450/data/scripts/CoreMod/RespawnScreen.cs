using ProtoBuf;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;

namespace NexusSyncMod
{
    public class RespawnScreen
    {
        private bool _init = false;
        public const ushort NETWORK_ID = 2935;

        public List<IMyEntity> RenderedGrids = new List<IMyEntity>();


        private int MaxGrids = 0;
        private HashSet<IMyEntity> _spawned;


        public RespawnScreen()
        {

            Debug.Write("Initilizing Systems! Madeby: Casimir");
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(NETWORK_ID, MessageHandler);

        }

        private void MessageHandler(ushort arg1, byte[] arg2, ulong arg3, bool arg4)
        {
            try
            {
                ServerMessage RecievedMessage = MyAPIGateway.Utilities.SerializeFromBinary<ServerMessage>(arg2);
                IMyPlayer Client = MyAPIGateway.Session.LocalHumanPlayer;
                ulong SteamId = Client.SteamUserId;

                if (RecievedMessage.ClearRenderedGrids)
                {
                    foreach (IMyEntity Grid in RenderedGrids)
                    {
                        Grid?.Close();
                    }

                    RenderedGrids.Clear();
                    return;
                }


                if (RecievedMessage.PlayerSteamID == SteamId && RecievedMessage != null)
                {
                    //Debug.Write("GridBuildersCountA: " + RecievedMessage.GridBuilders.Count);

                    if (RecievedMessage.GridBuilders.Count == 0)
                        return;



                    foreach (ClientGridBuilder Builder in RecievedMessage.GridBuilders)
                    {
                        List<MyObjectBuilder_CubeGrid> TotalGrids = Builder.Grids;
                        //Debug.Write("TotalGrids: " + TotalGrids.Count);

                        if (TotalGrids.Count == 0)
                            continue;

                        //MyEntities.RemapObjectBuilderCollection(TotalGrids);
                        MaxGrids = TotalGrids.Count;
                        _spawned = new HashSet<IMyEntity>();
                        foreach (MyObjectBuilder_CubeGrid grid in TotalGrids)
                        {
                            //MyAPIGateway.Entities.RemapObjectBuilder(grid);
                            if (MyEntities.EntityExists(grid.EntityId))
                            {
                                continue;
                            }


                            MyAPIGateway.Entities.CreateFromObjectBuilderParallel(grid, false, Increment);

                        }

                    }
                }

            }
            catch (Exception ex)
            {

                Debug.Write("Error durring message recieved! \n" + ex);
            }
        }


        public void Increment(IMyEntity entity)
        {


            MyEntity Ent = (MyEntity)entity;
            Ent.IsPreview = false;
            Ent.SyncFlag = false;
            Ent.Save = false;


            //Debug.Write("A");

            _spawned.Add(Ent);
            //Debug.Write("B");
            if (_spawned.Count < MaxGrids)
                return;

            //Debug.Write("C");


            MyAPIGateway.Utilities.InvokeOnGameThread(() =>
            {
                try
                {
                    foreach (IMyEntity g in _spawned)
                    {
                        if (g == null)
                            continue;

                        RenderedGrids.Add(g);
                        MyAPIGateway.Entities.AddEntity(g, true);
                    }

                }
                catch (Exception ex)
                {
                    Debug.Write(ex.ToString());
                }
            });


            MaxGrids = 0;
        }



        public void UnloadData()
        {
            try
            {
                MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(NETWORK_ID, MessageHandler);
            }
            catch (Exception a)
            {
                //MyLog.Default.WriteLineAndConsole("Cannot remove event Handlers! Are they already removed?1" + a);
            }
        }


    }

    [ProtoContract]
    public class ServerMessage
    {
        [ProtoMember(1)]
        public readonly ulong PlayerSteamID;

        [ProtoMember(2)]
        public readonly string ServerIP;

        [ProtoMember(3)]
        public readonly long MessageAuthentication;

        [ProtoMember(4)]
        public bool ClearRenderedGrids = false;

        [ProtoMember(5)]
        public RespawnOption[] Spawns;

        [ProtoMember(6)]
        public List<ClientGridBuilder> GridBuilders = new List<ClientGridBuilder>();

        public ServerMessage(ulong SteamID, string Server, long Authentication)
        {
            PlayerSteamID = SteamID;
            ServerIP = Server;
            MessageAuthentication = Authentication;
        }

        public ServerMessage() { }
    }

    [ProtoContract]
    public class RespawnOption
    {
        [ProtoMember(1)]
        public long RespawnGridID;

        [ProtoMember(2)]
        public long RespawnBlockID;


        public RespawnOption() { }
    }

    [ProtoContract]
    public class ClientGridBuilder
    {
        [ProtoMember(1)]
        public List<MyObjectBuilder_CubeGrid> Grids = new List<MyObjectBuilder_CubeGrid>();


        public ClientGridBuilder() { }
    }


    public class Debug
    {
        private static bool EnableDebug = true;
        public static void Write(string msg)
        {
            if (EnableDebug)
            {
                MyAPIGateway.Utilities.ShowMessage("NexusMOD", msg);
                MyLog.Default.WriteLineAndConsole("NexusMOD: " + msg);


            }
        }
    }
}
