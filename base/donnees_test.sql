INSERT INTO utilisateur (id_utilisateur, nom, prenom, matricule, id_role, mdp) VALUES
(0, 'Dev', 'Dev', 'Dev', 1, crypt('Dev2025', gen_salt('bf')));

INSERT INTO espace_travail (id_espace_travail, nom)
VALUES (1, 'Espace Admin');

INSERT INTO utilisateur_espace_travail (id_utilisateur, id_espace_travail)
VALUES (0, 1);

INSERT INTO utilisateur (nom, prenom, matricule, id_role, mdp) VALUES
('Admin1', 'Admin1', 'Admin1', 1, crypt('Admin12025', gen_salt('bf')));

INSERT INTO espace_travail (id_espace_travail, nom)
VALUES (1, 'Espace Admin');

INSERT INTO utilisateur_espace_travail (id_utilisateur, id_espace_travail)
VALUES (757, 1);



CREATE EXTENSION IF NOT EXISTS pgcrypto;
SET client_encoding = 'UTF8';


INSERT INTO pole (pole) VALUES
('TOPO'), ('CARTO'), ('R2I'),('PATR');

INSERT INTO role (role) VALUES
('admin'),
('utilisateur');


INSERT INTO utilisateur (nom, prenom, matricule, id_role, mdp) VALUES
('Rakoto', 'Jean', 'MAT001', 1, crypt('Jean2025', gen_salt('bf'))),
('Rasoanaivo', 'Lalao', 'MAT002', 2, crypt('Lalao2025', gen_salt('bf'))),
('Andrianina', 'Fetra', 'MAT003', 2, crypt('Fetra2025', gen_salt('bf'))),
('Randrianarisoa', 'Tojo', 'MAT004', 2, crypt('Tojo2025', gen_salt('bf'))),

('Ratsimba', 'Hery', 'MAT005', 1, crypt('Hery2025', gen_salt('bf'))),
('Ramanandraibe', 'Miora', 'MAT006', 2, crypt('Miora2025', gen_salt('bf'))),
('Raherisoa', 'Tiana', 'MAT007', 2, crypt('Tiana2025', gen_salt('bf'))),
('Ravonimanana', 'Soa', 'MAT008', 2, crypt('Soa2025', gen_salt('bf'))),
('Rakotobe', 'Njaka', 'MAT009', 1, crypt('Njaka2025', gen_salt('bf'))),
('Razanatsimba', 'Fanja', 'MAT010', 2, crypt('Fanja2025', gen_salt('bf')));

INSERT INTO statuts_message (statuts) VALUES
('envoyé'),
('reçu'),
('lu'),
('archivé'),
('supprimé');

INSERT INTO statuts_message (statuts) VALUES
('modifié');

INSERT INTO espace_travail (nom, id_pole, id_admin) VALUES
('Projet SIG', 1, 1),
('Suivi cartographique', 2, 5),
('Analyse données', 3, 9);

INSERT INTO utilisateur_espace_travail (id_utilisateur, id_espace_travail) VALUES
(2, 1),
(3, 1),
(4, 2),
(6, 2),
(7, 3),
(8, 3),
(10, 1);

INSERT INTO groupe_discussion (nom, id_espace_travail, description, date_creation, id_createur) VALUES
('Discussion SIG', 1, 'Groupe de discussion sur les donnees SIG', NOW(), 1),
('Cartographie avancee', 2, 'Echange autour des cartes vectorielles', NOW(), 5),
('R2I Strategie', 3, 'Reflexion strategique du pole R2I', NOW(), 9);


INSERT INTO utilisateur_groupe_discussion (id_groupe_discussion, id_utilisateur, est_admin) VALUES
(1, 2, FALSE),
(1, 3, FALSE),
(1, 1, TRUE),

(2, 4, FALSE),
(2, 5, TRUE),
(2, 6, FALSE),

(3, 7, FALSE),
(3, 9, TRUE),
(3, 8, FALSE);

INSERT INTO message (id_expediteur, id_destinataire, contenu, date_envoie, id_groupe_discussion, id_status_msg) VALUES
(1, 2, 'Bonjour Lalao, comment vas-tu ?', NOW(), 1, 1),
(2, 1, 'Salut Jean, je vais bien merci ! Et toi ?', NOW(), 1, 2),
(3, 4, 'Tojo, as-tu vu la dernière carte ?', NOW(), 2, 1),
(5, 6, 'Miora, peux-tu vérifier les données de ce projet ?', NOW(), NULL, 1),
(7, 8, 'Fanja, on se retrouve pour discuter du projet demain ?', NOW(), NULL, 1);

INSERT INTO message (id_expediteur, id_destinataire, contenu, date_envoie, id_groupe_discussion, id_status_msg) VALUES
(8, 7, 'okey ?', NOW(), NULL, 1);










