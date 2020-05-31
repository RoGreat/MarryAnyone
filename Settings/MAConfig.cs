using System.Runtime.Serialization;

namespace MarryAnyone
{
    [DataContract(Name = "MarryAnyoneConfig", Namespace = "")]

    internal class MAConfig
    {
        [DataMember(Name = "difficulty", Order = 1, IsRequired = false)]
        public Difficulty Difficulty = Difficulty.Realistic;

        [DataMember(Name = "orientation", Order = 2, IsRequired = false)]
        public SexualOrientation SexualOrientation = SexualOrientation.Heterosexual;

        [DataMember(Name = "polygamy", Order = 3, IsRequired = false)]
        public bool IsPolygamous = false;

        [DataMember(Name = "incest", Order = 4, IsRequired = false)]
        public bool IsIncestual = false;

        [DataMember(Name = "debug", Order = 5, IsRequired = false)]
        public bool Debug = false;
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