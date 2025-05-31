using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StatusHandler : Singleton<StatusHandler>
{
    [Header("Curent Status")]
    [SerializeField] public STATUS status = STATUS.OnSplashScreen;

    [Header("Event")]
    [SerializeField] public UnityEvent<STATUS> statusChangedEvent;

    public enum STATUS { OnSplashScreen = 1, OnBet = 2, OnGameplay = 3, OnResult = 4 };

    private void Awake()
    {
        status = STATUS.OnSplashScreen;
    }

    #region Public Function
    public void ChangeStatus(STATUS status, bool isBroadcast = false)
    {
        this.status = status;

        if (isBroadcast)
            statusChangedEvent?.Invoke(status);
    }
    #endregion
}