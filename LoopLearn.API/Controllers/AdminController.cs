using LoopLearn.API.Services;
using LoopLearn.API.Services.Courses;
using LoopLearn.Entities.DTOs.Course;
using LoopLearn.Entities.DTOs.Users;
using LoopLearn.Entities.DTOs.Category;
using LoopLearn.Entities.DTOs.Tag;
using LoopLearn.Entities.Enums;
using LoopLearn.Entities.Interfaces;
using LoopLearn.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LoopLearn.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize(Roles = "Admin,SuperAdmin")]
	public class AdminController : ControllerBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly CourseMappingService _courseMappingService;
		private readonly UserManager<ApplicationUser> _userManager;

		public AdminController(IUnitOfWork unitOfWork,
			CourseMappingService courseMappingService,
			UserManager<ApplicationUser> userManager)
		{
			_unitOfWork = unitOfWork;
			_courseMappingService = courseMappingService;
			_userManager = userManager;
		}

		private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)
									  ?? throw new UnauthorizedAccessException();

		// =============================================
		// GET /api/admin/courses/pending
		// Returns all courses awaiting review, ordered by submission date
		// (oldest first — review in the order they came in).
		// =============================================
		[HttpGet("courses/pending")]
		public async Task<IActionResult> GetPendingCourses()
		{
			try
			{
				var pendingCourses = await _unitOfWork.Courses
					.GetAsync(
						predicate: c => c.Status == CourseStatus.PendingReview && !c.IsDeleted,
						selector: c => new PendingCourseDTO
						{
							Id = c.Id,
							Title = c.Title,
							Subtitle = c.Subtitle,
							ThumbnailUrl = c.ThumbnailUrl,
							InstructorName = c.Instructor.FullName,
							InstructorEmail = c.Instructor.Email ?? "Email Not Provided.",
							Category = c.Category.Name,
							Price = c.Price,
							IsFree = c.IsFree,
							SubmittedForReviewAt = c.SubmittedForReviewAt
						},
						includes: "Instructor,Category",
						orderBy: q => q.OrderBy(c => c.SubmittedForReviewAt)
					);

				return Ok(new { success = true, data = pendingCourses });
			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new { success = false, message = "Invalid token." });
			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}
		}

		// =============================================
		// POST /api/admin/courses/{id}/approve
		// Publishes a PendingReview course.
		// Only PendingReview courses can be approved —
		// Draft/Rejected/Archived cannot be approved directly.
		// =============================================
		[HttpPost("courses/{id:int}/approve")]
		public async Task<IActionResult> ApproveCourse(int id)
		{
			try
			{
				var adminId = GetUserId();

				var course = await _unitOfWork.Courses
					.GetFirstOrDefaultAsync(c => c.Id == id);

				if (course is null || course.IsDeleted)
					return NotFound(new { success = false, message = "Course not found." });

				if (!CourseWorkflowService.CanApprove(course.Status))
					return BadRequest(new
					{
						success = false,
						message = $"Cannot approve a course with status '{course.Status}'. " +
								  $"Only PendingReview courses can be approved."
					});

				course.Status = CourseStatus.Published;
				course.PublishedAt = DateTime.UtcNow;
				course.UpdatedAt = DateTime.UtcNow;
				_unitOfWork.Courses.Update(course);

				var reviewHistory = await _unitOfWork.CourseReviewHistories.GetFirstOrDefaultAsync(c => c.CourseId == course.Id);
				if (reviewHistory is null)
				{
					await _unitOfWork.CourseReviewHistories.AddAsync(new CourseReviewHistory
					{
						CourseId = course.Id,
						Action = CourseReviewAction.Approved,
						Comment = "Your course is approved.",
						PerformedById = adminId,
						PerformedAt = DateTime.UtcNow
					});
				}
				else
				{
					reviewHistory.Action = CourseReviewAction.Approved;
					reviewHistory.Comment = "Your course is approved.";
					reviewHistory.PerformedById = adminId;
					reviewHistory.PerformedAt = DateTime.UtcNow;

					_unitOfWork.CourseReviewHistories.Update(reviewHistory);
				}

				await _unitOfWork.SaveAsync();

				return Ok(new
				{
					success = true,
					message = $"Course '{course.Title}' has been approved and is now published."
				});
			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new { success = false, message = "Invalid token." });
			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}
		}

		// =============================================
		// POST /api/admin/courses/{id}/reject
		// Rejects a PendingReview course with a required comment.
		// Status moves to Rejected — instructor can fix and re-submit.
		// =============================================
		[HttpPost("courses/{id:int}/reject")]
		public async Task<IActionResult> RejectCourse(int id, [FromBody] RejectCourseDTO model)
		{
			try
			{
				var adminId = GetUserId();

				var course = await _unitOfWork.Courses
					.GetFirstOrDefaultAsync(c => c.Id == id);

				if (course is null || course.IsDeleted)
					return NotFound(new { success = false, message = "Course not found." });

				if (!CourseWorkflowService.CanReject(course.Status))
					return BadRequest(new
					{
						success = false,
						message = $"Cannot reject a course with status '{course.Status}'. " +
								  $"Only PendingReview courses can be rejected."
					});

				course.Status = CourseStatus.Rejected;
				course.UpdatedAt = DateTime.UtcNow;
				_unitOfWork.Courses.Update(course);

				var reviewHistory = await _unitOfWork.CourseReviewHistories.GetFirstOrDefaultAsync(c => c.CourseId == course.Id);
				if (reviewHistory is null)
				{
					await _unitOfWork.CourseReviewHistories.AddAsync(new CourseReviewHistory
					{
						CourseId = course.Id,
						Action = CourseReviewAction.Rejected,
						Comment = model.Comment,
						PerformedById = adminId,
						PerformedAt = DateTime.UtcNow
					});
				}
				else
				{
					reviewHistory.Action = CourseReviewAction.Rejected;
					reviewHistory.Comment = model.Comment;
					reviewHistory.PerformedById = adminId;
					reviewHistory.PerformedAt = DateTime.UtcNow;

					_unitOfWork.CourseReviewHistories.Update(reviewHistory);
				}

				await _unitOfWork.SaveAsync();

				return Ok(new
				{
					success = true,
					message = $"Course '{course.Title}' has been rejected.",
					rejectionReason = model.Comment
				});
			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new { success = false, message = "Invalid token." });
			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}
		}

		// =============================================
		// GET /api/admin/courses/{id}/review-history
		// Full audit trail — admins can view history for any course.
		// =============================================
		[HttpGet("courses/{id:int}/review-history")]
		public async Task<IActionResult> GetReviewHistory(int id)
		{
			try
			{
				var course = await _unitOfWork.Courses
					.GetFirstOrDefaultAsync(c => c.Id == id);

				if (course is null || course.IsDeleted)
					return NotFound(new { success = false, message = "Course not found." });

				var history = await _unitOfWork.CourseReviewHistories
					.GetAsync(
						predicate: h => h.CourseId == id,
						selector: h => new CourseReviewHistoryDTO
						{
							Id = h.Id,
							Action = h.Action.ToString(),
							Comment = h.Comment,
							PerformedBy = h.PerformedBy.FullName,
							PerformedAt = h.PerformedAt
						},
						includes: "PerformedBy",
						orderBy: q => q.OrderByDescending(h => h.PerformedAt)
					);

				return Ok(new { success = true, data = history });
			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new { success = false, message = "Invalid token." });
			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}
		}

		// =============================================
		// GET /api/admin/courses
		// Gets all courses, optionally filtered by status (PendingReview, Published, Rejected, Draft, Archived).
		// =============================================
		[HttpGet("courses")]
		public async Task<IActionResult> GetAllCourses(
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 10,
			[FromQuery] CourseStatus? status = null)
		{
			try
			{
				if (page <= 0 || pageSize <= 0)
					return BadRequest(new { success = false, message = "Page and pageSize must be greater than 0." });

				var adminId = GetUserId();

				var courses = await _unitOfWork.Courses
					.GetAsync(
						predicate: c => (status == null || c.Status == status),
						selector: c => new AdminCourseCardDTO
						{
							Id = c.Id,
							Title = c.Title,
							Subtitle = c.Subtitle,
							ThumbnailUrl = c.ThumbnailUrl,
							Category = c.Category.Name,
							InstructorName = c.Instructor.FullName,
							InstructorEmail = c.Instructor.Email ?? "Email Not Provided.",
							Price = c.Price,
							IsFree = c.IsFree,
							Status = c.Status.ToString(),
							CreatedAt = c.CreatedAt,
							UpdatedAt = c.UpdatedAt,
							SubmittedForReviewAt = c.SubmittedForReviewAt ?? DateTime.MinValue,
							PublishedAt = c.PublishedAt ?? DateTime.MinValue
						},
						includes: "Instructor,Category",
						orderBy: q => q.OrderBy(c => c.SubmittedForReviewAt).ThenByDescending(c => c.UpdatedAt)
					);

				if (courses is null || !courses.Any())
					return NotFound(new { success = false, message = "No courses found." });


				var pagedCourses = courses
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.ToList();

				// stats
				var allCourses = await _unitOfWork.Courses.GetAsync(selector: c => new { c.Id, c.Status });
				var statusCounts = new
				{
					PendingReview = allCourses.Count(c => c.Status == CourseStatus.PendingReview),
					Published = allCourses.Count(c => c.Status == CourseStatus.Published),
					Rejected = allCourses.Count(c => c.Status == CourseStatus.Rejected),
					Draft = allCourses.Count(c => c.Status == CourseStatus.Draft),
					Archived = allCourses.Count(c => c.Status == CourseStatus.Archived)
				};

				var totalCount = courses.Count();
				// Header metadata
				Response.Headers.Append("Total-Count", totalCount.ToString());
				Response.Headers.Append("Page-Number", page.ToString());
				Response.Headers.Append("Page-Size", pageSize.ToString());
				Response.Headers.Append("Access-Control-Expose-Headers", "Total-Count, Page-Number, Page-Size");


				return Ok(new { success = true, data = pagedCourses, coursesStatusCounts = statusCounts });
			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new { success = false, message = "Invalid token." });
			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}
		}

		// ============================================= 
		// GET /api/admin/courses/{id}
		// Gets detailed info for any course and its instructor by ID, regardless of status for reviewing the course content.
		// =============================================
		[HttpGet("courses/{courseId:int}")]
		public async Task<IActionResult> GetCourseDetail(int courseId)
		{
			try
			{
				if (courseId < 1)
					return BadRequest(new
					{
						success = false,
						message = $"Validation error for course ID {courseId}."
					});

				var course = await _unitOfWork.Courses.GetFirstOrDefaultAsync(
							predicate: c => c.Id == courseId,
							includes: "Category,Instructor,Sections,CourseTags,Requirements,LearningOutcomes,Sections.Lessons,Sections.Quizzes,Sections.Quizzes.Questions,Sections.Quizzes.Questions.Options");

				if (course == null)
					return NotFound(new
					{
						success = false,
						message = $"The courese With ID {courseId} is not found."
					});

				var courseDetails = _courseMappingService.MapToCourseDetailDTO(course);
				var instructorDetails = new
				{
					Id = course.Instructor.Id,
					FullName = course.Instructor.FullName,
					UserName = course.Instructor.UserName,
					Email = course.Instructor.Email ?? "Email Not Provided.",
					ProfileImageUrl = course.Instructor.ProfileImageUrl ?? "Profile Image Not Provided."
				};
				return Ok(new
				{
					success = true,
					message = $"Course details for ID {courseId} retrieved successfully.",
					data = new
					{
						CourseDetails = courseDetails,
						InstructorDetails = instructorDetails
					}
				});

			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}
		}

		// ============================================
		// GET /api/admin/dashboard/stats
		// Provides key metrics for the admin dashboard, such as:
		// - Total number of courses
		// - Number of courses in each status (PendingReview, Published, Rejected, Draft, Archived)
		// - Total number of enrollments
		// - Total number of users
		// - Total number of instructors
		// - Total number of students
		// - Total number of Payments, revenue and last 30 days revenue and payment stats
		// ============================================
		[HttpGet("dashboard/stats")]
		public async Task<IActionResult> GetDashboardStats()
		{
			try
			{
				// Course stats
				var allCourses = await _unitOfWork.Courses.GetAllAsync(ignoreQueryFilters: true);

				var courseStats = new
				{
					TotalCourses = allCourses.Count(c => !c.IsDeleted),
					PendingReview = allCourses.Count(c => c.Status == CourseStatus.PendingReview && !c.IsDeleted),
					Published = allCourses.Count(c => c.Status == CourseStatus.Published && !c.IsDeleted),
					Rejected = allCourses.Count(c => c.Status == CourseStatus.Rejected && !c.IsDeleted),
					Draft = allCourses.Count(c => c.Status == CourseStatus.Draft && !c.IsDeleted),
					Archived = allCourses.Count(c => c.Status == CourseStatus.Archived && !c.IsDeleted),
					Deleted = allCourses.Count(c => c.IsDeleted)
				};

				// Enrollment stats
				var allEnrollments = await _unitOfWork.Enrollments.GetAllAsync();

				var enrollmentStats = new
				{
					TotalEnrollments = allEnrollments.Count(),
					ActiveEnrollments = allEnrollments.Count(e => e.Status == EnrollmentStatus.Active),
					RefundedEnrollments = allEnrollments.Count(e => e.Status == EnrollmentStatus.Refunded),
					SuspendedEnrollments = allEnrollments.Count(e => e.Status == EnrollmentStatus.Suspended),
					CompletedEnrollments = allEnrollments.Count(e => e.IsCompleted)
				};

				// user stats
				var allUsers = _userManager.Users.Count();

				var userStats = new
				{
					TotalUsers = allUsers,
					TotalSuperAdmins = (await _userManager.GetUsersInRoleAsync("SuperAdmin")).Count,
					TotalAdmins = (await _userManager.GetUsersInRoleAsync("Admin")).Count,
					TotalInstructors = (await _userManager.GetUsersInRoleAsync("Instructor")).Count,
					TotalStudents = (await _userManager.GetUsersInRoleAsync("Student")).Count
				};


				// payment stats
				var allPayments = await _unitOfWork.Payments.GetAllAsync();
				var recentPayments = allPayments
									.Where(p => DateTime.UtcNow - p.CreatedAt <= TimeSpan.FromDays(30))
									.ToList();

				var paymentStats = new
				{
					TotalPayments = allPayments.Count(),
					TotalRevenue = allPayments.Where(p => p.Status == PaymentStatus.Succeeded).Sum(p => p.Amount),
					SuccessfulPayments = allPayments.Count(p => p.Status == PaymentStatus.Succeeded),
					FailedPayments = allPayments.Count(p => p.Status == PaymentStatus.Failed),
					PendingPayments = allPayments.Count(p => p.Status == PaymentStatus.Pending),
					RefundedPayments = allPayments.Count(p => p.Status == PaymentStatus.Refunded),
					Last30Days = new
					{
						TotalPayments = recentPayments.Count(),
						TotalRevenue = recentPayments.Where(p => p.Status == PaymentStatus.Succeeded).Sum(p => p.Amount),
						SuccessfulPayments = recentPayments.Count(p => p.Status == PaymentStatus.Succeeded),
						FailedPayments = recentPayments.Count(p => p.Status == PaymentStatus.Failed),
						PendingPayments = recentPayments.Count(p => p.Status == PaymentStatus.Pending),
						RefundedPayments = recentPayments.Count(p => p.Status == PaymentStatus.Refunded)
					}
				};

				return Ok(new
				{
					success = true,
					data = new
					{
						CourseStats = courseStats,
						UserStats = userStats,
						EnrollmentStats = enrollmentStats,
						PaymentStats = paymentStats
					}
				});

			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new { success = false, message = "Invalid token." });
			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}
		}

		// ============================================
		// GET /api/admin/users
		// Returns all users with their roles.
		// Optionally filter by role (e.g., only Instructors) or search by (e.g., username, email, or full name).
		// ============================================
		[HttpGet("users")]
		public async Task<IActionResult> GetAllUsers(
			[FromQuery] string? role = null,
			[FromQuery] string? searchTerm = null,
			[FromQuery] int page = 1,
			[FromQuery] int pageSize = 10)
		{
			try
			{
				if (page < 1 || pageSize < 1)
					return BadRequest(new
					{
						success = false,
						message = "Page and pageSize must be greater than 0."
					});

				var validRoles = new[] { "SuperAdmin", "Admin", "Instructor", "Student" };
				if (role is not null && !validRoles.Contains(role))
					return BadRequest(new
					{
						success = false,
						message = $"Invalid role '{role}'. Valid roles are: {string.Join(", ", validRoles)}."
					});

				var users = (role is not null)
					? await _userManager.GetUsersInRoleAsync(role)
					: await _userManager.Users.ToListAsync();

				if (!string.IsNullOrEmpty(searchTerm))
				{
					searchTerm = searchTerm.Trim().ToUpper();
					users = users.Where(u =>
						u.NormalizedUserName.Contains(searchTerm) ||
						u.NormalizedEmail.Contains(searchTerm) ||
						u.FullName.ToUpper().Contains(searchTerm))
						.ToList();
				}

				if (!users.Any())
					return NotFound(new
					{
						success = false,
						message = "No users found."
					});

				var totalCount = users.Count();

				var pagedUsers = users
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.ToList();

				var PagedUserdDTOs = new List<AdminUserDTO>();
				foreach (var user in pagedUsers)
				{
					var roles = await _userManager.GetRolesAsync(user);
					PagedUserdDTOs.Add(new AdminUserDTO
					{
						Id = user.Id,
						FullName = user.FullName,
						UserName = user.UserName,
						Email = user.Email,
						Role = roles.FirstOrDefault() ?? "No Role",
						IsLocked = await _userManager.IsLockedOutAsync(user),
						CreatedAt = user.CreatedAt,
						LastLoginAt = user.LastLoginAt
					});
				}

				// Header metadata
				Response.Headers.Append("Total-Count", totalCount.ToString());
				Response.Headers.Append("Page-Number", page.ToString());
				Response.Headers.Append("Page-Size", pageSize.ToString());
				Response.Headers.Append("Access-Control-Expose-Headers", "Total-Count, Page-Number, Page-Size");

				return Ok(new { success = true, data = PagedUserdDTOs });
			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new { success = false, message = "Invalid token." });
			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}
		}

		// ============================================
		// GET /api/admin/users/{id}
		// Returns detailed info for a specific user by ID, including their role and lockout status.
		// ============================================
		[HttpGet("users/{id}")]
		public async Task<IActionResult> GetUserById(string id)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(id);
				if (user is null)
					return NotFound(new { success = false, message = "User not found." });

				var roles = await _userManager.GetRolesAsync(user);

				var userRole = roles.FirstOrDefault() ?? "No Role";

				var allEnrollments = await _unitOfWork.Enrollments
						.GetAllAsync(e => e.StudentId == user.Id && e.Status == EnrollmentStatus.Active);

				var userDTO = new AdminUserDetailDTO
				{
					Id = user.Id,
					FullName = user.FullName,
					UserName = user.UserName,
					Email = user.Email,
					Role = userRole,
					Bio = user.Bio,
					ProfileImageUrl = user.ProfileImageUrl,
					IsLocked = await _userManager.IsLockedOutAsync(user),
					CreatedAt = user.CreatedAt,
					LastLoginAt = user.LastLoginAt,
					EnrollmentCount = allEnrollments is not null ? allEnrollments.Count() : 0
				};

				if (userRole != "Student")
				{
					var allCourses = await _unitOfWork.Courses
						.GetAllAsync(c => c.InstructorId == user.Id);
					userDTO.CourseCount = allCourses is not null ? allCourses.Count() : 0;
				}

				return Ok(new { success = true, data = userDTO });
			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new { success = false, message = "Invalid token." });
			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}
		}

		// ============================================
		// PATCH /api/admin/users/{id}/status
		// Allows admins to ban or unban a user by ID, with an optional reason for the action.
		// ============================================
		[HttpPatch("users/{id}/status")]
		public async Task<IActionResult> UpdateUserStatus(string id, [FromBody] UpdateUserStatusDTO model)
		{
			try
			{
				var adminId = GetUserId();

				if (id == adminId)
					return BadRequest(new
					{
						success = false,
						message = "You can not change your own status."
					});
				var user = await _userManager.FindByIdAsync(id);

				if (user is null)
					return NotFound(new
					{
						success = false,
						message = "User not Found"
					});
				var userRoles = await _userManager.GetRolesAsync(user);
				if (userRoles.Contains("SuperAdmin"))
					return BadRequest(new
					{
						success = false,
						message = "SuperAdmin can not be banned"
					});

				if (model.IsBanned)
				{
					await _userManager.SetLockoutEnabledAsync(user, true);
					await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));

					return Ok(new
					{
						success = true,
						message = $"User {user.UserName} has been banned successfully.",
						data = new
						{
							userId = user.Id,
							isBanned = true,
							reason = model.Reason ?? "No reason Provided",
							bannedAt = DateTime.UtcNow
						}
					});
				}
				else
				{
					await _userManager.SetLockoutEndDateAsync(user, null);
					await _userManager.ResetAccessFailedCountAsync(user);

					return Ok(new
					{
						success = true,
						message = $"User {user.UserName} has been unbanned successfully.",
						data = new
						{
							userId = user.Id,
							isBanned = false,
							reason = model.Reason ?? "No reason Provided",
							unbannedAt = DateTime.UtcNow
						}
					});
				}


			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new { success = false, message = "Invalid token." });
			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}

		}

		// ============================================
		// PATCH /api/admin/users/{id}/role
		// Allow admin to change user's role
		// SuperAdmin role can not be changed
		// ============================================
		[HttpPatch("users/{id}/role")]
		public async Task<IActionResult> UpdateUserRole(string id, [FromBody] UpdateUserRoleDTO model)
		{
			try
			{
				var adminId = GetUserId();

				if (id == adminId)
					return BadRequest(new
					{
						success = false,
						message = "You can not Change your own role."
					});

				var validRoles = new[] { "Student", "Instructor", "Admin" };
				if (!validRoles.Contains(model.NewRole))
					return BadRequest(new
					{
						success = false,
						message = $"Invalid role. Valid roles are: {string.Join(", ", validRoles)}"
					});

				var user = await _userManager.FindByIdAsync(id);
				if (user is null)
					return NotFound(new { success = false, message = "User not found." });

				var userRoles = await _userManager.GetRolesAsync(user);
				if (userRoles.Contains("SuperAdmin"))
					return BadRequest(new
					{
						success = false,
						message = "You can not modify SuperAdmin role"
					});

				if (userRoles.Contains(model.NewRole))
					return BadRequest(new
					{
						success = false,
						message = "User already has this role."
					});

				var adminUser = await _userManager.FindByIdAsync(adminId);
				var adminRoles = await _userManager.GetRolesAsync(adminUser);
				if ((model.NewRole == "Admin" || userRoles.Contains("Admin")) && !adminRoles.Contains("SuperAdmin"))
					return BadRequest(new
					{
						success = false,
						message = "Only SuperAdmin Can change Admin role."
					});

				await using var transaction = await _unitOfWork.BeginTransactionAsync();
				try
				{
					var removeResult = await _userManager.RemoveFromRolesAsync(user, userRoles);
					if (!removeResult.Succeeded)
					{
						await transaction.RollbackAsync();
						return StatusCode(500, new
						{
							success = false,
							message = string.Join(", ", removeResult.Errors.Select(e => e.Description))
						});
					}
					var addRoleResult = await _userManager.AddToRoleAsync(user, model.NewRole);
					if (!addRoleResult.Succeeded)
					{
						await transaction.RollbackAsync();
						return StatusCode(500, new
						{
							success = false,
							message = string.Join(", ", addRoleResult.Errors.Select(e => e.Description))
						});
					}

					await transaction.CommitAsync();
				}
				catch
				{
					await transaction.RollbackAsync();
					throw;
				}

				return Ok(new
				{
					success = true,
					message = $"User '{user.UserName}' role has been updated successfully.",
					data = new
					{
						userId = user.Id,
						userName = user.UserName,
						previousRoles = userRoles,
						newRole = model.NewRole,
						changedBy = adminId,
						changedAt = DateTime.UtcNow
					}
				});


			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new { success = false, message = "Invalid token." });
			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}
		}

		// ============================================
		// GET /api/admin/instructor-applications
		// Returns all pending instructor applications for admin review.
		// ============================================
		[HttpGet("instructor-applications")]
		public async Task<IActionResult> GetInstructorApplications([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
		{
			try
			{
				if (page < 1 || pageSize < 1)
					return BadRequest(new
					{
						success = false,
						message = "Page and pageSize must be greater than 0."
					});

				var pagedApplications = await _userManager.Users
					.Where(s => s.IsInstructorRequested)
					.OrderBy(s => s.InstructorRequestedAt)
					.Skip((page - 1) * pageSize)
					.Take(pageSize)
					.Select(s => new InstructorApplicationDTO
					{
						UserId = s.Id,
						UserName = s.UserName ?? string.Empty,
						FullName = s.FullName ?? string.Empty,
						Email = s.Email ?? string.Empty,
						Bio = s.Bio ?? string.Empty,
						ProfileImageUrl = s.ProfileImageUrl ?? string.Empty,
						RequestedAt = s.InstructorRequestedAt ?? DateTime.MinValue
					})
					.ToListAsync();

				if (!pagedApplications.Any())
					return NotFound(new
					{
						success = false,
						message = "No pending instructor applications found."
					});

				var totalCount = await _userManager.Users
					.Where(s => s.IsInstructorRequested)
					.CountAsync();

				// Header metadata
				Response.Headers.Append("Total-Count", totalCount.ToString());
				Response.Headers.Append("Page-Number", page.ToString());
				Response.Headers.Append("Page-Size", pageSize.ToString());
				Response.Headers.Append("Access-Control-Expose-Headers", "Total-Count, Page-Number, Page-Size");

				return Ok(new { success = true, data = pagedApplications });
			}
			catch (UnauthorizedAccessException)
			{
				return Unauthorized(new { success = false, message = "Invalid token." });
			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}
		}

		// =============================================
		// PATCH /api/admin/{id}/instructor-application/reject
		// Rejects an instructor application by user ID.
		// =============================================
		[HttpPatch("users/{id}/instructor-applications/reject")]
		public async Task<IActionResult> RejectInstructorApplication(string id)
		{
			try
			{
				var user = await _userManager.FindByIdAsync(id);

				if (user is null)
					return NotFound(new { success = false, message = "User not found." });

				if (!user.IsInstructorRequested)
					return BadRequest(new { success = false, message = "This user has not applied to be an instructor." });

				user.IsInstructorRequested = false;
				user.InstructorRequestedAt = null;
				await _userManager.UpdateAsync(user);

				return Ok(new
				{
					success = true,
					message = $"Instructor application for user '{user.UserName}' has been rejected.",
					data = new
					{
						UserId = user.Id,
						UserName = user.UserName,
						Email = user.Email,
						RejectedAt = DateTime.UtcNow
					}
				});
			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}
		}

		// ============================================
		// POST /api/admin/categories
		// Allows admins to create a new category.
		// ============================================
		[HttpPost("categories")]
		public async Task<IActionResult> CreateCategory([FromBody] CreateUpdateCategoryDTO model)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(model.Name))
					return BadRequest(new
					{
						success = false,
						message = "Category name is required."
					});

				var existingCategory = await _unitOfWork.Categories
					.GetFirstOrDefaultAsync(c => c.Name.ToLower() == model.Name.ToLower());

				if (existingCategory != null)
					return Conflict(new
					{
						success = false,
						message = "A category with this name already exists."
					});

				var newCategory = new Category
				{
					Name = model.Name,
					Description = model.Description
				};

				await _unitOfWork.Categories.AddAsync(newCategory);
				await _unitOfWork.SaveAsync();

				return Ok(new
				{
					success = true,
					message = "Category created successfully.",
					data = newCategory
				});
			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}

		}

		// ============================================
		// PATCH /api/admin/categories/{id}
		// Allows admins to update an existing category by ID.
		// ============================================
		[HttpPatch("categories/{id:int}")]
		public async Task<IActionResult> UpdateCategory(int id, [FromBody] CreateUpdateCategoryDTO model)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(model.Name) && string.IsNullOrEmpty(model.Description))
					return BadRequest(new
					{
						success = false,
						message = "At least one field (Name or Description) must be provided for update."
					});

				var category = await _unitOfWork.Categories.GetFirstOrDefaultAsync(c => c.Id == id);

				if (category == null)
					return NotFound(new { success = false, message = "Category not found." });


				if (category.Name == model.Name && category.Description == model.Description)
					return BadRequest(new { success = false, message = "No changes detected." });

				if (!string.IsNullOrWhiteSpace(model.Name))
				{
					var existingCategory = await _unitOfWork.Categories
						.GetFirstOrDefaultAsync(c => c.Name.ToLower() == model.Name.ToLower() && c.Id != id);

					if (existingCategory != null)
						return Conflict(new
						{
							success = false,
							message = "A category with this name already exists."
						});
					category.Name = model.Name;
				}
				if (model.Description is not null)
					category.Description = model.Description;
				
				_unitOfWork.Categories.Update(category);
				await _unitOfWork.SaveAsync();

				return Ok(new
				{
					success = true,
					message = "Category updated successfully.",
					data = category
				});
			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}

		}

		// ============================================
		// POST /api/admin/tags
		// Allows admins to create a new tag.
		// ============================================
		[HttpPost("tags")]
		public async Task<IActionResult> CreateTag([FromBody] CreateUpdateTagDTO model)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(model.Name))
					return BadRequest(new
					{
						success = false,
						message = "Tag name is required."
					});

				var existingTag = await _unitOfWork.Tags
					.GetFirstOrDefaultAsync(t => t.Name.ToLower() == model.Name.ToLower());

				if (existingTag != null)
					return Conflict(new
					{
						success = false,
						message = "A tag with this name already exists."
					});

				var newTag = new Tag { Name = model.Name };

				await _unitOfWork.Tags.AddAsync(newTag);
				await _unitOfWork.SaveAsync();

				return Ok(new
				{
					success = true,
					message = "Tag created successfully.",
					data = newTag
				});
			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}
		}

		// ============================================
		// PATCH /api/admin/tags/{id}
		// Allows admins to update an existing tag by ID.
		// ============================================
		[HttpPatch("tags/{id:int}")]
		public async Task<IActionResult> UpdateTag(int id, [FromBody] CreateUpdateTagDTO model)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(model.Name))
					return BadRequest(new
					{
						success = false,
						message = "Tag name is required."
					});
				
				var tag = await _unitOfWork.Tags.GetFirstOrDefaultAsync(t => t.Id == id);
				if (tag == null)
					return NotFound(new { success = false, message = "Tag not found." });
				
				if (tag.Name == model.Name)
					return BadRequest(new { success = false, message = "No changes detected." });
				
				var existingTag = await _unitOfWork.Tags
					.GetFirstOrDefaultAsync(t => t.Name.ToLower() == model.Name.ToLower() && t.Id != id);
				
				if (existingTag != null)
					return Conflict(new
					{
						success = false,
						message = "A tag with this name already exists."
					});
				
				tag.Name = model.Name;
				_unitOfWork.Tags.Update(tag);
				await _unitOfWork.SaveAsync();
				
				return Ok(new
				{
					success = true,
					message = "Tag updated successfully.",
					data = tag
				});
			}
			catch (Exception e)
			{
				return StatusCode(500, new { success = false, message = e.Message });
			}
		}


	}
}
