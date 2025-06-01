using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LocalizeTutorContent
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

    [SerializeField]
    protected List<string> contents = new List<string>();
    public List<string> Content
    {
        get
        {
            return contents;
        }
        set
        {
            contents = value;
        }
    }

    public LocalizeTutorContent()
    {
        code = "en";
        contents = new List<string>();
    }

    public LocalizeTutorContent(string _code, List<string> _content)
    {
        code = _code;
        contents = _content;
    }
}
