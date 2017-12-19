using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassSync.Models
{
    public class Class
    {
        public string Term { get { return "Spring 2018"; } }    // Would obviously need to be dynamic in a prod environent.
        public Course Course { get; set; }
        public string Number { get; set; }
        public string Section { get; set; }
        public List<TimePeriod> Times { get; set; }
        public string Location { get; set; }
        public List<Instructor> Instructors { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        /* Optional Nice-to-Have Data -- useful for de-duping and data-cleansing */
        public string Component { get; set; }
        public string Notes { get; set; }

        public override string ToString()
        {
            return string.Format("Course: {0}, Class: {1}, Instructors: {2}, Times: {3}, Dates: {4}", Course.CourseCode, Number, Instructors.Count(), Times.Count(), StartDate.ToString("yyyy-MM-dd") + " to " + EndDate.ToString("yyyy-MM-dd"));
        }
    }
}
