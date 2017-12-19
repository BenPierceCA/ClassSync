using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassSync.Models
{
    public class Instructor
    {
        public string RawName { get; set; }     // Full name so we can later diagnose parsing errors
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }

        // Could likely go even deeper with these properties, if we're willing to take a performance
        // hit crawling the instructor pages and fleshing this out. Not important for proof of concept.
        public string ProfileUrl { get; set; }
    }
}
