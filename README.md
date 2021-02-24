# 5ESGF-BD-2021
Bienvenue sur le dépôt d'évaluation du cours c#/BigData.

Chaque groupe est invité à créer un Fork de ce dépôt principal muni d'un compte sur Github, travailler sur ce fork au sein du groupe, par le biais de validations, de push sur le server, et de pulls/tirages sur les machines locales des utilisateurs du groupe habilités sur le fork. Une fois le travail effectué et remonté sur le fork, une pull-request sera créée depuis le fork vers le dépôt principal pour fusion et évaluation.

Le fichier de solution "ESGF.Sudoku.Spark.sln" constitue l'environnement de base du travail et s'ouvre dans Visual Studio (attention à bien ouvrir la solution et ne pas rester en "vue de dossier").
En l'état, la solution contient:
- Le projet d'exemples en c# fournis avec Spark.Net est disponible dans la solution, ainsi que les [instructions permettant de les exécuter](Microsoft.Spark.CSharp.Examples).
- Un projet de Console vide "ESGF.Sudoku.Spark.RecursiveSearch.csproj" vide, référençant le package Nuget de .Net pour Sharp, correspondant à la première étape de création du projet de l'un des groupes de travail.

Les autres groupes sont invités à àjouter à la solution leur propre projet de Console créé sur ce modèle.