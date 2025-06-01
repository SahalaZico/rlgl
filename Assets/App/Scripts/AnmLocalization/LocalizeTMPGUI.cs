using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LocalizeTMPGUI : TextMeshProUGUI
{
    [SerializeField]
    protected string translate_key = "";
    public string TranslateKey
    {
        get
        {
            return translate_key;
        }
    }

    [SerializeField]
    protected List<LocalizeProfile> langs = new List<LocalizeProfile>();
    public List<LocalizeProfile> Langs
    {
        get
        {
            return langs;
        }
        set
        {
            langs = value;
        }
    }

    public void OnTranslateTMP(string code)
    {
        LocalizeProfile fetchProfile = langs.Where(x => x.Code == code).FirstOrDefault();
        if (fetchProfile != null)
        {
            this.text = fetchProfile.Text;
        }
    }

    protected override void Start()
    {
        base.Start();

        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnTranslateText += this.OnTranslateTMP;
            LocalizationManager.Instance.AddAvailableTextUI(this);
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnTranslateText -= this.OnTranslateTMP;
        }
    }

    public override void Rebuild(CanvasUpdate update)
    {
        base.Rebuild(update);
    }
}
