using System.Collections.Generic;

namespace BetJson
{
    [System.Serializable]
    public class ButtonBet
    {
        public int amount { get; set; }
        public string state { get; set; }

        public ButtonBet()
        {
            amount = 0;
            state = "";
        }
    }

    [System.Serializable]
    public class Root
    {
        public int total_amount { get; set; }
        public ButtonBet button_bet { get; set; }

        public Root()
        {
            total_amount = 0;
            button_bet = new ButtonBet();
        }
    }

    namespace Response
    {
        public class Decrypted
        {
            public List<List<string>> result { get; set; }
            public int model_map { get; set; }
            public long balance { get; set; }
        }

        public class Root
        {
            public bool status { get; set; }
            public string message { get; set; }
            public string data { get; set; }
            public Decrypted decrypted { get; set; }
            public string type { get; set; }
        }
    }

    namespace ResponseSweep
    {
        public class Decrypted
        {
            public bool trap { get; set; }
            public string prev_step { get; set; }
            public string current_step { get; set; }
            public int dice_number { get; set; }
            public string dice_history { get; set; }
            public int model_map { get; set; }
            public string item_feature { get; set; }
            public List<List<string>> result { get; set; }
            public int multiplier { get; set; }
            public int amount_to_collect { get; set; }
            public long balance { get; set; }
        }

        public class Root
        {
            public bool status { get; set; }
            public string message { get; set; }
            public string data { get; set; }
            public Decrypted decrypted { get; set; }
            public string type { get; set; }
        }
    }
}