using Eventmanager.Application.Model;
using System;

namespace Eventmanager.Model
{
    public class Ticket : Entity
    {
        protected Ticket() { }
        [System.Diagnostics.CodeAnalysis.SetsRequiredMembers]
        public Ticket(Guest guest, Contingent contingent, TicketState ticketState, DateTime reservationDateTime, int pax)
        {
            Guest = guest;
            Contingent = contingent;
            TicketState = ticketState;
            ReservationDateTime = reservationDateTime;
            Pax = pax;
        }
        public required Guest Guest { get; set; }
        public required Contingent Contingent { get; set; }
        public required TicketState TicketState { get; set; }
        public required DateTime ReservationDateTime { get; set; }
        public required int Pax { get; set; }
    }
}