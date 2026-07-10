using ESAM.GrowTracking.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ESAM.GrowTracking.Persistence.Configurations.Entities
{
    internal sealed class UserSessionRoleCampusSelectedConfiguration : IEntityTypeConfiguration<UserSessionRoleCampusSelected>
    {
        public void Configure(EntityTypeBuilder<UserSessionRoleCampusSelected> builder)
        {
            builder.ToTable("UserSessionRoleCampusesSelected");
            builder.HasKey(usrcs => usrcs.Id);
            builder.Property(usrcs => usrcs.Id).IsRequired().ValueGeneratedOnAdd();
            builder.Property(usrcs => usrcs.UserSessionWorkProfileSelectedId).IsRequired();
            builder.Property(usrcs => usrcs.UserId).IsRequired();
            builder.Property(usrcs => usrcs.RoleId).IsRequired();
            builder.Property(usrcs => usrcs.CampusId).IsRequired();
            builder.Property(usrcs => usrcs.IsActive).IsRequired();
            builder.Property(usrcs => usrcs.CreatedAt).IsRequired().HasDefaultValueSql("GETUTCDATE()");
            builder.HasIndex(usrcs => new { usrcs.UserSessionWorkProfileSelectedId, usrcs.IsActive });
            builder.HasOne(usrcs => usrcs.UserSessionWorkProfileSelected).WithMany(uswps => uswps.UserSessionRoleCampusesSelected)
                .HasForeignKey(usrcs => usrcs.UserSessionWorkProfileSelectedId).IsRequired().OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(usrcs => usrcs.UserRoleCampus).WithMany(urc => urc.UserSessionRoleCampusesSelected)
                .HasForeignKey(usrcs => new { usrcs.UserId, usrcs.RoleId, usrcs.CampusId }).IsRequired().OnDelete(DeleteBehavior.Restrict);
        }
    }

    //internal sealed class UserSessionRoleCampusSelectedConfiguration : IEntityTypeConfiguration<UserSessionRoleCampusSelected>
    //{
    //    public void Configure(EntityTypeBuilder<UserSessionRoleCampusSelected> builder)
    //    {
    //        builder.ToTable("UserSessionRoleCampusesSelected");
    //        builder.HasKey(usrcs => usrcs.UserSessionId);
    //        builder.Property(usrcs => usrcs.UserSessionId).IsRequired().ValueGeneratedNever();
    //        builder.Property(usrcs => usrcs.UserId).IsRequired();
    //        builder.Property(usrcs => usrcs.RoleId).IsRequired();
    //        builder.Property(usrcs => usrcs.CampusId).IsRequired();
    //        builder.HasOne(usrcs => usrcs.UserSessionWorkProfileSelected).WithOne(uswps => uswps.UserSessionRoleCampusSelected)
    //            .HasForeignKey<UserSessionRoleCampusSelected>(usrcs => usrcs.UserSessionId).IsRequired().OnDelete(DeleteBehavior.Restrict);
    //        builder.HasOne(usrcs => usrcs.UserRoleCampus).WithMany(urc => urc.UserSessionRoleCampusesSelected)
    //            .HasForeignKey(usrcs => new { usrcs.UserId, usrcs.RoleId, usrcs.CampusId }).IsRequired().OnDelete(DeleteBehavior.Restrict);
    //    }
    //}
}