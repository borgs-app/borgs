using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace BorgImageReader
{
    public class Histogram
    {
        public Dictionary<string, List<BigInteger>> Data { get; set; }
        public List<KeyValuePair<string, string>> FormattedData => Format();

        public Histogram()
        {
            Data = new Dictionary<string, List<BigInteger>>();
        }

        public void AddData(string key, int? value)
        {
            if(!value.HasValue)
                Data.Add(key, new List<BigInteger>());
            else if (Data.TryGetValue(key, out var data))
                Data[key].Add(value.Value);
            else
                Data.Add(key, new List<BigInteger>() { value.Value });
        }

        private List<KeyValuePair<string, string>> Format()
        {
            return Data.AsEnumerable()
                .Select(x => new KeyValuePair<string, string>(x.Key, string.Join(",", x.Value)))
                .Where(x => x.Key != "000000")
                .ToList();
        }
    }
}
