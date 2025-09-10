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
-- WHERE id_groupe_discussion IS NULL
WHERE (
    (id_expediteur = 5 AND id_destinataire = 6) OR
    (id_expediteur = 6 AND id_destinataire = 5)
)
ORDER BY date_envoie;


SELECT 
    CASE 
        WHEN id_expediteur = 10 THEN id_destinataire
        ELSE id_expediteur
    END AS id_autre_utilisateur,
    
    CASE 
        WHEN id_expediteur = 10 THEN nom_destinataire
        ELSE nom_expediteur
    END AS nom_autre_utilisateur,
    
    'prive' AS type
FROM v_discussions_individuelles
WHERE id_expediteur = 10 OR id_destinataire = 10
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



select id_utilisateur, nom || ' ' || prenom as nom, matricule 
from utilisateur where lower(nom) 
like '%rak%' 
or prenom like '%rak%' 
or matricule like '%rak%';

SELECT *
FROM v_discussions_individuelles
WHERE (
    (id_expediteur = 5 AND id_destinataire = 1) OR
    (id_expediteur = 1 AND id_destinataire = 5)
);

SELECT 
    COALESCE(id_groupe_discussion, id_expediteur) AS id_discussion,
    id_destinataire,
    COUNT(*) AS unread_count
FROM message
WHERE id_expediteur = 1
  AND id_status_msg = 1
GROUP BY COALESCE(id_groupe_discussion, id_expediteur, id_destinataire);

UPDATE message
SET id_status_msg = 2
WHERE id_destinataire = @idUser
  AND COALESCE(id_groupe_discussion, id_expediteur) = @idDiscussion
  AND id_status_msg = 1;


  SELECT 
    COALESCE(id_groupe_discussion, id_expediteur) AS id_discussion,
    COUNT(*) AS unread_count
FROM message
WHERE id_destinataire = 7
AND id_status_msg = 1
GROUP BY COALESCE(id_groupe_discussion, id_expediteur);




-- Messages privés non lus
SELECT 
    id_expediteur AS id_discussion,
    COUNT(*) AS unread_count
FROM message
WHERE id_destinataire = 1
  AND id_status_msg = 1
GROUP BY id_expediteur

UNION ALL

-- Messages de groupe non lus
SELECT 
    m.id_groupe_discussion AS id_discussion,
    COUNT(*) AS unread_count
FROM message msg
JOIN utilisateur_groupe_discussion m ON m.id_groupe_discussion = msg.id_groupe_discussion
WHERE m.id_utilisateur = 1
  AND msg.id_status_msg = 1
GROUP BY m.id_groupe_discussion;











SELECT 
    id_expediteur AS id,
    'prive' AS type,
    COUNT(*) AS unread_count
FROM message
WHERE id_destinataire = 8
AND id_status_msg = 1
GROUP BY id_expediteur

UNION ALL

SELECT 
    m.id_groupe_discussion AS id,
    'groupe' AS type,
    COUNT(*) AS unread_count
FROM message msg
JOIN utilisateur_groupe_discussion m ON m.id_groupe_discussion = msg.id_groupe_discussion
WHERE m.id_utilisateur = 1
AND msg.id_status_msg = 1
GROUP BY m.id_groupe_discussion;




select 
    m.id_message,
    m.id_expediteur,
    u.nom || ' ' || u.prenom AS nom_expediteur,
    m.id_destinataire,
    m.id_groupe_discussion,
    m.contenu,
    m.date_envoie,
    m.id_status_msg
from message m 
join utilisateur u on u.id_utilisateur = m.id_expediteur 
order by id_message desc;



INSERT INTO message_utilisateur_statut (id_message, id_utilisateur, id_status_msg)
SELECT m.id_message, 1, 3 -- 3 = lu
FROM message m
LEFT JOIN message_utilisateur_statut s
  ON s.id_message = m.id_message AND s.id_utilisateur = 1 AND s.id_status_msg = 3
WHERE m.id_expediteur = 203
          AND m.id_destinataire = 1
          AND s.id_utilisateur IS NULL;





-- Messages privés non lus
        SELECT 
            m.id_expediteur AS id,
            'prive' AS type,
            COUNT(*) AS unread_count
        FROM message m
        LEFT JOIN message_utilisateur_statut mus 
            ON mus.id_message = m.id_message AND mus.id_utilisateur = 7 AND mus.id_status_msg = 1 -- 3 = lu
        WHERE m.id_destinataire = 7
        AND mus.id_message_utilisateur_statut IS NULL
        GROUP BY m.id_expediteur

        UNION ALL

        -- Messages de groupe non lus
        SELECT 
            m.id_groupe_discussion AS id,
            'groupe' AS type,
            COUNT(*) AS unread_count
        FROM message m
        JOIN utilisateur_groupe_discussion ugd ON ugd.id_groupe_discussion = m.id_groupe_discussion
        LEFT JOIN message_utilisateur_statut mus 
            ON mus.id_message = m.id_message AND mus.id_utilisateur = 7 AND mus.id_status_msg = 1
        WHERE ugd.id_utilisateur = 7
        AND m.id_expediteur != 7
        AND mus.id_message_utilisateur_statut IS NULL
        GROUP BY m.id_groupe_discussion;


SELECT 
  m.*, 
  EXISTS (
    SELECT 1 
    FROM message_utilisateur_statut mus 
    WHERE mus.id_message = m.id_message 
      AND mus.id_utilisateur = 1
      AND mus.id_status_msg = 3
  ) AS est_lu
FROM v_utilisateur_message m
WHERE ...



-- exemple pseudo SQL
SELECT 
    v.*,
    ARRAY(
        SELECT u.prenom
        FROM message_utilisateur_statut mus
        JOIN utilisateur u ON u.id_utilisateur = mus.id_utilisateur
        WHERE mus.id_message = v.id_message AND mus.id_status_msg = 3
    ) AS liste_utilisateur_vu
FROM v_utilisateur_message v
WHERE v.id_groupe_discussion = 5
ORDER BY v.id_message ASC;



SELECT 
    v.*, 
    EXISTS (
        SELECT 1 
        FROM message_utilisateur_statut mus 
        WHERE mus.id_message = v.id_message 
            AND mus.id_utilisateur != 1
            AND mus.id_status_msg = 3
    ) AS est_lu,
    ARRAY(
        SELECT u.prenom
        FROM message_utilisateur_statut mus
        JOIN utilisateur u ON u.id_utilisateur = mus.id_utilisateur
        WHERE mus.id_message = v.id_message AND mus.id_status_msg = 3
    ) AS liste_utilisateur_vu
FROM v_utilisateur_message v
WHERE v.id_groupe_discussion = 8
ORDER BY v.id_message ASC



SELECT g.id_groupe_discussion AS id, g.nom, 'groupe' AS type
FROM groupe_discussion g
JOIN utilisateur_groupe ug ON ug.id_groupe_discussion = g.id_groupe_discussion
WHERE ug.id_utilisateur = @idUtilisateur
  AND LOWER(g.nom) LIKE LOWER(@searchTerm)





select 
    pj.id_piece_joint, 
    pj.id_message, 
    pj.id_type_piece_joint, 
    pj.chemin, 
    pj.date_ajout, 
    tpj.type 
from piece_joint pj 
join type_piece_joint tpj on tpj.id_type_piece_joint = pj.id_type_piece_joint;


select 
        m.id_message,
        m.id_expediteur,
        u.nom || ' ' || u.prenom AS nom_expediteur,
        m.id_destinataire,
        m.id_groupe_discussion,
        m.contenu,
        m.date_envoie,
        m.id_status_msg,
        pj.id_piece_joint,
        pj.chemin
    from message m 
    join utilisateur u on u.id_utilisateur = m.id_expediteur 
    left join piece_joint pj on pj.id_message = m.id_message
    order by id_message desc


SELECT 
                v.*,
                EXISTS (
                    SELECT 1 
                    FROM message_utilisateur_statut mus 
                    WHERE mus.id_message = v.id_message 
                      AND mus.id_utilisateur = 7
                      AND mus.id_status_msg = 3
                ) AS est_lu
            FROM v_utilisateur_message v
            WHERE 
                (v.id_expediteur = 7 AND v.id_destinataire = 1)
             OR (v.id_expediteur = 1 AND v.id_destinataire = 7)
            ORDER BY v.id_message ASC;