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


    }
}