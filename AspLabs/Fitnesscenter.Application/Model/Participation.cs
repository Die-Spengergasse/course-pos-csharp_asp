#pragma warning disable CS8618
namespace Fitnesscenter.Model;

public class Participation
{
    protected Participation() { }

    public Participation(TrainingSession session, Member member, int? rating)
    {
        TrainingSession = session;
        Member = member;
        Rating = rating;
    }

    public int Id { get; set; }
    public TrainingSession TrainingSession { get; set; }
    public Member Member { get; set; }
    public int? Rating { get; set; }
}

#pragma warning restore CS8618
