# Décisions sur le design

> Ca fait beaucoup la, nan ?

Ce document explique les differentes décisions en lien avec le design. Il explique pourquoi cette décision a été prise. Aucun retour en arrière, si une décision est révisée, nouvelle enregistrement dans ce fichier pour expliquer pourquoi. Si il manque une décision "majeure" (qui a besoin d'une explication ou qui change une décision d'ici), merci de le signaler ou de l'ajouter.

Ce document est utile à mon moi du futur et à quiconque s'intéresse à projet.

Pour rappel, FloraEngine (ou Flora) et un moteur de rendu de monde en voxel généré procéduralement, grâce à Silk.NET et OpenGL.

Dernière mise-à-jour du document : `08/02/2026 - Michel-Ange (Misheru504)`

---
<details>
<summary>DEC-001 : Création du projet</summary>

**Date** : 07-01-2026 |
**Statut** : Adopté

**Contexte** :
Nécéssité de refactoriser le code LizardEngine, maintenant que les bases de Silk.NET ont été apprises.

**Options** :
1. **Rester sur LizardEngine** — Aucun nouveau projet / Besoin de refactoriser tout le code (long)
2. **Création d'un nouveau projet** — Refactorisation simple, rebranding possible / Besoin de remettre en place un GitHub

**Décision** : Création d'un nouveau projet

**Conséquences** :
- Refactorisation simple et permet un rebranding (changement de nom)
- Création d'un nouveau dépot GitHub, chronophage
</details>


<details>
<summary>DEC-002 : Taille des chunks</summary>

**Date** : 07-01-2026 |
**Statut** : Adopté

**Contexte** :
Les chunks de grande taille mettent plus de temps à se générer (volume multiplié par 8 lorsque la largeur est multiplié par 2), besoin d'un compromis.

**Options** :
1. **16x16x16** — Chunks petit, rapide à utiliser / Prennent peu de place sur le monde, besoin de plus
2. **32x32x32** — Grand chunks sur le terrain, couvre une grande surface / Lourd dans la mémoire

**Décision** : 16x16x16

**Conséquences** :
- Réduction de l'utilisation de la mémoire et du temps de génération/meshing
- Augmentation du nombre (peut aller jusqu'à 1000)
- Conditions de réévaluation: Nouvel algorithme de génération
</details>

<details>
<summary>DEC-003 : Greedy meshing</summary>

**Date** : 11-01-2026 |
**Statut** : Adopté

**Contexte** :
Le culled meshing est rapide mais peu efficace en optimisation de mesh, l'ajout de greedy meshing pourrait grandement diminuer le nombre de triangles

**Options** :
1. **Greedy meshing uniquement** — Optimisation des mesh / Difficile de tester facilement
2. **Culled meshing uniquement** — Facile à changer et maintenir / Peu efficace en nombre de triangle
2. **Garder les deux** — Combine les avantages des deux (toggle pour changer entre l'un ou l'autre) / Maintenance double

**Décision** : Garder les deux

**Conséquences** :
- Facile de tester avec le culled meshing, optimisé grâce au greedy meshing final
- Maintenance double, chaque changement fait sur l'un (ou la structure d'un triangle) doit être modifier sur l'autre
- Conditions de réévaluation: Déploiement en production, retirer l'un des deux
</details>

<details>
<summary>DEC-004 : Proto-chunks</summary>

**Date** : 25-01-2026 |
**Statut** : Révisé (voir DEC-006)

**Contexte** :
La génération de détails sur le terrain et le meshing cross chunk a besoin des chunks adjacents.
Un proto chunk permettrai de mettre la génération du chunk en pause, et sera fini lorsque c'est nécéssaire.

**Options** :
1. **Création du chunk à la volée** — Pas besoin des chunks adjacents / Difficulté d'implémenation
2. **Création d'un proto-chunk** — Facile à mettre en place / Nécéssite plus de mémoire

**Décision** : Création d'un proto-chunk

**Conséquences** :
- Facile à mettre en place
- Utilise plus de mémoire et à besoin de garder les chunks dans cet état dans une mémoire séparé
- Conditions de réévaluation: Nouveau système de génération
</details>

<details>
<summary>DEC-005 : Renommer le projet</summary>

**Date** : 28-01-2026 |
**Statut** : Adopté

**Contexte** :
Le nom FloreEngine ressemble beaucoup à Floor, qui perd son sens en anglais. Proposition de traduire le nom en anglais: Flora

**Options** :
1. **FloreEngine** — Aucun changement sur le code / Ambiguité anglophone
2. **FloraEngine** — Nom qui sonne mieux en anglais / Refactorisation du code

**Décision** : FloraEngine

**Conséquences** :
- Nom plus clair
- Refactorisation nécéssaire
</details>

<details>
<summary>DEC-006 : Changement proto-chunks</summary>

**Date** : 06-02-2026 |
**Statut** : Adopté

**Contexte** :
Les protos chunks sont difficiles a géré dans le générateur et coutent chère en mémoire. Il est possible de générer les features sans les chunks adjacents.

**Options** :
1. **Garder les proto chunks** — Facile à concevoir / Difficile à mettre en place en multithreading]
2. **Changement pour un nouveau système** — Les chunks n'ont plus besoin des autres pour la génération / Difficle à implementer

**Décision** : Changement pour un nouveau système

**Conséquences** :
- Multithreading plus simple
- Création des features plus difficile
- Conditions de réévaluation: Implémentation trop complexe
</details>

<details>
<summary>DEC-007 : Restructuration du projet</summary>

**Date** : 08-02-2026 |
**Statut** : Adopté

**Contexte** :
Le projet commence a être tentaculaire, et a s'éparpiller dans tout les sens. Il est nécéssaire de refactoriser le code maintenant pour le rendre plus simple à maintenir

**Options** :
1. **Restructuration maintenant** — Une fois fait, plus aucun problème / Très chronophage, bloque tout le projet
2. **Restructuration étalé dans le temps** — Simple a faire, petit à petit / Long dans dans le temps, peut prendre plus de temps à terme

**Décision** : Restructuration maintenant

**Conséquences** :
- Code final plus simple à maintenir et comprendre
- Bloquage du projet jusqu'à la fin de la restructuration
</details>

---

## Template pour les prochaines décisions

```
<details>
<summary>DEC-XXX : [Titre court]</summary>

**Date** : DD-MM-YYYY |
**Statut** : Adopté / Révisé (voir DEC-YYY) / Abandonné

**Contexte** :
[Quel problème on résout ? Pourquoi maintenant ?]

**Options** :
1. **Option A** — [Avantages / Inconvénients]
2. **Option B** — [Avantages / Inconvénients]

**Décision** : [Ce qui a été choisi]

**Conséquences** :
- [Impact positif]
- [Impact négatif / dette technique acceptée]
- Conditions de réévaluation: [Condition(s)]
</details>
```
