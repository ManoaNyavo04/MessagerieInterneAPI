﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessagerieInterneAPI
{
    [Table("v_utilisateur_espace_travail")]
    public class UtilisateurEspaceTravailView
    {
        [Key]
        [Column("id_utilisateur")]
        public int IdUtilisateur { get; set; }
        [Column("matricule")]
        public string Matricule { get; set; }
        [Column("nom")]
        public string Nom { get; set; }
        [Column("prenom")]
        public string Prenom { get; set; }
        [Column("id_espace_travail")]
        public int IdEspaceTravail { get; set; }
        [Column("espace_travail")]
        public string EspaceTravail { get; set; }
        [Column("id_pole")]
        public int IdPole { get; set; }

        public UtilisateurEspaceTravailView() { }
        public UtilisateurEspaceTravailView(int idutilisateur, String matricule, String nom, String prenom, int idespace, String espace, int idpole)
        {
            IdUtilisateur = idutilisateur;
            Matricule = matricule;
            Nom = nom;
            Prenom = prenom;
            IdEspaceTravail = idespace;
            EspaceTravail = espace;
            IdPole = idpole;
        }
        
    }
}