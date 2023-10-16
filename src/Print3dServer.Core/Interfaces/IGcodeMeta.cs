namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IGcodeMeta : IPrint3dBase
    {
        #region Properties
        public string JobId { get; set; }
        public string FileName { get; set; }
        public double? PrintStartTime { get; set; }
        public long FileSize { get; set; }
        public double Modified { get;set; }
        public string Slicer { get; set; }
        public string SlicerVersion { get; set; }
        public double LayerHeight { get; set; }
        public double FirstLayerHeight { get; set; }
        public double ObjectHeight { get; set; }
        public double FilamentTotal { get; set; }
        public double FilamentWeightTotal { get; set; }
        public double EstimatedPrintTime { get; set; }
        public double FirstLayerExtrTemp { get; set; }
        public double FirstLayerBedTemp { get; set; }
        public double Layers { get; }
        #endregion

        #region Collections
        IList<IGcodeImage> GcodeImages { get; set; }
        #endregion
    }
}
