namespace Fitnesscenter.Application.Commands;

// TODO: Add validation.
public record CreateMemberCmd(
    string FirstName,
    string LastName,
    string Email,
    string MembershipType,
    bool IsActive);

