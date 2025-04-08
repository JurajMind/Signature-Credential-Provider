using System;
using System.Windows;
using System.Windows.Controls;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace NDtw.Visualization.Wpf
{
    public partial class DtwByVariablePlot : UserControl
    {
        public static DependencyProperty DtwProperty =
            DependencyProperty.Register(
                "Dtw",
                typeof (IDtw),
                typeof (DtwByVariablePlot),
                new FrameworkPropertyMetadata(null, (d, e) => ((DtwByVariablePlot) d).OnDtwChanged()));

        public DtwByVariablePlot()
        {
            InitializeComponent();
        }

        public IDtw Dtw
        {
            get { return (IDtw) GetValue(DtwProperty); }
            set { SetValue(DtwProperty, value); }
        }

        public void OnDtwChanged()
        {
            Tuple<int, int>[] dtwPath = Dtw.GetPath();
            int xLength = Dtw.XLength;
            int yLength = Dtw.YLength;
            double cost = Dtw.GetCost();
            double costNormalized = Dtw.GetCost()/Math.Sqrt(xLength*xLength + yLength*yLength);

            var plotModel =
                new PlotModel(String.Format("Dtw norm by length: {0:0.00}, total: {1:0.00}", costNormalized, cost));

            for (int variableIndex = 0; variableIndex < Dtw.SeriesVariables.Length; variableIndex++)
            {
                SeriesVariable variableA = Dtw.SeriesVariables[variableIndex];
                double[] variableASeries = variableA.GetPreprocessedXSeries();
                SeriesVariable variableB = Dtw.SeriesVariables[variableIndex];
                double[] variableBSeries = variableB.GetPreprocessedYSeries();

                string axisTitleAndKey = String.Format("Value ({0})", variableA.VariableName);
                plotModel.Axes.Add(new LinearAxis(AxisPosition.Left, axisTitleAndKey)
                {
                    Key = axisTitleAndKey,
                    PositionTier = variableIndex
                });

                var plotSeriesA = new LineSeries(variableA.VariableName) {YAxisKey = axisTitleAndKey};
                for (int i = 0; i < xLength; i++)
                    plotSeriesA.Points.Add(new DataPoint(i, variableASeries[i]));

                var plotSeriesB = new LineSeries(variableB.VariableName) {YAxisKey = axisTitleAndKey};
                for (int i = 0; i < yLength; i++)
                    plotSeriesB.Points.Add(new DataPoint(i, variableBSeries[i]));

                var plotSeriesPath = new LineSeries("Dtw")
                {
                    YAxisKey = axisTitleAndKey,
                    StrokeThickness = 0.5,
                    Color = OxyColors.DimGray,
                };

                for (int i = 0; i < dtwPath.Length; i++)
                {
                    plotSeriesPath.Points.Add(new DataPoint(dtwPath[i].Item1, variableASeries[dtwPath[i].Item1]));
                    plotSeriesPath.Points.Add(new DataPoint(dtwPath[i].Item2, variableBSeries[dtwPath[i].Item2]));
                    plotSeriesPath.Points.Add(new DataPoint(double.NaN, double.NaN));
                }

                plotModel.Series.Add(plotSeriesA);
                plotModel.Series.Add(plotSeriesB);
                plotModel.Series.Add(plotSeriesPath);
            }

            plotModel.Axes.Add(new LinearAxis(AxisPosition.Bottom, "Index"));

            Plot.Model = plotModel;
        }
    }
}