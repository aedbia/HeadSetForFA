﻿using ABEasyLib;
using FacialAnimation;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using static Verse.Widgets;

namespace HeadSetForFA
{
    public class HeadSetMod : Mod
    {
        public static HSMSetting setting;
        private static Vector2 ScrollPosition0 = Vector2.zero;
        private string search = "";
        private static List<ABEasyUtility.ScrollViewContent> settingsContent = new List<ABEasyUtility.ScrollViewContent>();
        private bool isFliter = false;
        private static List<ABEasyUtility.ScrollViewContent> fliters = new List<ABEasyUtility.ScrollViewContent>();
        public HeadSetMod(ModContentPack content) : base(content)
        {
            setting = GetSettings<HSMSetting>();
        }

        public override void WriteSettings()
        {
            try
            {
                ResolveFaceGraphics();
            }
            catch (Exception ingore) { }
            base.WriteSettings();
        }

        public static void ResolveFaceGraphics()
        {
            if (Current.Game == null)
            {
                return;
            }
            Map map = Current.Game.CurrentMap;
            if (map == null)
            {
                return;
            }
            List<Pawn> pawns;
            if (HSMCache.FARaceList != null)
            {
                pawns = map.mapPawns.AllPawns.
                Where(x => HSMCache.FARaceList.Contains(x.def) && x.def.race != null).ToList();
            }
            else
            {
                pawns = new List<Pawn>();
            }
            if (pawns.NullOrEmpty())
            {
                return;
            }
            foreach (Pawn pawn in pawns)
            {
                pawn.Drawer.renderer.SetAllGraphicsDirty();
                if (pawn.IsColonist)
                {
                    PortraitsCache.SetDirty(pawn);
                }
            }
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect rect0 = inRect.TopPart(0.04f);
            Rect sl = new Rect(rect0.x, rect0.y, rect0.height, rect0.height);
            Rect el = new Rect(rect0.x + rect0.height + 5f, rect0.y, rect0.width / 2 - rect0.height - 10f, rect0.height);
            search = TextArea(el, search);
            if (ButtonImage(sl, TexButton.SearchButton))
            {
                isFliter = true;
                GUI.DrawTexture(sl, TexButton.SearchButton);
            }
            CheckboxLabeled(rect0.RightHalf(), "Enable_For_Not_Colonist".Translate(), ref HSMSetting.EnableForNotColonist);
            Rect rect1 = inRect.BottomPart(0.95f);
            DrawWindowBackground(rect1);
            Rect outRect = rect1.ContractedBy(5f);
            if (HSMCache.FARaceList.NullOrEmpty())
            {
                return;
            }
            if (settingsContent == null)
            {
                settingsContent = new List<ABEasyUtility.ScrollViewContent>();
            }
            for (int a = 0; a < HSMCache.FARaceList.Count; a++)
            {
                ThingDef race = HSMCache.FARaceList[a];
                if (!settingsContent.ContainsAny(b => b.Id == race.defName))
                {
                    settingsContent.Add(new SettingsUnit(outRect.width, outRect.height / 2f, outRect.height / 20f, race));
                }
            }
            if (search.NullOrEmpty())
            {
                fliters = settingsContent;
            }
            else
            if (settingsContent.Count != 0 && isFliter)
            {
                SettingsUnit.fliter = true;
                fliters = settingsContent.Where(s => s.Id.IndexOf(search, StringComparison.CurrentCultureIgnoreCase) != -1).ToList();
                SettingsUnit.fliter = false;
                isFliter = false;
            }
            if (fliters.Count != 0 && HSMSetting.datas != null)
            {
                ABEasyUtility.DrawScrollPanel(outRect, fliters, ref ScrollPosition0);
            }

        }

        public override string SettingsCategory()
        {
            return Content.Name;
        }
    }
    public class SettingsUnit : ABEasyUtility.ScrollViewContent
    {
        public static bool fliter = false;
        private bool fold = true;
        public string raceName;
        private readonly ThingDef raceDef;
        private float floatHight = 0;
        private float unitHight;
        private bool enableXeno = false;
        private bool BaseSetting = false;
        public static readonly string noAge = "noAge";
        private static Color WindowBGBorderColor = new ColorInt(97, 108, 122).ToColor;
        private Dictionary<string, DrawDataSetting> ownDatas = new Dictionary<string, DrawDataSetting>();
        private string unfoldStr = "";
        public override string Id
        {
            get
            {
                if (fliter)
                {
                    return raceName;
                }
                else
                {
                    return base.Id;
                }
            }
        }
        public override float Height
        {
            get
            {
                if (fold)
                {
                    return unitHight;
                }
                else if (floatHight != 0)
                {
                    return floatHight;
                }

                else
                {
                    return base.Height;
                }
            }
        }
        public SettingsUnit(float width, float height, float foldHeight, ThingDef raceDef) : base(width, height, raceDef.defName)
        {
            this.raceDef = raceDef;
            raceName = raceDef.label;
            unitHight = foldHeight;
        }

        public override void DrawContect(Rect inRect)
        {
            Rect rect0 = new Rect(inRect.x, inRect.y, inRect.width, unitHight);
            DrawBoxSolid(rect0, WindowBGBorderColor);
            DrawBox(inRect);
            GUI.Label(rect0, raceName + " defName[" + Id + "]", HSMCache.TextStyle);
            GUI.DrawTexture(new Rect(rect0.x + rect0.width - unitHight, rect0.y, unitHight, unitHight), fold ? TexButton.ReorderDown : TexButton.ReorderUp);
            float lh = 0;
            if (ButtonInvisible(rect0))
            {
                fold = !fold;
            }
            if (fold)
            {
                return;
            }
            Rect rect1 = new Rect(inRect.x + 10f, inRect.y + unitHight + 5, inRect.width - 10, unitHight);
            lh += rect0.height + 5f;
            if (ButtonText(rect1.LeftPart(0.3f), "reset".Translate()))
            {
                HSMSetting.datas.Remove(raceDef.defName);
            }
            Rect cr = rect1.RightPart(0.65f);
            CheckboxLabeled(cr.RightHalf(), "Xeno_Mode".Translate(), ref enableXeno);
            CheckboxLabeled(cr.LeftHalf(), "BaseHairSettings".Translate(), ref BaseSetting);
            HSMSetting.CheckSettingData(raceDef);
            DrawLineVertical(inRect.x + 5, inRect.y + rect0.height, inRect.height - rect0.height);
            if (HSMSetting.datas.TryGetValue(Id, out Dictionary<string, HSMSetting.HSMData> ageDatas))
            {
                rect1.y += rect1.height + 5f;
                lh += rect0.height + 5f;
                if (raceDef.race != null)
                {
                    if (ownDatas == null)
                    {
                        ownDatas = new Dictionary<string, DrawDataSetting>();
                    }
                    if (raceDef.race.lifeStageAges.NullOrEmpty())
                    {
                        if (ageDatas.TryGetValue(noAge, out HSMSetting.HSMData data))
                        {
                            DrawDataSetting draw;
                            if (ownDatas.TryGetValue(noAge, out DrawDataSetting drawData))
                            {
                                draw = drawData;
                            }
                            else
                            {
                                draw = new DrawDataSetting();
                                ownDatas.Add(noAge, draw);

                            }
                            draw.xenoMode = enableXeno;
                            draw.BaseOffsetAndScale = BaseSetting;
                            int unitCount = draw.xenoMode ? DrawDataSetting.xenotypeDefs.Count + 1 : 9;
                            rect1.height = draw.fold ? unitHight : unitCount * (unitHight + 5f) - 5f;
                            DrawBox(inRect);
                            if (noAge != unfoldStr)
                            {
                                draw.fold = true;
                            }
                            if (draw.DrawData(rect1, ">= 0" + "years_old".Translate(), unitHight, ref data))
                            {
                                unfoldStr = noAge;
                            }

                            lh += rect1.height + 5;
                        }
                        else
                        {
                            ageDatas.Add(noAge, new HSMSetting.HSMData());
                        }
                    }
                    else
                    {
                        foreach (LifeStageAge stage in raceDef.race.lifeStageAges)
                        {
                            if (ageDatas.TryGetValue(stage.def.defName, out HSMSetting.HSMData data))
                            {
                                DrawDataSetting draw;
                                if (ownDatas.TryGetValue(stage.def.defName, out DrawDataSetting drawData))
                                {
                                    draw = drawData;
                                }
                                else
                                {
                                    draw = new DrawDataSetting();
                                    ownDatas.Add(stage.def.defName, draw);
                                }
                                draw.xenoMode = enableXeno;
                                draw.BaseOffsetAndScale = BaseSetting;
                                int unitCount = draw.xenoMode ? DrawDataSetting.xenotypeDefs.Count + 1 : 9;
                                rect1.height = draw.fold ? unitHight : unitCount * (unitHight + 5f) - 5f;
                                DrawBox(inRect);
                                if (stage.def.defName != unfoldStr)
                                {
                                    draw.fold = true;
                                }
                                if (draw.DrawData(rect1, stage.def.label + " : >= " + stage.minAge + " " + "years_old".Translate(), unitHight, ref data))
                                {
                                    unfoldStr = stage.def.defName;
                                }
                                rect1.y += rect1.height + 5f;
                                lh += rect1.height + 5f;
                            }
                            else
                            {
                                ageDatas.Add(stage.def.defName, new HSMSetting.HSMData());
                            }
                        }
                    }

                }
                floatHight = lh;
            }
            else
            {
                HSMSetting.datas.Add(Id, new Dictionary<string, HSMSetting.HSMData>());
            }
        }

        internal class DrawDataSetting
        {
            public static readonly List<XenotypeDef> xenotypeDefs = DefDatabase<XenotypeDef>.AllDefs.ToList();
            public bool fold = true;
            public bool xenoMode = false;
            public bool BaseOffsetAndScale = false;
            public bool DrawData(Rect inRect, string label, float foldHeight, ref HSMSetting.HSMData data)
            {
                Rect rect0 = new Rect(inRect.x, inRect.y, inRect.width - 5f, foldHeight);
                DrawBoxSolid(rect0, Color.gray);
                Rect bl = new Rect(rect0.x + rect0.width - rect0.height - 5f, rect0.y, rect0.height, rect0.height);
                GUI.DrawTexture(bl, fold ? TexButton.ReorderDown : TexButton.ReorderUp);
                rect0.x += 5f;
                string text = BaseOffsetAndScale ? "(HairBaseOffsetAndSize)".Translate().RawText : "";
                Label(rect0, label + text);
                rect0.x -= 5f;
                if (ButtonInvisible(rect0))
                {
                    fold = !fold;
                    if (!fold)
                    {
                        return true;
                    }
                }
                if (fold)
                {
                    return false;
                }
                rect0.x += 5f;
                rect0.width -= 5f;
                rect0.y += rect0.height + 5f;

                if (xenoMode)
                {
                    Rect iconLoc = new Rect(rect0.x, rect0.y, rect0.height, rect0.height);
                    Rect radioLoc = new Rect(rect0.x + iconLoc.width + 5f, rect0.y, rect0.width - iconLoc.width - 5f, rect0.height);
                    for (int i = 0; i < xenotypeDefs.Count; i++)
                    {
                        XenotypeDef def = xenotypeDefs[i];
                        GUI.DrawTexture(iconLoc, def.Icon ?? Texture2D.grayTexture);
                        if (Mouse.IsOver(rect0))
                        {
                            DrawHighlight(rect0);
                        }
                        if (RadioButtonLabeled(radioLoc, "Ignore".Translate() + " " + def.label, data.NoFaXenos.Contains(def.defName)))
                        {
                            if (data.NoFaXenos.Contains(def.defName))
                            {
                                data.NoFaXenos.Remove(def.defName);
                            }
                            else
                            {
                                data.NoFaXenos.Add(def.defName);
                            }
                        }
                        iconLoc.y += iconLoc.height + 5f;
                        radioLoc.y += radioLoc.height + 5f;
                        rect0.y += rect0.height + 5f;
                    }
                }
                else
                {
                    Rect lc = rect0.LeftPart(0.33f);
                    Rect mc = rect0.LeftPart(0.667f).RightPart(0.5f);
                    Rect rc = rect0.RightPart(0.33f);
                    if (Mouse.IsOver(lc))
                    {
                        DrawHighlight(lc);
                    }
                    CheckboxLabeled(lc, "EnableFAForFemale".Translate(), ref data.EnableForFemale);
                    lc.y += rect0.height + 5f;
                    if (Mouse.IsOver(lc))
                    {
                        DrawHighlight(lc);
                    }
                    CheckboxLabeled(lc, "OffsetForFemale".Translate(), ref data.OffsetForFemale, !data.EnableForFemale);
                    lc.y += rect0.height + 5f;
                    if (Mouse.IsOver(lc))
                    {
                        DrawHighlight(lc);
                    }
                    CheckboxLabeled(lc, "SizeForFemale".Translate(), ref data.SizeForFemale, !data.EnableForFemale);
                    lc.y += rect0.height + 5f;
                    if (Mouse.IsOver(mc))
                    {
                        DrawHighlight(mc);
                    }
                    CheckboxLabeled(mc, "EnableFAForMale".Translate(), ref data.EnableForMale);
                    mc.y += rect0.height + 5f;
                    if (Mouse.IsOver(mc))
                    {
                        DrawHighlight(mc);
                    }
                    CheckboxLabeled(mc, "OffsetForMale".Translate(), ref data.OffsetForMale, !data.EnableForMale);
                    mc.y += rect0.height + 5f;
                    if (Mouse.IsOver(mc))
                    {
                        DrawHighlight(mc);
                    }
                    CheckboxLabeled(mc, "SizeForMale".Translate(), ref data.SizeForMale, !data.EnableForMale);
                    if (Mouse.IsOver(rc))
                    {
                        DrawHighlight(rc);
                    }
                    CheckboxLabeled(rc, "EnableFAForNone".Translate(), ref data.EnableForNone);
                    rc.y += rect0.height + 5f;
                    if (Mouse.IsOver(rc))
                    {
                        DrawHighlight(rc);
                    }
                    CheckboxLabeled(rc, "OffsetForNone".Translate(), ref data.OffsetForNone, !data.EnableForNone);
                    rc.y += rect0.height + 5f;
                    if (Mouse.IsOver(rc))
                    {
                        DrawHighlight(rc);
                    }
                    CheckboxLabeled(rc, "SizeForNone".Translate(), ref data.SizeForNone, !data.EnableForNone);
                    rect0.y = lc.y;
                    rect0.height *= 5;
                    rect0.height += 20f;
                    Listing_Standard ls = new Listing_Standard();
                    ls.Begin(rect0.LeftPart(0.07f));
                    ls.Label("South".Translate(), foldHeight);
                    ls.GapLine(9f);
                    ls.Label("North".Translate(), foldHeight);
                    ls.GapLine(9f);
                    ls.Label("East".Translate(), foldHeight);
                    ls.GapLine(9f);
                    ls.Label("West".Translate(), foldHeight);
                    ls.GapLine(9f);
                    ls.Label("Size".Translate(), foldHeight);
                    ls.End();

                    if (!BaseOffsetAndScale)
                    {
                        ls.Begin(rect0.RightPart(0.92f).LeftPart(0.49f));
                        ls.FloatAdjust("x:" + data.OffsetSouth.x.ToString("F2"), ref data.OffsetSouth.x, 0.01f, -1, 1, 2, foldHeight, TextAnchor.MiddleLeft);
                        ls.FloatAdjust("x:" + data.OffsetNorth.x.ToString("F2"), ref data.OffsetNorth.x, 0.01f, -1, 1, 2, foldHeight, TextAnchor.MiddleLeft);
                        ls.FloatAdjust("x:" + data.OffsetEast.x.ToString("F2"), ref data.OffsetEast.x, 0.01f, -1, 1, 2, foldHeight, TextAnchor.MiddleLeft);
                        ls.FloatAdjust("x:" + data.OffsetWest.x.ToString("F2"), ref data.OffsetWest.x, 0.01f, -1, 1, 2, foldHeight, TextAnchor.MiddleLeft);
                        ls.FloatAdjust("heigh".Translate() + data.Size.x.ToString("F2"), ref data.Size.x, 0.01f, 0.5f, 2, 2, foldHeight, TextAnchor.MiddleLeft);
                        ls.End();
                        ls.Begin(rect0.RightPart(0.92f).RightPart(0.49f));
                        ls.FloatAdjust("y:" + data.OffsetSouth.y.ToString("F2"), ref data.OffsetSouth.y, 0.01f, -1, 1, 2, foldHeight, TextAnchor.MiddleLeft);
                        ls.FloatAdjust("y:" + data.OffsetNorth.y.ToString("F2"), ref data.OffsetNorth.y, 0.01f, -1, 1, 2, foldHeight, TextAnchor.MiddleLeft);
                        ls.FloatAdjust("y:" + data.OffsetEast.y.ToString("F2"), ref data.OffsetEast.y, 0.01f, -1, 1, 2, foldHeight, TextAnchor.MiddleLeft);
                        ls.FloatAdjust("y:" + data.OffsetWest.y.ToString("F2"), ref data.OffsetWest.y, 0.01f, -1, 1, 2, foldHeight, TextAnchor.MiddleLeft);
                        ls.FloatAdjust("width".Translate() + data.Size.y.ToString("F2"), ref data.Size.y, 0.01f, 0.5f, 2, 2, foldHeight, TextAnchor.MiddleLeft);
                        ls.End();
                    }
                    else
                    {
                        ls.Begin(rect0.RightPart(0.92f).LeftPart(0.49f));
                        ls.FloatAdjust("x:" + data.BaseHairOffsetS.x.ToString("F2"), ref data.BaseHairOffsetS.x, 0.01f, -1, 1, 2, foldHeight, TextAnchor.MiddleLeft);
                        ls.FloatAdjust("x:" + data.BaseHairOffsetN.x.ToString("F2"), ref data.BaseHairOffsetN.x, 0.01f, -1, 1, 2, foldHeight, TextAnchor.MiddleLeft);
                        ls.FloatAdjust("x:" + data.BaseHairOffsetE.x.ToString("F2"), ref data.BaseHairOffsetE.x, 0.01f, -1, 1, 2, foldHeight, TextAnchor.MiddleLeft);
                        ls.FloatAdjust("x:" + data.BaseHairOffsetW.x.ToString("F2"), ref data.BaseHairOffsetW.x, 0.01f, -1, 1, 2, foldHeight, TextAnchor.MiddleLeft);
                        ls.FloatAdjust("heigh".Translate() + data.BaseSize.x.ToString("F2"), ref data.BaseSize.x, 0.01f, 0.5f, 2, 2, foldHeight, TextAnchor.MiddleLeft);
                        ls.End();
                        ls.Begin(rect0.RightPart(0.92f).RightPart(0.49f));
                        ls.FloatAdjust("y:" + data.BaseHairOffsetS.y.ToString("F2"), ref data.BaseHairOffsetS.y, 0.01f, -1, 1, 2, foldHeight, TextAnchor.MiddleLeft);
                        ls.FloatAdjust("y:" + data.BaseHairOffsetN.y.ToString("F2"), ref data.BaseHairOffsetN.y, 0.01f, -1, 1, 2, foldHeight, TextAnchor.MiddleLeft);
                        ls.FloatAdjust("y:" + data.BaseHairOffsetE.y.ToString("F2"), ref data.BaseHairOffsetE.y, 0.01f, -1, 1, 2, foldHeight, TextAnchor.MiddleLeft);
                        ls.FloatAdjust("y:" + data.BaseHairOffsetW.y.ToString("F2"), ref data.BaseHairOffsetW.y, 0.01f, -1, 1, 2, foldHeight, TextAnchor.MiddleLeft);
                        ls.FloatAdjust("width".Translate() + data.BaseSize.y.ToString("F2"), ref data.BaseSize.y, 0.01f, 0.5f, 2, 2, foldHeight, TextAnchor.MiddleLeft);
                        ls.End();
                    }
                }
                return false;
            }
        }

    }

    public class HSMSetting : ModSettings
    {
        public static bool FirstLoad = true;
        public static bool EnableForNotColonist = true;
        public static Dictionary<string, Dictionary<string, HSMData>> datas = new Dictionary<string, Dictionary<string, HSMData>>();
        public override void ExposeData()
        {
            HSUtility.Look(ref datas, "datas");
            Scribe_Values.Look(ref FirstLoad, "FirstLoad", true);
            Scribe_Values.Look(ref EnableForNotColonist, "EnableForNotColonist", true);
        }
        public void InitializeData()
        {
            if (!HSMCache.FARaceList.NullOrEmpty())
            {
                for (int a = 0; a < HSMCache.FARaceList.Count; a++)
                {
                    ThingDef race = HSMCache.FARaceList[a];
                    CheckSettingData(race);
                }
            }
        }

        public static void CheckSettingData(ThingDef raceDef)
        {
            if (datas == null)
            {
                datas = new Dictionary<string, Dictionary<string, HSMData>>();
            }
            bool reSet = false;
            if (datas.Count == 0 || !datas.Keys.Contains(raceDef.defName))
            {
                reSet = true;
            }
            else if (datas[raceDef.defName] == null || datas[raceDef.defName].Any(k => k.Value == null))
            {
                reSet = true;
            }
            if (reSet)
            {

                FaceAdjustmentDef adjustDef = DefDatabase<FaceAdjustmentDef>.AllDefs.Where(a => a.RaceName == raceDef.defName).FirstOrDefault();
                if (raceDef.race != null)
                {
                    if (raceDef.race.lifeStageAges.NullOrEmpty())
                    {
                        HSMData data = new HSMData();
                        if (adjustDef != null)
                        {
                            data.Size = adjustDef.Size;
                            data.OffsetSouth = adjustDef.OffsetSouth;
                            data.OffsetWest = adjustDef.OffsetWest;
                            data.OffsetEast = adjustDef.OffsetEast;
                            data.OffsetNorth = adjustDef.OffsetNorth;
                        }
                        datas.SetOrAdd(raceDef.defName, new Dictionary<string, HSMData>() { { SettingsUnit.noAge, data } });
                    }
                    else
                    {
                        Dictionary<string, HSMData> keyValuePairs = new Dictionary<string, HSMData>();
                        for (int i = 0; i < raceDef.race.lifeStageAges.Count; i++)
                        {
                            LifeStageAge age = raceDef.race.lifeStageAges[i];
                            HSMData data = new HSMData();
                            if (adjustDef != null)
                            {
                                FaceAdjustmentDef.AgeBasedParam ageBased = null;
                                for (int j = 0; j < adjustDef.AgeBasedParams.Count; j++)
                                    if (adjustDef.AgeBasedParams[j].Age == age.minAge)
                                    {
                                        ageBased = adjustDef.AgeBasedParams[j];
                                    }

                                if (ageBased == null)
                                {
                                    //Log.Warning("No AgeBasedParam for " + raceDef.defName + " " + age.minAge);
                                    data.Size = adjustDef.Size;
                                    data.OffsetSouth = adjustDef.OffsetSouth;
                                    data.OffsetWest = adjustDef.OffsetWest;
                                    data.OffsetEast = adjustDef.OffsetEast;
                                    data.OffsetNorth = adjustDef.OffsetNorth;
                                }
                                else
                                {
                                    data.Size = ageBased.Size;
                                    data.OffsetSouth = ageBased.OffsetSouth;
                                    data.OffsetWest = ageBased.OffsetWest;
                                    data.OffsetEast = ageBased.OffsetEast;
                                    data.OffsetNorth = ageBased.OffsetNorth;
                                }
                            }
                            keyValuePairs.SetOrAdd(age.def.defName, data);
                        }
                        datas.SetOrAdd(raceDef.defName, keyValuePairs);
                    }
                }
            }
        }

        public class HSMData : IExposable
        {
            public Vector2 Size = new Vector2(1.5f, 1.5f);
            public Vector2 OneAndHalf = new Vector2(1.5f, 1.5f);
            public Vector2 OffsetNorth = Vector2.zero;
            public Vector2 OffsetSouth = Vector2.zero;
            public Vector2 OffsetEast = Vector2.zero;
            public Vector2 OffsetWest = Vector2.zero;
            public Vector2 BaseSize = Vector2.one;
            public Vector2 BaseHairOffsetN = Vector2.zero;
            public Vector2 BaseHairOffsetS = Vector2.zero;
            public Vector2 BaseHairOffsetE = Vector2.zero;
            public Vector2 BaseHairOffsetW = Vector2.zero;
            public bool DefWriteMode = false;
            public bool EnableForFemale = true;
            public bool OffsetForFemale = true;
            public bool EnableForMale = true;
            public bool OffsetForMale = true;
            public bool SizeForFemale = true;
            public bool SizeForMale = true;
            public bool EnableForNone = true;
            public bool OffsetForNone = true;
            public bool SizeForNone = true;
            public List<string> NoFaXenos = new List<string>();
            public void ExposeData()
            {
                HSUtility.Look(ref Size, "Size", 2, defaultValue: OneAndHalf);
                HSUtility.Look(ref OffsetEast, "OffsetEast", 4);
                HSUtility.Look(ref OffsetSouth, "OffsetSouth", 4);
                HSUtility.Look(ref OffsetNorth, "OffsetNorth", 4);
                HSUtility.Look(ref OffsetWest, "OffsetWest", 4);
                HSUtility.Look(ref BaseSize, "BaseHairSize", 2, defaultValue: Vector2.one);
                HSUtility.Look(ref BaseHairOffsetN, "BaseHairOffsetN", 4);
                HSUtility.Look(ref BaseHairOffsetS, "BaseHairOffsetS", 4);
                HSUtility.Look(ref BaseHairOffsetE, "BaseHairOffsetE", 4);
                HSUtility.Look(ref BaseHairOffsetW, "BaseHairOffsetW", 4);
                Scribe_Values.Look(ref EnableForFemale, "EnableFAForFemale", true);
                Scribe_Values.Look(ref OffsetForFemale, "OffsetForFemale", true);
                Scribe_Values.Look(ref EnableForMale, "EnableFAForMale", true);
                Scribe_Values.Look(ref OffsetForMale, "OffsetForMale", true);
                Scribe_Values.Look(ref EnableForNone, "EnableFAForNone", true);
                Scribe_Values.Look(ref OffsetForNone, "OffsetForNone", true);
                Scribe_Values.Look(ref SizeForFemale, "SizeForFemale", true);
                Scribe_Values.Look(ref SizeForMale, "SizeForMale", true);
                Scribe_Values.Look(ref SizeForNone, "SizeForNone", true);
                Scribe_Values.Look(ref DefWriteMode, "DefWriteMode", false);
                Scribe_Collections.Look(ref NoFaXenos, "NoFaXenos", LookMode.Value);
            }

            public bool CanDrawXenoFA(Pawn pawn)
            {
                if (CanDrawFA(pawn))
                {
                    if (NoFaXenos.NullOrEmpty())
                    {
                        return true;
                    }
                    else
                    {
                        if (pawn.genes == null || pawn.genes.Xenotype == null)
                        {
                            return true;
                        }
                        else
                        {
                            return !NoFaXenos.Contains(pawn.genes.Xenotype.defName);
                        }

                    }
                }
                else
                {
                    return false;
                }
            }

            public bool CanDrawFA(Pawn pawn, bool enableOffsetOrSize = false, bool IsOffset = true)
            {
                if (pawn == null)
                {
                    return false;
                }
                if (!EnableForNotColonist && !pawn.IsColonist)
                {
                    return false;
                }
                if (pawn.gender == Gender.Male && EnableForMale)
                {
                    if (!enableOffsetOrSize)
                    {
                        return true;
                    }
                    else
                    {
                        return IsOffset ? OffsetForMale : SizeForMale;
                    }
                }
                else
                if (pawn.gender == Gender.Female && EnableForFemale)
                {
                    if (!enableOffsetOrSize)
                    {
                        return true;
                    }
                    else
                    {
                        return IsOffset ? OffsetForFemale : SizeForFemale;
                    }
                }
                else
                if (pawn.gender == Gender.None && EnableForNone)
                {
                    if (!enableOffsetOrSize)
                    {
                        return true;
                    }
                    else
                    {
                        return IsOffset ? OffsetForNone : SizeForNone;
                    }
                }
                return false;
            }

            public Vector2 GetOffset(int face, bool addBase = false)
            {
                if (addBase)
                {
                    switch (face)
                    {
                        case 0: return OffsetNorth + BaseHairOffsetN;
                        case 1: return OffsetEast + BaseHairOffsetE;
                        case 2: return OffsetSouth + BaseHairOffsetS;
                        case 3: return OffsetWest + BaseHairOffsetW;
                    }
                }
                else
                {
                    switch (face)
                    {
                        case 0: return OffsetNorth;
                        case 1: return OffsetEast;
                        case 2: return OffsetSouth;
                        case 3: return OffsetWest;
                    }
                }
                return Vector2.zero;
            }
        }
    }
    [StaticConstructorOnStartup]
    public static class HSMCache
    {
        public static List<ThingDef> FARaceList = new List<ThingDef>();
        public static GUIStyle TextStyle;
        internal static readonly Texture2D questionMark = ContentFinder<Texture2D>.Get("UI/Overlays/QuestionMark", true);
        static HSMCache()
        {

            FARaceList = DefDatabase<ThingDef>.AllDefs
                .Where(x => x.category.ToString() == "Pawn" && x.HasComp(typeof(FacialAnimation.HeadControllerComp)) && x.defName != null && x.race != null).ToList();
            if (HSMSetting.FirstLoad)
            {
                HSMSetting.FirstLoad = false;
                if (!HSMSetting.datas.NullOrEmpty())
                {
                    HSMSetting.datas.Clear();
                }
            }
            try
            {
                HeadSetMod.setting.ExposeData();
            }
            finally
            {
                HeadSetMod.setting.InitializeData();
                HeadSetMod.setting.Write();
            }
            CreateTextStyle();
        }
        internal static void CreateTextStyle()
        {
            if (TextStyle == null)
            {
                TextStyle = new GUIStyle(Text.CurTextAreaReadOnlyStyle);
                TextStyle.alignment = TextAnchor.MiddleCenter;
            }
        }
    }
}