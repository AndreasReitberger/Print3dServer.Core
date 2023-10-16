namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IToolhead : IHeaterComponent
    {
        #region Properties
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }

        #endregion
    }
}
