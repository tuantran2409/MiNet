using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MiNet.Controllers
{
    [Authorize]
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public UploadController(IWebHostEnvironment env)
        {
            _env = env;
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded" });

            // Kiểm tra file có phải ảnh không
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { error = "File không phải ảnh hợp lệ" });

            // Giới hạn dung lượng 5MB
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(new { error = "Ảnh không được vượt quá 5MB" });

            // Tạo thư mục uploads/chat nếu chưa có
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "chat");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Tạo tên file duy nhất
            var fileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Lưu file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về đường dẫn ảnh
            var imageUrl = $"/uploads/chat/{fileName}";
            return Ok(new { url = imageUrl });
        }

        // THÊM METHOD NÀY ĐỂ UPLOAD FILE (PDF, Word, Excel, ZIP, ...)
        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded" });

            // Giới hạn dung lượng 20MB
            if (file.Length > 20 * 1024 * 1024)
                return BadRequest(new { error = "File không được vượt quá 20MB" });

            // Tạo thư mục uploads/files nếu chưa có
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "files");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Tạo tên file duy nhất (giữ nguyên đuôi file gốc)
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Lưu file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Trả về thông tin file
            var fileUrl = $"/uploads/files/{fileName}";
            return Ok(new
            {
                url = fileUrl,
                name = file.FileName,
                size = file.Length,
                type = file.ContentType
            });
        }
    }
}