using System;

public enum STATE { initial = 1, forge = 2, sweep = 3, cashout = 4 };

[Serializable]
public class InitialBetRequest
{
    public float total_amount = 0;
    public ButtonBet button_bet = new ButtonBet();
}

[Serializable]
public class ForgeCheckRequest
{
    public float total_amount = 0;
    public ButtonBetLevel button_bet = new ButtonBetLevel();
}

[Serializable]
public class SweepBetRequest
{
    public float total_amount = 0;
    public ButtonBet button_bet = new ButtonBet();
}

[Serializable]
public class CashoutRequest
{
    public float total_amount = 0;
    public ButtonBet button_bet = new ButtonBet();
}

[Serializable]
public class ButtonBet
{
    public float amount = 0;
    public string state = string.Empty;
}

[Serializable]
public class ButtonBetLevel
{
    public float amount = 0;
    public string state = string.Empty;
    public int level = 0;
}