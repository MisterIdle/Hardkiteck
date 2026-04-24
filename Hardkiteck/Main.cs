using HarmonyLib;
using Parkitect;
using Parkitect.UI;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace Hardkiteck
{
    public class Hardkiteck : AbstractMod, IModSettings
    {
        public override string getName() => "Hardkiteck";
        public override string getDescription() => "Hardcore mode for Parkitect";
        public override string getVersionNumber() => "1.0";
        public override string getIdentifier() => "misteridle.harditeck";
        public override bool isMultiplayerModeCompatible() => true;
        public override bool isRequiredByAllPlayersInMultiplayerMode() => true;

        public static float MoneyMultiplier;
        public static float HungerMultiplier;
        public static float ThirstMultiplier;
        public static float TirednessMultiplier;
        public static float NauseaMultiplier;
        public static float TerraformingMultiplier;
        public static float MaxGuestsMultiplier;
        public static float GoalMultiplier;
        public static bool ForceNoBlueprintGoal;

        static Hardkiteck()
        {
            LoadSettings();
            try
            {
                var harmony = new Harmony("misteridle.harditeck");
                harmony.PatchAll();
            }
            catch (Exception e)
            {
                Debug.LogError("[Hardcore Mod] ERROR: " + e.Message);
            }
        }

        public static void LoadSettings()
        {
            MoneyMultiplier = PlayerPrefs.GetFloat("Hardkiteck_Money", 0.5f);
            HungerMultiplier = PlayerPrefs.GetFloat("Hardkiteck_Hunger", 1.5f);
            ThirstMultiplier = PlayerPrefs.GetFloat("Hardkiteck_Thirst", 1.5f);
            TirednessMultiplier = PlayerPrefs.GetFloat("Hardkiteck_Tiredness", 1.5f);
            NauseaMultiplier = PlayerPrefs.GetFloat("Hardkiteck_Nausea", 1.5f);
            TerraformingMultiplier = PlayerPrefs.GetFloat("Hardkiteck_Terraforming", 2f);
            MaxGuestsMultiplier = PlayerPrefs.GetFloat("Hardkiteck_MaxGuests", 0.7f);
            GoalMultiplier = PlayerPrefs.GetFloat("Hardkiteck_Goal", 2f);
            ForceNoBlueprintGoal = PlayerPrefs.GetInt("Hardkiteck_NoBP", 1) == 1;
        }

        public void onSettingsOpened()
        {
            LoadSettings();
        }

        public void onSettingsClosed()
        {
            PlayerPrefs.SetFloat("Hardkiteck_Money", MoneyMultiplier);
            PlayerPrefs.SetFloat("Hardkiteck_Hunger", HungerMultiplier);
            PlayerPrefs.SetFloat("Hardkiteck_Thirst", ThirstMultiplier);
            PlayerPrefs.SetFloat("Hardkiteck_Tiredness", TirednessMultiplier);
            PlayerPrefs.SetFloat("Hardkiteck_Nausea", NauseaMultiplier);
            PlayerPrefs.SetFloat("Hardkiteck_Terraforming", TerraformingMultiplier);
            PlayerPrefs.SetFloat("Hardkiteck_MaxGuests", MaxGuestsMultiplier);
            PlayerPrefs.SetFloat("Hardkiteck_Goal", GoalMultiplier);
            PlayerPrefs.SetInt("Hardkiteck_NoBP", ForceNoBlueprintGoal ? 1 : 0);
            PlayerPrefs.Save();
        }

        public void onDrawSettingsUI()
        {
            MoneyMultiplier = DrawSlider("Starting Money", MoneyMultiplier, 0.1f, 1.0f);
            MaxGuestsMultiplier = DrawSlider("Guest Capacity", MaxGuestsMultiplier, 0.1f, 1.5f);
            TerraformingMultiplier = DrawSlider("Terraforming Cost", TerraformingMultiplier, 1.0f, 5.0f);
            GoalMultiplier = DrawSlider("Goal Multiplier", GoalMultiplier, 1.0f, 5.0f);

            GUILayout.Space(10);
            GUILayout.Label("Guest Needs (Survival):");
            HungerMultiplier = DrawSlider("Hunger", HungerMultiplier, 1.0f, 3.0f);
            ThirstMultiplier = DrawSlider("Thirst", ThirstMultiplier, 1.0f, 3.0f);
            TirednessMultiplier = DrawSlider("Tiredness", TirednessMultiplier, 1.0f, 3.0f);
            NauseaMultiplier = DrawSlider("Nausea", NauseaMultiplier, 1.0f, 3.0f);

            GUILayout.Space(10);
            ForceNoBlueprintGoal = GUILayout.Toggle(ForceNoBlueprintGoal, " Disable Blueprints (Mandatory Goal)");
        }

        private float DrawSlider(string label, float value, float min, float max)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label} (x{value:0.00})", GUILayout.Width(220));
            float newValue = GUILayout.HorizontalSlider(value, min, max);
            GUILayout.EndHorizontal();
            return newValue;
        }
    }

    public static class HardcoreProcessor
    {
        private static readonly Regex NumberRegex = new Regex(@"\b\d+([\s\u00A0]\d+)*\b", RegexOptions.Compiled);
        public const string ProcessLock = "\u200B";

        public static string Process(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            if (input.Contains(ProcessLock)) return input;
            if (Settings.Instance != null && !Settings.Instance.isModActive("misteridle.harditeck")) return input;

            string result = NumberRegex.Replace(input, m =>
            {
                string clean = m.Value.Replace(" ", "").Replace("\u00A0", "");
                if (long.TryParse(clean, out long val) && val >= 100)
                {
                    return (val * Hardkiteck.GoalMultiplier).ToString("#,##0").Replace(",", " ");
                }
                return m.Value;
            });

            return result + ProcessLock;
        }
    }

    [HarmonyPatch(typeof(Park), "Initialize")]
    public class SessionPatch
    {
        static void Postfix(Park __instance)
        {
            if (Settings.Instance != null && !Settings.Instance.isModActive("misteridle.harditeck")) return;

            if (__instance.parkInfo != null && __instance.settings.moneyEnabled)
                __instance.parkInfo.setMoney(__instance.parkInfo.money * Hardkiteck.MoneyMultiplier);

            if (__instance.settings != null)
            {
                __instance.settings.hungerMultiplicator *= Hardkiteck.HungerMultiplier;
                __instance.settings.thirstMultiplicator *= Hardkiteck.ThirstMultiplier;
                __instance.settings.tirednessMultiplicator *= Hardkiteck.TirednessMultiplier;
                __instance.settings.nauseaMultiplicator *= Hardkiteck.NauseaMultiplier;
                __instance.settings.terraformingCostMultiplicator *= Hardkiteck.TerraformingMultiplier;
                __instance.settings.maxGuestMultiplicator *= Hardkiteck.MaxGuestsMultiplier;
            }
        }
    }

    [HarmonyPatch(typeof(GoalsController), "initialize")]
    public class GoalsControllerPatch
    {
        static void Postfix(GoalsController __instance)
        {
            if (Settings.Instance != null && !Settings.Instance.isModActive("misteridle.harditeck")) return;
            if (!Hardkiteck.ForceNoBlueprintGoal) return;

            try
            {
                Type blueprintGoalType = AccessTools.TypeByName("NoTrackedRideBlueprintsGoal");
                if (blueprintGoalType != null)
                {
                    var goalsList = Traverse.Create(__instance).Field("goals").GetValue<IList>();
                    if (goalsList != null)
                    {
                        bool found = false;
                        foreach (var g in goalsList)
                        {
                            if (g.GetType() == blueprintGoalType)
                            {
                                found = true;
                                Traverse.Create(g).Field("isOptional").SetValue(false);
                                Traverse.Create(g).Property("isOptional").SetValue(false);
                                break;
                            }
                        }

                        if (!found)
                        {
                            var newGoal = Activator.CreateInstance(blueprintGoalType);
                            Traverse.Create(newGoal).Field("isOptional").SetValue(false);
                            Traverse.Create(newGoal).Property("isOptional").SetValue(false);
                            goalsList.Add(newGoal);

                            var initMethod = AccessTools.Method(blueprintGoalType, "initialize");
                            initMethod?.Invoke(newGoal, null);
                        }
                    }
                }
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(GoalsController), "getGoalDescriptions")]
    public class GoalDescriptionsPatch
    {
        static void Postfix(GoalsController __instance, ref string __result)
        {
            if (Settings.Instance != null && !Settings.Instance.isModActive("misteridle.harditeck")) return;
            if (!Hardkiteck.ForceNoBlueprintGoal) return;

            try
            {
                Type blueprintGoalType = AccessTools.TypeByName("NoTrackedRideBlueprintsGoal");
                if (blueprintGoalType != null)
                {
                    bool found = false;
                    foreach (var g in __instance.getGoals())
                    {
                        if (g.GetType() == blueprintGoalType)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        var dummyGoal = Activator.CreateInstance(blueprintGoalType);
                        string desc = Traverse.Create(dummyGoal).Property("description").GetValue<string>();
                        if (!string.IsNullOrEmpty(desc))
                        {
                            __result += "• <indent=0.7em>" + desc + "</indent>\n";
                        }
                    }
                }
            }
            catch { }
        }
    }

    [HarmonyPatch(typeof(NoTrackedRideBlueprintsGoal), "canBeMandatory", MethodType.Getter)]
    public class ForceMandatoryBlueprintGoalPatch
    {
        static void Postfix(ref bool __result)
        {
            if (Settings.Instance != null && Settings.Instance.isModActive("misteridle.harditeck"))
            {
                __result = true;
            }
        }
    }

    [HarmonyPatch(typeof(NumberScenarioGoal), "value", MethodType.Getter)]
    public class GoalPatch
    {
        static void Postfix(ref int __result)
        {
            if (Settings.Instance != null && Settings.Instance.isModActive("misteridle.harditeck"))
                __result = (int)(__result * Hardkiteck.GoalMultiplier);
        }
    }

    [HarmonyPatch(typeof(AbstractScenarioMarker), "getScenarioDescription")]
    public class MarkerSourcePatch
    {
        static void Postfix(ref string __result) => __result = HardcoreProcessor.Process(__result);
    }

    [HarmonyPatch(typeof(SavegameFile), "getDescription")]
    public class SavegameDescPatch
    {
        static void Postfix(ref string __result) => __result = HardcoreProcessor.Process(__result);
    }

    [HarmonyPatch(typeof(ScenarioMarkerTooltip), "setScenarioMarker")]
    public class TooltipUIPatch
    {
        static void Postfix(ScenarioMarkerTooltip __instance)
        {
            var traverse = Traverse.Create(__instance);
            var title = traverse.Field("scenarioNameText").GetValue<TextMeshProUGUI>();
            var desc = traverse.Field("scenarioDescriptionText").GetValue<TextMeshProUGUI>();

            if (title != null) title.text = HardcoreProcessor.Process(title.text);
            if (desc != null) desc.text = HardcoreProcessor.Process(desc.text);
        }
    }

    [HarmonyPatch(typeof(ScenarioMarkerTooltip), "LateUpdate")]
    public class TooltipRefreshPatch
    {
        static void Postfix(ScenarioMarkerTooltip __instance)
        {
            var desc = Traverse.Create(__instance).Field("scenarioDescriptionText").GetValue<TextMeshProUGUI>();
            if (desc != null)
            {
                desc.text = HardcoreProcessor.Process(desc.text);
            }
        }
    }
}
