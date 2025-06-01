using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITutorialNode : Button
{
    [SerializeField] protected bool nodeActive = false;
    [SerializeField] protected GameObject nodeOff = null;
    [SerializeField] protected GameObject nodeOn = null;

    [SerializeField] protected int indexPage = 0;

    public int IndexPage
    {
        get
        {
            return indexPage;
        }
    }

    public UnityEvent onClickNode;

    public void OnClickNodeEvent()
    {
        onClickNode?.Invoke();
    }

    public void SetActiveNode(bool state)
    {
        nodeActive = state;
        nodeOff.SetActive(!nodeActive);
        nodeOn.SetActive(nodeActive);
    }

    protected override void Start()
    {
        base.Start();
    }
}
