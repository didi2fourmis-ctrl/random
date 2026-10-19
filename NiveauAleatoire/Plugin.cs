using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace NiveauAleatoire
{
    [BepInPlugin("com.niveaualeatoire.ultrakill", "Niveau Aleatoire", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        // Noms des scènes du jeu. Les niveaux secrets (x-S) sont volontairement absents.
        const string NiveauxNormaux =
            "Level 0-1, Level 0-2, Level 0-3, Level 0-4, Level 0-5, " +
            "Level 1-1, Level 1-2, Level 1-3, Level 1-4, " +
            "Level 2-1, Level 2-2, Level 2-3, Level 2-4, " +
            "Level 3-1, Level 3-2, " +
            "Level 4-1, Level 4-2, Level 4-3, Level 4-4, " +
            "Level 5-1, Level 5-2, Level 5-3, Level 5-4, " +
            "Level 6-1, Level 6-2, " +
            "Level 7-1, Level 7-2, Level 7-3, Level 7-4";
        const string NiveauxEncore = "Level 0-E, Level 1-E";
        const string NiveauxPrime = "Level P-1, Level P-2";

        static Plugin instance;
        static bool inLevelChange;

        ConfigEntry<bool> enabled;
        ConfigEntry<string> levels;
        ConfigEntry<bool> includeEncores;
        ConfigEntry<bool> includePrimes;

        void Awake()
        {
            instance = this;
            enabled = Config.Bind("General", "Enabled", true, "Active ou désactive le mod.");
            levels = Config.Bind("General", "Levels", NiveauxNormaux,
                "Niveaux possibles, séparés par des virgules. Ajoute les nouveaux niveaux ici quand ils sortent (ex : Level 8-1).");
            includeEncores = Config.Bind("General", "IncludeEncores", true, "Inclure les niveaux Encore (" + NiveauxEncore + ").");
            includePrimes = Config.Bind("General", "IncludePrimeSanctums", true, "Inclure les Prime Sanctums (" + NiveauxPrime + ").");

            var harmony = new Harmony("com.niveaualeatoire.ultrakill");

            // FinalRank.LevelChange = le moment où le joueur quitte l'écran de score de fin de niveau
            foreach (var m in typeof(FinalRank).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                         .Where(m => m.Name == "LevelChange"))
            {
                harmony.Patch(m,
                    prefix: new HarmonyMethod(typeof(Plugin), nameof(BeforeLevelChange)),
                    finalizer: new HarmonyMethod(typeof(Plugin), nameof(AfterLevelChange)));
            }

            // Pendant ce LevelChange, on remplace la scène que le jeu veut charger
            foreach (var m in typeof(SceneHelper).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                         .Where(m => m.Name == "LoadScene" && m.GetParameters().FirstOrDefault()?.ParameterType == typeof(string)))
            {
                harmony.Patch(m, prefix: new HarmonyMethod(typeof(Plugin), nameof(BeforeLoadScene)));
            }

            Logger.LogInfo("Niveau Aleatoire chargé !");
        }

        static void BeforeLevelChange() => inLevelChange = true;

        static Exception AfterLevelChange(Exception __exception)
        {
            inLevelChange = false;
            return __exception;
        }

        static void BeforeLoadScene(ref string __0)
        {
            if (!inLevelChange || instance == null || !instance.enabled.Value) return;
            inLevelChange = false;

            string next = instance.PickLevel();
            if (next == null) return;

            instance.Logger.LogInfo($"Fin de niveau : {__0} remplacé par {next}");
            __0 = next;
        }

        string PickLevel()
        {
            var list = new List<string>(Split(levels.Value));
            if (includeEncores.Value) list.AddRange(Split(NiveauxEncore));
            if (includePrimes.Value) list.AddRange(Split(NiveauxPrime));

            // Pas deux fois le même niveau d'affilée
            string current = SceneManager.GetActiveScene().name;
            list = list.Distinct().Where(l => l != current && !l.EndsWith("-S")).ToList(); // jamais de niveau secret
            if (list.Count == 0) return null;

            return list[Random.Range(0, list.Count)];
        }

        static IEnumerable<string> Split(string s) =>
            s.Split(',').Select(x => x.Trim()).Where(x => x.Length > 0);
    }
}
