using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace FORCEBuild.UI.Winform.Render
{
    public class ShadowCreater
    {
        public ShadowCreater(Control ctl, int width, ShadowPositon sp)
        {
            switch (sp)
            {
                case ShadowPositon.up:
                    var rec = new Rectangle(ctl.Location.X, ctl.Location.Y - width, ctl.Width, width);
                    var lgb = new LinearGradientBrush(rec, Color.FromArgb(0, 0, 0, 0), Color.Black, 90);
                    ctl.Parent.Paint += (s, e) => e.Graphics.FillRectangle(lgb, rec);
                    break;
                case ShadowPositon.down:
                    var rec1 = new Rectangle(ctl.Location.X, ctl.Location.Y + ctl.Height, ctl.Width, width);
                    var lgb1 = new LinearGradientBrush(rec1, Color.Black, Color.FromArgb(0, 0, 0, 0), 90);
                    ctl.Parent.Paint += (s, e) => e.Graphics.FillRectangle(lgb1, rec1);
                    break;
                default:
                    return;
            }


        }
        public enum ShadowPositon
        {
            up,
            left,
            right,
            down
        }
    }
}
