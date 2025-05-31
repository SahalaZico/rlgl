using Newtonsoft.Json;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CryptoCS: MonoBehaviour
{
    [System.Serializable]
    public class DataParse
    {
        public string data = "";

        public DataParse() {
            data = "";
        }
    }

    [System.Serializable]
    public class EncryptRoot
    {
        public string encrypted_data { get; set; }
    }

    [System.Serializable]
    public class DecryptRoot
    {
        public string decrypted_data { get; set; }
    }

    public static CryptoCS Instance { get; protected set; }

    protected APIHandler apiHandler = null;
    [SerializeField] protected string key = "";
    [Header("Encrypt URL")]
    [SerializeField] protected string linkEncrypt = "/game/enc-result";
    [Header("Encrypt URL")]
    [SerializeField] protected string linkDecrypt = "/game/dec-result";

    protected System.Action<bool, string> OnReturnEncryptCallback = null;
    protected System.Action<bool, string> OnReturnDecryptCallback = null;

    protected void Awake()
    {
        Instance = this;
    }

    protected void Start()
    {
        apiHandler = GetComponent<APIHandler>();
    }

    public void ReceiveEncryptedData(string encryptedData)
    {
        //Debug.Log("EncryptJS: " + encryptedData);
        //string returnJsonRaw = $"{{\"encrypted_data\":\"{encryptedData}\"}}";
        //Debug.Log("EncryptJS Parsed: " + returnJsonRaw);
        EncryptRoot encryptBody = new EncryptRoot()
        {
            encrypted_data = encryptedData
        };
        string encryptedJsonStr = JsonConvert.SerializeObject(encryptBody);
        Debug.Log("DecryptJS Parsed: " + encryptedJsonStr);
        OnReturnEncryptCallback?.Invoke(true, encryptedJsonStr);
    }

    public void ReceiveDecryptedData(string decryptedData)
    {
        //Debug.Log("DecryptJS: " + decryptedData);
        //string returnJsonRaw = $"{{\"decrypted_data\":\"{decryptedData}\"}}";
        //Debug.Log("DecryptJS Parsed: " + returnJsonRaw);
        DecryptRoot decryptBody = new DecryptRoot()
        {
            decrypted_data = decryptedData
        };
        string decryptedJsonStr = JsonConvert.SerializeObject(decryptBody);
        Debug.Log("DecryptJS Parsed: " + decryptedJsonStr);
        OnReturnDecryptCallback?.Invoke(true, decryptedJsonStr);
    }

    protected IEnumerator IEEncryptTask(DataParse data, string key, System.Action<bool, string> onReturn = null)
    {
#if UNITY_EDITOR
        string dataJsonStr = JsonConvert.SerializeObject(data, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.Default });
        using (UnityWebRequest www = UnityWebRequest.Post(apiHandler.EnvCache.game + linkEncrypt, dataJsonStr, "application/json"))
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
#else
        OnReturnEncryptCallback = onReturn;
        string jsCommand = $"encryptDataAndSendBack('{data.data}', '{this.key}', '{gameObject.name}', 'ReceiveEncryptedData')";
        Application.ExternalCall("eval", jsCommand);
#endif
        yield return null;
    }

    public void EncryptAESWithECB(DataParse data, string key, System.Action<bool, string> onReturn = null)
    {
        StartCoroutine(IEEncryptTask(data, key, onReturn));
    }

    protected IEnumerator IEDecryptTask(DataParse data, string key, System.Action<bool, string> onReturn = null)
    {
#if UNITY_EDITOR
        string dataJsonStr = JsonConvert.SerializeObject(data, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.Default });
        using (UnityWebRequest www = UnityWebRequest.Post(apiHandler.EnvCache.game + linkDecrypt, dataJsonStr, "application/json"))
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
#else
        OnReturnDecryptCallback = onReturn;
        string jsCommand = $"decryptDataAndSendBack('{data.data}', '{this.key}', '{gameObject.name}', 'ReceiveDecryptedData')";
        Application.ExternalCall("eval", jsCommand);
#endif
        yield return null;
    }

    public void DecryptAESWithECB(DataParse data, string key, System.Action<bool, string> onReturn = null)
    {
        StartCoroutine(IEDecryptTask(data, key, onReturn));
    }
}
