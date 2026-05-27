namespace apbd_cw12.DTOs;

public class PatientGetDto
{
    public string Pesel { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public string Sex { get; set; }
    public List<AdmissionDto> Admissions { get; set; }
    public List<BedAssignmentDto> BedAssignments { get; set; }
}