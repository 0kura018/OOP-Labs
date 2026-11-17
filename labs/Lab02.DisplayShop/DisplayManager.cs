using Lab02.DisplayShop.Logging;

namespace Lab02.DisplayShop;

public sealed class DisplayManager
{
    private readonly List<Display> assignedDisplays = [];
    private readonly Logger logger;
    public string Name { get; }
    public IReadOnlyList<Display> AssignedDisplays => assignedDisplays.AsReadOnly();

    public DisplayManager(string name, Logger? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        this.logger = logger ?? new Logger("Manager");
        this.logger.Trace("Manager % created.", Name);
    }

    public bool AssignDisplay(Display display)
    {
        ArgumentNullException.ThrowIfNull(display);
        if (assignedDisplays.Contains(display))
        {
            logger.Warn("% already has display %; duplicate assignment ignored.", Name, display.Model);
            return false;
        }
        assignedDisplays.Add(display);
        logger.Trace("Display % assigned to %.", display.Model, Name);
        return true;
    }

    public void Assist()
    {
        if (assignedDisplays.Count < 2)
        {
            logger.Warn("Manager % has % display(s); at least two are needed for comparison.", Name, assignedDisplays.Count);
            return;
        }
        for (int i = 0; i + 1 < assignedDisplays.Count; i++)
            assignedDisplays[i].CompareWithMonitor(assignedDisplays[i + 1]);
    }

    public Display BuyDisplay(Display display)
    {
        ArgumentNullException.ThrowIfNull(display);
        if (!assignedDisplays.Remove(display))
            throw new InvalidOperationException($"Display {display.Model} is not assigned to manager {Name}.");
        logger.Info("Manager % sold %.", Name, display.Model);
        if (assignedDisplays.Count == 0) logger.Warn("Manager % has no displays left.", Name);
        return display; // Remove only this manager's reference. Other managers keep theirs.
    }
}
