using AndreasReitberger.API.Print3dServer.Core.Enums;

namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IPrint3dJobStatus : IPrint3dBase
    {
        #region Properties
        public string JobId { get; set; }
        public double? StartTime { get; set; }
        public double? EndTime { get; set; }
        public double? FilamentUsed { get; set; }
        public double? PrintDuration { get; set; }
        public double? TotalPrintDuration { get; set; }
        public string FileName { get; set; }
        public bool FileExists { get; set; }
        public Print3dJobState State { get; set; }
        public IGcodeMeta Meta { get; set; }
        #endregion
    }
}
