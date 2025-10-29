CREATE OR REPLACE VIEW v_info_utilisateur AS (
    select 
    u.id_utilisateur, 
    u.nom, 
    u.prenom, 
    u.matricule, 
    u.mdp, 
    u.id_role, 
    r.role 
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
        m.id_expediteur,
        m.id_destinataire,
        'prive' AS type
    FROM message m
    JOIN utilisateur u
        ON u.id_utilisateur = m.id_expediteur OR u.id_utilisateur = m.id_destinataire
    WHERE m.id_groupe_discussion IS NULL;

CREATE VIEW v_discussions_individuelles AS
    SELECT DISTINCT
        e.id_utilisateur AS id_expediteur,
        e.nom || ' ' || e.prenom AS nom_expediteur,
        d.id_utilisateur AS id_destinataire,
        d.nom || ' ' || d.prenom AS nom_destinataire,
        m.contenu,

        'prive' AS type
    FROM message m
    JOIN utilisateur e ON e.id_utilisateur = m.id_expediteur
    JOIN utilisateur d ON d.id_utilisateur = m.id_destinataire
    WHERE m.id_groupe_discussion IS NULL;


CREATE or REPLACE VIEW v_utilisateur_message AS (
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
        pj.chemin,
        pj.nom_original
    from message m 
    join utilisateur u on u.id_utilisateur = m.id_expediteur 
    left join piece_joint pj on pj.id_message = m.id_message
    order by id_message desc
);

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
    join utilisateur u on u.id_utilisateur = uet.id_utilisateur 
    join espace_travail et on et.id_espace_travail = uet.id_espace_travail
);

