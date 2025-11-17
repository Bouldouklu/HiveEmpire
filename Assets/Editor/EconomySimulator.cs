using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HiveEmpire.Editor
{
    /// <summary>
    /// Economic simulation tool for balancing Hive Empire's 24-week campaign.
    /// Simulates income generation, upgrade affordability, and decision density.
    /// </summary>
    public class EconomySimulator : EditorWindow
    {
        #region Simulation Parameters

        // Starting Conditions
        private float startingMoney = 50f;
        private int startingBees = 0;
        private float realSecondsPerWeek = 60f;

        // Bee Parameters
        private float beeSpeed = 10f;
        private float averagePatchDistance = 50f; // Average distance from hive

        // Flower Patch Parameters
        private int[] flowerPatchUnlockCosts = { 1, 15, 50, 90, 100, 150 };
        private int capacityTierCount = 5;
        private int[] capacityUpgradeCosts = { 50, 150, 400, 900, 2000 };
        private int baseCapacity = 5;
        private int bonusCapacityPerTier = 5;

        // Bee Purchase Parameters
        private int beePurchaseTierCount = 10;
        private int[] beePurchaseCosts = { 25, 60, 150, 350, 800, 1800, 4000, 8500, 18000, 38000 };
        private int[] beesPerPurchase = { 2, 3, 5, 8, 12, 18, 25, 35, 50, 70 };

        // Recipe Parameters
        private int recipeTierCount = 5;
        private int[] recipeUpgradeCosts = { 100, 300, 800, 2000, 5000 };
        private float[] tierProductionTimeMultipliers = { 1f, 0.85f, 0.75f, 0.65f, 0.50f };
        private float[] tierValueMultipliers = { 1f, 1.2f, 1.4f, 1.6f, 2.0f };

        // Seasonal Modifiers (Spring, Summer, Autumn)
        private float[] seasonIncomeModifiers = { 1.0f, 1.5f, 1.3f };
        private float[] seasonProductionTimeModifiers = { 1.0f, 1.1f, 0.85f };
        private float[] seasonBeeSpeedModifiers = { 1.1f, 0.9f, 1.0f };

        #endregion

        #region Simulation State

        private SimulationResult lastResult;
        private Vector2 scrollPosition;
        private bool showDetailedLog = false;

        #endregion

        #region Editor Window

        [MenuItem("Game/Economy Simulator")]
        public static void ShowWindow()
        {
            GetWindow<EconomySimulator>("Economy Simulator");
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("Hive Empire - Economic Simulator", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            DrawStartingConditions();
            DrawBeeParameters();
            DrawFlowerPatchParameters();
            DrawBeePurchaseParameters();
            DrawRecipeParameters();
            DrawSeasonalModifiers();

            EditorGUILayout.Space();

            if (GUILayout.Button("Run Simulation", GUILayout.Height(30)))
            {
                RunSimulation();
            }

            if (lastResult != null)
            {
                DrawSimulationResults();
            }

            EditorGUILayout.EndScrollView();
        }

        #endregion

        #region Parameter UI

        private void DrawStartingConditions()
        {
            EditorGUILayout.LabelField("Starting Conditions", EditorStyles.boldLabel);
            startingMoney = EditorGUILayout.FloatField("Starting Money", startingMoney);
            startingBees = EditorGUILayout.IntField("Starting Bees", startingBees);
            realSecondsPerWeek = EditorGUILayout.FloatField("Seconds Per Week", realSecondsPerWeek);
            EditorGUILayout.Space();
        }

        private void DrawBeeParameters()
        {
            EditorGUILayout.LabelField("Bee Parameters", EditorStyles.boldLabel);
            beeSpeed = EditorGUILayout.FloatField("Bee Speed (units/s)", beeSpeed);
            averagePatchDistance = EditorGUILayout.FloatField("Avg Patch Distance", averagePatchDistance);
            EditorGUILayout.Space();
        }

        private void DrawFlowerPatchParameters()
        {
            EditorGUILayout.LabelField("Flower Patch Parameters", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Unlock Costs (6 patches):");
            for (int i = 0; i < 6; i++)
            {
                flowerPatchUnlockCosts[i] = EditorGUILayout.IntField($"  Patch {i + 1}", flowerPatchUnlockCosts[i]);
            }

            capacityTierCount = EditorGUILayout.IntField("Capacity Tier Count", capacityTierCount);
            baseCapacity = EditorGUILayout.IntField("Base Capacity", baseCapacity);
            bonusCapacityPerTier = EditorGUILayout.IntField("Bonus Per Tier", bonusCapacityPerTier);

            EditorGUILayout.LabelField($"Capacity Upgrade Costs ({capacityTierCount} tiers):");
            for (int i = 0; i < capacityTierCount && i < capacityUpgradeCosts.Length; i++)
            {
                capacityUpgradeCosts[i] = EditorGUILayout.IntField($"  Tier {i + 1}", capacityUpgradeCosts[i]);
            }

            EditorGUILayout.Space();
        }

        private void DrawBeePurchaseParameters()
        {
            EditorGUILayout.LabelField("Bee Purchase Parameters", EditorStyles.boldLabel);
            beePurchaseTierCount = EditorGUILayout.IntField("Purchase Tier Count", beePurchaseTierCount);

            EditorGUILayout.LabelField($"Purchase Costs & Bees ({beePurchaseTierCount} tiers):");
            for (int i = 0; i < beePurchaseTierCount && i < beePurchaseCosts.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                beePurchaseCosts[i] = EditorGUILayout.IntField($"  Tier {i + 1} Cost", beePurchaseCosts[i]);
                beesPerPurchase[i] = EditorGUILayout.IntField($"Bees", beesPerPurchase[i]);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
        }

        private void DrawRecipeParameters()
        {
            EditorGUILayout.LabelField("Recipe Parameters", EditorStyles.boldLabel);
            recipeTierCount = EditorGUILayout.IntField("Recipe Tier Count", recipeTierCount);

            EditorGUILayout.LabelField($"Upgrade Costs ({recipeTierCount} tiers):");
            for (int i = 0; i < recipeTierCount && i < recipeUpgradeCosts.Length; i++)
            {
                recipeUpgradeCosts[i] = EditorGUILayout.IntField($"  Tier {i + 1}", recipeUpgradeCosts[i]);
            }

            EditorGUILayout.LabelField($"Tier Multipliers ({recipeTierCount + 1} tiers, 0=base):");
            for (int i = 0; i < recipeTierCount && i < tierProductionTimeMultipliers.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                tierProductionTimeMultipliers[i] = EditorGUILayout.FloatField($"  Tier {i} Time", tierProductionTimeMultipliers[i]);
                tierValueMultipliers[i] = EditorGUILayout.FloatField($"Value", tierValueMultipliers[i]);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
        }

        private void DrawSeasonalModifiers()
        {
            EditorGUILayout.LabelField("Seasonal Modifiers", EditorStyles.boldLabel);
            string[] seasons = { "Spring", "Summer", "Autumn" };

            for (int i = 0; i < 3; i++)
            {
                EditorGUILayout.LabelField($"{seasons[i]} Modifiers:");
                EditorGUILayout.BeginHorizontal();
                seasonIncomeModifiers[i] = EditorGUILayout.FloatField("  Income", seasonIncomeModifiers[i]);
                seasonProductionTimeModifiers[i] = EditorGUILayout.FloatField("  Prod Time", seasonProductionTimeModifiers[i]);
                seasonBeeSpeedModifiers[i] = EditorGUILayout.FloatField("  Bee Speed", seasonBeeSpeedModifiers[i]);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
        }

        #endregion

        #region Simulation Logic

        private void RunSimulation()
        {
            lastResult = new SimulationResult();
            SimulationState state = new SimulationState();

            // Initialize
            state.money = startingMoney;
            state.totalBees = startingBees;
            state.activeRecipeCount = 1; // Start with 1 free recipe (like Forest Honey in actual game)
            state.currentWeek = 0;

            Debug.Log("=== STARTING ECONOMIC SIMULATION ===");
            Debug.Log($"Starting Money: ${startingMoney:F2}, Starting Bees: {startingBees}");

            // Simulate 24 weeks
            for (int week = 1; week <= 24; week++)
            {
                state.currentWeek = week;
                int seasonIndex = GetSeasonIndex(week);

                WeekSimulationResult weekResult = new WeekSimulationResult { week = week };

                // Make strategic decisions FIRST (so new purchases take effect this week)
                MakeStrategicDecisions(state, weekResult);

                // Then simulate the week with the new resources
                SimulateWeek(state, seasonIndex, weekResult);
                lastResult.weekResults.Add(weekResult);

                Debug.Log($"Week {week}: Income=${weekResult.incomeGenerated:F2}, Total Money=${state.money:F2}, Decisions={weekResult.decisionsThisWeek}");
            }

            // Calculate summary statistics
            CalculateSummaryStatistics(lastResult, state);

            Debug.Log("=== SIMULATION COMPLETE ===");
            Debug.Log($"Final Money: ${state.money:F2}");
            Debug.Log($"Total Decisions: {lastResult.totalDecisions}");
            Debug.Log($"Completion %: {lastResult.completionPercentage:F1}%");
        }

        private void SimulateWeek(SimulationState state, int seasonIndex, WeekSimulationResult result)
        {

            // Calculate effective bee speed with seasonal modifier
            float effectiveBeeSpeed = beeSpeed * seasonBeeSpeedModifiers[seasonIndex];
            float roundTripTime = (averagePatchDistance * 2) / effectiveBeeSpeed;
            float deliveriesPerSecond = state.allocatedBees / roundTripTime;
            float deliveriesPerWeek = deliveriesPerSecond * realSecondsPerWeek;

            // Estimate pollen collected (simplified: assume each delivery = 1 pollen)
            float pollenCollected = deliveriesPerWeek * state.activePatchCount;

            // Estimate recipe production (simplified model)
            // Assume on average recipes need 3 pollen and take 15s base production time
            int avgRecipeTierInt = Mathf.RoundToInt(state.averageRecipeTier);
            float avgProductionTime = 15f * tierProductionTimeMultipliers[Mathf.Min(avgRecipeTierInt, tierProductionTimeMultipliers.Length - 1)]
                                           * seasonProductionTimeModifiers[seasonIndex];
            float avgRecipeValue = 10f * tierValueMultipliers[Mathf.Min(avgRecipeTierInt, tierValueMultipliers.Length - 1)]
                                        * seasonIncomeModifiers[seasonIndex];

            float recipesCompleted = Mathf.Min(pollenCollected / 3f, realSecondsPerWeek / avgProductionTime * state.activeRecipeCount);
            float incomeThisWeek = recipesCompleted * avgRecipeValue;

            result.incomeGenerated = incomeThisWeek;
            result.pollenCollected = pollenCollected;
            result.recipesCompleted = recipesCompleted;

            state.money += incomeThisWeek;
        }

        private void MakeStrategicDecisions(SimulationState state, WeekSimulationResult weekResult)
        {
            int decisionsThisWeek = 0;

            // Strategy: Prioritize bees early, then patches, then upgrades

            // 1. Buy bees if affordable
            if (state.beePurchaseTier < beePurchaseTierCount &&
                state.money >= beePurchaseCosts[state.beePurchaseTier])
            {
                state.money -= beePurchaseCosts[state.beePurchaseTier];
                state.totalBees += beesPerPurchase[state.beePurchaseTier];
                state.allocatedBees = state.totalBees; // Simple: allocate all bees
                state.beePurchaseTier++;
                decisionsThisWeek++;
            }

            // 2. Unlock new flower patches
            if (state.activePatchCount < 6 &&
                state.money >= flowerPatchUnlockCosts[state.activePatchCount])
            {
                state.money -= flowerPatchUnlockCosts[state.activePatchCount];
                state.activePatchCount++;
                decisionsThisWeek++;
            }

            // 3. Unlock recipes (simplified: assume $25 per recipe, unlock up to patch count * 2)
            int maxRecipes = state.activePatchCount * 2;
            if (state.activeRecipeCount < maxRecipes && state.money >= 25)
            {
                state.money -= 25;
                state.activeRecipeCount++;
                decisionsThisWeek++;
            }

            // 4. Upgrade capacity on existing patches
            for (int i = 0; i < state.activePatchCount; i++)
            {
                if (state.patchCapacityTiers[i] < capacityTierCount &&
                    state.money >= capacityUpgradeCosts[state.patchCapacityTiers[i]])
                {
                    state.money -= capacityUpgradeCosts[state.patchCapacityTiers[i]];
                    state.patchCapacityTiers[i]++;
                    decisionsThisWeek++;
                    break; // One upgrade per week for simplicity
                }
            }

            // 5. Upgrade recipes
            for (int i = 0; i < state.activeRecipeCount; i++)
            {
                if (state.recipeTiers[i] < recipeTierCount &&
                    state.money >= recipeUpgradeCosts[state.recipeTiers[i]])
                {
                    state.money -= recipeUpgradeCosts[state.recipeTiers[i]];
                    state.recipeTiers[i]++;
                    state.averageRecipeTier = (float)state.recipeTiers.Average();
                    decisionsThisWeek++;
                    break; // One upgrade per week for simplicity
                }
            }

            weekResult.decisionsThisWeek = decisionsThisWeek;
        }

        private void CalculateSummaryStatistics(SimulationResult result, SimulationState finalState)
        {
            result.totalDecisions = result.weekResults.Sum(w => w.decisionsThisWeek);
            result.totalIncome = result.weekResults.Sum(w => w.incomeGenerated);
            result.finalMoney = finalState.money;

            // Calculate max possible upgrades
            int maxPossibleUpgrades =
                6 + // Flower patch unlocks
                12 + // Recipe unlocks (assuming 12 total)
                (6 * capacityTierCount) + // Capacity upgrades
                (12 * recipeTierCount) + // Recipe upgrades
                beePurchaseTierCount; // Bee purchases

            int actualUpgrades =
                finalState.activePatchCount +
                finalState.activeRecipeCount +
                finalState.patchCapacityTiers.Sum() +
                finalState.recipeTiers.Sum() +
                finalState.beePurchaseTier;

            result.completionPercentage = (actualUpgrades / (float)maxPossibleUpgrades) * 100f;
            result.decisionsPerWeek = result.totalDecisions / 24f;
        }

        private int GetSeasonIndex(int week)
        {
            if (week <= 8) return 0; // Spring
            if (week <= 16) return 1; // Summer
            return 2; // Autumn
        }

        #endregion

        #region Results UI

        private void DrawSimulationResults()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("=== SIMULATION RESULTS ===", EditorStyles.boldLabel);

            EditorGUILayout.LabelField($"Total Income Generated: ${lastResult.totalIncome:F2}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Final Money: ${lastResult.finalMoney:F2}");
            EditorGUILayout.LabelField($"Total Decisions: {lastResult.totalDecisions}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Avg Decisions/Week: {lastResult.decisionsPerWeek:F1}");
            EditorGUILayout.LabelField($"Completion Percentage: {lastResult.completionPercentage:F1}%", EditorStyles.boldLabel);

            EditorGUILayout.Space();

            // Visual decision density graph (simplified text representation)
            EditorGUILayout.LabelField("Decision Density by Week:", EditorStyles.boldLabel);
            StringBuilder graph = new StringBuilder();
            foreach (var week in lastResult.weekResults)
            {
                graph.AppendLine($"Week {week.week:D2}: {new string('█', week.decisionsThisWeek)} ({week.decisionsThisWeek})");
            }
            EditorGUILayout.TextArea(graph.ToString(), GUILayout.Height(200));

            EditorGUILayout.Space();

            // Income progression
            showDetailedLog = EditorGUILayout.Foldout(showDetailedLog, "Detailed Week-by-Week Results");
            if (showDetailedLog)
            {
                StringBuilder detailed = new StringBuilder();
                foreach (var week in lastResult.weekResults)
                {
                    detailed.AppendLine($"Week {week.week}: Income=${week.incomeGenerated:F2}, Pollen={week.pollenCollected:F0}, Recipes={week.recipesCompleted:F1}, Decisions={week.decisionsThisWeek}");
                }
                EditorGUILayout.TextArea(detailed.ToString(), GUILayout.Height(300));
            }
        }

        #endregion

        #region Data Structures

        private class SimulationState
        {
            public float money;
            public int totalBees;
            public int allocatedBees;
            public int activePatchCount = 0;
            public int activeRecipeCount = 0;
            public int beePurchaseTier = 0;
            public int[] patchCapacityTiers = new int[6];
            public int[] recipeTiers = new int[12];
            public float averageRecipeTier = 0f;
            public int currentWeek;
        }

        private class WeekSimulationResult
        {
            public int week;
            public float incomeGenerated;
            public float pollenCollected;
            public float recipesCompleted;
            public int decisionsThisWeek;
        }

        private class SimulationResult
        {
            public List<WeekSimulationResult> weekResults = new List<WeekSimulationResult>();
            public float totalIncome;
            public float finalMoney;
            public int totalDecisions;
            public float decisionsPerWeek;
            public float completionPercentage;
        }

        #endregion
    }
}
