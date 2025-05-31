using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; protected set; }

    [SerializeField] protected List<AudioLibrary> libraries = new List<AudioLibrary>();

    public void Stop(string libIndex, string key)
    {
        AudioLibrary getLib = libraries.Where(x => x.index == libIndex).FirstOrDefault();
        if (getLib != null)
        {
            getLib.Stop(key);
        }
    }

    public void Play(string libIndex, string key, bool doLoop = false)
    {
        AudioLibrary getLib = libraries.Where(x => x.index == libIndex).FirstOrDefault();
        if (getLib != null) {
            getLib.Play(key, doLoop);
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }
}
