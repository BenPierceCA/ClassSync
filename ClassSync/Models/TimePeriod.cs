using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassSync.Models
{
    public class TimePeriod
    {
        public DayOfWeek? Day { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public double? TotalMinutes { get; set; }
        public bool IsTBD { get; set; }
        public bool IsUnknown { get; set; }
        public string Notes { get; set; }   // Useful for instances where there's not a valid date/time and we want to store what we found in here for diagnostic/informational purposes.
    }
}
