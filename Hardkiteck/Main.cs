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
    public class HardkiteckSettings
    {
        private const string KeyMoney = "Hardkiteck_Money";
        private const string KeyHunger = "Hardkiteck_Hunger";
        private const string KeyThirst = "Hardkiteck_Thirst";
        private const string KeyTiredness = "Hardkiteck_Tiredness";
        private const string KeyNausea = "Hardkiteck_Nausea";
        private const string KeyTerraforming = "Hardkiteck_Terraforming";
        private const string KeyMaxGuests = "Hardkiteck_MaxGuests";
        private const string KeyGoal = "Hardkiteck_Goal";
        private const string KeyNoBlueprint = "Hardkiteck_NoBP";

        public float MoneyMultiplier = 0.5f;
        public float HungerMultiplier = 1.5f;
        public float ThirstMultiplier = 1.5f;
        public float TirednessMultiplier = 1.5f;
        public float NauseaMultiplier = 1.5f;
        public float TerraformingMultiplier = 2.0f;
        public float MaxGuestsMultiplier = 0.7f;
        public float GoalMultiplier = 2.0f;
        public bool ForceNoBlueprintGoal = true;

        public void Load()
        {
            MoneyMultiplier = PlayerPrefs.GetFloat(KeyMoney, 0.5f);
            HungerMultiplier = PlayerPrefs.GetFloat(KeyHunger, 1.5f);
            ThirstMultiplier = PlayerPrefs.GetFloat(KeyThirst, 1.5f);
            TirednessMultiplier = PlayerPrefs.GetFloat(KeyTiredness, 1.5f);
            NauseaMultiplier = PlayerPrefs.GetFloat(KeyNausea, 1.5f);
            TerraformingMultiplier = PlayerPrefs.GetFloat(KeyTerraforming, 2.0f);
            MaxGuestsMultiplier = PlayerPrefs.GetFloat(KeyMaxGuests, 0.7f);
            GoalMultiplier = PlayerPrefs.GetFloat(KeyGoal, 2.0f);
            ForceNoBlueprintGoal = PlayerPrefs.GetInt(KeyNoBlueprint, 1) == 1;
        }

        public void Save()
        {
            PlayerPrefs.SetFloat(KeyMoney, MoneyMultiplier);
            PlayerPrefs.SetFloat(KeyHunger, HungerMultiplier);
            PlayerPrefs.SetFloat(KeyThirst, ThirstMultiplier);
            PlayerPrefs.SetFloat(KeyTiredness, TirednessMultiplier);
            PlayerPrefs.SetFloat(KeyNausea, NauseaMultiplier);
            PlayerPrefs.SetFloat(KeyTerraforming, TerraformingMultiplier);
            PlayerPrefs.SetFloat(KeyMaxGuests, MaxGuestsMultiplier);
            PlayerPrefs.SetFloat(KeyGoal, GoalMultiplier);
            PlayerPrefs.SetInt(KeyNoBlueprint, ForceNoBlueprintGoal ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public class Hardkiteck : AbstractMod, IModSettings
    {
        public const string ModId = "misteridle.harditeck";

        public static readonly HardkiteckSettings Settings = new HardkiteckSettings();

        public static bool IsActive { get; private set; } = false;

        public override string getName() => "Hardkiteck";
        public override string getDescription() => "Hardcore mode for Parkitect";
        public override string getVersionNumber() => "1.0";
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
        public void onSettingsClosed() => Settings.Save();

        private static readonly Color ColorEconomy = new Color(0.95f, 0.80f, 0.30f);
        private static readonly Color ColorGuests = new Color(0.40f, 0.80f, 0.60f);
        private static readonly Color ColorChallenge = new Color(0.85f, 0.40f, 0.40f);
        private static readonly Color ColorDefault = Color.white;

        public void onDrawSettingsUI()
        {
            DrawButton("Reset to Defaults", ResetSettings);

            DrawSectionHeader("ECONOMY", ColorEconomy);
            Settings.MoneyMultiplier = DrawStepper("Starting Money", Settings.MoneyMultiplier, 0.1f, 3.0f, 0.05f);
            Settings.TerraformingMultiplier = DrawStepper("Terraforming Cost", Settings.TerraformingMultiplier, 0.1f, 5.0f, 0.25f);

            GUILayout.Space(8);
            DrawSectionHeader("GUESTS", ColorGuests);
            Settings.MaxGuestsMultiplier = DrawStepper("Max Guests", Settings.MaxGuestsMultiplier, 0.1f, 3.0f, 0.05f);

            GUILayout.Space(4);
            GUILayout.Label("Survival Needs:");
            Settings.HungerMultiplier = DrawStepper("Hunger", Settings.HungerMultiplier, 0.1f, 3.0f, 0.1f);
            Settings.ThirstMultiplier = DrawStepper("Thirst", Settings.ThirstMultiplier, 0.1f, 3.0f, 0.1f);
            Settings.TirednessMultiplier = DrawStepper("Tiredness", Settings.TirednessMultiplier, 0.1f, 3.0f, 0.1f);
            Settings.NauseaMultiplier = DrawStepper("Nausea", Settings.NauseaMultiplier, 0.1f, 3.0f, 0.1f);

            GUILayout.Space(8);
            DrawSectionHeader("CHALLENGE", ColorChallenge);
            Settings.GoalMultiplier = DrawStepper("Goal Multiplier", Settings.GoalMultiplier, 0.1f, 5.0f, 0.25f);

            GUILayout.Space(4);
            GUI.color = ColorChallenge;
            Settings.ForceNoBlueprintGoal = GUILayout.Toggle(
                Settings.ForceNoBlueprintGoal,
                " Disable Blueprints (Mandatory Goal)"
            );
            GUI.color = ColorDefault;

            GUILayout.Space(8);
            DrawDifficultyHint();
        }

        private void DrawDifficultyHint()
        {
            var settings = Settings;

            bool isEasy = settings.MoneyMultiplier > 1.0f
                       || settings.TerraformingMultiplier < 1.0f
                       || settings.MaxGuestsMultiplier > 1.0f
                       || settings.HungerMultiplier < 1.0f
                       || settings.ThirstMultiplier < 1.0f
                       || settings.TirednessMultiplier < 1.0f
                       || settings.NauseaMultiplier < 1.0f
                       || settings.GoalMultiplier < 1.0f;

            bool isHard = settings.MoneyMultiplier < 1.0f
                       || settings.TerraformingMultiplier > 1.0f
                       || settings.MaxGuestsMultiplier < 1.0f
                       || settings.HungerMultiplier > 1.0f
                       || settings.ThirstMultiplier > 1.0f
                       || settings.TirednessMultiplier > 1.0f
                       || settings.NauseaMultiplier > 1.0f
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
            Settings.HungerMultiplier = 1.5f;
            Settings.ThirstMultiplier = 1.5f;
            Settings.TirednessMultiplier = 1.5f;
            Settings.NauseaMultiplier = 1.5f;
            Settings.TerraformingMultiplier = 2.0f;
            Settings.MaxGuestsMultiplier = 0.7f;
            Settings.GoalMultiplier = 2.0f;
            Settings.ForceNoBlueprintGoal = true;
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
                config.terraformingCostMultiplicator *= s.TerraformingMultiplier;
                config.maxGuestMultiplicator *= s.MaxGuestsMultiplier;
            }
        }
    }

    [HarmonyPatch(typeof(NoTrackedRideBlueprintsGoal), "canBeMandatory", MethodType.Getter)]
    public class BlueprintGoalMandatoryPatch
    {
        static void Postfix(ref bool __result)
        {
            if (Hardkiteck.IsActive)
                __result = true;
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
        private static readonly Type BlueprintGoalType =
            AccessTools.TypeByName("NoTrackedRideBlueprintsGoal");

        static void Postfix(GoalsController __instance)
        {
            if (!Hardkiteck.IsActive) 
                return;
            if (!Hardkiteck.Settings.ForceNoBlueprintGoal) 
                return;

            if (BlueprintGoalType == null) 
                return;

            var goals = Traverse.Create(__instance).Field("goals").GetValue<IList>();
            
            if (goals == null) 
                return;

            object existing = FindGoalOfType(goals, BlueprintGoalType);

            if (existing != null)
            {
                SetMandatory(existing);
            }
            else
            {
                TryAddBlueprintGoal(goals);
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

        private static void TryAddBlueprintGoal(IList goals)
        {
            try
            {
                var newGoal = Activator.CreateInstance(BlueprintGoalType);
                SetMandatory(newGoal);
                goals.Add(newGoal);
                AccessTools.Method(BlueprintGoalType, "initialize")?.Invoke(newGoal, null);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Hardkiteck] Could not inject blueprint goal: {e.Message}");
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
        private static readonly Type BlueprintGoalType =
            AccessTools.TypeByName("NoTrackedRideBlueprintsGoal");

        static void Postfix(GoalsController __instance, ref string __result)
        {
            if (!Hardkiteck.IsActive) return;
            if (!Hardkiteck.Settings.ForceNoBlueprintGoal) return;
            if (BlueprintGoalType == null) return;

            bool alreadyShown = false;
            foreach (var goal in __instance.getGoals())
            {
                if (goal.GetType() == BlueprintGoalType)
                {
                    alreadyShown = true;
                    break;
                }
            }

            if (alreadyShown) return;

            string description = TryGetBlueprintGoalDescription();
            if (!string.IsNullOrEmpty(description))
                __result += $"• <indent=0.7em>{description}</indent>\n";
        }

        private static string TryGetBlueprintGoalDescription()
        {
            try
            {
                var dummy = Activator.CreateInstance(BlueprintGoalType);
                return Traverse.Create(dummy).Property("description").GetValue<string>();
            }
            catch (Exception e)
            {
                Debug.LogError($"[Hardkiteck] Could not read blueprint goal description: {e.Message}");
                return null;
            }
        }
    }
}
