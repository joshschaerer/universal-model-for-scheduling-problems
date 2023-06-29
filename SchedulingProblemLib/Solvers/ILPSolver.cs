using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Gurobi;
using SchedulingProblem.Model;
using SchedulingProblem.Solvers;

namespace SchedulingProblem.Solvers
{
    /// <summary>
    /// ILP course model
    /// </summary>
    public class ILPSolver : ISolver
    {
        private readonly Scenario _scene;
        private readonly int timelimit;
        private GRBVar[][][][] _vars;
        public ILPSolver(Scenario scenario, int timelimit)
        {
            this._scene = scenario;
            this.timelimit = timelimit;
        }

        /// <summary>
        /// Creates and solves the model 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="TimeoutException"></exception>
        /// <exception cref="InfeasibleException"></exception>
        public (int, bool[][][][], string) Optimize(bool noreturn = false)
        {
            using var env = new GRBEnv();

            env.TimeLimit = timelimit;
            env.Set(GRB.IntParam.Seed, new Random().Next(0, 2000000000));
            using var model = new GRBModel(env);
            CreateVariables(model);
            CreateConstraints(model);
            CreateObjective(model);
            model.Optimize();
            int status = model.Status;
            if (noreturn)
            {
                if (status == GRB.Status.TIME_LIMIT && model.SolCount == 0)
                {
                    throw new TimeoutException();
                }
                if (status == GRB.Status.INFEASIBLE)
                {
                    throw new InfeasibleException();
                }
                if (status == GRB.Status.OPTIMAL || (status == GRB.Status.TIME_LIMIT && model.SolCount > 0))
                {
                    return ((int)Math.Round(model.ObjVal, 0), null, "");
                }

            }
            if (status == GRB.Status.UNBOUNDED)
            {
                return (-1, _vars.Select(l => l.Select(p => p.Select(ts => ts.Select(t => t.X > 0).ToArray()).ToArray()).ToArray()).ToArray(), "The model cannot be solved because it is unbounded");
            }
            if (status == GRB.Status.TIME_LIMIT && model.SolCount == 0)
            {
                throw new TimeoutException();
            }
            if (status == GRB.Status.OPTIMAL || (status == GRB.Status.TIME_LIMIT && model.SolCount > 0))
            {

                bool[][][][] x = new bool[_scene.People.Count][][][];
                for (var personId = 0; personId < _scene.People.Count; personId++)
                {
                    bool[][][] x2 = new bool[_scene.TimeSlots.Count][][];
                    for (var timeSlotId = 0; timeSlotId < _scene.TimeSlots.Count; timeSlotId++)
                    {
                        // Check whether the person is available at the time slot
                        if (!_scene.People[personId].Absences.Contains(timeSlotId))
                        {
                            bool[][] x3 = new bool[_scene.Tasks.Count][];
                            for (var taskId = 0; taskId < _scene.Tasks.Count; taskId++)
                            {
                                // Check whether the person has the required skills for the task or is specifically required
                                if (_scene.Tasks[taskId].Skills.Length == 0 || _scene.People[personId].Skills
                                        .Any(_scene.Tasks[taskId].Skills.Contains) || _scene.Tasks[taskId].ReqSpecPpl.Contains(_scene.People[personId].Id))
                                {
                                    bool[] x4 = new bool[_scene.Locations.Count];
                                    for (var locationId = 0; locationId < _scene.Locations.Count; locationId++)
                                    {
                                        var location = _scene.Locations[locationId];
                                        // Check whether the location provides the required skills for the task or is specifically required
                                        if (_scene.Tasks[taskId].Skills.Length == 0 ||
                                            _scene.Locations[locationId].Skills.Length == 0 || _scene
                                                .Locations[locationId]
                                                .Skills
                                                .Any(_scene.Tasks[taskId].Skills.Contains) || _scene.Tasks[taskId].ReqSpecLoc.Contains(_scene.Locations[locationId].Id))
                                        {
                                            x4[locationId] =
                                                _vars[personId][timeSlotId][taskId][locationId].X > 0;
                                        }
                                        else
                                        {
                                            x4[locationId] = false;
                                        }
                                    }

                                    x3[taskId] = x4;
                                }
                                else
                                {
                                    x3[taskId] = new bool[] { false };
                                }
                            }

                            x2[timeSlotId] = x3;
                        }
                        else
                        {
                            x2[timeSlotId] = new bool[][] { new bool[] { false } };
                        }
                    }

                    x[personId] = x2;
                } 

                return ((int)Math.Round(model.ObjVal, 0), x, "");
            }
            if (status == GRB.Status.INF_OR_UNBD ||
                status == GRB.Status.INFEASIBLE)
            {
                model.ComputeIIS();
                foreach (var i in model.GetConstrs())
                {
                    if (i.IISConstr == 1)
                    {
                        Console.WriteLine(i.ConstrName + "\t \t" + i.Sense + " " + i.RHS);
                    }
                }
                throw new InfeasibleException();

            }
            return (-1, _vars.Select(l => l.Select(p => p.Select(ts => ts.Select(t => t.X > 0).ToArray()).ToArray()).ToArray()).ToArray(), "Optimization was stopped with status " + status);
        }

        private void CreateVariables(GRBModel model)
        {
            _vars = new GRBVar[_scene.People.Count][][][];
            for (var personId = 0; personId < _scene.People.Count; personId++)
            {
                GRBVar[][][] timeslots = new GRBVar[_scene.TimeSlots.Count][][];
                for (var timeSlotId = 0; timeSlotId < _scene.TimeSlots.Count; timeSlotId++)
                {
                    // Check whether the person is available at the time slot
                    if (_scene.People[personId].Absences.Contains(timeSlotId))
                    {
                        timeslots[timeSlotId] = new GRBVar[][] { new GRBVar[] { } };
                    }
                    else
                    {
                        GRBVar[][] tasks = new GRBVar[_scene.Tasks.Count][];
                        for (var taskId = 0; taskId < _scene.Tasks.Count; taskId++)
                        {
                            // Check whether the person has the required skills for the task or is specifically required
                            if (_scene.Tasks[taskId].Skills.Length == 0 || _scene.People[personId].Skills
                                    .Any(_scene.Tasks[taskId].Skills.Contains) || _scene.Tasks[taskId].ReqSpecPpl.Contains(_scene.People[personId].Id))
                            {
                                GRBVar[] locations = new GRBVar[_scene.Locations.Count];
                                for (var locationId = 0; locationId < _scene.Locations.Count; locationId++)
                                {
                                    // Check whether the location provides the required skills for the task or is specifically required
                                    if (_scene.Tasks[taskId].Skills.Length == 0 ||
                                        _scene.Locations[locationId].Skills.Length == 0 || _scene
                                            .Locations[locationId].Skills
                                            .Any(_scene.Tasks[taskId].Skills.Contains) || _scene.Tasks[taskId].ReqSpecLoc.Contains(_scene.Locations[locationId].Id))
                                    {
                                        locations[locationId] = model.AddVar(0, 1, _scene.People[personId].Wage, GRB.BINARY, null); ;
                                    }
                                    else
                                    {
                                        locations[locationId] = null;
                                    }
                                }
                                tasks[taskId] = locations;
                            }
                            else
                            {
                                tasks[taskId] = new GRBVar[] { };
                            }
                        }
                        timeslots[timeSlotId] = tasks;
                    }
                }
                _vars[personId] = timeslots;
            }
        }

        private void CreateConstraints(GRBModel model)
        {
            // === Default Constraints ===

            // --- Each Task recurs n times ---
            // --- Each Task occurs at specific time slots if required ---
            for (int taskId = 0; taskId < _scene.Tasks.Count; taskId++)
            {
                GRBLinExpr nrOfUsedTimeSlots = 0.0;
                for (int timeSlotId = 0; timeSlotId < _scene.TimeSlots.Count; timeSlotId++)
                {
                    var isTimeSlotUsed = new List<GRBVar>();
                    for (int personId = 0; personId < _scene.People.Count; personId++)
                    {
                        // Check whether the person is available at the time slot
                        if (_vars[personId][timeSlotId].Length == 1 && _vars[personId][timeSlotId][0].Length == 0) continue;
                        // Check whether the person has the required skills for the task
                        if (_vars[personId][timeSlotId][taskId].Length == 0) continue;
                        for (int locationId = 0; locationId < _scene.Locations.Count; locationId++)
                        {
                            // Check whether the location provides the required skills for the task
                            if (_vars[personId][timeSlotId][taskId][locationId] is null) continue;
                            var v = _vars[personId][timeSlotId][taskId][locationId];
                            isTimeSlotUsed.Add(v);
                        }
                    }
                    //if ANY of the variables in a timeslot are assigned => 1 otherwise => 0 
                    var x = model.AddVar(0, 1, 0, GRB.BINARY, null);
                    model.AddGenConstrOr(x, isTimeSlotUsed.ToArray(), null);
                    nrOfUsedTimeSlots += x;

                    // Check whether the task specifically requires the time slot
                    if (_scene.HasReqTS)
                    {
                        // Check whether the task specifically requires the time slot
                        if (_scene.Tasks[taskId].ReqSpecTS
                            .Contains(_scene.TimeSlots[timeSlotId].Id)) model.AddConstr(x == 1, null);
                    }
                }
                model.AddConstr(nrOfUsedTimeSlots == _scene.Tasks[taskId].Reps, "Reps " + taskId);
            }

            if (_scene.HasReqNrOfPpl)
            {
                for (int taskId = 0; taskId < _scene.Tasks.Count; taskId++)
                {

                    for (int timeSlotId = 0; timeSlotId < _scene.TimeSlots.Count; timeSlotId++)
                    {

                        for (int locationId = 0; locationId < _scene.Locations.Count; locationId++)
                        {
                            var possiblePeople = new List<GRBVar>();
                            GRBLinExpr nrOfPeople = 0.0;
                            for (int personId = 0; personId < _scene.People.Count; personId++)
                            {
                                // Check whether the person is available at the time slot
                                if (_vars[personId][timeSlotId].Length == 1 && _vars[personId][timeSlotId][0].Length == 0) continue;
                                // Check whether the person has the required skills for the task
                                if (_vars[personId][timeSlotId][taskId].Length == 0) continue;
                                // Check whether the location provides the required skills for the task
                                if (_vars[personId][timeSlotId][taskId][locationId] is null) continue;
                                var v = _vars[personId][timeSlotId][taskId][locationId];
                                possiblePeople.Add(v);
                                nrOfPeople += v;
                            }
                            var isAnyoneAssigned = model.AddVar(0, 1, 0, GRB.BINARY, "is Anyone Assigned to task " + taskId + " in TimeSlot " + timeSlotId + " at location " + locationId);
                            model.AddGenConstrOr(isAnyoneAssigned, possiblePeople.ToArray(), null);
                            model.AddGenConstrIndicator(isAnyoneAssigned, 1, nrOfPeople, GRB.GREATER_EQUAL, _scene.Tasks[taskId].ReqPpl, null);
                        }
                    }
                }
            }

            if (_scene.HasReqPeople)
            {
                // --- Each Task employes specific people if required ---
                for (int taskId = 0; taskId < _scene.Tasks.Count; taskId++)
                {
                    for (int personId = 0; personId < _scene.People.Count; personId++)
                    {
                        GRBLinExpr nrOfEmployments = 0.0;
                        for (int timeSlotId = 0; timeSlotId < _scene.TimeSlots.Count; timeSlotId++)
                        {
                            // Check whether the person is available at the time slot
                            if (_vars[personId][timeSlotId].Length == 1 && _vars[personId][timeSlotId][0].Length == 0) continue;
                            // Check whether the person has the required skills for the task
                            if (_vars[personId][timeSlotId][taskId].Length == 0) continue;
                            for (int locationId = 0; locationId < _scene.Locations.Count; locationId++)
                            {
                                // Check whether the location provides the required skills for the task
                                if (_vars[personId][timeSlotId][taskId][locationId] is null) continue;
                                var v = _vars[personId][timeSlotId][taskId][locationId];
                                nrOfEmployments += 1 * v;
                            }
                        }
                        // Check whether the task specifically requires the person
                        if (_scene.Tasks[taskId].ReqSpecPpl.Contains(_scene.People[personId].Id))
                            model.AddConstr(nrOfEmployments >= 1, "Require Specific People " + personId + " Task " + taskId);
                    }
                }
            }

            if (_scene.HasReqLoc)
            {
                // --- Each Task has a specific location if required ---
                for (int taskId = 0; taskId < _scene.Tasks.Count; taskId++)
                {
                    for (int locationId = 0; locationId < _scene.Locations.Count; locationId++)
                    {
                        GRBLinExpr nrOfLocations = 0.0;
                        for (int personId = 0; personId < _scene.People.Count; personId++)
                        {
                            for (int timeSlotId = 0; timeSlotId < _scene.TimeSlots.Count; timeSlotId++)
                            {
                                // Check whether the person is available at the time slot
                                if (_vars[personId][timeSlotId].Length == 1 && _vars[personId][timeSlotId][0].Length == 0) continue;
                                // Check whether the person has the required skills for the task
                                if (_vars[personId][timeSlotId][taskId].Length == 0) continue;
                                // Check whether the location provides the required skills for the task
                                if (_vars[personId][timeSlotId][taskId][locationId] is null) continue;
                                var v = _vars[personId][timeSlotId][taskId][locationId];
                                nrOfLocations += 1 * v;
                            }
                        }
                        // Check whether the task specifically requires the location
                        if (_scene.Tasks[taskId].ReqSpecLoc.Contains(_scene.Locations[locationId].Id))
                            model.AddConstr(nrOfLocations == _scene.Tasks[taskId].Reps, null);
                    }
                }
            }

            // --- Each person works at most n tasks per time slot --- Capacity
            for (int personId = 0; personId < _vars.Length; personId++)
            {
                for (int timeSlotId = 0; timeSlotId < _vars[personId].Length; timeSlotId++)
                {
                    // Check whether the person is available at the time slot
                    if (_vars[personId][timeSlotId].Length == 1 && _vars[personId][timeSlotId][0].Length == 0)
                        continue;
                    GRBLinExpr nrOfWorkedTasks = 0.0;
                    for (int taskId = 0; taskId < _vars[personId][timeSlotId].Length; taskId++)
                    {
                        // Check whether the person has the required skills for the task
                        if (_vars[personId][timeSlotId][taskId].Length == 0) continue;
                        for (int locationId = 0;
                             locationId < _vars[personId][timeSlotId][taskId].Length;
                             locationId++)
                        {
                            // Check whether the location provides the required skills for the task
                            if (_vars[personId][timeSlotId][taskId][locationId] is null) continue;
                            var v = _vars[personId][timeSlotId][taskId][locationId];
                            nrOfWorkedTasks += 1 * v;
                        }
                    }
                    model.AddConstr(nrOfWorkedTasks <= _scene.People[personId].Capacity, null);
                }
            }

            // Each person works at most in one location per time slot.
            // "You can't change room fast enough"
            for (var personId = 0; personId < _scene.People.Count; personId++)
            {
                if (_scene.People[personId].Capacity > 1) continue;
                for (var timeSlotId = 0; timeSlotId < _scene.TimeSlots.Count; timeSlotId++)
                {
                    GRBLinExpr nrOfLocations = 0.0;

                    for (var locationId = 0; locationId < _scene.Locations.Count; locationId++)
                    {
                        // Check whether the person is available at the time slot
                        if (_vars[personId][timeSlotId].Length == 1 && _vars[personId][timeSlotId][0].Length == 0) continue;
                        for (var taskId = 0; taskId < _scene.Tasks.Count; taskId++)
                        {
                            // Check whether the person has the required skills for the task
                            if (_vars[personId][timeSlotId][taskId].Length == 0) continue;

                            // Check whether the location provides the required skills for the task
                            if (_vars[personId][timeSlotId][taskId][locationId] is null) continue;
                            nrOfLocations.Add(_vars[personId][timeSlotId][taskId][locationId]);
                        }
                    }
                    model.AddConstr(nrOfLocations <= 1, null);
                }

            }
            model.Update();
        }

        private void CreateObjective(GRBModel model)
        {
            // === Objective ===
            GRBLinExpr objective = 0.0;

            // === Soft constraints === 
            // Number of used time slots per person (helper variable)
            var numberOfUsedTimeSlotsPerPerson = model.AddVars(_scene.People.Count, GRB.INTEGER);
            for (int personId = 0; personId < _vars.Length; personId++)
            {
                GRBLinExpr usedTimeSlots = 0.0;
                for (int timeSlotId = 0; timeSlotId < _vars[personId].Length; timeSlotId++)
                {
                    var possibleAssignments = new List<GRBVar>();
                    if (_vars[personId][timeSlotId].Length == 1 && _vars[personId][timeSlotId][0].Length == 0) continue;// Check whether the person is available at the time slot
                    for (int taskId = 0; taskId < _vars[personId][timeSlotId].Length; taskId++)
                    {
                        if (_vars[personId][timeSlotId][taskId].Length == 0) continue;// Check whether the person has the required skills for the task
                        for (int locationId = 0; locationId < _vars[personId][timeSlotId][taskId].Length; locationId++)
                        {
                            var v = _vars[personId][timeSlotId][taskId][locationId];
                            if (v is null) continue;// Check whether the location provides the required skills for the task
                            possibleAssignments.Add(v);
                        }
                    }
                    var isTimeSlotUsed = model.AddVar(0, 1, 0, GRB.BINARY, null);
                    model.AddGenConstrOr(isTimeSlotUsed, possibleAssignments.ToArray(), "isTimeSlotUsed" + timeSlotId);
                    usedTimeSlots += isTimeSlotUsed;
                }
                model.AddConstr(numberOfUsedTimeSlotsPerPerson[personId] == usedTimeSlots, "Number of Timeslots used: Person " + personId);
            }

            // --- People have travel costs ---
            // --- Each person has a wage ---
            // --- try to hit workloads ---
            var workloadDeviations = new List<GRBVar>();
            for (int personId = 0; personId < _vars.Length; personId++)
            {
                for (int timeSlotId = 0; timeSlotId < _vars[personId].Length; timeSlotId++)
                {
                    if (_vars[personId][timeSlotId].Length == 1 && _vars[personId][timeSlotId][0].Length == 0) continue;// Check whether the person is available at the time slot
                    for (int taskId = 0; taskId < _vars[personId][timeSlotId].Length; taskId++)
                    {
                        if (_vars[personId][timeSlotId][taskId].Length == 0) continue;// Check whether the person has the required skills for the task
                        for (int locationId = 0; locationId < _vars[personId][timeSlotId][taskId].Length; locationId++)
                        {
                            var v = _vars[personId][timeSlotId][taskId][locationId];
                            if (v is null) continue;// Check whether the location provides the required skills for the task
                            if (_scene.TravelCostWeight != 0) objective += _scene.TravelCostWeight * _scene.TravelCost[personId, locationId] * v;
                            if (_scene.WageWeight != 0) objective += _scene.WageWeight * _scene.People[personId].Wage * v;
                        }
                    }
                }
                if (_scene.FairnessWeight != 0)
                {
                    var operand = 100 / (_scene.TimeSlots.Count - _scene.People[personId].Absences.Length);
                    var x = model.AddVar(-100, 100, 0, GRB.INTEGER, null);
                    model.AddConstr(x == (numberOfUsedTimeSlotsPerPerson[personId] * operand) - _scene.People[personId].Workload, "workload deviation " + personId);
                    var y = model.AddVar(-100, 100, 0, GRB.INTEGER, null);
                    model.AddGenConstrAbs(y, x, "absolute workload deviation" + personId);
                    workloadDeviations.Add(y);
                }
                // for each person save their percent deviations 

            }
            if (_scene.FairnessWeight != 0)
            {
                var max = model.AddVar(0, 100, 0, GRB.INTEGER, null);
                var min = model.AddVar(0, 100, 0, GRB.INTEGER, null);
                model.AddGenConstrMax(max, workloadDeviations.ToArray(), 0, "workload max");
                model.AddGenConstrMin(min, workloadDeviations.ToArray(), 100, "workload min");
                objective += _scene.FairnessWeight * (max - min);
            }


            if (_scene.PrefNrOfPeopleWeight != 0)
            {
                for (int taskId = 0; taskId < _scene.Tasks.Count; taskId++)
                {
                    GRBLinExpr usedPeople = new GRBLinExpr();
                    for (int personId = 0; personId < _scene.People.Count; personId++)
                    {
                        var possiblePeople = new List<GRBVar>();
                        for (int timeSlotId = 0; timeSlotId < _scene.TimeSlots.Count; timeSlotId++)
                        {
                            // Check whether the person is available at the time slot
                            if (_vars[personId][timeSlotId].Length == 1 && _vars[personId][timeSlotId][0].Length == 0) continue;
                            // Check whether the person has the required skills for the task
                            if (_vars[personId][timeSlotId][taskId].Length == 0) continue;
                            for (int locationId = 0; locationId < _scene.Locations.Count; locationId++)
                            {
                                // Check whether the location provides the required skills for the task
                                if (_vars[personId][timeSlotId][taskId][locationId] is null) continue;
                                var v = _vars[personId][timeSlotId][taskId][locationId];
                                possiblePeople.Add(v);
                            }
                        }
                        var z = model.AddVar(0, 1, 0, GRB.BINARY, "usedPeople");
                        model.AddGenConstrOr(z, possiblePeople.ToArray(), "");
                        usedPeople += z;
                    }

                    var operand = usedPeople - _scene.Tasks[taskId].PrefPpl;
                    // Temporary decision variables
                    var x = model.AddVar(-_scene.People.Count, _scene.People.Count, 0, GRB.INTEGER, null);
                    model.AddConstr(x == operand, null);
                    var y = model.AddVar(-_scene.People.Count, _scene.People.Count, 0, GRB.INTEGER, null);
                    model.AddGenConstrAbs(y, x, null);
                    objective += _scene.PrefNrOfPeopleWeight * y;
                }
            }

            // The least amount of locations are required / using Locations costs money
            if (_scene.minLocWeight != 0)
            {
                for (var locationId = 0; locationId < _scene.Locations.Count; locationId++)
                {
                    var possibleAssignments = new List<GRBVar>();
                    for (var personId = 0; personId < _scene.People.Count; personId++)
                    {
                        for (var timeSlotId = 0; timeSlotId < _scene.TimeSlots.Count; timeSlotId++)
                        {
                            // Check whether the person is available at the time slot
                            if (_vars[personId][timeSlotId].Length == 1 && _vars[personId][timeSlotId][0].Length == 0) continue;
                            for (var taskId = 0; taskId < _scene.Tasks.Count; taskId++)
                            {
                                // Check whether the person has the required skills for the task
                                if (_vars[personId][timeSlotId][taskId].Length == 0) continue;
                                // Check whether the location provides the required skills for the task
                                if (_vars[personId][timeSlotId][taskId][locationId] is null) continue;
                                possibleAssignments.Add(_vars[personId][timeSlotId][taskId][locationId]);
                            }
                        }
                    }
                    var x = model.AddVar(0, 1, 0, GRB.BINARY, "usedLocations");
                    model.AddGenConstrOr(x, possibleAssignments.ToArray(), "");
                    objective += (_scene.minLocWeight * x);
                }
            }

            // The least amount of time slots are required
            if (_scene.minTSWeight != 0)
            {
                for (var timeSlotId = 0; timeSlotId < _scene.TimeSlots.Count; timeSlotId++)
                {
                    var possibleAssignments = new List<GRBVar>();
                    for (var personId = 0; personId < _scene.People.Count; personId++)
                    {
                        // Check whether the person is available at the time slot
                        if (_vars[personId][timeSlotId].Length == 1 && _vars[personId][timeSlotId][0].Length == 0) continue;
                        for (var locationId = 0; locationId < _scene.Locations.Count; locationId++)
                        {
                            for (var taskId = 0; taskId < _scene.Tasks.Count; taskId++)
                            {
                                // Check whether the person has the required skills for the task
                                if (_vars[personId][timeSlotId][taskId].Length == 0) continue;
                                // Check whether the location provides the required skills for the task
                                if (_vars[personId][timeSlotId][taskId][locationId] is null) continue;
                                possibleAssignments.Add(_vars[personId][timeSlotId][taskId][locationId]);
                            }
                        }
                    }
                    var x = model.AddVar(0, 1, 0, GRB.BINARY, "usedLocations");
                    model.AddGenConstrOr(x, possibleAssignments.ToArray(), "");
                    objective += (_scene.minTSWeight * x);
                }
            }

            model.SetObjective(objective, GRB.MINIMIZE);
            model.Update();
        }
    }
}