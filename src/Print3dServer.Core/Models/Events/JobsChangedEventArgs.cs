using AndreasReitberger.API.Print3dServer.Core.Interfaces;
using Newtonsoft.Json;

namespace AndreasReitberger.API.Print3dServer.Core.Events
{
    public partial class JobsChangedEventArgs : Print3dBaseEventArgs, IJobsChangedEventArgs
    {
        #region Properties
        public IPrint3dJob? NewJob { get; set; }
        public IPrint3dJob? OldJob { get; set; }
        #endregion

        #region Overrides
        public override string ToString() => JsonConvert.SerializeObject(this, Formatting.Indented);
        #endregion
    }
}
