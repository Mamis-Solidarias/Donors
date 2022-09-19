using MamisSolidarias.Infrastructure.Donors.Models;
using Microsoft.EntityFrameworkCore;

namespace MamisSolidarias.Infrastructure.Donors;

public class DonorsDbContext: DbContext
{
    public DbSet<Donor> Donors { get; set; }

    public DonorsDbContext(DbContextOptions<DonorsDbContext> options) : base(options)
    {
    }
    
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        new DonorCondfigurator().Configure(modelBuilder.Entity<Donor>());
    }
    
}