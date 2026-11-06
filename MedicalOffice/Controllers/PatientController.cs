
using MedicalOffice.CustomControllers;
using MedicalOffice.Data;
using MedicalOffice.Models;
using MedicalOffice.Utilities;
using MedicalOffice.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

public class PatientController : ElephantController
{
    private readonly MedicalOfficeContext _context;

    public PatientController(MedicalOfficeContext context)
    {
        _context = context;
    }

    // GET: PATIENTS
    public async Task<IActionResult> Index(string? SearchString, int? DoctorID, int? MedicalTrialID, int? ConditionID,
        string? actionButton, int? page, int? pageSizeID, string? CoverageFilter, string sortDirection = "asc", string sortField = "Patient")    
    {
        //List of sort options.
        //NOTE: make sure this array has matching values to the column headings
        string[] sortOptions = new[] { "Patient", "Age", "Coverage", "Doctor" };

        //Count the number of filters applied - start by assuming no filters
        ViewData["Filtering"] = "btn-outline-secondary";
        int numberFilters = 0;
        //Then in each "test" for filtering, add to the count of Filters applied

        PopulateDropDownLists();    //Data for Doctor and MedicalTrial Filter DDL

        //Extra Select List for Conditions
        ViewData["ConditionID"] = new SelectList(_context.Conditions.OrderBy(c => c.ConditionName), "ID", "ConditionName");

        //SelectList for the Coverage Enum
        if (Enum.TryParse(CoverageFilter, out Coverage selectedCoverage))
        {
            ViewBag.CoverageSelectList = Coverage.OHIP.ToSelectList(selectedCoverage);
        }
        else
        {
            ViewBag.CoverageSelectList = Coverage.OHIP.ToSelectList(null);
        }

        //Start with Includes but make sure your expression returns an
        //IQueryable<Patient> so we can add filter and sort options later.
        var patients = _context.Patients
                            .Include(p => p.Doctor)
                            .Include(p => p.MedicalTrial)
                            .Include(p => p.PatientConditions)
                                .ThenInclude(p => p.Condition)
                            .AsNoTracking();

        //Add as many filters as needed
        if (DoctorID.HasValue)
        {
            patients = patients.Where(p => p.DoctorID == DoctorID);
            numberFilters++;
        }
        if (MedicalTrialID.HasValue)
        {
            patients = patients.Where(p => p.MedicalTrialID == MedicalTrialID);
            numberFilters++;
        }
        if (!String.IsNullOrEmpty(SearchString))
        {
            patients = patients.Where(p => p.LastName.ToUpper().Contains(SearchString.ToUpper())
                                   || p.FirstName.ToUpper().Contains(SearchString.ToUpper()));
            numberFilters++;
        }
        if (ConditionID.HasValue)
        {
            patients = patients.Where(p => p.PatientConditions.Any(c => c.ConditionID == ConditionID));
            numberFilters++;
        }
        if (!String.IsNullOrEmpty(CoverageFilter))
        {
            patients = patients.Where(p => p.Coverage == selectedCoverage);
            numberFilters++;
        }

        //Give feedback about the state of the filters
        if (numberFilters != 0)
        {
            //Toggle the Open/Closed state of the collapse depending on if we are filtering
            ViewData["Filtering"] = " btn-danger";
            //Show how many filters have been applied
            ViewData["numberFilters"] = "(" + numberFilters.ToString()
                + " Filter" + (numberFilters > 1 ? "s" : "") + " Applied)";
            //Keep the Bootstrap collapse open
            @ViewData["ShowFilter"] = " show";
        }

        //Before we sort, see if we have called for a change of filtering or sorting
        if (!String.IsNullOrEmpty(actionButton)) //Form Submitted!
        {
            page = 1;//Reset page to start

            if (sortOptions.Contains(actionButton))//Change of sort is requested
            {
                if (actionButton == sortField) //Reverse order on same field
                {
                    sortDirection = sortDirection == "asc" ? "desc" : "asc";
                }
                sortField = actionButton;//Sort by the button clicked
            }
        }
        if (sortField == "Age")
        {
            if (sortDirection == "asc")
            {
                patients = patients
                    .OrderByDescending(p => p.DOB);
            }
            else
            {
                patients = patients
                    .OrderBy(p => p.DOB);
            }
        }
        else if (sortField == "Coverage")
        {
            if (sortDirection == "asc")
            {
                patients = patients
                    .OrderBy(p => p.Coverage)
                    .ThenBy(p => p.LastName)
                    .ThenBy(p => p.FirstName);
            }
            else
            {
                patients = patients
                    .OrderByDescending(p => p.Coverage)
                    .ThenBy(p => p.LastName)
                    .ThenBy(p => p.FirstName);
            }
        }
        else if (sortField == "Doctor")
        {
            if (sortDirection == "asc")
            {
                patients = patients
                    .OrderBy(p => p.Doctor.LastName)
                    .ThenBy(p => p.Doctor.FirstName)
                    .ThenBy(p => p.LastName)
                    .ThenBy(p => p.FirstName);
            }
            else
            {
                patients = patients
                    .OrderByDescending(p => p.Doctor.LastName)
                    .ThenByDescending(p => p.Doctor.FirstName)
                    .ThenBy(p => p.LastName)
                    .ThenBy(p => p.FirstName);
            }
        }
        else //Sorting by Patient Name
        {
            if (sortDirection == "asc")
            {
                patients = patients
                    .OrderBy(p => p.LastName)
                    .ThenBy(p => p.FirstName);
            }
            else
            {
                patients = patients
                    .OrderByDescending(p => p.LastName)
                    .ThenByDescending(p => p.FirstName);
            }
        }

        //Set sort for next time
        ViewData["sortField"] = sortField;
        ViewData["sortDirection"] = sortDirection;

        //Handle Paging
        int pageSize = PageSizeHelper.SetPageSize(HttpContext, pageSizeID, ControllerName());
        ViewData["pageSizeID"] = PageSizeHelper.PageSizeList(pageSize);
        var pagedData = await PaginatedList<Patient>.CreateAsync(patients.AsNoTracking(), page ?? 1, pageSize);

        return View(pagedData);
    }

    // GET: PATIENTS/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var patient = await _context.Patients
            .Include(p => p.Doctor)
            .Include(p => p.MedicalTrial)
            .Include(p => p.PatientConditions)
                .ThenInclude(p => p.Condition)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ID == id);

        if (patient == null)
        {
            return NotFound();
        }

        return View(patient);
    }

    // GET: PATIENTS/Create
    public IActionResult Create()
    {
        Patient patient = new Patient();
        PopulateAssignedConditionData(patient);
        PopulateDropDownLists();
        return View(patient);
    }

    // POST: PATIENTS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ID,OHIP,FirstName,MiddleName,LastName,DOB,ExpYrVisits,Phone,EMail,Coverage, MedicalTrialID, DoctorID")] Patient patient, string[] selectedOptions)
    {
        try
        {
            //Add the selected conditions
            if (selectedOptions != null)
            {
                foreach (var condition in selectedOptions)
                {
                    var conditionToAdd = new PatientCondition { PatientID = patient.ID, ConditionID = int.Parse(condition) };
                    patient.PatientConditions.Add(conditionToAdd);
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(patient);
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { patient.ID });
            }
        }
        catch (RetryLimitExceededException /* dex */)//This is a Transaction in the Database!
        {
            ModelState.AddModelError("", "Unable to save changes after multiple attempts. " +
                "Try again, and if the problem persists, see your system administrator.");
        }
        catch (DbUpdateException dex)
        {
            string message = dex.GetBaseException().Message;
            if (message.Contains("UNIQUE") && message.Contains("Patients.OHIP"))
            {
                ModelState.AddModelError("OHIP", "Unable to save changes. Remember, " +
                    "you cannot have duplicate OHIP numbers.");
            }
            else if (message.Contains("UNIQUE") && message.Contains("Patients.DOB"))
            {
                ModelState.AddModelError("", "Unable to save changes. Remember, " +
                    "you cannot have duplicate combinations of First Name, Last Name, and Date of Birth.");
            }
            else
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }
        }

        PopulateAssignedConditionData(patient);
        PopulateDropDownLists(patient);
        return View(patient);
    }

    // GET: PATIENTS/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var patient = await _context.Patients
            .Include(p => p.PatientConditions)
                .ThenInclude(p => p.Condition)
            .FirstOrDefaultAsync(p => p.ID == id);

        if (patient == null)
        {
            return NotFound();
        }

        PopulateAssignedConditionData(patient);
        PopulateDropDownLists(patient);
        return View(patient);
    }

    // POST: PATIENTS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, string[] selectedOptions, Byte[] RowVersion)
    {
        //Go get the patient to update
        var patientToUpdate = await _context.Patients
            .Include(p => p.PatientConditions)
                .ThenInclude(p => p.Condition)
            .FirstOrDefaultAsync(p => p.ID == id);

        //Check that you got it or exit with a not found error
        if (patientToUpdate == null)
        {
            return NotFound();
        }

        //Put the original RowVersion value in the OriginalValues collection for the entity
        _context.Entry(patientToUpdate).Property("RowVersion").OriginalValue = RowVersion;

        //Update the medical history
        UpdatePatientConditions(selectedOptions, patientToUpdate);

        //Try updating it with the values posted
        if (await TryUpdateModelAsync<Patient>(patientToUpdate, "", 
            p => p.OHIP, p => p.FirstName, p => p.MiddleName, p => p.LastName, p => p.DOB,
            p => p.ExpYrVisits, p => p.Phone, p => p.EMail, p => p.Coverage, p => p.MedicalTrialID, p => p.DoctorID))
        {
            try
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("Details", new { patientToUpdate.ID });
            }
            catch (DbUpdateConcurrencyException ex)// Added for concurrency
            {
                var exceptionEntry = ex.Entries.Single();
                var clientValues = (Patient)exceptionEntry.Entity;
                var databaseEntry = exceptionEntry.GetDatabaseValues();
                if (databaseEntry == null)
                {
                    ModelState.AddModelError("",
                        "Unable to save changes. The Patient was deleted by another user.");
                }
                else
                {
                    var databaseValues = (Patient)databaseEntry.ToObject();
                    if (databaseValues.FirstName != clientValues.FirstName)
                        ModelState.AddModelError("FirstName", "Current value: "
                            + databaseValues.FirstName);
                    if (databaseValues.MiddleName != clientValues.MiddleName)
                        ModelState.AddModelError("MiddleName", "Current value: "
                            + databaseValues.MiddleName);
                    if (databaseValues.LastName != clientValues.LastName)
                        ModelState.AddModelError("LastName", "Current value: "
                            + databaseValues.LastName);
                    if (databaseValues.OHIP != clientValues.OHIP)
                        ModelState.AddModelError("OHIP", "Current value: "
                            + databaseValues.OHIP);
                    if (databaseValues.DOB != clientValues.DOB)
                        ModelState.AddModelError("DOB", "Current value: "
                            + String.Format("{0:d}", databaseValues.DOB));
                    if (databaseValues.Phone != clientValues.Phone)
                        ModelState.AddModelError("Phone", "Current value: "
                            + databaseValues.PhoneFormatted);
                    if (databaseValues.EMail != clientValues.EMail)
                        ModelState.AddModelError("EMail", "Current value: "
                            + databaseValues.EMail);
                    if (databaseValues.ExpYrVisits != clientValues.ExpYrVisits)
                        ModelState.AddModelError("ExpYrVisits", "Current value: "
                            + databaseValues.ExpYrVisits);
                    if (databaseValues.Coverage != clientValues.Coverage)
                        ModelState.AddModelError("Coverage", "Current value: "
                            + databaseValues.Coverage);
                    //For the foreign key, we need to go to the database to get the information to show
                    if (databaseValues.DoctorID != clientValues.DoctorID)
                    {
                        Doctor? databaseDoctor = await _context.Doctors.FirstOrDefaultAsync(i => i.ID == databaseValues.DoctorID);
                        ModelState.AddModelError("DoctorID", $"Current value: {databaseDoctor?.Summary}");
                    }
                    //A little extra work for the nullable foreign key.  No sense going to the database and asking for something
                    //we already know is not there.
                    if (databaseValues.MedicalTrialID != clientValues.MedicalTrialID)
                    {
                        if (databaseValues.MedicalTrialID.HasValue)
                        {
                            MedicalTrial? databaseMedicalTrial = await _context.MedicalTrials.FirstOrDefaultAsync(i => i.ID == databaseValues.MedicalTrialID);
                            ModelState.AddModelError("MedicalTrialID", $"Current value: {databaseMedicalTrial?.TrialName}");
                        }
                        else
                        {
                            ModelState.AddModelError("MedicalTrialID", $"Current value: None");
                        }
                    }
                    ModelState.AddModelError(string.Empty, "The record you attempted to edit "
                            + "was modified by another user after you received your values. The "
                            + "edit operation was canceled and the current values in the database "
                            + "have been displayed. If you still want to save your version of this record, click "
                            + "the Save button again. Otherwise click the 'Back to Patient List' hyperlink.");

                    //Final steps before redisplaying: Update RowVersion from the Database
                    //and remove the RowVersion error from the ModelState
                    patientToUpdate.RowVersion = databaseValues.RowVersion ?? Array.Empty<byte>();
                    ModelState.Remove("RowVersion");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                ModelState.AddModelError("", "Unable to save changes after multiple attempts. " +
                    "Try again, and if the problem persists, see your system administrator.");
            }
            catch (DbUpdateException dex)
            {
                string message = dex.GetBaseException().Message;
                if (message.Contains("UNIQUE") && message.Contains("Patients.OHIP"))
                {
                    ModelState.AddModelError("OHIP", "Unable to save changes. Remember, " +
                        "you cannot have duplicate OHIP numbers.");
                }
                else if (message.Contains("UNIQUE") && message.Contains("Patients.DOB"))
                {
                    ModelState.AddModelError("", "Unable to save changes. Remember, " +
                        "you cannot have duplicate combinations of First Name, Last Name, and Date of Birth.");
                }
                else
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                }
            }

        }
        //If the form fails repoulate basically re-execute get
        PopulateAssignedConditionData(patientToUpdate);
        PopulateDropDownLists(patientToUpdate);
        return View(patientToUpdate);
    }

    // GET: PATIENTS/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var patient = await _context.Patients
            .Include(p => p.Doctor)
            .Include(p => p.MedicalTrial)
            .Include(p => p.PatientConditions)
                .ThenInclude(p => p.Condition)
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ID == id);

        if (patient == null)
        {
            return NotFound();
        }

        return View(patient);
    }

    // POST: PATIENTS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var patient = await _context.Patients
            .Include(p => p.Doctor)
            .Include(p => p.MedicalTrial)
            .Include(p => p.PatientConditions)
                .ThenInclude(p => p.Condition)
            .FirstOrDefaultAsync(m => m.ID == id);

        try
        {
            if (patient != null)
            {
                _context.Patients.Remove(patient);
            }

            await _context.SaveChangesAsync();

            var returnUrl = ViewData["returnURL"]?.ToString();

            if (string.IsNullOrEmpty(returnUrl))
            {
                return RedirectToAction(nameof(Index));
            }

            return LocalRedirect(returnUrl);
        }
        catch (DbUpdateException)
        {
            //Note: there is really no reason a delete should fail if you can "talk" to the database.
            ModelState.AddModelError("", "Unable to delete record. Try again, and if the problem persists see your system administrator.");
        }
        return View(patient);
    }

    // Populates the Doctor dropdown and selects the patient's current Doctor when editing.
    private void PopulateDropDownLists(Patient? patient = null)
    {
        ViewData["DoctorID"] = DoctorSelectList(patient?.DoctorID);
        ViewData["MedicalTrialID"] = MedicalTrialList(patient?.MedicalTrialID);

    }

    //This is a twist on the PopulateDropDownLists approach
    //  Create methods that return each SelectList separately
    //  and one method to put them all into ViewData.
    //This approach allows for AJAX requests to refresh
    //DDL Data at a later date.
    private SelectList DoctorSelectList(int? selectedId)
    {
        return new SelectList(_context.Doctors
            .OrderBy(d => d.LastName)
            .ThenBy(d => d.FirstName), "ID", "FormalName", selectedId);
    }

    private SelectList MedicalTrialList(int? selectedId)
    {
        return new SelectList(_context.MedicalTrials
            .OrderBy(m => m.TrialName), "ID", "TrialName", selectedId);
    }

    /// <summary>
    /// Prepares a collection of checkbox ViewModel objects for each condition.
    /// Sets Assigned = true for conditions already in the patient's medical history.
    /// </summary>
    /// <param name="patient">The Patient entity with included PatientConditions</param>
    private void PopulateAssignedConditionData(Patient patient)
    {
        //Ensure PatientConditions are loaded before calling this method
        var allConditions = _context.Conditions.ToList();

        var assignedConditionIds = new HashSet<int>(
            patient.PatientConditions.Select(pc => pc.ConditionID));

        var conditionOptions = allConditions.Select(condition => new CheckOptionVM
        {
            ID = condition.ID,
            DisplayText = condition.ConditionName,
            Assigned = assignedConditionIds.Contains(condition.ID)
        }).ToList();

        ViewData["ConditionOptions"] = conditionOptions;
    }

    /// <summary>
    /// Updates the PatientConditions for a Patient to match the selected checkboxes.
    /// </summary>
    /// <param name="selectedOptions">IDs of the selected conditions</param>
    /// <param name="patientToUpdate">The Patient entity to update</param>
    private void UpdatePatientConditions(string[]? selectedOptions, Patient patientToUpdate)
    {
        // If no options are selected, clear all conditions
        if (selectedOptions == null || selectedOptions.Length == 0)
        {
            patientToUpdate.PatientConditions = new List<PatientCondition>();
            return;
        }

        var selectedIds = new HashSet<string>(selectedOptions); // from razor
        var currentConditionIds = new HashSet<int>(
            patientToUpdate.PatientConditions.Select(pc => pc.ConditionID)); // pre exsisting

        foreach (var condition in _context.Conditions)
        {
            string conditionIdStr = condition.ID.ToString();

            bool isSelected = selectedIds.Contains(conditionIdStr);
            bool isCurrentlyAssigned = currentConditionIds.Contains(condition.ID);

            if (isSelected && !isCurrentlyAssigned)
            {
                // Add new condition
                patientToUpdate.PatientConditions.Add(new PatientCondition
                {
                    PatientID = patientToUpdate.ID,
                    ConditionID = condition.ID
                });
            }
            else if (!isSelected && isCurrentlyAssigned)
            {
                // Remove unselected condition
                var conditionToRemove = patientToUpdate.PatientConditions //Icollection find the matching conditionID in the collection give the value to conditionToRemove
                    .SingleOrDefault(pc => pc.ConditionID == condition.ID);

                if (conditionToRemove != null)
                {
                    _context.Remove(conditionToRemove);
                }
            }
        }
    }

    private bool PatientExists(int? id)
    {
        return _context.Patients.Any(e => e.ID == id);
    }
}
