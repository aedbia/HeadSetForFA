using FacialAnimation;
using RimWorld;
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
        public static ModContentPack contentPack;
        private static Vector2 po = Vector2.zero;
        private static Vector2 po1 = Vector2.zero;
        private static float LabelHeigh = 0;
        private static float AgeBarHeigh = 0;
        private static int LabelHeighLimit = 0;
        private static string raceName = "Race";
        private static string AgeStage = "AgeStage";
        private static bool XenoMode = false;
        private static float? showAge;
        private static GUIStyle TextStyle
        {
            get
            {
                GUIStyle x = Text.CurTextAreaReadOnlyStyle;
                x.alignment = TextAnchor.MiddleCenter;
                return x;
            }
        }
        public HeadSetMod(ModContentPack content) : base(content)
        {
            setting = GetSettings<HSMSetting>();
            contentPack = content;
        }

        public override void WriteSettings()
        {
            if (raceName != "Race" && AgeStage != "AgeStage")
            {
                if (AgeStage == "No_AgeStage")
                {
                    ResolveFaceGraphics(raceName, null);
                }
                else
                {
                    ResolveFaceGraphics(raceName, AgeStage);
                }
            }
            base.WriteSettings();
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Rect OutRect = inRect.LeftPart(0.12f).TopPart(0.95f);
            Rect viewRect = new Rect(0, 0, OutRect.width, LabelHeigh);
            Rect rect0 = new Rect(0, 0, OutRect.width, 0);
            Widgets.DrawBox(OutRect, 1, BaseContent.WhiteTex);
            Widgets.BeginScrollView(OutRect, ref po, viewRect, false);
            string s = "No_AgeStage";
            if (!HSMCache.FARaceList.NullOrEmpty())
            {
                if (raceName == "Race")
                {
                    raceName = HSMCache.FARaceList.FirstOrDefault().defName;
                }
                if (raceName != "Race" && AgeStage == "AgeStage")
                {
                    ThingDef def = HSMCache.FARaceList.FirstOrDefault(x => x.defName == raceName);
                    if (def.race != null)
                    {
                        bool ao = def.race.lifeStageAges.NullOrEmpty();
                        AgeStage = ao ? s : def.race.lifeStageAges.FirstOrDefault().def.defName;
                        if (!ao && showAge == null)
                        {
                            showAge = def.race.lifeStageAges.FirstOrDefault().minAge;
                        }
                        else if (ao)
                        {
                            showAge = null;
                        }
                    }

                }
                for (int x = 0; x < HSMCache.FARaceList.Count; x++)
                {
                    ThingDef a = HSMCache.FARaceList[x];
                    if (a.defName != null && a.label != null)
                    {
                        float b = Text.CalcHeight(a.label, rect0.width - 4f);
                        Rect loc0 = new Rect(rect0.x + 2f, rect0.y + 4f, rect0.width - 4f, b);
                        if (LabelHeighLimit < HSMCache.FARaceList.Count)
                        {
                            LabelHeigh += b + 8f;
                            LabelHeighLimit++;
                        }
                        rect0.height = b + 8f;
                        if (Widgets.RadioButtonLabeled(loc0, a.label, raceName == a.defName))
                        {
                            raceName = a.defName;
                            if (a.race != null)
                            {
                                bool ao = a.race.lifeStageAges.NullOrEmpty();
                                AgeStage = ao ? s : a.race.lifeStageAges.FirstOrDefault().def.defName;
                                if (!ao && showAge != a.race.lifeStageAges.FirstOrDefault().minAge)
                                {
                                    showAge = a.race.lifeStageAges.FirstOrDefault().minAge;
                                }
                                else if (ao)
                                {
                                    showAge = null;
                                }
                            }
                            XenoMode = false;
                        }
                        if (Mouse.IsOver(loc0))
                        {
                            DrawHighlight(loc0);
                        }
                        rect0.y += b + 8f;
                    }
                }
            }
            Widgets.EndScrollView();
            if (raceName != "Race" && AgeStage != "AgeStage")
            {
                setting.InitializeData(raceName, AgeStage);
            }
            Rect AgeBar = new Rect(inRect.x + OutRect.width, inRect.y, inRect.width - OutRect.width - 5f, AgeBarHeigh);

            if (raceName != "Race")
            {
                ThingDef a = DefDatabase<ThingDef>.GetNamedSilentFail(raceName);
                if (!a.race.lifeStageAges.NullOrEmpty())
                {
                    List<LifeStageAge> list1 = a.race.lifeStageAges.GroupBy(aa => aa.def.defName).Select(ab => ab.FirstOrDefault()).ToList();
                    if (!list1.NullOrEmpty())
                    {
                        List<TabRecord> list2 = new List<TabRecord>();
                        float LabelWidth0 = AgeBar.width / list1.Count;
                        for (int x = 0; x < list1.Count; x++)
                        {
                            LifeStageAge age = list1[x];
                            float heigh = Text.CalcHeight(age.def.label, LabelWidth0 - 8f);
                            list2.Add(new TabRecord(age.def.label, () =>
                            {
                                AgeStage = age.def.defName;
                                XenoMode = false;
                                showAge = age.minAge;
                            }, AgeStage == age.def.defName));
                            if (heigh + 8f > AgeBarHeigh)
                            {
                                AgeBarHeigh = heigh + 8f;
                            }
                        }
                        string xa = a.race.lifeStageAges.FirstOrDefault(x => x.def.defName == AgeStage) != null ? a.race.lifeStageAges.FirstOrDefault(x => x.def.defName == AgeStage).def.label : "";
                        GUI.Label(AgeBar.LeftPart(0.28f).RightPart(0.95f), xa + ": " + (showAge == null ? "" : showAge.ToStringSafe()) + "years_old".Translate());
                        AgeBar.y += AgeBar.height;
                        TabDrawer.DrawTabs(AgeBar.RightPart(0.70f), list2);
                        AgeBar.y -= AgeBar.height;
                    }
                    else
                    {
                        float x = Text.CalcHeight(s.Translate(), AgeBar.width - 8f);
                        AgeBarHeigh = x + 10f;
                        Rect TextLoc = new Rect(AgeBar.x + 4f, AgeBar.y + 5f, AgeBar.width - 8f, x);
                        GUI.Label(TextLoc, s.Translate(), TextStyle);

                        if (AgeStage != s)
                        {
                            AgeStage = s;
                            XenoMode = false;
                        }
                        if (showAge != null)
                        {
                            showAge = null;
                        }
                    }

                }
            }
            Rect aaa1 = new Rect(AgeBar.x + 2f, AgeBar.y + AgeBar.height, AgeBar.width - 2f, OutRect.height - AgeBar.height);
            DrawMenuSection(aaa1);
            if (AgeStage != "AgeStage")
            {
                string key = raceName + AgeStage;
                Listing_Standard ls = new Listing_Standard();
                ls.Begin(aaa1);
                Rect xx1 = ls.GetRect(20f);
                Widgets.CheckboxLabeled(xx1.LeftPart(0.32f), "OffsetForFemale".Translate(), ref setting.data[key].OffsetForFemale);
                Widgets.CheckboxLabeled(xx1.RightPart(0.66f).LeftHalf(), "OffsetForNone".Translate(), ref setting.data[key].OffsetForNone);
                Widgets.CheckboxLabeled(xx1.RightPart(0.32f), "OffsetForMale".Translate(), ref setting.data[key].OffsetForMale);
                Rect xx0 = ls.GetRect(20f);
                Widgets.CheckboxLabeled(xx0.LeftPart(0.32f), "SizeForFemale".Translate(), ref setting.data[key].SizeForFemale);
                Widgets.CheckboxLabeled(xx0.RightPart(0.66f).LeftHalf(), "SizeForNone".Translate(), ref setting.data[key].SizeForNone);
                Widgets.CheckboxLabeled(xx0.RightPart(0.32f), "SizeForMale".Translate(), ref setting.data[key].SizeForMale);
                Rect xx3 = ls.GetRect(20f);
                Widgets.CheckboxLabeled(xx3.LeftPart(0.49f), "DefWriteMode".Translate(), ref setting.data[key].DefWriteMode);
                if (Mouse.IsOver(xx3.LeftPart(0.49f)))
                {
                    TooltipHandler.TipRegion(xx3, "DefWriteMode_Tip".Translate());
                }
                if (ButtonText(xx3.RightPart(0.49f), "Xeno_Mode".Translate()))
                {
                    XenoMode = !XenoMode;
                }
                ls.GapLine(8f);
                if (!XenoMode)
                {
                    ls.Label("East".Translate() + ":");
                    ls.FloatAdjust("x:", ref setting.data[key].OffsetEast.x, 0.0001f, -2f, 2f, 4);
                    ls.FloatAdjust("y:", ref setting.data[key].OffsetEast.y, 0.0001f, -2f, 2f, 4);
                    ls.GapLine(8f);
                    ls.Label("South".Translate() + ":");
                    ls.FloatAdjust("x:", ref setting.data[key].OffsetSouth.x, 0.0001f, -2f, 2f, 4);
                    ls.FloatAdjust("y:", ref setting.data[key].OffsetSouth.y, 0.0001f, -2f, 2f, 4);
                    ls.GapLine(8f);
                    ls.Label("West".Translate() + ":");
                    ls.FloatAdjust("x:", ref setting.data[key].OffsetWest.x, 0.0001f, -2f, 2f, 4);
                    ls.FloatAdjust("y:", ref setting.data[key].OffsetWest.y, 0.0001f, -2f, 2f, 4);
                    ls.GapLine(8f);
                    ls.Label("North".Translate() + ":");
                    ls.FloatAdjust("x:", ref setting.data[key].OffsetNorth.x, 0.0001f, -2f, 2f, 4);
                    ls.FloatAdjust("y:", ref setting.data[key].OffsetNorth.y, 0.0001f, -2f, 2f, 4);
                    ls.GapLine(8f);
                    ls.Label("Size".Translate() + ":");
                    ls.FloatAdjust("heigh".Translate(), ref setting.data[key].Size.x, 0.01f, 0f, 2f, 2);
                    ls.FloatAdjust("width".Translate(), ref setting.data[key].Size.y, 0.01f, 0f, 2f, 2);
                    ls.GapLine(8f);
                    ls.Gap(4f);
                    Rect xx2 = ls.GetRect(24f);
                    if (ButtonText(xx2.RightPart(0.33f), "Reset_This".Translate()))
                    {
                        setting.data.Remove(key);
                    };
                    if (ButtonText(xx2.LeftPart(0.66f).RightHalf(), "Reset_This_Race".Translate()))
                    {
                        List<string> list = setting.data.Keys.Where(key1 => key1.IndexOf(raceName) != -1).ToList();
                        if (!list.NullOrEmpty())
                        {
                            for (int index = 0; index < list.Count; index++)
                            {
                                setting.data.Remove(list[index]);
                            }
                            HSMCache.LoadAll();
                        }
                        ResolveFaceGraphics(raceName, null);
                    };
                    if (ButtonText(xx2.LeftPart(0.32f), "Reset_All".Translate()))
                    {
                        setting.data.Clear();
                        HSMCache.LoadAll();
                        ResolveFaceGraphics(null, null);
                    };
                }
                else
                {
                    List<XenotypeDef> xeno = DefDatabase<XenotypeDef>.AllDefs.Where(x => x.defName != null).ToList();
                    if (!xeno.NullOrEmpty())
                    {
                        Rect xx4 = ls.GetRect(430f);
                        Rect xx5 = new Rect(-5f, -5f, xx4.width - 30f, 30 * xeno.Count + 5f);
                        Rect xx6 = new Rect(0, 0, xx5.width, 30f);
                        BeginScrollView(xx4, ref po1, xx5, true);
                        foreach (XenotypeDef def in xeno)
                        {

                            Label(new Rect(xx6.x + 30f, xx6.y + 5f, 100f, 20f), def.label);
                            if (def.Icon != null)
                            {
                                GUI.DrawTexture(new Rect(xx6.x, xx6.y + 2f, 26f, 26f), def.Icon);
                            }
                            if (setting.data[key].NoFaXenos == null)
                            {
                                setting.data[key].NoFaXenos = new List<string>();
                            }
                            bool noDraw = !setting.data[key].NoFaXenos.NullOrEmpty() && setting.data[key].NoFaXenos.Contains(def.defName);
                            bool draw = setting.data[key].NoFaXenos.NullOrEmpty() || !setting.data[key].NoFaXenos.Contains(def.defName);
                            if (RadioButtonLabeled(xx6.RightHalf().RightPart(0.48f), "No_Draw_FAFace".Translate(), noDraw))
                            {
                                if (draw)
                                {
                                    setting.data[key].NoFaXenos.Add(def.defName);
                                }
                            }

                            if (RadioButtonLabeled(xx6.RightHalf().LeftPart(0.48f), "Draw_FAFace".Translate(), draw))
                            {
                                if (noDraw)
                                {
                                    setting.data[key].NoFaXenos.Remove(def.defName);
                                }
                            }
                            DrawLineHorizontal(xx6.x, xx6.y + xx6.height, xx6.width);
                            xx6.y += 30f;
                        }
                        EndScrollView();
                    }
                }
                ls.End();
            }
        }
        public static void ResolveFaceGraphics(string raceName, string lifeStage)
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
            if (raceName != null)
            {
                pawns = map.mapPawns.AllPawns.
                Where(x => x.def.defName == raceName && x.def.race != null
                && (lifeStage == null || x.def.race.lifeStageAges.Any(a => a.def.defName == lifeStage))).ToList();
            }
            else
            {
                pawns = map.mapPawns.AllPawns.
                Where(x => !HSMCache.FARaceList.NullOrEmpty() && HSMCache.FARaceList.Contains(x.def)).ToList();
            }
            if (pawns.NullOrEmpty())
            {
                return;
            }
            foreach (Pawn pawn in pawns)
            {
                HSMSetting.HSMData data = HSMCache.GetHSMData(pawn);
                if (data.NoFaXenos.NullOrEmpty() || !data.NoFaXenos.Contains(pawn.genes.Xenotype.defName))
                {
                    pawn.Drawer.renderer.graphics.ResolveAllGraphics();
                }
            }
        }
        public override string SettingsCategory()
        {
            return this.Content.Name;
        }
    }



    public class HSMSetting : ModSettings
    {
        public static bool FirstLoad = true;
        public Dictionary<string, HSMData> data = new Dictionary<string, HSMData>();
        public override void ExposeData()
        {
            Scribe_Collections.Look(ref data, "data", LookMode.Value, LookMode.Deep);
            Scribe_Values.Look(ref FirstLoad, "FirstLoad", true);
        }
        public void InitializeData(string raceName, string ageStage)
        {
            if (data == null)
            {
                data = new Dictionary<string, HSMData>();
            }
            string key = raceName + ageStage;
            if (!data.ContainsKey(key) || data[key] == null)
            {
                HSMData data1 = new HSMData();
                if (ageStage == "No_AgeStage")
                {
                    data1 = GetAdjustDef(raceName, null);
                }
                else
                {
                    LifeStageAge age = DefDatabase<ThingDef>.GetNamedSilentFail(raceName).race.lifeStageAges
                        .Where(x => x.def.defName == ageStage).FirstOrDefault();
                    data1 = GetAdjustDef(raceName, age.minAge);
                }
                data.SetOrAdd(key, data1);
            }
        }
        public HSMData GetAdjustDef(string raceName, float? minAge)
        {
            FaceAdjustmentDef face = DefDatabase<FaceAdjustmentDef>.AllDefs.Where(cc => cc.RaceName == raceName).FirstOrDefault();

            if (face == null)
            {
                return new HSMData();
            }
            else if (face.AgeBasedParams.NullOrEmpty())
            {
                return new HSMData()
                {
                    OffsetEast = face.OffsetEast,
                    OffsetSouth = face.OffsetSouth,
                    OffsetWest = face.OffsetWest,
                    OffsetNorth = face.OffsetNorth,
                    Size = face.Size,
                    DefaultOffsetEast = face.OffsetEast,
                    DefaultOffsetSouth = face.OffsetSouth,
                    DefaultOffsetWest = face.OffsetWest,
                    DefaultOffsetNorth = face.OffsetNorth,
                    DefaultSize = face.Size
                };
            }
            else
            {
                FaceAdjustmentDef.AgeBasedParam ageBased = face.AgeBasedParams.Where(x => x.Age == minAge).FirstOrDefault();
                if (ageBased == null)
                {
                    return new HSMData()
                    {
                        HasFADef = true,
                        OffsetEast = face.OffsetEast,
                        OffsetSouth = face.OffsetSouth,
                        OffsetWest = face.OffsetWest,
                        OffsetNorth = face.OffsetNorth,
                        Size = face.Size,
                        DefaultOffsetEast = face.OffsetEast,
                        DefaultOffsetSouth = face.OffsetSouth,
                        DefaultOffsetWest = face.OffsetWest,
                        DefaultOffsetNorth = face.OffsetNorth,
                        DefaultSize = face.Size
                    };
                }
                else
                {
                    return new HSMData()
                    {
                        HasFADef = true,
                        OffsetEast = ageBased.OffsetEast,
                        OffsetSouth = ageBased.OffsetSouth,
                        OffsetWest = ageBased.OffsetWest,
                        OffsetNorth = ageBased.OffsetNorth,
                        DefaultOffsetEast = ageBased.OffsetEast,
                        DefaultOffsetSouth = ageBased.OffsetSouth,
                        DefaultOffsetWest = ageBased.OffsetWest,
                        DefaultOffsetNorth = ageBased.OffsetNorth,
                        Size = ageBased.Size,
                        DefaultSize = ageBased.Size
                    };
                }
            }
        }
        public class HSMData : IExposable
        {
            public Vector2 Size = new Vector2(1.5f, 1.5f);
            public Vector2 OffsetNorth = Vector2.zero;
            public Vector2 OffsetSouth = Vector2.zero;
            public Vector2 OffsetEast = Vector2.zero;
            public Vector2 OffsetWest = Vector2.zero;
            public Vector2 DefaultSize = new Vector2(1.5f, 1.5f);
            public Vector2 DefaultOffsetNorth = Vector2.zero;
            public Vector2 DefaultOffsetSouth = Vector2.zero;
            public Vector2 DefaultOffsetEast = Vector2.zero;
            public Vector2 DefaultOffsetWest = Vector2.zero;
            public bool DefWriteMode = false;
            public bool OffsetForFemale = true;
            public bool OffsetForMale = true;
            public bool SizeForFemale = true;
            public bool SizeForMale = true;
            public bool OffsetForNone = true;
            public bool SizeForNone = true;
            public List<string> NoFaXenos = new List<string>();
            public bool HasFADef = false;
            public void ExposeData()
            {
                HelperMethod.Look(ref Size, "Size", 2);
                HelperMethod.Look(ref OffsetEast, "OffsetEast", 4);
                HelperMethod.Look(ref OffsetSouth, "OffsetSouth", 4);
                HelperMethod.Look(ref OffsetNorth, "OffsetNorth", 4);
                HelperMethod.Look(ref OffsetWest, "OffsetWest", 4);
                HelperMethod.Look(ref DefaultSize, "DefaultSize", 2);
                HelperMethod.Look(ref DefaultOffsetEast, "DefaultOffsetEast", 4);
                HelperMethod.Look(ref DefaultOffsetSouth, "DefaultOffsetSouth", 4);
                HelperMethod.Look(ref DefaultOffsetNorth, "DefaultOffsetNorth", 4);
                HelperMethod.Look(ref DefaultOffsetWest, "DefaultOffsetWest", 4);
                Scribe_Values.Look(ref OffsetForFemale, "OffsetForFemale", true);
                Scribe_Values.Look(ref OffsetForMale, "OffsetForMale", true);
                Scribe_Values.Look(ref OffsetForNone, "OffsetForNone", true);
                Scribe_Values.Look(ref SizeForFemale, "SizeForFemale", true);
                Scribe_Values.Look(ref SizeForMale, "SizeForMale", true);
                Scribe_Values.Look(ref SizeForNone, "SizeForNone", true);
                Scribe_Values.Look(ref DefWriteMode, "DefWriteMode", false);
                Scribe_Values.Look(ref HasFADef, "HasFADef", false);
                Scribe_Collections.Look(ref NoFaXenos, "NoFaXenos", LookMode.Value);
            }
        }
    }
    [StaticConstructorOnStartup]
    public static class HSMCache
    {
        public static List<ThingDef> FARaceList = new List<ThingDef>();
        static HSMCache()
        {
            FARaceList = DefDatabase<ThingDef>.AllDefs
                .Where(x => x.category.ToString() == "Pawn" && x.HasComp(typeof(FacialAnimation.HeadControllerComp)) && x.defName != null && x.race != null).ToList();
            if (HSMSetting.FirstLoad)
            {
                HSMSetting.FirstLoad = false;
                if (!HeadSetMod.setting.data.NullOrEmpty())
                {
                    HeadSetMod.setting.data.Clear();
                }
            }
            LoadAll();
            HeadSetMod.setting.Write();
        }
        public static void LoadAll()
        {
            if (FARaceList.NullOrEmpty())
            {
                return;
            }
            foreach (ThingDef def in FARaceList)
            {

                if (def.race.lifeStageAges.NullOrEmpty())
                {
                    HeadSetMod.setting.InitializeData(def.defName, "No_AgeStage");
                    HSMSetting.HSMData data = HeadSetMod.setting.GetAdjustDef(def.defName, null);
                    HeadSetMod.setting.data[def.defName + "No_AgeStage"].HasFADef = data.HasFADef;
                    HeadSetMod.setting.data[def.defName + "No_AgeStage"].DefaultOffsetEast = data.DefaultOffsetEast;
                    HeadSetMod.setting.data[def.defName + "No_AgeStage"].DefaultOffsetWest = data.DefaultOffsetWest;
                    HeadSetMod.setting.data[def.defName + "No_AgeStage"].DefaultOffsetNorth = data.DefaultOffsetNorth;
                    HeadSetMod.setting.data[def.defName + "No_AgeStage"].DefaultOffsetSouth = data.DefaultOffsetSouth;
                    HeadSetMod.setting.data[def.defName + "No_AgeStage"].DefaultSize = data.DefaultSize;
                }
                else
                {
                    List<LifeStageAge> ages = def.race.lifeStageAges.GroupBy(aa => aa.def.defName).Select(ab => ab.FirstOrDefault()).ToList();
                    foreach (LifeStageAge age in ages)
                    {
                        HeadSetMod.setting.InitializeData(def.defName, age.def.defName);
                        HSMSetting.HSMData data = HeadSetMod.setting.GetAdjustDef(def.defName, age.minAge);
                        HeadSetMod.setting.data[def.defName + age.def.defName].HasFADef = data.HasFADef;
                        HeadSetMod.setting.data[def.defName + age.def.defName].DefaultOffsetEast = data.DefaultOffsetEast;
                        HeadSetMod.setting.data[def.defName + age.def.defName].DefaultOffsetWest = data.DefaultOffsetWest;
                        HeadSetMod.setting.data[def.defName + age.def.defName].DefaultOffsetNorth = data.DefaultOffsetNorth;
                        HeadSetMod.setting.data[def.defName + age.def.defName].DefaultOffsetSouth = data.DefaultOffsetSouth;
                        HeadSetMod.setting.data[def.defName + age.def.defName].DefaultSize = data.DefaultSize;
                    }
                }
            }
        }
        public static HSMSetting.HSMData GetHSMData(Pawn pawn)
        {
            string race;
            string age;

            if (pawn.ageTracker.CurLifeStage != null)
            {
                race = pawn.def.defName; age = pawn.ageTracker.CurLifeStage.defName;
            }
            else
            {
                race = pawn.def.defName; age = "No_AgeStage";
            }
            string x = race + age;
            if (!HeadSetMod.setting.data.ContainsKey(x))
            {
                HeadSetMod.setting.InitializeData(race, age);
            }
            return HeadSetMod.setting.data[x];
        }
    }
}