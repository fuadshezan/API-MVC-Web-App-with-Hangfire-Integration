using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace P3.API.Models.Domain;

[PrimaryKey("SymName", "DtDate")]
public partial class tblHistoryData_1Min
{
    [Key]
    [StringLength(15)]
    [Unicode(false)]
    public string SymName { get; set; } = null!;

    [Key]
    [Column("dtDate", TypeName = "smalldatetime")]
    public DateTime DtDate { get; set; }

    [Column(TypeName = "money")]
    public decimal Open { get; set; }

    [Column(TypeName = "money")]
    public decimal High { get; set; }

    [Column(TypeName = "money")]
    public decimal Low { get; set; }

    [Column(TypeName = "money")]
    public decimal Close { get; set; }

    public long Volume { get; set; }

    [Column("VWAP", TypeName = "money")]
    public decimal Vwap { get; set; } = 0;

    [Column(TypeName = "smallmoney")]
    public decimal Change { get; set; }

    [StringLength(30)]
    [Unicode(false)]
    public string TimeFrame { get; set; } = "History Data (1 Min)";

    [StringLength(30)]
    [Unicode(false)]
    public string? GapStatus { get; set; }
}
