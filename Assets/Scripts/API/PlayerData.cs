using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; protected set; }

    protected UserDataResponse user = null;
    public UserDataResponse User
    {
        get
        {
            return user;
        }
    }

    public void SetUserData(UserDataResponse input)
    {
        user = input;
    }

    void Awake()
    {
        PlayerData.Instance = this;
    }
}
