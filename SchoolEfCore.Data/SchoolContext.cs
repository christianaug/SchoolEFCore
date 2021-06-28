using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Text;
using SchoolEFCore.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace SchoolEFCore.Data
{
	public class SchoolContext : DbContext
	{
		public static readonly ILoggerFactory loggerFactory = LoggerFactory.Create(
			builder =>
			{
				builder.AddConsole();
			}
		);
		public DbSet<Student> Students { get; set; }
		public DbSet<Course> Courses { get; set; }
		public DbSet<CourseStudent> CourseStudents { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder
				.UseLoggerFactory(loggerFactory)
				.EnableSensitiveDataLogging()
				.UseSqlServer("Data Source = (localdb)\\MSSQLLocalDB; Initial Catalog = SchoolDB");
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			//tells the model builder that the entity SamuraiBattle has a key that consists of
			//both the StudentId and the CourseId, they keys are also foreign keys that point to the the actual entries in either table
			//this is the final step needed to create a many to many
			modelBuilder.Entity<CourseStudent>().HasKey(s => new { s.StudentId, s.CourseId });

		}

	}
}
