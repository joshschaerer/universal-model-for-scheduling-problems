using SchedulingProblem.Model;
using System;
using System.Collections.Generic;
namespace SchedulingProblem.Scenarios;

public static class NurseSchedulingProblem
{
    public static Scenario SpitalMuri()
    {
        List<Location> locations = new List<Location> { new(0, "Spital Muri", new int[] { }) };

        List<TimeSlot> days = new List<TimeSlot>();
        for (var i = 0; i < 31; i++)
        {
            days.Add(new TimeSlot(i, $"{i + 1}. Januar"));
        }

        List<Person> doctors = new List<Person>();
        doctors.Add(new Person(00, $"doc 00", 100, new int[] { 1, 8, 7, 12 }, new int[] { }, 1, 1));
        doctors.Add(new Person(01, $"doc 01", 100, new int[] { }, new int[] { }, 1, 1));
        doctors.Add(new Person(02, $"doc 02", 100, new int[] { }, new int[] { }, 1, 1));
        doctors.Add(new Person(03, $"doc 03", 100, new int[] { 1, 4, 7, 12 }, new int[] { }, 1, 1));
        doctors.Add(new Person(04, $"doc 04", 100, new int[] { }, new int[] { }, 1, 1));
        doctors.Add(new Person(05, $"doc 05", 100, new int[] { }, new int[] { }, 1, 1));
        doctors.Add(new Person(06, $"doc 06", 100, new int[] { 1, 3, 7, 12 }, new int[] { }, 1, 1));
        doctors.Add(new Person(07, $"doc 07", 100, new int[] { }, new int[] { }, 1, 1));
        doctors.Add(new Person(08, $"doc 08", 100, new int[] { }, new int[] { }, 1, 1));
        doctors.Add(new Person(09, $"doc 09", 100, new int[] { 1, 4, 9, 12 }, new int[] { }, 1, 1));
        doctors.Add(new Person(10, $"doc 10", 100, new int[] { }, new int[] { }, 1, 1));
        doctors.Add(new Person(11, $"doc 11", 100, new int[] { }, new int[] { }, 1, 1));
        doctors.Add(new Person(12, $"doc 12", 100, new int[] { }, new int[] { }, 1, 1));
        doctors.Add(new Person(13, $"doc 13", 100, new int[] { 1, 4, 7, 12 }, new int[] { }, 1, 1));
        doctors.Add(new Person(14, $"doc 14", 100, new int[] { 14, 28 }, new int[] { }, 1, 1));
        doctors.Add(new Person(15, $"doc 15", 100, new int[] { 15, 31 }, new int[] { }, 1, 1));
        doctors.Add(new Person(16, $"doc 16", 100, new int[] { 18, 19 }, new int[] { }, 1, 1));
        doctors.Add(new Person(17, $"doc 17", 100, new int[] { 1, 4, 7, 13 }, new int[] { }, 1, 1));

        List<SchedulingTask> tasks = new List<SchedulingTask>
        {
            new (0, "Frühdienst 06:30-12:30", 31, 1, new int[]{},new int[]{},new int[] {}, new int[]{}),
            new (1, "Spätdienst 16:30-03:30", 31, 1, new int[]{},new int[]{},new int[] {}, new int[]{}),
            new (2, "Zwischendienst 12:30-20:30", 31, 1, new int[]{},new int[]{},new int[] {}, new int[]{}),
            new (3, "Nachtdienst 23:00-08:30", 31, 1, new int[]{},new int[]{},new int[] {}, new int[]{}),
        };

        List<Skill> skills = new List<Skill>();


        Scenario scenario = new(doctors, days, tasks, locations, skills);
        scenario.HasAbsences = true;
        scenario.WageWeight = 1;
        scenario.FairnessWeight = 1;
        scenario.minTSWeight = 1;
        scenario.minLocWeight = 1;
        return scenario;
    }
}