using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KMLServer.Models
{
    public class HydrantModel
    {
        public string type { get; set; }
        public string name { get; set; }
        public Crs crs { get; set; }
        public Feature[] features { get; set; }
    }

    public class Crs
    {
        public string type { get; set; }
        public Properties properties { get; set; }
    }

    public class Properties
    {
        public string name { get; set; }
    }

    public class Feature
    {
        public string type { get; set; }
        public Properties1 properties { get; set; }
        public Geometry geometry { get; set; }
    }

    public class Properties1
    {
        public int OBJECTID { get; set; }
        public string LOCATION { get; set; }
        public string GATE_VALVE { get; set; }
        public string TEST_DATE { get; set; }
        public float OUTLET_SIZ { get; set; }
        public float PITOT { get; set; }
        public float STATIC { get; set; }
        public float RESIDUAL { get; set; }
        public float FLOW_OBSER { get; set; }
        public float FLOW_AT_20 { get; set; }
        public string INSTALL_DA { get; set; }
        public string HYDRANT_NU { get; set; }
        public string MAIN_SIZE { get; set; }
    }

    public class Geometry
    {
        public string type { get; set; }
        public float[] coordinates { get; set; }
    }

}
