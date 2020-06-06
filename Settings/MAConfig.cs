using System.Runtime.Serialization;

namespace MarryAnyone
{
    [DataContract(Name = "MarryAnyoneConfig", Namespace = "")]

    internal class MAConfig
    {
        [DataMember(Name = "difficulty", Order = 0)]
        public Difficulty Difficulty = Difficulty.Realistic;

        [DataMember(Name = "orientation", Order = 1)]
        public SexualOrientation SexualOrientation = SexualOrientation.Heterosexual;

        [DataMember(Name = "polygamy", Order = 2)]
        public bool IsPolygamous = false;

        [DataMember(Name = "incest", Order = 3)]
        public bool IsIncestual = false;

        [DataMember(Name = "debug", Order = 4)]
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