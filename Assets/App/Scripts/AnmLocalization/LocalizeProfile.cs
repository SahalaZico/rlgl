using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LocalizeProfile
{
    [SerializeField]
    protected string code = "en";
    public string Code
    {
        get
        {
            return code;
        }
        set
        {
            code = value;
        }
    }

    [SerializeField]
    protected string text = "";
    public string Text
    {
        get
        {
            return text;
        }
        set
        {
            text = value;
        }
    }

    public LocalizeProfile()
    {
        code = "en";
        text = "";
    }

    public LocalizeProfile(string _code, string _text)
    {
        code = _code;
        text = _text;
    }
}
