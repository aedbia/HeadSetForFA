using FacialAnimation;
using HarmonyLib;
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
            finally
            {
                Log.Message("HeadSetForFA OffsetFix Harmony Patching Finished");
            }
            try
            {
                MethodInfo method0 = AccessTools.Method(typeof(PawnRenderTree), nameof(PawnRenderTree.TryGetMatrix));
                if (method0 != null)
                {
                    harmony.Patch(method0, postfix: new HarmonyMethod(typeof(HarmonyPatchA1).GetMethod(nameof(Postfix_TryGetMatrix))));
                }
            }
            finally
            {
                Log.Message("HeadSetForFA ScaleFix Harmony Patching Finished");
            }
        }

        public static void Postfix_TryGetMatrix(PawnRenderNode node, PawnDrawParms parms, ref Matrix4x4 matrix)
        {
            if (node.Props.workerClass == typeof(PawnRenderNodeWorker_Apparel_Head) && node.children.NullOrEmpty())
            {
                node.Props.Worker.LayerFor();
                if (FacialAnimationMod.Settings.IgnoreHairMeshParams)
                {
                    Vector3 offset = GraphicHelper.GetHeadOffset(parms.pawn)[parms.facing.AsInt];
                    if (offset != Vector3.zero)
                    {
                        matrix *= Matrix4x4.Translate(offset);
                    }
                    Vector2 FAScale = GraphicHelper.GetHeadMeshSet(parms.pawn)/1.5f;
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