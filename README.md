# Chaos Swap — mod ULTRAKILL

Toutes les **10 à 30 secondes** (aléatoire), chaque ennemi vivant est transformé en un
**ennemi aléatoire** : ennemis de base, boss (Minos Prime, Sisyphus Prime, V2, Gabriel…
selon ce que le jeu propose dans le Sandbox), et chaque nouvel ennemi a **25 % de chances
d'être radiant** (niveau 1 à 3).

## Installation

1. Installer **BepInEx 5** (x64) dans le dossier d'ULTRAKILL
   (ou via r2modman / Thunderstore Mod Manager), lancer le jeu une fois puis le fermer.
2. Installer le **SDK .NET** (8 ou plus) : https://dotnet.microsoft.com/download
3. Double-clique sur `compiler.bat`, ou dans ce dossier, compiler :
   ```
   dotnet build -c Release -p:GamePath="C:\Program Files (x86)\Steam\steamapps\common\ULTRAKILL"
   ```
   (adapte le chemin si ton jeu est ailleurs : Steam → clic droit ULTRAKILL → Gérer → Parcourir les fichiers locaux)
4. La DLL est copiée automatiquement dans `ULTRAKILL\BepInEx\plugins\ChaosSwap\ChaosSwap.dll`.
   Sinon, copie `bin\Release\net471\ChaosSwap.dll` dans `BepInEx\plugins` à la main.

## Touches

| Touche | Action |
|---|---|
| **F8** | Forcer une transformation tout de suite |
| **F9** | Activer / désactiver le mod |

## Configuration

Après le premier lancement : `BepInEx\config\com.chaosswap.ultrakill.cfg`

- `MinInterval` / `MaxInterval` : délai entre les transformations (10 / 30 par défaut)
- `RadiantChance` : probabilité d'un ennemi radiant (0.25)
- `MaxRadianceTier` : niveau de radiance max (3)
- `ReplaceLevelBosses` : transformer aussi les boss d'origine du niveau (désactivé par
  défaut car ça peut casser la fin du niveau / les cinématiques). Les boss **apparus grâce
  au mod** sont toujours re-transformés.
- `Blacklist` : ennemis à ne jamais faire apparaître, ex. `Cancerous Rodent, Very Cancerous Rodent`
- `ShowMessages` : messages dans le HUD

## Fonctionnement

- La liste des ennemis vient de la base de données du Sandbox du jeu, donc les mises à
  jour d'ULTRAKILL ajoutent automatiquement les nouveaux ennemis. La liste trouvée est
  écrite dans `BepInEx\LogOutput.log`.
- L'ancien ennemi est tué instantanément (pour que les vagues et portes des arènes
  continuent de fonctionner) puis remplacé au même endroit. Les ennemis au sol sont
  replacés sur le NavMesh.
- Le timer s'arrête quand le jeu est en pause.
