using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Net;

namespace HauntedHaven.Backend
{
    public class APIManager : MonoBehaviour
    {
        public static APIManager Instance { get; protected set; }

        [System.Serializable]
        public class ENVJsonParser
        {
            public string master = "";
            public string game = "";
            public string code = "";
            public string socket = "";
            public bool useParser = false;

            public ENVJsonParser()
            {
                master = "";
                game = "";
                code = "";
                socket = "";
                useParser = false;
            }
        }

        protected EnvProfiler envCache = null;
        public EnvProfiler EnvCache {
            get
            {
                return envCache;
            }
        }

        [Header("API List")]
        [SerializeField]
        protected string urllogin = "/api/operator/auth-game/?agent=meja-hoki&game={0}&token=abcd12345";
        [SerializeField]
        protected string urlData = "/api/auth/data";
        [SerializeField]
        protected string urlSendBet = "/game/send_bet";
        [SerializeField]
        protected string urlHistory = "/game/history";
        [SerializeField]
        protected string urlSettings = "/game/settings/properties";
        [SerializeField]
        protected string urlResetGame = "/game/flush_lastbet";

        [SerializeField]
        protected UnityEvent<UserDataResponse> OnReturnAuthData;

        protected IEnumerator IEnumReadStreamEnv(System.Action OnReturn = null)
        {
            string filePath = "";
            try
            {
                filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "env.json");
            } catch (System.Exception errorEx)
            {
                Debug.LogError("Error catch: " + errorEx.Message);
                StartCoroutine(IEnumReadEnv(OnReturn));
                yield break;
            }
            using (UnityWebRequest webRequest = UnityWebRequest.Get(filePath))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();

                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                        Debug.LogError("Data/Connection Error: " + webRequest.error);
                        StartCoroutine(IEnumReadEnv(OnReturn));
                        break;
                    case UnityWebRequest.Result.ProtocolError:
                        Debug.LogError("HTTP Error: " + webRequest.error);
                        StartCoroutine(IEnumReadEnv(OnReturn));
                        break;
                    case UnityWebRequest.Result.Success:
                        string jsonString = webRequest.downloadHandler.text;
                        Debug.Log(jsonString);
                        ENVJsonParser parser = JsonConvert.DeserializeObject<ENVJsonParser>(jsonString);
                        envCache = ScriptableObject.CreateInstance<EnvProfiler>();
                        envCache.master = parser.master;
                        envCache.game = parser.game;
                        envCache.code = parser.code;
                        envCache.socket = parser.socket;
                        envCache.useParser = parser.useParser;
                        ClientSocketIO.Instance.DoUseParser(envCache.useParser);
                        ClientSocketIO.Instance.SetUrl(envCache.socket);
                        OnReturn?.Invoke();
                        Debug.Log($"Name: {envCache.game}, Value: {envCache.code}");
                        Debug.Log("Received: " + webRequest.downloadHandler.text);
                        break;
                }
            }
        }

        protected IEnumerator IEnumReadEnv(System.Action OnReturn = null)
        {
            Debug.Log("Read default env");
            var envReader = Resources.LoadAsync<EnvProfiler>("EnvProfile");

            while (!envReader.isDone)
            {
                yield return null;
            }

            envCache = envReader.asset as EnvProfiler;
            ClientSocketIO.Instance.DoUseParser(envCache.useParser);
            ClientSocketIO.Instance.SetUrl(envCache.socket);
            OnReturn?.Invoke();
        }

        protected IEnumerator TriggerLoginIE(Action<UserDataResponse> nextAction = null)
        {
            //?agent=meja-hoki&game={0}&token=abcd12345
            string formattedParam = string.Format("?agent={0}&game={1}&token={2}", envCache.agentName, envCache.code, envCache.userToken);
            string formattedLink = (envCache.master + urllogin + formattedParam);
            UnityWebRequest request = UnityWebRequest.Get(formattedLink);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error Login: " + request.error);
            }
            else
            {
                string responseJson = request.downloadHandler.text;
                Debug.Log("Login: " + responseJson);
                try
                {
                    UserDataResponse response = JsonUtility.FromJson<UserDataResponse>(responseJson);
                    nextAction?.Invoke(response);
                }
                catch (Exception exception)
                {
                    string message = exception.Message.ToLower();
                    if (message.Contains("invalid value"))
                    {
                        StartCoroutine(TriggerAuthDataIE((user) => { 
                            PlayerData.Instance.SetUserData(user);
                            UIAudio.Instance.Initialize(user.data.game.sounds.music, user.data.game.sounds.effect);
                        }));
                    }
                    Debug.Log("Error parse: " + exception.Message);
                }
            }
        }

        protected IEnumerator IEFlushLastBet(Action<bool, string> onReturn)
        {
            string formattedLink = envCache.game + urlHistory;
            UnityWebRequest request = UnityWebRequest.Get(formattedLink);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + request.error);
                onReturn?.Invoke(false, "");
            }
            else
            {
                string responseJson = request.downloadHandler.text;
                Debug.Log("History: " + responseJson);
                onReturn?.Invoke(true, responseJson);
            }
        }

        protected IEnumerator IEHistory(Action<bool, string> onReturn)
        {
            string formattedLink = envCache.game + urlHistory;
            UnityWebRequest request = UnityWebRequest.Get(formattedLink);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + request.error);
            }
            else
            {
                string responseJson = request.downloadHandler.text;
                Debug.Log("History: " + responseJson);
                onReturn?.Invoke(true, responseJson);
            }
        }

        protected IEnumerator TriggerAuthDataIE(Action<UserDataResponse> nextAction = null)
        {
            string formattedLink = envCache.game + urlData;
            UnityWebRequest request = UnityWebRequest.Get(formattedLink);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError ||
                request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("Error: " + request.error);
            }
            else
            {
                string responseJson = request.downloadHandler.text;
                Debug.Log("AuthData: " + responseJson);
                UserDataResponse response = JsonUtility.FromJson<UserDataResponse>(responseJson);
                nextAction?.Invoke(response);
                OnReturnAuthData?.Invoke(response);
            }
        }

        protected IEnumerator IESendBet(string data, Action<bool, string> onReturn = null)
        {
            using (UnityWebRequest www = UnityWebRequest.Post(envCache.game + urlSendBet, data, "application/json"))
            {
                www.downloadHandler = new DownloadHandlerBuffer();
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    onReturn?.Invoke(false, "");
                    Debug.LogError(www.error);
                }
                else
                {
                    var resultText = www.downloadHandler.text;
                    onReturn?.Invoke(true, resultText);
                    Debug.Log(resultText);
                }
            }
        }

        protected IEnumerator IEUpdateSettings(string data, Action<bool, string> onReturn = null)
        {
            using (UnityWebRequest www = UnityWebRequest.Post(envCache.game + urlSettings, data, "application/json"))
            {
                www.downloadHandler = new DownloadHandlerBuffer();
                yield return www.SendWebRequest();

                if (www.result != UnityWebRequest.Result.Success)
                {
                    onReturn?.Invoke(false, "");
                    Debug.LogError(www.error);
                }
                else
                {
                    var resultText = www.downloadHandler.text;
                    onReturn?.Invoke(true, resultText);
                    Debug.Log(resultText);
                }
            }
        }

        public void UpdateSettings(string data, Action<bool, string> onReturn = null)
        {
            StartCoroutine(IEUpdateSettings(data, onReturn));
        }

        public void SendBet(string data, Action<bool, string> onReturn = null)
        {
            StartCoroutine(IESendBet(data, onReturn));
        }

        public void FlushLastBet(Action<bool, string> onReturn)
        {
            StartCoroutine(IEFlushLastBet(onReturn));
        }
        
        public void GetHistory(Action<bool, string> onReturn)
        {
            StartCoroutine(IEHistory(onReturn));
        }

        public void OnStartAuthData(UserDataResponse userData)
        {
            StartCoroutine(TriggerAuthDataIE((user) => { 
                PlayerData.Instance.SetUserData(user);
                UIAudio.Instance.Initialize(user.data.game.sounds.music, user.data.game.sounds.effect);
            }));
        }

        protected void OnStartReturn()
        {
#if UNITY_EDITOR
            StartCoroutine(TriggerLoginIE(OnStartAuthData));
#else
            StartCoroutine(TriggerAuthDataIE((user) => {
                PlayerData.Instance.SetUserData(user);
                UIAudio.Instance.Initialize(user.data.game.sounds.music, user.data.game.sounds.effect);
            }));
#endif
        }

        protected void Awake()
        {
            Instance = this;
        }

        protected void Start()
        {
            StartCoroutine(IEnumReadStreamEnv(OnStartReturn));
        }
    }
}
