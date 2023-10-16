using AndreasReitberger.API.Print3dServer.Core.Interfaces;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class Print3dBaseEventArgs : EventArgs, IPrint3dBaseEventArgs
    {
        #region Properties
        public string? Message { get; set; }
        public string? Printer { get; set; }
        public long CallbackId { get; set; }
        public string? SessonId { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}
