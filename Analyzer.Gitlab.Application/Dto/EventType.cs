using Elasticsearch.Net;
using System;
using System.Runtime.Serialization;

namespace Analyzer.Git.Application.Dto.GitLab
{
    [Serializable]
    [StringEnum]
    public enum EventType
    {
        [EnumMember(Value = "opened")]
        Opened,
        [EnumMember(Value = "merged")]
        Merged,
        [EnumMember(Value = "comment")]
        Comment
    }
}
