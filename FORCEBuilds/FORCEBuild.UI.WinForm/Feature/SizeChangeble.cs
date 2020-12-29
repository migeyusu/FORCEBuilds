using System.Drawing;
using System.Windows.Forms;

namespace FORCEBuild.UI.Winform.Feature
{
    public class SizeChangeble
    {
        private bool _down = false;
        private ControlStatue _controlStatue;
        private Point _startpPoint, _formPosition;
        private Size _lastsize;
        private readonly Form _form;
        private readonly int _border;

        public enum ControlStatue
        {
            Left = 0,
            Right = 1,
            Down = 3,
            Def = 4,
        }

        public Size MinSize { get; set; }

        public SizeChangeble(Form form, int border, Size size)
        {
            form.MouseMove += FormSizeControl;
            form.MouseDown += DownSign;
            form.MouseUp += UpSign;
            _form = form;
            _border = border;
            MinSize = size;
        }

        private void DownSign(object obj, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _down = true;
                _startpPoint = Control.MousePosition;
                _lastsize = _form.Size;
                _formPosition = _form.Location;
            }
        }

        private void UpSign(object obj, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _down = false;
                _controlStatue = ControlStatue.Def;
            }
        }

        private void FormSizeControl(object obj, MouseEventArgs e)
        {
            if (!_down)
            {
                if (e.X < _border)
                {
                    _form.Cursor = Cursors.SizeWE;
                    _controlStatue = ControlStatue.Left;
                }
                else if (e.X > _form.Width - _border)
                {
                    _form.Cursor = Cursors.SizeWE;
                    _controlStatue = ControlStatue.Right;
                }
                else if (e.Y > _form.Height - _border)
                {
                    _form.Cursor = Cursors.SizeNS;
                    _controlStatue = ControlStatue.Down;
                }
                else
                {
                    _form.Cursor = Cursors.Default;
                    _controlStatue = ControlStatue.Def;
                }
            }
            else
            {
                var mid = 0;
                switch (_controlStatue)
                {
                    case ControlStatue.Down:
                        mid = Control.MousePosition.Y - _startpPoint.Y + _lastsize.Height;
                        if (mid < MinSize.Height)
                            return;
                        _form.Height = mid;
                        break;
                    case ControlStatue.Left:
                        var indent = Control.MousePosition.X - _startpPoint.X;
                        _form.Location = new Point(_formPosition.X + indent, _formPosition.Y);
                        mid = -indent + _lastsize.Width;
                        if (mid < MinSize.Width)
                            return;
                        _form.Width = mid;
                        break;
                    case ControlStatue.Right:
                        mid = Control.MousePosition.X - _startpPoint.X + _lastsize.Width;
                        if (mid < MinSize.Width)
                            return;
                        _form.Width = mid;
                        break;
                }
            }
        }
    }
}
