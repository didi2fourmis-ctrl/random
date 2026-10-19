# Mods ULTRAKILL

| Mod | Ce qu'il fait |
|---|---|
| [ChaosSwap](ChaosSwap/README.md) | Toutes les 10 s, chaque ennemi est transformé en un ennemi aléatoire (boss et radiants compris), avec un timer à l'écran |
| [NiveauAleatoire](NiveauAleatoire/README.md) | À la fin d'un niveau, t'envoie dans un niveau aléatoire (Encores et Prime Sanctums compris, sans les niveaux secrets) |
| [EnnemisInvisibles](EnnemisInvisibles/README.md) | Rend tous les ennemis invisibles |

## Installation

1. Installer **BepInEx 5** (x64) dans le dossier d'ULTRAKILL (ou via r2modman), lancer le
   jeu une fois puis le fermer.
2. Installer le **SDK .NET** (8 ou plus) : https://dotnet.microsoft.com/download
3. Si ton jeu n'est pas dans `C:\Program Files (x86)\Steam\steamapps\common\ULTRAKILL`,
   change le chemin `GamePath` dans `Directory.Build.props` (une seule fois pour tous les mods).
4. Double-clique sur **`compiler.bat`** : tous les mods sont compilés et copiés dans
   `ULTRAKILL\BepInEx\plugins\<NomDuMod>\`.

Tu ne veux pas un des mods ? Supprime son dossier dans `BepInEx\plugins`.
