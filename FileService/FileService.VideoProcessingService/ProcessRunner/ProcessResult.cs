namespace FileService.VideoProcessing.ProcessRunner;

public sealed record ProcessResult(
    int ExitCode,
    string StandardOutput,
    string StandardError);
