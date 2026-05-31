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

				// seed test users if no users exist (other than super admin)
				await SeedTestUsersAsync(userManager, logger);

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

				// seed courses, enrollments, feedbacks, and quizzes only if there are no courses
				await SeedCoursesAsync(context, userManager, logger);
				await SeedEnrollmentsAndProgressAsync(context, userManager, logger);
				await SeedFeedbacksAsync(context, userManager, logger);
				await SeedQuizzesAsync(context, logger);

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

		// ─────────────────────────────────────────────────────────────
		// Test Users — Instructors & Students
		// ─────────────────────────────────────────────────────────────
		private static async Task SeedTestUsersAsync(UserManager<ApplicationUser> userManager, ILogger logger)
		{
			var testUsers = new[]
			{
		new
		{
			User = new ApplicationUser
			{
				UserName = "instructor1",
				FirstName = "Ahmed",
				LastName = "Hassan",
				Email = "instructor1@looplearn.com",
				EmailConfirmed = true,
				PhoneNumberConfirmed = true,
				Bio = "Senior .NET Developer with 8 years of experience.",
				BirthDate = new DateTime(1990, 5, 15),
				Gender = Gender.Male,
				CreatedAt = DateTime.UtcNow
			},
			Password = "Instructor1@",
			Role = "Instructor"
		},
		new
		{
			User = new ApplicationUser
			{
				UserName = "instructor2",
				FirstName = "Sara",
				LastName = "Mohamed",
				Email = "instructor2@looplearn.com",
				EmailConfirmed = true,
				PhoneNumberConfirmed = true,
				Bio = "Full Stack Developer specializing in React and Node.js.",
				BirthDate = new DateTime(1993, 8, 22),
				Gender = Gender.Female,
				CreatedAt = DateTime.UtcNow
			},
			Password = "Instructor2@",
			Role = "Instructor"
		},
		new
		{
			User = new ApplicationUser
			{
				UserName = "student1",
				FirstName = "Omar",
				LastName = "Ali",
				Email = "student1@looplearn.com",
				EmailConfirmed = true,
				PhoneNumberConfirmed = true,
				BirthDate = new DateTime(2000, 3, 10),
				Gender = Gender.Male,
				CreatedAt = DateTime.UtcNow
			},
			Password = "Student1@",
			Role = "Student"
		},
		new
		{
			User = new ApplicationUser
			{
				UserName = "student2",
				FirstName = "Nour",
				LastName = "Ibrahim",
				Email = "student2@looplearn.com",
				EmailConfirmed = true,
				PhoneNumberConfirmed = true,
				BirthDate = new DateTime(2001, 11, 5),
				Gender = Gender.Female,
				CreatedAt = DateTime.UtcNow
			},
			Password = "Student2@",
			Role = "Student"
		},
		new
		{
			User = new ApplicationUser
			{
				UserName = "student3",
				FirstName = "Youssef",
				LastName = "Khaled",
				Email = "student3@looplearn.com",
				EmailConfirmed = true,
				PhoneNumberConfirmed = true,
				BirthDate = new DateTime(1999, 7, 18),
				Gender = Gender.Male,
				CreatedAt = DateTime.UtcNow
			},
			Password = "Student3@",
			Role = "Student"
		},
		new
		{
			User = new ApplicationUser
			{
				UserName = "admin1",
				FirstName = "Laila",
				LastName = "Mahmoud",
				Email = "admin1@looplearn.com",
				EmailConfirmed = true,
				PhoneNumberConfirmed = true,
				BirthDate = new DateTime(1988, 2, 28),
				Gender = Gender.Female,
				CreatedAt = DateTime.UtcNow
			},
			Password = "Asdiop00@",
			Role = "Admin"
		}
	};

			foreach (var testUser in testUsers)
			{
				// تأكد إنه مش موجود قبل ما تضيفه
				if (await userManager.FindByEmailAsync(testUser.User.Email) is not null)
					continue;

				var result = await userManager.CreateAsync(testUser.User, testUser.Password);
				if (result.Succeeded)
				{
					await userManager.AddToRoleAsync(testUser.User, testUser.Role);
					logger.LogInformation($"Seeded {testUser.Role}: {testUser.User.UserName}");
				}
				else
				{
					logger.LogError($"Failed to seed {testUser.User.UserName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
				}
			}
		}

		// ─────────────────────────────────────────────────────────────
		// Courses + Sections + Lessons
		// ─────────────────────────────────────────────────────────────
		private static async Task SeedCoursesAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger logger)
		{
			if (await context.Courses.AnyAsync())
				return;

			var instructor1 = await userManager.FindByEmailAsync("instructor1@looplearn.com");
			var instructor2 = await userManager.FindByEmailAsync("instructor2@looplearn.com");

			if (instructor1 is null || instructor2 is null)
			{
				logger.LogWarning("Instructors not found — skipping course seeding.");
				return;
			}

			var programmingCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Programming");
			var webDevCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Web Development");
			var dsCategory = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Data Science");

			var csharpTag = await context.Tags.FirstOrDefaultAsync(t => t.Name == "C#");
			var dotnetTag = await context.Tags.FirstOrDefaultAsync(t => t.Name == ".NET Core");
			var jsTag = await context.Tags.FirstOrDefaultAsync(t => t.Name == "JavaScript");
			var reactTag = await context.Tags.FirstOrDefaultAsync(t => t.Name == "React");
			var pythonTag = await context.Tags.FirstOrDefaultAsync(t => t.Name == "Python");
			var mlTag = await context.Tags.FirstOrDefaultAsync(t => t.Name == "Machine Learning");

			var courses = new List<Course>
	{
        // ── Course 1: Free, Published ──────────────────
        new Course
		{
			Title = "C# for Beginners",
			Subtitle = "Learn C# from scratch with hands-on projects",
			Description = "A complete beginner course covering C# fundamentals, OOP, and basic .NET concepts.",
			ThumbnailUrl = "https://placehold.co/600x400?text=CSharp",
			Price = 0,
			IsFree = true,
			Level = CourseLevel.Beginner,
			Language = "en",
			Status = CourseStatus.Published,
			InstructorId = instructor1.Id,
			CategoryId = programmingCategory!.Id,
			CreatedAt = DateTime.UtcNow.AddMonths(-3),
			UpdatedAt = DateTime.UtcNow.AddMonths(-1),
			CourseTags = csharpTag is not null
				? new List<CourseTag> { new CourseTag { TagId = csharpTag.Id } }
				: new List<CourseTag>(),
			Requirements = new List<CourseRequirement>
			{
				new CourseRequirement { Description = "Basic computer knowledge" },
				new CourseRequirement { Description = "No prior programming experience needed" }
			},
			LearningOutcomes = new List<CourseLearningOutcome>
			{
				new CourseLearningOutcome { Description = "Understand C# syntax and fundamentals" },
				new CourseLearningOutcome { Description = "Build simple console applications" },
				new CourseLearningOutcome { Description = "Apply OOP principles in C#" }
			},
			TargetAudiences = new List<CourseTargetAudience>
			{
				new CourseTargetAudience { Description = "Absolute beginners in programming" },
				new CourseTargetAudience { Description = "Developers switching to C#" }
			},
			Sections = new List<Section>
			{
				new Section
				{
					Title = "Getting Started",
					Order = 1,
					Lessons = new List<Lesson>
					{
						new Lesson { Title = "Introduction to C#", Order = 1, Duration = TimeSpan.FromMinutes(10), VideoUrl = "https://example.com/lesson1", IsPreview = true, Description = "Overview of C# and .NET" },
						new Lesson { Title = "Setting Up Visual Studio", Order = 2, Duration = TimeSpan.FromMinutes(8), VideoUrl = "https://example.com/lesson2", IsPreview = true, Description = "Install and configure your IDE" },
						new Lesson { Title = "Your First C# Program", Order = 3, Duration = TimeSpan.FromMinutes(15), VideoUrl = "https://example.com/lesson3", IsPreview = false, Description = "Hello World and basic syntax" }
					}
				},
				new Section
				{
					Title = "Variables and Data Types",
					Order = 2,
					Lessons = new List<Lesson>
					{
						new Lesson { Title = "Value Types vs Reference Types", Order = 1, Duration = TimeSpan.FromMinutes(20), VideoUrl = "https://example.com/lesson4", IsPreview = false, Description = "Understanding the type system" },
						new Lesson { Title = "Working with Strings", Order = 2, Duration = TimeSpan.FromMinutes(18), VideoUrl = "https://example.com/lesson5", IsPreview = false, Description = "String manipulation and formatting" }
					}
				},
				new Section
				{
					Title = "Object-Oriented Programming",
					Order = 3,
					Lessons = new List<Lesson>
					{
						new Lesson { Title = "Classes and Objects", Order = 1, Duration = TimeSpan.FromMinutes(25), VideoUrl = "https://example.com/lesson6", IsPreview = false, Description = "Creating and using classes" },
						new Lesson { Title = "Inheritance and Polymorphism", Order = 2, Duration = TimeSpan.FromMinutes(30), VideoUrl = "https://example.com/lesson7", IsPreview = false, Description = "OOP advanced concepts" }
					}
				}
			}
		},

        // ── Course 2: Paid, Published ──────────────────
        new Course
		{
			Title = "ASP.NET Core Web API",
			Subtitle = "Build production-ready REST APIs with ASP.NET Core 8",
			Description = "Master ASP.NET Core 8 by building a complete Web API from scratch with EF Core, JWT, and best practices.",
			ThumbnailUrl = "https://placehold.co/600x400?text=ASPNET",
			Price = 49.99m,
			IsFree = false,
			Level = CourseLevel.Intermediate,
			Language = "en",
			Status = CourseStatus.Published,
			InstructorId = instructor1.Id,
			CategoryId = programmingCategory!.Id,
			CreatedAt = DateTime.UtcNow.AddMonths(-2),
			UpdatedAt = DateTime.UtcNow.AddDays(-10),
			CourseTags = new List<CourseTag>
			{
				dotnetTag is not null ? new CourseTag { TagId = dotnetTag.Id } : null,
				csharpTag is not null ? new CourseTag { TagId = csharpTag.Id } : null
			}.Where(t => t != null).ToList()!,
			Requirements = new List<CourseRequirement>
			{
				new CourseRequirement { Description = "Basic C# knowledge" },
				new CourseRequirement { Description = "Understanding of OOP concepts" }
			},
			LearningOutcomes = new List<CourseLearningOutcome>
			{
				new CourseLearningOutcome { Description = "Build RESTful APIs with ASP.NET Core 8" },
				new CourseLearningOutcome { Description = "Implement JWT authentication" },
				new CourseLearningOutcome { Description = "Use Entity Framework Core with SQL Server" }
			},
			TargetAudiences = new List<CourseTargetAudience>
			{
				new CourseTargetAudience { Description = "C# developers who want to build APIs" },
				new CourseTargetAudience { Description = "Backend developers learning .NET" }
			},
			Sections = new List<Section>
			{
				new Section
				{
					Title = "Introduction to ASP.NET Core",
					Order = 1,
					Lessons = new List<Lesson>
					{
						new Lesson { Title = "What is ASP.NET Core?", Order = 1, Duration = TimeSpan.FromMinutes(12), VideoUrl = "https://example.com/aspnet1", IsPreview = true, Description = "Overview and architecture" },
						new Lesson { Title = "Project Structure", Order = 2, Duration = TimeSpan.FromMinutes(15), VideoUrl = "https://example.com/aspnet2", IsPreview = false, Description = "Understanding the solution structure" }
					}
				},
				new Section
				{
					Title = "Building Your First API",
					Order = 2,
					Lessons = new List<Lesson>
					{
						new Lesson { Title = "Controllers and Routing", Order = 1, Duration = TimeSpan.FromMinutes(22), VideoUrl = "https://example.com/aspnet3", IsPreview = false, Description = "HTTP methods and routing" },
						new Lesson { Title = "DTOs and Validation", Order = 2, Duration = TimeSpan.FromMinutes(20), VideoUrl = "https://example.com/aspnet4", IsPreview = false, Description = "Data transfer objects" },
						new Lesson { Title = "Entity Framework Core Setup", Order = 3, Duration = TimeSpan.FromMinutes(25), VideoUrl = "https://example.com/aspnet5", IsPreview = false, Description = "Database integration" }
					}
				}
			}
		},

        // ── Course 3: Free, Published (instructor2) ────
        new Course
		{
			Title = "React.js Fundamentals",
			Subtitle = "Build modern UIs with React and JavaScript",
			Description = "Learn React from the ground up — components, hooks, state management, and API integration.",
			ThumbnailUrl = "https://placehold.co/600x400?text=React",
			Price = 0,
			IsFree = true,
			Level = CourseLevel.Beginner,
			Language = "en",
			Status = CourseStatus.Published,
			InstructorId = instructor2.Id,
			CategoryId = webDevCategory!.Id,
			CreatedAt = DateTime.UtcNow.AddMonths(-1),
			UpdatedAt = DateTime.UtcNow.AddDays(-5),
			CourseTags = new List<CourseTag>
			{
				reactTag is not null ? new CourseTag { TagId = reactTag.Id } : null,
				jsTag is not null ? new CourseTag { TagId = jsTag.Id } : null
			}.Where(t => t != null).ToList()!,
			Requirements = new List<CourseRequirement>
			{
				new CourseRequirement { Description = "Basic HTML and CSS knowledge" },
				new CourseRequirement { Description = "JavaScript fundamentals" }
			},
			LearningOutcomes = new List<CourseLearningOutcome>
			{
				new CourseLearningOutcome { Description = "Build React components and manage state" },
				new CourseLearningOutcome { Description = "Use React Hooks effectively" },
				new CourseLearningOutcome { Description = "Integrate REST APIs with React" }
			},
			TargetAudiences = new List<CourseTargetAudience>
			{
				new CourseTargetAudience { Description = "Frontend developers new to React" }
			},
			Sections = new List<Section>
			{
				new Section
				{
					Title = "React Basics",
					Order = 1,
					Lessons = new List<Lesson>
					{
						new Lesson { Title = "What is React?", Order = 1, Duration = TimeSpan.FromMinutes(8), VideoUrl = "https://example.com/react1", IsPreview = true, Description = "Introduction to React" },
						new Lesson { Title = "JSX Syntax", Order = 2, Duration = TimeSpan.FromMinutes(14), VideoUrl = "https://example.com/react2", IsPreview = false, Description = "Writing JSX" },
						new Lesson { Title = "Components and Props", Order = 3, Duration = TimeSpan.FromMinutes(20), VideoUrl = "https://example.com/react3", IsPreview = false, Description = "Building reusable components" }
					}
				},
				new Section
				{
					Title = "React Hooks",
					Order = 2,
					Lessons = new List<Lesson>
					{
						new Lesson { Title = "useState Hook", Order = 1, Duration = TimeSpan.FromMinutes(18), VideoUrl = "https://example.com/react4", IsPreview = false, Description = "Managing component state" },
						new Lesson { Title = "useEffect Hook", Order = 2, Duration = TimeSpan.FromMinutes(22), VideoUrl = "https://example.com/react5", IsPreview = false, Description = "Side effects in React" }
					}
				}
			}
		},

        // ── Course 4: Paid, Draft (instructor2) ────────
        new Course
		{
			Title = "Machine Learning with Python",
			Subtitle = "From theory to practice with scikit-learn and TensorFlow",
			Description = "A comprehensive ML course covering supervised, unsupervised learning, and neural networks.",
			ThumbnailUrl = "https://placehold.co/600x400?text=ML",
			Price = 79.99m,
			IsFree = false,
			Level = CourseLevel.Advanced,
			Language = "en",
			Status = CourseStatus.Draft,
			InstructorId = instructor2.Id,
			CategoryId = dsCategory!.Id,
			CreatedAt = DateTime.UtcNow.AddDays(-20),
			UpdatedAt = DateTime.UtcNow.AddDays(-2),
			CourseTags = new List<CourseTag>
			{
				pythonTag is not null ? new CourseTag { TagId = pythonTag.Id } : null,
				mlTag is not null ? new CourseTag { TagId = mlTag.Id } : null
			}.Where(t => t != null).ToList()!,
			Requirements = new List<CourseRequirement>
			{
				new CourseRequirement { Description = "Python programming experience" },
				new CourseRequirement { Description = "Basic statistics knowledge" }
			},
			LearningOutcomes = new List<CourseLearningOutcome>
			{
				new CourseLearningOutcome { Description = "Implement ML algorithms from scratch" },
				new CourseLearningOutcome { Description = "Build neural networks with TensorFlow" }
			},
			TargetAudiences = new List<CourseTargetAudience>
			{
				new CourseTargetAudience { Description = "Python developers interested in AI/ML" }
			},
			Sections = new List<Section>
			{
				new Section
				{
					Title = "Introduction to ML",
					Order = 1,
					Lessons = new List<Lesson>
					{
						new Lesson { Title = "What is Machine Learning?", Order = 1, Duration = TimeSpan.FromMinutes(15), VideoUrl = "https://example.com/ml1", IsPreview = true, Description = "ML overview and types" }
					}
				}
			}
		}
	};

			await context.Courses.AddRangeAsync(courses);
			await context.SaveChangesAsync();
			logger.LogInformation($"Successfully seeded {courses.Count} courses with sections and lessons.");
		}

		// ─────────────────────────────────────────────────────────────
		// Enrollments + StudentLessonProgress
		// ─────────────────────────────────────────────────────────────
		private static async Task SeedEnrollmentsAndProgressAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger logger)
		{
			if (await context.Enrollments.AnyAsync())
				return;

			var student1 = await userManager.FindByEmailAsync("student1@looplearn.com");
			var student2 = await userManager.FindByEmailAsync("student2@looplearn.com");
			var student3 = await userManager.FindByEmailAsync("student3@looplearn.com");

			if (student1 is null || student2 is null || student3 is null)
			{
				logger.LogWarning("Students not found — skipping enrollment seeding.");
				return;
			}

			var freeCourse = await context.Courses
				.Include(c => c.Sections).ThenInclude(s => s.Lessons)
				.FirstOrDefaultAsync(c => c.Title == "C# for Beginners");

			var paidCourse = await context.Courses
				.Include(c => c.Sections).ThenInclude(s => s.Lessons)
				.FirstOrDefaultAsync(c => c.Title == "ASP.NET Core Web API");

			var reactCourse = await context.Courses
				.Include(c => c.Sections).ThenInclude(s => s.Lessons)
				.FirstOrDefaultAsync(c => c.Title == "React.js Fundamentals");

			if (freeCourse is null || paidCourse is null || reactCourse is null)
			{
				logger.LogWarning("Courses not found — skipping enrollment seeding.");
				return;
			}

			var enrollments = new List<Enrollment>
	{
        // student1 → C# (free) — completed
        new Enrollment
		{
			StudentId = student1.Id,
			CourseId = freeCourse.Id,
			Status = EnrollmentStatus.Active,
			ProgressPercentage = 100,
			IsCompleted = true,
			EnrolledAt = DateTime.UtcNow.AddMonths(-2),
			CompletedAt = DateTime.UtcNow.AddMonths(-1),
			LastAccessAt = DateTime.UtcNow.AddMonths(-1)
		},
        // student1 → ASP.NET (paid) — in progress
        new Enrollment
		{
			StudentId = student1.Id,
			CourseId = paidCourse.Id,
			Status = EnrollmentStatus.Active,
			ProgressPercentage = 40,
			IsCompleted = false,
			EnrolledAt = DateTime.UtcNow.AddDays(-15),
			LastAccessAt = DateTime.UtcNow.AddDays(-1)
		},
        // student2 → C# (free) — in progress
        new Enrollment
		{
			StudentId = student2.Id,
			CourseId = freeCourse.Id,
			Status = EnrollmentStatus.Active,
			ProgressPercentage = 60,
			IsCompleted = false,
			EnrolledAt = DateTime.UtcNow.AddDays(-20),
			LastAccessAt = DateTime.UtcNow.AddDays(-2)
		},
        // student2 → React (free) — just enrolled
        new Enrollment
		{
			StudentId = student2.Id,
			CourseId = reactCourse.Id,
			Status = EnrollmentStatus.Active,
			ProgressPercentage = 0,
			IsCompleted = false,
			EnrolledAt = DateTime.UtcNow.AddDays(-3),
			LastAccessAt = DateTime.UtcNow.AddDays(-3)
		},
        // student3 → React (free) — completed
        new Enrollment
		{
			StudentId = student3.Id,
			CourseId = reactCourse.Id,
			Status = EnrollmentStatus.Active,
			ProgressPercentage = 100,
			IsCompleted = true,
			EnrolledAt = DateTime.UtcNow.AddDays(-25),
			CompletedAt = DateTime.UtcNow.AddDays(-5),
			LastAccessAt = DateTime.UtcNow.AddDays(-5)
		}
	};

			await context.Enrollments.AddRangeAsync(enrollments);

			// ── Lesson Progress ───────────────────────────────
			var allLessons = freeCourse.Sections
				.SelectMany(s => s.Lessons)
				.Concat(reactCourse.Sections.SelectMany(s => s.Lessons))
				.ToList();

			var progressRecords = new List<StudentLessonProgress>();

			// student1 — C# course completed (كل الـ lessons)
			foreach (var lesson in freeCourse.Sections.SelectMany(s => s.Lessons))
			{
				progressRecords.Add(new StudentLessonProgress
				{
					StudentId = student1.Id,
					LessonId = lesson.Id,
					IsCompleted = true,
					WatchedPercentage = 100,
					LastSecondWatched = (int)lesson.Duration.TotalSeconds,
					CompletedAt = DateTime.UtcNow.AddMonths(-1)
				});
			}

			// student3 — React course completed
			foreach (var lesson in reactCourse.Sections.SelectMany(s => s.Lessons))
			{
				progressRecords.Add(new StudentLessonProgress
				{
					StudentId = student3.Id,
					LessonId = lesson.Id,
					IsCompleted = true,
					WatchedPercentage = 100,
					LastSecondWatched = (int)lesson.Duration.TotalSeconds,
					CompletedAt = DateTime.UtcNow.AddDays(-5)
				});
			}

			await context.StudentLessonProgresses.AddRangeAsync(progressRecords);
			await context.SaveChangesAsync();
			logger.LogInformation("Successfully seeded enrollments and lesson progress.");
		}

		// ─────────────────────────────────────────────────────────────
		// Feedbacks
		// ─────────────────────────────────────────────────────────────
		private static async Task SeedFeedbacksAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, ILogger logger)
		{
			if (await context.Feedbacks.AnyAsync())
				return;

			var student1 = await userManager.FindByEmailAsync("student1@looplearn.com");
			var student3 = await userManager.FindByEmailAsync("student3@looplearn.com");

			if (student1 is null || student3 is null)
			{
				logger.LogWarning("Students not found — skipping feedback seeding.");
				return;
			}

			var freeCourse = await context.Courses.FirstOrDefaultAsync(c => c.Title == "C# for Beginners");
			var reactCourse = await context.Courses.FirstOrDefaultAsync(c => c.Title == "React.js Fundamentals");

			if (freeCourse is null || reactCourse is null)
			{
				logger.LogWarning("Courses not found — skipping feedback seeding.");
				return;
			}

			var feedbacks = new List<Feedback>
	{
		new Feedback
		{
			StudentId = student1.Id,
			CourseId = freeCourse.Id,
			Rating = 5,
			Comment = "Excellent course! Very clear explanations and great examples.",
			CreatedAt = DateTime.UtcNow.AddMonths(-1),
			UpdatedAt = DateTime.UtcNow.AddMonths(-1)
		},
		new Feedback
		{
			StudentId = student3.Id,
			CourseId = reactCourse.Id,
			Rating = 4,
			Comment = "Very good course. Covers all the fundamentals well.",
			CreatedAt = DateTime.UtcNow.AddDays(-4),
			UpdatedAt = DateTime.UtcNow.AddDays(-4)
		}
	};

			await context.Feedbacks.AddRangeAsync(feedbacks);
			await context.SaveChangesAsync();
			logger.LogInformation("Successfully seeded feedbacks.");
		}

		// ─────────────────────────────────────────────────────────────
		// Quizzes + Questions + Options
		// ─────────────────────────────────────────────────────────────
		private static async Task SeedQuizzesAsync(ApplicationDbContext context, ILogger logger)
		{
			if (await context.Quizzes.AnyAsync())
				return;

			var freeCourse = await context.Courses
				.Include(c => c.Sections)
				.FirstOrDefaultAsync(c => c.Title == "C# for Beginners");

			if (freeCourse is null)
			{
				logger.LogWarning("Course not found — skipping quiz seeding.");
				return;
			}

			var firstSection = freeCourse.Sections.OrderBy(s => s.Order).FirstOrDefault();

			if (firstSection is null)
				return;

			var quiz = new Quiz
			{
				Title = "C# Basics Quiz",
				Description = "Test your understanding of C# fundamentals.",
				PassingScore = 70,
				IsRequired = true,
				Type = QuizType.SectionQuiz,
				SectionId = firstSection.Id,
				CourseId = freeCourse.Id,
				Questions = new List<Question>
		{
			new Question
			{
				Body = "Which keyword is used to declare a variable in C#?",
				Points = 10,
				Options = new List<Option>
				{
					new Option { Body = "var", IsCorrect = true },
					new Option { Body = "let", IsCorrect = false },
					new Option { Body = "dim", IsCorrect = false },
					new Option { Body = "define", IsCorrect = false }
				}
			},
			new Question
			{
				Body = "What is the correct way to create a class in C#?",
				Points = 10,
				Options = new List<Option>
				{
					new Option { Body = "class MyClass { }", IsCorrect = true },
					new Option { Body = "def MyClass { }", IsCorrect = false },
					new Option { Body = "struct MyClass { }", IsCorrect = false },
					new Option { Body = "object MyClass { }", IsCorrect = false }
				}
			},
			new Question
			{
				Body = "Which of the following is a value type in C#?",
				Points = 10,
				Options = new List<Option>
				{
					new Option { Body = "int", IsCorrect = true },
					new Option { Body = "string", IsCorrect = false },
					new Option { Body = "array", IsCorrect = false },
					new Option { Body = "class", IsCorrect = false }
				}
			}
		}
			};

			await context.Quizzes.AddAsync(quiz);
			await context.SaveChangesAsync();
			logger.LogInformation("Successfully seeded quizzes with questions and options.");
		}

	}
}