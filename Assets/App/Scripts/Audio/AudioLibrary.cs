using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class AudioEntry
{
    public string key = "default";
    public AudioSource source = null;
    protected bool silent = false;

    public AudioEntry()
    {
        key = "default";
        source = null;
        silent = false;
    }

    public void SetSilent(bool state)
    {
        silent = state;
        source.volume = silent ? 0f : 1f;
    }
}

[Serializable]
public class AudioLibrary
{
    public string index = "default";
    public List<AudioEntry> entries = new List<AudioEntry>();

    public void Stop(string key)
    {
        AudioEntry entry = entries.Where(x => x.key == key).FirstOrDefault();
        if (entry != null)
        {
            entry.source.Stop();
        }
    }

    public void Play(string key, bool doLoop)
    {
        AudioEntry entry = entries.Where(x => x.key == key).FirstOrDefault();
        if (entry != null)
        {
            entry.source.loop = doLoop;
            entry.source.Play();
        }
    }

    public void Play(string key)
    {
        AudioEntry entry = entries.Where(x => x.key == key).FirstOrDefault();
        if (entry != null)
        {
            entry.source.Play();
        }
    }
}
