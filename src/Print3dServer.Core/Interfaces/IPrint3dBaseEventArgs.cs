namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IPrint3dBaseEventArgs
    {
        #region Properties
        public string? Message { get; set; }
        public string? Printer { get; set; }
        public long CallbackId { get; set; }
        public string? SessonId { get; set; }
        public string? AuthToken { get; set; }
        #endregion
    }
}
