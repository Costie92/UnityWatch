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

        public static readonly string hero_SoldierPath = "hcp/HeroSoldier";
        public static readonly string hero_HookPath = "hcp/HeroHook";

        public static string GetHeroPhotonNetworkInstanciatePath(E_HeroType type)
        {
            string str = "";
            switch (type)
            {
                case E_HeroType.Soldier:
                    str = hero_SoldierPath;
                    break;
                case E_HeroType.Hook:
                    str = hero_HookPath;
                    break;
                default:
                    Debug.LogError("GetHeroPhotonNetworkInstanciatePath: 넘겨받은 히어로 타입이 적절치 못함.");
                    break;
            }
            return str;
        }
    }
}