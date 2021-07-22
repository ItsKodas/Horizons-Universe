using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRageMath;

namespace Spawn
{
    public class Character
    {
        private static Vector3D SpawnPosition = new Vector3D(0, 0, 0); //XYZ in meters of spawn position

        public void BeforeSpawn(ref List<MyObjectBuilder_CubeGrid> Grids, ref List<long> Players, ref Vector3D SpawnPos, ref bool AlignToGravity, string CustomData = null)
        {
            Grids.Clear();
            SpawnPos = SpawnPosition;
        }
    }
}
