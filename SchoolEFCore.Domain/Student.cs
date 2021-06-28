using System;
using System.Collections.Generic;
using System.Text;

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
