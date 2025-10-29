CREATE OR REPLACE FUNCTION reset_tables()
RETURNS void AS $$
BEGIN
    -- Désactiver les déclencheurs définis par l'utilisateur
    ALTER TABLE pole DISABLE TRIGGER USER;
    ALTER TABLE role DISABLE TRIGGER USER;
    ALTER TABLE statuts_message DISABLE TRIGGER USER;
    ALTER TABLE utilisateur DISABLE TRIGGER USER;
    ALTER TABLE espace_travail DISABLE TRIGGER USER;
    ALTER TABLE utilisateur_espace_travail DISABLE TRIGGER USER;
    ALTER TABLE groupe_discussion DISABLE TRIGGER USER;
    ALTER TABLE utilisateur_groupe_discussion DISABLE TRIGGER USER;
    ALTER TABLE message DISABLE TRIGGER USER;
    ALTER TABLE type_piece_joint DISABLE TRIGGER USER;
    ALTER TABLE piece_joint DISABLE TRIGGER USER;
    ALTER TABLE message_utilisateur_statut DISABLE TRIGGER USER;

    -- Supprimer toutes les lignes des tables en respectant les dépendances
    DELETE FROM message_utilisateur_statut;
    DELETE FROM piece_joint;
    DELETE FROM message;
    DELETE FROM utilisateur_groupe_discussion;
    DELETE FROM groupe_discussion;
    DELETE FROM utilisateur_espace_travail;
    DELETE FROM espace_travail;
    DELETE FROM utilisateur;
    DELETE FROM statuts_message;
    DELETE FROM role;
    DELETE FROM pole;
    DELETE FROM type_piece_joint;

    -- Réinitialiser les séquences à 1
    ALTER SEQUENCE pole_id_pole_seq RESTART WITH 1;
    ALTER SEQUENCE role_id_role_seq RESTART WITH 1;
    ALTER SEQUENCE statuts_message_id_status_msg_seq RESTART WITH 1;
    ALTER SEQUENCE utilisateur_id_utilisateur_seq RESTART WITH 1;
    ALTER SEQUENCE espace_travail_id_espace_travail_seq RESTART WITH 1;
    ALTER SEQUENCE utilisateur_espace_travail_id_utilisateur_espace_travail_seq RESTART WITH 1;
    ALTER SEQUENCE groupe_discussion_id_groupe_discussion_seq RESTART WITH 1;
    ALTER SEQUENCE utilisateur_groupe_discussion_id_utilisateur_groupe_discussion_seq RESTART WITH 1;
    ALTER SEQUENCE message_id_message_seq RESTART WITH 1;
    ALTER SEQUENCE type_piece_joint_id_type_piece_joint_seq RESTART WITH 1;
    ALTER SEQUENCE piece_joint_id_piece_joint_seq RESTART WITH 1;
    ALTER SEQUENCE message_utilisateur_statut_id_message_utilisateur_statut_seq RESTART WITH 1;

    -- Réactiver les déclencheurs définis par l'utilisateur
    ALTER TABLE pole ENABLE TRIGGER USER;
    ALTER TABLE role ENABLE TRIGGER USER;
    ALTER TABLE statuts_message ENABLE TRIGGER USER;
    ALTER TABLE utilisateur ENABLE TRIGGER USER;
    ALTER TABLE espace_travail ENABLE TRIGGER USER;
    ALTER TABLE utilisateur_espace_travail ENABLE TRIGGER USER;
    ALTER TABLE groupe_discussion ENABLE TRIGGER USER;
    ALTER TABLE utilisateur_groupe_discussion ENABLE TRIGGER USER;
    ALTER TABLE message ENABLE TRIGGER USER;
    ALTER TABLE type_piece_joint ENABLE TRIGGER USER;
    ALTER TABLE piece_joint ENABLE TRIGGER USER;
    ALTER TABLE message_utilisateur_statut ENABLE TRIGGER USER;
END;
$$ LANGUAGE plpgsql;

SELECT reset_tables();

