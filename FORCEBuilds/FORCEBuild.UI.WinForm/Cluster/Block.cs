using System;
using System.Drawing;
using System.Windows.Forms;

namespace FORCEBuild.UI.Winform.Cluster
{
    /// <summary>
    /// 控制单个panel形成一个按钮
    /// </summary>
    class Block
    {
        public Color Default { get; set; }
        public Color ClickColor { get; set; }
        public Block(Panel pl,EventHandler commonclick)
        {
            Default = Color.FromArgb(44, Color.White);
            ClickColor = Color.FromArgb(44, Color.White);
            pl.BackColor = Default;
            pl.Click += commonclick;
            pl.MouseDown += (o, e) =>
            {
                ((Control)o).BackColor = ClickColor;
            };
            pl.MouseUp += (o, e) =>
            {
                ((Control)o).BackColor = Default;
            };
            foreach(Control ctl in pl.Controls)
            {
                ctl.BackColor = Color.Transparent;
                ctl.Click += commonclick;
                ctl.MouseDown += (o, e) =>
                {
                    ((Control)o).Parent.BackColor = ClickColor;
                };
                ctl.MouseUp += (o, e) =>
                {
                    ((Control)o).Parent.BackColor = Default;
                };
            }
        }
    }
}
