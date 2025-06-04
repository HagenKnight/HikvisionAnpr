using HikvisionHelpers.Helpers;
using HikvisionHelpers.SDK;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Runtime.InteropServices;

namespace AnprSdk
{
    internal class Program
    {
        static AnprSettings anprSettings = new AnprSettings();

        static string ListeningIp = string.Empty;

        static private Int32[] m_lAlarmHandle = new Int32[200];
        static private Int32 iListenHandle = -1;
        static private int iDeviceNumber = 0; //添加设备个数
        static private uint iLastErr = 0;
        static private string strErr;

        static private CHCNetSDK.MSGCallBack_V31 m_falarmData_V31 = null;
        static private CHCNetSDK.MSGCallBack m_falarmData = null;


        private CHCNetSDK.NET_VCA_TRAVERSE_PLANE m_struTraversePlane = new CHCNetSDK.NET_VCA_TRAVERSE_PLANE();
        private CHCNetSDK.NET_VCA_AREA m_struVcaArea = new CHCNetSDK.NET_VCA_AREA();
        private CHCNetSDK.NET_VCA_INTRUSION m_struIntrusion = new CHCNetSDK.NET_VCA_INTRUSION();
        private CHCNetSDK.UNION_STATFRAME m_struStatFrame = new CHCNetSDK.UNION_STATFRAME();
        private CHCNetSDK.UNION_STATTIME m_struStatTime = new CHCNetSDK.UNION_STATTIME();

        static void Main(string[] args)
        {
            SystemConfig();

            #region Hikvision SDK

            bool m_bInitSDK = CHCNetSDK.NET_DVR_Init();
            if (!m_bInitSDK)
            {
                Log.Error("NET_DVR_Init error!");
                return;
            }

            byte[] strIP = new byte[16 * 16];
            uint dwValidNum = 0;
            Boolean bEnableBind = false;

            //获取本地PC网卡IP信息
            if (CHCNetSDK.NET_DVR_GetLocalIP(strIP, ref dwValidNum, ref bEnableBind))
            {
                if (dwValidNum > 0)
                {
                    //取第一张网卡的IP地址为默认监听端口
                    ListeningIp = System.Text.Encoding.UTF8.GetString(strIP, 0, 16);
                    Log.Information($"Listening IP: {ListeningIp}");
                    CHCNetSDK.NET_DVR_SetValidIP(0, true); //绑定第一张网卡
                }
            }

            // 保存SDK日志 - To save the SDK log
            CHCNetSDK.NET_DVR_SetLogToFile(3, anprSettings.Path, true);
            for (int i = 0; i < 200; i++)
            {
                m_lAlarmHandle[i] = -1;
            }

            // 设置报警回调函数 - Setting the alarm callback function
            if (m_falarmData_V31 == null)
                m_falarmData_V31 = new CHCNetSDK.MSGCallBack_V31(MsgCallback_V31);

            CHCNetSDK.NET_DVR_SetDVRMessageCallBack_V31(m_falarmData_V31, IntPtr.Zero);

            while (true)
            {
                Log.Information("Waiting for device connection...");
                // 连接设备 - Connect to the device
                CHCNetSDK.NET_DVR_DEVICEINFO_V30 deviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();

                Log.Information($"Attempting connect to {anprSettings.Device.IpAddress}:{anprSettings.Device.Port}...");
                int lUserID = CHCNetSDK.NET_DVR_Login_V30(anprSettings.Device.IpAddress, anprSettings.Device.Port, anprSettings.Credentials.User, anprSettings.Credentials.Password, ref deviceInfo);

                if (lUserID < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    strErr = $"Login failed, error code: {Convert.ToString(iLastErr, 16)}";
                    Log.Error(strErr);
                    //NET_DVR_Cleanup();
                    Thread.Sleep(1000);
                    continue;
                }
                Log.Information($"Device connected successfully, User ID: {lUserID}");
                // 设置报警回调函数 - Set the alarm callback function
                if (!CHCNetSDK.NET_DVR_SetDVRMessageCallBack_V31(m_falarmData_V31, IntPtr.Zero))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    strErr = $"SetDVRMessageCallBack_V31 failed, error code: {iLastErr}";
                    Log.Error(strErr);
                    continue;
                }
                //// 开始监听报警信息 - Start listening for alarm information
                //iListenHandle = CHCNetSDK.NET_DVR_StartListen_V30(ListeningIp, anprSettings.Server.Port);
                //if (iListenHandle < 0)
                //{
                //    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                //    strErr = $"StartListen_V30 failed, error code: {iLastErr}";
                //    Log.Error(strErr);
                //    continue;
                //}
                //Log.Information($"Listening started successfully on IP: {ListeningIp}, Port: {anprSettings.Server.Port}");
                // 等待报警信息 - Wait for alarm information
                while (true)
                {
                    Thread.Sleep(1000); // Sleep to avoid busy waiting
                }
            }

            #endregion
        }



        // 接收到报警信息 - Received alarm information
        public static bool MsgCallback_V31(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            //通过lCommand来判断接收到的报警信息类型，不同的lCommand对应不同的pAlarmInfo内容
            AlarmMessageHandle(lCommand, ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
            return true; //回调函数需要有返回，表示正常接收到数据
        }

        // 处理报警信息 - Handle alarm information
        public static void AlarmMessageHandle(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            Log.Information($"Received alarm command: {lCommand}");

            //通过lCommand来判断接收到的报警信息类型，不同的lCommand对应不同的pAlarmInfo内容

            switch (lCommand)
            {
                case CHCNetSDK.COMM_ALARM: //(DS-8000老设备)移动侦测、视频丢失、遮挡、IO信号量等报警信息
                                           // ProcessCommAlarm(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;

                case CHCNetSDK.COMM_ALARM_V30://移动侦测、视频丢失、遮挡、IO信号量等报警信息
                    //ProcessCommAlarm_V30(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;

                case CHCNetSDK.COMM_ALARM_RULE://进出区域、入侵、徘徊、人员聚集等异常行为检测报警信息
                                               // ProcessCommAlarm_RULE(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;

                case CHCNetSDK.COMM_UPLOAD_PLATE_RESULT://交通抓拍结果上传(老报警信息类型)
                    //ProcessCommAlarm_Plate(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;

                case CHCNetSDK.COMM_ITS_PLATE_RESULT://交通抓拍结果上传(新报警信息类型)
                    ProcessCommAlarm_ITSPlate(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;

                case CHCNetSDK.COMM_ALARM_PDC://客流量统计报警信息
                    //ProcessCommAlarm_PDC(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;

                case CHCNetSDK.COMM_ITS_PARK_VEHICLE://客流量统计报警信息
                    //ProcessCommAlarm_PARK(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;

                case CHCNetSDK.COMM_DIAGNOSIS_UPLOAD://VQD报警信息
                    //ProcessCommAlarm_VQD(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;

                case CHCNetSDK.COMM_UPLOAD_FACESNAP_RESULT://人脸抓拍结果信息
                    //ProcessCommAlarm_FaceSnap(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;

                case CHCNetSDK.COMM_SNAP_MATCH_ALARM://人脸比对结果信息
                    //ProcessCommAlarm_FaceMatch(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;

                case CHCNetSDK.COMM_ALARM_FACE_DETECTION://人脸侦测报警信息
                    //ProcessCommAlarm_FaceDetect(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;

                case CHCNetSDK.COMM_ALARMHOST_CID_ALARM://报警主机CID报警上传
                    //ProcessCommAlarm_CIDAlarm(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;

                case CHCNetSDK.COMM_ALARM_ACS://门禁主机报警上传
                    //ProcessCommAlarm_AcsAlarm(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;

                case CHCNetSDK.COMM_ID_INFO_ALARM://身份证刷卡信息上传
                    //ProcessCommAlarm_IDInfoAlarm(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;

                default:
                    {
                        //报警设备IP地址
                        string strIP = pAlarmer.sDeviceIP;

                        //报警信息类型
                        string stringAlarm = "upload alarm，alarm message type：" + lCommand;

                        //if (InvokeRequired)
                        //{
                        //    object[] paras = new object[3];
                        //    paras[0] = DateTime.Now.ToString(); //当前PC系统时间
                        //    paras[1] = strIP;
                        //    paras[2] = stringAlarm;
                        //    listViewAlarmInfo.BeginInvoke(new UpdateListBoxCallback(UpdateClientList), paras);

                        //}
                        //else
                        //{
                        //    //创建该控件的主线程直接更新信息列表 
                        //    UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm);
                        //}
                    }
                    break;

            }



        }


        private static void ProcessCommAlarm_ITSPlate(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            CHCNetSDK.NET_ITS_PLATE_RESULT struITSPlateResult = new CHCNetSDK.NET_ITS_PLATE_RESULT();
            uint dwSize = (uint)Marshal.SizeOf(struITSPlateResult);
            struITSPlateResult = (CHCNetSDK.NET_ITS_PLATE_RESULT)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_ITS_PLATE_RESULT));

            //保存抓拍图片
            for (int i = 0; i < struITSPlateResult.dwPicNum; i++)
            {
                if (struITSPlateResult.struPicInfo[i].dwDataLen != 0)
                {
                    string str = "ITS_UserID_[" + pAlarmer.lUserID + "]_Pictype_" + struITSPlateResult.struPicInfo[i].byType + "_Num" + (i + 1) + ".jpg";
                    FileStream fs = new FileStream(str, FileMode.Create);
                    int iLen = (int)struITSPlateResult.struPicInfo[i].dwDataLen;
                    byte[] by = new byte[iLen];
                    Marshal.Copy(struITSPlateResult.struPicInfo[i].pBuffer, by, 0, iLen);
                    fs.Write(by, 0, iLen);
                    fs.Close();
                }
            }

            //报警设备IP地址
            string strIP = pAlarmer.sDeviceIP;

            //抓拍时间：年月日时分秒
            string strTimeYear = string.Format("{0:D4}", struITSPlateResult.struSnapFirstPicTime.wYear) +
                string.Format("{0:D2}", struITSPlateResult.struSnapFirstPicTime.byMonth) +
                string.Format("{0:D2}", struITSPlateResult.struSnapFirstPicTime.byDay) + " "
                + string.Format("{0:D2}", struITSPlateResult.struSnapFirstPicTime.byHour) + ":"
                + string.Format("{0:D2}", struITSPlateResult.struSnapFirstPicTime.byMinute) + ":"
                + string.Format("{0:D2}", struITSPlateResult.struSnapFirstPicTime.bySecond) + ":"
                + string.Format("{0:D3}", struITSPlateResult.struSnapFirstPicTime.wMilliSec);

            //上传结果
            string stringPlateLicense = System.Text.Encoding.GetEncoding("GBK").GetString(struITSPlateResult.struPlateInfo.sLicense).TrimEnd('\0');
            string stringAlarm = "capture upload，" + "license plate：" + stringPlateLicense + "，Serial number of vehicle：" + struITSPlateResult.struVehicleInfo.dwIndex;
            //if (InvokeRequired)
            //{
            //    object[] paras = new object[3];
            //    paras[0] = strTimeYear;//当前系统时间为：DateTime.Now.ToString();
            //    paras[1] = strIP;
            //    paras[2] = stringAlarm;
            //    listViewAlarmInfo.BeginInvoke(new UpdateListBoxCallback(UpdateClientList), paras);
            //}
            //else
            //{
            //    //创建该控件的主线程直接更新信息列表 
            //    UpdateClientList(DateTime.Now.ToString(), strIP, stringAlarm);
            //}
        }

        private static void SystemConfig()
        {
            var config = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .Build();

            Log.Logger = new LoggerConfiguration()
               .ReadFrom.Configuration(config)
               .Enrich.FromLogContext() //Adds more information to our logs from built in Serilog 
                                        //.WriteTo.Console()
               .CreateLogger();


            // Ensure the Microsoft.Extensions.Configuration.Binder package is installed
            anprSettings = config.GetSection("AnprSettings").Get<AnprSettings>();
        }
    }
}
