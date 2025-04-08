using System;
using System.Collections.Generic;

namespace InkTest
{
    internal class Data
    {
        private static List<List<double>> _data;

        public static void InsertData(List<List<double>> data)
        {
            _data = data;
        }

        public static List<Measurement> GetData()
        {
            var measurements = new List<Measurement>();

            DateTime startDate = DateTime.Now.AddMinutes(-10);
            var r = new Random();

            for (int i = 0; i < 5; i++)
            {
                for (int j = 0; j < 110; j++)
                {
                    measurements.Add(new Measurement
                    {
                        DetectorId = i,
                        DateTime = startDate.AddMinutes(j),
                        Value = r.Next(1, 30)
                    });
                }
            }
            measurements.Sort((m1, m2) => m1.DateTime.CompareTo(m2.DateTime));
            return measurements;
        }

        public static List<Measurement> GetUpdateData(DateTime dateTime)
        {
            var measurements = new List<Measurement>();
            var r = new Random();

            for (int i = 0; i < 100; i++)
            {
                measurements.Add(new Measurement
                {
                    DetectorId = i,
                    DateTime = dateTime.AddMinutes(1),
                    Value = r.Next(1, 30)
                });
            }
            return measurements;
        }
    }
}