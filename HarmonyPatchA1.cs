using FacialAnimation;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using Verse;


namespace HeadSetForFA
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatchA1
    {
        static HarmonyPatchA1()
        {
            Harmony harmony = new Harmony("ABFAA.HeadSetForFA");
            try
            {
                MethodInfo method0 = AccessTools.Method(typeof(PawnRenderNodeWorker), nameof(PawnRenderNodeWorker.OffsetFor));
                if (method0 != null)
                {
                    harmony.Patch(method0, transpiler: new HarmonyMethod(typeof(HarmonyPatchA1).GetMethod(nameof(Transpiler_OffsetFor))));
                }
            }
            catch (Exception e)
            {
                Log.Message("HeadSetForFA OffsetFix Harmony Patching failed");
            }
            try
            {
                MethodInfo method0 = AccessTools.Method(typeof(PawnRenderTree), nameof(PawnRenderTree.TryGetMatrix));
                if (method0 != null)
                {
                    harmony.Patch(method0, postfix: new HarmonyMethod(typeof(HarmonyPatchA1).GetMethod(nameof(Postfix_TryGetMatrix))));
                }
            }
            catch (Exception e)
            {
                Log.Message("HeadSetForFA ScaleFix Harmony Patching failed");
            }
            try
            {
                MethodInfo method0 = AccessTools.Method(typeof(GraphicHelper), nameof(GraphicHelper.GetHeadOffset));
                if (method0 != null)
                {
                    harmony.Patch(method0, prefix: new HarmonyMethod(typeof(HarmonyPatchA1).GetMethod(nameof(Prefix_GetHeadOffset))));
                }
            }
            catch (Exception e)
            {
                Log.Message("HeadSetForFA SettingPatchOffset Harmony Patching failed");
            }
            try
            {
                MethodInfo method0 = AccessTools.Method(typeof(GraphicHelper), nameof(GraphicHelper.GetHeadMeshSet));
                if (method0 != null)
                {
                    harmony.Patch(method0, prefix: new HarmonyMethod(typeof(HarmonyPatchA1).GetMethod(nameof(Prefix_GetHeadMeshSet))));
                }
            }
            catch (Exception e)
            {
                Log.Message("HeadSetForFA SettingPatchScale Harmony Patching failed");
            }
            try
            {
                Type type = AccessTools.TypeByName("FacialAnimation.DrawFaceGraphicsComp");
                if (type != null)
                {
                    MethodInfo method0 = AccessTools.Method(type, "CheckEnableDrawing");
                    if (method0 != null)
                    {
                        harmony.Patch(method0, postfix: new HarmonyMethod(typeof(HarmonyPatchA1).GetMethod(nameof(Postfix_CheckEnableDrawing))));
                    }
                }
            }
            catch (Exception e)
            {
                Log.Message("HeadSetForFA SettingPatchDraw Harmony Patching failed");
            }
        }

        public static void Postfix_CheckEnableDrawing(Pawn pawn,ref bool __result)
        {
            if (pawn == null)
            {
                return;
            }
            if (__result)
            {
                __result = GetData(pawn).CanDrawXenoFA(pawn);
            }
        }

        public static bool Prefix_GetHeadMeshSet(Pawn pawn, ref Vector2 __result)
        {
            Vector2 vector2;
            HSMSetting.HSMData ageData = GetData(pawn);
            if (ageData == null)
            {
                return true;
            }
            if (!ageData.CanDrawFA(pawn, true, false))
            {
                vector2 = new Vector2(1.5f,1.5f);
            }
            else
            {
                vector2 = ageData.Size;
            }
            __result = pawn.story.headType.narrow ? new Vector2((float)(vector2.x / 1.086995905648755), vector2.y) : vector2;
            return false;
        }

        public static bool Prefix_GetHeadOffset(Pawn pawn, ref List<Vector3> __result)
        {
            HSMSetting.HSMData ageData = GetData(pawn);
            if (ageData == null || ageData.CanDrawFA(pawn, true))
            {
                return true;
            }
            List<Vector3> faceSet = new List<Vector3>
                    {
                        Vector3.zero,
                        Vector3.zero,
                        Vector3.zero,
                        Vector3.zero
                    };
            if (ageData.CanDrawFA(pawn, true))
            {
                faceSet[Rot4.North.AsInt] = new Vector3(ageData.OffsetNorth.x, 0, ageData.OffsetNorth.y);
                faceSet[Rot4.South.AsInt] = new Vector3(ageData.OffsetSouth.x, 0, ageData.OffsetSouth.y);
                faceSet[Rot4.East.AsInt] = new Vector3(ageData.OffsetEast.x, 0, ageData.OffsetEast.y);
                faceSet[Rot4.West.AsInt] = new Vector3(ageData.OffsetWest.x, 0, ageData.OffsetWest.y);
            }
            __result = faceSet;
            return false;
        }
        private static HSMSetting.HSMData GetData(Pawn pawn)
        {
            if (pawn == null)
            {
                return null;
            }
            if (HSMSetting.datas.TryGetValue(pawn.def.defName, out Dictionary<string, HSMSetting.HSMData> data))
            {
                if (pawn.def.race == null)
                {
                    return null;
                }
                if (pawn.def.race.lifeStageAges.NullOrEmpty())
                {
                    if (data.TryGetValue(SettingsUnit.noAge, out HSMSetting.HSMData ageData))
                    {
                        return ageData;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    if (data.TryGetValue(pawn.ageTracker.CurLifeStage.defName, out HSMSetting.HSMData ageData))
                    {
                        return ageData;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                return null;
            }
        }

        public static void Postfix_TryGetMatrix(PawnRenderNode node, PawnDrawParms parms, ref Matrix4x4 matrix)
        {
            if (node.Props.workerClass == typeof(PawnRenderNodeWorker_Apparel_Head) && node.children.NullOrEmpty())
            {
                if (FacialAnimationMod.Settings.IgnoreHairMeshParams)
                {
                    Vector3 offset = GraphicHelper.GetHeadOffset(parms.pawn)[parms.facing.AsInt];
                    if (offset != Vector3.zero)
                    {
                        matrix *= Matrix4x4.Translate(offset);
                    }
                    Vector2 FAScale = GraphicHelper.GetHeadMeshSet(parms.pawn) / 1.5f;
                    if (FAScale != Vector2.one)
                    {
                        matrix *= Matrix4x4.Scale(new Vector3(FAScale.x, 1f, FAScale.y));
                    }
                }
            }
        }

        public static IEnumerable<CodeInstruction> Transpiler_OffsetFor(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo patchinfo = AccessTools.Method(typeof(HarmonyPatchA1), nameof(GetOffset));
            List<CodeInstruction> codes = instructions.ToList();
            for (int a = 0; a < codes.Count; a++)
            {
                CodeInstruction code = codes[a];
                if (a == 0)
                {
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call, patchinfo);
                }
                else
                {
                    yield return code;
                }
            }

        }

        public static Vector3 GetOffset(object node, PawnDrawParms parms)
        {

            if (node.Isnt<PawnRenderNode_Hair>() && node.Isnt<PawnRenderNode_Beard>() && node.Isnt<PawnRenderNode_Tattoo_Head>())
            {

                return Vector3.zero;
            }
            else
            {
                Vector3 offset = Vector3.zero;
                if (FacialAnimationMod.Settings.IgnoreHairMeshParams)
                {
                    List<Vector3> FAoffsets = GraphicHelper.GetHeadOffset(parms.pawn);
                    if (!FAoffsets.NullOrEmpty())
                    {
                        offset += FAoffsets[parms.facing.AsInt];
                    }
                }
                return offset;
            }
        }

        /*public static IEnumerable<CodeInstruction> TranPrefixRenderPawnInternal(IEnumerable<CodeInstruction> codeInstructions, ILGenerator generator)
        {
            Label l1 = generator.DefineLabel();
            Label l2 = generator.DefineLabel();
            List<CodeInstruction> codes = codeInstructions.ToList();
            for (int a = 0; a < codes.Count; a++)
            {
                CodeInstruction code = codes[a];

            }
        }*/
    }

}