using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.Models


{
    public class PatientDocument : UploadedFile
    {
        [Display(Name = "Patient")]
        //WIll serve as a foreign key to the Patient table.  This is a required field.
        public int PatientID { get; set; }

        //Retrieve the Patient object associated with this document.  This is a navigation property.
        public Patient? Patient { get; set; }
    

    }
}
