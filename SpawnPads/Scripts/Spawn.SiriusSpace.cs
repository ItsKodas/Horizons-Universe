using NLog;
using Sandbox;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Planet;
using Sandbox.Game.World;
using Sandbox.Game.World.Generator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Entity;
using VRage.Utils;
using VRageMath;

namespace Spawn
{
    public class SiriusSpace
    {

        /*  This is a SpaceSpawnScript for inside a Specified Region! You will have to figure out which region is safe for spawn spawns to start in so things dont jump around.
         *  So this space spawn scrip, we want to pick a random point inside of a sector. So you will have to predefine the script to a region
         * 
         */

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static Vector3D RegionCenter = new Vector3D(449880.12, -1226169.03, -1842427.21); //This is XYZ coords of the region center. This can be pulled from Nexus Controller Sectorsetup.
        private static double RegionRadius = 600; //Radius is in KM. This also can be pulled from Controller SectorSetup
        private static bool SpawnNearPlayers = true; //Spawn near friendly players
        private static float CollisionRadius = 100;
        private static float MaxDistanceToFriend = 15000; 


        private static long Identity;

        public void BeforeSpawn(ref List<MyObjectBuilder_CubeGrid> Grids, ref List<long> Players, ref Vector3D SpawnPos, ref bool AlignToGravity, string CustomData = null)
        {
            Identity = Players[0];



            Result End = GetSpawns();


           // Task<Result> P = InvokeAsync<Result>(GetSpawns, "NexusSpawnScript");
           // Log.Info("Waiting for task to finish");
           // P.Wait(5000);

            //Result End = P.Result;
            SpawnPos = End.SpawnPos;


            foreach (var grid in Grids)
            {
                foreach (var block in grid.CubeBlocks)
                {
                    block.Owner = Players[0];
                    block.BuiltBy = Players[0];
                }
            }

        }




        private static Result GetSpawns()
        {
            float optimalSpawnDistance = MySession.Static.Settings.OptimalSpawnDistance;
            float num = (optimalSpawnDistance - optimalSpawnDistance * 0.5f) * 0.9f;

            Vector3 UpVector = new Vector3();
            Vector3 ForwardVector = new Vector3();
            BoundingSphereD Region = new BoundingSphereD(RegionCenter, RegionRadius * 1000);
            if (SpawnNearPlayers)
            {
                List<Vector3D> friendlyPlayerPositions = GetFriendlyPlayerPositions(Identity);
                try
                {
                    foreach (Vector3D friendPosition in friendlyPlayerPositions)
                    {
                        if (Region.Contains(friendPosition) != ContainmentType.Contains)
                            continue;


                        if (!Enumerable.Any<BoundingBoxD>((IEnumerable<BoundingBoxD>)MyPlanets.Static.GetPlanetAABBs(), (Func<BoundingBoxD, bool>)((BoundingBoxD x) => x.Contains(friendPosition) != ContainmentType.Disjoint)))
                        {
                            Vector3D center = friendPosition + MyUtils.GetRandomVector3Normalized() * (optimalSpawnDistance * MyUtils.GetRandomFloat(0.5f, 1.5f));
                            Vector3D? vector3D = MyProceduralWorldModule.FindFreeLocationCloseToAsteroid(new BoundingSphereD(center, MaxDistanceToFriend), new BoundingSphereD(friendPosition, num), false, true, CollisionRadius, num, out ForwardVector, out UpVector);
                            if (vector3D.HasValue)
                            {

                                return new Result(vector3D.Value, ForwardVector, UpVector);
                            }
                        }
                    }
                }
                finally
                {

                }
            }





            BoundingBoxD boundingBoxD = BoundingBoxD.CreateInvalid();
            BoundingBoxD box = new BoundingBoxD(new Vector3D(-25000.0), new Vector3D(25000.0));
            foreach (MyEntity entity in MyEntities.GetEntities())
            {
                if (entity.Parent != null)
                {
                    continue;
                }
                BoundingBoxD worldAABB = entity.PositionComp.WorldAABB;
                if (entity is MyPlanet)
                {
                    if (worldAABB.Contains(Vector3D.Zero) != 0)
                    {
                        boundingBoxD.Include(worldAABB);
                    }
                }
                else
                {
                    box.Include(worldAABB);
                }
            }
            box.Include(boundingBoxD.GetInflated(25000.0));
            if (MyEntities.IsWorldLimited())
            {
                Vector3D vector3D2 = new Vector3D(MyEntities.WorldSafeHalfExtent());
                box = new BoundingBoxD(Vector3D.Clamp(box.Min, -vector3D2, Vector3D.Zero), Vector3D.Clamp(box.Max, Vector3D.Zero, vector3D2));
            }
            Vector3D vector3D3 = Vector3D.Zero;
            for (int i = 0; i < 50; i++)
            {
                vector3D3 = MyUtils.GetRandomPosition(ref box);
                if (boundingBoxD.Contains(vector3D3) == ContainmentType.Disjoint)
                {
                    break;
                }
            }
           
            BoundingSphereD value = new BoundingSphereD(boundingBoxD.Center, Math.Max(0.0, boundingBoxD.HalfExtents.Min()));
            Vector3D? vector3D4 = MyProceduralWorldModule.FindFreeLocationCloseToAsteroid(Region, value, true, true, CollisionRadius, num, out ForwardVector, out UpVector);
            if (vector3D4.HasValue)
            {
                return new Result(vector3D4.Value, ForwardVector, UpVector);
            }
            Vector3D position = MyEntities.FindFreePlace(Region.Center, CollisionRadius, 100, 5, 2) ?? vector3D3;
            return new Result(position, ForwardVector, UpVector);

        }


        private static List<Vector3D> GetFriendlyPlayerPositions(long identityId)
        {
            List<Vector3D> RandomFriendlyPositions = new List<Vector3D>();
            MyFaction Faction = MySession.Static.Factions.GetPlayerFaction(identityId);

            if (Faction == null)
                return RandomFriendlyPositions;

            foreach (MyIdentity allIdentity in MySession.Static.Players.GetAllIdentities().Where(x => Faction.Members.ContainsKey(x.IdentityId)))
            {
                MyCharacter character = allIdentity.Character;
                if (character != null && !character.IsDead && !character.MarkedForClose)
                {
                    RandomFriendlyPositions.Add(character.PositionComp.GetPosition());
                }
            }

            return RandomFriendlyPositions;
        }

        public static Task<T> InvokeAsync<T>(Func<T> action, [CallerMemberName] string caller = "")
        {
            //Jimm thank you. This is the best
            var ctx = new TaskCompletionSource<T>();
            MySandboxGame.Static.Invoke(() =>
            {
                try
                {
                    ctx.SetResult(action.Invoke());
                }
                catch (Exception e)
                {
                    ctx.SetException(e);
                }

            }, caller);
            return ctx.Task;
        }

    }


    public struct Result
    {
        public Vector3D SpawnPos;
        public Vector3 Forward;
        public Vector3 Up;

        public Result(Vector3D SpawnPos, Vector3 Forward, Vector3 Up)
        {
            this.SpawnPos = SpawnPos;
            this.Forward = Forward;
            this.Up = Up;
        }
    }
}
