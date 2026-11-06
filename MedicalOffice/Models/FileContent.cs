using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedicalOffice.Models
{
    //This class stores the real picture data referenced in the uploaded file class. 
    public class FileContent
    {
        [Key, ForeignKey("UploadedFile")] 
        public int FileContentID { get; set; }

        [ScaffoldColumn(false)] 
        public byte[]? Content { get; set; }
        public UploadedFile? UploadedFile { get; set; }
    }
}
