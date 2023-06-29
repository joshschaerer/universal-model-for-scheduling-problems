using SchedulingProblem.Model;
using System.Collections.Generic;

namespace SchedulingProblem.Scenarios
{
    public static class TestScenario
    {
        public static Scenario ProblemOne()
        {
            Person p1 = new(0, "Jack", 60, new int[] { 2, 3 }, new int[] { 2 }, 20, 1),
                p2 = new(1, "Jones", 80, new int[] { 1 }, new int[] { 0, 1, 2 }, 10, 1),
                p3 = new(2, "Nick", 100, new int[] { 1, 3 }, new int[] { 0, 1 }, 12, 1),
                p4 = new(3, "Hans", 100, new int[] { 4, 5, 6 }, new int[] { 0, 1, 2 }, 14, 1),
                p5 = new(4, "Peter", 100, new int[] { 4 }, new int[] { 1, 2 }, 22, 1),
                p6 = new(5, "Max", 100, new int[] { 5, 6 }, new int[] { 2 }, 18, 1);
            TimeSlot s1 = new(0, "Monday"),
                s2 = new(1, "Tuesday"),
                s3 = new(2, "Wednesday"),
                s4 = new(3, "Thursday"),
                s5 = new(4, "Friday");
            SchedulingTask t1 = new(0, "Morning shift", 5, 2, new int[] { 5 }, new int[] { }, new int[] { }, new int[] { 0, 1 }),
                t2 = new(1, "Afternoon shift", 3, 1, new int[] { }, new int[] { 3, 4 }, new int[] { 1 }, new int[] { 0 }),
                t3 = new(2, "Night shift", 1, 2, new int[] { }, new int[] { }, new int[] { }, new int[] { 2 });
            Location l1 = new(0, "Room 1", new int[] { 0, 1 }),
                l2 = new(1, "Room 2", new int[] { 2 });

            Scenario scene = new Scenario(new List<Person> { p1, p2, p3, p4, p5, p6 },
                new List<TimeSlot> { s1, s2, s3, s4, s5 },
                new List<SchedulingTask> { t1, t2, t3 },
                new List<Location> { l1, l2 },
                new List<Skill> { new(0, "Nurse"), new(1, "Doctor"), new(2, "Surgeon") },
                false,
                new List<Person> { p2, p3 },
                new List<Person> { p4 });
            return scene;
        }

        public static Scenario Example()
        {
            return new Scenario
            (
                new List<Person> { new Person(0, "", 100, new int[] { }, new int[] { }, 1, 1) },
                new List<TimeSlot> { new TimeSlot(0, "") },
                new List<SchedulingTask> { new SchedulingTask(0, "", 1, 1, new int[] { }, new int[] { }, new int[] { }, new int[] { }) },
                new List<Location> { new Location(0, "", new int[] { }) },
                new List<Skill> { new(0, "") },
                false,
                new List<Person> { },
                new List<Person> { }
            );
        }
    }

}
