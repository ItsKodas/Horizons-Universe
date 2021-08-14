using NLog;
using Sandbox.Game.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;

namespace Nexus.Scripts.NexusGateScripts
{
    public class GateFilter
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        /*  Gate filter allows you to control how grids are being allowed through the gates
         *  This script should be placed into the instance/NexusSpawnPads/Scripts (Yes, I know its not a spawn pad, but I didnt want to rename the folder)
         * 
         *  A script lets you personallize control on how your server wants this specific function to operate without bugging me for custom configs, and then forcing a plugin update
         *  If you need help making a custom filter, let me know and Ill do my best to point you in the right direction.
         * 
         * 
         *  GateInput Function returns a bool to determing if the grid is allowed through the gate. You can also specific a custom response when it returns false
         */


        public bool AllowSmallGrids = true;

        public bool AllowLargeGrids = true;



        public bool GateInput(List<MyCubeGrid> Grids, List<long> AllPlayers, Vector3D Target, ref string ChatResponse)
        {
            /* Grids - all grids of the ship coming through the gate
             * AllPlayers - Players sitting in or in suits around the ship that are being transfered with the ship
             * Target - Target spawn location
             * ChatResponse - Optional Chat response to tell players why they cant connect
             * 
             * 
             */



            ChatResponse = "";

            foreach (var Grid in Grids)
            {

                if (Grid.GridSizeEnum == VRage.Game.MyCubeSize.Large && !AllowLargeGrids)
                {
                    ChatResponse = "Large grids are not allowed through this gate!";
                    return false;
                }

                if (Grid.GridSizeEnum == VRage.Game.MyCubeSize.Small && !AllowSmallGrids)
                {
                    ChatResponse = "Large grids are not allowed through this gate!";
                    return false;
                }
            }

            //Allow it to pass
            return true;
        }


        /*

        // You can do anything here when the grid gets pasted after transfer. Just uncomment the function (leaving it commented will prevent the program from calling it)


        public void GateOutput(List<MyCubeGrid> Grids, List<long> AllPlayers)
        {
            
        }

        */


    }
}
