using System.Drawing;
using System.Windows.Forms;

namespace FORCEBuild.UI.Winform.Feature
{
    public class ControlSizer
    {
        /// <summary>
        /// 允许调节者和被调节高度者不是同一个控件
        /// </summary>
        /// <param name="minheight"></param>
        /// <param name="ctlborder"></param>
        /// <param name="binded"></param>
        /// <param name="becontroled"></param>
        public ControlSizer(int minheight, int ctlborder, Control binded,Control becontroled)
        {
            var mousedown = false;
            var start = new Point();
            var height = 0;
            binded.MouseMove += (o, e) =>
            {
                if (!mousedown)
                {
                    if (e.Location.Y > binded.Height - ctlborder)
                    {
                        binded.Cursor = Cursors.SizeNS;
                    }
                    else
                    {
                        binded.Cursor = Cursors.IBeam;
                    }
                }
                else
                {
                    if (binded.Cursor == Cursors.SizeNS)
                    {
                        var nowheight = height + e.Location.Y - start.Y;
                        if (nowheight < minheight)
                        {
                            nowheight = minheight;
                            return;
                        }
                        becontroled.Height = nowheight;
                    }
                }
            };
            binded.MouseDown += (o, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    mousedown = true;
                    start = e.Location;
                    height = becontroled.Height;
                }
            };
            binded.MouseUp += (o, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    mousedown = false;
                }
            };
        }

        public ControlSizer(int minheight,int ctlborder,Control binded)
        {
            var mousedown = false;
            var start = new Point();
            var height = 0;
            binded.MouseMove += (o, e) =>
            {
                if (!mousedown)
                {
                    if (e.Location.Y > binded.Height - ctlborder)
                    {
                        binded.Cursor = Cursors.SizeNS;
                    }
                    else
                    {
                        binded.Cursor = Cursors.IBeam;
                    }
                }
                else
                {
                    if (binded.Cursor == Cursors.SizeNS)
                    {
                        var nowheight = height + e.Location.Y - start.Y;
                        if (nowheight < minheight)
                        {
                            nowheight = minheight;
                            return;
                        }
                        binded.Height = nowheight;
                    }
                }
            };
            binded.MouseDown += (o, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    mousedown = true;
                    start = e.Location;
                    height = binded.Height;
                }
            };
            binded.MouseUp += (o, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    mousedown = false;
                }
            };
        }
    }
}
