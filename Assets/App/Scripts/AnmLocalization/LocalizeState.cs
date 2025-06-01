using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LocalizeState
{
    [SerializeField]
    protected string code = "en";
    public string Code
    {
        get
        {
            return code;
        }
    }

    protected Dictionary<string, string> dictionary = new Dictionary<string, string>();
    public Dictionary<string, string> IDictionary
    {
        get
        {
            return dictionary;
        }
        set
        {
            dictionary = value;
        }
    }

    public LocalizeState() {
        code = "en";
    }

    public LocalizeState(string _code, Dictionary<string, string> _dictionary)
    {
        code = _code;
        dictionary = _dictionary;
    }
}
