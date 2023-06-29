using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SchedulingProblem.Solvers;
using SchedulingProblem.Scenarios;
using System.Collections.Generic;
using System.Linq;
using SchedulingProblem.Model;

namespace SchedulingProblemTests.Model
{
    [TestClass]
    public class DefaultConstraintTests
    {
        private static int _timeLimit = 10;
        private static object[] _scenarioS1ResultForILP;
        private static object[] _scenarioS1ResultForSAT;
        private static object[] _scenarioS2ResultForILP;
        private static object[] _scenarioS2ResultForSAT;
        private static object[] _scenarioS3ResultForILP;
        private static object[] _scenarioS3ResultForSAT;
        private static object[] _scenarioS4ResultForILP;
        private static object[] _scenarioS4ResultForSAT;

        static DefaultConstraintTests()
        {
            Scenario s1 = TestScenario.ProblemOne();
            Scenario s2 = NurseSchedulingProblem.SpitalMuri();
            Scenario s3 = CourseSchedulingProblem.Berufsbildner();
            Scenario s4 = PresentationSchedulingProblem.FMSBasel();

            _scenarioS1ResultForILP = new object[] { (s1, new ILPSolver(s1, _timeLimit).Optimize()) };
            _scenarioS1ResultForSAT = new object[] { (s1, new SATSolver(s1, _timeLimit).Optimize()) };
            _scenarioS2ResultForILP = new object[] { (s2, new ILPSolver(s2, _timeLimit).Optimize()) };
            _scenarioS2ResultForSAT = new object[] { (s2, new SATSolver(s2, _timeLimit).Optimize()) };
            _scenarioS3ResultForILP = new object[] { (s3, new ILPSolver(s3, _timeLimit).Optimize()) };
            _scenarioS3ResultForSAT = new object[] { (s3, new SATSolver(s3, _timeLimit).Optimize()) };
            _scenarioS4ResultForILP = new object[] { (s4, new ILPSolver(s4, _timeLimit).Optimize()) };
            _scenarioS4ResultForSAT = new object[] { (s4, new SATSolver(s4, _timeLimit).Optimize()) };
        }

        private static IEnumerable<object[]> TestData()
        {
            yield return _scenarioS1ResultForILP;
            yield return _scenarioS1ResultForSAT;
            yield return _scenarioS2ResultForILP;
            yield return _scenarioS2ResultForSAT;
            yield return _scenarioS3ResultForILP;
            yield return _scenarioS3ResultForSAT;
            yield return _scenarioS4ResultForILP;
            yield return _scenarioS4ResultForSAT;
        }

        /**
         *
         * HARD CONSTRAINTS
         *
         */

        [TestMethod]
        [DynamicData(nameof(TestData), DynamicDataSourceType.Method)]
        public void PersonOnlyInOnePlaceAtATime((Scenario s, (int o, bool[][][][] v, string m) r) p)
        {
            // we need to assure that a person can only be physically present in location in one instance of time

            // We iterate over every person
            for (int personId = 0; personId < p.r.v.Length; personId++)
            {
                // and over all their timeslots
                for (int timeSlotId = 0; timeSlotId < p.r.v[personId].Length; timeSlotId++)
                {
                    // and then assure that they only work in one place 
                    for (int taskId = 0; taskId < p.r.v[personId][timeSlotId].Length; taskId++)
                    {
                        int nrOfLocations = 0;
                        for (int locationId = 0; locationId < p.r.v[personId][timeSlotId][taskId].Length; locationId++)
                        {
                            if (p.r.v[personId][timeSlotId][taskId][locationId])
                            {
                                nrOfLocations++;
                            }
                        }

                        Assert.IsTrue(nrOfLocations <= 1);
                    }
                }
            }
        }

        // Hard constraint 1 - Absences
        [DataTestMethod]
        [DynamicData(nameof(TestData), DynamicDataSourceType.Method)]
        public void AbsentPeopleAreNotWorking((Scenario s, (int o, bool[][][][] v, string m) r) p)
        {
            // we need to assure that when a person is absent they are not assigned to work

            // We iterate over every person
            for (int personId = 0; personId < p.r.v.Length; personId++)
            {
                // and over all their timeslots
                for (int timeSlotId = 0; timeSlotId < p.r.v[personId].Length; timeSlotId++)
                {
                    // Check whether the person is available at the time slot
                    if (p.s.People[personId].Absences.Contains(timeSlotId))
                    {
                        Assert.AreEqual(false, p.r.v[personId][timeSlotId][0][0]);
                    }
                }
            }
        }

        // Hard constraint 2 - Capacity
        [DataTestMethod]
        [DynamicData(nameof(TestData), DynamicDataSourceType.Method)]
        public void PersonCanOnlyWorkOnNTasksAtATime((Scenario s, (int o, bool[][][][] v, string m) r) p)
        {
            // We iterate over every person
            for (int personId = 0; personId < p.r.v.Length; personId++)
            {
                // and over all the timeslots
                for (int timeSlotId = 0; timeSlotId < p.r.v[personId].Length; timeSlotId++)
                {
                    //and then assure that they only work on one task
                    int nrOfTasks = 0;
                    for (int taskId = 0; taskId < p.r.v[personId][timeSlotId].Length; taskId++)
                    {
                        for (int locationId = 0; locationId < p.r.v[personId][timeSlotId][taskId].Length; locationId++)
                        {
                            if (p.r.v[personId][timeSlotId][taskId][locationId]) nrOfTasks++;
                        }
                    }

                    Assert.IsTrue(nrOfTasks <= p.s.People[personId].Capacity);
                }
            }
        }


        // Hard constraint 3 - Repetition
        [DataTestMethod]
        [DynamicData(nameof(TestData), DynamicDataSourceType.Method)]
        public void TasksAreAssignedNTimes((Scenario s, (int o, bool[][][][] v, string m) r) p)
        {
            // we need to assure that tasks are assigned at least n times

            // We iterate over every task
            for (int taskId = 0; taskId < p.s.Tasks.Count; taskId++)
            {
                int nrOfTasks = 0;
                // and over all the timeslots
                for (int timeSlotId = 0; timeSlotId < p.s.TimeSlots.Count; timeSlotId++)
                {
                    for (int personId = 0; personId < p.s.People.Count; personId++)
                    {
                        // Check whether the person is available at the time slot
                        if (p.r.v[personId][timeSlotId].Length == 1 && p.r.v[personId][timeSlotId][0][0] == false) continue;
                        // Check whether the person has the required skills for the task
                        if (p.r.v[personId][timeSlotId][taskId].Length == 1 && p.r.v[personId][timeSlotId][taskId][0] == false) continue;
                        for (int locationId = 0; locationId < p.r.v[personId][timeSlotId][taskId].Length; locationId++)
                        {
                            if (p.r.v[personId][timeSlotId][taskId][locationId]) nrOfTasks++;
                        }
                    }
                }
                Console.WriteLine("Task: " + taskId);
                Console.WriteLine("Actual: " + nrOfTasks);
                // Console.WriteLine("Should be: " + p.s.Tasks[taskId].Repetitions * p.s.Tasks[taskId].RequiredNrOfPeople);
                // Assert.IsTrue(nrOfTasks <= p.s.Tasks[taskId].Repetitions * p.s.Tasks[taskId].RequiredNrOfPeople);
            }
        }

        // Hard constraint 3 - Workload (Task)
        [DataTestMethod]
        [DynamicData(nameof(TestData), DynamicDataSourceType.Method)]
        public void TasksInvolveRequiredAmountOfPeople((Scenario s, (int o, bool[][][][] v, string m) r) p)
        {
            // we need to assure that tasks involve exactly the amount of people needed

            // We iterate over every task
            for (int taskId = 0; taskId < p.s.Tasks.Count; taskId++)
            {
                // and over all the timeslots
                for (int timeSlotId = 0; timeSlotId < p.s.TimeSlots.Count; timeSlotId++)
                {
                    bool hasTask = false;
                    int nrOfPeople = 0;
                    for (int personId = 0; personId < p.s.People.Count; personId++)
                    {
                        // Check whether the person is available at the time slot
                        if (p.r.v[personId][timeSlotId].Length == 1 && p.r.v[personId][timeSlotId][0][0] == false) continue;
                        // Check whether the person has the required skills for the task
                        if (p.r.v[personId][timeSlotId][taskId].Length == 1 && p.r.v[personId][timeSlotId][taskId][0] == false) continue;
                        for (int locationId = 0; locationId < p.r.v[personId][timeSlotId][taskId].Length; locationId++)
                        {
                            if (p.r.v[personId][timeSlotId][taskId][locationId])
                            {
                                hasTask = true;
                                nrOfPeople++;
                            }
                        }
                    }
                    if (!hasTask) continue;
                    Console.WriteLine("Task: " + taskId);
                    Console.WriteLine("Time slot: " + timeSlotId);
                    Console.WriteLine("  Actual: " + nrOfPeople);
                    // Console.WriteLine("  Should be: " + p.s.Tasks[taskId].RequiredNrOfPeople);
                    // Assert.IsTrue(nrOfPeople == p.s.Tasks[taskId].RequiredNrOfPeople);
                }
            }
        }

        // Hard constraint 4 - Skills (Person & Task)
        [DataTestMethod]
        [DynamicData(nameof(TestData), DynamicDataSourceType.Method)]
        public void TasksArePerformedByPeopleWithTheRequiredSkills((Scenario s, (int o, bool[][][][] v, string m) r) p)
        {
            // we need to assure that tasks are performed by people which have the required skills

            // We iterate over every task
            for (int taskId = 0; taskId < p.s.Tasks.Count; taskId++)
            {
                // and check if the task requires a skill
                if (p.s.Tasks[taskId].Skills.Length == 0) continue;
                // and collect all skills
                var skills = new HashSet<int>();
                // and over all the timeslots
                for (int timeSlotId = 0; timeSlotId < p.s.TimeSlots.Count; timeSlotId++)
                {
                    for (int personId = 0; personId < p.s.People.Count; personId++)
                    {
                        // Check whether the person is available at the time slot
                        if (p.r.v[personId][timeSlotId].Length == 1 && p.r.v[personId][timeSlotId][0][0] == false) continue;
                        // Check whether the person has the required skills for the task
                        if (p.r.v[personId][timeSlotId][taskId].Length == 1 && p.r.v[personId][timeSlotId][taskId][0] == false) continue;
                        for (int locationId = 0; locationId < p.r.v[personId][timeSlotId][taskId].Length; locationId++)
                        {
                            if (p.r.v[personId][timeSlotId][taskId][locationId])
                                skills.UnionWith(p.s.People[personId].Skills);
                        }
                    }
                }
                Console.WriteLine("Task: " + taskId);
                Array.ForEach(skills.ToArray(), Console.Write);
                Console.WriteLine(" ");
                Array.ForEach(p.s.Tasks[taskId].Skills, Console.Write);
                Console.WriteLine(" ");
                Assert.IsTrue(p.s.Tasks[taskId].Skills.ToHashSet().IsSubsetOf(skills.ToArray()));
            }
        }

        // Hard constraint 4 - Skills (Location & Task)
        [DataTestMethod]
        [DynamicData(nameof(TestData), DynamicDataSourceType.Method)]
        public void TasksArePerformedInALocationWithTheRequiredSkills((Scenario s, (int o, bool[][][][] v, string m) r) p)
        {
            // we need to assure that tasks are performed by people which have the required skills

            // We iterate over every task
            for (int taskId = 0; taskId < p.s.Tasks.Count; taskId++)
            {
                // and check if the task requires a skill
                if (p.s.Tasks[taskId].Skills.Length == 0) continue;
                // and collect all skills
                var skills = new HashSet<int>();
                // and over all the timeslots
                for (int timeSlotId = 0; timeSlotId < p.s.TimeSlots.Count; timeSlotId++)
                {
                    for (int personId = 0; personId < p.s.People.Count; personId++)
                    {
                        // Check whether the person is available at the time slot
                        if (p.r.v[personId][timeSlotId].Length == 1 && p.r.v[personId][timeSlotId][0][0] == false) continue;
                        // Check whether the person has the required skills for the task
                        if (p.r.v[personId][timeSlotId][taskId].Length == 1 && p.r.v[personId][timeSlotId][taskId][0] == false) continue;
                        for (int locationId = 0; locationId < p.r.v[personId][timeSlotId][taskId].Length; locationId++)
                        {
                            if (p.r.v[personId][timeSlotId][taskId][locationId])
                                skills.UnionWith(p.s.Locations[locationId].Skills);
                        }
                    }
                }
                Console.WriteLine("Task: " + taskId);
                Array.ForEach(skills.ToArray(), Console.Write);
                Console.WriteLine(" ");
                Array.ForEach(p.s.Tasks[taskId].Skills, Console.Write);
                Console.WriteLine(" ");
                Assert.IsTrue(p.s.Tasks[taskId].Skills.ToHashSet().IsSubsetOf(skills.ToArray()));
            }
        }

        // Hard constraint 5 - Workload (Person)
        [DataTestMethod]
        [DynamicData(nameof(TestData), DynamicDataSourceType.Method)]
        public void PeopleWorkAtMostAsManyTasksAsTheirWorkload((Scenario s, (int o, bool[][][][] v, string m) r) p)
        {
            // we need to assure that each person works at most as many time slots as their workload specifies

            for (int personId = 0; personId < p.r.v.Length; personId++)
            {
                var nrOfTimeSlots = 0;
                for (int taskId = 0; taskId < p.s.Tasks.Count; taskId++)
                {
                    for (int locationId = 0; locationId < p.s.Locations.Count; locationId++)
                    {
                        for (int timeSlotId = 0; timeSlotId < p.s.TimeSlots.Count; timeSlotId++)
                        {
                            // Check whether the person is available at the time slot
                            if (p.r.v[personId][timeSlotId].Length == 1 && p.r.v[personId][timeSlotId][0][0] == false) continue;
                            // Check whether the person has the required skills for the task
                            if (p.r.v[personId][timeSlotId][taskId].Length == 1 && p.r.v[personId][timeSlotId][taskId][0] == false) continue;
                            if (p.r.v[personId][timeSlotId][taskId][locationId]) nrOfTimeSlots++;
                        }
                    }
                }
                var workloadPercentage = 100 / p.s.TimeSlots.Count * nrOfTimeSlots;
                Console.WriteLine("Person: " + personId);
                Console.WriteLine("  Actual: " + workloadPercentage);
                Console.WriteLine("  Should be: " + p.s.People[personId].Workload);
                Assert.IsTrue(workloadPercentage <= p.s.People[personId].Workload);
            }
        }

        // Hard constraint 6 - Task requires specific people
        [DataTestMethod]
        [DynamicData(nameof(TestData), DynamicDataSourceType.Method)]
        public void TaskRequiresSpecificPeople((Scenario s, (int o, bool[][][][] v, string m) r) p)
        {
            // we need to assure that a task employes specific people if required

            for (int taskId = 0; taskId < p.s.Tasks.Count; taskId++)
            {
                // Check whether the task does require specific people
                if (p.s.Tasks[taskId].ReqSpecPpl.Length == 0) continue;
                // Collect all people
                var people = new HashSet<int>();
                for (int personId = 0; personId < p.s.People.Count; personId++)
                {
                    for (int timeSlotId = 0; timeSlotId < p.s.TimeSlots.Count; timeSlotId++)
                    {
                        // Check whether the person is available at the time slot
                        if (p.r.v[personId][timeSlotId].Length == 1 && p.r.v[personId][timeSlotId][0][0] == false) continue;
                        // Check whether the person has the required skills for the task
                        if (p.r.v[personId][timeSlotId][taskId].Length == 1 && p.r.v[personId][timeSlotId][taskId][0] == false) continue;
                        for (int locationId = 0; locationId < p.r.v[personId][timeSlotId][taskId].Length; locationId++)
                        {
                            if (p.r.v[personId][timeSlotId][taskId][locationId])
                                people.Add(p.s.People[personId].Id);
                        }
                    }
                }
                Console.WriteLine("Task: " + taskId);
                Array.ForEach(people.ToArray(), Console.Write);
                Console.WriteLine(" ");
                Array.ForEach(p.s.Tasks[taskId].ReqSpecPpl, Console.Write);
                Console.WriteLine(" ");
                Assert.IsTrue(p.s.Tasks[taskId].ReqSpecPpl.ToHashSet().IsSubsetOf(people.ToArray()));
            }
        }

        // Task requires specific time slots
        [DataTestMethod]
        [DynamicData(nameof(TestData), DynamicDataSourceType.Method)]
        public void TaskRequiresSpecificTimeSlots((Scenario s, (int o, bool[][][][] v, string m) r) p)
        {
            // we need to assure that a task occurs at specific time slots if required

            for (int taskId = 0; taskId < p.s.Tasks.Count; taskId++)
            {
                // Check whether the task does require specific time slots
                if (p.s.Tasks[taskId].ReqSpecTS.Length == 0) continue;
                // Collect all time slots
                var timeSlots = new HashSet<int>();
                for (int timeSlotId = 0; timeSlotId < p.s.TimeSlots.Count; timeSlotId++)
                {
                    for (int personId = 0; personId < p.s.People.Count; personId++)
                    {
                        // Check whether the person is available at the time slot
                        if (p.r.v[personId][timeSlotId].Length == 1 && p.r.v[personId][timeSlotId][0][0] == false) continue;
                        // Check whether the person has the required skills for the task
                        if (p.r.v[personId][timeSlotId][taskId].Length == 1 && p.r.v[personId][timeSlotId][taskId][0] == false) continue;
                        for (int locationId = 0; locationId < p.r.v[personId][timeSlotId][taskId].Length; locationId++)
                        {
                            if (p.r.v[personId][timeSlotId][taskId][locationId])
                                timeSlots.Add(p.s.TimeSlots[timeSlotId].Id);
                        }
                    }
                }
                Console.WriteLine("Task: " + taskId);
                Array.ForEach(timeSlots.ToArray(), Console.Write);
                Console.WriteLine(" ");
                Array.ForEach(p.s.Tasks[taskId].ReqSpecTS, Console.Write);
                Console.WriteLine(" ");
                Assert.IsTrue(p.s.Tasks[taskId].ReqSpecTS.ToHashSet().IsSubsetOf(timeSlots.ToArray()));
            }
        }

        // Task requires specific location
        [DataTestMethod]
        [DynamicData(nameof(TestData), DynamicDataSourceType.Method)]
        public void TaskRequiresSpecificLocation((Scenario s, (int o, bool[][][][] v, string m) r) p)
        {
            // we need to assure that a task occurs at specific time slots if required

            for (int taskId = 0; taskId < p.s.Tasks.Count; taskId++)
            {
                // Check whether the task does require specific time slots
                if (p.s.Tasks[taskId].ReqSpecLoc.Length == 0) continue;
                // Collect all locations
                var locations = new HashSet<int>();
                for (int locationId = 0; locationId < p.s.Locations.Count; locationId++)
                {
                    for (int personId = 0; personId < p.s.People.Count; personId++)
                    {
                        for (int timeSlotId = 0; timeSlotId < p.s.TimeSlots.Count; timeSlotId++)
                        {
                            // Check whether the person is available at the time slot
                            if (p.r.v[personId][timeSlotId].Length == 1 && p.r.v[personId][timeSlotId][0][0] == false) continue;
                            // Check whether the person has the required skills for the task
                            if (p.r.v[personId][timeSlotId][taskId].Length == 1 && p.r.v[personId][timeSlotId][taskId][0] == false) continue;
                            if (p.r.v[personId][timeSlotId][taskId][locationId])
                                locations.Add(p.s.TimeSlots[timeSlotId].Id);
                        }
                    }
                }
                Console.WriteLine("Task: " + taskId);
                Array.ForEach(locations.ToArray(), Console.Write);
                Console.WriteLine(" ");
                Array.ForEach(p.s.Tasks[taskId].ReqSpecLoc, Console.Write);
                Console.WriteLine(" ");
                Assert.IsTrue(p.s.Tasks[taskId].ReqSpecLoc.ToHashSet().IsSubsetOf(locations.ToArray()));
            }
        }


        /**
         *
         * SOFT CONSTRAINTS
         *
         */

        // Soft constraint 1
        [DataTestMethod]
        [DynamicData(nameof(TestData), DynamicDataSourceType.Method)]
        public void TheLeastAmountOfLocationsAreUsed((Scenario s, (int o, bool[][][][] v, string m) r) p)
        {
            for (int locationId = 0; locationId < p.s.Locations.Count; locationId++)
            {
                var tasks = new HashSet<SchedulingTask>();
                for (int personId = 0; personId < p.s.People.Count; personId++)
                {
                    for (int timeSlotId = 0; timeSlotId < p.s.TimeSlots.Count; timeSlotId++)
                    {
                        // Check whether the person is available at the time slot
                        if (p.r.v[personId][timeSlotId].Length == 1 && p.r.v[personId][timeSlotId][0][0] == false) continue;
                        for (int taskId = 0; taskId < p.s.Tasks.Count; taskId++)
                        {
                            // Check whether the person has the required skills for the task
                            if (p.r.v[personId][timeSlotId][taskId].Length == 1 && p.r.v[personId][timeSlotId][taskId][0] == false) continue;
                            if (p.r.v[personId][timeSlotId][taskId][locationId])
                                tasks.Add(p.s.Tasks[taskId]);
                        }
                    }
                }
                Console.WriteLine("Location: " + p.s.Locations[locationId].Id);
                Array.ForEach(tasks.ToArray(), Console.Write);
                Console.WriteLine(" ");
                Console.WriteLine("Amount: " + tasks.ToArray().Length);
            }
        }

        // Soft constraint 2
        [DataTestMethod]
        [DynamicData(nameof(TestData), DynamicDataSourceType.Method)]
        public void TheLeastAmountOfTimeSlotsAreUsed((Scenario s, (int o, bool[][][][] v, string m) r) p)
        {
            for (int timeSlotId = 0; timeSlotId < p.s.TimeSlots.Count; timeSlotId++)
            {
                var tasks = new HashSet<SchedulingTask>();
                for (int personId = 0; personId < p.s.People.Count; personId++)
                {
                    // Check whether the person is available at the time slot
                    if (p.r.v[personId][timeSlotId].Length == 1 && p.r.v[personId][timeSlotId][0][0] == false) continue;
                    for (int locationId = 0; locationId < p.s.Locations.Count; locationId++)
                    {
                        for (int taskId = 0; taskId < p.s.Tasks.Count; taskId++)
                        {
                            // Check whether the person has the required skills for the task
                            if (p.r.v[personId][timeSlotId][taskId].Length == 1 && p.r.v[personId][timeSlotId][taskId][0] == false) continue;
                            if (p.r.v[personId][timeSlotId][taskId][locationId])
                                tasks.Add(p.s.Tasks[taskId]);
                        }
                    }
                }
                Console.WriteLine("Time slot: " + p.s.TimeSlots[timeSlotId].Id);
                Array.ForEach(tasks.ToArray(), Console.Write);
                Console.WriteLine(" ");
                Console.WriteLine("Amount: " + tasks.ToArray().Length);
            }
        }

        // // Soft constraint 3
        // [DataTestMethod]
        // [DynamicData(nameof(TestData), DynamicDataSourceType.Method)]
        // public void PeopleChangeTheLocationALittleAsPossible((Scenario s, (int o, bool[][][][] v, string m) r) p)
        // {
        //     for (var personId = 0; personId < p.s.MinLocationChanges.Count; personId++)
        //     {
        //         var locations = new HashSet<Location>();
        //         for (var timeSlotId = 0;
        //              timeSlotId < p.r.v[p.s.MinLocationChanges[personId].Id].Length;
        //              timeSlotId++)
        //         {
        //             for (var taskId = 0;
        //                  taskId < p.r.v[p.s.MinLocationChanges[personId].Id][timeSlotId].Length;
        //                  taskId++)
        //             {
        //                 for (var locationId = 0;
        //                      locationId < p.r.v[p.s.MinLocationChanges[personId].Id][timeSlotId][taskId].Length;
        //                      locationId++)
        //                 {
        //                     if (p.r.v[p.s.MinLocationChanges[personId].Id][timeSlotId][taskId][locationId])
        //                         locations.Add(p.s.Locations[locationId]);
        //                 }
        //             }
        //         }
        //         Console.WriteLine("Person: " + p.s.MinLocationChanges[personId].Id);
        //         Array.ForEach(locations.ToArray(), l => Console.Write(l));
        //         Console.WriteLine(" ");
        //         Console.WriteLine("Amount: " + locations.ToArray().Length);
        //     }
        // }

        // // Soft constraint 4
        // [DataTestMethod]
        // [DynamicData(nameof(TestData), DynamicDataSourceType.Method)]
        // public void PeopleHaveEmptyTimeSlotsALittleAsPossible((Scenario s, (int o, bool[][][][] v, string m) r) p)
        // {
        //     for (var personId = 0; personId < p.s.MinEmptyTS.Count; personId++)
        //     {
        //         var lastTimeSlot = new TimeSlot(-1, "");
        //         var emptyTimeSlots = new HashSet<TimeSlot>();
        //         for (var timeSlotId = 0;
        //              timeSlotId < p.r.v[p.s.MinEmptyTS[personId].Id].Length;
        //              timeSlotId++)
        //         {
        //             for (var taskId = 0;
        //                  taskId < p.r.v[p.s.MinEmptyTS[personId].Id][timeSlotId].Length;
        //                  taskId++)
        //             {
        //                 for (var locationId = 0;
        //                      locationId < p.r.v[p.s.MinEmptyTS[personId].Id][timeSlotId][taskId].Length;
        //                      locationId++)
        //                 {
        //                     if (p.r.v[p.s.MinEmptyTS[personId].Id][timeSlotId][taskId][locationId])
        //                     {
        //                         if (p.s.TimeSlots[timeSlotId].Id - lastTimeSlot.Id > 1)
        //                             emptyTimeSlots.Add(p.s.TimeSlots[timeSlotId]);
        //                         lastTimeSlot = p.s.TimeSlots[timeSlotId];
        //                     }
        //                 }
        //             }
        //         }
        //         Console.WriteLine("Person: " + p.s.MinEmptyTS[personId].Id);
        //         Array.ForEach(emptyTimeSlots.ToArray(), Console.Write);
        //         Console.WriteLine(" ");
        //         Console.WriteLine("Amount: " + emptyTimeSlots.ToArray().Length);
        //     }
        // }
    }
}