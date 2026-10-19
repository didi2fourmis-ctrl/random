using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using UnityEngine;

namespace EnnemisInvisibles
{
    [BepInPlugin("com.ennemisinvisibles.ultrakill", "Ennemis Invisibles", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        ConfigEntry<bool> enabled;
        ConfigEntry<bool> hideEffects;
        ConfigEntry<KeyCode> toggleKey;

        // Renderers qu'on a cachés nous-mêmes (pour pouvoir les réafficher avec la touche)
        readonly HashSet<Renderer> hidden = new HashSet<Renderer>();
        float scanTimer;

        void Awake()
        {
            enabled = Config.Bind("General", "Enabled", true, "Active ou désactive le mod.");
            hideEffects = Config.Bind("General", "HideEffects", false,
                "Cacher aussi les effets attachés aux ennemis (particules, traînées, flash d'attaque, aura radiante...). " +
                "Désactivé = seul le corps des ennemis est invisible, les attaques se voient encore un peu.");
            toggleKey = Config.Bind("Keys", "Toggle", KeyCode.F10, "Active/désactive l'invisibilité en jeu.");
            Logger.LogInfo("Ennemis Invisibles chargé !");
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey.Value))
            {
                enabled.Value = !enabled.Value;
                if (!enabled.Value) ShowAll();
                else scanTimer = 0f;
                var hud = MonoSingleton<HudMessageReceiver>.Instance;
                if (hud != null) hud.SendHudMessage(enabled.Value ? "ENNEMIS INVISIBLES : ON" : "ENNEMIS INVISIBLES : OFF");
            }
            if (!enabled.Value) return;

            // Scan 10 fois par seconde : attrape les ennemis qui apparaissent
            // et les morceaux que le jeu réactive (téléportations, changements d'armes...)
            scanTimer -= Time.unscaledDeltaTime;
            if (scanTimer > 0f) return;
            scanTimer = 0.1f;
            HideAll();
        }

        void HideAll()
        {
            hidden.RemoveWhere(r => r == null);
            foreach (var eid in FindObjectsOfType<EnemyIdentifier>())
            {
                foreach (var r in eid.GetComponentsInChildren<Renderer>(true))
                {
                    if (!r.enabled) continue;
                    if (!hideEffects.Value && !(r is MeshRenderer || r is SkinnedMeshRenderer)) continue;
                    r.enabled = false;
                    hidden.Add(r);
                }
            }
        }

        void ShowAll()
        {
            foreach (var r in hidden)
                if (r != null) r.enabled = true;
            hidden.Clear();
        }
    }
}
