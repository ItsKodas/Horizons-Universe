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
    public class SolSpace
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        //Parent Zone:                                          Center                 Radius in M
        public BoundingSphereD MainSphere = new BoundingSphereD(new Vector3D(61353.68, 468847.08, 194222.23), 640000);

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
            BoundingSphereD DemusZone = new BoundingSphereD(new Vector3D(61353.68, 468847.08, 194222.23), 100000);
            BoundingSphereD EarthZone = new BoundingSphereD(new Vector3D(-30643.74, 125791.25, -41736.14), 47000);
            BoundingSphereD MoonZone = new BoundingSphereD(new Vector3D(71965.24, 71965.24, 71965.24), 13000);
            BoundingSphereD AlienZone = new BoundingSphereD(new Vector3D(61353.68, 809353.66, -6829.57), 60000);
            BoundingSphereD ArkorusZone = new BoundingSphereD(new Vector3D(65163.92, 445345.00, 362584.60), 28000);
            BoundingSphereD EuropaZone = new BoundingSphereD(new Vector3D(260921.74, 821138.25, 3346.20), 13000);
            
            SpawnExclusionZones.Add(DemusZone);

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
