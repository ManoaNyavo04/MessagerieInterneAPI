CREATE OR REPLACE VIEW v_utilisateur_groupe_discussion AS (
    select 
        gd.id_groupe_discussion,
        gd.nom as groupe ,
        gd.description,
        u.id_utilisateur,
        u.matricule, 
        u.nom, 
        u.prenom, 
        ugd.est_admin 
    from utilisateur_groupe_discussion ugd 
    join utilisateur u on u.id_utilisateur = ugd.id_utilisateur 
    join groupe_discussion gd on gd.id_groupe_discussion = ugd.id_groupe_discussion
    order by u.id_utilisateur 
);
