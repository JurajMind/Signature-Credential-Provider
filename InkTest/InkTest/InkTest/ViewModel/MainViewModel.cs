using System.Collections.Generic;
using OxyPlot;

namespace InkTest.ViewModel
{
    public class MainViewModel
    {
        public MainViewModel(List<double> data)
        {
            Title = data.Count.ToString();
            Points = new List<DataPoint>
            {
                new DataPoint(10, 10),
                new DataPoint(10, 20)
            };
            int index = 0;
            foreach (double d in data)
            {
                Points.Add(new DataPoint(index, 10));
                index = index + 1;
            }
        }

        public string Title { get; private set; }

        public IList<DataPoint> Points { get; private set; }
    }
}