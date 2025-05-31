using System;

[Serializable]
public class BetResponse
{
    public string status = string.Empty;
    public string message = string.Empty;
    public string data = string.Empty;
    public string type = string.Empty;
}

///---///

[Serializable]
public class InitialBet
{
    public InitialBetBonus[] bet_bonus = new InitialBetBonus[] { };
    public float multiplier = 10.0f;
    public float maximum_multiplier = 10.0f;
    public long amount_to_collect = 0;
    public long balance = 1350000;
}

[Serializable]
public class InitialBetBonus
{
    public string type = ""; // money, add_time, speed_up, electrocute, bomb
    public float show_at_multiplier = 0f;
    public float value = 0f;
}