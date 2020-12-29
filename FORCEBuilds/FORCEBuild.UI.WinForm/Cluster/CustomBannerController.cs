using System;
using System.Drawing;
using System.Windows.Forms;

namespace FORCEBuild.UI.Winform.Cluster
{
    /// <summary>
    /// 组织控件形成条状菜单栏
    /// </summary>
    class CustomBannerController
    {
        Control _onfocusctl;
        readonly Color _onfocuscr;
        readonly Color _outfocuscr;

        public CustomBannerController(Color onfocus, Color outfocus, params Control[] ctls)
        {
            _onfocuscr = onfocus;
            _outfocuscr = outfocus;
            foreach (var ctl in ctls)
            {
                ctl.BackColor = outfocus;
                ctl.MouseEnter += ctl_MouseEnter;
                ctl.Click += ctl_Click;
                ctl.MouseLeave += ctl_MouseLeave;
            }
        }

        void ctl_MouseLeave(object sender, EventArgs e)
        {
            var ctl = (Control)sender;
            if (ctl != _onfocusctl)
                ctl.BackColor = _outfocuscr;
        }

        void ctl_Click(object sender, EventArgs e)
        {
            if (_onfocusctl != null)
            {
                _onfocusctl.BackColor = _outfocuscr;
            }
            _onfocusctl = (Control)sender;
            _onfocusctl.BackColor = _onfocuscr;
        }

        void ctl_MouseEnter(object sender, EventArgs e)
        {
            var ctl = (Control)sender;
            if (ctl == _onfocusctl)
                return;
            ctl.BackColor = Color.Transparent;
        }
    }
}
