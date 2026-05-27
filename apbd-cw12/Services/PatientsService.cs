using apbd_cw12.DTOs;
using apbd_cw12.Models;
using Microsoft.EntityFrameworkCore;

namespace apbd_cw12.Services;

public class PatientsService : IPatientsService
{
    private readonly HospitalDbContext _context;

    public PatientsService(HospitalDbContext context)
    {
        _context = context;
    }

    public async Task<List<PatientGetDto>> GetPatientsAsync(string? search)
    {
        var query = _context.Patients
            .Include(p => p.Admissions).ThenInclude(a => a.Ward)
            .Include(p => p.BedAssignments).ThenInclude(ba => ba.Bed).ThenInclude(b => b.BedType)
            .Include(p => p.BedAssignments).ThenInclude(ba => ba.Bed).ThenInclude(b => b.Room).ThenInclude(r => r.Ward)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchPattern = $"%{search}%";
            query = query.Where(p => 
                EF.Functions.Like(p.FirstName, searchPattern) || 
                EF.Functions.Like(p.LastName, searchPattern));
        }

        var patients = await query.ToListAsync();

       
        return patients.Select(p => new PatientGetDto
        {
            Pesel = p.Pesel,
            FirstName = p.FirstName,
            LastName = p.LastName,
            Age = p.Age,
            Sex = p.Sex ? "Male" : "Female",
            Admissions = p.Admissions.Select(a => new AdmissionDto
            {
                Id = a.Id,
                AdmissionDate = a.AdmissionDate,
                DischargeDate = a.DischargeDate,
                Ward = new WardDto
                {
                    Id = a.Ward.Id,
                    Name = a.Ward.Name,
                    Description = a.Ward.Description
                }
            }).ToList(),
            BedAssignments = p.BedAssignments.Select(ba => new BedAssignmentDto
            {
                Id = ba.Id,
                From = ba.From,
                To = ba.To,
                Bed = new BedDto
                {
                    Id = ba.Bed.Id,
                    BedType = new BedTypeDto
                    {
                        Id = ba.Bed.BedType.Id,
                        Name = ba.Bed.BedType.Name,
                        Description = ba.Bed.BedType.Description
                    },
                    Room = new RoomDto
                    {
                        Id = ba.Bed.Room.Id,
                        HasTv = ba.Bed.Room.HasTv,
                        Ward = new WardDto
                        {
                            Id = ba.Bed.Room.Ward.Id,
                            Name = ba.Bed.Room.Ward.Name,
                            Description = ba.Bed.Room.Ward.Description
                        }
                    }
                }
            }).ToList()
        }).ToList();
    }

    public async Task AssignBedAsync(string pesel, CreateBedAssignmentRequest request)
    {
        var patientExists = await _context.Patients.AnyAsync(p => p.Pesel == pesel);
        if (!patientExists)
            throw new ArgumentException($"Nie znaleziono pacjenta o numerze PESEL: {pesel}");

        var requestTo = request.To ?? DateTime.MaxValue;

        var availableBed = await _context.Beds
            .Where(b => b.Room.Ward.Name == request.Ward && b.BedType.Name == request.BedType)
            .Where(b => !b.BedAssignments.Any(ba => 
                ba.From < requestTo && (ba.To ?? DateTime.MaxValue) > request.From))
            .FirstOrDefaultAsync();

        if (availableBed == null)
            throw new InvalidOperationException($"Brak wolnych łóżek typu '{request.BedType}' na oddziale '{request.Ward}' we wskazanym terminie.");

        var newAssignment = new BedAssignment
        {
            PatientPesel = pesel,
            BedId = availableBed.Id,
            From = request.From,
            To = request.To
        };

        _context.BedAssignments.Add(newAssignment);
        await _context.SaveChangesAsync();
    }
}