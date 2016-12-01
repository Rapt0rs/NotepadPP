using DirectXOverlayWindow;
using NotepadPP.Master;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static DirectXOverlayWindow.Native;

namespace NotepadPP.DirectX {
    class OverlayRenderer {
        #region Imports
        [DllImport("user32.dll")]
        private static extern ushort GetAsyncKeyState(int vKey);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);
        #endregion

        #region Public

        #endregion
        #region Private
        private Process game;
        private CBase Master;
        private OverlayWindow overlay;
        private int tealBrush;
        private int blackBrush;
        private int blackBrush2;
        private int redBrush;
        private int font;
        private int fontSmall;
        private int titleFont;
        private bool doLoop;
        private Thread draw;
        private bool mouseDown;
        private bool justToggled;
        private bool move;
        private int Yshift;
        private int Xshift;
        private int Yoffset;
        private int Xoffset;
        private POINT mousePos;
        private Stopwatch watch;
        #endregion

        #region Public
        public OverlayRenderer(Process g, CBase b) {
            watch = new Stopwatch();
            game = g;
            Master = b;
            mouseDown = false;
            move = false;
            overlay = new OverlayWindow(false);
            tealBrush = overlay.Graphics.CreateBrush(System.Drawing.Color.FromArgb(80, 0, 255, 255));
            blackBrush = overlay.Graphics.CreateBrush(System.Drawing.Color.FromArgb(80, 0, 0, 0));
            blackBrush2 = overlay.Graphics.CreateBrush(System.Drawing.Color.FromArgb(130, 0, 0, 0));
            redBrush = overlay.Graphics.CreateBrush(System.Drawing.Color.FromArgb(130, 255, 0, 0));
            mousePos = new POINT() { X = 0, Y = 0 };
            font = overlay.Graphics.CreateFont("Arial", 15);
            fontSmall = overlay.Graphics.CreateFont("Arial", 12, true);
            titleFont = overlay.Graphics.CreateFont("Arial", 18, true);

            doLoop = true;

            Yoffset = 0;
            Xoffset = 0;
            Xshift = 0;
            Yshift = 350;
        }
        public int getWidth() {
            return overlay.Width;
        }
        public int getHeight() {
            return overlay.Height;
        }
        public void Start() {
            if(!doLoop) {
                doLoop = true;
            }
            draw = new Thread(() => drawThread());
            draw.Name = "NpPP=> UI Draw Thread";
            watch.Start();
            draw.Start();
        }
        public void Stop() {

            doLoop = false;
            draw.Join();
            watch.Stop();
            overlay.Graphics.BeginScene();
            overlay.Graphics.ClearScene();
            overlay.Graphics.EndScene();
        }
        public bool isGameActive() {
            var activatedHandle = GetForegroundWindow();
            if (activatedHandle == IntPtr.Zero) {
                return false;       // No window is currently activated
            }

            var procId = game.Id;
            int activeProcId;
            GetWindowThreadProcessId(activatedHandle, out activeProcId);

            return activeProcId == procId;
        }
        #endregion
        #region Private
        private bool worldToScreen(Client c, float[] world, out float[] screen) {
            screen = new float[] { 0, 0 };
            float[] ViewMatrix = c.getViewMatrix();
            float f1Temp = ViewMatrix[12] * world[0] + ViewMatrix[13] * world[1] + ViewMatrix[14] * world[2] + ViewMatrix[15];

            screen[0] = ViewMatrix[0] * world[0] + ViewMatrix[1] * world[1] + ViewMatrix[2] * world[2] + ViewMatrix[3];
            screen[1] = ViewMatrix[4] * world[0] + ViewMatrix[5] * world[1] + ViewMatrix[6] * world[2] + ViewMatrix[7];

            if (f1Temp < 0.01f)
                return false;

            float invFlTemp= 1.0f / f1Temp;
            screen[0] *= invFlTemp;
            screen[1] *= invFlTemp;

            int width = getWidth();
            int height = getHeight();

            float x = width / 2;
            float y = height / 2;

            x += 0.5f * screen[0] * width + 0.5f;
            y -= 0.5f * screen[1] * height + 0.5f;

            screen[0] = x;
            screen[1] = y;
            return true;
        }
        private Entity[] getEntities(Client c) {
            return c.getEntities();
        }
        private void RenderESP() {
            Client c = Master.getActiveClient();
            if(c == null) {
                return;
            }


            foreach(Entity ent in getEntities(c)) {
                float[] screenPos = new float[] { 0, 0 };
                if (ent.team != 0 && ent.team != c.me.team && ent.health > 0 && ent.Position != null) {
                    ent.Position[2] += 80.0f;
                    if (worldToScreen(c, ent.Position, out screenPos) && ent.distance <= 15.0f) {
                        c.GlowEntity(ent);
                        overlay.Graphics.DrawText("Enemy | " + Math.Round(ent.distance,0).ToString() + "m", fontSmall, redBrush, (int)screenPos[0] - 33, (int)screenPos[1]);
                        overlay.Graphics.FillRectangle((int)screenPos[0] - 33, (int)screenPos[1] - 8, (int)(66.0 * (ent.health / 100.0)), 5, redBrush);
                        overlay.Graphics.DrawRectangle((int)screenPos[0] - 33, (int)screenPos[1] - 8, 66, 5, 1, blackBrush2);

                    }
                }
            }

        }
        private int distance(int x1, int y1, int x2, int y2) {
            return (int)Math.Round(Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow(y2 - y1, 2)), 0);
        }
        private void handleInput() {
            Direct2DRenderer pDevice = overlay.Graphics;
            if(justToggled) {
                if (mousePos.X > 5 + Xshift && mousePos.X < 175 + Xshift) {
                    if (mousePos.Y > 5 + Yshift && mousePos.Y < 25 + Yshift) {
                        move = true;

                        Xoffset = mousePos.X - Xshift;
                        Yoffset = mousePos.Y - Yshift;
                    }
                }
                if (mousePos.X > 15 + Xshift && mousePos.X < 30 + Xshift) {
                    if (mousePos.Y > 50 + Yshift && mousePos.Y < 65 + Yshift) {
                        Master.ToggleEnabled();
                    }
                }
                if (mousePos.X > 15 + Xshift && mousePos.X < 30 + Xshift) {
                    if (mousePos.Y > 70 + Yshift && mousePos.Y < 85 + Yshift) {
                        Master.ToggleMode();
                    }
                }
                if (mousePos.X > 15 + Xshift && mousePos.X < 30 + Xshift) {
                    if (mousePos.Y > 90 + Yshift && mousePos.Y < 105 + Yshift) {
                        Master.ToggleEsp();
                    }
                }
                if (mousePos.X > 15 + Xshift && mousePos.X < 30 + Xshift) {
                    if (mousePos.Y > 110 + Yshift && mousePos.Y < 125 + Yshift) {
                        Master.ToggleBHop();
                    }
                }

                Yshift += 25;

                if (distance(mousePos.X, mousePos.Y, 22 + Xshift, 117 + Yshift) <= 8) {
                    Master.SetDelay(0);
                }
                if (distance(mousePos.X, mousePos.Y, 22 + Xshift, 137 + Yshift) <= 8) {
                    Master.SetDelay(20);
                }
                if (distance(mousePos.X, mousePos.Y, 22 + Xshift, 157 + Yshift) <= 8) {
                    Master.SetDelay(80);
                }

                Yshift -= 25;
            }
            if(move) {
                if (mouseDown) {
                    Xshift = mousePos.X - Xoffset;
                    Yshift = mousePos.Y - Yoffset;
                } else {
                    move = false;
                }
            }
        }
        private void drawThread() {
            Direct2DRenderer pDevice = overlay.Graphics;
            
            while (doLoop) {
                if(!isGameActive()) {
                    pDevice.BeginScene();
                    pDevice.ClearScene();
                    pDevice.EndScene();

                    while(!isGameActive() && doLoop) {
                        Thread.Sleep(100);
                    }
                    if (!doLoop) { break; }
                }


                watch.Reset();
                watch.Start();
                if ((GetAsyncKeyState((int)0x01) & 0x8000) != 0) {
                    if (!mouseDown) {
                        justToggled = true;
                        mouseDown = true;
                    } else {
                        justToggled = false;
                    }
                } else {
                    mouseDown = false;
                    justToggled = false;
                }
                GetCursorPos(out mousePos);

                handleInput();

                pDevice.BeginScene();
                pDevice.ClearScene();

                if(Master.renderesp) {
                    RenderESP();
                }

                pDevice.FillRectangle(5 + Xshift, 5 + Yshift, 170, 210, blackBrush);
                pDevice.DrawRectangle(5 + Xshift, 5 + Yshift, 170, 210, 5, tealBrush);
                pDevice.DrawText("LysDick Hex", titleFont, tealBrush, 15 + Xshift, 10 + Yshift);

                pDevice.DrawRectangle(15 + Xshift, 50 + Yshift, 15, 15, 1, tealBrush);
                pDevice.DrawText("Enable Triggerbot", font, tealBrush, 35 + Xshift, 49 + Yshift);
                pDevice.DrawRectangle(15 + Xshift, 70 + Yshift, 15, 15, 1, tealBrush);
                pDevice.DrawText("Slow Mode", font, tealBrush, 35 + Xshift, 69 + Yshift);
                pDevice.DrawRectangle(15 + Xshift, 90 + Yshift, 15, 15, 1, tealBrush);
                pDevice.DrawText("ESP (15m)", font, tealBrush, 35 + Xshift, 89 + Yshift);
                pDevice.DrawRectangle(15 + Xshift, 110 + Yshift, 15, 15, 1, tealBrush);
                pDevice.DrawText("Bunny Hop (Space)", font, tealBrush, 35 + Xshift, 109 + Yshift);

                if (Master.enabled) {
                    pDevice.FillRectangle(16 + Xshift, 51 + Yshift, 12, 12, tealBrush);
                }
                if (Master.mode == 1) {
                    pDevice.FillRectangle(16 + Xshift, 71 + Yshift, 12, 12, tealBrush);
                }
                if (Master.renderesp) {
                    pDevice.FillRectangle(16 + Xshift, 91 + Yshift, 12, 12, tealBrush);
                }
                if(Master.doBHop) {
                    pDevice.FillRectangle(16 + Xshift, 111 + Yshift, 12, 12, tealBrush);
                }

                Yshift += 25;

                pDevice.DrawCircle(22 + Xshift, 117 + Yshift, 8, 1, tealBrush);
                pDevice.DrawText("0ms Delay", font, tealBrush, 35 + Xshift, 109 + Yshift);
                pDevice.DrawCircle(22 + Xshift, 137 + Yshift, 8, 1, tealBrush);
                pDevice.DrawText("20ms Delay", font, tealBrush, 35 + Xshift, 129 + Yshift);
                pDevice.DrawCircle(22 + Xshift, 157 + Yshift, 8, 1, tealBrush);
                pDevice.DrawText("80ms Delay", font, tealBrush, 35 + Xshift, 149 + Yshift);
                if (Master.delay == 0) {
                    pDevice.FillCircle(22 + Xshift, 117 + Yshift, 7, tealBrush);
                }
                if (Master.delay == 20) {
                    pDevice.FillCircle(22 + Xshift, 137 + Yshift, 7, tealBrush);
                }

                if (Master.delay == 80) {
                    pDevice.FillCircle(22 + Xshift, 157 + Yshift, 7, tealBrush);
                }
                while(watch.ElapsedMilliseconds < (50 / 3)) {
                    Thread.Sleep(1);
                }
                Yshift -= 25;
                pDevice.EndScene();
            }
        }
        #endregion
    }
}
