using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace hcp
{
    public static class Constants
    {
        public static readonly string fpsCamLayerName = "FPSCam";
        public static readonly int fpsCamLayerMask = LayerMask.NameToLayer(fpsCamLayerName);
        public static readonly string mapLayerName = "MAP";
        public static readonly int mapLayerMask = LayerMask.NameToLayer(mapLayerName);
        public static readonly string teamA_LayerName = "TEAM A";
        public static readonly int teamA_LayerMask = LayerMask.NameToLayer(teamA_LayerName);
        public static readonly string teamB_LayerName = "TEAM B";
        public static readonly int teamB_LayerMask = LayerMask.NameToLayer(teamB_LayerName);
        public static readonly string teamC_LayerName = "TEAM C";
        public static readonly int teamC_LayerMask = LayerMask.NameToLayer(teamC_LayerName);
        public static readonly string teamD_LayerName = "TEAM D";
        public static readonly int teamD_LayerMask = LayerMask.NameToLayer(teamD_LayerName);

    }
}