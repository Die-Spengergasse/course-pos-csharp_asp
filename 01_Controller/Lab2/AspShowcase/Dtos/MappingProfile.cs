using AspShowcase.Commands;
using AspShowcase.Models;
using AutoMapper;

namespace AspShowcase.Dtos
{
    public class MappingProfile : Profile  // using AutoMapper;
    {
        public MappingProfile()
        {
            // Ich kann aus einer Instanz einer Klasse Task automatisch
            // eine Instanz der Klasse TaskDto erzeugen lassen.
            CreateMap<Task, TaskDto>();
            CreateMap<TaskCmd, Task>();
        }
    }
}