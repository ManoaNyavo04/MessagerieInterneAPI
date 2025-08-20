CREATE EXTENSION IF NOT EXISTS pgcrypto;


-- INSÉRER LES PÔLES
INSERT INTO pole (pole) VALUES 
('carto'), ('topo'), ('patrimoine'), ('dev'), ('admin');

-- INSÉRER LES RÔLES
INSERT INTO role (role) VALUES 
('admin'), ('utilisateur');

-- INSÉRER LES STATUTS DE MESSAGE
INSERT INTO statuts_message (statuts) VALUES 
('envoyé'), ('reçu'), ('lu');

-- INSÉRER DES UTILISATEURS AVEC MDP CRYPTÉ
INSERT INTO utilisateur (nom, prenom, matricule, id_role, mdp) VALUES
('Rakoto', 'Jean', 'MAT001', 1, crypt('Jean2025', gen_salt('bf'))),
('Rasoa', 'Marie', 'MAT002', 2, crypt('Marie2025', gen_salt('bf'))),
('Randria', 'Paul', 'MAT003', 2, crypt('Paul2025', gen_salt('bf'))),
('Rajo', 'Nina', 'MAT004', 2, crypt('Nina2025', gen_salt('bf'))),
('Andry', 'Lucas', 'MAT005', 1, crypt('Lucas2025', gen_salt('bf')));

-- CRÉER 2 ESPACES DE TRAVAIL
INSERT INTO espace_travail (nom, id_pole, id_admin) VALUES
('Espace Carto', 1, 1),
('Espace Dev', 4, 5);

-- LIAISON UTILISATEURS - ESPACES DE TRAVAIL
INSERT INTO utilisateur_espace_travail (id_utilisateur, id_espace_travail) VALUES
(1, 1), (2, 1), (3, 1),
(4, 2), (5, 2);

-- CRÉER GROUPES DE DISCUSSION
INSERT INTO groupe_discussion (nom, id_espace_travail, description, date_creation, id_createur) VALUES
('Projet Carto A', 1, 'Discussion sur projet A', NOW(), 1),
('Dev Backend', 2, 'Discussion backend', NOW(), 5);

-- AJOUTER MEMBRES AUX GROUPES
INSERT INTO utilisateur_groupe_discussion (id_groupe_discussion, id_utilisateur, est_admin) VALUES
(1, 1, true), (1, 2, false), (1, 3, false),
(2, 4, false), (2, 5, true);

-- INSÉRER MESSAGES PRIVÉS (id_destinataire défini, id_groupe_discussion NULL)
INSERT INTO message (id_expediteur, id_destinataire, id_groupe_discussion, contenu, date_envoie, id_status_msg) VALUES
(1, 2, NULL, 'Salut Marie, comment ça va ?', NOW(), 1),
(2, 1, NULL, 'Salut Jean, ça va bien merci !', NOW(), 2),
(3, 1, NULL, 'Bonjour chef, j’ai terminé la tâche.', NOW(), 1);

-- INSÉRER MESSAGES DE GROUPE (id_groupe_discussion défini, id_destinataire NULL)
INSERT INTO message (id_expediteur, id_destinataire, id_groupe_discussion, contenu, date_envoie, id_status_msg) VALUES
(1, NULL, 1, 'Bienvenue dans le groupe Projet Carto A !', NOW(), 1),
(2, NULL, 1, 'Merci, ravie de rejoindre l’équipe.', NOW(), 1),
(5, NULL, 2, 'Mise à jour du backend déployée.', NOW(), 2),
(4, NULL, 2, 'Parfait, je vais tester.', NOW(), 1);

-- INSÉRER TYPES DE PIÈCES JOINTES
INSERT INTO type_piece_joint (type, taille) VALUES 
('pdf', 2048), ('image', 1024), ('docx', 3072);

-- INSÉRER PIÈCES JOINTES
INSERT INTO piece_joint (id_message, id_type_piece_joint, chemin, date_ajout) VALUES
(4, 1, '/files/projetA_intro.pdf', NOW()),
(7, 2, '/images/screenshot.png', NOW());
