using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace ChaosSwap
{
    // Marque les ennemis créés par le mod (ils peuvent être re-transformés, même les boss)
    public class ChaosSwapped : MonoBehaviour { }

    [BepInPlugin("com.chaosswap.ultrakill", "Chaos Swap", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        const string DatabasePath = "Assets/Data/Sandbox/Spawnable Objects Database.asset";

        ConfigEntry<bool> enabled;
        ConfigEntry<float> minInterval;
        ConfigEntry<float> maxInterval;
        ConfigEntry<float> radiantChance;
        ConfigEntry<int> maxRadianceTier;
        ConfigEntry<bool> replaceLevelBosses;
        ConfigEntry<bool> showMessages;
        ConfigEntry<bool> showTimer;
        ConfigEntry<string> blacklist;
        ConfigEntry<KeyCode> forceKey;
        ConfigEntry<KeyCode> toggleKey;

        readonly List<GameObject> pool = new List<GameObject>();
        bool poolLoaded;
        float timer;
        int enemyCount;
        float countRefresh;
        GUIStyle timerStyle;

        void Awake()
        {
            enabled = Config.Bind("General", "Enabled", true, "Active ou désactive le mod.");
            minInterval = Config.Bind("General", "MinInterval", 10f, "Délai minimum (secondes) entre deux transformations.");
            maxInterval = Config.Bind("General", "MaxInterval", 30f, "Délai maximum (secondes) entre deux transformations.");
            radiantChance = Config.Bind("Radiance", "RadiantChance", 0.25f, "Probabilité (0-1) qu'un nouvel ennemi soit radiant.");
            maxRadianceTier = Config.Bind("Radiance", "MaxRadianceTier", 3, "Niveau de radiance maximum (1 = normal radiant).");
            replaceLevelBosses = Config.Bind("General", "ReplaceLevelBosses", false,
                "Transformer aussi les boss d'origine du niveau (peut casser les cinématiques / la fin du niveau).");
            showMessages = Config.Bind("General", "ShowMessages", true, "Affiche un message dans le HUD après chaque transformation.");
            showTimer = Config.Bind("General", "ShowTimer", true, "Affiche le temps restant avant la prochaine transformation.");
            blacklist = Config.Bind("General", "Blacklist", "",
                "Noms d'ennemis (tels qu'affichés dans le menu Sandbox) à ne jamais faire apparaître, séparés par des virgules.");
            forceKey = Config.Bind("Keys", "ForceSwap", KeyCode.F8, "Force une transformation immédiate.");
            toggleKey = Config.Bind("Keys", "Toggle", KeyCode.F9, "Active/désactive le mod en jeu.");

            SceneManager.sceneLoaded += (s, m) => ResetTimer();
            ResetTimer();
            Logger.LogInfo("Chaos Swap chargé !");
        }

        void ResetTimer()
        {
            float min = Mathf.Max(1f, minInterval.Value);
            float max = Mathf.Max(min, maxInterval.Value);
            timer = Random.Range(min, max);
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey.Value))
            {
                enabled.Value = !enabled.Value;
                Hud(enabled.Value ? "<color=red>CHAOS SWAP : ON</color>" : "CHAOS SWAP : OFF");
            }
            if (!enabled.Value) return;

            if (Input.GetKeyDown(forceKey.Value))
            {
                SwapAll();
                ResetTimer();
                return;
            }

            timer -= Time.deltaTime; // Time.deltaTime = 0 en pause

            // Compter les ennemis 2 fois par seconde suffit (FindObjectsOfType est coûteux)
            countRefresh -= Time.unscaledDeltaTime;
            if (countRefresh <= 0f)
            {
                countRefresh = 0.5f;
                enemyCount = GetTargets().Count;
            }
            if (timer <= 0f)
            {
                SwapAll();
                ResetTimer();
            }
        }

        void LoadPool()
        {
            if (poolLoaded) return;
            poolLoaded = true;

            SpawnableObjectsDatabase db = null;
            try
            {
                db = Addressables.LoadAssetAsync<SpawnableObjectsDatabase>(DatabasePath).WaitForCompletion();
            }
            catch (Exception e)
            {
                Logger.LogWarning("Impossible de charger la base Sandbox via Addressables : " + e.Message);
            }
            if (db == null)
                db = Resources.FindObjectsOfTypeAll<SpawnableObjectsDatabase>().FirstOrDefault();
            if (db == null || db.enemies == null)
            {
                Logger.LogError("Aucune base d'ennemis trouvée, le mod ne peut rien faire apparaître.");
                return;
            }

            var banned = new HashSet<string>(
                blacklist.Value.Split(',').Select(x => x.Trim().ToLowerInvariant()).Where(x => x.Length > 0));

            foreach (var entry in db.enemies)
            {
                if (entry == null || entry.gameObject == null) continue;
                if (banned.Contains((entry.objectName ?? "").ToLowerInvariant())) continue;
                if (entry.gameObject.GetComponentInChildren<EnemyIdentifier>(true) == null) continue;
                pool.Add(entry.gameObject);
            }
            Logger.LogInfo($"{pool.Count} ennemis disponibles : " + string.Join(", ", pool.Select(p => p.name)));
        }

        List<EnemyIdentifier> GetTargets()
        {
            var result = new List<EnemyIdentifier>();
            foreach (var eid in FindObjectsOfType<EnemyIdentifier>())
            {
                if (eid == null || eid.dead || !eid.gameObject.activeInHierarchy) continue;
                // Éviter les sous-parties d'un ennemi (ex : EID enfant d'un autre EID)
                if (eid.transform.parent != null && eid.transform.parent.GetComponentInParent<EnemyIdentifier>() != null) continue;

                bool spawnedByUs = eid.GetComponentInParent<ChaosSwapped>() != null;
                bool isLevelBoss = eid.GetComponent<BossHealthBar>() != null;
                if (isLevelBoss && !spawnedByUs && !replaceLevelBosses.Value) continue;

                result.Add(eid);
            }
            return result;
        }

        void SwapAll()
        {
            LoadPool();
            if (pool.Count == 0) return;

            var targets = GetTargets();
            if (targets.Count == 0) return;

            int radiantCount = 0;
            foreach (var eid in targets)
            {
                try
                {
                    if (Swap(eid)) radiantCount++;
                }
                catch (Exception e)
                {
                    Logger.LogWarning($"Échec de la transformation de {eid?.name} : {e}");
                }
            }

            string msg = $"<color=red>CHAOS !</color> {targets.Count} ennemi(s) transformé(s)";
            if (radiantCount > 0) msg += $" (<color=#00ffff>{radiantCount} radiant(s)</color>)";
            Hud(msg);
        }

        // Retourne true si le nouvel ennemi est radiant
        bool Swap(EnemyIdentifier old)
        {
            GameObject prefab = pool[Random.Range(0, pool.Count)];
            Transform oldRoot = old.transform;
            Vector3 pos = oldRoot.position;
            Quaternion rot = Quaternion.Euler(0f, oldRoot.eulerAngles.y, 0f);

            // Les ennemis au sol doivent apparaître sur le NavMesh
            if (prefab.GetComponentInChildren<NavMeshAgent>(true) != null &&
                NavMesh.SamplePosition(pos, out NavMeshHit hit, 50f, NavMesh.AllAreas))
                pos = hit.position;

            GameObject spawned = Instantiate(prefab, pos, rot, oldRoot.parent);
            spawned.AddComponent<ChaosSwapped>();
            spawned.SetActive(true);

            bool radiant = false;
            var neid = spawned.GetComponentInChildren<EnemyIdentifier>(true);
            if (neid != null && Random.value < radiantChance.Value)
            {
                radiant = true;
                neid.radianceTier = Random.Range(1, Mathf.Max(1, maxRadianceTier.Value) + 1);
                neid.BuffAll();
            }

            // On tue l'ancien ennemi pour que les vagues / portes des arènes continuent de fonctionner,
            // puis on le supprime pour qu'il "disparaisse" au profit du nouveau.
            old.InstaKill();
            Destroy(old.gameObject);
            return radiant;
        }

        // Timer affiché en haut au centre de l'écran, seulement quand il y a des ennemis
        void OnGUI()
        {
            if (!enabled.Value || !showTimer.Value || enemyCount == 0) return;

            if (timerStyle == null)
            {
                timerStyle = new GUIStyle(GUI.skin.label)
                {
                    alignment = TextAnchor.UpperCenter,
                    fontStyle = FontStyle.Bold,
                    richText = true
                };
            }
            timerStyle.fontSize = Mathf.RoundToInt(Screen.height / 30f);

            string color = timer <= 3f ? "red" : "white";
            string text = $"<color={color}>CHAOS : {Mathf.CeilToInt(Mathf.Max(0f, timer))}s</color>";
            var rect = new Rect(0f, Screen.height * 0.03f, Screen.width, Screen.height * 0.1f);

            // Ombre pour rester lisible sur fond clair
            var shadow = new Rect(rect.x + 2f, rect.y + 2f, rect.width, rect.height);
            GUI.Label(shadow, $"<color=black>CHAOS : {Mathf.CeilToInt(Mathf.Max(0f, timer))}s</color>", timerStyle);
            GUI.Label(rect, text, timerStyle);
        }

        void Hud(string msg)
        {
            if (!showMessages.Value) return;
            var hud = MonoSingleton<HudMessageReceiver>.Instance;
            if (hud != null) hud.SendHudMessage(msg);
        }
    }
}
