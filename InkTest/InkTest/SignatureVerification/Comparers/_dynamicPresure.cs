using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Media;
using NDtw;
using NDtw.Visualization.Wpf;

namespace SignatureVerification
{
    public class _dynamicPresure : SignatureComparer
    {
        private bool _boundaryConstrainStart = true;
        private bool _boundaryConstraintEnd = true;
        private DistanceMeasure _distanceMeasure = DistanceMeasure.Euclidean;
        private Dtw _dtw;

        public _dynamicPresure() : base(new Guid("588da3ab-840a-4ff2-a2df-f6047d1d6912"))
        {
        }

        public override double Compare(Stroke signature, Stroke patern, double weight)
        {
            double[] paternX = GetFeatureSeries(patern);
            double[] signatureX = GetFeatureSeries(signature);
            _dtw = new Dtw(paternX, signatureX, _distanceMeasure, _boundaryConstrainStart, _boundaryConstraintEnd);

            return weight*_dtw.GetCost()*Support.normLength(_dtw);
        }

        private double[] GetFeatureSeries(Stroke stroke)
        {
            return stroke.StylusPoints.Select(p => (double)p.PressureFactor).ToArray();
        }

        public override UIElement DrawnGui(Stroke signature, Stroke patern, double weight)
        {
            var result = new Border();

            var stack = new StackPanel();


            var textBlock = new TextBlock();
            textBlock.Text = "Dynamic presure ";
            stack.Children.Add(textBlock);

            double treshold = Compare(signature, patern, 0);

            var tresholdText = new TextBlock();
            tresholdText.Text = String.Format("Weight: {0} : Cost: {1} : Whole : {2}", weight, _dtw.GetCost(),
                _dtw.GetCost()*weight);
            stack.Children.Add(tresholdText);


            UIElement plot = DtwByVariablePlot();


            Border borderMatrix = DtwByMatrixPlot();

            var StrokVisualization = DrawnStroke(signature);
        
            var stackH = new StackPanel();
            stackH.Orientation = Orientation.Horizontal;
            stackH.Children.Add(plot);
            stackH.Children.Add(borderMatrix);
            stackH.Children.Add(StrokVisualization);
            stack.Children.Add(stackH);

            return stack;
        }

        private Border DrawnStroke(Stroke stroke)
        {
            var InkCanvas = new InkCanvas();
            var inkBorder = new Border();
            inkBorder.BorderThickness = new Thickness(1);
            inkBorder.BorderBrush = Brushes.Black;
            InkCanvas.EditingMode = InkCanvasEditingMode.None;
            InkCanvas.Strokes = new StrokeCollection(new List<Stroke>() { new CustomStroke(stroke.StylusPoints, (GetFeatureSeries(stroke))) });
            inkBorder.Child = InkCanvas;

            return inkBorder;

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

        public _dynamicPresure SetSettings(DistanceMeasure distanceMeasure = DistanceMeasure.Euclidean,
            bool boundaryConstrainStart = true, bool boundaryConstraintEnd = false)
        {
            _distanceMeasure = distanceMeasure;
            _boundaryConstrainStart = boundaryConstrainStart;
            _boundaryConstraintEnd = boundaryConstrainStart;

            return this;
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


            var featureValues = patterns.Select(p => GetFeatureSeries(p));

            var featureValueRange = featureValues.Max(p => p.Max()) - featureValues.Min(p => p.Min());

            var featureDistinc =
                featureValues.SelectMany(f => f.Select(ff => Decimal.Round((decimal)ff, 2, MidpointRounding.AwayFromZero)))
                    .Distinct()
                    .ToList();

            double avrg = partialTreshold.Average();
            double min = partialTreshold.Min();
            double max = partialTreshold.Max();
            return (2 * avrg / (max + min)) * featureValueRange;
        }

        public override string GetName()
        {
            return "Dynamic presure";
        }
    }
}