using LoopLearn.Entities.Enums;
using LoopLearn.Entities.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LoopLearn.DataAccess.Data
{
    public static class SeedingService
    {
        /// <summary>
        /// Seeds initial data into the database if not already present.
        /// This includes roles, super admin user, categories, and tags.
        /// </summary>
        public static async Task SeedDatabaseAsync(this ApplicationDbContext context, ILogger logger, RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            try
            {
                // Apply any pending migrations
                await context.Database.MigrateAsync();

                // Seed roles if they don't exist
                if (!await roleManager.Roles.AnyAsync())
                {
                    logger.LogInformation("Seeding roles...");
                    await SeedRolesAsync(roleManager, logger);
                }

                // Seed super admin if no users exist
                if (!await userManager.Users.AnyAsync())
                {
                    logger.LogInformation("Seeding super admin user...");
                    await SeedSuperAdminAsync(userManager, logger);
                }

                // Seed categories if none exist
                if (!await context.Categories.AnyAsync())
                {
                    logger.LogInformation("Seeding categories...");
                    await SeedCategoriesAsync(context, logger);
                }

                // Seed tags if none exist
                if (!await context.Tags.AnyAsync())
                {
                    logger.LogInformation("Seeding tags...");
                    await SeedTagsAsync(context, logger);
                }

                logger.LogInformation("Database seeding completed successfully.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database.");
                throw;
            }
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
            var roleNames = new[] 
            { 
                "SuperAdmin", 
                "Admin", 
                "Instructor", 
                "Student" 
            };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole { Name = roleName });
                    if (result.Succeeded)
                    {
                        logger.LogInformation($"Successfully created role: {roleName}");
                    }
                    else
                    {
                        logger.LogError($"Failed to create role {roleName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }

            logger.LogInformation($"Successfully seeded {roleNames.Length} roles.");
        }

        private static async Task SeedSuperAdminAsync(UserManager<ApplicationUser> userManager, ILogger logger)
        {
            // Create super admin user
            var superAdminUser = new ApplicationUser
            {
                UserName = "superadmin",
                FirstName = "Super",
                LastName = "Admin",
                Email = "superadmin@looplearn.com",
                PhoneNumber = "01091602597",
                PhoneNumberConfirmed = true,
                BirthDate = new DateTime(2003, 04, 22),
                Gender = Gender.Male,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = null
            };

            const string superAdminPassword = "Asdiop00@";

            var result = await userManager.CreateAsync(superAdminUser, superAdminPassword);

            if (result.Succeeded)
            {
                // Assign SuperAdmin role
                var roleResult = await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");

                if (roleResult.Succeeded)
                {
                    logger.LogInformation($"Successfully created super admin user with username: {superAdminUser.UserName}");
                }
                else
                {
                    logger.LogError($"Failed to assign SuperAdmin role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                logger.LogError($"Failed to create super admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }

        private static async Task SeedCategoriesAsync(ApplicationDbContext context, ILogger logger)
        {
            var categories = new[]
            {
                new Category { Name = "Programming", Description = "Learn programming languages and software development" },
                new Category { Name = "Web Development", Description = "Master web development with HTML, CSS, and JavaScript" },
                new Category { Name = "Mobile Development", Description = "Develop mobile applications for iOS and Android" },
                new Category { Name = "Data Science", Description = "Explore data science, machine learning, and AI" },
                new Category { Name = "Design", Description = "Learn UI/UX design and graphic design" },
                new Category { Name = "Business", Description = "Develop business and entrepreneurship skills" },
                new Category { Name = "Marketing", Description = "Digital marketing and social media marketing" },
                new Category { Name = "Finance", Description = "Personal finance and investment strategies" },
                new Category { Name = "Cybersecurity", Description = "Learn cybersecurity and ethical hacking" },
                new Category { Name = "Cloud Computing", Description = "Master cloud platforms and infrastructure" },
                new Category { Name = "DevOps", Description = "Learn DevOps practices and tools" },
                new Category { Name = "Database Management", Description = "SQL, NoSQL, and database design" },
                new Category { Name = "Artificial Intelligence", Description = "Deep learning, neural networks, and AI" },
                new Category { Name = "Game Development", Description = "Create games with Unity and Unreal Engine" },
                new Category { Name = "Languages", Description = "Learn new languages and improve communication" },
                new Category { Name = "Personal Development", Description = "Self-improvement and productivity skills" },
                new Category { Name = "Photography", Description = "Photography techniques and editing" },
                new Category { Name = "Music Production", Description = "Music creation and audio production" },
                new Category { Name = "Video Editing", Description = "Video production and editing skills" },
                new Category { Name = "IT & Software", Description = "IT infrastructure and software tools" }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
            logger.LogInformation($"Successfully seeded {categories.Length} categories.");
        }

        private static async Task SeedTagsAsync(ApplicationDbContext context, ILogger logger)
        {
            var tags = new[]
            {
                new Tag { Name = "C#" }, new Tag { Name = "Python" }, new Tag { Name = "JavaScript" },
                new Tag { Name = "Java" }, new Tag { Name = "C++" }, new Tag { Name = "React" },
                new Tag { Name = "Angular" }, new Tag { Name = "Vue.js" }, new Tag { Name = "Node.js" },
                new Tag { Name = "TypeScript" }, new Tag { Name = ".NET" }, new Tag { Name = ".NET Core" },
                new Tag { Name = "ASP.NET" }, new Tag { Name = "SQL" }, new Tag { Name = "MongoDB" },
                new Tag { Name = "Firebase" }, new Tag { Name = "REST API" }, new Tag { Name = "GraphQL" },
                new Tag { Name = "Microservices" }, new Tag { Name = "Docker" }, new Tag { Name = "Kubernetes" },
                new Tag { Name = "AWS" }, new Tag { Name = "Azure" }, new Tag { Name = "Google Cloud" },
                new Tag { Name = "Machine Learning" }, new Tag { Name = "Deep Learning" }, new Tag { Name = "TensorFlow" },
                new Tag { Name = "PyTorch" }, new Tag { Name = "Data Analysis" }, new Tag { Name = "Pandas" },
                new Tag { Name = "Numpy" }, new Tag { Name = "Matplotlib" }, new Tag { Name = "Excel" },
                new Tag { Name = "Power BI" }, new Tag { Name = "Git" }, new Tag { Name = "GitHub" },
                new Tag { Name = "CI/CD" }, new Tag { Name = "Linux" }, new Tag { Name = "Windows" },
                new Tag { Name = "macOS" }, new Tag { Name = "Agile" }, new Tag { Name = "Scrum" },
                new Tag { Name = "Figma" }, new Tag { Name = "Photoshop" }, new Tag { Name = "Adobe XD" },
                new Tag { Name = "UI Design" }, new Tag { Name = "UX Design" }, new Tag { Name = "Wireframing" },
                new Tag { Name = "Prototyping" }, new Tag { Name = "Responsive Design" }, new Tag { Name = "Mobile App" },
                new Tag { Name = "iOS" }, new Tag { Name = "Android" }, new Tag { Name = "Flutter" },
                new Tag { Name = "React Native" }, new Tag { Name = "Swift" }, new Tag { Name = "Kotlin" },
                new Tag { Name = "Unity" }, new Tag { Name = "Unreal Engine" }, new Tag { Name = "Game Design" },
                new Tag { Name = "SEO" }, new Tag { Name = "SEM" }, new Tag { Name = "Social Media" },
                new Tag { Name = "Content Marketing" }, new Tag { Name = "Email Marketing" }, new Tag { Name = "Affiliate Marketing" },
                new Tag { Name = "Growth Hacking" }, new Tag { Name = "Analytics" }, new Tag { Name = "Investment" },
                new Tag { Name = "Stock Market" }, new Tag { Name = "Cryptocurrency" }, new Tag { Name = "Personal Finance" },
                new Tag { Name = "Accounting" }, new Tag { Name = "Cybersecurity" }, new Tag { Name = "Ethical Hacking" },
                new Tag { Name = "Network Security" }, new Tag { Name = "Penetration Testing" }, new Tag { Name = "OWASP" },
                new Tag { Name = "Time Management" }, new Tag { Name = "Leadership" }, new Tag { Name = "Communication" },
                new Tag { Name = "Productivity" }, new Tag { Name = "Motivation" }, new Tag { Name = "English" },
                new Tag { Name = "Spanish" }, new Tag { Name = "French" }, new Tag { Name = "German" },
                new Tag { Name = "Chinese" }, new Tag { Name = "Japanese" }, new Tag { Name = "Arabic" },
                new Tag { Name = "Beginner" }, new Tag { Name = "Intermediate" }, new Tag { Name = "Advanced" },
                new Tag { Name = "Expert" }, new Tag { Name = "Certification" }, new Tag { Name = "Project-Based" },
                new Tag { Name = "Hands-On" }, new Tag { Name = "Interactive" }, new Tag { Name = "Live Training" },
                new Tag { Name = "Self-Paced" }
            };

            await context.Tags.AddRangeAsync(tags);
            await context.SaveChangesAsync();
            logger.LogInformation($"Successfully seeded {tags.Length} tags.");
        }
    }
}