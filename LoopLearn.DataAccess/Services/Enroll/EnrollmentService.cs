using LoopLearn.Entities.Enums;
using LoopLearn.Entities.Interfaces;
using LoopLearn.Entities.Models;

namespace LoopLearn.DataAccess.Services.Enroll
{
    public class EnrollmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EnrollmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// Creates an enrollment for a student.
        /// </summary>
        /// <param name="studentId">The student's identity ID.</param>
        /// <param name="courseId">The course to enroll in.</param>
        /// <param name="paymentId">
        ///     The linked payment record ID. Null for free courses.
        ///     Passing this links the enrollment to the payment for audit and refund purposes.
        /// </param>
        /// <returns>
        ///     The created <see cref="Enrollment"/>, or null if the student was already enrolled.
        /// </returns>
        public async Task<Enrollment?> EnrollStudentAsync(string studentId, int courseId, int? paymentId = null)
        {
            var existing = await _unitOfWork.Enrollments
                          .GetFirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == courseId);

            // Active enrollment — genuinely already enrolled
            if (existing is not null && existing.Status == EnrollmentStatus.Active)
                return null;

            if (existing is not null && existing.Status == EnrollmentStatus.Refunded)
            {
                // Re-enrollment after refund — reset the existing record
                // rather than creating a duplicate row
                existing.Status = EnrollmentStatus.Active;
                existing.PaymentId = paymentId;
                existing.ProgressPercentage = 0;
                existing.IsCompleted = false;
                existing.EnrolledAt = DateTime.UtcNow;
                existing.LastAccessAt = DateTime.UtcNow;
                existing.CompletedAt = null;
                _unitOfWork.Enrollments.Update(existing);
                return existing;
            }

            var enrollment = new Enrollment
            {
                StudentId = studentId,
                CourseId = courseId,
                PaymentId = paymentId,
                Status = EnrollmentStatus.Active,
                ProgressPercentage = 0,
                IsCompleted = false,
                EnrolledAt = DateTime.UtcNow,
                LastAccessAt = DateTime.UtcNow
            };

            await _unitOfWork.Enrollments.AddAsync(enrollment);

            // Future side-effects go here — they automatically apply to
            // both free and paid flows:
            // await _emailService.SendWelcomeEmailAsync(studentId, courseId);
            // await _analyticsService.TrackEnrollmentAsync(studentId, courseId);

            return enrollment;
        }
    }
}

