using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WEB_LAB2_FINAL.Models.ViewModel
{
    public class PersonVM
    {
        public System.Guid Id { get; set; }

        [Required]
        [DisplayName("Фамилия")]
        [StringLength(100)]

        public string LastName { get; set; }

        [Required]
        [DisplayName("Имя")]
        [StringLength(100)]

        public string FirstName { get; set; }

        [DisplayName("Отчество")]
        [StringLength(100)]

        public string Patronymic { get; set; }

        [Required]
        [DisplayName("Полных лет")]
        [Range(18, 100)]

        public int Age { get; set; }

        [DisplayName("Пол")]
        [StringLength(1)]

        public string Gender { get; set; }

        [Required]
        [DisplayName("Трудоустроен")]
        public bool HasJob { get; set; }

        [Required]
        [DisplayName("Дата рождения")]
        [DisplayFormat(DataFormatString = "{O: yyyy - MM - dd}", ApplyFormatInEditMode = true) ]
        public DateTime Birthday { get; set; }

        [Required]
        [DisplayName("Дата и время добавления")]
        [DisplayFormat(DataFormatString = "{Q: yyyy - MM - ddThh:mm}", ApplyFormatInEditMode = true)]
        public DateTime InsertedDateTime { get; set; }
        public System.Guid UserId { get; set; }
    }
}