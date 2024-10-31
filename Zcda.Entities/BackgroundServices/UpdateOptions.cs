namespace Zcda.Entities.BackgroundServices
{
    public sealed class UpdateOptions
    {
        public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromSeconds(15);
        public double MaxPercentageChange { get; set; } = 0.02; // 2%
    }
}