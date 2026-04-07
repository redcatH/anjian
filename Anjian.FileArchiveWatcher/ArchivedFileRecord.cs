using System;

namespace Anjian;

public sealed record ArchivedFileRecord(
    int SequenceNumber,
    string SourcePath,
    string ArchivedPath,
    string Md5,
    DateTime ArchivedAt,
    string Message);
