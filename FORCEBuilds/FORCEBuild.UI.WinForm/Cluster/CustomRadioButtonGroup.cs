using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace FORCEBuild.UI.WinForm.Cluster
{
    /// <summary>
    /// 组织控件具有单选特性
    /// </summary>
    class CustomRadioButtonGroup
    {
        public Control _focusCtl;
        readonly Color _onfocusCr;
        readonly Color _outfocusCr;
        readonly Color _onclickCr;
        readonly Dictionary<Control, FocusFunc> _ctlList;
        public CustomRadioButtonGroup(Color onfocus,Color outfocus,Color onclick)
        {
            _onfocusCr = onfocus;
            _outfocusCr = outfocus;
            _onclickCr = onclick;
            _ctlList = new Dictionary<Control, FocusFunc>();
        }
        public void Add(Control ctl, Action onfocusFunc,Action outfocusFunc)
        {
            _ctlList.Add(ctl, new FocusFunc { OnFocus = onfocusFunc, OutFocus = outfocusFunc });
            ctl.BackColor = _outfocusCr;
            ctl.MouseEnter += ctl_MouseEnter;
            ctl.MouseClick += ctl_MouseClick;
            ctl.MouseLeave += ctl_MouseLeave;
        }

        void ctl_MouseLeave(object sender, EventArgs e)
        {
            var ctl = (Control)sender;
            if (ctl != _focusCtl)
                ctl.BackColor = _outfocusCr;
        }

        public void ctl_MouseClick(object sender, MouseEventArgs e)
        {
            if (_focusCtl == (Control)sender)
            {
                return;
            }
            if (_focusCtl == null)
            {
                _focusCtl = (Control)sender;
            }
            else
            {
                _focusCtl.BackColor = _outfocusCr;
                _ctlList[_focusCtl].OutFocus();
                _focusCtl = (Control)sender;
            }
            _ctlList[_focusCtl].OnFocus();
            _focusCtl.BackColor = _onclickCr;
        }

        void ctl_MouseEnter(object sender, EventArgs e)
        {
            var ctl = (Control)sender;
            if (ctl == _focusCtl)
                return;
            ctl.BackColor = _onfocusCr;
        }

        public struct FocusFunc
        {
            public Action OutFocus;
            public Action OnFocus;
        }
    }

}
