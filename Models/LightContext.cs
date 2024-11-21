using Microsoft.EntityFrameworkCore;

public class LightContext(DbContextOptions<LightContext> options) : DbContext(options)
{
    public required DbSet<Light> Lights { get; set; }
}
