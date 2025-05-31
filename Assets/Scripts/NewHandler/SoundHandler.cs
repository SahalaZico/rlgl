using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

public class SoundHandler : Singleton<SoundHandler>
{
    [Header("UI Parent")]
    [SerializeField] GameObject uiObject;
    [SerializeField] Transform bgm;
    [SerializeField] Transform sfx;

    [Header("Button")]
    [SerializeField] Button buttonClose;
    [SerializeField] Button buttonBgm;
    [SerializeField] Button buttonSfx;
    [SerializeField] Button buttonConfirm;

    public enum SFX_NAME { alarmdoll = 1, pushrun = 2, placebetbutton = 3, helicopter = 4,
        boxbreak = 5, bonustimer = 6, bonusspeedup = 7, bonusexplosion = 8, bonuselectricbomb = 9,
        charactershot = 10, explosiontimeup = 11, youwin = 12, gameover = 13 };

    Sounds sounds;
    AudioSource currentBgm, currentSfx;
    bool isBgm, isSfx;

    private void Awake()
    {
        uiObject.SetActive(false);

        buttonClose.onClick.AddListener(() => OnClickButtonClose());
        buttonBgm.onClick.AddListener(OnClickButtonBgm);
        buttonSfx.onClick.AddListener(OnClickButtonSfx);
        buttonConfirm.onClick.AddListener(OnClickButtonConfirm);

        Init(); // dummy
    }

    #region Public Function
    public void Init()
    {
        //userDataResponse = StatusHandler.Instance.userData;
        sounds = new Sounds()
        {
             effect = true,
             music = true
        };
        isBgm = sounds.music;
        isSfx = sounds.effect;

        RefreshVolume();
    }

    public void Save()
    {
        //userDataResponse = StatusHandler.Instance.userData;
        sounds = new Sounds()
        {
            effect = isSfx,
            music = isBgm
        };

        RefreshVolume();
    }

    public void Show()
    {
        uiObject.SetActive(true);

        //Init();
        RefreshInformation();
    }

    public void PlayBgm(string bgmName)
    {
        Transform result = bgm.Find(bgmName);
        if (result == null)
        {
            Debug.Log("Gameobject " + bgmName + " not found");
            return;
        }

        if (currentBgm)
            currentBgm.Stop();

        AudioSource audioSource = result.GetComponent<AudioSource>();
        currentBgm = audioSource;
        currentBgm.Play();
    }

    public void PlayOneShotSFX(SFX_NAME sfxEnumName, bool isLoop = false)
    {
        Transform result = sfx.Find(sfxEnumName.ToString());
        if (result == null)
        {
            Debug.Log("Gameobject " + sfxEnumName.ToString() + " not found");
            return;
        }

        AudioSource audioSource = result.GetComponent<AudioSource>();
        if (isLoop != audioSource.loop)
            audioSource.loop = isLoop;
        audioSource.Play();
    }

    public void StopOneShotSFX(SFX_NAME sfxEnumName)
    {
        Transform result = sfx.Find(sfxEnumName.ToString());
        if (result == null)
        {
            Debug.Log("Gameobject " + sfxEnumName.ToString() + " not found");
            return;
        }

        AudioSource audioSource = result.GetComponent<AudioSource>();
        audioSource.Stop();
    }

    public void PlaySfx(SFX_NAME sfxEnumName, bool isIndependent = false)
    {
        Transform result = sfx.Find(sfxEnumName.ToString());
        if (result == null)
        {
            Debug.Log("Gameobject " + sfxEnumName.ToString() + " not found");
            return;
        }

        if (isIndependent)
        {
            Transform clone = Instantiate(result, sfx);
            clone.gameObject.name = result.gameObject.name + "_clone";

            AudioSource audioSourceClone = clone.GetComponent<AudioSource>();
            audioSourceClone.Play();

            if (!audioSourceClone.loop)
                Destroy(clone.gameObject, audioSourceClone.clip.length);

            return;
        }

        AudioSource audioSource = result.GetComponent<AudioSource>();
        currentSfx = audioSource;
        audioSource.Play();
    }

    public void StopCurrentSfx()
    {
        if (currentSfx)
            currentSfx.Stop();
    }

    public void DeleteIndipendentSfx(SFX_NAME sfxEnumName)
    {
        Transform result = sfx.Find(sfxEnumName.ToString() + "_clone");
        if (result)
            Destroy(result.gameObject);
    }
    #endregion

    #region Private Function
    private void RefreshVolume()
    {
        AudioSource[] bgmAudioSources = bgm.GetComponentsInChildren<AudioSource>();
        foreach (var bgmAudioSource in bgmAudioSources)
            bgmAudioSource.mute = !isBgm;

        AudioSource[] sfxAudioSources = sfx.GetComponentsInChildren<AudioSource>();
        foreach (var sfxAudioSource in sfxAudioSources)
            sfxAudioSource.mute = !isSfx;
    }

    private void RefreshInformation()
    {
        buttonBgm.transform.Find("Off").gameObject.SetActive(!isBgm);
        buttonBgm.transform.Find("On").gameObject.SetActive(isBgm);

        buttonSfx.transform.Find("Off").gameObject.SetActive(!isSfx);
        buttonSfx.transform.Find("On").gameObject.SetActive(isSfx);
    }
    #endregion

    #region Listener
    private void OnClickButtonClose(bool saveSettings = false)
    {
        Debug.Log("button close clicked..");
        uiObject.SetActive(false);

        if (!saveSettings)
            Init();
        else
            Save();
        RefreshInformation();
    }

    private void OnClickButtonBgm()
    {
        Debug.Log("button bgm clicked..");

        isBgm = !isBgm;
        RefreshInformation();
        RefreshVolume();
    }

    private void OnClickButtonSfx()
    {
        Debug.Log("button sfx clicked..");

        isSfx = !isSfx;
        RefreshInformation();
        RefreshVolume();
    }

    private void OnClickButtonConfirm()
    {
        Debug.Log("button confirm clicked..");

        OnClickButtonClose(true);

        //buttonClose.interactable = false;
        //buttonBgm.interactable = false;
        //buttonSfx.interactable = false;
        //buttonConfirm.interactable = false;

        //SettingPropertiesRequest settingPropertiesRequest = new SettingPropertiesRequest
        //{
        //    effect = isBgm,
        //    music = isSfx,
        //    language = userDataResponse.data.player.player_language
        //};
        //string dataParsedStr = JsonConvert.SerializeObject(settingPropertiesRequest, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.Default });

        //APIHandler.Instance.SendSettingProperties(dataParsedStr, (bool isSuccess, string returnRes) =>
        //{
        //    if (!isSuccess)
        //    {
        //        BetResponse responseBet = JsonConvert.DeserializeObject<BetResponse>(returnRes);
        //        PopupHandler.Instance.Show(new PopupForm
        //        {
        //            contentText = responseBet.message,
        //            yesText = "Retry",
        //            noText = "No",
        //            yesAction = () => { OnClickButtonConfirm(); },
        //            noAction = () => { }
        //        });

        //        return;
        //    }

        //    buttonClose.interactable = true;
        //    buttonBgm.interactable = true;
        //    buttonSfx.interactable = true;
        //    buttonConfirm.interactable = true;

        //    sounds.music = isBgm;
        //    sounds.effect = isSfx;
        //    uiObject.SetActive(false);
        //});
    }
    #endregion
}