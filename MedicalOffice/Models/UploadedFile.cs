using System.ComponentModel.DataAnnotations;

namespace MedicalOffice.Models
{
    
    //This class is like a dummy info class.
    public class UploadedFile
    {
        public int ID { get; set; }
        
        [StringLength(255, ErrorMessage = "The name of the file cannot be more than 255 characters.")]
        [Display(Name = "File Name")]
        public string? FileName { get; set; }
        [StringLength(255)]

        //What type of file is this?  PDF, Word, Excel, etc.
        public string? MimeType { get; set; }

        //Navigation property for the FileContent
        public FileContent? FileContent { get; set; } = new FileContent(); //It will already have a file content object created for it.
    }

}
