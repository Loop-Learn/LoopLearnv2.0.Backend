using LoopLearn.Entities.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LoopLearn.DataAccess.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        #region DbSets
        public DbSet<Category> Categories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Section> Sections { get; set; }
        public DbSet<Lesson> Lessons { get; set; }

        public DbSet<CourseRequirement> CourseRequirements { get; set; }
        public DbSet<CourseLearningOutcome> CourseLearningOutcomes { get; set; }
        public DbSet<CourseTargetAudience> CourseTargetAudiences { get; set; }

        public DbSet<Tag> Tags { get; set; }
        public DbSet<CourseTag> CourseTags { get; set; }

        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<StudentLessonProgress> StudentLessonProgresses { get; set; }

        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<LessonComment> LessonComments { get; set; }

        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Option> Options { get; set; }

        public DbSet<QuizAttempt> QuizAttempts { get; set; }
        public DbSet<StudentAnswer> StudentAnswers { get; set; }

        #endregion

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Composite Keys
            modelBuilder.Entity<CourseTag>()
                .HasKey(x => new { x.CourseId, x.TagId });

            modelBuilder.Entity<Enrollment>()
                .HasKey(x => new { x.StudentId, x.CourseId });

            modelBuilder.Entity<StudentLessonProgress>()
                .HasKey(x => new { x.StudentId, x.LessonId });

            // Course Configuration
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Instructor)
                .WithMany(u => u.Courses)
                .HasForeignKey(c => c.InstructorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Category)
                .WithMany(c=>c.Courses)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Course>()
                .Property(c => c.Price)
                .HasPrecision(18, 2);

            // Section Configuration
            modelBuilder.Entity<Section>()
                .HasOne(s => s.Course)
                .WithMany(c => c.Sections)
                .HasForeignKey(s => s.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Lesson Configuration
            modelBuilder.Entity<Lesson>()
                .HasOne(l => l.Section)
                .WithMany(s => s.Lessons)
                .HasForeignKey(l => l.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            // StudentLessonProgress Configuration
            modelBuilder.Entity<StudentLessonProgress>()
                .HasOne(slp => slp.Student)
                .WithMany(u => u.LessonProgresses)
                .HasForeignKey(slp => slp.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentLessonProgress>()
                .HasOne(slp => slp.Lesson)
                .WithMany(l => l.LessonProgresses)
                .HasForeignKey(slp => slp.LessonId)
                .OnDelete(DeleteBehavior.Restrict);

            // LessonComment Configuration
            modelBuilder.Entity<LessonComment>()
                .HasOne(lc => lc.Student)
                .WithMany(u => u.LessonComments)
                .HasForeignKey(lc => lc.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LessonComment>()
                .HasOne(lc => lc.Lesson)
                .WithMany(l => l.LessonComments)
                .HasForeignKey(lc => lc.LessonId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LessonComment>()
                .HasOne(x => x.ParentComment)
                .WithMany()
                .HasForeignKey(x => x.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Enrollment Configuration
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Student)
                .WithMany(u => u.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Feedback Configuration
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Student)
                .WithMany(u => u.Feedbacks)
                .HasForeignKey(f => f.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Course)
                .WithMany(c => c.Feedbacks)
                .HasForeignKey(f => f.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Feedback>()
                .Property(f => f.Rating)
                .HasPrecision(2, 1);

            // Quiz Configuration
            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Section)
                .WithMany(s => s.Quizzes)
                .HasForeignKey(q => q.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Quiz>()
                .HasOne(q => q.Course)
                .WithMany(c => c.Quizzes)
                .HasForeignKey(q => q.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            // Question Configuration
            modelBuilder.Entity<Question>()
                .HasOne(q => q.Quiz)
                .WithMany(qz => qz.Questions)
                .HasForeignKey(q => q.QuizId)
                .OnDelete(DeleteBehavior.Restrict);

            // Option Configuration
            modelBuilder.Entity<Option>()
                .HasOne(o => o.Question)
                .WithMany(q => q.Options)
                .HasForeignKey(o => o.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            // QuizAttempt Configuration
            modelBuilder.Entity<QuizAttempt>()
                .HasOne(qa => qa.Student)
                .WithMany()
                .HasForeignKey(qa => qa.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuizAttempt>()
                .HasOne(qa => qa.Quiz)
                .WithMany()
                .HasForeignKey(qa => qa.QuizId)
                .OnDelete(DeleteBehavior.Restrict);

            // StudentAnswer Configuration
            modelBuilder.Entity<StudentAnswer>()
                .HasOne(sa => sa.QuizAttempt)
                .WithMany(qa => qa.Answers)
                .HasForeignKey(sa => sa.QuizAttemptId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentAnswer>()
                .HasOne(sa => sa.Question)
                .WithMany()
                .HasForeignKey(sa => sa.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<StudentAnswer>()
                .HasOne(sa => sa.Option)
                .WithMany()
                .HasForeignKey(sa => sa.OptionId)
                .OnDelete(DeleteBehavior.Restrict);

            // Apply Restrict delete behavior to all remaining relationships not explicitly configured
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                if (relationship.DeleteBehavior == DeleteBehavior.Cascade)
                {
                    relationship.DeleteBehavior = DeleteBehavior.Restrict;
                }
            }
        }
    }
}
