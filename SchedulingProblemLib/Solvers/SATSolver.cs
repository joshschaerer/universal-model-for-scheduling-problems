using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Gurobi;
using SATInterface;
using SchedulingProblem.Model;
using static Gurobi.GRB;

namespace SchedulingProblem.Solvers
{
    /// <summary>
    /// SAT course model
    /// </summary>
    public class SATSolver : ISolver
    {
        /// <summary>
        /// Scenario which should be solved
        /// </summary>
        private readonly Scenario _scene;
        /// <summary>
        /// Limitation on how long the solver may optimize in seconds
        /// </summary>
        private readonly int _timeLimit;
        /// <summary>
        /// 4-dimensional array which stores all variables needed
        /// </summary>
        private BoolExpr[][][][] _vars;
        private LinExpr _vObj;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="timeLimit"></param>
        public SATSolver(Scenario scene, int timeLimit)
        {
            this._scene = scene;
            this._timeLimit = timeLimit;
        }

        /// <summary>
        /// Creates and optimizes the model
        /// </summary>
        /// <returns>4-dimensional boolean array storing the optimal solution</returns>
        /// <exception cref="InfeasibleException">Thrown if model is not feasible</exception>
        public (int, bool[][][][], string) Optimize(bool noreturn = false)
        {
            using var model = new SATInterface.Model(new Configuration()
            {
                OptimizationFocus = OptimizationFocus.Incumbent,
                RandomSeed = new Random().Next(0, 2000000000),
                TimeLimit = TimeSpan.FromSeconds(_timeLimit),
                Verbosity = 2
            });

            CreateVariables(model);
            CreateConstraints(model);
            CreateObjective(model);
            model.Solve();

            if (noreturn)
            {
                if (model.State == State.Unsatisfiable)
                {
                    throw new InfeasibleException();
                }
                if (model.State == State.Undecided)
                {
                    throw new TimeoutException();
                }
                if (model.State == State.Satisfiable)
                {
                    return (_vObj.X, null, "");
                }
            }

            bool[][][][] x = new bool[_scene.People.Count][][][];
            if (model.State == State.Satisfiable)
            {
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
                                                _vars[personId][timeSlotId][taskId][locationId].X;
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
                return (_vObj.X, x, "");
            }
            else
            {
                throw new InfeasibleException();
            }
        }

        /// <summary>
        /// Creates all variables needed
        /// </summary>
        /// <param name="model"></param>
        private void CreateVariables(SATInterface.Model model)
        {
            _vars = new BoolExpr[_scene.People.Count][][][];
            for (var personId = 0; personId < _scene.People.Count; personId++)
            {
                BoolExpr[][][] vars2 = new BoolExpr[_scene.TimeSlots.Count][][];
                for (var timeSlotId = 0; timeSlotId < _scene.TimeSlots.Count; timeSlotId++)
                {
                    // Check whether the person is available at the time slot
                    if (!_scene.People[personId].Absences.Contains(timeSlotId))
                    {
                        BoolExpr[][] vars3 = new BoolExpr[_scene.Tasks.Count][];
                        for (var taskId = 0; taskId < _scene.Tasks.Count; taskId++)
                        {
                            // Check whether the person has the required skills for the task or is specifically required
                            if (_scene.Tasks[taskId].Skills.Length == 0 || _scene.People[personId].Skills.Any(_scene.Tasks[taskId].Skills.Contains) || _scene.Tasks[taskId].ReqSpecPpl.Contains(_scene.People[personId].Id))
                            {
                                BoolExpr[] vars4 = new BoolExpr[_scene.Locations.Count];
                                for (var locationId = 0; locationId < _scene.Locations.Count; locationId++)
                                {
                                    // Check whether the location provides the required skills for the task or is specifically required
                                    if (_scene.Tasks[taskId].Skills.Length == 0 || _scene.Locations[locationId].Skills.Length == 0 || _scene.Locations[locationId].Skills
                                            .Any(_scene.Tasks[taskId].Skills.Contains) || _scene.Tasks[taskId].ReqSpecLoc.Contains(_scene.Locations[locationId].Id))
                                    {
                                        vars4[locationId] = model.AddVar();
                                    }
                                    else
                                    {
                                        vars4[locationId] = null;
                                    }
                                }
                                vars3[taskId] = vars4;
                            }
                            else
                            {
                                vars3[taskId] = new BoolExpr[] { };
                            }
                        }
                        vars2[timeSlotId] = vars3;
                    }
                    else
                    {
                        vars2[timeSlotId] = new BoolExpr[][] { new BoolExpr[] { } };
                    }
                }
                _vars[personId] = vars2;
            }
        }

        /// <summary>
        /// Creates all constraints needed
        /// </summary>
        /// <param name="model"></param>
        private void CreateConstraints(SATInterface.Model model)
        {
            // Each Task recurs n times
            // Each Task occurs at specific time slots if required
            for (var taskId = 0; taskId < _scene.Tasks.Count; taskId++)
            {
                var nrOfUSedTimeSlots = new LinExpr();
                for (var timeSlotId = 0; timeSlotId < _scene.TimeSlots.Count; timeSlotId++)
                {
                    var isTimeSlotUsed = new List<BoolExpr>();
                    for (var personId = 0; personId < _scene.People.Count; personId++)
                    {
                        // Check whether the person is available at the time slot
                        if (_vars[personId][timeSlotId].Length == 1 && _vars[personId][timeSlotId][0].Length == 0) continue;
                        // Check whether the person has the required skills for the task
                        if (_vars[personId][timeSlotId][taskId].Length == 0) continue;
                        for (var locationId = 0; locationId < _scene.Locations.Count; locationId++)
                        {
                            // Check whether the location provides the required skills for the task
                            if (_vars[personId][timeSlotId][taskId][locationId] is null) continue;
                            var v = _vars[personId][timeSlotId][taskId][locationId];
                            isTimeSlotUsed.Add(v);
                        }
                    }
                    nrOfUSedTimeSlots.AddTerm(model.Or(isTimeSlotUsed));

                    // Check whether the task specifically requires the time slot
                    if (_scene.HasReqTS)
                    {
                        if (_scene.Tasks[taskId].ReqSpecTS
                        .Contains(_scene.TimeSlots[timeSlotId].Id)) model.AddConstr(model.Sum(isTimeSlotUsed) == 1);
                    }

                }
                model.AddConstr(nrOfUSedTimeSlots == _scene.Tasks[taskId].Reps);
            }

            if (_scene.HasReqNrOfPpl)
            {
                for (int taskId = 0; taskId < _scene.Tasks.Count; taskId++)
                {

                    for (int timeSlotId = 0; timeSlotId < _scene.TimeSlots.Count; timeSlotId++)
                    {
                        for (int locationId = 0; locationId < _scene.Locations.Count; locationId++)
                        {
                            LinExpr nrOfPeople = new LinExpr();
                            var possiblePeople = new List<BoolExpr>();
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
                                nrOfPeople.AddTerm(v);
                            }
                            model.AddConstr(model.Or(model.And(model.Or(possiblePeople), nrOfPeople >= _scene.Tasks[taskId].ReqPpl), nrOfPeople == 0));
                        }
                    }
                }
            }

            if (_scene.HasReqPeople)
            {
                // Each Task employes specific people if required
                for (int taskId = 0; taskId < _scene.Tasks.Count; taskId++)
                {
                    for (int personId = 0; personId < _scene.People.Count; personId++)
                    {
                        LinExpr nrOfEmployments = new LinExpr();
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
                                nrOfEmployments.AddTerm(v);
                            }
                        }
                        // Check whether the task specifically requires the person
                        if (_scene.Tasks[taskId].ReqSpecPpl.Contains(_scene.People[personId].Id))
                            model.AddConstr(nrOfEmployments >= 1);
                    }
                }
            }

            if (_scene.HasReqLoc)
            {
                // Each Task has a specific location if required
                for (int taskId = 0; taskId < _scene.Tasks.Count; taskId++)
                {
                    for (int locationId = 0; locationId < _scene.Locations.Count; locationId++)
                    {
                        LinExpr nrOfLocations = new LinExpr();
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
                                nrOfLocations.AddTerm(v);
                            }
                        }
                        // Check whether the task specifically requires the location
                        if (_scene.Tasks[taskId].ReqSpecLoc.Contains(_scene.Locations[locationId].Id))
                            model.AddConstr(nrOfLocations == _scene.Tasks[taskId].Reps);
                    }
                }
            }

            if (_scene.HasCapacity)
            {
                // Each person works at most n tasks per time slot.
                for (var personId = 0; personId < _scene.People.Count; personId++)
                {
                    for (var timeSlotId = 0; timeSlotId < _scene.TimeSlots.Count; timeSlotId++)
                    {
                        // Check whether the person is available at the time slot
                        if (_vars[personId][timeSlotId].Length == 1 && _vars[personId][timeSlotId][0].Length == 0) continue;
                        List<BoolExpr> nrOfWorkedTasks = new List<BoolExpr>();
                        for (var taskId = 0; taskId < _scene.Tasks.Count; taskId++)
                        {
                            // Check whether the person has the required skills for the task
                            if (_vars[personId][timeSlotId][taskId].Length == 0) continue;
                            for (var locationId = 0; locationId < _scene.Locations.Count; locationId++)
                            {
                                // Check whether the location provides the required skills for the task
                                if (_vars[personId][timeSlotId][taskId][locationId] is null) continue;
                                nrOfWorkedTasks.Add(_vars[personId][timeSlotId][taskId][locationId]);
                            }
                        }
                        model.AddConstr(model.Sum(nrOfWorkedTasks) <= _scene.People[personId].Capacity);
                    }
                }
            }


            // Each person works at most in one location per time slot.
            // "You can't change room fast enough"
            for (var personId = 0; personId < _scene.People.Count; personId++)
            {
                if (_scene.People[personId].Capacity > 1) continue;
                for (var timeSlotId = 0; timeSlotId < _scene.TimeSlots.Count; timeSlotId++)
                {
                    LinExpr nrOfLocations = new LinExpr();

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
                            nrOfLocations.AddTerm(_vars[personId][timeSlotId][taskId][locationId]);
                        }
                    }
                    model.AddConstr(nrOfLocations <= 1);
                }
            }
        }

        /// <summary>
        /// Creates the objective function
        /// </summary>
        /// <param name="model"></param>
        private void CreateObjective(SATInterface.Model model)
        {

            _vObj = new LinExpr();

            // Wage
            // Workload
            var workloadDeviations = new List<UIntVar>();
            for (var personId = 0; personId < _scene.People.Count; personId++)
            {
                LinExpr usedTimeSlots = new LinExpr();
                List<BoolExpr> obj = new List<BoolExpr>();
                for (var timeSlotId = 0; timeSlotId < _scene.TimeSlots.Count; timeSlotId++)
                {
                    var possibleAssignments = new List<BoolExpr>();
                    // Check whether the person is available at the time slot
                    if (_vars[personId][timeSlotId].Length == 1 && _vars[personId][timeSlotId][0].Length == 0) continue;
                    for (var taskId = 0; taskId < _scene.Tasks.Count; taskId++)
                    {
                        // Check whether the person has the required skills for the task
                        if (_vars[personId][timeSlotId][taskId].Length == 0) continue;
                        for (var locationId = 0; locationId < _scene.Locations.Count; locationId++)
                        {
                            // Check whether the location provides the required skills for the task
                            if (_vars[personId][timeSlotId][taskId][locationId] is null) continue;
                            var v = _vars[personId][timeSlotId][taskId][locationId];
                            if (_scene.WageWeight != 0) obj.Add(v);
                            if (_scene.TravelCostWeight != 0) _vObj.AddTerm(v, _scene.TravelCost[personId, locationId] * _scene.TravelCostWeight);
                            possibleAssignments.Add(v);
                        }
                    }
                    usedTimeSlots.AddTerm(model.Or(possibleAssignments));
                }
                _vObj.AddTerm(model.Sum(obj), _scene.People[personId].Wage * _scene.WageWeight);
                if (_scene.FairnessWeight != 0)
                {
                    // Get difference to the workload for each person
                    var operand = 100 / (_scene.TimeSlots.Count - _scene.People[personId].Absences.Length) * usedTimeSlots;
                    // Temporary decision variables
                    var x = model.AddUIntVar(1);
                    model.AddConstr(x == 1 | x == -1);
                    var y = model.AddUIntVar(100);
                    model.AddConstr(y == _scene.People[personId].Workload - operand);
                    model.AddConstr(x * y >= 0);
                    var z = model.AddUIntVar(100);
                    model.AddConstr(z == x * y);
                    workloadDeviations.Add(z);
                }
            }
            if (_scene.FairnessWeight != 0)
            {
                // Weight the difference
                // Temporary decision variables
                var max = model.AddUIntVar(100);
                var min = model.AddUIntVar(100);
                for (int i = 0; i < workloadDeviations.Count; i++)
                {
                    model.AddConstr(max >= workloadDeviations[i]);
                    model.AddConstr(min <= workloadDeviations[i]);
                }
                _vObj.AddTerm(_scene.FairnessWeight * (max - min));
            }

            if (_scene.PrefNrOfPeopleWeight != 0)
            {
                for (int taskId = 0; taskId < _scene.Tasks.Count; taskId++)
                {
                    LinExpr usedPeople = new LinExpr();
                    for (int personId = 0; personId < _scene.People.Count; personId++)
                    {
                        var possiblePeople = new List<BoolExpr>();
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
                        usedPeople.AddTerm(model.Or(possiblePeople));
                    }

                    var operand = usedPeople - _scene.Tasks[taskId].PrefPpl;
                    // Temporary decision variables
                    var x = model.AddUIntVar(1);
                    model.AddConstr(x == 1 | x == -1);
                    var y = model.AddUIntVar(_scene.People.Count);
                    model.AddConstr(y == operand);
                    model.AddConstr(x * y >= 0);
                    _vObj.AddTerm(x * y, _scene.PrefNrOfPeopleWeight);
                }
            }

            if (_scene.minLocWeight != 0)
            {
                // The least amount of locations are required
                for (var locationId = 0; locationId < _scene.Locations.Count; locationId++)
                {
                    List<BoolExpr> obj = new List<BoolExpr>();
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
                                obj.Add(_vars[personId][timeSlotId][taskId][locationId]);
                            }
                        }
                    }
                    _vObj.AddTerm(model.Or(obj), _scene.minLocWeight);
                }
            }

            if (_scene.minTSWeight != 0)
            {
                // The least amount of time slots are required
                for (var timeSlotId = 0; timeSlotId < _scene.TimeSlots.Count; timeSlotId++)
                {
                    List<BoolExpr> obj = new List<BoolExpr>();
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
                                obj.Add(_vars[personId][timeSlotId][taskId][locationId]);
                            }
                        }
                    }
                    _vObj.AddTerm(model.Or(obj), _scene.minTSWeight);
                }
            }
            model.Minimize(_vObj, () =>
            {

            });
        }
    }
}


