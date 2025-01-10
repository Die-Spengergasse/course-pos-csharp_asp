using System.Collections.Generic;

namespace Eventmanager.Application.Dtos;

public record EventWithShowsDto(int Id, string Name, List<ShowDto> Shows);

