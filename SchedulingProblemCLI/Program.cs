using SchedulingProblem.Model;
using SchedulingProblem.Scenarios;
using SchedulingProblem.Solvers;
using System.Diagnostics;

namespace SchedulingProblem.Benchmarks
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var hasArgs = args != null && args.Length > 3;
            var time = int.Parse(hasArgs ? args[0] : "10");
            var problem = hasArgs ? args[1] : "NurseSmall";
            var solverName = hasArgs ? args[2] : "sat";
            var nrOfTimes = int.Parse(hasArgs ? args[3] : "1");


            Scenario scene;
            switch (problem)
            {
                case "NurseBig": scene = JSONParser.GetNurseBig(); break;
                case "NurseMedium": scene = JSONParser.GetNurseMedium(); break;
                case "NurseSmall": scene = JSONParser.GetNurseSmall(); break;
                case "PresBig": scene = JSONParser.GETPresentationBig(); break;
                case "PresMedium": scene = JSONParser.GetPresentationMedium(); break;
                case "PresSmall": scene = JSONParser.GetPresentationSmall(); break;
                case "CourseBig": scene = JSONParser.GetCourseBig(); break;
                case "CourseMedium": scene = JSONParser.GetCourseMedium(); break;
                case "CourseSmall": scene = JSONParser.GetCourseSmall(); break;
                default: scene = JSONParser.GetPresentationSmall(); break;
            }
            ISolver solver;
            if (solverName == "sat") solver = new SATSolver(scene, time);
            else solver = new ILPSolver(scene, time);
            Stopwatch sw = new Stopwatch();
            try
            {
                for (int i = 0; i < nrOfTimes; i++)
                {
                    sw.Start();
                    Console.WriteLine("Started solving " + problem + " with " + solver + "at" + DateTime.Now.ToShortTimeString());
                    Console.WriteLine("Finishing in at most" + time + " seconds");
                    var r1 = solver.Optimize(false);
                    var r2 = sw.ElapsedMilliseconds / 1000D;
                    Console.WriteLine("Result: " + r1 + "\t Time: " + r2);
                    sw.Reset();
                }
            }
            catch (InfeasibleException e)
            {
                var r2 = sw.ElapsedMilliseconds / 1000D;
                sw.Reset();
                Console.WriteLine("Solver failed to complete. Model is infeasible. Run Time " + r2 + " seconds");
            }
            catch (TimeoutException e)
            {
                var r2 = sw.ElapsedMilliseconds / 1000D;
                sw.Reset();
                Console.WriteLine("Solver reached timelimit. No solutions were found. Run Time " + r2 + " seconds");
            }
        }
    }
}