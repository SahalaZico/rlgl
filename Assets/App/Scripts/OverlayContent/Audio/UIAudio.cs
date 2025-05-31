using HauntedHaven.Backend;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class UIAudio : MonoBehaviour
{
    public static UIAudio Instance { get; protected set; }

    [SerializeField] protected GameObject container = null;
    [SerializeField] protected AudioMixer mainMixer = null;
    [SerializeField] protected UIButtonToggle toggleBGM = null;
    [SerializeField] protected UIButtonToggle toggleSFX = null;

    [SerializeField] float originalSFX = 1f;
    [SerializeField] float originalBGM = 1f;

    bool cacheSfx, cacheBGM;

    public void OnStateBGMChanged(bool state)
    {
        mainMixer.SetFloat("bgm", state ? originalBGM : (Mathf.Log10(0.0001f) * 20f));
    }

    public void OnStateSFXChanged(bool state)
    {
        mainMixer.SetFloat("sfx", state ? originalSFX : (Mathf.Log10(0.0001f) * 20f));
    }

    public void SetActive(bool input)
    {
        AudioManager.Instance.Play("sfx", "confirm");
        container.SetActive(input);
    }

    protected void OnReturnUpdateSettings(bool success, string response)
    {
        if (!success)
        {
            Debug.LogError("Error APISettings: " + response);
        }
    }

    public void OnClickConfirm()
    {
        var body = new SettingsJson.RootRequest
        {
            effect = toggleSFX.CurrentState,
            music = toggleBGM.CurrentState
        };
        string bodyStr = JsonConvert.SerializeObject(body);
        APIManager.Instance.UpdateSettings(bodyStr, OnReturnUpdateSettings);

        cacheBGM = toggleBGM.CurrentState;
        cacheSfx = toggleSFX.CurrentState;

        SetActive(false);
    }

    public void OnClickClose()
    {
        toggleBGM.SetState(cacheBGM);
        toggleSFX.SetState(cacheSfx);

        SetActive(false);
    }

    public void Initialize(bool bgmState, bool sfxState)
    {
        mainMixer.SetFloat("bgm", bgmState ? originalBGM : (Mathf.Log10(0.0001f) * 20f));
        mainMixer.SetFloat("sfx", sfxState ? originalSFX : (Mathf.Log10(0.0001f) * 20f));

        toggleBGM.SetState(bgmState, false);
        toggleSFX.SetState(sfxState, false);

        cacheBGM = bgmState;
        cacheSfx = sfxState;

        toggleBGM.OnStateChanged += OnStateBGMChanged;
        toggleSFX.OnStateChanged += OnStateSFXChanged;

        Debug.Log("Initialize UI Audio Complete");
    }

    // Start is called before the first frame update
    protected void Awake()
    {
        UIAudio.Instance = this;
    }

    protected void Start()
    {
        mainMixer.SetFloat("bgm", (Mathf.Log10(0.0001f) * 20f));
        mainMixer.SetFloat("sfx", (Mathf.Log10(0.0001f) * 20f));
    }
}
