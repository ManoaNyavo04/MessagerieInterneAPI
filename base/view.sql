CREATE OR REPLACE VIEW v_info_utilisateur AS (
    select 
    u.id_utilisateur, 
    u.nom, 
    u.prenom, 
    u.matricule, 
    u.mdp, 
    u.id_role, 
    r.role,
    u.deleted
    from utilisateur u  
    join role r on r.id_role = u.id_role order by u.id_utilisateur
);

CREATE OR REPLACE VIEW v_utilisateur_groupe_discussion AS (
    select 
        gd.id_groupe_discussion,
        gd.nom as groupe ,
        gd.description,
        u.id_utilisateur,
        u.matricule, 
        u.nom, 
        u.prenom, 
        ugd.est_admin,
        ugd.statuts,
        gd.id_espace_travail,
        'groupe' AS type 
    from utilisateur_groupe_discussion ugd 
    join utilisateur u on u.id_utilisateur = ugd.id_utilisateur 
    join groupe_discussion gd on gd.id_groupe_discussion = ugd.id_groupe_discussion
    order by u.id_utilisateur 
);

CREATE VIEW v_discussions_individuelles AS
    SELECT DISTINCT
        u.id_utilisateur AS id_utilisateur,
        u.nom || ' ' || u.prenom AS nom_expediteur,
        u.nom || ' ' || u.prenom AS nom_destinataire,
        u.matricule,
        m.id_expediteur,
        m.id_destinataire,
        'prive' AS type
    FROM message m
    JOIN utilisateur u
        ON u.id_utilisateur = m.id_expediteur OR u.id_utilisateur = m.id_destinataire
    WHERE m.id_groupe_discussion IS NULL;

CREATE OR REPLACE VIEW v_discussions_individuelles AS
SELECT
    m.id_message,
    m.id_expediteur,
    m.id_destinataire,

    u_exp.id_utilisateur  AS id_expediteur_user,
    u_exp.nom || ' ' || u_exp.prenom AS nom_expediteur,
    u_exp.matricule AS matricule_expediteur,

    u_dest.id_utilisateur AS id_destinataire_user,
    u_dest.nom || ' ' || u_dest.prenom AS nom_destinataire,
    u_dest.matricule AS matricule_destinataire

FROM message m
JOIN utilisateur u_exp  ON u_exp.id_utilisateur = m.id_expediteur
JOIN utilisateur u_dest ON u_dest.id_utilisateur = m.id_destinataire
WHERE m.id_groupe_discussion IS NULL;



CREATE OR REPLACE VIEW v_utilisateur_message AS
SELECT 
    m.id_message,
    m.id_expediteur,

    u_exp.nom || ' ' || u_exp.prenom AS nom_expediteur,
    u_exp.matricule AS matricule_expediteur,

    m.id_destinataire,
    u_dest.nom || ' ' || u_dest.prenom AS nom_destinataire,
    u_dest.matricule AS matricule_destinataire,

    m.id_groupe_discussion,
    m.contenu,
    m.date_envoie,
    m.id_status_msg,
    pj.id_piece_joint,
    pj.chemin,
    pj.nom_original,
    m.id_espace_travail,
    m.date_modification,
    m.modifiable_jusqua
FROM message m
JOIN utilisateur u_exp ON u_exp.id_utilisateur = m.id_expediteur
LEFT JOIN utilisateur u_dest ON u_dest.id_utilisateur = m.id_destinataire
LEFT JOIN piece_joint pj ON pj.id_message = m.id_message;




CREATE or REPLACE VIEW v_piece_joint AS (
    select 
        pj.id_piece_joint, 
        pj.id_message, 
        pj.id_type_piece_joint, 
        pj.chemin, 
        pj.date_ajout, 
        tpj.type,
        pj.nom_original 
    from piece_joint pj 
    join type_piece_joint tpj on tpj.id_type_piece_joint = pj.id_type_piece_joint
);

CREATE OR REPLACE VIEW v_utilisateur_espace_travail AS (
    select 
        u.id_utilisateur, 
        u.matricule, 
        u.nom, 
        u.prenom, 
        et.id_espace_travail, 
        et.nom as espace_travail, 
        et.id_pole 
    from utilisateur_espace_travail uet 
    join utilisateur u on u.id_utilisateur
     = uet.id_utilisateur 
    join espace_travail et on et.id_espace_travail = uet.id_espace_travail
);

CREATE OR REPLACE VIEW v_pole_espace_travail AS (
    SELECT 
        et.id_espace_travail,
        et.nom,
        et.id_pole::integer, 
        et.id_admin,
        p.pole,
        et.deleted
    FROM espace_travail et
    JOIN pole p ON p.id_pole = et.id_pole
);


