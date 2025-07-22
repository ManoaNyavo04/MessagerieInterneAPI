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
        'groupe' AS type 
    from utilisateur_groupe_discussion ugd 
    join utilisateur u on u.id_utilisateur = ugd.id_utilisateur 
    join groupe_discussion gd on gd.id_groupe_discussion = ugd.id_groupe_discussion
    order by u.id_utilisateur 
);

CREATE VIEW v_discussions_individuelles AS
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
