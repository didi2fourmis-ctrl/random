# Niveau Aléatoire — mod ULTRAKILL

Quand tu finis un niveau (après l'écran de score), le jeu t'envoie dans un **niveau
aléatoire** au lieu du niveau suivant, n'importe où entre **0-1 et 8-4**.

- Niveaux possibles : tous les niveaux normaux (0-1 à 8-4), les **Encores** (0-E, 1-E)
  et les **Prime Sanctums** (P-1, P-2).
- Les **niveaux secrets** (x-S) sont toujours exclus.
- Tirage imprévisible, et par défaut **aucun niveau ne revient tant que tous les autres
  n'ont pas été faits** (dans un ordre aléatoire différent à chaque fois).

## Configuration

`BepInEx\config\com.niveaualeatoire.ultrakill.cfg`

- `IncludeEncores` : inclure 0-E et 1-E (oui par défaut)
- `IncludePrimeSanctums` : inclure P-1 et P-2 (oui par défaut)
- `NoRepeatUntilAllPlayed` : pas de répétition avant d'avoir tout fait (oui par défaut).
  Mets `false` pour un tirage 100 % au hasard à chaque fois.
- `ExtraLevels` : niveaux à ajouter quand de nouveaux sortent, ex. `Level 9-1`
- `ExcludedLevels` : niveaux à ne jamais tirer

## Ça ne marche pas ?

Ouvre `BepInEx\LogOutput.log` et cherche `Niveau Aleatoire` :
- `prochain niveau tiré au sort = ...` doit apparaître à la fin de chaque niveau ;
- si cette ligne n'apparaît jamais, envoie le fichier de log.
