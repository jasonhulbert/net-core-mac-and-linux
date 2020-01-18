using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

[DataContract]
public class Metadata
{
    [DataMember]
    public string Practitioner { get; set; }

    [DataMember]
    public string Patient { get; set; }

    [DataMember]
    public DateTime DateRecorded { get; set; }

    [DataMember]
    public List<string> Tags { get; set; }

    [DataMember]
    public AudioFile File { get; set; }
}