[System.Serializable]
public class AnomaliRunnerBonusPoint
{
    public string key = "";
    public float range = -1f;
    public float baseValue = 0f;

    protected bool doSpawn = false;
    public bool DoSpawn
    {
        get
        {
            return doSpawn;
        }
        set
        {
            doSpawn = value;
        }
    }

    protected bool doOpen = false;
    public bool DoOpen
    {
        get
        {
            return doOpen;
        }
        set
        {
            doOpen = value;
        }
    }

    protected float spawnAt = -1f;
    public float SpawnAt
    {
        get
        {
            return spawnAt;
        }
        set
        {
            spawnAt = value;
        }
    }
}
