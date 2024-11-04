using AndreasReitberger.API.Print3dServer.Core.Enums;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IPrint3dJobStatus : IPrint3dBase
    {
        #region Properties
        public string JobId { get; set; }
        public double? Done { get; set; }
        public double? DonePercentage { get; set; }
        public long? Repeat { get; set; }
        public double? StartTime { get; set; }
        public DateTime? StartTimeGeneralized { get; set; }
        public double? EndTime { get; set; }
        public DateTime? EndTimeGeneralized { get; set; }
        public double? FilamentUsed { get; set; }
        public double? RemainingPrintTime { get; set; }
        public TimeSpan? RemainingPrintTimeGeneralized { get; set; }
        public double? PrintDuration { get; set; }
        public TimeSpan? PrintDurationGeneralized { get; set; }
        public double? TotalPrintDuration { get; set; }
        public TimeSpan? TotalPrintDurationGeneralized { get; set; }
        public string FileName { get; set; }
        public bool FileExists { get; set; }
        public Print3dJobState? State { get; set; }
        public IGcodeMeta? Meta { get; set; }
        #endregion
    }
}
