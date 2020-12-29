using System.Drawing;
using System.Windows.Forms;

namespace FORCEBuild.UI.Winform.Feature
{
    public class ControlMoveable
    {
        private Point _startLocation;//初始鼠标地址   
        private bool _moving = false;
        public ControlMoveable(Control control)
        {
            control.MouseDown += BaseDown;
            control.MouseMove += BaseMove;
            control.MouseUp += BaseUp;
        }
        public void BaseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _moving = true;
                ((Control)sender).Cursor = Cursors.Hand;
                _startLocation = new Point(-e.X, -e.Y);
            }
        }
        public void BaseMove(object sender, MouseEventArgs e)
        {
            if (_moving)
            {
                var p1 = Control.MousePosition;
                p1.Offset(_startLocation);
                p1 = ((Control)sender).Parent.PointToClient(p1);
                ((Control)sender).Location = p1;
            }
        }
        public void BaseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _moving = false;
                ((Control)sender).Cursor = Cursors.Default;
                ((Control)sender).Focus();
            }
        }
    }
}
