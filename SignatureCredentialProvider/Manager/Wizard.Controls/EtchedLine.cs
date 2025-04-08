using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Wizard.Controls
{
    /// <summary>
    ///     Summary description for UserControl1.
    /// </summary>
    public class EtchedLine : UserControl
    {
        /// <summary>
        ///     Required designer variable.
        /// </summary>
        private readonly Container components = null;

        private Color _darkColor = SystemColors.ControlDark;
        private EtchEdge _edge = EtchEdge.Top;

        private Color _lightColor = SystemColors.ControlLightLight;

        public EtchedLine()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            // Avoid receiving the focus.
            SetStyle(ControlStyles.Selectable, false);
        }

        [Category("Appearance")]
        private Color DarkColor
        {
            get { return _darkColor; }

            set
            {
                _darkColor = value;
                Refresh();
            }
        }

        [Category("Appearance")]
        private Color LightColor
        {
            get { return _lightColor; }

            set
            {
                _lightColor = value;
                Refresh();
            }
        }

        [Category("Appearance")]
        public EtchEdge Edge
        {
            get { return _edge; }

            set
            {
                _edge = value;
                Refresh();
            }
        }

        /// <summary>
        ///     Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Brush lightBrush = new SolidBrush(_lightColor);
            Brush darkBrush = new SolidBrush(_darkColor);
            var lightPen = new Pen(lightBrush, 1);
            var darkPen = new Pen(darkBrush, 1);

            if (Edge == EtchEdge.Top)
            {
                e.Graphics.DrawLine(darkPen, 0, 0, Width, 0);
                e.Graphics.DrawLine(lightPen, 0, 1, Width, 1);
            }
            else if (Edge == EtchEdge.Bottom)
            {
                e.Graphics.DrawLine(darkPen, 0, Height - 2,
                    Width, Height - 2);
                e.Graphics.DrawLine(lightPen, 0, Height - 1,
                    Width, Height - 1);
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            Refresh();
        }

        #region Component Designer generated code

        /// <summary>
        ///     Required method for Designer support - do not modify
        ///     the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            // 
            // EtchedLine
            // 
            this.Name = "EtchedLine";
        }

        #endregion
    }
}