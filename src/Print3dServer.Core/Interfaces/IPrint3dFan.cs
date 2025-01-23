namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IPrint3dFan
    {
        #region Properties
        public bool On { get; set; }
        public long? Voltage { get; set; }
        public int? Speed { get; }
        public int? Percent { get; }
        #endregion

        #region Methods

        public Task<bool> SetFanSpeedAsync(IPrint3dServerClient client, string command, object? data);

        #endregion
    }
}
