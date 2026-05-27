using apbd_cw12.DTOs;

namespace apbd_cw12.Services;

public interface IPatientsService
{
    Task<List<PatientGetDto>> GetPatientsAsync(string? search);
    Task AssignBedAsync(string pesel, CreateBedAssignmentRequest request);
}