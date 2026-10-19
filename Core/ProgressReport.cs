namespace ConsolidadorLogs.Core
{
    public class ProgressReport
    {
        public int Percentual { get; set; }
        public string Status { get; set; } = string.Empty;

        public ProgressReport(int percentual, string status)
        {
            Percentual = percentual;
            Status = status;
        }
    }
}