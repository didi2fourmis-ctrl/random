using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace NiveauAleatoire
{
    [BepInPlugin("com.niveaualeatoire.ultrakill", "Niveau Aleatoire", "1.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        // Noms des scènes du jeu. Les niveaux secrets (x-S) sont volontairement absents.
        static readonly string[] NiveauxNormaux =
        {
            "Level 0-1", "Level 0-2", "Level 0-3", "Level 0-4", "Level 0-5",
            "Level 1-1", "Level 1-2", "Level 1-3", "Level 1-4",
            "Level 2-1", "Level 2-2", "Level 2-3", "Level 2-4",
            "Level 3-1", "Level 3-2",
            "Level 4-1", "Level 4-2", "Level 4-3", "Level 4-4",
            "Level 5-1", "Level 5-2", "Level 5-3", "Level 5-4",
            "Level 6-1", "Level 6-2",
            "Level 7-1", "Level 7-2", "Level 7-3", "Level 7-4",
            "Level 8-1", "Level 8-2", "Level 8-3", "Level 8-4",
        };
        static readonly string[] NiveauxEncore = { "Level 0-E", "Level 1-E" };
        static readonly string[] NiveauxPrime = { "Level P-1", "Level P-2" };

        static Plugin instance;
        // Niveau tiré au sort à la fin du niveau, appliqué au prochain chargement de scène
        static string pendingLevel;

        // Vrai hasard, indépendant de la graine de Unity
        readonly Random rng = new Random(Guid.NewGuid().GetHashCode());
        // "Sac" de niveaux : on les fait tous avant d'en refaire un
        readonly List<string> bag = new List<string>();

        ConfigEntry<bool> enabled;
        ConfigEntry<bool> includeEncores;
        ConfigEntry<bool> includePrimes;
        ConfigEntry<bool> noRepeat;
        ConfigEntry<string> extraLevels;
        ConfigEntry<string> excludedLevels;

        void Awake()
        {
            instance = this;
            enabled = Config.Bind("General", "Enabled", true, "Active ou désactive le mod.");
            includeEncores = Config.Bind("General", "IncludeEncores", true, "Inclure les niveaux Encore (0-E, 1-E).");
            includePrimes = Config.Bind("General", "IncludePrimeSanctums", true, "Inclure les Prime Sanctums (P-1, P-2).");
            noRepeat = Config.Bind("General", "NoRepeatUntilAllPlayed", true,
                "Ne jamais refaire un niveau tant que tous les autres n'ont pas été tirés (ordre toujours aléatoire).");
            extraLevels = Config.Bind("General", "ExtraLevels", "",
                "Niveaux en plus (noms de scène séparés par des virgules), ex : Level 9-1");
            excludedLevels = Config.Bind("General", "ExcludedLevels", "",
                "Niveaux à ne jamais tirer, ex : Level 0-1, Level 0-2");

            var harmony = new Harmony("com.niveaualeatoire.ultrakill");
            const BindingFlags all = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            // FinalRank.LevelChange = le joueur quitte l'écran de score de fin de niveau
            int patched = 0;
            foreach (var m in typeof(FinalRank).GetMethods(all).Where(m => m.Name == "LevelChange"))
            {
                harmony.Patch(m, prefix: new HarmonyMethod(typeof(Plugin), nameof(BeforeLevelChange)));
                patched++;
            }

            // Toutes les fonctions de chargement de scène du jeu (LoadScene, LoadSceneAsync...)
            foreach (var m in typeof(SceneHelper).GetMethods(all)
                         .Where(m => m.Name.StartsWith("LoadScene") && m.GetParameters().FirstOrDefault()?.ParameterType == typeof(string)))
            {
                harmony.Patch(m, prefix: new HarmonyMethod(typeof(Plugin), nameof(BeforeLoadScene)));
                patched++;
            }
            Logger.LogInfo($"Niveau Aleatoire chargé ! ({patched} fonctions du jeu modifiées)");

            SceneManager.sceneLoaded += (s, mode) => pendingLevel = null;
        }

        static void BeforeLevelChange(FinalRank __instance)
        {
            if (instance == null || !instance.enabled.Value || pendingLevel != null) return;

            pendingLevel = instance.PickLevel();
            if (pendingLevel == null) return;
            instance.Logger.LogInfo($"Fin de niveau : prochain niveau tiré au sort = {pendingLevel}");

            // On remplace aussi directement la destination de l'écran de score
            var field = AccessTools.Field(typeof(FinalRank), "targetLevelName");
            if (field != null && field.FieldType == typeof(string))
                field.SetValue(__instance, pendingLevel);
        }

        static void BeforeLoadScene(ref string __0)
        {
            if (pendingLevel == null || __0 == pendingLevel) return;
            // Le joueur retourne au menu : on ne touche à rien
            if (__0 == "Main Menu") { pendingLevel = null; return; }

            instance.Logger.LogInfo($"Chargement de {__0} remplacé par {pendingLevel}");
            __0 = pendingLevel;
        }

        List<string> AllLevels()
        {
            var list = new List<string>(NiveauxNormaux);
            if (includeEncores.Value) list.AddRange(NiveauxEncore);
            if (includePrimes.Value) list.AddRange(NiveauxPrime);
            list.AddRange(Split(extraLevels.Value));
            var excluded = new HashSet<string>(Split(excludedLevels.Value));
            return list.Distinct().Where(l => !excluded.Contains(l) && !l.EndsWith("-S")).ToList(); // jamais de niveau secret
        }

        string PickLevel()
        {
            string current = SceneManager.GetActiveScene().name;
            var all = AllLevels();
            if (all.Count == 0) return null;

            if (!noRepeat.Value)
            {
                var choices = all.Where(l => l != current).ToList();
                if (choices.Count == 0) choices = all;
                return choices[rng.Next(choices.Count)];
            }

            bag.RemoveAll(l => !all.Contains(l));
            bag.Remove(current);
            if (bag.Count == 0)
            {
                bag.AddRange(all);
                bag.Remove(current);
                if (bag.Count == 0) bag.AddRange(all);
            }
            int i = rng.Next(bag.Count);
            string next = bag[i];
            bag.RemoveAt(i);
            return next;
        }

        static IEnumerable<string> Split(string s) =>
            s.Split(',').Select(x => x.Trim()).Where(x => x.Length > 0);
    }
}
