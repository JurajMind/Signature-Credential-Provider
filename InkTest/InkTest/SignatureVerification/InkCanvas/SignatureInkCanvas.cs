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
        private MicroTimer microTimer = new MicroTimer();
        private bool _stylusLeaveSurface = false;

        public SignatureInkCanvas()
        {
            StylusPlugIns.Add(plugin);
            _wholeStroke = new StylusPointCollection();
        }

        public Stroke WholeStroke
        {
            get
            {
                if (!_wholeStroke.Any())
                {
                    return null;
                }

                StylusPointCollection _cloned = _wholeStroke.Clone();
                StylusPoint lastStroke = _cloned.Last(l => l.PressureFactor > 0);
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

        protected override void OnStylusInAirMove(StylusEventArgs e)
        {
            base.OnStylusInAirMove(e);

            if (!_fistStroke)
                return;

            StylusPointCollection inAirStroke = e.GetStylusPoints(this);
            foreach (StylusPoint airStylusPoint in inAirStroke)
            {
                _wholeStroke.Add(new StylusPoint(airStylusPoint.X, airStylusPoint.Y, airStylusPoint.PressureFactor));
            }
        }

        protected override void OnStrokeCollected(InkCanvasStrokeCollectedEventArgs e)
        {
            base.OnStrokeCollected(e);
            if (!_wholeStroke.Any())
                _fistStroke = true;
            _wholeStroke.Add(e.Stroke.StylusPoints);
        }

        protected override void OnStylusDown(StylusDownEventArgs e)
        {
          
            if (_stylusLeaveSurface)
            {
                base.OnStylusEnter(e);
                _fistStroke = false;
                _wholeStroke.Clear();
                this.Strokes.Clear();
            }
            _stylusLeaveSurface = false;

        }

        protected override void OnStylusLeave(StylusEventArgs e)
        {
            base.OnStylusEnter(e);
            _stylusLeaveSurface = true;
           
        }

      

        public Stroke GetStroke()
        {
            return plugin.getStroke();
        }

        public List<StylusPointCollection> getStrokeIntervalsntervals()
        {
            return plugin.getStrokeIntervals();
        }
        
    }
}