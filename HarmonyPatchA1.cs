using AlienRace;
using FacialAnimation;
using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using UnityEngine;
using Verse;


namespace HeadSetForFA
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatchA1
    {
        public static FieldInfo Size1 = AccessTools.Field(typeof(TempData), nameof(TempData.Size));
        public static MethodInfo methodInfo = AccessTools.Method(typeof(HarmonyPatchA1), nameof(GetInternalData));
        public static MethodInfo methodInfo1 = AccessTools.Method(typeof(HarmonyPatchA1), nameof(getOffset_1));
        static HarmonyPatchA1()
        {
            Harmony harmony = new Harmony("ABFAA.HeadSetForFA");

            harmony.Patch(AccessTools.Method(typeof(GraphicHelper), nameof(GraphicHelper.GetHeadMeshSet)),
                     transpiler: new HarmonyMethod(typeof(HarmonyPatchA1), nameof(ChangeGetHeadMeshSet)));

            harmony.Patch(AccessTools.Method(typeof(GraphicHelper), nameof(GraphicHelper.GetHeadOffset)),
                transpiler: new HarmonyMethod(typeof(HarmonyPatchA1), nameof(ChangeGetHeadOffset)));
            MethodInfo methodInfo2 = AccessTools.Method(AccessTools.TypeByName("FacialAnimation.HarmonyPatches"), "PrefixRenderPawnInternal");
            if (methodInfo2 != null)
            {
                harmony.Patch(methodInfo2, transpiler: new HarmonyMethod(typeof(HarmonyPatchA1), nameof(TranPrefixRenderPawnInternal)));
            }
            int a = LoadedModManager.RunningModsListForReading.FindIndex(x => x.PackageIdPlayerFacing == "velc.HatsDisplaySelection");
            int b = LoadedModManager.RunningModsListForReading.IndexOf(HeadSetMod.contentPack);
            int c = LoadedModManager.RunningModsListForReading.FindIndex(x => x.PackageIdPlayerFacing == "erdelf.HumanoidAlienRaces");
            if (a != -1 && a < b)
            {
                MethodInfo methodInfo = AccessTools.AllTypes().SelectMany((Type type) => type.GetMethods(AccessTools.all))
                    .FirstOrDefault((MethodInfo x) => x.Name == "DrawHatWithHair_14");
                if (methodInfo != null)
                {
                    harmony.Patch(methodInfo,
                    prefix: new HarmonyMethod(typeof(HarmonyPatchA1), nameof(PrefixDrawHeadHair)));
                }
            }
            else
            {
                harmony.Patch(AccessTools.Method(typeof(Verse.PawnRenderer), "DrawHeadHair"),
                                prefix: new HarmonyMethod(typeof(HeadSetForFA.HarmonyPatchA1), nameof(HeadSetForFA.HarmonyPatchA1.PrefixDrawHeadHair)));
            }
            if (c == -1)
            {
                return;
            }
            if (c != -1 && c < b)
            {
                MethodInfo methodInfo = AccessTools.AllTypes().SelectMany((Type type) => type.GetMethods(AccessTools.all))
                    .FirstOrDefault((MethodInfo x) => Regex.Matches(x.Name, "DrawAddon").Count == 2);
                MethodInfo methodInfo1 = AccessTools.AllTypes().SelectMany((Type type) => type.GetMethods(AccessTools.all))
                    .FirstOrDefault((MethodInfo x) => x.Name == "GetHumanlikeHairSetForPawnHelper");
                if (methodInfo != null)
                {
                    //Log.Warning(methodInfo.Name.LastIndexOf("DrawAddon").ToString());
                    harmony.Patch(methodInfo,
                    prefix: new HarmonyMethod(typeof(HarmonyPatchA1.AlienPatches).GetMethod("PrefixDrawAddon")),
                    postfix: new HarmonyMethod(typeof(HarmonyPatchA1.AlienPatches).GetMethod("PostfixDrawAddon")));
                }
                if (methodInfo1 != null)
                {
                    harmony.Patch(methodInfo1,
                    postfix: new HarmonyMethod(typeof(HarmonyPatchA1.AlienPatches).GetMethod("PostfixGetHumanlikeHairSetForPawnHelper")));
                }
            }
        }

        public static IEnumerable<CodeInstruction> TranPrefixRenderPawnInternal(IEnumerable<CodeInstruction> codeInstructions, ILGenerator generator)
        {
            Label l1 = generator.DefineLabel();
            Label l2 = generator.DefineLabel();
            List<CodeInstruction> codes = codeInstructions.ToList();
            for (int a = 0; a < codes.Count; a++)
            {
                CodeInstruction code = codes[a];
                if (a == 0)
                {
                    CodeInstruction code1 = codes[1];
                    yield return code;
                    yield return code1;
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HarmonyPatchA1), nameof(DrawOrNot)));
                    yield return new CodeInstruction(OpCodes.Brfalse_S, l1);
                    yield return new CodeInstruction(OpCodes.Ldnull);
                    yield return new CodeInstruction(OpCodes.Br_S, l2);
                    yield return new CodeInstruction(code)
                    {
                        labels = new List<Label>() { l1 }
                    };
                }
                else if (a == 3)
                {
                    code.labels = new List<Label> { l2 };
                    yield return code;
                }
                else
                {
                    yield return code;
                }
            }
        }
        public static bool DrawOrNot(Pawn pawn)
        {
            if (HSMCache.FARaceList != null && HSMCache.FARaceList.Contains(pawn.def))
            {
                HSMSetting.HSMData data = HSMCache.GetHSMData(pawn);
                if (!data.NoFaXenos.NullOrEmpty() && data.NoFaXenos.Contains(pawn.genes.Xenotype.defName))
                {
                    return true;
                }
                return false;
            }
            return false;
        }



        public static void PrefixDrawHeadHair(ref Vector3 headOffset, ref Rot4 bodyFacing, Pawn ___pawn)
        {
            if (___pawn == null)
            {
                return;
            }
            if (___pawn.def == null)
            {
                return;
            }
            if (!___pawn.def.HasComp(typeof(HeadControllerComp)))
            {
                return;
            }
            Vector3 offsetForHair = GetOffsetOfHair(bodyFacing, ___pawn);
            headOffset += offsetForHair;
        }
        public static Vector3 GetOffsetOfHair(Rot4 Facing, Pawn pawn)
        {
            List<Vector3> offset = GraphicHelper.GetHeadOffset(pawn);
            if (Facing == Rot4.North)
            {
                return offset[Rot4.North.AsInt];
            }
            else if (Facing == Rot4.South)
            {
                return offset[Rot4.South.AsInt];
            }
            else if (Facing == Rot4.West)
            {
                return offset[Rot4.West.AsInt];
            }
            else
            {
                return offset[Rot4.East.AsInt];
            }
        }

        public static IEnumerable<CodeInstruction> ChangeGetHeadMeshSet(IEnumerable<CodeInstruction> codeInstructions)
        {
            if (codeInstructions != null && codeInstructions.LongCount() > 102)
            {
                List<CodeInstruction> codes = codeInstructions.ToList<CodeInstruction>();
                int x = 22;
                for (int i = x; i < 80; i++)
                {

                    if (i == 79)
                    {
                        codes[x].labels.Clear();
                    }
                    else
                    {
                        codes.Remove(codes[x]);
                    }
                }
                codes[19] = new CodeInstruction(OpCodes.Ldarg_0);
                codes[20] = new CodeInstruction(OpCodes.Call, methodInfo);
                codes.Insert(21, new CodeInstruction(OpCodes.Ldfld, Size1));
                return codes.AsEnumerable();
            }
            return codeInstructions;
        }

        public static IEnumerable<CodeInstruction> ChangeGetHeadOffset(IEnumerable<CodeInstruction> codeInstructions)
        {
            if (codeInstructions != null && codeInstructions.LongCount() > 199)
            {
                List<CodeInstruction> codes = codeInstructions.ToList<CodeInstruction>();
                CodeInstruction code0 = codes[14];
                CodeInstruction code1 = codes[32];
                int x = 33;
                for (int i = x; i < 199; i++)
                {
                    codes.Remove(codes[x]);
                }
                codes.Insert(33, code0);
                codes.Insert(34, new CodeInstruction(OpCodes.Call, methodInfo1));
                codes.Insert(35, code1);
                return codes.AsEnumerable<CodeInstruction>();
            }
            return codeInstructions;
        }

        public static TempData GetInternalData(Pawn pawn)
        {
            if (pawn == null)
            {
                return new TempData();
            }
            HSMSetting.HSMData data = HSMCache.GetHSMData(pawn);
            if (!data.NoFaXenos.NullOrEmpty() && data.NoFaXenos.Contains(pawn.genes.Xenotype.defName))
            {
                return new TempData();
            }
            TempData tempData = new TempData();
            if ((!data.OffsetForFemale && pawn.gender == Gender.Female) || (!data.OffsetForMale && pawn.gender == Gender.Male) || (!data.OffsetForNone && pawn.gender == Gender.None))
            {
                tempData.OffsetSouth = data.DefaultOffsetSouth; tempData.OffsetWest = data.DefaultOffsetWest;
                tempData.OffsetEast = data.DefaultOffsetEast; tempData.OffsetNorth = data.DefaultOffsetNorth;
            }
            else
            {
                tempData.OffsetSouth = data.OffsetSouth; tempData.OffsetWest = data.OffsetWest;
                tempData.OffsetEast = data.OffsetEast; tempData.OffsetNorth = data.OffsetNorth;
            }
            bool SizeSet = (!data.SizeForFemale && pawn.gender == Gender.Female) || (!data.SizeForMale && pawn.gender == Gender.Male) || (!data.SizeForFemale && pawn.gender == Gender.Female);
            if (SizeSet)
            {
                tempData.Size = data.DefaultSize;
            }
            else
            {
                tempData.Size = data.Size;
            }
            FaceAdjustmentDef face = DefDatabase<FaceAdjustmentDef>.AllDefs.Where(cc => cc.RaceName == pawn.def.defName).FirstOrDefault();
            if ((pawn.ageTracker.CurLifeStage.headSizeFactor != null && data.HasFADef) || data.DefWriteMode)
            {
                tempData.Size /= (pawn.ageTracker.CurLifeStage.headSizeFactor ?? 1f);
            }
            return tempData;
        }
        public static List<Vector3> getOffset_1(Pawn pawn)
        {
            TempData data = GetInternalData(pawn);
            List<Vector3> list = new List<Vector3>
            {
                        Vector3.zero,
                        Vector3.zero,
                        Vector3.zero,
                        Vector3.zero
            };
            list[Rot4.North.AsInt] = new Vector3(data.OffsetNorth.x, 0f, data.OffsetNorth.y);
            list[Rot4.East.AsInt] = new Vector3(data.OffsetEast.x, 0f, data.OffsetEast.y);
            list[Rot4.South.AsInt] = new Vector3(data.OffsetSouth.x, 0f, data.OffsetSouth.y);
            list[Rot4.West.AsInt] = new Vector3(data.OffsetWest.x, 0f, data.OffsetWest.y);
            return list;
        }
        public class TempData
        {
            public Vector2 Size = new Vector2(1.5f, 1.5f);
            public Vector2 OffsetNorth = Vector2.zero;
            public Vector2 OffsetSouth = Vector2.zero;
            public Vector2 OffsetEast = Vector2.zero;
            public Vector2 OffsetWest = Vector2.zero;
        }

        public class AlienPatches
        {
            public static TempData GetInternalDataForAlien(Pawn pawn)
            {
                if (pawn == null)
                {
                    return new TempData()
                    {
                        OffsetSouth = Vector2.zero,
                        OffsetWest = Vector2.zero,
                        OffsetEast = Vector2.zero,
                        OffsetNorth = Vector2.zero,
                        Size = Vector2.one
                    };
                }
                HSMSetting.HSMData data = HSMCache.GetHSMData(pawn);
                if (!data.NoFaXenos.NullOrEmpty() && data.NoFaXenos.Contains(pawn.genes.Xenotype.defName))
                {
                    return new TempData()
                    {
                        OffsetSouth = Vector2.zero,
                        OffsetWest = Vector2.zero,
                        OffsetEast = Vector2.zero,
                        OffsetNorth = Vector2.zero,
                        Size = Vector2.one
                    };
                }
                TempData tempData = new TempData();
                if ((!data.OffsetForFemale && pawn.gender == Gender.Female) || (!data.OffsetForMale && pawn.gender == Gender.Male) || (!data.OffsetForNone && pawn.gender == Gender.None))
                {
                    tempData.OffsetSouth = Vector2.zero; tempData.OffsetWest = Vector2.zero;
                    tempData.OffsetEast = Vector2.zero; tempData.OffsetNorth = Vector2.zero;
                }
                else
                {
                    tempData.OffsetSouth = data.OffsetSouth - data.DefaultOffsetSouth; tempData.OffsetWest = data.OffsetWest - data.DefaultOffsetWest;
                    tempData.OffsetEast = data.OffsetEast - data.DefaultOffsetEast; tempData.OffsetNorth = data.OffsetNorth - data.DefaultOffsetNorth;
                }
                bool SizeSet = (!data.SizeForFemale && pawn.gender == Gender.Female) || (!data.SizeForMale && pawn.gender == Gender.Male) || (!data.SizeForFemale && pawn.gender == Gender.Female);
                if (SizeSet)
                {
                    tempData.Size = Vector2.one;
                }
                else
                {
                    tempData.Size = data.Size / data.DefaultSize;
                }
                if (!data.HasFADef && data.DefWriteMode)
                {
                    tempData.Size /= (pawn.ageTracker.CurLifeStage.headSizeFactor ?? 1f);
                }

                return tempData;
            }
            public static void PostfixGetHumanlikeHairSetForPawnHelper(ref Vector2 __result, ref Pawn pawn)
            {
                Vector2 size = GetInternalData(pawn).Size;
                __result = (size / 1.5f) * __result;
            }
            public static void PostfixDrawAddon(ref object __0, Pawn __state)
            {
                AlienPartGenerator.BodyAddon ba = __0 as AlienPartGenerator.BodyAddon;
                if (ba.bodyPart != BodyPartDefOf.Head && ba.defaultOffset != "Head" && !ba.alignWithHead)
                {
                    return;
                }
                if (__state == null)
                {
                    return;
                }
                Pawn pawn = __state;
                TempData tempData = GetInternalDataForAlien(pawn);
                ba.defaultOffsets.north.offset -= tempData.OffsetNorth;
                ba.defaultOffsets.south.offset -= tempData.OffsetSouth;
                ba.defaultOffsets.east.offset -= tempData.OffsetEast;
            }
            public static void PrefixDrawAddon(ref object __0, ref Vector2 drawSize, object __3, out Pawn __state)
            {
                AlienPartGenerator.BodyAddon ba = __0 as AlienPartGenerator.BodyAddon;
                if (ba.bodyPart != BodyPartDefOf.Head && ba.defaultOffset != "Head" && !ba.alignWithHead)
                {
                    __state = null;
                    return;
                }
                Pawn pawn = __3.GetType().GetField("pawn").GetValue(__3) as Pawn;
                if (pawn == null)
                {
                    __state = null;
                    return;
                }
                if (pawn.def == null)
                {
                    __state = null;
                    return;
                }
                if (!pawn.def.HasComp(typeof(HeadControllerComp)))
                {
                    __state = null;
                    return;
                }
                __state = pawn;
                TempData tempData = GetInternalDataForAlien(pawn);
                //Log.Warning(ba.defaultOffsets.north.offset + "before");
                ba.defaultOffsets.north.offset += tempData.OffsetNorth;
                //Log.Warning(ba.defaultOffsets.north.offset + "after");
                ba.defaultOffsets.south.offset += tempData.OffsetSouth;
                ba.defaultOffsets.east.offset += tempData.OffsetEast;
                drawSize.x *= tempData.Size.x;
                drawSize.y *= tempData.Size.y;
            }
        }

    }

}