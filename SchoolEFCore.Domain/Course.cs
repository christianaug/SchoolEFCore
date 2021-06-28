using System;
using System.Collections.Generic;
using System.Text;

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
