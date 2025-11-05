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


    }
}