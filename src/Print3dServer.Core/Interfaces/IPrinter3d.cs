﻿namespace AndreasReitberger.API.Print3dServer.Core.Interfaces
{
    public interface IPrinter3d
    {
        #region Properties
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public bool IsActive { get; set; }
        public bool IsSelected { get; set; }
        public bool IsOnline { get; set; }
        public bool IsPaused { get; set; }
        public bool IsPrinting { get; set; }
        public string ActiveJobId { get; set; }
        public string ActiveJobName { get; set;}
        public string? ActiveJobState { get; set; }
        public long? LineSent { get; set; }
        public long? Layers { get; set; }
        public long? PauseState { get; set; }
        public long? Start { get; set; }
        public long? TotalLines { get; set; }
        public double? PrintStarted { get; set; }
        public double? PrintDuration { get; set; }
        public double? PrintDudationEstimated { get; set; }
        public double? Extruder1Temperature { get; set; }
        public double? Extruder2Temperature { get; set; }
        public double? Extruder3Temperature { get; set; }
        public double? Extruder4Temperature { get; set; }
        public double? Extruder5Temperature { get; set; }
        public double? HeatedBedTemperature { get; set; }
        public double? HeatedChamberTemperature { get; set; }
        public double? PrintProgress { get; set; }
        public double? RemainingPrintDuration { get; set; }
        public byte[] CurrentPrintImage { get; set; }
        public int? Repeat { get; set; }

        #endregion
    }
}
