using HarmonyLib;
using Parkitect;
using Parkitect.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace Hardkiteck
{
    public class HardkiteckSettings
    {
        private const string KeyMoney = "Hardkiteck_Money";
        private const string KeyHunger = "Hardkiteck_Hunger";
        private const string KeyThirst = "Hardkiteck_Thirst";
        private const string KeyTiredness = "Hardkiteck_Tiredness";
        private const string KeyNausea = "Hardkiteck_Nausea";
        private const string KeyHappiness = "Hardkiteck_Happiness";
        private const string KeyTerraforming = "Hardkiteck_Terraforming";
        private const string KeyMaxGuests = "Hardkiteck_MaxGuests";
        private const string KeyGoal = "Hardkiteck_Goal";
        private const string KeyNoBlueprint = "Hardkiteck_NoBP";
        private const string KeyNoPause = "Hardkiteck_NoPause";
        private const string KeyNoDebt = "Hardkiteck_NoDebt";
        private const string KeyAttractionPrice = "Hardkiteck_AttractionPrice";
        private const string KeyRollingRes = "Hardkiteck_RollingRes";
        private const string KeyAirRes = "Hardkiteck_AirRes";

        public float MoneyMultiplier = 0.5f;
        public float HungerMultiplier = 1.5f;
        public float ThirstMultiplier = 1.5f;
        public float TirednessMultiplier = 1.5f;
        public float NauseaMultiplier = 1.5f;
        public float HappinessMultiplier = 0.8f;
        public float TerraformingMultiplier = 2.0f;
        public float MaxGuestsMultiplier = 0.7f;
        public float GoalMultiplier = 2.0f;
        public float AttractionPriceMultiplier = 1.5f;
        public float RollingResistanceMultiplier = 1.2f;
        public float AirResistanceMultiplier = 1.2f;

        public bool ForceNoBlueprintGoal = true;
        public bool ForceNoPauseGoal = true;
        public bool ForceNoDebtGoal = true;

        public void Load()
        {
            MoneyMultiplier = PlayerPrefs.GetFloat(KeyMoney, 0.5f);
            HungerMultiplier = PlayerPrefs.GetFloat(KeyHunger, 1.5f);
            ThirstMultiplier = PlayerPrefs.GetFloat(KeyThirst, 1.5f);
            TirednessMultiplier = PlayerPrefs.GetFloat(KeyTiredness, 1.5f);
            NauseaMultiplier = PlayerPrefs.GetFloat(KeyNausea, 1.5f);
            HappinessMultiplier = PlayerPrefs.GetFloat(KeyHappiness, 0.8f);
            TerraformingMultiplier = PlayerPrefs.GetFloat(KeyTerraforming, 2.0f);
            MaxGuestsMultiplier = PlayerPrefs.GetFloat(KeyMaxGuests, 0.7f);
            GoalMultiplier = PlayerPrefs.GetFloat(KeyGoal, 2.0f);
            AttractionPriceMultiplier = PlayerPrefs.GetFloat(KeyAttractionPrice, 1.5f);
            RollingResistanceMultiplier = PlayerPrefs.GetFloat(KeyRollingRes, 1.2f);
            AirResistanceMultiplier = PlayerPrefs.GetFloat(KeyAirRes, 1.2f);

            ForceNoBlueprintGoal = PlayerPrefs.GetInt(KeyNoBlueprint, 1) == 1;
            ForceNoPauseGoal = PlayerPrefs.GetInt(KeyNoPause, 1) == 1;
            ForceNoDebtGoal = PlayerPrefs.GetInt(KeyNoDebt, 1) == 1;
        }

        public void Save()
        {
            PlayerPrefs.SetFloat(KeyMoney, MoneyMultiplier);
            PlayerPrefs.SetFloat(KeyHunger, HungerMultiplier);
            PlayerPrefs.SetFloat(KeyThirst, ThirstMultiplier);
            PlayerPrefs.SetFloat(KeyTiredness, TirednessMultiplier);
            PlayerPrefs.SetFloat(KeyNausea, NauseaMultiplier);
            PlayerPrefs.SetFloat(KeyHappiness, HappinessMultiplier);
            PlayerPrefs.SetFloat(KeyTerraforming, TerraformingMultiplier);
            PlayerPrefs.SetFloat(KeyMaxGuests, MaxGuestsMultiplier);
            PlayerPrefs.SetFloat(KeyGoal, GoalMultiplier);
            PlayerPrefs.SetFloat(KeyAttractionPrice, AttractionPriceMultiplier);
            PlayerPrefs.SetFloat(KeyRollingRes, RollingResistanceMultiplier);
            PlayerPrefs.SetFloat(KeyAirRes, AirResistanceMultiplier);

            PlayerPrefs.SetInt(KeyNoBlueprint, ForceNoBlueprintGoal ? 1 : 0);
            PlayerPrefs.SetInt(KeyNoPause, ForceNoPauseGoal ? 1 : 0);
            PlayerPrefs.SetInt(KeyNoDebt, ForceNoDebtGoal ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static class PriceManager
    {
        private static Dictionary<string, float> originalPrices = new Dictionary<string, float>();
        private static Dictionary<string, float> originalTrackPrices = new Dictionary<string, float>();

        public static void ApplyPrices()
        {
            if (!Hardkiteck.IsActive) return;

            float multiplier = Hardkiteck.Settings.AttractionPriceMultiplier;

            foreach (Attraction attr in Resources.FindObjectsOfTypeAll<Attraction>())
            {
                if (attr == null) continue;
                string id = attr.name.Replace("(Clone)", "").Trim() + "_attr";

                if (!originalPrices.ContainsKey(id))
                    originalPrices[id] = attr.price;

                attr.price = originalPrices[id] * multiplier;

                if (attr is TrackedRide tr)
                {
                    if (!originalTrackPrices.ContainsKey(id))
                        originalTrackPrices[id] = tr.trackPricePerUnit;

                    tr.trackPricePerUnit = originalTrackPrices[id] * multiplier;
                }
            }

            foreach (TrackSegment4 seg in Resources.FindObjectsOfTypeAll<TrackSegment4>())
            {
                if (seg == null) continue;
                string id = seg.name.Replace("(Clone)", "").Trim() + "_seg";

                if (!originalPrices.ContainsKey(id))
                    originalPrices[id] = seg.price;

                seg.price = originalPrices[id] * multiplier;
            }
        }
    }

    public class Hardkiteck : AbstractMod, IModSettings
    {
        public const string ModId = "misteridle.harditeck";
        public static readonly HardkiteckSettings Settings = new HardkiteckSettings();
        public static bool IsActive { get; private set; } = false;

        public override string getName() => "Hardkiteck";
        public override string getDescription() => "Hardcore mode for Parkitect";
        public override string getVersionNumber() => "1.2";
        public override string getIdentifier() => ModId;
        public override bool isMultiplayerModeCompatible() => true;
        public override bool isRequiredByAllPlayersInMultiplayerMode() => true;

        static Hardkiteck()
        {
            IsActive = true;
            Settings.Load();

            try
            {
                new Harmony(ModId).PatchAll();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Hardkiteck] Harmony patch failed: {e.Message}");
            }
        }

        public void onSettingsOpened() => Settings.Load();
        public void onSettingsClosed()
        {
            Settings.Save();
            PriceManager.ApplyPrices();
        }

        private static readonly Color ColorEconomy = new Color(0.95f, 0.80f, 0.30f);
        private static readonly Color ColorGuests = new Color(0.40f, 0.80f, 0.60f);
        private static readonly Color ColorPhysics = new Color(0.60f, 0.40f, 0.80f);
        private static readonly Color ColorChallenge = new Color(0.85f, 0.40f, 0.40f);
        private static readonly Color ColorDefault = Color.white;

        public void onDrawSettingsUI()
        {
            DrawButton("Reset to Defaults", ResetSettings);

            DrawSectionHeader("ECONOMY", ColorEconomy);
            Settings.MoneyMultiplier = DrawStepper("Starting Money", Settings.MoneyMultiplier, 0.1f, 3.0f, 0.05f);
            Settings.AttractionPriceMultiplier = DrawStepper("Attraction & Coaster Price", Settings.AttractionPriceMultiplier, 0.5f, 5.0f, 0.1f);
            Settings.TerraformingMultiplier = DrawStepper("Terraforming Cost", Settings.TerraformingMultiplier, 0.1f, 5.0f, 0.25f);

            GUILayout.Space(8);
            DrawSectionHeader("GUESTS", ColorGuests);
            Settings.MaxGuestsMultiplier = DrawStepper("Max Guests", Settings.MaxGuestsMultiplier, 0.1f, 3.0f, 0.05f);
            Settings.HappinessMultiplier = DrawStepper("Happiness Modifier", Settings.HappinessMultiplier, 0.1f, 3.0f, 0.1f);

            GUILayout.Space(4);
            GUILayout.Label("Survival Needs (Higher = faster decay):");
            Settings.HungerMultiplier = DrawStepper("Hunger", Settings.HungerMultiplier, 0.1f, 3.0f, 0.1f);
            Settings.ThirstMultiplier = DrawStepper("Thirst", Settings.ThirstMultiplier, 0.1f, 3.0f, 0.1f);
            Settings.TirednessMultiplier = DrawStepper("Tiredness", Settings.TirednessMultiplier, 0.1f, 3.0f, 0.1f);
            Settings.NauseaMultiplier = DrawStepper("Nausea", Settings.NauseaMultiplier, 0.1f, 3.0f, 0.1f);

            GUILayout.Space(8);
            DrawSectionHeader("COASTERS PHYSICS", ColorPhysics);
            GUILayout.Label("Higher resistance makes coasters lose speed faster:");
            Settings.RollingResistanceMultiplier = DrawStepper("Rolling Resistance", Settings.RollingResistanceMultiplier, 0.5f, 3.0f, 0.1f);
            Settings.AirResistanceMultiplier = DrawStepper("Air Resistance", Settings.AirResistanceMultiplier, 0.5f, 3.0f, 0.1f);

            GUILayout.Space(8);
            DrawSectionHeader("CHALLENGE", ColorChallenge);
            Settings.GoalMultiplier = DrawStepper("Goal Value Multiplier", Settings.GoalMultiplier, 0.1f, 5.0f, 0.25f);

            GUILayout.Space(4);
            GUI.color = ColorChallenge;
            Settings.ForceNoBlueprintGoal = GUILayout.Toggle(Settings.ForceNoBlueprintGoal, " Disable Blueprints (Mandatory Goal)");
            Settings.ForceNoPauseGoal = GUILayout.Toggle(Settings.ForceNoPauseGoal, " Win Without Pausing (Mandatory Goal)");
            Settings.ForceNoDebtGoal = GUILayout.Toggle(Settings.ForceNoDebtGoal, " No Loan Debts (Mandatory Goal)");
            GUI.color = ColorDefault;

            GUILayout.Space(8);
            DrawDifficultyHint();
        }

        private void DrawDifficultyHint()
        {
            var settings = Settings;

            bool isEasy = settings.MoneyMultiplier > 1.0f
                       || settings.AttractionPriceMultiplier < 1.0f
                       || settings.TerraformingMultiplier < 1.0f
                       || settings.MaxGuestsMultiplier > 1.0f
                       || settings.HappinessMultiplier > 1.0f
                       || settings.HungerMultiplier < 1.0f
                       || settings.RollingResistanceMultiplier < 1.0f
                       || settings.GoalMultiplier < 1.0f;

            bool isHard = settings.MoneyMultiplier < 1.0f
                       || settings.AttractionPriceMultiplier > 1.0f
                       || settings.TerraformingMultiplier > 1.0f
                       || settings.MaxGuestsMultiplier < 1.0f
                       || settings.HappinessMultiplier < 1.0f
                       || settings.HungerMultiplier > 1.0f
                       || settings.RollingResistanceMultiplier > 1.0f
                       || settings.GoalMultiplier > 1.0f;

            string label;
            Color color;

            if (isEasy && isHard)
            { label = "⚡ Mixed"; color = ColorEconomy; }
            else if (isEasy) { label = "✔ Easy"; color = ColorGuests; }
            else if (isHard) { label = "☠ Hardcore"; color = ColorChallenge; }
            else { label = "◆ Vanilla"; color = ColorDefault; }

            GUI.color = color;
            GUILayout.Label(label);
            GUI.color = ColorDefault;
        }

        private void DrawSectionHeader(string title, Color color)
        {
            GUI.color = color;
            GUILayout.Label($"▶  {title}", GUI.skin.label);
            GUI.color = ColorDefault;
            GUILayout.Space(2);
        }

        private void DrawButton(string label, Action onClick)
        {
            if (GUILayout.Button(label))
                onClick?.Invoke();
        }

        private float DrawStepper(string label, float value, float min, float max, float step)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.Width(180));

            bool decrease = GUILayout.Button("  -  ", GUILayout.Width(40));
            GUILayout.Label($"x{value:0.00}", GUILayout.Width(55));
            bool increase = GUILayout.Button("  +  ", GUILayout.Width(40));

            GUILayout.EndHorizontal();

            if (decrease) value = Mathf.Max(min, (float)Math.Round(value - step, 2));
            if (increase) value = Mathf.Min(max, (float)Math.Round(value + step, 2));

            return value;
        }

        private void ResetSettings()
        {
            Settings.MoneyMultiplier = 0.5f;
            Settings.AttractionPriceMultiplier = 1.5f;
            Settings.HungerMultiplier = 1.5f;
            Settings.ThirstMultiplier = 1.5f;
            Settings.TirednessMultiplier = 1.5f;
            Settings.NauseaMultiplier = 1.5f;
            Settings.HappinessMultiplier = 0.8f;
            Settings.RollingResistanceMultiplier = 1.2f;
            Settings.AirResistanceMultiplier = 1.2f;
            Settings.TerraformingMultiplier = 2.0f;
            Settings.MaxGuestsMultiplier = 0.7f;
            Settings.GoalMultiplier = 2.0f;
            Settings.ForceNoBlueprintGoal = true;
            Settings.ForceNoPauseGoal = true;
            Settings.ForceNoDebtGoal = true;
        }
    }

    [HarmonyPatch(typeof(Park), "Initialize")]
    public class ParkInitializePatch
    {
        static void Postfix(Park __instance)
        {
            if (!Hardkiteck.IsActive) return;

            var s = Hardkiteck.Settings;
            var parkInfo = __instance.parkInfo;
            var config = __instance.settings;

            if (parkInfo != null && config.moneyEnabled)
                parkInfo.setMoney(parkInfo.money * s.MoneyMultiplier);

            if (config != null)
            {
                config.hungerMultiplicator *= s.HungerMultiplier;
                config.thirstMultiplicator *= s.ThirstMultiplier;
                config.tirednessMultiplicator *= s.TirednessMultiplier;
                config.nauseaMultiplicator *= s.NauseaMultiplier;
                config.happinessMultiplicator *= s.HappinessMultiplier;

                config.trackedRideRollingResistanceMultiplicator *= s.RollingResistanceMultiplier;
                config.trackedRideAirResistanceMultiplicator *= s.AirResistanceMultiplier;

                config.terraformingCostMultiplicator *= s.TerraformingMultiplier;
                config.maxGuestMultiplicator *= s.MaxGuestsMultiplier;
            }

            PriceManager.ApplyPrices();
        }
    }

    [HarmonyPatch(typeof(NumberScenarioGoal), "value", MethodType.Getter)]
    public class ScenarioGoalValuePatch
    {
        static void Postfix(ref int __result)
        {
            if (Hardkiteck.IsActive)
                __result = (int)(__result * Hardkiteck.Settings.GoalMultiplier);
        }
    }

    [HarmonyPatch(typeof(GoalsController), "initialize")]
    public class GoalsControllerInitPatch
    {
        private static readonly Type BlueprintGoalType = AccessTools.TypeByName("NoTrackedRideBlueprintsGoal");
        private static readonly Type NoPauseGoalType = AccessTools.TypeByName("WinWithoutPausingGoal");
        private static readonly Type NoDebtGoalType = AccessTools.TypeByName("NoLoanDebtsGoal");

        static void Postfix(GoalsController __instance)
        {
            if (!Hardkiteck.IsActive)
                return;

            var goals = Traverse.Create(__instance).Field("goals").GetValue<IList>();
            if (goals == null)
                return;

            if (Hardkiteck.Settings.ForceNoBlueprintGoal && BlueprintGoalType != null)
                EnsureGoalMandatory(goals, BlueprintGoalType);

            if (Hardkiteck.Settings.ForceNoPauseGoal && NoPauseGoalType != null)
                EnsureGoalMandatory(goals, NoPauseGoalType);

            if (Hardkiteck.Settings.ForceNoDebtGoal && NoDebtGoalType != null)
                EnsureGoalMandatory(goals, NoDebtGoalType);
        }

        private static void EnsureGoalMandatory(IList goals, Type goalType)
        {
            object existing = FindGoalOfType(goals, goalType);
            if (existing != null)
            {
                SetMandatory(existing);
            }
            else
            {
                TryAddGoal(goals, goalType);
            }
        }

        private static object FindGoalOfType(IList goals, Type type)
        {
            foreach (var goal in goals)
                if (goal.GetType() == type) return goal;
            return null;
        }

        private static void SetMandatory(object goal)
        {
            var node = Traverse.Create(goal);
            node.Field("isOptional").SetValue(false);
            node.Property("isOptional").SetValue(false);
        }

        private static void TryAddGoal(IList goals, Type type)
        {
            try
            {
                var newGoal = Activator.CreateInstance(type);
                SetMandatory(newGoal);
                goals.Add(newGoal);
                AccessTools.Method(type, "initialize")?.Invoke(newGoal, null);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Hardkiteck] Could not inject goal {type.Name}: {e.Message}");
            }
        }
    }

    public static class HardcoreProcessor
    {
        private static readonly Regex NumberRegex = new Regex(@"\b\d+([\s\u00A0]\d+)*\b", RegexOptions.Compiled);
        public const string ProcessedMarker = "\u200B";

        public static string Process(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Contains(ProcessedMarker)) return input;
            if (!Hardkiteck.IsActive) return input;

            return NumberRegex.Replace(input, match =>
            {
                string digits = match.Value.Replace(" ", "").Replace("\u00A0", "");
                if (long.TryParse(digits, out long number) && number >= 100)
                {
                    return number
                        .ToString("#,##0")
                        .Replace(",", " ");
                }
                return match.Value;
            }) + ProcessedMarker;
        }
    }

    [HarmonyPatch(typeof(AbstractScenarioMarker), "getScenarioDescription")]
    public class ScenarioDescriptionPatch
    {
        static void Postfix(ref string __result) => __result = HardcoreProcessor.Process(__result);
    }

    [HarmonyPatch(typeof(SavegameFile), "getDescription")]
    public class SavegameDescriptionPatch
    {
        static void Postfix(ref string __result) => __result = HardcoreProcessor.Process(__result);
    }

    [HarmonyPatch(typeof(ScenarioMarkerTooltip), "setScenarioMarker")]
    public class TooltipSetPatch
    {
        static void Postfix(ScenarioMarkerTooltip __instance)
        {
            var node = Traverse.Create(__instance);
            var title = node.Field("scenarioNameText").GetValue<TextMeshProUGUI>();
            var desc = node.Field("scenarioDescriptionText").GetValue<TextMeshProUGUI>();

            if (title != null) title.text = HardcoreProcessor.Process(title.text);
            if (desc != null) desc.text = HardcoreProcessor.Process(desc.text);
        }
    }

    [HarmonyPatch(typeof(ScenarioMarkerTooltip), "LateUpdate")]
    public class TooltipRefreshPatch
    {
        static void Postfix(ScenarioMarkerTooltip __instance)
        {
            var desc = Traverse.Create(__instance)
                .Field("scenarioDescriptionText")
                .GetValue<TextMeshProUGUI>();

            if (desc != null)
                desc.text = HardcoreProcessor.Process(desc.text);
        }
    }

    [HarmonyPatch(typeof(GoalsController), "getGoalDescriptions")]
    public class GoalDescriptionsPatch
    {
        static void Postfix(GoalsController __instance, ref string __result)
        {
            if (!Hardkiteck.IsActive) return;

            AppendGoalDescriptionIfNeeded(__instance, ref __result, "NoTrackedRideBlueprintsGoal", Hardkiteck.Settings.ForceNoBlueprintGoal);
            AppendGoalDescriptionIfNeeded(__instance, ref __result, "WinWithoutPausingGoal", Hardkiteck.Settings.ForceNoPauseGoal);
            AppendGoalDescriptionIfNeeded(__instance, ref __result, "NoLoanDebtsGoal", Hardkiteck.Settings.ForceNoDebtGoal);
        }

        private static void AppendGoalDescriptionIfNeeded(GoalsController controller, ref string result, string typeName, bool isForced)
        {
            if (!isForced) return;

            Type goalType = AccessTools.TypeByName(typeName);
            if (goalType == null) return;

            bool alreadyShown = false;
            foreach (var goal in controller.getGoals())
            {
                if (goal.GetType() == goalType)
                {
                    alreadyShown = true;
                    break;
                }
            }

            if (alreadyShown) return;

            string description = TryGetGoalDescription(goalType);
            if (!string.IsNullOrEmpty(description))
                result += $"• <indent=0.7em>{description}</indent>\n";
        }

        private static string TryGetGoalDescription(Type type)
        {
            try
            {
                var dummy = Activator.CreateInstance(type);
                return Traverse.Create(dummy).Property("description").GetValue<string>();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Hardkiteck] Could not read goal description for {type.Name}: {e.Message}");
                return null;
            }
        }
    }
}