using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VersionCode : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        TMP_Text textVersion = transform.GetComponent<TMP_Text>();
        textVersion.text = "Version: " + Application.version;
    }
}