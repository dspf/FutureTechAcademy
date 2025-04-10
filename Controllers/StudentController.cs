using FutureTechAcademy.Models;
using FutureTechAcademy.Services;
using Microsoft.AspNetCore.Mvc;

namespace FutureTechAcademy.Controllers
{
    public class StudentController : Controller
    {
        private readonly CosmosDbService _cosmosDb;
        private readonly BlobStorageService _blobStorage;

        public StudentController(CosmosDbService cosmosDb, BlobStorageService blobStorage)
        {
            _cosmosDb = cosmosDb;
            _blobStorage = blobStorage;
        }

        // READ: List all students
        public async Task<IActionResult> Index()
        {
            var students = await _cosmosDb.GetStudentsAsync();
            return View(students);
        }

        // CREATE: Show form
        [HttpGet]
        public IActionResult AddStudent()
        {
            return View();
        }

        [HttpGet]
        public async Task<bool> CheckEmail(string email)
        {
            var student = await _cosmosDb.GetStudentByEmailAsync(email);
            return student != null;
        }
        // CREATE: Handle submission
        [HttpPost]
        public async Task<IActionResult> AddStudent(Student student, IFormFile profileImage)
        {
            var existingStudent = await _cosmosDb.GetStudentByEmailAsync(student.Email);
            if (existingStudent != null)
            {
                ModelState.AddModelError("Email", "A student with this email already exists");
                return View(student); // Return to form with error
            }

            if (profileImage != null && profileImage.Length > 0)
            {
                student.ProfileImageUrl = await _blobStorage.UploadImageAsync(profileImage, student.Id);
            }

            await _cosmosDb.AddStudentAsync(student);
            return RedirectToAction("Index");
        }

        [HttpGet("student/image/{blobName}")]
        public async Task<IActionResult> GetStudentImage(string blobName)
        {
            try
            {
                var stream = await _blobStorage.GetImageStreamAsync(blobName);
                if (stream == null)
                    return NotFound();

                return File(stream, "application/octet-stream"); // Auto-detects content type
            }
            catch (Exception ex)
            {
                // Log error here
                return StatusCode(500);
            }
        }

        // UPDATE: Show edit form
        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            var student = await _cosmosDb.GetStudentByIdAsync(id);
            return View(student);
        }

        // UPDATE: Handle edit
        [HttpPost]
        public async Task<IActionResult> Edit(Student student, IFormFile profileImage)
        {
            if (profileImage != null)
                student.ProfileImageUrl = await _blobStorage.UploadImageAsync(profileImage, student.Id);

            await _cosmosDb.UpdateStudentAsync(student);
            return RedirectToAction("Index");
        }

        // DELETE: Confirm delete
        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            var student = await _cosmosDb.GetStudentByIdAsync(id);
            return View(student);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Student student, IFormFile profileImage)
        {
            try
            {
                student.ProfileImageUrl = await _blobStorage.UploadImageAsync(profileImage, student.Id);
                if (student == null)
                {
                    TempData["DeleteError"] = "Student not found";
                    return RedirectToAction("Index");
                }

                await _cosmosDb.UpdateStudentAsync(student);
                TempData["DeleteSuccess"] = $"{student.FirstName} {student.LastName} was deleted successfully";
            }
            catch (Exception ex)
            {
                TempData["DeleteError"] = $"Error deleting student: {ex.Message}";
            }

            return RedirectToAction("Index");
        }


    }

}
