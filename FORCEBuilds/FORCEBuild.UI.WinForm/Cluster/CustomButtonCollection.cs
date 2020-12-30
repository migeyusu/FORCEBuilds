using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FORCEBuild.UI.WinForm.Cluster
{
    /// <summary>
    /// 组织控件形成可变字体菜单栏
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CustomButtonCollection<T> where T : Control
    {
        readonly List<T> BtnCollection;
        readonly Color _unenable;
        readonly Color _enable;
        readonly float _orisize;
        readonly float _selectsize;
        T preBtn;
        public CustomButtonCollection(Color unEnable, Color enable, float ori, float select, params T[] items)
        {
            BtnCollection = items.ToList<T>();
            foreach (var btn in BtnCollection)
            {
                btn.ForeColor = unEnable;
                btn.Click += btn_Click;
            }
            _unenable = unEnable;
            _enable = enable;
            preBtn = BtnCollection[0];
            _orisize = ori;
            _selectsize = select;

        }

        public void btn_Click(object sender, EventArgs e)
        {
            var nowBtn = (T)sender;
            preBtn.ForeColor = _unenable;
            preBtn.Font = new Font(preBtn.Font.FontFamily, _orisize);
            nowBtn.ForeColor = _enable;
            nowBtn.Font = new Font(preBtn.Font.FontFamily, _selectsize);
            preBtn = nowBtn;
        }

    }
}
