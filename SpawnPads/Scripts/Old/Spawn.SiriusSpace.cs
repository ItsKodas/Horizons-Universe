using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace Spawn
{
    public class SiriusSpace
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        //Parent Zone:                                          Center                 Radius in M
        public BoundingSphereD MainSphere = new BoundingSphereD(new Vector3D(449880.12, -1226169.03, -1842427.21), 600000);

        public List<BoundingSphereD> SpawnExclusionZones = new List<BoundingSphereD>();
        public void BeforeSpawn(ref List<MyObjectBuilder_CubeGrid> Grids, ref List<long> Players, ref Vector3D SpawnPos, ref bool AlignToGravity)
        {

            foreach (var grid in Grids)
            {
                foreach (var block in grid.CubeBlocks)
                {
                    block.Owner = Players[0];
                    block.BuiltBy = Players[0];
                }
            }


            //Add custom exclusion zones: 
            BoundingSphereD NekrosZone = new BoundingSphereD(new Vector3D(449880.12, -1226169.03, -1842427.21), 120000);
            BoundingSphereD CraitZone = new BoundingSphereD(new Vector3D(578402.30, -1524895.11, -1872100.32), 58000);
            BoundingSphereD SatreusZone = new BoundingSphereD(new Vector3D(718532.83, -1492826.12, -1946827.00), 24000);
            BoundingSphereD SegoviaZone = new BoundingSphereD(new Vector3D(175015.85, -857555.54, -2043375.74), 63000);
            BoundingSphereD TohilZone = new BoundingSphereD(new Vector3D(170631.24, -927969.31, -1820686.04), 15000);
            
            SpawnExclusionZones.Add(NekrosZone);
            SpawnExclusionZones.Add(CraitZone);
            SpawnExclusionZones.Add(SatreusZone);
            SpawnExclusionZones.Add(SegoviaZone);
            SpawnExclusionZones.Add(TohilZone);

            int MaxLoops = 250;
            for (int i = 1; i < MaxLoops; i++)
            {
                Vector3D Point = GenerateRandomPointInSphere();
                if (IsPointValid(Point, out SpawnPos))
                {
                    Log.Info("Found valid spawnpoint after: " + i + " trys!");
                    return;
                }
            }

            Log.Warn("Couldnt find a safe spawn after " + MaxLoops + "trys! Spawning it randomly!");
            SpawnPos = GenerateRandomPointInSphere();
        }

        private bool IsPointValid(Vector3D GivenPoint, out Vector3D Spawn)
        {
            Spawn = new Vector3D(0, 0, 0);
            foreach (BoundingSphereD Zone in SpawnExclusionZones)
            {
                if (Zone.Contains(GivenPoint) == ContainmentType.Contains)
                {
                    return false;
                }
            }

            Spawn = GivenPoint;
            return true;
        }

        private Vector3D GenerateRandomPointInSphere()
        {
            Vector3D SphereCenter = MainSphere.Center;
            Vector3D Rando = Vector3D.Normalize(MyUtils.GetRandomVector3D());
            Rando *= MyUtils.GetRandomInt((int)MainSphere.Radius); ;
            return SphereCenter - Rando;
        }
    }
}
