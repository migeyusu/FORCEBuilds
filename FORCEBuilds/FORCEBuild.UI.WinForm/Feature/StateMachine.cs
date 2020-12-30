using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FORCEBuild.UI.WinForm.Feature
{
    class SimpleStateMachine
    {
        public SimpleStateMachine()
        {
            _ctlList = new Dictionary<Control, FocusFunc>();
        }
        public Control Focusing;
        readonly Dictionary<Control, FocusFunc> _ctlList;
        public void Add(Control ctl, Action onfocusFunc,Action outfocusFunc)
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
                _ctlList[Focusing].OutFocus();
                Focusing = (Control)sender;
            }
            _ctlList[Focusing].OnFocus();
        }
        public struct FocusFunc
        {
            public Action OutFocus;
            public Action OnFocus;
        }
    }
}
