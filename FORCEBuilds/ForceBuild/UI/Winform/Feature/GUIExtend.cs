using System.Drawing;
using System.Windows.Forms;

namespace FORCEBuild.UI.Winform.Feature
{
    public static class GuiExtend
    {
        //const int CS_DropSHADOW = 0x20000;
        //const int GCL_STYLE = -26;
        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //public static extern int SetClassLong(IntPtr hwnd, int nIndex, int dwNewLong);
        //[DllImport("user32.dll", CharSet = CharSet.Auto)]
        //public static extern int GetClassLong(IntPtr hwnd, int nIndex);

        /// <summary>
        /// 设置控件不同鼠标状态的背景色
        /// </summary>
        /// <param name="ctl"></param>
        /// <param name="focusCr"></param>
        /// <param name="defaultCr"></param>
        public static void BtnFeature(this Control ctl,Color focusCr,Color defaultCr)
        {
            ctl.MouseEnter += (o, e) =>
            {
                ((Control)o).BackColor = focusCr;
            };
            ctl.MouseLeave += (o, e) =>
            {
                ((Control)o).BackColor = defaultCr;
            };
        }
        /// <summary>
        /// textbox设置提示文字
        /// </summary>
        public static void PointText(this TextBox tb, string text)
        {
            tb.Text = text;
            tb.ForeColor = Color.DimGray;
            tb.GotFocus += (o, e) =>
            {
                if (tb.Text == text)
                {
                    tb.Text = "";
                }
                tb.ForeColor = Color.Black;
            };
            tb.KeyDown += (o, e) =>
            {
                if (tb.ForeColor == Color.DimGray)
                {
                    tb.ForeColor = Color.Black;
                    tb.Text = "";
                }
            };
            tb.KeyUp += (o, e) =>
            {
                if (tb.Text == "")
                {
                    tb.Text = text;
                    tb.ForeColor = Color.DimGray;
                }
            };
        }
        public static void FocusImgChange(this PictureBox pb,Image focusimg,Image defaultimg,Control controller)
        {
            controller.MouseEnter += (o, e) =>
            {
                pb.Image = focusimg;
            };
            controller.MouseLeave += (o, e) =>
            {
                pb.Image = defaultimg;
            };
        }

        public static string FontToString(this Font font)
        {
            return font.Name + "," + font.Size + "," + font.Style;
        }
    }
}
