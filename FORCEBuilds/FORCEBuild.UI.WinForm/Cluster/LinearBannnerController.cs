using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FORCEBuild.UI.WinForm.Cluster
{
    class LinearBannnerController
    {
        Control _onfocusctl;
        readonly Color _onfocuscr;
        readonly Color _onclickcr;
        readonly List<Control> savectls;
        readonly Image[] imgs;
        public LinearBannnerController(Color onfocuscr, Color onclickcr, Color linearstart, Color linearend, float angle, params Control[] ctls)
        {
            _onfocuscr = onfocuscr;
            _onclickcr = onclickcr;
            savectls = new List<Control>();
            imgs = new Bitmap[ctls.Length];
            for (var i = 0; i < ctls.Length; ++i)
            {
                Brush b;
                if (i % 2 == 1)
                {
                    b = new LinearGradientBrush(ctls[i].ClientRectangle, linearstart, linearend, angle);
                }
                else
                {
                    b = new LinearGradientBrush(ctls[i].ClientRectangle, linearend, linearstart, angle);
                }
                savectls.Add(ctls[i]);
                var bmp = new Bitmap(ctls[i].Width, ctls[i].Height);
                Graphics.FromImage(bmp).FillRectangle(b, ctls[i].ClientRectangle);
                ctls[i].BackgroundImage = bmp;
                ctls[i].MouseEnter += LinearBannnerController_MouseEnter;
                ctls[i].MouseLeave += LinearBannnerController_MouseLeave;
                ctls[i].Click += LinearBannnerController_Click;
                imgs[i] = bmp;
            }
        }

        void LinearBannnerController_Click(object sender, EventArgs e)
        {
            var ctl = (Control)sender;
            if (_onfocusctl != null)
            {
                _onfocusctl.BackColor = Color.Transparent;
                _onfocusctl.BackgroundImage = imgs[savectls.IndexOf(ctl)];
            }
            _onfocusctl = ctl;
            _onfocusctl.BackgroundImage = null;
            _onfocusctl.BackColor = _onclickcr;
        }

        void LinearBannnerController_MouseLeave(object sender, EventArgs e)
        {
            var ctl = (Control)sender;
            if (ctl != _onfocusctl)
            {
                ctl.BackColor = Color.White;
                ctl.BackgroundImage = imgs[savectls.IndexOf(ctl)];
            }
        }

        void LinearBannnerController_MouseEnter(object sender, EventArgs e)
        {
            var ctl = (Control)sender;
            if (ctl == _onfocusctl)
            {
                return;
            }
            ctl.BackColor = _onfocuscr;
            ctl.BackgroundImage = null;
        }
    }

}
