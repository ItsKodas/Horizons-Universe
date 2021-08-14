
using ProtoBuf;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.Utils;
using VRageMath;
using VRageRender;
using static NexusSyncMod.ModCore;

namespace NexusSyncMod
{
    public class SpawnPad
    {
        private static string font = "White";
        private static int DissapearTime = 1000 - 25;
        private static int CountdownTimer = 3;
        private static Regex RegCustomData = new Regex(":(.*)");
        private static Guid StorageGUID = new Guid("9416E3EB-216D-493D-914D-98AA90E88FB1");


        public enum SpawnType
        {
            Single,
            Multi
        }

        private SpawnType type;
        private MyEntity entity;
        private IMyCubeBlock block;
        private IMyTerminalBlock TerminalBlock;
        private IMyRadioAntenna RadioAnt;
        private string subType;
        private int CurrentTimer = 4;

        public double MaxDistance = 1.5;

        private SpawnPadConfigs Configs = new SpawnPadConfigs();

        private List<IMyPlayer> ContainedPlayers = new List<IMyPlayer>();


        public SpawnPad(MyEntity entity)
        {
            block = (IMyCubeBlock)entity;
            TerminalBlock = (IMyTerminalBlock)block;
            RadioAnt = (IMyRadioAntenna)block;

            subType = block.BlockDefinition.SubtypeId;

            if (subType == "SpawnPadSingle")
            {
                type = SpawnType.Single;
                MaxDistance = 1.5f;
            }
            else if (subType == "SpawnPadMulti")
            {
                type = SpawnType.Multi;
                MaxDistance = 2.5;
            }

            this.entity = entity;

            //If this doesnt have a storage component, go ahead and add one
            if (!LoadData())
                SaveData();

            AddCustomDataConfigs();
            //RadioAnt.CustomDataChanged += RadioAnt_CustomDataChanged;

        }

        public void Update()
        {

            //If the configs failed to recieve, skip update
            if (!GetCustomDataSettings())
                return;

            AccumulatePlayers();
            CheckStatus();


        }


        private IEnumerable<IMyCharacter> GetCharactersInBlock()
        {
            Vector3D BlockPosition = block.PositionComp.GetPosition();
            IEnumerable<IMyCharacter> Characters = MyEntities.GetEntities().OfType<IMyCharacter>();

            foreach (var Character in Characters)
            {

                if (!Character.InScene || Character.IsBot || Character.IsDead || !Character.IsPlayer)
                    continue;

                if (Vector3D.Distance(Character.GetPosition(), BlockPosition) < MaxDistance)
                    yield return (Character);
            }
        }

        private void AccumulatePlayers()
        {
            bool PassedNewTimerCheck = true;

            // Accumulate all players in the zone
            List<IMyPlayer> _ContainedPlayers = new List<IMyPlayer>();
            foreach (var Character in GetCharactersInBlock())
            {
                if (Character == null)
                    continue;

                IMyPlayer MyPlayer = MyAPIGateway.Players.GetPlayerControllingEntity(Character);

                if (MyPlayer == null)
                    continue;

                _ContainedPlayers.Add(MyPlayer);

                if (!ContainedPlayers.Contains(MyPlayer))
                {
                    //Reset the countdown timer if a new player joins
                    ContainedPlayers.Add(MyPlayer);
                    CurrentTimer = CountdownTimer;
                    PassedNewTimerCheck = false;
                    continue;
                }
            }

            if (_ContainedPlayers.Count == 0)
            {
                ContainedPlayers.Clear();
                return;
            }


            // Remove any players in the dictionary that no longer belong
            for (int i = ContainedPlayers.Count - 1; i >= 0; i--)
            {
                if (!_ContainedPlayers.Contains(ContainedPlayers[i]))
                {
                    // Reset the current timer if a player left
                    CurrentTimer = CountdownTimer;
                    PassedNewTimerCheck = false;
                    ContainedPlayers.RemoveAt(i);
                }
            }

            if (PassedNewTimerCheck)
                CurrentTimer--;
        }

        private void CheckStatus()
        {
            if (ContainedPlayers.Count == 0)
                return;

            //Check to see if pad is enabled and working
            if (!RadioAnt.Enabled || !RadioAnt.IsWorking)
            {
                BroadcastMessage("SpawnPad is disabled or non-functional!");
                return;
            }


            //Check to see if the target server is online
            if (Configs.TargetServerID > 0 && !NexusAPI.IsServerOnline(Configs.TargetServerID))
            {
                BroadcastMessage("Target server is not online!");
                return;
            }


            // Check to see if the target server has room
            if (!HasRoom(Configs.TargetServerID))
                return;


            // Begin checks for either single or multi spawn pad
            if (type == SpawnType.Single)
            {
                if (ContainedPlayers.Count > 1)
                {
                    BroadcastMessage("Only one player per pad!");
                    return;
                }
            }
            else if (type == SpawnType.Multi)
            {
                if (Configs.MaxPlayers != 0 && ContainedPlayers.Count > Configs.MaxPlayers)
                {
                    BroadcastMessage($"Max of {Configs.MaxPlayers} players on this pad!");
                    return;
                }

                if (Configs.MinPlayers != 0 && ContainedPlayers.Count < Configs.MinPlayers)
                {
                    BroadcastMessage($"You need {Configs.MinPlayers - ContainedPlayers.Count} more players to use this pad!");
                    return;
                }
            }

            CheckSingleStatus();
        }

        private bool HasRoom(int TargetServer)
        {

            if (TargetServer <= 0)
                return true;

            var List = NexusAPI.GetAllOnlineServers();

            int TotalOnlinePlayers = 0;
            foreach (var OnlinePlayer in NexusAPI.GetAllOnlinePlayers())
            {
                if (OnlinePlayer.OnServer == TargetServer)
                    TotalOnlinePlayers++;
            }


            foreach (var Server in List)
            {
                if (Server.ServerID == TargetServer)
                {

                    //Check max players
                    if (Server.MaxPlayers < TotalOnlinePlayers + ContainedPlayers.Count)
                    {
                        //If its over the max playercount, lets check reserved player count

                        bool Passed = true;
                        foreach (var Player in ContainedPlayers)
                        {
                            if (!Server.ReservedPlayers.Contains(Player.SteamUserId))
                            {
                                //Lets not pass if one of the contained players is not on the reserved slots
                                Passed = false;
                                break;
                            }
                        }


                        if (Passed)
                        {
                            return true;
                        }
                        else
                        {
                            BroadcastMessage("Target server is full");
                        }

                        return false;
                    }


                    return true;
                }
            }

            return false;
        }


        private void CheckSingleStatus()
        {
            //Check each players status in the configs
            string Message = "";
            bool PassedChecks = true;


            foreach (var Player in ContainedPlayers)
            {
                if (Player == null)
                    continue;

                PlayerPadUse Use = Configs.GetPlayerFromPad(Player.IdentityId);

               

                //Check spawn count limit
                if (Use != null && Configs.MaxSpawnsForPlayer != 0 && Use.Count >= Configs.MaxSpawnsForPlayer)
                {
                    Message = $"{Player.DisplayName} has reached their spawn limit!";
                    PassedChecks = false;
                    break;
                }

                //Check each timer for individual players
                if (Use != null && Configs.SpawnTimer != 0 && Use.LastUse + TimeSpan.FromMinutes(Configs.SpawnTimer) > DateTime.Now)
                {

                    TimeSpan TimeLeft = DateTime.Now - (Use.LastUse + TimeSpan.FromMinutes(Configs.SpawnTimer));
                    Message = $"{Player.DisplayName} has {TimeLeft.ToString(@"hh\:mm\:ss")} left until next spawn use!";
                    PassedChecks = false;
                    break;
                }


                //Check each players promote level

                /*

                if (Player.PromoteLevel <= Configs.MinimumRole)
                {
                    Message = $"{Player.DisplayName} doesnt have the required role!";
                    PassedChecks = false;
                    break;
                }
                */
            }

            //If they didnt pass the checks, broadcast why and return
            if (!PassedChecks)
            {
                BroadcastMessage(Message);
                return;
            }

            // If all players passed the checks, check the current timer status
            if (CurrentTimer <= 0)
            {
                //If timer is less than or equal to 0, send all clients to server via spawn message
                BeginSpawn();


            }
            else
            {
                //Display the current timer countdown

                BroadcastMessage($"Spawning at {TerminalBlock.DisplayNameText} in {CurrentTimer}");
            }
        }

        


        private void BeginSpawn()
        {
            List<ulong> AllSteamIDs = new List<ulong>();
            foreach (var player in ContainedPlayers)
            {
                AllSteamIDs.Add(player.SteamUserId);
            }

            ServerSpawnMessage Message = new ServerSpawnMessage();
            Message.ToServerID = Configs.TargetServerID;
            Message.ShipPrefabName = Configs.PrefabName;
            Message.ScriptName = Configs.ScriptName;
            Message.ContainedPlayers = AllSteamIDs;
            Message.CustomData = Configs.CustomData;

            Configs.AddPlayerUses(ContainedPlayers);

            Message.SendMessageToServer();

            CurrentTimer = 5;

            SaveData();
            //CloseAllPlayers();
        }



        private void BroadcastMessage(string Message)
        {
            foreach (var Player in ContainedPlayers)
            {
                if (Player == null)
                {
                    continue;
                }

                MyVisualScriptLogicProvider.ShowNotification(Message, DissapearTime, font, Player.IdentityId);
            }
        }

        private void AddCustomDataConfigs()
        {

            string CurrentData = TerminalBlock.CustomData;
            int CurrentLineCount = CurrentData.Split(new[] { "\r\n", "\r", "\n", Environment.NewLine }, StringSplitOptions.None).Length;

            StringBuilder B = new StringBuilder();
            B.AppendLine("PrefabName:");
            B.AppendLine("ScriptName:");
            B.AppendLine("ToServerID:1");
            B.AppendLine("MinPlayers:0");
            B.AppendLine("MaxPlayers:0");
            B.AppendLine("MaxSpawnsForPlayer:0");
            B.AppendLine("SpawnTimer (min):0");
            B.AppendLine("MinRole:None");
            B.AppendLine("CustomData:");

            string TargetCustomData = B.ToString();
            int TargetLineCount = TargetCustomData.Split(new[] { "\r\n", "\r", "\n", Environment.NewLine }, StringSplitOptions.None).Length;

            if (CurrentLineCount < TargetLineCount)
            {
                //MyAPIGateway.Utilities?.ShowMessage("SyncMod", "Updating new customdata!");
                TerminalBlock.CustomData = B.ToString();
            }

            GetCustomDataSettings();
        }

        private bool GetCustomDataSettings()
        {
            try
            {
                MatchCollection Collection = RegCustomData.Matches(TerminalBlock.CustomData);

                //Make sure we have the collections we need
                if (Collection.Count < 9)
                {
                    if (string.IsNullOrEmpty(TerminalBlock.CustomData))
                        AddCustomDataConfigs();

                    //AddCustomDataConfigs();
                    return false;
                }


                Configs.PrefabName = Collection[0].Groups[1].Value;
                Configs.ScriptName = Collection[1].Groups[1].Value;
                Configs.TargetServerID = TryParseInt(Collection[2].Groups[1].Value);
                Configs.MinPlayers = TryParseInt(Collection[3].Groups[1].Value);
                Configs.MaxPlayers = TryParseInt(Collection[4].Groups[1].Value);

                Configs.MaxSpawnsForPlayer = TryParseInt(Collection[5].Groups[1].Value);
                Configs.SpawnTimer = TryParseDouble(Collection[6].Groups[1].Value);

                try
                {
                    Configs.MinimumRole = (MyPromoteLevel)Enum.Parse(typeof(MyPromoteLevel), Collection[7].Groups[1].Value);
                
                }
                catch (Exception _)
                {
                    Configs.MinimumRole = MyPromoteLevel.None;
                    MyLog.Default?.WriteLineAndConsole($"NexusSyncMod: MyPromoteLevel: '{Collection[7].Groups[1].Value}' was not in correct format!");
                }




                Configs.CustomData = Collection[8].Groups[1].Value;
                return true;

            }
            catch (Exception Ex)
            {
                MyLog.Default?.WriteLineAndConsole($"NexusSyncMod: {Ex.ToString()}");
            }

            return false;
        }

        private int TryParseInt(string Input)
        {
            try
            {
                int result = Int32.Parse(Input);
                return result;
            }
            catch (Exception ex)
            {
                MyLog.Default?.WriteLineAndConsole($"NexusSyncMod: String '{Input}' was not in correct format for Int32!");
                return 0;
            }

        }

        private double TryParseDouble(string Input)
        {
            try
            {
                double result = Double.Parse(Input);



                return result;
            }
            catch (Exception ex)
            {
                MyLog.Default?.WriteLineAndConsole($"NexusSyncMod: String '{Input}' was not in correct format for Int32!");
                return 0;
            }

        }

        private bool LoadData()
        {
            if (entity.Storage != null && entity.Storage.ContainsKey(StorageGUID))
            {
                string Data = entity.Storage[StorageGUID];
                byte[] SavedData = Convert.FromBase64String(Data);

                try
                {
                    SpawnPadConfigs OldConfigs = MyAPIGateway.Utilities.SerializeFromBinary<SpawnPadConfigs>(SavedData);
                    if (OldConfigs == null)
                        return true;

                    Configs = OldConfigs;
                    //MyAPIGateway.Utilities?.ShowMessage("SyncMod", "Loading old configs!");
                    return true;

                }
                catch (Exception Ex)
                {
                    MyLog.Default?.WriteLineAndConsole($"SyncMod: {Ex.ToString()}");
                    return false;
                }

            }

            return false;
        }

        public void SaveData()
        {
            if (entity.Storage != null)
            {
                var newByteData = MyAPIGateway.Utilities.SerializeToBinary(Configs);
                var base64string = Convert.ToBase64String(newByteData);
                entity.Storage[StorageGUID] = base64string;
                //MyVisualScriptLogicProvider.ShowNotification("Data Saved", 2000);
            }
            else
            {
                entity.Storage = new MyModStorageComponent();

                var newByteData = MyAPIGateway.Utilities.SerializeToBinary(Configs);
                var base64string = Convert.ToBase64String(newByteData);
                entity.Storage[StorageGUID] = base64string;
            }

            //MyAPIGateway.Utilities?.ShowMessage("SyncMod", "Saving data!");
        }

        private void CloseAllPlayers()
        {
            foreach (var player in ContainedPlayers)
            {
                player.Character.Close();
            }
        }

    }


    [ProtoContract]
    public class SpawnPadConfigs
    {

        public string PrefabName;
        public string ScriptName;
        public int TargetServerID { get; set; } = 0;
        public int MinPlayers { get; set; } = 0;
        public int MaxPlayers { get; set; } = 0;

        public int MaxSpawnsForPlayer { get; set; } = 0;
        public double SpawnTimer { get; set; } = 0;

        public MyPromoteLevel MinimumRole;
        public string CustomData;

        [ProtoMember(1)]
        Dictionary<long, PlayerPadUse> PlayerUses = new Dictionary<long, PlayerPadUse>();

        public PlayerPadUse GetPlayerFromPad(long Player)
        {
            if (PlayerUses.ContainsKey(Player))
            {
                return PlayerUses[Player];
            }
            else
            {
                return null;
            }
        }


        public void AddPlayerUses(List<IMyPlayer> Players)
        {

            foreach (var Player in Players)
            {

                if (PlayerUses.ContainsKey(Player.IdentityId))
                {
                    PlayerUses[Player.IdentityId].Count++;
                    PlayerUses[Player.IdentityId].LastUse = DateTime.Now;
                }
                else
                {

                    PlayerPadUse Use = new PlayerPadUse();
                    PlayerUses.Add(Player.IdentityId, Use);
                }
            }
        }



        public SpawnPadConfigs() { }
    }

    [ProtoContract]
    public class PlayerPadUse
    {
        [ProtoMember(1)]
        public DateTime LastUse;

        [ProtoMember(2)]
        public int Count;

        public PlayerPadUse()
        {
            LastUse = DateTime.Now;
            Count = 1;
        }

    }
}
