﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OLWebApi.Domain.Entities;

namespace OLWebApi.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Comerciante> Comerciante { get; set; }
        public DbSet<Establecimiento> Establecimiento { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Establecimiento>()
                .HasOne(e => e.Comerciante)
                .WithMany(c => c.Establecimientos)
                .HasForeignKey(e => e.IdComerciante);
        }

    }
}

