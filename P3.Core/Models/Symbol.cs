using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace P3.Web.Models
{
    [Keyless]
    public class Symbol
    {
		//[DisplayName("Symbol")]
		//public string SymName { get; set; }
		//[DisplayName("Date")]
		//[DataType(DataType.Date)]
		//public DateTime dtDate { get; set; }

		[DisplayName("Symbol")]
		public string SymName { get; set; }
		public string ComName { get; set; }
		public string Sector { get; set; }
		public string SubSector { get; set; }
		public string HeadQuarter { get; set; }
		public DateTime? DateFirstAdded { get; set; }
		public string Cik { get; set; }
		public string? Founded { get; set; }
		public string IndexName { get; set; }
		public string UniqueId { get; set; }
		public string ExchangeShortName { get; set; }
		public bool IsIndex { get; set; }
		[Column(TypeName = "money")]
		public double? HAT { get; set; }
		[Column(TypeName = "money")]
		public double? H52W { get; set; }
		[Column(TypeName = "money")]
		public double? HYTD { get; set; }
		[Column(TypeName = "money")]
		public double? HTM { get; set; }
		[Column(TypeName = "money")]
		public double? HTW { get; set; }
		[Column(TypeName = "money")]
		public double? HTD { get; set; }
		[Column(TypeName = "smalldatetime")]
		public DateTime? HDate { get; set; }
		public bool AllowToShort { get; set; }
		[Column(TypeName = "smalldatetime")]
		public DateTime? dtDailyMin { get; set; }
		[Column(TypeName = "smalldatetime")]
		public DateTime? dtDailyMax { get; set; }
		public bool CTD { get; set; }
	}
}
