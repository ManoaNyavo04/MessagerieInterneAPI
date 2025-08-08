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



SELECT *
FROM message
WHERE id_groupe_discussion IS NULL
AND (
    (id_expediteur = 5 AND id_destinataire = 6) OR
    (id_expediteur = 6 AND id_destinataire = 5)
)
ORDER BY date_envoie;


SELECT 
    CASE 
        WHEN id_expediteur = 7 THEN id_destinataire
        ELSE id_expediteur
    END AS id_autre_utilisateur,
    
    CASE 
        WHEN id_expediteur = 7 THEN nom_destinataire
        ELSE nom_expediteur
    END AS nom_autre_utilisateur,
    
    'prive' AS type
FROM v_discussions_individuelles
WHERE id_expediteur = 7 OR id_destinataire = 7
GROUP BY id_autre_utilisateur, nom_autre_utilisateur;



SELECT 
    CASE 
        WHEN id_expediteur = @userId THEN id_destinataire
        ELSE id_expediteur
    END AS id_autre_utilisateur,
    
    CASE 
        WHEN id_expediteur = @userId THEN nom_destinataire
        ELSE nom_expediteur
    END AS nom_autre_utilisateur,
    
    'prive' AS type
FROM v_discussions_individuelles
WHERE id_expediteur = @userId OR id_destinataire = @userId
GROUP BY id_autre_utilisateur, nom_autre_utilisateur;



select u.id_utilisateur, u.nom, u.prenom, u.matricule, u.mdp, u.id_role, r.role 
from utilisateur u  
join role r on r.id_role = u.id_role order by u.id_utilisateur;


