using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BestHTTP.SocketIO3;
using BestHTTP.SocketIO3.Parsers;
using BestHTTP.SocketIO3.Events;
using UnityEditor;
using System;
using UnityEngine.Events;
using Newtonsoft.Json;

public class ClientSocketIO : MonoBehaviour
{
    public static ClientSocketIO Instance { get; protected set; }

    [SerializeField] protected bool autoConnect = true;
    [SerializeField] protected bool useParser = true;
    [SerializeField] protected string url = "https://donn22dev.saibara619.xyz/sim/";

    protected SocketManager client = null;

    [Header("Event")]
    [SerializeField] UnityEvent<SocketIO.AnomaliJSON.Response.RangeData.Root> OnReturnRangeData;
    [SerializeField] UnityEvent<SocketIO.AnomaliJSON.Response.RangeUpdate> OnReturnRangeUpdate;
    [SerializeField] UnityEvent<SocketIO.AnomaliJSON.Response.DollState> OnReturnDollUpdate;
    [SerializeField] UnityEvent<SocketIO.AnomaliJSON.Response.TimerState> OnReturnTimerUpdate;
    [SerializeField] UnityEvent<SocketIO.AnomaliJSON.Response.GameInfo> OnReturnBetConfirmed;
    [SerializeField] UnityEvent<SocketIO.AnomaliJSON.Response.GameInfo> OnReturnBetFailed;

    [SerializeField] UnityEvent OnReturnCaughtByDoll;
    [SerializeField] UnityEvent OnReturnStopMove;
    [SerializeField] UnityEvent OnReturnTimeUp;
    [SerializeField] UnityEvent<SocketIO.AnomaliJSON.Response.GameInfo> OnReturnCashoutSuccess;
    [SerializeField] UnityEvent<SocketIO.AnomaliJSON.Response.GameInfo> OnReturnRestrictRun;
    [SerializeField] UnityEvent<SocketIO.AnomaliJSON.Response.RangeCrash> OnReturnCrash;

    public void SetUrl(string input)
    {
        url = input;
    }

    public void DoUseParser(bool input)
    {
        useParser = input;
    }

    protected void OnInfoGame(SocketIO.AnomaliJSON.Response.GameInfo output)
    {
        Debug.Log("Info: " + output.type + ", " + output.message + ", " + output.status);
        if (output.status == false)
        {
            if (output.type != "game" && output.type != "cashOut")
            {
                UIPopup.Instance.Show("Error", output.message);
            }
        }

        if (output.type == "bet_confirmed")
        {
            if (output.status == true)
                OnReturnBetConfirmed?.Invoke(output);
        }
        else if (output.type == "bet_error_internal")
        {
            OnReturnBetFailed?.Invoke(output);
        }
        else if (output.type == "invalid_session_or_balance")
        {
            OnReturnBetFailed?.Invoke(output);
        }
        else if (output.type == "running_stopped")
        {
            if (output.status == true)
            {
                OnReturnStopMove?.Invoke();
            }
        }
        else if (output.type == "game" && output.message.Contains("caught by the doll"))
        {
            OnReturnCaughtByDoll?.Invoke();
        }
        else if (output.type == "game" && output.message.ToLower().Contains("time is up"))
        {
            OnReturnTimeUp?.Invoke();
        }
        else if (output.type == "game" && output.message.ToLower().Contains("cannot run"))
        {
            OnReturnRestrictRun?.Invoke(output);
        }
        else if (output.type == "cashOut" && output.message.ToLower().Contains("cash out successful"))
        {
            if (output.status == true)
            {
                try
                {
                    string serializeData = JsonConvert.SerializeObject(output.data);
                    Debug.Log("Reward Cashout: " + serializeData);
                } catch (System.Exception ex)
                {
                    Debug.Log("Error Data Cashout Success: " + ex.Message);
                }
                OnReturnCashoutSuccess?.Invoke(output);
            }
        }
    }

    protected void OnRangeData(SocketIO.AnomaliJSON.Response.RangeData.Root output)
    {
        OnReturnRangeData?.Invoke(output);
    }

    protected void OnRangeUpdate(SocketIO.AnomaliJSON.Response.RangeUpdate output)
    {
        OnReturnRangeUpdate?.Invoke(output);
    }

    protected void OnDollUpdate(SocketIO.AnomaliJSON.Response.DollState output)
    {
        OnReturnDollUpdate?.Invoke(output);
    }

    protected void OnTimerUpdate(SocketIO.AnomaliJSON.Response.TimerState output)
    {
        OnReturnTimerUpdate?.Invoke(output);
    }

    protected void OnCrash(SocketIO.AnomaliJSON.Response.RangeCrash output)
    {
        OnReturnCrash?.Invoke(output);
        Debug.Log("Crashed at " + output.range);
    }

    public void RegisterEvents()
    {
        if (client == null)
            return;

        client.Socket.On<SocketIO.AnomaliJSON.Response.GameInfo>("info", OnInfoGame);
        client.Socket.On<SocketIO.AnomaliJSON.Response.RangeData.Root>("rangeData", OnRangeData);
        client.Socket.On<SocketIO.AnomaliJSON.Response.RangeUpdate>("rangeUpdate", OnRangeUpdate);
        client.Socket.On<SocketIO.AnomaliJSON.Response.DollState>("dollUpdate", OnDollUpdate);
        client.Socket.On<SocketIO.AnomaliJSON.Response.TimerState>("timerUpdate", OnTimerUpdate);
        client.Socket.On<SocketIO.AnomaliJSON.Response.RangeCrash>("crash", OnCrash);
    }

    public void SendEvent(string eventName, params object[] args)
    {
        if (client == null)
            return;

        if (client.GetSocket() == null)
            return;

        client.Socket.Emit(eventName, args);
    }

    public void ConnectBasedOnAuthPlayer(UserDataResponse userData)
    {
        Debug.Log("Try connect to server based on userauth: " + userData.data.player.player_session + ", " + userData.data.player.agent_id);
        var playerData = userData.data.player;
        SocketOptions options = new SocketOptions();
        //options.AutoConnect = false;
        options.AdditionalQueryParams = new PlatformSupport.Collections.ObjectModel.ObservableDictionary<string, string>() {
            {"agent_id", playerData.agent_id},
            {"session_token", playerData.player_session}
        };
        options.ConnectWith = BestHTTP.SocketIO3.Transports.TransportTypes.WebSocket;

        client = new SocketManager(new System.Uri(url), options);
        if (useParser)
            client.Parser = new MsgPackParser();
        client.Socket.On("connect", () => Debug.Log("connected with SID: " + client.Handshake.Sid));
        //client.Open();
        RegisterEvents();
    }

    public void ConnectToServer()
    {
        Debug.Log("Try connect to server");

        SocketOptions options = new SocketOptions();

        var addQueryParams = new PlatformSupport.Collections.ObjectModel.ObservableDictionary<string, string>();

        addQueryParams.Add("agent_id", "meja-hoki");
        addQueryParams.Add("session_token", "abcd123455");

        options.AdditionalQueryParams = addQueryParams;
        options.ConnectWith = BestHTTP.SocketIO3.Transports.TransportTypes.WebSocket;

        client = new SocketManager(new System.Uri(url), options);
        if (useParser)
            client.Parser = new MsgPackParser();
        client.Socket.On("connect", () => Debug.Log("connected as " + client.Handshake.Sid));

        RegisterEvents();
    }

    protected void Awake() {
        Instance = this;
        if (autoConnect)
            ConnectToServer();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
