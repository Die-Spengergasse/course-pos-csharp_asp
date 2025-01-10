using Hypermedia;
using IdHasher;
using System;
namespace Eventmanager.Application.Dtos;

public record GuestDto(
    Id Id,
    string Firstname, string Lastname, DateOnly BirthDate) : HypermediaDto;
