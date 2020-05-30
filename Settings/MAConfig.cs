using System.Runtime.Serialization;

namespace MarryAnyone
{
    [DataContract(Name = "MarryAnyoneConfig", Namespace = "")]

    internal class MAConfig
    {
        [DataMember(Name = "difficulty", IsRequired = false)]
        public Difficulty Difficulty = Difficulty.Realistic;

        [DataMember(Name = "orientation", IsRequired = false)]
        public SexualOrientation SexualOrientation = SexualOrientation.Heterosexual;

        [DataMember(Name ="polygamy", IsRequired = false)]
        public bool IsPolygamous = false;

        [DataMember(Name = "incest", IsRequired = false)]
        public bool IsIncestual = false;
    }

    public enum Difficulty
    {
        Realistic,
        Easy,
        VeryEasy
    }

    public enum SexualOrientation
    {
        Heterosexual,
        Homosexual,
        Bisexual
    }
}