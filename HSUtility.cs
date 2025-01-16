using RimWorld;
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace HeadSetForFA
{
    public static class HSUtility
    {
        public static void FloatAdjust(this Listing_Standard ls, string label, ref float val, float countChange, float min, float max, int keepCount = 0,float maxHeight = 30f,TextAnchor textAnchor = TextAnchor.MiddleCenter)
        {
            Rect rect = ls.GetRect(maxHeight);
            Rect rect1 = rect.LeftPart(0.2f);
            TextAnchor anchor = Text.Anchor;
            Text.Anchor = textAnchor;
            Widgets.Label(rect1, label);
            Text.Anchor = anchor;
            rect.x += rect1.width + 2f;
            rect.width -= rect1.width + 2f;
            if (Widgets.ButtonText(rect.LeftPart(0.15f), "-" + countChange))
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
            Widgets.TextFieldNumeric(new Rect(rect.x + 0.15f * rect.width + 2f, rect.y, rect.width * 0.70f - 4f, rect.height), ref val, ref x, min, max);
            if (Widgets.ButtonText(rect.RightPart(0.15f), "+" + countChange))
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
            ls.Gap(5f);
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
        internal static void Look(ref Dictionary<string, Dictionary<string, HSMSetting.HSMData>> dict, string label)
        {
            if (Scribe.EnterNode(label))
            {
                try
                {
                    if (Scribe.mode == LoadSaveMode.Saving && dict == null)
                    {
                        if (Scribe.mode == LoadSaveMode.Saving)
                        {
                            if (dict != null)
                            {
                                foreach (string key in dict.Keys)
                                {
                                    Dictionary<string, HSMSetting.HSMData> valuePairs = dict[key];
                                    if (valuePairs == null)
                                    {
                                        valuePairs = new Dictionary<string, HSMSetting.HSMData>();
                                    }
                                    foreach (string key0 in valuePairs.Keys)
                                    {
                                        HSMSetting.HSMData value = valuePairs[key0];
                                        Scribe_Deep.Look(ref value, false, key0);
                                    }
                                }
                                return;
                            }
                            Scribe.saver.WriteAttribute("IsNull", "True");
                        }
                    }
                    else
                    {
                        if (Scribe.mode == LoadSaveMode.LoadingVars)
                        {
                            XmlNode curXmlParent = Scribe.loader.curXmlParent;
                            XmlAttribute xmlAttribute = curXmlParent.Attributes["IsNull"];
                            if (xmlAttribute != null && xmlAttribute.Value.Equals("true", StringComparison.InvariantCultureIgnoreCase))
                            {
                                dict = null;
                            }
                            else
                            {
                                {
                                    Dictionary<string, Dictionary<string, HSMSetting.HSMData>> d = new Dictionary<string, Dictionary<string, HSMSetting.HSMData>>();
                                    foreach (XmlNode childNode in curXmlParent.ChildNodes)
                                    {
                                        Dictionary<string, HSMSetting.HSMData> item = new Dictionary<string, HSMSetting.HSMData>();
                                        foreach (XmlNode childNode2 in childNode.ChildNodes)
                                        {
                                            HSMSetting.HSMData data = new HSMSetting.HSMData();
                                            Scribe_Deep.Look(ref data, false, childNode2.Name);
                                            item.SetOrAdd(childNode2.Name, data);
                                        }
                                        d.SetOrAdd(childNode.Name, item);
                                    }
                                }
                            }
                        }
                        return;
                    }
                    return;
                }
                finally
                {
                    Scribe.ExitNode();
                }
            }

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                dict = null;
            }
        }
    }
}