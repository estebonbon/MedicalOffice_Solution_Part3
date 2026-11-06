using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.Models
{
    public class DoctorDocument : UploadedFile
    {
        [Display(Name = "Doctor")]

        //WIll serve as a foreign key to the Doctor table.  This is a required field.
        public int DoctorID { get; set; }

        //Retrieve the Doctor object associated with this document.  This is a navigation property.
        public Doctor? Docotor { get; set; }
    }
}
