using System;
using System.Drawing;
using System.Windows.Forms;

namespace FORCEBuild.UI.Winform.Cluster
{
    /// <summary>
    /// 组织控件形成flat形式按钮
    /// </summary>
    public class ColorButtonCollection
    {
        public Color DefaultColor { get; set; }
        public Color MouseIn { get; set; }
        public Color MouseDown { get; set; }
        public ColorButtonCollection(Color defautcolor, Color mousein, Color mousedown)
        {
            DefaultColor = defautcolor;
            MouseIn = mousein;
            MouseDown = mousedown;
        }

        public void Set(Control ctl)
        {
            ctl.BackColor = DefaultColor;
            ctl.MouseEnter += ctl_MouseEnter;
            ctl.MouseDown += ctl_MouseDown;
            ctl.MouseLeave += ctl_MouseLeave;
            ctl.MouseUp += ctl_MouseUp;
        }

        void ctl_MouseUp(object sender, MouseEventArgs e)
        {
            ((Control)sender).BackColor = DefaultColor;
        }

        void ctl_MouseLeave(object sender, EventArgs e)
        {
            ((Control)sender).BackColor = DefaultColor;
        }

        void ctl_MouseDown(object sender, MouseEventArgs e)
        {
            ((Control)sender).BackColor = MouseDown;
        }

        void ctl_MouseEnter(object sender, EventArgs e)
        {
            ((Control)sender).BackColor = MouseIn;
        }
    }
}
