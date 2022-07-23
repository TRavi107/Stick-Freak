using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.Runtime.InteropServices;

namespace RamailoGames
{
    public class ScoreAPI : MonoBehaviour
    {
        //public enum APIs { 
        //    ramailo,staging
        //}

        const string version = "0.3.10";

        //js library function that extracts csrf and gameid from webpage
        [DllImport("__Internal")]
        private static extern string GetData();

        //public APIs api;

        const string RAMAILO = "://ramailogames.com/";
        const string STAGING = "://staging.ramailogames.com/";

        //url of api
        static string URL = "http://ramailogames.com/";

        //score api instance (used to call coroutines from static functions)
        public static ScoreAPI instance { get; private set; }
        
        //csrf token and game_id required for making requests (set using js library function or calling SendMessage from webpage)
        string csrf { get; set; }
        public int game_id { get; private set; }
        public int competition_id { get; private set; }
        public bool competition { get; private set; }

        string totalscore = "totalscore";
        string highscore = "highscore";
        string coin = "coin";

        //Editor code so it runs on editor
#if UNITY_EDITOR || !UNITY_WEBGL || DEVELOPMENT_BUILD

        private void Awake()
        {

            if(instance)
            {
                DestroyImmediate(gameObject);
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(this);

                initplayerprefs();
            }

        }



        //actual code for webgl
#elif UNITY_WEBGL
        void Awake()
        {
            if(instance)
            {
                DestroyImmediate(gameObject);
            }
            else
            {                
                instance = this;
                DontDestroyOnLoad(this); 
                if(!forcePrefs)
                    setData();  //set csrf, gameid and URL protocol <- (weird ik) 
                initplayerprefs();
            }
            
        }
#endif

        private void initplayerprefs()
        {
            var game_name = Application.productName;
            totalscore += game_name;
            highscore += game_name;
            coin += game_name;

            if (!PlayerPrefs.HasKey(highscore))
            {
                PlayerPrefs.SetInt(highscore, 0);
            }
            if (!PlayerPrefs.HasKey(totalscore))
            {
                PlayerPrefs.SetInt(totalscore, 0);
            }
            if (!PlayerPrefs.HasKey(coin))
            {
                PlayerPrefs.SetInt(coin, 0);
            }
        }

        //set csrf and gameid
        public void setData(string data="")
        {
            //if data is empty means its jslib function call, else its called from webpage using SendMessage API
            if(data.Length==0)
            {
                data = GetData();
            }            
            root parsed = JsonUtility.FromJson<root>(data);
            URL = (!parsed.isStaging ? parsed.protocol + RAMAILO : parsed.protocol + STAGING);
            csrf = parsed.csrf;
            game_id = parsed.game_id;
            competition = parsed.competition;
            competition_id = parsed.competition_id;
            Debug.Log(version);
        }

        [SerializeField]
        bool forcePrefs = false;

        void SkipLogin()
        {
            forcePrefs = true;
        }

        /// <summary>
        /// static function to get user data, callback sends sucess flag(bool) and data(Data_RequestData). Check Data_RequestData class.
        /// </summary>

        public static void GetData(UnityAction<bool,Data_RequestData> callback=null)
        {
#if UNITY_EDITOR || !UNITY_WEBGL || DEVELOPMENT_BUILD
            GetDataFromPrefs(callback);
#elif UNITY_WEBGL
            if(instance.forcePrefs)
                GetDataFromPrefs(callback);    
            else
                instance.StartCoroutine(instance.requestdata(callback));   
#endif
        }

        private static void GetDataFromPrefs(UnityAction<bool, Data_RequestData> callback)
        {
            Data_RequestData res = new Data_RequestData();
            res.total_score = PlayerPrefs.GetInt(instance.totalscore);
            res.high_score = PlayerPrefs.GetInt(instance.highscore);
            res.coin = PlayerPrefs.GetInt(instance.coin);
            res.is_competition = true;
            res.king_of_game = true;
            res.base_xp = 100;
            res.level_up_score = 10;
            res.game_level = 0;
            callback?.Invoke(true, res);
        }

        /// <summary>
        ///static function to submit score, callback sends sucess flag(bool) and response message (string).
        /// </summary>
        public static void SubmitScore(int score, int playtime,int coin=0, UnityAction<bool,string> callback=null)
        {
#if UNITY_EDITOR || !UNITY_WEBGL || DEVELOPMENT_BUILD
            SubmitToPrefs(score, coin, callback);
#elif UNITY_WEBGL
            if(instance.forcePrefs)
            {
                SubmitToPrefs(score, coin, callback);
                return;
            }            
            WWWForm postdata = new WWWForm();
            postdata.AddField("score", score);
            postdata.AddField("game_id", instance.game_id);
            postdata.AddField("played_time", playtime);
            postdata.AddField("coin", coin);
            if(instance.competition)
            {                
                postdata.AddField("competition_id", instance.competition_id);                
            }            
            instance.StartCoroutine(instance.submitscore( postdata,callback));
#endif
        }

        private static void SubmitToPrefs(int score, int coin, UnityAction<bool, string> callback)
        {
            PlayerPrefs.SetInt(instance.totalscore, PlayerPrefs.GetInt(instance.totalscore) + score);
            if (PlayerPrefs.GetInt(instance.highscore) < score)
            {
                PlayerPrefs.SetInt(instance.highscore, score);
            }
            PlayerPrefs.SetInt(instance.coin, PlayerPrefs.GetInt(instance.coin) + coin);
            callback?.Invoke(true, "");
        }

        /// <summary>
        ///static function to submit score, callback sends sucess flag(bool) and response message (string).
        /// </summary>
        public static void SubmitScore(int score, int playtime, UnityAction<bool, string> callback = null)
        {
            SubmitScore(score, playtime, 0, callback);
        }

        /// <summary>
        /// Count as a gameplay
        /// </summary>
        /// <param name="callback"></param>
        public static void GameStart(UnityAction<bool> callback=null)
        {
#if UNITY_EDITOR || !UNITY_WEBGL || DEVELOPMENT_BUILD            
            callback?.Invoke(true);
#elif UNITY_WEBGL
            if(instance.forcePrefs)
            {
                callback?.Invoke(true);
                return;
            }
             var v = UnityWebRequest.Post(URL + "game/" + instance.game_id + "/count", new WWWForm());
            v.SetRequestHeader("Content-type", "application/x-www-form-urlencoded");
            v.SetRequestHeader("Accept", "application/json");
            v.SetRequestHeader("X-CSRF-TOKEN", instance.csrf);
            v.SendWebRequest().completed += (AsyncOperation obj) => { callback?.Invoke(v.result == UnityWebRequest.Result.Success); };
#endif
        }

        //Coroutines for webrequests
        IEnumerator submitscore( WWWForm postdata,UnityAction<bool,string> callback)
        {
            string apiurl = competition ?  (URL + "competition/score"): (URL + "game/score/store");
            var v = UnityWebRequest.Post( apiurl, postdata);
            v.SetRequestHeader("Content-type", "application/x-www-form-urlencoded");
            v.SetRequestHeader("Accept", "application/json");
            v.SetRequestHeader("X-CSRF-TOKEN", instance.csrf);
            yield return v.SendWebRequest();

            if (v.result != UnityWebRequest.Result.ConnectionError && v.result != UnityWebRequest.Result.ProtocolError)
            {
                var res = JsonUtility.FromJson<Root_response>(v.downloadHandler.text);
                //if (res.status)
                //{                 
                //    //increase player count
                //    v = UnityWebRequest.Post(URL + "game/" + instance.game_id + "/count", new WWWForm());
                //    v.SetRequestHeader("Content-type", "application/x-www-form-urlencoded");
                //    v.SetRequestHeader("Accept", "application/json");
                //    v.SetRequestHeader("X-CSRF-TOKEN", instance.csrf);
                //    yield return v.SendWebRequest();                    
                //}
                //callback
                callback?.Invoke(res.status, res.message);
            }
            else
            {
                callback?.Invoke(false, "Connection Error");
            }
        }
        IEnumerator requestdata(UnityAction<bool,Data_RequestData> callback)
        {
            string apiurl = competition ? (URL+"competition/score" + "?competition_id="+competition_id) : (URL + "game/score" + "?game_id=" + game_id);
            var v = UnityWebRequest.Get(apiurl);
            v.SetRequestHeader("Content-type", "application/x-www-form-urlencoded");
            var x = v.SendWebRequest();
            yield return x;
            if (v.result != UnityWebRequest.Result.ConnectionError && v.result != UnityWebRequest.Result.ProtocolError)
            {
                string data = v.downloadHandler.text;                
                var parseddata = JsonUtility.FromJson<root_RequestData>(data);
                callback?.Invoke(parseddata.status, parseddata.data);
            }
            else
            {
                callback?.Invoke(false, null);
            }
        }


        //classes to parse JSON data from server
        [System.Serializable]
        public class root
        {
            public string csrf;
            public int game_id;
            public int competition_id;
            public bool competition;
            public string protocol;
            public bool isStaging;
        }

        [System.Serializable]
        public class root_RequestData
        {
            public bool status;
            public string message;
            public Data_RequestData data;
        }        

        [System.Serializable]
        public class Root_response
        {
            public bool status;
            public string message;
        }        
    }
    [System.Serializable]
    public class Data_RequestData
    {
        public int high_score;
        public int total_score;
        public int base_xp;
        public int level_up_score;
        public int game_level;
        public bool king_of_game;
        public bool is_competition;
        public float coin;
    }

    
    public struct levelinfo
    {
        public int level;
        public float exp;
        public float exptonext;
    }

    public static class Helper
    {        
        public static levelinfo GetLevelInfo(int totalexp,int base_xp=100,float percent_increase=10)
        {
            int level = 0;
            float remaining = totalexp;
            float exptonext = base_xp;
            percent_increase = 1+(percent_increase/ 100);
            
            if(percent_increase<0 || base_xp<=0)
            {
                return new levelinfo {
                    level = level,
                    exptonext = exptonext,
                    exp = remaining
                };
            }

            while (remaining>=exptonext)
            {
                remaining -= exptonext;
                exptonext *= percent_increase;
                level++;                
            }

            return new levelinfo {
                level = level,
                exptonext = exptonext,
                exp = remaining
            };
        }

        public static levelinfo GetLevelInfo(Data_RequestData data)
        {
            return GetLevelInfo(data.total_score, data.base_xp, data.level_up_score);
        }
    }
}
