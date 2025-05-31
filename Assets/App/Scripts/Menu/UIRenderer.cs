using UnityEngine;

public class UIRenderer : MonoBehaviour
{
    protected static UIRenderer instance = null;
    public static UIRenderer Instance { get { return instance; } protected set { instance = value; } }

    [SerializeField] UIPage initialPage = null;
    [SerializeField] UIPage page = null;

    public void OpenPage(UIPage target)
    {
        if (page != null)
            page.SetActive(false);

        page = target;
        page.SetActive(true);
    }

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        if (initialPage == null)
            return;

        OpenPage(initialPage);
    }
}
