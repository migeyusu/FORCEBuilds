using System;
using System.Drawing;
using System.Windows.Forms;

namespace FORCEBuild.UI.WinForm.Cluster
{
    /// <summary>
    /// 组织panel上的所以控件形成一个整体按钮
    /// </summary>
    class BlockCollection
    {
        public Color Default { get; set; }
        public Color ClickColor { get; set; }
        public BlockCollection(Color defaultcr,Color clickcr)
        {
            Default = defaultcr;
            ClickColor = clickcr;
        }
        public void AddPanel(Panel pnl, EventHandler commonclick)
        {
            pnl.BackColor = Default;
            if (commonclick != null)
                pnl.Click += commonclick;
            pnl.MouseDown += (o, e) =>
            {
                ((Control)o).BackColor = ClickColor;
            };
            pnl.MouseUp += (o, e) =>
            {
                ((Control)o).BackColor = Default;
            };
            foreach (Control ctl in pnl.Controls)
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
