using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FormProject.Models
{
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage ="این فیلد اجباری است")]
        [Display(Name ="نام و نام خانوادگی")]
        [MaxLength(22, ErrorMessage ="تعداد کاراکتر بیش از حد مجاز")]
        public string FullName { get; set; }
        [Required(ErrorMessage = "این فیلد اجباری است")]
        [Display(Name = "نام پدر")]
        [MaxLength(20, ErrorMessage ="تعداد کاراکتر بیش از حد مجاز")]
        public string FatherName { get; set; }
        [Required(ErrorMessage = "این فیلد اجباری است")]
        [Display(Name = "کد ملی")]
        [StringLength(10, ErrorMessage = "تعداد کاراکتر کمتر یا بیشتر از حد مجاز")]
        public string IdCardNumber { get; set; }
        [Required(ErrorMessage = "این فیلد اجباری است")]
        [Display(Name = "وضعیت تعهل")]
        [MaxLength(5)]
        public string MaritalStatus { get; set; }
        [Required(ErrorMessage = "این فیلد اجباری است")]
        [Display(Name = "وضعیت نظام وظیفه")]
        [MaxLength(20)]
        public string MilitaryServiceStatus { get; set; }
        public string ImageName { get; set; }
        [Required(ErrorMessage = "این فیلد اجباری است")]
        [Display(Name = "آپلود عکس")]
        [NotMapped]
        public IFormFile ImageFile { get; set; }
        [Required(ErrorMessage = "این فیلد اجباری است")]
        [Display(Name = "شماره تماس")]
        [StringLength(11, ErrorMessage ="تعداد کاراکتر کمتر یا بیشتر از حد مجاز")]
        public string CallNumber { get; set; }
        [Required(ErrorMessage = "این فیلد اجباری است")]
        [Display(Name = "آدرس")]
        [MaxLength(70, ErrorMessage ="تعداد کاراکتر بیش از حد مجاز")]
        public string Address { get; set; }
        [Required(ErrorMessage = "این فیلد اجباری است")]
        [Display(Name = "تاریخ تولد")]
        public string DateOfBirth { get; set; }
    }
}
