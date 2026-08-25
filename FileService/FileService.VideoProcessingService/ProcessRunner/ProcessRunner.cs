using System.Diagnostics;
using System.Text;
using CSharpFunctionalExtensions;
using FileService.Domain;
using Microsoft.Extensions.Logging;
using Shared.Kernel;

namespace FileService.VideoProcessing.ProcessRunner;

public sealed class ProcessRunner : IProcessRunner
{
    private readonly ILogger<ProcessRunner> _logger;

    public ProcessRunner(ILogger<ProcessRunner> logger)
    {
        _logger = logger;
    }

    public async Task<Result<ProcessResult, Error>> RunAsync(
        ProcessCommand command,
        Action<string>? onOutput = null,
        CancellationToken cancellationToken = default)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = command.ExecutableFile,
                Arguments = command.NormalizedArguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (_, args) =>
        {
            if (args.Data is null)
                return;

            outputBuilder.AppendLine(args.Data);
            onOutput?.Invoke(args.Data);
        };

        process.ErrorDataReceived += (_, args) =>
        {
            if (args.Data is null)
                return;

            errorBuilder.AppendLine(args.Data);
            onOutput?.Invoke(args.Data);
        };

        var started = false;

        try
        {
            started = process.Start();
            if (!started)
                return FileErrors.ProcessFailed();

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            if (started && !process.HasExited)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(
                        exception,
                        "Failed to stop cancelled process {FileName}",
                        command.ExecutableFile);
                }
            }

            _logger.LogWarning(
                "Process was cancelled: {FileName} {Arguments}",
                command.ExecutableFile,
                command.Arguments);

            return FileErrors.OperationCanceled();
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to start or execute process: {FileName} {Arguments}",
                command.ExecutableFile,
                command.Arguments);

            return FileErrors.ProcessFailed();
        }

        var result = new ProcessResult(
            process.ExitCode,
            outputBuilder.ToString(),
            errorBuilder.ToString());

        if (result.ExitCode != 0)
        {
            _logger.LogError(
                "Process failed: {FileName} {Arguments} ExitCode: {ExitCode} Error: {Error}",
                command.ExecutableFile,
                command.Arguments,
                result.ExitCode,
                result.StandardError);

            return FileErrors.ProcessFailed();
        }

        return result;
    }
}
