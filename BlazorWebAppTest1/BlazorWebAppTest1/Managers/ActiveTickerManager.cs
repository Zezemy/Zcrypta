using System.Collections.Concurrent;

namespace BlazorWebAppTest1.Managers
{
    internal sealed class ActiveTickerManager
    {
        private readonly ConcurrentBag<string> _activeTickers = [];

        public void AddTicker(string ticker)
        {
            if (!_activeTickers.Contains(ticker))
            {
                _activeTickers.Add(ticker);
            }
        }

        public IReadOnlyCollection<string> GetAllTickers()
        {
            return _activeTickers.ToArray();
        }
    }
}