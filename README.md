# SchoolEFCore

## What is Entity Framework Core?

EF Core is a known as an Object Relational Mapper (ORM) it is a way for you as a developer to interact with the data mapped directly to objects that you can work with in your applications.

In this case the EF Core takes the database results and maps them to Entities, objects that closely model the schemas in your database, and allows you to work with the data within the domain of your application.

### DbContext

What in the heck is a DbContext? well for many of you familiar with databases you can think it as a session with a database, all of your communication between your application and the database happens through this class.  Its responsible for:

1. Taking your queries  and translating them into the appropriate SQL Queries to send to the database
2. taking query results and translating them into entities
3. adding new entities
4. keep track of entities using the ChangeTracker
5. and saving changes made to entities

### ChangeTracker

The ChangeTracker tracks changes made to entities by assigning them a State:

- Added
- Deleted
- Modified
- Unchanged
- Detached

when an operation is applied like adding a new entity or modifying an existing one, these states are applied directly onto the entity so that the change tracker knows what operations it should apply when generating its SQL, and it will update the context for the entity that is stored in your application.

## Getting Started

### Models

the models we are using might look strange if you're new to EF Core but to sum it up, each class contains properties, they represent the columns in the tables. the other properties are called navigation properties, the context will understand that there exists a relationship between your entities and when querying will allow you traverse your way through these relationships.

The models consist of the following, a Student and Course Entity and another entity that model the many-to-many relationship between the two.

```csharp
namespace SchoolEFCore.Domain
{
	public class Student
	{
		public Guid Id { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public DateTime DateOfBirth { get; set; }
		//navigation properties
		public List<Course> Courses{ get; set; }
	}
}
```

```csharp
namespace SchoolEFCore.Domain
{
	public class Course
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		//navigation properties
		public List<Student> Students{ get; set; }
	}
}
```

```csharp
namespace SchoolEFCore.Domain
{
	public class CourseStudent
	{
		public Guid StudentId { get; set; }
		public Guid CourseId { get; set; }
		//navigation properties
		public Student Student { get; set; }
		public Course Course { get; set; }
	}
}
```

### DbContext

This file is the context that was being referred to in the beginning of this post. You might have noticed the properties with the type *DbSet,* this is the way the context keeps track of all the entities that you are referencing in your queries.

```csharp
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
```

## Working With Data

when working with an entity there are two scenarios you have to take into account: connected and disconnected.

In the connected scenario the same instance of the DbContext is retrieving, inserting, updating, and deleting entities. You can think of this version as a console application or a windows forms application, really anywhere where the context is still able to watch for changes in entities over its lifetime.

The disconnected scenario is when different instances of the DbContext is used to save and retrieve entities. the issue with the disconnected scenario is that the change tracker isn't able to watch for changes in entities, you are responsible for watching the entities and applying one of the entity states mentioned above so that the SQL is generated appropriately when SaveChanges() is called

## Connected Scenarios

### Adding Data

Adding a new entry is relatively easy and thats thanks to the ChangeTracker. We build a new entity by instantiating a new Student object, we then call the DbSet.Add() method on the Students DbSet.

This saves the student to the DbContext with the Entity State set as "Added". When SaveChanges() is called it builds the insert statement bellow.

```csharp
public static void AddNewStudent()
		{
			using (var context = new SchoolContext())
			{
				var student = new Student()
				{
					FirstName = "Christian",
					LastName = "Augustyn",
					DateOfBirth = new DateTime(1999, 1, 10)
				};

				context.Students.Add(student);
				context.SaveChanges();
			}
		}
```

```powershell
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (21ms) [Parameters=[@p0='cc438b71-93ab-4a09-0bd0-08d93a59630c', @p1='1999-01-10T00:00:00.0000000', @p2='Christian' (Size = 4000), @p3='Augustyn' (Size = 4000)], CommandType='Text', CommandTimeout='30']
      SET NOCOUNT ON;
      INSERT INTO [Students] ([Id], [DateOfBirth], [FirstName], [LastName])
      VALUES (@p0, @p1, @p2, @p3);
```

You can also nest your entities when adding them, this is also some magic that EF Core is able to do thanks to the change Tracker, The DbSet.Add() method will take the entity passed in as a parameter and will apply the "Added" state to all the entities down the graph that aren't being tracked. it infers that a relationship exists because of the existing models and will generate the SQL to insert both items even though you are only referencing the Courses DbSet.

```csharp
public static void AddCourseWithStudent()
		{
			using (var context = new SchoolContext())
			{
				var course = new Course()
				{
					Name = "Math",
					Students = new List<Student>
					{
						new Student()
						{
							FirstName = "John",
							LastName = "Doe",
							DateOfBirth = new DateTime(1989, 2, 12)
						}
					}
				};

				context.Courses.Add(course);
				context.SaveChanges();
			}
		}
```

And here is the generated SQL for that graph entity. It first inserts the new course as well as the new student and infers the relationship between them and inserts that into the CourseStudent Table with the associated Id's from both of the new entries! pretty cool stuff!

```powershell
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (22ms) [Parameters=[@p0='12468e33-6872-4d4d-59b2-08d93a5bb4d5', @p1='Math' (Size = 4000)], CommandType='Text', CommandTimeout='30']
      SET NOCOUNT ON;
      INSERT INTO [Courses] ([Id], [Name])
      VALUES (@p0, @p1);
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (1ms) [Parameters=[@p0='72e51fec-e51a-4ed0-709b-08d93a5bb4de', @p1='1989-02-12T00:00:00.0000000', @p2='John' (Size = 4000), @p3='Doe' (Size = 4000)], CommandType='Text', CommandTimeout='30']
      SET NOCOUNT ON;
      INSERT INTO [Students] ([Id], [DateOfBirth], [FirstName], [LastName])
      VALUES (@p0, @p1, @p2, @p3);
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (1ms) [Parameters=[@p4='12468e33-6872-4d4d-59b2-08d93a5bb4d5', @p5='72e51fec-e51a-4ed0-709b-08d93a5bb4de'], CommandType='Text', CommandTimeout='30']
      SET NOCOUNT ON;
      INSERT INTO [CourseStudent] ([CoursesId], [StudentsId])
      VALUES (@p4, @p5);
```

### Modifying Data

Modifying data in a connected scenario is easy as well, as long as the ChangeTracker is able to watch entities within its life time the changes will happen with ease.

```csharp
public static void ModifyExistingStudent()
		{
			using (var context = new SchoolContext())
			{
				var student = context.Students
					.Where(s => EF.Functions.Like(s.FirstName, "Christian"))
					.FirstOrDefault();

				if (student == null)
				{
					return;
				}

				student.LastName = "Pulisic";
				context.SaveChanges();
			}
		}
```

The SQL that is generated for the updated entity looks a little different from adding a new one. The first SQL statement used is to select the Student with the name "Christian", now that the student persists in the DbContext, the Change Tracker will watch for any changes applied to that entity. The second statement that was generated uses and update instead of an insert, that's because the Change Tracker noticed that there was a modification to the last name property and as a result set the entities state to "Modified".

```powershell
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (15ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [s].[Id], [s].[DateOfBirth], [s].[FirstName], [s].[LastName]
      FROM [Students] AS [s]
      WHERE [s].[FirstName] LIKE N'Christian'
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (13ms) [Parameters=[@p1='cc438b71-93ab-4a09-0bd0-08d93a59630c', @p0='Pulisic' (Size = 4000)], CommandType='Text', CommandTimeout='30']
      SET NOCOUNT ON;
      UPDATE [Students] SET [LastName] = @p0
      WHERE [Id] = @p1;
      SELECT @@ROWCOUNT;
```

### Deleting Data

Removing an entity is very simple as well, in this case were are looking at retrieving the entity whos name is "John", we can use the DbSet.Remove() method to remove an entity that is currently being tracked.

```csharp
public static void RemoveExistingStudent()
		{
			using (var context = new SchoolContext())
			{
				var student = context.Students
					.Where(s => EF.Functions.Like(s.FirstName, "John"))
					.FirstOrDefault();

				if (student == null)
				{
					return;
				}

				context.Students.Remove(student);
				context.SaveChanges();
			}
		}
```

Because the change tracker was watching the entity, it knew to apply the "Deleted" state to the entity which generated the delete statement in SQL.

```powershell
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (15ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [s].[Id], [s].[DateOfBirth], [s].[FirstName], [s].[LastName]
      FROM [Students] AS [s]
      WHERE [s].[FirstName] LIKE N'John'
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (14ms) [Parameters=[@p0='72e51fec-e51a-4ed0-709b-08d93a5bb4de'], CommandType='Text', CommandTimeout='30']
      SET NOCOUNT ON;
      DELETE FROM [Students]
      WHERE [Id] = @p0;
      SELECT @@ROWCOUNT;
```

## Disconnected Scenario

### Adding Data

In this situation we are creating a new student outside of the DbContext, and check  the Id, if it's empty we can assign the entity state as added, otherwise the entity already exists and we set its state as modified. the DbContext.Entry() method returns back an EntityEntry which gives access to modify its state within the context.

You can also use the DbSet.Add() method to set the entity state as Added.

```csharp
public static void AddNewStudentDisconnected()
		{
			var disconnectedStudent = new Student()
			{
				FirstName = "Isaac",
				LastName = "Newton",
				DateOfBirth = new DateTime(1643, 1, 4)
			};

			using (var context = new SchoolContext())
			{
				var studentEntry = context.Entry(disconnectedStudent);

				if(disconnectedStudent.Id == Guid.Empty)
				{
					studentEntry.State = EntityState.Added;
				}
				else
				{
					studentEntry.State = EntityState.Modified;
				}
				
				//OR
				//context.Students.Add(disconnectedStudent);

				context.SaveChanges();
			}

		}
```

Because the entity state was set as "Added" EF Core generated an insert statement just like if we were to use DbSet.Add().

```powershell
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (26ms) [Parameters=[@p0='677941ce-c690-473f-97b0-08d93a6ebb23', @p1='1643-01-04T00:00:00.0000000', @p2='Isaac' (Size = 4000), @p3='Newton' (Size = 4000)], CommandType='Text', CommandTimeout='30']
      SET NOCOUNT ON;
      INSERT INTO [Students] ([Id], [DateOfBirth], [FirstName], [LastName])
      VALUES (@p0, @p1, @p2, @p3);
```

### Modifying Data

Modifying an entity is similar to adding, this time lets retrieve an entity from the database using a separate DbContext instance.

```csharp
public static void ModifyExistingStudentDisconnected()
		{
			Student disconnectedStudent;

			using (var context = new SchoolContext())
			{
				disconnectedStudent = context.Students
					.Where(s => EF.Functions.Like(s.FirstName, "Isaac"))
					.FirstOrDefault();
			}

			using (var context = new SchoolContext())
			{
				var studentEntry = context.Entry(disconnectedStudent);

				if (disconnectedStudent.Id == Guid.Empty)
				{
					studentEntry.State = EntityState.Added;
				}
				else
				{
					studentEntry.State = EntityState.Modified;
				}

				context.SaveChanges();
			}

		}
```

Although there were never any modifications done directly on the entity with the name "Isaac", it still existed and we have successfully told the Change Tracker that this entity is not new and is instead "Modified".

```powershell
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (18ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [s].[Id], [s].[DateOfBirth], [s].[FirstName], [s].[LastName]
      FROM [Students] AS [s]
      WHERE [s].[FirstName] LIKE N'Isaac'
info: Microsoft.EntityFrameworkCore.Infrastructure[10403]
      Entity Framework Core 5.0.7 initialized 'SchoolContext' using provider 'Microsoft.EntityFrameworkCore.SqlServer' with options: SensitiveDataLoggingEnabled
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (17ms) [Parameters=[@p3='677941ce-c690-473f-97b0-08d93a6ebb23', @p0='1643-01-04T00:00:00.0000000', @p1='Isaac' (Size = 4000), @p2='Newton' (Size = 4000)], CommandType='Text', CommandTimeout='30']
      SET NOCOUNT ON;
      UPDATE [Students] SET [DateOfBirth] = @p0, [FirstName] = @p1, [LastName] = @p2
      WHERE [Id] = @p3;
      SELECT @@ROWCOUNT;
```

### Deleting Data

Deleting an entity is easy, we need to set the state of the entity as "Deleted" and the amazing part is that we can make an entity using only the Id property. In this example we are going to retrieve the entity in a separate instance and remove it by creating an entity that only has the Id.

```csharp
public static void RemoveStudentDisconnected()
		{
			Student disconnectedStudent;

			using (var context = new SchoolContext())
			{
				disconnectedStudent = context.Students
					.Where(s => EF.Functions.Like(s.FirstName, "Christian"))
					.FirstOrDefault();
			}

			var fakeStudent = new Student()
			{
				Id = disconnectedStudent.Id
			};

			using (var context = new SchoolContext())
			{
				context.Entry(fakeStudent).State = EntityState.Deleted;
				context.SaveChanges();
			}
		}
```

As you can see we have successfully removed the entry in the database without actually needing the entity its self, we made an entity that uses the same Id!

```powershell
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (17ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT TOP(1) [s].[Id], [s].[DateOfBirth], [s].[FirstName], [s].[LastName]
      FROM [Students] AS [s]
      WHERE [s].[FirstName] LIKE N'Christian'
info: Microsoft.EntityFrameworkCore.Infrastructure[10403]
      Entity Framework Core 5.0.7 initialized 'SchoolContext' using provider 'Microsoft.EntityFrameworkCore.SqlServer' with options: SensitiveDataLoggingEnabled
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (14ms) [Parameters=[@p0='cc438b71-93ab-4a09-0bd0-08d93a59630c'], CommandType='Text', CommandTimeout='30']
      SET NOCOUNT ON;
      DELETE FROM [Students]
      WHERE [Id] = @p0;
      SELECT @@ROWCOUN
```

