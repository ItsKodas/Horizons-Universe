using NLog;
using Sandbox;
using Sandbox.Game.Entities;
using Sandbox.Game.Entities.Character;
using Sandbox.Game.Entities.Planet;
using Sandbox.Game.GameSystems;
using Sandbox.Game.World;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Utils;
using VRageMath;

namespace Spawn
{
    public class Crait
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private static string PlanetName = "Planet-Crait-1298343996d66560";
        private static bool SpawnNearFactionMates = true;
        private static float SpawnHeight = 10;
        private static float MinimalAirDensity = .80f;

        private static float CollisionRadius;

        public void BeforeSpawn(ref List<MyObjectBuilder_CubeGrid> Grids, ref List<long> Players, ref Vector3D SpawnPos, ref bool AlignToGravity)
        {
            try
            {

                MyPlanet ChosenSpawn = MyPlanets.GetPlanets().Find(x => x.Name.Contains(PlanetName));
                AlignToGravity = true;

                if (ChosenSpawn is null)
                {
                    Log.Error("Invalid Planet!");
                    return;
                }

                //AutoGet Collision Radius
                CollisionRadius = (float)FindBoundingSphere(Grids).Radius + 10;


                Log.Warn("Checking value!");
                bool flag = false;
                SpawnPos = Vector3D.Zero;


                foreach (var grid in Grids)
                {
                    foreach (var block in grid.CubeBlocks)
                    {
                        block.Owner = Players[0];
                        block.BuiltBy = Players[0];
                    }
                }

                if (SpawnNearFactionMates)
                {
                    List<Vector3D> friendlyPlayerPositions = GetFriendlyPlayerPositions(Players[0]);
                    Log.Warn("Attempting to spawn near faction mate!");


                    try
                    {
                        BoundingBoxD worldAABB = ChosenSpawn.PositionComp.WorldAABB;
                        for (int num = friendlyPlayerPositions.Count - 1; num >= 0; num--)
                        {
                            if (worldAABB.Contains(friendlyPlayerPositions[num]) == ContainmentType.Disjoint)
                            {
                                friendlyPlayerPositions.RemoveAt(num);
                            }
                        }



                        for (int i = 0; i < 30; i += 3)
                        {
                            if (flag)
                            {
                                break;
                            }

                            foreach (Vector3D item in friendlyPlayerPositions)
                            {
                                Vector3D? vector3D = FindPositionAbovePlanet(item, ChosenSpawn, true, i, i + 3, out Vector3 forward, out Vector3 up);
                                if (vector3D.HasValue)
                                {
                                    SpawnPos = vector3D.Value;
                                    flag = true;
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Warn(e);
                    }
                }
                if (!flag)
                {
                    Log.Warn("Spawning at random pos around planet!");
                    Vector3D center = ChosenSpawn.PositionComp.WorldVolume.Center;
                    for (int j = 0; j < 50; j++)
                    {
                        Vector3 value = MyUtils.GetRandomVector3Normalized();
                        if (value.Dot(MySector.DirectionToSunNormalized) < 0f && j < 20)
                        {
                            value = -value;
                        }


                        SpawnPos = center + value * ChosenSpawn.AverageRadius;
                        Vector3D? vector3D2 = FindPositionAbovePlanet(SpawnPos, ChosenSpawn, j < 20, 0, 30, out Vector3 forward, out Vector3 up);
                        if (vector3D2.HasValue)
                        {
                            SpawnPos = vector3D2.Value;
                            if ((SpawnPos - center).Dot(MySector.DirectionToSunNormalized) > 0.0)
                            {
                                return;
                            }
                        }
                    }
                }
            }catch(Exception ex)
            {
                Log.Error(ex);
            }

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

        private static Vector3D? FindPositionAbovePlanet(Vector3D friendPosition, MyPlanet info, bool testFreeZone, int distanceIteration, int maxDistanceIterations, out Vector3 forward, out Vector3 up)
        {
            Log.Info("Finding position above planet!");
            MyPlanet planet = info;
            Vector3D center = planet.PositionComp.WorldAABB.Center;
            Vector3D axis = Vector3D.Normalize(friendPosition - center);
            float optimalSpawnDistance = MySession.Static.Settings.OptimalSpawnDistance;
            float minimalClearance = (optimalSpawnDistance - optimalSpawnDistance * 0.5f) * 0.9f;


            for (int i = 0; i < 20; i++)
            {

                Vector3D randomPerpendicularVector = MyUtils.GetRandomPerpendicularVector(ref axis);
                float num = optimalSpawnDistance * (MyUtils.GetRandomFloat(0.549999952f, 1.65f) + (float)distanceIteration * 0.05f);
                Vector3D globalPos = friendPosition + randomPerpendicularVector * num;
                globalPos = planet.GetClosestSurfacePointGlobal(ref globalPos);
                if (!TestLanding(info, globalPos, testFreeZone, minimalClearance, ref distanceIteration))
                {
                    if (distanceIteration > maxDistanceIterations)
                    {
                        break;
                    }
                    continue;
                }

                Vector3D? shipOrientationForPlanetSpawn = GetShipOrientationForPlanetSpawn(ref globalPos, out forward, out up);
                if (shipOrientationForPlanetSpawn.HasValue)
                {
                    return shipOrientationForPlanetSpawn.Value;
                }

            }

            forward = default(Vector3);
            up = default(Vector3);
            return null;
        }

        private static Vector3D? GetShipOrientationForPlanetSpawn(ref Vector3D landingPosition, out Vector3 forward, out Vector3 up)
        {
            Log.Warn("Getting ship orientation for spawn!");

            Vector3 vector = MyGravityProviderSystem.CalculateNaturalGravityInPoint(landingPosition);
            if (Vector3.IsZero(vector))
            {
                vector = Vector3.Up;
            }
            Vector3D value = Vector3D.Normalize(vector);
            Vector3D value2 = -value;



            Vector3D? result = landingPosition + value2 * SpawnHeight;
            forward = Vector3.CalculatePerpendicularVector(-value);
            up = -value;
            return result;
        }

        public void AfterSpawn(ref HashSet<IMyCubeGrid> Grids)
        {
            Log.Warn("Running after spawn!");
        }



        private static bool CheckTerrain(MyPlanet Planet, Vector3D landingPosition, Vector3D DeviationNormal, Vector3D GravityVector)
        {
            Vector3 vector = (Vector3)DeviationNormal * CollisionRadius;
            Vector3 value = Vector3.Cross(vector, GravityVector);
            MyOrientedBoundingBoxD myOrientedBoundingBoxD = new MyOrientedBoundingBoxD(landingPosition, new Vector3D(CollisionRadius * 2f, Math.Min(10f, CollisionRadius * 0.5f), CollisionRadius * 2f), Quaternion.CreateFromForwardUp(DeviationNormal, GravityVector));
            int num = -1;
            for (int i = 0; i < 4; i++)
            {
                num = -num;
                int num2 = (i <= 1) ? 1 : (-1);
                Vector3D point = Planet.GetClosestSurfacePointGlobal(landingPosition + vector * num + value * num2);
                if (!myOrientedBoundingBoxD.Contains(ref point))
                {
                    return false;
                }
            }
            return true;
        }


        private static bool TestLanding(MyPlanet Planet, Vector3D landingPosition, bool testFreeZone, float minimalClearance, ref int distanceIteration)
        {
            if (testFreeZone && MinimalAirDensity > 0f && Planet.GetAirDensity(landingPosition) < MinimalAirDensity)
            {
                return false;
            }

            

            Vector3D center = Planet.PositionComp.WorldAABB.Center;
            Vector3D GravityVector = Vector3D.Normalize(landingPosition - center);
            Vector3D DeviationNormal = MyUtils.GetRandomPerpendicularVector(ref GravityVector);

            if (!CheckTerrain(Planet, landingPosition, DeviationNormal, GravityVector))
            {
                return false;
            }

            if (testFreeZone && !IsZoneFree(new BoundingSphereD(landingPosition, minimalClearance)))
            {
                distanceIteration++;
                return false;
            }

            return true;


        }

        private static bool IsZoneFree(BoundingSphereD safeZone)
        {
            ClearToken<MyEntity> clearToken = ListExtensions.GetClearToken(MyEntities.GetTopMostEntitiesInSphere(ref safeZone));
            try
            {
                foreach (MyEntity item in clearToken.List)
                {
                    if (item is MyCubeGrid)
                    {
                        return false;
                    }
                }
            }
            finally
            {
                ((IDisposable)clearToken).Dispose();
            }
            return true;
        }



        private BoundingSphereD FindBoundingSphere(List<MyObjectBuilder_CubeGrid> grids)
        {
            BoundingSphere result = new BoundingSphere(Vector3.Zero, float.MinValue);
            foreach (MyObjectBuilder_CubeGrid myObjectBuilder_CubeGrid in grids)
            {
                BoundingSphere boundingSphere = MyCubeGridExtensions.CalculateBoundingSphere(myObjectBuilder_CubeGrid);
                MatrixD m = myObjectBuilder_CubeGrid.PositionAndOrientation.HasValue ? myObjectBuilder_CubeGrid.PositionAndOrientation.Value.GetMatrix() : MatrixD.Identity;
                result.Include(boundingSphere.Transform(m));
            }
            return result;
        }


    }
}
