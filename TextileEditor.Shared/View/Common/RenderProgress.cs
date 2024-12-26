namespace TextileEditor.Shared.View.Common;
/// <summary>
/// Represents the progress of a rendering process.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="RenderProgress"/> struct.
/// </remarks>
public readonly struct RenderProgress(int phase, int maxPhase, int step, int maxStep, RenderProgressStates status)
{
    /// <summary>
    /// The current phase of the rendering process.
    /// </summary>
    public int Phase { get; init; } = phase;

    /// <summary>
    /// The total number of phases in the rendering process.
    /// </summary>
    public int MaxPhase { get; init; } = maxPhase;

    /// <summary>
    /// The current step within the current phase.
    /// </summary>
    public int Step { get; init; } = step;

    /// <summary>
    /// The total number of steps within the current phase.
    /// </summary>
    public int MaxStep { get; init; } = maxStep;

    /// <summary>
    /// The current state of the rendering process.
    /// </summary>
    public RenderProgressStates Status { get; init; } = status;

    /// <summary>
    /// The percentage progress of the rendering process, dynamically calculated.
    /// </summary>
    public double Progress
    {
        get
        {
            if (MaxPhase == 0) return 0.0; // Avoid division by zero
            if (MaxStep == 0) return Phase / (double)MaxPhase;

            double completedPhaseProgress = (Phase - 1) / (double)MaxPhase;
            double currentPhaseProgress = (Step / (double)MaxStep) / MaxPhase;

            return Math.Min(1.0, completedPhaseProgress + currentPhaseProgress); // Clamp to 100%
        }
    }

    /// <summary>
    /// Returns a string representation of the current progress.
    /// </summary>
    public override string ToString()
    {
        return $"Phase: {Phase}/{MaxPhase}, Step: {Step}/{MaxStep}, Progress: {Progress:P2}, Status: {Status}";
    }
}

/// <summary>
/// States representing the progress of a rendering process.
/// </summary>
public enum RenderProgressStates
{
    /// <summary>
    /// The rendering process has not started yet.
    /// </summary>
    NotStarted,

    /// <summary>
    /// The rendering process is in the initialization stage.
    /// </summary>
    Initializing,

    /// <summary>
    /// The rendering process is initialized and ready to start.
    /// </summary>
    Ready,

    /// <summary>
    /// The rendering process is currently in progress.
    /// </summary>
    Processing,

    /// <summary>
    /// The rendering process has completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// The rendering process encountered an error and failed.
    /// </summary>
    Failed,

    /// <summary>
    /// The rendering process was canceled before completion.
    /// </summary>
    Canceled
}
