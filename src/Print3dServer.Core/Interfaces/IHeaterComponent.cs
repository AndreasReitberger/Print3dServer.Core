namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IHeaterComponent
    {
        #region Properties

        public Guid Id { get; set; }
        public string Name { get; set; }
        public double TempRead { get; set; }
        public double TempSet { get; set; }
        public long Error { get; set; }

        #endregion
    }
}
