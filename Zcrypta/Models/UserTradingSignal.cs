﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace Zcrypta.Models;

public partial class UserTradingSignal
{
    public long Id { get; set; }

    public string UserId { get; set; }

    public long TradingSignalId { get; set; }

    public virtual TradingSignal TradingSignal { get; set; }
}