using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ClassSync.DTOs
{
    public class Inputs
    {
        public string term { get; set; }
        public string subject { get; set; }
        [JsonProperty("__invalid_name__course-inequality")]
        public string invalidNameCourseInequality { get; set; }
        [JsonProperty("__invalid_name__course-number")]
        public string invalidNameCourseNumber { get; set; }
        [JsonProperty("__invalid_name__course-career")]
        public string invalidNameCourseCareer { get; set; }
    
        public string session { get; set; }
        [JsonProperty("__invalid_name__start-time-inequality")]
        public string invalidNameStartTimeInequality { get; set; }
        [JsonProperty("__invalid_name__start-time")]
        public string invalidNameStartTime { get; set; }
        [JsonProperty("__invalid_name__end-time-inequality")]
        public string invalidNameEndTimeInequality { get; set; }
        [JsonProperty("__invalid_name__end-time")]
        public string invalidNameEndTime { get; set; }
        [JsonProperty("__invalid_name__instructor-name")]
        public string invalidNameInstructorName { get; set; }
        public string current_strm { get; set; }
    }

    public class Json
    {
        public Inputs inputs { get; set; }
    }

    public class NCStateDTO
    {
        public string html { get; set; }
        public Json json { get; set; }
    }

    public class NCStateSubjectsDTO
    {
        public string subj_js { get; set; }
        public int subject_count { get; set; }
        public string subj_html { get; set; }
        public string sess_html { get; set; }
    }
}
