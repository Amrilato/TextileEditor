namespace TextileEditor.Shared.Services;

public readonly record struct BackgroundTaskProgress(int Step, int Max, string Description);
