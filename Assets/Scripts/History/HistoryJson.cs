using System.Collections.Generic;

namespace HistoryJson
{
    public class Data
    {
        public int round_id { get; set; }
        //public List<List<string>> result { get; set; }
        public string player_id { get; set; }
        public string agent_id { get; set; }
        public string created_date { get; set; }
        public Entry data { get; set; }
    }

    public class Entry
    {
        public DetailBet detail_bet { get; set; }
        public double total_amount { get; set; }
        public double total_win { get; set; }
        public double last_balance { get; set; }
    }

    public class DetailBet
    {
        public double amount { get; set; }
        public string state { get; set; }
        public string multiplier { get; set; }
    }

    public class Root
    {
        public bool status { get; set; }
        public string message { get; set; }
        public List<Data> data { get; set; }
    }
}
