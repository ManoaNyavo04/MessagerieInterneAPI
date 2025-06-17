CREATE OR REPLACE VIEW v_utilisateur_groupe_discussion AS (
    select 
        u.id_utilisateur,
        gd.id_groupe_discussion,
        u.nom, 
        u.prenom, 
        u.matricule, 
        gd.nom as groupe 
    from utilisateur_groupe_discussion ugd 
    join utilisateur u on u.id_utilisateur = ugd.id_utilisateur 
    join groupe_discussion gd on gd.id_groupe_discussion = ugd.id_groupe_discussion
);