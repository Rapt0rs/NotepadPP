using NotepadPP.Master;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NotepadPP.Memory;

namespace NotepadPP {
    public partial class Form1 : Form {
        // -- Imports -- \\
        [DllImport("user32.dll")]
        public static extern ushort GetAsyncKeyState(int vKey);
        public static bool IsKeyPushedDown(System.Windows.Forms.Keys vKey) {
            return 0 != (GetAsyncKeyState((int)vKey) & 0x8000);
        }
        public static bool IsKeyPushedDownWait(System.Windows.Forms.Keys vKey) {
            if (!IsKeyPushedDown(vKey)) { return false; }
            while(IsKeyPushedDown(vKey)) {
                Thread.Sleep(5);
            }
            return true;
        }

        // -- Delegates -- \\
        private delegate void buttonclick(object sender, EventArgs e);
        private delegate void setdelaycallback(int delay);

        // -- Variables -- \\
        private SpeechSynthesizer synthesizer;
        private CBase Master;
        private Thread KeyLoop;
        private bool doLoop;
       
        // -- Initializer -- \\
        public Form1() {
            this.WindowState = FormWindowState.Minimized;
            InitializeComponent();
            Master = new CBase(this);
            doLoop = true;
            synthesizer = new SpeechSynthesizer();
            synthesizer.Volume = 100;  
            synthesizer.Rate = 2;
        }

        // -- Button Events -- \\
        public void button1_Click(object sender, EventArgs e) {
            if(button1.InvokeRequired) {
                buttonclick callback = new buttonclick(button1_Click);
                this.Invoke(callback, new object[] { sender, e });
                return;
            }
            Master.enabled = !Master.enabled;
            if(Master.enabled) {
                label1.ForeColor = Color.LimeGreen;
                synthesizer.SpeakAsync("Enabled");
                label1.Text = "On";
            } else {
                label1.ForeColor = Color.Red;
                synthesizer.SpeakAsync("Disabled");
                label1.Text = "Off";
            }
        }
        public void button2_Click(object sender, EventArgs e) {
            if (button2.InvokeRequired) {
                buttonclick callback = new buttonclick(button2_Click);
                this.Invoke(callback, new object[] { sender, e });
                return;
            }
            if (Master.mode == 0) {
                synthesizer.SpeakAsync("Slow");
                Master.mode = 1;
                button2.Text = "Mode: Slow (Key 7)";
            } else {
                synthesizer.SpeakAsync("Fast");
                Master.mode = 0;
                button2.Text = "Mode: Fast (Key 7)";
            }
        }
        public void button3_Click(object sender, EventArgs e) {
            if(Master.delay == 0) {
                setDelay(20);
            } else if(Master.delay == 20) {
                setDelay(80);
            } else if(Master.delay == 80) {
                setDelay(0);
            }
        }
        public void button4_Click(object sender, EventArgs e) {
            if (button1.InvokeRequired) {
                buttonclick callback = new buttonclick(button4_Click);
                this.Invoke(callback, new object[] { sender, e });
                return;
            }
            Master.renderesp = !Master.renderesp;
            if (Master.renderesp) {
                synthesizer.SpeakAsync("ESP Enabled");
            } else {
                synthesizer.SpeakAsync("ESP Disabled");
            }
        }
        public void button5_Click(object sender, EventArgs e) {
            if (button5.InvokeRequired) {
                buttonclick callback = new buttonclick(button5_Click);
                this.Invoke(callback, new object[] { sender, e });
                return;
            }
            Master.doBHop = !Master.doBHop;
            if (Master.doBHop) {
                synthesizer.SpeakAsync("B Hop Enabled");
            } else {
                synthesizer.SpeakAsync("B Hop Disabled");
            }
        }
        // -- Button Functions -- \\
        public void setDelay(int ms) {
            if (button3.InvokeRequired) {
                setdelaycallback callback = new setdelaycallback(setDelay);
                this.Invoke(callback, new object[] { ms });
                return;
            }
            Master.delay = ms;
            synthesizer.SpeakAsync(ms.ToString() + " ms");
            button3.Text = "Delay: " + ms.ToString() + "ms (Key 8)";
        }

        // -- Form Events -- \\
        private void Form1_Load(object sender, EventArgs e) {
            OffsetUpdater ou = new OffsetUpdater();
            ou.RunUpdate();

            Master.Start();
            KeyLoop = new Thread(() => KeyThread());
            KeyLoop.Name = "NpPP=> Key Event Handler";
            KeyLoop.Start();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            Master.Stop();
            doLoop = false;
            KeyLoop.Join();
        }
       
        // -- Keybind Checks -- \\
        private void KeyThread() {
            while(doLoop) {
                if(IsKeyPushedDown(Keys.D6)) {

                    button1_Click(null, null);

                    while(IsKeyPushedDown(Keys.D6)) {
                        Thread.Sleep(5);
                    }
                }
                if (IsKeyPushedDown(Keys.D7)) {

                    button2_Click(null, null);

                    while (IsKeyPushedDown(Keys.D7)) {
                        Thread.Sleep(5);
                    }
                }
                if (IsKeyPushedDown(Keys.D8)) {

                    button3_Click(null, null);

                    while (IsKeyPushedDown(Keys.D8)) {
                        Thread.Sleep(5);
                    }
                }
                if (IsKeyPushedDown(Keys.D9)) {

                    button4_Click(null, null);

                    while (IsKeyPushedDown(Keys.D9)) {
                        Thread.Sleep(5);
                    }
                }
                if (IsKeyPushedDown(Keys.D0)) {

                    button5_Click(null, null);

                    while (IsKeyPushedDown(Keys.D0)) {
                        Thread.Sleep(5);
                    }
                }
                Thread.Sleep(10);
            }
        }
    }
}
