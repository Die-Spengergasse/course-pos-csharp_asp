using AspShowcase.Commands;
using AspShowcase.Dtos;
using AspShowcase.Infrastructure;
using AspShowcase.Models;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace AspShowcase.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly AspShowcaseContext _db;
        private readonly IMapper _mapper;  // braucht builder.Services.AddAutoMapper(typeof(MappingProfile));

        public TasksController(AspShowcaseContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        /// <summary>
        /// Default GET Route, also /api/tasks?expiresAfter=2023-01-02
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAllTasks([FromQuery] DateTime? expiresAfter)
        {
            expiresAfter ??= DateTime.MinValue;  // Zuweisung nur wenn der Wert NULL ist.

            // SELECT * FROM Tasks INNER JOIN Teacher ON (TeacherId = Id)
            // ORDER BY ExpirationDate

            var tasks = _mapper.ProjectTo<TaskDto>(
                _db.Tasks
                    .Where(t => t.ExpirationDate >= expiresAfter)
                    .OrderBy(t => t.ExpirationDate))
                .ToList();
            return Ok(tasks);  // HTTP 200 + Payload
        }

        /// <summary>
        /// Reagiert auf /api/tasks/82AA8096-4CC6-B320-0A34-FECE6C209F5B
        /// </summary>
        [HttpGet("{guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetTaskById(Guid guid)
        {
            var task = _mapper.ProjectTo<TaskDto>(_db.Tasks.Where(t => t.Guid == guid))
                .FirstOrDefault();
            if (task is null) { return NotFound(); }
            return Ok(task);
        }

        /// <summary>
        /// INSERT
        /// Reagiert auf POST /api/tasks
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AddTask([FromBody] TaskCmd taskCmd)
        {
            // Wir prüfen, ob es die Fremdschlüsselwerte TeamGUID und TaskGUID überhaupt gibt.
            var team = _db.Teams.FirstOrDefault(t => t.Guid == taskCmd.TeamGuid);
            var teacher = _db.Teachers.FirstOrDefault(t => t.Guid == taskCmd.TeacherGuid);
            if (team is null) { return BadRequest("Invalid Team GUID"); }
            if (teacher is null) { return BadRequest("Invalid Teacher GUID"); }

            // Erzeugt einen Task aus der Cmd Klasse, also TaskCmd --> Task
            // Siehe Mapping Profile: CreateMap<TaskCmd, Task>()
            var task = _mapper.Map<Task>(taskCmd, opt =>
            {
                opt.AfterMap((src, dest) =>
                {
                    dest.Team = team;
                    dest.Teacher = teacher;
                });
            });

            _db.Tasks.Add(task);
            try { _db.SaveChanges(); }
            catch (DbUpdateException e) { return BadRequest(e.InnerException?.Message ?? e.Message); }
            return CreatedAtAction(nameof(AddTask), new { Guid = task.Guid });
        }

        /// <summary>
        /// UPDATE
        /// Reagiert auf PUT /api/tasks/1234-56-78900
        /// </summary>
        [HttpPut("{guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdateTask(Guid guid, [FromBody] TaskCmd taskCmd)
        {
            if (guid != taskCmd.Guid) { return BadRequest(); }
            // Wir prüfen, ob es die Fremdschlüsselwerte TeamGUID und TaskGUID überhaupt gibt.
            // Ist es sinnvoll, bei einem bestehendem Task auch nachträglich das Team und
            // den Lehrer zu ändern?
            var team = _db.Teams.FirstOrDefault(t => t.Guid == taskCmd.TeamGuid);
            var teacher = _db.Teachers.FirstOrDefault(t => t.Guid == taskCmd.TeacherGuid);
            if (team is null) { return BadRequest("Invalid Team GUID"); }
            if (teacher is null) { return BadRequest("Invalid Teacher GUID"); }

            // Suche den alten Task in der Datenbank
            var task = _db.Tasks.FirstOrDefault(t => t.Guid == taskCmd.Guid);
            if (task is null) { return NotFound(); }
            // Map(source, dest) aktualisiert das dest Objekt mit den Daten von source.
            _mapper.Map(taskCmd, task, opt =>
            {
                opt.AfterMap((src, dest) =>
                {
                    dest.Team = team;
                    dest.Teacher = teacher;
                });
            });

            try { _db.SaveChanges(); }
            catch (DbUpdateException e) { return BadRequest(e.InnerException?.Message ?? e.Message); }
            return NoContent();
        }

        /// <summary>
        /// DELETE
        /// Reagiert auf DELETE /api/tasks/1234-56-78900
        /// </summary>
        [HttpDelete("{guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public IActionResult DeleteTask(Guid guid)
        {
            var task = _db.Tasks.FirstOrDefault(t => t.Guid == guid);
            if (task is null) { return NotFound(); }
            _db.Tasks.Remove(task);
            try { _db.SaveChanges(); }
            catch (DbUpdateException e) { return BadRequest(e.InnerException?.Message ?? e.Message); }
            return NoContent();
        }
    }
}