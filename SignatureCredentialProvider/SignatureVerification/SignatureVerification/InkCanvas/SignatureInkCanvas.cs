using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using MicroLibrary;

namespace SignatureVerification
{
    public class SignatureInkCanvas : InkCanvas
    {
        private readonly StylusPointCollection _wholeStroke;
        private readonly SignaturePlugin plugin = new SignaturePlugin();
        private bool _fistStroke;
        private bool _stylusLeaveSurface;
        private MicroTimer microTimer = new MicroTimer();

        public SignatureInkCanvas()
        {
            StylusPlugIns.Add(plugin);
            _wholeStroke = new StylusPointCollection();
        }

        /// <summary>
        ///     In Whole stroke all drawing is stored as one stroke, this include off surface movement
        /// </summary>
        public Stroke WholeStroke
        {
            get
            {
                if (!_wholeStroke.Any())
                {
                    return null;
                }

                var _cloned = _wholeStroke.Clone();
                //remove last "off surface" movement
                var lastStroke = _cloned.Last(l => l.PressureFactor > 0);
                _cloned = new StylusPointCollection(_cloned.TakeWhile(l => l != lastStroke));
                return new Stroke(_cloned);
            }
        }

        public void AddTime(List<double> pointTime)
        {
            plugin.addTime(pointTime);
        }

        public List<double> GetTime()
        {
            return plugin.getTime();
        }

        /// <summary>
        ///     Event that is trigered when stylus move over surface, we store its moveement if alredy some stroke is drawen
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStylusInAirMove(StylusEventArgs e)
        {
            base.OnStylusInAirMove(e);

            if (!_fistStroke)
                return;

            var inAirStroke = e.GetStylusPoints(this);
            foreach (var airStylusPoint in inAirStroke)
            {
                _wholeStroke.Add(new StylusPoint(airStylusPoint.X, airStylusPoint.Y, airStylusPoint.PressureFactor));
            }
        }

        /// <summary>
        ///     When stroke is recorded ( whole movement is ended by pen tip off the surface )  we set first stroke and added all
        ///     stroke points to whole stroke
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStrokeCollected(InkCanvasStrokeCollectedEventArgs e)
        {
            base.OnStrokeCollected(e);
            if (!_wholeStroke.Any())
                _fistStroke = true;
            _wholeStroke.Add(e.Stroke.StylusPoints);
        }

        /// <summary>
        ///     When stylus enter surface, we chech if it leave surface, if so, we cant garanted if whole movement was recorder
        ///     if so, we clear _wholestroke
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStylusDown(StylusDownEventArgs e)
        {
            if (_stylusLeaveSurface)
            {
                OnStylusEnter(e);
                _fistStroke = false;
                _wholeStroke.Clear();
                Strokes.Clear();
            }
            _stylusLeaveSurface = false;
        }

        /// <summary>
        ///     When stylus leave surface, we set flag to true, we dont clear _wholestroke now, b
        ///     becouse stylus might leavu becouse user want click on user control.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStylusLeave(StylusEventArgs e)
        {
            OnStylusEnter(e);
            _stylusLeaveSurface = true;
        }

        //Route to plugin whole stroke
        public Stroke GetStroke()
        {
            return plugin.getStroke();
        }

        /// <summary>
        ///     Route to plugin time information about stroke
        /// </summary>
        /// <returns></returns>
        public List<StylusPointCollection> getStrokeIntervalsntervals()
        {
            return plugin.getStrokeIntervals();
        }
    }
}