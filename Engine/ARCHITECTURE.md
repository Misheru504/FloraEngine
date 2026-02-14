# Architecture du projet

> Pourquoi faire simple quand on peut faire compliquer ?

Cette documentation sert à expliquer la structure du projet ainsi d'autres choses nécessaire à la compréhension du projet.

Il est utile à mon moi du futur et à quiconque s'intéresse à projet.

Pour rappel, FloraEngine (ou Flora) et un moteur de rendu de monde en voxel généré procéduralement, grâce à Silk.NET et OpenGL.

Dernière mise-à-jour du document : `08/02/2026 - Michel-Ange (Misheru504)`

---

```
Engine
├── Core
├── Diagnostics -> Dépend de Core
├── Physics -> Dépend de Core et World
├── World -> Dépend de Core
├── Rendering -> Dépend de Core et World
├── Player -> Dépend de Core et Physics
└── UI -> Dépend de Core et Diagnostics
```

---

## Roadmap technique
| Version   | Nom      | Focus principal                          |
|-----------|----------|------------------------------------------|
| α-1       | [Pyrite](https://github.com/Misheru504/FloraEngine/releases/tag/alpha1) | Fondations (voir release) |
| α-2       | [Euclase](https://github.com/Misheru504/FloraEngine/releases/tag/alpha2) | Refactor de la structure |
| α-3       | [Rutile](https://github.com/Misheru504/FloraEngine/releases/tag/alpha3) | 🤫 |