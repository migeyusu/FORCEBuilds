using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FORCEBuild.Windows
{
    public class WindowOperate
    {
        [DllImport("user32.dll", EntryPoint = "keybd_event")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
        [DllImport("user32.dll", EntryPoint = "mouse_event")]
        //dwdata为滚动值，正直表示向前，负值表示向后，最后一个长为零。
        public static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);
        [DllImport("user32.dll")]
        public static extern bool LockWorkStation();
        [DllImport("user32.dll")]
        public static extern void BlockInput(bool Block);
        const string MouseIns = "mouse";
        const string MoveIns = "move";
        const string KeyIns = "key";
        const string DownIns = "down";
        const string UpIns = "up";
        const string LeftIns = "left";
        const string RightIns = "right";
        const string MiddleIns = "middle";
        const int KEYEVENTF_KEYUP = 2;
        const int KEYEVENTF_KEYDOWN = 0;
        const int MOUSEEVENTF_MOVE = 0x0001; // 移动鼠标   
        const int MOUSEEVENTF_LEFTUP = 0x0004; // 鼠标左键抬起  
        const int MOUSEEVENTF_RIGHTDOWN = 0x0008; // 鼠标右键按下  
        const int MOUSEEVENTF_LEFTDOWN = 0x0002; // 鼠标左键按下  
        const int MOUSEEVENTF_RIGHTUP = 0x0010; // 鼠标右键抬起   
        const int MOUSEEVENTF_MIDDLEDOWN = 0x0020; // 鼠标中键按下 
        const int MOUSEEVENTF_MIDDLEUP = 0x0040; // 鼠标中键抬起   
        const int MOUSEEVENTF_ABSOLUTE = 0x8000; // 绝对坐标 
        public WindowOperate()
        {
            Width = Screen.PrimaryScreen.Bounds.Width;
            Height = Screen.PrimaryScreen.Bounds.Height;
        }
        public  int Width { get; set; }
        public  int Height { get; set; }
        public  void LeftMouseDown()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
        }
        public  void RightMouseDown()
        {
            mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
        }
        public  void LeftMouseUp()
        {
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }
        public  void RightMouseUp()
        {
            mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
        }
        public  void MiddleMouseUp()
        {
            mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
        }
        public  void MiddleMouseDown()
        {
            mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
        }
        public  void MouseMove(int x,int y)//需要转成绝对地址
        {
            mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, x * 65536 / Width, y * 65536 / Height, 0, 0);
        }
        public  void KeyDown(byte keyvalue)
        {
            keybd_event(keyvalue, 0, KEYEVENTF_KEYDOWN,0);
        }
        public  void KeyUp(byte keyvalue)
        {
            keybd_event(keyvalue, 0, KEYEVENTF_KEYUP, 0);
        }
        public void ControlEventHandler(string ctlstr)//直接来自于协议的指令
        {
            if (ctlstr.Trim() == "")
                return;
            var ops = ctlstr.Split('|');
            switch(ops[0])
            {
                case MouseIns:
                    switch (ops[1])
                    {
                        case LeftIns:
                            if (ops[2] == DownIns)
                            {
                                mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                            }
                            else
                            {
                                mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                            }
                            break;
                        case RightIns:
                            if(ops[2]==DownIns)
                            {
                                mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                            }
                            else
                            {
                                mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                            }
                            break;
                        case MiddleIns:
                            if(ops[2]==DownIns)
                            {
                                mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, 0);
                            }
                            else
                            {
                                mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, 0);
                            }
                            break;
                    }
                    break;
                case MoveIns:
                    var x = int.Parse(ops[1]);
                    var y = int.Parse(ops[2]);
                    mouse_event(MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE, x * 65536 / Width, y * 65536 / Height, 0, 0);
                    break;
                case KeyIns://key的值为byte值对应keyeventargs.keyvalue.tostring()
                    var keyvalue = byte.Parse(ops[1]);
                    if(ops[2]==DownIns)
                    {
                        keybd_event(keyvalue, 0, KEYEVENTF_KEYDOWN, 0);
                    }
                    else
                    {
                        keybd_event(keyvalue, 0, KEYEVENTF_KEYUP, 0);
                    }
                    break;
            }
        }
        private static FileStream fs;
        public static void KeyMouseHook()
        {
            //屏蔽ctl+alt+delete
            fs = new FileStream(Environment.ExpandEnvironmentVariables("%windir%\\system32\\taskmgr.exe"), FileMode.Open);
            BlockInput(true);
        }
        public static void KeyMouseHookEnd()
        {
            fs.Close();
            BlockInput(false);
        }
    }
}
