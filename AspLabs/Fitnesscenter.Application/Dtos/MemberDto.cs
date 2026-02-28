using System;

namespace Fitnesscenter.Application.Dtos;

public record MemberDto(
    int Id, string FirstName, string LastName, string Email,
    string MembershipType, DateTime? ActiveSince,
    bool IsActive);
