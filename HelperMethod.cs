using RimWorld;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace HeadSetForFA
{
    public static class HelperMethod
    {
        public static void FloatAdjust(this Listing_Standard ls, string label, ref float val, float countChange, float min, float max, int keepCount = 0)
        {
            Rect rect = ls.GetRect(24f);
            Rect rect1 = rect.LeftPart(0.1f);
            TextAnchor anchor = Verse.Text.Anchor;
            Verse.Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect1, label);
            Verse.Text.Anchor = anchor;
            rect.x += rect1.width + 2f;
            rect.width -= rect1.width + 2f;
            if (Widgets.ButtonText(rect.LeftPart(0.08f), "-" + countChange))
            {
                SoundDefOf.DragSlider.PlayOneShotOnCamera();
                val -= countChange * GenUI.CurrentAdjustmentMultiplier();
                if (val < min)
                {
                    val = min;
                }
                else if (val > max)
                {
                    val = max;
                }
            }
            string x = val.ToString("f" + keepCount.ToString());
            Widgets.TextFieldNumeric(new Rect(rect.x + 0.08f * rect.width + 2f, rect.y, rect.width * 0.84f - 4f, rect.height), ref val, ref x, min, max);
            if (Widgets.ButtonText(rect.RightPart(0.08f), "+" + countChange))
            {
                SoundDefOf.DragSlider.PlayOneShotOnCamera();
                val += countChange * GenUI.CurrentAdjustmentMultiplier();
                if (val < min)
                {
                    val = min;
                }
                else if (val > max)
                {
                    val = max;
                }
            }
            ls.Gap(4f);
        }



        internal static void Look(ref Vector2 vector2, string label, int keepCount = 0, Vector2 defaultValue = default(Vector2))
        {
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                if ((vector2 != null || defaultValue == null) && (vector2 == null || vector2.Equals(defaultValue)))
                {
                    return;
                }
                if (vector2 == null)
                {
                    if (Scribe.EnterNode(label))
                    {
                        try
                        {
                            Scribe.saver.WriteAttribute("IsNull", "True");
                        }
                        finally
                        {
                            Scribe.ExitNode();
                        }
                    }
                }
                else
                {
                    string keepCountStr = keepCount.ToString();
                    string format1 = "({0:F" + keepCountStr + "}, {1:F" + keepCountStr + "})";
                    string a = string.Format(format1, new object[2] { vector2.x, vector2.y });
                    Scribe.saver.WriteElement(label, a);
                }
            }
            else if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                vector2 = ScribeExtractor.ValueFromNode(Scribe.loader.curXmlParent[label], defaultValue);
            }
        }
    }
}