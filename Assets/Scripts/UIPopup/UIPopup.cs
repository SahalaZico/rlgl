using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIPopup : MonoBehaviour
{
    public static UIPopup Instance { get; protected set; }

    [SerializeField] protected RectTransform container = null;
    [SerializeField] protected TMP_Text detail = null;
    [SerializeField] protected TMP_Text textTitle = null;

    public void Close()
    {
        container.gameObject.SetActive(false);
        detail.text = "";
        textTitle.text = "";
    }

    public void OnClickConfirm()
    {
        Close();
    }

    public void Show(string title, string content)
    {
        container.gameObject.SetActive(true);
        detail.text = content;
        textTitle.text = title;
    }

    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }
}
