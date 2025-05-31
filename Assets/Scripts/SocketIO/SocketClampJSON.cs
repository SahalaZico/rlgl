using Newtonsoft.Json;
using SocketIO.AnomaliJSON.Response.DropItem;
using SocketIO.AnomaliJSON.Response.RangeData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SocketIO.AnomaliJSON
{
    namespace Body
    {
        public class Bet
        {
            public ButtonBet button_bet = new ButtonBet();
        }

        public class ButtonBet
        {
            public double amount = 0.0;
        }
    }

    namespace Response
    {
        namespace DropItem
        {
            public class DropMember
            {
                public string color { get; set; }
                public double range { get; set; }
                public string item { get; set; }
                public object value { get; set; }
            }
        }

        public class GameDataInfo
        {
            public double balance { get; set; }
            public double range = 0.0;
            public double winAmount = 0.0;
            public Dictionary<string, DropMember> drops { get; set; }
        }

        public class GameInfo
        {
            public bool status = true;
            public string type = "";
            public string message = "";

            public GameDataInfo data = new GameDataInfo();
        }

        public class RangeUpdate
        {
            public string range = "";
        }

        public class RangeCrash
        {
            public string range = "";
        }

        public class DollState
        {
            public string status { get; set; }
        }

        public class TimerState
        {
            public float remainingTime { get; set; }
        }

        namespace RangeData
        {
            public class _1
            {
                public string name { get; set; }
                public double from { get; set; }
                public double to { get; set; }
            }

            public class _2
            {
                public string name { get; set; }
                public double from { get; set; }
                public double to { get; set; }
            }

            public class _3
            {
                public string name { get; set; }
                public double from { get; set; }
                public double to { get; set; }
            }

            public class _4
            {
                public string name { get; set; }
                public double from { get; set; }
                public double to { get; set; }
            }

            public class GroupRange
            {
                [JsonProperty("1")]
                public RangeData._1 _1 { get; set; }

                [JsonProperty("2")]
                public RangeData._2 _2 { get; set; }

                [JsonProperty("3")]
                public RangeData._3 _3 { get; set; }

                [JsonProperty("4")]
                public RangeData._4 _4 { get; set; }
            }

            public class Root
            {
                public GroupRange groupRange { get; set; }
                public double baseMaxRange { get; set; }
            }
        }
    }
}
