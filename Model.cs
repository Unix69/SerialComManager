using MongoDB.Bson;
using System;
using System.Collections.Generic;

namespace SerialCommunicationManager
{

    public class Plant
    {
        public Object _id { get; set; }
        public string CustomerName { get; set; }
        /// <summary>
        /// SPEA SIA Customer Plant Code
        /// </summary>
        public string PlantCode { get; set; }
        public List<Equipment> Equipments;

        public Plant()
        {
            Equipments = new List<Equipment>();
        }
    }

    public class Equipment
    {
        public ObjectId _id { get; set; }
        public string SerialNumber { get; set; }
        public string Model { get; set; }
        public string Family { get; set; }

        /// <summary>
        /// List of Racks inside the Equipment
        /// </summary>
        public List<Rack> Racks;

        /// <summary>
        /// List of sensors inside the Equipment to get environment information
        /// </summary>
        public List<Sensor> Sensors;

        public Equipment()
        {
            this._id = ObjectId.GenerateNewId();
            this.Racks = new List<Rack>();
            this.Sensors = new List<Sensor>();
        }
    }

    public class Rack
    {
        public ObjectId _id { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }

        /// <summary>
        /// The List of sensors inside the Rack
        /// </summary>
        public List<Sensor> Sensors;
        /// <summary>
        /// (The Underscore is used just for schema semplicity)
        /// </summary>
        public List<Unit> Units;

        public Rack()
        {
            this._id = ObjectId.GenerateNewId();
            this.Units = new List<Unit>();
            this.Sensors = new List<Sensor>();
        }

    }

    public class Unit
    {
        public ObjectId _id { get; set; }
        public string SerialNumber { get; set; }
        //define type of object (mechanic, electric)
        public int Type { get; set; }
        public int SlotNumber { get; set; }
        public string PartNumber { get; set; }
        public string ElectricalLevel { get; set; }
        public string MechanicalLevel { get; set; }
        public DateTime LastCalibrationDate { get; set; }

        public List<Sensor> Sensors;
        public Unit()
        {
            this._id = ObjectId.GenerateNewId();
            this.Sensors = new List<Sensor>();
        }
    }

    public class Sensor
    {
        public Object _id { get; set; }
        public string SerialNumber { get; set; }

        public string IPAddress { get; set; }
        public string CanAddress { get; set; }
        public string Vendor { get; set; }
        public string PartNumber { get; set; }
        public string Type { get; set; }

        public Sensor() {
            this._id = ObjectId.GenerateNewId();
        }

    }



    /// <summary>
    /// Measure given by sensor or TPGM
    /// </summary>
    public class Measure
    {
        public ObjectId _id { get; set; }

        /// <summary>
        /// Reference to the agent that get the measure (Sensor ID, Unit ID, RackID, Equipment ID, Equipment SN...)
        /// </summary>
        public string ReferenceID { get; set; }

        /// <summary>
        /// Sensor, Unit, Rack, Equipment...
        /// </summary>
        public string ReferenceType { get; set; }


        /// <summary>
        /// MeasureID: In case of multiple measure per source (Example: MultiSensor Device, ABB Network Analyzer, SPEA SM100, Test Num in the TPGM...)
        /// Not Used anymore. Multiple sensor sends multiple Measure objects
        /// </summary>
        //public Object MeasureID { get; set; }


        public DateTime Timestamp { get; set; }
        public string MeasureUnit { get; set; }

        /// <summary>
        /// Temperature, Humidity, Relay Count, CDCOLL Measure ANL, CDCOLL Measure FUNC
        /// </summary>
        public string MeasureType { get; set; }

        /// <summary>
        /// Optional. Reference to the component tested inside a DUT, a Unit, 
        /// </summary>
        public string DrawReference { get; set; }

        /// <summary>
        /// Optional
        /// </summary>
        public string RelayProbeType { get; set; }

        /// <summary>
        /// Optional. Used for relay switch counter
        /// </summary>
        public int StressIndex { get; set; }

        /// <summary>
        /// Measure Value
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// A value greater than the High Limit is a fail
        /// </summary>
        public double HighLimit { get; set; }

        /// <summary>
        /// A value lower than the Low Limit is a fail
        /// </summary>
        public double LowLimit { get; set; }


        /// <summary>
        /// Result of the measure: PASS, FAIL, FAIL(-), FAIL (+). If applicable
        /// </summary>        
        public string Result { get; set; }

        /// <summary>
        /// If applicable
        /// </summary>
        public string TPGMName { get; set; }

        /// <summary>
        /// Product/DUT SN. Applicable just in case of TPGM related to a DUT. Not applicable for self diagnostic TPGM
        /// Populated only for DUT TPGM
        /// </summary>
        public string ProductSN { get; set; }

        /// <summary>
        /// Optional. Task Id on the TPGM (if applicable)
        /// </summary>
        public string Task { get; set; }


        /// <summary>
        /// Optional. Test Id on the TPGM (if applicable)
        /// </summary>
        public string Test { get; set; }

        /// <summary>
        /// Optional. TP Id on the TPGM (if applicable)
        /// </summary>
        public string TestPoint { get; set; }

        /// <summary>
        /// Optional. Test Notes written by the author of the TPGM
        /// </summary>
        public string Remark { get; set; }


        public Measure() {
            this._id = ObjectId.GenerateNewId();

        }

    }

    public class Alarm
    {
        public ObjectId _id { get; set; }

        /// <summary>
        /// Event identification (could be a code or an integer)
        /// </summary>
        public string AlarmId { get; set; }
        /// <summary>
        /// INFO, WARNING, ERROR, FATAL
        /// </summary>
        public int AlarmLevel { get; set; }
        /// <summary>
        /// Reference to the Equipment ID
        /// </summary>
        public string EquipmentID { get; set; }
        public DateTime Timestamp { get; set; }
        public string Remark { get; set; }
    }

    /// <summary>
    /// Equipment event such On, Off, Tpgm load, Tpgm end, Start, Stop, ...
    /// </summary>
    public class Event
    {

        public ObjectId _id { get; set; }

        /// <summary>
        /// Event identification (could be a code or an integer)
        /// </summary>
        public UInt64 EventId { get; set; }

        /// <summary>
        /// Reference to the Equipment ID
        /// </summary>
        public string EquipmentID { get; set; }
        public DateTime Timestamp { get; set; }
        public string Remark { get; set; }


        public Event() {
            this._id = ObjectId.GenerateNewId();
        }
    }

    /// <summary>
    /// Just for the schema
    /// </summary>
    class TPGM
    {
        public ObjectId _id { get; set; }

        public string TpgmName { get; set; }

        /// <summary>
        /// ID of the Unit, Rack, or Equipment that the TGPM is testing
        /// </summary>
        public Object ReferenceID { get; set; }

        
    }

    /// <summary>
    /// Just for the schema
    /// </summary>
    class DataLogLoader
    {
        public ObjectId _id { get; set; }
        public string FileName { get; set; }

        public Object ReferenceID { get; set; }

       
    }







}
