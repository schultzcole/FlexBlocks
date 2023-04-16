using System.Globalization;
using BenchmarkDotNet.Analysers;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Filters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Validators;

namespace Benchmarks;

public class Config : IConfig
{
    private readonly IConfig _default = DefaultConfig.Instance;

    public IEnumerable<IExporter> GetExporters()
    {
        yield return HtmlExporter.Default;
    }

    public IEnumerable<IDiagnoser> GetDiagnosers()
    {
        yield return MemoryDiagnoser.Default;
    }

    public IEnumerable<IColumnProvider> GetColumnProviders() => _default.GetColumnProviders();
    public IEnumerable<ILogger> GetLoggers() => _default.GetLoggers();
    public IEnumerable<IAnalyser> GetAnalysers() => _default.GetAnalysers();
    public IEnumerable<Job> GetJobs() => _default.GetJobs();
    public IEnumerable<IValidator> GetValidators() => _default.GetValidators();
    public IEnumerable<HardwareCounter> GetHardwareCounters() => _default.GetHardwareCounters();
    public IEnumerable<IFilter> GetFilters() => _default.GetFilters();
    public IEnumerable<BenchmarkLogicalGroupRule> GetLogicalGroupRules() => _default.GetLogicalGroupRules();
    public IEnumerable<IColumnHidingRule> GetColumnHidingRules() => _default.GetColumnHidingRules();
    public IOrderer? Orderer => _default.Orderer;
    public SummaryStyle SummaryStyle => _default.SummaryStyle;
    public ConfigUnionRule UnionRule => _default.UnionRule;
    public string ArtifactsPath => _default.ArtifactsPath;
    public CultureInfo? CultureInfo => _default.CultureInfo;
    public ConfigOptions Options => _default.Options;
    public TimeSpan BuildTimeout => _default.BuildTimeout;
    public IReadOnlyList<Conclusion> ConfigAnalysisConclusion => _default.ConfigAnalysisConclusion;
}
