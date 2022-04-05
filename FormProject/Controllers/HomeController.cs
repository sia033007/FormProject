using FormProject.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FormProject.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using ClosedXML.Excel;
using DinkToPdf;
using DinkToPdf.Contracts;
using System.Runtime.Loader;
using System.Reflection;

namespace FormProject.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ApplicationDbContext _dbContext;
        private IConverter _converter;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment hostEnvironment, ApplicationDbContext dbContext, IConverter converter)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
            _dbContext = dbContext;
            _converter = converter;
        }
        [HttpPost]
        public async Task<IActionResult> Admin([Bind("FullName,FatherName")] UserProfile model)
        {
            if (model.FullName == "adminhastam" && model.FatherName == "adminhastam")
            {
                List<Claim> claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, model.FullName),
                        new Claim(ClaimTypes.Role, "Admin")

                    };

                var identity = new ClaimsIdentity(claims, "MyAuth");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("MyAuth", principal);
                TempData["success"] = "به عنوان ادمین وارد شدید";
                return RedirectToAction("GetUsers");
            }
            TempData["failed"] = "نام کاربری یا رمز عبور اشتباه است";
            return RedirectToAction("Index");
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _dbContext.Users.ToListAsync();
            return View(users);
        }
        public IActionResult Excel()
        {
            using (var workbook = new XLWorkbook())
            {
                var users = _dbContext.Users.ToList();
                var worksheet = workbook.Worksheets.Add("کاربران");
                var currentRow = 1;
                worksheet.Cell(currentRow, 1).Value = "نام و نام خانوادگی";
                worksheet.Cell(currentRow, 2).Value = "نام پدر";
                worksheet.Cell(currentRow, 3).Value = "کد ملی";
                worksheet.Cell(currentRow, 4).Value = "تاریخ تولد";
                worksheet.Cell(currentRow, 5).Value = "وضعیت تعهل";
                worksheet.Cell(currentRow, 6).Value = "وضعیت نظام وظیفه";
                worksheet.Cell(currentRow, 7).Value = "شماره تماس";
                worksheet.Cell(currentRow, 8).Value = "آدرس";
                worksheet.Cells().Style.Fill.BackgroundColor = XLColor.FromName("PowderBlue");

                foreach (var user in users)
                {
                    currentRow++;
                    worksheet.Cell(currentRow, 1).Value = user.FullName;
                    worksheet.Cell(currentRow, 2).Value = user.FatherName;
                    worksheet.Cell(currentRow, 3).Value = user.IdCardNumber;
                    worksheet.Cell(currentRow, 4).Value = user.DateOfBirth;
                    worksheet.Cell(currentRow, 5).Value = user.MaritalStatus;
                    worksheet.Cell(currentRow, 6).Value = user.MilitaryServiceStatus;
                    worksheet.Cell(currentRow, 7).Value = user.CallNumber;
                    worksheet.Cell(currentRow, 8).Value = user.Address;
                }
                worksheet.Rows().AdjustToContents();
                worksheet.Columns().AdjustToContents();
                worksheet.Cells().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);


                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(
                        content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "users.xlsx");
                }
            }
        }
        public ViewResult Index() => View();
        [HttpPost]
        public async Task<IActionResult> Index(UserProfile user)
        {
            if (ModelState.IsValid)
            {
                //Save image to wwwroot/image
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string fileName = "056";
                string extension = Path.GetExtension(user.ImageFile.FileName);
                user.ImageName = fileName = fileName + DateTime.Now.ToString("yymmssfff") + user.CallNumber + extension;
                string path = Path.Combine(wwwRootPath + "/image/", fileName);
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await user.ImageFile.CopyToAsync(fileStream);
                }
                //Insert record
                await _dbContext.Users.AddAsync(user);
                await _dbContext.SaveChangesAsync();
                TempData["success"] = "با تشکر با شما تماس گرفته خواهد شد";
                ModelState.Clear();
                return View();
            }
            TempData["failed"] = "مشکلی رخ داده است. دوباره تلاش کنید";
            return View();
        }
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            return View(user);

        }
        public IActionResult CreatePdf()
        {
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 7, Bottom = 7 },
                DocumentTitle = "گزارش کاربران"

            };
            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = GetHTMLString(),
                WebSettings = { DefaultEncoding = "utf-8",UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Style.css"), LoadImages = true }
            };
            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };
            var file = _converter.Convert(pdf);
            return File(file, "application/pdf", "Users.pdf");

        }
        public string GetHTMLString()
        {
            var users = _dbContext.Users.ToList();
            var wwwRootPath = _hostEnvironment.WebRootPath;
            var sb = new StringBuilder();
            foreach (var user in users)
            {
                var picAddress = Path.Combine(wwwRootPath + "/image/", user.ImageName);
                sb.Append(@"
                        <html dir='rtl' lang='fa-IR'>
                            <head>
                            </head>
                            <body dir='rtl'>");

                sb.AppendFormat(@"<table align ='center'>
                                      <tbody>
                                        <tr>
                                          <td></td>
                                          <td><img src={0} width='110px' height='130px' /></td>
                                        </tr>
                                        <tr>
                                          <td>نام و نام خانوادگی :
                                                 {1}</td>
                                          <td>نام پدر :
                                                 {2}</td>
                                        </tr>
                                        <tr>
                                          <td>کد ملی :
                                                 {3}</td>
                                          <td>تاریخ تولد :
                                                 {4}</td>
                                        </tr>
                                        <tr>
                                          <td>وضعیت تعهل :
                                                 {5}</td>
                                          <td>وضعیت نظام وظیفه :
                                                 {6}</td>
                                        </tr>
                                        <tr>
                                          <td>شماره تماس :
                                                 {7}</td>
                                          <td id='address'>آدرس :
                                                 {8}</td>
                                        </tr>", picAddress, user.FullName, user.FatherName, user.IdCardNumber,
                                        user.DateOfBirth, user.MaritalStatus, user.MilitaryServiceStatus, user.CallNumber, user.Address);
                sb.Append(@"
                                </table>
                            </body>
                        </html>");
            }
            return sb.ToString();
        }
        public IActionResult GetUsers()
        {
            return View();
        }
    public IActionResult Complete()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
    internal class CustomAssemblyLoadContext : AssemblyLoadContext
    {
        public IntPtr LoadUnmanagedLibrary(string absolutePath)
        {
            return LoadUnmanagedDll(absolutePath);
        }
        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            return LoadUnmanagedDllFromPath(unmanagedDllName);
        }
        protected override Assembly Load(AssemblyName assemblyName)
        {
            throw new NotImplementedException();
        }
    }
}
