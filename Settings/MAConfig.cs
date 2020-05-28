using System.Runtime.Serialization;

namespace MarryAnyone
{
    [DataContract(Name = "MarryAnyoneConfig", Namespace = "")]

    internal class MAConfig
    {
        [DataMember(Name ="polygamy", IsRequired = false)]
        public bool IsPolygamous = false;

        [DataMember(Name = "incest", IsRequired = false)]
        public bool IsIncestual = false;

        [DataMember(Name = "orientation", IsRequired = false)]
        public SexualOrientation SexualOrientation = SexualOrientation.Heterosexual;
    }

    public enum SexualOrientation
    {
        Heterosexual,
        Homosexual,
        Bisexual
    }
}