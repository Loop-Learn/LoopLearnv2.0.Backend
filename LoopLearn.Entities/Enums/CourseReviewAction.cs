using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoopLearn.Entities.Enums
{
	public enum CourseReviewAction
	{
		Submitted,   // Instructor submitted
		Approved,    // Admin approved
		Rejected,    // Admin rejected
		Archived    // Admin archived
	}
}
