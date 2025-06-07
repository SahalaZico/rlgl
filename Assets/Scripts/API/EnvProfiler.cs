using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnvProfile", menuName = "Env Profile")]
public class EnvProfiler : ScriptableObject
{
    public string master = "https://apin22dev.saibara619.xyz";
    public string game = "https://fomn22dev.saibara619.xyz";
    public string socket = "https://donn22dev.saibara619.xyz/sim/";
    public bool useParser = true;
    public string code = "FOM00001";
    public string userToken = "abcd123452";
    public string agentName = "meja-hoki";
}
