﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Zcrypta.Models;

public partial class SignalStrategy
{
    public long Id { get; set; }

    public int StrategyType { get; set; }

    public int Interval { get; set; }

    public string CreatedBy { get; set; }

    public DateTime CreateDate { get; set; }

    public string UpdatedBy { get; set; }

    public DateTime? UpdateDate { get; set; }

    public virtual ICollection<UserSignalStrategy> UserSignalStrategies { get; set; } = new List<UserSignalStrategy>();
}