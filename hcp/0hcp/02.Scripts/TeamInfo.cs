using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
namespace hcp
{
    public class TeamInfo : MonoBehaviourPun
    {
        [SerializeField]
        int myTeamLayer;
        public int MyTeamLayer
        {
            get
            {
                return myTeamLayer;
            }
        }

        [SerializeField]
        List<int> enemyTeamLayer = new List<int>();
        public List<int> EnemyTeamLayer
        {
            get { return enemyTeamLayer; }
        }


        [SerializeField]
        List<Hero> enemyHeroes = new List<Hero>();
        public List<Hero> EnemyHeroes
        {
            get
            {
                return enemyHeroes;
            }
        }
        [SerializeField]
        List<Hero> myTeamHeroes = new List<Hero>();
        public List<Hero> MyTeamHeroes
        {
            get
            {
                return myTeamHeroes;
            }
        }
        static TeamInfo _instance = null;
        public static TeamInfo GetInstance()
        {
            return _instance;
        }

        private void Awake()
        {
            if (_instance == null)
                _instance = this;
        }

        IEnumerator Start()
        {
            yield return new WaitForSeconds(2f);
            StartCoroutine(WaitForAllHeroBorn());
        }
        Dictionary<int, string> teamInfoDic = new Dictionary<int, string>();

        void GetTeamInfoFromNetworkManager()
        {
            teamInfoDic.Clear();

            if (NetworkManager.instance == null)
                return;

            teamInfoDic = NetworkManager.instance .Teams;

            List<int> enemyLayerList = new List<int>();    //자기 팀 외로.
            int myPhotonViewIDKey =0;
            Hero[] heroes = GameObject.FindObjectsOfType<Hero>();
            for (int i = 0; i < heroes.Length; i++)
            {
                if (heroes[i].photonView.IsMine)
                {
                    myPhotonViewIDKey = heroes[i].photonView.ViewID / 1000;
                }
            }

            
            Dictionary<int, string>.Enumerator enu = teamInfoDic.GetEnumerator();
            while (enu.MoveNext())
            {
                int photonViewIDKey = enu.Current.Key;
                string layerName = enu.Current.Value;

                if (myPhotonViewIDKey == photonViewIDKey)
                {
                    myTeamLayer = LayerMask.NameToLayer(layerName);
                }
                else
                {
                    //적 레이어임.
                    if (false == enemyLayerList.Contains(photonViewIDKey))  //추가된 적 레이어가 아니면.
                    {
                        enemyLayerList.Add(LayerMask.NameToLayer(layerName));
                    }
                }
            }
            enemyTeamLayer = enemyLayerList;
            StartCoroutine(WaitForAllHeroBorn());
        }

        IEnumerator WaitForAllHeroBorn()
        {
            int heroCounts = 0;
            while (heroCounts != PhotonNetwork.CurrentRoom.PlayerCount)
            {
                heroCounts = GameObject.FindObjectsOfType<Hero>().Length;
                yield return new WaitForSeconds(1f);
            }
            //히어로 전부 생성된 시간임.
            GetTeamInfoFromNetworkManager();

            Hero[] heroes = GameObject.FindObjectsOfType<Hero>();
            myTeamHeroes.Clear();
            enemyHeroes.Clear();
            for (int i = 0; i < heroes.Length; i++)
            {
                int heroPhotonID = heroes[i].photonView.ViewID / 1000;
                if (heroPhotonID == myTeamLayer)
                {
                    myTeamHeroes.Add(heroes[i]);
                }
                else
                {
                    enemyHeroes.Add(heroes[i]);
                }
            }
            if (teamSettingIsDone == null)
            {
                Debug.LogError("팀세팅 이벤트를 리스닝 하고 있는 애들이 없어.");
            }
            teamSettingIsDone();
        }

        public int EnemyMaskedLayer//에너미가 한 개 이상이면 그에 맞게 마스킹해서 줌.
        {
            get
            {
                int layer = -1;
                for (int i = 0; i < enemyTeamLayer.Count; i++)
                {
                    if (layer == -1)    //맨처음.
                    {
                        layer = 1 << enemyTeamLayer[i];
                    }
                    else
                    {
                        layer = layer | 1 << enemyTeamLayer[i];
                    }
                }
                if (layer == -1)
                {
                    Debug.LogError("에너미 레이어에 설정오류 존재.");
                }
                return layer;
            }
        }
        public bool IsThisLayerEnemy(int layer)
        {
            return enemyTeamLayer.Contains(layer);
        }

        //팀세팅 끝났을 때 이벤트.
        System.Action teamSettingIsDone;
        public void AddListnerToTeamSettingIsDone(System.Action act)
        {
            teamSettingIsDone += act;
        }

        public int GetTeamLayerByPhotonViewID(int photonViewID)
        {
            return LayerMask.NameToLayer(teamInfoDic[photonViewID / 1000]);
        }


        /*
        public void SetMyTeamInfo(int myTeamLayer, params Hero[] heroes)
        {
            this.myTeamLayer = myTeamLayer;
            if (heroes == null || heroes.Length ==0)
            {
                Debug.LogError("SetMyTeamInfo 의 히어로 정보가 불충분");
                myTeamHeroes = new Hero[0];
            }
            this.myTeamHeroes = heroes;
        }
        public void SetEnemyTeamInfo(int enemyTeamLayer, params Hero[] heroes)
        {
            this.enemyTeamLayer = enemyTeamLayer;
            if (heroes == null || heroes.Length == 0)
            {
                Debug.LogError("SetEnemyTeamInfo 의 히어로 정보가 불충분");
                enemyHeroes = new Hero[0];
            }
            this.enemyHeroes = heroes;
        }
        */
    }
}