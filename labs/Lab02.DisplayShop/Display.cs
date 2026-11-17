using System.Globalization;
using Lab02.DisplayShop.Logging;

namespace Lab02.DisplayShop;

public sealed class Display
{
    private readonly Logger logger;
    public string Model { get; }
    public int Width { get; }
    public int Height { get; }
    public float Ppi { get; }
    public double DiagonalInches => Math.Sqrt((double)Width * Width + (double)Height * Height) / Ppi;

    public Display(string model, int width, int height, float ppi, Logger? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(model);
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width), "Width must be positive.");
        if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height), "Height must be positive.");
        if (!float.IsFinite(ppi) || ppi <= 0)
            throw new ArgumentOutOfRangeException(nameof(ppi), "PPI must be positive and finite.");
        Model = model; Width = width; Height = height; Ppi = ppi;
        this.logger = logger ?? new Logger("Display");
        this.logger.Trace("Display % created (%, % ppi).", Model, SizeText(), Ppi);
    }

    public int CompareSize(Display other)
    {
        ArgumentNullException.ThrowIfNull(other);
        double difference = DiagonalInches - other.DiagonalInches;
        int comparison = Math.Abs(difference) < 0.05 ? 0 : Math.Sign(difference);
        string relation = comparison > 0 ? "bigger than" : comparison < 0 ? "smaller than" : "about the same size as";
        logger.Info("% (%) is % % (%).", Model, SizeText(), relation, other.Model, other.SizeText());
        return comparison;
    }

    public int CompareSharpness(Display other)
    {
        ArgumentNullException.ThrowIfNull(other);
        int comparison = Ppi.CompareTo(other.Ppi);
        string relation = comparison > 0 ? "sharper than" : comparison < 0 ? "less sharp than" : "as sharp as";
        logger.Info("% (% ppi) is % % (% ppi).", Model, Ppi, relation, other.Model, other.Ppi);
        return comparison;
    }

    public string CompareWithMonitor(Display other)
    {
        int size = CompareSize(other);
        int sharpness = CompareSharpness(other);
        string verdict = size == 0 && sharpness == 0 ? "equivalent size and sharpness"
            : size >= 0 && sharpness >= 0 ? $"{Model} is at least as large and sharp"
            : size <= 0 && sharpness <= 0 ? $"{other.Model} is at least as large and sharp"
            : "trade-off: one screen is larger, the other is sharper";
        logger.Info("Comparison % vs %: %.", Model, other.Model, verdict);
        return verdict;
    }

    private string SizeText() => DiagonalInches.ToString("F1", CultureInfo.InvariantCulture) + "\"";
}
