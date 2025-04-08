using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using InkTest.ViewModel;
using NDtw;
using NDtw.Preprocessing;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;

namespace InkTest
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private readonly Stroke _wholeStroke;
        private Dtw _dtwX;
        private Dtw _dtwY;
        private int _sakoeChibaMaxShift = 50;
        private DistanceMeasure? _selectedDistanceMeasure;
        private int _slopeConstraintAside = 1;
        private int _slopeConstraintDiagonal = 1;
        private bool _useBoundaryConstraintEnd = true;
        private bool _useBoundaryConstraintStart = true;
        private bool _useSakoeChibaMaxShift = true;
        private bool _useSlopeConstraint = true;
        private double[] dynamicXOrigin;
        private double[] dynamicYOrigin;
        private bool firstStroke = true;
        private double[] lastStrokePresure;
        private double[] lastStrokeX;
        private double[] lastStrokeY;
        private double[] originPresure;
        private MainViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();
            var pointTime = new List<double>();
            InkCanvas.addTime(pointTime);
            DataContext = this;
            InitializeData();
        }

        public MainWindow(Stroke externalStroke)
        {
            InitializeComponent();
            DataContext = this;
            InitializeData();


            _wholeStroke = externalStroke;
            Calculate();
        }

        public MainWindow(Stroke firsStroke, Stroke secondStroke)
        {
            InitializeComponent();
            DataContext = this;
            InitializeData();


            _wholeStroke = firsStroke.Clone();
            Calculate();
            firstStroke = false;

            _wholeStroke = secondStroke.Clone();
            Calculate();
        }

        public ObservableCollection<IPreprocessor> Preprocessors { get; private set; }
        public ObservableCollection<DistanceMeasure> DistanceMeasures { get; private set; }

        public DistanceMeasure? SelectedDistanceMeasure
        {
            get { return _selectedDistanceMeasure; }
            set
            {
                _selectedDistanceMeasure = value;
                NotifyPropertyChanged(() => SelectedDistanceMeasure);
                Recalculate();
            }
        }

        public bool UseBoundaryConstraintStart
        {
            get { return _useBoundaryConstraintStart; }
            set
            {
                _useBoundaryConstraintStart = value;
                NotifyPropertyChanged(() => UseBoundaryConstraintStart);
                Recalculate();
            }
        }

        public bool UseBoundaryConstraintEnd
        {
            get { return _useBoundaryConstraintEnd; }
            set
            {
                _useBoundaryConstraintEnd = value;
                NotifyPropertyChanged(() => UseBoundaryConstraintEnd);
                Recalculate();
            }
        }

        public bool UseSakoeChibaMaxShift
        {
            get { return _useSakoeChibaMaxShift; }
            set
            {
                _useSakoeChibaMaxShift = value;
                NotifyPropertyChanged(() => UseSakoeChibaMaxShift);
                Recalculate();
            }
        }

        public int SakoeChibaMaxShift
        {
            get { return _sakoeChibaMaxShift; }
            set
            {
                _sakoeChibaMaxShift = value;
                NotifyPropertyChanged(() => SakoeChibaMaxShift);
            }
        }

        public bool UseSlopeConstraint
        {
            get { return _useSlopeConstraint; }
            set
            {
                _useSlopeConstraint = value;
                NotifyPropertyChanged(() => UseSlopeConstraint);
                Recalculate();
            }
        }

        public int SlopeConstraintDiagonal
        {
            get { return _slopeConstraintDiagonal; }
            set
            {
                _slopeConstraintDiagonal = value;
                NotifyPropertyChanged(() => SlopeConstraintDiagonal);
            }
        }

        public int SlopeConstraintAside
        {
            get { return _slopeConstraintAside; }
            set
            {
                _slopeConstraintAside = value;
                NotifyPropertyChanged(() => SlopeConstraintAside);
            }
        }

        public bool CanRecalculate
        {
            get { return lastStrokeX != null && lastStrokeX.Length != 0 && _selectedDistanceMeasure != null; }
        }

        //Dynamic tab
        public PlotModel Model { get; set; }
        public PlotModel Model2 { get; set; }
        public PlotModel Model3 { get; set; }

        public PlotModel ModelOrigin { get; set; }
        public PlotModel ModelOrigin2 { get; set; }
        public PlotModel ModelOrigin3 { get; set; }

        //Time Tab
        public PlotModel ModelT { get; set; }
        public PlotModel ModelT2 { get; set; }
        public PlotModel ModelT3 { get; set; }

        public PlotModel ModelTO { get; set; }
        public PlotModel ModelTO2 { get; set; }
        public PlotModel ModelTO3 { get; set; }

        public Dtw DtwX
        {
            get { return _dtwX; }
            private set
            {
                _dtwX = value;
                NotifyPropertyChanged(() => DtwX);
            }
        }

        public Dtw DtwY
        {
            get { return _dtwY; }
            private set
            {
                _dtwY = value;
                NotifyPropertyChanged(() => DtwY);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void InitializeData()
        {
            var nonePreprocessor = new NonePreprocessor();
            Preprocessors = new ObservableCollection<IPreprocessor>
            {
                nonePreprocessor,
                new CentralizationPreprocessor(),
                new NormalizationPreprocessor(),
                new StandardizationPreprocessor()
            };

            DistanceMeasures = new ObservableCollection<DistanceMeasure>
            {
                DistanceMeasure.Manhattan,
                DistanceMeasure.Euclidean,
                DistanceMeasure.SquaredEuclidean,
                DistanceMeasure.Maximum
            };

            SelectedDistanceMeasure = DistanceMeasure.Euclidean;
        }

        protected void NotifyPropertyChanged<T>(Expression<Func<T>> expression)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(StrongTypingHelper.GetProperty(expression).Name));
        }

        public void Recalculate()
        {
            if (!CanRecalculate)
                return;

            try
            {
                var dtwX = new Dtw(dynamicXOrigin, lastStrokeX, SelectedDistanceMeasure.Value,
                    UseBoundaryConstraintStart,
                    UseBoundaryConstraintEnd,
                    UseSlopeConstraint ? SlopeConstraintDiagonal : (int?) null,
                    UseSlopeConstraint ? SlopeConstraintAside : (int?) null,
                    UseSakoeChibaMaxShift ? SakoeChibaMaxShift : (int?) null);

                var dtwY = new Dtw(dynamicYOrigin, lastStrokeY.ToArray(), SelectedDistanceMeasure.Value,
                    UseBoundaryConstraintStart,
                    UseBoundaryConstraintEnd,
                    UseSlopeConstraint ? SlopeConstraintDiagonal : (int?) null,
                    UseSlopeConstraint ? SlopeConstraintAside : (int?) null,
                    UseSakoeChibaMaxShift ? SakoeChibaMaxShift : (int?) null);

                var dtwAX = new Dtw(acceleration(dynamicXOrigin.ToList()).ToArray(),
                    acceleration(lastStrokeX.ToList()).ToArray(), SelectedDistanceMeasure.Value,
                    UseBoundaryConstraintStart,
                    UseBoundaryConstraintEnd,
                    UseSlopeConstraint ? SlopeConstraintDiagonal : (int?) null,
                    UseSlopeConstraint ? SlopeConstraintAside : (int?) null,
                    UseSakoeChibaMaxShift ? SakoeChibaMaxShift : (int?) null);

                var dtwAY = new Dtw(acceleration(dynamicYOrigin.ToList()).ToArray(),
                    acceleration(lastStrokeY.ToList()).ToArray(), SelectedDistanceMeasure.Value,
                    UseBoundaryConstraintStart,
                    UseBoundaryConstraintEnd,
                    UseSlopeConstraint ? SlopeConstraintDiagonal : (int?) null,
                    UseSlopeConstraint ? SlopeConstraintAside : (int?) null,
                    UseSakoeChibaMaxShift ? SakoeChibaMaxShift : (int?) null);

                var presureDtw = new Dtw(lastStrokePresure, originPresure, SelectedDistanceMeasure.Value,
                    UseBoundaryConstraintStart,
                    UseBoundaryConstraintEnd,
                    UseSlopeConstraint ? SlopeConstraintDiagonal : (int?) null,
                    UseSlopeConstraint ? SlopeConstraintAside : (int?) null,
                    UseSakoeChibaMaxShift ? SakoeChibaMaxShift : (int?) null);

                DtwX = dtwX;
                DtwY = dtwY;

                double tresholdTest = (MyExtensions.normLength(dtwX) + MyExtensions.normLength(dtwY))*
                                      DynamiCSlider.Value +
                                      (MyExtensions.normLength(dtwAX) + MyExtensions.normLength(dtwAY))*
                                      AccelerationCSlider.Value +
                                      MyExtensions.normLength(presureDtw)*PresureCSlider.Value;

                CalulatedTreshold.Text = tresholdTest.ToString();

                if (tresholdTest <= TresholdCSlider.Value)
                {
                    TresholdNotify.Background = new SolidColorBrush(Colors.GreenYellow);
                }
                else
                    TresholdNotify.Background = new SolidColorBrush(Colors.DarkRed);


                dtw.Content = String.Format("Speed Total:{0}|Norm:{3}| X:{1} ;Y:{2}", dtwX.GetCost() + dtwY.GetCost(),
                    dtwX.GetCost(), dtwY.GetCost(), MyExtensions.normLength(dtwX) + MyExtensions.normLength(dtwY));
                dtwA.Content = String.Format("Dyn Total:{0}|Norm:{3}| X:{1} ;Y:{2}", dtwAX.GetCost() + dtwAY.GetCost(),
                    dtwAX.GetCost(), dtwAY.GetCost(), MyExtensions.normLength(dtwAX) + MyExtensions.normLength(dtwAY));
                dtwPresure.Content = String.Format("Presure: {0}| Norm:{1}", presureDtw.GetCost(),
                    MyExtensions.normLength(presureDtw));
            }
            catch (Exception)
            {
                dtw.Content = "Error";
                dtwA.Content = "Error";
                dtwPresure.Content = "Error";
            }
        }

        private double distance(StylusPoint a, StylusPoint b)
        {
            return Math.Sqrt(Math.Pow(b.X - a.X, 2) + Math.Pow(b.Y - a.Y, 2));
        }

        private double distanceX(StylusPoint a, StylusPoint b)
        {
            return Math.Abs(a.X - b.X);
        }

        private double distanceY(StylusPoint a, StylusPoint b)
        {
            return Math.Abs(a.Y - b.Y);
        }


        private List<double> distance(Stroke stroke)
        {
            var result = new List<double>();
            result.Add(0);
            for (int i = 0; i < stroke.StylusPoints.Count - 1; i++)
            {
                result.Add(distance(stroke.StylusPoints[i], stroke.StylusPoints[i + 1]));
            }

            return result;
        }

        private List<double> distanceX(Stroke stroke)
        {
            var result = new List<double>();
            result.Add(0);
            for (int i = 0; i < stroke.StylusPoints.Count - 1; i++)
            {
                result.Add(distanceX(stroke.StylusPoints[i], stroke.StylusPoints[i + 1]));
            }

            return result;
        }

        private List<double> distanceY(Stroke stroke)
        {
            var result = new List<double>();
            result.Add(0d);
            for (int i = 0; i < stroke.StylusPoints.Count - 1; i++)
            {
                result.Add(distanceY(stroke.StylusPoints[i], stroke.StylusPoints[i + 1]));
            }

            return result;
        }

        private List<double> acceleration(List<double> values)
        {
            var result = new List<double>();
            result.Add(0d);
            for (int i = 0; i < values.Count - 1; i++)
            {
                result.Add(Math.Abs(values[i] - values[i + 1]));
            }

            return result;
        }


        private List<double> timeDelta(List<double> times)
        {
            var result = new List<double>();
            result.Add(0);
            for (int i = 0; i < times.Count - 1; i++)
            {
                result.Add(times[i + 1] - times[i]);
            }

            return result.ToList();
        }

        private void Calculate()
        {
            Stroke wholeStroke;
            if (_wholeStroke == null)
                wholeStroke = InkCanvas.WholeStroke;
            else
            {
                wholeStroke = _wholeStroke;
            }
            if (wholeStroke == null)
                return;

            //var intervals = InkCanvas.getStrokeIntervalsntervals();
            // var test = intervals.GroupBy(i => i.Count);

            //var testTime = InkCanvas.getTime();
            //testTime = testTime.Select(l => l - testTime[0]).ToList();

            //var diff = timeDelta(testTime);
            //var diffGb = diff.GroupBy(l => l);

            var Data = new List<List<double>>();

            List<double> distanceX = this.distanceX(wholeStroke).ToList();

            List<double> presure = wholeStroke.StylusPoints.Select(p => (double) p.PressureFactor).ToList();

            List<double> distanceY = this.distanceY(wholeStroke);
            List<double> distance = this.distance(wholeStroke);


            var dataForPlot1 = new List<Tuple<List<double>, string>>
            {
                new Tuple<List<double>, string>(distanceX, "X"),
                new Tuple<List<double>, string>(distanceY, "Y"),
                //new Tuple<List<double>, string>(distance, "Distance"),
            };

            Model = CreatModel(dataForPlot1, "Dynamic", "Distance");


            var dataForPlot2 = new List<Tuple<List<double>, string>>
            {
                //new Tuple<List<double>, string>(test.Select( g => (double)g.Count()).ToList(), "Time"),
                //new Tuple<List<double>, string>(diff,"Diff")
                new Tuple<List<double>, string>(acceleration(distanceX), "a.X"),
                new Tuple<List<double>, string>(acceleration(distanceY), "a.Y"),
                //new Tuple<List<double>, string>(acceleration(distance), "a"),
            };

            Model2 = CreatModel(dataForPlot2, "A", "Acceleration");

            var dataForPlot3 = new List<Tuple<List<double>, string>>
            {
                new Tuple<List<double>, string>(presure, "Presure"),
                new Tuple<List<double>, string>(acceleration(presure), "Presure dynamic")
            };

            Model3 = CreatModel(dataForPlot3, "Presure", "Presure dynamic");

            Model.InvalidatePlot(true);


            //// TimeTab

            //var dataForPlotT = new List<Tuple<List<double>, string>>()
            //{
            //    //new Tuple<List<double>, string>(test.OrderBy(k => k.Key).Select(s => (double)s.Count()).ToList(),"Time from period")
            //};
            //this.ModelT = CreatModel(dataForPlotT, "Time", "Ordered");


            //var dataForPlotT2 = new List<Tuple<List<double>, string>>()
            //{
            //    //new Tuple<List<double>, string>(InkCanvas.getTime().ToList(),"Time from stroke")
            //};
            //this.ModelT2 = CreatModel(dataForPlotT2, "Time", "Stroke");


            //var dataForPlotT3 = new List<Tuple<List<double>, string>>()
            //{
            //    //new Tuple<List<double>, string>(diff.Where( a=> a!= 0).ToList(),"Diff time except zero")
            //};
            //this.ModelT3 = CreatModel(dataForPlotT3, "Time", "E! Zero");


            OnPropertyChanged("Model");
            OnPropertyChanged("Model2");
            OnPropertyChanged("Model3");

            OnPropertyChanged("ModelT");
            OnPropertyChanged("ModelT2");
            OnPropertyChanged("ModelT3");
            if (firstStroke)
            {
                // Origin stroke 
                ModelOrigin = CreatModel(dataForPlot1, "Dynamic", "Distance");
                ModelOrigin2 = CreatModel(dataForPlot2, "A", "Acceleration");
                ModelOrigin3 = CreatModel(dataForPlot3, "Presure", "Presure dynamic");
                dynamicXOrigin = distanceX.ToArray();
                dynamicYOrigin = distanceY.ToArray();

                originPresure = presure.ToArray();

                //this.ModelTO = CreatModel(dataForPlotT, "Time", "Ordered");
                //this.ModelTO2 = CreatModel(dataForPlotT2, "Time", "Stroke");
                //this.ModelTO3 = CreatModel(dataForPlotT3, "Time", "E! Zero");

                OnPropertyChanged("ModelOrigin");
                OnPropertyChanged("ModelOrigin2");
                OnPropertyChanged("ModelOrigin3");

                OnPropertyChanged("ModelTO");
                OnPropertyChanged("ModelTO2");
                OnPropertyChanged("ModelTO3");


                InkCanvasOrigin.Strokes.Add(wholeStroke);
                firstStroke = false;
            }

            lastStrokeX = distanceX.ToArray();
            lastStrokeY = distanceY.ToArray();

            lastStrokePresure = presure.ToArray();


            InkCanvas.Strokes.Clear();
            InkCanvas.Strokes.Add(wholeStroke);

            Recalculate();


            //PlotView.Model.InvalidatePlot(true);
        }

        private PlotModel CreatModel(List<Tuple<List<double>, string>> data, string Title, string Subtitle)
        {
            var plot = new PlotModel
            {
                Title = Title,
                Subtitle = Subtitle,
            };

            plot.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Minimum = data.Min(d => d.Item1.Min()),
                Maximum = data.Max(d => d.Item1.Max()),
                MajorStep = data.Max(d => d.Item1.Max())/10,
                MinorStep = data.Max(d => d.Item1.Max())/30,
                TickStyle = TickStyle.None,
                IsZoomEnabled = true,
            });
            plot.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0,
                Maximum = data.Max(d => d.Item1.Count),
                MajorStep = 1,
                MinorStep = 0.25,
                TickStyle = TickStyle.None
            });

            foreach (var tuple in data)
            {
                plot.Series.Add(createLineSeries(tuple.Item1, tuple.Item2));
            }

            return plot;
        }


        private static LineSeries createLineSeries(List<double> data, string Title)
        {
            var ls = new LineSeries
            {
                Title = Title
            };

            for (int i = 0; i < data.Count; i++)
            {
                ls.Points.Add(new DataPoint(i, data[i]));
            }

            return ls;
        }


        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void InkCanvas_StylusEnter(object sender, StylusEventArgs e)
        {
            InkCanvas.Strokes.Clear();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            InkCanvasOrigin.Strokes.Clear();
            firstStroke = true;
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            Calculate();
        }

        private void InkCanvas_OnStylusLeave(object sender, StylusEventArgs e)
        {
            Calculate();
        }

        private void TresholdCSlider_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            TresholdLabel.Content = "Treshold size:" + e.NewValue;
        }
    }
}