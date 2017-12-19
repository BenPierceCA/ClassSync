using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ClassSync.Models;
using Dapper;

namespace ClassSync.Repositories
{
    public class ClassRepository
    {
        private SqlConnection GetConnection()
        {
            var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["ClassSync"].ConnectionString);

            return connection;
        }

        public void Clear()
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                connection.Execute("dbo.ClassTruncate", commandType: CommandType.StoredProcedure);
            }
        }

        private string PrettyTime(TimeSpan? ts)
        {
            string result = string.Empty;

            if (ts != null)
            {
                string hour = string.Empty;
                string minute = string.Empty;
                string ampm = string.Empty;

                TimeSpan time = (TimeSpan) ts;
                if (time.Hours == 0)
                {
                    hour = "12";
                } else if (time.Hours > 0 && time.Hours < 13)
                {
                    hour = time.Hours.ToString().PadLeft(2, '0');
                } else
                {
                    hour = (time.Hours - 12).ToString().PadLeft(2, '0');
                }
                minute = time.Minutes.ToString().PadLeft(2, '0');
                ampm = (time.Hours >= 12) ? "PM" : "AM";

                result = $"{hour}:{minute} {ampm}";
            }

            return result;
        }

        public void Insert(List<Class> _classes)
        {
            using (var connection = GetConnection())
            {
                connection.Open();

                foreach (Class cls in _classes)
                {
                    foreach (Instructor instructor in cls.Instructors)
                    {
                        foreach (TimePeriod time in cls.Times)
                        {
                            connection.Execute("dbo.ClassInsert", new
                            {
                                Term = cls.Term,
                                CourseCode = cls.Course.CourseCode,
                                CourseName = cls.Course.CourseName,
                                CourseDescription = cls.Course.CourseDescription,
                                Number = cls.Number,
                                Section = cls.Section,
                                Location = cls.Location,
                                StartDate = cls.StartDate,
                                EndDate = cls.EndDate,
                                DayOfWeek = time.Day.ToString(),
                                StartTime = PrettyTime(time.StartTime),
                                EndTime = PrettyTime(time.EndTime),
                                TotalMinutes = time.TotalMinutes,
                                IsTimeTBD = time.IsTBD,
                                IsTimeUnknown = time.IsUnknown,
                                TimeNotes = time.Notes,
                                InstructorFirstName = instructor.FirstName,
                                InstructorLastName = instructor.LastName,
                                InstructorProfileUrl = instructor.ProfileUrl,
                                Component = cls.Component,
                                Notes = cls.Notes
                            }, commandType: CommandType.StoredProcedure);
                        }
                    }
                }
            }
        }
    }
}
