﻿using Zcrypta.Entities.Dtos;

namespace Zcrypta.Entities.Interfaces
{
    public interface IPriceUpdateClientContract
    {
        Task ReceiveStockPriceUpdate(CurrentPrice update);
    }
}