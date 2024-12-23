﻿namespace Zcrypta.Entities.Models.Chart
{
    public class Marker
    {
        public DateTime Time { get; set; }
        public decimal Price { get; set; }
        public Direction MarkerDirection { get; set; }
        public string Text { get; set; }

        public enum Direction
        {
            Buy = 1,
            Sell = 2,
        }
    }
}
