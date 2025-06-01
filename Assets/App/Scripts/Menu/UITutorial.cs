using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.UI;
using Vuplex.WebView;

public class UITutorial : MonoBehaviour
{
    public static UITutorial Instance { get; private set; }

    [SerializeField] private CanvasWebViewPrefab canvasWeb = null;
    public CanvasWebViewPrefab CanvasWeb
    {
        get
        {
            return canvasWeb;
        }
    }

    [SerializeField] private GameObject container = null;
    [SerializeField] private CanvasGroup layout = null;

    [SerializeField] protected List<GameObject> contents = new List<GameObject>();
    [SerializeField] protected List<UITutorialNode> nodes = new List<UITutorialNode>();

    [SerializeField] protected UITutorialBtn btnPrev = null;
    [SerializeField] protected UITutorialBtn btnNext = null;

    [SerializeField] protected List<LocalizeTutorContent> states = new List<LocalizeTutorContent>();
    public List<LocalizeTutorContent> States
    {
        get
        {
            return states;
        }
        set
        {
            states = value;
        }
    }

    int currentIndex = 0;

    public bool IsActiveMenu()
    {
        if (container == null)
            return false;

        return container.activeSelf;
    }

    public async void OpenPageByIndex(int index)
    {
        string codeLang = LocalizationManager.Instance.DefaultLang;
        LocalizeTutorContent content = states.Where(x => x.Code == codeLang).FirstOrDefault();
        if (content == null)
            return;

        await canvasWeb.WaitUntilInitialized();

        canvasWeb.WebView.LoadHtml(content.Content[index]);
        //foreach (var content in contents) {
        //    if (content == null) continue;

        //    content.SetActive(false);
        //}

        //contents[index].SetActive(true);
    }

    protected void RenderNavigations()
    {
        string codeLang = LocalizationManager.Instance.DefaultLang;
        LocalizeTutorContent content = states.Where(x => x.Code == codeLang).FirstOrDefault();
        if (content == null)
            return;

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] == null)
                continue;

            nodes[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < content.Content.Count; i++) {
            if (nodes[i] == null)
                continue;

            nodes[i].gameObject.SetActive(true);
        }
    }

    protected void ValidationButtons()
    {
        string codeLang = LocalizationManager.Instance.DefaultLang;
        LocalizeTutorContent content = states.Where(x => x.Code == codeLang).FirstOrDefault();
        if (content == null)
            return;

        btnNext.interactable = (currentIndex < content.Content.Count - 1);
        btnPrev.interactable = (currentIndex > 0);
    }

    protected void OnClickOpenPage(int index)
    {
        OpenPageByIndex(index);

        foreach (var node in nodes)
        {
            if (node == null) continue;

            node.SetActiveNode(false);
        }

        currentIndex = index;
        nodes[currentIndex].SetActiveNode(true);
        ValidationButtons();
        AudioManager.Instance.Play("sfx", "confirm");
    }

    public void SetPageEventByIndex(int index)
    {
        OpenPageByIndex(index);

        foreach (var node in nodes)
        {
            if (node == null) continue;

            node.SetActiveNode(false);
        }

        currentIndex = index;
        nodes[currentIndex].SetActiveNode(true);
        ValidationButtons();
    }

    public void SetActive(bool state)
    {
        AudioManager.Instance.Play("sfx", "confirm");
        container?.SetActive(state);
        if (layout != null)
            layout.alpha = state ? 1 : 0;

        switch (state)
        {
            case true:
                RenderNavigations();
                SetPageEventByIndex(0);
                break;
            default:
                break;
        }
    }

    public void OnClickPrev()
    {
        int newIndex = currentIndex;
        newIndex--;
        if (newIndex < 0)
            newIndex = 0;

        OnClickOpenPage(newIndex);
    }

    public void OnClickNext()
    {
        string codeLang = LocalizationManager.Instance.DefaultLang;
        LocalizeTutorContent content = states.Where(x => x.Code == codeLang).FirstOrDefault();
        if (content == null)
            return;

        int newIndex = currentIndex;
        newIndex++;
        if (newIndex >= content.Content.Count - 1)
            newIndex = content.Content.Count - 1;

        OnClickOpenPage(newIndex);
    }

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        foreach (var node in nodes) {
            if (node == null) continue;

            int indexFetch = node.IndexPage;
            node.onClickNode.RemoveAllListeners();
            node.onClickNode.AddListener(() => OnClickOpenPage(indexFetch));
        }

        SetPageEventByIndex(0);
    }
}
