# LoopLearn Platform - Project Documentation

> **Last Updated:** May 25, 2026  
> **Project Status:** Mid-Stage Development (MVP Features Complete)  
> **Branch:** Profile  
> **Repository:** https://github.com/Loop-Learn/LoopLearnv2.0.Backend

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Technology Stack](#technology-stack)
3. [Architecture Design](#architecture-design)
4. [Database Schema](#database-schema)
5. [API Endpoints](#api-endpoints)
6. [Authentication & Authorization](#authentication--authorization)
7. [Business Logic Rules](#business-logic-rules)
8. [Implementation Status](#implementation-status)
9. [Known Issues & Technical Debt](#known-issues--technical-debt)
10. [Next Steps & Priorities](#next-steps--priorities)

---

## Project Overview

### Purpose
LoopLearn is an **ASP.NET Core 8 Web API** designed as a comprehensive online learning platform similar to Udemy/Coursera. It enables:

- **Instructors** to create, publish, and manage courses with structured content (sections, lessons, quizzes)
- **Students** to enroll, track progress, take assessments, and provide feedback
- **Admins** to manage courses, instructors, and platform moderation
- **Comprehensive course discovery** through search, filtering, and categorization

### Core Features
- ✅ User authentication & role-based access control (JWT)
- ✅ Course CRUD operations with nested content management
- ✅ Student enrollment and progress tracking
- ✅ Quiz system with grading
- ✅ Course reviews and ratings
- ✅ Advanced search with relevance scoring
- ✅ Database seeding with initial data
- ⏳ Payment processing (pending)
- ⏳ Email notifications (pending)
- ⏳ Certificate generation (pending)

### Target Users
- **Students:** Learners wanting to upskill
- **Instructors:** Subject matter experts creating courses
- **Admins:** Platform moderators and content reviewers
- **SuperAdmins:** System administrators with full control

---

## Technology Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| **Framework** | ASP.NET Core | 8.0 |
| **Database** | SQL Server | Latest |
| **ORM** | Entity Framework Core | 8.0 |
| **Authentication** | JWT (HS256) | - |
| **Authorization** | ASP.NET Core Identity | 8.0 |
| **API Documentation** | Swagger/OpenAPI | - |
| **Serialization** | System.Text.Json | Built-in |
| **Logging** | ILogger (built-in) | - |
| **Dependency Injection** | Built-in Container | - |
| **CORS** | ASP.NET Core CORS | - |

### Key Libraries
```csharp
Microsoft.EntityFrameworkCore.SqlServer
Microsoft.AspNetCore.Identity
Microsoft.AspNetCore.Authentication.JwtBearer
Microsoft.IdentityModel.Tokens
System.IdentityModel.Tokens.Jwt
Swashbuckle.AspNetCore (Swagger)
```

---

## Architecture Design

### Architectural Pattern: **Layered N-Tier Architecture**

```
┌─────────────────────────────────────┐
│  LoopLearn.API                      │  Presentation Layer
│  (Controllers, Middleware)          │  - HTTP endpoints
│  - CourseController                 │  - Request/Response handling
│  - AuthController                   │  - Validation
│  - [Other Controllers]              │
└────────────────┬────────────────────┘
                 │ Depends On
┌────────────────▼────────────────────┐
│  LoopLearn.DataAccess               │  Data Access & Services Layer
│  (Repositories, Services, DbContext)│  - Repository implementations
│  - CourseRepository                 │  - UnitOfWork pattern
│  - GenericRepository<T>             │  - AuthService
│  - UnitOfWork                       │  - Database migrations
│  - ApplicationDbContext             │
│  - Database seeding                 │
└────────────────┬────────────────────┘
                 │ Depends On
┌────────────────▼────────────────────┐
│  LoopLearn.Entities                 │  Domain/Entity Layer
│  (Models, DTOs, Interfaces)         │  - Entity models
│  - Models (Course, User, etc.)      │  - Data transfer objects
│  - Enums & Constants                │  - Repository interfaces
│  - Custom validation attributes     │  - Exception types
│  - Domain logic                     │
└────────────────┬────────────────────┘
                 │ Accesses
┌────────────────▼────────────────────┐
│  SQL Server Database                │  Persistence Layer
│  - Relational data storage          │
│  - Identity management              │
│  - Audit trails                     │
└─────────────────────────────────────┘
```

### Design Patterns Implemented

1. **Repository Pattern**
   - `IGenericRepository<T>` for common CRUD
   - Specific repositories: `ICourseRepository`, `ITagRepository`, etc.
   - Abstracts data access logic

2. **Unit of Work Pattern**
   - `IUnitOfWork` interface
   - Centralized transaction management
   - Coordinates multiple repositories

3. **Dependency Injection**
   - Constructor injection throughout
   - Scoped lifetime for DbContext
   - Service registration in `Program.cs`

4. **DTO Pattern**
   - Decouples API contracts from models
   - `CourseCreationDTO`, `UpdateCourseDTO`, `CourseDetailDTO`, etc.
   - Request/response validation

5. **Async/Await**
   - Non-blocking I/O operations
   - Better scalability

6. **Custom Validation Attributes**
   - `[CourseTitle]`, `[Username]`, `[Email]`
   - Domain-specific validation rules

---

## Database Schema

### Entity-Relationship Diagram (Simplified)

```
ApplicationUser (PK: Id)
├── (1:N) Courses (FK: InstructorId)
├── (1:N) Enrollments (FK: StudentId)
├── (1:N) Feedbacks (FK: StudentId)
├── (1:N) LessonComments (FK: StudentId)
└── (1:N) StudentLessonProgress (FK: StudentId)

Course (PK: Id)
├── (N:1) Category (FK: CategoryId)
├── (N:1) ApplicationUser/Instructor (FK: InstructorId)
├── (1:N) Sections (FK: CourseId)
├── (1:N) Quizzes (FK: CourseId)
├── (1:N) Enrollments (FK: CourseId)
├── (1:N) Feedbacks (FK: CourseId)
├── (1:N) CourseTags (FK: CourseId)
├── (1:N) CourseRequirements (FK: CourseId)
├── (1:N) CourseLearningOutcomes (FK: CourseId)
└── (1:N) CourseTargetAudiences (FK: CourseId)

Section (PK: Id)
├── (N:1) Course (FK: CourseId)
├── (1:N) Lessons (FK: SectionId)
└── (1:N) Quizzes (FK: SectionId)

Lesson (PK: Id)
├── (N:1) Section (FK: SectionId)
├── (1:N) StudentLessonProgress (FK: LessonId)
└── (1:N) LessonComments (FK: LessonId)

Quiz (PK: Id)
├── (N:1) Section (FK: SectionId, optional)
├── (N:1) Course (FK: CourseId, optional)
├── (1:N) Questions (FK: QuizId)
└── (1:N) QuizAttempts (FK: QuizId)

Question (PK: Id)
├── (N:1) Quiz (FK: QuizId)
└── (1:N) Options (FK: QuestionId)

Option (PK: Id)
└── (N:1) Question (FK: QuestionId)

QuizAttempt (PK: Id)
├── (N:1) ApplicationUser/Student (FK: StudentId)
├── (N:1) Quiz (FK: QuizId)
└── (1:N) StudentAnswers (FK: QuizAttemptId)

StudentAnswer (PK: Id)
├── (N:1) QuizAttempt (FK: QuizAttemptId)
├── (N:1) Question (FK: QuestionId)
└── (N:1) Option (FK: SelectedOptionId, optional)

Enrollment (PK: StudentId + CourseId)
├── (N:1) ApplicationUser/Student (FK: StudentId)
└── (N:1) Course (FK: CourseId)

StudentLessonProgress (PK: StudentId + LessonId)
├── (N:1) ApplicationUser/Student (FK: StudentId)
└── (N:1) Lesson (FK: LessonId)

Feedback (PK: Id)
├── (N:1) ApplicationUser/Student (FK: StudentId)
└── (N:1) Course (FK: CourseId)

LessonComment (PK: Id)
├── (N:1) ApplicationUser/Student (FK: StudentId)
└── (N:1) Lesson (FK: LessonId)

Category (PK: Id)
└── (1:N) Courses (FK: CategoryId)

Tag (PK: Id)
└── (1:N) CourseTags (FK: TagId)

CourseTag (PK: CourseId + TagId)
├── (N:1) Course (FK: CourseId)
└── (N:1) Tag (FK: TagId)

CourseRequirement (PK: Id)
└── (N:1) Course (FK: CourseId)

CourseLearningOutcome (PK: Id)
└── (N:1) Course (FK: CourseId)

CourseTargetAudience (PK: Id)
└── (N:1) Course (FK: CourseId)
```

### Core Entities Details

#### **ApplicationUser** (extends IdentityUser)
```csharp
Properties:
- Id (PK, string)
- FirstName, LastName
- FullName (computed from First + Last)
- Email, UserName (from IdentityUser)
- PhoneNumber (from IdentityUser)
- PasswordHash (from IdentityUser)
- Bio, BirthDate, Gender
- ProfileImageUrl
- CreatedAt (DateTime)
- LastLoginAt (DateTime, nullable)

Navigation Properties:
- Courses (ICollection) - Courses created by instructor
- Enrollments (ICollection) - Courses student is enrolled in
- Feedbacks (ICollection) - Reviews left by student
- LessonComments (ICollection) - Comments on lessons
- LessonProgresses (ICollection) - Progress tracking
```

#### **Course**
```csharp
Properties:
- Id (PK, int)
- Title (required, string)
- Subtitle (string)
- Description (required, string, max 5000)
- ThumbnailUrl (URL string)
- Price (decimal, 0-10000)
- IsFree (bool)
- Level (enum: Beginner, Intermediate, Advanced)
- Language (string)
- Status (enum: Draft, Pending, Published, Archived, Rejected)
- CreatedAt, UpdatedAt (DateTime)
- InstructorId (FK, string)
- CategoryId (FK, int)

Navigation Properties:
- Instructor (ApplicationUser)
- Category (Category)
- Sections (ICollection)
- Quizzes (ICollection)
- Enrollments (ICollection)
- Feedbacks (ICollection)
- Requirements (ICollection)
- LearningOutcomes (ICollection)
- TargetAudiences (ICollection)
- CourseTags (ICollection)
```

#### **Section**
```csharp
Properties:
- Id (PK, int)
- Title (required, string)
- Order (int) - sequence in course
- CourseId (FK, int)

Navigation Properties:
- Course (Course)
- Lessons (ICollection)
- Quizzes (ICollection)
```

#### **Lesson**
```csharp
Properties:
- Id (PK, int)
- Title (required, string)
- Description (string)
- VideoUrl (string)
- Order (int) - sequence in section
- Duration (TimeSpan)
- IsPreview (bool)
- SectionId (FK, int)

Navigation Properties:
- Section (Section)
- LessonProgresses (ICollection)
- LessonComments (ICollection)
```

#### **Quiz**
```csharp
Properties:
- Id (PK, int)
- Title (required, string)
- Description (string)
- Type (enum: MultipleChoice, TrueFalse, ShortAnswer, Essay)
- PassingScore (int, 0-100)
- IsRequired (bool)
- SectionId (FK, int, nullable)
- CourseId (FK, int, nullable)

Navigation Properties:
- Section (Section)
- Course (Course)
- Questions (ICollection)
- QuizAttempts (ICollection)
```

#### **Question**
```csharp
Properties:
- Id (PK, int)
- Title (required, string)
- QuestionText (required, string)
- QuizId (FK, int)
- PointsValue (int)

Navigation Properties:
- Quiz (Quiz)
- Options (ICollection)
```

#### **Option**
```csharp
Properties:
- Id (PK, int)
- OptionText (required, string)
- IsCorrect (bool)
- Explanation (string)
- QuestionId (FK, int)

Navigation Properties:
- Question (Question)
```

#### **Enrollment** (Composite Key: StudentId + CourseId)
```csharp
Properties:
- StudentId (PK, FK, string)
- CourseId (PK, FK, int)
- ProgressPercentage (double, 0-100)
- IsCompleted (bool)
- EnrolledAt (DateTime)
- CompletedAt (DateTime, nullable)
- LastAccessAt (DateTime, nullable)

Navigation Properties:
- Student (ApplicationUser)
- Course (Course)
```

#### **StudentLessonProgress** (Composite Key: StudentId + LessonId)
```csharp
Properties:
- StudentId (PK, FK, string)
- LessonId (PK, FK, int)
- IsCompleted (bool)
- WatchedPercentage (double, 0-100)
- LastSecondWatched (int)
- CompletedAt (DateTime, nullable)

Navigation Properties:
- Student (ApplicationUser)
- Lesson (Lesson)
```

#### **Feedback**
```csharp
Properties:
- Id (PK, int)
- StudentId (FK, string)
- CourseId (FK, int)
- Rating (int, 1-5)
- Comment (string)
- CreatedAt (DateTime)
- UpdatedAt (DateTime, nullable)

Navigation Properties:
- Student (ApplicationUser)
- Course (Course)
```

#### **QuizAttempt**
```csharp
Properties:
- Id (PK, int)
- StudentId (FK, string)
- QuizId (FK, int)
- Score (int)
- IsPassed (bool)
- StartedAt (DateTime)
- SubmittedAt (DateTime)

Navigation Properties:
- Student (ApplicationUser)
- Quiz (Quiz)
- Answers (ICollection<StudentAnswer>)
```

#### **StudentAnswer**
```csharp
Properties:
- Id (PK, int)
- QuizAttemptId (FK, int)
- QuestionId (FK, int)
- SelectedOptionId (FK, int, nullable)
- AnswerText (string, for essay/short answer)

Navigation Properties:
- QuizAttempt (QuizAttempt)
- Question (Question)
- SelectedOption (Option)
```

#### **Category**
```csharp
Properties:
- Id (PK, int)
- Name (required, unique, string)
- Description (string)

Navigation Properties:
- Courses (ICollection)
```

#### **Tag**
```csharp
Properties:
- Id (PK, int)
- Name (required, unique, string)

Navigation Properties:
- CourseTags (ICollection)
```

#### **CourseTag** (Composite Key: CourseId + TagId)
```csharp
Properties:
- CourseId (PK, FK, int)
- TagId (PK, FK, int)

Navigation Properties:
- Course (Course)
- Tag (Tag)
```

#### **CourseRequirement**
```csharp
Properties:
- Id (PK, int)
- Description (required, string)
- CourseId (FK, int)

Navigation Properties:
- Course (Course)
```

#### **CourseLearningOutcome**
```csharp
Properties:
- Id (PK, int)
- Description (required, string)
- CourseId (FK, int)

Navigation Properties:
- Course (Course)
```

#### **CourseTargetAudience**
```csharp
Properties:
- Id (PK, int)
- Description (required, string)
- CourseId (FK, int)

Navigation Properties:
- Course (Course)
```

#### **LessonComment**
```csharp
Properties:
- Id (PK, int)
- StudentId (FK, string)
- LessonId (FK, int)
- Comment (required, string)
- CreatedAt (DateTime)
- UpdatedAt (DateTime, nullable)

Navigation Properties:
- Student (ApplicationUser)
- Lesson (Lesson)
```

---

## API Endpoints

### Base URL
```
https://api.looplearn.com/api
```

### Response Format (Standard)
```json
{
  "success": true,
  "message": "Operation successful",
  "data": { /* entity data */ },
  "errors": [ /* validation errors */ ]
}
```

### Authentication
All endpoints requiring authentication use JWT Bearer token:
```
Authorization: Bearer <jwt_token>
```

---

### **AuthController** - `/auth`

#### Login
```
POST /auth/login
Content-Type: application/json
Authentication: None (Public)

Request:
{
  "emailOrUsername": "user@example.com",
  "password": "SecurePass123"
}

Response (200 OK):
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresOn": "2026-06-25T10:00:00Z",
  "isAuthenticated": true,
  "message": "Login successful"
}

Error Cases:
- 400 Bad Request: Invalid email/username format
- 401 Unauthorized: Invalid credentials or account locked
- 500 Internal Server Error: Unexpected error
```

#### Register
```
POST /auth/register
Content-Type: application/json
Authentication: None (Public)

Request:
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "userName": "johndoe",
  "password": "SecurePass123",
  "confirmPassword": "SecurePass123",
  "birthDate": "1990-01-15",
  "gender": 0 (0=Male, 1=Female, 2=Other)
}

Response (200 OK):
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresOn": "2026-06-25T10:00:00Z",
  "isAuthenticated": true,
  "message": "Registration successful"
}

Error Cases:
- 400 Bad Request: Validation error or email already exists
- 500 Internal Server Error: Unexpected error
```

---

### **CourseController** - `/course`

#### Get All Courses (Paginated)
```
GET /course/all?page=1&pageSize=10
Authentication: None (Public)

Query Parameters:
- page (int): Page number, default 1
- pageSize (int): Items per page, default 10

Response (200 OK):
{
  "success": true,
  "message": "Successfully retrieved 10 courses",
  "data": [
    {
      "id": 1,
      "title": "JavaScript Basics",
      "subtitle": "Learn JS from scratch",
      "thumbnailUrl": "https://...",
      "instructorName": "Jane Smith",
      "averageRating": 4.5,
      "totalRatings": 120,
      "price": 49.99,
      "isFree": false,
      "level": "Beginner"
    },
    ...
  ]
}

Headers:
- Total-Count: 150
- Page: 1
- PageSize: 10
- Access-Control-Expose-Headers: Total-Count, Page, PageSize

Error Cases:
- 400 Bad Request: Invalid page or pageSize
- 204 No Content: No courses found
- 500 Internal Server Error
```

#### Get Courses by Categories
```
GET /course/categories?categories=Programming&categories=WebDevelopment&page=1&pageSize=10
Authentication: None (Public)

Query Parameters:
- categories[] (array of string): Category names to filter by
- page (int): Page number, default 1
- pageSize (int): Items per page, default 10

Response (200 OK): Same as "Get All Courses"

Error Cases:
- 400 Bad Request: No categories provided
- 404 Not Found: No courses for provided categories
- 500 Internal Server Error
```

#### Search Courses
```
GET /course/search/javascript?page=1&pageSize=10
Authentication: None (Public)

Path Parameters:
- searchTerm (string): Search keyword

Query Parameters:
- page (int): Page number
- pageSize (int): Items per page

Response (200 OK):
{
  "success": true,
  "message": "Successfully retrieved 5 courses",
  "data": [ /* CourseCardDTO array */ ]
}

Behavior:
- Splits search term into words
- Searches in: Title (2x weight), Subtitle (1x weight), Tags (1x weight)
- Ranks by relevance score, then rating, then alphabetical
- Pagination applied to results

Error Cases:
- 400 Bad Request: Empty search term
- 204 No Content: No matches found
- 500 Internal Server Error
```

#### Get Course Details
```
GET /course/{courseId}
Authentication: None (Public)

Path Parameters:
- courseId (int): Course ID

Response (200 OK):
{
  "success": true,
  "message": "Course details retrieved successfully",
  "data": {
    "id": 1,
    "title": "JavaScript Basics",
    "subtitle": "Learn JS from scratch",
    "description": "Comprehensive JavaScript course...",
    "thumbnailUrl": "https://...",
    "price": 49.99,
    "isFree": false,
    "level": "Beginner",
    "language": "English",
    "createdAt": "2026-01-15T10:00:00Z",
    "updatedAt": "2026-05-20T15:30:00Z",
    "instructorId": "user-id-123",
    "instructorName": "Jane Smith",
    "categoryName": "Programming",
    "averageRating": 4.5,
    "totalRatings": 120,
    "enrollmentCount": 500,
    "sectionCount": 8,
    "tags": ["JavaScript", "Web Development", "Frontend"],
    "requirements": ["Basic computer knowledge", "Internet connection"],
    "learningOutcomes": ["Master JavaScript fundamentals", "Build real applications"],
    "sections": [
      {
        "id": 1,
        "title": "Getting Started",
        "order": 1,
        "lessons": [
          {
            "id": 1,
            "title": "Welcome",
            "duration": "00:10:30",
            "isPreview": true,
            "videoURL": "https://video.url",
            "order": 1
          },
          ...
        ],
        "quizzes": [
          {
            "id": 1,
            "quizTitle": "Section 1 Quiz",
            "description": "Test your knowledge"
          },
          ...
        ]
      },
      ...
    ],
    "feedbacks": [
      {
        "username": "John Doe",
        "avatar": "https://avatar.url",
        "comment": "Great course!",
        "rating": 5,
        "postedAt": "2026-05-20T10:00:00Z"
      },
      ...
    ]
  }
}

Error Cases:
- 400 Bad Request: Invalid courseId
- 404 Not Found: Course not found
- 500 Internal Server Error
```

#### Create Course
```
POST /course/create
Content-Type: application/json
Authentication: Required (Roles: Instructor, Admin, SuperAdmin)

Request:
{
  "title": "Advanced JavaScript",
  "category": "Programming",
  "status": 2 (0=Draft, 1=Pending, 2=Published, 3=Archived, 4=Rejected)
}

Response (201 Created):
{
  "success": true,
  "message": "Course created successfully",
  "data": {
    "id": 5,
    "title": "Advanced JavaScript",
    "category": "Programming",
    "status": "Draft"
  }
}

Error Cases:
- 400 Bad Request: Validation errors
- 401 Unauthorized: Invalid token
- 403 Forbidden: User role not allowed
- 404 Not Found: Category not found
- 500 Internal Server Error

Validation Rules:
- Title: Required, 3-200 characters, specific pattern
- Category: Required, must exist in database
- Status: Optional, defaults to Draft
```

#### Update Course (NEW)
```
PUT /course/{courseId}
Content-Type: application/json
Authentication: Required (Roles: Instructor, Admin, SuperAdmin)

Path Parameters:
- courseId (int): Course ID to update

Request:
{
  "description": "Complete course description...",
  "price": 49.99,
  "isFree": false,
  "thumbnailUrl": "https://...",
  "language": "English",
  "subtitle": "Master the fundamentals",
  "learningOutcomes": [
    "Understand core concepts",
    "Build real projects",
    "Deploy to production"
  ],
  "targetAudiences": [
    "Beginners",
    "Career changers",
    "Self-taught developers"
  ],
  "tags": ["JavaScript", "Programming", "Web Development"],
  "requirements": [
    "Basic computer knowledge",
    "Internet connection",
    "Code editor installed"
  ],
  "sections": [
    {
      "id": null,
      "title": "Getting Started",
      "order": 1,
      "lessons": [
        {
          "id": null,
          "title": "Welcome Lesson",
          "description": "Introduction to the course",
          "videoUrl": "https://video.url",
          "order": 1,
          "duration": "00:15:30",
          "isPreview": true
        },
        {
          "id": null,
          "title": "Course Overview",
          "description": "What you'll learn",
          "videoUrl": "https://video.url",
          "order": 2,
          "duration": "00:08:45",
          "isPreview": false
        }
      ],
      "quizzes": [
        {
          "id": null,
          "title": "Getting Started Quiz",
          "description": "Test your understanding",
          "type": 0,
          "passingScore": 70,
          "isRequired": true
        }
      ]
    },
    {
      "id": null,
      "title": "Core Concepts",
      "order": 2,
      "lessons": [ /* ... */ ],
      "quizzes": [ /* ... */ ]
    }
  ]
}

Response (200 OK):
{
  "success": true,
  "message": "Course updated successfully",
  "data": {
    "id": 1,
    "title": "JavaScript Basics",
    "updatedAt": "2026-05-25T14:30:00Z"
  }
}

Error Cases:
- 400 Bad Request: Validation errors, invalid data
- 401 Unauthorized: Invalid token
- 403 Forbidden: Not course owner/admin
- 404 Not Found: Course not found
- 500 Internal Server Error

Authorization Rules:
- Only course instructor/admin/superadmin can update
- Updates are atomic (all or nothing)

Validation Rules:
- Description: Required, 10-5000 characters
- Price: Required, 0-10000 range
- IsFree: If true, price set to 0
- ThumbnailUrl: Required, valid URL
- Language: Required, 2-50 characters
- Subtitle: Optional, max 500 characters
- LearningOutcomes: Required, min 1
- TargetAudiences: Required, min 1
- Tags: Required, min 1
- Requirements: Optional, can be empty
- Sections: Required, min 1
  - Each section must have Title, Order
  - Min 1 lesson or quiz per section
- Lessons: Title, VideoUrl, Order, Duration required
- Quizzes: Title, Type, PassingScore, IsRequired required
```

---

### **EnrollmentController** - `/enrollment` (Inferred - Not Yet Implemented)

#### Enroll in Course
```
POST /enrollment/courses/{courseId}
Authentication: Required (Role: Student)

Path Parameters:
- courseId (int): Course to enroll in

Response (201 Created):
{
  "success": true,
  "message": "Successfully enrolled in course",
  "data": {
    "studentId": "user-id",
    "courseId": 1,
    "enrolledAt": "2026-05-25T14:30:00Z"
  }
}

Error Cases:
- 400 Bad Request: Invalid courseId
- 401 Unauthorized: Not authenticated
- 403 Forbidden: Course not published
- 404 Not Found: Course not found
- 409 Conflict: Already enrolled

Business Rules:
- Only published courses can be enrolled
- Duplicate enrollments prevented
- EnrolledAt timestamp recorded
```

#### Get Student's Enrolled Courses
```
GET /enrollment/courses?page=1&pageSize=10
Authentication: Required

Response (200 OK):
{
  "success": true,
  "message": "Retrieved student's courses",
  "data": [
    {
      "courseId": 1,
      "title": "JavaScript Basics",
      "instructorName": "Jane Smith",
      "progressPercentage": 45.5,
      "isCompleted": false,
      "enrolledAt": "2026-05-20T10:00:00Z",
      "lastAccessAt": "2026-05-25T14:00:00Z"
    },
    ...
  ]
}

Query Parameters:
- page (int): Pagination
- pageSize (int): Items per page
```

#### Get Enrollment Progress
```
GET /enrollment/progress/{courseId}
Authentication: Required

Response (200 OK):
{
  "success": true,
  "message": "Retrieved progress",
  "data": {
    "courseId": 1,
    "progressPercentage": 45.5,
    "isCompleted": false,
    "completedLessons": 9,
    "totalLessons": 20,
    "completedQuizzes": 2,
    "totalQuizzes": 5,
    "lastAccessAt": "2026-05-25T14:00:00Z",
    "enrolledAt": "2026-05-20T10:00:00Z"
  }
}
```

---

### **ProgressController** - `/progress` (Inferred - Not Yet Implemented)

#### Get Lesson Progress
```
GET /progress/lessons/{lessonId}
Authentication: Required

Response (200 OK):
{
  "success": true,
  "data": {
    "lessonId": 5,
    "isCompleted": false,
    "watchedPercentage": 65.0,
    "lastSecondWatched": 980,
    "completedAt": null
  }
}
```

#### Update Lesson Progress
```
PUT /progress/lessons/{lessonId}
Content-Type: application/json
Authentication: Required

Request:
{
  "watchedPercentage": 100.0,
  "lastSecondWatched": 945,
  "isCompleted": true
}

Response (200 OK):
{
  "success": true,
  "message": "Progress updated"
}

Business Rules:
- Watched percentage must be 0-100
- Marks lesson complete if percentage >= 80%
- Updates course progress automatically
- Records CompletedAt if just completed
```

#### Get Course Progress
```
GET /progress/courses/{courseId}
Authentication: Required

Response (200 OK):
{
  "success": true,
  "data": {
    "courseId": 1,
    "progressPercentage": 45.5,
    "completedLessons": 9,
    "totalLessons": 20,
    "completedQuizzes": 2,
    "totalQuizzes": 5,
    "isCompleted": false
  }
}
```

---

### **QuizController** - `/quiz` (Inferred - Not Yet Implemented)

#### Get Quiz Details
```
GET /quiz/{quizId}
Authentication: None (Public)

Response (200 OK):
{
  "success": true,
  "data": {
    "id": 1,
    "title": "Section 1 Quiz",
    "description": "Test your understanding",
    "type": 0,
    "passingScore": 70,
    "isRequired": true,
    "questions": [
      {
        "id": 1,
        "title": "Question 1",
        "questionText": "What is JavaScript?",
        "pointsValue": 10,
        "options": [
          {
            "id": 1,
            "optionText": "A programming language",
            "isCorrect": true,
            "explanation": "JavaScript is indeed a programming language"
          },
          {
            "id": 2,
            "optionText": "A coffee brand",
            "isCorrect": false
          }
        ]
      },
      ...
    ]
  }
}
```

#### Submit Quiz Attempt
```
POST /quiz/{quizId}/submit
Content-Type: application/json
Authentication: Required

Path Parameters:
- quizId (int): Quiz to submit

Request:
{
  "answers": [
    {
      "questionId": 1,
      "selectedOptionId": 1
    },
    {
      "questionId": 2,
      "selectedOptionId": 3
    },
    {
      "questionId": 3,
      "answerText": "My essay answer here"
    }
  ]
}

Response (200 OK):
{
  "success": true,
  "message": "Quiz submitted successfully",
  "data": {
    "attemptId": 5,
    "score": 85,
    "isPassed": true,
    "passingScore": 70,
    "message": "Congratulations! You passed the quiz."
  }
}

Error Cases:
- 400 Bad Request: Invalid answers
- 401 Unauthorized: Not authenticated
- 404 Not Found: Quiz not found
- 500 Internal Server Error

Business Rules:
- Validates answer formats
- Calculates score based on correct answers
- Marks IsPassed based on PassingScore
- Records attempt for audit trail
- Updates course progress if required quiz
```

#### Get Quiz Attempts
```
GET /quiz/{quizId}/attempts
Authentication: Required

Response (200 OK):
{
  "success": true,
  "data": [
    {
      "attemptId": 5,
      "score": 85,
      "isPassed": true,
      "startedAt": "2026-05-25T10:00:00Z",
      "submittedAt": "2026-05-25T10:15:30Z"
    },
    ...
  ]
}
```

---

### **FeedbackController** - `/feedback` (Inferred - Not Yet Implemented)

#### Post Course Review
```
POST /feedback/courses/{courseId}
Content-Type: application/json
Authentication: Required

Request:
{
  "rating": 5,
  "comment": "Excellent course! Very well-structured and the instructor explains concepts clearly."
}

Response (201 Created):
{
  "success": true,
  "message": "Review posted successfully",
  "data": {
    "id": 10,
    "courseId": 1,
    "rating": 5,
    "comment": "Excellent course!",
    "createdAt": "2026-05-25T14:30:00Z"
  }
}

Validation:
- Rating: Required, 1-5
- Comment: Optional, max 2000 characters
- Student must be enrolled in course
- Only one review per student per course
```

#### Get Course Reviews
```
GET /feedback/courses/{courseId}?page=1&pageSize=10
Authentication: None (Public)

Response (200 OK):
{
  "success": true,
  "message": "Retrieved 10 reviews",
  "data": [
    {
      "id": 10,
      "username": "John Doe",
      "avatar": "https://avatar.url",
      "rating": 5,
      "comment": "Excellent course!",
      "createdAt": "2026-05-25T14:30:00Z"
    },
    ...
  ]
}

Query Parameters:
- page (int): Pagination
- pageSize (int): Items per page
```

#### Update Review
```
PUT /feedback/{feedbackId}
Content-Type: application/json
Authentication: Required

Request:
{
  "rating": 4,
  "comment": "Still great but could use more advanced examples"
}

Response (200 OK):
{
  "success": true,
  "message": "Review updated successfully"
}

Authorization Rules:
- Only review author can update own review
```

#### Delete Review
```
DELETE /feedback/{feedbackId}
Authentication: Required

Response (200 OK):
{
  "success": true,
  "message": "Review deleted successfully"
}

Authorization Rules:
- Only review author or admin can delete
```

---

### **CategoryController** - `/category` (Likely Exists)

#### Get All Categories
```
GET /category/all
Authentication: None (Public)

Response (200 OK):
{
  "success": true,
  "data": [
    {
      "id": 1,
      "name": "Programming",
      "description": "Learn to code"
    },
    {
      "id": 2,
      "name": "Design",
      "description": "Design skills"
    },
    ...
  ]
}
```

---

## Authentication & Authorization

### JWT Token Structure

```
Header:
{
  "alg": "HS256",
  "typ": "JWT"
}

Payload:
{
  "sub": "user-id-123",
  "email": "user@example.com",
  "nameid": "user-id-123",
  "unique_name": "username",
  "role": "Instructor",
  "iat": 1234567890,
  "exp": 1234651290,
  "iss": "LoopLearn",
  "aud": "LoopLearnAPI"
}

Signature:
HMACSHA256(base64UrlEncode(header) + "." + base64UrlEncode(payload), secret)
```

### Token Claims

| Claim | Type | Purpose |
|-------|------|---------|
| `sub` | string | Subject (User ID) |
| `email` | string | User email |
| `nameid` | string | User ID (for extracting in code) |
| `unique_name` | string | Username |
| `role` | string | User role (Student/Instructor/Admin/SuperAdmin) |
| `iat` | int | Issued at timestamp |
| `exp` | int | Expiration timestamp |
| `iss` | string | Issuer (LoopLearn) |
| `aud` | string | Audience (LoopLearnAPI) |

### Available Roles

| Role | Permissions |
|------|-------------|
| **Student** | ✅ Enroll in courses<br>✅ View published courses<br>✅ Track progress<br>✅ Take quizzes<br>✅ Leave reviews<br>✅ Comment on lessons<br>❌ Create courses<br>❌ Publish courses<br>❌ Manage other users |
| **Instructor** | ✅ All Student permissions<br>✅ Create courses<br>✅ Edit own courses<br>✅ Add/manage sections & lessons<br>✅ Create quizzes<br>✅ View student progress<br>❌ Delete other instructor's courses<br>❌ Manage users<br>❌ Publish own courses (requires admin) |
| **Admin** | ✅ All Instructor permissions<br>✅ Publish/reject courses<br>✅ Archive courses<br>✅ View all analytics<br>✅ Manage user roles<br>✅ Delete inappropriate content<br>✅ View platform statistics<br>❌ Change superadmin role |
| **SuperAdmin** | ✅ Full system access<br>✅ Manage all users and roles<br>✅ Access all resources<br>✅ Configure system settings |

### Password Policy

```csharp
Minimum Length: 8 characters
Require Digit: true (at least one number)
Require Uppercase: true (at least one CAPITAL letter)
Require Lowercase: true (at least one lowercase letter)
```

### Account Security

```csharp
Max Failed Login Attempts: 5
Lockout Duration: 15 minutes
```

### Authorization in Endpoints

```csharp
// Public (no authentication required)
[HttpGet("all")]
public async Task<IActionResult> GetCourses() { }

// Role-based (one or more roles)
[Authorize(Roles = "Instructor,Admin,SuperAdmin")]
[HttpPost("create")]
public async Task<IActionResult> CreateCourse() { }

// Only current user's own resources
[Authorize]
[HttpPut("profile")]
public async Task<IActionResult> UpdateProfile() { }
```

---

## Business Logic Rules

### Course Management

#### Course Lifecycle
```
Draft
  ↓
(Instructor submits for review)
  ↓
Pending
  ├─ (Admin approves)
  │  ↓
  │  Published
  │  ↓
  │  (Deprecated)
  │  ↓
  │  Archived
  │
  └─ (Admin rejects)
     ↓
     Rejected
```

#### Course Creation Rules
- ✅ Only **Instructor**, **Admin**, or **SuperAdmin** roles can create
- ✅ New courses default to **Draft** status
- ✅ Must select valid category
- ✅ **InstructorId** automatically assigned from JWT token
- ✅ **CreatedAt** and **UpdatedAt** timestamped

#### Course Publication Rules
- ✅ **Draft** → **Pending** when instructor submits
- ✅ **Pending** → **Published** when admin approves
- ✅ **Pending** → **Rejected** when admin rejects (with reason)
- ✅ **Published** → **Archived** when course deprecated
- ✅ Any status can become **Archived**

#### Course Update Rules (NEW)
- ✅ Only course **Instructor**, **Admin**, or **SuperAdmin** can update
- ✅ Updates are **atomic** (all-or-nothing)
- ✅ All nested data replaced (sections, lessons, quizzes)
- ✅ Related collections managed:
  - Learning outcomes replaced
  - Target audiences replaced
  - Tags created/reused as needed
  - Requirements replaced
  - Sections/lessons/quizzes completely regenerated

#### Course Pricing Rules
- ✅ If **IsFree = true** → **Price** automatically set to **0**
- ✅ If **IsFree = false** → **Price** required (0.01 - 10,000)
- ✅ Price changes only on unpublished courses (business decision)

#### Course Content Rules
- ✅ Courses organized in **Sections**
- ✅ Sections contain **Lessons** and/or **Quizzes**
- ✅ Lessons have **VideoUrl** (required), **Duration**, **Order**
- ✅ Lessons can be marked **IsPreview** (free preview)
- ✅ Section **Order** determines display sequence
- ✅ Lesson **Order** determines sequence within section
- ✅ Maximum sections/lessons determined by business (not enforced in code)

---

### Student Enrollment

#### Enrollment Rules
- ✅ Students can enroll in **Published** courses only
- ✅ **Duplicate prevention**: One enrollment per student per course
- ✅ **EnrolledAt** timestamp recorded automatically
- ✅ **LastAccessAt** updated on each access
- ✅ Students can enroll in multiple courses

#### Progress Calculation
```csharp
Formula: ProgressPercentage = (CompletedLessons / TotalLessons) * 100

Additional Rules:
- Lesson marked complete when WatchedPercentage >= 80%
- Required quizzes must be passed
- Optional quizzes don't affect completion
- ProgressPercentage updated after each lesson/quiz interaction
```

#### Enrollment Completion Rules
- ✅ **IsCompleted = true** when:
  - All lessons watched >= 80%
  - All required quizzes passed
  - ProgressPercentage = 100%
- ✅ **CompletedAt** timestamp recorded
- ✅ Student receives completion notification (future)
- ✅ Certificate generated (future)

---

### Lesson Progress Tracking

#### Tracking Capabilities
- ✅ **WatchedPercentage**: 0-100% of lesson watched
- ✅ **LastSecondWatched**: Resume capability (in seconds)
- ✅ **IsCompleted**: Boolean completion flag
- ✅ **CompletedAt**: Timestamp of completion

#### Progress Rules
- ✅ Only **enrolled students** can track progress
- ✅ Progress persists across sessions (resume from last point)
- ✅ Cannot go backward in watched percentage
- ✅ Watched percentage > 80% auto-completes (configurable)
- ✅ Multiple views of same lesson allowed
- ✅ Most recent watch session tracked

---

### Quiz & Assessment

#### Quiz Types
```csharp
enum QuizType
{
  MultipleChoice = 0,
  TrueFalse = 1,
  ShortAnswer = 2,
  Essay = 3
}
```

#### Quiz Structure
- ✅ Quiz has **Title**, **Description**, **Type**
- ✅ Quiz belongs to **Section** or **Course** (not both required)
- ✅ Quiz contains **Questions**
- ✅ Each question has **Options** (for MC/TF) or **AnswerText** (for essay)
- ✅ **PassingScore** (0-100): minimum score to pass
- ✅ **IsRequired**: affects course completion

#### Quiz Attempt Rules
- ✅ Student can attempt quiz multiple times
- ✅ Score calculated: (CorrectAnswers / TotalQuestions) * 100
- ✅ **IsPassed = (Score >= PassingScore)**
- ✅ Best score tracked (future optimization)
- ✅ All attempts recorded for audit trail
- ✅ **StartedAt** and **SubmittedAt** timestamped
- ✅ Time limits enforced (future enhancement)

#### Grading Rules
- ✅ **Multiple Choice/True-False**: Auto-graded against correct option
- ✅ **Short Answer**: Keyword matching (future: ML-based)
- ✅ **Essay**: Manual grading by instructor (future)

#### Required vs Optional Quizzes
- ✅ **IsRequired = true**:
  - Must pass (score >= PassingScore)
  - Must complete to finish course
  - Blocks certificate (future)
- ✅ **IsRequired = false**:
  - Optional for course completion
  - Does not affect progress %
  - Can attempt for practice

---

### Course Reviews & Ratings

#### Review Rules
- ✅ Only **enrolled students** can review
- ✅ **One review per student per course** (can update, not duplicate)
- ✅ **Rating**: 1-5 scale (integer)
- ✅ **Comment**: Optional, max 2000 characters
- ✅ **CreatedAt** and **UpdatedAt** timestamps

#### Rating Calculation
```csharp
AverageRating = Feedbacks.Average(f => f.Rating)
TotalRatings = Feedbacks.Count()
```

#### Review Visibility
- ✅ Reviews displayed on course detail page
- ✅ Sorted by recency (newest first)
- ✅ Anonymous option (future enhancement)
- ✅ Helpful voting (future enhancement)

#### Review Moderation
- ✅ Admin can delete inappropriate reviews
- ✅ Users can edit/delete own reviews
- ✅ Flagging system (future)

---

### Course Discovery & Search

#### Search Algorithm
```csharp
1. Split search term into words (space/comma/newline delimited)
2. Match against:
   - Title (2x weight): "javascript basics" matches "JavaScript"
   - Subtitle (1x weight): "learn js" matches "Learn JS"
   - Tags (1x weight): matches tag names
3. Calculate relevance score per course
4. Sort by:
   - Relevance score (descending)
   - Average rating (descending)
   - Title (alphabetical ascending)
5. Apply pagination
```

#### Filtering Capabilities
- ✅ By **Category**: Multi-select (AND operator)
- ✅ By **Price Range**: Free / Paid / Price threshold
- ✅ By **Level**: Beginner / Intermediate / Advanced
- ✅ By **Instructor**: (future)
- ✅ By **Rating**: 4+ stars, 3+ stars, etc. (future)

#### Pagination
- ✅ Standard pagination: page & pageSize
- ✅ Default: page=1, pageSize=10
- ✅ Response headers: Total-Count, Page, PageSize

---

### Lesson Comments (Future Enhancement)

#### Comment Rules
- ✅ Only **enrolled students** can comment
- ✅ Comments tied to specific **Lesson**
- ✅ Timestamp recorded (**CreatedAt**, **UpdatedAt**)
- ✅ Can edit/delete own comments
- ✅ Admin can delete any comment
- ✅ Threading (future)

---

## Implementation Status

### ✅ Completed Features

#### Authentication & Authorization
- ✅ JWT token generation with role-based claims
- ✅ User registration with validation
- ✅ User login with email/username
- ✅ Password hashing and verification
- ✅ Account lockout after failed attempts
- ✅ Role-based access control (RBAC)
- ✅ Token expiration validation
- ✅ Custom validation attributes

#### Course Management
- ✅ Create course (title, category, status)
- ✅ Get all courses (paginated, with filtering)
- ✅ Get course by ID (detailed view)
- ✅ Filter by categories
- ✅ Full-text search with relevance scoring
- ✅ **Update course (NEW)** - comprehensive update with:
  - Basic properties (description, price, language, etc.)
  - Learning outcomes management
  - Target audiences management
  - Tags management
  - Requirements management
  - Complete sections/lessons/quizzes rebuild

#### Database & Data Access
- ✅ Entity Framework Core integration
- ✅ SQL Server configuration
- ✅ DbContext with all entities
- ✅ Repository pattern implementation
- ✅ Unit of Work pattern
- ✅ Generic repository for CRUD
- ✅ Database migrations
- ✅ Database seeding with sample data
- ✅ Composite keys (CourseTag, Enrollment, StudentLessonProgress)
- ✅ Foreign key relationships
- ✅ Navigation properties

#### Architecture
- ✅ Layered architecture (API, DataAccess, Entities)
- ✅ Dependency injection setup
- ✅ CORS configuration
- ✅ Swagger/OpenAPI documentation
- ✅ Custom exception handling
- ✅ Error response standardization
- ✅ Logging infrastructure

#### Infrastructure
- ✅ Program.cs configuration
- ✅ JWT configuration with validation
- ✅ Identity framework setup
- ✅ Password policy enforcement
- ✅ Custom validation attributes
- ✅ Database connection management

---

### ⏳ Pending Implementation (High Priority)

#### Student Enrollment
- ⏳ Enroll student in course endpoint
- ⏳ Get student's enrolled courses
- ⏳ Prevent duplicate enrollments
- ⏳ Unroll/unenroll functionality
- ⏳ Enrollment status tracking

#### Progress Tracking
- ⏳ Track lesson watch progress
- ⏳ Update watched percentage
- ⏳ Resume from last position
- ⏳ Calculate course progress %
- ⏳ Mark lesson complete
- ⏳ Check course completion status

#### Quiz & Assessment
- ⏳ Get quiz with questions
- ⏳ Submit quiz answers
- ⏳ Auto-grade quiz (MC/TF)
- ⏳ Calculate score and pass/fail
- ⏳ Record quiz attempt
- ⏳ Support multiple attempts
- ⏳ Generate quiz report

#### Course Reviews
- ⏳ Post course review
- ⏳ Get course reviews
- ⏳ Update own review
- ⏳ Delete review
- ⏳ Calculate average rating
- ⏳ Moderation capabilities

#### Lesson Comments (Nice-to-Have)
- ⏳ Post comment on lesson
- ⏳ Get lesson comments
- ⏳ Update own comment
- ⏳ Delete comment
- ⏳ Comment threading

---

### ⏳ Pending Implementation (Medium Priority)

#### Payment Integration
- ⏳ Stripe integration for paid courses
- ⏳ PayPal integration
- ⏳ Payment processing
- ⏳ Transaction tracking
- ⏳ Refund handling
- ⏳ Invoice generation

#### Notifications
- ⏳ Email notifications on enrollment
- ⏳ Completion congratulations email
- ⏳ Quiz result notifications
- ⏳ Course update notifications
- ⏳ Push notifications (future)

#### Certificates
- ⏳ Certificate generation on completion
- ⏳ Certificate template customization
- ⏳ PDF generation
- ⏳ Certificate validation/verification

#### Admin Features
- ⏳ Course approval/rejection workflow
- ⏳ Analytics dashboard
- ⏳ Student progress reports
- ⏳ Instructor statistics
- ⏳ Platform metrics

#### Content Management
- ⏳ File upload (videos, images)
- ⏳ Video streaming optimization
- ⏳ Course deletion (soft delete)
- ⏳ Content versioning
- ⏳ Draft save/auto-save

---

### ⏳ Pending Implementation (Low Priority / Future)

- ⏳ Course recommendations engine
- ⏳ Wishlist/save for later
- ⏳ Learning paths
- ⏳ Discussion forums
- ⏳ Live streaming support
- ⏳ Peer review system
- ⏳ Badges & achievements
- ⏳ Leaderboards
- ⏳ Mobile app optimization
- ⏳ Offline content download

---

## Known Issues & Technical Debt

### 🔴 Critical Issues

#### 1. UpdateCourse Transaction Management
**Severity:** High  
**Issue:** `UpdateCourse` calls `SaveAsync()` multiple times within the method, potentially causing partial updates on failure.  
**Impact:** Database could have orphaned sections/lessons if update fails mid-operation.  
**Recommendation:**
```csharp
// Use DbContextTransaction for atomicity
using (var transaction = await _context.Database.BeginTransactionAsync())
{
    try
    {
        // All update operations
        await _unitOfWork.SaveAsync();
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

#### 2. N+1 Query Problem
**Severity:** Medium  
**Issue:** `GetCoursesById` includes multiple navigation properties:
```csharp
"Instructor,Category,Feedbacks,Enrollments,Sections,CourseTags,Requirements,LearningOutcomes"
```
This causes multiple queries per course retrieval.  
**Impact:** Performance degradation with large datasets.  
**Recommendation:** Use projection/SelectMany for specific DTOs instead of eager loading all.

#### 3. No Soft Delete Implementation
**Severity:** Medium  
**Issue:** Courses deleted via cascade delete lose all student progress/enrollments.  
**Impact:** Data loss, audit trail lost.  
**Recommendation:** Add `IsDeleted` (bool) flag to Course, Category, and user-generated entities.

---

### 🟡 Medium Issues

#### 4. Missing Cascade Delete Configuration
**Severity:** Medium  
**Issue:** Relationships defined without explicit cascade delete configuration.  
**Impact:** Orphaned records possible if not properly configured in DbContext.OnModelCreating.  
**Recommendation:** Review all FK relationships in ApplicationDbContext for cascade behavior.

#### 5. No Concurrency Control
**Severity:** Medium  
**Issue:** No optimistic locking implemented (no RowVersion/Timestamp).  
**Impact:** Multiple instructors could corrupt course data simultaneously.  
**Recommendation:** Add `[Timestamp]` property to Course and other edit-able entities.

```csharp
[Timestamp]
public byte[] RowVersion { get; set; }
```

#### 6. Validation Rules Partially Implemented
**Severity:** Low  
**Issue:** Some validation happens in DTOs, but business logic validation (like "IsFree + Price") happens in controller.  
**Impact:** Inconsistent validation logic, harder to maintain.  
**Recommendation:** Move business logic to domain models or separate validation service.

#### 7. Error Handling Not Comprehensive
**Severity:** Low  
**Issue:** Some endpoints catch generic `Exception` and return 500.  
**Impact:** Client can't differentiate between validation vs. server errors.  
**Recommendation:** Use specific exception types (BadRequestException, ConflictException, etc.).

---

### 🟢 Low Priority / Nice-to-Have

#### 8. No Rate Limiting
**Severity:** Low  
**Issue:** API has no throttling/rate limiting.  
**Recommendation:** Implement AspNetCoreRateLimit or similar middleware.

#### 9. No Caching Strategy
**Severity:** Low  
**Issue:** No caching layer implemented.  
**Recommendation:** Add Redis for categories, popular courses, etc.

#### 10. No API Versioning
**Severity:** Low  
**Issue:** API uses no versioning scheme.  
**Recommendation:** Implement API versioning (e.g., `/api/v1/course`).

#### 11. Pagination Not Consistent
**Severity:** Low  
**Issue:** Some endpoints may lack pagination while others have it.  
**Recommendation:** Ensure all list endpoints are paginated.

#### 12. File Upload Not Implemented
**Severity:** Medium  
**Issue:** Courses use URLs for videos/images, no file upload capability.  
**Recommendation:** Implement Azure Blob Storage or AWS S3 integration.

#### 13. Search Algorithm Could Be Enhanced
**Severity:** Low  
**Issue:** Search uses client-side LINQ, not full-text search.  
**Recommendation:** Implement SQL Server full-text search or Elasticsearch.

#### 14. No Comprehensive Logging
**Severity:** Low  
**Issue:** Basic logging only; no request/response logging or structured logging.  
**Recommendation:** Implement Serilog for structured logging.

#### 15. DTOs Missing Some Properties
**Severity:** Low  
**Issue:** Some DTOs incomplete (e.g., CourseDetailDTO missing status).  
**Recommendation:** Audit all DTOs for completeness.

---

## Next Steps & Priorities

### 🎯 Phase 1: MVP Core Features (Weeks 1-2)

**Goal:** Enable students to enroll, progress, and complete courses

#### 1.1 Implement Enrollment Endpoints (2-3 days)
- [ ] `POST /enrollment/courses/{courseId}` - Enroll in course
- [ ] `GET /enrollment/courses` - Get student's courses
- [ ] `DELETE /enrollment/courses/{courseId}` - Unenroll
- [ ] Business logic:
  - Validate course is published
  - Prevent duplicate enrollments
  - Initialize enrollment record
  - Create enrollment notification (stub)
- [ ] Error handling:
  - Already enrolled
  - Course not found
  - Course not published

#### 1.2 Implement Progress Tracking (2-3 days)
- [ ] `GET /progress/lessons/{lessonId}` - Get lesson progress
- [ ] `PUT /progress/lessons/{lessonId}` - Update watched %
- [ ] `GET /progress/courses/{courseId}` - Get course progress
- [ ] Business logic:
  - Update WatchedPercentage
  - Auto-complete at 80%
  - Update LastAccessAt on course
  - Calculate course ProgressPercentage
  - Prevent watched % from going backward
- [ ] Error handling:
  - Student not enrolled
  - Lesson not found

#### 1.3 Implement Quiz Submission (3-4 days)
- [ ] `GET /quiz/{quizId}` - Get quiz with questions
- [ ] `POST /quiz/{quizId}/submit` - Submit answers
- [ ] `GET /quiz/{quizId}/attempts` - Get attempts
- [ ] Business logic:
  - Validate answer format
  - Auto-grade MC/TF questions
  - Calculate score
  - Determine pass/fail
  - Record QuizAttempt
  - Update course completion if required
- [ ] Error handling:
  - Invalid question/option IDs
  - Quiz not found

**Estimated Effort:** 1.5 weeks  
**Blockers:** None  
**Success Metrics:**
- Students can enroll in courses
- Progress tracked accurately
- Quizzes graded automatically

---

### 🎯 Phase 2: User Experience Features (Weeks 3-4)

**Goal:** Add reviews and improve course feedback

#### 2.1 Implement Reviews/Feedback (2-3 days)
- [ ] `POST /feedback/courses/{courseId}` - Post review
- [ ] `GET /feedback/courses/{courseId}` - Get reviews
- [ ] `PUT /feedback/{feedbackId}` - Update review
- [ ] `DELETE /feedback/{feedbackId}` - Delete review
- [ ] Business logic:
  - One review per student per course
  - Calculate average rating
  - Only enrolled students can review
  - Prevent duplicate reviews
- [ ] Error handling:
  - Student not enrolled
  - Duplicate review

#### 2.2 Add Lesson Comments (Optional) (2 days)
- [ ] `POST /lessons/{lessonId}/comments` - Post comment
- [ ] `GET /lessons/{lessonId}/comments` - Get comments
- [ ] `PUT /comments/{commentId}` - Update comment
- [ ] `DELETE /comments/{commentId}` - Delete comment

**Estimated Effort:** 1 week  
**Blockers:** Phase 1 completion  
**Success Metrics:**
- Students can review courses
- Rating calculations work
- Comments displayed on lessons

---

### 🎯 Phase 3: Monetization (Weeks 5-6)

**Goal:** Enable paid courses

#### 3.1 Integrate Payment Provider (3-4 days)
- [ ] Choose: Stripe or PayPal
- [ ] `POST /payments/checkout` - Create checkout session
- [ ] `POST /payments/webhook` - Handle payment webhook
- [ ] `GET /payments/status/{transactionId}` - Check status
- [ ] Business logic:
  - Free courses skip payment
  - Paid courses require payment
  - Grant access after payment
  - Handle failed payments
  - Refund logic

#### 3.2 Add Payment Records (1-2 days)
- [ ] Create Payment entity model
- [ ] Track transaction history
- [ ] Calculate instructor revenue
- [ ] Platform commission tracking

**Estimated Effort:** 1.5 weeks  
**Blockers:** Payment provider API keys  
**Success Metrics:**
- Payment processing works
- Access granted on successful payment
- Transaction history tracked

---

### 🎯 Phase 4: Content Features (Weeks 7-8)

**Goal:** Enable file uploads and better content management

#### 4.1 File Upload Integration (3-4 days)
- [ ] Choose: Azure Blob Storage or AWS S3
- [ ] `POST /upload/lessons/{lessonId}/video` - Upload lesson video
- [ ] `POST /upload/courses/{courseId}/thumbnail` - Upload course image
- [ ] `POST /upload/users/avatar` - Upload profile picture
- [ ] Business logic:
  - Validate file types (video/image)
  - Virus scan (optional)
  - Generate thumbnail (for videos)
  - Return CDN URL

#### 4.2 Course Deletion (1 day)
- [ ] `DELETE /courses/{courseId}` - Soft delete course
- [ ] Business logic:
  - Soft delete (IsDeleted = true)
  - Hide from listings
  - Preserve student data
  - Admin can hard delete

#### 4.3 Draft Auto-Save (1-2 days)
- [ ] `PUT /courses/{courseId}/autosave` - Save draft
- [ ] Persist incomplete course data
- [ ] Timestamp last save

**Estimated Effort:** 1.5 weeks  
**Blockers:** Cloud storage setup  
**Success Metrics:**
- Video uploads work
- Thumbnails generated
- Soft delete preserves data

---

### 🎯 Phase 5: Admin & Analytics (Weeks 9-10)

**Goal:** Add moderation and instructor analytics

#### 5.1 Course Approval Workflow (2-3 days)
- [ ] `PUT /admin/courses/{courseId}/publish` - Approve course
- [ ] `PUT /admin/courses/{courseId}/reject` - Reject with reason
- [ ] Status transitions: Pending → Published/Rejected
- [ ] Admin notifications

#### 5.2 Analytics Dashboard (2-3 days)
- [ ] `GET /admin/analytics/courses` - Course stats
- [ ] `GET /admin/analytics/students` - Student stats
- [ ] `GET /admin/analytics/instructors` - Instructor revenue
- [ ] Metrics:
  - Total enrollments per course
  - Completion rate
  - Average rating
  - Revenue by instructor

#### 5.3 Moderation Tools (1-2 days)
- [ ] Delete inappropriate reviews
- [ ] Flag content for review
- [ ] Instructor removal
- [ ] User suspension

**Estimated Effort:** 1.5 weeks  
**Blockers:** Phase 1 completion  
**Success Metrics:**
- Course approval workflow works
- Analytics dashboard functional
- Moderation tools operational

---

### 🎯 Phase 6: Notifications & Certificates (Weeks 11-12)

**Goal:** Enhance engagement with notifications and credentials

#### 6.1 Email Notifications (2-3 days)
- [ ] Integration: SendGrid or Mailgun
- [ ] Enrollment confirmation email
- [ ] Course completion congratulations
- [ ] Quiz result notifications
- [ ] Course update notifications
- [ ] Newsletter (optional)

#### 6.2 Certificate Generation (2-3 days)
- [ ] `GET /certificates/{courseId}` - Generate/download certificate
- [ ] PDF generation (iTextSharp or similar)
- [ ] Certificate template customization
- [ ] Verification link
- [ ] LinkedIn integration (future)

**Estimated Effort:** 1.5 weeks  
**Blockers:** Email service setup  
**Success Metrics:**
- Emails sent reliably
- Certificates generated on completion
- Certificate verification works

---

### 📋 Summary Timeline

```
Phase 1: Enrollment & Progress (Weeks 1-2) ████████░░
Phase 2: Reviews & Comments (Weeks 3-4) ██████░░░░
Phase 3: Payments (Weeks 5-6) ████████░░
Phase 4: File Uploads & Management (Weeks 7-8) ████████░░
Phase 5: Admin & Analytics (Weeks 9-10) ████████░░
Phase 6: Notifications & Certificates (Weeks 11-12) ████████░░

Estimated Total: 12 weeks (3 months)
With parallel work: 6-8 weeks
```

---

### 🚀 Quick Start for Next Developer

1. **Clone Repository**
   ```bash
   git clone https://github.com/Loop-Learn/LoopLearnv2.0.Backend.git
   cd LoopLearnv2.0.Backend
   git checkout Profile
   ```

2. **Setup Database**
   ```bash
   # Update appsettings.json with your connection string
   dotnet ef database update
   ```

3. **Run Application**
   ```bash
   dotnet run
   # API available at: https://localhost:7xxx
   # Swagger UI: https://localhost:7xxx/swagger
   ```

4. **Start with Phase 1**
   - Implement enrollment endpoints
   - Follow the patterns already established in CourseController
   - Use UpdateCourse endpoint as reference for nested data handling

5. **Key Files to Review**
   - `LoopLearn.API/Controllers/CourseController.cs` - Endpoint patterns
   - `LoopLearn.Entities/Models/Course.cs` - Entity structure
   - `LoopLearn.DataAccess/Implementation/UnitOfWork.cs` - Data access pattern
   - `LoopLearn.API/Program.cs` - Configuration

6. **Testing Approach**
   - Create Postman/Insomnia collection
   - Test each endpoint manually before moving forward
   - Implement integration tests (optional but recommended)

---

## Code Examples & Patterns

### Creating a New Endpoint (Pattern)

```csharp
[HttpPost("{courseId}/enroll")]
[Authorize(Roles = "Student")]
public async Task<IActionResult> EnrollCourse(int courseId)
{
    try
    {
        // 1. Validate input
        if (courseId < 1)
        {
            return BadRequest(new
            {
                success = false,
                message = "Course ID must be greater than 0"
            });
        }

        // 2. Get current user
        var userId = GetUserId();

        // 3. Get entity from database
        var course = await _unitOfWork.Courses.GetFirstOrDefaultAsync(
            c => c.Id == courseId);

        if (course is null)
        {
            return NotFound(new
            {
                success = false,
                message = "Course not found"
            });
        }

        // 4. Business logic validation
        if (course.Status != CourseStatus.Published)
        {
            return BadRequest(new
            {
                success = false,
                message = "Only published courses can be enrolled"
            });
        }

        // 5. Check for duplicates
        var existingEnrollment = await _unitOfWork.Enrollments
            .GetFirstOrDefaultAsync(e => e.StudentId == userId && e.CourseId == courseId);

        if (existingEnrollment != null)
        {
            return Conflict(new
            {
                success = false,
                message = "Already enrolled in this course"
            });
        }

        // 6. Create entity
        var enrollment = new Enrollment
        {
            StudentId = userId,
            CourseId = courseId,
            EnrolledAt = DateTime.UtcNow,
            ProgressPercentage = 0,
            IsCompleted = false
        };

        // 7. Save to database
        await _unitOfWork.Enrollments.AddAsync(enrollment);
        await _unitOfWork.SaveAsync();

        // 8. Return success response
        return CreatedAtAction(nameof(GetEnrollment), new { courseId = courseId },
            new
            {
                success = true,
                message = "Successfully enrolled in course",
                data = new
                {
                    studentId = userId,
                    courseId = courseId,
                    enrolledAt = enrollment.EnrolledAt
                }
            });
    }
    catch (UnauthorizedAccessException)
    {
        return Unauthorized(new { success = false, message = "Invalid Token" });
    }
    catch (Exception ex)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, new
        {
            success = false,
            message = "An unexpected error occurred"
        });
    }
}
```

### DTO Pattern

```csharp
// Create a DTO for the request
public class EnrollmentDTO
{
    [Required]
    public int CourseId { get; set; }
}

// Use in controller
[HttpPost("enroll")]
[Authorize]
public async Task<IActionResult> Enroll([FromBody] EnrollmentDTO request)
{
    // Validate
    if (!ModelState.IsValid)
    {
        return BadRequest(ModelState);
    }

    // Use request.CourseId
}
```

### Helper Method Pattern

```csharp
private string GetUserId()
{
    var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    if (string.IsNullOrEmpty(userIdClaim))
    {
        throw new UnauthorizedAccessException("User ID not found in token");
    }
    return userIdClaim;
}

private async Task<bool> IsUserEnrolled(string userId, int courseId)
{
    var enrollment = await _unitOfWork.Enrollments
        .GetFirstOrDefaultAsync(e => e.StudentId == userId && e.CourseId == courseId);
    return enrollment != null;
}
```

---

## Testing Checklist

### Unit Testing (Optional but Recommended)

```csharp
[TestFixture]
public class EnrollmentControllerTests
{
    private Mock<IUnitOfWork> _unitOfWorkMock;
    private EnrollmentController _controller;

    [SetUp]
    public void Setup()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _controller = new EnrollmentController(_unitOfWorkMock.Object);
    }

    [Test]
    public async Task EnrollCourse_WithValidCourseId_ReturnsCreatedResult()
    {
        // Arrange
        int courseId = 1;
        var course = new Course { Id = courseId, Status = CourseStatus.Published };

        _unitOfWorkMock
            .Setup(x => x.Courses.GetFirstOrDefaultAsync(It.IsAny<Expression<Func<Course, bool>>>(), It.IsAny<string>()))
            .ReturnsAsync(course);

        // Act
        var result = await _controller.EnrollCourse(courseId);

        // Assert
        Assert.IsInstanceOf<CreatedAtActionResult>(result);
    }

    [Test]
    public async Task EnrollCourse_WithInvalidCourseId_ReturnsBadRequest()
    {
        // Arrange
        int courseId = -1;

        // Act
        var result = await _controller.EnrollCourse(courseId);

        // Assert
        Assert.IsInstanceOf<BadRequestObjectResult>(result);
    }
}
```

### Integration Testing

```csharp
[TestFixture]
public class EnrollmentIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [Test]
    public async Task EnrollCourse_Integration_ReturnsCreated()
    {
        // Arrange
        var token = GetTestToken(); // Mock JWT token
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.PostAsync("/api/enrollment/courses/1", null);

        // Assert
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
    }
}
```

### Manual Testing with Postman

```json
{
  "info": {
    "name": "LoopLearn API",
    "schema": "https://schema.getpostman.com/json/collection/v2.1.0/collection.json"
  },
  "item": [
    {
      "name": "Auth",
      "item": [
        {
          "name": "Login",
          "request": {
            "method": "POST",
            "url": "{{baseUrl}}/api/auth/login",
            "body": {
              "mode": "raw",
              "raw": "{\"emailOrUsername\": \"user@example.com\", \"password\": \"SecurePass123\"}"
            }
          }
        }
      ]
    },
    {
      "name": "Enrollment",
      "item": [
        {
          "name": "Enroll in Course",
          "request": {
            "method": "POST",
            "url": "{{baseUrl}}/api/enrollment/courses/1",
            "header": {
              "Authorization": "Bearer {{token}}"
            }
          }
        }
      ]
    }
  ]
}
```

---

## References & Resources

### ASP.NET Core Documentation
- https://learn.microsoft.com/en-us/aspnet/core/
- https://learn.microsoft.com/en-us/ef/core/

### Entity Framework Core
- https://learn.microsoft.com/en-us/ef/core/
- Migrations: https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/

### JWT Authentication
- https://learn.microsoft.com/en-us/aspnet/core/security/authentication/
- https://jwt.io/

### Best Practices
- REST API Design: https://restfulapi.net/
- Error Handling: https://www.rfc-editor.org/rfc/rfc7231
- Pagination: https://www.citusdata.com/blog/2016/03/30/five-ways-to-paginate/

### Design Patterns
- Repository Pattern: https://martinfowler.com/eaaCatalog/repository.html
- Unit of Work: https://martinfowler.com/eaaCatalog/unitOfWork.html
- DTO Pattern: https://martinfowler.com/eaaCatalog/dataTransferObject.html

---

## Conclusion

LoopLearn is a well-architected online learning platform with solid foundation. The implementation of the UpdateCourse endpoint demonstrates the maturity of the codebase. The next developer should focus on completing the MVP features (Phases 1-2) to enable core functionality before moving to monetization and advanced features.

**Key Takeaways:**
- ✅ Architecture is scalable and maintainable
- ✅ Design patterns are consistently applied
- ✅ Database schema is well-designed
- ⚠️ Some transaction management issues to fix
- ✅ Clear path forward with prioritized phases

**Good luck with development! 🚀**

---

*Document Version: 1.0*  
*Last Updated: May 25, 2026*  
*Next Review: After Phase 1 Completion*
