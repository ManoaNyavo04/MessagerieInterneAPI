﻿using MessagerieInterneAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace MessagerieInterneAPI
{
    public class EspaceTravailService
    {
        private readonly AppDbContext _context;

        public EspaceTravailService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<UtilisateurEspaceTravailView>> GetEspacesByUtilisateurIdAsync(int idUtilisateur)
        {
            return await _context.UtilisateurEspaceTravailView
                .Where(u => u.IdUtilisateur == idUtilisateur)
                .ToListAsync();
        }

        public async Task<List<UtilisateurEspaceTravailView>> GetMembreEspacesTravail(int idEspaceTravail)
        {
            return await _context.UtilisateurEspaceTravailView
                .Where(u => u.IdEspaceTravail == idEspaceTravail)
                .ToListAsync();
        }

        public async Task<EspaceTravailModel?> GetEspacesTravailIdAsync(int idEspace)
        {
            return await _context.EspaceTravail
                .Where(u => u.Id_espace_travail == idEspace)
                .FirstOrDefaultAsync();
        }

        public async Task<UtilisateurEspaceTravailModel?> AffecterUtilisateurVersEspaceTravail(UtilisateurEspaceTravailModel model)
        {
            bool exists = await _context.UtilisateurEspaceTravail
                    .AnyAsync(u =>
                        u.Id_utilisateur == model.Id_utilisateur &&
                        u.Id_espace_travail == model.Id_espace_travail
                    );

            if (exists)
            {
                return null;
            }
            _context.UtilisateurEspaceTravail.Add(model);
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<List<EspaceTravailModel>> GetAllEspaceTravail()
        {
            return await _context.EspaceTravail.ToListAsync();
        }

        public async Task<EspaceTravailModel> CreerEspaceTravail(EspaceTravailDTO espaceTravail, int idAdmin)
        {
            var espace = new EspaceTravailModel
            {
                Nom = espaceTravail.Nom,
                Id_pole = espaceTravail.Id_pole.HasValue ? espaceTravail.Id_pole : null,
                Id_admin = idAdmin
            };
            _context.EspaceTravail.Add(espace);
            await _context.SaveChangesAsync();
            return espace;
        }

        public async Task<EspaceTravailModel> CreerEspaceTravailAsync(EspaceTravailDTO dto, int idAdmin)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Création de l’espace
                var espace = new EspaceTravailModel
                {
                    Nom = dto.Nom,
                    Id_pole = dto.Id_pole,
                    Id_admin = idAdmin
                };

                _context.EspaceTravail.Add(espace);
                await _context.SaveChangesAsync();

                // Ajouter l’admin comme membre
                var membres = new List<UtilisateurEspaceTravailModel>
                {
                    new UtilisateurEspaceTravailModel
                    {
                        Id_utilisateur = idAdmin,
                        Id_espace_travail = espace.Id_espace_travail
                    }
                };

                // Ajouter les autres membres (éviter doublons)
                if (dto.Utilisateurs != null && dto.Utilisateurs.Any())
                {
                    membres.AddRange(dto.Utilisateurs
                        .Distinct()
                        .Where(u => u != idAdmin)
                        .Select(u => new UtilisateurEspaceTravailModel
                        {
                            Id_utilisateur = u,
                            Id_espace_travail = espace.Id_espace_travail
                        }));
                }

                _context.UtilisateurEspaceTravail.AddRange(membres);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return espace;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task AjouterNouveauxMembres(int idEspace, List<int> nouveauxMembres)
        {
            if (nouveauxMembres == null || !nouveauxMembres.Any())
                throw new ArgumentException("Aucun membre à ajouter.");

            // 1️⃣ Récupérer les membres déjà existants
            var membresExistants = await _context.UtilisateurEspaceTravail
                .Where(uet => uet.Id_espace_travail == idEspace)
                .Select(uet => uet.Id_utilisateur)
                .ToListAsync();

            // 2️⃣ Filtrer les nouveaux membres pour éviter doublons
            var membresAInserer = nouveauxMembres
                .Distinct()
                .Except(membresExistants)
                .ToList();

            if (!membresAInserer.Any()) return;

            // 3️⃣ Ajouter uniquement les nouveaux
            var ajout = membresAInserer.Select(id => new UtilisateurEspaceTravailModel
            {
                Id_utilisateur = id,
                Id_espace_travail = idEspace
            });

            _context.UtilisateurEspaceTravail.AddRange(ajout);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PoleEspaceTravailView>> GetAllPoleEspaceTravail()
        {
            return await _context.PoleEspaceTravailView.ToListAsync();
        }





    }
}