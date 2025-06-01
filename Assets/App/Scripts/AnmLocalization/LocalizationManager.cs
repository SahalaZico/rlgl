using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class LocalizationManager : MonoBehaviour
{
    public delegate void TranslateText(string code);
    public static LocalizationManager Instance { get; set; }

    public TranslateText OnTranslateText;

    [SerializeField]
    protected List<LocalizeState> states = new List<LocalizeState>();

    [SerializeField]
    protected string defaultLang = "en";
    public string DefaultLang
    {
        get
        {
            return defaultLang;
        }
    }

    protected List<LocalizeTMPGUI> availableTextUIs = new List<LocalizeTMPGUI>();

    protected bool localLoaded = false;
    public bool LocalLoaded
    {
        get
        {
            return localLoaded;
        }
    }

    //Dictionary<string, string> labels =new Dictionary<string, string>();

    public string GetVirtualTranslateText(string keyTrans, string defaultTrans = "")
    {
        LocalizeState fetchState = states.Where(x => x.Code == defaultLang).FirstOrDefault();

        if (!fetchState.IDictionary.ContainsKey(keyTrans))
            return defaultTrans;

        string translateValue = fetchState.IDictionary[keyTrans];
        LocalizeProfile createProfile = new LocalizeProfile(fetchState.Code, translateValue);
        return createProfile.Text;
    }

    public void SwitchLanguageTo(string code)
    {
        defaultLang = code;

        OnTranslateText?.Invoke(defaultLang);
    }

    public void RefreshAllAvailableTextUI()
    {
        for (int i = 0; i < availableTextUIs.Count; i++) {
            if (availableTextUIs[i] == null)
                continue;

            for (int j = 0; j < states.Count; j++) {
                if (states[j] == null)
                    continue;

                LocalizeState fetchState = states[j];
                string translateValue = states[j].IDictionary[availableTextUIs[i].TranslateKey];
                LocalizeProfile createProfile = new LocalizeProfile(fetchState.Code, translateValue);
                availableTextUIs[i].Langs.Add(createProfile);
            }
        }
    }

    public void AddAvailableTextUI(LocalizeTMPGUI textUI)
    {
        availableTextUIs.Add(textUI);

        if (!string.IsNullOrEmpty(textUI.TranslateKey))
        {
            for (int j = 0; j < states.Count; j++)
            {
                if (states[j] == null)
                    continue;

                LocalizeState fetchState = states[j];
                string translateValue = states[j].IDictionary[textUI.TranslateKey];
                LocalizeProfile createProfile = new LocalizeProfile(fetchState.Code, translateValue);
                textUI.Langs.Add(createProfile);
            }
        }

        OnTranslateText?.Invoke(defaultLang);
    }

    protected IEnumerator IEnumReadStreamLang()
    {
        string filePath = "";
        try
        {
            filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "language.json");
        }
        catch (System.Exception errorEx)
        {
            Debug.LogError("Error catch: " + errorEx.Message);
            yield break;
        }

        yield return null;

        using (UnityWebRequest webRequest = UnityWebRequest.Get(filePath))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Data/Connection Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    string jsonParsed = webRequest.downloadHandler.text;
                    Debug.Log("Received: " + jsonParsed);

                    JArray rootArray = JArray.Parse(jsonParsed);
                    // Loop through each language object
                    foreach (JObject langObject in rootArray)
                    {
                        foreach (var property in langObject.Properties())
                        {
                            string languageCode = property.Name; // "en", "id", etc.
                            JObject languageContent = (JObject)property.Value;

                            JObject labelsByLang = (JObject)languageContent["labels"];
                            Dictionary<string, string> dictionary = labelsByLang.ToObject<Dictionary<string, string>>();

                            LocalizeState stateInput = new LocalizeState(languageCode, dictionary);
                            states.Add(stateInput);

                            // Now get 'how_to_play'
                            JObject tutorial = (JObject)languageContent["how_to_play"];

                            // Get 'pages' array
                            JArray pages = (JArray)tutorial["pages"];

                            List<string> contents = new List<string>();
                            // Example: Read all page contents
                            foreach (JObject page in pages)
                            {
                                string content = page["content"].ToString();
                                contents.Add(content);
                            }

                            LocalizeTutorContent localTutor = new LocalizeTutorContent(languageCode, contents);
                            UITutorial.Instance.States.Add(localTutor);
                        }
                    }
                    RefreshAllAvailableTextUI();
                    localLoaded = true;
                    OnTranslateText?.Invoke(defaultLang);
                    break;
            }
        }

        yield return null;
    }

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(IEnumReadStreamLang());
    }
}
