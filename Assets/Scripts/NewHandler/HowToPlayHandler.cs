using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HowToPlayHandler : Singleton<HowToPlayHandler>
{
    [Header("UI Parent")]
    [SerializeField] GameObject uiObject;
    [SerializeField] Transform content;
    [SerializeField] Transform navigation;

    [Header("Button")]
    [SerializeField] Button buttonClose;
    [SerializeField] Button buttonPrev;
    [SerializeField] Button buttonNext;

    int actualMaxPage = 7;
    int page, startPage, endPage;

    private void Awake()
    {
        //PlayerPrefs.DeleteAll();

        uiObject.SetActive(false);

        buttonClose.onClick.AddListener(OnClickButtonClose);
        buttonPrev.onClick.AddListener(OnClickButtonPrev);
        buttonNext.onClick.AddListener(OnClickButtonNext);
    }
    
    #region Public Function
    public void Show(int tutorialType = 0)
    {
        switch (tutorialType)
        {
            case 0:
                page = 1;
                startPage = page;
                endPage = actualMaxPage;

                break;
            case 1:
                int tutorial1 = PlayerPrefs.GetInt("Tutorial1", 0);
                if (tutorial1 == 1) return;

                PlayerPrefs.SetInt("Tutorial1", 1);

                page = 1;
                startPage = page;
                endPage = 1;

                break;
            case 2:
                int tutorial2 = PlayerPrefs.GetInt("Tutorial2", 0);
                if (tutorial2 == 1) return;

                PlayerPrefs.SetInt("Tutorial2", 1);

                page = 2;
                startPage = page;
                endPage = 5;

                break;
            case 3:
                int tutorial3 = PlayerPrefs.GetInt("Tutorial3", 0);
                if (tutorial3 == 1) return;

                PlayerPrefs.SetInt("Tutorial3", 1);

                page = 6;
                startPage = page;
                endPage = 7;

                break;
        }
        
        RefreshInformation();

        uiObject.SetActive(true);
    }
    #endregion

    #region Private Function
    private void RefreshInformation()
    {
        for(int i=1; i <= actualMaxPage; i++)
        {
            bool isShow = (i == page);
            navigation.Find("Page" + i).Find("Active").gameObject.SetActive(isShow);
            content.Find("" + i).gameObject.SetActive(isShow);

            bool isNavigationShow = (i >= startPage) && (i <= endPage);
            navigation.Find("Page" + i).gameObject.SetActive(isNavigationShow);
        }
    }
    #endregion

    #region Listener
    private void OnClickButtonClose()
    {
        Debug.Log("button close clicked..");
        uiObject.SetActive(false);
    }

    private void OnClickButtonPrev()
    {
        Debug.Log("button prev clicked..");

        page -= 1;
        if (page < startPage)
            page = endPage;

        RefreshInformation();
    }

    private void OnClickButtonNext()
    {
        Debug.Log("button next clicked..");

        page += 1;
        if (page > endPage)
            page = startPage;

        RefreshInformation();
    }
    #endregion
}