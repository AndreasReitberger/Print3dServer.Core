using AndreasReitberger.API.Print3dServer.Core.Interfaces;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class JobStartedEventArgs : Print3dBaseEventArgs, IJobStartedEventArgs
    {
        #region Properties
        public IPrint3dJob? Job { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}
