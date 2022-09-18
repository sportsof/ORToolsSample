using Google.OrTools.LinearSolver;
using OperationsResearch;
using ORToolsSampleWinForms.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Windows.Forms;

namespace ORToolsSampleWinForms
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void buttonCalc_Click(object sender, EventArgs e)
        {
            var s = new Stopwatch();
            s.Start();

            for (int i = 0; i < 1; i++)
            {
                SolveProblem();
            }
            s.Stop();

            MessageBox.Show("Elapsed millisecond: " + s.ElapsedMilliseconds);
        }

        private void SolveProblem()
        {
            var rawData = new List<SolverVar>();

            // Исходные данные задачи
            rawData.Add(new SolverVar { xId = 1, Koef = 2, Min = 0, Max = 10, ConstraintKoefs = new double[] { 1, 2 } });
            rawData.Add(new SolverVar { xId = 2, Koef = -3, Min = 0, Max = 100, ConstraintKoefs = new double[] { 2, 10 } });
            rawData.Add(new SolverVar { xId = 3, Koef = 4, Min = 0, Max = 20, ConstraintKoefs = new double[] { 3, -5 } });

            var rawConstraints = new List<SolverConstraint>()
            {
                new SolverConstraint { Name = "Constraint 1", Lb = double.NegativeInfinity, Ub = 900 },
                new SolverConstraint { Name = "Constraint 2", Lb = double.NegativeInfinity, Ub = 950 }
            };

            // Create linear solver
            Solver solver = Solver.CreateSolver("GLOP");

            if (solver is null)
            {
                return;
            }

            // Create Variables. Set Min & Max value
            var vars = new List<Variable>();

            for (int i = 0; i < rawData.Count; ++i)
            {
                vars.Add(solver.MakeNumVar(rawData[i].Min, rawData[i].Max, rawData[i].xId.ToString()));
            }

            // Add Constraints
            //  lowerBound ≤ Sum(a(i) x(i)) ≤ upperBound
            var constraints = new List<Constraint>();

            for (int i = 0; i < rawConstraints.Count; ++i)
            {
                var constraint = solver.MakeConstraint(rawConstraints[i].Lb, rawConstraints[i].Ub, rawConstraints[i].Name);

                for (int j = 0; j < vars.Count; ++j)
                {
                    constraint.SetCoefficient(vars[j], rawData[j].ConstraintKoefs[i]);
                }

                constraints.Add(constraint);
            }

            var objective = solver.Objective();

            for (int i = 0; i < vars.Count; ++i)
            {
                objective.SetCoefficient(vars[i], rawData[i].Koef);
            }

            objective.SetMinimization();

            Solver.ResultStatus resultStatus = solver.Solve();

            // Check that the problem has an optimal solution.
            if (resultStatus != Solver.ResultStatus.OPTIMAL)
            {
                toolStripStatusLabel.Text = "Optimal solution not found. ";

                if (resultStatus == Solver.ResultStatus.FEASIBLE)
                {
                    toolStripStatusLabel.Text += "Suboptimal solution was found.";
                }
                else
                {
                    toolStripStatusLabel.Text += "Could not solve the problem.";
                    return;
                }
            }

            var reportStr = $"Problem solved in {solver.Iterations()} iterations for {solver.WallTime()} milliseconds.\n" +
                $"Objective function value: {objective.Value()}\n\n";

            for(int i = 0; i < rawData.Count; i++)
            {
                reportStr += $"Optimal value of var {rawData[i].xId} - {vars[i].SolutionValue()}\n";
            }

            MessageBox.Show(reportStr);
        }
    }
}