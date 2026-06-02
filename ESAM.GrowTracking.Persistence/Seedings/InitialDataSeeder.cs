using ESAM.GrowTracking.Domain.Entities;
using ESAM.GrowTracking.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace ESAM.GrowTracking.Persistence.Seedings
{
    internal static class InitialDataSeeder
    {
        private static readonly DateTime SeedDate = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        internal static void Seed(this ModelBuilder modelBuilder)
        {
            SeedModulesAndPermissions(modelBuilder);
            SeedWorkProfiles(modelBuilder);
            SeedRoles(modelBuilder);
            SeedBusinessUnitsAndCampuses(modelBuilder);
            SeedPeopleAndUsers(modelBuilder);
            SeedUserAssignments(modelBuilder);
        }

        private static void SeedModulesAndPermissions(ModelBuilder modelBuilder)
        {
            var module1 = new Module(1, "Académico");
            modelBuilder.Entity<Module>().HasData(module1);
            var permission1 = new Permission(1, 1, "Agregar Proyectos");
            var permission2 = new Permission(2, 1, "Agregar Calificación");
            var permission3 = new Permission(3, 1, "Ver Calificaciones");
            modelBuilder.Entity<Permission>().HasData(permission1, permission2, permission3);
        }

        private static void SeedWorkProfiles(ModelBuilder modelBuilder)
        {
            var workProfile1 = new WorkProfile(1, "Gestor", WorkProfileType.WithRoles);
            var workProfile2 = new WorkProfile(2, "Docente", WorkProfileType.OnlyWorkProfile);
            var workProfile3 = new WorkProfile(3, "Estudiante", WorkProfileType.OnlyWorkProfile);
            modelBuilder.Entity<WorkProfile>().HasData(workProfile1, workProfile2, workProfile3);
            var workProfilePermission1 = new WorkProfilePermission(2, 1, false, 1, SeedDate);
            var workProfilePermission2 = new WorkProfilePermission(2, 2, true, 1, SeedDate);
            var workProfilePermission3 = new WorkProfilePermission(2, 3, true, 1, SeedDate);
            var workProfilePermission4 = new WorkProfilePermission(3, 1, false, 1, SeedDate);
            var workProfilePermission5 = new WorkProfilePermission(3, 2, false, 1, SeedDate);
            var workProfilePermission6 = new WorkProfilePermission(3, 3, true, 1, SeedDate);
            modelBuilder.Entity<WorkProfilePermission>().HasData(workProfilePermission1, workProfilePermission2, workProfilePermission3, workProfilePermission4,
                workProfilePermission5, workProfilePermission6);
        }

        private static void SeedRoles(ModelBuilder modelBuilder)
        {
            var role1 = new Role(1, "Coordinador de T. I.", 1, createdAt: SeedDate);
            modelBuilder.Entity<Role>().HasData(role1);
            var rolePermission1 = new RolePermission(1, 1, true, 1, SeedDate);
            var rolePermission2 = new RolePermission(1, 2, true, 1, SeedDate);
            var rolePermission3 = new RolePermission(1, 3, true, 1, SeedDate);
            modelBuilder.Entity<RolePermission>().HasData(rolePermission1, rolePermission2, rolePermission3);
        }

        private static void SeedBusinessUnitsAndCampuses(ModelBuilder modelBuilder)
        {
            var businessUnit1 = new BusinessUnit(1, "ESAM", "ESAM", "https://esam.edu.bo/", new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc), 1, SeedDate);
            modelBuilder.Entity<BusinessUnit>().HasData(businessUnit1);
            var campus1 = new Campus(1, 1, "ESAM Sucre 2", "https://esam.edu.bo/Sucre2", new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc), 1, SeedDate);
            var campus2 = new Campus(2, 1, "ESAM Monteagudo", "https://esam.edu.bo/Monteagudo", new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc), 1, SeedDate);
            modelBuilder.Entity<Campus>().HasData(campus1, campus2);
        }

        // User 1: lflorespadilla
        // Password user1: 5681003
        // User 2: echirinina
        // Password user2: 13071262
        private static void SeedPeopleAndUsers(ModelBuilder modelBuilder)
        {
            var person1 = new Person(1, "Luis Fernando", "Flores", "Padilla", "5681003", IdentityDocumentType.IdentityCard, Gender.Man, MaritalStatus.Married, 1, SeedDate);
            var person2 = new Person(2, "Efrain", "Chiri", "Nina", "13071262", IdentityDocumentType.IdentityCard, Gender.Man, MaritalStatus.Single, 1, SeedDate);
            modelBuilder.Entity<Person>().HasData(person1, person2);
            var user1 = new User(1, "lflorespadilla", "luis.flores@esam.edu.bo", "1DAIl850O7FsKxxjnPtRuw==", "cNqBlSDNez491Q3/7bC8mmNnFisihQ28n1MlWy6fXyU=",
                "2bb48cdd-afbd-48f7-ab11-0cd74eea240e", 1, SeedDate);
            var user2 = new User(2, "echirinina", "efrain.chiri@esam.edu.bo", "pxAU4s4HEGtDsUFFA3y1vw==", "PKi+hECJUsg7aujM85GlYNGEAu2J1ZrNS6QqJ603WpU=",
                "2f01a267-92db-4703-99f5-5b995167d3bd", 1, SeedDate);
            modelBuilder.Entity<User>().HasData(user1, user2);
        }

        private static void SeedUserAssignments(ModelBuilder modelBuilder)
        {
            var userRoleCampus1 = new UserRoleCampus(1, 1, 1, 1, SeedDate);
            var userRoleCampus2 = new UserRoleCampus(1, 1, 2, 1, SeedDate);
            var userRoleCampus3 = new UserRoleCampus(2, 1, 1, 1, SeedDate);
            var userRoleCampus4 = new UserRoleCampus(2, 1, 2, 1, SeedDate);
            modelBuilder.Entity<UserRoleCampus>().HasData(userRoleCampus1, userRoleCampus2, userRoleCampus3, userRoleCampus4);
            var userWorkProfile1 = new UserWorkProfile(1, 1, 1, SeedDate);
            var userWorkProfile2 = new UserWorkProfile(1, 2, 1, SeedDate);
            var userWorkProfile3 = new UserWorkProfile(1, 3, 1, SeedDate);
            var userWorkProfile4 = new UserWorkProfile(2, 1, 1, SeedDate);
            var userWorkProfile5 = new UserWorkProfile(2, 2, 1, SeedDate);
            var userWorkProfile6 = new UserWorkProfile(2, 3, 1, SeedDate);
            modelBuilder.Entity<UserWorkProfile>().HasData(userWorkProfile1, userWorkProfile2, userWorkProfile3, userWorkProfile4, userWorkProfile5, userWorkProfile6);
        }
    }
}