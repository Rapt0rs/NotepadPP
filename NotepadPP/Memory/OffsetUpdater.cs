using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

namespace NotepadPP.Memory {
    #region Offset Classes 
    class BaseOffsets {
        public static int ViewMatrix = 0x04AB2844;
        public static int EntityList = 0x04AC0CA4;
        public static int LocalPlayer = 0x00A9E8C8;
        public static int GlowPointer = 0x04FD91C4;
    };

    class StaticOffsets {
        public static int Team = 0xF0;
        public static int Health = 0xFC;
        public static int Index = 0x64;
        public static int Flags = 0x100;
        public static int Pos = 0x134; //unknown
        public static int GlowIndex = 0xA320;
        public static int CrosshairId = 0xAA70;
        public static int EntitySize = 0x10; //unknown
    };
    #endregion

    class OffsetUpdater {
        #region Private Variables
        private bool updateFileFound = false;
        private bool updateFileFound2 = false;
        #endregion
        #region Public Functions
        public OffsetUpdater() {
            updateFileFound = File.Exists("OffsetManager.txt");
            updateFileFound2 = File.Exists("Dump.txt");
        }
        public void RunUpdate() {
            if (updateFileFound) {
                //--- Y3t1y3t Small Offset Dumper
                string[] lines = File.ReadAllLines("OffsetManager.txt");
                foreach (string line in lines) {
                    string safeline = line.Replace("0x", "");
                    if (safeline.Contains("Extra -> m_dwGlowObject: _______________________ ")) {
                        string nl = safeline.Replace("Extra -> m_dwGlowObject: _______________________ ", "");
                        BaseOffsets.GlowPointer = int.Parse(nl, NumberStyles.HexNumber);
                    }
                    if (safeline.Contains("EntityList -> m_dwEntityList: __________________ ")) {
                        string nl = safeline.Replace("EntityList -> m_dwEntityList: __________________ ", "");
                        BaseOffsets.EntityList = int.Parse(nl, NumberStyles.HexNumber);
                    }
                    if (safeline.Contains("LocalPlayer -> m_dwLocalPlayer: ________________ ")) {
                        string nl = safeline.Replace("LocalPlayer -> m_dwLocalPlayer: ________________ ", "");
                        BaseOffsets.LocalPlayer = int.Parse(nl, NumberStyles.HexNumber);
                    }
                    if (safeline.Contains("EngineRender -> m_dwViewMatrix: ________________ ")) {
                        string nl = safeline.Replace("EngineRender -> m_dwViewMatrix: ________________ ", "");
                        BaseOffsets.ViewMatrix = int.Parse(nl, NumberStyles.HexNumber);
                    }

                    if (safeline.Contains("BaseEntity -> m_dwIndex: _______________________ ")) {
                        string nl = safeline.Replace("BaseEntity -> m_dwIndex: _______________________ ", "");
                        StaticOffsets.Index = int.Parse(nl, NumberStyles.HexNumber);
                    }
                    if (safeline.Contains("DT_BasePlayer -> m_iHealth: ____________________ ")) {
                        string nl = safeline.Replace("DT_BasePlayer -> m_iHealth: ____________________ ", "");
                        StaticOffsets.Health = int.Parse(nl, NumberStyles.HexNumber);
                    }
                    if (safeline.Contains("DT_BasePlayer -> m_fFlags: _____________________ ")) {
                        string nl = safeline.Replace("DT_BasePlayer -> m_fFlags: _____________________ ", "");
                        StaticOffsets.Flags = int.Parse(nl, NumberStyles.HexNumber);
                    }
                    if (safeline.Contains("DT_BaseEntity -> m_iTeamNum: ___________________ ")) {
                        string nl = safeline.Replace("DT_BaseEntity -> m_iTeamNum: ___________________ ", "");
                        StaticOffsets.Team = int.Parse(nl, NumberStyles.HexNumber);
                    }
                    if (safeline.Contains("DT_CSPlayer -> m_iGlowIndex: ___________________ ")) {
                        string nl = safeline.Replace("DT_CSPlayer -> m_iGlowIndex: ___________________ ", "");
                        StaticOffsets.GlowIndex = int.Parse(nl, NumberStyles.HexNumber);
                    }
                    if (safeline.Contains("DT_Local -> m_iCrossHairID: ____________________ ")) {
                        string nl = safeline.Replace("DT_Local -> m_iCrossHairID: ____________________ ", "");
                        StaticOffsets.CrosshairId = int.Parse(nl, NumberStyles.HexNumber);
                    }
                }
            } else if(updateFileFound2) {
                //--- Yeti's Offset Dumper
                // TODO: add scan for offsets
            }
        }
        #endregion

    }
}
