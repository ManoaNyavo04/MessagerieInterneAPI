CREATE OR REPLACE FUNCTION reset_messagerie(current_user_id integer)
RETURNS void AS $$
BEGIN
    -- Désactiver les triggers user sur toutes les tables importantes
    ALTER TABLE message_utilisateur_statut DISABLE TRIGGER USER;
    ALTER TABLE piece_joint DISABLE TRIGGER USER;
    ALTER TABLE message DISABLE TRIGGER USER;
    ALTER TABLE utilisateur_groupe_discussion DISABLE TRIGGER USER;
    ALTER TABLE groupe_discussion DISABLE TRIGGER USER;
    ALTER TABLE utilisateur_espace_travail DISABLE TRIGGER USER;
    ALTER TABLE espace_travail DISABLE TRIGGER USER;
    ALTER TABLE pole DISABLE TRIGGER USER;
    ALTER TABLE utilisateur DISABLE TRIGGER USER;

    ----------------------------------------------------------------
    --           SUPPRESSION DES DONNÉES (ordre logique)           --
    ----------------------------------------------------------------

    DELETE FROM message_utilisateur_statut;
    DELETE FROM piece_joint;
    DELETE FROM message;
    DELETE FROM utilisateur_groupe_discussion;
    DELETE FROM groupe_discussion;
    DELETE FROM utilisateur_espace_travail;
    DELETE FROM espace_travail;
    DELETE FROM pole;

    -- Supprimer TOUS les utilisateurs SAUF celui qui exécute
    DELETE FROM utilisateur WHERE id_utilisateur <> current_user_id;

    ----------------------------------------------------------------
    --        RÉINITIALISATION DES SÉQUENCES                      --
    ----------------------------------------------------------------
    ALTER SEQUENCE message_utilisateur_statut_id_message_utilisateur_statut_seq RESTART WITH 1;
    ALTER SEQUENCE piece_joint_id_piece_joint_seq RESTART WITH 1;
    ALTER SEQUENCE message_id_message_seq RESTART WITH 1;
    ALTER SEQUENCE utilisateur_groupe_discussion_id_utilisateur_groupe_discuss_seq RESTART WITH 1;
    ALTER SEQUENCE groupe_discussion_id_groupe_discussion_seq RESTART WITH 1;
    ALTER SEQUENCE utilisateur_espace_travail_id_utilisateur_espace_travail_seq RESTART WITH 1;
    ALTER SEQUENCE espace_travail_id_espace_travail_seq RESTART WITH 1;
    ALTER SEQUENCE pole_id_pole_seq RESTART WITH 1;
    -- NE PAS réinitialiser utilisateur_id_utilisateur_seq, puisque tu as un user = 0
    -- ALTER SEQUENCE utilisateur_id_utilisateur_seq RESTART WITH 1;

    ----------------------------------------------------------------
    --           RÉACTIVER LES TRIGGERS                           --
    ----------------------------------------------------------------
    ALTER TABLE message_utilisateur_statut ENABLE TRIGGER USER;
    ALTER TABLE piece_joint ENABLE TRIGGER USER;
    ALTER TABLE message ENABLE TRIGGER USER;
    ALTER TABLE utilisateur_groupe_discussion ENABLE TRIGGER USER;
    ALTER TABLE groupe_discussion ENABLE TRIGGER USER;
    ALTER TABLE utilisateur_espace_travail ENABLE TRIGGER USER;
    ALTER TABLE espace_travail ENABLE TRIGGER USER;
    ALTER TABLE pole ENABLE TRIGGER USER;
    ALTER TABLE utilisateur ENABLE TRIGGER USER;

END;
$$ LANGUAGE plpgsql;



SELECT reset_messagerie();
