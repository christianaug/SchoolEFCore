using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SchoolEFCore.Data;
using SchoolEFCore.Domain;

namespace SchoolEFCore
{
	class Program
	{
		static void Main(string[] args)
		{
			//AddNewStudent();
			//ModifyExistingStudent();
			//AddCourseWithStudent();
			//RemoveExistingStudent();
			//AddNewStudentDisconnected();
			//ModifyExistingStudentDisconnected();
			//RemoveStudentDisconnected();
		}

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

		//DISCONNECTED

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

				context.SaveChanges();
			}

		}

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

		public static void GetStudentByName(string name)
		{
			using (var context = new SchoolContext())
			{
				var student = context.Students.Where(s => EF.Functions.Like(s.FirstName, name)).FirstOrDefault();
				Console.WriteLine(student.LastName);
			}
		}
	}
}
