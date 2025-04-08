using System.Collections.Generic;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Input.StylusPlugIns;
using MicroLibrary;

namespace SignatureVerification
{
    internal class SignaturePlugin : StylusPlugIn
    {
        private readonly StylusPointCollection _stylusPointCollection = new StylusPointCollection();
        private readonly List<StylusPointCollection> intervals = new List<StylusPointCollection>();
        private MicroTimer microTimer = new MicroTimer();
        private List<double> PointTime = new List<double>();

        private void MicroTimerInit()
        {
            // Instantiate new MicroTimer and add event handler
            microTimer = new MicroTimer();
            microTimer.MicroTimerElapsed +=
                OnTimedEvent;

            microTimer.Interval = 1000; // Call micro timer every 1000µs (1ms)

            // Can choose to ignore event if late by Xµs (by default will try to catch up)
            //microTimer.IgnoreEventIfLateBy = 500; // 500µs (0.5ms)

            microTimer.Enabled = false; // Start timer

            // Do something whilst events happening, for demo sleep 2000ms (2sec)
            //System.Threading.Thread.Sleep(2000);

            //microTimer.Enabled = false; // Stop timer (executes asynchronously)

            // Alternatively can choose stop here until current timer event has finished
            // microTimer.StopAndWait(); // Stop timer (waits for timer thread to terminate)

            // Wait for user input
            //Console.ReadLine();
        }

        private void OnTimedEvent(object sender,
            MicroTimerEventArgs timerEventArgs)
        {
            // Do something small that takes significantly less time than Interval
            intervals.Add(_stylusPointCollection.Clone());
            //Debug.WriteLine(string.Format(
            //    "Count = {0:#,0}  Timer = {1:#,0} µs, " +
            //    "LateBy = {2:#,0} µs, ExecutionTime = {3:#,0} µs",
            //    timerEventArgs.TimerCount, timerEventArgs.ElapsedMicroseconds,
            //    timerEventArgs.TimerLateBy, timerEventArgs.CallbackFunctionExecutionTime));
        }

        /// <summary>
        ///     Function that added time to inkCanvas
        /// </summary>
        /// <param name="PointTime"></param>
        public void addTime(List<double> PointTime)
        {
            this.PointTime = PointTime;
        }

        public List<double> getTime()
        {
            return PointTime;
        }

        public Stroke getStroke()
        {
            return new Stroke(_stylusPointCollection);
        }

        public List<StylusPointCollection> getStrokeIntervals()
        {
            return intervals;
        }

        /// <summary>
        ///     Event that are triger when stylus is moved, we get war stylus point and store its time.
        /// </summary>
        /// <param name="rawStylusInput"></param>
        protected override void OnStylusMove(RawStylusInput rawStylusInput)
        {
            base.OnStylusMove(rawStylusInput);

            var points = rawStylusInput.GetStylusPoints();

            foreach (var stylusPoint in points)
            {
                _stylusPointCollection.Add(new StylusPoint(stylusPoint.X, stylusPoint.Y, stylusPoint.PressureFactor));
                long time = rawStylusInput.Timestamp;
                PointTime.Add(time);
            }
        }

        /// <summary>
        ///     Event when stylus touch the surface,we start timer to pool for better acuracy
        /// </summary>
        /// <param name="rawStylusInput"></param>
        protected override void OnStylusDown(RawStylusInput rawStylusInput)
        {
            MicroTimerInit();
        }

        /// <summary>
        ///     When stylus enter surface, we clean old data
        /// </summary>
        /// <param name="rawStylusInput"></param>
        /// <param name="confirmed"></param>
        protected override void OnStylusEnter(RawStylusInput rawStylusInput, bool confirmed)
        {
            base.OnStylusEnter(rawStylusInput, confirmed);
            _stylusPointCollection.Clear();
            intervals.Clear();
        }

        /// <summary>
        ///     When stylus leave surface, we stop timer for better performance
        /// </summary>
        /// <param name="rawStylusInput"></param>
        protected override void OnStylusUp(RawStylusInput rawStylusInput)
        {
            microTimer.Enabled = false;
        }
    }
}