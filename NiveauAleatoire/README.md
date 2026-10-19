# Niveau Aléatoire — mod ULTRAKILL

Quand tu finis un niveau (après l'écran de score), le jeu t'envoie dans un **niveau
aléatoire** au lieu du niveau suivant.

- Niveaux possibles : tous les niveaux normaux (0-1 à 7-4), les **Encores** (0-E, 1-E)
  et les **Prime Sanctums** (P-1, P-2).
- Les **niveaux secrets** (x-S) sont toujours exclus.
- Jamais deux fois le même niveau d'affilée.

## Configuration

`BepInEx\config\com.niveaualeatoire.ultrakill.cfg`

- `Levels` : la liste des niveaux possibles (noms de scène, séparés par des virgules).
  Quand de nouveaux niveaux sortent, ajoute-les ici, ex. `Level 8-1`.
- `IncludeEncores` : inclure 0-E et 1-E (oui par défaut)
- `IncludePrimeSanctums` : inclure P-1 et P-2 (oui par défaut)
