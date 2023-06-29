
using Newtonsoft.Json;
using SchedulingProblem.Model;
using System.IO;
using System.Reflection;

namespace SchedulingProblem.Scenarios
{

    public static class JSONParser
    {


        #region Nurse Scheduling Problem
        public static Scenario GetNurseSmall()
        {
            var buildDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var x = JsonConvert.DeserializeObject<Scenario>(File.ReadAllText(buildDir + @"\Scenarios\nurse-small.json"));
            return x;
        }

        public static Scenario GetNurseMedium()
        {
            var buildDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var x = JsonConvert.DeserializeObject<Scenario>(File.ReadAllText(buildDir + @"\Scenarios\nurse-medium.json"));
            return x;
        }

        public static Scenario GetNurseBig()
        {
            var buildDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var x = JsonConvert.DeserializeObject<Scenario>(File.ReadAllText(buildDir + @"\Scenarios\nurse-big.json"));
            return x;
        }
        #endregion

        #region Course Scheduling Problem
        public static Scenario GetCourseSmall()
        {
            var buildDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var x = JsonConvert.DeserializeObject<Scenario>(File.ReadAllText(buildDir + @"\Scenarios\course-small.json"));
            return x;
        }

        public static Scenario GetCourseMedium()
        {
            var buildDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var x = JsonConvert.DeserializeObject<Scenario>(File.ReadAllText(buildDir + @"\Scenarios\course-medium.json"));
            return x;
        }

        public static Scenario GetCourseBig()
        {
            var buildDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var x = JsonConvert.DeserializeObject<Scenario>(File.ReadAllText(buildDir + @"\Scenarios\course-big.json"));
            return x;
        }
        #endregion

        #region Presentation Scheduling Problme
        public static Scenario GetPresentationSmall()
        {
            var buildDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var x = JsonConvert.DeserializeObject<Scenario>(File.ReadAllText(buildDir + @"\Scenarios\presentation-small.json"));
            return x;
        }

        public static Scenario GetPresentationMedium()
        {
            var buildDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var x = JsonConvert.DeserializeObject<Scenario>(File.ReadAllText(buildDir + @"\Scenarios\presentation-medium.json"));
            return x;
        }

        public static Scenario GETPresentationBig()
        {
            var buildDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var x = JsonConvert.DeserializeObject<Scenario>(File.ReadAllText(buildDir + @"\Scenarios\presentation-big.json"));
            return x;
        }
        #endregion
    }
}