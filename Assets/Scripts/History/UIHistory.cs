using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using HauntedHaven.Backend;

public class UIHistory : MonoBehaviour
{
    public static UIHistory Instance { get; private set; }

    [SerializeField] private GameObject container = null;
    [SerializeField] private CanvasGroup layout = null;
    [SerializeField] private List<HistoryEntry> entries = new List<HistoryEntry>();

    protected void OnRenderData(bool onReturnSuccess, string response)
    {
        if (!onReturnSuccess)
        {
            Debug.Log("Error Response");
            return;
        }

        HistoryJson.Root rootResponse = JsonConvert.DeserializeObject<HistoryJson.Root>(response);
        foreach (HistoryEntry entry in entries) {
            entry.ClearData();
        }

        List<HistoryJson.Data> entriesRaw = rootResponse.data;
        int indexRender = 0;
        for (int i = 0; i < entriesRaw.Count; i++) {
            if (indexRender >= 10)
                break;
            if (entriesRaw[i] == null)
                continue;

            entries[indexRender].SetData(entriesRaw[i]);
            indexRender++;
        }
    }

    public void SetActive(bool state)
    {
        container?.SetActive(state);
        if (layout != null)
            layout.alpha = state ? 1 : 0;

        if (state)
            APIManager.Instance.GetHistory(OnRenderData);
    }

    private void Awake()
    {
        UIHistory.Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
