using Hangfire;
using Hangfire.Storage;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using NCrontab;
using Newtonsoft.Json;
using P3.Core.Data;
using P3.Web.Models;
using P3.Web.Models.DTO;
using System.Text;

namespace P3.Core.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly ApplicationDbContext _context;
		private readonly IHttpClientFactory _httpClientFactory;
		public readonly JobStorage _jobstorage;

		public HomeController(ILogger<HomeController> logger, ApplicationDbContext context,
								IHttpClientFactory httpClientFactory, JobStorage jobStorage)
		{
			_logger = logger;
			_context = context;
			_httpClientFactory = httpClientFactory;
			_jobstorage = jobStorage;
		}
		public IActionResult Index(string message = null)
		{
			string next_occurance_daily = get_job_details("History_dataDaily_1");
			string next_occurance_minute = get_job_details("History_dataMinute_2");
			ViewBag.NextOccuranceDaily = next_occurance_daily;
			ViewBag.NextOccuranceMinute= next_occurance_minute;
			ViewBag.Message = message;

			return View();
		}
		public async Task <IActionResult> TestApiConnection()
		{
			try
			{
				var Message = string.Empty;
				using var client = _httpClientFactory.CreateClient("baseurl");
				//Console.WriteLine(client.BaseAddress + "historicaldata/test");
				var response = await client.GetAsync($"/api/historicaldata/test");
				if (response.IsSuccessStatusCode)
				{
					var responseData = await response.Content.ReadAsStringAsync();
					var error_message = JsonConvert.DeserializeObject<ErrorModel>(responseData);
					Message = error_message.Message;
				}
				else
				{
					Message = "Not Connected With API";
				}
				Console.WriteLine("ViewBag.Message after assignment: " + Message);

				var routeValues = new RouteValueDictionary {
				  { "message", Message}
				};
				return RedirectToAction("Index","Home",routeValues);
			}
			catch
			{
				throw new Exception("An Error Occured In API Connection.");
			}
		}
		public string get_job_details(string jobId)
		{

			//IStorageConnection connection = JobStorage.Current.GetConnection();
			//JobData jobData = connection.GetJobData(jobId);
			//string stateName = jobData.State;
			////JobData jobDetails = connection.GetJobData(id);

			//            Console.WriteLine(jobData);
			using (var connection = JobStorage.Current.GetConnection())
			{
				var recurringJobs = connection.GetRecurringJobs();
				foreach (var recurringJob in recurringJobs)
				{
					if (recurringJob.Id == jobId)
					{
						TimeZoneInfo bdtimezone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Dhaka");
						CrontabSchedule schedule = CrontabSchedule.Parse(recurringJob.Cron);
						DateTime nextOccurrence = schedule.GetNextOccurrence(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, bdtimezone));
						return nextOccurrence.ToString("dd/M/yyyy h:mm:ss tt");
					}
				}
			}
			return null;
		}

		public IActionResult ProcessData()
		{
			string CronExpressionForDailyJob = "30 05 * * *";
			string CronExpressionForMinuteJob = "30 07 * * *";
			string CronExpressionForFearGreed = "30 18 * * *";
			string CronExpressionForDailyToWeekly = "* 15 * * 7";
			//RecurringJob.AddOrUpdate(() => FireJob(), CronExpressionPer1Min);
			var jobIdDaily = "History_dataDaily_1"; // Unique identifier for the recurring job
			var jobIdMinute = "History_dataMinute_2"; // Unique identifier for the recurring job
			var jobFG = "Fear_Greed_50";
			var jobDailyToWeekly = "Daily_To_Weekly";
			TimeZoneInfo bdTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Dhaka");

			var options = new RecurringJobOptions
			{
				//TimeZone = TimeZoneInfo.Utc // Time zone for the job schedule
				TimeZone = bdTimeZone                          // You can add more options here if needed
			};

			RecurringJob.AddOrUpdate(jobIdDaily, () => DailyJob(),CronExpressionForDailyJob,options);
			RecurringJob.AddOrUpdate(jobIdMinute, () => MinuteJob(), CronExpressionForMinuteJob, options);
			RecurringJob.AddOrUpdate(jobFG,() => GetFearGreedIndex(),CronExpressionForFearGreed,options);
			RecurringJob.AddOrUpdate(jobDailyToWeekly, () => DailyToWeeklyConvert(), CronExpressionForDailyToWeekly, options);
			return RedirectToAction(nameof(Index)); // Return a view named "ProcessResult"
		}
		public async Task GetFailedDate()
		{
			var data = await _context.tblDownloadStatus.OrderBy(x=>x.dtDate).ThenBy(x => x.SymName).ToListAsync();
			//data = data.Where(x => x.SymName == "ZY").ToList();
			int count = 0;
			foreach (var item in data)
			{
				count++;
				Console.WriteLine($"{item.dtDate}: {item.SymName} {item.DailyDataStatus} {item.MinDataStatus}");
				if (item.MinDataStatus.Equals("pending"))
				{
					BackgroundJob.Enqueue(() => CallApiForMinuteData(item.SymName, item.dtDate.ToString("yyyy-MM-dd")));
					//CallApiForMinuteData(item.SymName, item.dtDate.ToString("yyyy-MM-dd"));
				}
				if (item.DailyDataStatus.Equals("pending"))
				{
					BackgroundJob.Enqueue(() => CallApiForDailyData(item.SymName, item.dtDate.ToString("yyyy-MM-dd")));
				}
				if( count % 750 == 0)
				{
					await Task.Delay(60000);

				}
				Console.WriteLine(count);
				// break;
		
			}
        }
		
		public async Task<IActionResult> DailyJob()
		{
			var symbols = _context.tblSymbol.Select(symbol => symbol.SymName).ToList();
			var dayOfWeek = DateTime.Today.DayOfWeek.ToString();

			if (dayOfWeek != "Sunday" && dayOfWeek != "Monday")
			{
				// Here add the code to create a table per symbol with date.
				await _context.Database.ExecuteSqlRawAsync(" EXEC prcInsertIntoDownloadStatus ");
				var date = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");

					// Process the data and return a view or perform any other actions
				foreach (var symbol in symbols)
				{
					BackgroundJob.Enqueue(() => CallApiForDailyData(symbol,date));
					//Console.WriteLine($"{jobid} executed.");
				}
			}
			return View("ProcessData");
		}
		
		public async Task<IActionResult> MinuteJob()
		{
			var symbols = _context.tblSymbol.Select(symbol => symbol.SymName).ToList();
			var dayOfWeek = DateTime.Today.DayOfWeek.ToString();
			if (dayOfWeek != "Sunday" && dayOfWeek != "Monday")
			{
				// Here add the code to create a table per symbol with date.
				var date = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");

				// Process the data and return a view or perform any other actions
				foreach (var symbol in symbols)
				{
					BackgroundJob.Enqueue(() => CallApiForMinuteData(symbol, date));
					//Console.WriteLine($"{jobid} executed.");
				}
			}
			return View("ProcessData");
		}
		
		public async Task CallApiForMinuteData(string symbol, string date)
		{
			// Create an anonymous object to hold the data you want to send in JSON format
			var requestData = new
			{
				Symbol = symbol,
				Date = date
			};

			// Serialize the requestData to JSON
			var jsonRequest = JsonConvert.SerializeObject(requestData);
			var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

			//var response = await client.PostAsync("/api/historicaldata/minutedata", content);
			using var client = _httpClientFactory.CreateClient("baseurl");
			var response = await client.PostAsync($"/api/historicaldata/minutedata",content);
			if (!response.IsSuccessStatusCode)
			{
				var responseData = await response.Content.ReadAsStringAsync();
				var error_message = JsonConvert.DeserializeObject<ErrorModel>(responseData);
				Console.WriteLine("Error: "+ error_message.Message);
				throw new Exception(error_message.Message);
			}
			else
			{
				Console.WriteLine("Sueccess Minute Data: "+ symbol +" "+date);
			}

		}
		
		public async Task CallApiForDailyData(string symbol,string date)
		{
			// Create an anonymous object to hold the data you want to send in JSON format
			var requestData = new
			{
				Symbol = symbol,
				Date = date
			};

			// Serialize the requestData to JSON
			var jsonRequest = JsonConvert.SerializeObject(requestData);
			var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
			using var client = _httpClientFactory.CreateClient("baseurl");
			var response = await client.PostAsync($"/api/historicaldata/dailydata",content);
			if (!response.IsSuccessStatusCode)
			{
				var responseData = await response.Content.ReadAsStringAsync();
				var error_message = JsonConvert.DeserializeObject<ErrorModel>(responseData);
				Console.WriteLine("Error: " + error_message.Message);
				throw new Exception(error_message.Message);
			}
			else
			{
				Console.WriteLine("Sueccess Daily Data: "+ symbol +" "+ date);
			}
		}
		
		public async Task GetFearGreedIndex()
		{
			var dayOfWeek = DateTime.Today.DayOfWeek.ToString();
			if (dayOfWeek != "Saturday" && dayOfWeek != "Sunday")
			{
				using var client = _httpClientFactory.CreateClient();
				var response = await client.GetAsync("http://101.2.165.187:7756/fgmeter");
				if (!response.IsSuccessStatusCode)
				{
					Console.WriteLine("Error");
					throw new Exception();
				}
				else
				{
					Console.WriteLine("Sueccess");
				}
			}
			return;
		}
		public async Task DailyToWeeklyConvert()
		{
			using var client = _httpClientFactory.CreateClient();
			//var response = await client.GetAsync("http://127.0.0.1:5000/weeklydata");
			var response = await client.GetAsync("http://127.0.0.1:7050/weeklydata");
            if (!response.IsSuccessStatusCode)
			{
				Console.WriteLine("Error");
				throw new Exception();
			}
			else
			{
				Console.WriteLine("Sueccess");
			}
			return;
		}

		public async Task<IActionResult> DeleteDataFromHangFire()
		{
			try
			{
				string sql_query = $"EXEC prcDeleteJobsFromHangfire";
				_context.Database.ExecuteSqlRaw(sql_query);
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
				//throw new Exception("An Error Occured.");
			}
			return RedirectToAction("Index");
		}
	}
}