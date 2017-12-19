using System;
using System.Collections.Generic;
using System.Linq;
using ClassSync.SyncProviders;
using ClassSync.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClassSync.IntegrationTests
{
   /************************************************************************************
        Important Notes:

        1. We'd need to know the timezone of each university in order to convert to UTC (which is what we'd want to store in the DB)
        3. Better defensive coding on the HTML shredding (right now there are a few safer, more robust, ways to parse things out).
        2. Didn't go too crazy parsing out instructor names (IE: middle names, etc...). Would need to work on a more accurate algo for that.

        TODO: Add subject area to model and database.
    *************************************************************************************/
    [TestClass]
    public class NCStateTests
    {
        // These globals would likely be provider specific database config.
        private string rootUrl = "https://www.acs.ncsu.edu/php/coursecat/search.php";
        private string subjectsUrl = "https://www.acs.ncsu.edu/php/coursecat/subjects.php";
        private string curTermId = "2181";
        private string curSTerm = "2181";

        [TestMethod]
        public void GetAll_ShouldReturn_AllClassRecords()
        {
            DateTime startTime = DateTime.Now;

            NCState provider = new NCState(rootUrl, curTermId, curSTerm, subjectsUrl);
            List<Class> classes = provider.GetAll();

            Double minutes = DateTime.Now.Subtract(startTime).TotalSeconds / 60.0;
            minutes = Math.Round(minutes, 2);

            // Save results (inefficient -- better to bulk load into DB)
            provider.Save(classes); // Would normally go into a controller, not provider.

            Assert.AreNotEqual(0, classes.Count);

            int instructorCount = classes.SelectMany(instructor => instructor.Instructors).Select(i => i.RawName).Distinct().Count();
            int courseCount = classes.Select(course => course.Course).Select(i => i.CourseCode).Distinct().Count();
            int classCount = classes.Select(cls => cls.Number).Distinct().Count();

            Console.WriteLine("******************************* RESULTS *********************************************");
            Console.WriteLine("Total sync time: " + minutes + " minutes.");
            Console.WriteLine("Total instructors processed: " + instructorCount);
            Console.WriteLine("Total courses processed: " + courseCount);
            Console.WriteLine("Total classes processed: " + classCount);
            Console.WriteLine("***************************** END RESULTS *******************************************");           
        }
    }
}
