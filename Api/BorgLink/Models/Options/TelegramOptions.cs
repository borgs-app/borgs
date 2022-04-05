using System;
using System.Collections.Generic;
using System.Text;

namespace BorgLink.Models.Options
{
    public class TelegramOptions
    {
        public List<TelegramOption> Accounts { get; set; }
    }

    public class TelegramOption
    {
        public long Id { get; set; }
        public string Token { get; set; }
    }
}
