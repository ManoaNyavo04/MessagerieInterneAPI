﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MessagerieInterneAPI
{
    [Table("espace_travail")]
    public class EspaceTravailModel
    {
        [Key]
        [Column("id_espace_travail")]
        public int Id_espace_travail { get; set; }

        [Column("nom")]
        public string Nom { get; set; }
        [Column("id_pole")]
        public int? Id_pole { get; set; }
        [Column("id_admin")]
        public int? Id_admin { get; set; }

        public EspaceTravailModel() { }

        public EspaceTravailModel(int id_espace_travail, string nom_espace, int? id_pole, int? id_admin)
        {
            this.Id_espace_travail = id_espace_travail;
            this.Nom = nom_espace;
            this.Id_pole = id_pole;
            this.Id_admin = id_admin;
        }
    }
    
}