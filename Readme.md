1) Prérequis :
Avant de commencer, assurez-vous d'avoir installé :
    - Node.js
    - .NET SDK 8.0
    - Postgresql 12

2) Installation des dépendances :
Packages NuGet à installer :
    dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.5
    dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.7
    dotnet add package BCrypt.Net-Next
    dotnet add package System.IdentityModel.Tokens.Jwt
    dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.5
    dotnet add package Microsoft.EntityFrameworkCore
    dotnet add package Swashbuckle.AspNetCore

3) Base de données :
Le projet utilise PostgreSQL comme système de gestion de base de données.

Étapes :
Créez une base de données PostgreSQL.

Copiez-collez le contenu du dossier /base dans votre base de données (il contient les scripts de création de tables, vues, etc.).

4) Lancer le projet :
Compilez et exécutez le projet :
    dotnet build
    dotnet run
