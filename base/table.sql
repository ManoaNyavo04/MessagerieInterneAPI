CREATE DATABASE messagerie;

\c messagerie;
SET client_encoding = 'UTF8';


CREATE TABLE pole(
    id_pole serial primary key,
    pole VARCHAR(100)
);

CREATE TABLE role (
    id_role serial primary key,
    role VARCHAR(100) -- 0: admin et 1: simple user
);

CREATE TABLE statuts_message(
    id_status_msg serial primary key,
    statuts VARCHAR(100)
);

CREATE TABLE utilisateur(
    id_utilisateur serial primary key,
    nom VARCHAR(100),
    prenom VARCHAR(100),
    matricule VARCHAR(100),                       
    id_role INT,
    foreign key (id_role) references role(id_role)
);
alter TABLE utilisateur ADD COLUMN mdp TEXT;

CREATE TABLE espace_travail(
    id_espace_travail serial primary key,
    nom VARCHAR(100),
    id_pole INT,
    id_admin INT,
    foreign key (id_pole) references pole(id_pole),
    foreign key (id_admin) references utilisateur(id_utilisateur)
);

CREATE TABLE utilisateur_espace_travail(
    id_utilisateur_espace_travail serial primary key,
    id_utilisateur INT,
    id_espace_travail INT,
    foreign key (id_espace_travail) references espace_travail(id_espace_travail),
    foreign key (id_utilisateur) references utilisateur(id_utilisateur)
);

CREATE TABLE groupe_discussion (
    id_groupe_discussion serial primary key,
    nom VARCHAR(100),
    id_espace_travail INT,
    description text,
    date_creation timestamp,
    id_createur INT,
    foreign key (id_espace_travail) references espace_travail(id_espace_travail),
    foreign key (id_createur) references utilisateur(id_utilisateur)
);

CREATE TABLE utilisateur_groupe_discussion(
    id_utilisateur_groupe_discussion serial primary key,
    id_groupe_discussion INT,
    id_utilisateur INT,
    est_admin BOOLEAN,
    foreign key (id_utilisateur) references utilisateur(id_utilisateur),
    foreign key (id_groupe_discussion) references groupe_discussion(id_groupe_discussion)
);
ALTER TABLE utilisateur_groupe_discussion ADD COLUMN statuts INT default 0; -- 0 : membre, 1 : retiré

CREATE TABLE message (
    id_message serial primary key,
    id_expediteur INT,
    id_destinataire INT,
    id_groupe_discussion INT,
    contenu text,
    date_envoie timestamp,
    id_status_msg INT,
    foreign key (id_expediteur) references utilisateur(id_utilisateur),
    foreign key (id_destinataire) references utilisateur(id_utilisateur),
    foreign key (id_groupe_discussion) references groupe_discussion(id_groupe_discussion),
    foreign key (id_status_msg) references statuts_message(id_status_msg)
);
ALTER TABLE message ADD COLUMN id_espace_travail INT;
ALTER TABLE message
ADD CONSTRAINT fk_message_espace
FOREIGN KEY (id_espace_travail)
REFERENCES espace_travail(id_espace_travail);



CREATE TABLE type_piece_joint (
    id_type_piece_joint serial primary key,
    type VARCHAR(100),
    taille INT
);

CREATE TABLE piece_joint (
    id_piece_joint serial primary key,
    id_message INT,
    id_type_piece_joint INT,
    chemin text,
    date_ajout timestamp,
    foreign key (id_message) references message(id_message),
    foreign key (id_type_piece_joint) references type_piece_joint(id_type_piece_joint)
);
ALTER TABLE piece_joint ADD COLUMN nom_original TEXT;


CREATE TABLE message_utilisateur_statut (
    id_message_utilisateur_statut serial primary key,
    id_message INT,
    id_utilisateur INT,
    id_status_msg INT,
    date_statut timestamp default NOW(),
    foreign key (id_message) references message(id_message),
    foreign key (id_utilisateur) references utilisateur(id_utilisateur),
    foreign key (id_status_msg) references statuts_message(id_status_msg)
);

