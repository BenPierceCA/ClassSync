using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ClassSync.DTOs;
using ClassSync.Helpers;
using ClassSync.Models;
using ClassSync.Repositories;
using HtmlAgilityPack;
using Newtonsoft.Json;
using RestSharp.Deserializers;

namespace ClassSync.SyncProviders
{
    public class NCState : SyncProviderBase
    {
        private string TermId { get; set; }
        private string CurrentSTrm { get; set; }
        private string SubjectsUrl { get; set; }

        public NCState(string _url, string _termId, string _currentStrm, string _subjectsUrl) : base(_url)
        {
            TermId = _termId;
            CurrentSTrm = _currentStrm;
            SubjectsUrl = _subjectsUrl;
        }

        public override List<Class> GetAll()
        {
            List<Class> results = new List<Class>();

            List<string> subjects = GetAllSubjects();

            foreach (string subject in subjects)
            {
                results.AddRange(Get(subject));
            }

            return results;
        }

        private List<Class> Get(string _subject)
        {
            List<Class> results = new List<Class>();
            string response = SimpleHTMLHelper.Post(URL, GetFormParams(_subject));

            // response will be serialized to json with one html attribute that holds all of the
            // information we're interested in.
            NCStateDTO data = JsonConvert.DeserializeObject<NCStateDTO>(response);

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(data.html);

            var courses = from course in htmlDoc.DocumentNode.Descendants("section")
                         where course.Attributes["class"].Value == "course"
                         select course;

            foreach (var course in courses)
            {
                // Course attributes
                string id = course.Attributes["id"].Value;
                string courseName = course.Descendants("small").FirstOrDefault().InnerText;
                string courseDescription = course.Descendants("p").FirstOrDefault().InnerText;
                Course oneCourse = new Course()
                {
                    CourseCode = id,
                    CourseName = courseName,
                    CourseDescription = courseDescription
                };

                // Class attributes (represented by a table).
                var classTable = from table in course.Descendants("table")
                                 select table;

                // Get all the rows of the table -- each row represents a class.
                var classes = (from cls in classTable.FirstOrDefault().Descendants("tr")                              
                              select cls).Skip(2);  // First two rows are header rows and can be ignored

                foreach (var cls in classes)
                {
                    // Get all the class attributes (from td's)
                    var attributes = from att in cls.Descendants("td")
                                     select att;

                    // Raw from the HTML
                    string classSection = attributes.ElementAt(0).InnerText;
                    string component = attributes.ElementAt(1).InnerText;
                    string classNumber = attributes.ElementAt(2).InnerText;                                        
                    var timeNode = attributes.ElementAt(4);
                    string location = attributes.ElementAt(5).InnerText;
                    var instructorNode = attributes.ElementAt(6);
                    string dates = attributes.ElementAt(7).InnerText;

                    // Cleaned Sub Objects
                    List<TimePeriod> timePeriods = ParseTimePeriods(timeNode);
                    List<Instructor> instructors = ParseInstructors(instructorNode);
                    DateTime startDate = DateTime.MinValue;
                    DateTime endDate = DateTime.MinValue;

                    if (dates.Contains(" - "))
                    {
                        startDate = ParseDate(dates.Substring(0, dates.IndexOf(" - ")).Trim());
                        endDate = ParseDate(dates.Substring(dates.IndexOf(" - ") + 3).Trim());
                    }
                    else
                    {
                        DateTime.TryParse(dates, out startDate);
                    }

                    Class oneClass = new Class()
                    {
                        Component = component,
                        Course = oneCourse,
                        EndDate = endDate,
                        Instructors = instructors,
                        Location = location,
                        Notes = string.Empty,
                        Number = classNumber,
                        Section = classSection,
                        StartDate = startDate,
                        Times = timePeriods
                    };

                    results.Add(oneClass);
                }

            }

            return results;
        }

        public void Save(List<Class> classes)
        {
            ClassRepository repo = new ClassRepository();
            repo.Clear();
            repo.Insert(classes);
        }

        private List<string> GetAllSubjects()
        {
            List<string> subjects = new List<string>();

            string response = SimpleHTMLHelper.Post(SubjectsUrl, GetSubjectFormsParams());

            // response will be serialized to json with one html attribute that holds all of the
            // information we're interested in.
            NCStateSubjectsDTO data = JsonConvert.DeserializeObject<NCStateSubjectsDTO>(response);

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(data.subj_html);

            var subjectsNode = from subject in htmlDoc.DocumentNode.Descendants("a")
                          where subject.Attributes["href"].Value == "#"
                          select subject;

            foreach (var subjectItem in subjectsNode)
            {
                subjects.Add(subjectItem.Attributes["data-value"].Value);
            }

            return subjects;
        }

        // Will consistantly parse out a date such as 08/16/17 (MM:dd:yy)
        private DateTime ParseDate(string date)
        {
            int month = Int32.Parse(date.Substring(0, 2));
            int day = Int32.Parse(date.Substring(3, 2));
            int year = Int32.Parse("20" + date.Substring(6, 2));

            return new DateTime(year, month, day);
        }

        /***************************************************************************
            Parses the instructors out of the "Instructor" table column. Each
            anchor tag represents an instructor.
            
            Not getting too crazy with the name parsing stuff because this is only
            a poc.  
        ****************************************************************************/

        private List<Instructor> ParseInstructors(HtmlNode node)
        {
            List<Instructor> result = new List<Instructor>();

            var instructors = from instructor in node.Descendants("a")
                where instructor.Attributes["class"].Value == "instructor-link"
                select instructor;

            foreach (var instructor in instructors)
            {
                string rawName = instructor.InnerText;
                string profileUrl = instructor.Attributes["href"].Value;
                string firstName = string.Empty;
                string lastName = string.Empty;
                string middleName = string.Empty;

                lastName = rawName.Substring(0, rawName.IndexOf(",")).Trim();
                firstName = rawName.Substring(rawName.IndexOf(",") + 1).Trim();

                if (firstName.Contains(" "))
                {
                    middleName = firstName.Substring(firstName.IndexOf(" ")).Trim();
                    firstName = firstName.Substring(0, firstName.IndexOf(" ")).Trim();                    
                }

                Instructor oneInstructor = new Instructor()
                {
                    FirstName = firstName,
                    LastName = lastName,
                    MiddleName = middleName,
                    ProfileUrl = profileUrl,
                    RawName = rawName
                };

                result.Add(oneInstructor);
            }

            return result;
        }

        /***************************************************************************
            Parses the class time periods out of the "Day/Time" table column.

            We know which days are selected because their li tag will have the
            "meet hidden-xs" class.

            Note: Would break this class into smaller, testable, pieces 
                  in a production situation and more defensive coding against 
                  html anomolies. 
        ****************************************************************************/
        private List<TimePeriod> ParseTimePeriods(HtmlNode node)
        {
            List<TimePeriod> result = new List<TimePeriod>();

            string startTime = string.Empty;
            string endTime = string.Empty;
            bool isTBD = false;
            bool isUnknown = false;            

            string time = string.Empty;

            if (node.InnerHtml.ToUpper() == "TBD")
            {
                time = node.InnerHtml.ToUpper();
                isTBD = true;
            }
            else
            {
                time = node.InnerHtml.Substring(node.InnerHtml.IndexOf("</ul>") + 5).Trim(); // In this format 8:30 AM  - 9:20 AM or possibly TBD
            }

            TimeSpan tStart = new TimeSpan();
            TimeSpan tEnd = new TimeSpan();

            string notes = time;
            double? totalMinutes = null;

            if (time.Contains("-") && time != "-")
            {
                startTime = time.Substring(0, time.IndexOf("-")).Trim();
                endTime = time.Substring(time.IndexOf("-") + 1).Trim();

                DateTime dateTime = DateTime.ParseExact(startTime,
                                    "h:mm tt", CultureInfo.InvariantCulture);
                tStart = dateTime.TimeOfDay;

                if (endTime.Contains("<"))
                {
                    endTime = endTime.Substring(0, endTime.IndexOf("<")).Trim();
                }

                dateTime = DateTime.ParseExact(endTime, "h:mm tt", CultureInfo.InvariantCulture);

                tEnd = dateTime.TimeOfDay;

                totalMinutes = tEnd.Subtract(tStart).TotalMinutes;
            }
            else
            {
                isTBD = (time.ToUpper() == "TBD");
                isUnknown = (time.ToUpper() != "TBD");
            }
            
            var days = from day in node.Descendants("li")
                       where day.Attributes["class"].Value == "meet hidden-xs"
                       select day;

            if (days.Count() > 0)
            {
                foreach (var day in days)
                {
                    string dayName = day.Descendants("abbr").ElementAt(0).Attributes["title"].Value.Replace(" - meet", ""); // Returns the day name in readable format
                    DayOfWeek dow = ((DayOfWeek) Enum.Parse(typeof(DayOfWeek), dayName, true));

                    TimePeriod timePeriod = new TimePeriod()
                    {
                        Day = dow,
                        EndTime = tEnd,
                        IsTBD = isTBD,
                        IsUnknown = isUnknown,
                        Notes = notes,
                        StartTime = tStart,  
                        TotalMinutes = totalMinutes                      
                    };

                    result.Add(timePeriod);
                }
            }
            else
            {
                TimePeriod timePeriod = new TimePeriod()
                {
                    Day = null,
                    EndTime = null,
                    IsTBD = isTBD,
                    IsUnknown = isUnknown,
                    Notes = notes,
                    StartTime = null
                };

                result.Add(timePeriod);
            }

            return result;
        }

        private Dictionary<string, string> GetFormParams(string _subject)
        {
            Dictionary<string, string> parms = new Dictionary<string, string>();
            parms.Add("term", TermId);
            parms.Add("subject", _subject);
            parms.Add("course-inequality", "=");
            parms.Add("course-number", "");
            parms.Add("course-career", "");
            parms.Add("session", "");
            parms.Add("start-time-inequality", "<=");
            parms.Add("start-time", "");
            parms.Add("end-time-inequality", "<=");
            parms.Add("end-time", "");
            parms.Add("instructor-name", "");
            parms.Add("current_strm", CurrentSTrm);

            return parms;
        }

        private Dictionary<string, string> GetSubjectFormsParams()
        {
            Dictionary<string, string> parms = new Dictionary<string, string>();
            parms.Add("strm", TermId);

            return parms;
        }


    }
}
