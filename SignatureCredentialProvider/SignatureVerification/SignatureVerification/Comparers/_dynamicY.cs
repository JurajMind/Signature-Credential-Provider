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
    /// <summary>
    ///     This is signature comparer that use speed in Y axle as signature feature to compare
    /// </summary>
    public class _dynamicY : SignatureComparer
    {
        private bool _boundaryConstrainStart = true;
        private bool _boundaryConstraintEnd = true;
        private DistanceMeasure _distanceMeasure = DistanceMeasure.Euclidean;
        private Dtw _dtw;

        public _dynamicY() : base(new Guid("b930487e-a23a-46e6-ac0b-2d1886f9bed9"))
        {
        }

        /// <summary>
        ///     This function calculate similarity by given signature
        /// </summary>
        /// <param name="signature">Signature to wich similarity will be calculated</param>
        /// <param name="patern"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public override double Compare(Stroke signature, Stroke patern, double weight)
        {
            var paternX = GetFeatureSeries(signature);
            var signatureX = GetFeatureSeries(patern);
            _dtw = new Dtw(paternX, signatureX, _distanceMeasure, _boundaryConstrainStart, _boundaryConstraintEnd);

            return weight*_dtw.GetCost()*Support.normLength(_dtw);
        }

        /// <summary>
        ///     Function that take signature similarty that use this comparer : speed in Y axle
        /// </summary>
        /// <param name="stroke"></param>
        /// <returns></returns>
        private double[] GetFeatureSeries(Stroke stroke)
        {
            return SupportFunctions.distance(stroke, (a, b) => a.Y - b.Y).ToArray();
        }

        /// <summary>
        ///     This funciton return UI element of signature vizialization by this comparer
        ///     It return DTW vizualization alons vizualization of signate feature
        /// </summary>
        /// <param name="signature">Give signateu, for witch vizualization will be provided</param>
        /// <param name="patern">Paterns signatures</param>
        /// <param name="weight">Comparer weight</param>
        /// <returns></returns>
        public override UIElement DrawnGui(Stroke signature, Stroke patern, double weight)
        {
            var result = new Border();

            var stack = new StackPanel();


            var textBlock = new TextBlock();
            textBlock.Text = "Dynamic Y";
            stack.Children.Add(textBlock);


            var treshold = Compare(signature, patern, 0);


            var plot = DtwByVariablePlot();


            var borderMatrix = DtwByMatrixPlot();

            var StrokVisualization = DrawnStroke(signature);

            var stackH = new StackPanel();
            stackH.Orientation = Orientation.Horizontal;
            stackH.Children.Add(plot);
            stackH.Children.Add(borderMatrix);
            stackH.Children.Add(StrokVisualization);

            stack.Children.Add(stackH);

            return stack;
        }

        /// <summary>
        ///     This function return DTW matrix plot of comparer
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        ///     This function return
        /// </summary>
        /// This function return DTW variable plot
        /// <returns></returns>
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

        /// <summary>
        ///     This function return vizualization of comparer signature feature
        /// </summary>
        /// <param name="stroke">Stroke, for witch vizualizaton of feature will be made</param>
        /// <returns></returns>
        private Border DrawnStroke(Stroke stroke)
        {
            var InkCanvas = new InkCanvas();
            var inkBorder = new Border();
            inkBorder.BorderThickness = new Thickness(1);
            inkBorder.BorderBrush = Brushes.Black;
            InkCanvas.EditingMode = InkCanvasEditingMode.None;
            InkCanvas.Strokes =
                new StrokeCollection(new List<Stroke>
                {
                    new CustomStroke(stroke.StylusPoints, SupportFunctions.Normalize(GetFeatureSeries(stroke)))
                });
            inkBorder.Child = InkCanvas;

            return inkBorder;
        }

        public _dynamicY SetSettings(DistanceMeasure distanceMeasure = DistanceMeasure.Euclidean,
            bool boundaryConstrainStart = true, bool boundaryConstraintEnd = true)
        {
            _distanceMeasure = distanceMeasure;
            _boundaryConstrainStart = boundaryConstrainStart;
            _boundaryConstraintEnd = boundaryConstrainStart;
            return this;
        }

        /// <summary>
        ///     Function, that calculate weight of this signature comparer
        /// </summary>
        /// <param name="patterns"></param>
        /// <returns></returns>
        public override double CalculateWeights(List<Stroke> patterns)
        {
            var partialTreshold = new List<double>();

            for (var i = 0; i < patterns.Count; i++)
            {
                for (var j = 0; j < patterns.Count; j++)
                {
                    if (i == j)
                        continue;

                    partialTreshold.Add(Compare(patterns[i], patterns[j], 1));
                }
            }


            var featureValues = patterns.Select(p => GetFeatureSeries(p));

            var featureValueRange = featureValues.Max(p => p.Max()) - featureValues.Min(p => p.Min());

            var avrg = partialTreshold.Average();
            var min = partialTreshold.Min();
            var max = partialTreshold.Max();
            return (2*avrg/(max + min))*featureValueRange;
        }

        public override string GetName()
        {
            return "Dynamic Y";
        }
    }
}