using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIButtonToggle : Button
{
    public delegate void EventStateChanged(bool state);

    [SerializeField] protected bool autoStart = true;
    [SerializeField] protected bool initialState = true;
    [SerializeField] protected GameObject stateOff = null;
    [SerializeField] protected GameObject stateOn = null;

    protected bool currentState = false;
    public bool CurrentState
    {
        get
        {
            return currentState;
        }
    }

    public EventStateChanged OnStateChanged;

    public void OnClickToggle()
    {
        bool input = !currentState;
        SetState(input);
    }

    public void SetState(bool state, bool notify = true)
    {
        currentState = state;
        stateOff.SetActive(!currentState);
        stateOn.SetActive(currentState);
        if (notify)
            OnStateChanged?.Invoke(currentState);
    }

    protected override void Start()
    {
        if (autoStart)
        {
            currentState = initialState;
            SetState(currentState, false);
        }
        base.Start();
    }
}
