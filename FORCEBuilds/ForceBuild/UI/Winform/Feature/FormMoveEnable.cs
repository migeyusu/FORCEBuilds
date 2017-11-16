using System.Drawing;
using System.Windows.Forms;

namespace FORCEBuild.UI.Winform.Feature
{
    public class FormMoveEnable
    {
        private bool _opend = false;
        private  Point _starting;
        private Rectangle _rec;
        public FormMoveEnable() { }

        public FormMoveEnable(Control ctl, Form fm)
        {
            ctl.MouseDown += (obj, ea) =>
            {
                if (ea.Button == MouseButtons.Left)
                {
                    _opend = true;
                    _starting = fm.PointToClient(Control.MousePosition);
                    _starting = new Point(-_starting.X, -_starting.Y);
                }
            };
            ctl.MouseMove += delegate
            {
                if (_opend)
                {
                    var px = Control.MousePosition;
                    px.Offset(_starting);
                    fm.Location = px;
                }
            };
            ctl.MouseUp += (obj, ea) =>
            {
                if (ea.Button == MouseButtons.Left)
                    _opend = false;
            };
        }

        public FormMoveEnable(Control ctl, Form fm, int borderwidth)
        {
            _rec = new Rectangle(borderwidth, 0, fm.Width - borderwidth * 2, fm.Height - borderwidth);
            fm.Resize += (o, e) =>
            {
                var fmo = (Form)o;
                _rec = new Rectangle(borderwidth, 0, fmo.Width - borderwidth * 2, fmo.Height - borderwidth);
            };
            ctl.MouseDown += (obj, ea) =>
            {
                if (ea.Button == MouseButtons.Left && _rec.Contains(ea.Location))
                {
                    _opend = true;
                    _starting = fm.PointToClient(Control.MousePosition);
                    _starting = new Point(-_starting.X, -_starting.Y);
                }
            };
            ctl.MouseMove += delegate
            {
                if (_opend)
                {
                    var px = Control.MousePosition;
                    px.Offset(_starting);
                    fm.Location = px;
                }
            };
            ctl.MouseUp += (obj, ea) =>
            {
                if (ea.Button == MouseButtons.Left)
                    _opend = false;
            };
        }

        public FormMoveEnable(Control ctl, Form fm, Rectangle enableArea)
        {
            ctl.MouseDown += (obj, ea) =>
            {
                if (enableArea.Contains(ea.Location) && ea.Button == MouseButtons.Left)
                {
                    _opend = true;
                    _starting = fm.PointToClient(Control.MousePosition);
                    _starting = new Point(-_starting.X, -_starting.Y);
                }
            };
            ctl.MouseMove += delegate
            {
                if (_opend)
                {
                    var px = Control.MousePosition;
                    px.Offset(_starting);
                    fm.Location = px;
                }
            };
            ctl.MouseUp += (obj, ea) =>
            {
                if (ea.Button == MouseButtons.Left)
                    _opend = false;
            };
        }

        public void Set(Control ctl, Form fm)
        {
            ctl.MouseDown += (obj, ea) =>
            {
                if (ea.Button == MouseButtons.Left)
                {
                    _opend = true;
                    _starting = fm.PointToClient(Control.MousePosition);
                    _starting = new Point(-_starting.X, -_starting.Y);
                }
            };
            ctl.MouseMove += delegate
            {
                if (_opend)
                {
                    var px = Control.MousePosition;
                    px.Offset(_starting);
                    fm.Location = px;
                }
            };
            ctl.MouseUp += (obj, ea) =>
            {
                if (ea.Button == MouseButtons.Left)
                    _opend = false;
            };
        }
    }
}
