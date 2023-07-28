namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IPrint3dJob
    {
        #region Properties
        public Guid Id { get; set; }
        public string JobId { get; set; }
        public double? TimeAdded { get; set; }
        public double? TimeInQueue { get; set; }
        public string FileName { get; set; }
        #endregion
    }
}
