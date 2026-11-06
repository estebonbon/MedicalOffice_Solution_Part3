
using MedicalOffice.CustomControllers;
using MedicalOffice.Data;
using MedicalOffice.Models;
using MedicalOffice.Utilities;
using MedicalOffice.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;

public class DoctorController : ElephantController
{
    private readonly MedicalOfficeContext _context;

    public DoctorController(MedicalOfficeContext context)
    {
        _context = context;
    }

    // GET: DOCTORS
    public async Task<IActionResult> Index(int? page, int? pageSizeID)
    {
        var doctors = _context.Doctors
             .Include(d => d.DoctorSpecialties)
                .ThenInclude(d => d.Specialty)
             .AsNoTracking();

        //Handle Paging
        int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
        ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
        var pagedData = await PaginatedList<Doctor>.CreateAsync(doctors.AsNoTracking(), page ?? 1, pageSize);

        return View(pagedData);
    }

    // GET: DOCTORS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var doctor = await _context.Doctors
            .Include(d => d.Patients)
            .Include(d => d.DoctorSpecialties)
                .ThenInclude(d => d.Specialty)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ID == id);

        if (doctor == null)
        {
            return NotFound();
        }

        return View(doctor);
    }

    // GET: DOCTORS/Create
    public async Task<IActionResult> Create()
    {
        Doctor doctor = new Doctor();
        await PopulateAssignedSpecialtyDataAsync(doctor);
        return View(doctor);
    }

    // POST: DOCTORS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ID,FirstName,MiddleName,LastName,Patients")] Doctor doctor, string[] selectedOptions)
    {
        try
        {
            await UpdateDoctorSpecialtiesAsync(selectedOptions, doctor);
            if (ModelState.IsValid)
            {
                _context.Add(doctor);
                await _context.SaveChangesAsync();
                RedirectToAction("Details", new { doctor.ID });
            }
        }
        catch (RetryLimitExceededException /* dex */)
        {
            ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
        }

        //Validation Error so give the user another chance.
        await PopulateAssignedSpecialtyDataAsync(doctor);
        return View(doctor);
    }

    // GET: DOCTORS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var doctor = await _context.Doctors
               .Include(d => d.DoctorSpecialties)
                    .ThenInclude(d => d.Specialty)
               .FirstOrDefaultAsync(d => d.ID == id);

        if (doctor == null)
        {
            return NotFound();
        }

        await PopulateAssignedSpecialtyDataAsync(doctor);
        return View(doctor);
    }

    // POST: DOCTORS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string[] selectedOptions)
    {
        //Go get the Doctor to update
        var doctorToUpdate = await _context.Doctors
            .Include(d => d.DoctorSpecialties)
                .ThenInclude(d => d.Specialty)
            .FirstOrDefaultAsync(p => p.ID == id);

        //Check that you got it or exit with a not found error
        if (doctorToUpdate == null)
        {
            return NotFound();
        }

        //Update the Doctor's Specialties
        await UpdateDoctorSpecialtiesAsync(selectedOptions, doctorToUpdate);

        //Try updating it with the values posted
        if (await TryUpdateModelAsync<Doctor>(doctorToUpdate, "",
                d => d.FirstName, d => d.MiddleName, d => d.LastName))
        {
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { doctorToUpdate.ID });
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DoctorExists(doctorToUpdate.ID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
        }

        //Validation Error so give the user another chance.
        await PopulateAssignedSpecialtyDataAsync(doctorToUpdate);
        return View(doctorToUpdate);
    }

    // GET: DOCTORS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var doctor = await _context.Doctors
            .Include(d => d.DoctorSpecialties)
                .ThenInclude(d => d.Specialty)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ID == id);

        if (doctor == null)
        {
            return NotFound();
        }

        return View(doctor);
    }

    // POST: DOCTORS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var doctor = await _context.Doctors
            .Include(d => d.DoctorSpecialties)
                .ThenInclude(d => d.Specialty)
            .FirstOrDefaultAsync(m => m.ID == id);

        try
        {
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
            }

            await _context.SaveChangesAsync();

            var returnUrl = ViewData["returnURL"]?.ToString();
            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction(nameof(Index));
            }
            return Redirect(returnUrl);
        }
        catch (DbUpdateException dex)
        {
            if (dex.GetBaseException().Message.Contains("FOREIGN KEY constraint failed"))
            {
                ModelState.AddModelError("", "Unable to Delete Doctor. Remember, you cannot delete a " +
                    "Doctor that has patients assigned.");
            }
            else
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem" +
                    " persists see your system administrator.");
            }
        }
        return View(doctor);

    }

    public async Task<FileContentResult> Download(int id)

    {
        var theFile = await _context.UploadedFiles //This should be quering DoctorDocuments not UploadedFiles
               .Include(d => d.FileContent) //It is telling the application to include the other entity using this include line, so include the related relationship record its created un the uplodedFile class
               .Where(f => f.ID == id)
               .FirstOrDefaultAsync();

        if (theFile?.FileContent?.Content == null || theFile.MimeType == null)
        {
            // FALLBACK: If the file or its MIME type is missing, return an empty file response.
            // new FileContentResult(...) → Creates a response containing file data.
            // Array.Empty<byte>()       → Creates an empty byte array (zero bytes).
            // "application/octet-stream" → Generic MIME type for binary data.
            // return                   → Sends the response back and exits the method.
            // IMPORTANT: This does NOT retrieve the missing file. It returns an empty response.

            return new FileContentResult(Array.Empty<byte>(), "application/octet-stream");
        }

        //Three arguments will passed here, the Actual BYTES AKA JPEP, or PDF content, MimeType represent what type of file it is, and lastly the name of the file
        // File() is a built-in MVC Controller method that returns the file's bytes, MIME type, and filename to the browser.
        return File(theFile.FileContent.Content, theFile.MimeType, theFile.FileName);
    }

    //Helper Method
    private async Task AddDocumentsAsync(Doctor doctor, List<IFormFile> theFiles)
    {
        if (doctor == null || theFiles == null) //If either is null do nothing
            return;

        foreach (var f in theFiles) // goes through each file and make sure it is not null
        {
            if (f != null)
            {
                string mimeType = f.ContentType;
                string fileName = Path.GetFileName(f.FileName); //This shortens the path delivering only the file name
                long fileLength = f.Length; // Counts the number of bytes in the file, NOT their values (e.g., [5, 12, 9, 8, 8] = 5 bytes).

                if (!string.IsNullOrWhiteSpace(fileName) && fileLength > 0)
                {
                    using var tempBox = new MemoryStream();
                    await f.CopyToAsync(tempBox); //async operation so it is awaited, wait for it to finish

                    var document = new DoctorDocument //We create a labeler Object inheriting UploadedFile
                    {
                        MimeType = mimeType,
                        FileName = fileName,
                        //FileContent = new FileContent //We already have a FileContnet onject created in Uploaded File this here is redundant and confusing
                        //{
                        //    Content = tempBox.ToArray() //Converts the tempbox byte array and stores that new array in the content property of FileContent
                        //}
                    };
                    document.FileContent!.Content = tempBox.ToArray(); //! does NOT create an object or check whether it exists. It only suppresses the compiler's null warning.

                    doctor.DoctorDocuments.Add(document); //This adds it to the ICollection in Doctor for doctor documents
                }
            }
        }
    }

    private async Task PopulateAssignedSpecialtyDataAsync(Doctor doctor)
    {
        var allSpecialties = await _context.Specialties
            .OrderBy(s => s.SpecialtyName)
            .ToListAsync();

        var assignedSpecialtyIds = new HashSet<int>(
            doctor.DoctorSpecialties?.Select(ds => ds.SpecialtyID) ?? Enumerable.Empty<int>()
        );

        var selected = new List<ListOptionVM>();
        var available = new List<ListOptionVM>();

        foreach (var specialty in allSpecialties)
        {
            var option = new ListOptionVM
            {
                ID = specialty.ID,
                DisplayText = specialty.SpecialtyName
            };

            if (assignedSpecialtyIds.Contains(specialty.ID))
            {
                selected.Add(option);
            }
            else
            {
                available.Add(option);
            }
        }

        ViewData["selOpts"] = new MultiSelectList(selected, "ID", "DisplayText");
        ViewData["availOpts"] = new MultiSelectList(available, "ID", "DisplayText");
    }

    private async Task UpdateDoctorSpecialtiesAsync(string[] selectedOptions, Doctor doctorToUpdate)
    {
        if (selectedOptions == null || selectedOptions.Length == 0)
        {
            doctorToUpdate.DoctorSpecialties = new List<DoctorSpecialty>();
            return;
        }

        var selectedIds = selectedOptions.Select(int.Parse).ToHashSet();
        var currentIds = doctorToUpdate.DoctorSpecialties.Select(ds => ds.SpecialtyID).ToHashSet();

        var allSpecialties = await _context.Specialties.ToListAsync();

        foreach (var specialty in allSpecialties)
        {
            var specialtyId = specialty.ID;
            var isSelected = selectedIds.Contains(specialtyId);
            var isCurrentlyAssigned = currentIds.Contains(specialtyId);

            if (isSelected && !isCurrentlyAssigned)
            {
                doctorToUpdate.DoctorSpecialties.Add(new DoctorSpecialty
                {
                    SpecialtyID = specialtyId,
                    DoctorID = doctorToUpdate.ID
                });
            }

            if (!isSelected && isCurrentlyAssigned)
            {
                var toRemove = doctorToUpdate.DoctorSpecialties
                    .FirstOrDefault(ds => ds.SpecialtyID == specialtyId);

                if (toRemove != null)
                {
                    _context.DoctorSpecialties.Remove(toRemove);
                }
            }
        }
    }

    private bool DoctorExists(int? id)
    {
        return _context.Doctors.Any(e => e.ID == id);
    }
}
