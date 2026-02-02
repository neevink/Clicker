using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Clicker
{
    public partial class Form1 : Form
    {
        private static bool pressed = false;

        public Form1()
        {
            InitializeComponent();
            timer.Enabled = true;

            timer.Interval = 1000 / (int)Properties.Settings.Default["TapsAtSecond"];
            tapsAtSecond.Text = Convert.ToInt32(1 / (timer.Interval / 1000.0)).ToString();
        }

        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // убираем хук
            UnHook();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetHook();
        }

        #region Timer

        public void CtrlKeyDown()
        {
            if (pressed == true)
            {
                pressed = false;
                indicatorPictire.Image = Properties.Resources.red;
            }
            else
            {
                indicatorPictire.Image = Properties.Resources.green;
                pressed = true;
            }
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            if (pressed)
            {
                pressLeftMouse();
            }
        }
        #endregion

        #region Button hook

        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc callback, IntPtr hInstance, uint threadId);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hInstance);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, int wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string lpFileName);

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        const int WH_KEYBOARD_LL = 13; // Номер глобального LowLevel-хука на клавиатуру
        const int WM_KEYDOWN = 0x100; // Сообщения нажатия клавиши

        private LowLevelKeyboardProc _proc;

        private static IntPtr hhook = IntPtr.Zero;

        public void SetHook()
        {
            IntPtr hInstance = LoadLibrary("User32");
            _proc = hookProc;
            hhook = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, hInstance, 0);
        }

        public static void UnHook()
        {
            UnhookWindowsHookEx(hhook);
        }

        public IntPtr hookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                //////ОБРАБОТКА НАЖАТИЯ
                if (Marshal.ReadInt32(lParam) == 162)
                {
                    CtrlKeyDown();
                    return (IntPtr)1;
                }
                else
                {
                    return CallNextHookEx(hhook, code, (int)wParam, lParam);
                }
            }
            else
                return CallNextHookEx(hhook, code, (int)wParam, lParam);
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, int dwExtraInfo);

        static void pressLeftMouse()
        {
            mouse_event((uint)MouseEventFlags.LEFTDOWN, 0, 0, 0, 0);
            mouse_event((uint)MouseEventFlags.LEFTUP, 0, 0, 0, 0);
        }

        [Flags]
        public enum MouseEventFlags
        {
            LEFTDOWN = 0x00000002,
            LEFTUP = 0x00000004,
            MIDDLEDOWN = 0x00000020,
            MIDDLEUP = 0x00000040,
            MOVE = 0x00000001,
            ABSOLUTE = 0x00008000,
            RIGHTDOWN = 0x00000008,
            RIGHTUP = 0x00000010
        }
        #endregion

        private void acceptButton_Click(object sender, EventArgs e)
        {
            try
            {
                int delta = int.Parse(tapsAtSecond.Text);
                if(delta <= 0)
                {
                    throw new Exception("Число должно быть больше нуля.");
                }
                timer.Interval = 1000 / delta;
                Properties.Settings.Default["TapsAtSecond"] = delta;
                Properties.Settings.Default.Save();
            }
            catch(Exception exc)
            {
                MessageBox.Show(exc.Message);
            }
        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Этот кликер я написал для своей\nмамы. Ей было сложно тянутся до\nкнопки F12 и я написал новое\nприложение.", "О программе");
        }
    }
}
