using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NotepadPP.DirectX;

namespace NotepadPP.Master {
    class CBase {
        // -- Imports -- \\
        #region Imports
        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_MIDDLEDOWN = 0x020;
        private const uint MOUSEEVENTF_MIDDLEUP = 0x040;
        #endregion

        // -- Variables -- \\
        #region Public
        public bool enabled;
        public int mode;
        public int delay;
        public bool renderesp;
        public bool doBHop;
        #endregion
        #region Private
        private Thread MainThread;
        private Thread bHopThread;
        private Process game;
        private bool doLoop;
        private IntPtr engineBase;
        private IntPtr clientBase;
        private Form1 main;
        private bool hovering;
        private OverlayRenderer render;
        private Client c;
        #endregion

        // -- Functions -- \\
        #region Public

        public CBase(Form1 mWindow) {
            renderesp = false;
            main = mWindow;
            hovering = false;
            delay = 0;
            mode = 0;
            enabled = false;
            doLoop = true;
            game = null;
        }
        public void Start() {
            MainThread = new Thread(() => Main());
            MainThread.Name = "NpPP=> Trigger Thread";
            MainThread.Start();
            bHopThread = new Thread(() => bHopLoop());
            bHopThread.Name = "NpPP=> Bunny Hop Thread";
            bHopThread.Start();
        }
        public void Stop() {
            doLoop = false;
            MainThread.Join();
            bHopThread.Join();
        }
        public double distance(Entity ent1, Entity ent2) {
            return 0;
        }

        public void ToggleBHop() {
            main.button5_Click(null, null);
        }
        public void ToggleEnabled() {
            main.button1_Click(null, null);
        }
        public void ToggleMode() {
            main.button2_Click(null, null);
        }
        public void SetDelay(int delay) {
            main.setDelay(delay);
        }
        public void ToggleEsp() {
            main.button4_Click(null, null);
        }
        public Client getActiveClient() {
            return c;
        }
        public OverlayRenderer getRenderEngine() {
            return render;
        }
        #endregion
        #region Private
        private void getGame() {
            bool doDelay = false;
            while (doLoop) {
                Process[] procs = Process.GetProcessesByName("csgo");
                if (procs.Length > 0) {
                    game = procs[0];
                    break;
                } else {
                    doDelay = true;
                }
                Thread.Sleep(100);
            }
            if(doDelay) {
                Thread.Sleep(10000);
                getGame();
                return;
            }
            if(render == null) {
                render = new OverlayRenderer(game, this);
            }
            render.Start();
        }
        private void getModules() {
            while (doLoop && clientBase.ToInt32() == 0 && engineBase.ToInt32() == 0 && !game.HasExited) {
                ProcessModuleCollection modules = game.Modules;
                foreach (ProcessModule module in modules) {
                    if (module.ModuleName == "client.dll") {
                        clientBase = module.BaseAddress;
                    }
                    if (module.ModuleName == "engine.dll") {
                        engineBase = module.BaseAddress;
                    }
                }
                if (doLoop && enabled && clientBase.ToInt32() == 0 && engineBase.ToInt32() == 0) {
                    Thread.Sleep(100);
                }
            }
        }
        private void bHopLoop() {
            while (doLoop) {
                if(game != null) {
                    if (doBHop) {
                        if (Form1.IsKeyPushedDownWait(System.Windows.Forms.Keys.Space)) {
                            while(true) {
                                mouse_event(MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, new UIntPtr(0));
                                Thread.Sleep(10);
                                mouse_event(MOUSEEVENTF_MIDDLEUP, 0, 0, 0, new UIntPtr(0));
                                Thread.Sleep(10);
                                if(Form1.IsKeyPushedDownWait(System.Windows.Forms.Keys.Space) || !doBHop) {
                                    break;
                                }
                            }
                        }   
                    }
                    Thread.Sleep(5);
                    if (game.HasExited) {
                        break;
                    }
                }
                if (render != null) {
                    while (!render.isGameActive()) {
                        Thread.Sleep(100);
                    }
                }
            }
        }
        private void doBot() {
            c = new Client(engineBase, clientBase, game, main, this);
            c.Start();
            bool rendering = false;
            while (doLoop && !game.HasExited) {
                if(render != null) {
                    while (!render.isGameActive()) {
                        Thread.Sleep(100);
                    }
                }
                if (enabled) {
                    if (c.getTarget().team != 0 && c.getTarget().team != c.me.team) {
                        if (!hovering) {
                            hovering = true;
                            if (delay != 0) {
                                Thread.Sleep(delay);
                            }
                        }
                        if (mode == 0) {
                            //--- Fast
                            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, new UIntPtr(0));
                            Thread.Sleep(20);
                            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, new UIntPtr(0));
                        } else {
                            //--- Slow
                            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, new UIntPtr(0));
                            Thread.Sleep(20);
                            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, new UIntPtr(0));
                            Thread.Sleep(300);
                        }
                    } else {
                        hovering = false;
                    }
                }
                Thread.Sleep(5);
            }
            c.Stop();
        }

        private void Main() {
            while(doLoop) {
                if(render != null) {
                    render.Stop();
                }
                if (game == null) {
                    getGame();
                } else if (game.HasExited) {
                    getGame();
                }
                clientBase = new IntPtr(0);
                engineBase = new IntPtr(0);
                getModules();
                if(game.HasExited) {
                    continue;
                }
                if (!doLoop) { break; }
                doBot();
                if (!doLoop) { break; }
            }
            if (render != null) {
                render.Stop();
            }
        }
        #endregion
    }
}
