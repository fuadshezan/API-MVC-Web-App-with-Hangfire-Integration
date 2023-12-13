namespace P3.API.Models.DTO
{
    public class HistoryDataDailyMain
    {
        public string Symbol { get; set; }
        public List<HistoryDataDaily> Historical { get; set; }
    }
}
