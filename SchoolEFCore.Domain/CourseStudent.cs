using System;
using System.Collections.Generic;
using System.Text;

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
