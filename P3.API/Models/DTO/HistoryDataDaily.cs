﻿namespace P3.API.Models.DTO
{
    public class HistoryDataDaily
    {
        public string Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal AdjClose { get; set; }
        public long Volume { get; set; }
        public long? UnadjustedVolume { get; set; }
        public decimal? Change { get; set; }
        public decimal? ChangePercent { get; set; }
        public decimal? Vwap { get; set; }
        public string? Label { get; set; }
        public decimal? ChangeOverTime { get; set; }
    }
}
