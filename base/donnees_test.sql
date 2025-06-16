CREATE EXTENSION IF NOT EXISTS pgcrypto;

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


