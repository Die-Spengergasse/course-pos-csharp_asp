#pragma warning disable CS8618
using System;

namespace Fitnesscenter.Model;

public class Visit
{
    protected Visit() { }

    public Visit(Member member, DateTime start, DateTime? end)
    {
        Member = member;
        Start = start;
        End = end;
    }

    public int Id { get; set; }
    public Member Member { get; set; }
    public DateTime Start { get; set; }
    public DateTime? End { get; set; }
}

#pragma warning restore CS8618
