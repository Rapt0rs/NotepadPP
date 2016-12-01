using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NotepadPP.Memory;

namespace NotepadPP.Master {
    #region structs
    public struct Entity {
        public IntPtr Base;
        public int id;
        public int health;
        public int team;
        public int target;
        public int glow;
        public float[] Position;
        public float distance;
    }
    #endregion

    class Client {
        #region Public Variables
        public int state;
        public Entity me;
        public CBase Master;
        #endregion
        #region Private Variables
        private Process game;
        private Mem M;
        private Thread updateEntitiesLoop;
        private IntPtr engineBase;
        private IntPtr clientBase;
        private Entity[] players;
        private Form1 main;
        private bool doLoop;
        private float[] viewMatrix;
        #endregion

        #region Public Functions
        public Client(IntPtr engine, IntPtr client, Process g, Form1 mWindow, CBase m) {
            Master = m;
            viewMatrix = new float[16];
            main = mWindow;
            state = 0;
            M = new Mem(g,this);
            players = new Entity[64];
            doLoop = true;
            engineBase = engine;
            clientBase = client;
            game = g;
        }
        public void Start() {
            updateEntitiesLoop = new Thread(() => UpdateEntities());
            updateEntitiesLoop.Name = "NpPP=> Update Entites Thread";
            updateEntitiesLoop.Start();
        }
        public void Stop() {
            doLoop = false;
            updateEntitiesLoop.Join();
        }
        public Entity getTarget() {
            try {
                if (me.target == 0) {
                    return new Entity() { team = 0 };
                }

                return players[me.target - 1];
            } catch (Exception) {
                return new Entity() { team = 0 };
            }
        }

        public void GlowEntity(Entity ent) {
            IntPtr AbsGlowBase = M.ReadPtr(Add(clientBase, BaseOffsets.GlowPointer));
            int glowIndex = ent.glow;
            int glowBase = AbsGlowBase.ToInt32() + glowIndex * 0x38;
            M.WriteFloat(new IntPtr(glowBase + 4), 1.0f);
            M.WriteFloat(new IntPtr(glowBase + 8), 0.0f);
            M.WriteFloat(new IntPtr(glowBase + 12), 0.0f);
            M.WriteFloat(new IntPtr(glowBase + 0x10), 0.7f);
            M.WriteBool(new IntPtr(glowBase + 0x24), true);
            M.WriteBool(new IntPtr(glowBase + 0x25), false);
        }

        public Entity[] getEntities() {
            return players;
        }
        public float[] getViewMatrix() {
            for (int i = 0; i < 16; i++) {
                viewMatrix[i] = M.ReadFloat(Add(clientBase, BaseOffsets.ViewMatrix + (i * 0x4)));
            }
            return viewMatrix;
        }
        #endregion
        #region Private Functions
        private IntPtr Add(IntPtr one, int two) {
            return new IntPtr(one.ToInt32() + two);
        }
        private void UpdateEntities() {
            while (doLoop) {
                IntPtr localPlayer = M.ReadPtr(Add(clientBase, BaseOffsets.LocalPlayer));
                if (localPlayer.ToInt32() != 0) {
                    me = buildEntity(localPlayer,true);
                    for (int i = 0; i < 64; i++) {
                        IntPtr entBase = M.ReadPtr(
                                Add(
                                    Add(
                                        clientBase,
                                        BaseOffsets.EntityList
                                    ),
                                    i * (int)StaticOffsets.EntitySize
                                )
                           );
                        if (entBase.ToInt32() == 0) {
                            for (int j = i; j < 64; j++) {
                                players[j] = new Entity() { team = 0 } ;
                            }
                            continue;
                        }

                        players[i] = buildEntity(entBase,false);
                    }
                }
                Thread.Sleep(10);
            }
        }
        private float distance(float x1, float y1, float z1, float x2, float y2, float z2) {
            return (int)Math.Round(Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow(y2 - y1, 2) + Math.Pow(z2 - z1, 2)), 0);
        }
       
        private Entity buildEntity(IntPtr dwBase, bool isMe) {
            Entity ent = new Entity();
            ent.Base = dwBase;
            ent.id = M.ReadInt32(Add(dwBase, StaticOffsets.Index));
            ent.health = M.ReadInt32(Add(dwBase, StaticOffsets.Health));
            ent.team = M.ReadInt32(Add(dwBase, StaticOffsets.Team));
            ent.target = M.ReadInt32(Add(dwBase, StaticOffsets.CrosshairId));
            ent.glow = M.ReadInt32(Add(dwBase, StaticOffsets.GlowIndex));
            float x = M.ReadFloat(Add(dwBase, StaticOffsets.Pos));
            float y = M.ReadFloat(Add(dwBase, StaticOffsets.Pos + 0x4));
            float z = M.ReadFloat(Add(dwBase, StaticOffsets.Pos + 0x8));
            
            ent.Position = new float[] { x, y, z };


            if(!isMe) {
                ent.distance = distance(ent.Position[0], ent.Position[1], ent.Position[2], me.Position[0], me.Position[1], me.Position[2]) / 100.0f;
            } else {
                ent.distance = 0;
            }
            return ent;
        }
        #endregion

    }
}
