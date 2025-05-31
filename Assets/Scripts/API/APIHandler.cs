using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class APIHandler : MonoBehaviour
{
    public static APIHandler Instance { get; protected set; }

    protected EnvProfiler envCache = null;
    public EnvProfiler EnvCache
    {
        get
        {
            return envCache;
        }
    }

    [Header("API List")]
    [SerializeField] string urllogin = "/api/operator/auth-game/?agent=meja-hoki&game={0}&token={1]";
    [SerializeField] string urlData = "/api/auth/data";
    [SerializeField] string urlSendBet = "/game/send_bet";
    [SerializeField] protected string urlHistory = "/game/history";

    [Header("Response")]
    [SerializeField] public UserDataResponse userDataResponse;
    [SerializeField] public InitialBet initialBet;

    [Header("Event")]
    [SerializeField] UnityEvent<UserDataResponse> OnReturnAuthData;

    [Header("Socket")]
    [SerializeField] public ClientSocketIO socket = null;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        userDataResponse = new UserDataResponse();
        initialBet = new InitialBet();

        StartCoroutine(ReadEnvIE(()=>
        {
#if UNITY_EDITOR
            StartCoroutine(TriggerLoginIE(OnStartAuthData));
#else
            OnStartAuthData(null);
#endif
        }));
    }

    #region Public Function
    public void OnStartAuthData(UserDataResponse userData)
    {
        StartCoroutine(TriggerAuthDataIE((user) =>
        {
            PlayerData.Instance.SetUserData(user);
            socket?.SetUrl(envCache.socket);
            socket?.DoUseParser(envCache.useParser);
            Debug.Log("Save to playerdata");
        }));
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

    public void GetHistory(Action<bool, string> onReturn)
    {
        StartCoroutine(IEHistory(onReturn));
    }

    public void SendBet(string dataRequest, Action<bool, string> onReturn = null)
    {
        Debug.Log("Raw request: " + dataRequest);
        CryptoCS.DataParse body = new CryptoCS.DataParse();
        body.data = dataRequest;

        CryptoCS.Instance.EncryptAESWithECB(body, "", (bool success, string returnRes) =>
        {
            if (!success)
            {
                onReturn?.Invoke(false, "");
                return;
            }

            CryptoCS.EncryptRoot returnObj = JsonConvert.DeserializeObject<CryptoCS.EncryptRoot>(returnRes);
            CryptoCS.DataParse dataClass = new CryptoCS.DataParse()
            {
                data = returnObj.encrypted_data
            };
            string dataEncryptJson = JsonConvert.SerializeObject(dataClass);

            StartCoroutine(SendBetIE(dataEncryptJson, onReturn));
        });
    }
    #endregion

    #region Private Function
    private IEnumerator ReadEnvIE(Action OnReturn = null)
    {
        var envReader = Resources.LoadAsync<EnvProfiler>("EnvProfile");

        while (!envReader.isDone)
        {
            yield return null;
        }

        envCache = envReader.asset as EnvProfiler;
        OnReturn?.Invoke();
    }

    private IEnumerator TriggerLoginIE(Action<UserDataResponse> nextAction = null)
    {
        string formattedLink = string.Format(envCache.master + urllogin, envCache.code, envCache.userToken);
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
            Debug.Log("Login: " + responseJson);
            //UserDataResponse response = JsonUtility.FromJson<UserDataResponse>(responseJson);
            nextAction?.Invoke(new UserDataResponse ());
        }
    }

    private IEnumerator TriggerAuthDataIE(Action<UserDataResponse> nextAction = null)
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

    private IEnumerator SendBetIE(string data, Action<bool, string> onReturn = null)
    {
        using (UnityWebRequest www = UnityWebRequest.Post(envCache.game + urlSendBet, data, "application/json"))
        {
            www.downloadHandler = new DownloadHandlerBuffer();
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                var resultText = www.downloadHandler.text;
                onReturn?.Invoke(false, resultText);
                Debug.LogError(www.error);
            }
            else
            {
                var resultText = www.downloadHandler.text;
                Debug.Log(resultText);

                BetResponse responseBet = JsonConvert.DeserializeObject<BetResponse>(resultText);
                CryptoCS.DataParse dataClass = new CryptoCS.DataParse();
                dataClass.data = responseBet.data;

                CryptoCS.Instance.DecryptAESWithECB(dataClass, "", (isSuccess, returnRes) =>
                {
                    if (!isSuccess)
                    {
                        onReturn?.Invoke(false, "");
                        return;
                    }

                    CryptoCS.DecryptRoot returnObj = JsonConvert.DeserializeObject<CryptoCS.DecryptRoot>(returnRes);

                    onReturn?.Invoke(true, returnObj.decrypted_data);
                    Debug.Log(returnObj.decrypted_data);
                });
            }
        }
    }
    #endregion
}