namespace P3.API.Models.DTO
{
    public class HistoryDataMin
    {
        public string date { get; set; }
        public decimal open { get; set; }
        public decimal low { get; set; }
        public decimal high { get; set; }
        public decimal close { get; set; }
        public long volume { get; set; }
    }

}
