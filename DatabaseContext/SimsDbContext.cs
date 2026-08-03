using Microsoft.EntityFrameworkCore;
using SIMS.DatabaseContext.Entities;

namespace SIMS.DatabaseContext
{
    public class SimsDbContext : DbContext
    {
        public SimsDbContext(DbContextOptions<SimsDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseStudent> CourseStudents { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Grade> Grades { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Submission> Submissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Users
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);
            });

            // Students
            modelBuilder.Entity<Student>(entity =>
            {
                entity.ToTable("Students");

                entity.HasKey(s => s.Id);

                entity.Property(s => s.StudentCode)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(s => s.FullName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(s => s.Email)
                    .HasMaxLength(100);

                entity.Property(s => s.Phone)
                    .HasMaxLength(20);

                entity.Property(s => s.Address)
                    .HasMaxLength(200);

                entity.HasOne(s => s.User)
                    .WithMany()
                    .HasForeignKey(s => s.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Courses
            modelBuilder.Entity<Course>(entity =>
            {
                entity.ToTable("Courses");
                entity.HasKey(c => c.Id);
            });

            // CourseStudent
            modelBuilder.Entity<CourseStudent>(entity =>
            {
                entity.ToTable("CourseStudent");
                entity.HasKey(cs => cs.Id);
            });

            // Teachers
            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.ToTable("Teachers");

                entity.HasKey(t => t.Id);

                entity.Property(t => t.Id)
                    .HasColumnName("TeacherId");

                entity.Property(t => t.TeacherCode)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(t => t.FullName)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(t => t.Email)
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(t => t.Phone)
                    .HasMaxLength(20);

                entity.Property(t => t.Department)
                    .HasMaxLength(100);

                entity.Property(t => t.CreatedDate)
                    .HasColumnType("datetime");
            });

            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.ToTable("Schedules");

                entity.HasKey(s => s.Id);

                entity.Property(s => s.Room)
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(s => s.StartTime)
                    .HasColumnType("time");

                entity.Property(s => s.EndTime)
                    .HasColumnType("time");

                entity.Property(s => s.StartDate)
                    .HasColumnType("date");

                entity.Property(s => s.EndDate)
                    .HasColumnType("date");

                entity.Property(s => s.CreatedAt)
                    .HasColumnType("datetime");

                entity.HasOne(s => s.Course)
                    .WithMany()
                    .HasForeignKey(s => s.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Teacher)
                    .WithMany()
                    .HasForeignKey(s => s.TeacherId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Teacher - User
            modelBuilder.Entity<Teacher>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Attendances
            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.ToTable("Attendances");

                entity.HasKey(a => a.Id);

                entity.Property(a => a.AttendanceDate)
                    .HasColumnType("date");

                entity.Property(a => a.Status)
                    .HasMaxLength(20)
                    .IsRequired();

                entity.Property(a => a.Note)
                    .HasMaxLength(250);

                entity.Property(a => a.CreatedAt)
                    .HasColumnType("datetime");

                entity.Property(a => a.UpdatedAt)
                    .HasColumnType("datetime");

                entity.HasIndex(a => new
                {
                    a.ScheduleId,
                    a.StudentId,
                    a.AttendanceDate
                }).IsUnique();

                entity.HasOne(a => a.Schedule)
                    .WithMany()
                    .HasForeignKey(a => a.ScheduleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Student)
                    .WithMany()
                    .HasForeignKey(a => a.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Grades
            modelBuilder.Entity<Grade>(entity =>
            {
                entity.ToTable("Grades");

                entity.HasKey(g => g.Id);

                entity.Property(g => g.AssignmentScore)
                    .HasColumnType("decimal(5,2)");

                entity.Property(g => g.MidtermScore)
                    .HasColumnType("decimal(5,2)");

                entity.Property(g => g.FinalScore)
                    .HasColumnType("decimal(5,2)");

                entity.Property(g => g.TotalScore)
                    .HasColumnType("decimal(5,2)");

                entity.Property(g => g.Note)
                    .HasMaxLength(250);

                entity.Property(g => g.CreatedAt)
                    .HasColumnType("datetime");

                entity.Property(g => g.UpdatedAt)
                    .HasColumnType("datetime");

                entity.HasIndex(g => new
                {
                    g.CourseId,
                    g.StudentId
                }).IsUnique();

                entity.HasOne(g => g.Course)
                    .WithMany()
                    .HasForeignKey(g => g.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.Student)
                    .WithMany()
                    .HasForeignKey(g => g.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(g => g.Teacher)
                    .WithMany()
                    .HasForeignKey(g => g.TeacherId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Assignments
            modelBuilder.Entity<Assignment>(entity =>
            {
                entity.ToTable("Assignments");

                entity.HasKey(a => a.Id);

                entity.Property(a => a.Title)
                    .HasMaxLength(200)
                    .IsRequired();

                entity.Property(a => a.MaxScore)
                    .HasColumnType("decimal(5,2)");

                entity.Property(a => a.DueDate)
                    .HasColumnType("datetime");

                entity.Property(a => a.CreatedAt)
                    .HasColumnType("datetime");

                entity.Property(a => a.UpdatedAt)
                    .HasColumnType("datetime");

                entity.HasOne(a => a.Course)
                    .WithMany()
                    .HasForeignKey(a => a.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(a => a.Teacher)
                    .WithMany()
                    .HasForeignKey(a => a.TeacherId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Submissions
            modelBuilder.Entity<Submission>(entity =>
            {
                entity.ToTable("Submissions");

                entity.HasKey(s => s.Id);

                entity.Property(s => s.OriginalFileName)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(s => s.StoredFileName)
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(s => s.FilePath)
                    .HasMaxLength(500)
                    .IsRequired();

                entity.Property(s => s.Score)
                    .HasColumnType("decimal(5,2)");

                entity.Property(s => s.Feedback)
                    .HasMaxLength(1000);

                entity.Property(s => s.SubmittedAt)
                    .HasColumnType("datetime");

                entity.Property(s => s.GradedAt)
                    .HasColumnType("datetime");

                entity.HasIndex(s => new
                {
                    s.AssignmentId,
                    s.StudentId
                }).IsUnique();

                entity.HasOne(s => s.Assignment)
                    .WithMany()
                    .HasForeignKey(s => s.AssignmentId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(s => s.Student)
                    .WithMany()
                    .HasForeignKey(s => s.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


        }
    }
}