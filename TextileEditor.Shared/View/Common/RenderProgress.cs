namespace TextileEditor.Shared.View.Common;
/// <summary>
/// Represents the progress of a rendering process.
/// </summary>
public readonly struct RenderProgress
{
    /// <summary>
    /// The current phase of the rendering process.
    /// </summary>
    public int Phase { get; init; }

    /// <summary>
    /// The total number of phases in the rendering process.
    /// </summary>
    public int MaxPhase { get; init; }

    /// <summary>
    /// The current step within the current phase.
    /// </summary>
    public int Step { get; init; }

    /// <summary>
    /// The total number of steps within the current phase.
    /// </summary>
    public int MaxStep { get; init; }

    /// <summary>
    /// The current state of the rendering process.
    /// </summary>
    public RenderProgressStates Status { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderProgress"/> struct.
    /// </summary>
    public RenderProgress(int phase, int maxPhase, int step, int maxStep, RenderProgressStates status)
    {
        Phase = phase;
        MaxPhase = maxPhase;
        Step = step;
        MaxStep = maxStep;
        Status = status;
    }

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
/// States representing the rendering progress.
/// </summary>
public enum RenderProgressStates
{
    NotStarted,
    Initializing,
    LoadingAssets,
    Rendering,
    PostProcessing,
    Finalizing,
    Completed,
    Failed,
    Canceled
}
