using System;

[Serializable]
public class UserDataResponse
{
    public bool status = false;
    public string message = string.Empty;
    public Data data = new Data();
}

[Serializable]
public class Data
{
    public Player player = new Player();
    public Game game = new Game();
}

[Serializable]
public class Player
{
    public string player_id = string.Empty;
    public string agent_id = string.Empty;
    public string player_name = string.Empty;
    public double player_balance = 1350000;
    public string player_currency = string.Empty;
    public string player_language = string.Empty;
    public string player_session = string.Empty;
    public string player_last_active = string.Empty;
    public LastBet last_bet = new LastBet();
}

[Serializable]
public class LastBet
{
    public string state = string.Empty;
    public int level = 0;
    public double amount = 13500000;
    public float multiplier = 0f;
    public double amount_to_collect = 0;
}

[Serializable]
public class Game
{
    public string game_code = string.Empty;
    public string lobby_url = string.Empty;
    public LimitBet limit_bet = new LimitBet();
    public long[] chip_base = new long[] { };
    public PrizeDetail prize_detail = new PrizeDetail();
    public string[] running_text = new string[] { };
    public Sounds sounds = new Sounds();
}

[Serializable]
public class LimitBet
{
    public int minimal = 0;
    public int maximal = 0;
    public int minimal_50 = 0;
    public int maximal_50 = 0;
    public int multiplication = 0;
    public int multiplication_50 = 0;
}

[Serializable]
public class PrizeDetail
{
    public long stage_1 = 0;
    public long stage_2 = 0;
    public long stage_3 = 0;
}

[Serializable]
public class Sounds
{
    public bool effect = false;
    public bool music = false;
}