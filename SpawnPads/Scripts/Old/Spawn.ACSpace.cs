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
    public class ACSpace
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        //Parent Zone:                                          Center                 Radius in M
        public BoundingSphereD MainSphere = new BoundingSphereD(new Vector3D(-1801336.64, 86848.78, 1115550.61), 350000);

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
            BoundingSphereD LumaZone = new BoundingSphereD(new Vector3D(-1801336.64, 86848.78, 1115550.61), 102000);
            BoundingSphereD PertamZone = new BoundingSphereD(new Vector3D(-1838758.48, 156391.15, 915861.77), 35000);
            BoundingSphereD TritonZone = new BoundingSphereD(new Vector3D(-1997732.27, 30095.65, 1292300.79), 37000);
            BoundingSphereD QunZone = new BoundingSphereD(new Vector3D(-1770733.23, 178870.99, 916771.09), 20000);
            BoundingSphereD KimiZone = new BoundingSphereD(new Vector3D(-1699184.97, 49074.48, 1150214.86), 20000);
            
            SpawnExclusionZones.Add(LumaZone);
            SpawnExclusionZones.Add(PertamZone);
            SpawnExclusionZones.Add(TritonZone);
            SpawnExclusionZones.Add(QunZone);
            SpawnExclusionZones.Add(KimiZone);

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
