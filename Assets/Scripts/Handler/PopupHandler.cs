using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupHandler : Singleton<PopupHandler>
{
    [Header("User Interface")]
    [SerializeField] GameObject uiObject;
    [SerializeField] TMP_Text textContent;
    [SerializeField] TMP_Text textYes;
    [SerializeField] TMP_Text textNo;
    [SerializeField] Button buttonYes;
    [SerializeField] Button buttonNo;

    private void Awake()
    {
        uiObject.SetActive(false);
    }

    #region Public Function
    public void Show(PopupForm form)
    {
        uiObject.SetActive(true);

        textContent.text = form.contentText;
        textYes.text = form.yesText;
        textNo.text = form.noText;

        buttonYes.gameObject.SetActive(form.yesAction != null);
        buttonNo.gameObject.SetActive(form.noAction != null);

        buttonYes.onClick.RemoveAllListeners();
        buttonYes.onClick.AddListener(() =>
        {
            uiObject.SetActive(false);
            form.yesAction?.Invoke();
        });

        buttonNo.onClick.RemoveAllListeners();
        buttonNo.onClick.AddListener(() =>
        {
            uiObject.SetActive(false);
            form.noAction?.Invoke();
        });
    }
    #endregion

    #region Private Function
    #endregion
}

public class PopupForm
{
    public string contentText = string.Empty;
    public string yesText = string.Empty;
    public string noText = string.Empty;
    public Action yesAction = null;
    public Action noAction = null;
}