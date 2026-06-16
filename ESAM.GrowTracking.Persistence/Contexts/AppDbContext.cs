using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Persistence.Seedings;
using Microsoft.EntityFrameworkCore;

namespace ESAM.GrowTracking.Persistence.Contexts
{
    public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<BlacklistedAccessTokenSession> BlacklistedAccessTokensSession => Set<BlacklistedAccessTokenSession>();

        public DbSet<BlacklistedAccessTokenTemporary> BlacklistedAccessTokensTemporary => Set<BlacklistedAccessTokenTemporary>();

        public DbSet<BlacklistedRefreshToken> BlacklistedRefreshTokens => Set<BlacklistedRefreshToken>();

        public DbSet<BusinessUnit> BusinessUnits => Set<BusinessUnit>();

        public DbSet<Campus> Campuses => Set<Campus>();

        public DbSet<Module> Modules => Set<Module>();

        public DbSet<Permission> Permissions => Set<Permission>();

        public DbSet<Person> People => Set<Person>();

        public DbSet<User> Users => Set<User>();

        public DbSet<UserPhoto> UserPhotos => Set<UserPhoto>();

        public DbSet<UserDevice> UserDevices => Set<UserDevice>();

        public DbSet<UserSession> UserSessions => Set<UserSession>();

        public DbSet<UserSessionRefreshToken> UserSessionRefreshTokens => Set<UserSessionRefreshToken>();

        public DbSet<Role> Roles => Set<Role>();

        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

        public DbSet<WorkProfile> WorkProfiles => Set<WorkProfile>();

        public DbSet<WorkProfilePermission> WorkProfilePermissions => Set<WorkProfilePermission>();

        public DbSet<UserRoleCampus> UserRoleCampuses => Set<UserRoleCampus>();

        public DbSet<UserWorkProfile> UserWorkProfiles => Set<UserWorkProfile>();

        public DbSet<UserSessionWorkProfileSelected> UserSessionWorkProfilesSelected => Set<UserSessionWorkProfileSelected>();

        public DbSet<UserSessionRoleCampusSelected> UserSessionRoleCampusesSelected => Set<UserSessionRoleCampusSelected>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
            modelBuilder.Seed();
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<string>().HaveMaxLength(256);
            configurationBuilder.Properties<Enum>().HaveConversion<byte>();
        }
    }
}