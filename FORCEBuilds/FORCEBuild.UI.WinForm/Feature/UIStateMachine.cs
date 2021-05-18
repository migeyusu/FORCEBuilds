using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FORCEBuild.UI.WinForm.Feature
{
    public class UIStateMachine
    {
        public UIStateMachine()
        {
            _ctlList = new Dictionary<Control, FocusFunc>();
        }
        public Control Focusing;
        readonly Dictionary<Control, FocusFunc> _ctlList;
        /// <summary>
        /// 允许通过设置func函数的返回值停止切换
        /// </summary>
        /// <param name="ctl"></param>
        /// <param name="onfocusFunc"></param>
        /// <param name="outfocusFunc"></param>
        public void Add(Control ctl, Func<bool> onfocusFunc, Func<bool> outfocusFunc)
        {
            _ctlList.Add(ctl, new FocusFunc { OnFocus = onfocusFunc, OutFocus = outfocusFunc });
            ctl.MouseClick += ctl_MouseClick;
        }
        void ctl_MouseClick(object sender, MouseEventArgs e)
        {
            if (Focusing == (Control)sender)
            {
                return;
            }
            if (Focusing == null)
            {
                Focusing = (Control)sender;
                //  _ctlList[Focusing].OnFocus();
            }
            else
            {
                var permission = _ctlList[Focusing].OutFocus();//返回false则不作切换
                if (!permission)
                    return;
                Focusing = (Control)sender;
            }
            _ctlList[Focusing].OnFocus();
        }
        public struct FocusFunc
        {
            public Func<bool> OutFocus;
            public Func<bool> OnFocus;
        }
    }
}
