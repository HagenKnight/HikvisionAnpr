using System.Xml.Serialization;

namespace HikvisionHelpers.Helpers
{
    [XmlRoot(ElementName = "EventNotificationAlert", Namespace = "http://www.isapi.org/ver20/XMLSchema")]
    public class EventNotificationAlert
    {

        [XmlElement(ElementName = "ipAddress")]
        public string IpAddress;

        [XmlElement(ElementName = "ipv6Address")]
        public string Ipv6Address;

        [XmlElement(ElementName = "portNo")]
        public int PortNo;

        [XmlElement(ElementName = "protocol")]
        public string Protocol;

        [XmlElement(ElementName = "macAddress")]
        public string MacAddress;

        [XmlElement(ElementName = "channelID")]
        public int ChannelID;

        [XmlElement(ElementName = "dateTime")]
        public DateTime DateTime;

        [XmlElement(ElementName = "activePostCount")]
        public int ActivePostCount;

        [XmlElement(ElementName = "eventType")]
        public string EventType;

        [XmlElement(ElementName = "eventState")]
        public string EventState;

        [XmlElement(ElementName = "eventDescription")]
        public string EventDescription;

        [XmlElement(ElementName = "channelName")]
        public string ChannelName;

        [XmlElement(ElementName = "ANPR")]
        public ANPR ANPR;

        [XmlElement(ElementName = "UUID")]
        public string UUID;

        [XmlElement(ElementName = "picNum")]
        public int PicNum;

        [XmlAttribute(AttributeName = "version")]
        public double Version;

        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns;

        [XmlText]
        public string Text;
    }

    [XmlRoot(ElementName = "ANPR")]
    public class ANPR
    {

        [XmlElement(ElementName = "licensePlate")]
        public string LicensePlate;

        [XmlElement(ElementName = "line")]
        public int Line;

        [XmlElement(ElementName = "confidenceLevel")]
        public int ConfidenceLevel;

        [XmlElement(ElementName = "plateType")]
        public string PlateType;

        [XmlElement(ElementName = "plateColor")]
        public string PlateColor;

        [XmlElement(ElementName = "licenseBright")]
        public int LicenseBright;

        [XmlElement(ElementName = "pilotsafebelt")]
        public string Pilotsafebelt;

        [XmlElement(ElementName = "vicepilotsafebelt")]
        public string Vicepilotsafebelt;

        [XmlElement(ElementName = "pilotsunvisor")]
        public string Pilotsunvisor;

        [XmlElement(ElementName = "vicepilotsunvisor")]
        public string Vicepilotsunvisor;

        [XmlElement(ElementName = "envprosign")]
        public string Envprosign;

        [XmlElement(ElementName = "dangmark")]
        public string Dangmark;

        [XmlElement(ElementName = "uphone")]
        public string Uphone;

        [XmlElement(ElementName = "pendant")]
        public string Pendant;

        [XmlElement(ElementName = "plateCharBelieve")]
        public string PlateCharBelieve;

        [XmlElement(ElementName = "speedLimit")]
        public int SpeedLimit;

        [XmlElement(ElementName = "illegalInfo")]
        public IllegalInfo IllegalInfo;

        [XmlElement(ElementName = "vehicleType")]
        public string VehicleType;

        [XmlElement(ElementName = "featurePicFileName")]
        public int FeaturePicFileName;

        [XmlElement(ElementName = "detectDir")]
        public int DetectDir;

        [XmlElement(ElementName = "relaLaneDirectionType")]
        public int RelaLaneDirectionType;

        [XmlElement(ElementName = "detectType")]
        public int DetectType;

        [XmlElement(ElementName = "barrierGateCtrlType")]
        public int BarrierGateCtrlType;

        [XmlElement(ElementName = "alarmDataType")]
        public int AlarmDataType;

        [XmlElement(ElementName = "dwIllegalTime")]
        public int DwIllegalTime;

        [XmlElement(ElementName = "vehicleInfo")]
        public VehicleInfo VehicleInfo;

        [XmlElement(ElementName = "pictureInfoList")]
        public PictureInfoList PictureInfoList;

        [XmlElement(ElementName = "originalLicensePlate")]
        public string OriginalLicensePlate;
    }


    [XmlRoot(ElementName = "illegalInfo")]
    public class IllegalInfo
    {

        [XmlElement(ElementName = "illegalCode")]
        public int IllegalCode;
    }

    [XmlRoot(ElementName = "vehicleInfo")]
    public class VehicleInfo
    {

        [XmlElement(ElementName = "index")]
        public int Index;

        [XmlElement(ElementName = "vehicleType")]
        public int VehicleType;

        [XmlElement(ElementName = "colorDepth")]
        public int ColorDepth;

        [XmlElement(ElementName = "color")]
        public string Color;

        [XmlElement(ElementName = "speed")]
        public int Speed;

        [XmlElement(ElementName = "length")]
        public int Length;

        [XmlElement(ElementName = "vehicleLogoRecog")]
        public int VehicleLogoRecog;

        [XmlElement(ElementName = "vehileSubLogoRecog")]
        public int VehileSubLogoRecog;

        [XmlElement(ElementName = "vehileModel")]
        public int VehileModel;
    }

    [XmlRoot(ElementName = "pictureInfoList")]
    public class PictureInfoList
    {

        [XmlElement(ElementName = "pictureInfo")]
        public List<PictureInfo> PictureInfo;
    }

    [XmlRoot(ElementName = "pictureInfo")]
    public class PictureInfo
    {

        [XmlElement(ElementName = "fileName")]
        public string FileName;

        [XmlElement(ElementName = "type")]
        public string Type;

        [XmlElement(ElementName = "dataType")]
        public int DataType;

        [XmlElement(ElementName = "plateRect")]
        public PlateRect PlateRect;
    }

    [XmlRoot(ElementName = "plateRect")]
    public class PlateRect
    {

        [XmlElement(ElementName = "X")]
        public int X;

        [XmlElement(ElementName = "Y")]
        public int Y;

        [XmlElement(ElementName = "width")]
        public int Width;

        [XmlElement(ElementName = "height")]
        public int Height;
    }

}
