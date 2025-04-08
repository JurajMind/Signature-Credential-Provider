using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using NDtw;
using NDtw.Visualization.Wpf;

namespace SignatureVerification.Comparers
{
    public class DtwComparer : SignatureComparer
    {
        private readonly string _guiName;
        private bool _boundaryConstrainStart = true;
        private bool _boundaryConstraintEnd = true;
        private DistanceMeasure _distanceMeasure = DistanceMeasure.Euclidean;
        private Dtw _dtw;

        public DtwComparer(string Name)
        {
            _guiName = Name;
        }

        public Dtw Dtw
        {
            get { return _dtw; }
            private set { _dtw = value; }
        }

        public new virtual double Compare(Stroke signature, Stroke patern, double weight)
        {
            double[] paternX = SupportFunctions.distance(patern, (a, b) => a.X - b.X).ToArray();
            double[] signatureX = SupportFunctions.distance(signature, (a, b) => a.X - b.X).ToArray();
            Dtw = new Dtw(paternX, signatureX, _distanceMeasure, _boundaryConstrainStart, _boundaryConstraintEnd);

            return weight*_dtw.GetCost()*Support.normLength(_dtw);
        }

        public void SetSettings(DistanceMeasure distanceMeasure = DistanceMeasure.Euclidean,
            bool boundaryConstrainStart = true, bool boundaryConstraintEnd = true)
        {
            _distanceMeasure = distanceMeasure;
            _boundaryConstrainStart = boundaryConstrainStart;
            _boundaryConstraintEnd = boundaryConstrainStart;
        }

        public override UIElement drawnGui(Stroke signature, Stroke patern, double weight)
        {
            var result = new Border();

            var stack = new StackPanel();


            var textBlock = new TextBlock();
            textBlock.Text = _guiName;
            stack.Children.Add(textBlock);


            double treshold = Compare(signature, patern, 0);


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

        public override double CalculateWeights(List<Stroke> patterns)
        {
            var partialTreshold = new List<double>();

            for (int i = 0; i < patterns.Count; i++)
            {
                for (int j = 0; j < patterns.Count; j++)
                {
                    if (i == j)
                        continue;

                    partialTreshold.Add(Compare(patterns[i], patterns[j], 1));
                }
            }


            double avrg = partialTreshold.Average();
            return 1;
        }

        public override string getName()
        {
            return _guiName;
        }
    }
}