using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using P3.API.Models.Domain;
using P3.API.Models.DTO;
using P3.API.Repository.IRepository;

namespace P3.API.Controllers
{
	[Route("api/[controller]")]
    [ApiController]
    public class HistoricalDataController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IUnitOfWork _unitOfWork;

        public HistoricalDataController(IUnitOfWork unitofwork, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _unitOfWork = unitofwork;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }
        [HttpGet("test")]
        public async Task<IActionResult> TestConnection()
        {
			return Ok(new
			{
				Code = "200",
				Message = "Connected with the API project."
			});
		}


		[HttpPost("minutedata")]
		public async Task<IActionResult> GetMinuteDataFromFMP([FromBody] SymbolData model)
		{
			try
			{

				var apiKey = _configuration["APIInfo:Key"].ToString();
				var client = _httpClientFactory.CreateClient("baseurl");
				var response = await client.GetAsync($"historical-chart/1min/{model.Symbol}?from={model.Date}&to={model.Date}&apikey={apiKey}");
				if (response.IsSuccessStatusCode)
				{
					var reponseData = await response.Content.ReadAsStringAsync();
					var historyDataList = JsonConvert.DeserializeObject<List<HistoryDataMin>>(reponseData);
					if (historyDataList.Count == 0)
					{
						await _unitOfWork.HistoryData_1Min.ExecuteSQLProcedureAsync($"EXEC prcUpdateDownloadStatus 'Min','{model.Symbol}','{model.Date}','Not Found'");
						return Ok(new
						{
							Code = "200",
							Message = "Response data is null."
						});
					}
					DateTime first_row_date = DateTime.Parse(historyDataList[0].date);
					if (model.Date.Equals(first_row_date.ToString("yyyy-MM-dd")))
					{
						// Map the deserialized objects to your model
						var mappedDataList = new List<tblHistoryData_1Min>();
						foreach (var historyData in historyDataList)
						{
							var mappedData = new tblHistoryData_1Min
							{
								SymName = model.Symbol.ToUpper(), // Set appropriate value
								DtDate = DateTime.Parse(historyData.date),
								Open = historyData.open,
								High = historyData.high,
								Low = historyData.low,
								Close = historyData.close,
								Volume = historyData.volume,
								Change = historyData.open - historyData.close,
							};
							mappedDataList.Add(mappedData);
						}

						await _unitOfWork.HistoryData_1Min.CreateAsync(mappedDataList);
						await _unitOfWork.SaveAsync();
						await _unitOfWork.HistoryData_1Min.ExecuteSQLProcedureAsync($"EXEC prcUpdateDownloadStatus 'Min','{mappedDataList[0].SymName}','{first_row_date.Date}'");
					}
					return Ok();
				}
				else
				{
					return BadRequest(new
					{
						Code = "400",
						Message = $"An error occurred while inserting data your request (Minute History), custom error."
					});
				}

			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
				return BadRequest(new
				{
					Code = "400",
					Message = $"An error occurred while processing your request(Minute History). {ex.Message}. Inner: {ex.InnerException}"
				});
			}

		}

		[HttpPost("dailydata")]
        public async Task<IActionResult> GetDailyDataFromFmp([FromBody] SymbolData model)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("baseurl");
                var response = await client.GetAsync($"historical-price-full/{model.Symbol}?from={model.Date}&to={model.Date}&apikey=2b2bbacbc149bcba58903f591ae3d3c8");
                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var historyDataDaily = JsonConvert.DeserializeObject<HistoryDataDailyMain>(responseData);
                    if (historyDataDaily.Historical == null || historyDataDaily.Symbol== null)
                    {
						await _unitOfWork.HistoryData_1Min.ExecuteSQLProcedureAsync($"EXEC prcUpdateDownloadStatus 'Daily','{model.Symbol}','{model.Date}','Not Found'");

						return Ok(new
						{
							Code = "200",
							Message = "Response data is null."
						});
                    }
                    DateTime daily_date = DateTime.Parse(historyDataDaily.Historical[0].Date);
                    if (model.Date.Equals(daily_date.Date.ToString("yyyy-MM-dd")))
                    {
                        var mapped_data = new tblHistoryData
                        {
                            SymName = model.Symbol.ToUpper(),
                            DtDate = DateTime.Parse(model.Date),
                            Open = historyDataDaily.Historical[0].Open,
                            High = historyDataDaily.Historical[0].High,
                            Low = historyDataDaily.Historical[0].Low,
                            Close = historyDataDaily.Historical[0].Close,
                            Volume = historyDataDaily.Historical[0].Volume,
							Vwap = historyDataDaily.Historical[0].Vwap ?? 0,
                            Change = historyDataDaily.Historical[0].Change ?? 0
                        };
                        await _unitOfWork.tblHistoryData.CreateAsync(mapped_data);
                        await _unitOfWork.SaveAsync();
						// await _unitOfWork.tblHistoryData.ExecuteSQLProcedureAsync($"Exec prcInsertHistoryTempToMain 'Daily'");
						await _unitOfWork.tblHistoryData.ExecuteSQLProcedureAsync($"EXEC prcUpdateDownloadStatus 'Daily','{mapped_data.SymName}','{daily_date.Date}'");
						return Ok();
                    }
                }
                else
                {
                    return BadRequest(new
                    {
                        Code = "400",
                        Message = "An error occurred while processing your request (Daily data), custom error."
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Code = "400",
                    Message = $"An error occurred while processing your request (Daily data). {ex.Message} Inner: {ex.InnerException}"
                });
            }
            return NotFound();
        }
    }
}
