select 
    u.id_utilisateur,
    gd.id_groupe_discussion,
    u.nom, 
    u.prenom, 
    u.matricule, 
    gd.nom as groupe,
    'groupe' AS type
from utilisateur_groupe_discussion ugd 
join utilisateur u on u.id_utilisateur = ugd.id_utilisateur 
join groupe_discussion gd on gd.id_groupe_discussion = ugd.id_groupe_discussion;


-- utilisateur groupe discussion view
select 
    gd.id_groupe_discussion, 
    gd.nom, 
    gd.description, 
    u.id_utilisateur, 
    u.matricule, 
    u.nom as nom_user, 
    u.prenom, ugd.est_admin 
from groupe_discussion gd 
join utilisateur_groupe_discussion ugd on gd.id_groupe_discussion = ugd.id_groupe_discussion 
join utilisateur u on u.id_utilisateur = ugd.id_utilisateur;

-- utilisateur message 1-1 
SELECT DISTINCT u.id_utilisateur AS id, u.nom AS nom, 'prive' AS type
FROM message m
JOIN utilisateur u
    ON (u.id_utilisateur = m.id_expediteur AND m.id_destinataire = 5)
    OR (u.id_utilisateur = m.id_destinataire AND m.id_expediteur = 5)
WHERE m.id_groupe_discussion IS NULL
  AND u.id_utilisateur != 5;

-- mes discussions 1-1
CREATE VIEW v_messages_prives AS
SELECT DISTINCT
    u.id_utilisateur AS id_utilisateur,
    u.nom AS nom,
    m.id_expediteur,
    m.id_destinataire,
    'prive' AS type
FROM message m
JOIN utilisateur u
    ON u.id_utilisateur = m.id_expediteur OR u.id_utilisateur = m.id_destinataire
WHERE m.id_groupe_discussion IS NULL;

-- mes groupes discussions
CREATE VIEW v_groupes_utilisateur AS
SELECT 
    gd.id_groupe_discussion AS id,
    gd.nom AS nom,
    gm.id_utilisateur AS id_utilisateur,
    'groupe' AS type
FROM groupe_discussion gd
JOIN groupe_membres gm ON gm.id_groupe_discussion = gd.id_groupe_discussion;
