using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using NDtw;
using NDtw.Visualization.Wpf;

namespace SignatureVerification
{
    public class _yDistributionVelocity : SignatureComparer
    {
        private bool _boundaryConstrainStart = true;
        private bool _boundaryConstraintEnd = true;
        private DistanceMeasure _distanceMeasure = DistanceMeasure.Euclidean;
        private Dtw _dtw;

        public _yDistributionVelocity()
            : base(new Guid("6D8293DB-5D80-4B5F-AAD3-A25006DD1561"))
        {
        }

        public override double Compare(Stroke signature, Stroke patern, double weight)
        {
            double[] paternX = SupportFunctions.distance(patern, (a, b) => a.Y - b.Y).ToArray();
            double[] signatureX = SupportFunctions.distance(signature, (a, b) => a.Y - b.Y).ToArray();
            _dtw = new Dtw(paternX, signatureX, _distanceMeasure, _boundaryConstrainStart, _boundaryConstraintEnd);

            return weight * _dtw.GetCost() * Support.normLength(_dtw);
        }

        public override UIElement DrawnGui(Stroke signature, Stroke patern, double weight)
        {
            var result = new Border();

            var stack = new StackPanel();


            var textBlock = new TextBlock();
            textBlock.Text = "Y distribution of velocity";
            stack.Children.Add(textBlock);


            double threshold = Compare(signature, patern, 0);


            UIElement plot = DtwByVariablePlot();


            Border borderMatrix = DtwByMatrixPlot();

            var stackH = new StackPanel();
            stackH.Orientation = Orientation.Horizontal;
            stackH.Children.Add(plot);
            stackH.Children.Add(borderMatrix);
            stack.Children.Add(stackH);

            return stack;
        }

        private Border DtwByMatrixPlot()
        {
            var plotMatrix = new DtwMatrixPlot();
            plotMatrix.Dtw = _dtw;
            plotMatrix.OnDataChanged();
            plotMatrix.Width = 500;
            plotMatrix.Height = 500;
            var borderMatrix = new Border();
            borderMatrix.Child = plotMatrix;
            return borderMatrix;
        }

        private UIElement DtwByVariablePlot()
        {
            var plot = new DtwByVariablePlot();
            plot.Dtw = _dtw;
            plot.OnDtwChanged();
            plot.Width = 500;
            plot.Height = 500;
            var border = new Border();
            border.Child = plot;
            return border;
        }

        public _yDistributionVelocity SetSettings(DistanceMeasure distanceMeasure = DistanceMeasure.Euclidean,
            bool boundaryConstrainStart = true, bool boundaryConstraintEnd = true)
        {
            _distanceMeasure = distanceMeasure;
            _boundaryConstrainStart = boundaryConstrainStart;
            _boundaryConstraintEnd = boundaryConstrainStart;
            return this;
        }

        public override double CalculateWeights(List<Stroke> patterns)
        {
            var partialThreshold = new List<double>();

            for (int i = 0; i < patterns.Count; i++)
            {
                for (int j = 0; j < patterns.Count; j++)
                {
                    if (i == j)
                        continue;

                    partialThreshold.Add(Compare(patterns[i], patterns[j], 1));
                }
            }


            double avrg = partialThreshold.Average();
            double min = partialThreshold.Min();
            double max = partialThreshold.Max();
            return (max + min) / avrg;
        }

        public override string GetName()
        {
            return "Y distribution of velocity";
        }
    }
}