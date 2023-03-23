﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using Microsoft.VisualBasic;
using Avago.ATF.StandardLibrary;
using Ivi.Visa.Interop;
using LibEqmtDriver;
using NationalInstruments.ModularInstruments.NIRfsg;
using NationalInstruments.ModularInstruments.NIRfsa;
using NationalInstruments.ModularInstruments.SystemServices.DeviceServices;
//using ni_NoiseFloor;
using NationalInstruments.RFmx.InstrMX;
using NationalInstruments.RFmx.SpecAnMX;
using MPAD_TestTimer;
using TCPHandlerProtocol;
using ni_NoiseFloorWrapper;
using Avago.ATF.Logger;
using Avago.ATF.LogService;
using System.Threading.Tasks;
using ClothoSharedItems;
using Avago.ATF.Shares;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace MyProduct
{
    public partial class MyDUT : IDisposable
    {
        MyUtility myUtility = new MyUtility();

        #region Variable - StopWatch : Stopwatch

        Stopwatch Speedo = new Stopwatch();

        #endregion

        #region Variable - TCF & Setting Files

        #region Variable - Dictionary : TCF
        public Dictionary<string, string>[] DicTestPA;
        public Dictionary<string, string> DicCalInfo;
        Dictionary<string, string> DicWaveForm;
        Dictionary<string, string> DicWaveFormMutate;
        Dictionary<string, string> DicTestLabel;
        Dictionary<string, string>[] DicMipiKey;
        Dictionary<string, string>[] DicPwrBlast;
        Dictionary<string, string> DicWaveFormAlias;
        //Dictionary<string, Dictionary<string, string>> DicLocalSettingFile;
        //Dictionary<string, Dictionary<double, double>> DicLocalCableLossFile, DicLocalBoardLossFile;
        #endregion

        public static string CalFilePath;
        public static string LocSetFilePath;
        public static string StartNo_UNIT_ID;         //use during OTP programing for unit_id
        public static string StopNo_UNIT_ID;          //use during OTP programing for unit_id
        public static IO_TextFile IO_TxtFile = new IO_TextFile();

        #endregion

        #region Variable - Interface : Equipment Class
        //LibEqmtDriver.SCU.iSwitch Eq.Site[0]._EqSwitch, Eq.Site[0]._EqSwitchSplit;

        //string[] Eq.Site[0]._SMUSetting;
        //string[] Eq.Site[0]._VCCSetting;

        //LibEqmtDriver.SMU.iPowerSupply[] Eq.Site[0]._EqSMU;
        //LibEqmtDriver.SMU.Drive_SMU Eq.Site[0]._Eq_SMUDriver;
        //LibEqmtDriver.SMU.iSmu[] Eq.Site[0]._EqPPMU;

        //Dictionary<string, LibEqmtDriver.SMU.iSmu> PpmuResources = new Dictionary<string, LibEqmtDriver.SMU.iSmu>(); // for PPMU control @ MIPI Class

        //LibEqmtDriver.DC_1CH.iDCSupply_1CH[] Eq.Site[0]._Eq_DCSupply;
        //LibEqmtDriver.DC_1CH.iDCSupply_1CH Eq.Site[0]._Eq_DC_1CH;
        //LibEqmtDriver.DC.iDCSupply Eq.Site[0]._EqDC;
        //LibEqmtDriver.PS.iPowerSensor Eq.Site[0]._EqPwrMeter;
        //LibEqmtDriver.SA.iSigAnalyzer Eq.Site[0]._EqSA01, Eq.Site[0]._EqSA02;
        //LibEqmtDriver.SG.iSiggen Eq.Site[0]._EqSG01, Eq.Site[0]._EqSG02;
        //LibEqmtDriver.SG.N5182A_WAVEFORM_MODE ModulationType;

        //LibEqmtDriver.MIPI.iMiPiCtrl Eq.Site[0]._EqMiPiCtrl;

        //LibEqmtDriver.NF_VST.NF_NiPXI_VST EqVST;
        //LibEqmtDriver.NF_VST.NF_NI_RFmx Eq.Site[0]._EqRFmx; //Seoul

        //LibEqmtDriver.TuneableFilter.iTuneFilterDriver Eq.Site[0]._EqTuneFilter;

        //public bool isUseRFmxForTxMeasure = true;

        #endregion

        #region Variable: Handler

        //private ExistechHandler TcpClient;
        private HontechHandler TcpClient; //Ivan
        TCPHandlerProtocol.HontechHandler handler; //Ivan
        public string HandlerAddress = "2"; //Ivan
        public string Handler_Info = "FALSE";
        public string strHandlerType = "HandlerSim";
        public bool Flag_HandlerInfor = false;
        public bool Flag_Init_TTD = false;

        #endregion

        #region Variable - String : Alias for Equipment Information
        //public static string InstrumentInfo = "";
        public static string InstrumentInfo_SwitchBox { get; set; }
        public static string Alias_Vcc1 { get; set; }
        public static string Alias_Vcc2 { get; set; }
        public static string Alias_Vbatt { get; set; }
        public static string Alias_Vlna { get; set; }
        public static string Alias_HSDIO { get; set; }
        public static string Alias_VST { get; set; }
        #endregion

        #region Variable : For Clotho

        //public static long M_MFG_ID { get; set; }
        //public static long M_OTP_MODULE_ID { get; set; }
        public static int StopOnContinueFail1A { get; set; }
        public static int StopOnContinueFail2A { get; set; }
        public static bool HandlerArmYieldDeltaEnable = false;  // ArmRate
        public static int HandlerArmTestCount { get; set; }
        public static int HandlerArmThreshold { get; set; }
        public static bool DpatEnable = false;   // Dpat
        public static bool WebQueryValidation { get; set; }
        public static bool LOCAL_GUDB_Enable { get; set; }

        public int MFG_ID_ReadError;

        static ATFLogControl logger = ATFLogControl.Instance;
        #endregion

        #region Variable : For Test

        #region Variable : Structurer

        public static s_EqmtStatus EqmtStatus;
        public static s_OffBias_AfterTest BiasStatus;
        public s_SNPFile SNPFile;
        public s_SNPDatalog SNPDatalog;
        public s_StopOnFail StopOnFail;

        #endregion 

        //public static List<string> ResultBuilder.FailedTests = new List<string>();
        //public static List<string> ResultBuilder.FailedTests[0] = new List<string>();
        public static List<string> FailedTests = new List<string>();
        public static List<string> FailedTestsFlag = new List<string>();

        #region Variable : Initial Flag

        string PreviousSWMode = "";
        string PreviousMXAMode = "";
        bool MXA_DisplayEnable = false;
        double SGTargetPin = -999;         //Global variable for SG input power
        double CurrentSaAttn = -999;
        double CurrentSa2Attn = -999;

        #endregion

        #region Variable : PDM Leakage

        string s_strSMU = ""; // Ben - Add for PDM Leakage

        #endregion

        #region Variable : Self Calibration

        bool bTestCalibration = false; //VST Calibration : True during Power Calibration & NFR Calibration

        #endregion

        #region Variable : FirstDut
        static bool FirstDut = true; //Ivan
        static bool FirstDut_DCSupply = true;  // Ben - Change for 'Multi_DCSupply'
        static bool FirstDut_VSTCalibration = true; // Ben - add for 'VST Calibration'
        #endregion

        #region Variable : Power sensor
        static int PowerSensorMeasuringType = 1;
        static int PowerSensorCalType = 0;
        #endregion

        #region Variable : Switch Box

        bool bMultiSW = false;

        private int switchLifeCycle = 10000000;
        private int triggerThreshold = 1000000;

        #endregion

        #region Variable : Result

        int R_ReadMipiReg = -999;
        Int64 R_ReadMipiReg_long = -999;
        Int64 R_partSN2_preReg = -999;
        Int64 R_partSN2_postReg = -999;
        Int64 R_partSN2_2DID = -999;
        Int64 R_MIPI_2DID_DELTA = -999;

        double R_NF1_Ampl = -999,
            R_NF2_Ampl = -999,
            R_NF1_Freq = -999,
            R_NF2_Freq = -999,
            R_H2_Ampl = -999,
            R_H2_Freq = -999,
            R_Pin = -999,
            R_Pin1 = -999,
            R_Pin2 = -999,
            R_Pout = -999,
            R_Pout1 = -999,
            R_Pout2 = -999,
            R_ITotal = -999,
            R_MIPI = -999,
            R_DCSupply = -999,
            R_Switch = -999,
            R_Temperature = -999,
            R_RFCalStatus = -999;

        PCBUnitTrace PCBUnitTrace = new PCBUnitTrace();
        #endregion

        #region Variable : Misc

        public bool InitInstrStatus = true;

        public int tmpUnit_No;
        public s_Result[] Results;
        public int TestCount;
        public s_TraceData[] MXATrace;
        public s_TraceNo Result_MXATrace;
        public s_TraceData[] PXITrace;
        public s_TraceData[] PXITraceRaw;
        public s_TraceNo Result_PXITrace;
        float dummyData;
        string dummyStrData;

        int multiRBW_cnt;
        int rbw_counter;
        string rbwParamName = null;
        int NoOfPts = 0;
        double[] RXContactdBm;
        double[] RXContactFreq;
        double[] RXContactGain;//Seoul
        double[] RXPathLoss;//Seoul
        double[] LNAInputLoss;//Seoul
        double[] TXPAOnFreq;//Seoul
        string TXCenterFreq;//Seoul
        double[] Cold_NF;//Seoul
        double[] Cold_NoisePower;//Seoul
        double[] Hot_NF;//Seoul
        double[] Hot_NoisePower;//Seoul
        double[] NFRise;//Seoul
        string calDir;//Seoul
        bool NFCalFlag = true;//Seoul
        Dictionary<double, double> RxGainDic;

        // Ben, Add for MIPI NFR
        double[][] Cold_MIPI_NF_new;//Seoul
        double[][] Cold_MIPI_NoisePower_new;//Seoul

        double[][] NF_new;//Seoul
        double[][] NoisePower_new;//Seoul
        double[][] Cold_NF_new;//Seoul
        double[][] Cold_NoisePower_new;//Seoul
        double[][] Hot_NF_new;//Seoul
        double[][] Hot_NoisePower_new;//Seoul

        public string mfgLotID_Path = @"C:\\Avago.ATF.Common\\OTPLogger\\";           //Default path for OTP programming datalogger
        public string mfgLotID;
        public string OTPLogFilePath = null;
        public int otpUnitID;
        public string deviceID;

        //MIPI variables - Custom MIPI Setting
        public string CusMipiKey;
        public string CusMipiRegMap;
        public string CusPMTrigMap;
        public string CusSlaveAddr;
        public string CusMipiPair;
        public string CusMipiSite;
        public string DicMipiTKey;

        //MIPI OTP Variable
        int totalbits = 16;         //default total bit for 2 register address is 16bits (binary)
        int effectiveBits = 16;     //default effective bit for 2 register address is 16bits (binary) - eg. JEDI SN ID only use up to 14bits
        string[] dataHex = null;
        int[] dataDec;
        string[] dataBinary = null;
        string[] biasDataArr = null;
        Int32 dataDec_Conv = 0;
        string dataSizeHex = "0xFFFF";  //default size for 2 byte register (16bit)
        int tmpOutData = 0;
        string appendDec = null;
        string appendHex = null;
        string appendBinary = null;
        string effectiveData = null;
        string tmpData = null;

        bool b_lockBit = true;
        int i_lockBit = 0;
        int i_testFlag = 0;
        bool b_testFlag = true;
        int i_bitPos = 0;     //bit position to compare (0 -> LSB , 7 -> MSB)
        bool BurnOTP = false;
        int[] efuseCtrlAddress = new int[3];
        string[] tempData = new string[2];

        Int32 tmpOutData_DecConv = 255;     //set to default very high because this variable use to check empty register (byright the value will be '0' if blank register)
        string CM_SITE = "NA";

        //Power Blast variables - PWRBlast sheet
        bool b_PwrBlastTKey = false;
        public double CtrFreqMHz_pwrBlast;
        public double StartPwrLvldBm_pwrBlast;
        public double StopPwrLvldBm_pwrBlast;
        public int StepPwrLvl_pwrBlast;
        public double DwellTmS_pwrBlast;
        public double Transient_mS_pwrBlast;
        public int Transient_Step_pwrBlast;

        //double unit variable
        public int PreviousModID = -1;
        //public bool DuplicatedModuleID = false;
        //public int DuplicatedModuleIDCtr = 0;
        public bool PauseOnDuplicateID = false;
        public Int64 Previous2DID = -1;
        public string Previous2DIDmark = "-1";

        // Delta 2DID
        public bool Delta2DIDCheckEnable = false;
        public bool DeltaMfgIDCheckEnable = false;

        //Golden Eagle variable
        public bool b_GE_Header = false;
        public string MeasBand = null;
        public s_GE_Header Rslt_GE_Header;

        ////correlation factor 
        //public static List<string> corrFileTestNameList = new List<string>();  // Test names found in correlation file.
        //public static Dictionary<string, float> GuCalFactorsDict = new Dictionary<string, float>();

        //Boardloss Variable
        public static string BoardlossFilePath;
        double[] In_BoardLoss;
        double[] Out_BoardLoss;

        //GU Ver Enable
        private bool blnSuccess = true;
        public bool InitSuccess
        {
            get { return blnSuccess; }
            set { blnSuccess = value; }
        }
        public bool GuVerEnable { get; set; }
        public string ProductTag = "";

        //ArmRate
        //ChoonChin (20220221) - Handler arm yield info [not yet complete]
        public static double TotalTest = 0;
        public static double TotalTestArm1 = 0;
        public static double TotalTestArm2 = 0;
        public static bool SingleArm = false;
        public static int SingleArmCalc = 0;
        public static double TotalPassArm1 = 0;
        public static double TotalPassArm2 = 0;
        public static bool TriggerStop = false;
        public static string HandlerArmNumberNoReset = "999";
        public static int TCF_ArmDeltaTestCount = 0;
        public static double TCF_ArmYieldThreshold = 100;

        #endregion

        #endregion

        public MyDUT(ref StringBuilder sb)
        {
            LoadTCFandSettingFiles(ref sb);
        }
        ~MyDUT()
        {
            Dispose();
        }
        public void Dispose()
        {
            UnInit();
        }

        public void RunTest(ref ATFReturnResult results)
        {
            #region Variable - List : Fail Flag 

            //ResultBuilder.FailedTests.Clear(); 
            //ResultBuilder.FailedTests[0].Clear();

            //Reset failed test
            ResultBuilder.InitializeResults();

            StopOnFail.TestFail = false;

            #endregion

            #region Variable : Results & Trace Data

            Results = new s_Result[DicTestPA.Length];
            MXATrace = new s_TraceData[DicTestPA.Length];
            PXITrace = new s_TraceData[DicTestPA.Length];
            PXITraceRaw = new s_TraceData[DicTestPA.Length];

            #endregion

            #region Variable : Test Time

            double TestTimeFBar, TestTimePA;

            Speedo.Reset();
            Speedo.Start();

            #endregion

            #region Variable : VST, SMU Calibration
            Stopwatch t_TimeVSTTempCal = new Stopwatch();
            bool bSelfcal_Flag = false;
            double dSelfCalLast_SA_temperature = 0.0f;
            double dSelfCalLast_SG_temperature = 0.0f;
            double dSA_temperature = 0.0f;
            double dSG_temperature = 0.0f;
            double dVbatt_temperature = 0.0f;
            double dVdd_temperature = 0.0f;
            double[] dVcc_temperature = new double[Eq.Site[0]._VCCSetting.Count()];
            #endregion;

            #region Variable : Test

            TestCount = 0; //reset to start

            LibEqmtDriver.MIPI.Lib_Var.b_setNIVIO = true;          //do this once for every unit for NI6570

            #endregion

            #region Read the board temp with temp sensor

            R_Temperature = -999;
            if (Eq.Site[0]._EqMiPiCtrl != null) Eq.Site[0]._EqMiPiCtrl.BoardTemperature(out R_Temperature);  //Read Testboard Temperature

            if (double.IsNaN(R_Temperature))
                R_Temperature = -999;

            #endregion

            Thread T = new Thread(new ThreadStart(HTS_Handler_Command_Readback));
            T.Start();

            if (EqmtStatus.PXI_VST && !ClothoDataObject.Instance.RunOptions.HasFlag(RunOption.SIMULATE))
            {
                #region VST & SMU Calibration depoend on equipment temp

                if (!bTestCalibration)
                {
                    t_TimeVSTTempCal.Reset();
                    t_TimeVSTTempCal.Start();

                    try
                    {
                        #region Read Temparture of Equipments

                        dSA_temperature = Eq.Site[0]._EqVST.rfsaSession.DeviceCharacteristics.GetDeviceTemperature();
                        dSG_temperature = Eq.Site[0]._EqVST.rfsgSession.DeviceCharacteristics.DeviceTemperature;

                        if (Eq.Site[0]._SMUSetting.Count() > 3 || Eq.Site[0]._VCCSetting.Count() > 1)
                        {
                            dVcc_temperature[0] = Eq.Site[0]._Eq_SMUDriver.CheckDeviceTemperature(Eq.Site[0]._SMUSetting[0], Eq.Site[0]._EqSMU);
                            dVcc_temperature[1] = Eq.Site[0]._Eq_SMUDriver.CheckDeviceTemperature(Eq.Site[0]._SMUSetting[1], Eq.Site[0]._EqSMU);
                            dVbatt_temperature = Eq.Site[0]._Eq_SMUDriver.CheckDeviceTemperature(Eq.Site[0]._SMUSetting[2], Eq.Site[0]._EqSMU);
                            dVdd_temperature = Eq.Site[0]._Eq_SMUDriver.CheckDeviceTemperature(Eq.Site[0]._SMUSetting[3], Eq.Site[0]._EqSMU);
                        }
                        else
                        {
                            dVcc_temperature[0] = Eq.Site[0]._Eq_SMUDriver.CheckDeviceTemperature(Eq.Site[0]._SMUSetting[0], Eq.Site[0]._EqSMU);
                            dVbatt_temperature = Eq.Site[0]._Eq_SMUDriver.CheckDeviceTemperature(Eq.Site[0]._SMUSetting[1], Eq.Site[0]._EqSMU);
                            dVdd_temperature = Eq.Site[0]._Eq_SMUDriver.CheckDeviceTemperature(Eq.Site[0]._SMUSetting[2], Eq.Site[0]._EqSMU);
                        }
                        #endregion

                        #region Re-Calibrate the Equipments if the temperature delta of epuip is over 2.0 C degree // Change from 1.0 C to 2.0 C

                        CheckVSTTemperature(ref dSelfCalLast_SA_temperature, ref dSelfCalLast_SG_temperature, ref dSA_temperature, ref dSG_temperature, ref bSelfcal_Flag);

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Fail to Check VST Temperature : " + ex.Message, "Fail", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    t_TimeVSTTempCal.Stop();
                }
                #endregion
            }

            #region build Results of temperature of the Equipment
            if (EqmtStatus.PXI_VST)
            {
                ResultBuilder.BuildResults(ref results, "Temp_SA_LastSelfCal", "C", dSelfCalLast_SA_temperature);
                ResultBuilder.BuildResults(ref results, "Temp_SA", "C", dSA_temperature);
                ResultBuilder.BuildResults(ref results, "Temp_SA_Delta", "C", Math.Abs(dSelfCalLast_SA_temperature - dSA_temperature));
                ResultBuilder.BuildResults(ref results, "Temp_SG_LastSelfCal", "C", dSelfCalLast_SG_temperature);
                ResultBuilder.BuildResults(ref results, "Temp_SG", "C", dSG_temperature);
                ResultBuilder.BuildResults(ref results, "Temp_SG_Delta", "C", Math.Abs(dSelfCalLast_SG_temperature - dSG_temperature));
            }
            if (Eq.Site[0]._SMUSetting.Count() > 3)
            {
                ResultBuilder.BuildResults(ref results, "Temp_Vcc1", "C", dVcc_temperature[0]);
                ResultBuilder.BuildResults(ref results, "Temp_Vcc2", "C", dVcc_temperature[1]);
            }
            else
            {
                ResultBuilder.BuildResults(ref results, "Temp_Vcc", "C", dVcc_temperature[0]);
            }
            ResultBuilder.BuildResults(ref results, "Temp_Vbatt", "C", dVbatt_temperature);
            ResultBuilder.BuildResults(ref results, "Temp_Vdd", "C", dVdd_temperature);
            ResultBuilder.BuildResults(ref results, "Temp_SelfCalFlag", "dec", (bSelfcal_Flag ? 1 : 0));
            ResultBuilder.BuildResults(ref results, "Temp_Testboard", "C", R_Temperature);
            ResultBuilder.BuildResults(ref results, "M_TIME_VSTTempCal", "mS", t_TimeVSTTempCal.ElapsedMilliseconds);

            #endregion

            foreach (Dictionary<string, string> currTestCond in DicTestPA)
            {
                int tmpTestNo = Convert.ToInt16(myUtility.ReadTcfData(currTestCond, TCF_Header.ConstTestNum));
                string tn = String.Format("RunTest_{0}", tmpTestNo);
                string testMode = myUtility.ReadTcfData(currTestCond, TCF_Header.ConstTestMode);
                StopWatchManager.Instance.StartTest(tn, testMode);

                MXATrace[TestCount].Multi_Trace = new s_TraceNo[1][];
                MXATrace[TestCount].Multi_Trace[0] = new s_TraceNo[2]; //initialize to 2 for 2x MXA trace only

                PXITrace[TestCount].Multi_Trace = new s_TraceNo[10][];  //maximum of 10 RBW trace can be stored
                PXITraceRaw[TestCount].Multi_Trace = new s_TraceNo[10][];  //maximum of 10 RBW trace can be stored

                for (int i = 0; i < PXITrace[TestCount].Multi_Trace.Length; i++)
                {
                    PXITrace[TestCount].Multi_Trace[i] = new s_TraceNo[15]; //initialize to 15 for 15x PXI trace loop only
                    PXITraceRaw[TestCount].Multi_Trace[i] = new s_TraceNo[15]; //initialize to 15 for 15x PXI trace loop only
                }

                if (StopOnFail.Enable != true)      //reset stop on fail flag if enable flag is false
                {
                    StopOnFail.TestFail = false;
                }

                try
                {
                    ExecuteTest(currTestCond, ref results);

                    //if (currTestCond["PARAMETER NAME"].ToUpper().Contains("OTP_MODULE_ID"))
                    if (currTestCond["PARAMETER NAME"].ToUpper() == ("OTP_MODULE_ID"))
                    {
                        ATFResultBuilder.AddResult(ref results, "M_OTP_STATUS_PCB-STRIP-MAX-X", "", PCBUnitTrace.PCBUnitTrace_PcbStrip_max_X);
                        ATFResultBuilder.AddResult(ref results, "M_OTP_STATUS_PCB-STRIP-MAX-Y", "", PCBUnitTrace.PCBUnitTrace_PcbStrip_max_Y);
                        ATFResultBuilder.AddResult(ref results, "M_OTP_STATUS_PCB-PANEL-MAX-X", "", PCBUnitTrace.PCBUnitTrace_PcbPanel_max_X);
                        ATFResultBuilder.AddResult(ref results, "M_OTP_STATUS_PCB-PANEL-MAX-Y", "", PCBUnitTrace.PCBUnitTrace_PcbPanel_max_Y);
                        ATFResultBuilder.AddResult(ref results, "M_OTP_STATUS_PCB-STRIP-X", "", PCBUnitTrace.PCBUnitTrace_PcbStrip_X);
                        ATFResultBuilder.AddResult(ref results, "M_OTP_STATUS_PCB-STRIP-Y", "", PCBUnitTrace.PCBUnitTrace_PcbStrip_Y);
                        ATFResultBuilder.AddResult(ref results, "M_OTP_STATUS_PCB-PANEL-X", "", PCBUnitTrace.PCBUnitTrace_PcbPanel_X);
                        ATFResultBuilder.AddResult(ref results, "M_OTP_STATUS_PCB-PANEL-Y", "", PCBUnitTrace.PCBUnitTrace_PcbPanel_Y);
                        ATFResultBuilder.AddResult(ref results, "M_OTP_STATUS_PCB-STRIP-EDGE", "", PCBUnitTrace.PCBUnitTrace_PCBstrip_Edge);
                        ATFResultBuilder.AddResult(ref results, "M_OTP_STATUS_PCB-PANEL-EDGE", "", PCBUnitTrace.PCBUnitTrace_PCBpanel_Edge);
                    }
                }
                catch (Exception ex)
                {
                    MPAD_TestTimer.LoggingManager.Instance.LogError(string.Format("Test Number : {0}, Test Parameter : {1}, Test Parameter : {2}\n{3}\n\n{4}\n{5}", currTestCond["TEST NUMBER"],
                        currTestCond["TEST PARAMETER"], currTestCond["PARAMETER NAME"], TestCount, ex.Message, ex.StackTrace));
                    throw ex;
                }
                TestCount++;

                StopWatchManager.Instance.Stop(tn);
            }

            Speedo.Stop();
            TestTimePA = Speedo.Elapsed.TotalMilliseconds;

            #region Close RFmx Session after NF Calibration for save cal file -Seoul
            for (int i = 0; i < DicTestPA.Count(); i++)
            {
                string testMode, testParameter;
                DicTestPA[i].TryGetValue("TEST MODE", out testMode);
                DicTestPA[i].TryGetValue("TEST PARAMETER", out testParameter);

                if (testMode.ToUpper() == "CALIBRATION" && testParameter.ToUpper() == "NF_CAL")
                {
                    Eq.Site[0]._EqRFmx.CloseSession();
                    MessageBox.Show("The NF calibration is finished.");
                    break;
                }
            }
            #endregion

            #region Power Off SMU and RF Power - for next DUT
            if (BiasStatus.SMU)
            {
                float offVolt = 0.0f;
                float offCurr = 1e-3f;

                if (EqmtStatus.SMU)
                {
                    string[] SetSMU = EqmtStatus.SMU_CH.Split(',');

                    string[] SetSMUSelect = new string[SetSMU.Count()];
                    for (int i = 0; i < SetSMU.Count(); i++)
                    {
                        int smuVChannel = Convert.ToInt16(SetSMU[i]);
                        SetSMUSelect[i] = Eq.Site[0]._SMUSetting[smuVChannel];       //rearrange the Eq.Site[0]._SMUSetting base on reqquired channel only from total of 8 channel available  
                        Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[smuVChannel], Eq.Site[0]._EqSMU, offVolt, offCurr);
                    }
                }
            }
            if (EqmtStatus.MXG01)
            {
                Eq.Site[0]._EqSG01.SetAmplitude(-110);
                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
            }
            if (EqmtStatus.MXG02)
            {
                Eq.Site[0]._EqSG02.SetAmplitude(-110);
                Eq.Site[0]._EqSG02.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
            }
            if (EqmtStatus.PM)
            {
                Eq.Site[0]._EqPwrMeter.SetOffset(1, 0); //reset power sensor offset to default : 0
            }
            if (EqmtStatus.MIPI)
            {
                Eq.Site[0]._EqMiPiCtrl.TurnOff_VIO(0);      //mipi pair 0 - DIO 0, DIO 1 and DIO 2 - For DUT TX
                Eq.Site[0]._EqMiPiCtrl.TurnOff_VIO(1);      //mipi pair 1 - DIO 3, DIO 4 and DIO 5 - For ref Unit on Test Board / DUT RX
                Eq.Site[0]._EqMiPiCtrl.TurnOff_VIO(2);      //mipi pair 2 - DIO 6, DIO 7 and DIO 8 - For future use
                Eq.Site[0]._EqMiPiCtrl.TurnOff_VIO(3);      //mipi pair 3 - DIO 9, DIO 10 and DIO 11 - For future use
            }
            #endregion

            Eq.Site[0]._EqSwitch.SaveLocalMechSwStatusFile();

            #region Build Result - the count of switch & test time

            ATFResultBuilder.AddResult(ref results, "M_SPDT1Count", "", Eq.Site[0]._EqSwitch.SPDT1CountValue());
            ATFResultBuilder.AddResult(ref results, "M_SPDT2Count", "", Eq.Site[0]._EqSwitch.SPDT2CountValue());
            ATFResultBuilder.AddResult(ref results, "M_SPDT3Count", "", Eq.Site[0]._EqSwitch.SPDT3CountValue());
            ATFResultBuilder.AddResult(ref results, "M_SPDT4Count", "", Eq.Site[0]._EqSwitch.SPDT4CountValue());
            ATFResultBuilder.AddResult(ref results, "M_SP6T1_1Count", "", Eq.Site[0]._EqSwitch.SP6T1_1CountValue());
            ATFResultBuilder.AddResult(ref results, "M_SP6T1_2Count", "", Eq.Site[0]._EqSwitch.SP6T1_2CountValue());
            ATFResultBuilder.AddResult(ref results, "M_SP6T1_3Count", "", Eq.Site[0]._EqSwitch.SP6T1_3CountValue());
            ATFResultBuilder.AddResult(ref results, "M_SP6T1_4Count", "", Eq.Site[0]._EqSwitch.SP6T1_4CountValue());
            ATFResultBuilder.AddResult(ref results, "M_SP6T1_5Count", "", Eq.Site[0]._EqSwitch.SP6T1_5CountValue());
            ATFResultBuilder.AddResult(ref results, "M_SP6T1_6Count", "", Eq.Site[0]._EqSwitch.SP6T1_6CountValue());
            ATFResultBuilder.AddResult(ref results, "M_SP6T2_1Count", "", Eq.Site[0]._EqSwitch.SP6T2_1CountValue());
            ATFResultBuilder.AddResult(ref results, "M_SP6T2_2Count", "", Eq.Site[0]._EqSwitch.SP6T2_2CountValue());
            ATFResultBuilder.AddResult(ref results, "M_SP6T2_3Count", "", Eq.Site[0]._EqSwitch.SP6T2_3CountValue());
            ATFResultBuilder.AddResult(ref results, "M_SP6T2_4Count", "", Eq.Site[0]._EqSwitch.SP6T2_4CountValue());
            ATFResultBuilder.AddResult(ref results, "M_SP6T2_5Count", "", Eq.Site[0]._EqSwitch.SP6T2_5CountValue());
            ATFResultBuilder.AddResult(ref results, "M_SP6T2_6Count", "", Eq.Site[0]._EqSwitch.SP6T2_6CountValue());
            ATFResultBuilder.AddResult(ref results, "PATestTime", "mS", TestTimePA);
            ATFResultBuilder.AddResult(ref results, "TotalTestTime", "mS", TestTimePA);

            #endregion

            T.Join();
            double[] ccb = HTS_Handler_Command_Readback_AddResult_Array();
            ATFResultBuilder.AddResult(ref results, "M_Handler-SiteNo", "No", ccb[0]);
            ATFResultBuilder.AddResult(ref results, "M_Handler-ArmNo", "No", ccb[1]);
            ATFResultBuilder.AddResult(ref results, "M_Handler-WorkpressForce", "kgf", ccb[2]);
            ATFCrossDomainWrapper.SetMfgIDAndModuleIDBySite(1, ResultBuilder.M_MFG_ID, ClothoDataObject.Instance.EnableOnlySeoulUser? ResultBuilder.M_OTP_MODULE_ID_MIPI:ResultBuilder.M_OTP_MODULE_ID_2DID_SYSTEM);

            #region ArmRate

            int TCF_ArmDeltaTestCount = 0;
            double TCF_ArmYieldThreshold = 100;
            double ArmYieldDelta = 0;

            if (HandlerArmYieldDeltaEnable)
            {
                TotalTest++;
                //double ArmYieldDelta = 0;
                int FailCount = FailedTests.Count;
                //int HandlerArm = Convert.ToInt16(HandlerArmNumberNoReset);
                int HandlerArm = Convert.ToInt16(ccb[1]);
                //Running single arm checking
                if (TotalTest % 2 != 0)
                {
                    SingleArmCalc = HandlerArm; //odd, remember arm number
                }
                else
                {
                    SingleArmCalc -= HandlerArm; //even, should have ran 2x testing
                    if (SingleArmCalc == 0) //no different in Arm no.
                    {
                        SingleArm = true;
                    }

                    SingleArmCalc = 0; //reset
                }

                //Accumulated Arm pass count, arm total test
                if (HandlerArm == 1)
                {
                    TotalTestArm1++;

                    if (FailCount == 0)
                        TotalPassArm1++;
                }
                else if (HandlerArm == 2)
                {
                    TotalTestArm2++;

                    if (FailCount == 0)
                        TotalPassArm2++;
                }

                //Report delta only after test count met TCF' setting
                if ((TotalTest >= (TCF_ArmDeltaTestCount + 1)) && (TCF_ArmDeltaTestCount != 0))
                {
                    ArmYieldDelta = Math.Round((((TotalPassArm1 / TotalTestArm1) - (TotalPassArm2 / TotalTestArm2)) * 100), 2);

                    //Check threshold value to see if exceed limit
                    if (!SingleArm)
                    {
                        //Reset things
                        TriggerStop = false;
                        TotalTest = 0;
                        TotalPassArm1 = 0;
                        TotalPassArm2 = 0;
                        TotalTestArm1 = 0;
                        TotalTestArm2 = 0;
                        SingleArm = false;

                        if ((Math.Abs(ArmYieldDelta) >= TCF_ArmYieldThreshold) && (TCF_ArmYieldThreshold != 0)) //trigger operator
                        {
                            TriggerStop = true;
                        }
                        else
                        {
                            //Reset things
                            TriggerStop = false;
                            TotalTest = 0;
                            TotalPassArm1 = 0;
                            TotalPassArm2 = 0;
                            TotalTestArm1 = 0;
                            TotalTestArm2 = 0;
                            SingleArm = false;
                        }
                    }
                    else
                    {
                        ArmYieldDelta = 0;
                    }

                    if (TriggerStop == true)
                    {
                        LoggingManager.Instance.LogInfoTestPlan("Handler arm yield delta > TCF setting");
                        //m_modelTpState.programLoadSuccess = false;
                        //PromptManager.Instance.ShowError("Handler arm yield delta > {0}, please restart Clotho and perform subcal.", m_tcfReaderSpara.DataObject.HandlerArmThreshold.ToString());
                        //return results;
                    }
                }
            }

            ATFResultBuilder.AddResult(ref results, "M_Handler-Arm1PassCount", "", TotalPassArm1);
            ATFResultBuilder.AddResult(ref results, "M_Handler-Arm1TestCount", "", TotalTestArm1);
            ATFResultBuilder.AddResult(ref results, "M_Handler-Arm2PassCount", "", TotalPassArm2);
            ATFResultBuilder.AddResult(ref results, "M_Handler-Arm2TestCount", "", TotalTestArm2);
            ATFResultBuilder.AddResult(ref results, "M_Handler-ArmYieldDelta", "", ArmYieldDelta);

        #endregion

            if (ClothoDataObject.Instance.WaferInformation != null && ClothoDataObject.Instance.WaferInformation.TryGetValue(tmpUnit_No, out string sWafer))
            {
                ATFCrossDomainWrapper.SetWaferIDBySite(1, sWafer);
            }
        }
        public void CheckVSTTemperature(ref double dSelfCalLast_SA_temp, ref double dSelfCalLast_SG_temp, ref double dSA_temp, ref double dSG_temp, ref bool bSelfcal_Flag)
        {
            StreamReader swTempreader;
            bool bTemperatureFailExist = false;
            string strTempLogLocation = "C:\\Avago.ATF.Common\\Input\\TemperatureLog.txt";
            string strReadTemp;
            double dSATemperatureDelta = 0, dSGTemperatureDelta = 0;
            //double dForceAlignmentDelta = 2.0; //allow-able temperature delta (C) before forcing full alignment, Change from 1.0 C to 2.0 C
            double dForceAlignmentDelta = 1.0;

            // added the function, 02. 03. 21 - Ben
            if (FirstDut_VSTCalibration)
            {
                dForceAlignmentDelta = -0.1;
                FirstDut_VSTCalibration = false;
            }
            else
            {
                //dForceAlignmentDelta = 2.0;
                dForceAlignmentDelta = 1.0;
            }

            #region Read Last Calibration Temp of VST

            bTemperatureFailExist = (File.Exists(strTempLogLocation) ? true : false);

            //Create temperature file
            if (!bTemperatureFailExist)
                ReCalibrationVST(ref dSelfCalLast_SA_temp, ref dSelfCalLast_SG_temp, ref bSelfcal_Flag, ref strTempLogLocation);

            //Read the temperature
            swTempreader = File.OpenText(strTempLogLocation);
            strReadTemp = swTempreader.ReadLine();
            swTempreader.Close();
            string[] strTmp = strReadTemp.Split(',');
            dSelfCalLast_SA_temp = Convert.ToDouble(strTmp[2]);
            dSelfCalLast_SG_temp = Convert.ToDouble(strTmp[4]);
            #endregion

            // Do Self Calibration if temperature delta > setting
            dSATemperatureDelta = Math.Abs(dSelfCalLast_SA_temp - dSA_temp);
            dSGTemperatureDelta = Math.Abs(dSelfCalLast_SG_temp - dSG_temp);

            if ((dSATemperatureDelta > dForceAlignmentDelta) || (dSGTemperatureDelta > dForceAlignmentDelta))
            {
                // Delete File & Re-Create temperature file

                File.Delete(strTempLogLocation);
                ReCalibrationVST(ref dSelfCalLast_SA_temp, ref dSelfCalLast_SG_temp, ref bSelfcal_Flag, ref strTempLogLocation);
            }
            else
                return;
        }
        public void ReCalibrationVST(ref double dSelfCalLast_SA_temp, ref double dSelfCalLast_SG_temp, ref bool bSelfcal_Flag, ref string str)
        {
            FileInfo TempFile;
            StreamWriter swTempFile;

            Eq.Site[0]._EqVST.rfsaSession.Calibration.Self.ClearSelfCalibrateRange();
            Eq.Site[0]._EqVST.rfsgSession.Calibration.Self.ClearSelfCalibrateRange();

            Eq.Site[0]._EqVST.rfsaSession.Calibration.Self.SelfCalibrateRange(0, 1400e6, 3000e6, -70, 20);
            Eq.Site[0]._EqVST.rfsgSession.Calibration.Self.SelfCalibrateRange(0, 1400e6, 3000e6, -40, 10);

            if (!bTestCalibration)
            {
                Eq.Site[0]._Eq_SMUDriver.CalSelfCalibrate(Eq.Site[0]._SMUSetting[0], Eq.Site[0]._EqSMU);
                Eq.Site[0]._Eq_SMUDriver.CalSelfCalibrate(Eq.Site[0]._SMUSetting[1], Eq.Site[0]._EqSMU);
                Eq.Site[0]._Eq_SMUDriver.CalSelfCalibrate(Eq.Site[0]._SMUSetting[2], Eq.Site[0]._EqSMU);
            }

            dSelfCalLast_SA_temp = Eq.Site[0]._EqVST.rfsaSession.DeviceCharacteristics.GetDeviceTemperature();
            dSelfCalLast_SG_temp = Eq.Site[0]._EqVST.rfsgSession.DeviceCharacteristics.DeviceTemperature;

            TempFile = new FileInfo(str);
            swTempFile = TempFile.CreateText();
            swTempFile.Close();
            swTempFile = TempFile.AppendText();
            swTempFile.WriteLine(DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss,") + "SA_Last_Cal_Temp," + dSelfCalLast_SA_temp + ",SG_Last_Cal_Temp," + dSelfCalLast_SG_temp);
            swTempFile.Close();
            bSelfcal_Flag = true;
        }
        private void ExecuteTest(Dictionary<string, string> TestPara, ref ATFReturnResult results)
        {
            Stopwatch tTime = new Stopwatch();

            tTime.Reset();
            tTime.Start();

            #region Read TCF Setting
            //Read TCF Setting

            string StrError = string.Empty;

            int _TestNum = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTestNum));       //use as array number for data store
            string _TestMode = myUtility.ReadTcfData(TestPara, TCF_Header.ConstTestMode);
            string _TestParam = myUtility.ReadTcfData(TestPara, TCF_Header.ConstTestParam);
            string _TestParaName = myUtility.ReadTcfData(TestPara, TCF_Header.ConstParaName);
            string _TestUsePrev = myUtility.ReadTcfData(TestPara, TCF_Header.ConstUsePrev);
            bool _Disp_ColdTrace = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstColdTrace).ToUpper() == "V" ? true : false);

            //Single Freq Condition
            float _TXFreq = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTXFreq));
            float _RXFreq = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstRXFreq));
            float _Pout = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPout));
            float _Pin = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPin));
            string _TXBand = myUtility.ReadTcfData(TestPara, TCF_Header.ConstTXBand);
            string _RXBand = myUtility.ReadTcfData(TestPara, TCF_Header.ConstRXBand);
            bool _TunePwr_TX = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTunePwr_TX).ToUpper() == "V" ? true : false);

            //Sweep TX1/RX1 Freq Condition
            float _Pout1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPout1));
            float _Pin1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPin1));
            float _StartTXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStartTXFreq1));
            float _StopTXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStopTXFreq1));
            float _StepTXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStepTXFreq1));
            float _DwellT1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDwellTime1));
            //bool _TunePwr_TX1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTunePwr_TX1).ToUpper() == "V" ? true : false);

            string _TunePwr_TX1_Method = "";

            string tmp = Convert.ToString(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTunePwr_TX1).ToUpper());

            if (tmp.Contains("V") == true)
            {
                if (tmp.Contains(",") == true)
                {
                    _TunePwr_TX1_Method = Convert.ToString(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTunePwr_TX1).ToUpper()).Split(',')[1];
                    if (_TunePwr_TX1_Method != "PS" && _TunePwr_TX1_Method != "SA")
                    {
                        _TunePwr_TX1_Method = "PS"; // Default Setting : with Power-Sensor
                    }
                }
                else
                {
                    _TunePwr_TX1_Method = "PS"; // Default Setting : with Power-Sensor
                }
            }

            bool _TunePwr_TX1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTunePwr_TX1).ToUpper().Contains("V") ? true : false);

            float _StartRXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStartRXFreq1));
            float _StopRXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStopRXFreq1));
            float _StepRXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStepRXFreq1));
            float _RX1SweepT = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstRX1SweepT));

            string _TX1Band = myUtility.ReadTcfData(TestPara, TCF_Header.ConstTX1Band);
            string _RX1Band = myUtility.ReadTcfData(TestPara, TCF_Header.ConstRX1Band);
            bool _SetRX1NDiag = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetRX1NDiag).ToUpper() == "V" ? true : false);

            //Sweep TX2/RX2 Freq Condition
            float _Pout2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPout2));
            float _Pin2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPin2));
            float _StartTXFreq2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStartTXFreq2));
            float _StopTXFreq2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStopTXFreq2));
            float _StepTXFreq2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStepTXFreq2));
            float _DwellT2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDwellTime2));
            bool _TunePwr_TX2 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTunePwr_TX2).ToUpper() == "V" ? true : false);

            float _StartRXFreq2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStartRXFreq2));
            float _StopRXFreq2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStopRXFreq2));
            float _StepRXFreq2 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStepRXFreq2));
            float _RX2SweepT = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstRX2SweepT));

            string _TX2Band = myUtility.ReadTcfData(TestPara, TCF_Header.ConstTX2Band);
            string _RX2Band = myUtility.ReadTcfData(TestPara, TCF_Header.ConstRX2Band);
            bool _SetRX2NDiag = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetRX2NDiag).ToUpper() == "V" ? true : false);

            //Misc
            string _PXI_MultiRBW = myUtility.ReadTcfData(TestPara, TCF_Header.PXI_MultiRBW);
            int _PXI_NoOfSweep = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.PXI_NoOfSweep));
            string _PoutTolerance = myUtility.ReadTcfData(TestPara, TCF_Header.ConstPoutTolerance);
            string _PinTolerance = myUtility.ReadTcfData(TestPara, TCF_Header.ConstPinTolerance);
            string _PowerMode = myUtility.ReadTcfData(TestPara, TCF_Header.ConstPowerMode);
            string _CalTag = myUtility.ReadTcfData(TestPara, TCF_Header.ConstCalTag);
            string _SwBand = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSwBand);
            string _Modulation = myUtility.ReadTcfData(TestPara, TCF_Header.ConstModulation);
            string _WaveFormName = myUtility.ReadTcfData(TestPara, TCF_Header.ConstWaveformName);
            bool _SetFullMod = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetFullMod).ToUpper() == "V" ? true : false);

            //Read TCF SMU Setting
            float[] _SMUVCh;
            _SMUVCh = new float[9];
            float[] _SMUILimitCh;
            _SMUILimitCh = new float[9];

            string _SMUSetCh = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUSetCh);
            string _SMUMeasCh = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUMeasCh);
            _SMUVCh[0] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh0));
            _SMUVCh[1] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh1));
            _SMUVCh[2] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh2));
            _SMUVCh[3] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh3));
            _SMUVCh[4] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh4));
            _SMUVCh[5] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh5));
            _SMUVCh[6] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh6));
            _SMUVCh[7] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh7));
            _SMUVCh[8] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh8));

            _SMUILimitCh[0] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh0Limit));
            _SMUILimitCh[1] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh1Limit));
            _SMUILimitCh[2] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh2Limit));
            _SMUILimitCh[3] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh3Limit));
            _SMUILimitCh[4] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh4Limit));
            _SMUILimitCh[5] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh5Limit));
            _SMUILimitCh[6] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh6Limit));
            _SMUILimitCh[7] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh7Limit));
            _SMUILimitCh[8] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUICh8Limit));

            //Read TCF DC Setting
            float[] _DCVCh;
            _DCVCh = new float[5];
            float[] _DCILimitCh;
            _DCILimitCh = new float[5];

            string _DCSetCh = myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCSetCh);
            string _DCMeasCh = myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCMeasCh);
            _DCVCh[1] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCVCh1));
            _DCVCh[2] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCVCh2));
            _DCVCh[3] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCVCh3));
            _DCVCh[4] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCVCh4));
            _DCILimitCh[1] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCICh1Limit));
            _DCILimitCh[2] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCICh2Limit));
            _DCILimitCh[3] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCICh3Limit));
            _DCILimitCh[4] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstDCICh4Limit));

            //MIPI
            string _MIPI_Set1 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_set1);
            string _MIPI_Set2 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_set2);
            int _MiPi_RegNo = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_RegNo));
            LibEqmtDriver.MIPI.Lib_Var.MIPI_Reg0 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_Reg0);
            LibEqmtDriver.MIPI.Lib_Var.MIPI_Reg1 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_Reg1);
            LibEqmtDriver.MIPI.Lib_Var.MIPI_Reg2 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_Reg2);
            LibEqmtDriver.MIPI.Lib_Var.MIPI_Reg3 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_Reg3);
            LibEqmtDriver.MIPI.Lib_Var.MIPI_Reg4 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_Reg4);
            LibEqmtDriver.MIPI.Lib_Var.MIPI_Reg5 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_Reg5);
            LibEqmtDriver.MIPI.Lib_Var.MIPI_Reg6 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_Reg6);
            LibEqmtDriver.MIPI.Lib_Var.MIPI_Reg7 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_Reg7);
            LibEqmtDriver.MIPI.Lib_Var.MIPI_Reg8 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_Reg8);
            LibEqmtDriver.MIPI.Lib_Var.MIPI_Reg9 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_Reg9);
            LibEqmtDriver.MIPI.Lib_Var.MIPI_RegA = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_RegA);
            LibEqmtDriver.MIPI.Lib_Var.MIPI_RegB = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_RegB);
            LibEqmtDriver.MIPI.Lib_Var.MIPI_RegC = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_RegC);
            LibEqmtDriver.MIPI.Lib_Var.MIPI_RegD = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_RegD);
            LibEqmtDriver.MIPI.Lib_Var.MIPI_RegE = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_RegE);
            LibEqmtDriver.MIPI.Lib_Var.MIPI_RegF = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPI_RegF);

            //Read Set Equipment Flag
            bool _SetSA1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetSA1).ToUpper() == "V" ? true : false);
            bool _SetSA2 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetSA2).ToUpper() == "V" ? true : false);
            bool _SetSG1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetSG1).ToUpper() == "V" ? true : false);
            bool _SetSG2 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetSG2).ToUpper() == "V" ? true : false);
            bool _SetSMU = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetSMU).ToUpper() == "V" ? true : false);

            //Read Off State Flag
            bool _OffSG1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstOffSG1).ToUpper() == "V" ? true : false);
            bool _OffSG2 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstOffSG2).ToUpper() == "V" ? true : false);
            bool _OffSMU = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstOffSMU).ToUpper() == "V" ? true : false);
            bool _OffDC = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstOffDC).ToUpper() == "V" ? true : false);

            //Read Require test parameter
            bool _Test_Pin = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Pin).ToUpper() == "V" ? true : false);
            bool _Test_Pout = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Pout).ToUpper() == "V" ? true : false);
            bool _Test_Pin1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Pin1).ToUpper() == "V" ? true : false);
            bool _Test_Pout1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Pout1).ToUpper() == "V" ? true : false);
            bool _Test_Pin2 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Pin2).ToUpper() == "V" ? true : false);
            bool _Test_Pout2 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Pout2).ToUpper() == "V" ? true : false);
            bool _Test_NF1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_NF1).ToUpper() == "V" ? true : false);
            bool _Test_NF2 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_NF2).ToUpper() == "V" ? true : false);
            bool _Test_MXATrace = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_MXATrace).ToUpper() == "V" ? true : false);
            bool _Test_MXATraceFreq = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_MXATraceFreq).ToUpper() == "V" ? true : false);
            bool _Test_Harmonic = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Harmonic).ToUpper() == "V" ? true : false);
            bool _Test_IMD = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_IMD).ToUpper() == "V" ? true : false);
            bool _Test_MIPI = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_MIPI).ToUpper() == "V" ? true : false);
            bool _Test_SMU = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_SMU).ToUpper() == "V" ? true : false);
            bool _Test_DCSupply = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_DCSupply).ToUpper() == "V" ? true : false);
            bool _Test_Switch = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Switch).ToUpper() == "V" ? true : false);
            bool _Test_TestTime = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_TestTime).ToUpper() == "V" ? true : false);

            //Read SA & SG setting
            string _SA1att = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSA1att);
            string _SA2att = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSA2att);
            double _SG1MaxPwr = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSG1MaxPwr));
            double _SG2MaxPwr = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSG2MaxPwr));
            double _SG1_DefaultFreq = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSG1_DefaultFreq));
            double _SG2_DefaultFreq = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSG2_DefaultFreq));
            double _PXI_Multiplier_RXIQRate = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMultiplier_RXIQRate));

            //Read Port Setting
            string _InfoTxPort = myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Tx_Port);
            string _InfoANTPort = myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_ANT_Port);
            string _InfoRxPort = myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Rx_Port);

            //Read Delay Setting
            int _Trig_Delay = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTrig_Delay));
            int _Generic_Delay = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstGeneric_Delay));
            int _RdCurr_Delay = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstRdCurr_Delay));
            int _RdPwr_Delay = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstRdPwr_Delay));
            int _Setup_Delay = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSetup_Delay));
            int _StartSync_Delay = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStartSync_Delay));
            int _StopSync_Delay = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStopSync_Delay));
            int _Estimate_TestTime = Convert.ToInt16(myUtility.ReadTcfData(TestPara, TCF_Header.ConstEstimate_TestTime));

            //Misc Setting
            string _Search_Method = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSearch_Method);
            //float _Search_Value = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSearch_Value));
            string _Search_Value = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSearch_Value);
            bool _Interpolation = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstInterpolation).ToUpper() == "V" ? true : false);
            bool _Abs_Value = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstAbs_Value).ToUpper() == "V" ? true : false);
            bool _Save_MXATrace = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSave_MXATrace).ToUpper() == "V" ? true : false);

            //NF Setting -Seoul
            string _SwBand_HotNF = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSwitching_Band_HotNF);
            double _NF_BW = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstNF_BW));
            double _NF_REFLevel = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstNF_REFLEVEL));
            double _NF_SweepTime = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstNF_SWEEPTIME));
            int _NF_Average = Convert.ToInt32(myUtility.ReadTcfData(TestPara, TCF_Header.ConstNF_AVERAGE));
            string _NF_CalTag = myUtility.ReadTcfData(TestPara, TCF_Header.ConstNF_CalTag);
            double _NF_SoakTime = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstNF_SoakTime));
            double _NF_Cal_HL = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstNF_Cal_HL));
            double _NF_Cal_LL = Convert.ToDouble(myUtility.ReadTcfData(TestPara, TCF_Header.ConstNF_Cal_LL));
            int TestUsePrev_ArrayNo = 0;

            //MIPI Voltage and current setting
            //Read TCF MIPI Voltage and current Setting
            float[] _MIPI_VSetCh;
            _MIPI_VSetCh = new float[3];
            float[] _MIPI_ILimitCh;
            _MIPI_ILimitCh = new float[3];

            string _MIPI_SetCh = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPISetCh);
            string _MIPI_MeasCh = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIMeasCh);

            try
            {
                _MIPI_VSetCh[0] = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIVSclk) == "" ? 0 : Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIVSclk));
                _MIPI_VSetCh[1] = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIVSdata) == "" ? 0 : Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIVSdata));
                _MIPI_VSetCh[2] = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIVSvio) == "" ? 0 : Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIVSvio));

                _MIPI_ILimitCh[0] = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIISclk) == "" ? 0 : Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIISclk));
                _MIPI_ILimitCh[1] = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIISdata) == "" ? 0 : Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIISdata));
                _MIPI_ILimitCh[2] = myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIISvio) == "" ? 0 : Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIISvio));

                //_MIPI_VSetCh[0] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIVSclk));
                //_MIPI_VSetCh[1] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIVSdata));
                //_MIPI_VSetCh[2] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIVSvio));

                //_MIPI_ILimitCh[0] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIISclk));
                //_MIPI_ILimitCh[1] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIISdata));
                //_MIPI_ILimitCh[2] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstMIPIISvio));
            }
            catch (Exception)
            {
                //optional .. will be required to set in TCF if needed
                _MIPI_VSetCh[0] = 0;
                _MIPI_VSetCh[1] = 0;
                _MIPI_VSetCh[2] = 0;

                _MIPI_ILimitCh[0] = 0;
                _MIPI_ILimitCh[1] = 0;
                _MIPI_ILimitCh[2] = 0;
            }

            string Tmp1stHeader = null;
            string Tmp2ndHeader = null;
            string[] TmpParamName;
            string TxPAOnScript = "";
            double _StepTXFreq = 0;

            string[] SetSMU;
            string[] MeasSMU;
            double[] R_SMU_ICh;
            string[] R_SMULabel_ICh;
            string[] R_SMULabel_VCh;
            R_SMU_ICh = new double[9];
            R_SMULabel_ICh = new string[9];
            R_SMULabel_VCh = new string[9];

            string[] SetDC;
            string[] MeasDC;
            double[] R_DC_ICh;
            string[] R_DCLabel_ICh;
            R_DC_ICh = new double[5];
            R_DCLabel_ICh = new string[5];
            string[] SetSMUSelect;

            bool MIPI_Read_Successful = false;

            //temp result storage use for MAX , MIN etc calculation 
            Results[TestCount].Multi_Results = new s_mRslt[15];      //default to 15 , need to check total enum of e_ResultTag
            Results[TestCount].TestNumber = _TestNum;
            Results[TestCount].Enable = true;

            //MIPI Voltage and current setting
            string[] VSetMIPI;
            string[] IMeasMIPI;
            double[] R_MIPI_ICh;
            string[] R_MIPILabel_ICh;
            R_MIPI_ICh = new double[3];
            R_MIPILabel_ICh = new string[3];

            #endregion

            //Load cal factor
            #region Cal Factor
            double _LossCouplerPath = 999;
            double _LossOutputPathRX1 = 999;
            double _LossOutputPathRX2 = 999;
            double _LossInputPathSG1 = 999;
            double _LossInputPathSG2 = 999;
            double _LossTestBoard = 999;

            #endregion

            InitResultVariable();

            //Test Variable
            #region Test Variable
            bool status = false;
            bool pwrSearch = false;
            int Index = 0;
            int tx1_noPoints = 0;
            int rx1_noPoints = 0;
            float tx1_span = 0;
            double rx1_span = 0;
            double rx1_cntrfreq = 0;
            double rx2_span = 0;
            double rx2_cntrfreq = 0;
            double totalInputLoss = 0;      //Input Pathloss + Testboard Loss
            double totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
            double totalRXLoss = 0;     //RX Pathloss + Testboard Loss
            double tolerancePwr = 0;

            //mxa#1 and mxa#2 setting variable
            int rx1_mxa_nopts = 0;
            double rx1_mxa_nopts_step = 0.1;        //step 0.1MHz , example mxa_nopts (601) , every points = 0.1MHz
            int rx2_mxa_nopts = 0;
            double rx2_mxa_nopts_step = 0.1;        //step 0.1MHz , example mxa_nopts (601) , every points = 0.1MHz

            string MkrCalSegmTag = null;
            string CalSegmData = null;
            double tbInputLoss = 0;
            double tbOutputLoss = 0;
            string MXA_Config = null;
            int markerNo = 1;

            int count;
            int txcount;
            int rxcount;
            double tmpInputLoss = 0;
            double tmpCouplerLoss = 0;
            double tmpAveInputLoss = 0;
            double tmpAveCouplerLoss = 0;
            double tmpRxLoss = 0;
            double tmpAveRxLoss = 0;
            double tmpMkrNoiseLoss = 0;
            double tmpAveMkrNoiseLoss = 0;
            double mkrNoiseLoss = 0;
            double AveMkrNoiseLossRX1 = 0;
            double AveMkrNoiseLossRX2 = 0;

            long paramTestTime = 0;
            long syncTest_Delay = 0;
            decimal trigDelay = 0;

            //COMMON case variable
            int resultTag;
            int arrayVal;
            double result;
            bool usePrevRslt = false;
            double prevRslt = 0;
            bool b_mipiTKey = false;

            //VST Variable
            double SG_IQRate = 0;

            #endregion

            #region Misc Setup Variable
            int istep;
            int indexdata = 0;

            double[] tx_freqArray;
            double[] rx_freqArray;
            double[] contactPwr_Array;
            double[] nfAmpl_Array;
            double[] nfAmplFreq_Array;

            //Variable use in VST Measure Function
            int NumberOfRuns = 5;
            double SGPowerLevel = -18;// -18 CDMA dBm //-20 LTE dBm  
            double SAReferenceLevel = -20;
            double SoakTime = 450e-3;
            double SoakFrequency = _StartTXFreq1 * 1e6;
            double vBW_Hz = 300;
            double RBW_Hz = 1e6;
            bool preSoakSweep = true; //to indicate if another sweep should be done **MAKE SURE TO SPLIT OUTPUT ARRAY**
            int preSoakSweepTemp = 0;
            double stepFreqMHz = 0;
            double tmpRXFreqHz = 0;
            int sweepPts = 0;

            //Variable for NF Result Fetch
            string[] TestUsePrev_Array;
            int NF_TestCount;
            int ColdNF_TestCount;
            int HotNF_TestCount;

            int Nop_NF;
            int Nop_ColdNF;
            int Nop_HotNF;
            int NumberOfRunsNF;
            int NumberOfRunsColdNF;
            int NumberOfRunsHotNF;
            double[] RXPathLoss_NF;
            double[] RXPathLoss_Cold;
            double[] RXPathLoss_Hot;

            s_TraceNo ResultMultiTrace_NF;
            s_TraceNo ResultMultiTrace_ColdNF;
            s_TraceNo ResultMultiTrace_HotNF;
            s_TraceNo ResultMultiTraceDelta;

            Dictionary<double, double> Dic_NF;
            Dictionary<double, double> Dic_ColdNF;
            Dictionary<double, double> Dic_HotNF;

            double MaxNFAmpl;
            double MaxNFFreq;
            double MaxColdNFAmpl;
            double MaxColdNFFreq;
            double MaxHotNFAmpl;
            double MaxHotNFFreq;
            double MaxNFRiseAmpl;
            double MaxNFRiseFreq;
            double CalcData;

            // Ben, add for MIPI NFR
            int ColdMIPINF_TestCount;
            int Nop_ColdMIPINF;
            int NumberOfRunsColdMIPINF;
            double[] RXPathLoss_ColdMIPI;
            s_TraceNo ResultMultiTrace_ColdMIPINF;
            Dictionary<double, double> Dic_ColdMIPINF;
            double MaxColdMIPINFAmpl;
            double MaxColdMIPINFFreq;
            double MaxNFMIPIRiseAmpl;
            double MaxNFMIPIRiseFreq;

            //testboard variable & Golden Eagle Header
            bool b_TestBoard_temp = false;
            try
            {
                b_GE_Header = Convert.ToBoolean(DicCalInfo[DataFilePath.Enable_GEHeader]);
            }
            catch
            {
                b_GE_Header = false;
            }

            bool b_SmuHeader = false;

            #endregion

            #region TEST
            switch (_TestMode.ToUpper())
            {
                case "MXA_TRACE":
                    switch (_TestParam.ToUpper())
                    {
                        case "CALC_MXA_TRACE":
                            #region Calculate MXA Trace

                            int mxaNo = 1;
                            int traceNo = 1;
                            double mxaTrace_Ampl = -999;

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], _CalTag.ToUpper(), _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));
                            #endregion

                            if (_Test_NF1)
                            {
                                mxaTrace_Ampl = -999;
                                mxaNo = 1;
                                Read_MXA_MultiTrace(mxaNo, traceNo, _TestUsePrev, _StartRXFreq1, _StopRXFreq1, _StepRXFreq1, _Search_Method, _TestParam, out R_NF1_Freq, out R_NF1_Ampl);
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, R_NF1_Freq, ref _LossOutputPathRX1, ref StrError);
                                R_NF1_Ampl = R_NF1_Ampl - _LossOutputPathRX1 - tbOutputLoss;

                                if (_Test_MXATrace)
                                {
                                    for (int i = 0; i < Result_MXATrace.FreqMHz.Length; i++)
                                    {
                                        mxaTrace_Ampl = Result_MXATrace.Ampl[i] - _LossOutputPathRX1 - tbOutputLoss;    //use same pathloss as previous data (from "Read_MXA_MultiTrace" function)

                                        ResultBuilder.BuildResults(ref results, "P" + i + "_" + _TestParaName + "_RX" + _RX1Band + "_Ampl", "dBm", mxaTrace_Ampl);

                                        if (_Test_MXATraceFreq)
                                        {
                                            ResultBuilder.BuildResults(ref results, "P" + i + "_" + _TestParaName + "_RX" + _RX1Band + "_Freq", "MHz", Result_MXATrace.FreqMHz[i]);
                                        }
                                    }
                                }
                            }
                            if (_Test_NF2)
                            {
                                mxaTrace_Ampl = -999;
                                mxaNo = 2;
                                Read_MXA_MultiTrace(mxaNo, traceNo, _TestUsePrev, _StartRXFreq2, _StopRXFreq2, _StepRXFreq2, _Search_Method, _TestParam, out R_NF2_Freq, out R_NF2_Ampl);
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX2CalSegm, R_NF2_Freq, ref _LossOutputPathRX2, ref StrError);
                                R_NF2_Ampl = R_NF2_Ampl - _LossOutputPathRX2 - tbOutputLoss;

                                if (_Test_MXATrace)
                                {
                                    for (int i = 0; i < Result_MXATrace.FreqMHz.Length; i++)
                                    {
                                        mxaTrace_Ampl = Result_MXATrace.Ampl[i] - _LossOutputPathRX2 - tbOutputLoss;    //use same pathloss as previous data (from "Read_MXA_MultiTrace" function)

                                        ResultBuilder.BuildResults(ref results, "P" + i + "_" + _TestParaName + "_RX" + _RX2Band + "_Ampl", "dBm", mxaTrace_Ampl);

                                        if (_Test_MXATraceFreq)
                                        {
                                            ResultBuilder.BuildResults(ref results, "P" + i + "_" + _TestParaName + "_RX" + _RX2Band + "_Freq", "MHz", Result_MXATrace.FreqMHz[i]);
                                        }
                                    }
                                }
                            }

                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            #endregion
                            break;

                        case "MERIT_FIGURE":
                            #region Figure of merit calculation

                            double tmpFreqMhz = -999;
                            double tmpAmpl = -999;
                            double count_FOM = 0;
                            double percent_FOM = -999;
                            double totalPts = -999;

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], _CalTag.ToUpper(), _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));
                            #endregion

                            if (_Test_NF1)
                            {
                                count_FOM = 0;
                                percent_FOM = -999;
                                mxaTrace_Ampl = -999;
                                mxaNo = 1;
                                traceNo = 1;
                                Read_MXA_MultiTrace(mxaNo, traceNo, _TestUsePrev, _StartRXFreq1, _StopRXFreq1, _StepRXFreq1, _Search_Method, _TestParam, out tmpFreqMhz, out tmpAmpl);
                                totalPts = Result_MXATrace.FreqMHz.Length - 1;

                                for (int i = 0; i < Result_MXATrace.FreqMHz.Length; i++)
                                {
                                    ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, Result_MXATrace.FreqMHz[i], ref _LossOutputPathRX1, ref StrError);
                                    mxaTrace_Ampl = Result_MXATrace.Ampl[i] - _LossOutputPathRX1 - tbOutputLoss;

                                    switch (_Search_Method.ToUpper())
                                    {
                                        case "MAX":
                                            if (mxaTrace_Ampl >= Convert.ToSingle(_Search_Value))
                                            {
                                                count_FOM++;
                                            }
                                            break;

                                        case "MIN":
                                            if (mxaTrace_Ampl <= Convert.ToSingle(_Search_Value))
                                            {
                                                count_FOM++;
                                            }
                                            break;

                                        default:
                                            MessageBox.Show("Test Parameter : " + _TestParam + "(" + _Search_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                            break;
                                    }
                                }

                                percent_FOM = Math.Round(((count_FOM / totalPts) * 100), 3);
                                R_NF1_Ampl = percent_FOM;
                                R_NF1_Freq = 1;     //dummy data
                            }

                            if (_Test_NF2)
                            {
                                count_FOM = 0;
                                percent_FOM = -999;
                                mxaTrace_Ampl = -999;
                                mxaNo = 2;
                                traceNo = 1;
                                Read_MXA_MultiTrace(mxaNo, traceNo, _TestUsePrev, _StartRXFreq2, _StopRXFreq2, _StepRXFreq2, _Search_Method, _TestParam, out tmpFreqMhz, out tmpAmpl);

                                for (int i = 0; i < Result_MXATrace.FreqMHz.Length; i++)
                                {
                                    ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX2CalSegm, Result_MXATrace.FreqMHz[i], ref _LossOutputPathRX2, ref StrError);
                                    mxaTrace_Ampl = Result_MXATrace.Ampl[i] - _LossOutputPathRX2 - tbOutputLoss;

                                    switch (_Search_Method.ToUpper())
                                    {
                                        case "MAX":
                                            if (mxaTrace_Ampl >= Convert.ToSingle(_Search_Value))
                                            {
                                                count_FOM++;
                                            }
                                            break;

                                        case "MIN":
                                            if (mxaTrace_Ampl <= Convert.ToSingle(_Search_Value))
                                            {
                                                count_FOM++;
                                            }
                                            break;

                                        default:
                                            MessageBox.Show("Test Parameter : " + _TestParam + "(" + _Search_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                            break;
                                    }
                                }

                                percent_FOM = (count_FOM / Result_MXATrace.FreqMHz.Length) * 100;
                                R_NF2_Ampl = percent_FOM;
                                R_NF2_Freq = 1;             //dummy data 
                            }

                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            #endregion
                            break;

                        default:
                            MessageBox.Show("Test Parameter : " + _TestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }
                    break;

                case "PXI_TRACE":
                    switch (_TestParam.ToUpper())
                    {
                        case "CALC_PXI_TRACE":
                            #region Calculate PXI Trace

                            double pxiTrace_Ampl = -999;
                            tmpAveRxLoss = 0;
                            tmpRxLoss = 0;
                            count = 0;

                            #region decode re-arrange multiple bandwidth (Ascending)
                            int bw_cnt = 0;
                            double[] tmpRBW_Hz = Array.ConvertAll(_PXI_MultiRBW.Split(','), double.Parse);  //split and convert string to double array
                            double[] multiRBW_Hz = new double[tmpRBW_Hz.Length];

                            Array.Sort(tmpRBW_Hz);
                            foreach (double key in tmpRBW_Hz)
                            {
                                multiRBW_Hz[bw_cnt] = Convert.ToDouble(key);
                                bw_cnt++;
                            }

                            multiRBW_cnt = multiRBW_Hz.Length;
                            #endregion

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], _CalTag.ToUpper(), _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));
                            #endregion

                            //Get average pathloss base on start and stop freq with 1MHz step freq
                            count = Convert.ToInt16((_StopRXFreq1 - _StartRXFreq1) / 1);
                            _RXFreq = _StartRXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                tmpRxLoss = Math.Round(tmpRxLoss + (float)_LossOutputPathRX1, 3);   //need to use round function because of C# float and double floating point bug/error
                                _RXFreq = Convert.ToSingle(Math.Round(_RXFreq + 1, 3));             //need to use round function because of C# float and double floating point bug/error
                            }
                            tmpAveRxLoss = tmpRxLoss / (count + 1);

                            if (_Test_NF1)
                            {
                                for (rbw_counter = 0; rbw_counter < multiRBW_cnt; rbw_counter++)
                                {
                                    rbwParamName = null;
                                    rbwParamName = "_" + Math.Abs(multiRBW_Hz[rbw_counter] / 1e6).ToString() + "MHz";

                                    pxiTrace_Ampl = -999;

                                    Read_PXI_MultiTrace(_TestUsePrev, _StartRXFreq1, _StopRXFreq1, _StepRXFreq1, _Search_Method, _TestParam, out R_NF1_Freq, out R_NF1_Ampl, rbw_counter, multiRBW_Hz[rbw_counter]);
                                    R_NF1_Ampl = R_NF1_Ampl - tmpAveRxLoss - tbOutputLoss;

                                    if (_Test_MXATrace)
                                    {
                                        for (int i = 0; i < Result_MXATrace.FreqMHz.Length; i++)
                                        {
                                            pxiTrace_Ampl = Result_MXATrace.Ampl[i] - tmpAveRxLoss - tbOutputLoss;    //use same pathloss as previous data (from "Read_PXI_MultiTrace" function)

                                            ResultBuilder.BuildResults(ref results, "P" + i + "_" + _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Ampl", "dBm", pxiTrace_Ampl);

                                            if (_Test_MXATraceFreq)
                                            {
                                                ResultBuilder.BuildResults(ref results, "P" + i + "_" + _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Freq", "MHz", Result_MXATrace.FreqMHz[i]);
                                            }
                                        }
                                    }

                                    if (_Test_NF1)
                                    {
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                    }
                                }
                            }

                            //Force test flag to false to ensure no repeated test data
                            //because we add to string builder upfront for PXI due to data reported base on number of sweep
                            _Test_NF1 = false;
                            tTime.Stop();
                            #endregion
                            break;

                        case "MERIT_FIGURE":
                            #region Figure of merit calculation

                            double tmpFreqMhz = -999;
                            double tmpAmpl = -999;
                            double count_FOM = 0;
                            double percent_FOM = -999;
                            double totalPts = -999;
                            tmpAveRxLoss = 0;
                            tmpRxLoss = 0;
                            count = 0;

                            #region decode re-arrange multiple bandwidth (Ascending)
                            bw_cnt = 0;
                            tmpRBW_Hz = Array.ConvertAll(_PXI_MultiRBW.Split(','), double.Parse);  //split and convert string to double array
                            multiRBW_Hz = new double[tmpRBW_Hz.Length];

                            Array.Sort(tmpRBW_Hz);
                            foreach (double key in tmpRBW_Hz)
                            {
                                multiRBW_Hz[bw_cnt] = Convert.ToDouble(key);
                                bw_cnt++;
                            }

                            multiRBW_cnt = multiRBW_Hz.Length;
                            #endregion

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], _CalTag.ToUpper(), _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));
                            #endregion

                            //Get average pathloss base on start and stop freq with 1MHz step freq
                            count = Convert.ToInt16((_StopRXFreq1 - _StartRXFreq1) / 1);
                            _RXFreq = _StartRXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                tmpRxLoss = Math.Round(tmpRxLoss + (float)_LossOutputPathRX1, 3);   //need to use round function because of C# float and double floating point bug/error
                                _RXFreq = Convert.ToSingle(Math.Round(_RXFreq + 1, 3));             //need to use round function because of C# float and double floating point bug/error
                            }
                            tmpAveRxLoss = tmpRxLoss / (count + 1);

                            if (_Test_NF1)
                            {
                                for (rbw_counter = 0; rbw_counter < multiRBW_cnt; rbw_counter++)
                                {
                                    rbwParamName = null;
                                    rbwParamName = "_" + Math.Abs(multiRBW_Hz[rbw_counter] / 1e6).ToString() + "MHz";

                                    count_FOM = 0;
                                    percent_FOM = -999;
                                    pxiTrace_Ampl = -999;

                                    Read_PXI_MultiTrace(_TestUsePrev, _StartRXFreq1, _StopRXFreq1, _StepRXFreq1, _Search_Method, _TestParam, out tmpFreqMhz, out tmpAmpl, rbw_counter, multiRBW_Hz[rbw_counter]);
                                    totalPts = Result_MXATrace.FreqMHz.Length - 1;

                                    for (int i = 0; i < Result_MXATrace.FreqMHz.Length; i++)
                                    {
                                        pxiTrace_Ampl = Result_MXATrace.Ampl[i] - tmpAveRxLoss - tbOutputLoss;

                                        switch (_Search_Method.ToUpper())
                                        {
                                            case "MAX":
                                                if (pxiTrace_Ampl >= Convert.ToSingle(_Search_Value))
                                                {
                                                    count_FOM++;
                                                }
                                                break;

                                            case "MIN":
                                                if (pxiTrace_Ampl <= Convert.ToSingle(_Search_Value))
                                                {
                                                    count_FOM++;
                                                }
                                                break;

                                            default:
                                                MessageBox.Show("Test Parameter : " + _TestParam + "(" + _Search_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                                break;
                                        }
                                    }

                                    percent_FOM = Math.Round(((count_FOM / totalPts) * 100), 3);
                                    R_NF1_Ampl = percent_FOM;
                                    R_NF1_Freq = 1;     //dummy data

                                    if (_Test_NF1)
                                    {
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                    }
                                }
                            }

                            //Force test flag to false to ensure no repeated test data
                            //because we add to string builder upfront for PXI due to data reported base on number of sweep
                            _Test_NF1 = false;

                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            #endregion
                            break;

                        case "MAX_MIN":
                            #region MAX MIN calculation
                            tmpFreqMhz = -999;
                            tmpAmpl = -999;
                            indexdata = 0;
                            tmpAveRxLoss = 0;
                            tmpRxLoss = 0;
                            count = 0;

                            #region decode re-arrange multiple bandwidth (Ascending)
                            bw_cnt = 0;
                            tmpRBW_Hz = Array.ConvertAll(_PXI_MultiRBW.Split(','), double.Parse);  //split and convert string to double array
                            multiRBW_Hz = new double[tmpRBW_Hz.Length];

                            Array.Sort(tmpRBW_Hz);
                            foreach (double key in tmpRBW_Hz)
                            {
                                multiRBW_Hz[bw_cnt] = Convert.ToDouble(key);
                                bw_cnt++;
                            }

                            multiRBW_cnt = multiRBW_Hz.Length;
                            #endregion

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], _CalTag.ToUpper(), _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));
                            #endregion

                            //Get average pathloss base on start and stop freq with 1MHz step freq
                            count = Convert.ToInt16((_StopRXFreq1 - _StartRXFreq1) / 1);
                            _RXFreq = _StartRXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                tmpRxLoss = Math.Round(tmpRxLoss + (float)_LossOutputPathRX1, 3);   //need to use round function because of C# float and double floating point bug/error
                                _RXFreq = Convert.ToSingle(Math.Round(_RXFreq + 1, 3));             //need to use round function because of C# float and double floating point bug/error
                            }
                            tmpAveRxLoss = tmpRxLoss / (count + 1);

                            if (_Test_NF1)
                            {
                                for (rbw_counter = 0; rbw_counter < multiRBW_cnt; rbw_counter++)
                                {
                                    rbwParamName = null;
                                    rbwParamName = "_" + Math.Abs(multiRBW_Hz[rbw_counter] / 1e6).ToString() + "MHz";

                                    Read_PXI_MultiTrace(_TestUsePrev, _StartRXFreq1, _StopRXFreq1, _StepRXFreq1, _Search_Method, _TestParam, out tmpFreqMhz, out tmpAmpl, rbw_counter, multiRBW_Hz[rbw_counter]);

                                    switch (_Search_Method.ToUpper())
                                    {
                                        case "MAX":
                                            R_NF1_Ampl = Result_MXATrace.Ampl.Max();
                                            indexdata = Array.IndexOf(Result_MXATrace.Ampl, R_NF1_Ampl);     //return index of max value
                                            R_NF1_Freq = Result_MXATrace.FreqMHz[indexdata];

                                            R_NF1_Ampl = R_NF1_Ampl - tmpAveRxLoss - tbOutputLoss;
                                            break;

                                        case "MIN":
                                            R_NF1_Ampl = Result_MXATrace.Ampl.Min();
                                            indexdata = Array.IndexOf(Result_MXATrace.Ampl, R_NF1_Ampl);     //return index of min value
                                            R_NF1_Freq = Result_MXATrace.FreqMHz[indexdata];

                                            R_NF1_Ampl = R_NF1_Ampl - tmpAveRxLoss - tbOutputLoss;
                                            break;

                                        default:
                                            MessageBox.Show("Test Parameter : " + _TestParam + "(" + _Search_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                            break;
                                    }

                                    if (_Test_NF1)
                                    {
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                    }
                                }
                            }

                            //Force test flag to false to ensure no repeated test data
                            //because we add to string builder upfront for PXI due to data reported base on number of sweep
                            _Test_NF1 = false;
                            tTime.Stop();
                            break;
                        #endregion

                        case "TRACE_MERIT_FIGURE":
                            #region Individual Trace Figure of merit calculation

                            tmpFreqMhz = -999;
                            tmpAmpl = -999;
                            count_FOM = 0;
                            percent_FOM = -999;
                            totalPts = -999;
                            tmpAveRxLoss = 0;
                            tmpRxLoss = 0;
                            count = 0;

                            //if excluded soak sweep trace , need to remove the array[0] from PXITrace[testnumber].Multi_Trace[0]
                            bool excludeSoakSweep = false;
                            int traceCount = 0;

                            for (int i = 0; i < PXITrace.Length; i++)
                            {
                                if (Convert.ToInt16(_TestUsePrev) == PXITrace[i].TestNumber)
                                {
                                    excludeSoakSweep = PXITrace[i].SoakSweep;
                                    traceCount = PXITrace[i].TraceCount;
                                }
                            }

                            #region decode re-arrange multiple bandwidth (Ascending)
                            bw_cnt = 0;
                            tmpRBW_Hz = Array.ConvertAll(_PXI_MultiRBW.Split(','), double.Parse);  //split and convert string to double array
                            multiRBW_Hz = new double[tmpRBW_Hz.Length];

                            Array.Sort(tmpRBW_Hz);
                            foreach (double key in tmpRBW_Hz)
                            {
                                multiRBW_Hz[bw_cnt] = Convert.ToDouble(key);
                                bw_cnt++;
                            }

                            multiRBW_cnt = multiRBW_Hz.Length;
                            #endregion

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], _CalTag.ToUpper(), _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq with 1MHz step freq
                            count = Convert.ToInt16((_StopRXFreq1 - _StartRXFreq1) / 1);
                            _RXFreq = _StartRXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                tmpRxLoss = Math.Round(tmpRxLoss + (float)_LossOutputPathRX1, 3);   //need to use round function because of C# float and double floating point bug/error
                                _RXFreq = Convert.ToSingle(Math.Round(_RXFreq + 1, 3));             //need to use round function because of C# float and double floating point bug/error
                            }
                            tmpAveRxLoss = tmpRxLoss / (count + 1);
                            #endregion

                            if (_Test_NF1)
                            {
                                for (rbw_counter = 0; rbw_counter < multiRBW_cnt; rbw_counter++)
                                {
                                    for (int traceNo = 0; traceNo < traceCount; traceNo++)
                                    {
                                        string rbwParamName = null;
                                        string traceName = null;

                                        if ((excludeSoakSweep) && (traceNo == 0))
                                        {
                                            traceName = "Soak";
                                        }
                                        else
                                        {
                                            traceName = (traceNo + 1).ToString();
                                        }

                                        rbwParamName = "_" + Math.Abs(multiRBW_Hz[rbw_counter] / 1e6).ToString() + "MHz" + "_TR" + traceName;

                                        count_FOM = 0;
                                        percent_FOM = -999;
                                        pxiTrace_Ampl = -999;

                                        Read_PXI_SingleTrace(_TestUsePrev, traceNo, _StartRXFreq1, _StopRXFreq1, _StepRXFreq1, _Search_Method, _TestParam, rbw_counter, multiRBW_Hz[rbw_counter]);
                                        totalPts = Result_MXATrace.FreqMHz.Length - 1;

                                        for (int i = 0; i < Result_MXATrace.FreqMHz.Length; i++)
                                        {
                                            pxiTrace_Ampl = Result_MXATrace.Ampl[i] - tmpAveRxLoss - tbOutputLoss;

                                            switch (_Search_Method.ToUpper())
                                            {
                                                case "MAX":
                                                    if (pxiTrace_Ampl >= Convert.ToSingle(_Search_Value))
                                                    {
                                                        count_FOM++;
                                                    }
                                                    break;

                                                case "MIN":
                                                    if (pxiTrace_Ampl <= Convert.ToSingle(_Search_Value))
                                                    {
                                                        count_FOM++;
                                                    }
                                                    break;

                                                default:
                                                    MessageBox.Show("Test Parameter : " + _TestParam + "(" + _Search_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                                    break;
                                            }
                                        }

                                        percent_FOM = Math.Round(((count_FOM / totalPts) * 100), 3);
                                        R_NF1_Ampl = percent_FOM;
                                        R_NF1_Freq = 1;     //dummy data

                                        if (_Test_NF1)
                                        {
                                            ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                            ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                        }
                                    }
                                }
                            }

                            //Force test flag to false to ensure no repeated test data
                            //because we add to string builder upfront for PXI due to data reported base on number of sweep
                            _Test_NF1 = false;

                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            #endregion
                            break;

                        case "NF_MAX_MIN":
                            #region NF_MAX MIN calculation

                            TestUsePrev_Array = _TestUsePrev.Split(',');
                            ColdNF_TestCount = 0;
                            HotNF_TestCount = 0;

                            for (int i = 0; i < PXITrace.Length; i++)
                            {
                                if (Convert.ToInt16(TestUsePrev_Array[0]) == PXITrace[i].TestNumber)
                                {
                                    ColdNF_TestCount = i;
                                }

                                if (Convert.ToInt16(TestUsePrev_Array[1]) == PXITrace[i].TestNumber)
                                {
                                    HotNF_TestCount = i;
                                }
                            }

                            Nop_ColdNF = PXITrace[ColdNF_TestCount].Multi_Trace[0][0].NoPoints;
                            Nop_HotNF = PXITrace[HotNF_TestCount].Multi_Trace[0][0].NoPoints;
                            NumberOfRunsColdNF = PXITrace[ColdNF_TestCount].TraceCount;
                            NumberOfRunsHotNF = PXITrace[HotNF_TestCount].TraceCount;
                            RXPathLoss_Cold = new double[Nop_ColdNF];
                            RXPathLoss_Hot = new double[Nop_HotNF];

                            Cold_NF_new = new double[NumberOfRunsColdNF][];
                            Cold_NoisePower_new = new double[NumberOfRunsColdNF][];
                            Hot_NF_new = new double[NumberOfRunsHotNF][];
                            Hot_NoisePower_new = new double[NumberOfRunsHotNF][];

                            ResultMultiTrace_ColdNF = new s_TraceNo();
                            ResultMultiTrace_ColdNF.Ampl = new double[Nop_ColdNF];
                            ResultMultiTrace_ColdNF.FreqMHz = new double[Nop_ColdNF];

                            ResultMultiTrace_HotNF = new s_TraceNo();
                            ResultMultiTrace_HotNF.Ampl = new double[Nop_HotNF];
                            ResultMultiTrace_HotNF.FreqMHz = new double[Nop_HotNF];

                            ResultMultiTraceDelta = new s_TraceNo();
                            ResultMultiTraceDelta.Ampl = new double[Nop_HotNF];
                            ResultMultiTraceDelta.FreqMHz = new double[Nop_HotNF];

                            Dic_ColdNF = new Dictionary<double, double>();
                            Dic_HotNF = new Dictionary<double, double>();

                            MaxColdNFAmpl = 0;
                            MaxColdNFFreq = 0;
                            MaxHotNFAmpl = 0;
                            MaxHotNFFreq = 0;
                            MaxNFRiseAmpl = 0;
                            MaxNFRiseFreq = 0;
                            CalcData = 0;

                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand_HotNF.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            for (int i = 0; i < NumberOfRunsColdNF; i++)
                            {
                                Cold_NF_new[i] = new double[Nop_ColdNF];
                                Cold_NoisePower_new[i] = new double[Nop_ColdNF];
                            }

                            for (int i = 0; i < NumberOfRunsHotNF; i++)
                            {
                                Hot_NF_new[i] = new double[Nop_HotNF];
                                Hot_NoisePower_new[i] = new double[Nop_HotNF];
                            }

                            // Cold NF RX path loss gathering
                            for (int i = 0; i < Nop_ColdNF; i++)
                            {
                                _RXFreq = Convert.ToSingle(PXITrace[ColdNF_TestCount].Multi_Trace[0][0].FreqMHz[i]);
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                RXPathLoss_Cold[i] = _LossOutputPathRX1;
                            }

                            // Hot NF RX path loss gathering
                            for (int i = 0; i < Nop_HotNF; i++)
                            {
                                _RXFreq = Convert.ToSingle(PXITrace[HotNF_TestCount].Multi_Trace[0][0].FreqMHz[i]);
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                RXPathLoss_Hot[i] = _LossOutputPathRX1;
                            }

                            // Cold NF Fetch    
                            for (int i = 0; i < NumberOfRunsColdNF; i++)
                            {
                                Eq.Site[0]._EqRFmx.cRFmxNF.RetrieveResults_NFColdSource(ColdNF_TestCount, 0, "result::" + "COLD" + ColdNF_TestCount.ToString() + "_" + i);

                                for (int j = 0; j < Nop_ColdNF; j++)
                                {
                                    double Cold_NF_withoutGain = (Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j].ToString().Contains("NaN") || Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j].ToString().Contains("Infinity") || Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j].ToString().Contains("∞")) ? 9999 : Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j];

                                    if (Cold_NF_withoutGain == 9999 || PXITrace[ColdNF_TestCount].Multi_Trace[0][i].RxGain[j].ToString().Contains("Infinity") || PXITrace[ColdNF_TestCount].Multi_Trace[0][i].RxGain[j].ToString().Contains("NaN"))
                                    {
                                        Cold_NF_new[i][j] = 9999;
                                    }

                                    else
                                    {
                                        Cold_NF_new[i][j] = Cold_NF_withoutGain - (PXITrace[ColdNF_TestCount].Multi_Trace[0][i].RxGain[j]);
                                    }

                                    Cold_NoisePower_new[i][j] = Eq.Site[0]._EqRFmx.cRFmxNF.coldSourcePower[j] - RXPathLoss_Cold[j];

                                }
                            }

                            // Hot NF Fetch
                            for (int i = 0; i < NumberOfRunsHotNF; i++)
                            {
                                for (int j = 0; j < Nop_HotNF; j++)
                                {
                                    Eq.Site[0]._EqRFmx.cRFmxNF.RetrieveResults_NFColdSource(HotNF_TestCount, j, "result::" + "HOT" + HotNF_TestCount + "_" + i.ToString() + "_" + j.ToString());
                                    double Hot_NF_withoutGain = (Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[0].ToString().Contains("NaN") || Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[0].ToString().Contains("Infinity") || Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[0].ToString().Contains("∞")) ? 9999 : Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[0];

                                    if (Hot_NF_withoutGain == 9999 || PXITrace[HotNF_TestCount].Multi_Trace[0][i].RxGain[j].ToString().Contains("Infinity") || PXITrace[HotNF_TestCount].Multi_Trace[0][i].RxGain[j].ToString().Contains("NaN"))
                                    {
                                        Hot_NF_new[i][j] = 9999;
                                    }

                                    else
                                    {
                                        Hot_NF_new[i][j] = Hot_NF_withoutGain - (PXITrace[HotNF_TestCount].Multi_Trace[0][i].RxGain[j]);
                                    }

                                    Hot_NoisePower_new[i][j] = Eq.Site[0]._EqRFmx.cRFmxNF.coldSourcePower[0] - RXPathLoss_Hot[j];

                                }
                            }

                            // Store Cold & Hot NF data into PXI Trace
                            StoreNFdata(ColdNF_TestCount, NumberOfRunsColdNF, Nop_ColdNF, Cold_NF_new);
                            StoreNFdata(HotNF_TestCount, NumberOfRunsHotNF, Nop_HotNF, Hot_NF_new);
                            StoreNFRisedata(TestCount, ColdNF_TestCount, HotNF_TestCount, NumberOfRunsColdNF, NumberOfRunsHotNF, Nop_ColdNF, Nop_HotNF, _TestParaName, _TestNum);

                            // Save Cold & Hot NF Trace if Save_MXATrace is enabled
                            Save_PXI_NF_TraceRaw(_TestParaName + "_Cold-NF", ColdNF_TestCount, _Save_MXATrace, 0, PXITrace[ColdNF_TestCount].Multi_Trace[0][0].RBW_Hz);
                            Save_PXI_NF_TraceRaw(_TestParaName + "_Hot-NF", HotNF_TestCount, _Save_MXATrace, 0, PXITrace[HotNF_TestCount].Multi_Trace[0][0].RBW_Hz);
                            Save_PXI_NF_TraceRaw(_TestParaName + "_NF-Rise", TestCount, _Save_MXATrace, 0, PXITrace[HotNF_TestCount].Multi_Trace[0][0].RBW_Hz);

                            #region Calculate Result
                            //Calculate the result from the sorted data
                            for (istep = 0; istep < Nop_ColdNF; istep++)     //get MAX data for every noPtsUser out of multitrace (from "use previous" setting)
                            {
                                for (int i = 0; i < PXITrace[ColdNF_TestCount].TraceCount; i++)
                                {
                                    if (i == 0)
                                    {
                                        CalcData = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_ColdNF.Ampl[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_ColdNF.FreqMHz[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].FreqMHz[istep];
                                    }

                                    if (CalcData < PXITrace[ColdNF_TestCount].Multi_Trace[0][i].Ampl[istep])
                                    {
                                        ResultMultiTrace_ColdNF.Ampl[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_ColdNF.FreqMHz[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].FreqMHz[istep];
                                        CalcData = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                    }
                                }

                            }

                            for (istep = 0; istep < Nop_HotNF; istep++)     //get MAX data for every noPtsUser out of multitrace (from "use previous" setting)
                            {
                                for (int i = 0; i < PXITrace[HotNF_TestCount].TraceCount; i++)
                                {
                                    if (i == 0)
                                    {
                                        CalcData = PXITrace[HotNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_HotNF.Ampl[istep] = PXITrace[HotNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_HotNF.FreqMHz[istep] = PXITrace[HotNF_TestCount].Multi_Trace[0][i].FreqMHz[istep];
                                    }

                                    if (CalcData < PXITrace[HotNF_TestCount].Multi_Trace[0][i].Ampl[istep])
                                    {
                                        ResultMultiTrace_HotNF.Ampl[istep] = PXITrace[HotNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_HotNF.FreqMHz[istep] = PXITrace[HotNF_TestCount].Multi_Trace[0][i].FreqMHz[istep];
                                        CalcData = PXITrace[HotNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                    }
                                }
                            }

                            for (int i = 0; i < Nop_ColdNF; i++)
                            {
                                Dic_ColdNF.Add(ResultMultiTrace_ColdNF.FreqMHz[i], ResultMultiTrace_ColdNF.Ampl[i]);
                            }

                            for (int i = 0; i < Nop_HotNF; i++)
                            {
                                Dic_HotNF.Add(ResultMultiTrace_HotNF.FreqMHz[i], ResultMultiTrace_HotNF.Ampl[i]);
                            }

                            int nfCount = 0;
                            foreach (var nfvalue in Dic_HotNF)
                            {
                                try
                                {
                                    if (Dic_HotNF[nfvalue.Key].ToString() == ("9999") || Dic_ColdNF[nfvalue.Key].ToString() == ("9999"))
                                    {
                                        ResultMultiTraceDelta.Ampl[nfCount] = 9999;
                                        ResultMultiTraceDelta.FreqMHz[nfCount] = nfvalue.Key;
                                    }

                                    else
                                    {
                                        ResultMultiTraceDelta.Ampl[nfCount] = Dic_HotNF[nfvalue.Key] - Dic_ColdNF[nfvalue.Key];
                                        ResultMultiTraceDelta.FreqMHz[nfCount] = nfvalue.Key;
                                    }

                                    nfCount++;

                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.ToString());
                                }
                            }

                            MaxColdNFAmpl = ResultMultiTrace_ColdNF.Ampl.Max();
                            MaxColdNFFreq = ResultMultiTrace_ColdNF.FreqMHz[Array.IndexOf(ResultMultiTrace_ColdNF.Ampl, ResultMultiTrace_ColdNF.Ampl.Max())];

                            MaxHotNFAmpl = ResultMultiTrace_HotNF.Ampl.Max();
                            MaxHotNFFreq = ResultMultiTrace_HotNF.FreqMHz[Array.IndexOf(ResultMultiTrace_HotNF.Ampl, ResultMultiTrace_HotNF.Ampl.Max())];

                            MaxNFRiseAmpl = ResultMultiTraceDelta.Ampl.Max();
                            MaxNFRiseFreq = ResultMultiTraceDelta.FreqMHz[Array.IndexOf(ResultMultiTraceDelta.Ampl, ResultMultiTraceDelta.Ampl.Max())];

                            ResultMultiTraceDelta.TargetPout = PXITrace[HotNF_TestCount].Multi_Trace[0][0].TargetPout;
                            ResultMultiTraceDelta.modulation = PXITrace[HotNF_TestCount].Multi_Trace[0][0].modulation;
                            ResultMultiTraceDelta.waveform = PXITrace[HotNF_TestCount].Multi_Trace[0][0].waveform;

                            #endregion

                            #region Build Result

                            if (!b_GE_Header)
                            {
                                #region Standard Result Format

                                if (_Test_NF1)
                                {
                                    for (int i = 0; i < Nop_ColdNF; i++)
                                    {
                                        List<double> listColdPower = new List<double>();
                                        for (int j = 0; j < NumberOfRunsColdNF; j++)
                                        {
                                            listColdPower.Add(Cold_NoisePower_new[j][i]);
                                        }

                                        if (_Disp_ColdTrace)
                                        {
                                            ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_" + ResultMultiTrace_ColdNF.FreqMHz[i] + "_Cold-Power" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dBm", listColdPower.Max());
                                        }
                                    }

                                    for (int i = 0; i < Nop_HotNF; i++)
                                    {
                                        List<double> listHotPower = new List<double>();
                                        for (int j = 0; j < NumberOfRunsHotNF; j++)
                                        {
                                            listHotPower.Add(Hot_NoisePower_new[j][i]);
                                        }

                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_" + ResultMultiTrace_HotNF.FreqMHz[i] + "_Hot-Power" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", listHotPower.Max());
                                    }

                                    for (int i = 0; i < Nop_ColdNF; i++)
                                    {
                                        if (_Disp_ColdTrace)
                                        {
                                            ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_" + ResultMultiTrace_ColdNF.FreqMHz[i] + "_Cold-NF" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", ResultMultiTrace_ColdNF.Ampl[i]);
                                        }
                                    }

                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Ampl_" + "Cold-Max-NF" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", MaxColdNFAmpl);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Freq_" + "Cold-Max-NF" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", MaxColdNFFreq);

                                    for (int i = 0; i < Nop_HotNF; i++)
                                    {
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_" + ResultMultiTrace_HotNF.FreqMHz[i] + "_Hot-NF" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", ResultMultiTrace_HotNF.Ampl[i]);
                                    }

                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Ampl_" + "Hot-Max-NF" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", MaxHotNFAmpl);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Freq_" + "Hot-Max-NF" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", MaxHotNFFreq);

                                    for (istep = 0; istep < Nop_HotNF; istep++)
                                    {
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_" + ResultMultiTraceDelta.FreqMHz[istep] + "_NF-Rise" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", ResultMultiTraceDelta.Ampl[istep]);
                                    }


                                    double[] maxOfmaxNFRiseAmpl = new double[NumberOfRunsHotNF];
                                    double[] maxOfmaxNFRiseFreq = new double[NumberOfRunsHotNF];

                                    for (istep = 0; istep < NumberOfRunsHotNF; istep++)
                                    {
                                        double maxNFRiseAmpl = PXITrace[TestCount].Multi_Trace[0][istep].Ampl.Max();
                                        double maxNFRiseFreq = PXITrace[TestCount].Multi_Trace[0][istep].FreqMHz[Array.IndexOf(PXITrace[TestCount].Multi_Trace[0][istep].Ampl, PXITrace[TestCount].Multi_Trace[0][istep].Ampl.Max())];

                                        maxOfmaxNFRiseAmpl[istep] = maxNFRiseAmpl;
                                        maxOfmaxNFRiseFreq[istep] = maxNFRiseFreq;

                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Ampl_" + "NF-Max-Rise" + (istep + 1) + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", maxNFRiseAmpl);
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Freq_" + "NF-Max-Rise" + (istep + 1) + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", maxNFRiseFreq);
                                    }

                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Ampl_" + "NF-Max-Rise-ALL" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", maxOfmaxNFRiseAmpl.Max());
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Freq_" + "NF-Max-Rise-ALL" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", maxOfmaxNFRiseFreq[Array.IndexOf(maxOfmaxNFRiseAmpl, maxOfmaxNFRiseAmpl.Max())]);
                                }

                                //Force test flag to false to ensure no repeated test data
                                //because we add to string builder upfront for PXI due to data reported base on number of sweep

                                _Test_Pin1 = false;
                                _Test_Pout1 = false;
                                _Test_SMU = false;
                                _Test_NF1 = false;

                                #endregion
                            }
                            else
                            {
                                #region Golden Eagle Result Format
                                b_SmuHeader = true;
                                string GE_TestParam = null;
                                Rslt_GE_Header = new s_GE_Header();
                                Decode_GE_Header(TestPara, out Rslt_GE_Header);

                                //Calculate no of tx freq + step
                                double[] TXFreq_List;
                                count = Convert.ToInt16(Math.Ceiling((Math.Round(_StopRXFreq1, 3) - Math.Round(_StartRXFreq1, 3)) / Math.Round(_StepRXFreq1, 3)));
                                TXFreq_List = new double[count + 1];
                                _TXFreq = _StartTXFreq1;

                                if (_StartTXFreq1 == _StopTXFreq1)   //for NMAX method
                                {
                                    _SetRX1NDiag = false;
                                    Rslt_GE_Header.Note = "_NOTE_NMAX"; //re-assign ge header
                                }
                                else
                                {
                                    _SetRX1NDiag = true;
                                    Rslt_GE_Header.Note = "_NOTE_NDIAG";    //re-assign ge header
                                }

                                for (int i = 0; i <= count; i++)
                                {
                                    TXFreq_List[i] = Math.Round(_TXFreq, 3);
                                    _TXFreq = _TXFreq + _StepTXFreq1;

                                    if (!_SetRX1NDiag)   //for NMAX method
                                    {
                                        _TXFreq = _StartTXFreq1;
                                    }

                                    if (_TXFreq > _StopTXFreq1) //For Last Freq match
                                    {
                                        _TXFreq = _StopTXFreq1;
                                    }
                                }

                                if (_Test_NF1)
                                {
                                    for (int i = 0; i < Nop_ColdNF; i++)
                                    {
                                        List<double> listColdPower = new List<double>();
                                        for (int j = 0; j < NumberOfRunsColdNF; j++)
                                        {
                                            listColdPower.Add(Cold_NoisePower_new[j][i]);
                                        }

                                        if (_Disp_ColdTrace)
                                        {
                                            Rslt_GE_Header.Param = "_Power_Cold";      //re-assign ge header 
                                            Rslt_GE_Header.Freq1 = "_Rx-" + ResultMultiTrace_ColdNF.FreqMHz[i] + "MHz"; //re-assign ge header 

                                            Rslt_GE_Header.Waveform = "_x";
                                            Rslt_GE_Header.Modulation = "_x";
                                            Rslt_GE_Header.PType = "_x";
                                            Rslt_GE_Header.Pwr = "_x";

                                            Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                            ResultBuilder.BuildResults(ref results, GE_TestParam, "dBm", listColdPower.Max());
                                        }
                                    }

                                    for (int i = 0; i < Nop_HotNF; i++)
                                    {
                                        List<double> listHotPower = new List<double>();
                                        for (int j = 0; j < NumberOfRunsHotNF; j++)
                                        {
                                            listHotPower.Add(Hot_NoisePower_new[j][i]);
                                        }

                                        Rslt_GE_Header.Param = "_Power_Hot";      //re-assign ge header 
                                        Rslt_GE_Header.Freq1 = "_Tx-" + TXFreq_List[i] + "MHz"; //re-assign ge header 
                                        Rslt_GE_Header.Freq2 = "_Rx-" + ResultMultiTrace_HotNF.FreqMHz[i] + "MHz"; //re-assign ge header 

                                        Rslt_GE_Header.Waveform = "_" + ResultMultiTraceDelta.waveform;
                                        Rslt_GE_Header.Modulation = "_" + ResultMultiTraceDelta.modulation;
                                        Rslt_GE_Header.PType = "_FixedPout";
                                        Rslt_GE_Header.Pwr = "_" + ResultMultiTraceDelta.TargetPout + "dBm";

                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "dBm", listHotPower.Max());
                                    }

                                    for (int i = 0; i < Nop_ColdNF; i++)
                                    {
                                        if (_Disp_ColdTrace)
                                        {
                                            Rslt_GE_Header.Param = "_NF_Cold";      //re-assign ge header 
                                            Rslt_GE_Header.Freq1 = "_Rx-" + ResultMultiTrace_ColdNF.FreqMHz[i] + "MHz"; //re-assign ge header 

                                            Rslt_GE_Header.Waveform = "_x";
                                            Rslt_GE_Header.Modulation = "_x";
                                            Rslt_GE_Header.PType = "_x";
                                            Rslt_GE_Header.Pwr = "_x";

                                            Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                            ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", ResultMultiTrace_ColdNF.Ampl[i]);
                                        }
                                    }

                                    if (_Disp_ColdTrace)
                                    {
                                        Rslt_GE_Header.Param = "_NF_Cold-Ampl-Max";      //re-assign ge header 
                                        Rslt_GE_Header.Freq1 = "_x"; //re-assign ge header 
                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", MaxColdNFAmpl);

                                        Rslt_GE_Header.Param = "_NF_Cold-Freq-Max";      //re-assign ge header 
                                        Rslt_GE_Header.Freq1 = "_x"; //re-assign ge header 
                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "MHz", MaxColdNFFreq);
                                    }

                                    for (int i = 0; i < Nop_HotNF; i++)
                                    {
                                        Rslt_GE_Header.Param = "_NF_Hot";      //re-assign ge header 
                                        Rslt_GE_Header.Freq1 = "_Tx-" + TXFreq_List[i] + "MHz"; //re-assign ge header 
                                        Rslt_GE_Header.Freq2 = "_Rx-" + ResultMultiTrace_HotNF.FreqMHz[i] + "MHz"; //re-assign ge header 

                                        Rslt_GE_Header.Waveform = "_" + ResultMultiTraceDelta.waveform;
                                        Rslt_GE_Header.Modulation = "_" + ResultMultiTraceDelta.modulation;
                                        Rslt_GE_Header.PType = "_FixedPout";
                                        Rslt_GE_Header.Pwr = "_" + ResultMultiTraceDelta.TargetPout + "dBm";

                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", ResultMultiTrace_HotNF.Ampl[i]);
                                    }

                                    Rslt_GE_Header.Param = "_NF_Hot-Ampl-Max";      //re-assign ge header 
                                    Rslt_GE_Header.Freq1 = "_Tx-" + _StartTXFreq1 + "MHz"; //re-assign ge header 
                                    Rslt_GE_Header.Freq2 = "_Tx-" + _StopTXFreq1 + "MHz"; //re-assign ge header 

                                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                    ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", MaxHotNFAmpl);

                                    Rslt_GE_Header.Param = "_NF_Hot-Freq-Max";      //re-assign ge header 
                                    Rslt_GE_Header.Freq1 = "_Tx-" + _StartTXFreq1 + "MHz"; //re-assign ge header 
                                    Rslt_GE_Header.Freq2 = "_Tx-" + _StopTXFreq1 + "MHz"; //re-assign ge header 

                                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                    ResultBuilder.BuildResults(ref results, GE_TestParam, "MHz", MaxHotNFFreq);

                                    for (istep = 0; istep < Nop_HotNF; istep++)
                                    {
                                        Rslt_GE_Header.Param = "_NF_Rise";      //re-assign ge header 
                                        Rslt_GE_Header.Freq1 = "_Tx-" + TXFreq_List[istep] + "MHz"; //re-assign ge header 
                                        Rslt_GE_Header.Freq2 = "_Rx-" + ResultMultiTraceDelta.FreqMHz[istep] + "MHz"; //re-assign ge header 
                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", ResultMultiTraceDelta.Ampl[istep]);
                                    }


                                    double[] maxOfmaxNFRiseAmpl = new double[NumberOfRunsHotNF];
                                    double[] maxOfmaxNFRiseFreq = new double[NumberOfRunsHotNF];

                                    for (istep = 0; istep < NumberOfRunsHotNF; istep++)
                                    {
                                        double maxNFRiseAmpl = PXITrace[TestCount].Multi_Trace[0][istep].Ampl.Max();
                                        double maxNFRiseFreq = PXITrace[TestCount].Multi_Trace[0][istep].FreqMHz[Array.IndexOf(PXITrace[TestCount].Multi_Trace[0][istep].Ampl, PXITrace[TestCount].Multi_Trace[0][istep].Ampl.Max())];

                                        maxOfmaxNFRiseAmpl[istep] = maxNFRiseAmpl;
                                        maxOfmaxNFRiseFreq[istep] = maxNFRiseFreq;

                                        Rslt_GE_Header.Param = "_NF_Rise-Ampl-Max";      //re-assign ge header 
                                        Rslt_GE_Header.Freq1 = "_Tx-" + _StartTXFreq1 + "MHz"; //re-assign ge header 
                                        Rslt_GE_Header.Freq2 = "_Tx-" + _StopTXFreq1 + "MHz"; //re-assign ge header 
                                        Rslt_GE_Header.MeasInfo = "_MAX" + (istep + 1); //re-assign ge header

                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", maxNFRiseAmpl);

                                        Rslt_GE_Header.Param = "_NF_Rise-Freq-Max";      //re-assign ge header 
                                        Rslt_GE_Header.Freq1 = "_Tx-" + _StartTXFreq1 + "MHz"; //re-assign ge header 
                                        Rslt_GE_Header.Freq2 = "_Tx-" + _StopTXFreq1 + "MHz"; //re-assign ge header 
                                        Rslt_GE_Header.MeasInfo = "_MAX" + (istep + 1); //re-assign ge header

                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "MHz", maxNFRiseFreq);
                                    }

                                    Rslt_GE_Header.Param = "_NF_Rise-Ampl-Max";      //re-assign ge header 
                                    Rslt_GE_Header.Freq1 = "_Tx-" + _StartTXFreq1 + "MHz"; //re-assign ge header 
                                    Rslt_GE_Header.Freq2 = "_Tx-" + _StopTXFreq1 + "MHz"; //re-assign ge header 
                                    Rslt_GE_Header.MeasInfo = "_MAXALL"; //re-assign ge header

                                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                    ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", maxOfmaxNFRiseAmpl.Max());

                                    Rslt_GE_Header.Param = "_NF_Rise-Freq-Max";      //re-assign ge header 
                                    Rslt_GE_Header.Freq1 = "_Tx-" + _StartTXFreq1 + "MHz"; //re-assign ge header 
                                    Rslt_GE_Header.Freq2 = "_Tx-" + _StopTXFreq1 + "MHz"; //re-assign ge header 
                                    Rslt_GE_Header.MeasInfo = "_MAXALL"; //re-assign ge header

                                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                    ResultBuilder.BuildResults(ref results, GE_TestParam, "MHz", maxOfmaxNFRiseFreq[Array.IndexOf(maxOfmaxNFRiseAmpl, maxOfmaxNFRiseAmpl.Max())]);
                                }

                                //Force test flag to false to ensure no repeated test data
                                //because we add to string builder upfront for PXI due to data reported base on number of sweep

                                _Test_Pin1 = false;
                                _Test_Pout1 = false;
                                _Test_SMU = false;
                                _Test_NF1 = false;

                                #endregion
                            }


                            #endregion

                            tTime.Stop();
                            break;
                        #endregion

                        case "NF_MAX_MIN_COLD":
                            #region NF_MAX MIN calculation

                            ColdNF_TestCount = 0;

                            for (int i = 0; i < PXITrace.Length; i++)
                            {
                                if (Convert.ToInt16(_TestUsePrev) == PXITrace[i].TestNumber)
                                {
                                    ColdNF_TestCount = i;
                                }
                            }

                            Nop_ColdNF = PXITrace[ColdNF_TestCount].Multi_Trace[0][0].NoPoints;
                            NumberOfRunsColdNF = PXITrace[ColdNF_TestCount].TraceCount;
                            RXPathLoss_Cold = new double[Nop_ColdNF];

                            Cold_NF_new = new double[NumberOfRunsColdNF][];
                            Cold_NoisePower_new = new double[NumberOfRunsColdNF][];

                            ResultMultiTrace_ColdNF = new s_TraceNo();
                            ResultMultiTrace_ColdNF.Ampl = new double[Nop_ColdNF];
                            ResultMultiTrace_ColdNF.FreqMHz = new double[Nop_ColdNF];

                            Dic_ColdNF = new Dictionary<double, double>();

                            MaxColdNFAmpl = 0;
                            MaxColdNFFreq = 0;
                            CalcData = 0;

                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand_HotNF.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            for (int i = 0; i < NumberOfRunsColdNF; i++)
                            {
                                Cold_NF_new[i] = new double[Nop_ColdNF];
                                Cold_NoisePower_new[i] = new double[Nop_ColdNF];
                            }

                            // Cold NF RX path loss gathering
                            for (int i = 0; i < Nop_ColdNF; i++)
                            {
                                _RXFreq = Convert.ToSingle(PXITrace[ColdNF_TestCount].Multi_Trace[0][0].FreqMHz[i]);
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                RXPathLoss_Cold[i] = _LossOutputPathRX1;
                            }

                            // Cold NF Fetch    
                            for (int i = 0; i < NumberOfRunsColdNF; i++)
                            {
                                Eq.Site[0]._EqRFmx.cRFmxNF.RetrieveResults_NFColdSource(ColdNF_TestCount, 0, "result::" + "COLD" + ColdNF_TestCount.ToString() + "_" + i);

                                for (int j = 0; j < Nop_ColdNF; j++)
                                {
                                    double Cold_NF_withoutGain = (Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j].ToString().Contains("NaN") || Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j].ToString().Contains("Infinity") || Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j].ToString().Contains("∞")) ? 9999 : Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j];

                                    if (Cold_NF_withoutGain == 9999 || PXITrace[ColdNF_TestCount].Multi_Trace[0][i].RxGain[j].ToString().Contains("Infinity") || PXITrace[ColdNF_TestCount].Multi_Trace[0][i].RxGain[j].ToString().Contains("NaN"))
                                    {
                                        Cold_NF_new[i][j] = 9999;
                                    }

                                    else
                                    {
                                        Cold_NF_new[i][j] = Cold_NF_withoutGain - (PXITrace[ColdNF_TestCount].Multi_Trace[0][i].RxGain[j]);
                                    }

                                    Cold_NoisePower_new[i][j] = Eq.Site[0]._EqRFmx.cRFmxNF.coldSourcePower[j] - RXPathLoss_Cold[j];

                                }
                            }

                            // Store Cold & Hot NF data into PXI Trace
                            StoreNFdata(ColdNF_TestCount, NumberOfRunsColdNF, Nop_ColdNF, Cold_NF_new);

                            // Save Cold & Hot NF Trace if Save_MXATrace is enabled
                            Save_PXI_NF_TraceRaw(_TestParaName + "_Cold-NF", ColdNF_TestCount, _Save_MXATrace, 0, PXITrace[ColdNF_TestCount].Multi_Trace[0][0].RBW_Hz);

                            #region Calculate Result
                            //Calculate the result from the sorted data
                            for (istep = 0; istep < Nop_ColdNF; istep++)     //get MAX data for every noPtsUser out of multitrace (from "use previous" setting)
                            {
                                for (int i = 0; i < PXITrace[ColdNF_TestCount].TraceCount; i++)
                                {
                                    if (i == 0)
                                    {
                                        CalcData = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_ColdNF.Ampl[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_ColdNF.FreqMHz[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].FreqMHz[istep];
                                    }

                                    if (CalcData < PXITrace[ColdNF_TestCount].Multi_Trace[0][i].Ampl[istep])
                                    {
                                        ResultMultiTrace_ColdNF.Ampl[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_ColdNF.FreqMHz[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].FreqMHz[istep];
                                        CalcData = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                    }
                                }

                            }

                            for (int i = 0; i < Nop_ColdNF; i++)
                            {
                                Dic_ColdNF.Add(ResultMultiTrace_ColdNF.FreqMHz[i], ResultMultiTrace_ColdNF.Ampl[i]);
                            }

                            MaxColdNFAmpl = ResultMultiTrace_ColdNF.Ampl.Max();
                            MaxColdNFFreq = ResultMultiTrace_ColdNF.FreqMHz[Array.IndexOf(ResultMultiTrace_ColdNF.Ampl, ResultMultiTrace_ColdNF.Ampl.Max())];


                            #endregion

                            #region Build Result
                            if (!b_GE_Header)
                            {
                                #region Standard Result Format
                                if (_Test_NF1)
                                {
                                    for (int i = 0; i < Nop_ColdNF; i++)
                                    {
                                        List<double> listColdPower = new List<double>();
                                        for (int j = 0; j < NumberOfRunsColdNF; j++)
                                        {
                                            listColdPower.Add(Cold_NoisePower_new[j][i]);
                                        }

                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_" + ResultMultiTrace_ColdNF.FreqMHz[i] + "_Cold-Power" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dBm", listColdPower.Max());
                                    }

                                    for (int i = 0; i < Nop_ColdNF; i++)
                                    {
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_" + ResultMultiTrace_ColdNF.FreqMHz[i] + "_Cold-NF" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", ResultMultiTrace_ColdNF.Ampl[i]);
                                    }

                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Ampl_" + "Cold-Max-NF" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", MaxColdNFAmpl);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Freq_" + "Cold-Max-NF" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", MaxColdNFFreq);

                                }

                                //Force test flag to false to ensure no repeated test data
                                //because we add to string builder upfront for PXI due to data reported base on number of sweep

                                _Test_Pin1 = false;
                                _Test_Pout1 = false;
                                _Test_SMU = false;
                                _Test_NF1 = false;

                                break;
                                #endregion
                            }
                            else
                            {
                                #region Golden Eagle Result Format
                                b_SmuHeader = true;
                                string GE_TestParam = null;
                                Rslt_GE_Header = new s_GE_Header();
                                Decode_GE_Header(TestPara, out Rslt_GE_Header);

                                //Calculate no of tx freq + step
                                double[] TXFreq_List;
                                count = Convert.ToInt16(Math.Ceiling((Math.Round(_StopRXFreq1, 3) - Math.Round(_StartRXFreq1, 3)) / Math.Round(_StepRXFreq1, 3)));
                                TXFreq_List = new double[count + 1];
                                _TXFreq = _StartTXFreq1;

                                if (_StartTXFreq1 == _StopTXFreq1)   //for NMAX method
                                {
                                    _SetRX1NDiag = false;
                                    Rslt_GE_Header.Note = "_NOTE_NMAX"; //re-assign ge header
                                }
                                else
                                {
                                    _SetRX1NDiag = true;
                                    Rslt_GE_Header.Note = "_NOTE_NDIAG";    //re-assign ge header
                                }

                                for (int i = 0; i <= count; i++)
                                {
                                    TXFreq_List[i] = _TXFreq;
                                    _TXFreq = _TXFreq + _StepTXFreq1;

                                    if (!_SetRX1NDiag)   //for NMAX method
                                    {
                                        _TXFreq = _StartTXFreq1;
                                    }

                                    if (_TXFreq > _StopTXFreq1) //For Last Freq match
                                    {
                                        _TXFreq = _StopTXFreq1;
                                    }
                                }

                                if (_Test_NF1)
                                {
                                    for (int i = 0; i < Nop_ColdNF; i++)
                                    {
                                        List<double> listColdPower = new List<double>();
                                        for (int j = 0; j < NumberOfRunsColdNF; j++)
                                        {
                                            listColdPower.Add(Cold_NoisePower_new[j][i]);
                                        }

                                        if (_Disp_ColdTrace)
                                        {
                                            Rslt_GE_Header.Param = "_Power_Cold";      //re-assign ge header 
                                            Rslt_GE_Header.Freq1 = "_Rx-" + ResultMultiTrace_ColdNF.FreqMHz[i] + "MHz"; //re-assign ge header 
                                            Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                            ResultBuilder.BuildResults(ref results, GE_TestParam, "dBm", listColdPower.Max());
                                        }
                                    }

                                    for (int i = 0; i < Nop_ColdNF; i++)
                                    {
                                        if (_Disp_ColdTrace)
                                        {
                                            Rslt_GE_Header.Param = "_NF_Cold";      //re-assign ge header 
                                            Rslt_GE_Header.Freq1 = "_Rx-" + ResultMultiTrace_ColdNF.FreqMHz[i] + "MHz"; //re-assign ge header 
                                            Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                            ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", ResultMultiTrace_ColdNF.Ampl[i]);
                                        }
                                    }

                                    Rslt_GE_Header.Param = "_NF_Cold-Ampl-Max";      //re-assign ge header 
                                    Rslt_GE_Header.Freq1 = "_x"; //re-assign ge header 
                                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                    ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", MaxColdNFAmpl);

                                    Rslt_GE_Header.Param = "_NF_Cold-Freq-Max";      //re-assign ge header 
                                    Rslt_GE_Header.Freq1 = "_x"; //re-assign ge header 
                                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                    ResultBuilder.BuildResults(ref results, GE_TestParam, "MHz", MaxColdNFFreq);
                                }
                                //Force test flag to false to ensure no repeated test data
                                //because we add to string builder upfront for PXI due to data reported base on number of sweep

                                _Test_Pin1 = false;
                                _Test_Pout1 = false;
                                _Test_SMU = false;
                                _Test_NF1 = false;

                                #endregion
                            }

                            #endregion

                            tTime.Stop();
                            break;
                        #endregion

                        case "NF_MAX_MIN_MIPI":
                            #region NF_MAX MIN calculation

                            TestUsePrev_Array = _TestUsePrev.Split(',');

                            ColdNF_TestCount = 0;
                            ColdMIPINF_TestCount = 0;
                            int ColdNF_ArryNum = 0;
                            int ColdNFMIPI_ArryNum = 0;

                            for (int i = 0; i < PXITrace.Length; i++)
                            {
                                if (TestUsePrev_Array.Count() > 1)
                                {
                                    if (Convert.ToInt16(TestUsePrev_Array[0]) == PXITrace[i].TestNumber) ColdNF_TestCount = i;
                                    else if (Convert.ToInt16(TestUsePrev_Array[1]) == PXITrace[i].TestNumber) ColdMIPINF_TestCount = i;
                                }
                                else
                                {
                                    if (Convert.ToInt16(_TestUsePrev) == PXITrace[i].TestNumber)
                                    {
                                        ColdNF_TestCount = i;
                                        ColdMIPINF_TestCount = i;
                                        ColdNFMIPI_ArryNum = 1;
                                    }
                                }
                            }

                            Nop_ColdNF = PXITrace[ColdNF_TestCount].Multi_Trace[0][0].NoPoints;
                            Nop_ColdMIPINF = PXITrace[ColdMIPINF_TestCount].Multi_Trace[0][0].NoPoints;
                            NumberOfRunsColdNF = PXITrace[ColdNF_TestCount].TraceCount;
                            NumberOfRunsColdMIPINF = PXITrace[ColdMIPINF_TestCount].TraceCount;
                            RXPathLoss_Cold = new double[Nop_ColdNF];
                            RXPathLoss_ColdMIPI = new double[Nop_ColdMIPINF];

                            Cold_NF_new = new double[NumberOfRunsColdNF][];
                            Cold_NoisePower_new = new double[NumberOfRunsColdNF][];
                            Cold_MIPI_NF_new = new double[NumberOfRunsColdMIPINF][];
                            Cold_MIPI_NoisePower_new = new double[NumberOfRunsColdMIPINF][];

                            ResultMultiTrace_ColdNF = new s_TraceNo();
                            ResultMultiTrace_ColdNF.Ampl = new double[Nop_ColdNF];
                            ResultMultiTrace_ColdNF.FreqMHz = new double[Nop_ColdNF];

                            ResultMultiTrace_ColdMIPINF = new s_TraceNo();
                            ResultMultiTrace_ColdMIPINF.Ampl = new double[Nop_ColdMIPINF];
                            ResultMultiTrace_ColdMIPINF.FreqMHz = new double[Nop_ColdMIPINF];

                            ResultMultiTraceDelta = new s_TraceNo();
                            ResultMultiTraceDelta.Ampl = new double[Nop_ColdMIPINF];
                            ResultMultiTraceDelta.FreqMHz = new double[Nop_ColdMIPINF];

                            Dic_ColdNF = new Dictionary<double, double>();
                            Dic_ColdMIPINF = new Dictionary<double, double>();

                            MaxColdNFAmpl = 0;
                            MaxColdNFFreq = 0;
                            MaxColdMIPINFAmpl = 0;
                            MaxColdMIPINFFreq = 0;
                            MaxNFMIPIRiseAmpl = 0;
                            MaxNFMIPIRiseFreq = 0;
                            CalcData = 0;

                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand_HotNF.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            for (int i = 0; i < NumberOfRunsColdNF; i++)
                            {
                                Cold_NF_new[i] = new double[Nop_ColdNF];
                                Cold_NoisePower_new[i] = new double[Nop_ColdNF];
                            }

                            for (int i = 0; i < NumberOfRunsColdMIPINF; i++)
                            {
                                Cold_MIPI_NF_new[i] = new double[Nop_ColdMIPINF];
                                Cold_MIPI_NoisePower_new[i] = new double[Nop_ColdMIPINF];
                            }

                            // Cold NF RX path loss gathering
                            for (int i = 0; i < Nop_ColdNF; i++)
                            {
                                _RXFreq = Convert.ToSingle(PXITrace[ColdNF_TestCount].Multi_Trace[ColdNF_ArryNum][0].FreqMHz[i]);
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                RXPathLoss_Cold[i] = _LossOutputPathRX1;
                            }

                            // Cold-MIPI NF RX path loss gathering
                            for (int i = 0; i < Nop_ColdMIPINF; i++)
                            {
                                _RXFreq = Convert.ToSingle(PXITrace[ColdMIPINF_TestCount].Multi_Trace[ColdNFMIPI_ArryNum][0].FreqMHz[i]);
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                RXPathLoss_ColdMIPI[i] = _LossOutputPathRX1;
                            }

                            // Cold NF Fetch    
                            for (int i = 0; i < NumberOfRunsColdNF; i++)
                            {
                                Eq.Site[0]._EqRFmx.cRFmxNF.RetrieveResults_NFColdSource(ColdNF_TestCount, 0, "result::" + "COLD" + ColdNF_TestCount.ToString() + "_" + i);

                                for (int j = 0; j < Nop_ColdNF; j++)
                                {
                                    double Cold_NF_withoutGain = (Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j].ToString().Contains("NaN") || Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j].ToString().Contains("Infinity")) ? 9999 : Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j];

                                    if (Cold_NF_withoutGain == 9999 || PXITrace[ColdNF_TestCount].Multi_Trace[ColdNF_ArryNum][i].RxGain[j].ToString().Contains("Infinity") || PXITrace[ColdNF_TestCount].Multi_Trace[ColdNF_ArryNum][i].RxGain[j].ToString().Contains("NaN"))
                                    {
                                        Cold_NF_new[i][j] = 9999;
                                    }

                                    else
                                    {
                                        Cold_NF_new[i][j] = Cold_NF_withoutGain - (PXITrace[ColdNF_TestCount].Multi_Trace[ColdNF_ArryNum][i].RxGain[j]);
                                    }

                                    Cold_NoisePower_new[i][j] = Eq.Site[0]._EqRFmx.cRFmxNF.coldSourcePower[j] - RXPathLoss_Cold[j];

                                }
                            }

                            // Cold-MIPI NF Fetch
                            for (int i = 0; i < NumberOfRunsColdMIPINF; i++)
                            {
                                for (int j = 0; j < Nop_ColdMIPINF; j++)
                                {
                                    Eq.Site[0]._EqRFmx.cRFmxNF.RetrieveResults_NFColdSource(ColdMIPINF_TestCount, j + ColdNFMIPI_ArryNum, "result::" + "COLD_MIPI" + ColdMIPINF_TestCount + "_" + i.ToString() + "_" + j.ToString());

                                    double Cold_MIPI_NF_withoutGain = (Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[0].ToString().Contains("NaN") || Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[0].ToString().Contains("Infinity")) ? 9999 : Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[0];

                                    if (Cold_MIPI_NF_withoutGain == 9999 || PXITrace[ColdMIPINF_TestCount].Multi_Trace[ColdNFMIPI_ArryNum][i].RxGain[j].ToString().Contains("Infinity") || PXITrace[ColdMIPINF_TestCount].Multi_Trace[ColdNFMIPI_ArryNum][i].RxGain[j].ToString().Contains("NaN"))
                                    {
                                        Cold_MIPI_NF_new[i][j] = 9999;
                                    }

                                    else
                                    {
                                        Cold_MIPI_NF_new[i][j] = Cold_MIPI_NF_withoutGain - (PXITrace[ColdMIPINF_TestCount].Multi_Trace[ColdNFMIPI_ArryNum][i].RxGain[j]);
                                    }

                                    Cold_MIPI_NoisePower_new[i][j] = Eq.Site[0]._EqRFmx.cRFmxNF.coldSourcePower[0] - RXPathLoss_ColdMIPI[j];

                                }
                            }

                            // Store Cold & Cold-MIPI NF data into PXI Trace
                            StoreNFdata(ColdNF_TestCount, NumberOfRunsColdNF, Nop_ColdNF, Cold_NF_new, ColdNF_ArryNum);
                            StoreNFdata(ColdMIPINF_TestCount, NumberOfRunsColdMIPINF, Nop_ColdMIPINF, Cold_MIPI_NF_new, ColdNFMIPI_ArryNum);
                            StoreNFRisedata(TestCount, ColdNF_TestCount, ColdMIPINF_TestCount, NumberOfRunsColdNF, NumberOfRunsColdMIPINF, Nop_ColdNF, Nop_ColdMIPINF, _TestParaName, _TestNum, ColdNF_ArryNum, ColdNFMIPI_ArryNum);

                            // Save Cold & Cold-MIPI NF Trace if Save_MXATrace is enabled
                            Save_PXI_NF_TraceRaw(_TestParaName + "_Cold-NF", ColdNF_TestCount, _Save_MXATrace, 0, PXITrace[ColdNF_TestCount].Multi_Trace[ColdNF_ArryNum][0].RBW_Hz);
                            Save_PXI_NF_TraceRaw(_TestParaName + "_Cold-MIPI-NF", ColdMIPINF_TestCount, _Save_MXATrace, 0, PXITrace[ColdMIPINF_TestCount].Multi_Trace[ColdNFMIPI_ArryNum][0].RBW_Hz);
                            Save_PXI_NF_TraceRaw(_TestParaName + "_NF-MIPI-Rise", TestCount, _Save_MXATrace, 0, PXITrace[ColdMIPINF_TestCount].Multi_Trace[ColdNFMIPI_ArryNum][0].RBW_Hz);

                            #region Calculate Result
                            //Calculate the result from the sorted data
                            for (istep = 0; istep < Nop_ColdNF; istep++)     //get MAX data for every noPtsUser out of multitrace (from "use previous" setting)
                            {
                                for (int i = 0; i < PXITrace[ColdNF_TestCount].TraceCount; i++)
                                {
                                    if (i == 0)
                                    {
                                        CalcData = PXITrace[ColdNF_TestCount].Multi_Trace[ColdNF_ArryNum][i].Ampl[istep];
                                        ResultMultiTrace_ColdNF.Ampl[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[ColdNF_ArryNum][i].Ampl[istep];
                                        ResultMultiTrace_ColdNF.FreqMHz[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[ColdNF_ArryNum][i].FreqMHz[istep];
                                    }

                                    if (CalcData < PXITrace[ColdNF_TestCount].Multi_Trace[ColdNF_ArryNum][i].Ampl[istep])
                                    {
                                        ResultMultiTrace_ColdNF.Ampl[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[ColdNF_ArryNum][i].Ampl[istep];
                                        ResultMultiTrace_ColdNF.FreqMHz[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[ColdNF_ArryNum][i].FreqMHz[istep];
                                        CalcData = PXITrace[ColdNF_TestCount].Multi_Trace[ColdNF_ArryNum][i].Ampl[istep];
                                    }
                                }

                            }

                            for (istep = 0; istep < Nop_ColdMIPINF; istep++)     //get MAX data for every noPtsUser out of multitrace (from "use previous" setting)
                            {
                                for (int i = 0; i < PXITrace[ColdMIPINF_TestCount].TraceCount; i++)
                                {
                                    if (i == 0)
                                    {
                                        CalcData = PXITrace[ColdMIPINF_TestCount].Multi_Trace[ColdNFMIPI_ArryNum][i].Ampl[istep];
                                        ResultMultiTrace_ColdMIPINF.Ampl[istep] = PXITrace[ColdMIPINF_TestCount].Multi_Trace[ColdNFMIPI_ArryNum][i].Ampl[istep];
                                        ResultMultiTrace_ColdMIPINF.FreqMHz[istep] = PXITrace[ColdMIPINF_TestCount].Multi_Trace[ColdNFMIPI_ArryNum][i].FreqMHz[istep];
                                    }

                                    if (CalcData < PXITrace[ColdMIPINF_TestCount].Multi_Trace[ColdNFMIPI_ArryNum][i].Ampl[istep])
                                    {
                                        ResultMultiTrace_ColdMIPINF.Ampl[istep] = PXITrace[ColdMIPINF_TestCount].Multi_Trace[ColdNFMIPI_ArryNum][i].Ampl[istep];
                                        ResultMultiTrace_ColdMIPINF.FreqMHz[istep] = PXITrace[ColdMIPINF_TestCount].Multi_Trace[ColdNFMIPI_ArryNum][i].FreqMHz[istep];
                                        CalcData = PXITrace[ColdMIPINF_TestCount].Multi_Trace[ColdNFMIPI_ArryNum][i].Ampl[istep];
                                    }
                                }
                            }

                            for (int i = 0; i < Nop_ColdNF; i++)
                            {
                                Dic_ColdNF.Add(ResultMultiTrace_ColdNF.FreqMHz[i], ResultMultiTrace_ColdNF.Ampl[i]);
                            }

                            for (int i = 0; i < Nop_ColdMIPINF; i++)
                            {
                                Dic_ColdMIPINF.Add(ResultMultiTrace_ColdMIPINF.FreqMHz[i], ResultMultiTrace_ColdMIPINF.Ampl[i]);
                            }

                            int nfCount_NF_MIPI_RISE = 0;
                            foreach (var nfvalue in Dic_ColdMIPINF)
                            {
                                try
                                {
                                    if (Dic_ColdMIPINF[nfvalue.Key].ToString() == ("9999") || Dic_ColdNF[nfvalue.Key].ToString() == ("9999"))
                                    {
                                        ResultMultiTraceDelta.Ampl[nfCount_NF_MIPI_RISE] = 9999;
                                        ResultMultiTraceDelta.FreqMHz[nfCount_NF_MIPI_RISE] = nfvalue.Key;
                                    }

                                    else
                                    {
                                        ResultMultiTraceDelta.Ampl[nfCount_NF_MIPI_RISE] = Dic_ColdMIPINF[nfvalue.Key] - Dic_ColdNF[nfvalue.Key];
                                        ResultMultiTraceDelta.FreqMHz[nfCount_NF_MIPI_RISE] = nfvalue.Key;
                                    }

                                    nfCount_NF_MIPI_RISE++;

                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show(e.ToString());
                                }
                            }

                            MaxColdNFAmpl = ResultMultiTrace_ColdNF.Ampl.Max();
                            MaxColdNFFreq = ResultMultiTrace_ColdNF.FreqMHz[Array.IndexOf(ResultMultiTrace_ColdNF.Ampl, ResultMultiTrace_ColdNF.Ampl.Max())];

                            MaxColdMIPINFAmpl = ResultMultiTrace_ColdMIPINF.Ampl.Max();
                            MaxColdMIPINFFreq = ResultMultiTrace_ColdMIPINF.FreqMHz[Array.IndexOf(ResultMultiTrace_ColdMIPINF.Ampl, ResultMultiTrace_ColdMIPINF.Ampl.Max())];

                            MaxNFMIPIRiseAmpl = ResultMultiTraceDelta.Ampl.Max();
                            MaxNFMIPIRiseFreq = ResultMultiTraceDelta.FreqMHz[Array.IndexOf(ResultMultiTraceDelta.Ampl, ResultMultiTraceDelta.Ampl.Max())];
                            #endregion

                            #region Build Result

                            if (!b_GE_Header)
                            {
                                #region Standard Result Format

                                if (_Test_NF1)
                                {
                                    for (int i = 0; i < Nop_ColdNF; i++)
                                    {
                                        List<double> listColdPower = new List<double>();
                                        for (int j = 0; j < NumberOfRunsColdNF; j++)
                                        {
                                            listColdPower.Add(Cold_NoisePower_new[j][i]);
                                        }

                                        if (_Disp_ColdTrace)
                                        {
                                            ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_" + ResultMultiTrace_ColdNF.FreqMHz[i] + "_Cold-Power", "dBm", listColdPower.Max());
                                        }
                                    }

                                    for (int i = 0; i < Nop_ColdMIPINF; i++)
                                    {
                                        List<double> listColdMIPIPower = new List<double>();
                                        for (int j = 0; j < NumberOfRunsColdMIPINF; j++)
                                        {
                                            listColdMIPIPower.Add(Cold_MIPI_NoisePower_new[j][i]);
                                        }

                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_" + ResultMultiTrace_ColdMIPINF.FreqMHz[i] + "_Cold-MIPI-Power", "dB", listColdMIPIPower.Max());
                                    }

                                    for (int i = 0; i < Nop_ColdNF; i++)
                                    {
                                        if (_Disp_ColdTrace)
                                        {
                                            ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_" + ResultMultiTrace_ColdNF.FreqMHz[i] + "_Cold-NF", "dB", ResultMultiTrace_ColdNF.Ampl[i]);
                                        }
                                    }

                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Ampl_" + "Cold-Max-NF" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", MaxColdNFAmpl);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Freq_" + "Cold-Max-NF" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", MaxColdNFFreq);

                                    for (int i = 0; i < Nop_ColdMIPINF; i++)
                                    {
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_" + ResultMultiTrace_ColdMIPINF.FreqMHz[i] + "_Cold-MIPI-NF", "dB", ResultMultiTrace_ColdMIPINF.Ampl[i]);
                                    }

                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Ampl_" + "Cold-MIPI-Max-NF", "dB", MaxColdMIPINFAmpl);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Freq_" + "Cold-MIPI-Max-NF", "dB", MaxColdMIPINFFreq);

                                    for (istep = 0; istep < Nop_ColdMIPINF; istep++)
                                    {
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_" + ResultMultiTraceDelta.FreqMHz[istep] + "_NF-MIPI-Rise", "dB", ResultMultiTraceDelta.Ampl[istep]);
                                    }


                                    double[] maxOfmaxNFMIPIRiseAmpl = new double[NumberOfRunsColdMIPINF];
                                    double[] maxOfmaxNFMIPIRiseFreq = new double[NumberOfRunsColdMIPINF];

                                    for (istep = 0; istep < NumberOfRunsColdMIPINF; istep++)
                                    {
                                        double maxNFMIPIRiseAmpl = PXITrace[TestCount].Multi_Trace[0][istep].Ampl.Max();
                                        double maxNFMIPIRiseFreq = PXITrace[TestCount].Multi_Trace[0][istep].FreqMHz[Array.IndexOf(PXITrace[TestCount].Multi_Trace[0][istep].Ampl, PXITrace[TestCount].Multi_Trace[0][istep].Ampl.Max())];

                                        maxOfmaxNFMIPIRiseAmpl[istep] = maxNFMIPIRiseAmpl;
                                        maxOfmaxNFMIPIRiseFreq[istep] = maxNFMIPIRiseFreq;

                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Ampl_" + "NF-MIPI-Max-Rise" + (istep + 1), "dB", maxNFMIPIRiseAmpl);
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Freq_" + "NF-MIPI-Max-Rise" + (istep + 1), "dB", maxNFMIPIRiseFreq);
                                    }

                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Ampl_" + "NF-MIPI-Max-Rise-ALL", "dB", maxOfmaxNFMIPIRiseAmpl.Max());
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Freq_" + "NF-MIPI-Max-Rise-ALL", "dB", maxOfmaxNFMIPIRiseFreq[Array.IndexOf(maxOfmaxNFMIPIRiseAmpl, maxOfmaxNFMIPIRiseAmpl.Max())]);
                                }

                                //Force test flag to false to ensure no repeated test data
                                //because we add to string builder upfront for PXI due to data reported base on number of sweep

                                _Test_SMU = false;
                                _Test_NF1 = false;

                                #endregion
                            }
                            else
                            {
                                #region Golden Eagle Result Format
                                b_SmuHeader = true;
                                string GE_TestParam = null;
                                Rslt_GE_Header = new s_GE_Header();
                                Decode_GE_Header(TestPara, out Rslt_GE_Header);

                                //Calculate no of tx freq + step

                                if (_Test_NF1)
                                {
                                    for (int i = 0; i < Nop_ColdNF; i++)
                                    {
                                        List<double> listColdPower = new List<double>();
                                        for (int j = 0; j < NumberOfRunsColdNF; j++)
                                        {
                                            listColdPower.Add(Cold_NoisePower_new[j][i]);
                                        }

                                        if (_Disp_ColdTrace)
                                        {
                                            Rslt_GE_Header.Param = "_Power_Cold";      //re-assign ge header 
                                            Rslt_GE_Header.Freq1 = "_Rx-" + ResultMultiTrace_ColdNF.FreqMHz[i] + "MHz"; //re-assign ge header 
                                            Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                            ResultBuilder.BuildResults(ref results, GE_TestParam, "dBm", listColdPower.Max());
                                        }
                                    }

                                    for (int i = 0; i < Nop_ColdMIPINF; i++)
                                    {
                                        List<double> listColdMIPIPower = new List<double>();
                                        for (int j = 0; j < NumberOfRunsColdMIPINF; j++)
                                        {
                                            listColdMIPIPower.Add(Cold_MIPI_NoisePower_new[j][i]);
                                        }

                                        Rslt_GE_Header.Param = "_Power_Cold-MIPI";      //re-assign ge header 
                                        Rslt_GE_Header.Freq1 = "_Rx-" + ResultMultiTrace_ColdMIPINF.FreqMHz[i] + "MHz"; //re-assign ge header 
                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "dBm", listColdMIPIPower.Max());
                                    }

                                    for (int i = 0; i < Nop_ColdNF; i++)
                                    {
                                        if (_Disp_ColdTrace)
                                        {
                                            Rslt_GE_Header.Param = "_NF_Cold";      //re-assign ge header 
                                            Rslt_GE_Header.Freq1 = "_Rx-" + ResultMultiTrace_ColdNF.FreqMHz[i] + "MHz"; //re-assign ge header 
                                            Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                            ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", ResultMultiTrace_ColdNF.Ampl[i]);
                                        }
                                    }

                                    if (_Disp_ColdTrace)
                                    {
                                        Rslt_GE_Header.Param = "_NF_Cold-Ampl-Max";      //re-assign ge header 
                                        Rslt_GE_Header.Freq1 = "_x"; //re-assign ge header 
                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", MaxColdNFAmpl);

                                        Rslt_GE_Header.Param = "_NF_Cold-Freq-Max";      //re-assign ge header 
                                        Rslt_GE_Header.Freq1 = "_x"; //re-assign ge header 
                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "MHz", MaxColdNFFreq);
                                    }

                                    for (int i = 0; i < Nop_ColdMIPINF; i++)
                                    {
                                        Rslt_GE_Header.Param = "_NF_Cold-MIPI";      //re-assign ge header 
                                        Rslt_GE_Header.Freq1 = "_Rx-" + ResultMultiTrace_ColdMIPINF.FreqMHz[i] + "MHz"; //re-assign ge header 
                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", ResultMultiTrace_ColdMIPINF.Ampl[i]);
                                    }

                                    Rslt_GE_Header.Param = "_NF_Cold-MIPI-Ampl-Max";      //re-assign ge header 
                                    Rslt_GE_Header.Freq1 = "_Rx-" + _StartRXFreq1 + "MHz"; //re-assign ge header 
                                    Rslt_GE_Header.Freq2 = "_Rx-" + _StopRXFreq1 + "MHz"; //re-assign ge header 
                                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                    ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", MaxColdMIPINFAmpl);

                                    Rslt_GE_Header.Param = "_NF_Cold-MIPI-Freq-Max";      //re-assign ge header 
                                    Rslt_GE_Header.Freq1 = "_Rx-" + _StartRXFreq1 + "MHz"; //re-assign ge header 
                                    Rslt_GE_Header.Freq2 = "_Rx-" + _StopRXFreq1 + "MHz"; //re-assign ge header 
                                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                    ResultBuilder.BuildResults(ref results, GE_TestParam, "MHz", MaxColdMIPINFFreq);

                                    for (istep = 0; istep < Nop_ColdMIPINF; istep++)
                                    {
                                        Rslt_GE_Header.Param = "_NF_Rise-MIPI";      //re-assign ge header 
                                        Rslt_GE_Header.Freq1 = "_Rx-" + ResultMultiTraceDelta.FreqMHz[istep] + "MHz"; //re-assign ge header 
                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", ResultMultiTraceDelta.Ampl[istep]);
                                    }


                                    double[] maxOfmaxNFRiseMIPIAmpl = new double[NumberOfRunsColdMIPINF];
                                    double[] maxOfmaxNFRiseMIPIFreq = new double[NumberOfRunsColdMIPINF];

                                    for (istep = 0; istep < NumberOfRunsColdMIPINF; istep++)
                                    {
                                        double maxNFRiseMIPIAmpl = PXITrace[TestCount].Multi_Trace[0][istep].Ampl.Max();
                                        double maxNFRiseMIPIFreq = PXITrace[TestCount].Multi_Trace[0][istep].FreqMHz[Array.IndexOf(PXITrace[TestCount].Multi_Trace[0][istep].Ampl, PXITrace[TestCount].Multi_Trace[0][istep].Ampl.Max())];

                                        maxOfmaxNFRiseMIPIAmpl[istep] = maxNFRiseMIPIAmpl;
                                        maxOfmaxNFRiseMIPIFreq[istep] = maxNFRiseMIPIFreq;

                                        Rslt_GE_Header.Param = "_NF_Rise-MIPI-Ampl-Max";      //re-assign ge header 
                                        Rslt_GE_Header.Freq1 = "_Rx-" + _StartRXFreq1 + "MHz"; //re-assign ge header 
                                        Rslt_GE_Header.Freq2 = "_Rx-" + _StopRXFreq1 + "MHz"; //re-assign ge header 
                                        Rslt_GE_Header.MeasInfo = "_MAX" + (istep + 1); //re-assign ge header
                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", maxNFRiseMIPIAmpl);

                                        Rslt_GE_Header.Param = "_NF_Rise-MIPI-Freq-Max";      //re-assign ge header 
                                        Rslt_GE_Header.Freq1 = "_Rx-" + _StartRXFreq1 + "MHz"; //re-assign ge header 
                                        Rslt_GE_Header.Freq2 = "_Rx-" + _StopRXFreq1 + "MHz"; //re-assign ge header 
                                        Rslt_GE_Header.MeasInfo = "_MAX" + (istep + 1); //re-assign ge header
                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "MHz", maxNFRiseMIPIFreq);
                                    }

                                    Rslt_GE_Header.Param = "_NF_Rise-MIPI-Ampl-Max";      //re-assign ge header 
                                    Rslt_GE_Header.Freq1 = "_Rx-" + _StartRXFreq1 + "MHz"; //re-assign ge header 
                                    Rslt_GE_Header.Freq2 = "_Rx-" + _StopRXFreq1 + "MHz"; //re-assign ge header 
                                    Rslt_GE_Header.MeasInfo = "_MAXALL"; //re-assign ge header
                                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                    ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", maxOfmaxNFRiseMIPIAmpl.Max());

                                    Rslt_GE_Header.Param = "_NF_Rise-MIPI-Freq-Max";      //re-assign ge header 
                                    Rslt_GE_Header.Freq1 = "_Rx-" + _StartRXFreq1 + "MHz"; //re-assign ge header 
                                    Rslt_GE_Header.Freq2 = "_Rx-" + _StopRXFreq1 + "MHz"; //re-assign ge header 
                                    Rslt_GE_Header.MeasInfo = "_MAXALL"; //re-assign ge header
                                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                    ResultBuilder.BuildResults(ref results, GE_TestParam, "MHz", maxOfmaxNFRiseMIPIFreq[Array.IndexOf(maxOfmaxNFRiseMIPIAmpl, maxOfmaxNFRiseMIPIAmpl.Max())]);
                                }

                                //Force test flag to false to ensure no repeated test data
                                //because we add to string builder upfront for PXI due to data reported base on number of sweep
                                _Test_SMU = false;
                                _Test_NF1 = false;

                                #endregion
                            }
                            #endregion

                            tTime.Stop();
                            #endregion
                            break;

                        case "NF_FETCH":
                            #region NF_COLD_HOT_FETCH calculation

                            TestUsePrev_Array = _TestUsePrev.Split(',');
                            NF_TestCount = 0;

                            for (int i = 0; i < PXITrace.Length; i++)
                            {
                                if (Convert.ToInt16(TestUsePrev_Array[0]) == PXITrace[i].TestNumber)
                                {
                                    NF_TestCount = i;
                                }
                            }

                            Nop_NF = PXITrace[NF_TestCount].Multi_Trace[0][0].NoPoints;
                            NumberOfRunsNF = PXITrace[NF_TestCount].TraceCount;
                            RXPathLoss_NF = new double[Nop_NF];

                            NF_new = new double[NumberOfRunsNF][];
                            NoisePower_new = new double[NumberOfRunsNF][];

                            ResultMultiTrace_NF = new s_TraceNo();
                            ResultMultiTrace_NF.Ampl = new double[Nop_NF];
                            ResultMultiTrace_NF.FreqMHz = new double[Nop_NF];

                            Dic_NF = new Dictionary<double, double>();

                            MaxNFAmpl = 0;
                            MaxNFFreq = 0;
                            CalcData = 0;

                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand_HotNF.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            for (int i = 0; i < NumberOfRunsNF; i++)
                            {
                                NF_new[i] = new double[Nop_NF];
                                NoisePower_new[i] = new double[Nop_NF];
                            }

                            // NF RX path loss gathering
                            for (int i = 0; i < Nop_NF; i++)
                            {
                                _RXFreq = Convert.ToSingle(PXITrace[NF_TestCount].Multi_Trace[0][0].FreqMHz[i]);
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                RXPathLoss_NF[i] = _LossOutputPathRX1;
                            }


                            if (PXITrace[NF_TestCount].Multi_Trace[0][0].MXA_No == "PXI_NF_COLD_Trace")
                            {
                                // Cold NF Fetch    
                                for (int i = 0; i < NumberOfRunsNF; i++)
                                {
                                    Eq.Site[0]._EqRFmx.cRFmxNF.RetrieveResults_NFColdSource(NF_TestCount, 0, "result::" + "COLD" + NF_TestCount.ToString() + "_" + i);

                                    for (int j = 0; j < Nop_NF; j++)
                                    {
                                        double NF_withoutGain = (Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j].ToString().Contains("NaN") || Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j].ToString().Contains("Infinity") || Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j].ToString().Contains("∞")) ? 9999 : Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j];

                                        if (NF_withoutGain == 9999 || PXITrace[NF_TestCount].Multi_Trace[0][i].RxGain[j].ToString().Contains("Infinity") || PXITrace[NF_TestCount].Multi_Trace[0][i].RxGain[j].ToString().Contains("NaN"))
                                        {
                                            NF_new[i][j] = 9999;
                                        }

                                        else
                                        {
                                            NF_new[i][j] = NF_withoutGain - PXITrace[NF_TestCount].Multi_Trace[0][i].RxGain[j];
                                        }

                                        NoisePower_new[i][j] = Eq.Site[0]._EqRFmx.cRFmxNF.coldSourcePower[j] - RXPathLoss_NF[j];
                                    }
                                }
                            }

                            else if (PXITrace[NF_TestCount].Multi_Trace[0][0].MXA_No == "PXI_NF_HOT_Trace")
                            {
                                // Hot NF Fetch
                                for (int i = 0; i < NumberOfRunsNF; i++)
                                {
                                    for (int j = 0; j < Nop_NF; j++)
                                    {
                                        Eq.Site[0]._EqRFmx.cRFmxNF.RetrieveResults_NFColdSource(NF_TestCount, j, "result::" + "HOT" + NF_TestCount + "_" + i.ToString() + "_" + j.ToString());

                                        double NF_withoutGain = (Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[0].ToString().Contains("NaN") || Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[0].ToString().Contains("Infinity") || Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[0].ToString().Contains("∞")) ? 9999 : Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[0];

                                        if (NF_withoutGain == 9999 || PXITrace[NF_TestCount].Multi_Trace[0][i].RxGain[j].ToString().Contains("Infinity") || PXITrace[NF_TestCount].Multi_Trace[0][i].RxGain[j].ToString().Contains("NaN"))
                                        {
                                            NF_new[i][j] = 9999;
                                        }

                                        else
                                        {
                                            NF_new[i][j] = NF_withoutGain - PXITrace[NF_TestCount].Multi_Trace[0][i].RxGain[j];
                                        }

                                        NoisePower_new[i][j] = Eq.Site[0]._EqRFmx.cRFmxNF.coldSourcePower[0] - RXPathLoss_NF[j];
                                    }
                                }
                            }

                            else { MessageBox.Show("Need to check if Cold NF & Hot NF data acquisition is performed or not"); }

                            // Store Cold & Hot NF data into PXI Trace
                            StoreNFdata(NF_TestCount, NumberOfRunsNF, Nop_NF, NF_new);

                            if (PXITrace[NF_TestCount].Multi_Trace[0][0].MXA_No == "PXI_NF_COLD_Trace")
                            {
                                // Save Cold & Hot NF Trace if Save_MXATrace is enabled
                                Save_PXI_NF_TraceRaw(_TestParaName + "_Cold-NF", NF_TestCount, _Save_MXATrace, 0, _NF_BW * 1e06);
                            }

                            else
                            {
                                // Save Cold & Hot NF Trace if Save_MXATrace is enabled
                                Save_PXI_NF_TraceRaw(_TestParaName + "_Hot-NF", NF_TestCount, _Save_MXATrace, 0, _NF_BW * 1e06);
                            }

                            #region Calculate Result
                            //Calculate the result from the sorted data
                            for (istep = 0; istep < Nop_NF; istep++)     //get MAX data for every noPtsUser out of multitrace (from "use previous" setting)
                            {
                                for (int i = 0; i < PXITrace[NF_TestCount].TraceCount; i++)
                                {
                                    if (i == 0)
                                    {
                                        CalcData = PXITrace[NF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_NF.Ampl[istep] = PXITrace[NF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_NF.FreqMHz[istep] = PXITrace[NF_TestCount].Multi_Trace[0][i].FreqMHz[istep];
                                    }

                                    if (CalcData < PXITrace[NF_TestCount].Multi_Trace[0][i].Ampl[istep])
                                    {
                                        ResultMultiTrace_NF.Ampl[istep] = PXITrace[NF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_NF.FreqMHz[istep] = PXITrace[NF_TestCount].Multi_Trace[0][i].FreqMHz[istep];
                                        CalcData = PXITrace[NF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                    }
                                }

                            }

                            for (int i = 0; i < Nop_NF; i++)
                            {
                                Dic_NF.Add(ResultMultiTrace_NF.FreqMHz[i], ResultMultiTrace_NF.Ampl[i]);
                            }

                            MaxNFAmpl = ResultMultiTrace_NF.Ampl.Max();
                            MaxNFFreq = ResultMultiTrace_NF.FreqMHz[Array.IndexOf(ResultMultiTrace_NF.Ampl, ResultMultiTrace_NF.Ampl.Max())];

                            #endregion

                            if (_Test_NF1)
                            {
                                if (PXITrace[NF_TestCount].Multi_Trace[0][0].MXA_No == "PXI_NF_COLD_Trace")
                                {
                                    for (int i = 0; i < Nop_NF; i++)
                                    {
                                        List<double> listColdPower = new List<double>();
                                        for (int j = 0; j < NumberOfRunsNF; j++)
                                        {
                                            listColdPower.Add(NoisePower_new[j][i]);
                                        }

                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_" + ResultMultiTrace_NF.FreqMHz[i] + "_Cold-Power" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dBm", listColdPower.Max());
                                    }

                                    for (int i = 0; i < Nop_NF; i++)
                                    {
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_" + ResultMultiTrace_NF.FreqMHz[i] + "_Cold-NF" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", ResultMultiTrace_NF.Ampl[i]);
                                    }

                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Ampl_" + "Cold-Max-NF" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", MaxNFAmpl);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Freq_" + "Cold-Max-NF" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", MaxNFFreq);
                                }

                                else
                                {
                                    for (int i = 0; i < Nop_NF; i++)
                                    {
                                        List<double> listHotPower = new List<double>();
                                        for (int j = 0; j < NumberOfRunsNF; j++)
                                        {
                                            listHotPower.Add(NoisePower_new[j][i]);
                                        }

                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_" + ResultMultiTrace_NF.FreqMHz[i] + "_Hot-Power" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", listHotPower.Max());
                                    }

                                    for (int i = 0; i < Nop_NF; i++)
                                    {
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_" + ResultMultiTrace_NF.FreqMHz[i] + "_Hot-NF" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", ResultMultiTrace_NF.Ampl[i]);
                                    }

                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Ampl_" + "Hot-Max-NF" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", MaxNFAmpl);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Freq_" + "Hot-Max-NF" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", MaxNFFreq);
                                }
                            }

                            //Force test flag to false to ensure no repeated test data
                            //because we add to string builder upfront for PXI due to data reported base on number of sweep

                            _Test_Pin1 = false;
                            _Test_Pout1 = false;
                            _Test_SMU = false;
                            _Test_NF1 = false;
                            tTime.Stop();
                            break;
                        #endregion

                        case "NFG_TRACE_COLD":
                            #region NFG_TRACE_COLD calculation

                            ColdNF_TestCount = 0;

                            for (int i = 0; i < PXITrace.Length; i++)
                            {
                                if (Convert.ToInt16(_TestUsePrev) == PXITrace[i].TestNumber)
                                {
                                    ColdNF_TestCount = i;
                                }
                            }

                            Nop_ColdNF = PXITrace[ColdNF_TestCount].Multi_Trace[0][0].NoPoints;
                            NumberOfRunsColdNF = PXITrace[ColdNF_TestCount].TraceCount;
                            RXPathLoss_Cold = new double[Nop_ColdNF];

                            Cold_NF_new = new double[NumberOfRunsColdNF][];
                            Cold_NoisePower_new = new double[NumberOfRunsColdNF][];

                            ResultMultiTrace_ColdNF = new s_TraceNo();
                            ResultMultiTrace_ColdNF.Ampl = new double[Nop_ColdNF];
                            ResultMultiTrace_ColdNF.FreqMHz = new double[Nop_ColdNF];
                            ResultMultiTrace_ColdNF.RxGain = new double[Nop_ColdNF];

                            Dic_ColdNF = new Dictionary<double, double>();

                            MaxColdNFAmpl = 0;
                            MaxColdNFFreq = 0;
                            CalcData = 0;

                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand_HotNF.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            for (int i = 0; i < NumberOfRunsColdNF; i++)
                            {
                                Cold_NF_new[i] = new double[Nop_ColdNF];
                                Cold_NoisePower_new[i] = new double[Nop_ColdNF];
                            }

                            // Cold NF RX path loss gathering
                            for (int i = 0; i < Nop_ColdNF; i++)
                            {
                                _RXFreq = Convert.ToSingle(PXITrace[ColdNF_TestCount].Multi_Trace[0][0].FreqMHz[i]);
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                RXPathLoss_Cold[i] = _LossOutputPathRX1;
                            }

                            // Cold NF Fetch    
                            for (int i = 0; i < NumberOfRunsColdNF; i++)
                            {
                                Eq.Site[0]._EqRFmx.cRFmxNF.RetrieveResults_NFColdSource(ColdNF_TestCount, 0, "result::" + "COLD" + ColdNF_TestCount.ToString() + "_" + i);

                                for (int j = 0; j < Nop_ColdNF; j++)
                                {
                                    double Cold_NF_withoutGain = (Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j].ToString().Contains("NaN") || Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j].ToString().Contains("Infinity") || Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j].ToString().Contains("∞")) ? 9999 : Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j];

                                    if (Cold_NF_withoutGain == 9999 || PXITrace[ColdNF_TestCount].Multi_Trace[0][i].RxGain[j].ToString().Contains("Infinity") || PXITrace[ColdNF_TestCount].Multi_Trace[0][i].RxGain[j].ToString().Contains("NaN"))
                                    {
                                        Cold_NF_new[i][j] = 9999;
                                    }

                                    else
                                    {
                                        Cold_NF_new[i][j] = Cold_NF_withoutGain - (PXITrace[ColdNF_TestCount].Multi_Trace[0][i].RxGain[j]);
                                    }

                                    Cold_NoisePower_new[i][j] = Eq.Site[0]._EqRFmx.cRFmxNF.coldSourcePower[j] - RXPathLoss_Cold[j];

                                }
                            }

                            // Store Cold & Hot NF data into PXI Trace
                            StoreNFdata(ColdNF_TestCount, NumberOfRunsColdNF, Nop_ColdNF, Cold_NF_new);

                            // Save Cold & Hot NF Trace if Save_MXATrace is enabled
                            Save_PXI_NF_TraceRaw(_TestParaName + "_Cold-NF", ColdNF_TestCount, _Save_MXATrace, 0, PXITrace[ColdNF_TestCount].Multi_Trace[0][0].RBW_Hz);

                            #region Calculate Result
                            //Calculate the result from the sorted data
                            for (istep = 0; istep < Nop_ColdNF; istep++)     //get MAX data for every noPtsUser out of multitrace (from "use previous" setting)
                            {
                                for (int i = 0; i < PXITrace[ColdNF_TestCount].TraceCount; i++)
                                {
                                    if (i == 0)
                                    {
                                        CalcData = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_ColdNF.Ampl[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_ColdNF.FreqMHz[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].FreqMHz[istep];
                                        ResultMultiTrace_ColdNF.RxGain[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].RxGain[istep];
                                    }

                                    if (CalcData < PXITrace[ColdNF_TestCount].Multi_Trace[0][i].Ampl[istep])
                                    {
                                        ResultMultiTrace_ColdNF.Ampl[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                        ResultMultiTrace_ColdNF.FreqMHz[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].FreqMHz[istep];
                                        ResultMultiTrace_ColdNF.RxGain[istep] = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].RxGain[istep];
                                        CalcData = PXITrace[ColdNF_TestCount].Multi_Trace[0][i].Ampl[istep];
                                    }
                                }

                            }

                            for (int i = 0; i < Nop_ColdNF; i++)
                            {
                                Dic_ColdNF.Add(ResultMultiTrace_ColdNF.FreqMHz[i], ResultMultiTrace_ColdNF.Ampl[i]);
                            }

                            #endregion

                            #region Build Result
                            if (!b_GE_Header)
                            {
                                #region Standard Result Format
                                if (_Test_NF1)
                                {
                                    for (int i = 0; i < Nop_ColdNF; i++)
                                    {
                                        List<double> listColdPower = new List<double>();
                                        for (int j = 0; j < NumberOfRunsColdNF; j++)
                                        {
                                            listColdPower.Add(Cold_NoisePower_new[j][i]);
                                        }

                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_" + ResultMultiTrace_ColdNF.FreqMHz[i] + "_Cold-Power" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dBm", listColdPower.Max());
                                    }

                                    for (int i = 0; i < Nop_ColdNF; i++)
                                    {
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_" + ResultMultiTrace_ColdNF.RxGain[i] + "_Cold-Gain" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", ResultMultiTrace_ColdNF.Ampl[i]);
                                    }

                                    for (int i = 0; i < Nop_ColdNF; i++)
                                    {
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_" + ResultMultiTrace_ColdNF.FreqMHz[i] + "_Cold-NF" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dB", ResultMultiTrace_ColdNF.Ampl[i]);
                                    }
                                }

                                //Force test flag to false to ensure no repeated test data
                                //because we add to string builder upfront for PXI due to data reported base on number of sweep

                                _Test_Pin1 = false;
                                _Test_Pout1 = false;
                                _Test_SMU = false;
                                _Test_NF1 = false;

                                break;
                                #endregion
                            }
                            else
                            {
                                #region Golden Eagle Result Format
                                b_SmuHeader = true;
                                string GE_TestParam = null;
                                Rslt_GE_Header = new s_GE_Header();
                                Decode_GE_Header(TestPara, out Rslt_GE_Header);

                                //Calculate no of tx freq + step
                                double[] TXFreq_List;
                                count = Convert.ToInt16(Math.Ceiling((Math.Round(_StopRXFreq1, 3) - Math.Round(_StartRXFreq1, 3)) / Math.Round(_StepRXFreq1, 3)));
                                TXFreq_List = new double[count + 1];
                                _TXFreq = _StartTXFreq1;

                                for (int i = 0; i <= count; i++)
                                {
                                    TXFreq_List[i] = _TXFreq;
                                    _TXFreq = _TXFreq + _StepTXFreq1;

                                    if (!_SetRX1NDiag)   //for NMAX method
                                    {
                                        _TXFreq = _StartTXFreq1;
                                    }

                                    if (_TXFreq > _StopTXFreq1) //For Last Freq match
                                    {
                                        _TXFreq = _StopTXFreq1;
                                    }
                                }

                                if (_Test_NF1)
                                {
                                    for (int i = 0; i < Nop_ColdNF; i++)
                                    {
                                        List<double> listColdPower = new List<double>();
                                        for (int j = 0; j < NumberOfRunsColdNF; j++)
                                        {
                                            listColdPower.Add(Cold_NoisePower_new[j][i]);
                                        }

                                        if (_Disp_ColdTrace)
                                        {
                                            Rslt_GE_Header.Param = "_Power_Cold";      //re-assign ge header 
                                            Rslt_GE_Header.Freq1 = "_Rx-" + ResultMultiTrace_ColdNF.FreqMHz[i] + "MHz"; //re-assign ge header 
                                            Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                            ResultBuilder.BuildResults(ref results, GE_TestParam, "dBm", listColdPower.Max());
                                        }
                                    }

                                    for (int i = 0; i < Nop_ColdNF; i++)
                                    {
                                        if (_Disp_ColdTrace)
                                        {
                                            Rslt_GE_Header.Param = "_Gain";      //re-assign ge header 
                                            Rslt_GE_Header.Freq1 = "_Rx-" + ResultMultiTrace_ColdNF.FreqMHz[i] + "MHz"; //re-assign ge header 
                                            Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                            ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", ResultMultiTrace_ColdNF.RxGain[i]);
                                        }
                                    }

                                    for (int i = 0; i < Nop_ColdNF; i++)
                                    {
                                        if (_Disp_ColdTrace)
                                        {
                                            Rslt_GE_Header.Param = "_NF";      //re-assign ge header 
                                            Rslt_GE_Header.Freq1 = "_Rx-" + ResultMultiTrace_ColdNF.FreqMHz[i] + "MHz"; //re-assign ge header 
                                            Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                            ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", ResultMultiTrace_ColdNF.Ampl[i]);
                                        }
                                    }
                                }
                                //Force test flag to false to ensure no repeated test data
                                //because we add to string builder upfront for PXI due to data reported base on number of sweep

                                _Test_Pin1 = false;
                                _Test_Pout1 = false;
                                _Test_SMU = false;
                                _Test_NF1 = false;

                                #endregion
                            }

                            #endregion

                            tTime.Stop();
                            break;
                        #endregion

                        default:
                            MessageBox.Show("Test Parameter : " + _TestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }
                    break;

                case "COMMON":
                    switch (_TestParam.ToUpper())
                    {
                        case "MAX_MIN":
                            #region MAX MIN calculation
                            //Find result MAX or MIN result from few sets of data - define in 'Use Previous' column - data example 4,6,9,10
                            if (_Test_Pin)
                            {
                                resultTag = (int)e_ResultTag.PIN;
                                SearchMAXMIN(_TestParam, _TestUsePrev, _Search_Method, resultTag, out result, out arrayVal);
                                R_Pin = result;
                            }
                            if (_Test_Pout)
                            {
                                resultTag = (int)e_ResultTag.POUT;
                                SearchMAXMIN(_TestParam, _TestUsePrev, _Search_Method, resultTag, out result, out arrayVal);
                                R_Pout = result;
                            }
                            if (_Test_Pin1)
                            {
                                resultTag = (int)e_ResultTag.PIN1;
                                SearchMAXMIN(_TestParam, _TestUsePrev, _Search_Method, resultTag, out result, out arrayVal);
                                R_Pin1 = result;
                            }
                            if (_Test_Pout1)
                            {
                                resultTag = (int)e_ResultTag.POUT1;
                                SearchMAXMIN(_TestParam, _TestUsePrev, _Search_Method, resultTag, out result, out arrayVal);
                                R_Pout1 = result;
                            }
                            if (_Test_Pin2)
                            {
                                resultTag = (int)e_ResultTag.PIN2;
                                SearchMAXMIN(_TestParam, _TestUsePrev, _Search_Method, resultTag, out result, out arrayVal);
                                R_Pin2 = result;
                            }
                            if (_Test_Pout2)
                            {
                                resultTag = (int)e_ResultTag.POUT2;
                                SearchMAXMIN(_TestParam, _TestUsePrev, _Search_Method, resultTag, out result, out arrayVal);
                                R_Pout2 = result;
                            }
                            if (_Test_NF1)
                            {
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                SearchMAXMIN(_TestParam, _TestUsePrev, _Search_Method, resultTag, out result, out arrayVal);
                                R_NF1_Ampl = result;
                                resultTag = (int)e_ResultTag.NF1_FREQ;
                                R_NF1_Freq = Results[arrayVal].Multi_Results[resultTag].Result_Data;
                            }
                            if (_Test_NF2)
                            {
                                resultTag = (int)e_ResultTag.NF2_AMPL;
                                SearchMAXMIN(_TestParam, _TestUsePrev, _Search_Method, resultTag, out result, out arrayVal);
                                R_NF2_Ampl = result;
                                resultTag = (int)e_ResultTag.NF2_FREQ;
                                R_NF2_Freq = Results[arrayVal].Multi_Results[resultTag].Result_Data;
                            }
                            if (_Test_Harmonic)
                            {
                                resultTag = (int)e_ResultTag.HARMONIC_AMPL;
                                SearchMAXMIN(_TestParam, _TestUsePrev, _Search_Method, resultTag, out result, out arrayVal);
                                R_H2_Ampl = result;
                            }
                            tTime.Stop();
                            #endregion
                            break;
                        case "AVERAGE":
                            #region AVERAGE calculation
                            //Find result average between few result - define in 'Use Previous' column - data example 4,6,9,10
                            if (_Test_Pin)
                            {
                                resultTag = (int)e_ResultTag.PIN;
                                R_Pin = CalcAverage(_TestUsePrev, resultTag);
                            }
                            if (_Test_Pout)
                            {
                                resultTag = (int)e_ResultTag.POUT;
                                R_Pout = CalcAverage(_TestUsePrev, resultTag);
                            }
                            if (_Test_Pin1)
                            {
                                resultTag = (int)e_ResultTag.PIN1;
                                R_Pin1 = CalcAverage(_TestUsePrev, resultTag);
                            }
                            if (_Test_Pout1)
                            {
                                resultTag = (int)e_ResultTag.POUT1;
                                R_Pout2 = CalcAverage(_TestUsePrev, resultTag);
                            }
                            if (_Test_Pin2)
                            {
                                resultTag = (int)e_ResultTag.PIN2;
                                R_Pin2 = CalcAverage(_TestUsePrev, resultTag);
                            }
                            if (_Test_Pout2)
                            {
                                resultTag = (int)e_ResultTag.POUT2;
                                R_Pout2 = CalcAverage(_TestUsePrev, resultTag);
                            }
                            if (_Test_NF1)
                            {
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                R_NF1_Ampl = CalcAverage(_TestUsePrev, resultTag);
                            }
                            if (_Test_NF2)
                            {
                                resultTag = (int)e_ResultTag.NF2_AMPL;
                                R_NF2_Ampl = CalcAverage(_TestUsePrev, resultTag);
                            }
                            if (_Test_Harmonic)
                            {
                                resultTag = (int)e_ResultTag.HARMONIC_AMPL;
                                R_H2_Ampl = CalcAverage(_TestUsePrev, resultTag);
                            }
                            tTime.Stop();
                            #endregion
                            break;
                        case "DELTA":
                            #region MAX MIN calculation
                            //Find result Delta between 2 result - define in 'Use Previous' column - data example 4,10
                            if (_Test_Pin)
                            {
                                resultTag = (int)e_ResultTag.PIN;
                                R_Pin = CalcDelta(_TestUsePrev, resultTag, _Abs_Value);
                            }
                            if (_Test_Pout)
                            {
                                resultTag = (int)e_ResultTag.POUT;
                                R_Pout = CalcDelta(_TestUsePrev, resultTag, _Abs_Value);
                            }
                            if (_Test_Pin1)
                            {
                                resultTag = (int)e_ResultTag.PIN1;
                                R_Pin1 = CalcDelta(_TestUsePrev, resultTag, _Abs_Value);
                            }
                            if (_Test_Pout1)
                            {
                                resultTag = (int)e_ResultTag.POUT1;
                                R_Pout2 = CalcDelta(_TestUsePrev, resultTag, _Abs_Value);
                            }
                            if (_Test_Pin2)
                            {
                                resultTag = (int)e_ResultTag.PIN2;
                                R_Pin2 = CalcDelta(_TestUsePrev, resultTag, _Abs_Value);
                            }
                            if (_Test_Pout2)
                            {
                                resultTag = (int)e_ResultTag.POUT2;
                                R_Pout2 = CalcDelta(_TestUsePrev, resultTag, _Abs_Value);
                            }
                            if (_Test_NF1)
                            {
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                R_NF1_Ampl = CalcDelta(_TestUsePrev, resultTag, _Abs_Value);
                            }
                            if (_Test_NF2)
                            {
                                resultTag = (int)e_ResultTag.NF2_AMPL;
                                R_NF2_Ampl = CalcDelta(_TestUsePrev, resultTag, _Abs_Value);
                            }
                            if (_Test_Harmonic)
                            {
                                resultTag = (int)e_ResultTag.HARMONIC_AMPL;
                                R_H2_Ampl = CalcDelta(_TestUsePrev, resultTag, _Abs_Value);
                            }
                            tTime.Stop();
                            #endregion
                            break;
                        case "SUM":
                            #region Summary calculation
                            //Find result summary between few result - define in 'Use Previous' column - data example 4,6,9,10
                            if (_Test_Pin)
                            {
                                resultTag = (int)e_ResultTag.PIN;
                                R_Pin = CalcSum(_TestUsePrev, resultTag);
                            }
                            if (_Test_Pout)
                            {
                                resultTag = (int)e_ResultTag.POUT;
                                R_Pout = CalcSum(_TestUsePrev, resultTag);
                            }
                            if (_Test_Pin1)
                            {
                                resultTag = (int)e_ResultTag.PIN1;
                                R_Pin1 = CalcSum(_TestUsePrev, resultTag);
                            }
                            if (_Test_Pout1)
                            {
                                resultTag = (int)e_ResultTag.POUT1;
                                R_Pout2 = CalcSum(_TestUsePrev, resultTag);
                            }
                            if (_Test_Pin2)
                            {
                                resultTag = (int)e_ResultTag.PIN2;
                                R_Pin2 = CalcSum(_TestUsePrev, resultTag);
                            }
                            if (_Test_Pout2)
                            {
                                resultTag = (int)e_ResultTag.POUT2;
                                R_Pout2 = CalcSum(_TestUsePrev, resultTag);
                            }
                            if (_Test_NF1)
                            {
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                R_NF1_Ampl = CalcSum(_TestUsePrev, resultTag);
                            }
                            if (_Test_NF2)
                            {
                                resultTag = (int)e_ResultTag.NF2_AMPL;
                                R_NF2_Ampl = CalcSum(_TestUsePrev, resultTag);
                            }
                            if (_Test_Harmonic)
                            {
                                resultTag = (int)e_ResultTag.HARMONIC_AMPL;
                                R_H2_Ampl = CalcSum(_TestUsePrev, resultTag);
                            }
                            tTime.Stop();
                            #endregion
                            break;

                        default:
                            MessageBox.Show("Test Parameter : " + _TestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }
                    break;

                case "CALIBRATION":
                    switch (_TestParam.ToUpper())
                    {
                        case "RF_CAL":
                            RF_Calibration(_Trig_Delay, _Generic_Delay, _RdCurr_Delay, _RdPwr_Delay, _Setup_Delay);
                            R_RFCalStatus = 1;
                            tTime.Stop();
                            break;
                        case "NF_CAL":
                            #region NF Calibration
                            if (NFCalFlag)
                            {
                                calDir = @"C:\Avago.ATF.Common\Input\Calibration_NF\" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + @"\";

                                if (!Directory.Exists(calDir))
                                {
                                    Directory.CreateDirectory(calDir);
                                }

                                NFCalFlag = false;
                            }

                            NoOfPts = (Convert.ToInt32((_StopRXFreq1 - _StartRXFreq1) / _StepRXFreq1)) + 1;
                            RXContactFreq = new double[NoOfPts];
                            RXPathLoss = new double[NoOfPts];
                            LNAInputLoss = new double[NoOfPts];

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand_HotNF.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            int indexStep = 0;
                            _RXFreq = _StartRXFreq1;
                            count = Convert.ToInt16((_StopRXFreq1 - _StartRXFreq1) / _StepRXFreq1);

                            for (int i = 0; i <= count; i++)
                            {
                                RXContactFreq[i] = Math.Round(_RXFreq, 3);

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                RXPathLoss[i] = _LossOutputPathRX1;//Seoul

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _RXFreq, ref _LossCouplerPath, ref StrError);
                                LNAInputLoss[i] = _LossCouplerPath;//Seoul

                                _RXFreq = Convert.ToSingle(Math.Round(_RXFreq + _StepRXFreq1, 3));           //need to use round function because of C# float and double floating point bug/error
                                indexStep = indexStep + Convert.ToInt32(_StepRXFreq1);
                            }

                            SetswitchPath(DicCalInfo, DataFilePath.LocSettingPath, TCF_Header.ConstSwitching_Band_HotNF, _SwBand_HotNF, _Setup_Delay);

                            Eq.Site[0]._EqVST.ConfigureTriggers();

                            NF_Calibration(TestCount, _NF_CalTag, RXContactFreq, LNAInputLoss, RXPathLoss, _NF_Cal_HL, _NF_Cal_LL, _NF_BW);

                            Eq.Site[0]._EqVST.ReConfigVST();
                            #endregion
                            break;

                        default:
                            MessageBox.Show("Test Parameter : " + _TestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }
                    break;

                case "DC":
                    switch (_TestParam.ToUpper())
                    {
                        case "PS4CH":
                            #region 4-Channel Power Supply Setting
                            //pass to global variable to be use outside this function
                            EqmtStatus.DC_CH = _DCSetCh;

                            //to select which channel to set and measure - Format in TCF(DCSet_Channel) 1,4 -> means CH1 & CH4 to set/measure
                            SetDC = _DCSetCh.Split(',');
                            MeasDC = _DCMeasCh.Split(',');

                            for (int i = 0; i < SetDC.Count(); i++)
                            {
                                int dcVChannel = Convert.ToInt16(SetDC[i]);
                                Eq.Site[0]._EqDC.SetVolt((dcVChannel), _DCVCh[dcVChannel], _DCILimitCh[dcVChannel]);
                                Eq.Site[0]._EqDC.DcOn(dcVChannel);
                            }

                            if (_Test_DCSupply)
                            {
                                for (int i = 0; i < MeasDC.Count(); i++)
                                {
                                    int dcIChannel = Convert.ToInt16(MeasDC[i]);

                                    if (_DCILimitCh[dcIChannel] > 0)
                                    {
                                        if (FirstDut)   //added delay before measure for 1st unit only because the DC Supply not stable
                                        {
                                            FirstDut = false;
                                            DelayMs(500);
                                            R_DC_ICh[dcIChannel] = Eq.Site[0]._EqDC.MeasI(dcIChannel);
                                            DelayMs(300);
                                        }

                                        R_DC_ICh[dcIChannel] = Eq.Site[0]._EqDC.MeasI(dcIChannel);
                                        if (R_DC_ICh[dcIChannel] < (_DCILimitCh[dcIChannel] * 0.1))     //if current measure less than 10%, do 2nd Time to ensure that current are measure correctly
                                        {
                                            DelayMs(_RdCurr_Delay);
                                            R_DC_ICh[dcIChannel] = Eq.Site[0]._EqDC.MeasI(dcIChannel);
                                            R_DC_ICh[dcIChannel] = Eq.Site[0]._EqDC.MeasI(dcIChannel);
                                        }
                                    }

                                    // pass out the test result label for every measurement channel
                                    string tempLabel = "DCI_CH" + MeasDC[i];
                                    foreach (string key in DicTestLabel.Keys)
                                    {
                                        if (key == tempLabel)
                                        {
                                            R_DCLabel_ICh[dcIChannel] = DicTestLabel[key].ToString();
                                            break;
                                        }
                                    }
                                }
                            }

                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            #endregion
                            break;

                        case "PS1CH":
                            #region 1-Channel Power Supply Setting
                            //pass to global variable to be use outside this function
                            EqmtStatus.DC_CH = _DCSetCh;

                            //to select which channel to set and measure - Format in TCF(DCSet_Channel) 1,4 -> means CH1 & CH4 to set/measure
                            SetDC = _DCSetCh.Split(',');
                            MeasDC = _DCMeasCh.Split(',');

                            for (int i = 0; i < 1; i++)
                            {
                                int dcVChannel = Convert.ToInt16(SetDC[i]);
                                Eq.Site[0]._Eq_DC_1CH.SetVolt((dcVChannel), _DCVCh[dcVChannel], _DCILimitCh[dcVChannel]);
                                Eq.Site[0]._Eq_DC_1CH.DcOn(dcVChannel);
                            }

                            if (_Test_DCSupply)
                            {
                                for (int i = 0; i < 1; i++)
                                {
                                    int dcIChannel = Convert.ToInt16(MeasDC[i]);

                                    if (_DCILimitCh[dcIChannel] > 0)
                                    {
                                        if (FirstDut) //added delay before measure for 1st unit only because the DC Supply not stable
                                        {
                                            FirstDut = false;
                                            DelayMs(500);
                                            R_DC_ICh[dcIChannel] = Eq.Site[0]._Eq_DC_1CH.MeasI(dcIChannel);
                                            DelayMs(300);
                                        }

                                        R_DC_ICh[dcIChannel] = Eq.Site[0]._Eq_DC_1CH.MeasI(dcIChannel);
                                        if (R_DC_ICh[dcIChannel] < (_DCILimitCh[dcIChannel] * 0.1))     //if current measure less than 10%, do 2nd Time to ensure that current are measure correctly
                                        {
                                            DelayMs(_RdCurr_Delay);
                                            R_DC_ICh[dcIChannel] = Eq.Site[0]._Eq_DC_1CH.MeasI(dcIChannel);
                                            //R_DC_ICh[dcIChannel] = Eq.Site[0]._Eq_DC_1CH.MeasI(dcIChannel);
                                        }
                                    }

                                    // pass out the test result label for every measurement channel
                                    string tempLabel = "DCI_CH" + MeasDC[i];
                                    foreach (string key in DicTestLabel.Keys)
                                    {
                                        if (key == tempLabel)
                                        {
                                            R_DCLabel_ICh[dcIChannel] = DicTestLabel[key].ToString();
                                            break;
                                        }
                                    }
                                }
                            }

                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ATFResultBuilder.AddResultToDict(_TestParaName + "_TestTime" + _TestNum, tTime.ElapsedMilliseconds, ref StrError);
                            //}
                            #endregion
                            break;

                        case "MULTI_DCSUPPLY":
                            #region Multiple 1-Channel Power Supply Setting
                            //pass to global variable to be use outside this function
                            EqmtStatus.DC_CH = _DCSetCh;

                            //to select which channel to set and measure - Format in TCF(DCSet_Channel) 1,4 -> means CH1 & CH4 to set/measure
                            SetDC = _DCSetCh.Split(',');
                            MeasDC = _DCMeasCh.Split(',');

                            if (FirstDut_DCSupply)
                            {
                                for (int i = 0; i < SetDC.Length; i++)
                                {
                                    int dcVChannel = Convert.ToInt16(SetDC[i]);
                                    Eq.Site[0]._Eq_DCSupply[i].SetVolt((dcVChannel), _DCVCh[dcVChannel], _DCILimitCh[dcVChannel]);
                                    Eq.Site[0]._Eq_DCSupply[i].DcOn(dcVChannel);
                                }

                                FirstDut_DCSupply = false;

                            }

                            if (_Test_DCSupply)
                            {
                                for (int i = 0; i < MeasDC.Length; i++)
                                {
                                    int dcIChannel = Convert.ToInt16(MeasDC[i]);

                                    if (_DCILimitCh[dcIChannel] > 0)
                                    {
                                        if (FirstDut) //added delay before measure for 1st unit only because the DC Supply not stable
                                        {
                                            //FirstDut = false; //debug mipi temp
                                            DelayMs(500);
                                            R_DC_ICh[dcIChannel] = Eq.Site[0]._Eq_DCSupply[i].MeasI(dcIChannel);
                                            DelayMs(300);
                                        }

                                        R_DC_ICh[dcIChannel] = Eq.Site[0]._Eq_DCSupply[i].MeasI(dcIChannel);
                                        if (R_DC_ICh[dcIChannel] < (_DCILimitCh[dcIChannel] * 0.1))     //if current measure less than 10%, do 2nd Time to ensure that current are measure correctly
                                        {
                                            DelayMs(_RdCurr_Delay);
                                            R_DC_ICh[dcIChannel] = Eq.Site[0]._Eq_DCSupply[i].MeasI(dcIChannel);
                                        }
                                    }

                                    // pass out the test result label for every measurement channel
                                    string tempLabel = "DCI_CH" + MeasDC[i];
                                    foreach (string key in DicTestLabel.Keys)
                                    {
                                        if (key == tempLabel)
                                        {
                                            R_DCLabel_ICh[dcIChannel] = DicTestLabel[key].ToString();
                                            break;
                                        }
                                    }
                                }
                            }

                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ATFResultBuilder.AddResultToDict(_TestParaName + "_TestTime" + _TestNum, tTime.ElapsedMilliseconds, ref StrError);
                            //}
                            #endregion
                            break;

                        case "SMU":
                            //string[] SetSMUSelect;
                            #region Set SMU
                            //pass to global variable to be use outside this function
                            EqmtStatus.SMU_CH = _SMUSetCh;

                            //to select which channel to set and measure - Format in TCF(DCSet_Channel) 1,4 -> means CH1 & CH4 to set/measure
                            SetSMU = _SMUSetCh.Split(',');
                            MeasSMU = _SMUMeasCh.Split(',');

                            SetSMUSelect = new string[SetSMU.Count()];
                            for (int i = 0; i < SetSMU.Count(); i++)
                            {
                                int smuVChannel = Convert.ToInt16(SetSMU[i]);
                                SetSMUSelect[i] = Eq.Site[0]._SMUSetting[smuVChannel];       //rearrange the Eq.Site[0]._SMUSetting base on reqquired channel only from total of 8 channel available  
                                Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[smuVChannel], Eq.Site[0]._EqSMU, _SMUVCh[smuVChannel], _SMUILimitCh[smuVChannel]);
                            }

                            Eq.Site[0]._Eq_SMUDriver.DcOn(SetSMUSelect, Eq.Site[0]._EqSMU);

                            if (_Test_SMU)
                            {
                                DelayMs(_RdCurr_Delay);
                                float _NPLC = 0.1f; // float _NPLC = 1;  toh

                                for (int i = 0; i < MeasSMU.Count(); i++)
                                {
                                    int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                    if (_SMUILimitCh[smuIChannel] > 0)
                                    {
                                        R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                    }

                                    // pass out the test result label for every measurement channel
                                    string tempLabel = "SMUI_CH" + MeasSMU[i];
                                    foreach (string key in DicTestLabel.Keys)
                                    {
                                        if (key == tempLabel)
                                        {
                                            R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                            break;
                                        }
                                    }
                                }
                            }

                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            #endregion
                            break;

                        case "SMU_LEAKAGE":
                            //string[] SetSMUSelect;
                            #region Set SMU

                            //pass to global variable to be use outside this function
                            EqmtStatus.SMU_CH = _SMUSetCh;

                            //to select which channel to set and measure - Format in TCF(DCSet_Channel) 1,4 -> means CH1 & CH4 to set/measure
                            SetSMU = _SMUSetCh.Split(',');
                            MeasSMU = _SMUMeasCh.Split(',');

                            #region Set higher current for fast charging
                            //set Vcc channel current to higher value for faster charging to eliminate current clamp during actual leakage current measurement
                            SetSMUSelect = new string[SetSMU.Count()];
                            for (int i = 0; i < SetSMU.Count(); i++)
                            {
                                int smuVChannel = Convert.ToInt16(SetSMU[i]);
                                SetSMUSelect[i] = Eq.Site[0]._SMUSetting[smuVChannel];       //rearrange the Eq.Site[0]._SMUSetting base on reqquired channel only from total of 8 channel available       

                                foreach (string key in DicTestLabel.Keys)
                                {
                                    string tempLabel = "SMUI_CH" + SetSMU[i];
                                    if (key == tempLabel)
                                    {
                                        string tmpValue = (DicTestLabel[key].ToString()).ToUpper();
                                        if (tmpValue.Contains("ICC"))
                                        {
                                            Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[smuVChannel], Eq.Site[0]._EqSMU, _SMUVCh[smuVChannel], 0.5f);
                                        }
                                        else
                                        {
                                            //Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[smuVChannel], Eq.Site[0]._EqSMU, _SMUVCh[smuVChannel], _SMUILimitCh[smuVChannel]);
                                            Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[smuVChannel], Eq.Site[0]._EqSMU, _SMUVCh[smuVChannel], 0.1f); // modified for Ibatt - 08.09.2020	
                                        }
                                        break;
                                    }
                                }
                            }

                            Eq.Site[0]._Eq_SMUDriver.DcOn(SetSMUSelect, Eq.Site[0]._EqSMU);

                            //if not set in TCF , will assume default setup delay of 50ms
                            if (_Setup_Delay <= 0)
                            {
                                DelayMs(50);
                            }
                            else
                            {
                                DelayMs(_Setup_Delay);
                            }

                            #endregion

                            //set back voltage & current to measurement limit
                            SetSMUSelect = new string[SetSMU.Count()];
                            for (int i = 0; i < SetSMU.Count(); i++)
                            {
                                int smuVChannel = Convert.ToInt16(SetSMU[i]);
                                SetSMUSelect[i] = Eq.Site[0]._SMUSetting[smuVChannel];       //rearrange the Eq.Site[0]._SMUSetting base on reqquired channel only from total of 8 channel available  
                                Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[smuVChannel], Eq.Site[0]._EqSMU, _SMUVCh[smuVChannel], _SMUILimitCh[smuVChannel]);
                            }

                            if (_Test_SMU)
                            {
                                // Current Meausre [Point -> Trace Mode] (don't need delay at this mode on NI PXI SMU), 15-10-20
                                if (s_strSMU != "NIPXI")
                                    DelayMs(_RdCurr_Delay);

                                float _NPLC = 1.0f;

                                for (int i = 0; i < MeasSMU.Count(); i++)
                                {
                                    int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                    if (_SMUILimitCh[smuIChannel] > 0)
                                    {
                                        // Current Meausre [Point -> Trace Mode] (don't need delay at this mode on NI PXI SMU), 15-10-20
                                        if (s_strSMU == "NIPXI")
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasITraceMode(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto, _RdCurr_Delay);
                                        else
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);

                                    }

                                    // pass out the test result label for every measurement channel
                                    string tempLabel = "SMUI_CH" + MeasSMU[i];
                                    foreach (string key in DicTestLabel.Keys)
                                    {
                                        if (key == tempLabel)
                                        {
                                            R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                            break;
                                        }
                                    }
                                }
                            }

                            //set back NPLC to faster rate for other test
                            //SetSMUSelect = new string[SetSMU.Count()];
                            //for (int i = 0; i < SetSMU.Count(); i++)
                            //{
                            //    float _NPLC = 0.1f;
                            //    int smuVChannel = Convert.ToInt16(SetSMU[i]);
                            //    Eq.Site[0]._Eq_SMUDriver.SetNPLC(Eq.Site[0]._SMUSetting[smuVChannel], Eq.Site[0]._EqSMU, _NPLC);
                            //}

                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            #endregion
                            break;

                        default:
                            MessageBox.Show("Test Parameter : " + _TestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }

                    break;
                case "MIPI":
                    switch (_TestParam.ToUpper())
                    {
                        #region MIPI

                        #region  MIPI - Fixed Mipi Pair and Slave Address (define from config file)
                        case "SETMIPI":

                            //Set MIPI
                            Eq.Site[0]._EqMiPiCtrl.TurnOn_VIO(0);      //mipi pair 0 - DIO 0, DIO 1 and DIO 2 - For DUT
                            Eq.Site[0]._EqMiPiCtrl.TurnOn_VIO(1);      //mipi pair 0 - DIO 3, DIO 4 and DIO 5 - For ref Unit on Test Board

                            Eq.Site[0]._EqMiPiCtrl.SendAndReadMIPICodes(out MIPI_Read_Successful, _MiPi_RegNo);
                            LibEqmtDriver.MIPI.Lib_Var.ReadSuccessful = MIPI_Read_Successful;
                            if (LibEqmtDriver.MIPI.Lib_Var.ReadSuccessful)
                                R_MIPI = 1;

                            //Measure SMU current
                            MeasSMU = _SMUMeasCh.Split(',');
                            if (_Test_SMU)
                            {
                                DelayMs(_RdCurr_Delay);
                                float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                for (int i = 0; i < MeasSMU.Count(); i++)
                                {
                                    int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                    if (_SMUILimitCh[smuIChannel] > 0)
                                    {
                                        R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                    }

                                    // pass out the test result label for every measurement channel
                                    string tempLabel = "SMUI_CH" + MeasSMU[i];
                                    foreach (string key in DicTestLabel.Keys)
                                    {
                                        if (key == tempLabel)
                                        {
                                            R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                            break;
                                        }
                                    }
                                }
                            }

                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            break;

                        case "SETMIPI_SMU":
                            //Set SMU
                            //string[] SetSMUSelect;

                            //pass to global variable to be use outside this function
                            EqmtStatus.SMU_CH = _SMUSetCh;

                            SetSMU = _SMUSetCh.Split(',');
                            MeasSMU = _SMUMeasCh.Split(',');

                            SetSMUSelect = new string[SetSMU.Count()];
                            for (int i = 0; i < SetSMU.Count(); i++)
                            {
                                int smuVChannel = Convert.ToInt16(SetSMU[i]);
                                SetSMUSelect[i] = Eq.Site[0]._SMUSetting[smuVChannel];       //rearrange the Eq.Site[0]._SMUSetting base on reqquired channel only from total of 8 channel available  
                                Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[smuVChannel], Eq.Site[0]._EqSMU, _SMUVCh[smuVChannel], _SMUILimitCh[smuVChannel]);
                            }

                            Eq.Site[0]._Eq_SMUDriver.DcOn(SetSMUSelect, Eq.Site[0]._EqSMU);

                            //Set MIPI
                            Eq.Site[0]._EqMiPiCtrl.TurnOn_VIO(0);      //mipi pair 0 - DIO 0, DIO 1 and DIO 2 - For DUT
                            Eq.Site[0]._EqMiPiCtrl.TurnOn_VIO(1);      //mipi pair 0 - DIO 3, DIO 4 and DIO 5 - For ref Unit on Test Board
                            Eq.Site[0]._EqMiPiCtrl.SendAndReadMIPICodes(out MIPI_Read_Successful, _MiPi_RegNo);
                            LibEqmtDriver.MIPI.Lib_Var.ReadSuccessful = MIPI_Read_Successful;
                            if (LibEqmtDriver.MIPI.Lib_Var.ReadSuccessful)
                                R_MIPI = 1;

                            if (_Test_SMU)
                            {
                                DelayMs(_RdCurr_Delay);
                                float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                for (int i = 0; i < MeasSMU.Count(); i++)
                                {
                                    int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                    if (_SMUILimitCh[smuIChannel] > 0)
                                    {
                                        R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                    }

                                    // pass out the test result label for every measurement channel
                                    string tempLabel = "SMUI_CH" + MeasSMU[i];
                                    foreach (string key in DicTestLabel.Keys)
                                    {
                                        if (key == tempLabel)
                                        {
                                            R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                            break;
                                        }
                                    }
                                }
                            }

                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            break;
                        #endregion

                        #region MIPI CUSTOM - flexible Mipi Reg, Mipi Pair and Slave Address (define from MIPI spreadsheet)
                        case "SETMIPI_CUSTOM":

                            //Search and return Data from Mipi custom spreadsheet 
                            searchMIPIKey(_TestParam, _SwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                            //Set MIPI
                            //Eq.Site[0]._EqMiPiCtrl.TurnOn_VIO(0);      //mipi pair 0 - DIO 0, DIO 1 and DIO 2 - For DUT
                            //Eq.Site[0]._EqMiPiCtrl.TurnOn_VIO(1);      //mipi pair 0 - DIO 3, DIO 4 and DIO 5 - For ref Unit on Test Board
                            Eq.Site[0]._EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                            Eq.Site[0]._EqMiPiCtrl.SendAndReadMIPICodesCustom(out MIPI_Read_Successful, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                            LibEqmtDriver.MIPI.Lib_Var.ReadSuccessful = MIPI_Read_Successful;
                            if (LibEqmtDriver.MIPI.Lib_Var.ReadSuccessful)
                                //if (WriteandReadMipi(CusMipiRegMap, CusPMTrigMap, CusMipiPair, CusSlaveAddr))
                                R_MIPI = 1;

                            //Measure SMU current
                            MeasSMU = _SMUMeasCh.Split(',');
                            if (_Test_SMU)
                            {
                                DelayMs(_RdCurr_Delay);
                                float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                for (int i = 0; i < MeasSMU.Count(); i++)
                                {
                                    int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                    if (_SMUILimitCh[smuIChannel] > 0)
                                    {
                                        R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                    }

                                    // pass out the test result label for every measurement channel
                                    string tempLabel = "SMUI_CH" + MeasSMU[i];
                                    foreach (string key in DicTestLabel.Keys)
                                    {
                                        if (key == tempLabel)
                                        {
                                            R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                            break;
                                        }
                                    }
                                }
                            }

                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            break;

                        case "SETMIPI_CUSTOM_SMU":
                            //Set SMU
                            //pass to global variable to be use outside this function
                            EqmtStatus.SMU_CH = _SMUSetCh;

                            SetSMU = _SMUSetCh.Split(',');
                            MeasSMU = _SMUMeasCh.Split(',');

                            SetSMUSelect = new string[SetSMU.Count()];
                            for (int i = 0; i < SetSMU.Count(); i++)
                            {
                                int smuVChannel = Convert.ToInt16(SetSMU[i]);
                                SetSMUSelect[i] = Eq.Site[0]._SMUSetting[smuVChannel];       //rearrange the Eq.Site[0]._SMUSetting base on reqquired channel only from total of 8 channel available  
                                Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[smuVChannel], Eq.Site[0]._EqSMU, _SMUVCh[smuVChannel], _SMUILimitCh[smuVChannel]);
                            }

                            Eq.Site[0]._Eq_SMUDriver.DcOn(SetSMUSelect, Eq.Site[0]._EqSMU);

                            //Search and return Data from Mipi custom spreadsheet 
                            searchMIPIKey(_TestParam, _SwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                            //Set MIPI
                            //Eq.Site[0]._EqMiPiCtrl.TurnOn_VIO(0);      //mipi pair 0 - DIO 0, DIO 1 and DIO 2 - For DUT
                            //Eq.Site[0]._EqMiPiCtrl.TurnOn_VIO(1);      //mipi pair 0 - DIO 3, DIO 4 and DIO 5 - For ref Unit on Test Board
                            Eq.Site[0]._EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                            Eq.Site[0]._EqMiPiCtrl.SendAndReadMIPICodesCustom(out MIPI_Read_Successful, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                            LibEqmtDriver.MIPI.Lib_Var.ReadSuccessful = MIPI_Read_Successful;
                            if (LibEqmtDriver.MIPI.Lib_Var.ReadSuccessful)
                                //if (WriteandReadMipi(CusMipiRegMap, CusPMTrigMap, CusMipiPair, CusSlaveAddr))
                                R_MIPI = 1;

                            if (_Test_SMU)
                            {
                                DelayMs(_RdCurr_Delay);
                                float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                for (int i = 0; i < MeasSMU.Count(); i++)
                                {
                                    int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                    if (_SMUILimitCh[smuIChannel] > 0)
                                    {
                                        R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                    }

                                    // pass out the test result label for every measurement channel
                                    string tempLabel = "SMUI_CH" + MeasSMU[i];
                                    foreach (string key in DicTestLabel.Keys)
                                    {
                                        if (key == tempLabel)
                                        {
                                            R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                            break;
                                        }
                                    }
                                }
                            }

                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            break;
                        #endregion

                        #region READ MIPI CUSTOM - flexible Mipi Reg, Mipi Pair and Slave Address (define from MIPI spreadsheet)

                        case "READMIPI_REG_CUSTOM":

                            //Search and return Data from Mipi custom spreadsheet 
                            searchMIPIKey(_TestParam, _SwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                            //Set MIPI and Read MIPI
                            Eq.Site[0]._EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                            switch (_Search_Method.ToUpper())
                            {
                                case "TEMP":
                                case "TEMPERATURE":

                                    if (FirstDut)
                                    {
                                        //debug mipi temp
                                        Eq.Site[0]._EqMiPiCtrl.SendAndReadMIPICodesCustom(out MIPI_Read_Successful, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                        LibEqmtDriver.MIPI.Lib_Var.ReadSuccessful = MIPI_Read_Successful;
                                        if (LibEqmtDriver.MIPI.Lib_Var.ReadSuccessful)
                                            R_MIPI = 1;
                                        //if (WriteandReadMipi(CusMipiRegMap, CusPMTrigMap, CusMipiPair, CusSlaveAddr))
                                        //    R_MIPI = 1;
                                        FirstDut = false;
                                        //debug mipi temp
                                    }

                                    #region Set SMU Channel - CMOS Temperature read out
                                    //Read temperature required VBatt to be turn ON
                                    //pass to global variable to be use outside this function
                                    EqmtStatus.SMU_CH = _SMUSetCh;

                                    SetSMU = _SMUSetCh.Split(',');
                                    MeasSMU = _SMUMeasCh.Split(',');

                                    SetSMUSelect = new string[SetSMU.Count()];
                                    for (int i = 0; i < SetSMU.Count(); i++)
                                    {
                                        int smuVChannel = Convert.ToInt16(SetSMU[i]);
                                        SetSMUSelect[i] = Eq.Site[0]._SMUSetting[smuVChannel];       //rearrange the Eq.Site[0]._SMUSetting base on required channel only from total of 8 channel available  
                                        Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[smuVChannel], Eq.Site[0]._EqSMU, _SMUVCh[smuVChannel], _SMUILimitCh[smuVChannel]);
                                    }

                                    Eq.Site[0]._Eq_SMUDriver.DcOn(SetSMUSelect, Eq.Site[0]._EqSMU);
                                    DelayMs(_RdCurr_Delay);
                                    #endregion

                                    dataDec_Conv = 0;
                                    R_MIPI = -999;

                                    Eq.Site[0]._EqMiPiCtrl.WriteMIPICodesCustom(CusMipiRegMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    DelayUs(100);       //fixed delay about 100uS before readback the register
                                    Eq.Site[0]._EqMiPiCtrl.ReadMIPICodesCustom(out dataDec_Conv, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    dutTempSensor((double)dataDec_Conv, out R_MIPI);

                                    if (!b_GE_Header)
                                    {
                                        _Test_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                        ResultBuilder.BuildResults(ref results, _TestParaName, "C", R_MIPI);
                                    }
                                    else
                                    {
                                        _Test_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 

                                        string GE_TestParam = null;
                                        Rslt_GE_Header = new s_GE_Header();
                                        Decode_GE_Header(TestPara, out Rslt_GE_Header);

                                        Rslt_GE_Header.Note = "_NOTE_" + _TestNum;      //re-assign ge header 
                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, _Test_SMU);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "C", R_MIPI);
                                    }
                                    break;

                                case "OTP":

                                    if (_Test_SMU)
                                    {
                                        #region Set SMU Channel - CMOS Temperature read out
                                        //Read temperature required VBatt to be turn ON
                                        //pass to global variable to be use outside this function
                                        EqmtStatus.SMU_CH = _SMUSetCh;

                                        SetSMU = _SMUSetCh.Split(',');
                                        MeasSMU = _SMUMeasCh.Split(',');

                                        SetSMUSelect = new string[SetSMU.Count()];
                                        for (int i = 0; i < SetSMU.Count(); i++)
                                        {
                                            int smuVChannel = Convert.ToInt16(SetSMU[i]);
                                            SetSMUSelect[i] = Eq.Site[0]._SMUSetting[smuVChannel];       //rearrange the Eq.Site[0]._SMUSetting base on required channel only from total of 8 channel available  
                                            Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[smuVChannel], Eq.Site[0]._EqSMU, _SMUVCh[smuVChannel], _SMUILimitCh[smuVChannel]);
                                        }

                                        Eq.Site[0]._Eq_SMUDriver.DcOn(SetSMUSelect, Eq.Site[0]._EqSMU);
                                        DelayMs(_RdCurr_Delay);
                                        #endregion

                                    }

                                    string[] RegisterArray = CusMipiRegMap.Split(':');

                                    foreach (var Register in RegisterArray)
                                    {
                                        Eq.Site[0]._EqMiPiCtrl.ReadMIPICodesCustom(out dataDec_Conv, Register, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));

                                        if (!b_GE_Header)
                                        {
                                            _Test_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                            ResultBuilder.BuildResults(ref results, _TestParaName, "dec", dataDec_Conv);
                                        }
                                        else
                                        {
                                            string GE_TestParam = null;
                                            Rslt_GE_Header = new s_GE_Header();
                                            Decode_GE_Header(TestPara, out Rslt_GE_Header);
                                            Rslt_GE_Header.Note = "_NOTE_" + _SwBand + "_" + Register + "_" + _TestNum;      //re-assign ge header 
                                            Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, _Test_SMU);
                                            ResultBuilder.BuildResults(ref results, GE_TestParam, "dec", dataDec_Conv);
                                        }
                                    }

                                    break;

                                default:
                                    //Eq.Site[0]._EqMiPiCtrl.WriteMIPICodesCustom(CusMipiRegMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    //DelayMs(_RdCurr_Delay);
                                    Eq.Site[0]._EqMiPiCtrl.ReadMIPICodesCustom(out R_ReadMipiReg, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));

                                    if (!b_GE_Header)
                                    {
                                        _Test_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                        ResultBuilder.BuildResults(ref results, _TestParaName, "dec", R_ReadMipiReg);
                                    }
                                    else
                                    {
                                        string GE_TestParam = null;
                                        Rslt_GE_Header = new s_GE_Header();
                                        Decode_GE_Header(TestPara, out Rslt_GE_Header);
                                        Rslt_GE_Header.Note = "_NOTE_" + _TestNum;      //re-assign ge header 
                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, _Test_SMU);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "dec", R_ReadMipiReg);
                                    }
                                    break;
                            }
                            //Eq.Site[0]._EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            break;

                        case "READ_OTP_CUSTOM":

                            //Init variable
                            dataDec = new int[2];

                            //Search and return Data from Mipi custom spreadsheet 
                            searchMIPIKey(_TestParam, _SwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                            //Set MIPI and Read MIPI
                            //example CusMipiRegMap must be in '42:XX 43:XX' where 42 is MSB reg address and XX - Data (don't care) ,  43 is LSB reg address and XX - Data (don't care)
                            Eq.Site[0]._EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                            DelayMs(_RdCurr_Delay);

                            #region Read Back MIPI register
                            biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter

                            switch (_Search_Method.ToUpper())
                            {
                                case "MFG_ID":
                                case "MFGID":
                                    R_ReadMipiReg = -999;   //set to fail value (default)
                                    R_MIPI = -999;          //set to fail value (default)
                                    tmpOutData = 0;
                                    totalbits = 16;         //total bit for 2 register address is 16bits (binary)
                                    effectiveBits = 16;     //Jedi OTP - Module S/N only used up until 14bits (binary)
                                    dataBinary = new string[2];
                                    appendBinary = null;
                                    dataDec = new int[2];

                                    for (int i = 0; i < biasDataArr.Length; i++)
                                    {
                                        Eq.Site[0]._EqMiPiCtrl.ReadMIPICodesCustom(out tmpOutData, biasDataArr[i], CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                        dataDec[i] = tmpOutData;
                                        dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');        //Convert DEC to 8 Bit Binary
                                        appendBinary = appendBinary + dataBinary[i];                            //concatenations for 2 set of binari data (MSB = binaryData[0] , LSB = binaryData[1])
                                    }

                                    if (appendBinary.Length == effectiveBits)                                   //Make sure that the length is 16bits
                                    {
                                        R_ReadMipiReg = Convert.ToInt32(appendBinary, 2);                      //Convert Binary to Decimal
                                    }

                                    //Build Test Result
                                    _Test_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                    if (R_ReadMipiReg == Convert.ToInt32(mfgLotID))
                                    {
                                        R_MIPI = 1;
                                    }
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_MIPI", "NA", R_MIPI);
                                    ResultBuilder.BuildResults(ref results, _TestParaName, "dec", R_ReadMipiReg);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_MSB", "dec", dataDec[0]);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_LSB", "dec", dataDec[1]);

                                    break;
                                case "UNIT_ID":
                                case "UNITID":
                                    R_ReadMipiReg = -999;   //set to fail value (default)
                                    R_MIPI = -999;          //set to fail value (default)
                                    tmpOutData = 0;
                                    totalbits = 16;         //total bit for 2 register address is 16bits (binary)
                                    effectiveBits = 14;     //Jedi OTP - Module S/N only used up until 14bits (binary)
                                    dataBinary = new string[2];
                                    appendBinary = null;
                                    dataDec = new int[2];

                                    for (int i = 0; i < biasDataArr.Length; i++)
                                    {
                                        Eq.Site[0]._EqMiPiCtrl.ReadMIPICodesCustom(out tmpOutData, biasDataArr[i], CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                        dataDec[i] = tmpOutData;
                                        dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');        //Convert DEC to 8 Bit Binary
                                        appendBinary = appendBinary + dataBinary[i];                            //concatenations for 2 set of binari data (MSB = binaryData[0] , LSB = binaryData[1])
                                    }

                                    if (appendBinary.Length > effectiveBits)                                                        //Make sure that the length is 16bits
                                    {
                                        effectiveData = appendBinary.Remove(0, appendBinary.Length - effectiveBits);                //remove first 2Bits from MSB to make effectiveData = 14 bits
                                        R_ReadMipiReg = Convert.ToInt32(effectiveData, 2);                                          //Convert Binary to Decimal
                                    }

                                    //Build Test Result
                                    _Test_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                    //ResultBuilder.BuildResults(ref results, _TestParaName + "_MIPI", "NA", R_MIPI);
                                    ResultBuilder.BuildResults(ref results, _TestParaName, "dec", R_ReadMipiReg);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_MSB", "dec", dataDec[0]);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_LSB", "dec", dataDec[1]);

                                    break;

                                default:
                                    MessageBox.Show("Test Parameter : " + _TestParam + "(" + _Search_Method + ") - Search Method not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    break;
                            }
                            #endregion

                            //Eq.Site[0]._EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            break;

                        #endregion

                        #region BURN OTP JEDI1 - flexible Mipi Reg, Mipi Pair and Slave Address (define from MIPI spreadsheet)

                        case "BURN_OTP_JEDI":

                            #region Check OTP status and sort data to burn

                            //Initialize to default
                            b_lockBit = true;
                            i_lockBit = 0;
                            i_testFlag = 0;
                            b_testFlag = true;
                            i_bitPos = 0;     //bit position to compare (0 -> LSB , 7 -> MSB)
                            BurnOTP = false;

                            dataBinary = new string[2];
                            appendBinary = null;
                            dataDec = new int[2];

                            //data to program 
                            dataHex = new string[2];

                            #region Set SMU
                            //pass to global variable to be use outside this function
                            EqmtStatus.SMU_CH = _SMUSetCh;

                            SetSMU = _SMUSetCh.Split(',');
                            MeasSMU = _SMUMeasCh.Split(',');

                            SetSMUSelect = new string[SetSMU.Count()];
                            for (int i = 0; i < SetSMU.Count(); i++)
                            {
                                int smuVChannel = Convert.ToInt16(SetSMU[i]);
                                SetSMUSelect[i] = Eq.Site[0]._SMUSetting[smuVChannel];       //rearrange the Eq.Site[0]._SMUSetting base on reqquired channel only from total of 8 channel available  
                                Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[smuVChannel], Eq.Site[0]._EqSMU, _SMUVCh[smuVChannel], _SMUILimitCh[smuVChannel]);
                            }

                            Eq.Site[0]._Eq_SMUDriver.DcOn(SetSMUSelect, Eq.Site[0]._EqSMU);
                            #endregion

                            #region Decode MIPI Register - Data from Mipi custom spreadsheet

                            //Search and return Data from Mipi custom spreadsheet 
                            searchMIPIKey(_TestParam, _SwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                            //Set MIPI and Read MIPI
                            Eq.Site[0]._EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                            DelayMs(_RdCurr_Delay);

                            #endregion

                            switch (_Search_Method.ToUpper())
                            {
                                case "MFG_ID":
                                case "MFGID":
                                    dataSizeHex = "0xFFFF";     //max data size is 16bit
                                    BurnOTP = false;

                                    dataDec_Conv = Convert.ToInt32(mfgLotID);         //convert string to int
                                    if (dataDec_Conv <= (Convert.ToInt32(dataSizeHex, 16)))
                                    {
                                        //MSB - dataHex[0] , LSB - dataHex[1]
                                        Sort_MSBnLSB(dataDec_Conv, out dataHex[0], out dataHex[1]);
                                        BurnOTP = true;         //set flag to true for burning otp
                                    }
                                    break;
                                case "UNIT_ID":
                                case "UNITID":
                                    dataSizeHex = "0x3FFF";         //max data size is 14bit 
                                    BurnOTP = false;

                                    //Set the DUT SN ID and Check if file exist , if not exist -> create and write default SN
                                    OTPLogFilePath = mfgLotID_Path + SNPFile.FileOutput_FileName + "_" + mfgLotID + ".txt";
                                    if (tmpUnit_No == 1)
                                    {
                                        if (!Directory.Exists(@mfgLotID_Path))
                                            System.IO.Directory.CreateDirectory(@mfgLotID_Path);

                                        if (!File.Exists(@OTPLogFilePath))
                                        {
                                            // write default SN to file 
                                            try
                                            {
                                                ArrayList LocalTextList = new ArrayList();
                                                LocalTextList.Add("[SN_ID]");
                                                LocalTextList.Add("SN_COUNT = 0");

                                                IO_TxtFile.CreateWrite_TextFile(@OTPLogFilePath, LocalTextList);
                                            }
                                            catch (FileNotFoundException)
                                            {
                                                throw new FileNotFoundException("Cannot Write Existing file!");
                                            }
                                        }
                                    }

                                    //read SN from files
                                    if (File.Exists(@OTPLogFilePath))
                                        tmpData = IO_TxtFile.ReadTextFile(OTPLogFilePath, "SN_ID", "SN_COUNT");

                                    otpUnitID = Convert.ToInt32(tmpData) + 1;       //next SN to burn 
                                    dataDec_Conv = otpUnitID;

                                    if (dataDec_Conv <= (Convert.ToInt32(dataSizeHex, 16)))
                                    {
                                        //MSB - dataHex[0] , LSB - dataHex[1]
                                        Sort_MSBnLSB(dataDec_Conv, out dataHex[0], out dataHex[1]);
                                        BurnOTP = true;         //set flag to true for burning otp
                                    }
                                    break;

                                case "TEST_FLAG":
                                case "TESTFLAG":
                                    //read test flag register
                                    Eq.Site[0]._EqMiPiCtrl.ReadMIPICodesCustom(out i_testFlag, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    i_bitPos = 7;     //bit position to compare (0 -> LSB , 7 -> MSB)
                                    b_testFlag = (Convert.ToByte(i_testFlag) & (1 << i_bitPos)) != 0;       //compare bit 7 -> if 0 >> false (not program) ; if 1 >> true (done program)

                                    //read lockbit register
                                    Eq.Site[0]._EqMiPiCtrl.ReadMIPICodesCustom(out i_lockBit, "E5", CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    i_bitPos = 0;     //bit position to compare (0 -> LSB , 7 -> MSB)
                                    b_lockBit = (Convert.ToByte(i_lockBit) & (1 << i_bitPos)) != 0;       //compare bit 7 -> if 0 >> false (not program) ; if 1 >> true (done program)

                                    //Normal Unit - All test pass -> Lockbit(E5 = 0) and TestFlag(Bit7 = 0) - both not program
                                    if (ResultBuilder.FailedTests[0].Count == 0 && !b_testFlag && !b_lockBit)
                                    {
                                        dataHex[0] = "80";      //Burn bit 7 -> 0x80 >> 1000 0000
                                        BurnOTP = true;         //set flag to true for burning otp
                                    }
                                    break;
                                default:
                                    MessageBox.Show("Test Parameter : " + _TestParam + "(" + _Search_Method + ") - Search Method not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    break;
                            }
                            #endregion

                            #region Decode MIPI Register and Burn OTP

                            biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter
                            efuseCtrlAddress = new int[3];
                            tempData = new string[2];

                            for (int i = 0; i < biasDataArr.Length; i++)
                            {
                                //Note : EFuse Control Register
                                //efuse cell_0 (0xE0, mirror address 0x0D)
                                //efuse cell_1 (0xE1, mirror address 0x0E)
                                //efuse cell_2 (0xE2, mirror address 0x21)
                                //efuse cell_3 (0xE3, mirror address 0x40)
                                //efuse cell_4 (0xE4, mirror address 0x41)

                                tempData = biasDataArr[i].Split(':');
                                switch (tempData[0].ToUpper())
                                {
                                    case "0D":
                                    case "E0":              //efuse cell_0 (0xE0)
                                        efuseCtrlAddress[2] = 0;
                                        efuseCtrlAddress[1] = 0;
                                        efuseCtrlAddress[0] = 0;
                                        break;
                                    case "0E":
                                    case "E1":              //efuse cell_1 (0xE1)
                                        efuseCtrlAddress[2] = 0;
                                        efuseCtrlAddress[1] = 0;
                                        efuseCtrlAddress[0] = 1;
                                        break;
                                    case "21":
                                    case "E2":              //efuse cell_2 (0xE2)
                                        efuseCtrlAddress[2] = 0;
                                        efuseCtrlAddress[1] = 1;
                                        efuseCtrlAddress[0] = 0;
                                        break;
                                    case "40":
                                    case "E3":              //efuse cell_3 (0xE3)
                                        efuseCtrlAddress[2] = 0;
                                        efuseCtrlAddress[1] = 1;
                                        efuseCtrlAddress[0] = 1;
                                        break;
                                    case "41":
                                    case "E4":              //efuse cell_4  (0xE4)
                                        efuseCtrlAddress[2] = 1;
                                        efuseCtrlAddress[1] = 0;
                                        efuseCtrlAddress[0] = 0;
                                        break;
                                    case "E5":                  //unknown Mirror Address - Design document not specified , use efuse cell_5 (0xE5)
                                        efuseCtrlAddress[2] = 1;
                                        efuseCtrlAddress[1] = 0;
                                        efuseCtrlAddress[0] = 1;
                                        break;
                                    default:
                                        MessageBox.Show("Test Parameter : " + _TestParam + "(" + tempData[0].ToUpper() + ") - OTP Address not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                        break;
                                }

                                #region Burn OTP Data

                                if (BurnOTP)
                                {
                                    //Burn twice to double confirm the otp programming is done completely
                                    //JediOTPBurn("C0", efuseCtrlAddress, dataHex[i], CusMipiPair, CusSlaveAddr);
                                    //JediOTPBurn("C0", efuseCtrlAddress, dataHex[i], CusMipiPair, CusSlaveAddr);

                                    //Burn5x to double confirm the otp programming is done completely
                                    for (int cnt = 0; cnt < 5; cnt++)
                                    {
                                        JediOTPBurn("C0", efuseCtrlAddress, dataHex[i], CusMipiPair, CusSlaveAddr);
                                    }
                                }

                                #endregion
                            }

                            #endregion

                            #region Read Back MIPI register

                            #region Turn off SMU and VIO - to prepare for read back mipi register
                            if (EqmtStatus.SMU)
                            {
                                Eq.Site[0]._Eq_SMUDriver.DcOff(Eq.Site[0]._SMUSetting, Eq.Site[0]._EqSMU);
                            }

                            Eq.Site[0]._EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                            DelayMs(_RdCurr_Delay);

                            //Set MIPI and Read MIPI
                            Eq.Site[0]._EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                            DelayMs(_RdCurr_Delay);
                            #endregion

                            switch (_Search_Method.ToUpper())
                            {
                                case "MFG_ID":
                                case "MFGID":
                                    R_MIPI = -999;          //set to fail value (default)
                                    tmpOutData = 0;
                                    totalbits = 16;         //total bit for 2 register address is 16bits (binary)
                                    effectiveBits = 16;     //Jedi OTP - Module S/N only used up until 14bits (binary)
                                    dataBinary = new string[2];
                                    appendBinary = null;
                                    dataDec = new int[2];

                                    for (int i = 0; i < biasDataArr.Length; i++)
                                    {
                                        Eq.Site[0]._EqMiPiCtrl.ReadMIPICodesCustom(out tmpOutData, biasDataArr[i], CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                        dataDec[i] = tmpOutData;
                                        dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');        //Convert DEC to 8 Bit Binary
                                        appendBinary = appendBinary + dataBinary[i];                            //concatenations for 2 set of binari data (MSB = binaryData[0] , LSB = binaryData[1])
                                    }

                                    if (appendBinary.Length == effectiveBits)                                   //Make sure that the length is 16bits
                                    {
                                        R_ReadMipiReg = Convert.ToInt32(appendBinary, 2);                      //Convert Binary to Decimal
                                    }

                                    //Build Test Result
                                    //_Test_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                    //ResultBuilder.BuildResults(ref results, _TestParaName + "_MIPI", "NA", R_MIPI);
                                    if (R_ReadMipiReg == Convert.ToInt32(mfgLotID))
                                    {
                                        R_MIPI = 1;
                                    }
                                    ResultBuilder.BuildResults(ref results, _TestParaName, "dec", R_ReadMipiReg);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_MSB", "dec", dataDec[0]);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_LSB", "dec", dataDec[1]);

                                    break;
                                case "UNIT_ID":
                                case "UNITID":
                                    R_MIPI = -999;          //set to fail value (default)
                                    tmpOutData = 0;
                                    totalbits = 16;         //total bit for 2 register address is 16bits (binary)
                                    effectiveBits = 14;     //Jedi OTP - Module S/N only used up until 14bits (binary)
                                    dataBinary = new string[2];
                                    appendBinary = null;
                                    dataDec = new int[2];

                                    for (int i = 0; i < biasDataArr.Length; i++)
                                    {
                                        Eq.Site[0]._EqMiPiCtrl.ReadMIPICodesCustom(out tmpOutData, biasDataArr[i], CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                        dataDec[i] = tmpOutData;
                                        dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');        //Convert DEC to 8 Bit Binary
                                        appendBinary = appendBinary + dataBinary[i];                            //concatenations for 2 set of binari data (MSB = binaryData[0] , LSB = binaryData[1])
                                    }

                                    if (appendBinary.Length > effectiveBits)                                                        //Make sure that the length is 16bits
                                    {
                                        effectiveData = appendBinary.Remove(0, appendBinary.Length - effectiveBits);                //remove first 2Bits from MSB to make effectiveData = 14 bits
                                        R_ReadMipiReg = Convert.ToInt32(effectiveData, 2);                                          //Convert Binary to Decimal
                                    }

                                    //Build Test Result
                                    //_Test_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                    //ResultBuilder.BuildResults(ref results, _TestParaName + "_MIPI", "NA", R_MIPI);
                                    if (R_ReadMipiReg == otpUnitID)
                                    {
                                        R_MIPI = 1;

                                        // write Unit ID data to file
                                        try
                                        {
                                            ArrayList LocalTextList = new ArrayList();
                                            LocalTextList.Add("[SN_ID]");
                                            LocalTextList.Add("SN_COUNT = " + otpUnitID);

                                            IO_TxtFile.CreateWrite_TextFile(@OTPLogFilePath, LocalTextList);
                                        }
                                        catch (FileNotFoundException)
                                        {
                                            throw new FileNotFoundException("Cannot Write Existing file!");
                                        }
                                    }

                                    ResultBuilder.BuildResults(ref results, _TestParaName, "dec", R_ReadMipiReg);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_MSB", "dec", dataDec[0]);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_LSB", "dec", dataDec[1]);

                                    break;

                                case "TEST_FLAG":
                                case "TESTFLAG":
                                    //set to fail value (default)
                                    R_MIPI = 0;
                                    b_testFlag = false;
                                    i_testFlag = 0;

                                    //read test flag register
                                    Eq.Site[0]._EqMiPiCtrl.ReadMIPICodesCustom(out i_testFlag, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    i_bitPos = 7;     //bit position to compare (0 -> LSB , 7 -> MSB)
                                    b_testFlag = (Convert.ToByte(i_testFlag) & (1 << i_bitPos)) != 0;       //compare bit 7 -> if 0 >> false (not program) ; if 1 >> true (done program)

                                    if (BurnOTP)
                                    {
                                        if (b_testFlag && BurnOTP)
                                            R_MIPI = 1;         //Only return '1' if burn OTP Flag and Read Bit 7 = 1
                                        else
                                            R_MIPI = 0;         //Only return '0' if burn OTP Flag and Read Bit 7 = 0 (note - readback Bit 7 failure)
                                    }

                                    //Retest unit and report out accordingly - Note : Not from Fresh Lots , already tested before
                                    if (!BurnOTP)
                                    {
                                        //Retest Unit (2A lot) Test Count pass and not pass SParam - lockBit(E5 = 0) and TestFlag(Bit7 = 1) already program
                                        if (ResultBuilder.FailedTests[0].Count == 0 && b_testFlag && !b_lockBit)
                                        {
                                            R_MIPI = 2;
                                        }
                                        //Retest Unit (2A lot) Test Count Fail and not pass SParam - lockBit(E5 = 0) and TestFlag(Bit7 = 1) already program
                                        if (ResultBuilder.FailedTests[0].Count > 0 && b_testFlag && !b_lockBit)
                                        {
                                            R_MIPI = 3;
                                        }
                                        //Retest Unit (redo all) - pass SParam - lockBit(E5 = 1) and TestFlag(Bit7 = 1) already program
                                        if (ResultBuilder.FailedTests[0].Count == 0 && b_testFlag && b_lockBit)
                                        {
                                            R_MIPI = 4;
                                        }
                                        //Retest Unit (redo all) Test Count Fail and pass SParam - lockBit(E5 = 1) and TestFlag(Bit7 = 1) already program
                                        if (ResultBuilder.FailedTests[0].Count > 0 && b_testFlag && b_lockBit)
                                        {
                                            R_MIPI = 5;
                                        }
                                    }

                                    _Test_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_MIPI", "NA", R_MIPI);

                                    break;

                                default:
                                    MessageBox.Show("Test Parameter : " + _TestParam + "(" + _Search_Method + ") - Search Method not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    break;
                            }
                            #endregion

                            Eq.Site[0]._EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            break;

                        #endregion

                        #region BURN OTP JEDI2 - flexible Mipi Reg, Mipi Pair and Slave Address (define from MIPI spreadsheet)

                        case "BURN_OTP_JEDI2":

                            #region Check OTP status and sort data to burn

                            //Initialize to default
                            b_lockBit = true;
                            i_lockBit = 0;
                            i_testFlag = 0;
                            b_testFlag = true;
                            i_bitPos = 0;     //bit position to compare (0 -> LSB , 7 -> MSB)
                            BurnOTP = false;

                            dataBinary = new string[2];
                            appendBinary = null;
                            dataDec = new int[2];

                            //data to program 
                            dataHex = new string[2];

                            #region Set SMU
                            //pass to global variable to be use outside this function
                            EqmtStatus.SMU_CH = _SMUSetCh;

                            SetSMU = _SMUSetCh.Split(',');
                            MeasSMU = _SMUMeasCh.Split(',');

                            SetSMUSelect = new string[SetSMU.Count()];
                            for (int i = 0; i < SetSMU.Count(); i++)
                            {
                                int smuVChannel = Convert.ToInt16(SetSMU[i]);
                                SetSMUSelect[i] = Eq.Site[0]._SMUSetting[smuVChannel];       //rearrange the Eq.Site[0]._SMUSetting base on reqquired channel only from total of 8 channel available  
                                Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[smuVChannel], Eq.Site[0]._EqSMU, _SMUVCh[smuVChannel], _SMUILimitCh[smuVChannel]);
                            }

                            Eq.Site[0]._Eq_SMUDriver.DcOn(SetSMUSelect, Eq.Site[0]._EqSMU);
                            #endregion

                            #region Decode MIPI Register - Data from Mipi custom spreadsheet & Read From DUT register

                            //Search and return Data from Mipi custom spreadsheet 
                            searchMIPIKey(_TestParam, _SwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);
                            biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter

                            //Set MIPI and Read MIPI
                            Eq.Site[0]._EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                            DelayMs(_RdCurr_Delay);

                            //Read DUT register
                            for (int i = 0; i < biasDataArr.Length; i++)
                            {
                                Eq.Site[0]._EqMiPiCtrl.ReadMIPICodesCustom(out tmpOutData, biasDataArr[i], CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');        //Convert DEC to 8 Bit Binary
                                appendBinary = appendBinary + dataBinary[i];                            //concatenations for 2 set of binari data (MSB = binaryData[0] , LSB = binaryData[1])
                            }

                            //data use for empty register comparison (byright return data should be '0' for empty register)
                            tmpOutData_DecConv = Convert.ToInt32(appendBinary, 2);                      //Convert Binary to Decimal

                            #endregion

                            switch (_Search_Method.ToUpper())
                            {
                                case "MFG_ID":
                                case "MFGID":
                                    dataSizeHex = "0xFFFF";     //max data size is 16bit
                                    BurnOTP = false;

                                    dataDec_Conv = Convert.ToInt32(mfgLotID);         //convert string to int
                                    if (dataDec_Conv <= (Convert.ToInt32(dataSizeHex, 16)))
                                    {
                                        //MSB - dataHex[0] , LSB - dataHex[1]
                                        Sort_MSBnLSB(dataDec_Conv, out dataHex[0], out dataHex[1]);

                                        if (tmpOutData_DecConv < 1)     //if register data is blank , then only set OTP Burn status to 'TRUE'
                                        {
                                            BurnOTP = true;         //set flag to true for burning otp
                                        }
                                    }
                                    break;
                                case "UNIT_ID":
                                case "UNITID":
                                    dataSizeHex = "0x7FFF";         //max data size is 15bit 
                                    BurnOTP = false;

                                    //Set the DUT SN ID and Check if file exist , if not exist -> create and write default SN
                                    OTPLogFilePath = mfgLotID_Path + SNPFile.FileOutput_FileName + "_" + mfgLotID + ".txt";
                                    if (tmpUnit_No == 1)
                                    {
                                        if (!Directory.Exists(@mfgLotID_Path))
                                            System.IO.Directory.CreateDirectory(@mfgLotID_Path);

                                        if (!File.Exists(@OTPLogFilePath))
                                        {
                                            //get the 1st running number for unit id from local setting file 
                                            StartNo_UNIT_ID = IO_TxtFile.ReadTextFile(LocSetFilePath, "TESTSITE_UNIT_ID", "START_UNIT_ID");
                                            StopNo_UNIT_ID = IO_TxtFile.ReadTextFile(LocSetFilePath, "TESTSITE_UNIT_ID", "STOP_UNIT_ID");

                                            // write default SN to file 
                                            try
                                            {
                                                ArrayList LocalTextList = new ArrayList();
                                                LocalTextList.Add("[SN_ID]");
                                                LocalTextList.Add("SN_COUNT = " + StartNo_UNIT_ID);     //write default start unit_id (unique starting number for every test site)

                                                IO_TxtFile.CreateWrite_TextFile(@OTPLogFilePath, LocalTextList);
                                            }
                                            catch (FileNotFoundException)
                                            {
                                                throw new FileNotFoundException("Cannot Write Existing file!");
                                            }
                                        }
                                    }

                                    //read SN from files
                                    if (File.Exists(@OTPLogFilePath))
                                        tmpData = IO_TxtFile.ReadTextFile(OTPLogFilePath, "SN_ID", "SN_COUNT");

                                    otpUnitID = Convert.ToInt32(tmpData) + 1;       //next SN to burn 
                                    dataDec_Conv = otpUnitID;

                                    if (dataDec_Conv <= (Convert.ToInt32(dataSizeHex, 16)))
                                    {
                                        //MSB - dataHex[0] , LSB - dataHex[1]
                                        Sort_MSBnLSB(dataDec_Conv, out dataHex[0], out dataHex[1]);

                                        if (tmpOutData_DecConv < 1)     //if register data is blank , then only set OTP Burn status to 'TRUE'
                                        {
                                            BurnOTP = true;         //set flag to true for burning otp
                                        }
                                    }
                                    break;

                                case "TEST_FLAG":
                                case "TESTFLAG":
                                    //read test flag register
                                    Eq.Site[0]._EqMiPiCtrl.ReadMIPICodesCustom(out i_testFlag, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    i_bitPos = 0;     //bit position to compare (0 -> LSB , 7 -> MSB)
                                    b_testFlag = (Convert.ToByte(i_testFlag) & (1 << i_bitPos)) != 0;       //compare bit 1 -> if 0 >> false (not program) ; if 1 >> true (done program)

                                    //read lockbit register
                                    Eq.Site[0]._EqMiPiCtrl.ReadMIPICodesCustom(out i_lockBit, "EB", CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    i_bitPos = 7;     //bit position to compare (0 -> LSB , 7 -> MSB)
                                    b_lockBit = (Convert.ToByte(i_lockBit) & (1 << i_bitPos)) != 0;       //compare bit 7 -> if 0 >> false (not program) ; if 1 >> true (done program)

                                    //Normal Unit - All test pass -> Lockbit(EB = 0) and TestFlag(Bit0 = 0) - both not program
                                    if (ResultBuilder.FailedTests[0].Count == 0 && !b_testFlag && !b_lockBit)
                                    {
                                        dataHex[0] = "1";      //Burn bit 1 -> 0x02 >> 0000 0010
                                        BurnOTP = true;         //set flag to true for burning otp
                                    }
                                    break;

                                case "CM_ID":
                                case "CMID":
                                    //read CM ID register
                                    Eq.Site[0]._EqMiPiCtrl.ReadMIPICodesCustom(out i_testFlag, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    i_bitPos = 7;     //bit position to compare (0 -> LSB , 7 -> MSB)
                                    b_testFlag = (Convert.ToByte(i_testFlag) & (1 << i_bitPos)) != 0;       //compare bit 1 -> if 0 >> false (not program) ; if 1 >> true (done program)

                                    //read lockbit register
                                    Eq.Site[0]._EqMiPiCtrl.ReadMIPICodesCustom(out i_lockBit, "EB", CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    i_bitPos = 7;     //bit position to compare (0 -> LSB , 7 -> MSB)
                                    b_lockBit = (Convert.ToByte(i_lockBit) & (1 << i_bitPos)) != 0;       //compare bit 7 -> if 0 >> false (not program) ; if 1 >> true (done program)

                                    //Normal Unit - All test pass -> Lockbit(EB = 0) and CM_ID (Bit7 = 0) - both not program
                                    if (ResultBuilder.FailedTests[0].Count == 0 && !b_testFlag && !b_lockBit)
                                    {
                                        //Have not define how to decode the lot traveller for CM Site defination - Shaz 24/11/2017
                                        switch (CM_SITE.ToUpper())
                                        {
                                            case "ASEKOR":
                                                dataHex[0] = "80";      //Burn bit 7 -> 0x80 >> 1000 0000
                                                BurnOTP = true;        //set flag to true for burning otp
                                                break;
                                            case "AMKOR":
                                                dataHex[0] = "00";      //Burn bit 7 -> 0x00 >> 0000 0000
                                                BurnOTP = true;        //set flag to true for burning otp
                                                break;
                                            default:
                                                MessageBox.Show("Test Parameter : " + _TestParam + "(" + CM_SITE + ") - CM SITE not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                                BurnOTP = false;        //set flag to false for burning otp
                                                break;
                                        }
                                    }
                                    break;

                                default:
                                    MessageBox.Show("Test Parameter : " + _TestParam + "(" + _Search_Method + ") - Search Method not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    break;
                            }
                            #endregion

                            #region Decode MIPI Register and Burn OTP

                            biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter
                            efuseCtrlAddress = new int[3];
                            tempData = new string[2];

                            for (int i = 0; i < biasDataArr.Length; i++)
                            {
                                //Note : EFuse Control Register
                                //efuse cell_3 (0xE3, mirror address 0x46)
                                //efuse cell_4 (0xE4, mirror address 0x47)
                                //efuse cell_5 (0xE5, mirror address 0x48)

                                tempData = biasDataArr[i].Split(':');
                                switch (tempData[0].ToUpper())
                                {
                                    case "E0":              //efuse cell_0 (0xE0)
                                        efuseCtrlAddress[2] = 0;
                                        efuseCtrlAddress[1] = 0;
                                        efuseCtrlAddress[0] = 0;
                                        break;
                                    case "E1":              //efuse cell_1 (0xE1)
                                        efuseCtrlAddress[2] = 0;
                                        efuseCtrlAddress[1] = 0;
                                        efuseCtrlAddress[0] = 1;
                                        break;
                                    case "E2":              //efuse cell_2 (0xE2)
                                        efuseCtrlAddress[2] = 0;
                                        efuseCtrlAddress[1] = 1;
                                        efuseCtrlAddress[0] = 0;
                                        break;
                                    case "E3":              //efuse cell_3 (0xE3)
                                        efuseCtrlAddress[2] = 0;
                                        efuseCtrlAddress[1] = 1;
                                        efuseCtrlAddress[0] = 1;
                                        break;
                                    case "E4":              //efuse cell_4  (0xE4)
                                        efuseCtrlAddress[2] = 1;
                                        efuseCtrlAddress[1] = 0;
                                        efuseCtrlAddress[0] = 0;
                                        break;
                                    case "E5":              //efuse cell_5  (0xE5)
                                        efuseCtrlAddress[2] = 1;
                                        efuseCtrlAddress[1] = 0;
                                        efuseCtrlAddress[0] = 1;
                                        break;
                                    default:
                                        MessageBox.Show("Test Parameter : " + _TestParam + "(" + tempData[0].ToUpper() + ") - OTP Address not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                        break;
                                }

                                #region Burn OTP Data

                                if (BurnOTP)
                                {
                                    //Burn twice to double confirm the otp programming is done completely
                                    //JediOTPBurn("C0", efuseCtrlAddress, dataHex[i], CusMipiPair, CusSlaveAddr);
                                    //JediOTPBurn("C0", efuseCtrlAddress, dataHex[i], CusMipiPair, CusSlaveAddr);

                                    //Burn5x to double confirm the otp programming is done completely
                                    for (int cnt = 0; cnt < 5; cnt++)
                                    {
                                        JediOTPBurn("C0", efuseCtrlAddress, dataHex[i], CusMipiPair, CusSlaveAddr);
                                    }
                                }

                                #endregion
                            }

                            #endregion

                            #region Read Back MIPI register

                            #region Turn off SMU and VIO - to prepare for read back mipi register
                            if (EqmtStatus.SMU)
                            {
                                Eq.Site[0]._Eq_SMUDriver.DcOff(Eq.Site[0]._SMUSetting, Eq.Site[0]._EqSMU);
                            }

                            Eq.Site[0]._EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                            DelayMs(_RdCurr_Delay);

                            //Set MIPI and Read MIPI
                            Eq.Site[0]._EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                            DelayMs(_RdCurr_Delay);
                            #endregion

                            switch (_Search_Method.ToUpper())
                            {
                                case "MFG_ID":
                                case "MFGID":
                                    R_MIPI = -999;          //set to fail value (default)
                                    tmpOutData = 0;
                                    totalbits = 16;         //total bit for 2 register address is 16bits (binary)
                                    effectiveBits = 16;     //Jedi OTP - Module S/N only used up until 14bits (binary)
                                    dataBinary = new string[2];
                                    appendBinary = null;
                                    dataDec = new int[2];

                                    for (int i = 0; i < biasDataArr.Length; i++)
                                    {
                                        Eq.Site[0]._EqMiPiCtrl.ReadMIPICodesCustom(out tmpOutData, biasDataArr[i], CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                        dataDec[i] = tmpOutData;
                                        dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');        //Convert DEC to 8 Bit Binary
                                        appendBinary = appendBinary + dataBinary[i];                            //concatenations for 2 set of binari data (MSB = binaryData[0] , LSB = binaryData[1])
                                    }

                                    if (appendBinary.Length == effectiveBits)                                   //Make sure that the length is 16bits
                                    {
                                        R_ReadMipiReg = Convert.ToInt32(appendBinary, 2);                      //Convert Binary to Decimal
                                    }

                                    //Build Test Result
                                    //_Test_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                    //ResultBuilder.BuildResults(ref results, _TestParaName + "_MIPI", "NA", R_MIPI);
                                    if (R_ReadMipiReg == Convert.ToInt32(mfgLotID))
                                    {
                                        R_MIPI = 1;
                                    }
                                    ResultBuilder.BuildResults(ref results, _TestParaName, "dec", R_ReadMipiReg);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_MSB", "dec", dataDec[0]);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_LSB", "dec", dataDec[1]);

                                    break;
                                case "UNIT_ID":
                                case "UNITID":
                                    R_MIPI = -999;          //set to fail value (default)
                                    tmpOutData = 0;
                                    totalbits = 16;         //total bit for 2 register address is 16bits (binary)
                                    effectiveBits = 15;     //Jedi OTP - Module S/N only used up until 14bits (binary)
                                    dataBinary = new string[2];
                                    appendBinary = null;
                                    dataDec = new int[2];

                                    for (int i = 0; i < biasDataArr.Length; i++)
                                    {
                                        Eq.Site[0]._EqMiPiCtrl.ReadMIPICodesCustom(out tmpOutData, biasDataArr[i], CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                        dataDec[i] = tmpOutData;
                                        dataBinary[i] = Convert.ToString(tmpOutData, 2).PadLeft(8, '0');        //Convert DEC to 8 Bit Binary
                                        appendBinary = appendBinary + dataBinary[i];                            //concatenations for 2 set of binari data (MSB = binaryData[0] , LSB = binaryData[1])
                                    }

                                    if (appendBinary.Length > effectiveBits)                                                        //Make sure that the length is 16bits
                                    {
                                        effectiveData = appendBinary.Remove(0, appendBinary.Length - effectiveBits);                //remove bits 7 from MSB to make effectiveData = 15 bits
                                        R_ReadMipiReg = Convert.ToInt32(effectiveData, 2);                                          //Convert Binary to Decimal
                                    }

                                    //Build Test Result
                                    //_Test_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                    //ResultBuilder.BuildResults(ref results, _TestParaName + "_MIPI", "NA", R_MIPI);
                                    if (R_ReadMipiReg == otpUnitID)
                                    {
                                        R_MIPI = 1;

                                        // write Unit ID data to file
                                        try
                                        {
                                            ArrayList LocalTextList = new ArrayList();
                                            LocalTextList.Add("[SN_ID]");
                                            LocalTextList.Add("SN_COUNT = " + otpUnitID);

                                            IO_TxtFile.CreateWrite_TextFile(@OTPLogFilePath, LocalTextList);
                                        }
                                        catch (FileNotFoundException)
                                        {
                                            throw new FileNotFoundException("Cannot Write Existing file!");
                                        }
                                    }

                                    ResultBuilder.BuildResults(ref results, _TestParaName, "dec", R_ReadMipiReg);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_MSB", "dec", dataDec[0]);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_LSB", "dec", dataDec[1]);

                                    break;

                                case "TEST_FLAG":
                                case "TESTFLAG":
                                    //set to fail value (default)
                                    R_MIPI = 0;
                                    b_testFlag = false;
                                    i_testFlag = 0;

                                    //read test flag register
                                    Eq.Site[0]._EqMiPiCtrl.ReadMIPICodesCustom(out i_testFlag, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    i_bitPos = 0;     //bit position to compare (0 -> LSB , 7 -> MSB)
                                    b_testFlag = (Convert.ToByte(i_testFlag) & (1 << i_bitPos)) != 0;       //compare bit 1 -> if 0 >> false (not program) ; if 1 >> true (done program)

                                    if (BurnOTP)
                                    {
                                        if (b_testFlag && BurnOTP)
                                            R_MIPI = 1;         //Only return '1' if burn OTP Flag and Read Bit 1 = 1
                                        else
                                            R_MIPI = 0;         //Only return '0' if burn OTP Flag and Read Bit 1 = 0 (note - readback Bit 1 failure)
                                    }

                                    //Retest unit and report out accordingly - Note : Not from Fresh Lots , already tested before
                                    if (!BurnOTP)
                                    {
                                        //Retest Unit (2A lot) Test Count pass and not pass SParam - lockBit(E5 = 0) and TestFlag(Bit1 = 1) already program
                                        if (ResultBuilder.FailedTests[0].Count == 0 && b_testFlag && !b_lockBit)
                                        {
                                            R_MIPI = 2;
                                        }
                                        //Retest Unit (2A lot) Test Count Fail and not pass SParam - lockBit(E5 = 0) and TestFlag(Bit1 = 1) already program
                                        if (ResultBuilder.FailedTests[0].Count > 0 && b_testFlag && !b_lockBit)
                                        {
                                            R_MIPI = 3;
                                        }
                                        //Retest Unit (redo all) - pass SParam - lockBit(E5 = 1) and TestFlag(Bit1 = 1) already program
                                        if (ResultBuilder.FailedTests[0].Count == 0 && b_testFlag && b_lockBit)
                                        {
                                            R_MIPI = 4;
                                        }
                                        //Retest Unit (redo all) Test Count Fail and pass SParam - lockBit(E5 = 1) and TestFlag(Bit1 = 1) already program
                                        if (ResultBuilder.FailedTests[0].Count > 0 && b_testFlag && b_lockBit)
                                        {
                                            R_MIPI = 5;
                                        }
                                    }

                                    _Test_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_MIPI", "NA", R_MIPI);

                                    break;

                                case "CM_ID":
                                case "CMID":
                                    //set to fail value (default)
                                    R_MIPI = 0;
                                    b_testFlag = false;
                                    i_testFlag = 0;

                                    //read test flag register
                                    Eq.Site[0]._EqMiPiCtrl.ReadMIPICodesCustom(out i_testFlag, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                    i_bitPos = 7;     //bit position to compare (0 -> LSB , 7 -> MSB)
                                    b_testFlag = (Convert.ToByte(i_testFlag) & (1 << i_bitPos)) != 0;       //compare bit 1 -> if 0 >> false (not program) ; if 1 >> true (done program)

                                    if (BurnOTP)
                                    {
                                        if (b_testFlag && BurnOTP)
                                            R_MIPI = 1;         //Only return '1' if burn OTP Flag and Read Bit 7 = 1 (note - ASEKr = 1)
                                        else
                                            R_MIPI = 0;         //Only return '0' if burn OTP Flag and Read Bit 7 = 0 (note - AmKor = 0)
                                    }

                                    //Retest unit and report out accordingly - Note : Not from Fresh Lots , already tested before
                                    if (!BurnOTP)
                                    {
                                        //Fail burn otp - set to -999 as default data
                                        R_MIPI = -999;
                                    }

                                    _Test_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_MIPI", "NA", R_MIPI);

                                    break;

                                default:
                                    MessageBox.Show("Test Parameter : " + _TestParam + "(" + _Search_Method + ") - Search Method not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    break;
                            }
                            #endregion

                            Eq.Site[0]._EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            break;

                        #endregion

                        //case "MIPI_LEAKAGE":
                        //    #region MIPI Leakage Test
                        //    #region SMU Biasing
                        //    //pass to global variable to be use outside this function
                        //    EqmtStatus.SMU_CH = _SMUSetCh;

                        //    SetSMU = _SMUSetCh.Split(',');
                        //    MeasSMU = _SMUMeasCh.Split(',');

                        //    SetSMUSelect = new string[SetSMU.Count()];
                        //    for (int i = 0; i < SetSMU.Count(); i++)
                        //    {
                        //        int smuVChannel = Convert.ToInt16(SetSMU[i]);
                        //        SetSMUSelect[i] = Eq.Site[0]._SMUSetting[smuVChannel];       //rearrange the Eq.Site[0]._SMUSetting base on reqquired channel only from total of 8 channel available  
                        //        Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[smuVChannel], Eq.Site[0]._EqSMU, _SMUVCh[smuVChannel], _SMUILimitCh[smuVChannel]);
                        //    }

                        //    Eq.Site[0]._Eq_SMUDriver.DcOn(SetSMUSelect, Eq.Site[0]._EqSMU);
                        //    #endregion

                        //    #region MIPI Biasing and MIPI command
                        //    //to select which Mipi Pin to set and measure - Format in TCF(_MIPI_SetCh) 0,1 -> means S_CLK & S_DATA to set/measure
                        //    //Note : 0 -> S_CLK, 1 - S_DATA, 2 - S_VIO
                        //    VSetMIPI = _MIPI_SetCh.Split(',');
                        //    IMeasMIPI = _MIPI_MeasCh.Split(',');

                        //    LibEqmtDriver.MIPI.s_MIPI_DCSet[] tmpMipi_DCSet = new LibEqmtDriver.MIPI.s_MIPI_DCSet[VSetMIPI.Length];
                        //    LibEqmtDriver.MIPI.s_MIPI_DCMeas[] tmpMipi_DCMeas = new LibEqmtDriver.MIPI.s_MIPI_DCMeas[IMeasMIPI.Length];

                        //    for (int i = 0; i < VSetMIPI.Length; i++)
                        //    {
                        //        int mipiCh = Convert.ToInt16(VSetMIPI[i]);
                        //        tmpMipi_DCSet[i].ChNo = mipiCh;
                        //        tmpMipi_DCSet[i].VChSet = _MIPI_VSetCh[mipiCh];
                        //        tmpMipi_DCSet[i].IChSet = _MIPI_ILimitCh[mipiCh];
                        //    }

                        //    //Search and return Data from Mipi custom spreadsheet 
                        //    searchMIPIKey(_TestParam, _SwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                        //    Eq.Site[0]._EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                        //    //Set DUT to PDM mode
                        //    Eq.Site[0]._EqMiPiCtrl.SendAndReadMIPICodesCustom(out MIPI_Read_Successful, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                        //    LibEqmtDriver.MIPI.Lib_Var.ReadSuccessful = MIPI_Read_Successful;
                        //    if (LibEqmtDriver.MIPI.Lib_Var.ReadSuccessful)
                        //        R_MIPI = 1;

                        //    //Set MIPI to PPMU Mode (Set Voltage and measure current)
                        //    Eq.Site[0]._EqMiPiCtrl.SetMeasureMIPIcurrent(_RdCurr_Delay, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16), tmpMipi_DCSet, IMeasMIPI, out tmpMipi_DCMeas);

                        //    #endregion

                        //    #region SMU measurement
                        //    if (_Test_SMU)
                        //    {
                        //        DelayMs(_RdCurr_Delay);
                        //        float _NPLC = 0.1f; // float _NPLC = 1;  toh
                        //        for (int i = 0; i < MeasSMU.Count(); i++)
                        //        {
                        //            int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                        //            if (_SMUILimitCh[smuIChannel] > 0)
                        //            {
                        //                R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                        //            }

                        //            // pass out the test result label for every measurement channel
                        //            string tempLabel = "SMUI_CH" + MeasSMU[i];
                        //            foreach (string key in DicTestLabel.Keys)
                        //            {
                        //                if (key == tempLabel)
                        //                {
                        //                    R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                        //                    break;
                        //                }
                        //            }
                        //        }
                        //    }
                        //    #endregion

                        //    #region built MIPI Leakage result
                        //    for (int i = 0; i < tmpMipi_DCMeas.Length; i++)
                        //    {
                        //        ResultBuilder.BuildResults(ref results, _TestParaName + "_" + tmpMipi_DCMeas[i].MipiPinNames, "A", tmpMipi_DCMeas[i].IChMeas);
                        //    }
                        //    #endregion

                        //    tTime.Stop();
                        //    //if (_Test_TestTime)
                        //    //{
                        //    //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                        //    //}
                        //    #endregion
                        //    break;

                        default:
                            MessageBox.Show("Test Parameter : " + _TestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;

                            #endregion
                    }
                    break;

                case "MIPI_OTP":
                    switch (_TestParam.ToUpper())
                    {
                        #region READ OTP REGISTER with customize bit selection
                        case "READ_OTP_SELECTIVE_BIT":
                            int iPre_OTPMSB = 0;
                            int iPre_MaximumDecValueofDataSize_OTPmoduleIDMSB = 0;
                            R_ReadMipiReg = -999;   //set to fail value (default)
                            R_MIPI = -999;          //set to fail value (default)
                            tmpOutData_DecConv = -999;  //set to fail value (default)
                            R_partSN2_2DID = -999;   //set to fail value (default)

                            if (_Search_Method.ToUpper() != "READ_2DID_FROM_OTHER_OTP_BIT" && _Search_Method.ToUpper() != "OTP_MODULE_ID_2DID_DELTA")
                            {
                                #region Read Register and return Data - derive from Mipi custom spreadsheet
                                //Search and return Data from Mipi custom spreadsheet 
                                searchMIPIKey(_TestParam, _SwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                                biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter
                                dataHex = new string[biasDataArr.Length];

                                if (dataHex.Length < 6)
                                {
                                    readout_OTPReg_viaEffectiveBit(_RdCurr_Delay, _SwBand, CusMipiRegMap, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out tmpOutData_DecConv, out dataSizeHex);
                                }
                                #endregion
                            }

                            switch (_Search_Method.ToUpper())
                            {
                                case "UNITID":
                                case "UNIT_ID":
                                case "UNIT_ID_MANUAL_SET":

                                    #region OTP Read

                                    if (_Search_Method.ToUpper() == "UNIT_ID")
                                    {
                                        int OTPdata = 0;

                                        searchMIPIKey(_TestParam, _SwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                                        if (CusMipiRegMap.Contains(" "))
                                        {
                                            string[] strOTPData = CusMipiRegMap.Split(' ');

                                            for (int i = 0; i < strOTPData.Count(); i++)
                                            {
                                                biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter
                                                dataHex = new string[biasDataArr.Length];

                                                dataDec_Conv = 0;

                                                Eq.Site[0]._EqMiPiCtrl.WriteMIPICodesCustom("F0:B0 F0:B1 " + strOTPData[i] + " F0:A1", Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                                Eq.Site[0]._EqMiPiCtrl.ReadMIPICodesCustom(out dataDec_Conv, "82:FF", CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));

                                                if (i != 0)
                                                {
                                                    OTPdata = OTPdata + (dataDec_Conv << (8 * i));
                                                }

                                                else
                                                    OTPdata = dataDec_Conv;
                                            }
                                        }

                                        R_ReadMipiReg = OTPdata;
                                    }
                                    else
                                    {

                                        #region Read Register and return Data - derive from Mipi custom spreadsheet
                                        //Search and return Data from Mipi custom spreadsheet 
                                        searchMIPIKey(_TestParam, _SwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey); biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter

                                        readout_OTPReg_viaEffectiveBit(_RdCurr_Delay, _SwBand, CusMipiRegMap, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out tmpOutData_DecConv, out dataSizeHex);
                                        #endregion

                                        R_ReadMipiReg = tmpOutData_DecConv;
                                    }
                                    #endregion
                                    break;
                                case "UNIT_2DID":
                                    #region UNIT 2DID Read back

                                    //reset to default value
                                    R_MIPI = -999;
                                    R_ReadMipiReg_long = -999;
                                    R_partSN2_2DID = -999;

                                    //2DID local variable
                                    string str2DIDmark = null;
                                    string strPartSN2 = null;
                                    string[] data2DID = new string[biasDataArr.Length];

                                    //Initialize variable
                                    bool b_reg2DID = false;     //bool for read DUT register
                                    bool b_str2DID = false;     //bool for read Handler 2DID
                                    bool success = false;
                                    char[] sort2DID;
                                    int regCount = 0;

                                    appendDec = null;
                                    string[] raw_OutdataHex;
                                    string raw_OutdataDec = null;
                                    dataDec_Conv = 0;

                                    #region Read Register and return Data - derive from Mipi custom spreadsheet
                                    //Search and return Data from Mipi custom spreadsheet 
                                    searchMIPIKey(_TestParam, _SwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                                    biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter
                                    dataHex = new string[biasDataArr.Length];
                                    raw_OutdataHex = new string[biasDataArr.Length];
                                    data2DID = new string[biasDataArr.Length];

                                    readout_OTPReg_viaEffectiveBit(_RdCurr_Delay, _SwBand, CusMipiRegMap, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out raw_OutdataHex, out dataSizeHex);

                                    //check if blank register
                                    appendDec = null;
                                    b_reg2DID = true;       //set to default

                                    for (int i = 0; i < raw_OutdataHex.Length; i++)
                                    {
                                        dataDec_Conv = int.Parse(raw_OutdataHex[i], System.Globalization.NumberStyles.HexNumber);

                                        if (dataDec_Conv <= 99)       //Note 2DID register max value until dec 99 only
                                        {
                                            if (dataDec_Conv <= 9)
                                            {
                                                raw_OutdataDec = "0" + dataDec_Conv;
                                            }
                                            else
                                            {
                                                raw_OutdataDec = dataDec_Conv.ToString();
                                            }
                                        }
                                        else
                                        {
                                            R_MIPI = 2; //register readback fail flag
                                            b_reg2DID = false;
                                        }

                                        appendDec = appendDec + raw_OutdataDec;
                                    }

                                    success = long.TryParse(appendDec, out R_ReadMipiReg_long);  //check if return data from register must not exceed '99' for every register address

                                    if (!success)
                                    {
                                        R_MIPI = 2; //register readback fail flag
                                        R_ReadMipiReg_long = -99999;
                                    }
                                    #endregion

                                    #region Read 2DID from Handler - Sort to last 12 digit

                                    if (EqmtStatus.handler)
                                    {
                                        #region 2DID Handler TCP
                                        HandlerLotInfo hli = new HandlerLotInfo();
                                        hli = TcpClient.LastTestedInTheLotQuery();
                                        #endregion

                                        #region decode 2DID
                                        //default setting
                                        //Total EFuse Register = 6
                                        //Total data = 24 numerics , Effective numeric data from 13 to 24 , numeric data  1 to 12 not use
                                        //Example str2DIDmark = "193713219999999999100500";

                                        str2DIDmark = string.Format("{0:D24}", hli.strBarcodeID);

                                        if (str2DIDmark != null && str2DIDmark.Length == 24)
                                        {
                                            b_str2DID = true;       //set to default
                                            dataDec_Conv = 0;
                                            count = 0;
                                            sort2DID = str2DIDmark.ToCharArray();

                                            for (int i = 12; i < str2DIDmark.Length; i++)
                                            {
                                                count++;
                                                if (count == 2) //do checking for every 2 number
                                                {
                                                    if (regCount < dataHex.Length)
                                                    {
                                                        //take 2 numerical data to form 1x Hex Data
                                                        //eg sort2DID[12] = '5' & eg sort2DID[13] = '0'
                                                        //Then if the appended sort2DID[12] & sort2DID[13] is a valid number (note : max number is 99)
                                                        string tmpDataDec = sort2DID[i - 1].ToString() + sort2DID[i].ToString();

                                                        success = Int32.TryParse(tmpDataDec, out dataDec_Conv);       //check if 2DID marking data is valid -  ie valid number(can convert from Hex to Integer back and forth & must be less than '99') 

                                                        if (!success)
                                                        {
                                                            MessageBox.Show("Test Parameter : " + _TestParam + " - String to Decimal Conversion Unsuccessful >> Input String With Wrong Format : " + tmpDataDec, "MyDUT", MessageBoxButtons.OK);
                                                            dataDec_Conv = 0;       //set to default '0' if fail conversion
                                                            i = str2DIDmark.Length; //force stop the loop because of converison failure
                                                            b_str2DID = false;
                                                            break;
                                                        }

                                                        if (success)
                                                        {
                                                            //store scan 2DID value for later comparison
                                                            if (dataDec_Conv <= 9)
                                                            {
                                                                data2DID[regCount] = "0" + dataDec_Conv.ToString();
                                                            }
                                                            else
                                                            {
                                                                data2DID[regCount] = dataDec_Conv.ToString();
                                                            }

                                                            dataHex[regCount] = dataDec_Conv.ToString("X2");        //convert dec to hex and pass data for burn otp process
                                                            count = 0;
                                                            regCount++;
                                                        }
                                                    }
                                                }
                                            }

                                            if (b_str2DID)
                                            {
                                                //re-arrange only partSN2_2DID
                                                strPartSN2 = null;
                                                for (int i = 0; i < data2DID.Length; i++)
                                                {
                                                    strPartSN2 = strPartSN2 + data2DID[i];
                                                }

                                                R_partSN2_2DID = Convert.ToInt64(strPartSN2);
                                            }
                                            else
                                            {
                                                //2DID marking data is invalid
                                                str2DIDmark = "-999";
                                                R_partSN2_2DID = -99999;
                                                R_MIPI = 3; //handler 2DID readback fail flag
                                            }
                                        }
                                        else
                                        {
                                            //if cannot read from handler
                                            b_str2DID = false;
                                            str2DIDmark = "-999";
                                            R_partSN2_2DID = -99999;
                                            R_MIPI = 3; //handler 2DID readback fail flag
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        R_MIPI = 3;     //fail to read handler 2DID 
                                    }

                                    #endregion

                                    #region compare Read Register vs last 12 digit 2DID (PartSN_2)

                                    if (b_str2DID && b_reg2DID)
                                    {
                                        if (R_ReadMipiReg_long == Previous2DID)
                                        {
                                            R_MIPI = 5;     //double unit or duplication 
                                        }
                                        else
                                        {
                                            R_MIPI = 0;     //pass status
                                            Previous2DID = R_ReadMipiReg_long;
                                        }
                                    }
                                    #endregion

                                    #endregion
                                    break;

                                case "READ_2DID_FROM_OTHER_OTP_BIT":
								case "OTP_MODULE_ID_2DID_DELTA":
                                    #region Read 2DID From other OTP BIT (TX - MODULE ID, PCB PANEL ID / RX - PCB STRIP ID, PCB LOT ID)

                                    //int iPre_OTPMSB = 0;
                                    //int iPre_MaximumDecValueofDataSize_OTPmoduleIDMSB = 0;

                                    if (_SwBand.Contains(","))
                                    {
                                        // parsing string
                                        string[] ParsingOTPRegister = _SwBand.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                        int arraycount = ParsingOTPRegister.GetLength(0);

                                        appendDec = null;
                                        b_reg2DID = true;       //set to default

                                        for (int i = 0; i < arraycount; i++)
                                        {
                                            dataDec_Conv = 0;
                                            string raw_OutDataDec_2DID_FROM_OTHER = null;

                                            searchMIPIKey(_TestParam, ParsingOTPRegister[i], out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                                            readout_OTPReg_viaEffectiveBit(_RdCurr_Delay, ParsingOTPRegister[i], CusMipiRegMap, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out raw_OutdataHex, out dataSizeHex);

                                            // dataSizeHex (String Value) to Binary Value

                                            string[] tmpDataSizeHex = new string[dataSizeHex.Length / 2];

                                            for (int j = 0; j < tmpDataSizeHex.Length; j++)
                                            {
                                                tmpDataSizeHex[j] = dataSizeHex.Substring(j * 2, 2);
                                            }

                                            int[] countflag = new int[tmpDataSizeHex.Length];
                                            int intValue = 0;
                                            string strDataSizeBin = null;

                                            for (int j = 0; j < countflag.Length; j++)
                                            {

                                                intValue = int.Parse(tmpDataSizeHex[j], System.Globalization.NumberStyles.HexNumber);
                                                strDataSizeBin = (Convert.ToString(intValue, 2)).Replace("0", "");

                                                for (int k = 0; k < strDataSizeBin.Length; k++)
                                                {
                                                    if (strDataSizeBin[k] == '1')
                                                        countflag[j]++;
                                                }
                                            }

                                            intValue = int.Parse(dataSizeHex, System.Globalization.NumberStyles.HexNumber);
                                            strDataSizeBin = (Convert.ToString(intValue, 2)).Replace("0", "");
                                            int MaximumDecValueofDataSize = Convert.ToInt32(strDataSizeBin, 2);

                                            string tmpOutDataBin = null; // Temp Out Data (Total) : Binary 
                                            string OutDataBin = null; // Final Out Data (Total) : Binary

                                            for (int j = 0; j < raw_OutdataHex.Length; j++)
                                            {

                                                int intValue_outdata = int.Parse(raw_OutdataHex[j], System.Globalization.NumberStyles.HexNumber);
                                                tmpOutDataBin = tmpOutDataBin + Convert.ToString(intValue_outdata, 2).PadLeft(countflag[j], '0');
                                            }

                                            for (int j = 0; j < strDataSizeBin.Length; j++)
                                            {
                                                if (strDataSizeBin[j] == '1')
                                                {
                                                    if (tmpOutDataBin[j] == '1')
                                                        OutDataBin = OutDataBin + "1";
                                                    else
                                                        OutDataBin = OutDataBin + "0";
                                                }
                                            }

                                            dataDec_Conv = Convert.ToInt32(OutDataBin, 2); // convert to Dec from binary data (Total Out Data)

                                            if (ParsingOTPRegister[i].Contains("_RX") || ParsingOTPRegister[i].Contains("_MSB"))
                                            {
                                                iPre_OTPMSB = dataDec_Conv;
                                                iPre_MaximumDecValueofDataSize_OTPmoduleIDMSB = MaximumDecValueofDataSize;

                                                //dataDec_Conv = dataDec_Conv << 4; // Hard Coding - it will change by the count of LSB.
                                                //iPre_OTPMSB = dataDec_Conv;
                                                //iPre_MaximumDecValueofDataSize_OTPmoduleIDMSB = MaximumDecValueofDataSize << 1;
                                                continue;
                                            }

                                            else if (ParsingOTPRegister[i].Contains("_TX") || ParsingOTPRegister[i].Contains("_LSB"))
                                            {
                                                // data shift of msb part
                                                for (int k = 0; k < countflag.Length; k++)
                                                {
                                                    iPre_OTPMSB = iPre_OTPMSB << countflag[k];
                                                    iPre_MaximumDecValueofDataSize_OTPmoduleIDMSB = iPre_MaximumDecValueofDataSize_OTPmoduleIDMSB << countflag[k];
                                                }

                                                dataDec_Conv = iPre_OTPMSB + dataDec_Conv;
                                                MaximumDecValueofDataSize = MaximumDecValueofDataSize + iPre_MaximumDecValueofDataSize_OTPmoduleIDMSB;
                                            }


                                            if (dataDec_Conv <= MaximumDecValueofDataSize)
                                            {
                                                if (MaximumDecValueofDataSize >= 1000)
                                                {

                                                    if (dataDec_Conv < 1000 && dataDec_Conv >= 100)
                                                        raw_OutDataDec_2DID_FROM_OTHER = "0" + dataDec_Conv;

                                                    else if (dataDec_Conv < 100 && dataDec_Conv >= 10)
                                                        raw_OutDataDec_2DID_FROM_OTHER = "00" + dataDec_Conv;

                                                    else if (dataDec_Conv < 10 && dataDec_Conv >= 0)
                                                        raw_OutDataDec_2DID_FROM_OTHER = "000" + dataDec_Conv;
                                                    else
                                                        raw_OutDataDec_2DID_FROM_OTHER = Convert.ToString(dataDec_Conv);
                                                }
                                                else if (MaximumDecValueofDataSize > 256)
                                                {

                                                    if (dataDec_Conv < 100 && dataDec_Conv >= 10)
                                                        raw_OutDataDec_2DID_FROM_OTHER = "00" + dataDec_Conv;

                                                    else if (dataDec_Conv < 10 && dataDec_Conv >= 0)
                                                        raw_OutDataDec_2DID_FROM_OTHER = "000" + dataDec_Conv;
                                                    else
                                                        raw_OutDataDec_2DID_FROM_OTHER = "0" + Convert.ToString(dataDec_Conv);

                                                }
                                                else
                                                {
                                                    if (dataDec_Conv < 10 && dataDec_Conv >= 0)
                                                        raw_OutDataDec_2DID_FROM_OTHER = "0" + dataDec_Conv;
                                                    else
                                                        raw_OutDataDec_2DID_FROM_OTHER = Convert.ToString(dataDec_Conv);
                                                }
                                            }
                                            else
                                            {
                                                R_MIPI = 2; //register readback fail flag
                                                b_reg2DID = false;
                                            }

                                            appendDec = appendDec + raw_OutDataDec_2DID_FROM_OTHER;

                                            // Save PCB Info to PCB Info Structure
                                            if (_TestParaName.ToUpper().Contains("OTP_MODULE_ID"))
                                            {
                                                if (ParsingOTPRegister[i].ToUpper().Contains("PCB_PANEL_ID"))
                                                {
                                                    PCBUnitTrace.PCBUnitTrace_PcbPanel = dataDec_Conv;
                                                }
                                                else if (ParsingOTPRegister[i].ToUpper().Contains("PCB_STRIP_ID"))
                                                {
                                                    PCBUnitTrace.PCBUnitTrace_PcbStrip = dataDec_Conv;
                                                }
                                                else if (ParsingOTPRegister[i].ToUpper().Contains("PCB_LOT_ID"))
                                                {
                                                    PCBUnitTrace.PCBUnitTrace_PcbLOT = dataDec_Conv;
                                                }
                                                else if (ParsingOTPRegister[i].ToUpper().Contains("MODULE_ID"))
                                                {
                                                    PCBUnitTrace.PCBUnitTrace_PcbModuleID = dataDec_Conv;
                                                }
                                            }
                                        }

                                        success = long.TryParse(appendDec, out R_ReadMipiReg_long);  //check if return data from register must not exceed '99' for every register address
                                        if (!success)
                                        {
                                            R_MIPI = 2; //register readback fail flag
                                            R_ReadMipiReg_long = -99999;
                                        }
                                    }

                                    if (_Search_Method.ToUpper() == "READ_2DID_FROM_OTHER_OTP_BIT" && _TestParaName.ToUpper().Contains("OTP_MODULE_ID"))
                                    {
                                        PCBUnitTrace.PCBUnitTrace_PcbStrip_max_X = Convert.ToDouble(DicCalInfo.GetValueOrDefault(DataFilePath.CalPath_PCB_STRIP_X, "38"));
                                        PCBUnitTrace.PCBUnitTrace_PcbStrip_max_Y = Convert.ToDouble(DicCalInfo.GetValueOrDefault(DataFilePath.CalPath_PCB_STRIP_Y, "14"));
                                        PCBUnitTrace.PCBUnitTrace_PcbPanel_max_X = Convert.ToDouble(DicCalInfo.GetValueOrDefault(DataFilePath.CalPath_PCB_PANEL_STRIP_X, "2"));
                                        PCBUnitTrace.PCBUnitTrace_PcbPanel_max_Y = Convert.ToDouble(DicCalInfo.GetValueOrDefault(DataFilePath.CalPath_PCB_PANEL_STRIP_Y, "4"));

                                        PCBUnitTrace.PCBUnitTrace_PcbStrip_X =
                                            ((PCBUnitTrace.PCBUnitTrace_PcbModuleID - 1) % PCBUnitTrace.PCBUnitTrace_PcbStrip_max_X) + 1;
                                        PCBUnitTrace.PCBUnitTrace_PcbStrip_Y =
                                            PCBUnitTrace.PCBUnitTrace_PcbStrip_max_Y - Math.Truncate((PCBUnitTrace.PCBUnitTrace_PcbModuleID - 1) / PCBUnitTrace.PCBUnitTrace_PcbStrip_max_X);
                                        PCBUnitTrace.PCBUnitTrace_PcbPanel_X =
                                             PCBUnitTrace.PCBUnitTrace_PcbStrip_X + ((PCBUnitTrace.PCBUnitTrace_PcbStrip - 1) % PCBUnitTrace.PCBUnitTrace_PcbPanel_max_X) * PCBUnitTrace.PCBUnitTrace_PcbStrip_max_X;
                                        PCBUnitTrace.PCBUnitTrace_PcbPanel_Y =
                                            PCBUnitTrace.PCBUnitTrace_PcbStrip_Y + Math.Truncate((PCBUnitTrace.PCBUnitTrace_PcbStrip - 1) / PCBUnitTrace.PCBUnitTrace_PcbPanel_max_X) * PCBUnitTrace.PCBUnitTrace_PcbStrip_max_Y;

                                        double strip_count_from_max_x = Math.Abs(PCBUnitTrace.PCBUnitTrace_PcbStrip_X - PCBUnitTrace.PCBUnitTrace_PcbStrip_max_X - 1);
                                        double strip_count_from_max_y = Math.Abs(PCBUnitTrace.PCBUnitTrace_PcbStrip_Y - PCBUnitTrace.PCBUnitTrace_PcbStrip_max_Y - 1);
                                        double panel_count_from_max_x =
                                            Math.Abs(PCBUnitTrace.PCBUnitTrace_PcbPanel_X - (PCBUnitTrace.PCBUnitTrace_PcbStrip_max_X * PCBUnitTrace.PCBUnitTrace_PcbPanel_max_X) - 1);
                                        double panel_count_from_max_y =
                                            Math.Abs(PCBUnitTrace.PCBUnitTrace_PcbPanel_Y - (PCBUnitTrace.PCBUnitTrace_PcbStrip_max_Y * PCBUnitTrace.PCBUnitTrace_PcbPanel_max_Y) - 1);

                                        PCBUnitTrace.PCBUnitTrace_PCBstrip_Edge =
                                            ForNumeric.Min(PCBUnitTrace.PCBUnitTrace_PcbStrip_X, PCBUnitTrace.PCBUnitTrace_PcbStrip_Y, strip_count_from_max_x, strip_count_from_max_y);
                                        PCBUnitTrace.PCBUnitTrace_PCBpanel_Edge =
                                            ForNumeric.Min(PCBUnitTrace.PCBUnitTrace_PcbPanel_X, PCBUnitTrace.PCBUnitTrace_PcbPanel_Y, panel_count_from_max_x, panel_count_from_max_y);

                                        if (
                                             (PCBUnitTrace.PCBUnitTrace_PcbModuleID <= 0) ||
                                             (PCBUnitTrace.PCBUnitTrace_PcbModuleID > (PCBUnitTrace.PCBUnitTrace_PcbStrip_max_X * PCBUnitTrace.PCBUnitTrace_PcbStrip_max_Y)) ||
                                             (PCBUnitTrace.PCBUnitTrace_PcbStrip <= 0) ||
                                             (PCBUnitTrace.PCBUnitTrace_PcbStrip > (PCBUnitTrace.PCBUnitTrace_PcbPanel_max_X * PCBUnitTrace.PCBUnitTrace_PcbPanel_max_Y))
                                            )
                                        {
                                            PCBUnitTrace.PCBUnitTrace_PcbStrip_X = -999;
                                            PCBUnitTrace.PCBUnitTrace_PcbStrip_Y = -999;
                                            PCBUnitTrace.PCBUnitTrace_PcbPanel_X = -999;
                                            PCBUnitTrace.PCBUnitTrace_PcbPanel_Y = -999;
                                            PCBUnitTrace.PCBUnitTrace_PCBpanel_Edge = -999;
                                            PCBUnitTrace.PCBUnitTrace_PCBstrip_Edge = -999;
                                        }
                                    }
									else
									{
                                        if (Delta2DIDCheckEnable == true && _TestParaName.Contains("M_OTP_MODULE_ID_2DID_DELTA"))
                                        {
                                            string Barcode2DID = ATFCrossDomainWrapper.GetStringFromCache("2DID_OTPID", "999");

                                            if (Barcode2DID.Length < 12)
                                            {
                                                ResultBuilder.M_OTP_MODULE_ID_2DID_SYSTEM = Convert.ToInt64(Barcode2DID);
                                                R_MIPI_2DID_DELTA = 1; //2DID and ModuleID comparison readback fail flag
                                            }
                                            else if (Barcode2DID.Length >= 24)
                                            {                                                
                                                string Barcode2DID2 = Barcode2DID;
                                                Barcode2DID = Barcode2DID.Substring(Barcode2DID.Length - 12, 12);
                                                Barcode2DID2 = Barcode2DID2.Substring(Barcode2DID2.Length - 15, 15);
                                                ResultBuilder.M_OTP_MODULE_ID_2DID_SYSTEM = Convert.ToInt64(Barcode2DID2);
                                                R_MIPI_2DID_DELTA = Convert.ToInt64(Barcode2DID) - R_ReadMipiReg_long; // 0 is success to read the OTP Module ID
                                                if (R_MIPI_2DID_DELTA != 0)
                                                {
                                                    R_MIPI_2DID_DELTA = 1; //2DID and ModuleID comparison readback fail flag
                                                }
                                            }
                                        }
									 }

                                    #endregion
                                    break;

                                default:
                                    R_ReadMipiReg = tmpOutData_DecConv;
                                    // Add Mfg readback error check
                                    if (_TestParaName == "MFG_ID")
                                    {
                                        if (DeltaMfgIDCheckEnable != true)
                                        {
                                            MFG_ID_ReadError = -1;
                                        }
                                        else
                                        {    
                                            if (R_ReadMipiReg == Convert.ToInt32(mfgLotID))
                                           
                                            {
                                                MFG_ID_ReadError = 0;
                                            }
                                            else
                                            {
                                                MFG_ID_ReadError = 1;
                                            }
                                        }
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_DELTA", "NA", MFG_ID_ReadError);
                                    }
                                    break;
                            }

                            //Build Test Result
                            if (!b_GE_Header)
                            {
                                _Test_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                if (_Search_Method.ToUpper() == "UNIT_2DID")
                                {
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_STATUS", "NA", R_MIPI);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_PARTSN2", "dec", R_partSN2_2DID);
                                    ResultBuilder.BuildResults(ref results, _TestParaName, "dec", R_ReadMipiReg_long);
                                }
                                else
                                {
                                    //ResultBuilder.BuildResults(ref results, _TestParaName + "_STATUS", "NA", R_MIPI);
                                    ResultBuilder.BuildResults(ref results, _TestParaName, "dec", R_ReadMipiReg);

                                    if (_TestParaName.Contains("NFR-PASS-FLAG"))
                                    {
                                        if (!ResultBuilder.CheckPass(_TestParaName, R_ReadMipiReg))
                                        {
                                            ResultBuilder.FailedTests[0].Add(_TestParaName);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                _Test_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 

                                if (_Search_Method.ToUpper() == "UNIT_2DID")
                                {
                                    string tmp_custom_header_param = null;
                                    string GE_TestParam = null;
                                    Rslt_GE_Header = new s_GE_Header();

                                    Decode_GE_Header(TestPara, out Rslt_GE_Header);
                                    tmp_custom_header_param = Rslt_GE_Header.Param;

                                    Rslt_GE_Header.Param = null;
                                    Rslt_GE_Header.Param = "_MIPI";
                                    Rslt_GE_Header.Note = null;
                                    Rslt_GE_Header.Note = "_NOTE_PARTSN2";  //re-assign ge header
                                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, _Test_SMU);
                                    ResultBuilder.BuildResults(ref results, GE_TestParam, "dec", R_partSN2_2DID);

                                    if (Rslt_GE_Header.b_Header)
                                    {
                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, _Test_SMU);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "dec", R_ReadMipiReg_long);
                                    }
                                    else
                                    {
                                        //set to false - no required to display the GE Header Format . Use Parameter Header define in TCF only . Cause RF1 & RF2 using custom header for CMOS X-Y, MFG_ID, MODULE_ID etc ..
                                        ResultBuilder.BuildResults(ref results, tmp_custom_header_param, "dec", R_ReadMipiReg_long);
                                    }

                                    Rslt_GE_Header.Param = null;
                                    Rslt_GE_Header.Param = "_MIPI";
                                    Rslt_GE_Header.Note = null;
                                    Rslt_GE_Header.Note = "_NOTE_2DID_OTP_STATUS";  //re-assign ge header
                                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, _Test_SMU);
                                    ResultBuilder.BuildResults(ref results, GE_TestParam, "NA", R_MIPI);
                                }
                                else if (_Search_Method.ToUpper() == "READ_2DID_FROM_OTHER_OTP_BIT")
                                {
                                    string GE_TestParam = null;
                                    Rslt_GE_Header = new s_GE_Header();
                                    Decode_GE_Header(TestPara, out Rslt_GE_Header);

                                    if (Rslt_GE_Header.b_Header)
                                    {
                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, _Test_SMU);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "dec", R_ReadMipiReg_long);
                                    }
                                    else
                                    {
                                        //set to false - no required to display the GE Header Format . Use Parameter Header define in TCF only . Cause RF1 & RF2 using custom header for CMOS X-Y, MFG_ID, MODULE_ID etc ..
                                        ResultBuilder.BuildResults(ref results, _TestParaName, "dec", R_ReadMipiReg_long);
                                    }
                                }
                                else if (_Search_Method.ToUpper() == "OTP_MODULE_ID_2DID_DELTA") //Ivan
                                {
                                    ResultBuilder.BuildResults(ref results, _TestParaName, "dec", R_MIPI_2DID_DELTA);
                                }
                                else
                                {
                                    string GE_TestParam = null;
                                    Rslt_GE_Header = new s_GE_Header();
                                    Decode_GE_Header(TestPara, out Rslt_GE_Header);

                                    if (Rslt_GE_Header.b_Header)
                                    {
                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, _Test_SMU);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "dec", R_ReadMipiReg);

                                        if (GE_TestParam.Contains("NFR-PASS-FLAG"))
                                        {
                                            if (!ResultBuilder.CheckPass(GE_TestParam, R_ReadMipiReg))
                                            {
                                                ResultBuilder.FailedTests[0].Add(GE_TestParam);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        //set to false - no required to display the GE Header Format . Use Parameter Header define in TCF only . Cause RF1 & RF2 using custom header for CMOS X-Y, MFG_ID, MODULE_ID etc ..
                                        ResultBuilder.BuildResults(ref results, _TestParaName, "dec", R_ReadMipiReg);

                                        if (_TestParaName.Contains("NFR-PASS-FLAG"))
                                        {
                                            if (!ResultBuilder.CheckPass(_TestParaName, R_ReadMipiReg))
                                            {
                                                ResultBuilder.FailedTests[0].Add(_TestParaName);
                                            }
                                        }
                                    }
                                }
                            }

                            //Eq.Site[0]._EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            break;
                        #endregion

                        #region BURN OTP REGISTER with customize bit selection
                        case "BURN_OTP_SELECTIVE_BIT":
                            //R_MIPI FLAG STATUS - REMARK
                            //R_MIPI = -1 > Register Not Blank , did not proceed to burn
                            //R_MIPI = 0 > Register Blank , proceed to burn , Read not same as write data
                            //R_MIPI = 1 > Register Blank , proceed to burn , Read same as write data

                            #region Set SMU
                            //pass to global variable to be use outside this function
                            EqmtStatus.SMU_CH = _SMUSetCh;
                            //to select which channel to set and measure - Format in TCF(DCSet_Channel) 1,4 -> means CH1 & CH4 to set/measure
                            SetSMU = _SMUSetCh.Split(',');

                            SetSMUSelect = new string[SetSMU.Count()];
                            for (int i = 0; i < SetSMU.Count(); i++)
                            {
                                int smuVChannel = Convert.ToInt16(SetSMU[i]);
                                SetSMUSelect[i] = Eq.Site[0]._SMUSetting[smuVChannel];       //rearrange the Eq.Site[0]._SMUSetting base on reqquired channel only from total of 8 channel available  
                                Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[smuVChannel], Eq.Site[0]._EqSMU, _SMUVCh[smuVChannel], _SMUILimitCh[smuVChannel]);

                                //Store the SMU Channel Label - to be reuse later during OTP Burn process
                                string tempLabel = "SMUI_CH" + SetSMU[i];
                                foreach (string key in DicTestLabel.Keys)
                                {
                                    if (key == tempLabel)
                                    {
                                        R_SMULabel_ICh[smuVChannel] = DicTestLabel[key].ToString().ToUpper();
                                        break;
                                    }
                                }
                            }

                            Eq.Site[0]._Eq_SMUDriver.DcOn(SetSMUSelect, Eq.Site[0]._EqSMU);
                            #endregion

                            #region Initialize variable to default
                            //Initialize to default
                            b_lockBit = true;
                            i_lockBit = -999;
                            i_testFlag = -999;
                            b_testFlag = true;
                            BurnOTP = false;
                            dataDec_Conv = -999;
                            dataSizeHex = null;

                            R_ReadMipiReg = -999;   //set to fail value (default)
                            R_MIPI = -999;          //set to fail value (default)
                            tmpOutData_DecConv = -999;

                            biasDataArr = null;
                            dataHex = null;
                            int sumVal = -1; //Default to 0
                            #endregion

                            #region Read Register and return Data - derive from Mipi custom spreadsheet
                            //Search and return Data from Mipi custom spreadsheet 
                            searchMIPIKey(_TestParam, _SwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                            biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter
                            dataHex = new string[biasDataArr.Length];

                            if (dataHex.Length < 6)
                            {
                                readout_OTPReg_viaEffectiveBit(_RdCurr_Delay, _SwBand, CusMipiRegMap, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out tmpOutData_DecConv, out dataSizeHex);
                            }
                            #endregion

                            switch (_Search_Method.ToUpper())
                            {
                                case "MFG_ID":
                                case "MFGID":
                                    #region Burn Manufacturing ID
                                    dataDec_Conv = Convert.ToInt32(mfgLotID);         //convert string to int
                                    if (dataDec_Conv < (Convert.ToInt32(dataSizeHex, 16)))
                                    {
                                        //MSB - dataHex[0] , LSB - dataHex[1]
                                        Sort_MSBnLSB(dataDec_Conv, out dataHex[0], out dataHex[1]);
                                        BurnOTP = true;         //set flag to true for burning otp
                                    }

                                    //read lock bit register and compare Lockbit register
                                    readout_OTPReg_viaEffectiveBit(_RdCurr_Delay, _SwBand, _Search_Value, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out i_lockBit, out dummyStrData);

                                    if ((i_lockBit != 0) || (tmpOutData_DecConv != 0))    // '0' not program , '1' have program
                                    {
                                        R_MIPI = -1;
                                        R_ReadMipiReg = tmpOutData_DecConv;
                                        break;
                                    }
                                    else
                                    {
                                        //burn OTP register
                                        if ((BurnOTP) && (i_lockBit == 0) && (tmpOutData_DecConv == 0))
                                        {
                                            //Set VBATT only to prepare for otp burn procedure
                                            for (int i = 0; i < SetSMU.Count(); i++)
                                            {
                                                bool found = R_SMULabel_ICh[i].Substring(0, R_SMULabel_ICh[i].Length).Contains("BAT");
                                                if (found)
                                                {
                                                    Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[i], Eq.Site[0]._EqSMU, (float)5.5, (float)0.1);
                                                }
                                            }

                                            //burn_OTPReg_viaEffectiveBit(_TestParam, CusMipiRegMap, CusMipiPair, CusSlaveAddr, dataHex);
                                            burn_AceOTPReg_viaEffectiveBit(_TestParam, CusMipiRegMap, CusMipiPair, CusSlaveAddr, dataHex);

                                            #region Read Back MIPI register

                                            #region Turn off SMU and VIO - to prepare for read back mipi register
                                            if (EqmtStatus.SMU)
                                            {
                                                Eq.Site[0]._Eq_SMUDriver.DcOff(Eq.Site[0]._SMUSetting, Eq.Site[0]._EqSMU);
                                            }

                                            Eq.Site[0]._EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                                            DelayMs(_RdCurr_Delay);
                                            #endregion

                                            //read back register and compare with program data
                                            readout_OTPReg_viaEffectiveBit(_RdCurr_Delay, _SwBand, CusMipiRegMap, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out R_ReadMipiReg, out dummyStrData);

                                            if (R_ReadMipiReg == Convert.ToInt32(mfgLotID))
                                            {
                                                R_MIPI = 1;
                                            }
                                            else
                                            {
                                                R_MIPI = 2;
                                            }

                                            #endregion
                                        }
                                    }
                                    #endregion
                                    break;

                                case "UNIT_ID":
                                case "UNITID":
                                    #region Burn Module ID
                                    //Set the DUT SN ID and Check if file exist , if not exist -> create and write default SN
                                    dataDec_Conv = GetNextModuleID(Convert.ToInt32(dataSizeHex, 16), out b_testFlag);

                                    if ((dataDec_Conv <= (Convert.ToInt32(dataSizeHex, 16))) && (b_testFlag))
                                    {
                                        //MSB - dataHex[0] , LSB - dataHex[1]
                                        Sort_MSBnLSB(dataDec_Conv, out dataHex[0], out dataHex[1]);
                                        BurnOTP = true;         //set flag to true for burning otp
                                    }

                                    //compare Lockbit register
                                    readout_OTPReg_viaEffectiveBit(_RdCurr_Delay, _SwBand, _Search_Value, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out i_lockBit, out dummyStrData);

                                    if ((i_lockBit != 0) || (tmpOutData_DecConv != 0))    // '0' not program , '1' have program
                                    {
                                        R_MIPI = -1;
                                        R_ReadMipiReg = tmpOutData_DecConv;
                                        break;
                                    }
                                    else
                                    {
                                        //burn OTP register
                                        if ((BurnOTP) && (i_lockBit == 0) && (tmpOutData_DecConv == 0))
                                        {
                                            //Set VBATT only to prepare for otp burn procedure
                                            for (int i = 0; i < SetSMU.Count(); i++)
                                            {
                                                bool found = R_SMULabel_ICh[i].Substring(0, R_SMULabel_ICh[i].Length).Contains("BAT");
                                                if (found)
                                                {
                                                    Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[i], Eq.Site[0]._EqSMU, (float)5.5, (float)0.1);
                                                }
                                            }

                                            //burn_OTPReg_viaEffectiveBit(_TestParam, CusMipiRegMap, CusMipiPair, CusSlaveAddr, dataHex);
                                            burn_AceOTPReg_viaEffectiveBit(_TestParam, CusMipiRegMap, CusMipiPair, CusSlaveAddr, dataHex);

                                            #region Read Back MIPI register

                                            #region Turn off SMU and VIO - to prepare for read back mipi register
                                            if (EqmtStatus.SMU)
                                            {
                                                Eq.Site[0]._Eq_SMUDriver.DcOff(Eq.Site[0]._SMUSetting, Eq.Site[0]._EqSMU);
                                            }

                                            Eq.Site[0]._EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                                            DelayMs(_RdCurr_Delay);

                                            #endregion

                                            //read back register and compare with program data
                                            readout_OTPReg_viaEffectiveBit(_RdCurr_Delay, _SwBand, CusMipiRegMap, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out R_ReadMipiReg, out dummyStrData);

                                            if (R_ReadMipiReg == dataDec_Conv)
                                            {
                                                R_MIPI = 1;
                                            }
                                            else
                                            {
                                                R_MIPI = 2;
                                            }

                                            #endregion
                                        }
                                    }
                                    #endregion
                                    break;

                                case "TEST_FLAG":
                                case "TESTFLAG":
                                    #region Burn Test Flag
                                    if (ResultBuilder.FailedTests[0].Count != 0)
                                    {

                                        //if (ResultBuilder.FailedTests[0].Count < 300)
                                        //{
                                        //    for (int i = 0; i < ResultBuilder.FailedTests[0].Count; i++)
                                        //    {
                                        //        ATFLogControl.Instance.Log(LogLevel.Error, LogSource.eHandler, ResultBuilder.FailedTests[0][i].ToString());
                                        //    }
                                        //}
                                        BurnOTP = false;
                                        R_MIPI = -1;
                                        R_ReadMipiReg = -999;
                                        break;
                                    }
                                    else
                                    {
                                        //if all pass spec
                                        BurnOTP = true;

                                        //read lock bit register and compare Lockbit register
                                        readout_OTPReg_viaEffectiveBit(_RdCurr_Delay, _SwBand, _Search_Value, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out i_lockBit, out dummyStrData);

                                        if ((i_lockBit != 0) || (tmpOutData_DecConv != 0))    // '0' not program , '1' have program
                                        {
                                            R_MIPI = -1;
                                            R_ReadMipiReg = tmpOutData_DecConv;
                                            break;
                                        }
                                        else
                                        {
                                            //burn OTP register
                                            if ((BurnOTP) && (i_lockBit == 0) && (tmpOutData_DecConv == 0))
                                            {
                                                //Set VBATT only to prepare for otp burn procedure
                                                for (int i = 0; i < SetSMU.Count(); i++)
                                                {
                                                    bool found = R_SMULabel_ICh[i].Substring(0, R_SMULabel_ICh[i].Length).Contains("BAT");
                                                    if (found)
                                                    {
                                                        Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[i], Eq.Site[0]._EqSMU, (float)5.5, (float)0.1);
                                                    }
                                                }

                                                //Get the program value from Mipi custom spreadsheet 
                                                //example : E3:80 -> E3:1000 0000 (in Binary) - will program bit7 
                                                //regMapValue[0] = E3 , regMapValue[1] = 80
                                                //dataHex will be stored with 80
                                                //split string with blank space as delimiter
                                                biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter
                                                for (int i = 0; i < biasDataArr.Length; i++)
                                                {
                                                    string[] regMapValue = biasDataArr[i].Split(':');
                                                    dataHex[i] = regMapValue[1];
                                                }

                                                //burn_OTPReg_viaEffectiveBit(_TestParam, CusMipiRegMap, CusMipiPair, CusSlaveAddr, dataHex);
                                                burn_AceOTPReg_viaEffectiveBit(_TestParam, CusMipiRegMap, CusMipiPair, CusSlaveAddr, dataHex);

                                                #region Read Back MIPI register
                                                #region Turn off SMU and VIO - to prepare for read back mipi register
                                                if (EqmtStatus.SMU)
                                                {
                                                    Eq.Site[0]._Eq_SMUDriver.DcOff(Eq.Site[0]._SMUSetting, Eq.Site[0]._EqSMU);
                                                }

                                                Eq.Site[0]._EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                                                DelayMs(_RdCurr_Delay);
                                                #endregion

                                                //read back register and compare with program data
                                                mask_viaEffectiveBit(dataHex, _SwBand, CusMipiRegMap, out dataDec_Conv);
                                                readout_OTPReg_viaEffectiveBit(_RdCurr_Delay, _SwBand, CusMipiRegMap, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out R_ReadMipiReg, out dummyStrData);
                                                if (R_ReadMipiReg == dataDec_Conv)
                                                {
                                                    R_MIPI = 1;
                                                }
                                                else
                                                {
                                                    R_MIPI = 2;
                                                }

                                                #endregion
                                            }
                                        }
                                    }

                                    #endregion
                                    break;
                                case "RF1-OUTLIER-FLAG":
                                    // Ivan - Dpat
                                    #region Burn RF1 OUTLIER Test Flag
                                    //int sumVal = -1; //Default to 0
                                    if (DpatEnable)
                                    {
                                        //Avago.ATF.Outlier.LotOutlierRecord.Instance.CheckDUTOutlier(ResultBuilder.M_MFG_ID, ResultBuilder.M_OTP_MODULE_ID_2DID, out sumVal);
                                        Avago.ATF.Outlier.LotOutlierRecord.Instance.CheckDUTOutlier(ResultBuilder.M_MFG_ID, ResultBuilder.M_OTP_MODULE_ID_2DID_SYSTEM, out sumVal);
                                        //MessageBox.Show(sumVal.ToString());
                                        if (sumVal <= 0)
                                        {
                                            BurnOTP = false;
                                            R_MIPI = -1;
                                            R_ReadMipiReg = tmpOutData_DecConv;
                                            break;
                                        }
                                        else
                                        {
                                            BurnOTP = true;

                                            //read lock bit register and compare Lockbit register
                                            readout_OTPReg_viaEffectiveBit(_RdCurr_Delay, _SwBand, _Search_Value, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out i_lockBit, out dummyStrData);

                                            if (tmpOutData_DecConv != 0)    // '0' not program , '1' have program
                                            {
                                                R_MIPI = -1;
                                                R_ReadMipiReg = tmpOutData_DecConv;
                                                break;
                                            }
                                            else
                                            {
                                                //burn OTP register
                                                if ((BurnOTP) && (sumVal > 0))
                                                {
                                                    //Set VBATT only to prepare for otp burn procedure
                                                    for (int i = 0; i < SetSMU.Count(); i++)
                                                    {
                                                        bool found = R_SMULabel_ICh[i].Substring(0, R_SMULabel_ICh[i].Length).Contains("BAT");
                                                        if (found)
                                                        {
                                                            Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[i], Eq.Site[0]._EqSMU, (float)5.5, (float)0.1);
                                                        }
                                                    }

                                                    //Get the program value from Mipi custom spreadsheet 
                                                    //example : E3:80 -> E3:1000 0000 (in Binary) - will program bit7 
                                                    //regMapValue[0] = E3 , regMapValue[1] = 80
                                                    //dataHex will be stored with 80
                                                    //split string with blank space as delimiter
                                                    biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter
                                                    for (int i = 0; i < biasDataArr.Length; i++)
                                                    {
                                                        string[] regMapValue = biasDataArr[i].Split(':');
                                                        dataHex[i] = regMapValue[1];
                                                    }

                                                    //burn_OTPReg_viaEffectiveBit(_TestParam, CusMipiRegMap, CusMipiPair, CusSlaveAddr, dataHex);
                                                    burn_AceOTPReg_viaEffectiveBit(_TestParam, CusMipiRegMap, CusMipiPair, CusSlaveAddr, dataHex);

                                                    #region Read Back MIPI register
                                                    #region Turn off SMU and VIO - to prepare for read back mipi register
                                                    if (EqmtStatus.SMU)
                                                    {
                                                        Eq.Site[0]._Eq_SMUDriver.DcOff(Eq.Site[0]._SMUSetting, Eq.Site[0]._EqSMU);
                                                    }

                                                    Eq.Site[0]._EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                                                    DelayMs(_RdCurr_Delay);
                                                    #endregion

                                                    //read back register and compare with program data
                                                    mask_viaEffectiveBit(dataHex, _SwBand, CusMipiRegMap, out dataDec_Conv);
                                                    readout_OTPReg_viaEffectiveBit(_RdCurr_Delay, _SwBand, CusMipiRegMap, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out R_ReadMipiReg, out dummyStrData);
                                                    if (R_ReadMipiReg == dataDec_Conv)
                                                    {
                                                        R_MIPI = 1;
                                                    }
                                                    else
                                                    {
                                                        R_MIPI = -1;
                                                    }

                                                    #endregion
                                                }
                                            }
                                        }
                                    }

                                    #endregion
                                    break;



                                case "CM_ID":
                                case "CMID":
                                    #region Burn CM ID
                                    //Check CM base on Device ID scanning
                                    string[] tmpDeviceID = new string[3];
                                    try
                                    {
                                        tmpDeviceID = deviceID.Split('-');
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception("DEVICE ID FORMAT INCORRECT (ENGR-XXXX-Y) : " + deviceID + " -> " + ex.Message);
                                    }

                                    switch (tmpDeviceID[2].ToUpper())
                                    {
                                        case "M":
                                            //Amkor Assembly
                                            R_MIPI = 1;
                                            R_ReadMipiReg = tmpOutData_DecConv;
                                            BurnOTP = false;        //Amkor CM ID = 0 , thus not required to Burn
                                            break;
                                        case "A":
                                            //AseKr Assembly
                                            R_MIPI = 1;
                                            BurnOTP = true;         //ASEKr CM ID = 1 , thus Otp Burn is required
                                            break;
                                        default:
                                            MessageBox.Show("CM Site : " + tmpDeviceID[2].ToUpper() + " (CM SITE not supported at this moment)", "MyDUT", MessageBoxButtons.OK);
                                            R_MIPI = -999;
                                            BurnOTP = false;        //set flag to false for burning otp
                                            break;
                                    }

                                    #region Check Lockbit and CM ID register and Burn Register
                                    //read lock bit register and compare Lockbit register
                                    readout_OTPReg_viaEffectiveBit(_RdCurr_Delay, _SwBand, _Search_Value, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out i_lockBit, out dummyStrData);

                                    if ((i_lockBit != 0) || (tmpOutData_DecConv != 0))    // '0' not program , '1' have program
                                    {
                                        R_MIPI = -1;
                                        R_ReadMipiReg = tmpOutData_DecConv;
                                        break;
                                    }
                                    else
                                    {
                                        //burn OTP register
                                        if ((BurnOTP) && (i_lockBit == 0) && (tmpOutData_DecConv == 0))
                                        {
                                            //Set VBATT only to prepare for otp burn procedure
                                            for (int i = 0; i < SetSMU.Count(); i++)
                                            {
                                                bool found = R_SMULabel_ICh[i].Substring(0, R_SMULabel_ICh[i].Length).Contains("BAT");
                                                if (found)
                                                {
                                                    Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[i], Eq.Site[0]._EqSMU, (float)5.5, (float)0.1);
                                                }
                                            }

                                            //Get the program value from Mipi custom spreadsheet 
                                            //example : E3:80 -> E3:1000 0000 (in Binary) - will program bit7 
                                            //regMapValue[0] = E3 , regMapValue[1] = 80
                                            //dataHex will be stored with 80
                                            //split string with blank space as delimiter
                                            biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter
                                            for (int i = 0; i < biasDataArr.Length; i++)
                                            {
                                                string[] regMapValue = biasDataArr[i].Split(':');
                                                dataHex[i] = regMapValue[1];
                                            }

                                            burn_OTPReg_viaEffectiveBit(_TestParam, CusMipiRegMap, CusMipiPair, CusSlaveAddr, dataHex);
                                            burn_AceOTPReg_viaEffectiveBit(_TestParam, CusMipiRegMap, CusMipiPair, CusSlaveAddr, dataHex);

                                            #region Read Back MIPI register

                                            #region Turn off SMU and VIO - to prepare for read back mipi register
                                            if (EqmtStatus.SMU)
                                            {
                                                Eq.Site[0]._Eq_SMUDriver.DcOff(Eq.Site[0]._SMUSetting, Eq.Site[0]._EqSMU);
                                            }

                                            Eq.Site[0]._EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                                            DelayMs(_RdCurr_Delay);
                                            #endregion

                                            //read back register and compare with program data
                                            mask_viaEffectiveBit(dataHex, _SwBand, CusMipiRegMap, out dataDec_Conv);
                                            readout_OTPReg_viaEffectiveBit(_RdCurr_Delay, _SwBand, CusMipiRegMap, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out R_ReadMipiReg, out dummyStrData);

                                            if ((R_ReadMipiReg == dataDec_Conv) && (BurnOTP))
                                            {
                                                //Good - Return Flag '1'
                                                R_MIPI = 1;
                                            }
                                            else if ((R_ReadMipiReg == 1) && (!BurnOTP))
                                            {
                                                //Bad - Return Flag '-999'
                                                R_MIPI = 2;
                                            }

                                            #endregion
                                        }
                                    }
                                    #endregion

                                    #endregion
                                    break;

                                case "UNIT_2DID":
                                    #region Burn 2DID data

                                    //note - Status Flag (R_MIPI) 
                                    //2DID Readback failure - 3
                                    //2DID info duplicate - 5
                                    //Lockbit/2DID register not empty - 7
                                    //2DID register readback not same as PartSN_2 - 9
                                    //Pass ALL - 0                               

                                    //2DID local variable
                                    string str2DIDmark = null;
                                    string strPartSN2 = null;

                                    //Initialize variable
                                    bool b_reg2DID = false;     //bool for read DUT register
                                    bool b_str2DID = false;     //bool for read Handler 2DID
                                    bool success = false;
                                    char[] sort2DID;
                                    int regCount = 0;

                                    dataDec_Conv = 0;
                                    BurnOTP = false;

                                    //reset to default value
                                    R_MIPI = -999;
                                    R_ReadMipiReg_long = -999;
                                    R_partSN2_2DID = -999;
                                    R_partSN2_preReg = -999;
                                    R_partSN2_postReg = -999;

                                    string[] raw_OutdataHex;
                                    string raw_OutdataDec = null;
                                    string[] data2DID;

                                    #region Read Register and return Data - derive from Mipi custom spreadsheet
                                    //Search and return Data from Mipi custom spreadsheet 
                                    searchMIPIKey(_TestParam, _SwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                                    biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter
                                    dataHex = new string[biasDataArr.Length];
                                    raw_OutdataHex = new string[biasDataArr.Length];
                                    data2DID = new string[biasDataArr.Length];

                                    readout_OTPReg_viaEffectiveBit(_RdCurr_Delay, _SwBand, CusMipiRegMap, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out raw_OutdataHex, out dataSizeHex);

                                    //check if blank register
                                    appendDec = null;
                                    b_reg2DID = true;   //set to default

                                    for (int i = 0; i < raw_OutdataHex.Length; i++)
                                    {
                                        dataDec_Conv = int.Parse(raw_OutdataHex[i], System.Globalization.NumberStyles.HexNumber);

                                        if (dataDec_Conv <= 99)       //Note 2DID register max value until dec 99 only
                                        {
                                            if (dataDec_Conv <= 9)
                                            {
                                                raw_OutdataDec = "0" + dataDec_Conv;
                                            }
                                            else
                                            {
                                                raw_OutdataDec = dataDec_Conv.ToString();
                                            }
                                        }
                                        else
                                        {
                                            R_MIPI = 2; //register readback fail flag
                                            b_reg2DID = false;
                                        }

                                        appendDec = appendDec + raw_OutdataDec;
                                    }

                                    success = long.TryParse(appendDec, out R_partSN2_preReg);  //check if return data from register must not exceed '99' for every register address
                                    if (!success)
                                    {
                                        b_reg2DID = false;
                                        R_MIPI = 2; //register readback fail flag
                                        R_partSN2_preReg = 999999999999;
                                    }
                                    #endregion

                                    if (EqmtStatus.handler)
                                    {
                                        #region 2DID Handler TCP
                                        HandlerLotInfo hli = new HandlerLotInfo();
                                        hli = TcpClient.LastTestedInTheLotQuery();
                                        #endregion

                                        #region decode 2DID
                                        //default setting
                                        //Total EFuse Register = 6
                                        //Total data = 24 numerics , Effective numeric data from 13 to 24 , numeric data  1 to 12 not use
                                        //Example str2DIDmark = "193713219999999999100500";

                                        str2DIDmark = string.Format("{0:D24}", hli.strBarcodeID);

                                        if (str2DIDmark != null && str2DIDmark.Length == 24)
                                        {
                                            //for double unit marking detection
                                            if (str2DIDmark != Previous2DIDmark)
                                            {
                                                sort2DID = new char[str2DIDmark.Length];
                                                Previous2DIDmark = str2DIDmark;
                                            }
                                            else
                                            {
                                                //flag for double unit marking
                                                str2DIDmark = "-1";
                                                R_partSN2_2DID = -1;
                                                R_MIPI = 5; //double unit fail flag
                                            }
                                        }
                                        else
                                        {
                                            //if cannot read from handler
                                            str2DIDmark = "-999";
                                            R_partSN2_2DID = -99999;
                                            R_MIPI = 3; //readback fail flag
                                        }

                                        if (str2DIDmark != "-999" && str2DIDmark != "0" && str2DIDmark != "-1")
                                        {
                                            b_str2DID = true;     //set to default                                            
                                            count = 0;
                                            sort2DID = str2DIDmark.ToCharArray();

                                            for (int i = 12; i < str2DIDmark.Length; i++)
                                            {
                                                count++;
                                                if (count == 2) //do checking for every 2 number
                                                {
                                                    if (regCount < dataHex.Length)
                                                    {
                                                        //take 2 numerical data to form 1x Hex Data
                                                        //eg sort2DID[12] = '5' & eg sort2DID[13] = '0'
                                                        //Then if the appended sort2DID[12] & sort2DID[13] is a valid number (note : max number is 99)
                                                        string tmpDataDec = sort2DID[i - 1].ToString() + sort2DID[i].ToString();

                                                        success = Int32.TryParse(tmpDataDec, out dataDec_Conv);       //check if 2DID marking data is valid -  ie valid number(can convert from Hex to Integer back and forth & must be less than '99') 

                                                        if (!success)
                                                        {
                                                            MessageBox.Show("Test Parameter : " + _TestParam + " - String to Decimal Conversion Unsuccessful >> Input String With Wrong Format : " + tmpDataDec, "MyDUT", MessageBoxButtons.OK);
                                                            dataDec_Conv = 0;       //set to default '0' if fail conversion
                                                            b_str2DID = false;
                                                            R_MIPI = 3; //handler 2DID readback fail flag
                                                        }

                                                        //store scan 2DID value for later comparison
                                                        if (dataDec_Conv <= 9)
                                                        {
                                                            data2DID[regCount] = "0" + dataDec_Conv.ToString();
                                                        }
                                                        else
                                                        {
                                                            data2DID[regCount] = dataDec_Conv.ToString();
                                                        }

                                                        dataHex[regCount] = dataDec_Conv.ToString("X2");        //convert dec to hex and pass data for burn otp process
                                                        count = 0;
                                                        regCount++;
                                                    }
                                                }
                                            }

                                            //re-arrange only partSN2_2DID
                                            for (int i = 0; i < data2DID.Length; i++)
                                            {
                                                strPartSN2 = strPartSN2 + data2DID[i];
                                            }

                                            R_partSN2_2DID = Convert.ToInt64(strPartSN2);
                                        }
                                        #endregion
                                    }
                                    else
                                    {
                                        //Handler not enable
                                        b_str2DID = false;
                                        R_MIPI = 3;
                                        R_ReadMipiReg_long = -99999;
                                        R_partSN2_2DID = -99999;
                                        R_partSN2_postReg = -99999;
                                    }

                                    //change OTP Burn flag to true if Read DUT Register and Read Handler 2DID is success
                                    if (b_str2DID && b_reg2DID)
                                    {
                                        BurnOTP = true;
                                    }

                                    if (BurnOTP)
                                    {
                                        #region Burn OTP
                                        //read lock bit register and compare Lockbit register
                                        readout_OTPReg_viaEffectiveBit(_RdCurr_Delay, _SwBand, _Search_Value, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out i_lockBit, out dummyStrData);

                                        if ((i_lockBit != 0) || (R_partSN2_preReg != 0) || !BurnOTP)    // '0' not program , '1' have program
                                        {
                                            if (R_partSN2_preReg == R_partSN2_2DID)
                                            {
                                                R_MIPI = 0;     // 2DID Pre register match with 2DID PartSN2
                                                R_partSN2_postReg = 0;       //make post register readout to 0 as per PE request
                                                break;
                                            }
                                            else
                                            {
                                                R_MIPI = 7;     //lockbit/2DID register not empty flag  
                                                R_partSN2_postReg = 0;       //make post register readout to 0 as per PE request
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            //burn OTP register
                                            //Set VBATT only to prepare for otp burn procedure
                                            for (int i = 0; i < SetSMU.Count(); i++)
                                            {
                                                bool found = R_SMULabel_ICh[i].Substring(0, R_SMULabel_ICh[i].Length).Contains("BAT");
                                                if (found)
                                                {
                                                    Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[i], Eq.Site[0]._EqSMU, (float)5.5, (float)0.1);
                                                }
                                            }

                                            burn_AceOTPReg_viaEffectiveBit(_TestParam, CusMipiRegMap, CusMipiPair, CusSlaveAddr, dataHex);

                                            #region Read Back MIPI register

                                            #region Turn off SMU and VIO - to prepare for read back mipi register
                                            if (EqmtStatus.SMU)
                                            {
                                                Eq.Site[0]._Eq_SMUDriver.DcOff(Eq.Site[0]._SMUSetting, Eq.Site[0]._EqSMU);
                                            }
                                            Eq.Site[0]._EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                                            DelayMs(_RdCurr_Delay);
                                            #endregion

                                            //read back register and compare with program data
                                            b_reg2DID = true;       //set to default
                                            appendDec = null;
                                            raw_OutdataHex = new string[biasDataArr.Length];

                                            readout_OTPReg_viaEffectiveBit(_RdCurr_Delay, _SwBand, CusMipiRegMap, CusPMTrigMap, CusSlaveAddr, CusMipiPair, CusMipiSite, out raw_OutdataHex, out dummyStrData);

                                            for (int i = 0; i < raw_OutdataHex.Length; i++)
                                            {
                                                //check if 2DID reg data is valid - ie less or equal '99'
                                                dataDec_Conv = int.Parse(raw_OutdataHex[i], System.Globalization.NumberStyles.HexNumber);
                                                if (dataDec_Conv > 99)
                                                {
                                                    b_reg2DID = false;
                                                    R_MIPI = 2;
                                                    appendDec = "-999999999999";
                                                    break;
                                                }
                                                else
                                                {
                                                    if (dataDec_Conv <= 9)
                                                    {
                                                        raw_OutdataDec = "0" + dataDec_Conv;
                                                    }
                                                    else
                                                    {
                                                        raw_OutdataDec = dataDec_Conv.ToString();
                                                    }

                                                    appendDec = appendDec + raw_OutdataDec;

                                                    if (data2DID[i] != raw_OutdataDec)
                                                    {
                                                        R_MIPI = 9;     //Burn OTP not same as PartSN_2 flag
                                                        b_reg2DID = false;
                                                    }
                                                }
                                            }

                                            R_partSN2_postReg = Convert.ToInt64(appendDec);

                                            if (b_reg2DID)
                                            {
                                                R_MIPI = 0;     //all pass flag                                                
                                            }

                                            #endregion
                                        }
                                        #endregion
                                    }

                                    #endregion
                                    break;

                                default:
                                    MessageBox.Show("Test Parameter : " + _TestParam + "(" + _Search_Method + ") - Search Method not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    break;
                            }

                            //Build Test Result
                            if (_TestParaName.Contains("RF1-OUTLIER-FLAG"))
                            {
                                ResultBuilder.BuildResults(ref results, "OUTLIER_SUMVAL", "NA", sumVal);
                            }

                            if (!b_GE_Header)
                            {
                                _Test_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 
                                if (_Search_Method.ToUpper() == "UNIT_2DID")
                                {
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_STATUS", "NA", R_MIPI);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_PARTSN2", "dec", R_partSN2_2DID);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_READ_OTP2DID_PRE", "dec", R_partSN2_preReg);
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_READ_OTP2DID_POST", "dec", R_partSN2_postReg);
                                }
                                else
                                {
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_STATUS", "NA", R_MIPI);
                                    ResultBuilder.BuildResults(ref results, _TestParaName, "dec", R_ReadMipiReg);

                                    if (_TestParaName.Contains("PASSFLAG"))
                                    {
                                        if (!ResultBuilder.CheckPass(_TestParaName, R_ReadMipiReg))
                                        {
                                            ResultBuilder.FailedTests[0].Add(_TestParaName);
                                        }
                                        if (!ResultBuilder.CheckPass(_TestParaName + "_STATUS", R_MIPI))
                                        {
                                            ResultBuilder.FailedTests[0].Add(_TestParaName + "_STATUS");
                                        }
                                    }
                                }

                            }
                            else
                            {
                                _Test_MIPI = false;         //ensure that the MIPI flag in this case is set to false to avoid duplicate result at the end 

                                if (_Search_Method.ToUpper() == "UNIT_2DID")
                                {
                                    string GE_TestParam = null;
                                    Rslt_GE_Header = new s_GE_Header();
                                    Decode_GE_Header(TestPara, out Rslt_GE_Header);

                                    Rslt_GE_Header.Param = "_MIPI_ReadTxReg";

                                    Rslt_GE_Header.Note = "_NOTE_READ_OTP2DID_PRE";  //re-assign ge header
                                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, _Test_SMU);
                                    ResultBuilder.BuildResults(ref results, GE_TestParam, "dec", R_partSN2_preReg);

                                    Rslt_GE_Header.Note = "_NOTE_READ_OTP2DID_POST";  //re-assign ge header
                                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, _Test_SMU);
                                    ResultBuilder.BuildResults(ref results, GE_TestParam, "dec", R_partSN2_postReg);

                                    Rslt_GE_Header.Param = "_MIPI_OTPBURN";

                                    Rslt_GE_Header.Note = "_NOTE_OTP_2DID";  //re-assign ge header
                                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, _Test_SMU);
                                    ResultBuilder.BuildResults(ref results, GE_TestParam, "dec", R_partSN2_2DID);

                                    Rslt_GE_Header.Note = "_NOTE_2DID_OTP_STATUS";  //re-assign ge header
                                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, _Test_SMU);
                                    ResultBuilder.BuildResults(ref results, GE_TestParam, "NA", R_MIPI);
                                }
                                else
                                {
                                    string GE_TestParam = null;
                                    Rslt_GE_Header = new s_GE_Header();
                                    Decode_GE_Header(TestPara, out Rslt_GE_Header);

                                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, _Test_SMU);
                                    ResultBuilder.BuildResults(ref results, GE_TestParam, "dec", R_ReadMipiReg);

                                    if (GE_TestParam.Contains("PASSFLAG"))
                                    {
                                        if (!ResultBuilder.CheckPass(GE_TestParam, R_ReadMipiReg))
                                        {
                                            ResultBuilder.FailedTests[0].Add(GE_TestParam);
                                        }
                                    }

                                    Rslt_GE_Header.Note = Rslt_GE_Header.Note + "_STATUS";  //re-assign ge header
                                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, _Test_SMU);
                                    ResultBuilder.BuildResults(ref results, GE_TestParam, "NA", R_MIPI);

                                    if (GE_TestParam.Contains("PASSFLAG_STATUS"))
                                    {
                                        if (!ResultBuilder.CheckPass(GE_TestParam, R_MIPI))
                                        {
                                            ResultBuilder.FailedTests[0].Add(GE_TestParam);
                                        }
                                    }
                                }
                            }

                            Eq.Site[0]._EqMiPiCtrl.TurnOff_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet

                            tTime.Stop();
                            break;

                        default:
                            MessageBox.Show("Test Parameter : " + _TestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;

                            #endregion
                    }
                    break;

                case "MIPI_VEC":
                    switch (_TestParam.ToUpper())
                    {
                        #region BURN LNA OTP
                        case "OTP_BURN_LNA":
                            int tmpError = 0;
                            int vectorErrorcount = -99999;
                            int tmpVecNo = 1;
                            //Search and return Data from Mipi custom spreadsheet
                            searchMIPIKey(_TestParam, _SwBand, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                            tmpError = Eq.Site[0]._EqMiPiCtrl.ReadVector(Convert.ToInt16(CusMipiPair), ref vectorErrorcount, "TEST");
                            R_MIPI = Convert.ToDouble(vectorErrorcount);

                            tTime.Stop();
                            break;

                            #endregion
                    }
                    break;

                case "SWITCH":
                    switch (_TestParam.ToUpper())
                    {
                        #region SWITCH Setup
                        case "SETSWITCH":

                            SetswitchPath(DicCalInfo, DataFilePath.LocSettingPath, TCF_Header.ConstSwBand, _SwBand, _Setup_Delay);

                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            break;

                            #endregion SWITCH Setup
                    }
                    break;

                case "NF":
                    switch (_TestParam.ToUpper())
                    {
                        #region NF test function
                        case "NF_CA_NDIAG":
                            // This sweep is a faster sweep , it is a continuous sweep base on SG freq sweep mode
                            #region NF CA NDIAG

                            prevRslt = 0;
                            status = false;
                            pwrSearch = false;
                            Index = 0;
                            tx1_span = 0;
                            tx1_noPoints = 0;
                            rx1_span = 0;
                            rx1_cntrfreq = 0;
                            rx2_span = 0;
                            rx2_cntrfreq = 0;
                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(_PoutTolerance);
                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                            {
                                tolerancePwr = 0.5;
                            }

                            //use for searching previous result - to get the DUT LNA gain from previous result
                            if (Convert.ToInt16(_TestUsePrev) > 0)
                            {
                                usePrevRslt = true;
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                prevRslt = Math.Round(ReportRslt(_TestUsePrev, resultTag), 3);
                            }

                            //DelayMs(_StartSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_CALTAG", _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region PowerSensor Offset, MXG , MXA1 and MXA2 configuration

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((_StopTXFreq1 - _StartTXFreq1) / _StepTXFreq1);
                            _TXFreq = _StartTXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, _TXFreq, ref _LossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)_LossInputPathSG1;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _TXFreq, ref _LossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)_LossCouplerPath;
                                _TXFreq = _TXFreq + _StepTXFreq1;
                            }

                            tmpAveInputLoss = tmpInputLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            //change PowerSensor, MXG setting
                            Eq.Site[0]._EqPwrMeter.SetOffset(1, totalOutputLoss);
                            Eq.Site[0]._EqSG01.SetFreq(Convert.ToDouble(_SG1_DefaultFreq));

                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", _SwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);
                            rbwParamName = "_" + Math.Abs(myUtility.MXA_Setting.RBW / 1e6).ToString() + "MHz";

                            if (PreviousMXAMode != _SwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                tx1_span = _StopTXFreq1 - _StartTXFreq1;
                                tx1_noPoints = Convert.ToInt16(tx1_span / _StepTXFreq1);
                                Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.LIST);
                                Eq.Site[0]._EqSG01.SET_LIST_TYPE(LibEqmtDriver.SG.N5182_LIST_TYPE.STEP);
                                Eq.Site[0]._EqSG01.SET_LIST_MODE(LibEqmtDriver.SG.INSTR_MODE.AUTO);
                                Eq.Site[0]._EqSG01.SET_LIST_TRIG_SOURCE(LibEqmtDriver.SG.N5182_TRIG_TYPE.TIM);
                                Eq.Site[0]._EqSG01.SET_CONT_SWEEP(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);

                                Eq.Site[0]._EqSG01.SET_START_FREQUENCY(_StartTXFreq1 - (_StepTXFreq1 / 2));
                                Eq.Site[0]._EqSG01.SET_STOP_FREQUENCY(_StopTXFreq1 + (_StepTXFreq1 / 2));
                                Eq.Site[0]._EqSG01.SET_TRIG_TIMERPERIOD(_DwellT1);
                                Eq.Site[0]._EqSG01.SET_SWEEP_POINT(tx1_noPoints + 2);   //need to add additional 2 points to calculated no of points because of extra point of start_freq and stop_freq for MXG and MXA sync

                                SGTargetPin = _Pin1 - totalInputLoss;
                                Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                Eq.Site[0]._ModulationType = (LibEqmtDriver.SG.N5182A_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.SG.N5182A_WAVEFORM_MODE), _WaveFormName);
                                Eq.Site[0]._EqSG01.SELECT_WAVEFORM(Eq.Site[0]._ModulationType);
                                Eq.Site[0]._EqSG01.SET_ROUTE_CONN_TOUT(LibEqmtDriver.SG.N5182A_ROUTE_SUBSYS.SweepRun);

                                Eq.Site[0]._EqSG01.SINGLE_SWEEP();      //need to sweep SG for power search - RF ON in sweep mode
                                if (_SetFullMod)
                                {
                                    //This setting will set the modulation for N5182A to full modulation
                                    //Found out that when this set to default (RMS) , the modulation is mutated (CW + Mod) when running under sweep mode for NF
                                    Eq.Site[0]._EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.Mod);
                                }
                                else
                                {
                                    Eq.Site[0]._EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.RMS);
                                }
                                #endregion

                                #region MXA 1 setup
                                DelayMs(_Setup_Delay);
                                rx1_span = (_StopRXFreq1 - _StartRXFreq1);
                                rx1_cntrfreq = _StartRXFreq1 + (rx1_span / 2);
                                rx1_mxa_nopts = (int)((rx1_span / rx1_mxa_nopts_step) + 1);

                                Eq.Site[0]._EqSA01.Select_Instrument(LibEqmtDriver.SA.N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);
                                //Eq.Site[0]._EqSA01.AUTO_ATTENUATION(true); //Anthony
                                Eq.Site[0]._EqSA01.AUTO_ATTENUATION(false); //Anthony
                                if (Convert.ToDouble(_SA1att) != CurrentSaAttn) //Anthony
                                {
                                    Eq.Site[0]._EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToDouble(_SA1att));
                                    CurrentSaAttn = Convert.ToDouble(_SA1att);
                                }
                                Eq.Site[0]._EqSA01.TRIGGER_SINGLE();
                                Eq.Site[0]._EqSA01.TRACE_AVERAGE(1);
                                Eq.Site[0]._EqSA01.AVERAGE_OFF();

                                Eq.Site[0]._EqSA01.FREQ_CENT(rx1_cntrfreq.ToString(), "MHz");
                                Eq.Site[0]._EqSA01.SPAN(rx1_span);
                                Eq.Site[0]._EqSA01.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                Eq.Site[0]._EqSA01.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                Eq.Site[0]._EqSA01.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);

                                //Eq.Site[0]._EqSA01.SWEEP_POINTS(myUtility.MXA_Setting.NoPoints);
                                Eq.Site[0]._EqSA01.SWEEP_POINTS(rx1_mxa_nopts);

                                if (_SetRX1NDiag)
                                {
                                    Eq.Site[0]._EqSA01.CONTINUOUS_MEASUREMENT_ON();
                                    Eq.Site[0]._EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToInt16(_SA1att));
                                    Eq.Site[0]._EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    trigDelay = (decimal)_DwellT1 + (decimal)0.1;       //fixed 0.1ms delay
                                    Eq.Site[0]._EqSA01.SET_TRIG_DELAY(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1, trigDelay.ToString());
                                    Eq.Site[0]._EqSA01.SWEEP_TIMES(Convert.ToInt16(tx1_noPoints * _DwellT1));
                                }
                                else
                                {
                                    Eq.Site[0]._EqSA01.CONTINUOUS_MEASUREMENT_ON();
                                    Eq.Site[0]._EqSA01.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                    Eq.Site[0]._EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    Eq.Site[0]._EqSA01.SWEEP_TIMES(Convert.ToInt16(_RX1SweepT));
                                }

                                //Initialize & clear MXA trace
                                Eq.Site[0]._EqSA01.MARKER_TURN_ON_NORMAL_POINT(1, (float)rx1_cntrfreq);
                                Eq.Site[0]._EqSA01.CLEAR_WRITE();

                                status = Eq.Site[0]._EqSA01.OPERATION_COMPLETE();

                                #endregion

                                #region MXA 2 setup
                                DelayMs(_Setup_Delay);
                                rx2_span = (_StopRXFreq2 - _StartRXFreq2);
                                rx2_cntrfreq = _StartRXFreq2 + (rx2_span / 2);
                                rx2_mxa_nopts = (int)((rx2_span / rx2_mxa_nopts_step) + 1);

                                Eq.Site[0]._EqSA02.Select_Instrument(LibEqmtDriver.SA.N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);
                                //Eq.Site[0]._EqSA02.AUTO_ATTENUATION(true); //Anthony
                                Eq.Site[0]._EqSA02.AUTO_ATTENUATION(false);
                                if (Convert.ToDouble(_SA1att) != CurrentSa2Attn) //Anthony
                                {
                                    Eq.Site[0]._EqSA02.AMPLITUDE_INPUT_ATTENUATION(Convert.ToDouble(_SA2att));
                                    CurrentSaAttn = Convert.ToDouble(_SA2att);
                                }
                                Eq.Site[0]._EqSA02.TRIGGER_SINGLE();
                                Eq.Site[0]._EqSA02.TRACE_AVERAGE(1);
                                Eq.Site[0]._EqSA02.AVERAGE_OFF();

                                Eq.Site[0]._EqSA02.FREQ_CENT(rx2_cntrfreq.ToString(), "MHz");
                                Eq.Site[0]._EqSA02.SPAN(rx2_span);
                                Eq.Site[0]._EqSA02.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                Eq.Site[0]._EqSA02.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                Eq.Site[0]._EqSA02.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);

                                Eq.Site[0]._EqSA02.SWEEP_POINTS(rx2_mxa_nopts);
                                //Eq.Site[0]._EqSA02.SWEEP_POINTS(myUtility.MXA_Setting.NoPoints);

                                if (_SetRX2NDiag)
                                {
                                    Eq.Site[0]._EqSA02.CONTINUOUS_MEASUREMENT_ON();
                                    Eq.Site[0]._EqSA02.AMPLITUDE_INPUT_ATTENUATION(Convert.ToInt16(_SA1att));
                                    Eq.Site[0]._EqSA02.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    trigDelay = (decimal)_DwellT1 + (decimal)0.1;       //fixed 0.1ms delay
                                    Eq.Site[0]._EqSA02.SET_TRIG_DELAY(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1, trigDelay.ToString());
                                    Eq.Site[0]._EqSA02.SWEEP_TIMES(Convert.ToInt16(tx1_noPoints * _DwellT1));
                                }
                                else
                                {
                                    Eq.Site[0]._EqSA02.CONTINUOUS_MEASUREMENT_ON();
                                    Eq.Site[0]._EqSA02.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                    Eq.Site[0]._EqSA02.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    Eq.Site[0]._EqSA02.SWEEP_TIMES(Convert.ToInt16(_RX2SweepT));
                                }

                                //Initialize & clear MXA trace
                                Eq.Site[0]._EqSA02.MARKER_TURN_ON_NORMAL_POINT(1, (float)rx2_cntrfreq);
                                Eq.Site[0]._EqSA02.CLEAR_WRITE();

                                status = Eq.Site[0]._EqSA02.OPERATION_COMPLETE();

                                #endregion

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = _SwBand.ToUpper();
                            }
                            #endregion

                            #region measure contact power (Pout1)
                            if (StopOnFail.TestFail == false)
                            {
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.LIST);
                                if (!_TunePwr_TX1)
                                {
                                    StopOnFail.TestFail = true;     //init to fail state as default
                                    if (_Test_Pout1)
                                    {
                                        DelayMs(_RdPwr_Delay);
                                        R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = Math.Round(SGTargetPin + totalInputLoss, 3);
                                        if (Math.Abs(_Pout1 - R_Pout1) <= (tolerancePwr + 3.5))
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                        }
                                    }
                                    else
                                    {
                                        //No Pout measurement required, default set flag to pass
                                        pwrSearch = true;
                                        StopOnFail.TestFail = false;
                                    }
                                }
                                else
                                {
                                    do
                                    {
                                        StopOnFail.TestFail = true;     //init to fail state as default

                                        R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                        //R_Pin = TargetPin + (float)_LossInputPathSG1;
                                        R_Pin1 = SGTargetPin + totalInputLoss;

                                        if (Math.Abs(_Pout1 - R_Pout1) >= tolerancePwr)
                                        {
                                            if ((Index == 0) && (SGTargetPin < _SG1MaxPwr))   //preset to initial target power for 1st measurement count
                                            {
                                                Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                                R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                            }

                                            SGTargetPin = SGTargetPin + (_Pout1 - R_Pout1);

                                            if (SGTargetPin < _SG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                            {
                                                Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                                DelayMs(_RdPwr_Delay);
                                            }
                                        }
                                        else if (SGTargetPin > _SG1MaxPwr)      //if input sig gen exit limit , exit pwr search loop
                                        {
                                            SGTargetPin = _Pin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                            break;
                                        }
                                        else
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                            break;
                                        }
                                        Index++;
                                    }
                                    while (Index < 10);     // max power search loop
                                }
                            }

                            #endregion

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)_StartSync_Delay)
                            {
                                syncTest_Delay = (long)_StartSync_Delay - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            if (pwrSearch)
                            {
                                Eq.Site[0]._EqSG01.SINGLE_SWEEP();
                                status = Eq.Site[0]._EqSG01.OPERATION_COMPLETE();

                                //Need to turn off sweep mode - interference when running multisite because SG will go back to start freq once completed sweep
                                Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.CW);       //setting will set back to default freq define earlier

                                DelayMs(_Trig_Delay);
                                Capture_MXA1_Trace(1, _TestNum, _TestParaName, _RX1Band, prevRslt, _Save_MXATrace);
                                Read_MXA1_Trace(1, _TestNum, out R_NF1_Freq, out R_NF1_Ampl, _Search_Method, _TestParaName);
                                Capture_MXA2_Trace(1, _TestNum, _TestParaName, _RX2Band, prevRslt, _Save_MXATrace);
                                Read_MXA2_Trace(1, _TestNum, out R_NF2_Freq, out R_NF2_Ampl, _Search_Method, _TestParaName);

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, R_NF1_Freq, ref _LossOutputPathRX1, ref StrError);
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX2CalSegm, R_NF2_Freq, ref _LossOutputPathRX2, ref StrError);

                                R_NF1_Ampl = R_NF1_Ampl - _LossOutputPathRX1 - tbOutputLoss;
                                R_NF2_Ampl = R_NF2_Ampl - _LossOutputPathRX2 - tbOutputLoss;

                                //Save_MXA1Trace(1, _TestParaName, _Save_MXATrace);
                                //Save_MXA2Trace(1, _TestParaName, _Save_MXATrace);

                                //Measure SMU current
                                MeasSMU = _SMUMeasCh.Split(',');
                                if (_Test_SMU)
                                {
                                    DelayMs(_RdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (_SMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else    //if fail power out search , set data to default
                            {

                                //Need to turn off sweep mode - interference when running multisite because SG will go back to start freq once completed sweep
                                Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.CW);       //setting will set back to default freq define earlier
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);

                                SGTargetPin = _Pin1 - totalInputLoss;       //reset the initial power setting to default
                                R_NF1_Freq = -999;
                                R_NF2_Freq = -999;

                                R_NF1_Ampl = 999;
                                R_NF2_Ampl = 999;

                                //Measure SMU current
                                MeasSMU = _SMUMeasCh.Split(',');
                                if (_Test_SMU)
                                {
                                    DelayMs(_RdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (_SMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (_OffSG1)
                            {
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                            }

                            //Initialize & clear MXA trace to prepare for next measurement
                            Eq.Site[0]._EqSA01.CLEAR_WRITE();
                            Eq.Site[0]._EqSA02.CLEAR_WRITE();
                            //Eq.Site[0]._EqSA01.SET_TRACE_DETECTOR("MAXHOLD");
                            //Eq.Site[0]._EqSA02.SET_TRACE_DETECTOR("MAXHOLD");

                            DelayMs(_StopSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq
                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ATFResultBuilder.AddResultToDict(_TestParaName + "_TestTime" + _TestNum, tTime.ElapsedMilliseconds, ref StrError);
                            //}

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)_Estimate_TestTime)
                            {
                                syncTest_Delay = (long)_Estimate_TestTime - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            #endregion
                            break;

                        case "NF_NONCA_NDIAG":
                            // This sweep is a faster sweep , it is a continuous sweep base on SG freq sweep mode
                            #region NF NONCA NDIAG

                            prevRslt = 0;
                            status = false;
                            pwrSearch = false;
                            Index = 0;
                            SAReferenceLevel = -20;
                            vBW_Hz = 300;
                            tx1_span = 0;
                            tx1_noPoints = 0;
                            rx1_span = 0;
                            rx1_cntrfreq = 0;
                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(_PoutTolerance);
                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                            {
                                tolerancePwr = 0.5;
                            }

                            //use for searching previous result - to get the DUT LNA gain from previous result
                            if (Convert.ToInt16(_TestUsePrev) > 0)
                            {
                                usePrevRslt = true;
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                prevRslt = Math.Round(ReportRslt(_TestUsePrev, resultTag), 3);
                            }

                            //DelayMs(_StartSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region PowerSensor Offset, MXG and MXA1 configuration

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((_StopTXFreq1 - _StartTXFreq1) / _StepTXFreq1);
                            _TXFreq = _StartTXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, _TXFreq, ref _LossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)_LossInputPathSG1;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _TXFreq, ref _LossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)_LossCouplerPath;
                                _TXFreq = _TXFreq + _StepTXFreq1;
                            }

                            tmpAveInputLoss = tmpInputLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset


                            //change PowerSensor, MXG setting
                            Eq.Site[0]._EqPwrMeter.SetOffset(1, totalOutputLoss);
                            Eq.Site[0]._EqSG01.SetFreq(Convert.ToDouble(_SG1_DefaultFreq));

                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", _SwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);
                            rbwParamName = "_" + Math.Abs(myUtility.MXA_Setting.RBW / 1e6).ToString() + "MHz";

                            if (PreviousMXAMode != _SwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                tx1_span = _StopTXFreq1 - _StartTXFreq1;
                                tx1_noPoints = Convert.ToInt16(tx1_span / _StepTXFreq1);
                                Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.LIST);
                                Eq.Site[0]._EqSG01.SET_LIST_TYPE(LibEqmtDriver.SG.N5182_LIST_TYPE.STEP);
                                Eq.Site[0]._EqSG01.SET_LIST_MODE(LibEqmtDriver.SG.INSTR_MODE.AUTO);
                                Eq.Site[0]._EqSG01.SET_LIST_TRIG_SOURCE(LibEqmtDriver.SG.N5182_TRIG_TYPE.TIM);
                                Eq.Site[0]._EqSG01.SET_CONT_SWEEP(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);

                                Eq.Site[0]._EqSG01.SET_START_FREQUENCY(_StartTXFreq1 - (_StepTXFreq1 / 2));
                                Eq.Site[0]._EqSG01.SET_STOP_FREQUENCY(_StopTXFreq1 + (_StepTXFreq1 / 2));
                                Eq.Site[0]._EqSG01.SET_TRIG_TIMERPERIOD(_DwellT1);
                                Eq.Site[0]._EqSG01.SET_SWEEP_POINT(tx1_noPoints + 2);   //need to add additional 2 points to calculated no of points because of extra point of start_freq and stop_freq for MXG and MXA sync

                                SGTargetPin = _Pin1 - totalInputLoss;
                                Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                Eq.Site[0]._ModulationType = (LibEqmtDriver.SG.N5182A_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.SG.N5182A_WAVEFORM_MODE), _WaveFormName);
                                Eq.Site[0]._EqSG01.SELECT_WAVEFORM(Eq.Site[0]._ModulationType);
                                Eq.Site[0]._EqSG01.SET_ROUTE_CONN_TOUT(LibEqmtDriver.SG.N5182A_ROUTE_SUBSYS.SweepRun);
                                Eq.Site[0]._EqSG01.SINGLE_SWEEP();      //need to sweep SG for power search - RF ON in sweep mode

                                if (_SetFullMod)
                                {
                                    //This setting will set the modulation for N5182A to full modulation
                                    //Found out that when this set to default (RMS) , the modulation is mutated (CW + Mod) when running under sweep mode for NF
                                    Eq.Site[0]._EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.Mod);
                                }
                                else
                                {
                                    Eq.Site[0]._EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.RMS);
                                }
                                #endregion

                                #region MXA 1 setup
                                DelayMs(_Setup_Delay);
                                rx1_span = (_StopRXFreq1 - _StartRXFreq1);
                                rx1_cntrfreq = _StartRXFreq1 + (rx1_span / 2);
                                rx1_mxa_nopts = (int)((rx1_span / rx1_mxa_nopts_step) + 1);

                                Eq.Site[0]._EqSA01.Select_Instrument(LibEqmtDriver.SA.N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);

                                //ANTHONY-ATT
                                Eq.Site[0]._EqSA01.AUTO_ATTENUATION(false);
                                if (Convert.ToDouble(_SA1att) != CurrentSaAttn)
                                {
                                    Eq.Site[0]._EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToDouble(_SA1att));
                                    CurrentSaAttn = Convert.ToDouble(_SA1att);
                                }

                                //Eq.Site[0]._EqSA01.ELEC_ATTEN_ENABLE(true);
                                Eq.Site[0]._EqSA01.TRIGGER_SINGLE();
                                Eq.Site[0]._EqSA01.TRACE_AVERAGE(1);
                                Eq.Site[0]._EqSA01.AVERAGE_OFF();

                                Eq.Site[0]._EqSA01.FREQ_CENT(rx1_cntrfreq.ToString(), "MHz");
                                Eq.Site[0]._EqSA01.SPAN(rx1_span);
                                Eq.Site[0]._EqSA01.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                Eq.Site[0]._EqSA01.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                Eq.Site[0]._EqSA01.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);

                                Eq.Site[0]._EqSA01.SWEEP_POINTS(rx1_mxa_nopts);

                                if (_SetRX1NDiag)
                                {
                                    Eq.Site[0]._EqSA01.CONTINUOUS_MEASUREMENT_ON();

                                    //ANTHONY-ATT
                                    if (Convert.ToDouble(_SA1att) != CurrentSaAttn) //Anthony
                                    {
                                        Eq.Site[0]._EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToDouble(_SA1att));
                                        CurrentSaAttn = Convert.ToDouble(_SA1att);
                                    }
                                    Eq.Site[0]._EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    trigDelay = (decimal)_DwellT1 + (decimal)0.1;       //fixed 0.1ms delay
                                    Eq.Site[0]._EqSA01.SET_TRIG_DELAY(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1, trigDelay.ToString());
                                    Eq.Site[0]._EqSA01.SWEEP_TIMES(Convert.ToInt16(tx1_noPoints * _DwellT1));
                                }
                                else
                                {
                                    Eq.Site[0]._EqSA01.CONTINUOUS_MEASUREMENT_ON();

                                    //ANTHONY-ATT
                                    if (myUtility.MXA_Setting.Attenuation != CurrentSaAttn)
                                    {
                                        Eq.Site[0]._EqSA01.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                        CurrentSaAttn = Convert.ToDouble(myUtility.MXA_Setting.Attenuation);
                                    }
                                    Eq.Site[0]._EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    Eq.Site[0]._EqSA01.SWEEP_TIMES(Convert.ToInt16(_RX1SweepT));
                                }

                                //Initialize & clear MXA trace
                                Eq.Site[0]._EqSA01.MARKER_TURN_ON_NORMAL_POINT(1, (float)rx1_cntrfreq);
                                Eq.Site[0]._EqSA01.CLEAR_WRITE();
                                status = Eq.Site[0]._EqSA01.OPERATION_COMPLETE();

                                #endregion

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = _SwBand.ToUpper();
                            }
                            #endregion

                            #region measure contact power (Pout1)
                            if (StopOnFail.TestFail == false)
                            {
                                //Just for maximator Special case // Trick - 39mA  21.06.16
                                Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[1], Eq.Site[0]._EqSMU, _SMUVCh[1], _SMUILimitCh[2]);

                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.LIST);
                                if (!_TunePwr_TX1)
                                {
                                    StopOnFail.TestFail = true;     //init to fail state as default
                                    if (_Test_Pout1)
                                    {
                                        DelayMs(_RdPwr_Delay);
                                        R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = Math.Round(SGTargetPin + totalInputLoss, 3);
                                        if (Math.Abs(_Pout1 - R_Pout1) <= (tolerancePwr + 3.5))
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                        }
                                    }
                                    else
                                    {
                                        //No Pout measurement required, default set flag to pass
                                        pwrSearch = true;
                                        StopOnFail.TestFail = false;
                                    }
                                }
                                else
                                {
                                    do
                                    {
                                        StopOnFail.TestFail = true;     //init to fail state as default
                                        R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                        //R_Pin = TargetPin + (float)_LossInputPathSG1;
                                        R_Pin1 = SGTargetPin + totalInputLoss;

                                        if (Math.Abs(_Pout1 - R_Pout1) >= tolerancePwr)
                                        {
                                            if ((Index == 0) && (SGTargetPin < _SG1MaxPwr))   //preset to initial target power for 1st measurement count
                                            {
                                                Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                                DelayMs(_RdPwr_Delay);
                                                R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                            }

                                            SGTargetPin = SGTargetPin + (_Pout1 - R_Pout1);

                                            if (SGTargetPin < _SG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                            {
                                                Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                                DelayMs(_RdPwr_Delay);
                                            }
                                        }
                                        else if (SGTargetPin > _SG1MaxPwr)      //if input sig gen exit limit , exit pwr search loop
                                        {
                                            SGTargetPin = _Pin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                            break;
                                        }
                                        else
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                            break;
                                        }

                                        Index++;
                                    }
                                    while (Index < 10);     // max power search loop
                                }
                            }

                            #endregion

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)_StartSync_Delay)
                            {
                                syncTest_Delay = (long)_StartSync_Delay - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            if (pwrSearch)
                            {
                                Eq.Site[0]._EqSG01.SINGLE_SWEEP();
                                status = Eq.Site[0]._EqSG01.OPERATION_COMPLETE();

                                //Need to turn off sweep mode - interference when running multisite because SG will go back to start freq once completed sweep
                                Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.CW);       //setting will set back to default freq define earlier

                                DelayMs(_Trig_Delay);
                                Capture_MXA1_Trace(1, _TestNum, _TestParaName, _RX1Band, prevRslt, _Save_MXATrace);
                                Read_MXA1_Trace(1, _TestNum, out R_NF1_Freq, out R_NF1_Ampl, _Search_Method, _TestParaName);

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, R_NF1_Freq, ref _LossOutputPathRX1, ref StrError);
                                R_NF1_Ampl = R_NF1_Ampl - _LossOutputPathRX1 - tbOutputLoss;
                                //Save_MXA1Trace(1, _TestParaName, _Save_MXATrace);

                                //Measure SMU current
                                MeasSMU = _SMUMeasCh.Split(',');
                                if (_Test_SMU)
                                {
                                    DelayMs(_RdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (_SMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else    //if fail power out search , set data to default
                            {
                                //Need to turn off sweep mode - interference when running multisite because SG will go back to start freq once completed sweep
                                Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.CW);       //setting will set back to default freq define earlier
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);

                                SGTargetPin = _Pin1 - totalInputLoss;       //reset the initial power setting to default
                                R_NF1_Freq = -999;
                                R_NF1_Ampl = 999;

                                //Measure SMU current
                                MeasSMU = _SMUMeasCh.Split(',');
                                if (_Test_SMU)
                                {
                                    DelayMs(_RdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (_SMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (_OffSG1)
                            {
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                            }

                            //Initialize & clear MXA trace to prepare for next measurement
                            Eq.Site[0]._EqSA01.CLEAR_WRITE();
                            //Eq.Site[0]._EqSA01.SET_TRACE_DETECTOR("MAXHOLD");

                            DelayMs(_StopSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq
                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ATFResultBuilder.AddResultToDict(_TestParaName + "_TestTime" + _TestNum, tTime.ElapsedMilliseconds, ref StrError);
                            //}

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)_Estimate_TestTime)
                            {
                                syncTest_Delay = (long)_Estimate_TestTime - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            #endregion
                            break;

                        case "NF_FIX_NMAX":
                            // This sweep is a slow sweep , will change SG freq and measure NF for every test points
                            // Using Marker Function Noise (measure at dBm/Hz) with External Amp Gain Offset
                            #region NOISE STEP SWEEP NDIAG/NMAX

                            prevRslt = 0;
                            status = false;
                            pwrSearch = false;
                            Index = 0;
                            tx1_span = 0;
                            tx1_noPoints = 0;
                            rx1_span = 0;
                            rx1_cntrfreq = 0;
                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(_PoutTolerance);
                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                            {
                                tolerancePwr = 0.5;
                            }

                            //use for searching previous result - to get the DUT LNA gain from previous result
                            if (Convert.ToInt16(_TestUsePrev) > 0)
                            {
                                usePrevRslt = true;
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                prevRslt = Math.Round(ReportRslt(_TestUsePrev, resultTag), 3);
                            }

                            DelayMs(_StartSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], _CalTag.ToUpper(), _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region Calc Average Pathloss, PowerSensor Offset, MXG and MXA1 configuration

                            #region Get Average Pathloss
                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            if (_StopTXFreq1 == _StartTXFreq1)
                            {
                                txcount = 1;
                            }
                            else
                            {
                                txcount = Convert.ToInt16(((_StopTXFreq1 - _StartTXFreq1) / _StepTXFreq1) + 1);
                            }

                            tx_freqArray = new double[txcount];
                            contactPwr_Array = new double[txcount];
                            nfAmpl_Array = new double[txcount];
                            nfAmplFreq_Array = new double[txcount];

                            _TXFreq = _StartTXFreq1;
                            for (int i = 0; i < txcount; i++)
                            {
                                tx_freqArray[i] = _TXFreq;

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, _TXFreq, ref _LossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)_LossInputPathSG1;

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _TXFreq, ref _LossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)_LossCouplerPath;

                                _TXFreq = _TXFreq + _StepTXFreq1;
                            }
                            //Calculate the average pathloss/pathgain
                            tmpAveInputLoss = tmpInputLoss / txcount;
                            tmpAveCouplerLoss = tmpCouplerLoss / txcount;
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            //Get average pathloss base on start and stop freq
                            rxcount = Convert.ToInt16(((_StopRXFreq1 - _StartRXFreq1) / _StepRXFreq1) + 1);
                            rx_freqArray = new double[rxcount];

                            _RXFreq = _StartRXFreq1;
                            for (int i = 0; i < rxcount; i++)
                            {
                                rx_freqArray[i] = _RXFreq;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                tmpRxLoss = tmpRxLoss + (float)_LossOutputPathRX1;
                                _RXFreq = _RXFreq + _StepRXFreq1;
                            }
                            tmpAveRxLoss = tmpRxLoss / rxcount;
                            totalRXLoss = tmpAveRxLoss - tbOutputLoss;
                            #endregion

                            #region config Power Sensor, MXA and MXG
                            //change PowerSensor,  Set Default Power for MXG setting
                            Eq.Site[0]._EqPwrMeter.SetOffset(1, totalOutputLoss);
                            SGTargetPin = _Pin1 - totalInputLoss;
                            Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                            Eq.Site[0]._EqSG01.SetFreq(Convert.ToDouble(_StartTXFreq1));
                            Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);

                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", _SwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);
                            rbwParamName = "_" + Math.Abs(myUtility.MXA_Setting.RBW / 1e6).ToString() + "MHz";

                            if (PreviousMXAMode != _SwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.FIX);
                                Eq.Site[0]._EqSG01.SetFreq(Math.Abs(Convert.ToDouble(_StartTXFreq1)));

                                SGTargetPin = _Pin1 - totalInputLoss;
                                Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                Eq.Site[0]._ModulationType = (LibEqmtDriver.SG.N5182A_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.SG.N5182A_WAVEFORM_MODE), _WaveFormName);
                                Eq.Site[0]._EqSG01.SELECT_WAVEFORM(Eq.Site[0]._ModulationType);
                                Eq.Site[0]._EqSG01.SINGLE_SWEEP();

                                if (_SetFullMod)
                                {
                                    //This setting will set the modulation for N5182A to full modulation
                                    //Found out that when this set to default (RMS) , the modulation is mutated (CW + Mod) when running under sweep mode for NF
                                    Eq.Site[0]._EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.Mod);
                                }
                                else
                                {
                                    Eq.Site[0]._EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.RMS);
                                }
                                #endregion

                                #region MXA 1 setup
                                DelayMs(_Setup_Delay);
                                if (_SetRX1NDiag)
                                {
                                    //NDIAG - RX Bandwidth base on stepsize
                                    rx1_span = _StepRXFreq1 * 2;
                                    rx1_cntrfreq = _StartRXFreq1;
                                    rx1_mxa_nopts = 101;    //fixed no of points
                                }
                                else
                                {
                                    //NMAX - will use full RX Badwidth (StartRX to StopRX)
                                    rx1_span = _StopRXFreq1 - _StartRXFreq1;
                                    rx1_cntrfreq = Math.Round(_StartRXFreq1 + (rx1_span / 2), 3);
                                    rx1_mxa_nopts = (int)((rx1_span / rx1_mxa_nopts_step) + 1);
                                }

                                Eq.Site[0]._EqSA01.Select_Instrument(LibEqmtDriver.SA.N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);
                                Eq.Site[0]._EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_FreeRun);
                                Eq.Site[0]._EqSA01.AUTO_ATTENUATION(false);
                                Eq.Site[0]._EqSA01.CONTINUOUS_MEASUREMENT_OFF();
                                Eq.Site[0]._EqSA01.TRACE_AVERAGE(1);
                                Eq.Site[0]._EqSA01.AVERAGE_OFF();

                                Eq.Site[0]._EqSA01.SPAN(rx1_span);
                                Eq.Site[0]._EqSA01.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                Eq.Site[0]._EqSA01.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                Eq.Site[0]._EqSA01.FREQ_CENT(rx1_cntrfreq.ToString(), "MHz");
                                Eq.Site[0]._EqSA01.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);

                                //Eq.Site[0]._EqSA01.SWEEP_POINTS(myUtility.MXA_Setting.NoPoints);
                                Eq.Site[0]._EqSA01.SWEEP_POINTS(rx1_mxa_nopts);
                                Eq.Site[0]._EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToInt16(_SA1att));
                                Eq.Site[0]._EqSA01.SWEEP_TIMES(Convert.ToInt16(_RX1SweepT));

                                //Initialize & clear MXA trace
                                Eq.Site[0]._EqSA01.MARKER_TURN_ON_NORMAL_POINT(1, (float)_StartRXFreq1);
                                Eq.Site[0]._EqSA01.CLEAR_WRITE();

                                status = Eq.Site[0]._EqSG01.OPERATION_COMPLETE();
                                #endregion

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = _SwBand.ToUpper();
                            }
                            #endregion

                            #endregion

                            #region measure contact power (Pout1)
                            if (StopOnFail.TestFail == false)
                            {
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);

                                if (!_TunePwr_TX1)
                                {
                                    StopOnFail.TestFail = true;     //init to fail state as default

                                    if (_Test_Pout1)
                                    {
                                        DelayMs(_RdPwr_Delay);
                                        R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = Math.Round(SGTargetPin + totalInputLoss, 3);
                                        if (Math.Abs(_Pout1 - R_Pout1) <= (tolerancePwr + 3.5))
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                        }
                                    }
                                    else
                                    {
                                        //No Pout measurement required, default set flag to pass
                                        pwrSearch = true;
                                        StopOnFail.TestFail = false;
                                    }

                                }
                                else
                                {
                                    do
                                    {
                                        StopOnFail.TestFail = true;     //init to fail state as default
                                        R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = SGTargetPin + totalInputLoss;

                                        if (Math.Abs(_Pout1 - R_Pout1) >= tolerancePwr)
                                        {
                                            if ((Index == 0) && (SGTargetPin < _SG1MaxPwr))   //preset to initial target power for 1st measurement count
                                            {
                                                Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                                DelayMs(_RdPwr_Delay);
                                                R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                            }

                                            SGTargetPin = SGTargetPin + (_Pout1 - R_Pout1);

                                            if (SGTargetPin < _SG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                            {
                                                Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                                DelayMs(_RdPwr_Delay);
                                            }
                                        }
                                        else if (SGTargetPin > _SG1MaxPwr)      //if input sig gen exit limit , exit pwr search loop
                                        {
                                            SGTargetPin = _Pin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                            break;
                                        }
                                        else
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                            break;
                                        }

                                        Index++;
                                    }
                                    while (Index < 10);     // max power search loop
                                }
                            }

                            #endregion

                            if (pwrSearch)
                            {
                                for (int i = 0; i < tx_freqArray.Length; i++)
                                {
                                    if (_SetRX1NDiag)   //NDIAG - RX Bandwidth base on stepsize else NMAX - will use full RX Badwidth (StartRX to StopRX)
                                    {
                                        Eq.Site[0]._EqSA01.FREQ_CENT(rx_freqArray[i].ToString(), "MHz");    //RX Bandwidth base on stepsize
                                        Eq.Site[0]._EqSG01.SetFreq(Convert.ToDouble(tx_freqArray[i]));
                                        Eq.Site[0]._EqSA01.TRIGGER_IMM();
                                        DelayMs(Convert.ToInt16(_RX1SweepT));       //Need to set same delay as sweep time before read trace  

                                        status = Eq.Site[0]._EqSG01.OPERATION_COMPLETE();
                                        Capture_MXA1_Trace(1, _TestNum, _TestParaName, _RX1Band, prevRslt, false);
                                        Read_MXA1_Trace(1, _TestNum, out nfAmplFreq_Array[i], out nfAmpl_Array[i], _Search_Method, _TestParaName);
                                        nfAmpl_Array[i] = nfAmpl_Array[i] - totalRXLoss;
                                    }
                                    else
                                    {
                                        Eq.Site[0]._EqSG01.SetFreq(Convert.ToDouble(tx_freqArray[i]));
                                        Eq.Site[0]._EqSA01.TRIGGER_IMM();
                                        DelayMs(_Trig_Delay);

                                        status = Eq.Site[0]._EqSG01.OPERATION_COMPLETE();
                                        Capture_MXA1_Trace(1, _TestNum, _TestParaName, _RX1Band, prevRslt, _Save_MXATrace);
                                        Read_MXA1_Trace(1, _TestNum, out nfAmplFreq_Array[i], out nfAmpl_Array[i], _Search_Method, _TestParaName);
                                        nfAmpl_Array[i] = nfAmpl_Array[i] - totalRXLoss;
                                    }
                                }

                                #region Search result MAX or MIN and Save to Datalog
                                //Find result MAX or MIN result
                                switch (_Search_Method.ToUpper())
                                {
                                    case "MAX":
                                        R_NF1_Ampl = nfAmpl_Array.Max();
                                        indexdata = Array.IndexOf(nfAmpl_Array, R_NF1_Ampl);     //return index of max value
                                        R_NF1_Freq = nfAmplFreq_Array[indexdata];
                                        break;

                                    case "MIN":
                                        R_NF1_Ampl = nfAmpl_Array.Min();
                                        indexdata = Array.IndexOf(nfAmpl_Array, R_NF1_Ampl);     //return index of max value
                                        R_NF1_Freq = nfAmplFreq_Array[indexdata];
                                        break;

                                    default:
                                        MessageBox.Show("Test Parameter : " + _TestParam + "(" + _Search_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                        break;
                                }

                                //Save all data to datalog 
                                if (_SetRX1NDiag)           //save trace method is different between NDIAG and NMAX
                                {
                                    if (_Save_MXATrace)
                                    {
                                        string[] templine = new string[4];
                                        ArrayList LocalTextList = new ArrayList();
                                        ArrayList tmpCalMsg = new ArrayList();

                                        //Calibration File Header
                                        LocalTextList.Add("#MXA1 NF STEP SWEEP DATALOG");
                                        LocalTextList.Add("#Date : " + DateTime.Now.ToShortDateString());
                                        LocalTextList.Add("#Time : " + DateTime.Now.ToLongTimeString());
                                        LocalTextList.Add("#Input TX Power : " + _Pin1 + " dBm");
                                        LocalTextList.Add("#Measure Contact Power : " + Math.Round(R_Pout1, 3) + " dBm");
                                        templine[0] = "#TX_FREQ";
                                        templine[1] = "NOISE_RXFREQ";
                                        templine[2] = "NOISE_AMPL";
                                        LocalTextList.Add(string.Join(",", templine));

                                        // Start looping until complete the freq range
                                        for (istep = 0; istep < tx_freqArray.Length; istep++)
                                        {
                                            //Sorted the calibration result to array
                                            templine[0] = Convert.ToString(tx_freqArray[istep]);
                                            templine[1] = Convert.ToString(nfAmplFreq_Array[istep]);
                                            templine[2] = Convert.ToString(Math.Round(nfAmpl_Array[istep], 3));
                                            LocalTextList.Add(string.Join(",", templine));
                                        }

                                        //Write cal data to csv file
                                        if (!Directory.Exists(SNPFile.FileOutput_Path))
                                        {
                                            Directory.CreateDirectory(SNPFile.FileOutput_Path);
                                        }
                                        //Write cal data to csv file
                                        string tempPath = SNPFile.FileOutput_Path + SNPFile.FileOutput_FileName + "_" + _TestParaName + "_Unit" + tmpUnit_No.ToString() + ".csv";
                                        //MessageBox.Show("Path : " + tempPath);
                                        IO_TxtFile.CreateWrite_TextFile(tempPath, LocalTextList);
                                    }
                                }
                                #endregion

                                //Measure SMU current
                                MeasSMU = _SMUMeasCh.Split(',');
                                if (_Test_SMU)
                                {
                                    DelayMs(_RdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (_SMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else    //if fail power out search , set data to default
                            {
                                SGTargetPin = _Pin1 - totalInputLoss;       //reset the initial power setting to default
                                R_NF1_Freq = -999;
                                R_NF1_Ampl = 999;

                                //Measure SMU current
                                MeasSMU = _SMUMeasCh.Split(',');
                                if (_Test_SMU)
                                {
                                    DelayMs(_RdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (_SMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (_OffSG1)
                            {
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                            }

                            //Initialize & clear MXA trace to prepare for next measurement
                            Eq.Site[0]._EqSA01.CLEAR_WRITE();

                            DelayMs(_StopSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq
                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ATFResultBuilder.AddResultToDict(_TestParaName + "_TestTime" + _TestNum, tTime.ElapsedMilliseconds, ref StrError);
                            //}

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)_Estimate_TestTime)
                            {
                                syncTest_Delay = (long)_Estimate_TestTime - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            #endregion
                            break;

                        case "RXPATH_CONTACT":
                            //this function is checking the pathloss/pathgain from antenna port to rx port

                            #region LXI_RXPATH_CONTACT
                            R_NF1_Freq = -99999;
                            R_NF1_Ampl = 99999;

                            NoOfPts = (Convert.ToInt32((_StopRXFreq1 - _StartRXFreq1) / _StepRXFreq1)) + 1;
                            RXContactdBm = new double[NoOfPts];
                            RXContactFreq = new double[NoOfPts];

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region Pathloss Offset

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((_StopRXFreq1 - _StartRXFreq1) / _StepRXFreq1);
                            _RXFreq = _StartRXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                RXContactFreq[i] = _RXFreq;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                tmpRxLoss = Math.Round(tmpRxLoss + (float)_LossOutputPathRX1, 3);   //need to use round function because of C# float and double floating point bug/error
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _RXFreq, ref _LossCouplerPath, ref StrError);
                                tmpCouplerLoss = Math.Round(tmpCouplerLoss + (float)_LossCouplerPath, 3);   //need to use round function because of C# float and double floating point bug/error
                                _RXFreq = Convert.ToSingle(Math.Round(_RXFreq + _StepRXFreq1, 3));           //need to use round function because of C# float and double floating point bug/error
                            }

                            tmpAveRxLoss = tmpRxLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveCouplerLoss - tbInputLoss;       //pathloss from SG to ANT Port inclusive fixed TB Loss
                            totalOutputLoss = tmpAveRxLoss - tbOutputLoss;          //pathgain from RX Port to SA inclusive fixed TB Loss

                            //Find actual SG Power Level
                            SGTargetPin = _Pin1 - totalInputLoss;
                            if (SGTargetPin > _SG1MaxPwr)       //exit test if SG Target Power is more that VST recommended Pout
                            {
                                break;
                            }

                            #region Decode MXA Config
                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", _SwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);

                            SAReferenceLevel = myUtility.MXA_Setting.RefLevel;
                            vBW_Hz = myUtility.MXA_Setting.VBW;
                            #endregion

                            #endregion

                            #region Test RX Path
                            if (PreviousMXAMode != _SwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                rx1_span = _StopRXFreq1 - _StartRXFreq1;
                                rx1_noPoints = Convert.ToInt16(rx1_span / _StepRXFreq1);
                                rx1_cntrfreq = (_StartRXFreq1 + _StopRXFreq1) / 2;
                                Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.FIX);

                                Eq.Site[0]._EqSG01.SET_CONT_SWEEP(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                                Eq.Site[0]._EqSG01.SetFreq(rx1_cntrfreq);

                                SGTargetPin = _Pin1 - totalInputLoss;
                                Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                Eq.Site[0]._ModulationType = (LibEqmtDriver.SG.N5182A_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.SG.N5182A_WAVEFORM_MODE), _WaveFormName);
                                Eq.Site[0]._EqSG01.SELECT_WAVEFORM(Eq.Site[0]._ModulationType);

                                #endregion
                                #region MXA 1 setup

                                DelayMs(_Setup_Delay);
                                rx1_span = (_StopRXFreq1 - _StartRXFreq1);
                                rx1_cntrfreq = _StartRXFreq1 + (rx1_span / 2);
                                rx1_mxa_nopts = (int)((rx1_span / rx1_mxa_nopts_step) + 1);

                                Eq.Site[0]._EqSA01.Select_Instrument(LibEqmtDriver.SA.N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);
                                Eq.Site[0]._EqSA01.AUTO_ATTENUATION(false);

                                if (Convert.ToDouble(_SA1att) != CurrentSaAttn) //Anthony
                                {
                                    Eq.Site[0]._EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToDouble(_SA1att));
                                    CurrentSaAttn = Convert.ToDouble(_SA1att);
                                }
                                Eq.Site[0]._EqSA01.TRIGGER_SINGLE();
                                Eq.Site[0]._EqSA01.TRACE_AVERAGE(1);
                                Eq.Site[0]._EqSA01.AVERAGE_OFF();
                                Eq.Site[0]._EqSA01.FREQ_CENT(rx1_cntrfreq.ToString(), "MHz");

                                Eq.Site[0]._EqSA01.SWEEP_TIMES(Convert.ToInt16(0));

                                Eq.Site[0]._EqSA01.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                Eq.Site[0]._EqSA01.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                Eq.Site[0]._EqSA01.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);

                                Eq.Site[0]._EqSA01.SWEEP_POINTS(1);
                                Eq.Site[0]._EqSA01.SPAN(0);

                                if (_SetRX1NDiag)
                                {
                                    Eq.Site[0]._EqSA01.CONTINUOUS_MEASUREMENT_ON();
                                    Eq.Site[0]._EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToInt16(_SA1att));
                                    Eq.Site[0]._EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    trigDelay = (decimal)_DwellT1 + (decimal)0.1;       //fixed 0.1ms delay
                                    Eq.Site[0]._EqSA01.SET_TRIG_DELAY(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1, trigDelay.ToString());
                                    Eq.Site[0]._EqSA01.SWEEP_TIMES(Convert.ToInt16(tx1_noPoints * _DwellT1));
                                }
                                else
                                {
                                    Eq.Site[0]._EqSA01.CONTINUOUS_MEASUREMENT_ON();

                                    if (myUtility.MXA_Setting.Attenuation != CurrentSaAttn) //Anthony
                                    {
                                        Eq.Site[0]._EqSA01.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                        CurrentSaAttn = Convert.ToDouble(myUtility.MXA_Setting.Attenuation);
                                    }

                                    Eq.Site[0]._EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_FreeRun);
                                }
                                #endregion

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = _SwBand.ToUpper();
                            }

                            R_NF1_Freq = rx1_cntrfreq;
                            Eq.Site[0]._EqSA01.TRIGGER_SINGLE();    //-14/3 Anthony added
                            Eq.Site[0]._EqSA01.TRIGGER_IMM();       //-14/3 Anthony added
                            Eq.Site[0]._EqSA01.OPERATION_COMPLETE();//-14/3 Anthony added
                            R_NF1_Ampl = (Eq.Site[0]._EqSA01.MEASURE_PEAK_POINT(1) - _LossOutputPathRX1 - tbInputLoss) - _Pin1;

                            Save_MXA1Trace(1, _TestParaName, _Save_MXATrace);

                            #endregion

                            //for test time checking
                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ATFResultBuilder.AddResultToDict(_TestParaName + "_TestTime" + _TestNum, tTime.ElapsedMilliseconds, ref StrError);
                            //}

                            #endregion
                            break;

                        case "NF_STEPSWEEP_NDIAG":
                            // This sweep is a slow sweep , will change SG freq and measure NF for every test points
                            #region NF STEP SWEEP NDIAG

                            int tx_freqPoints;

                            status = false;
                            pwrSearch = false;
                            Index = 0;
                            tx1_span = 0;
                            tx1_noPoints = 0;
                            rx1_span = 0;
                            rx1_cntrfreq = 0;
                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(_PoutTolerance);
                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                            {
                                tolerancePwr = 0.5;
                            }

                            DelayMs(_StartSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region PowerSensor Offset, MXG and MXA1 configuration

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((_StopTXFreq1 - _StartTXFreq1) / _StepTXFreq1);
                            tx_freqArray = new double[count];
                            rx_freqArray = new double[count];
                            contactPwr_Array = new double[count];
                            nfAmpl_Array = new double[count];
                            _TXFreq = _StartTXFreq1;
                            _RXFreq = _StartRXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                tx_freqArray[count] = _TXFreq;
                                rx_freqArray[count] = _RXFreq;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, _TXFreq, ref _LossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)_LossInputPathSG1;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _TXFreq, ref _LossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)_LossCouplerPath;
                                _TXFreq = _TXFreq + _StepTXFreq1;
                            }

                            tmpAveInputLoss = tmpInputLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            //change PowerSensor, MXG setting
                            Eq.Site[0]._EqPwrMeter.SetOffset(1, totalOutputLoss);
                            Eq.Site[0]._EqSG01.SetFreq(Convert.ToDouble(_SG1_DefaultFreq));
                            Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);

                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", _SwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);

                            if (PreviousMXAMode != _SwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                tx1_span = (_StopTXFreq1 - _StartTXFreq1) - _StepTXFreq1;
                                tx1_noPoints = Convert.ToInt16(tx1_span / _StepTXFreq1) + 1;       //need to add additional 1 points to calculated no of points  
                                Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.LIST);
                                Eq.Site[0]._EqSG01.SET_LIST_TYPE(LibEqmtDriver.SG.N5182_LIST_TYPE.STEP);
                                Eq.Site[0]._EqSG01.SET_LIST_MODE(LibEqmtDriver.SG.INSTR_MODE.AUTO);
                                Eq.Site[0]._EqSG01.SET_LIST_TRIG_SOURCE(LibEqmtDriver.SG.N5182_TRIG_TYPE.TIM);
                                Eq.Site[0]._EqSG01.SET_CONT_SWEEP(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);

                                Eq.Site[0]._EqSG01.SET_START_FREQUENCY(_StartTXFreq1 + (_StepTXFreq1 / 2));
                                Eq.Site[0]._EqSG01.SET_STOP_FREQUENCY(_StopTXFreq1 - (_StepTXFreq1 / 2));
                                Eq.Site[0]._EqSG01.SET_TRIG_TIMERPERIOD(_DwellT1);
                                Eq.Site[0]._EqSG01.SET_SWEEP_POINT(tx1_noPoints);

                                SGTargetPin = _Pin1 - totalInputLoss;
                                Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                Eq.Site[0]._ModulationType = (LibEqmtDriver.SG.N5182A_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.SG.N5182A_WAVEFORM_MODE), _WaveFormName);
                                Eq.Site[0]._EqSG01.SELECT_WAVEFORM(Eq.Site[0]._ModulationType);

                                Eq.Site[0]._EqSG01.SINGLE_SWEEP();      //need to sweep SG for power search - RF ON in sweep mode
                                #endregion

                                #region MXA 1 setup
                                rx1_span = (_StopRXFreq1 - _StartRXFreq1);
                                rx1_cntrfreq = _StartRXFreq1 + (rx1_span / 2);
                                Eq.Site[0]._EqSA01.Select_Instrument(LibEqmtDriver.SA.N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);
                                Eq.Site[0]._EqSA01.AUTO_ATTENUATION(true);
                                Eq.Site[0]._EqSA01.TRIGGER_SINGLE();
                                Eq.Site[0]._EqSA01.TRACE_AVERAGE(1);
                                Eq.Site[0]._EqSA01.AVERAGE_OFF();

                                Eq.Site[0]._EqSA01.FREQ_CENT(rx1_cntrfreq.ToString(), "MHz");
                                Eq.Site[0]._EqSA01.SPAN(rx1_span);
                                Eq.Site[0]._EqSA01.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                Eq.Site[0]._EqSA01.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                Eq.Site[0]._EqSA01.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);

                                Eq.Site[0]._EqSA01.SWEEP_POINTS(myUtility.MXA_Setting.NoPoints);


                                if (_SetRX1NDiag)
                                {
                                    Eq.Site[0]._EqSA01.CONTINUOUS_MEASUREMENT_ON();
                                    Eq.Site[0]._EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToInt16(_SA1att));
                                    Eq.Site[0]._EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    Eq.Site[0]._EqSA01.SWEEP_TIMES(Convert.ToInt16(tx1_noPoints * _DwellT1));
                                }
                                else
                                {
                                    Eq.Site[0]._EqSA01.CONTINUOUS_MEASUREMENT_ON();
                                    Eq.Site[0]._EqSA01.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                    Eq.Site[0]._EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    Eq.Site[0]._EqSA01.SWEEP_TIMES(Convert.ToInt16(_RX1SweepT));
                                }

                                //Initialize & clear MXA trace
                                Eq.Site[0]._EqSA01.MARKER_TURN_ON_NORMAL_POINT(1, (float)rx1_cntrfreq);
                                Eq.Site[0]._EqSA01.CLEAR_WRITE();
                                Eq.Site[0]._EqSA01.SET_TRACE_DETECTOR("MAXHOLD");
                                status = Eq.Site[0]._EqSA01.OPERATION_COMPLETE();

                                #endregion

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = _SwBand.ToUpper();
                            }
                            #endregion

                            #region measure contact power (Pout1)
                            Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.LIST);
                            if (!_TunePwr_TX1)
                            {
                                R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                R_Pin1 = Math.Round(SGTargetPin + totalInputLoss, 3);
                                //if (Math.Abs(_Pout1 - R_Pout1) <= tolerancePwr)
                                //{
                                pwrSearch = true;
                                //}
                            }
                            else
                            {
                                do
                                {
                                    R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                    //R_Pin = TargetPin + (float)_LossInputPathSG1;
                                    R_Pin1 = SGTargetPin + totalInputLoss;

                                    if (Math.Abs(_Pout1 - R_Pout1) >= tolerancePwr)
                                    {
                                        if ((Index == 0) && (SGTargetPin < _SG1MaxPwr))   //preset to initial target power for 1st measurement count
                                        {
                                            Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                            R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                        }

                                        SGTargetPin = SGTargetPin + (_Pout1 - R_Pout1);

                                        if (SGTargetPin < _SG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                        {
                                            Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                            DelayMs(_RdPwr_Delay);
                                        }
                                    }
                                    else if (SGTargetPin > _SG1MaxPwr)      //if input sig gen exit limit , exit pwr search loop
                                    {
                                        SGTargetPin = _Pin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                        break;
                                    }
                                    else
                                    {
                                        pwrSearch = true;
                                        break;
                                    }

                                    Index++;
                                }
                                while (Index < 10);     // max power search loop
                            }
                            #endregion

                            if (pwrSearch)
                            {
                                Eq.Site[0]._EqSG01.SINGLE_SWEEP();
                                status = Eq.Site[0]._EqSG01.OPERATION_COMPLETE();

                                //Need to turn off sweep mode - interference when running multisite because SG will go back to start freq once completed sweep
                                Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.CW);       //setting will set back to default freq define earlier

                                DelayMs(_Trig_Delay);
                                R_NF1_Freq = Eq.Site[0]._EqSA01.MEASURE_PEAK_FREQ(_Generic_Delay);

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, (R_NF1_Freq / 1000000), ref _LossOutputPathRX1, ref StrError);

                                R_NF1_Ampl = Eq.Site[0]._EqSA01.MEASURE_PEAK_POINT(_Generic_Delay) - _LossOutputPathRX1 - tbOutputLoss;
                                Save_MXA1Trace(1, _TestParaName, _Save_MXATrace);

                                #region Search result MAX or MIN and Save to Datalog
                                //Find result MAX or MIN result
                                switch (_Search_Method.ToUpper())
                                {
                                    case "MAX":
                                        //initialize start data 
                                        R_NF1_Ampl = nfAmpl_Array[0];
                                        R_NF1_Freq = tx_freqArray[0];
                                        R_Pout = contactPwr_Array[0];

                                        for (int j = 0; j < tx_freqArray.Length; j++)
                                        {
                                            if (R_NF1_Ampl < nfAmpl_Array[j])
                                            {
                                                R_NF1_Ampl = nfAmpl_Array[j];
                                                R_NF1_Freq = tx_freqArray[j];
                                                R_Pout = contactPwr_Array[j];
                                            }
                                        }
                                        break;
                                    case "MIN":
                                        //initialize start data 
                                        R_NF1_Ampl = nfAmpl_Array[0];
                                        R_NF1_Freq = rx_freqArray[0];
                                        R_Pout = contactPwr_Array[0];

                                        for (int j = 0; j < tx_freqArray.Length; j++)
                                        {
                                            if (R_NF1_Ampl > nfAmpl_Array[j])
                                            {
                                                R_NF1_Ampl = nfAmpl_Array[j];
                                                R_NF1_Freq = rx_freqArray[j];
                                                R_Pout = contactPwr_Array[j];
                                            }
                                        }
                                        break;
                                }

                                //Save all data to datalog 
                                if (_Save_MXATrace)
                                {
                                    string[] templine = new string[4];
                                    ArrayList LocalTextList = new ArrayList();
                                    ArrayList tmpCalMsg = new ArrayList();

                                    //Calibration File Header
                                    LocalTextList.Add("#MXA1 NF SWEEP DATALOG");
                                    LocalTextList.Add("#Date : " + DateTime.Now.ToShortDateString());
                                    LocalTextList.Add("#Time : " + DateTime.Now.ToLongTimeString());
                                    LocalTextList.Add("#TX Power : " + _Pout1 + " dBm");
                                    templine[0] = "#TX_FREQ";
                                    templine[1] = "RX_FREQ";
                                    templine[2] = "NF_POWER";
                                    templine[3] = "CONTACT";
                                    LocalTextList.Add(string.Join(",", templine));

                                    // Start looping until complete the freq range
                                    for (istep = 0; istep < tx_freqArray.Length; istep++)
                                    {
                                        //Sorted the calibration result to array
                                        templine[0] = Convert.ToString(tx_freqArray[istep]);
                                        templine[1] = Convert.ToString(rx_freqArray[istep]);
                                        templine[2] = Convert.ToString(Math.Round(nfAmpl_Array[istep], 3));
                                        templine[3] = Convert.ToString(Math.Round(contactPwr_Array[istep], 3));
                                        LocalTextList.Add(string.Join(",", templine));
                                    }

                                    //Write cal data to csv file
                                    string tempPath = SNPFile.FileOutput_Path + SNPFile.FileOutput_FileName + "_" + _TestParaName + "_Unit" + tmpUnit_No.ToString() + ".csv";
                                    //MessageBox.Show("Path : " + tempPath);
                                    IO_TxtFile.CreateWrite_TextFile(tempPath, LocalTextList);
                                }
                                #endregion

                                //Measure SMU current
                                MeasSMU = _SMUMeasCh.Split(',');
                                if (_Test_SMU)
                                {
                                    DelayMs(_RdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (_SMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else    //if fail power out search , set data to default
                            {
                                SGTargetPin = _Pin1 - totalInputLoss;       //reset the initial power setting to default
                                R_NF1_Freq = -999;
                                R_NF1_Ampl = 999;

                                //Measure SMU current
                                MeasSMU = _SMUMeasCh.Split(',');
                                if (_Test_SMU)
                                {
                                    DelayMs(_RdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (_SMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (_OffSG1)
                            {
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                            }

                            //Initialize & clear MXA trace to prepare for next measurement
                            Eq.Site[0]._EqSA01.CLEAR_WRITE();
                            Eq.Site[0]._EqSA01.SET_TRACE_DETECTOR("MAXHOLD");

                            DelayMs(_StopSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq
                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)_Estimate_TestTime)
                            {
                                syncTest_Delay = (long)_Estimate_TestTime - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            #endregion
                            break;

                        case "PXI_NF_NONCA_NDIAG":
                            // This is using PXI VST as Sweeper and Analyzer. Will do multiple sweep in one function because using script (Pwr Servo->Soak Sweep->SoakTime->MultiSweep)
                            // Slight different from LXI solution where you define number of sweep in multiple line in TCF

                            #region PXI NF NONCA NDIAG
                            //NOTE: Some of these inputs may have to be read from input-excel or defined elsewhere
                            //Variable use in VST Measure Function
                            NumberOfRuns = 5;
                            SGPowerLevel = -18;// -18 CDMA dBm //-20 LTE dBm  
                            SAReferenceLevel = -20;
                            SoakTime = 450e-3;
                            SoakFrequency = _StartTXFreq1 * 1e6;
                            vBW_Hz = 300;
                            RBW_Hz = 1e6;
                            preSoakSweep = true; //to indicate if another sweep should be done **MAKE SURE TO SPLIT OUTPUT ARRAY**
                            preSoakSweepTemp = preSoakSweep == true ? 1 : 0; //to indicate if another sweep should be done
                            stepFreqMHz = 0.1;
                            tmpRXFreqHz = _StartRXFreq1 * 1e6;
                            sweepPts = (Convert.ToInt32((_StopTXFreq1 - _StartTXFreq1) / stepFreqMHz)) + 1;
                            //----

                            status = false;
                            pwrSearch = false;
                            Index = 0;
                            tx1_span = 0;
                            tx1_noPoints = 0;
                            rx1_span = 0;
                            rx1_cntrfreq = 0;
                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(_PoutTolerance);

                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                                tolerancePwr = 0.5;

                            if (_PXI_NoOfSweep <= 0)                //check the number of sweep for pxi, set to default if user forget to keyin in excel
                                NumberOfRuns = 1;
                            else
                                NumberOfRuns = _PXI_NoOfSweep;

                            //use for searching previous result - to get the DUT LNA gain from previous result
                            if (Convert.ToInt16(_TestUsePrev) > 0)
                            {
                                usePrevRslt = true;
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                prevRslt = Math.Round(ReportRslt(_TestUsePrev, resultTag), 3);
                            }

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region PowerSensor Offset, MXG and MXA1 configuration

                            //Calculate PAPR offset for PXI SG
                            LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE modulationType;
                            modulationType = (LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE), _WaveFormName.ToUpper());
                            int modArrayNo = (int)Enum.Parse(modulationType.GetType(), modulationType.ToString()); // to get the int value from System.Enum
                            double papr_dB = Math.Round(LibEqmtDriver.NF_VST.NF_VSTDriver.SignalType[modArrayNo].SG_papr_dB, 3);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((_StopTXFreq1 - _StartTXFreq1) / _StepTXFreq1);
                            _TXFreq = _StartTXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, _TXFreq, ref _LossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)_LossInputPathSG1;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _TXFreq, ref _LossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)_LossCouplerPath;
                                _TXFreq = _TXFreq + _StepTXFreq1;
                            }

                            tmpAveInputLoss = tmpInputLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            //change PowerSensor, MXG setting
                            Eq.Site[0]._EqPwrMeter.SetOffset(1, totalOutputLoss);
                            //SGTargetPin = papr_dB - _Pin1 - totalInputLoss;   //wrong equation - Shaz 22/10/2019
                            SGTargetPin = _Pin1 - totalInputLoss + papr_dB;

                            #region MXA Setup

                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", _SwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);

                            SAReferenceLevel = myUtility.MXA_Setting.RefLevel;
                            vBW_Hz = myUtility.MXA_Setting.VBW;
                            //RBW_Hz = myUtility.MXA_Setting.RBW;

                            #endregion

                            //if (PreviousMXAMode != _SwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                //generate modulated signal

                                string TempWaveFormName = _WaveFormName.Replace("_", "");
                                string Script =
                                         "script powerServo\r\n"
                                       + "repeat forever\r\n"
                                       + "generate Signal" + TempWaveFormName + "\r\n"
                                       + "end repeat\r\n"
                                       + "end script";
                                try
                                {
                                    Eq.Site[0]._EqVST.rfsgSession.Arb.Scripting.WriteScript(Script);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message);
                                }
                                Eq.Site[0]._EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                Eq.Site[0]._EqVST.rfsgSession.RF.Frequency = _StartTXFreq1 * 1e6;
                                Eq.Site[0]._EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;

                                //Need to ensure that SG_IQRate re-define , because RX_CONTACT routine has overwritten the initialization data
                                Eq.Site[0]._EqVST.Get_s_SignalType(_Modulation, _WaveFormName, out SG_IQRate);
                                Eq.Site[0]._EqVST.rfsgSession.Arb.IQRate = SG_IQRate;

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = _SwBand.ToUpper();
                                #endregion
                            }

                            #endregion

                            #region measure contact power (Pout1)
                            if (StopOnFail.TestFail == false)
                            {
                                if (!_TunePwr_TX1)
                                {
                                    Eq.Site[0]._EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                    Eq.Site[0]._EqVST.rfsgSession.Initiate();
                                    StopOnFail.TestFail = true;     //init to fail state as default
                                    DelayMs(_RdPwr_Delay);
                                    R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                    R_Pin1 = Math.Round(SGTargetPin - papr_dB + totalInputLoss, 3);
                                    if (Math.Abs(_Pout1 - R_Pout1) <= (tolerancePwr + 3.5))
                                    {
                                        pwrSearch = true;
                                        StopOnFail.TestFail = false;
                                    }
                                }
                                else
                                {
                                    do
                                    {
                                        StopOnFail.TestFail = true;     //init to fail state as default
                                        R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = SGTargetPin - papr_dB + totalInputLoss;

                                        if (Math.Abs(_Pout1 - R_Pout1) >= tolerancePwr)
                                        {
                                            if ((Index == 0) && (SGTargetPin < _SG1MaxPwr))   //preset to initial target power for 1st measurement count
                                            {
                                                Eq.Site[0]._EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;
                                                Eq.Site[0]._EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                                Eq.Site[0]._EqVST.rfsgSession.Initiate();
                                                DelayMs(_RdPwr_Delay);
                                                R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                            }

                                            SGTargetPin = SGTargetPin + (_Pout1 - R_Pout1);

                                            if (SGTargetPin < _SG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                            {
                                                Eq.Site[0]._EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;
                                                DelayMs(_RdPwr_Delay);
                                            }

                                            if (SGTargetPin > _SG1MaxPwr)      //if input sig gen exit limit , exit pwr search loop
                                            {
                                                SGTargetPin = _Pin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            pwrSearch = true;
                                            SGPowerLevel = SGTargetPin;
                                            StopOnFail.TestFail = false;
                                            break;
                                        }

                                        Index++;
                                    }
                                    while (Index < 10);     // max power search loop
                                }
                            }

                            //total test time for each parameter will include the soak time
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)_Estimate_TestTime)
                            {
                                syncTest_Delay = (long)_Estimate_TestTime - paramTestTime;
                                SoakTime = syncTest_Delay * 1e-3;       //convert to second
                            }
                            else
                            {
                                SoakTime = 0;                //no soak required if power servo longer than expected total test time                                                        
                            }

                            #endregion

                            if (pwrSearch)
                            {
                                #region Measure VST
                                R_NF1_Freq = -888;
                                R_NF1_Ampl = 888;

                                //Measure SMU current
                                MeasSMU = _SMUMeasCh.Split(',');
                                if (_Test_SMU)
                                {
                                    DelayMs(_RdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (_SMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }

                                Eq.Site[0]._EqVST.rfsgSession.Abort();         //stop power servo script

                                Stopwatch timer1 = new Stopwatch();
                                Stopwatch timer2 = new Stopwatch();
                                timer1.Restart();

                                #region Config VST and Measure Noise

                                #region decode and re-arrange multiple bandwidth (Ascending)
                                int bw_cnt = 0;
                                double[] tmpRBW_Hz = Array.ConvertAll(_PXI_MultiRBW.Split(','), double.Parse);  //split and convert string to double array
                                double[] multiRBW_Hz = new double[tmpRBW_Hz.Length];

                                Array.Sort(tmpRBW_Hz);
                                foreach (double key in tmpRBW_Hz)
                                {
                                    multiRBW_Hz[bw_cnt] = Convert.ToDouble(key);
                                    bw_cnt++;
                                }

                                multiRBW_cnt = multiRBW_Hz.Length;
                                RBW_Hz = multiRBW_Hz[multiRBW_cnt - 1];   //the largest RBW is the last in array 
                                #endregion

                                //if (SoakTime <= 0)
                                if (_Estimate_TestTime <= 0)    //assume soaktime when 0 or less (set from TCF) does not required soak sweep
                                {
                                    preSoakSweep = false;
                                    preSoakSweepTemp = 0; //to indicate if another sweep should be done
                                }

                                Eq.Site[0]._EqVST.ConfigureVSTDuringTest(new LibEqmtDriver.NF_VST.NF_NiPXI_VST.Config(NumberOfRuns + preSoakSweepTemp, _TX1Band, _Modulation, _WaveFormName,
                                _StartTXFreq1 * 1e6, _StopTXFreq1 * 1e6, _StepTXFreq1 * 1e6, (_DwellT1 - 0.03) / 1000, _StartRXFreq1 * 1e6, _StopRXFreq1 * 1e6, _StepRXFreq1 * 1e6,
                                SGPowerLevel, SAReferenceLevel, SoakTime, SoakFrequency, RBW_Hz, vBW_Hz, preSoakSweep, _PXI_Multiplier_RXIQRate, multiRBW_Hz));

                                LibEqmtDriver.NF_VST.S_MultiRBW_Data[] MultiRBW_RsltMultiTrace = new LibEqmtDriver.NF_VST.S_MultiRBW_Data[multiRBW_cnt];

                                timer1.Stop();
                                timer2.Restart();

                                MultiRBW_RsltMultiTrace = Eq.Site[0]._EqVST.Measure_VST(sweepPts);

                                #endregion
                                timer2.Stop();

                                long time1 = timer1.ElapsedMilliseconds;
                                long time2 = timer2.ElapsedMilliseconds;

                                #region Sort and Store Trace Data
                                //Store multi trace from PXI to global array
                                for (rbw_counter = 0; rbw_counter < multiRBW_cnt; rbw_counter++)
                                {
                                    for (int n = 0; n < NumberOfRuns + preSoakSweepTemp; n++)
                                    {
                                        //temp trace array storage use for MAX , MIN etc calculation 
                                        PXITrace[TestCount].Enable = true;
                                        PXITrace[TestCount].SoakSweep = preSoakSweep;
                                        PXITrace[TestCount].TestNumber = _TestNum;
                                        PXITrace[TestCount].TraceCount = NumberOfRuns + preSoakSweepTemp;
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].NoPoints = sweepPts;
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].RBW_Hz = MultiRBW_RsltMultiTrace[rbw_counter].RBW_Hz;
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].FreqMHz = new double[sweepPts];
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].Ampl = new double[sweepPts];
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].Result_Header = _TestParaName;
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].MXA_No = "PXI_Trace";

                                        PXITraceRaw[TestCount].Multi_Trace[rbw_counter][n].FreqMHz = new double[sweepPts];
                                        PXITraceRaw[TestCount].Multi_Trace[rbw_counter][n].Ampl = new double[sweepPts];

                                        for (istep = 0; istep < sweepPts; istep++)
                                        {
                                            if (istep == 0)
                                                tmpRXFreqHz = _StartRXFreq1 * 1e6;
                                            else
                                                tmpRXFreqHz = tmpRXFreqHz + (stepFreqMHz * 1e6);

                                            if (usePrevRslt)    //PXI trace result minus out the DUT LNA Gain from previous result
                                            {
                                                PXITrace[TestCount].Multi_Trace[rbw_counter][n].FreqMHz[istep] = Math.Round(tmpRXFreqHz / 1e6, 3);
                                                PXITrace[TestCount].Multi_Trace[rbw_counter][n].Ampl[istep] = Math.Round(MultiRBW_RsltMultiTrace[rbw_counter].rsltTrace[istep, n], 3) - prevRslt;
                                            }
                                            else
                                            {
                                                PXITrace[TestCount].Multi_Trace[rbw_counter][n].FreqMHz[istep] = Math.Round(tmpRXFreqHz / 1e6, 3);
                                                PXITrace[TestCount].Multi_Trace[rbw_counter][n].Ampl[istep] = Math.Round(MultiRBW_RsltMultiTrace[rbw_counter].rsltTrace[istep, n], 3);
                                            }

                                            //Store Raw Trace Data to PXITraceRaw Array - Only actual data read from SA (not use in other than Save_PXI_TraceRaw function
                                            PXITraceRaw[TestCount].Multi_Trace[rbw_counter][n].FreqMHz[istep] = Math.Round(tmpRXFreqHz / 1e6, 3);
                                            PXITraceRaw[TestCount].Multi_Trace[rbw_counter][n].Ampl[istep] = Math.Round(MultiRBW_RsltMultiTrace[rbw_counter].rsltTrace[istep, n], 3);
                                        }
                                    }

                                    Save_PXI_TraceRaw(_TestParaName, _TestUsePrev, _Save_MXATrace, rbw_counter, multiRBW_Hz[rbw_counter]);
                                }

                                #endregion

                                #region Test Parameter Log

                                //Get average pathloss base on start and stop freq with 1MHz step freq
                                count = Convert.ToInt16((_StopRXFreq1 - _StartRXFreq1) / 1);
                                _RXFreq = _StartRXFreq1;
                                for (int i = 0; i <= count; i++)
                                {
                                    ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                    tmpRxLoss = Math.Round(tmpRxLoss + (float)_LossOutputPathRX1, 3);   //need to use round function because of C# float and double floating point bug/error
                                    _RXFreq = Convert.ToSingle(Math.Round(_RXFreq + 1, 3));             //need to use round function because of C# float and double floating point bug/error
                                }
                                tmpAveRxLoss = tmpRxLoss / (count + 1);
                                _LossOutputPathRX1 = tmpAveRxLoss;

                                for (rbw_counter = 0; rbw_counter < multiRBW_cnt; rbw_counter++)
                                {
                                    rbwParamName = null;
                                    rbwParamName = "_" + Math.Abs(multiRBW_Hz[rbw_counter] / 1e6).ToString() + "MHz";

                                    string[] tmpParamName;
                                    string tmp1stHeader = null;
                                    string tmp2ndHeader = null;
                                    tmpParamName = _TestParaName.Split('_');

                                    for (int i = 0; i < tmpParamName.Length; i++)
                                    {
                                        if (i > 0)
                                            tmp2ndHeader = tmp2ndHeader + "_" + tmpParamName[i];
                                    }

                                    //Sort out test result for all traces and Add test result
                                    for (int i = 0; i < PXITrace[TestCount].TraceCount; i++)
                                    {
                                        R_NF1_Freq = -888;
                                        R_NF1_Ampl = 888;
                                        double tmpNFAmpl = 999;
                                        int tmpIndex = 0;
                                        _TestParaName = "NF" + (i + 1) + tmp2ndHeader;

                                        switch (_Search_Method.ToUpper())
                                        {
                                            case "MAX":
                                                tmpNFAmpl = PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl.Max();
                                                tmpIndex = Array.IndexOf(PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl, tmpNFAmpl);     //return index of max value
                                                R_NF1_Ampl = Math.Round(tmpNFAmpl, 3);
                                                R_NF1_Freq = PXITrace[TestCount].Multi_Trace[rbw_counter][i].FreqMHz[tmpIndex];
                                                break;

                                            case "MIN":
                                                tmpNFAmpl = PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl.Min();
                                                tmpIndex = Array.IndexOf(PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl, tmpNFAmpl);     //return index of max value
                                                R_NF1_Ampl = Math.Round(tmpNFAmpl, 3);
                                                R_NF1_Freq = PXITrace[TestCount].Multi_Trace[rbw_counter][i].FreqMHz[tmpIndex];
                                                break;

                                            default:
                                                MessageBox.Show("Test Parameter : " + _TestParam + "(" + _Search_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                                break;
                                        }

                                        R_NF1_Ampl = R_NF1_Ampl - _LossOutputPathRX1 - tbOutputLoss;

                                        if (i == 0)
                                        {
                                            if (_Test_Pin1)
                                            {
                                                ResultBuilder.BuildResults(ref results, _TestParaName + rbwParamName + "_Pin1", "dBm", R_Pin1);
                                            }
                                            if (_Test_Pout1)
                                            {
                                                ResultBuilder.BuildResults(ref results, _TestParaName + rbwParamName + "_Pout1", "dBm", R_Pout1);
                                            }
                                            if (_Test_NF1)
                                            {
                                                ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                                ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                            }
                                            if (_Test_SMU)
                                            {
                                                MeasSMU = _SMUMeasCh.Split(',');
                                                for (int j = 0; j < MeasSMU.Count(); j++)
                                                {
                                                    ResultBuilder.BuildResults(ref results, _TestParaName + rbwParamName + "_" + R_SMULabel_ICh[Convert.ToInt16(MeasSMU[j])], "A", R_SMU_ICh[Convert.ToInt16(MeasSMU[j])]);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (_Test_NF1)
                                            {
                                                ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                                ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                            }
                                        }
                                    }

                                    //Force test flag to false to ensure no repeated test data
                                    //because we add to string builder upfront for PXI due to data reported base on number of sweep
                                    _Test_Pin1 = false;
                                    _Test_Pout1 = false;
                                    _Test_SMU = false;
                                }

                                //Force test flag to false to ensure no repeated test data
                                //because we add to string builder upfront for PXI due to data reported base on number of sweep
                                _Test_NF1 = false;
                                #endregion

                                #endregion
                            }
                            else                                            //if fail power out search , set data to default
                            {
                                #region If Power Servo Fail Routine
                                SGTargetPin = _Pin1 - totalInputLoss;       //reset the initial power setting to default
                                R_NF1_Freq = -999;
                                R_NF1_Ampl = 999;

                                //if (SoakTime <= 0)
                                if (_Estimate_TestTime <= 0)    //assume soaktime when 0 or less (set from TCF) does not required soak sweep
                                {
                                    preSoakSweep = false;
                                    preSoakSweepTemp = 0; //to indicate if another sweep should be done
                                }

                                #region measure SMU current - during fail power servo
                                //Measure SMU current
                                MeasSMU = _SMUMeasCh.Split(',');
                                if (_Test_SMU)
                                {
                                    DelayMs(_RdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (_SMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }

                                Eq.Site[0]._EqVST.rfsgSession.Abort();         //stop power servo script
                                #endregion

                                #region decode re-arrange multiple bandwidth (Ascending)
                                int bw_cnt = 0;
                                double[] tmpRBW_Hz = Array.ConvertAll(_PXI_MultiRBW.Split(','), double.Parse);  //split and convert string to double array
                                double[] multiRBW_Hz = new double[tmpRBW_Hz.Length];

                                Array.Sort(tmpRBW_Hz);
                                foreach (double key in tmpRBW_Hz)
                                {
                                    multiRBW_Hz[bw_cnt] = Convert.ToDouble(key);
                                    bw_cnt++;
                                }

                                multiRBW_cnt = multiRBW_Hz.Length;
                                RBW_Hz = multiRBW_Hz[multiRBW_cnt - 1];   //the largest RBW is the last in array 
                                #endregion

                                for (rbw_counter = 0; rbw_counter < multiRBW_cnt; rbw_counter++)
                                {
                                    rbwParamName = null;
                                    rbwParamName = "_" + Math.Abs(multiRBW_Hz[rbw_counter] / 1e6).ToString() + "MHz";

                                    //Store multi trace from PXI to global array
                                    for (int n = 0; n < NumberOfRuns + preSoakSweepTemp; n++)
                                    {
                                        //temp trace array storage use for MAX , MIN etc calculation 
                                        PXITrace[TestCount].Enable = true;
                                        PXITrace[TestCount].SoakSweep = preSoakSweep;
                                        PXITrace[TestCount].TestNumber = _TestNum;
                                        PXITrace[TestCount].TraceCount = NumberOfRuns + preSoakSweepTemp;
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].NoPoints = sweepPts;
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].RBW_Hz = multiRBW_Hz[rbw_counter];
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].FreqMHz = new double[sweepPts];
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].Ampl = new double[sweepPts];
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].Result_Header = _TestParaName;
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].MXA_No = "PXI_Trace";

                                        for (istep = 0; istep < sweepPts; istep++)
                                        {
                                            if (istep == 0)
                                                tmpRXFreqHz = _StartRXFreq1 * 1e6;
                                            else
                                                tmpRXFreqHz = tmpRXFreqHz + (stepFreqMHz * 1e6);

                                            PXITrace[TestCount].Multi_Trace[rbw_counter][n].FreqMHz[istep] = Math.Round(tmpRXFreqHz / 1e6, 3);
                                            PXITrace[TestCount].Multi_Trace[rbw_counter][n].Ampl[istep] = 999;
                                        }
                                    }

                                    #region Test Parameter Log
                                    string[] tmpParamName;
                                    string tmp1stHeader = null;
                                    string tmp2ndHeader = null;
                                    tmpParamName = _TestParaName.Split('_');
                                    for (int i = 0; i < tmpParamName.Length; i++)
                                    {
                                        if (i > 0)
                                            tmp2ndHeader = tmp2ndHeader + "_" + tmpParamName[i];
                                    }

                                    //Sort out test result for all traces and Add test result
                                    for (int i = 0; i < PXITrace[TestCount].TraceCount; i++)
                                    {
                                        R_NF1_Freq = -888;
                                        R_NF1_Ampl = 888;
                                        double tmpNFAmpl = 999;
                                        int tmpIndex = 0;
                                        _TestParaName = "NF" + (i + 1) + tmp2ndHeader;

                                        switch (_Search_Method.ToUpper())
                                        {
                                            case "MAX":
                                                tmpNFAmpl = PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl.Max();
                                                tmpIndex = Array.IndexOf(PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl, tmpNFAmpl);     //return index of max value
                                                R_NF1_Ampl = Math.Round(tmpNFAmpl, 3);
                                                R_NF1_Freq = PXITrace[TestCount].Multi_Trace[rbw_counter][i].FreqMHz[tmpIndex];
                                                break;

                                            case "MIN":
                                                tmpNFAmpl = PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl.Min();
                                                tmpIndex = Array.IndexOf(PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl, tmpNFAmpl);     //return index of max value
                                                R_NF1_Ampl = Math.Round(tmpNFAmpl, 3);
                                                R_NF1_Freq = PXITrace[TestCount].Multi_Trace[rbw_counter][i].FreqMHz[tmpIndex];
                                                break;

                                            default:
                                                MessageBox.Show("Test Parameter : " + _TestParam + "(" + _Search_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                                break;
                                        }

                                        if (i == 0)
                                        {
                                            if (_Test_Pin1)
                                            {
                                                ResultBuilder.BuildResults(ref results, _TestParaName + rbwParamName + "_Pin1", "dBm", R_Pin1);
                                            }
                                            if (_Test_Pout1)
                                            {
                                                ResultBuilder.BuildResults(ref results, _TestParaName + rbwParamName + "_Pout1", "dBm", R_Pout1);
                                            }
                                            if (_Test_NF1)
                                            {
                                                ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                                ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                            }
                                            if (_Test_SMU)
                                            {
                                                MeasSMU = _SMUMeasCh.Split(',');
                                                for (int j = 0; j < MeasSMU.Count(); j++)
                                                {
                                                    ResultBuilder.BuildResults(ref results, _TestParaName + rbwParamName + "_" + R_SMULabel_ICh[Convert.ToInt16(MeasSMU[j])], "A", R_SMU_ICh[Convert.ToInt16(MeasSMU[j])]);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (_Test_NF1)
                                            {
                                                ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                                ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                            }
                                        }
                                    }

                                    //Force test flag to false to ensure no repeated test data
                                    //because we add to string builder upfront for PXI due to data reported base on number of sweep
                                    _Test_Pin1 = false;
                                    _Test_Pout1 = false;
                                    _Test_SMU = false;
                                    #endregion
                                }

                                //Force test flag to false to ensure no repeated test data
                                //because we add to string builder upfront for PXI due to data reported base on number of sweep
                                _Test_NF1 = false;
                                #endregion
                            }


                            //for test time checking
                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            #endregion
                            break;

                        case "PXI_NF_FIX_NMAX":
                            // This function is similar to PXI_NF_NONCA_NDIAG, but the TX frequency range is fixed while RX band is swept

                            #region PXI NF FIX NMAX
                            //NOTE: Some of these inputs may have to be read from input-excel or defined elsewhere
                            //Variable use in VST Measure Function
                            NumberOfRuns = 5;
                            SGPowerLevel = -18;// -18 CDMA dBm //-20 LTE dBm  
                            SAReferenceLevel = -20;
                            SoakTime = 450e-3;
                            SoakFrequency = _StartTXFreq1 * 1e6;
                            vBW_Hz = 300;
                            RBW_Hz = 1e6;
                            preSoakSweep = true; //to indicate if another sweep should be done **MAKE SURE TO SPLIT OUTPUT ARRAY**
                            preSoakSweepTemp = preSoakSweep == true ? 1 : 0; //to indicate if another sweep should be done
                            stepFreqMHz = 0.1;
                            tmpRXFreqHz = _StartRXFreq1 * 1e6;
                            sweepPts = (Convert.ToInt32((_StopRXFreq1 - _StartRXFreq1) / stepFreqMHz)) + 1; //Determine sweep points according to RX frequency range
                            //----

                            status = false;
                            pwrSearch = false;
                            Index = 0;
                            tx1_span = 0;
                            tx1_noPoints = 0;
                            rx1_span = 0;
                            rx1_cntrfreq = 0;
                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(_PoutTolerance);

                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                                tolerancePwr = 0.5;

                            if (_PXI_NoOfSweep <= 0)                //check the number of sweep for pxi, set to default if user forget to keyin in excel
                                NumberOfRuns = 1;
                            else
                                NumberOfRuns = _PXI_NoOfSweep;

                            //use for searching previous result - to get the DUT LNA gain from previous result
                            if (Convert.ToInt16(_TestUsePrev) > 0)
                            {
                                usePrevRslt = true;
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                prevRslt = Math.Round(ReportRslt(_TestUsePrev, resultTag), 3);
                            }

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region PowerSensor Offset, MXG and MXA1 configuration

                            //Calculate PAPR offset for PXI SG
                            //LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE modulationType;
                            modulationType = (LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE), _WaveFormName.ToUpper());
                            modArrayNo = (int)Enum.Parse(modulationType.GetType(), modulationType.ToString()); // to get the int value from System.Enum
                            papr_dB = Math.Round(LibEqmtDriver.NF_VST.NF_VSTDriver.SignalType[modArrayNo].SG_papr_dB, 3);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((_StopTXFreq1 - _StartTXFreq1) / _StepTXFreq1);
                            _TXFreq = _StartTXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, _TXFreq, ref _LossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)_LossInputPathSG1;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _TXFreq, ref _LossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)_LossCouplerPath;
                                _TXFreq = _TXFreq + _StepTXFreq1;
                            }

                            tmpAveInputLoss = tmpInputLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            //change PowerSensor, MXG setting
                            Eq.Site[0]._EqPwrMeter.SetOffset(1, totalOutputLoss);
                            //SGTargetPin = papr_dB - _Pin1 - totalInputLoss;   //wrong equation - Shaz 22/10/2019
                            SGTargetPin = _Pin1 - totalInputLoss + papr_dB;

                            #region MXA Setup

                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", _SwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);

                            SAReferenceLevel = myUtility.MXA_Setting.RefLevel;
                            vBW_Hz = myUtility.MXA_Setting.VBW;
                            //RBW_Hz = myUtility.MXA_Setting.RBW;

                            #endregion

                            //if (PreviousMXAMode != _SwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                //generate modulated signal
                                string TempWaveFormName = _WaveFormName.Replace("_", "");
                                string Script =
                                         "script powerServo\r\n"
                                       + "repeat forever\r\n"
                                       + "generate Signal" + TempWaveFormName + "\r\n"
                                       + "end repeat\r\n"
                                       + "end script";
                                try
                                {
                                    Eq.Site[0]._EqVST.rfsgSession.Arb.Scripting.WriteScript(Script);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message);
                                }
                                Eq.Site[0]._EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                Eq.Site[0]._EqVST.rfsgSession.RF.Frequency = _StartTXFreq1 * 1e6;
                                Eq.Site[0]._EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;

                                //Need to ensure that SG_IQRate re-define , because RX_CONTACT routine has overwritten the initialization data
                                Eq.Site[0]._EqVST.Get_s_SignalType(_Modulation, _WaveFormName, out SG_IQRate);
                                Eq.Site[0]._EqVST.rfsgSession.Arb.IQRate = SG_IQRate;

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = _SwBand.ToUpper();
                                #endregion
                            }

                            #endregion

                            #region measure contact power (Pout1)
                            if (StopOnFail.TestFail == false)
                            {
                                if (!_TunePwr_TX1)
                                {
                                    Eq.Site[0]._EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                    Eq.Site[0]._EqVST.rfsgSession.Initiate();
                                    StopOnFail.TestFail = true;     //init to fail state as default
                                    DelayMs(_RdPwr_Delay);
                                    R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                    R_Pin1 = Math.Round(SGTargetPin - papr_dB + totalInputLoss, 3);
                                    if (Math.Abs(_Pout1 - R_Pout1) <= (tolerancePwr + 3.5))
                                    {
                                        pwrSearch = true;
                                        StopOnFail.TestFail = false;
                                    }
                                }
                                else
                                {
                                    do
                                    {
                                        StopOnFail.TestFail = true;     //init to fail state as default
                                        R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = SGTargetPin - papr_dB + totalInputLoss;

                                        if (Math.Abs(_Pout1 - R_Pout1) >= tolerancePwr)
                                        {
                                            if ((Index == 0) && (SGTargetPin < _SG1MaxPwr))   //preset to initial target power for 1st measurement count
                                            {
                                                Eq.Site[0]._EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;
                                                Eq.Site[0]._EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                                Eq.Site[0]._EqVST.rfsgSession.Initiate();
                                                DelayMs(_RdPwr_Delay);
                                                R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                            }

                                            SGTargetPin = SGTargetPin + (_Pout1 - R_Pout1);

                                            if (SGTargetPin < _SG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                            {
                                                Eq.Site[0]._EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;
                                                DelayMs(_RdPwr_Delay);
                                            }

                                            if (SGTargetPin > _SG1MaxPwr)      //if input sig gen exit limit , exit pwr search loop
                                            {
                                                SGTargetPin = _Pin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            pwrSearch = true;
                                            SGPowerLevel = SGTargetPin;
                                            StopOnFail.TestFail = false;
                                            break;
                                        }

                                        Index++;
                                    }
                                    while (Index < 10);     // max power search loop
                                }
                            }

                            //total test time for each parameter will include the soak time
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)_Estimate_TestTime)
                            {
                                syncTest_Delay = (long)_Estimate_TestTime - paramTestTime;
                                SoakTime = syncTest_Delay * 1e-3;       //convert to second
                            }
                            else
                            {
                                SoakTime = 0;                //no soak required if power servo longer than expected total test time                                                        
                            }

                            #endregion

                            if (pwrSearch)
                            {
                                #region Measure VST
                                R_NF1_Freq = -888;
                                R_NF1_Ampl = 888;

                                //Measure SMU current
                                MeasSMU = _SMUMeasCh.Split(',');
                                if (_Test_SMU)
                                {
                                    DelayMs(_RdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (_SMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }

                                Eq.Site[0]._EqVST.rfsgSession.Abort();         //stop power servo script

                                Stopwatch timer1 = new Stopwatch();
                                Stopwatch timer2 = new Stopwatch();
                                timer1.Restart();

                                int NumberofFixedTXRecords = (int)((_StopTXFreq1 - _StartTXFreq1) / _StepTXFreq1) + 1; //Get number of records according to TX freq range and step
                                for (int m = 0; m < NumberofFixedTXRecords; m++)
                                {
                                    #region Config VST and Measure Noise

                                    #region decode and re-arrange multiple bandwidth (Ascending)
                                    int bw_cnt = 0;
                                    double[] tmpRBW_Hz = Array.ConvertAll(_PXI_MultiRBW.Split(','), double.Parse);  //split and convert string to double array
                                    double[] multiRBW_Hz = new double[tmpRBW_Hz.Length];

                                    Array.Sort(tmpRBW_Hz);
                                    foreach (double key in tmpRBW_Hz)
                                    {
                                        multiRBW_Hz[bw_cnt] = Convert.ToDouble(key);
                                        bw_cnt++;
                                    }

                                    multiRBW_cnt = multiRBW_Hz.Length;
                                    RBW_Hz = multiRBW_Hz[multiRBW_cnt - 1];   //the largest RBW is the last in array 
                                    #endregion

                                    //if (SoakTime <= 0)
                                    if (_Estimate_TestTime <= 0)    //assume soaktime when 0 or less (set from TCF) does not required soak sweep
                                    {
                                        preSoakSweep = false;
                                        preSoakSweepTemp = 0; //to indicate if another sweep should be done
                                    }

                                    double FixedTXFreq = _StartTXFreq1 + m * _StepTXFreq1;

                                    Eq.Site[0]._EqVST.ConfigureVSTDuringTest_FixedTX(new LibEqmtDriver.NF_VST.NF_NiPXI_VST.Config(NumberOfRuns + preSoakSweepTemp, _TX1Band, _Modulation, _WaveFormName,
                                    FixedTXFreq * 1e6, FixedTXFreq * 1e6, _StepTXFreq1 * 1e6, (_DwellT1 - 0.03) / 1000, _StartRXFreq1 * 1e6, _StopRXFreq1 * 1e6, _StepRXFreq1 * 1e6,
                                    SGPowerLevel, SAReferenceLevel, SoakTime, SoakFrequency, RBW_Hz, vBW_Hz, preSoakSweep, _PXI_Multiplier_RXIQRate, multiRBW_Hz));

                                    LibEqmtDriver.NF_VST.S_MultiRBW_Data[] MultiRBW_RsltMultiTrace = new LibEqmtDriver.NF_VST.S_MultiRBW_Data[multiRBW_cnt];

                                    timer1.Stop();
                                    timer2.Restart();

                                    MultiRBW_RsltMultiTrace = Eq.Site[0]._EqVST.Measure_VST(sweepPts);

                                    #endregion
                                    timer2.Stop();

                                    long time1 = timer1.ElapsedMilliseconds;
                                    long time2 = timer2.ElapsedMilliseconds;

                                    #region Sort and Store Trace Data
                                    //Store multi trace from PXI to global array
                                    for (rbw_counter = 0; rbw_counter < multiRBW_cnt; rbw_counter++)
                                    {
                                        for (int n = 0; n < NumberOfRuns + preSoakSweepTemp; n++)
                                        {
                                            //temp trace array storage use for MAX , MIN etc calculation 
                                            PXITrace[TestCount].Enable = true;
                                            PXITrace[TestCount].SoakSweep = preSoakSweep;
                                            PXITrace[TestCount].TestNumber = _TestNum;
                                            PXITrace[TestCount].TraceCount = NumberOfRuns + preSoakSweepTemp;
                                            PXITrace[TestCount].Multi_Trace[rbw_counter][n].NoPoints = sweepPts;
                                            PXITrace[TestCount].Multi_Trace[rbw_counter][n].RBW_Hz = MultiRBW_RsltMultiTrace[rbw_counter].RBW_Hz;
                                            PXITrace[TestCount].Multi_Trace[rbw_counter][n].FreqMHz = new double[sweepPts];
                                            PXITrace[TestCount].Multi_Trace[rbw_counter][n].Ampl = new double[sweepPts];
                                            PXITrace[TestCount].Multi_Trace[rbw_counter][n].Result_Header = _TestParaName;
                                            PXITrace[TestCount].Multi_Trace[rbw_counter][n].MXA_No = "PXI_Trace";

                                            PXITraceRaw[TestCount].Multi_Trace[rbw_counter][n].FreqMHz = new double[sweepPts];
                                            PXITraceRaw[TestCount].Multi_Trace[rbw_counter][n].Ampl = new double[sweepPts];

                                            for (istep = 0; istep < sweepPts; istep++)
                                            {
                                                if (istep == 0)
                                                    tmpRXFreqHz = _StartRXFreq1 * 1e6;
                                                else
                                                    tmpRXFreqHz = tmpRXFreqHz + (stepFreqMHz * 1e6);

                                                if (usePrevRslt)    //PXI trace result minus out the DUT LNA Gain from previous result
                                                {
                                                    PXITrace[TestCount].Multi_Trace[rbw_counter][n].FreqMHz[istep] = Math.Round(tmpRXFreqHz / 1e6, 3);
                                                    PXITrace[TestCount].Multi_Trace[rbw_counter][n].Ampl[istep] = Math.Round(MultiRBW_RsltMultiTrace[rbw_counter].rsltTrace[istep, n], 3) - prevRslt;
                                                }
                                                else
                                                {
                                                    PXITrace[TestCount].Multi_Trace[rbw_counter][n].FreqMHz[istep] = Math.Round(tmpRXFreqHz / 1e6, 3);
                                                    PXITrace[TestCount].Multi_Trace[rbw_counter][n].Ampl[istep] = Math.Round(MultiRBW_RsltMultiTrace[rbw_counter].rsltTrace[istep, n], 3);
                                                }

                                                //Store Raw Trace Data to PXITraceRaw Array - Only actual data read from SA (not use in other than Save_PXI_TraceRaw function
                                                PXITraceRaw[TestCount].Multi_Trace[rbw_counter][n].FreqMHz[istep] = Math.Round(tmpRXFreqHz / 1e6, 3);
                                                PXITraceRaw[TestCount].Multi_Trace[rbw_counter][n].Ampl[istep] = Math.Round(MultiRBW_RsltMultiTrace[rbw_counter].rsltTrace[istep, n], 3);
                                            }
                                        }

                                        Save_PXI_TraceRaw(_TestParaName + "_FixedTX_" + FixedTXFreq.ToString() + "M", _TestUsePrev, _Save_MXATrace, rbw_counter, multiRBW_Hz[rbw_counter]);
                                    }

                                    #endregion

                                    #region Test Parameter Log

                                    //Get average pathloss base on start and stop freq with 1MHz step freq
                                    count = Convert.ToInt16((_StopRXFreq1 - _StartRXFreq1) / 1);
                                    _RXFreq = _StartRXFreq1;
                                    for (int i = 0; i <= count; i++)
                                    {
                                        ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                        tmpRxLoss = Math.Round(tmpRxLoss + (float)_LossOutputPathRX1, 3);   //need to use round function because of C# float and double floating point bug/error
                                        _RXFreq = Convert.ToSingle(Math.Round(_RXFreq + 1, 3));             //need to use round function because of C# float and double floating point bug/error
                                    }
                                    tmpAveRxLoss = tmpRxLoss / (count + 1);
                                    _LossOutputPathRX1 = tmpAveRxLoss;

                                    for (rbw_counter = 0; rbw_counter < multiRBW_cnt; rbw_counter++)
                                    {
                                        rbwParamName = null;
                                        rbwParamName = "_" + Math.Abs(multiRBW_Hz[rbw_counter] / 1e6).ToString() + "MHz";

                                        string[] tmpParamName;
                                        string tmp1stHeader = null;
                                        string tmp2ndHeader = null;
                                        tmpParamName = _TestParaName.Split('_');

                                        for (int i = 0; i < tmpParamName.Length; i++)
                                        {
                                            if (i > 0)
                                                tmp2ndHeader = tmp2ndHeader + "_" + tmpParamName[i];
                                        }

                                        //Sort out test result for all traces and Add test result
                                        for (int i = 0; i < PXITrace[TestCount].TraceCount; i++)
                                        {
                                            R_NF1_Freq = -888;
                                            R_NF1_Ampl = 888;
                                            double tmpNFAmpl = 999;
                                            int tmpIndex = 0;
                                            _TestParaName = "NF" + (i + 1) + tmp2ndHeader;

                                            switch (_Search_Method.ToUpper())
                                            {
                                                case "MAX":
                                                    tmpNFAmpl = PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl.Max();
                                                    tmpIndex = Array.IndexOf(PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl, tmpNFAmpl);     //return index of max value
                                                    R_NF1_Ampl = Math.Round(tmpNFAmpl, 3);
                                                    R_NF1_Freq = PXITrace[TestCount].Multi_Trace[rbw_counter][i].FreqMHz[tmpIndex];
                                                    break;

                                                case "MIN":
                                                    tmpNFAmpl = PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl.Min();
                                                    tmpIndex = Array.IndexOf(PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl, tmpNFAmpl);     //return index of max value
                                                    R_NF1_Ampl = Math.Round(tmpNFAmpl, 3);
                                                    R_NF1_Freq = PXITrace[TestCount].Multi_Trace[rbw_counter][i].FreqMHz[tmpIndex];
                                                    break;

                                                default:
                                                    MessageBox.Show("Test Parameter : " + _TestParam + "(" + _Search_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                                    break;
                                            }

                                            R_NF1_Ampl = R_NF1_Ampl - _LossOutputPathRX1 - tbOutputLoss;

                                            if (i == 0)
                                            {
                                                if (_Test_Pin1)
                                                {
                                                    ResultBuilder.BuildResults(ref results, _TestParaName + rbwParamName + "_Pin1", "dBm", R_Pin1);
                                                }
                                                if (_Test_Pout1)
                                                {
                                                    ResultBuilder.BuildResults(ref results, _TestParaName + rbwParamName + "_Pout1", "dBm", R_Pout1);
                                                }
                                                if (_Test_NF1)
                                                {
                                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                                }
                                                if (_Test_SMU)
                                                {
                                                    MeasSMU = _SMUMeasCh.Split(',');
                                                    for (int j = 0; j < MeasSMU.Count(); j++)
                                                    {
                                                        ResultBuilder.BuildResults(ref results, _TestParaName + rbwParamName + "_" + R_SMULabel_ICh[Convert.ToInt16(MeasSMU[j])], "A", R_SMU_ICh[Convert.ToInt16(MeasSMU[j])]);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (_Test_NF1)
                                                {
                                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                                }
                                            }
                                        }

                                        //Force test flag to false to ensure no repeated test data
                                        //because we add to string builder upfront for PXI due to data reported base on number of sweep
                                        _Test_Pin1 = false;
                                        _Test_Pout1 = false;
                                        _Test_SMU = false;
                                    }

                                    //Force test flag to false to ensure no repeated test data
                                    //because we add to string builder upfront for PXI due to data reported base on number of sweep
                                    _Test_NF1 = false;
                                    #endregion
                                }
                                #endregion
                            }
                            else                                            //if fail power out search , set data to default
                            {
                                #region If Power Servo Fail Routine
                                SGTargetPin = _Pin1 - totalInputLoss;       //reset the initial power setting to default
                                R_NF1_Freq = -999;
                                R_NF1_Ampl = 999;

                                //if (SoakTime <= 0)
                                if (_Estimate_TestTime <= 0)    //assume soaktime when 0 or less (set from TCF) does not required soak sweep
                                {
                                    preSoakSweep = false;
                                    preSoakSweepTemp = 0; //to indicate if another sweep should be done
                                }

                                #region measure SMU current - during fail power servo
                                //Measure SMU current
                                MeasSMU = _SMUMeasCh.Split(',');
                                if (_Test_SMU)
                                {
                                    DelayMs(_RdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (_SMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }

                                Eq.Site[0]._EqVST.rfsgSession.Abort();         //stop power servo script
                                #endregion

                                #region decode re-arrange multiple bandwidth (Ascending)
                                int bw_cnt = 0;
                                double[] tmpRBW_Hz = Array.ConvertAll(_PXI_MultiRBW.Split(','), double.Parse);  //split and convert string to double array
                                double[] multiRBW_Hz = new double[tmpRBW_Hz.Length];

                                Array.Sort(tmpRBW_Hz);
                                foreach (double key in tmpRBW_Hz)
                                {
                                    multiRBW_Hz[bw_cnt] = Convert.ToDouble(key);
                                    bw_cnt++;
                                }

                                multiRBW_cnt = multiRBW_Hz.Length;
                                RBW_Hz = multiRBW_Hz[multiRBW_cnt - 1];   //the largest RBW is the last in array 
                                #endregion

                                for (rbw_counter = 0; rbw_counter < multiRBW_cnt; rbw_counter++)
                                {
                                    rbwParamName = null;
                                    rbwParamName = "_" + Math.Abs(multiRBW_Hz[rbw_counter] / 1e6).ToString() + "MHz";

                                    //Store multi trace from PXI to global array
                                    for (int n = 0; n < NumberOfRuns + preSoakSweepTemp; n++)
                                    {
                                        //temp trace array storage use for MAX , MIN etc calculation 
                                        PXITrace[TestCount].Enable = true;
                                        PXITrace[TestCount].SoakSweep = preSoakSweep;
                                        PXITrace[TestCount].TestNumber = _TestNum;
                                        PXITrace[TestCount].TraceCount = NumberOfRuns + preSoakSweepTemp;
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].NoPoints = sweepPts;
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].RBW_Hz = multiRBW_Hz[rbw_counter];
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].FreqMHz = new double[sweepPts];
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].Ampl = new double[sweepPts];
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].Result_Header = _TestParaName;
                                        PXITrace[TestCount].Multi_Trace[rbw_counter][n].MXA_No = "PXI_Trace";

                                        for (istep = 0; istep < sweepPts; istep++)
                                        {
                                            if (istep == 0)
                                                tmpRXFreqHz = _StartRXFreq1 * 1e6;
                                            else
                                                tmpRXFreqHz = tmpRXFreqHz + (stepFreqMHz * 1e6);

                                            PXITrace[TestCount].Multi_Trace[rbw_counter][n].FreqMHz[istep] = Math.Round(tmpRXFreqHz / 1e6, 3);
                                            PXITrace[TestCount].Multi_Trace[rbw_counter][n].Ampl[istep] = 999;
                                        }
                                    }

                                    #region Test Parameter Log
                                    string[] tmpParamName;
                                    string tmp1stHeader = null;
                                    string tmp2ndHeader = null;
                                    tmpParamName = _TestParaName.Split('_');
                                    for (int i = 0; i < tmpParamName.Length; i++)
                                    {
                                        if (i > 0)
                                            tmp2ndHeader = tmp2ndHeader + "_" + tmpParamName[i];
                                    }

                                    //Sort out test result for all traces and Add test result
                                    for (int i = 0; i < PXITrace[TestCount].TraceCount; i++)
                                    {
                                        R_NF1_Freq = -888;
                                        R_NF1_Ampl = 888;
                                        double tmpNFAmpl = 999;
                                        int tmpIndex = 0;
                                        _TestParaName = "NF" + (i + 1) + tmp2ndHeader;

                                        switch (_Search_Method.ToUpper())
                                        {
                                            case "MAX":
                                                tmpNFAmpl = PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl.Max();
                                                tmpIndex = Array.IndexOf(PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl, tmpNFAmpl);     //return index of max value
                                                R_NF1_Ampl = Math.Round(tmpNFAmpl, 3);
                                                R_NF1_Freq = PXITrace[TestCount].Multi_Trace[rbw_counter][i].FreqMHz[tmpIndex];
                                                break;

                                            case "MIN":
                                                tmpNFAmpl = PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl.Min();
                                                tmpIndex = Array.IndexOf(PXITrace[TestCount].Multi_Trace[rbw_counter][i].Ampl, tmpNFAmpl);     //return index of max value
                                                R_NF1_Ampl = Math.Round(tmpNFAmpl, 3);
                                                R_NF1_Freq = PXITrace[TestCount].Multi_Trace[rbw_counter][i].FreqMHz[tmpIndex];
                                                break;

                                            default:
                                                MessageBox.Show("Test Parameter : " + _TestParam + "(" + _Search_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                                break;
                                        }

                                        if (i == 0)
                                        {
                                            if (_Test_Pin1)
                                            {
                                                ResultBuilder.BuildResults(ref results, _TestParaName + rbwParamName + "_Pin1", "dBm", R_Pin1);
                                            }
                                            if (_Test_Pout1)
                                            {
                                                ResultBuilder.BuildResults(ref results, _TestParaName + rbwParamName + "_Pout1", "dBm", R_Pout1);
                                            }
                                            if (_Test_NF1)
                                            {
                                                ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                                ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                            }
                                            if (_Test_SMU)
                                            {
                                                MeasSMU = _SMUMeasCh.Split(',');
                                                for (int j = 0; j < MeasSMU.Count(); j++)
                                                {
                                                    ResultBuilder.BuildResults(ref results, _TestParaName + rbwParamName + "_" + R_SMULabel_ICh[Convert.ToInt16(MeasSMU[j])], "A", R_SMU_ICh[Convert.ToInt16(MeasSMU[j])]);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (_Test_NF1)
                                            {
                                                ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Ampl", "dBm", R_NF1_Ampl);
                                                ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + rbwParamName + "_Freq", "MHz", R_NF1_Freq);
                                            }
                                        }
                                    }

                                    //Force test flag to false to ensure no repeated test data
                                    //because we add to string builder upfront for PXI due to data reported base on number of sweep
                                    _Test_Pin1 = false;
                                    _Test_Pout1 = false;
                                    _Test_SMU = false;
                                    #endregion
                                }

                                //Force test flag to false to ensure no repeated test data
                                //because we add to string builder upfront for PXI due to data reported base on number of sweep
                                _Test_NF1 = false;
                                #endregion
                            }


                            //for test time checking
                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            #endregion
                            break;

                        case "PXI_RXPATH_CONTACT":
                            //this function is checking the pathloss/pathgain from antenna port to rx port

                            #region PXI_RXPATH_CONTACT
                            R_NF1_Freq = -99999;
                            R_NF1_Ampl = 99999;

                            NoOfPts = (Convert.ToInt32((Math.Round(_StopRXFreq1, 3) - Math.Round(_StartRXFreq1, 3)) / Math.Round(_StepRXFreq1, 3))) + 1;
                            RXContactdBm = new double[NoOfPts];
                            RXContactFreq = new double[NoOfPts];

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region Pathloss Offset

                            //Calculate PAPR offset for PXI SG
                            modulationType = (LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE), _WaveFormName.ToUpper());
                            modArrayNo = (int)Enum.Parse(modulationType.GetType(), modulationType.ToString()); // to get the int value from System.Enum
                            papr_dB = Math.Round(LibEqmtDriver.NF_VST.NF_VSTDriver.SignalType[modArrayNo].SG_papr_dB, 3);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((Math.Round(_StopRXFreq1, 3) - Math.Round(_StartRXFreq1, 3)) / Math.Round(_StepRXFreq1, 3));
                            _RXFreq = _StartRXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                RXContactFreq[i] = _RXFreq;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                tmpRxLoss = Math.Round(tmpRxLoss + (float)_LossOutputPathRX1, 3);   //need to use round function because of C# float and double floating point bug/error
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _RXFreq, ref _LossCouplerPath, ref StrError);
                                tmpCouplerLoss = Math.Round(tmpCouplerLoss + (float)_LossCouplerPath, 3);   //need to use round function because of C# float and double floating point bug/error
                                _RXFreq = Convert.ToSingle(Math.Round(_RXFreq + _StepRXFreq1, 3));           //need to use round function because of C# float and double floating point bug/error
                            }

                            tmpAveRxLoss = tmpRxLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveCouplerLoss - tbInputLoss;       //pathloss from SG to ANT Port inclusive fixed TB Loss
                            totalOutputLoss = tmpAveRxLoss - tbOutputLoss;          //pathgain from RX Port to SA inclusive fixed TB Loss

                            //Find actual SG Power Level
                            SGTargetPin = _Pin1 - (totalInputLoss - papr_dB);
                            if (SGTargetPin > _SG1MaxPwr)       //exit test if SG Target Power is more that VST recommended Pout
                            {
                                break;
                            }

                            #region Decode MXA Config
                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", _SwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);

                            SAReferenceLevel = myUtility.MXA_Setting.RefLevel;
                            vBW_Hz = myUtility.MXA_Setting.VBW;
                            #endregion

                            #endregion

                            #region Test RX Path
                            Eq.Site[0]._EqVST.RXContactCheck(SGTargetPin, _StartRXFreq1, _StopRXFreq1, _StepRXFreq1, SAReferenceLevel, out RXContactdBm);

                            //Sort out test result
                            switch (_Search_Method.ToUpper())
                            {
                                case "MAX":
                                    R_NF1_Ampl = RXContactdBm.Max();
                                    R_NF1_Freq = RXContactFreq[Array.IndexOf(RXContactdBm, R_NF1_Ampl)];
                                    break;

                                case "MIN":
                                    R_NF1_Ampl = RXContactdBm.Min();
                                    R_NF1_Freq = RXContactFreq[Array.IndexOf(RXContactdBm, R_NF1_Ampl)];
                                    break;

                                case "AVE":
                                case "AVERAGE":
                                    R_NF1_Ampl = RXContactdBm.Average();
                                    R_NF1_Freq = RXContactFreq[0];          //return default freq i.e Start Freq
                                    break;

                                case "USER":
                                    //Note : this case required user to define freq that is within Start or Stop Freq and also same in step size
                                    if ((Convert.ToSingle(_Search_Value) >= _StartRXFreq1) && (Convert.ToSingle(_Search_Value) <= _StopRXFreq1))
                                    {
                                        try
                                        {
                                            R_NF1_Ampl = RXContactdBm[Array.IndexOf(RXContactFreq, Convert.ToSingle(_Search_Value))];     //return contact power from same array number(of index number associated with 'USER' Freq)
                                            R_NF1_Freq = Convert.ToSingle(_Search_Value);
                                        }
                                        catch       //if _Search_Value not in RXContactFreq list , will return error . Eg. User Define 1840.5 but Freq List , 1839, 1840, 1841 - > program will fail because 1840.5 is not Exactly same in freq list
                                        {
                                            R_NF1_Freq = Convert.ToSingle(_Search_Value);
                                            R_NF1_Ampl = 99999;
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Test Parameter : " + _TestParam + "(SEARCH METHOD : " + _Search_Method + ", USER DEFINE : " + _Search_Value + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    }
                                    break;

                                default:
                                    MessageBox.Show("Test Parameter : " + _TestParam + "(" + _Search_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    break;
                            }

                            R_NF1_Ampl = (R_NF1_Ampl - tmpAveRxLoss - tbOutputLoss) - _Pin1;      //return DUT only pathgain/loss result while excluding pathloss cal

                            #endregion

                            #region Sort and Store Trace Data
                            //Store RX Contact from PXI to global array
                            //temp trace array storage use for MAX , MIN etc calculation 
                            rbw_counter = 0;

                            PXITrace[TestCount].Enable = true;
                            PXITrace[TestCount].SoakSweep = false;
                            PXITrace[TestCount].TestNumber = _TestNum;
                            PXITrace[TestCount].TraceCount = 1;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].NoPoints = NoOfPts;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].RBW_Hz = 1e6;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].FreqMHz = new double[NoOfPts];
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].Ampl = new double[NoOfPts];
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].Result_Header = _TestParaName;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].MXA_No = "PXI_RXCONTACT_Trace";

                            PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].FreqMHz = new double[NoOfPts];
                            PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].Ampl = new double[NoOfPts];

                            for (istep = 0; istep < NoOfPts; istep++)
                            {
                                PXITrace[TestCount].Multi_Trace[0][0].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                PXITrace[TestCount].Multi_Trace[0][0].Ampl[istep] = Math.Round(RXContactdBm[istep], 3);

                                //Store Raw Trace Data to PXITraceRaw Array - Only actual data read from SA (not use in other than Save_PXI_TraceRaw function
                                PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].Ampl[istep] = Math.Round(RXContactdBm[istep], 3);
                            }

                            Save_PXI_TraceRaw(_TestParaName, _TestUsePrev, _Save_MXATrace, rbw_counter, 1e6);

                            #endregion

                            //for test time checking
                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}

                            #endregion
                            break;

                        case "PXI_FIXED_POWERBLAST":
                            //this function is to provide fixed power/freq to stress the unit

                            #region PXI NF FIX POWERBLAST
                            status = false;
                            pwrSearch = false;

                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(_PoutTolerance);

                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                                tolerancePwr = 0.5;

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region PowerSensor Offset, MXG and MXA1 configuration

                            //Calculate PAPR offset for PXI SG
                            modulationType = (LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE), _WaveFormName.ToUpper());
                            modArrayNo = (int)Enum.Parse(modulationType.GetType(), modulationType.ToString()); // to get the int value from System.Enum
                            papr_dB = Math.Round(LibEqmtDriver.NF_VST.NF_VSTDriver.SignalType[modArrayNo].SG_papr_dB, 3);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((_StopTXFreq1 - _StartTXFreq1) / _StepTXFreq1);
                            _TXFreq = _StartTXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, _TXFreq, ref _LossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)_LossInputPathSG1;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _TXFreq, ref _LossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)_LossCouplerPath;
                                _TXFreq = _TXFreq + _StepTXFreq1;
                            }

                            tmpAveInputLoss = tmpInputLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            //change PowerSensor, MXG setting
                            Eq.Site[0]._EqPwrMeter.SetOffset(1, totalOutputLoss);
                            SGTargetPin = _Pin1 - totalInputLoss + papr_dB;

                            //Not use - got some bug when only testing single band (shaz - 14/03/2017)
                            //if (PreviousMXAMode != _SwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                //generate modulated signal
                                string TempWaveFormName = _WaveFormName.Replace("_", "");
                                string Script =
                                         "script powerServo\r\n"
                                       + "repeat forever\r\n"
                                       + "generate Signal" + TempWaveFormName + "\r\n"
                                       + "end repeat\r\n"
                                       + "end script";
                                Eq.Site[0]._EqVST.rfsgSession.Arb.Scripting.WriteScript(Script);
                                Eq.Site[0]._EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                Eq.Site[0]._EqVST.rfsgSession.RF.Frequency = _StartTXFreq1 * 1e6;
                                Eq.Site[0]._EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;

                                //Need to ensure that SG_IQRate re-define , because RX_CONTACT routine has overwritten the initialization data
                                Eq.Site[0]._EqVST.Get_s_SignalType(_Modulation, _WaveFormName, out SG_IQRate);
                                Eq.Site[0]._EqVST.rfsgSession.Arb.IQRate = SG_IQRate;

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = _SwBand.ToUpper();
                                #endregion
                            }

                            #endregion

                            #region measure contact power (Pout1)
                            if (StopOnFail.TestFail == false)
                            {
                                if (!_TunePwr_TX1)
                                {
                                    Eq.Site[0]._EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                    Eq.Site[0]._EqVST.rfsgSession.Initiate();
                                    StopOnFail.TestFail = true;     //init to fail state as default
                                    DelayMs(_RdPwr_Delay);
                                    R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                    R_Pin1 = Math.Round(SGTargetPin - papr_dB + totalInputLoss, 3);

                                    if (Math.Abs(_Pout1 - R_Pout1) <= (tolerancePwr + 3.5))
                                    {
                                        pwrSearch = true;
                                        StopOnFail.TestFail = false;
                                    }
                                }
                                else
                                {
                                    do
                                    {
                                        StopOnFail.TestFail = true;     //init to fail state as default
                                        R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = SGTargetPin - papr_dB + totalInputLoss;

                                        if (Math.Abs(_Pout1 - R_Pout1) >= tolerancePwr)
                                        {
                                            if ((Index == 0) && (SGTargetPin < _SG1MaxPwr))   //preset to initial target power for 1st measurement count
                                            {
                                                Eq.Site[0]._EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;
                                                Eq.Site[0]._EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                                Eq.Site[0]._EqVST.rfsgSession.Initiate();
                                                DelayMs(_RdPwr_Delay);
                                                R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                            }

                                            SGTargetPin = SGTargetPin + (_Pout1 - R_Pout1);

                                            if (SGTargetPin < _SG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                            {
                                                Eq.Site[0]._EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;
                                                DelayMs(_RdPwr_Delay);
                                            }

                                            if (SGTargetPin > _SG1MaxPwr)      //if input sig gen exit limit , exit pwr search loop
                                            {
                                                SGTargetPin = _Pin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            pwrSearch = true;
                                            SGPowerLevel = SGTargetPin;
                                            StopOnFail.TestFail = false;
                                            break;
                                        }

                                        Index++;
                                    }
                                    while (Index < 10);     // max power search loop
                                }
                            }

                            #endregion

                            if (pwrSearch)
                            {
                                //Measure SMU current
                                MeasSMU = _SMUMeasCh.Split(',');
                                if (_Test_SMU)
                                {
                                    DelayMs(_RdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (_SMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }

                                //to control DUT soaking time
                                paramTestTime = tTime.ElapsedMilliseconds;
                                if (paramTestTime < (long)_Estimate_TestTime)
                                {
                                    syncTest_Delay = (long)_Estimate_TestTime - paramTestTime;
                                    DelayMs((int)syncTest_Delay);
                                }

                                Eq.Site[0]._EqVST.rfsgSession.Abort();         //stop power servo script
                            }
                            else                                            //if fail power out search , set data to default
                            {
                                //Measure SMU current
                                MeasSMU = _SMUMeasCh.Split(',');
                                if (_Test_SMU)
                                {
                                    DelayMs(_RdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (_SMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }

                                Eq.Site[0]._EqVST.rfsgSession.Abort();         //stop power servo script
                            }

                            //for test time checking
                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            #endregion
                            break;

                        case "PXI_RAMP_POWERBLAST":
                            //this function is to provide fixed power/freq to stress the unit

                            #region PXI NF POWERBLAST RAMP
                            status = false;
                            pwrSearch = false;
                            R_Pin1 = -999;        //set test flag to default -999
                            _Test_Pin1 = true;
                            _Test_Pout1 = false;
                            _Test_NF1 = false;
                            _Test_SMU = false;

                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(_PoutTolerance);

                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                                tolerancePwr = 0.5;

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            searchPowerBlastKey(_TestParam, _SwBand, out CtrFreqMHz_pwrBlast, out StartPwrLvldBm_pwrBlast, out StopPwrLvldBm_pwrBlast, out StepPwrLvl_pwrBlast, out DwellTmS_pwrBlast, out Transient_mS_pwrBlast, out Transient_Step_pwrBlast, out b_PwrBlastTKey);
                            #endregion

                            #region PowerSensor Offset, MXG and MXA1 configuration
                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get pathloss base on PowerBlast Center freq
                            ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, CtrFreqMHz_pwrBlast, ref _LossInputPathSG1, ref StrError);
                            ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, CtrFreqMHz_pwrBlast, ref _LossCouplerPath, ref StrError);

                            totalInputLoss = Math.Round((float)_LossInputPathSG1 - tbInputLoss, 3);
                            totalOutputLoss = Math.Abs((float)_LossCouplerPath - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            //change PowerSensor, MXG setting
                            Eq.Site[0]._EqPwrMeter.SetOffset(1, Math.Round(totalOutputLoss, 3));
                            SGTargetPin = _Pin1 - totalInputLoss;
                            double startPwrLvl = Math.Round(StartPwrLvldBm_pwrBlast - totalInputLoss, 3);
                            double stopPwrLvl = Math.Round(StopPwrLvldBm_pwrBlast - totalInputLoss, 3);

                            #endregion

                            //Power Ramp routine
                            Eq.Site[0]._EqVST.PowerRamp(_Modulation, _WaveFormName, CtrFreqMHz_pwrBlast * 1e6, Transient_mS_pwrBlast / 1e3, Transient_Step_pwrBlast,
                                                DwellTmS_pwrBlast / 1e3, StepPwrLvl_pwrBlast, startPwrLvl, stopPwrLvl);

                            R_Pin1 = 1;        //set test flag to default 1 indicate complete test only

                            //for test time checking
                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            #endregion
                            break;

                        case "PXI_RXPATH_GAIN": //Seoul
                                                //this function is checking the pathloss/pathgain from antenna port to rx port

                            #region PXI_RXPATH_GAIN
                            R_NF1_Freq = -99999;
                            R_NF1_Ampl = 99999;

                            NoOfPts = (Convert.ToInt32((Math.Round(_StopRXFreq1, 3) - Math.Round(_StartRXFreq1, 3)) / Math.Round(_StepRXFreq1, 3))) + 1;
                            RXContactdBm = new double[NoOfPts];
                            RXContactFreq = new double[NoOfPts];
                            RXContactGain = new double[NoOfPts];//Seoul
                            RXPathLoss = new double[NoOfPts]; //Seoul
                            LNAInputLoss = new double[NoOfPts]; //Seoul

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region Pathloss Offset

                            //Calculate PAPR offset for PXI SG
                            modulationType = (LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE), _WaveFormName.ToUpper());
                            modArrayNo = (int)Enum.Parse(modulationType.GetType(), modulationType.ToString()); // to get the int value from System.Enum
                            papr_dB = Math.Round(LibEqmtDriver.NF_VST.NF_VSTDriver.SignalType[modArrayNo].SG_papr_dB, 3);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((Math.Round(_StopRXFreq1, 3) - Math.Round(_StartRXFreq1, 3)) / Math.Round(_StepRXFreq1, 3));
                            _RXFreq = _StartRXFreq1;

                            for (int i = 0; i <= count; i++)
                            {
                                RXContactFreq[i] = _RXFreq;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                tmpRxLoss = Math.Round(tmpRxLoss + (float)_LossOutputPathRX1, 3);   //need to use round function because of C# float and double floating point bug/error
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _RXFreq, ref _LossCouplerPath, ref StrError);
                                tmpCouplerLoss = Math.Round(tmpCouplerLoss + (float)_LossCouplerPath, 3);   //need to use round function because of C# float and double floating point bug/error
                                _RXFreq = Convert.ToSingle(Math.Round(_RXFreq + _StepRXFreq1, 3));           //need to use round function because of C# float and double floating point bug/error
                                RXPathLoss[i] = _LossOutputPathRX1;//Seoul
                                LNAInputLoss[i] = _LossCouplerPath;//Seoul
                            }

                            tmpAveRxLoss = tmpRxLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveCouplerLoss - tbInputLoss;       //pathloss from SG to ANT Port inclusive fixed TB Loss
                            totalOutputLoss = tmpAveRxLoss - tbOutputLoss;          //pathgain from RX Port to SA inclusive fixed TB Loss

                            //Find actual SG Power Level
                            SGTargetPin = _Pin1 - (totalInputLoss - papr_dB);
                            if (SGTargetPin > _SG1MaxPwr)       //exit test if SG Target Power is more that VST recommended Pout
                            {
                                break;
                            }

                            #region Decode MXA Config
                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", _SwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);

                            SAReferenceLevel = myUtility.MXA_Setting.RefLevel;
                            vBW_Hz = myUtility.MXA_Setting.VBW;
                            #endregion

                            #endregion

                            #region Test RX Path
                            Eq.Site[0]._EqVST.RXContactCheck(SGTargetPin, _StartRXFreq1, _StopRXFreq1, _StepRXFreq1, SAReferenceLevel, out RXContactdBm);

                            for (int i = 0; i < RXContactdBm.Length; i++)
                            {
                                if (RXContactdBm[i].ToString().Contains("Infinity") || RXContactdBm[i].ToString().Contains("∞"))
                                {
                                    RXContactGain[i] = -999;
                                }
                                else
                                {
                                    RXContactGain[i] = (RXContactdBm[i] - RXPathLoss[i] - tbOutputLoss) - _Pin1; //Seoul for RX Gain trace
                                }
                            }

                            //Sort out test result
                            switch (_Search_Method.ToUpper())
                            {
                                case "MAX":
                                    R_NF1_Ampl = RXContactGain.Max();
                                    R_NF1_Freq = RXContactFreq[Array.IndexOf(RXContactGain, R_NF1_Ampl)];
                                    break;

                                case "MIN":
                                    R_NF1_Ampl = RXContactGain.Min();
                                    R_NF1_Freq = RXContactFreq[Array.IndexOf(RXContactGain, R_NF1_Ampl)];
                                    break;

                                case "AVE":
                                case "AVERAGE":
                                    R_NF1_Ampl = RXContactGain.Average();
                                    R_NF1_Freq = RXContactFreq[0];          //return default freq i.e Start Freq
                                    break;

                                case "USER":
                                    //Note : this case required user to define freq that is within Start or Stop Freq and also same in step size
                                    if ((Convert.ToSingle(_Search_Value) >= _StartRXFreq1) && (Convert.ToSingle(_Search_Value) <= _StopRXFreq1))
                                    {
                                        try
                                        {
                                            R_NF1_Ampl = RXContactGain[Array.IndexOf(RXContactFreq, Convert.ToSingle(_Search_Value))];     //return contact power from same array number(of index number associated with 'USER' Freq)
                                            R_NF1_Freq = Convert.ToSingle(_Search_Value);
                                        }
                                        catch       //if _Search_Value not in RXContactFreq list , will return error . Eg. User Define 1840.5 but Freq List , 1839, 1840, 1841 - > program will fail because 1840.5 is not Exactly same in freq list
                                        {
                                            R_NF1_Freq = Convert.ToSingle(_Search_Value);
                                            R_NF1_Ampl = 99999;
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Test Parameter : " + _TestParam + "(SEARCH METHOD : " + _Search_Method + ", USER DEFINE : " + _Search_Value + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    }
                                    break;

                                default:
                                    MessageBox.Show("Test Parameter : " + _TestParam + "(" + _Search_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    break;
                            }

                            #endregion

                            #region Sort and Store Trace Data
                            //Store RX Contact from PXI to global array
                            //temp trace array storage use for MAX , MIN etc calculation 
                            rbw_counter = 0;

                            PXITrace[TestCount].Enable = true;
                            PXITrace[TestCount].SoakSweep = false;
                            PXITrace[TestCount].TestNumber = _TestNum;
                            PXITrace[TestCount].TraceCount = 1;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].NoPoints = NoOfPts;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].RBW_Hz = 1e6;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].FreqMHz = new double[NoOfPts];
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].Ampl = new double[NoOfPts];
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].Result_Header = _TestParaName;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].MXA_No = "PXI_RXCONTACT_Trace";

                            PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].FreqMHz = new double[NoOfPts];
                            PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].Ampl = new double[NoOfPts];

                            for (istep = 0; istep < NoOfPts; istep++)
                            {
                                PXITrace[TestCount].Multi_Trace[0][0].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                PXITrace[TestCount].Multi_Trace[0][0].Ampl[istep] = Math.Round(RXContactGain[istep], 3);

                                //Store Raw Trace Data to PXITraceRaw Array - Only actual data read from SA (not use in other than Save_PXI_TraceRaw function
                                PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].Ampl[istep] = Math.Round(RXContactGain[istep], 3);
                            }

                            Save_PXI_TraceRaw(_TestParaName, _TestUsePrev, _Save_MXATrace, rbw_counter, 1e6);

                            #endregion

                            for (int i = 0; i < NoOfPts; i++)
                            {
                                if (!b_GE_Header)
                                {
                                    ResultBuilder.BuildResults(ref results, _TestParaName + "_" + RXContactFreq[i] + "_Rx-Gain", "dB", RXContactGain[i].ToString().Contains("Infinity") ? -999 : RXContactGain[i]);
                                }
                                else
                                {
                                    b_SmuHeader = true;
                                    string GE_TestParam = null;
                                    Rslt_GE_Header = new s_GE_Header();
                                    Decode_GE_Header(TestPara, out Rslt_GE_Header);

                                    Rslt_GE_Header.Freq1 = "_Rx-" + RXContactFreq[i] + "MHz";
                                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                    ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", RXContactGain[i].ToString().Contains("Infinity") ? -999 : RXContactGain[i]);
                                }

                            }

                            //for test time checking
                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}


                            #endregion
                            break;

                        case "PXI_NF_COLD":

                            #region PXI NF COLD
                            //NOTE: Some of these inputs may have to be read from input-excel or defined elsewhere
                            //Variable use in VST Measure Function

                            #region RxGain & Loss gatherring for NF Measurement

                            if (_PXI_NoOfSweep <= 0)                //check the number of sweep for pxi, set to default if user forget to keyin in excel
                                NumberOfRuns = 1;
                            else
                                NumberOfRuns = _PXI_NoOfSweep;

                            //For Collecting LNA Gain & Loss from previous Data -Seoul
                            NoOfPts = (Convert.ToInt32(Math.Ceiling((Math.Round(_StopRXFreq1, 3) - Math.Round(_StartRXFreq1, 3)) / Math.Round(_StepRXFreq1, 3)))) + 1;

                            RXContactFreq = new double[NoOfPts];
                            RXContactGain = new double[NoOfPts];
                            RXPathLoss = new double[NoOfPts];
                            LNAInputLoss = new double[NoOfPts];
                            TXPAOnFreq = new double[NoOfPts];
                            RxGainDic = new Dictionary<double, double>();

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand_HotNF.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            //For Collecting RX Gain trace Number from Previous setting -Seoul
                            TestUsePrev_ArrayNo = 0;
                            for (int i = 0; i < PXITrace.Length; i++)
                            {
                                if (Convert.ToInt16(_TestUsePrev) == PXITrace[i].TestNumber)
                                {
                                    TestUsePrev_ArrayNo = i;
                                }
                            }

                            for (int i = 0; i < PXITrace[TestUsePrev_ArrayNo].Multi_Trace[0][0].FreqMHz.Length; i++)
                            {
                                RxGainDic.Add(Math.Round(PXITrace[TestUsePrev_ArrayNo].Multi_Trace[0][0].FreqMHz[i], 3), PXITrace[TestUsePrev_ArrayNo].Multi_Trace[0][0].Ampl[i]);
                            }

                            _TXFreq = _StartTXFreq1;
                            _RXFreq = _StartRXFreq1;

                            count = Convert.ToInt16(Math.Ceiling((Math.Round(_StopRXFreq1, 3) - Math.Round(_StartRXFreq1, 3)) / Math.Round(_StepRXFreq1, 3)));

                            if ((_StopTXFreq1 - _StartTXFreq1) == (_StopRXFreq1 - _StartRXFreq1))
                            {
                                _StepTXFreq = _StepRXFreq1;
                            }

                            else
                            {
                                _StepTXFreq = (_StopTXFreq1 - _StartTXFreq1) / (NoOfPts - 1);
                            }


                            for (int i = 0; i <= count; i++)
                            {
                                TXPAOnFreq[i] = Math.Round(_TXFreq, 3);
                                RXContactFreq[i] = Math.Round(_RXFreq, 3);

                                if (RxGainDic.TryGetValue(Math.Round(_RXFreq, 3), out RXContactGain[i])) { }
                                else
                                {
                                    MessageBox.Show("Need to check between RxGain & NF Frequency Range");
                                }

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                RXPathLoss[i] = _LossOutputPathRX1;//Seoul

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _RXFreq, ref _LossCouplerPath, ref StrError);
                                LNAInputLoss[i] = _LossCouplerPath;//Seoul

                                _TXFreq = Convert.ToSingle(Math.Round(_TXFreq + _StepTXFreq, 3));
                                _RXFreq = Convert.ToSingle(Math.Round(_RXFreq + _StepRXFreq1, 3));           //need to use round function because of C# float and double floating point bug/error

                                if (_RXFreq > _StopRXFreq1)//For Last Freq match
                                {
                                    _TXFreq = _StopTXFreq1;
                                    _RXFreq = _StopRXFreq1;
                                }
                            }
                            #endregion

                            #region Switching for NF Test
                            //Switching for NF Testing -Seoul

                            SetswitchPath(DicCalInfo, DataFilePath.LocSettingPath, TCF_Header.ConstSwitching_Band_HotNF, _SwBand_HotNF, _Setup_Delay);
                            #endregion

                            #region Cold NF Measurement
                            Cold_NF_new = new double[NumberOfRuns][];
                            Cold_NoisePower_new = new double[NumberOfRuns][];

                            for (int i = 0; i < NumberOfRuns; i++)
                            {
                                Cold_NF_new[i] = new double[NoOfPts];
                                Cold_NoisePower_new[i] = new double[NoOfPts];
                            }

                            Eq.Site[0]._EqVST.rfsaSession.Utility.Reset();

                            Eq.Site[0]._EqVST.PreConfig_VSTSA();
                            Eq.Site[0]._EqVST.ConfigureTriggers();  //Disable Trigger for NF Testing
                            Eq.Site[0]._EqVST.rfsaSession.Configuration.Triggers.ReferenceTrigger.Disable(); //Disable Reference Trigger for NF Testing.

                            Eq.Site[0]._EqRFmx.cRFmxNF.specNFColdSource2[TestCount][1].Commit(""); //Configure dummy setting before actual NF measurement

                            for (int i = 0; i < NumberOfRuns; i++)
                            {
                                Eq.Site[0]._EqRFmx.cRFmxNF.specNFColdSource2[TestCount][0].Initiate("", "COLD" + TestCount.ToString() + "_" + i);
                                Eq.Site[0]._EqRFmx.WaitForAcquisitionComplete(1);
                            }
                            #endregion

                            #region ResetRFSA and Re-configure after NF Measurement

                            Eq.Site[0]._EqVST.rfsaSession.Utility.Reset();
                            Eq.Site[0]._EqVST.PreConfig_VSTSA();

                            #endregion

                            #region Sort and Store Trace Data
                            //Store multi trace from PXI to global array
                            for (int n = 0; n < NumberOfRuns; n++)
                            {
                                //temp trace array storage use for MAX , MIN etc calculation 
                                PXITrace[TestCount].Enable = true;
                                PXITrace[TestCount].SoakSweep = preSoakSweep;
                                PXITrace[TestCount].TestNumber = _TestNum;
                                PXITrace[TestCount].TraceCount = NumberOfRuns;
                                PXITrace[TestCount].Multi_Trace[0][n].NoPoints = NoOfPts;
                                PXITrace[TestCount].Multi_Trace[0][n].RBW_Hz = _NF_BW * 1e06;
                                PXITrace[TestCount].Multi_Trace[0][n].FreqMHz = new double[NoOfPts];
                                PXITrace[TestCount].Multi_Trace[0][n].Ampl = new double[NoOfPts];
                                PXITrace[TestCount].Multi_Trace[0][n].Result_Header = _TestParaName;
                                PXITrace[TestCount].Multi_Trace[0][n].MXA_No = "PXI_NF_COLD_Trace";
                                PXITrace[TestCount].Multi_Trace[0][n].RxGain = new double[NoOfPts]; //Yoonchun

                                PXITraceRaw[TestCount].Multi_Trace[0][n].FreqMHz = new double[NoOfPts];
                                PXITraceRaw[TestCount].Multi_Trace[0][n].Ampl = new double[NoOfPts];
                                PXITraceRaw[TestCount].Multi_Trace[0][n].RxGain = new double[NoOfPts]; //Yoonchun

                                for (istep = 0; istep < NoOfPts; istep++)
                                {
                                    PXITrace[TestCount].Multi_Trace[0][n].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                    PXITrace[TestCount].Multi_Trace[0][n].RxGain[istep] = Math.Round(RXContactGain[istep], 3); //Yoonchun

                                    PXITraceRaw[TestCount].Multi_Trace[0][n].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                    PXITraceRaw[TestCount].Multi_Trace[0][n].RxGain[istep] = Math.Round(RXContactGain[istep], 3); //Yoonchun
                                }
                            }
                            #endregion

                            //Force test flag to false to ensure no repeated test data
                            //because we add to string builder upfront for PXI due to data reported base on number of sweep
                            _Test_Pin1 = false;
                            _Test_Pout1 = false;
                            _Test_SMU = false;

                            //Force test flag to false to ensure no repeated test data
                            //because we add to string builder upfront for PXI due to data reported base on number of sweep
                            _Test_NF1 = false;
                            b_SmuHeader = true;

                            tTime.Stop();

                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}

                            #endregion
                            break;

                        case "PXI_NF_HOT":
                            // This is using PXI VST as Sweeper and Analyzer. Will do multiple sweep in one function because using script (Pwr Servo->Soak Sweep->SoakTime->MultiSweep)
                            // Slight different from LXI solution where you define number of sweep in multiple line in TCF

                            #region PXI NF HOT
                            //NOTE: Some of these inputs may have to be read from input-excel or defined elsewhere
                            //Variable use in VST Measure 

                            SGPowerLevel = -18;// -18 CDMA dBm //-20 LTE dBm  
                            SAReferenceLevel = -20;
                            SoakTime = 450e-3;
                            SoakFrequency = _StartTXFreq1 * 1e6;
                            vBW_Hz = 300;
                            RBW_Hz = 1e6;
                            preSoakSweep = true; //to indicate if another sweep should be done **MAKE SURE TO SPLIT OUTPUT ARRAY**
                            preSoakSweepTemp = preSoakSweep == true ? 1 : 0; //to indicate if another sweep should be done
                            stepFreqMHz = 0.1;
                            tmpRXFreqHz = _StartRXFreq1 * 1e6;
                            sweepPts = (Convert.ToInt32((_StopTXFreq1 - _StartTXFreq1) / stepFreqMHz)) + 1;
                            //----

                            status = false;
                            pwrSearch = false;
                            Index = 0;
                            tx1_span = 0;
                            tx1_noPoints = 0;
                            rx1_span = 0;
                            rx1_cntrfreq = 0;
                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(_PoutTolerance);

                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                                tolerancePwr = 0.5;

                            if (_PXI_NoOfSweep <= 0)                //check the number of sweep for pxi, set to default if user forget to keyin in excel
                                NumberOfRuns = 1;
                            else
                                NumberOfRuns = _PXI_NoOfSweep;

                            //use for searching previous result - to get the DUT LNA gain from previous result
                            if (Convert.ToInt16(_TestUsePrev) > 0)
                            {
                                usePrevRslt = true;
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                prevRslt = Math.Round(ReportRslt(_TestUsePrev, resultTag), 3);
                            }

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region PowerSensor Offset, MXG and MXA1 configuration

                            //Calculate PAPR offset for PXI SG
                            modulationType = (LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE), _WaveFormName.ToUpper());
                            modArrayNo = (int)Enum.Parse(modulationType.GetType(), modulationType.ToString()); // to get the int value from System.Enum
                            papr_dB = Math.Round(LibEqmtDriver.NF_VST.NF_VSTDriver.SignalType[modArrayNo].SG_papr_dB, 3);

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((_StopTXFreq1 - _StartTXFreq1) / _StepTXFreq1);
                            _TXFreq = _StartTXFreq1;
                            TXCenterFreq = Convert.ToString((_StartTXFreq1 + _StopTXFreq1) / 2); //Seoul

                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, _TXFreq, ref _LossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)_LossInputPathSG1;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _TXFreq, ref _LossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)_LossCouplerPath;
                                _TXFreq = _TXFreq + _StepTXFreq1;
                            }

                            tmpAveInputLoss = tmpInputLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            //change PowerSensor, MXG setting

                            if (EqmtStatus.PM)
                            {
                                Eq.Site[0]._EqPwrMeter.SetOffset(1, totalOutputLoss);
                            }
                            else
                            {
                                if (!Eq.Site[0]._isUseRFmxForTxMeasure) // VSA Measure mode
                                {
                                    // Reset RFSA
                                    Eq.Site[0]._EqVST.PreConfig_VSTSA(RfsaAcquisitionType.Spectrum);

                                    // Set RF SA Reference Level (target pout - totalInputLoss - totalOutputLoss)
                                    Eq.Site[0]._EqVST.rfsaSession.Configuration.Vertical.ReferenceLevel = _Pout1 - totalOutputLoss + 3; // + 3 dB

                                }
                                Eq.Site[0]._EqVST.ConfigureTriggers();
                                Eq.Site[0]._EqVST.rfsaSession.Configuration.Triggers.ReferenceTrigger.Disable();
                            }
                            //SGTargetPin = papr_dB - _Pin1 - totalInputLoss;   //wrong equation - Shaz 22/10/2019
                            SGTargetPin = _Pin1 - totalInputLoss + papr_dB;

                            //if (PreviousMXAMode != _SwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                //generate modulated signal
                                string TempWaveFormName = _WaveFormName.Replace("_", "");
                                string Script =
                                         "script powerServo\r\n"
                                       + "repeat forever\r\n"
                                       + "generate Signal" + TempWaveFormName + "\r\n"
                                       + "end repeat\r\n"
                                       + "end script";
                                TxPAOnScript = Script;

                                try
                                {
                                    Eq.Site[0]._EqVST.rfsgSession.Arb.Scripting.WriteScript(Script);
                                }
                                catch (Exception ex)
                                {
                                    MessageBox.Show(ex.Message);
                                }
                                Eq.Site[0]._EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                Eq.Site[0]._EqVST.rfsgSession.RF.Frequency = Convert.ToDouble(TXCenterFreq) * 1e6; //Seoul
                                //EqVST.rfsgSession.RF.Frequency = _StartTXFreq1 * 1e6; // Original
                                Eq.Site[0]._EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;

                                //Need to ensure that SG_IQRate re-define , because RX_CONTACT routine has overwritten the initialization data
                                Eq.Site[0]._EqVST.Get_s_SignalType(_Modulation, _WaveFormName, out SG_IQRate);
                                Eq.Site[0]._EqVST.rfsgSession.Arb.IQRate = SG_IQRate;

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = _SwBand.ToUpper();
                                #endregion
                            }

                            #endregion

                            #region measure contact power (Pout1) - Power Senseor
                            if (StopOnFail.TestFail == false)
                            {
                                if (!_TunePwr_TX1)
                                {
                                    Eq.Site[0]._EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                    Eq.Site[0]._EqVST.rfsgSession.Initiate();
                                    StopOnFail.TestFail = true;     //init to fail state as default
                                    DelayMs(_RdPwr_Delay);

                                    if (EqmtStatus.PM) R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                    else
                                    {
                                        if (!Eq.Site[0]._isUseRFmxForTxMeasure) // VSA Measure mode
                                            R_Pout1 = Eq.Site[0]._EqVST.MeasureChanPower(Convert.ToDouble(TXCenterFreq) * 10e5, SG_IQRate, 600) + totalOutputLoss;

                                        else
                                        {
                                            Eq.Site[0]._EqRFmx.cRFmxChp.InitiateSpec(TestCount);
                                            R_Pout1 = Eq.Site[0]._EqRFmx.cRFmxChp.RetrieveResults(TestCount) + totalOutputLoss;
                                        }
                                    }

                                    R_Pin1 = Math.Round(SGTargetPin - papr_dB + totalInputLoss, 3);
                                    if (Math.Abs(_Pout1 - R_Pout1) <= (tolerancePwr + 3.5))
                                    {
                                        pwrSearch = true;
                                        StopOnFail.TestFail = false;
                                    }
                                }
                                else
                                {
                                    do
                                    {
                                        StopOnFail.TestFail = true;     //init to fail state as default

                                        if (EqmtStatus.PM) R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                        else
                                        {
                                            if (!Eq.Site[0]._isUseRFmxForTxMeasure) // VSA Measure mode
                                                R_Pout1 = Eq.Site[0]._EqVST.MeasureChanPower(Convert.ToDouble(TXCenterFreq) * 10e5, SG_IQRate, 600) + totalOutputLoss;

                                            else
                                            {
                                                Eq.Site[0]._EqRFmx.cRFmxChp.InitiateSpec(TestCount);
                                                R_Pout1 = Eq.Site[0]._EqRFmx.cRFmxChp.RetrieveResults(TestCount) + totalOutputLoss;
                                            }
                                        }

                                        R_Pin1 = SGTargetPin - papr_dB + totalInputLoss;

                                        if (Math.Abs(_Pout1 - R_Pout1) >= tolerancePwr)
                                        {
                                            if ((Index == 0) && (R_Pin1 < _SG1MaxPwr))   //preset to initial target power for 1st measurement count
                                            {
                                                Eq.Site[0]._EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;
                                                Eq.Site[0]._EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                                                Eq.Site[0]._EqVST.rfsgSession.Initiate();
                                                DelayMs(_RdPwr_Delay);
                                                if (EqmtStatus.PM) R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                                else
                                                {
                                                    if (!Eq.Site[0]._isUseRFmxForTxMeasure) // VSA Measure mode
                                                        R_Pout1 = Eq.Site[0]._EqVST.MeasureChanPower(Convert.ToDouble(TXCenterFreq) * 10e5, SG_IQRate, 600) + totalOutputLoss;

                                                    else
                                                    {
                                                        Eq.Site[0]._EqRFmx.cRFmxChp.InitiateSpec(TestCount);
                                                        R_Pout1 = Eq.Site[0]._EqRFmx.cRFmxChp.RetrieveResults(TestCount) + totalOutputLoss;
                                                    }
                                                }
                                            }
                                            if (R_Pout1 > -20) SGTargetPin = SGTargetPin + (_Pout1 - R_Pout1);
                                            else SGTargetPin = SGTargetPin;

                                            if (R_Pin1 < _SG1MaxPwr && SGTargetPin < _SG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                            {
                                                Eq.Site[0]._EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;
                                                DelayMs(_RdPwr_Delay);
                                            }

                                            else if ((SGTargetPin > _SG1MaxPwr) || (R_Pin1 > _SG1MaxPwr))      //if input sig gen exit limit , exit pwr search loop
                                            {
                                                SGTargetPin = _Pin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                                R_Pin1 = SGTargetPin + totalInputLoss;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            pwrSearch = true;
                                            SGPowerLevel = SGTargetPin;
                                            StopOnFail.TestFail = false;
                                            break;
                                        }

                                        Index++;
                                    }
                                    while (Index < 10);     // max power search loop
                                }
                            }

                            #endregion measure contact power (Pout1) - Power Senseor

                            #region RxGain & Loss gatherring for NF Measurement

                            //For Collecting LNA Gain & Loss from previous Data -Seoul
                            NoOfPts = (Convert.ToInt32(Math.Ceiling((Math.Round(_StopRXFreq1, 3) - Math.Round(_StartRXFreq1, 3)) / Math.Round(_StepRXFreq1, 3)))) + 1;

                            RXContactFreq = new double[NoOfPts];
                            RXContactGain = new double[NoOfPts];
                            RXPathLoss = new double[NoOfPts];
                            LNAInputLoss = new double[NoOfPts];
                            TXPAOnFreq = new double[NoOfPts];
                            RxGainDic = new Dictionary<double, double>();

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand_HotNF.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            //For Collecting RX Gain trace Number from Previous setting -Seoul
                            TestUsePrev_ArrayNo = 0;
                            for (int i = 0; i < PXITrace.Length; i++)
                            {
                                if (Convert.ToInt16(_TestUsePrev) == PXITrace[i].TestNumber)
                                {
                                    TestUsePrev_ArrayNo = i;
                                }
                            }

                            for (int i = 0; i < PXITrace[TestUsePrev_ArrayNo].Multi_Trace[0][0].FreqMHz.Length; i++)
                            {
                                RxGainDic.Add(Math.Round(PXITrace[TestUsePrev_ArrayNo].Multi_Trace[0][0].FreqMHz[i], 3), PXITrace[TestUsePrev_ArrayNo].Multi_Trace[0][0].Ampl[i]);
                            }

                            _TXFreq = _StartTXFreq1;
                            _RXFreq = _StartRXFreq1;
                            count = Convert.ToInt16(Math.Ceiling((Math.Round(_StopRXFreq1, 3) - Math.Round(_StartRXFreq1, 3)) / Math.Round(_StepRXFreq1, 3)));

                            if ((_StopTXFreq1 - _StartTXFreq1) == (_StopRXFreq1 - _StartRXFreq1))
                            {
                                _StepTXFreq = _StepRXFreq1;
                            }

                            else
                            {
                                _StepTXFreq = (_StopTXFreq1 - _StartTXFreq1) / (NoOfPts - 1);

                                //Add - 27.01.2021
                                if ((_StepTXFreq != 0) && (_StepTXFreq != _StepTXFreq1))
                                {
                                    _StepTXFreq = _StepTXFreq1;
                                }
                            }


                            for (int i = 0; i <= count; i++)
                            {
                                TXPAOnFreq[i] = Math.Round(_TXFreq, 3);
                                RXContactFreq[i] = Math.Round(_RXFreq, 3);

                                if (RxGainDic.TryGetValue(Math.Round(_RXFreq, 3), out RXContactGain[i])) { }
                                else
                                {
                                    MessageBox.Show("Need to check between RxGain & NF Frequency Range");
                                }

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                RXPathLoss[i] = _LossOutputPathRX1;//Seoul

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _RXFreq, ref _LossCouplerPath, ref StrError);
                                LNAInputLoss[i] = _LossCouplerPath;//Seoul

                                _TXFreq = Convert.ToSingle(Math.Round(_TXFreq + _StepTXFreq, 3));
                                _RXFreq = Convert.ToSingle(Math.Round(_RXFreq + _StepRXFreq1, 3));           //need to use round function because of C# float and double floating point bug/error

                                if (_RXFreq > _StopRXFreq1)//For Last Freq match
                                {
                                    _RXFreq = _StopRXFreq1;
                                }

                                // Add - 27.01.2021, Bug fixed
                                if (_TXFreq > _StopTXFreq1)
                                {
                                    _TXFreq = _StopTXFreq1;
                                }
                            }
                            #endregion

                            #region Switching for NF Test

                            //Switching for NF Testing -Seoul
                            SetswitchPath(DicCalInfo, DataFilePath.LocSettingPath, TCF_Header.ConstSwitching_Band_HotNF, _SwBand_HotNF, _Setup_Delay);

                            #endregion

                            //Measure SMU current
                            MeasSMU = _SMUMeasCh.Split(',');
                            if (_Test_SMU)
                            {
                                DelayMs(_RdCurr_Delay);
                                float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                for (int i = 0; i < MeasSMU.Count(); i++)
                                {
                                    int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                    if (_SMUILimitCh[smuIChannel] > 0)
                                    {
                                        R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                    }

                                    // pass out the test result label for every measurement channel
                                    string tempLabel = "SMUI_CH" + MeasSMU[i];
                                    foreach (string key in DicTestLabel.Keys)
                                    {
                                        if (key == tempLabel)
                                        {
                                            R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                            break;
                                        }
                                    }
                                }
                            }
                            //total test time for each parameter will include the soak time
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)_NF_SoakTime)
                            {
                                syncTest_Delay = (long)_NF_SoakTime - paramTestTime;
                                SoakTime = syncTest_Delay * 1e-3;       //convert to second
                            }
                            else
                            {
                                SoakTime = 0;                //no soak required if power servo longer than expected total test time                                                        
                            }

                            DelayMs(Convert.ToInt32(SoakTime * 1e3)); //Delay for Soak Time
                            Eq.Site[0]._EqVST.rfsgSession.Abort(); //Abort after power server & soak time

                            #region Hot NF Measurement
                            Hot_NF_new = new double[NumberOfRuns][];
                            Hot_NoisePower_new = new double[NumberOfRuns][];

                            for (int i = 0; i < NumberOfRuns; i++)
                            {
                                Hot_NF_new[i] = new double[NoOfPts];
                                Hot_NoisePower_new[i] = new double[NoOfPts];
                            }

                            Eq.Site[0]._EqVST.rfsaSession.Utility.Reset();

                            Eq.Site[0]._EqVST.PreConfig_VSTSA();
                            Eq.Site[0]._EqVST.ConfigureTriggers();  //Disable Trigger for NF Testing
                            Eq.Site[0]._EqVST.rfsaSession.Configuration.Triggers.ReferenceTrigger.Disable(); //Disable Reference Trigger for NF Testing.

                            Eq.Site[0]._EqVST.rfsgSession.Arb.Scripting.WriteScript(TxPAOnScript);
                            Eq.Site[0]._EqVST.rfsgSession.Arb.Scripting.SelectedScriptName = "powerServo";
                            Eq.Site[0]._EqVST.Get_s_SignalType(_Modulation, _WaveFormName, out SG_IQRate);
                            Eq.Site[0]._EqVST.rfsgSession.Arb.IQRate = SG_IQRate;
                            Eq.Site[0]._EqVST.rfsgSession.RF.PowerLevel = SGTargetPin;

                            Eq.Site[0]._EqRFmx.cRFmxNF.specNFColdSource2[TestCount][RXContactFreq.Length].Commit(""); //Configure dummy setting before actual NF measurement

                            for (int i = 0; i < NumberOfRuns; i++)
                            {
                                for (int j = 0; j < RXContactFreq.Length; j++)
                                {
                                    double[] rxFreqPoint = new double[1];
                                    double[] rxGainPoint = new double[1];
                                    rxFreqPoint[0] = RXContactFreq[j];
                                    rxGainPoint[0] = RXContactGain[j];

                                    Eq.Site[0]._EqVST.DoneVSGinit.Reset();
                                    Eq.Site[0]._EqRFmx.cRFmxNF.DoneNFCommit.Reset();

                                    ThreadPool.QueueUserWorkItem(Eq.Site[0]._EqRFmx.cRFmxNF.NFcommit2, new LibEqmtDriver.NF_VST.NF_NI_RFmx.RFmxNF(TestCount, j, rxFreqPoint, rxGainPoint));
                                    ThreadPool.QueueUserWorkItem(Eq.Site[0]._EqVST.VSGInitiate, new LibEqmtDriver.NF_VST.NF_NiPXI_VST.Config_SG(TestCount, j, TxPAOnScript, _Modulation, _WaveFormName, SGTargetPin, TXPAOnFreq[j]));

                                    Eq.Site[0]._EqVST.DoneVSGinit.WaitOne();
                                    Eq.Site[0]._EqRFmx.cRFmxNF.DoneNFCommit.WaitOne();

                                    Eq.Site[0]._EqRFmx.cRFmxNF.specNFColdSource2[TestCount][j].Initiate("", "HOT" + TestCount + "_" + i.ToString() + "_" + j.ToString());
                                    Eq.Site[0]._EqRFmx.WaitForAcquisitionComplete(1);
                                }
                                Eq.Site[0]._EqVST.rfsgSession.Abort(); //SG Abort after HotPA NF Test
                            }
                            #endregion

                            #region ResetRFSA and Re-configure after NF Measurement
                            Eq.Site[0]._EqVST.rfsaSession.Utility.Reset();
                            Eq.Site[0]._EqVST.PreConfig_VSTSA();
                            #endregion

                            #region Sort and Store Trace Data
                            //Store multi trace from PXI to global array
                            for (int n = 0; n < NumberOfRuns; n++)
                            {
                                //temp trace array storage use for MAX , MIN etc calculation 
                                PXITrace[TestCount].Enable = true;
                                PXITrace[TestCount].SoakSweep = preSoakSweep;
                                PXITrace[TestCount].TestNumber = _TestNum;
                                PXITrace[TestCount].TraceCount = NumberOfRuns;
                                PXITrace[TestCount].Multi_Trace[0][n].NoPoints = NoOfPts;
                                PXITrace[TestCount].Multi_Trace[0][n].RBW_Hz = _NF_BW * 1e06;
                                PXITrace[TestCount].Multi_Trace[0][n].FreqMHz = new double[NoOfPts];
                                PXITrace[TestCount].Multi_Trace[0][n].Ampl = new double[NoOfPts];
                                PXITrace[TestCount].Multi_Trace[0][n].Result_Header = _TestParaName;
                                PXITrace[TestCount].Multi_Trace[0][n].MXA_No = "PXI_NF_HOT_Trace";
                                PXITrace[TestCount].Multi_Trace[0][n].RxGain = new double[NoOfPts]; //Yoonchun

                                PXITraceRaw[TestCount].Multi_Trace[0][n].FreqMHz = new double[NoOfPts];
                                PXITraceRaw[TestCount].Multi_Trace[0][n].Ampl = new double[NoOfPts];
                                PXITraceRaw[TestCount].Multi_Trace[0][n].RxGain = new double[NoOfPts]; //Yoonchun

                                PXITrace[TestCount].Multi_Trace[0][n].TargetPout = Math.Round(_Pout1, 3);
                                PXITrace[TestCount].Multi_Trace[0][n].modulation = _Modulation;

                                foreach (string key in DicWaveFormAlias.Keys)
                                {
                                    if (key == _WaveFormName.ToUpper())
                                    {
                                        if (_WaveFormName.ToUpper() != "CW")
                                        {
                                            PXITrace[TestCount].Multi_Trace[0][n].waveform = DicWaveFormAlias[key].ToString();
                                        }
                                    }
                                }


                                for (istep = 0; istep < NoOfPts; istep++)
                                {
                                    PXITrace[TestCount].Multi_Trace[0][n].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                    PXITrace[TestCount].Multi_Trace[0][n].RxGain[istep] = Math.Round(RXContactGain[istep], 3);

                                    PXITraceRaw[TestCount].Multi_Trace[0][n].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                    PXITraceRaw[TestCount].Multi_Trace[0][n].RxGain[istep] = Math.Round(RXContactGain[istep], 3);
                                }
                            }
                            #endregion

                            #region build result

                            if (!b_GE_Header)
                            {
                                //Sort out test result for all traces and Add test result
                                for (int i = 0; i < PXITrace[TestCount].TraceCount; i++)
                                {
                                    if (i == 0)
                                    {
                                        if (_Test_Pin1)
                                        {
                                            //ResultBuilder.BuildResults(ref results, _TestParaName + rbwParamName + "_Pin1" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dBm", R_Pin1);        //coding bug -  rbwParamName not define 23/02/2018
                                            ResultBuilder.BuildResults(ref results, _TestParaName + "_" + _NF_BW + "MHz_Pin1" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dBm", R_Pin1);
                                        }
                                        if (_Test_Pout1)
                                        {
                                            //ResultBuilder.BuildResults(ref results, _TestParaName + rbwParamName + "_Pout1" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dBm", R_Pout1);      //coding bug -  rbwParamName not define 23/02/2018
                                            ResultBuilder.BuildResults(ref results, _TestParaName + "_" + _NF_BW + "MHz_Pout1" + "_" + _WaveFormName + "_" + _Pout1 + "dBm", "dBm", R_Pout1);
                                        }
                                        if (_Test_SMU)
                                        {
                                            MeasSMU = _SMUMeasCh.Split(',');
                                            for (int j = 0; j < MeasSMU.Count(); j++)
                                            {
                                                //ResultBuilder.BuildResults(ref results, _TestParaName + rbwParamName + "_" + R_SMULabel_ICh[Convert.ToInt16(MeasSMU[j])], "A", R_SMU_ICh[Convert.ToInt16(MeasSMU[j])]);      //coding bug -  rbwParamName not define 23/02/2018
                                                ResultBuilder.BuildResults(ref results, _TestParaName + "_" + _NF_BW + "MHz_" + R_SMULabel_ICh[Convert.ToInt16(MeasSMU[j])], "A", R_SMU_ICh[Convert.ToInt16(MeasSMU[j])]);
                                            }
                                        }
                                        if (_Test_NF1)
                                        {
                                        }
                                    }
                                    else
                                    {
                                        if (_Test_NF1)
                                        {
                                        }
                                    }
                                }

                                //Force test flag to false to ensure no repeated test data
                                //because we add to string builder upfront for PXI due to data reported base on number of sweep
                                _Test_Pin1 = false;
                                _Test_Pout1 = false;
                                _Test_SMU = false;

                                //Force test flag to false to ensure no repeated test data
                                //because we add to string builder upfront for PXI due to data reported base on number of sweep
                                _Test_NF1 = false;
                            }
                            else
                            {
                                //note - build header for Golden Eagle will use geneic function
                                b_SmuHeader = true;

                                //Force test flag to false to ensure no repeated test data
                                //because we add to string builder upfront for PXI due to data reported base on number of sweep
                                _Test_NF1 = false;
                            }

                            #endregion

                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}
                            #endregion
                            break;

                        case "PXI_NF_COLD_MIPI":
                            // This is using PXI VST as Sweeper and Analyzer. Will do multiple sweep in one function because using script (Pwr Servo->Soak Sweep->SoakTime->MultiSweep)
                            // Slight different from LXI solution where you define number of sweep in multiple line in TCF

                            #region PXI NF COLD MIPI
                            //NOTE: Some of these inputs may have to be read from input-excel or defined elsewhere
                            //Variable use in VST Measure Function

                            #region RxGain & Loss gatherring for NF Measurement

                            if (_PXI_NoOfSweep <= 0)                //check the number of sweep for pxi, set to default if user forget to keyin in excel
                                NumberOfRuns = 1;
                            else
                                NumberOfRuns = _PXI_NoOfSweep;

                            //For Collecting LNA Gain & Loss from previous Data -Seoul
                            NoOfPts = (Convert.ToInt32(Math.Ceiling((Math.Round(_StopRXFreq1, 3) - Math.Round(_StartRXFreq1, 3)) / Math.Round(_StepRXFreq1, 3)))) + 1;

                            RXContactFreq = new double[NoOfPts];
                            RXContactGain = new double[NoOfPts];
                            RXPathLoss = new double[NoOfPts];
                            LNAInputLoss = new double[NoOfPts];
                            TXPAOnFreq = new double[NoOfPts];
                            RxGainDic = new Dictionary<double, double>();

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand_HotNF.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            //For Collecting RX Gain trace Number from Previous setting -Seoul
                            TestUsePrev_ArrayNo = 0;
                            for (int i = 0; i < PXITrace.Length; i++)
                            {
                                if (Convert.ToInt16(_TestUsePrev) == PXITrace[i].TestNumber)
                                {
                                    TestUsePrev_ArrayNo = i;
                                }
                            }

                            for (int i = 0; i < PXITrace[TestUsePrev_ArrayNo].Multi_Trace[0][0].FreqMHz.Length; i++)
                            {
                                RxGainDic.Add(Math.Round(PXITrace[TestUsePrev_ArrayNo].Multi_Trace[0][0].FreqMHz[i], 3), PXITrace[TestUsePrev_ArrayNo].Multi_Trace[0][0].Ampl[i]);
                            }

                            _RXFreq = _StartRXFreq1;

                            count = Convert.ToInt16(Math.Ceiling((Math.Round(_StopRXFreq1, 3) - Math.Round(_StartRXFreq1, 3)) / Math.Round(_StepRXFreq1, 3)));

                            for (int i = 0; i <= count; i++)
                            {
                                RXContactFreq[i] = Math.Round(_RXFreq, 3);

                                if (RxGainDic.TryGetValue(Math.Round(_RXFreq, 3), out RXContactGain[i])) { }
                                else
                                {
                                    MessageBox.Show("Need to check between RxGain & NF Frequency Range");
                                }

                                _RXFreq = Convert.ToSingle(Math.Round(_RXFreq + _StepRXFreq1, 3));           //need to use round function because of C# float and double floating point bug/error

                                if (_RXFreq > _StopRXFreq1)//For Last Freq match
                                {
                                    _RXFreq = _StopRXFreq1;
                                }
                            }
                            #endregion

                            #region Switching for NF Test
                            //Switching for NF Testing -Seoul

                            SetswitchPath(DicCalInfo, DataFilePath.LocSettingPath, TCF_Header.ConstSwitching_Band_HotNF, _SwBand_HotNF, _Setup_Delay);
                            #endregion

                            #region Cold_MIPI NF Measurement

                            Eq.Site[0]._EqVST.rfsaSession.Utility.Reset();

                            Eq.Site[0]._EqVST.PreConfig_VSTSA();
                            Eq.Site[0]._EqVST.ConfigureTriggers();  //Disable Trigger for NF Testing
                            Eq.Site[0]._EqVST.rfsaSession.Configuration.Triggers.ReferenceTrigger.Disable(); //Disable Reference Trigger for NF Testing.

                            Eq.Site[0]._EqRFmx.cRFmxNF.specNFColdSource2[TestCount][RXContactFreq.Length].Commit(""); //Configure dummy setting before actual NF measurement

                            Eq.Site[0]._EqMiPiCtrl.BurstMIPIforNFR(3);

                            for (int i = 0; i < NumberOfRuns; i++)
                            {
                                for (int j = 0; j < RXContactFreq.Length; j++)
                                {
                                    double[] rxFreqPoint = new double[1];
                                    double[] rxGainPoint = new double[1];
                                    rxFreqPoint[0] = RXContactFreq[j];
                                    rxGainPoint[0] = RXContactGain[j];

                                    Eq.Site[0]._EqRFmx.cRFmxNF.DoneNFCommit.Reset();

                                    ThreadPool.QueueUserWorkItem(Eq.Site[0]._EqRFmx.cRFmxNF.NFcommit2, new LibEqmtDriver.NF_VST.NF_NI_RFmx.RFmxNF(TestCount, j, rxFreqPoint, rxGainPoint));

                                    Eq.Site[0]._EqRFmx.cRFmxNF.DoneNFCommit.WaitOne();

                                    Eq.Site[0]._EqRFmx.cRFmxNF.specNFColdSource2[TestCount][j].Initiate("", "COLD_MIPI" + TestCount + "_" + i.ToString() + "_" + j.ToString());

                                    Eq.Site[0]._EqRFmx.WaitForAcquisitionComplete(1);
                                }
                            }
                            #endregion

                            #region ResetRFSA and Re-configure after NF Measurement

                            Eq.Site[0]._EqVST.rfsaSession.Utility.Reset();
                            Eq.Site[0]._EqVST.PreConfig_VSTSA();

                            Eq.Site[0]._EqMiPiCtrl.AbortBurst();

                            #endregion

                            #region Sort and Store Trace Data
                            //Store multi trace from PXI to global array
                            for (int n = 0; n < NumberOfRuns; n++)
                            {
                                //temp trace array storage use for MAX , MIN etc calculation 
                                PXITrace[TestCount].Enable = true;
                                //PXITrace[TestCount].SoakSweep = preSoakSweep;
                                PXITrace[TestCount].TestNumber = _TestNum;
                                PXITrace[TestCount].TraceCount = NumberOfRuns;
                                PXITrace[TestCount].Multi_Trace[0][n].NoPoints = NoOfPts;
                                PXITrace[TestCount].Multi_Trace[0][n].RBW_Hz = _NF_BW * 1e06;
                                PXITrace[TestCount].Multi_Trace[0][n].FreqMHz = new double[NoOfPts];
                                PXITrace[TestCount].Multi_Trace[0][n].Ampl = new double[NoOfPts];
                                PXITrace[TestCount].Multi_Trace[0][n].Result_Header = _TestParaName;
                                PXITrace[TestCount].Multi_Trace[0][n].MXA_No = "PXI_NF_COLD_MIPI_Trace";
                                PXITrace[TestCount].Multi_Trace[0][n].RxGain = new double[NoOfPts]; //Yoonchun

                                PXITraceRaw[TestCount].Multi_Trace[0][n].FreqMHz = new double[NoOfPts];
                                PXITraceRaw[TestCount].Multi_Trace[0][n].Ampl = new double[NoOfPts];
                                PXITraceRaw[TestCount].Multi_Trace[0][n].RxGain = new double[NoOfPts]; //Yoonchun

                                for (istep = 0; istep < NoOfPts; istep++)
                                {
                                    PXITrace[TestCount].Multi_Trace[0][n].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                    PXITrace[TestCount].Multi_Trace[0][n].RxGain[istep] = Math.Round(RXContactGain[istep], 3); //Yoonchun

                                    PXITraceRaw[TestCount].Multi_Trace[0][n].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                    PXITraceRaw[TestCount].Multi_Trace[0][n].RxGain[istep] = Math.Round(RXContactGain[istep], 3); //Yoonchun
                                }
                            }
                            #endregion

                            //Force test flag to false to ensure no repeated test data
                            //because we add to string builder upfront for PXI due to data reported base on number of sweep
                            _Test_NF1 = false;
                            b_SmuHeader = true;

                            tTime.Stop();

                            //if (_Test_TestTime)
                            //{
                            //    BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}

                            #endregion
                            break;

                        case "PXI_NF_MEAS":

                            #region NF Measurement + Gain

                            if (_PXI_NoOfSweep <= 0)                //check the number of sweep for pxi, set to default if user forget to keyin in excel
                                NumberOfRuns = 1;
                            else
                                NumberOfRuns = _PXI_NoOfSweep;

                            NoOfPts = (Convert.ToInt32(Math.Ceiling((Math.Round(_StopRXFreq1, 3) - Math.Round(_StartRXFreq1, 3)) / Math.Round(_StepRXFreq1, 3)))) + 1;
                            double[] ipList = new double[NoOfPts];
                            double[] opList = new double[NoOfPts];
                            In_BoardLoss = new double[NoOfPts];
                            Out_BoardLoss = new double[NoOfPts];

                            #region Switching for Gain Test
                            //Switching for Gain Testing
                            SetswitchPath(DicCalInfo, DataFilePath.LocSettingPath, TCF_Header.ConstSwBand, _SwBand, _Setup_Delay);
                            #endregion

                            #region Decode MXA Config
                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", _SwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);

                            SAReferenceLevel = myUtility.MXA_Setting.RefLevel;
                            vBW_Hz = myUtility.MXA_Setting.VBW;
                            #endregion

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region Pathloss Data
                            RXContactFreq = new double[NoOfPts];
                            _RXFreq = _StartRXFreq1;

                            for (int i = 0; i < NoOfPts; i++)
                            {
                                RXContactFreq[i] = _RXFreq;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.BoardLossTag, myUtility.CalSegm_Setting.In_TBCalSegm, _RXFreq, ref In_BoardLoss[i], ref StrError);
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.BoardLossTag, myUtility.CalSegm_Setting.Out_TBCalSegm, _RXFreq, ref Out_BoardLoss[i], ref StrError);

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _RXFreq, ref _LossCouplerPath, ref StrError);
                                ipList[i] = Math.Round(_LossCouplerPath + In_BoardLoss[i], 3);

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                opList[i] = Math.Round(_LossOutputPathRX1 + Out_BoardLoss[i], 3);

                                _RXFreq = Convert.ToSingle(Math.Round(_RXFreq + _StepRXFreq1, 3));           //need to use round function because of C# float and double floating point bug/error

                                if (_RXFreq > _StopRXFreq1)//For Last Freq match
                                {
                                    _RXFreq = _StopRXFreq1;
                                }
                            }

                            //Find actual SG Power Level
                            SGTargetPin = Math.Round(_Pin1 - ipList.Average(), 3);

                            if (SGTargetPin > _SG1MaxPwr)       //exit test if SG Target Power is more that VST recommended Pout
                            {
                                break;
                            }

                            #endregion

                            #region Contact Check
                            //NF Test Config
                            RXContactdBm = new double[NoOfPts];
                            double[] S21 = new double[NoOfPts];
                            Eq.Site[0]._EqVST.RXContactCheck(SGTargetPin, _StartRXFreq1, _StopRXFreq1, _StepRXFreq1, SAReferenceLevel, out RXContactdBm);

                            for (int i = 0; i < RXContactdBm.Length; i++)
                            {
                                double tmpGain = (RXContactdBm[i].ToString().Contains("Nan") || RXContactdBm[i].ToString().Contains("Infinity") || RXContactdBm[i].ToString().Contains("∞")) ? 9999 : RXContactdBm[i];
                                S21[i] = (tmpGain - opList[i]) - _Pin1; //Seoul for RX Gain trace
                            }

                            #endregion

                            #region NF Measurement

                            #region Switching for NF Test
                            //Switching for NF Testing -Seoul
                            SetswitchPath(DicCalInfo, DataFilePath.LocSettingPath, TCF_Header.ConstSwitching_Band_HotNF, _SwBand_HotNF, _Setup_Delay);

                            #endregion

                            Cold_NF_new = new double[NumberOfRuns][];
                            Cold_NoisePower_new = new double[NumberOfRuns][];

                            for (int i = 0; i < NumberOfRuns; i++)
                            {
                                Cold_NF_new[i] = new double[NoOfPts];
                                Cold_NoisePower_new[i] = new double[NoOfPts];
                            }

                            Eq.Site[0]._EqVST.PreConfig_VSTSA();
                            Eq.Site[0]._EqVST.ConfigureTriggers();  //Disable Trigger for NF Testing

                            Eq.Site[0]._EqRFmx.cRFmxNF.specNFColdSource2[TestCount][1].Commit(""); //Configure dummy setting before actual NF measurement

                            for (int i = 0; i < NumberOfRuns; i++)
                            {
                                Eq.Site[0]._EqRFmx.cRFmxNF.specNFColdSource2[TestCount][0].Initiate("", "COLD" + TestCount.ToString() + "_" + i);
                                Eq.Site[0]._EqRFmx.WaitForAcquisitionComplete(1);
                            }

                            #region ResetRFSA and Re-configure after NF Measurement

                            Eq.Site[0]._EqVST.rfsaSession.Utility.Reset();
                            Eq.Site[0]._EqVST.PreConfig_VSTSA();

                            #endregion

                            #endregion

                            #region Fetch NF Data

                            for (int i = 0; i < NumberOfRuns; i++)
                            {
                                Eq.Site[0]._EqRFmx.cRFmxNF.RetrieveResults_NFColdSource(TestCount, 0, "result::" + "COLD" + TestCount.ToString() + "_" + i);

                                for (int j = 0; j < NoOfPts; j++)
                                {
                                    double Cold_NF_withoutGain = (Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j].ToString().Contains("NaN") || Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j].ToString().Contains("Infinity")) ? 9999 : Eq.Site[0]._EqRFmx.cRFmxNF.dutNoiseFigure[j];

                                    if (Cold_NF_withoutGain == 9999)
                                    {
                                        Cold_NF_new[i][j] = 9999;
                                    }

                                    else
                                    {
                                        Cold_NF_new[i][j] = Cold_NF_withoutGain - S21[j];
                                    }

                                    Cold_NoisePower_new[i][j] = Eq.Site[0]._EqRFmx.cRFmxNF.coldSourcePower[j] - opList[j];

                                }
                            }

                            #endregion

                            #region Test Param Log
                            if (!b_GE_Header)
                            {
                                for (int i = 0; i < NumberOfRuns; i++)
                                {
                                    for (int j = 0; j < NoOfPts; j++)
                                    {
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_" + _RX1Band + "_" + RXContactFreq[j] + "_RUN" + i + "_Gain", "dB", S21[j]);
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_" + _RX1Band + "_" + RXContactFreq[j] + "_RUN" + i + "_NF", "dB", Cold_NF_new[i][j]);
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_" + _RX1Band + "_" + RXContactFreq[j] + "_RUN" + i + "_ColdPower", "dBm", Cold_NoisePower_new[i][j]);
                                    }
                                }
                            }
                            else
                            {
                                b_SmuHeader = true;
                                string GE_TestParam = null;
                                Rslt_GE_Header = new s_GE_Header();
                                Decode_GE_Header(TestPara, out Rslt_GE_Header);

                                for (int i = 0; i < NumberOfRuns; i++)
                                {
                                    for (int j = 0; j < NoOfPts; j++)
                                    {
                                        Rslt_GE_Header.Freq1 = "_Rx-" + RXContactFreq[j] + "MHz";  //re-assign ge header

                                        Rslt_GE_Header.Param = "_NF";  //re-assign ge header
                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", Cold_NF_new[i][j]);

                                        Rslt_GE_Header.Param = "_Gain_Rx";      //re-assign ge header 
                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", S21[j]);

                                        Rslt_GE_Header.Param = "_Power_Cold";      //re-assign ge header 
                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "dBm", Cold_NoisePower_new[i][j]);
                                    }
                                }
                            }

                            _Test_NF1 = false;
                            _Test_Pin1 = false;

                            #endregion

                            tTime.Stop();

                            #endregion

                            break;

                        case "PXI_RXPATH_GAIN_NF": //Seoul
                                                   //this function is checking the pathloss/pathgain from antenna port to rx port

                            #region PXI_RXPATH_GAIN
                            R_NF1_Freq = -99999;
                            R_NF1_Ampl = 99999;

                            NoOfPts = (Convert.ToInt32(Math.Ceiling((Math.Round(_StopRXFreq1, 3) - Math.Round(_StartRXFreq1, 3)) / Math.Round(_StepRXFreq1, 3)))) + 1;
                            RXContactdBm = new double[NoOfPts];
                            RXContactFreq = new double[NoOfPts];
                            RXContactGain = new double[NoOfPts];//Seoul
                            RXPathLoss = new double[NoOfPts]; //Seoul
                            LNAInputLoss = new double[NoOfPts]; //Seoul
                            In_BoardLoss = new double[NoOfPts];
                            Out_BoardLoss = new double[NoOfPts];

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region Pathloss Offset

                            //Calculate PAPR offset for PXI SG
                            modulationType = (LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE), _WaveFormName.ToUpper());
                            modArrayNo = (int)Enum.Parse(modulationType.GetType(), modulationType.ToString()); // to get the int value from System.Enum
                            papr_dB = Math.Round(LibEqmtDriver.NF_VST.NF_VSTDriver.SignalType[modArrayNo].SG_papr_dB, 3);

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16(Math.Ceiling((Math.Round(_StopRXFreq1, 3) - Math.Round(_StartRXFreq1, 3)) / Math.Round(_StepRXFreq1, 3)));

                            _RXFreq = _StartRXFreq1;

                            for (int i = 0; i <= count; i++)
                            {
                                RXContactFreq[i] = Math.Round(_RXFreq, 3);

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                tmpRxLoss = Math.Round(tmpRxLoss + (float)_LossOutputPathRX1, 3);   //need to use round function because of C# float and double floating point bug/error

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _RXFreq, ref _LossCouplerPath, ref StrError);
                                tmpCouplerLoss = Math.Round(tmpCouplerLoss + (float)_LossCouplerPath, 3);   //need to use round function because of C# float and double floating point bug/error

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.BoardLossTag, myUtility.CalSegm_Setting.In_TBCalSegm, _RXFreq, ref In_BoardLoss[i], ref StrError);
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.BoardLossTag, myUtility.CalSegm_Setting.Out_TBCalSegm, _RXFreq, ref Out_BoardLoss[i], ref StrError);

                                _RXFreq = Convert.ToSingle(Math.Round(_RXFreq + _StepRXFreq1, 3));           //need to use round function because of C# float and double floating point bug/error

                                if (_RXFreq > _StopRXFreq1)
                                    _RXFreq = _StopRXFreq1;

                                RXPathLoss[i] = _LossOutputPathRX1;      //Seoul
                                LNAInputLoss[i] = _LossCouplerPath;       //Seoul
                            }

                            //get average boardloss
                            tbInputLoss = In_BoardLoss.Average();
                            tbOutputLoss = Out_BoardLoss.Average();

                            tmpAveRxLoss = tmpRxLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveCouplerLoss - tbInputLoss;       //pathloss from SG to ANT Port inclusive fixed TB Loss
                            totalOutputLoss = tmpAveRxLoss - tbOutputLoss;          //pathgain from RX Port to SA inclusive fixed TB Loss

                            //Find actual SG Power Level
                            SGTargetPin = _Pin1 - (totalInputLoss - papr_dB);
                            if (SGTargetPin > _SG1MaxPwr)       //exit test if SG Target Power is more that VST recommended Pout
                            {
                                break;
                            }

                            #region Decode MXA Config
                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", _SwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);

                            SAReferenceLevel = myUtility.MXA_Setting.RefLevel;
                            vBW_Hz = myUtility.MXA_Setting.VBW;
                            #endregion

                            #endregion

                            #region Test RX Path
                            Eq.Site[0]._EqVST.RXContactCheck(SGTargetPin, Math.Round(_StartRXFreq1, 3), Math.Round(_StopRXFreq1, 3), Math.Round(_StepRXFreq1, 3), SAReferenceLevel, out RXContactdBm);

                            for (int i = 0; i < RXContactdBm.Length; i++)
                            {
                                if (RXContactdBm[i].ToString().Contains("Infinity") || RXContactdBm[i].ToString().Contains("∞"))
                                {
                                    RXContactGain[i] = -999;
                                }
                                else
                                {
                                    double OrgTargetPin = _Pin1 - (LNAInputLoss[i] - In_BoardLoss[i]);
                                    RXContactGain[i] = (RXContactdBm[i] - RXPathLoss[i] - Out_BoardLoss[i]) - (_Pin1 + (SGTargetPin - OrgTargetPin)); //Seoul for RX Gain trace
                                }
                            }

                            //Sort out test result
                            switch (_Search_Method.ToUpper())
                            {
                                case "MAX":
                                    R_NF1_Ampl = RXContactGain.Max();
                                    R_NF1_Freq = RXContactFreq[Array.IndexOf(RXContactGain, R_NF1_Ampl)];
                                    break;

                                case "MIN":
                                    R_NF1_Ampl = RXContactGain.Min();
                                    R_NF1_Freq = RXContactFreq[Array.IndexOf(RXContactGain, R_NF1_Ampl)];
                                    break;

                                case "AVE":
                                case "AVERAGE":
                                    R_NF1_Ampl = RXContactGain.Average();
                                    R_NF1_Freq = RXContactFreq[0];          //return default freq i.e Start Freq
                                    break;

                                case "USER":
                                    //Note : this case required user to define freq that is within Start or Stop Freq and also same in step size
                                    if ((Convert.ToSingle(_Search_Value) >= _StartRXFreq1) && (Convert.ToSingle(_Search_Value) <= _StopRXFreq1))
                                    {
                                        try
                                        {
                                            R_NF1_Ampl = RXContactGain[Array.IndexOf(RXContactFreq, Convert.ToSingle(_Search_Value))];     //return contact power from same array number(of index number associated with 'USER' Freq)
                                            R_NF1_Freq = Convert.ToSingle(_Search_Value);
                                        }
                                        catch       //if _Search_Value not in RXContactFreq list , will return error . Eg. User Define 1840.5 but Freq List , 1839, 1840, 1841 - > program will fail because 1840.5 is not Exactly same in freq list
                                        {
                                            R_NF1_Freq = Convert.ToSingle(_Search_Value);
                                            R_NF1_Ampl = 99999;
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Test Parameter : " + _TestParam + "(SEARCH METHOD : " + _Search_Method + ", USER DEFINE : " + _Search_Value + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    }
                                    break;

                                default:
                                    MessageBox.Show("Test Parameter : " + _TestParam + "(" + _Search_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    break;
                            }

                            #endregion

                            #region Sort and Store Trace Data
                            //Store RX Contact from PXI to global array
                            //temp trace array storage use for MAX , MIN etc calculation 
                            rbw_counter = 0;

                            PXITrace[TestCount].Enable = true;
                            PXITrace[TestCount].SoakSweep = false;
                            PXITrace[TestCount].TestNumber = _TestNum;
                            PXITrace[TestCount].TraceCount = 1;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].NoPoints = NoOfPts;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].RBW_Hz = 1e6;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].FreqMHz = new double[NoOfPts];
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].Ampl = new double[NoOfPts];
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].Result_Header = _TestParaName;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].MXA_No = "PXI_RXCONTACT_Trace";

                            PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].FreqMHz = new double[NoOfPts];
                            PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].Ampl = new double[NoOfPts];

                            for (istep = 0; istep < NoOfPts; istep++)
                            {
                                PXITrace[TestCount].Multi_Trace[0][0].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                PXITrace[TestCount].Multi_Trace[0][0].Ampl[istep] = Math.Round(RXContactGain[istep], 3);

                                //Store Raw Trace Data to PXITraceRaw Array - Only actual data read from SA (not use in other than Save_PXI_TraceRaw function
                                PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].Ampl[istep] = Math.Round(RXContactGain[istep], 3);
                            }

                            Save_PXI_TraceRaw(_TestParaName, _TestUsePrev, _Save_MXATrace, rbw_counter, 1e6);

                            #endregion

                            if (_Test_NF1)      //Only display for NFR test Gain .. For Abs NF gain test - not required to display (else will have duplicate header)
                            {
                                for (int i = 0; i < NoOfPts; i++)
                                {
                                    if (!b_GE_Header)
                                    {
                                        ResultBuilder.BuildResults(ref results, _TestParaName + "_" + RXContactFreq[i] + "_Rx-Gain", "dB", RXContactGain[i].ToString().Contains("Infinity") ? -999 : RXContactGain[i]);
                                    }
                                    else
                                    {
                                        b_SmuHeader = true;
                                        string GE_TestParam = null;
                                        Rslt_GE_Header = new s_GE_Header();
                                        Decode_GE_Header(TestPara, out Rslt_GE_Header);

                                        Rslt_GE_Header.Freq1 = "_Rx-" + RXContactFreq[i] + "MHz";
                                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                                        ResultBuilder.BuildResults(ref results, GE_TestParam, "dB", RXContactGain[i].ToString().Contains("Infinity") ? -999 : RXContactGain[i]);
                                    }
                                }
                            }

                            //for test time checking
                            tTime.Stop();

                            #endregion
                            break;


                        case "PXI_NF_COLD_ALLINONE":
                        case "PXI_NF_COLD_MIPI_ALLINONE":
                            // Seoul, Ben
                            // This function can measure switch length setting, TRx mipi setting (with Voltage setting), Rx gain and nf cold at once.

                            #region PXI_NF_COLD_ALLINONE & PXI_NF_COLD_MIPI_ALLINONE

                            Task[] taskNFColdAllinOne = new Task[10];

                            #region Set Switch path for Rx Gain

                            taskNFColdAllinOne[0] = Task.Factory.StartNew(() => SetswitchPath(DicCalInfo, DataFilePath.LocSettingPath, TCF_Header.ConstSwBand, _SwBand, _Setup_Delay));

                            #endregion Set Switch path for Rx Gain

                            #region Tx, Rx MIPI Setting

                            EqmtStatus.SMU_CH = _SMUSetCh;

                            SetSMU = _SMUSetCh.Split(',');
                            MeasSMU = _SMUMeasCh.Split(',');

                            if (_Test_SMU)
                            {
                                SetSMUSelect = new string[SetSMU.Count()];
                                for (int i = 0; i < SetSMU.Count(); i++)
                                {
                                    int smuVChannel = Convert.ToInt16(SetSMU[i]);
                                    SetSMUSelect[i] = Eq.Site[0]._SMUSetting[smuVChannel];       //rearrange the Eq.Site[0]._SMUSetting base on reqquired channel only from total of 8 channel available  
                                    Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[smuVChannel], Eq.Site[0]._EqSMU, _SMUVCh[smuVChannel], _SMUILimitCh[smuVChannel]);
                                }

                                Eq.Site[0]._Eq_SMUDriver.DcOn(SetSMUSelect, Eq.Site[0]._EqSMU);
                            }
                            bool ReadSuccessFulSet1 = false, ReadSuccessFulSet2 = false;
                            int iMIPITestCount = 0;

                            if (_MIPI_Set1 != "" && _MIPI_Set1 != "0")
                            {
                                //Search and return Data from Mipi custom spreadsheet 
                                searchMIPIKey(_TestParam, _MIPI_Set1, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                                //Set MIPI
                                Eq.Site[0]._EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                                Eq.Site[0]._EqMiPiCtrl.SendAndReadMIPICodesCustom(out MIPI_Read_Successful, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                LibEqmtDriver.MIPI.Lib_Var.ReadSuccessful = MIPI_Read_Successful;
                                ReadSuccessFulSet1 = MIPI_Read_Successful;
                                iMIPITestCount++;
                            }
                            if (_MIPI_Set2 != "" && _MIPI_Set2 != "0")
                            {
                                //Search and return Data from Mipi custom spreadsheet 
                                searchMIPIKey(_TestParam, _MIPI_Set2, out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                                //Set MIPI
                                Eq.Site[0]._EqMiPiCtrl.TurnOn_VIO(Convert.ToInt16(CusMipiPair));        //mipi pair - derive from MIPI spereadsheet
                                Eq.Site[0]._EqMiPiCtrl.SendAndReadMIPICodesCustom(out MIPI_Read_Successful, CusMipiRegMap, CusPMTrigMap, Convert.ToInt16(CusMipiPair), Convert.ToInt32((CusSlaveAddr), 16));
                                LibEqmtDriver.MIPI.Lib_Var.ReadSuccessful = MIPI_Read_Successful;
                                ReadSuccessFulSet2 = MIPI_Read_Successful;
                                iMIPITestCount++;
                            }

                            if (iMIPITestCount > 1)
                            {
                                if (ReadSuccessFulSet1 && ReadSuccessFulSet2) R_MIPI = 1;
                                else if (ReadSuccessFulSet1 && !ReadSuccessFulSet2) R_MIPI = 2;
                                else if (!ReadSuccessFulSet1 && ReadSuccessFulSet2) R_MIPI = 3;
                                else R_MIPI = -999;
                            }
                            else
                            {
                                if (LibEqmtDriver.MIPI.Lib_Var.ReadSuccessful) R_MIPI = 1;
                                else R_MIPI = -999;
                            }


                            if (_Test_SMU)
                            {
                                DelayMs(_RdCurr_Delay);
                                float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                for (int i = 0; i < MeasSMU.Count(); i++)
                                {
                                    int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                    if (_SMUILimitCh[smuIChannel] > 0)
                                    {
                                        R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                    }

                                    // pass out the test result label for every measurement channel
                                    string tempLabel = "SMUI_CH" + MeasSMU[i];
                                    foreach (string key in DicTestLabel.Keys)
                                    {
                                        if (key == tempLabel)
                                        {
                                            R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                            break;
                                        }
                                    }
                                }
                            }

                            #endregion Tx, Rx MIPI Setting

                            #region Rx Gain Measure

                            R_NF1_Freq = -99999;
                            R_NF1_Ampl = 99999;

                            NoOfPts = (Convert.ToInt32(Math.Ceiling((Math.Round(_StopRXFreq1, 3) - Math.Round(_StartRXFreq1, 3)) / Math.Round(_StepRXFreq1, 3)))) + 1;
                            RXContactdBm = new double[NoOfPts];
                            RXContactFreq = new double[NoOfPts];
                            RXContactGain = new double[NoOfPts];//Seoul
                            RXPathLoss = new double[NoOfPts]; //Seoul
                            LNAInputLoss = new double[NoOfPts]; //Seoul
                            In_BoardLoss = new double[NoOfPts];
                            Out_BoardLoss = new double[NoOfPts];

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);
                            #endregion

                            #region Pathloss Offset

                            //Calculate PAPR offset for PXI SG
                            modulationType = (LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.NF_VST.VST_WAVEFORM_MODE), _WaveFormName.ToUpper());
                            modArrayNo = (int)Enum.Parse(modulationType.GetType(), modulationType.ToString()); // to get the int value from System.Enum
                            papr_dB = Math.Round(LibEqmtDriver.NF_VST.NF_VSTDriver.SignalType[modArrayNo].SG_papr_dB, 3);

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16(Math.Ceiling((Math.Round(_StopRXFreq1, 3) - Math.Round(_StartRXFreq1, 3)) / Math.Round(_StepRXFreq1, 3)));
                            _RXFreq = _StartRXFreq1;

                            for (int i = 0; i <= count; i++)
                            {
                                RXContactFreq[i] = Math.Round(_RXFreq, 3);

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                tmpRxLoss = Math.Round(tmpRxLoss + (float)_LossOutputPathRX1, 3);   //need to use round function because of C# float and double floating point bug/error

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _RXFreq, ref _LossCouplerPath, ref StrError);
                                tmpCouplerLoss = Math.Round(tmpCouplerLoss + (float)_LossCouplerPath, 3);   //need to use round function because of C# float and double floating point bug/error

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.BoardLossTag, myUtility.CalSegm_Setting.In_TBCalSegm, _RXFreq, ref In_BoardLoss[i], ref StrError);
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.BoardLossTag, myUtility.CalSegm_Setting.Out_TBCalSegm, _RXFreq, ref Out_BoardLoss[i], ref StrError);

                                _RXFreq = Convert.ToSingle(Math.Round(_RXFreq + _StepRXFreq1, 3));           //need to use round function because of C# float and double floating point bug/error

                                if (_RXFreq > _StopRXFreq1)
                                    _RXFreq = _StopRXFreq1;

                                RXPathLoss[i] = _LossOutputPathRX1;      //Seoul
                                LNAInputLoss[i] = _LossCouplerPath;       //Seoul
                            }

                            //get average boardloss
                            tbInputLoss = In_BoardLoss.Average();
                            tbOutputLoss = Out_BoardLoss.Average();

                            tmpAveRxLoss = tmpRxLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveCouplerLoss - tbInputLoss;       //pathloss from SG to ANT Port inclusive fixed TB Loss
                            totalOutputLoss = tmpAveRxLoss - tbOutputLoss;          //pathgain from RX Port to SA inclusive fixed TB Loss

                            //Find actual SG Power Level
                            SGTargetPin = _Pin1 - (totalInputLoss - papr_dB);
                            if (SGTargetPin > _SG1MaxPwr)       //exit test if SG Target Power is more that VST recommended Pout
                            {
                                break;
                            }

                            #region Decode MXA Config
                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", _SwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);

                            SAReferenceLevel = myUtility.MXA_Setting.RefLevel;
                            vBW_Hz = myUtility.MXA_Setting.VBW;
                            #endregion

                            #endregion

                            #region Test RX Path
                            Eq.Site[0]._EqVST.RXContactCheck(SGTargetPin, Math.Round(_StartRXFreq1, 3), Math.Round(_StopRXFreq1, 3), Math.Round(_StepRXFreq1, 3), SAReferenceLevel, out RXContactdBm);

                            for (int i = 0; i < RXContactdBm.Length; i++)
                            {
                                if (RXContactdBm[i].ToString().Contains("Infinity") || RXContactdBm[i].ToString().Contains("∞"))
                                {
                                    RXContactGain[i] = -999;
                                }
                                else
                                {
                                    double OrgTargetPin = _Pin1 - (LNAInputLoss[i] - In_BoardLoss[i]);
                                    RXContactGain[i] = (RXContactdBm[i] - RXPathLoss[i] - Out_BoardLoss[i]) - (_Pin1 + (SGTargetPin - OrgTargetPin)); //Seoul for RX Gain trace
                                }
                            }

                            //Sort out test result
                            switch (_Search_Method.ToUpper())
                            {
                                case "MAX":
                                    R_NF1_Ampl = RXContactGain.Max();
                                    R_NF1_Freq = RXContactFreq[Array.IndexOf(RXContactGain, R_NF1_Ampl)];
                                    break;

                                case "MIN":
                                    R_NF1_Ampl = RXContactGain.Min();
                                    R_NF1_Freq = RXContactFreq[Array.IndexOf(RXContactGain, R_NF1_Ampl)];
                                    break;

                                case "AVE":
                                case "AVERAGE":
                                    R_NF1_Ampl = RXContactGain.Average();
                                    R_NF1_Freq = RXContactFreq[0];          //return default freq i.e Start Freq
                                    break;

                                case "USER":
                                    //Note : this case required user to define freq that is within Start or Stop Freq and also same in step size
                                    if ((Convert.ToSingle(_Search_Value) >= _StartRXFreq1) && (Convert.ToSingle(_Search_Value) <= _StopRXFreq1))
                                    {
                                        try
                                        {
                                            R_NF1_Ampl = RXContactGain[Array.IndexOf(RXContactFreq, Convert.ToSingle(_Search_Value))];     //return contact power from same array number(of index number associated with 'USER' Freq)
                                            R_NF1_Freq = Convert.ToSingle(_Search_Value);
                                        }
                                        catch       //if _Search_Value not in RXContactFreq list , will return error . Eg. User Define 1840.5 but Freq List , 1839, 1840, 1841 - > program will fail because 1840.5 is not Exactly same in freq list
                                        {
                                            R_NF1_Freq = Convert.ToSingle(_Search_Value);
                                            R_NF1_Ampl = 99999;
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("Test Parameter : " + _TestParam + "(SEARCH METHOD : " + _Search_Method + ", USER DEFINE : " + _Search_Value + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    }
                                    break;

                                default:
                                    MessageBox.Show("Test Parameter : " + _TestParam + "(" + _Search_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                    break;
                            }

                            #endregion Test RX Path

                            #region Store - Rx Gain Data

                            if (_TestParam.ToUpper().Contains("PXI_NF_COLD_ALLINONE")) rbw_counter = 1; // 0 : for NF Cold Data , 1 : for Rx Gain Data
                            else if (_TestParam.ToUpper().Contains("PXI_NF_COLD_MIPI_ALLINONE")) rbw_counter = 2; // 0 : for NF Cold Data , 1 : for Rx Gain Data

                            PXITrace[TestCount].Enable = true;
                            PXITrace[TestCount].SoakSweep = false;
                            PXITrace[TestCount].TestNumber = _TestNum;
                            PXITrace[TestCount].TraceCount = 1;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].NoPoints = NoOfPts;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].RBW_Hz = 1e6;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].FreqMHz = new double[NoOfPts];
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].Ampl = new double[NoOfPts];
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].Result_Header = _TestParaName;
                            PXITrace[TestCount].Multi_Trace[rbw_counter][0].MXA_No = "PXI_RXContact_Trace_AllInOne";

                            PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].FreqMHz = new double[NoOfPts];
                            PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].Ampl = new double[NoOfPts];

                            for (istep = 0; istep < NoOfPts; istep++)
                            {
                                PXITrace[TestCount].Multi_Trace[rbw_counter][0].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                PXITrace[TestCount].Multi_Trace[rbw_counter][0].Ampl[istep] = Math.Round(RXContactGain[istep], 3);

                                //Store Raw Trace Data to PXITraceRaw Array - Only actual data read from SA (not use in other than Save_PXI_TraceRaw function
                                PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].FreqMHz[istep] = Math.Round(RXContactFreq[istep], 3);
                                PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].Ampl[istep] = Math.Round(RXContactGain[istep], 3);
                            }

                            Save_PXI_TraceRaw(_TestParaName, _TestUsePrev, _Save_MXATrace, rbw_counter, 1e6);

                            #endregion Store - Rx Gain Data

                            #endregion Rx Gain Measure

                            #region NF Cold Measure

                            RxGainDic = new Dictionary<double, double>();

                            if (_PXI_NoOfSweep <= 0)                //check the number of sweep for pxi, set to default if user forget to keyin in excel
                                NumberOfRuns = 1;
                            else
                                NumberOfRuns = _PXI_NoOfSweep;

                            for (int i = 0; i < PXITrace[TestCount].Multi_Trace[rbw_counter][0].FreqMHz.Length; i++)
                            {
                                RxGainDic.Add(Math.Round(PXITrace[TestCount].Multi_Trace[rbw_counter][0].FreqMHz[i], 3), PXITrace[TestCount].Multi_Trace[rbw_counter][0].Ampl[i]);
                            }

                            #region Switching for NF Test
                            //Switching for NF Testing -Seoul

                            SetswitchPath(DicCalInfo, DataFilePath.LocSettingPath, TCF_Header.ConstSwitching_Band_HotNF, _SwBand_HotNF, _Setup_Delay);
                            #endregion

                            #region Cold NF Measurement
                            Cold_NF_new = new double[NumberOfRuns][];
                            Cold_NoisePower_new = new double[NumberOfRuns][];

                            for (int i = 0; i < NumberOfRuns; i++)
                            {
                                Cold_NF_new[i] = new double[NoOfPts];
                                Cold_NoisePower_new[i] = new double[NoOfPts];
                            }

                            Eq.Site[0]._EqVST.rfsaSession.Utility.Reset();

                            Eq.Site[0]._EqVST.PreConfig_VSTSA();
                            Eq.Site[0]._EqVST.ConfigureTriggers();  //Disable Trigger for NF Testing
                            Eq.Site[0]._EqVST.rfsaSession.Configuration.Triggers.ReferenceTrigger.Disable(); //Disable Reference Trigger for NF Testing.

                            Eq.Site[0]._EqRFmx.cRFmxNF.specNFColdSource2[TestCount][1].Commit(""); //Configure dummy setting before actual NF measurement

                            for (int i = 0; i < NumberOfRuns; i++)
                            {
                                Eq.Site[0]._EqRFmx.cRFmxNF.specNFColdSource2[TestCount][0].Initiate("", "COLD" + TestCount.ToString() + "_" + i);
                                Eq.Site[0]._EqRFmx.WaitForAcquisitionComplete(1);
                            }
                            #endregion

                            #region ResetRFSA and Re-configure after NF Measurement

                            Eq.Site[0]._EqVST.rfsaSession.Utility.Reset();
                            Eq.Site[0]._EqVST.PreConfig_VSTSA();

                            #endregion

                            #region Store data - NF Cold
                            //Store RX Contact from PXI to global array
                            //temp trace array storage use for MAX , MIN etc calculation 


                            for (int n = 0; n < NumberOfRuns; n++)
                            {
                                //temp trace array storage use for MAX , MIN etc calculation 
                                PXITrace[TestCount].TraceCount = NumberOfRuns;
                                PXITrace[TestCount].Multi_Trace[0][n].NoPoints = NoOfPts;
                                PXITrace[TestCount].Multi_Trace[0][n].RBW_Hz = _NF_BW * 1e06;
                                PXITrace[TestCount].Multi_Trace[0][n].FreqMHz = new double[NoOfPts];
                                PXITrace[TestCount].Multi_Trace[0][n].Ampl = new double[NoOfPts];
                                PXITrace[TestCount].Multi_Trace[0][n].Result_Header = _TestParaName;
                                PXITrace[TestCount].Multi_Trace[0][n].MXA_No = "PXI_NF_COLD_Trace_AllInOne";
                                PXITrace[TestCount].Multi_Trace[0][n].RxGain = new double[NoOfPts]; //Yoonchun

                                PXITraceRaw[TestCount].Multi_Trace[0][n].FreqMHz = new double[NoOfPts];
                                PXITraceRaw[TestCount].Multi_Trace[0][n].Ampl = new double[NoOfPts];
                                PXITraceRaw[TestCount].Multi_Trace[0][n].RxGain = new double[NoOfPts]; //Yoonchun

                                for (istep = 0; istep < NoOfPts; istep++)
                                {
                                    PXITrace[TestCount].Multi_Trace[0][n].FreqMHz[istep] = PXITrace[TestCount].Multi_Trace[rbw_counter][0].FreqMHz[istep];
                                    PXITrace[TestCount].Multi_Trace[0][n].RxGain[istep] = PXITrace[TestCount].Multi_Trace[rbw_counter][0].Ampl[istep]; //Yoonchun

                                    PXITraceRaw[TestCount].Multi_Trace[0][n].FreqMHz[istep] = PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].FreqMHz[istep];
                                    PXITraceRaw[TestCount].Multi_Trace[0][n].RxGain[istep] = PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].Ampl[istep]; //Yoonchun
                                }
                            }

                            #endregion Store data - NF Cold

                            #endregion NF Cold Measure

                            if (_TestParam.ToUpper().Contains("PXI_NF_COLD_ALLINONE"))
                            {
                                tTime.Stop();
                                break;
                            }

                            #region NF Cold MIPI Measure

                            int iColdNFMIPI_Counter = 1;

                            #region Cold_MIPI NF Measurement
                            Eq.Site[0]._EqVST.rfsaSession.Utility.Reset();

                            Eq.Site[0]._EqVST.PreConfig_VSTSA();
                            Eq.Site[0]._EqVST.ConfigureTriggers();  //Disable Trigger for NF Testing
                            Eq.Site[0]._EqVST.rfsaSession.Configuration.Triggers.ReferenceTrigger.Disable(); //Disable Reference Trigger for NF Testing.

                            Eq.Site[0]._EqRFmx.cRFmxNF.specNFColdSource2[TestCount][RXContactFreq.Length].Commit(""); //Configure dummy setting before actual NF measurement

                            Eq.Site[0]._EqMiPiCtrl.BurstMIPIforNFR(3);

                            for (int i = 0; i < NumberOfRuns; i++)
                            {
                                for (int j = 0; j < RXContactFreq.Length; j++)
                                {
                                    double[] rxFreqPoint = new double[1];
                                    double[] rxGainPoint = new double[1];
                                    rxFreqPoint[0] = RXContactFreq[j];
                                    rxGainPoint[0] = RXContactGain[j];

                                    Eq.Site[0]._EqRFmx.cRFmxNF.DoneNFCommit.Reset();

                                    ThreadPool.QueueUserWorkItem(Eq.Site[0]._EqRFmx.cRFmxNF.NFcommit2, new LibEqmtDriver.NF_VST.NF_NI_RFmx.RFmxNF(TestCount, j, rxFreqPoint, rxGainPoint));

                                    Eq.Site[0]._EqRFmx.cRFmxNF.DoneNFCommit.WaitOne();

                                    Eq.Site[0]._EqRFmx.cRFmxNF.specNFColdSource2[TestCount][j + 1].Initiate("", "COLD_MIPI" + TestCount + "_" + i.ToString() + "_" + j.ToString());

                                    Eq.Site[0]._EqRFmx.WaitForAcquisitionComplete(1);
                                }
                            }
                            #endregion

                            #region ResetRFSA and Re-configure after NF Measurement

                            Eq.Site[0]._EqVST.rfsaSession.Utility.Reset();
                            Eq.Site[0]._EqVST.PreConfig_VSTSA();

                            Eq.Site[0]._EqMiPiCtrl.AbortBurst();

                            #endregion

                            #region Sort and Store Trace Data
                            //Store multi trace from PXI to global array
                            for (int n = 0; n < NumberOfRuns; n++)
                            {
                                //temp trace array storage use for MAX , MIN etc calculation 
                                PXITrace[TestCount].Enable = true;
                                //PXITrace[TestCount].SoakSweep = preSoakSweep;
                                PXITrace[TestCount].TestNumber = _TestNum;
                                PXITrace[TestCount].TraceCount = NumberOfRuns;
                                PXITrace[TestCount].Multi_Trace[iColdNFMIPI_Counter][n].NoPoints = NoOfPts;
                                PXITrace[TestCount].Multi_Trace[iColdNFMIPI_Counter][n].RBW_Hz = _NF_BW * 1e06;
                                PXITrace[TestCount].Multi_Trace[iColdNFMIPI_Counter][n].FreqMHz = new double[NoOfPts];
                                PXITrace[TestCount].Multi_Trace[iColdNFMIPI_Counter][n].Ampl = new double[NoOfPts];
                                PXITrace[TestCount].Multi_Trace[iColdNFMIPI_Counter][n].Result_Header = _TestParaName;
                                PXITrace[TestCount].Multi_Trace[iColdNFMIPI_Counter][n].MXA_No = "PXI_NF_COLD_MIPI_Trace_AllInOne";
                                PXITrace[TestCount].Multi_Trace[iColdNFMIPI_Counter][n].RxGain = new double[NoOfPts]; //Yoonchun

                                PXITraceRaw[TestCount].Multi_Trace[iColdNFMIPI_Counter][n].FreqMHz = new double[NoOfPts];
                                PXITraceRaw[TestCount].Multi_Trace[iColdNFMIPI_Counter][n].Ampl = new double[NoOfPts];
                                PXITraceRaw[TestCount].Multi_Trace[iColdNFMIPI_Counter][n].RxGain = new double[NoOfPts]; //Yoonchun

                                for (istep = 0; istep < NoOfPts; istep++)
                                {
                                    PXITrace[TestCount].Multi_Trace[iColdNFMIPI_Counter][n].FreqMHz[istep] = PXITrace[TestCount].Multi_Trace[rbw_counter][0].FreqMHz[istep];
                                    PXITrace[TestCount].Multi_Trace[iColdNFMIPI_Counter][n].RxGain[istep] = PXITrace[TestCount].Multi_Trace[rbw_counter][0].Ampl[istep]; //Yoonchun

                                    PXITraceRaw[TestCount].Multi_Trace[iColdNFMIPI_Counter][n].FreqMHz[istep] = PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].FreqMHz[istep];
                                    PXITraceRaw[TestCount].Multi_Trace[iColdNFMIPI_Counter][n].RxGain[istep] = PXITraceRaw[TestCount].Multi_Trace[rbw_counter][0].Ampl[istep]; //Yoonchun
                                }
                            }
                            #endregion


                            #endregion NF Cold MIPI Measure

                            #endregion PXI_NF_COLD_ALLINONE & PXI_NF_COLD_MIPI_ALLINONE
                            tTime.Stop();

                            break;

                        default:
                            MessageBox.Show("NF Test Parameter : " + _TestParam.ToUpper() + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                            #endregion
                    }
                    break;

                case "MKR_NF":
                    switch (_TestParam.ToUpper())
                    {
                        #region LXI FBAR NOISE TEST - Using MARKER NOISE for normalization
                        //Using Marker Noise function during pathloss calibration to normalize dB/xMHz Noise Floor to dB/Hz
                        //Normalize the result with Calibration Tag -> RFOUT_xxxxx_MKRNoise_xMHz

                        case "NF_CA_NDIAG":
                            // This sweep is a faster sweep , it is a continuous sweep base on SG freq sweep mode
                            #region NF CA NDIAG

                            prevRslt = 0;
                            status = false;
                            pwrSearch = false;
                            Index = 0;
                            tx1_span = 0;
                            tx1_noPoints = 0;
                            rx1_span = 0;
                            rx1_cntrfreq = 0;
                            rx2_span = 0;
                            rx2_cntrfreq = 0;
                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(_PoutTolerance);
                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                            {
                                tolerancePwr = 0.5;
                            }

                            //use for searching previous result - to get the DUT LNA gain from previous result
                            if (Convert.ToInt16(_TestUsePrev) > 0)
                            {
                                usePrevRslt = true;
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                prevRslt = Math.Round(ReportRslt(_TestUsePrev, resultTag), 3);
                            }

                            //DelayMs(_StartSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_CALTAG", _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", _SwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);
                            rbwParamName = "_" + Math.Abs(myUtility.MXA_Setting.RBW / 1e6).ToString() + "MHz";
                            #endregion

                            #region PowerSensor Offset, MXG , MXA1 and MXA2 configuration

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((_StopTXFreq1 - _StartTXFreq1) / _StepTXFreq1);
                            _TXFreq = _StartTXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, _TXFreq, ref _LossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)_LossInputPathSG1;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _TXFreq, ref _LossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)_LossCouplerPath;
                                _TXFreq = _TXFreq + _StepTXFreq1;
                            }

                            tmpAveInputLoss = tmpInputLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            #region MXA1 Marker Offset Calculation
                            tmpMkrNoiseLoss = 0;
                            count = Convert.ToInt16((_StopRXFreq1 - _StartRXFreq1) / _StepRXFreq1);
                            _RXFreq = _StartRXFreq1;
                            MkrCalSegmTag = myUtility.CalSegm_Setting.RX1CalSegm + "_MKRNoise_" + myUtility.MXA_Setting.RBW / 1e6 + "MHz";
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, MkrCalSegmTag, _RXFreq, ref mkrNoiseLoss, ref StrError);
                                tmpMkrNoiseLoss = tmpMkrNoiseLoss + mkrNoiseLoss;
                                _RXFreq = _RXFreq + _StepRXFreq1;
                            }
                            AveMkrNoiseLossRX1 = tmpMkrNoiseLoss / (count + 1);
                            #endregion

                            #region MXA2 Marker Offset Calculation
                            tmpMkrNoiseLoss = 0;
                            count = Convert.ToInt16((_StopRXFreq2 - _StartRXFreq2) / _StepRXFreq2);
                            _RXFreq = _StartRXFreq2;
                            MkrCalSegmTag = myUtility.CalSegm_Setting.RX2CalSegm + "_MKRNoise_" + myUtility.MXA_Setting.RBW / 1e6 + "MHz";
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, MkrCalSegmTag, _RXFreq, ref mkrNoiseLoss, ref StrError);
                                tmpMkrNoiseLoss = tmpMkrNoiseLoss + mkrNoiseLoss;
                                _RXFreq = _RXFreq + _StepRXFreq2;
                            }
                            AveMkrNoiseLossRX2 = tmpMkrNoiseLoss / (count + 1);
                            #endregion

                            //change PowerSensor, MXG setting
                            Eq.Site[0]._EqPwrMeter.SetOffset(1, totalOutputLoss);
                            Eq.Site[0]._EqSG01.SetFreq(Convert.ToDouble(_SG1_DefaultFreq));

                            if (PreviousMXAMode != _SwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                tx1_span = _StopTXFreq1 - _StartTXFreq1;
                                tx1_noPoints = Convert.ToInt16(tx1_span / _StepTXFreq1);
                                Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.LIST);
                                Eq.Site[0]._EqSG01.SET_LIST_TYPE(LibEqmtDriver.SG.N5182_LIST_TYPE.STEP);
                                Eq.Site[0]._EqSG01.SET_LIST_MODE(LibEqmtDriver.SG.INSTR_MODE.AUTO);
                                Eq.Site[0]._EqSG01.SET_LIST_TRIG_SOURCE(LibEqmtDriver.SG.N5182_TRIG_TYPE.TIM);
                                Eq.Site[0]._EqSG01.SET_CONT_SWEEP(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);

                                Eq.Site[0]._EqSG01.SET_START_FREQUENCY(_StartTXFreq1 - (_StepTXFreq1 / 2));
                                Eq.Site[0]._EqSG01.SET_STOP_FREQUENCY(_StopTXFreq1 + (_StepTXFreq1 / 2));
                                Eq.Site[0]._EqSG01.SET_TRIG_TIMERPERIOD(_DwellT1);
                                Eq.Site[0]._EqSG01.SET_SWEEP_POINT(tx1_noPoints + 2);   //need to add additional 2 points to calculated no of points because of extra point of start_freq and stop_freq for MXG and MXA sync

                                SGTargetPin = _Pin1 - totalInputLoss;
                                Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                Eq.Site[0]._ModulationType = (LibEqmtDriver.SG.N5182A_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.SG.N5182A_WAVEFORM_MODE), _WaveFormName);
                                Eq.Site[0]._EqSG01.SELECT_WAVEFORM(Eq.Site[0]._ModulationType);
                                Eq.Site[0]._EqSG01.SET_ROUTE_CONN_TOUT(LibEqmtDriver.SG.N5182A_ROUTE_SUBSYS.SweepRun);

                                Eq.Site[0]._EqSG01.SINGLE_SWEEP();      //need to sweep SG for power search - RF ON in sweep mode
                                if (_SetFullMod)
                                {
                                    //This setting will set the modulation for N5182A to full modulation
                                    //Found out that when this set to default (RMS) , the modulation is mutated (CW + Mod) when running under sweep mode for NF
                                    Eq.Site[0]._EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.Mod);
                                }
                                else
                                {
                                    Eq.Site[0]._EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.RMS);
                                }
                                #endregion

                                #region MXA 1 setup
                                DelayMs(_Setup_Delay);
                                rx1_span = (_StopRXFreq1 - _StartRXFreq1);
                                rx1_cntrfreq = _StartRXFreq1 + (rx1_span / 2);
                                rx1_mxa_nopts = (int)((rx1_span / rx1_mxa_nopts_step) + 1);

                                Eq.Site[0]._EqSA01.Select_Instrument(LibEqmtDriver.SA.N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);
                                //Eq.Site[0]._EqSA01.AUTO_ATTENUATION(true); //Anthony
                                Eq.Site[0]._EqSA01.AUTO_ATTENUATION(false); //Anthony
                                if (Convert.ToDouble(_SA1att) != CurrentSaAttn) //Anthony
                                {
                                    Eq.Site[0]._EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToDouble(_SA1att));
                                    CurrentSaAttn = Convert.ToDouble(_SA1att);
                                }
                                Eq.Site[0]._EqSA01.TRIGGER_SINGLE();
                                Eq.Site[0]._EqSA01.TRACE_AVERAGE(1);
                                Eq.Site[0]._EqSA01.AVERAGE_OFF();

                                Eq.Site[0]._EqSA01.FREQ_CENT(rx1_cntrfreq.ToString(), "MHz");
                                Eq.Site[0]._EqSA01.SPAN(rx1_span);
                                Eq.Site[0]._EqSA01.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                Eq.Site[0]._EqSA01.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                Eq.Site[0]._EqSA01.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);

                                //Eq.Site[0]._EqSA01.SWEEP_POINTS(myUtility.MXA_Setting.NoPoints);
                                Eq.Site[0]._EqSA01.SWEEP_POINTS(rx1_mxa_nopts);

                                if (_SetRX1NDiag)
                                {
                                    Eq.Site[0]._EqSA01.CONTINUOUS_MEASUREMENT_ON();
                                    Eq.Site[0]._EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToInt16(_SA1att));
                                    Eq.Site[0]._EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    trigDelay = (decimal)_DwellT1 + (decimal)0.1;       //fixed 0.1ms delay
                                    Eq.Site[0]._EqSA01.SET_TRIG_DELAY(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1, trigDelay.ToString());
                                    Eq.Site[0]._EqSA01.SWEEP_TIMES(Convert.ToInt16(tx1_noPoints * _DwellT1));
                                }
                                else
                                {
                                    Eq.Site[0]._EqSA01.CONTINUOUS_MEASUREMENT_ON();
                                    Eq.Site[0]._EqSA01.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                    Eq.Site[0]._EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    Eq.Site[0]._EqSA01.SWEEP_TIMES(Convert.ToInt16(_RX1SweepT));
                                }

                                //Initialize & clear MXA trace
                                Eq.Site[0]._EqSA01.MARKER_TURN_ON_NORMAL_POINT(1, (float)rx1_cntrfreq);
                                Eq.Site[0]._EqSA01.CLEAR_WRITE();

                                status = Eq.Site[0]._EqSA01.OPERATION_COMPLETE();

                                #endregion

                                #region MXA 2 setup
                                DelayMs(_Setup_Delay);
                                rx2_span = (_StopRXFreq2 - _StartRXFreq2);
                                rx2_cntrfreq = _StartRXFreq2 + (rx2_span / 2);
                                rx2_mxa_nopts = (int)((rx2_span / rx2_mxa_nopts_step) + 1);

                                Eq.Site[0]._EqSA02.Select_Instrument(LibEqmtDriver.SA.N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);
                                //Eq.Site[0]._EqSA02.AUTO_ATTENUATION(true); //Anthony
                                Eq.Site[0]._EqSA02.AUTO_ATTENUATION(false);
                                if (Convert.ToDouble(_SA1att) != CurrentSa2Attn) //Anthony
                                {
                                    Eq.Site[0]._EqSA02.AMPLITUDE_INPUT_ATTENUATION(Convert.ToDouble(_SA2att));
                                    CurrentSaAttn = Convert.ToDouble(_SA2att);
                                }
                                Eq.Site[0]._EqSA02.TRIGGER_SINGLE();
                                Eq.Site[0]._EqSA02.TRACE_AVERAGE(1);
                                Eq.Site[0]._EqSA02.AVERAGE_OFF();

                                Eq.Site[0]._EqSA02.FREQ_CENT(rx2_cntrfreq.ToString(), "MHz");
                                Eq.Site[0]._EqSA02.SPAN(rx2_span);
                                Eq.Site[0]._EqSA02.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                Eq.Site[0]._EqSA02.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                Eq.Site[0]._EqSA02.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);

                                Eq.Site[0]._EqSA02.SWEEP_POINTS(rx2_mxa_nopts);
                                //Eq.Site[0]._EqSA02.SWEEP_POINTS(myUtility.MXA_Setting.NoPoints);

                                if (_SetRX2NDiag)
                                {
                                    Eq.Site[0]._EqSA02.CONTINUOUS_MEASUREMENT_ON();
                                    Eq.Site[0]._EqSA02.AMPLITUDE_INPUT_ATTENUATION(Convert.ToInt16(_SA1att));
                                    Eq.Site[0]._EqSA02.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    trigDelay = (decimal)_DwellT1 + (decimal)0.1;       //fixed 0.1ms delay
                                    Eq.Site[0]._EqSA02.SET_TRIG_DELAY(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1, trigDelay.ToString());
                                    Eq.Site[0]._EqSA02.SWEEP_TIMES(Convert.ToInt16(tx1_noPoints * _DwellT1));
                                }
                                else
                                {
                                    Eq.Site[0]._EqSA02.CONTINUOUS_MEASUREMENT_ON();
                                    Eq.Site[0]._EqSA02.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                    Eq.Site[0]._EqSA02.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    Eq.Site[0]._EqSA02.SWEEP_TIMES(Convert.ToInt16(_RX2SweepT));
                                }

                                //Initialize & clear MXA trace
                                Eq.Site[0]._EqSA02.MARKER_TURN_ON_NORMAL_POINT(1, (float)rx2_cntrfreq);
                                Eq.Site[0]._EqSA02.CLEAR_WRITE();

                                status = Eq.Site[0]._EqSA02.OPERATION_COMPLETE();

                                #endregion

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = _SwBand.ToUpper();
                            }
                            #endregion

                            #region measure contact power (Pout1)
                            if (StopOnFail.TestFail == false)
                            {
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.LIST);
                                if (!_TunePwr_TX1)
                                {
                                    StopOnFail.TestFail = true;     //init to fail state as default
                                    if (_Test_Pout1)
                                    {
                                        DelayMs(_RdPwr_Delay);
                                        R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = Math.Round(SGTargetPin + totalInputLoss, 3);
                                        if (Math.Abs(_Pout1 - R_Pout1) <= (tolerancePwr + 3.5))
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                        }
                                    }
                                    else
                                    {
                                        //No Pout measurement required, default set flag to pass
                                        pwrSearch = true;
                                        StopOnFail.TestFail = false;
                                    }
                                }
                                else
                                {
                                    do
                                    {
                                        StopOnFail.TestFail = true;     //init to fail state as default

                                        R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                        //R_Pin = TargetPin + (float)_LossInputPathSG1;
                                        R_Pin1 = SGTargetPin + totalInputLoss;

                                        if (Math.Abs(_Pout1 - R_Pout1) >= tolerancePwr)
                                        {
                                            if ((Index == 0) && (SGTargetPin < _SG1MaxPwr))   //preset to initial target power for 1st measurement count
                                            {
                                                Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                                R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                            }

                                            SGTargetPin = SGTargetPin + (_Pout1 - R_Pout1);

                                            if (SGTargetPin < _SG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                            {
                                                Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                                DelayMs(_RdPwr_Delay);
                                            }
                                        }
                                        else if (SGTargetPin > _SG1MaxPwr)      //if input sig gen exit limit , exit pwr search loop
                                        {
                                            SGTargetPin = _Pin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                            break;
                                        }
                                        else
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                            break;
                                        }
                                        Index++;
                                    }
                                    while (Index < 10);     // max power search loop
                                }
                            }

                            #endregion

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)_StartSync_Delay)
                            {
                                syncTest_Delay = (long)_StartSync_Delay - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            if (pwrSearch)
                            {
                                Eq.Site[0]._EqSG01.SINGLE_SWEEP();
                                status = Eq.Site[0]._EqSG01.OPERATION_COMPLETE();

                                //Need to turn off sweep mode - interference when running multisite because SG will go back to start freq once completed sweep
                                Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.CW);       //setting will set back to default freq define earlier

                                DelayMs(_Trig_Delay);
                                Capture_MXA1_Trace(1, _TestNum, _TestParaName, _RX1Band, prevRslt, _Save_MXATrace, AveMkrNoiseLossRX1);     //add mkrNoise offset to normalize the MXA trace to dB/Hz
                                Read_MXA1_Trace(1, _TestNum, out R_NF1_Freq, out R_NF1_Ampl, _Search_Method, _TestParaName);
                                Capture_MXA2_Trace(1, _TestNum, _TestParaName, _RX2Band, prevRslt, _Save_MXATrace, AveMkrNoiseLossRX2);     //add mkrNoise offset to normalize the MXA trace to dB/Hz
                                Read_MXA2_Trace(1, _TestNum, out R_NF2_Freq, out R_NF2_Ampl, _Search_Method, _TestParaName);

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, R_NF1_Freq, ref _LossOutputPathRX1, ref StrError);
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX2CalSegm, R_NF2_Freq, ref _LossOutputPathRX2, ref StrError);

                                R_NF1_Ampl = R_NF1_Ampl - _LossOutputPathRX1 - tbOutputLoss;
                                R_NF2_Ampl = R_NF2_Ampl - _LossOutputPathRX2 - tbOutputLoss;

                                //Save_MXA1Trace(1, _TestParaName, _Save_MXATrace);
                                //Save_MXA2Trace(1, _TestParaName, _Save_MXATrace);

                                //Measure SMU current
                                MeasSMU = _SMUMeasCh.Split(',');
                                if (_Test_SMU)
                                {
                                    DelayMs(_RdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (_SMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else    //if fail power out search , set data to default
                            {

                                //Need to turn off sweep mode - interference when running multisite because SG will go back to start freq once completed sweep
                                Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.CW);       //setting will set back to default freq define earlier
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);

                                SGTargetPin = _Pin1 - totalInputLoss;       //reset the initial power setting to default
                                R_NF1_Freq = -999;
                                R_NF2_Freq = -999;

                                R_NF1_Ampl = 999;
                                R_NF2_Ampl = 999;

                                //Measure SMU current
                                MeasSMU = _SMUMeasCh.Split(',');
                                if (_Test_SMU)
                                {
                                    DelayMs(_RdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (_SMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (_OffSG1)
                            {
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                            }

                            //Initialize & clear MXA trace to prepare for next measurement
                            Eq.Site[0]._EqSA01.CLEAR_WRITE();
                            Eq.Site[0]._EqSA02.CLEAR_WRITE();
                            //Eq.Site[0]._EqSA01.SET_TRACE_DETECTOR("MAXHOLD");
                            //Eq.Site[0]._EqSA02.SET_TRACE_DETECTOR("MAXHOLD");

                            DelayMs(_StopSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq
                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ATFResultBuilder.AddResultToDict(_TestParaName + "_TestTime" + _TestNum, tTime.ElapsedMilliseconds, ref StrError);
                            //}

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)_Estimate_TestTime)
                            {
                                syncTest_Delay = (long)_Estimate_TestTime - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            #endregion
                            break;

                        case "NF_NONCA_NDIAG":
                            // This sweep is a faster sweep , it is a continuous sweep base on SG freq sweep mode
                            #region NF NONCA NDIAG

                            prevRslt = 0;
                            status = false;
                            pwrSearch = false;
                            Index = 0;
                            SAReferenceLevel = -20;
                            vBW_Hz = 300;
                            tx1_span = 0;
                            tx1_noPoints = 0;
                            rx1_span = 0;
                            rx1_cntrfreq = 0;
                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(_PoutTolerance);
                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                            {
                                tolerancePwr = 0.5;
                            }

                            //use for searching previous result - to get the DUT LNA gain from previous result
                            if (Convert.ToInt16(_TestUsePrev) > 0)
                            {
                                usePrevRslt = true;
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                prevRslt = Math.Round(ReportRslt(_TestUsePrev, resultTag), 3);
                            }

                            //DelayMs(_StartSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", _SwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);
                            rbwParamName = "_" + Math.Abs(myUtility.MXA_Setting.RBW / 1e6).ToString() + "MHz";
                            #endregion

                            #region PowerSensor Offset, MXG and MXA1 configuration

                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            count = Convert.ToInt16((_StopTXFreq1 - _StartTXFreq1) / _StepTXFreq1);
                            _TXFreq = _StartTXFreq1;
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, _TXFreq, ref _LossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)_LossInputPathSG1;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _TXFreq, ref _LossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)_LossCouplerPath;
                                _TXFreq = _TXFreq + _StepTXFreq1;
                            }

                            tmpAveInputLoss = tmpInputLoss / (count + 1);
                            tmpAveCouplerLoss = tmpCouplerLoss / (count + 1);
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            count = Convert.ToInt16((_StopRXFreq1 - _StartRXFreq1) / _StepRXFreq1);
                            _RXFreq = _StartRXFreq1;
                            MkrCalSegmTag = myUtility.CalSegm_Setting.RX1CalSegm + "_MKRNoise_" + myUtility.MXA_Setting.RBW / 1e6 + "MHz";
                            for (int i = 0; i <= count; i++)
                            {
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, MkrCalSegmTag, _RXFreq, ref mkrNoiseLoss, ref StrError);
                                tmpMkrNoiseLoss = tmpMkrNoiseLoss + mkrNoiseLoss;
                                _RXFreq = _RXFreq + _StepRXFreq1;
                            }
                            tmpAveMkrNoiseLoss = tmpMkrNoiseLoss / (count + 1);

                            //change PowerSensor, MXG setting
                            Eq.Site[0]._EqPwrMeter.SetOffset(1, totalOutputLoss);
                            Eq.Site[0]._EqSG01.SetFreq(Convert.ToDouble(_SG1_DefaultFreq));

                            if (PreviousMXAMode != _SwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                tx1_span = _StopTXFreq1 - _StartTXFreq1;
                                tx1_noPoints = Convert.ToInt16(tx1_span / _StepTXFreq1);
                                Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.LIST);
                                Eq.Site[0]._EqSG01.SET_LIST_TYPE(LibEqmtDriver.SG.N5182_LIST_TYPE.STEP);
                                Eq.Site[0]._EqSG01.SET_LIST_MODE(LibEqmtDriver.SG.INSTR_MODE.AUTO);
                                Eq.Site[0]._EqSG01.SET_LIST_TRIG_SOURCE(LibEqmtDriver.SG.N5182_TRIG_TYPE.TIM);
                                Eq.Site[0]._EqSG01.SET_CONT_SWEEP(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);

                                Eq.Site[0]._EqSG01.SET_START_FREQUENCY(_StartTXFreq1 - (_StepTXFreq1 / 2));
                                Eq.Site[0]._EqSG01.SET_STOP_FREQUENCY(_StopTXFreq1 + (_StepTXFreq1 / 2));
                                Eq.Site[0]._EqSG01.SET_TRIG_TIMERPERIOD(_DwellT1);
                                Eq.Site[0]._EqSG01.SET_SWEEP_POINT(tx1_noPoints + 2);   //need to add additional 2 points to calculated no of points because of extra point of start_freq and stop_freq for MXG and MXA sync

                                SGTargetPin = _Pin1 - totalInputLoss;
                                Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                Eq.Site[0]._ModulationType = (LibEqmtDriver.SG.N5182A_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.SG.N5182A_WAVEFORM_MODE), _WaveFormName);
                                Eq.Site[0]._EqSG01.SELECT_WAVEFORM(Eq.Site[0]._ModulationType);
                                Eq.Site[0]._EqSG01.SET_ROUTE_CONN_TOUT(LibEqmtDriver.SG.N5182A_ROUTE_SUBSYS.SweepRun);
                                Eq.Site[0]._EqSG01.SINGLE_SWEEP();      //need to sweep SG for power search - RF ON in sweep mode

                                if (_SetFullMod)
                                {
                                    //This setting will set the modulation for N5182A to full modulation
                                    //Found out that when this set to default (RMS) , the modulation is mutated (CW + Mod) when running under sweep mode for NF
                                    Eq.Site[0]._EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.Mod);
                                }
                                else
                                {
                                    Eq.Site[0]._EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.RMS);
                                }
                                #endregion

                                #region MXA 1 setup
                                DelayMs(_Setup_Delay);
                                rx1_span = (_StopRXFreq1 - _StartRXFreq1);
                                rx1_cntrfreq = _StartRXFreq1 + (rx1_span / 2);
                                rx1_mxa_nopts = (int)((rx1_span / rx1_mxa_nopts_step) + 1);

                                Eq.Site[0]._EqSA01.Select_Instrument(LibEqmtDriver.SA.N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);

                                //ANTHONY-ATT
                                Eq.Site[0]._EqSA01.AUTO_ATTENUATION(false);
                                if (Convert.ToDouble(_SA1att) != CurrentSaAttn)
                                {
                                    Eq.Site[0]._EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToDouble(_SA1att));
                                    CurrentSaAttn = Convert.ToDouble(_SA1att);
                                }

                                //Eq.Site[0]._EqSA01.ELEC_ATTEN_ENABLE(true);
                                Eq.Site[0]._EqSA01.TRIGGER_SINGLE();
                                Eq.Site[0]._EqSA01.TRACE_AVERAGE(1);
                                Eq.Site[0]._EqSA01.AVERAGE_OFF();

                                Eq.Site[0]._EqSA01.FREQ_CENT(rx1_cntrfreq.ToString(), "MHz");
                                Eq.Site[0]._EqSA01.SPAN(rx1_span);
                                Eq.Site[0]._EqSA01.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                Eq.Site[0]._EqSA01.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                Eq.Site[0]._EqSA01.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);

                                Eq.Site[0]._EqSA01.SWEEP_POINTS(rx1_mxa_nopts);

                                if (_SetRX1NDiag)
                                {
                                    Eq.Site[0]._EqSA01.CONTINUOUS_MEASUREMENT_ON();

                                    //ANTHONY-ATT
                                    if (Convert.ToDouble(_SA1att) != CurrentSaAttn) //Anthony
                                    {
                                        Eq.Site[0]._EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToDouble(_SA1att));
                                        CurrentSaAttn = Convert.ToDouble(_SA1att);
                                    }
                                    Eq.Site[0]._EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    trigDelay = (decimal)_DwellT1 + (decimal)0.1;       //fixed 0.1ms delay
                                    Eq.Site[0]._EqSA01.SET_TRIG_DELAY(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1, trigDelay.ToString());
                                    Eq.Site[0]._EqSA01.SWEEP_TIMES(Convert.ToInt16(tx1_noPoints * _DwellT1));
                                }
                                else
                                {
                                    Eq.Site[0]._EqSA01.CONTINUOUS_MEASUREMENT_ON();

                                    //ANTHONY-ATT
                                    if (myUtility.MXA_Setting.Attenuation != CurrentSaAttn)
                                    {
                                        Eq.Site[0]._EqSA01.AMPLITUDE_INPUT_ATTENUATION(myUtility.MXA_Setting.Attenuation);
                                        CurrentSaAttn = Convert.ToDouble(myUtility.MXA_Setting.Attenuation);
                                    }
                                    Eq.Site[0]._EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_Ext1);
                                    Eq.Site[0]._EqSA01.SWEEP_TIMES(Convert.ToInt16(_RX1SweepT));
                                }

                                //Initialize & clear MXA trace
                                Eq.Site[0]._EqSA01.MARKER_TURN_ON_NORMAL_POINT(1, (float)rx1_cntrfreq);
                                Eq.Site[0]._EqSA01.CLEAR_WRITE();
                                status = Eq.Site[0]._EqSA01.OPERATION_COMPLETE();

                                #endregion

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = _SwBand.ToUpper();
                            }
                            #endregion

                            #region measure contact power (Pout1)
                            if (StopOnFail.TestFail == false)
                            {
                                //Just for maximator Special case // Trick - 39mA  21.06.16
                                //Eq.Site[0]._Eq_SMUDriver.SetVolt(Eq.Site[0]._SMUSetting[1], Eq.Site[0]._EqSMU, _SMUVCh[1], _SMUILimitCh[2]);

                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.LIST);
                                if (!_TunePwr_TX1)
                                {
                                    StopOnFail.TestFail = true;     //init to fail state as default
                                    if (_Test_Pout1)
                                    {
                                        DelayMs(_RdPwr_Delay);
                                        R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = Math.Round(SGTargetPin + totalInputLoss, 3);
                                        if (Math.Abs(_Pout1 - R_Pout1) <= (tolerancePwr + 3.5))
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                        }
                                    }
                                    else
                                    {
                                        //No Pout measurement required, default set flag to pass
                                        pwrSearch = true;
                                        StopOnFail.TestFail = false;
                                    }
                                }
                                else
                                {
                                    do
                                    {
                                        StopOnFail.TestFail = true;     //init to fail state as default
                                        R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                        //R_Pin = TargetPin + (float)_LossInputPathSG1;
                                        R_Pin1 = SGTargetPin + totalInputLoss;

                                        if (Math.Abs(_Pout1 - R_Pout1) >= tolerancePwr)
                                        {
                                            if ((Index == 0) && (SGTargetPin < _SG1MaxPwr))   //preset to initial target power for 1st measurement count
                                            {
                                                Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                                DelayMs(_RdPwr_Delay);
                                                R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                            }

                                            SGTargetPin = SGTargetPin + (_Pout1 - R_Pout1);

                                            if (SGTargetPin < _SG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                            {
                                                Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                                DelayMs(_RdPwr_Delay);
                                            }
                                        }
                                        else if (SGTargetPin > _SG1MaxPwr)      //if input sig gen exit limit , exit pwr search loop
                                        {
                                            SGTargetPin = _Pin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                            break;
                                        }
                                        else
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                            break;
                                        }

                                        Index++;
                                    }
                                    while (Index < 10);     // max power search loop
                                }
                            }

                            #endregion

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)_StartSync_Delay)
                            {
                                syncTest_Delay = (long)_StartSync_Delay - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            if (pwrSearch)
                            {
                                Eq.Site[0]._EqSG01.SINGLE_SWEEP();
                                status = Eq.Site[0]._EqSG01.OPERATION_COMPLETE();

                                //Need to turn off sweep mode - interference when running multisite because SG will go back to start freq once completed sweep
                                Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.CW);       //setting will set back to default freq define earlier

                                DelayMs(_Trig_Delay);
                                Capture_MXA1_Trace(1, _TestNum, _TestParaName, _RX1Band, prevRslt, _Save_MXATrace, tmpAveMkrNoiseLoss);     //add mkrNoise offset to normalize the MXA trace to dB/Hz
                                Read_MXA1_Trace(1, _TestNum, out R_NF1_Freq, out R_NF1_Ampl, _Search_Method, _TestParaName);

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, R_NF1_Freq, ref _LossOutputPathRX1, ref StrError);
                                R_NF1_Ampl = R_NF1_Ampl - _LossOutputPathRX1 - tbOutputLoss;
                                //Save_MXA1Trace(1, _TestParaName, _Save_MXATrace);

                                //Measure SMU current
                                MeasSMU = _SMUMeasCh.Split(',');
                                if (_Test_SMU)
                                {
                                    DelayMs(_RdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (_SMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else    //if fail power out search , set data to default
                            {
                                //Need to turn off sweep mode - interference when running multisite because SG will go back to start freq once completed sweep
                                Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.CW);       //setting will set back to default freq define earlier
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);

                                SGTargetPin = _Pin1 - totalInputLoss;       //reset the initial power setting to default
                                R_NF1_Freq = -999;
                                R_NF1_Ampl = 999;

                                //Measure SMU current
                                MeasSMU = _SMUMeasCh.Split(',');
                                if (_Test_SMU)
                                {
                                    DelayMs(_RdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (_SMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (_OffSG1)
                            {
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                            }

                            //Initialize & clear MXA trace to prepare for next measurement
                            Eq.Site[0]._EqSA01.CLEAR_WRITE();
                            //Eq.Site[0]._EqSA01.SET_TRACE_DETECTOR("MAXHOLD");

                            DelayMs(_StopSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq
                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ATFResultBuilder.AddResultToDict(_TestParaName + "_TestTime" + _TestNum, tTime.ElapsedMilliseconds, ref StrError);
                            //}

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)_Estimate_TestTime)
                            {
                                syncTest_Delay = (long)_Estimate_TestTime - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            #endregion
                            break;

                        case "NF_FIX_NMAX":
                            // This sweep is a slow sweep , will change SG freq and measure NF for every test points
                            // Using Marker Function Noise (measure at dBm/Hz) with External Amp Gain Offset
                            #region NOISE STEP SWEEP NDIAG/NMAX

                            prevRslt = 0;
                            status = false;
                            pwrSearch = false;
                            Index = 0;
                            tx1_span = 0;
                            tx1_noPoints = 0;
                            rx1_span = 0;
                            rx1_cntrfreq = 0;
                            totalInputLoss = 0;      //Input Pathloss + Testboard Loss
                            totalOutputLoss = 0;     //Output Pathloss + Testboard Loss
                            tolerancePwr = Convert.ToDouble(_PoutTolerance);
                            if (tolerancePwr <= 0)      //just to ensure that tolerance power cannot be 0dBm
                            {
                                tolerancePwr = 0.5;
                            }

                            //use for searching previous result - to get the DUT LNA gain from previous result
                            if (Convert.ToInt16(_TestUsePrev) > 0)
                            {
                                usePrevRslt = true;
                                resultTag = (int)e_ResultTag.NF1_AMPL;
                                prevRslt = Math.Round(ReportRslt(_TestUsePrev, resultTag), 3);
                            }

                            DelayMs(_StartSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq

                            #region Decode Calibration Path and Segment Data
                            CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], _CalTag.ToUpper(), _SwBand.ToUpper());
                            myUtility.Decode_CalSegm_Setting(CalSegmData);

                            MXA_Config = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NFCA_MXA_Config", _SwBand.ToUpper());
                            myUtility.Decode_MXA_Setting(MXA_Config);
                            rbwParamName = "_" + Math.Abs(myUtility.MXA_Setting.RBW / 1e6).ToString() + "MHz";
                            #endregion

                            #region Calc Average Pathloss, PowerSensor Offset, MXG and MXA1 configuration

                            #region Get Average Pathloss
                            //Fixed Testboard loss from config file (this tesboard loss must be -ve value , gain will be +ve value)
                            tbInputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "INPUTLOSS"));
                            tbOutputLoss = Convert.ToDouble(myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "TESTBOARD_LOSS", "OUTPUTLOSS"));

                            //Get average pathloss base on start and stop freq
                            if (_StopTXFreq1 == _StartTXFreq1)
                            {
                                txcount = 1;
                            }
                            else
                            {
                                txcount = Convert.ToInt16(((_StopTXFreq1 - _StartTXFreq1) / _StepTXFreq1) + 1);
                            }

                            tx_freqArray = new double[txcount];
                            contactPwr_Array = new double[txcount];
                            nfAmpl_Array = new double[txcount];
                            nfAmplFreq_Array = new double[txcount];

                            _TXFreq = _StartTXFreq1;
                            for (int i = 0; i < txcount; i++)
                            {
                                tx_freqArray[i] = _TXFreq;

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.TXCalSegm, _TXFreq, ref _LossInputPathSG1, ref StrError);
                                tmpInputLoss = tmpInputLoss + (float)_LossInputPathSG1;

                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _TXFreq, ref _LossCouplerPath, ref StrError);
                                tmpCouplerLoss = tmpCouplerLoss + (float)_LossCouplerPath;

                                _TXFreq = _TXFreq + _StepTXFreq1;
                            }
                            //Calculate the average pathloss/pathgain
                            tmpAveInputLoss = tmpInputLoss / txcount;
                            tmpAveCouplerLoss = tmpCouplerLoss / txcount;
                            totalInputLoss = tmpAveInputLoss - tbInputLoss;
                            totalOutputLoss = Math.Abs(tmpAveCouplerLoss - tbOutputLoss);     //Need to remove -ve sign from cal factor for power sensor offset

                            //Get average pathloss base on start and stop freq
                            rxcount = Convert.ToInt16(((_StopRXFreq1 - _StartRXFreq1) / _StepRXFreq1) + 1);
                            rx_freqArray = new double[rxcount];

                            _RXFreq = _StartRXFreq1;
                            for (int i = 0; i < rxcount; i++)
                            {
                                rx_freqArray[i] = _RXFreq;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                                tmpRxLoss = tmpRxLoss + (float)_LossOutputPathRX1;
                                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, MkrCalSegmTag, _RXFreq, ref mkrNoiseLoss, ref StrError);
                                tmpMkrNoiseLoss = tmpMkrNoiseLoss + mkrNoiseLoss;
                                _RXFreq = _RXFreq + _StepRXFreq1;
                            }
                            tmpAveRxLoss = tmpRxLoss / rxcount;
                            totalRXLoss = tmpAveRxLoss - tbOutputLoss;
                            tmpAveMkrNoiseLoss = tmpMkrNoiseLoss / rxcount;
                            #endregion

                            #region config Power Sensor, MXA and MXG
                            //change PowerSensor,  Set Default Power for MXG setting
                            Eq.Site[0]._EqPwrMeter.SetOffset(1, totalOutputLoss);
                            SGTargetPin = _Pin1 - totalInputLoss;
                            Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                            Eq.Site[0]._EqSG01.SetFreq(Convert.ToDouble(_StartTXFreq1));
                            Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);

                            if (PreviousMXAMode != _SwBand.ToUpper())       //do this for 1st initial setup - same band will skip
                            {
                                #region MXG setup
                                Eq.Site[0]._EqSG01.SetFreqMode(LibEqmtDriver.SG.N5182_FREQUENCY_MODE.FIX);
                                Eq.Site[0]._EqSG01.SetFreq(Math.Abs(Convert.ToDouble(_StartTXFreq1)));

                                SGTargetPin = _Pin1 - totalInputLoss;
                                Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);
                                Eq.Site[0]._ModulationType = (LibEqmtDriver.SG.N5182A_WAVEFORM_MODE)Enum.Parse(typeof(LibEqmtDriver.SG.N5182A_WAVEFORM_MODE), _WaveFormName);
                                Eq.Site[0]._EqSG01.SELECT_WAVEFORM(Eq.Site[0]._ModulationType);
                                Eq.Site[0]._EqSG01.SINGLE_SWEEP();

                                if (_SetFullMod)
                                {
                                    //This setting will set the modulation for N5182A to full modulation
                                    //Found out that when this set to default (RMS) , the modulation is mutated (CW + Mod) when running under sweep mode for NF
                                    Eq.Site[0]._EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.Mod);
                                }
                                else
                                {
                                    Eq.Site[0]._EqSG01.SET_ALC_TRAN_REF(LibEqmtDriver.SG.N5182A_ALC_TRAN_REF.RMS);
                                }
                                #endregion

                                #region MXA 1 setup
                                DelayMs(_Setup_Delay);
                                if (_SetRX1NDiag)
                                {
                                    //NDIAG - RX Bandwidth base on stepsize
                                    rx1_span = _StepRXFreq1 * 2;
                                    rx1_cntrfreq = _StartRXFreq1;
                                    rx1_mxa_nopts = 101;    //fixed no of points
                                }
                                else
                                {
                                    //NMAX - will use full RX Badwidth (StartRX to StopRX)
                                    rx1_span = _StopRXFreq1 - _StartRXFreq1;
                                    rx1_cntrfreq = Math.Round(_StartRXFreq1 + (rx1_span / 2), 3);
                                    rx1_mxa_nopts = (int)((rx1_span / rx1_mxa_nopts_step) + 1);
                                }

                                Eq.Site[0]._EqSA01.Select_Instrument(LibEqmtDriver.SA.N9020A_INSTRUMENT_MODE.SpectrumAnalyzer);
                                Eq.Site[0]._EqSA01.Select_Triggering(LibEqmtDriver.SA.N9020A_TRIGGERING_TYPE.RF_FreeRun);
                                Eq.Site[0]._EqSA01.AUTO_ATTENUATION(false);
                                Eq.Site[0]._EqSA01.CONTINUOUS_MEASUREMENT_OFF();
                                Eq.Site[0]._EqSA01.TRACE_AVERAGE(1);
                                Eq.Site[0]._EqSA01.AVERAGE_OFF();

                                Eq.Site[0]._EqSA01.SPAN(rx1_span);
                                Eq.Site[0]._EqSA01.RESOLUTION_BW(myUtility.MXA_Setting.RBW);
                                Eq.Site[0]._EqSA01.VIDEO_BW(myUtility.MXA_Setting.VBW);
                                Eq.Site[0]._EqSA01.FREQ_CENT(rx1_cntrfreq.ToString(), "MHz");
                                Eq.Site[0]._EqSA01.AMPLITUDE_REF_LEVEL(myUtility.MXA_Setting.RefLevel);

                                //Eq.Site[0]._EqSA01.SWEEP_POINTS(myUtility.MXA_Setting.NoPoints);
                                Eq.Site[0]._EqSA01.SWEEP_POINTS(rx1_mxa_nopts);
                                Eq.Site[0]._EqSA01.AMPLITUDE_INPUT_ATTENUATION(Convert.ToInt16(_SA1att));
                                Eq.Site[0]._EqSA01.SWEEP_TIMES(Convert.ToInt16(_RX1SweepT));

                                //Initialize & clear MXA trace
                                Eq.Site[0]._EqSA01.MARKER_TURN_ON_NORMAL_POINT(1, (float)_StartRXFreq1);
                                Eq.Site[0]._EqSA01.CLEAR_WRITE();

                                status = Eq.Site[0]._EqSG01.OPERATION_COMPLETE();
                                #endregion

                                //reset current MXA mode to previous mode
                                PreviousMXAMode = _SwBand.ToUpper();
                            }
                            #endregion

                            #endregion

                            #region measure contact power (Pout1)
                            if (StopOnFail.TestFail == false)
                            {
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.ON);

                                if (!_TunePwr_TX1)
                                {
                                    StopOnFail.TestFail = true;     //init to fail state as default

                                    if (_Test_Pout1)
                                    {
                                        DelayMs(_RdPwr_Delay);
                                        R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = Math.Round(SGTargetPin + totalInputLoss, 3);
                                        if (Math.Abs(_Pout1 - R_Pout1) <= (tolerancePwr + 3.5))
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                        }
                                    }
                                    else
                                    {
                                        //No Pout measurement required, default set flag to pass
                                        pwrSearch = true;
                                        StopOnFail.TestFail = false;
                                    }

                                }
                                else
                                {
                                    do
                                    {
                                        StopOnFail.TestFail = true;     //init to fail state as default
                                        R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                        R_Pin1 = SGTargetPin + totalInputLoss;

                                        if (Math.Abs(_Pout1 - R_Pout1) >= tolerancePwr)
                                        {
                                            if ((Index == 0) && (SGTargetPin < _SG1MaxPwr))   //preset to initial target power for 1st measurement count
                                            {
                                                Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                                DelayMs(_RdPwr_Delay);
                                                R_Pout1 = Eq.Site[0]._EqPwrMeter.MeasPwr(1);
                                            }

                                            SGTargetPin = SGTargetPin + (_Pout1 - R_Pout1);

                                            if (SGTargetPin < _SG1MaxPwr)       //do this if the input sig gen does not exceed limit
                                            {
                                                Eq.Site[0]._EqSG01.SetAmplitude((float)SGTargetPin);
                                                DelayMs(_RdPwr_Delay);
                                            }
                                        }
                                        else if (SGTargetPin > _SG1MaxPwr)      //if input sig gen exit limit , exit pwr search loop
                                        {
                                            SGTargetPin = _Pin1 - totalInputLoss;    //reset target Sig Gen to initial setting
                                            break;
                                        }
                                        else
                                        {
                                            pwrSearch = true;
                                            StopOnFail.TestFail = false;
                                            break;
                                        }

                                        Index++;
                                    }
                                    while (Index < 10);     // max power search loop
                                }
                            }

                            #endregion

                            if (pwrSearch)
                            {
                                for (int i = 0; i < tx_freqArray.Length; i++)
                                {
                                    if (_SetRX1NDiag)   //NDIAG - RX Bandwidth base on stepsize else NMAX - will use full RX Badwidth (StartRX to StopRX)
                                    {
                                        Eq.Site[0]._EqSA01.FREQ_CENT(rx_freqArray[i].ToString(), "MHz");    //RX Bandwidth base on stepsize
                                        Eq.Site[0]._EqSG01.SetFreq(Convert.ToDouble(tx_freqArray[i]));
                                        Eq.Site[0]._EqSA01.TRIGGER_IMM();
                                        DelayMs(Convert.ToInt16(_RX1SweepT));       //Need to set same delay as sweep time before read trace  

                                        status = Eq.Site[0]._EqSG01.OPERATION_COMPLETE();
                                        Capture_MXA1_Trace(1, _TestNum, _TestParaName, _RX1Band, prevRslt, false, tmpAveMkrNoiseLoss);      //add mkrNoise offset to normalize the MXA trace to dB/Hz
                                        Read_MXA1_Trace(1, _TestNum, out nfAmplFreq_Array[i], out nfAmpl_Array[i], _Search_Method, _TestParaName);
                                        nfAmpl_Array[i] = nfAmpl_Array[i] - totalRXLoss;
                                    }
                                    else
                                    {
                                        Eq.Site[0]._EqSG01.SetFreq(Convert.ToDouble(tx_freqArray[i]));
                                        Eq.Site[0]._EqSA01.TRIGGER_IMM();
                                        DelayMs(_Trig_Delay);

                                        status = Eq.Site[0]._EqSG01.OPERATION_COMPLETE();
                                        Capture_MXA1_Trace(1, _TestNum, _TestParaName, _RX1Band, prevRslt, _Save_MXATrace, tmpAveMkrNoiseLoss);     //add mkrNoise offset to normalize the MXA trace to dB/Hz
                                        Read_MXA1_Trace(1, _TestNum, out nfAmplFreq_Array[i], out nfAmpl_Array[i], _Search_Method, _TestParaName);
                                        nfAmpl_Array[i] = nfAmpl_Array[i] - totalRXLoss;
                                    }
                                }

                                #region Search result MAX or MIN and Save to Datalog
                                //Find result MAX or MIN result
                                switch (_Search_Method.ToUpper())
                                {
                                    case "MAX":
                                        R_NF1_Ampl = nfAmpl_Array.Max();
                                        indexdata = Array.IndexOf(nfAmpl_Array, R_NF1_Ampl);     //return index of max value
                                        R_NF1_Freq = nfAmplFreq_Array[indexdata];
                                        break;

                                    case "MIN":
                                        R_NF1_Ampl = nfAmpl_Array.Min();
                                        indexdata = Array.IndexOf(nfAmpl_Array, R_NF1_Ampl);     //return index of max value
                                        R_NF1_Freq = nfAmplFreq_Array[indexdata];
                                        break;

                                    default:
                                        MessageBox.Show("Test Parameter : " + _TestParam + "(" + _Search_Method + ") not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                                        break;
                                }

                                //Save all data to datalog 
                                if (_SetRX1NDiag)           //save trace method is different between NDIAG and NMAX
                                {
                                    if (_Save_MXATrace)
                                    {
                                        string[] templine = new string[4];
                                        ArrayList LocalTextList = new ArrayList();
                                        ArrayList tmpCalMsg = new ArrayList();

                                        //Calibration File Header
                                        LocalTextList.Add("#MXA1 NF STEP SWEEP DATALOG");
                                        LocalTextList.Add("#Date : " + DateTime.Now.ToShortDateString());
                                        LocalTextList.Add("#Time : " + DateTime.Now.ToLongTimeString());
                                        LocalTextList.Add("#Input TX Power : " + _Pin1 + " dBm");
                                        LocalTextList.Add("#Measure Contact Power : " + Math.Round(R_Pout1, 3) + " dBm");
                                        templine[0] = "#TX_FREQ";
                                        templine[1] = "NOISE_RXFREQ";
                                        templine[2] = "NOISE_AMPL";
                                        LocalTextList.Add(string.Join(",", templine));

                                        // Start looping until complete the freq range
                                        for (istep = 0; istep < tx_freqArray.Length; istep++)
                                        {
                                            //Sorted the calibration result to array
                                            templine[0] = Convert.ToString(tx_freqArray[istep]);
                                            templine[1] = Convert.ToString(nfAmplFreq_Array[istep]);
                                            templine[2] = Convert.ToString(Math.Round(nfAmpl_Array[istep], 3));
                                            LocalTextList.Add(string.Join(",", templine));
                                        }

                                        //Write cal data to csv file
                                        if (!Directory.Exists(SNPFile.FileOutput_Path))
                                        {
                                            Directory.CreateDirectory(SNPFile.FileOutput_Path);
                                        }
                                        //Write cal data to csv file
                                        string tempPath = SNPFile.FileOutput_Path + SNPFile.FileOutput_FileName + "_" + _TestParaName + "_Unit" + tmpUnit_No.ToString() + ".csv";
                                        //MessageBox.Show("Path : " + tempPath);
                                        IO_TxtFile.CreateWrite_TextFile(tempPath, LocalTextList);
                                    }
                                }
                                #endregion

                                //Measure SMU current
                                MeasSMU = _SMUMeasCh.Split(',');
                                if (_Test_SMU)
                                {
                                    DelayMs(_RdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (_SMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            else    //if fail power out search , set data to default
                            {
                                SGTargetPin = _Pin1 - totalInputLoss;       //reset the initial power setting to default
                                R_NF1_Freq = -999;
                                R_NF1_Ampl = 999;

                                //Measure SMU current
                                MeasSMU = _SMUMeasCh.Split(',');
                                if (_Test_SMU)
                                {
                                    DelayMs(_RdCurr_Delay);
                                    float _NPLC = 0.1f; // float _NPLC = 1;  toh
                                    for (int i = 0; i < MeasSMU.Count(); i++)
                                    {
                                        int smuIChannel = Convert.ToInt16(MeasSMU[i]);
                                        if (_SMUILimitCh[smuIChannel] > 0)
                                        {
                                            R_SMU_ICh[smuIChannel] = Eq.Site[0]._Eq_SMUDriver.MeasI(Eq.Site[0]._SMUSetting[smuIChannel], Eq.Site[0]._EqSMU, _NPLC, LibEqmtDriver.SMU.ePSupply_IRange._Auto);
                                        }

                                        // pass out the test result label for every measurement channel
                                        string tempLabel = "SMUI_CH" + MeasSMU[i];
                                        foreach (string key in DicTestLabel.Keys)
                                        {
                                            if (key == tempLabel)
                                            {
                                                R_SMULabel_ICh[smuIChannel] = DicTestLabel[key].ToString();
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            if (_OffSG1)
                            {
                                Eq.Site[0]._EqSG01.EnableRF(LibEqmtDriver.SG.INSTR_OUTPUT.OFF);
                            }

                            //Initialize & clear MXA trace to prepare for next measurement
                            Eq.Site[0]._EqSA01.CLEAR_WRITE();

                            DelayMs(_StopSync_Delay);     //Delay to sync multiple site so that no interference between ovelapping RX Freq
                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ATFResultBuilder.AddResultToDict(_TestParaName + "_TestTime" + _TestNum, tTime.ElapsedMilliseconds, ref StrError);
                            //}

                            //to sync the total test time for each parameter - use in NF multiband testsite
                            paramTestTime = tTime.ElapsedMilliseconds;
                            if (paramTestTime < (long)_Estimate_TestTime)
                            {
                                syncTest_Delay = (long)_Estimate_TestTime - paramTestTime;
                                DelayMs((int)syncTest_Delay);
                            }

                            #endregion
                            break;

                        default:
                            MessageBox.Show("NF Test Parameter : " + _TestParam.ToUpper() + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                            #endregion
                    }
                    break;

                case "TESTBOARD":
                    switch (_TestParam.ToUpper())
                    {
                        #region Read TestBoard OTP/Temperature
                        case "TEMPERATURE":
                            R_Temperature = -999;
                            b_TestBoard_temp = true;

                            Eq.Site[0]._EqMiPiCtrl.BoardTemperature(out R_Temperature);

                            tTime.Stop();
                            //if (_Test_TestTime)
                            //{
                            //    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                            //}

                            //ResultBuilder.BuildResults(ref results, _TestParaName, "degC", R_Temperature);
                            break;

                            #endregion
                    }
                    break;

                default:
                    MessageBox.Show("Test Mode " + _TestMode + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                    break;

            }

            #endregion

            //Add test result
            if (!b_GE_Header)
            {
                #region add test result
                if (_Test_Pin)
                {
                    ResultBuilder.BuildResults(ref results, _TestParaName + "_Pin", "dBm", R_Pin);

                    //use as temp data storage for calculating MAX, MIN etc of multiple result
                    Results[TestCount].Multi_Results[(int)e_ResultTag.PIN].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.PIN].Result_Header = _TestParaName + "_Pin";
                    Results[TestCount].Multi_Results[(int)e_ResultTag.PIN].Result_Data = R_Pin;
                }
                if (_Test_Pout)
                {
                    ResultBuilder.BuildResults(ref results, _TestParaName + "_Pout", "dBm", R_Pout);

                    //use as temp data storage for calculating MAX, MIN etc of multiple result
                    Results[TestCount].Multi_Results[(int)e_ResultTag.POUT].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.POUT].Result_Header = _TestParaName + "_Pout";
                    Results[TestCount].Multi_Results[(int)e_ResultTag.POUT].Result_Data = R_Pout;
                }
                if (_Test_Pin1)
                {
                    ResultBuilder.BuildResults(ref results, _TestParaName + "_Pin1", "dBm", R_Pin1);

                    //use as temp data storage for calculating MAX, MIN etc of multiple result
                    Results[TestCount].Multi_Results[(int)e_ResultTag.PIN1].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.PIN1].Result_Header = _TestParaName + "_Pin1";
                    Results[TestCount].Multi_Results[(int)e_ResultTag.PIN1].Result_Data = R_Pin1;
                }
                if (_Test_Pout1)
                {
                    ResultBuilder.BuildResults(ref results, _TestParaName + "_Pout1", "dBm", R_Pout1);

                    //use as temp data storage for calculating MAX, MIN etc of multiple result
                    Results[TestCount].Multi_Results[(int)e_ResultTag.POUT1].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.POUT1].Result_Header = _TestParaName + "_Pout1";
                    Results[TestCount].Multi_Results[(int)e_ResultTag.POUT1].Result_Data = R_Pout1;
                }
                if (_Test_Pin2)
                {
                    ResultBuilder.BuildResults(ref results, _TestParaName + "_Pin2", "dBm", R_Pin2);

                    //use as temp data storage for calculating MAX, MIN etc of multiple result
                    Results[TestCount].Multi_Results[(int)e_ResultTag.PIN2].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.PIN2].Result_Header = _TestParaName + "_Pin2";
                    Results[TestCount].Multi_Results[(int)e_ResultTag.PIN2].Result_Data = R_Pin2;
                }
                if (_Test_Pout2)
                {
                    ResultBuilder.BuildResults(ref results, _TestParaName + "_Pout2", "dBm", R_Pout2);

                    //use as temp data storage for calculating MAX, MIN etc of multiple result
                    Results[TestCount].Multi_Results[(int)e_ResultTag.POUT2].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.POUT2].Result_Header = _TestParaName + "_Pout2";
                    Results[TestCount].Multi_Results[(int)e_ResultTag.POUT2].Result_Data = R_Pout2;
                }
                if (_Test_NF1)
                {
                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Ampl", "dBm", R_NF1_Ampl);
                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX1Band + "_Freq", "MHz", R_NF1_Freq);

                    //use as temp data storage for calculating MAX, MIN etc of multiple result
                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF1_AMPL].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF1_AMPL].Result_Header = _TestParaName + "_RX" + _RX1Band + "_Ampl";
                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF1_AMPL].Result_Data = R_NF1_Ampl;

                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF1_FREQ].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF1_FREQ].Result_Header = _TestParaName + "_RX" + _RX1Band + "_FREQ";
                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF1_FREQ].Result_Data = R_NF1_Freq;
                }
                if (_Test_NF2)
                {
                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX2Band + "_Ampl", "dBm", R_NF2_Ampl);
                    ResultBuilder.BuildResults(ref results, _TestParaName + "_RX" + _RX2Band + "_Freq", "MHz", R_NF2_Freq);

                    //use as temp data storage for calculating MAX, MIN etc of multiple result
                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF2_AMPL].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF2_AMPL].Result_Header = _TestParaName + "_RX" + _RX2Band + "_Ampl";
                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF2_AMPL].Result_Data = R_NF2_Ampl;

                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF2_FREQ].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF2_FREQ].Result_Header = _TestParaName + "_RX" + _RX2Band + "_FREQ";
                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF2_FREQ].Result_Data = R_NF2_Freq;
                }
                if (_Test_Harmonic)
                {
                    ResultBuilder.BuildResults(ref results, _TestParaName + "_Ampl", "dBm", R_H2_Ampl);
                    ResultBuilder.BuildResults(ref results, _TestParaName + "_Freq", "MHz", R_H2_Freq);

                    //use as temp data storage for calculating MAX, MIN etc of multiple result
                    Results[TestCount].Multi_Results[(int)e_ResultTag.HARMONIC_AMPL].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.HARMONIC_AMPL].Result_Header = _TestParaName + "_Ampl";
                    Results[TestCount].Multi_Results[(int)e_ResultTag.HARMONIC_AMPL].Result_Data = R_H2_Ampl;

                    Results[TestCount].Multi_Results[(int)e_ResultTag.HARMONIC_FREQ].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.HARMONIC_FREQ].Result_Header = _TestParaName + "_Freq";
                    Results[TestCount].Multi_Results[(int)e_ResultTag.HARMONIC_FREQ].Result_Data = R_H2_Freq;
                }
                if (_Test_MIPI)
                    ResultBuilder.BuildResults(ref results, _TestParaName + "_MIPI", "NA", R_MIPI);
                if (_Test_SMU)
                {
                    MeasSMU = _SMUMeasCh.Split(',');
                    for (int i = 0; i < MeasSMU.Count(); i++)
                    {
                        ResultBuilder.BuildResults(ref results, _TestParaName + "_" + R_SMULabel_ICh[Convert.ToInt16(MeasSMU[i])], "A", R_SMU_ICh[Convert.ToInt16(MeasSMU[i])]);
                    }
                }
                if (_Test_DCSupply)
                {
                    MeasDC = _DCMeasCh.Split(',');
                    for (int i = 0; i < MeasDC.Count(); i++)
                    {
                        ResultBuilder.BuildResults(ref results, _TestParaName + "_" + R_DCLabel_ICh[Convert.ToInt16(MeasDC[i])], "A", R_DC_ICh[Convert.ToInt16(MeasDC[i])]);
                    }
                }
                if (_Test_Switch)
                    ResultBuilder.BuildResults(ref results, _TestParaName + "_Status", "NA", R_Switch);
                if (R_RFCalStatus == 1)
                    ResultBuilder.BuildResults(ref results, _TestParaName + "_Status", "NA", R_RFCalStatus);
                if (b_TestBoard_temp)
                {
                    ResultBuilder.BuildResults(ref results, _TestParaName, "C", R_Temperature);
                }
                if (_Test_TestTime)
                {
                    //ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.ElapsedMilliseconds);
                    ResultBuilder.BuildResults(ref results, _TestParaName + "_TestTime" + _TestNum, "mS", tTime.Elapsed.TotalMilliseconds);
                }
                #endregion
            }
            else
            {
                string GE_TestParam = null;
                Rslt_GE_Header = new s_GE_Header();
                Decode_GE_Header(TestPara, out Rslt_GE_Header);

                #region add test result
                if (_Test_Pin)
                {
                    Rslt_GE_Header.Param = "_Pin";      //re-assign ge header 
                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                    ResultBuilder.BuildResults(ref results, GE_TestParam, "dBm", R_Pin);

                    //use as temp data storage for calculating MAX, MIN etc of multiple result
                    Results[TestCount].Multi_Results[(int)e_ResultTag.PIN].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.PIN].Result_Header = GE_TestParam;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.PIN].Result_Data = R_Pin;
                }
                if (_Test_Pout)
                {
                    Rslt_GE_Header.Param = "_Pout";      //re-assign ge header 
                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                    ResultBuilder.BuildResults(ref results, GE_TestParam, "dBm", R_Pin);

                    //use as temp data storage for calculating MAX, MIN etc of multiple result
                    Results[TestCount].Multi_Results[(int)e_ResultTag.POUT].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.POUT].Result_Header = GE_TestParam;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.POUT].Result_Data = R_Pout;
                }
                if (_Test_Pin1)
                {
                    Rslt_GE_Header.Param = "_Pin";      //re-assign ge header 
                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                    ResultBuilder.BuildResults(ref results, GE_TestParam, "dBm", R_Pin1);

                    //use as temp data storage for calculating MAX, MIN etc of multiple result
                    Results[TestCount].Multi_Results[(int)e_ResultTag.PIN1].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.PIN1].Result_Header = GE_TestParam;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.PIN1].Result_Data = R_Pin1;
                }
                if (_Test_Pout1)
                {
                    Rslt_GE_Header.Param = "_Pout";      //re-assign ge header 
                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                    ResultBuilder.BuildResults(ref results, GE_TestParam, "dBm", R_Pout1);

                    //use as temp data storage for calculating MAX, MIN etc of multiple result
                    Results[TestCount].Multi_Results[(int)e_ResultTag.POUT1].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.POUT1].Result_Header = GE_TestParam;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.POUT1].Result_Data = R_Pout1;
                }
                if (_Test_Pin2)
                {
                    Rslt_GE_Header.Param = "_Pin";      //re-assign ge header 
                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                    ResultBuilder.BuildResults(ref results, GE_TestParam, "dBm", R_Pin2);

                    //use as temp data storage for calculating MAX, MIN etc of multiple result
                    Results[TestCount].Multi_Results[(int)e_ResultTag.PIN2].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.PIN2].Result_Header = GE_TestParam;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.PIN2].Result_Data = R_Pin2;
                }
                if (_Test_Pout2)
                {
                    Rslt_GE_Header.Param = "_Pout";      //re-assign ge header 
                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                    ResultBuilder.BuildResults(ref results, GE_TestParam, "dBm", R_Pout2);

                    //use as temp data storage for calculating MAX, MIN etc of multiple result
                    Results[TestCount].Multi_Results[(int)e_ResultTag.POUT2].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.POUT2].Result_Header = GE_TestParam;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.POUT2].Result_Data = R_Pout2;
                }
                if (_Test_NF1)
                {
                    if (Rslt_GE_Header.Param.Contains("Gain"))
                    {
                        Rslt_GE_Header.Freq1 = "_Rx-" + Convert.ToString(Math.Round(_StartRXFreq1, 6)) + "MHz";  // Start Freq
                        Rslt_GE_Header.Freq2 = "_Rx-" + Convert.ToString(Math.Round(_StopRXFreq1, 6)) + "MHz";  // Stop Freq
                    }
                    Rslt_GE_Header.Param = Rslt_GE_Header.Param + "-Ampl";      //re-assign ge header 
                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                    ResultBuilder.BuildResults(ref results, GE_TestParam, "dBm", R_NF1_Ampl);

                    //use as temp data storage for calculating MAX, MIN etc of multiple result
                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF1_AMPL].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF1_AMPL].Result_Header = GE_TestParam;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF1_AMPL].Result_Data = R_NF1_Ampl;

                    Rslt_GE_Header.Param = Rslt_GE_Header.Param + "-Freq";      //re-assign ge header
                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                    ResultBuilder.BuildResults(ref results, GE_TestParam, "MHz", R_NF1_Freq);

                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF1_FREQ].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF1_FREQ].Result_Header = GE_TestParam;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF1_FREQ].Result_Data = R_NF1_Freq;
                }
                if (_Test_NF2)
                {
                    Rslt_GE_Header.Param = Rslt_GE_Header.Param + "-Ampl";      //re-assign ge header 
                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                    ResultBuilder.BuildResults(ref results, Rslt_GE_Header.Param, "dBm", R_NF2_Ampl);

                    //use as temp data storage for calculating MAX, MIN etc of multiple result
                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF2_AMPL].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF2_AMPL].Result_Header = GE_TestParam;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF2_AMPL].Result_Data = R_NF2_Ampl;

                    Rslt_GE_Header.Param = Rslt_GE_Header.Param + "-Freq";      //re-assign ge header
                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                    ResultBuilder.BuildResults(ref results, GE_TestParam, "MHz", R_NF2_Freq);

                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF2_FREQ].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF2_FREQ].Result_Header = GE_TestParam;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.NF2_FREQ].Result_Data = R_NF2_Freq;
                }
                if (_Test_Harmonic)
                {
                    Rslt_GE_Header.Param = Rslt_GE_Header.Param + "-Ampl";      //re-assign ge header 
                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                    ResultBuilder.BuildResults(ref results, Rslt_GE_Header.Param, "dBm", R_H2_Ampl);

                    //use as temp data storage for calculating MAX, MIN etc of multiple result
                    Results[TestCount].Multi_Results[(int)e_ResultTag.HARMONIC_AMPL].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.HARMONIC_AMPL].Result_Header = GE_TestParam;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.HARMONIC_AMPL].Result_Data = R_H2_Ampl;

                    Rslt_GE_Header.Param = Rslt_GE_Header.Param + "-Freq";      //re-assign ge header
                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                    ResultBuilder.BuildResults(ref results, GE_TestParam, "MHz", R_H2_Freq);

                    Results[TestCount].Multi_Results[(int)e_ResultTag.HARMONIC_FREQ].Enable = true;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.HARMONIC_FREQ].Result_Header = GE_TestParam;
                    Results[TestCount].Multi_Results[(int)e_ResultTag.HARMONIC_FREQ].Result_Data = R_H2_Freq;
                }
                if (_Test_MIPI)
                {
                    Rslt_GE_Header.Note = "_NOTE_" + _TestParaName + "_" + _TestNum;      //re-assign ge header 
                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, _Test_SMU);
                    ResultBuilder.BuildResults(ref results, GE_TestParam, "NA", R_MIPI);
                }

                if (_Test_SMU)
                {
                    MeasSMU = _SMUMeasCh.Split(',');
                    int ch_cnt = MeasSMU.Count();
                    double[] smuMeas_I = new double[ch_cnt];
                    string[] tmp_GE_TestParam = new string[ch_cnt];

                    Rslt_GE_Header.MeasType = "N";      //re-assign ge header 
                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, R_SMU_ICh, out smuMeas_I, out tmp_GE_TestParam);

                    for (int i = 0; i < MeasSMU.Count(); i++)
                    {
                        ResultBuilder.BuildResults(ref results, tmp_GE_TestParam[i], "A", smuMeas_I[i]);
                    }
                }
                if (_Test_DCSupply)
                {
                    MeasDC = _DCMeasCh.Split(',');

                    for (int i = 0; i < MeasDC.Count(); i++)
                    {
                        Rslt_GE_Header.Note = "_NOTE_" + _TestParaName + "_" + R_DCLabel_ICh[Convert.ToInt16(MeasDC[i])];      //re-assign ge header 
                        Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);

                        ResultBuilder.BuildResults(ref results, GE_TestParam, "A", R_DC_ICh[Convert.ToInt16(MeasDC[i])]);
                    }
                }
                if (_Test_Switch)
                {
                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                    ResultBuilder.BuildResults(ref results, GE_TestParam, "NA", R_Switch);
                }
                if (R_RFCalStatus == 1)
                {
                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                    ResultBuilder.BuildResults(ref results, GE_TestParam, "NA", R_RFCalStatus);
                }
                if (b_TestBoard_temp)
                {
                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                    ResultBuilder.BuildResults(ref results, GE_TestParam, "C", R_Temperature);
                }
                if (_Test_TestTime)
                {
                    Rslt_GE_Header.MeasType = "M";      //re-assign ge header 
                    Rslt_GE_Header.Param = "_TIME";      //re-assign ge header 
                    Rslt_GE_Header.Note = "_NOTE_" + _TestNum;      //re-assign ge header 

                    Construct_GE_Header(TestPara, Rslt_GE_Header, DicTestLabel, MeasBand, out GE_TestParam, b_SmuHeader);
                    //ResultBuilder.BuildResults(ref results, GE_TestParam, "mS", tTime.ElapsedMilliseconds);
                    ResultBuilder.BuildResults(ref results, GE_TestParam, "mS", tTime.Elapsed.TotalMilliseconds);
                }
                #endregion
            }
        }
        private void InitResultVariable()
        {
            R_NF1_Ampl = -999;
            R_NF2_Ampl = -999;
            R_NF1_Freq = -999;
            R_NF2_Freq = -999;
            R_H2_Ampl = -999;
            R_H2_Freq = -999;
            R_Pin = -999;
            R_Pout = -999;
            R_ITotal = -999;
            R_MIPI = -999;
            R_DCSupply = -999;
            R_Switch = -999;
            R_RFCalStatus = -999;
        }        
        //Get Power Blast Setting
        public void searchPowerBlastKey(string testParam, string searchKey, out double cntrFreqMhz, out double startPwrlvl, out double stopPwrlvl, out int stepPwrlvl,
                                        out double dwellTimeMs, out double transientMs, out int transientStep, out bool b_pwrBlastTKey)
        {
            string tmpStingOut = null;

            //initialize variable - reset to default
            b_pwrBlastTKey = false;
            cntrFreqMhz = -999;
            startPwrlvl = -999;
            stopPwrlvl = -999;
            stepPwrlvl = -999;
            dwellTimeMs = -999;
            transientMs = -999;
            transientStep = -999;

            //Data from Mipi custom spreadsheet 
            foreach (Dictionary<string, string> currPwrBlast in DicPwrBlast)
            {
                currPwrBlast.TryGetValue("TEST SELECTION", out DicMipiTKey);

                if (searchKey.ToUpper() == DicMipiTKey)
                {
                    currPwrBlast.TryGetValue("CENTERFREQ_MHZ", out tmpStingOut);
                    cntrFreqMhz = Convert.ToDouble(tmpStingOut);

                    currPwrBlast.TryGetValue("START_PWRLVL", out tmpStingOut);
                    startPwrlvl = Convert.ToDouble(tmpStingOut);

                    currPwrBlast.TryGetValue("STOP_PWRLVL", out tmpStingOut);
                    stopPwrlvl = Convert.ToDouble(tmpStingOut);

                    currPwrBlast.TryGetValue("STEP_PWRLVL", out tmpStingOut);
                    stepPwrlvl = Convert.ToInt32(tmpStingOut);

                    currPwrBlast.TryGetValue("DWELLT_MS", out tmpStingOut);
                    dwellTimeMs = Convert.ToDouble(tmpStingOut);

                    currPwrBlast.TryGetValue("TRANSIENT_MS", out tmpStingOut);
                    transientMs = Convert.ToDouble(tmpStingOut);

                    currPwrBlast.TryGetValue("TRANSIENT_STEP", out tmpStingOut);
                    transientStep = Convert.ToInt32(tmpStingOut);

                    b_pwrBlastTKey = true;          //change flag if match
                }
            }

            if (!b_pwrBlastTKey)        //if cannot find , show error
                MessageBox.Show("Failed to find Power Blast Test Selection KEY (" + searchKey.ToUpper() + ") in PWRBlast sheet \n\n", testParam.ToUpper(), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        public void NFvariables(Dictionary<string, string> TestPara, string TAG, out double[] AntForTxMeasure, out double[] LNAInputLoss, out double[] RXPathLoss, out double[] RXContactFreq)
        {
            string StrError = string.Empty;

            double _RXFreq = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstRXFreq));
            double _StartRXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStartRXFreq1));
            double _StopRXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStopRXFreq1));
            double _StepRXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStepRXFreq1));
            string _SwBand_HotNF = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSwitching_Band_HotNF);

            //Load cal factor           
            double _LossCouplerPath = 999;
            double _LossOutputPathRX1 = 999;

            NoOfPts = (Convert.ToInt32(Math.Ceiling((Math.Round(_StopRXFreq1, 3) - Math.Round(_StartRXFreq1, 3)) / Math.Round(_StepRXFreq1, 3)))) + 1;

            RXContactFreq = new double[NoOfPts];
            RXPathLoss = new double[NoOfPts];
            LNAInputLoss = new double[NoOfPts];
            AntForTxMeasure = new double[0];


            #region Decode Calibration Path and Segment Data
            string CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand_HotNF.ToUpper());
            myUtility.Decode_CalSegm_Setting(CalSegmData);
            #endregion

            #region Get the loss data at Rx Freq
            _RXFreq = _StartRXFreq1;

            int count = Convert.ToInt16(Math.Ceiling((Math.Round(_StopRXFreq1, 3) - Math.Round(_StartRXFreq1, 3)) / Math.Round(_StepRXFreq1, 3)));

            for (int i = 0; i <= count; i++)
            {
                RXContactFreq[i] = Math.Round(_RXFreq, 3);

                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.RX1CalSegm, _RXFreq, ref _LossOutputPathRX1, ref StrError);
                RXPathLoss[i] = _LossOutputPathRX1;//Seoul

                ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _RXFreq, ref _LossCouplerPath, ref StrError);
                LNAInputLoss[i] = _LossCouplerPath;//Seoul

                _RXFreq = Convert.ToSingle(Math.Round(_RXFreq + _StepRXFreq1, 3));           //need to use round function because of C# float and double floating point bug/error

                if (_RXFreq > _StopRXFreq1)//For Last Freq match
                {
                    _RXFreq = _StopRXFreq1;
                }
            }
            #endregion Get the loss data at Rx Freq

            #region Get the loss data at Tx Freq

            double _TXFreq = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstTXFreq));
            double _StartTxFreq = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStartTXFreq1));
            double _StopTxFreq = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStopTXFreq1));
            double _StepTXFreq1 = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstStepTXFreq1));
            string _SwBand_NF = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSwBand);

            if (TAG.ToUpper().Contains("HOT"))
            {
                int NoOfPtsTx = (Convert.ToInt32(Math.Ceiling((_StopTxFreq - _StartTxFreq) / _StepTXFreq1))) + 1;
                AntForTxMeasure = new double[NoOfPtsTx];

                CalSegmData = myUtility.ReadTextFile(DicCalInfo[DataFilePath.LocSettingPath], "NF_NONCA_CALTAG", _SwBand_NF.ToUpper());
                myUtility.Decode_CalSegm_Setting(CalSegmData);

                _TXFreq = _StartTxFreq;

                double _LossAntForTxMeasure = 999;

                count = Convert.ToInt16(Math.Ceiling((_StopTxFreq - _StartTxFreq) / _StepTXFreq1));

                for (int i = 0; i <= count; i++)
                {
                    ATFCrossDomainWrapper.Cal_GetCalData1DCombined(LocalSetting.CalTag, myUtility.CalSegm_Setting.ANTCalSegm, _TXFreq, ref _LossAntForTxMeasure, ref StrError);
                    AntForTxMeasure[i] = _LossAntForTxMeasure;

                    _TXFreq = Convert.ToSingle(Math.Round(_TXFreq + _StepTXFreq1, 3));           //need to use round function because of C# float and double floating point bug/error

                    if (_TXFreq > _StopTxFreq)//For Last Freq match
                    {
                        _TXFreq = _StopTxFreq;
                    }
                }
            }
            #endregion Get the loss data at Tx Freq
        }
        public void StoreNFdata(int TestCount, int NumberOfRuns, int NoOfPts, double[][] NFdata, int ArryNum = 0)
        {
            for (int n = 0; n < NumberOfRuns; n++)
            {
                for (int istep = 0; istep < NoOfPts; istep++)
                {
                    PXITrace[TestCount].Multi_Trace[ArryNum][n].Ampl[istep] = Math.Round(NFdata[n][istep], 3);
                    PXITraceRaw[TestCount].Multi_Trace[ArryNum][n].Ampl[istep] = Math.Round(NFdata[n][istep], 3);
                }
            }
        }
        public void StoreNFRisedata(int NFRiseTestCount, int ColdNFTestCount, int HotNFTestCount, int ColdNFNumberOfRuns, int HotNFNumberOfRuns, int ColdNFNoOfPts, int HotNFNoOfPts, string TestParaName, int TestNum, int ColdArryNum = 0, int HotArryNum = 0)
        {
            for (int n = 0; n < HotNFNumberOfRuns; n++)
            {
                Dictionary<double, double> dic_ColdNF = new Dictionary<double, double>();
                Dictionary<double, double> dic_HotNF = new Dictionary<double, double>();

                //temp trace array storage use for MAX , MIN etc calculation 
                PXITrace[TestCount].Enable = true;
                //PXITrace[TestCount].SoakSweep = preSoakSweep;
                PXITrace[TestCount].TestNumber = TestNum;
                PXITrace[TestCount].TraceCount = HotNFNumberOfRuns;
                PXITrace[TestCount].Multi_Trace[0][n].NoPoints = HotNFNoOfPts;
                PXITrace[TestCount].Multi_Trace[0][n].RBW_Hz = PXITrace[HotNFTestCount].Multi_Trace[0][n].RBW_Hz;
                PXITrace[TestCount].Multi_Trace[0][n].FreqMHz = new double[HotNFNoOfPts];
                PXITrace[TestCount].Multi_Trace[0][n].Ampl = new double[HotNFNoOfPts];
                PXITrace[TestCount].Multi_Trace[0][n].Result_Header = TestParaName;
                PXITrace[TestCount].Multi_Trace[0][n].MXA_No = "PXI_NF_RISE_Trace";

                PXITraceRaw[TestCount].Multi_Trace[0][n].FreqMHz = new double[HotNFNoOfPts];
                PXITraceRaw[TestCount].Multi_Trace[0][n].Ampl = new double[HotNFNoOfPts];

                for (int istep = 0; istep < ColdNFNoOfPts; istep++)
                {
                    dic_ColdNF.Add(PXITrace[ColdNFTestCount].Multi_Trace[ColdArryNum][0].FreqMHz[istep], PXITrace[ColdNFTestCount].Multi_Trace[ColdArryNum][0].Ampl[istep]); //1st sweep of cold NF data is used for NF Rise calculation                    
                }

                for (int istep = 0; istep < HotNFNoOfPts; istep++)
                {
                    dic_HotNF.Add(PXITrace[HotNFTestCount].Multi_Trace[HotArryNum][n].FreqMHz[istep], PXITrace[HotNFTestCount].Multi_Trace[HotArryNum][n].Ampl[istep]);
                }

                for (int istep = 0; istep < HotNFNoOfPts; istep++)
                {
                    double nfFreq = PXITrace[HotNFTestCount].Multi_Trace[HotArryNum][n].FreqMHz[istep];

                    PXITrace[TestCount].Multi_Trace[0][n].FreqMHz[istep] = PXITrace[HotNFTestCount].Multi_Trace[HotArryNum][n].FreqMHz[istep];
                    PXITraceRaw[TestCount].Multi_Trace[0][n].FreqMHz[istep] = PXITrace[HotNFTestCount].Multi_Trace[HotArryNum][n].FreqMHz[istep];

                    if (dic_HotNF[nfFreq].ToString() == ("9999") || dic_ColdNF[nfFreq].ToString() == ("9999"))
                    {
                        PXITrace[TestCount].Multi_Trace[0][n].Ampl[istep] = 9999;
                        PXITraceRaw[TestCount].Multi_Trace[0][n].Ampl[istep] = 9999;
                    }

                    else
                    {
                        PXITrace[TestCount].Multi_Trace[0][n].Ampl[istep] = dic_HotNF[nfFreq] - dic_ColdNF[nfFreq];
                        PXITraceRaw[TestCount].Multi_Trace[0][n].Ampl[istep] = dic_HotNF[nfFreq] - dic_ColdNF[nfFreq];
                    }
                }
            }

        }

        public void Decode_GE_Header(Dictionary<string, string> TestPara, out s_GE_Header GE_Header)
        {
            //Note : MeasBand param required to report out the correct measurement band

            #region initialize GE Header to default value

            #region Read Configuration from TCF
            string _TestMode = myUtility.ReadTcfData(TestPara, TCF_Header.ConstTestMode);
            string _TestParam = myUtility.ReadTcfData(TestPara, TCF_Header.ConstTestParam);
            string _TestParaName = myUtility.ReadTcfData(TestPara, TCF_Header.ConstParaName);
            string _TX1Band = myUtility.ReadTcfData(TestPara, TCF_Header.ConstTX1Band);
            if (_TX1Band == "0" || _TX1Band == null || _TX1Band == "")
            {
                _TX1Band = "x";
            }
            string _RX1Band = myUtility.ReadTcfData(TestPara, TCF_Header.ConstRX1Band);
            if (_RX1Band == "0" || _RX1Band == null || _RX1Band == "")
            {
                _RX1Band = "x";
            }
            string _Pout1 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstPout1);
            string _Pin1 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstPin1);
            string _PowerMode = myUtility.ReadTcfData(TestPara, TCF_Header.ConstPowerMode);
            if (_PowerMode == "0" || _PowerMode == null || _PowerMode == "")
            {
                _PowerMode = "x";
            }
            string _Modulation = myUtility.ReadTcfData(TestPara, TCF_Header.ConstModulation);
            if (_Modulation == "0" || _Modulation == null || _Modulation == "")
            {
                _Modulation = "x";
            }
            string _WaveFormName = myUtility.ReadTcfData(TestPara, TCF_Header.ConstWaveformName);
            if (_WaveFormName == "0" || _WaveFormName == null || _WaveFormName == "")
            {
                _WaveFormName = "x";
            }
            string _Search_Method = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSearch_Method);
            string _NF_BW = myUtility.ReadTcfData(TestPara, TCF_Header.ConstNF_BW);
            if (_NF_BW == "0" || _NF_BW == null || _NF_BW == "")
            {
                _NF_BW = "x";
            }
            string _Note = myUtility.ReadTcfData(TestPara, TCF_Header.ConstNote);
            if (_Note == "0" || _Note == null || _Note == "")
            {
                _Note = "x";
            }
            string _TxDac = myUtility.ReadTcfData(TestPara, TCF_Header.ConstTxDac);
            if (_TxDac == "0" || _TxDac == null || _TxDac == "")
            {
                _TxDac = "x";
            }
            string _RxDac = myUtility.ReadTcfData(TestPara, TCF_Header.ConstRxDac);
            if (_RxDac == "0" || _RxDac == null || _RxDac == "")
            {
                _RxDac = "x";
            }

            //Sweep TX1/RX1 Freq Condition
            string _StartTXFreq1 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstStartTXFreq1);
            string _StopTXFreq1 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstStopTXFreq1);
            string _StartRXFreq1 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstStartRXFreq1);
            string _StopRXFreq1 = myUtility.ReadTcfData(TestPara, TCF_Header.ConstStopRXFreq1);

            // Add - Ben
            string _InfoTxPort = myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Tx_Port);
            string _InfoANTPort = myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_ANT_Port);
            string _InfoRxPort = myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_Rx_Port);

            #endregion

            GE_Header = new s_GE_Header();
            GE_Header.Dac = new string[3];      //Temp Initialize to 3 arrays

            GE_Header.b_Header = true;
            GE_Header.MeasType = "x";
            GE_Header.Param = "_x";
            GE_Header.Band = "_x";
            GE_Header.Pmode = "_x";
            GE_Header.Modulation = "_x";
            GE_Header.Waveform = "_x";
            GE_Header.PType = "_x";
            GE_Header.Pwr = "_x";
            GE_Header.MeasInfo = "_x";
            GE_Header.Freq1 = "_x";
            GE_Header.Freq2 = "_x";
            // Add - Ben
            GE_Header.InfoTxPort = "_x";
            GE_Header.InfoANTPort = "_x";
            GE_Header.InfoRxPort = "_x";
            GE_Header.Vcc = "_x";
            GE_Header.Vbat = "_x";
            GE_Header.Vdd = "_x";
            GE_Header.Note = "_NOTE_x";

            if (_InfoTxPort == "0")
                _InfoTxPort = "x";

            if (_InfoANTPort == "0")
                _InfoANTPort = "x";

            if (_InfoRxPort == "0")
                _InfoRxPort = "x";

            #region decode Dac Header

            int TxDacArr = 0;
            int RxDacArr = 0;
            string[] tmpArr;

            foreach (string key in DicTestLabel.Keys)
            {
                if (key == "TXDAC")
                {
                    tmpArr = DicTestLabel[key].ToString().Split('=');
                    TxDacArr = Convert.ToInt16(tmpArr[1]);
                }
                if (key == "RXDAC")
                {
                    tmpArr = DicTestLabel[key].ToString().Split('=');
                    RxDacArr = Convert.ToInt16(tmpArr[1]);

                    try
                    {
                        var _RxDic = _RxDac.Split('=');
                        if (_RxDic.Length > 1 && _RxDac.Split('=')[1].Split(';').Count() != RxDacArr)
                        {
                            RxDacArr = _RxDac.Split('=')[1].Split(';').Count();
                        }
                    }
                    catch (Exception Ex)
                    {
                        RxDacArr = Convert.ToInt16(tmpArr[1]);
                    }
                }
            }

            GE_Header.Dac = new string[TxDacArr + RxDacArr];

            for (int i = 0; i < GE_Header.Dac.Length; i++)
            {
                GE_Header.Dac[i] = "_x";    //Initialize to default
            }

            #region Search and return Data from Mipi custom spreadsheet
            bool b_mipiTKey = false;
            string[] tmpArr_addrs;
            string[] tmpBias_Data;
            int regNo = 0;

            if (_TxDac != "x" || _RxDac != "x")
            {
                if (_TxDac != "x")
                {
                    regNo = 0;       //start tx reg array always at 0
                    tmpArr = _TxDac.Split('=');
                    tmpArr_addrs = tmpArr[1].Split(';');

                    searchMIPIKey(_TestParam, tmpArr[0], out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                    biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter

                    for (int j = 0; j < TxDacArr; j++)
                    {
                        for (int i = 0; i < biasDataArr.Length; i++)
                        {
                            tmpBias_Data = biasDataArr[i].Split(':');
                            if ((Convert.ToInt32(tmpBias_Data[0], 16)) == (Convert.ToInt32(tmpArr_addrs[j], 16)))
                            {
                                GE_Header.Dac[regNo] = "_0x" + tmpBias_Data[1];
                                regNo++;
                                break;
                            }
                        }
                    }
                }
                if (_RxDac != "x")
                {
                    regNo = TxDacArr;       //start rx reg no array after TxDacArr
                    tmpArr = _RxDac.Split('=');
                    tmpArr_addrs = tmpArr[1].Split(';');

                    searchMIPIKey(_TestParam, tmpArr[0], out CusMipiRegMap, out CusPMTrigMap, out CusSlaveAddr, out CusMipiPair, out CusMipiSite, out b_mipiTKey);

                    biasDataArr = CusMipiRegMap.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);     //split string with blank space as delimiter

                    for (int j = 0; j < RxDacArr; j++)
                    {
                        for (int i = 0; i < biasDataArr.Length; i++)
                        {
                            tmpBias_Data = biasDataArr[i].Split(':');
                            if ((Convert.ToInt32(tmpBias_Data[0], 16)) == (Convert.ToInt32(tmpArr_addrs[j], 16)))
                            {
                                GE_Header.Dac[regNo] = "_0x" + tmpBias_Data[1];
                                regNo++;
                                break;
                            }
                        }
                    }
                }
            }

            #endregion

            #endregion

            #region Read Waveform Alias
            foreach (string key in DicWaveFormAlias.Keys)
            {
                if (key == _WaveFormName.ToUpper())
                {
                    if (_WaveFormName.ToUpper() != "CW")
                    {
                        GE_Header.Waveform = "_" + DicWaveFormAlias[key].ToString();
                    }
                }
            }
            #endregion

            #endregion

            switch (_TestMode.ToUpper())
            {
                case "MXA_TRACE":
                    #region LXI Trace Calculation
                    switch (_TestParam.ToUpper())
                    {
                        case "CALC_MXA_TRACE":
                            break;
                        case "MERIT_FIGURE":
                            break;
                        default:
                            MessageBox.Show("Test Parameter : " + _TestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }
                    break;
                #endregion
                case "PXI_TRACE":
                    #region PXI Trace Calculation
                    GE_Header.MeasType = "N";
                    switch (_TestParam.ToUpper())
                    {
                        case "CALC_PXI_TRACE":
                            break;
                        case "MERIT_FIGURE":
                            break;
                        case "MAX_MIN":
                            break;
                        case "TRACE_MERIT_FIGURE":
                            break;
                        case "NF_MAX_MIN":
                            GE_Header.Band = "_" + _RX1Band;
                            GE_Header.Pmode = "_" + _PowerMode;
                            GE_Header.Modulation = "_" + _Modulation;
                            GE_Header.MeasInfo = "_" + _NF_BW + "MBW";

                            // Add - Ben
                            GE_Header.InfoTxPort = "_" + _InfoTxPort;
                            GE_Header.InfoANTPort = "_" + _InfoANTPort;
                            GE_Header.InfoRxPort = "_" + _InfoRxPort;
                            break;
                        case "NF_MAX_MIN_COLD":
                            GE_Header.Band = "_" + _RX1Band;
                            GE_Header.Pmode = "_" + _PowerMode;
                            GE_Header.Modulation = "_" + _Modulation;
                            GE_Header.MeasInfo = "_" + _NF_BW + "MBW";

                            // Add - Ben
                            GE_Header.InfoTxPort = "_" + _InfoTxPort;
                            GE_Header.InfoANTPort = "_" + _InfoANTPort;
                            GE_Header.InfoRxPort = "_" + _InfoRxPort;
                            break;
                        case "NF_MAX_MIN_MIPI":
                            GE_Header.Band = "_" + _RX1Band;
                            GE_Header.Pmode = "_" + _PowerMode;
                            GE_Header.Modulation = "_" + _Modulation;
                            GE_Header.MeasInfo = "_" + _NF_BW + "MBW";

                            // Add - Ben
                            GE_Header.InfoTxPort = "_" + _InfoTxPort;
                            GE_Header.InfoANTPort = "_" + _InfoANTPort;
                            GE_Header.InfoRxPort = "_" + _InfoRxPort;
                            break;
                        case "NF_FETCH":
                            GE_Header.Band = "_" + _RX1Band;
                            GE_Header.Pmode = "_" + _PowerMode;
                            GE_Header.Modulation = "_" + _Modulation;
                            GE_Header.MeasInfo = "_" + _NF_BW + "MBW";
                            break;
                        case "NFG_TRACE_COLD":
                            GE_Header.MeasType = "NFG";
                            GE_Header.Param = "_NF";
                            GE_Header.Band = "_" + _RX1Band;
                            GE_Header.Pmode = "_" + _PowerMode;
                            GE_Header.PType = "_FixedPin";
                            GE_Header.Pwr = "_" + _Pin1 + "dBm";
                            GE_Header.MeasInfo = "_" + _NF_BW + "MBW";

                            // Add - Ben
                            GE_Header.InfoTxPort = "_" + _InfoTxPort;
                            GE_Header.InfoANTPort = "_" + _InfoANTPort;
                            GE_Header.InfoRxPort = "_" + _InfoRxPort;
                            break;
                        default:
                            MessageBox.Show("Test Parameter : " + _TestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }
                    break;
                #endregion
                case "COMMON":
                    #region Common Math Function
                    switch (_TestParam.ToUpper())
                    {
                        case "MAX_MIN":
                            break;
                        case "AVERAGE":
                            break;
                        case "DELTA":
                            break;
                        case "SUM":
                            break;

                        default:
                            MessageBox.Show("Test Parameter : " + _TestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }
                    break;
                #endregion
                case "CALIBRATION":
                    #region System Calibration
                    switch (_TestParam.ToUpper())
                    {
                        case "RF_CAL":
                            break;
                        case "NF_CAL":
                            break;
                        default:
                            MessageBox.Show("Test Parameter : " + _TestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }
                    break;
                #endregion
                case "DC":
                    #region DC Setting
                    GE_Header.MeasType = "M";
                    switch (_TestParam.ToUpper())
                    {
                        case "PS4CH":
                        case "PS1CH":
                        case "MULTI_DCSUPPLY":
                            GE_Header.Note = "_NOTE_" + _Note;
                            break;
                        case "SMU":
                            break;
                        case "SMU_LEAKAGE":
                            GE_Header.Pmode = "_" + _PowerMode;
                            break;
                        default:
                            MessageBox.Show("Test Parameter : " + _TestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }
                    break;
                #endregion
                case "MIPI":
                    #region MIPI
                    GE_Header.MeasType = "M";
                    switch (_TestParam.ToUpper())
                    {
                        case "SETMIPI":
                        case "SETMIPI_SMU":
                        case "SETMIPI_CUSTOM":
                        case "SETMIPI_CUSTOM_SMU":
                            GE_Header.Param = "_MIPI_NumBitErrors";
                            GE_Header.Band = "_" + _TX1Band;
                            GE_Header.Pmode = "_" + _PowerMode;
                            GE_Header.Modulation = "_" + _Modulation;
                            GE_Header.Note = "_NOTE_" + _Note;

                            // Add - Ben
                            GE_Header.InfoTxPort = "_" + _InfoTxPort;
                            GE_Header.InfoANTPort = "_" + _InfoANTPort;
                            GE_Header.InfoRxPort = "_" + _InfoRxPort;
                            break;
                        case "READMIPI_REG_CUSTOM":
                            switch (_Search_Method.ToUpper())
                            {
                                case "TEMP":
                                case "TEMPERATURE":
                                    GE_Header.MeasType = "N";
                                    GE_Header.Param = "_MIPI_TEMP";
                                    GE_Header.Band = "_" + _TX1Band;
                                    GE_Header.Pmode = "_" + _PowerMode;
                                    GE_Header.Note = "_NOTE_" + _Note;
                                    break;
                            }
                            break;
                        case "READ_OTP_CUSTOM":
                            GE_Header.Param = "_MIPI_NumBitErrors";
                            GE_Header.Band = "_" + _TX1Band;
                            GE_Header.Pmode = "_" + _PowerMode;
                            GE_Header.Modulation = "_" + _Modulation;
                            GE_Header.Note = "_NOTE_" + _Note;
                            break;
                        case "BURN_OTP_JEDI":
                        case "BURN_OTP_JEDI2":
                            GE_Header.Param = "_MIPI_OTPBURN";
                            GE_Header.Note = "_NOTE_" + _Note;
                            break;
                        case "MIPI_LEAKAGE":
                            break;
                        default:
                            MessageBox.Show("Test Parameter : " + _TestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }
                    break;
                #endregion
                case "MIPI_OTP":
                    #region MIPI OTP
                    switch (_TestParam.ToUpper())
                    {
                        case "READ_OTP_SELECTIVE_BIT":
                            GE_Header.MeasType = "M";
                            GE_Header.Param = "_MIPI";
                            GE_Header.Note = "_NOTE_" + _TestParaName;

                            switch (_Search_Method.ToUpper())
                            {
                                //case "REV_ID":
                                //    GE_Header.Param = "_MIPI_ReadRxReg-21";
                                //    GE_Header.Note = "_NOTE_" + _Note;
                                //    break;
                                //case "CM_ID":
                                //    GE_Header.Param = "_MIPI_CM-ID";
                                //    GE_Header.Note = "_NOTE_" + _Note;
                                //    break;
                                case "UNIT_ID":
                                case "UNIT_ID_MANUAL_SET":
                                case "UNIT_2DID":
                                case "CUSTOM_HEADER":
                                case "READ_2DID_FROM_OTHER_OTP_BIT":
                                    GE_Header.b_Header = false;     //set to false - no required to display the GE Header Format . Use Parameter Header define in TCF only . Cause RF1 & RF2 using custom header for CMOS X-Y, MFG_ID, MODULE_ID etc ..
                                    GE_Header.Param = _TestParaName;
                                    break;
                            }
                            break;
                        case "BURN_OTP_SELECTIVE_BIT":
                            GE_Header.MeasType = "M";
                            GE_Header.Param = "_MIPI_OTPBURN";
                            GE_Header.Note = "_NOTE_" + _TestParaName;
                            break;
                        default:
                            MessageBox.Show("Test Parameter : " + _TestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }
                    break;
                #endregion
                case "MIPI_VEC":
                    #region MIPI Vector
                    switch (_TestParam.ToUpper())
                    {
                        case "OTP_BURN_LNA":
                            GE_Header.MeasType = "M";
                            GE_Header.Param = "_MIPI_OTPBURN";
                            GE_Header.Note = "_NOTE_" + _TestParaName;
                            break;
                        default:
                            MessageBox.Show("Test Parameter : " + _TestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }
                    break;
                #endregion
                case "SWITCH":
                    #region Switch Config
                    GE_Header.MeasType = "M";
                    switch (_TestParam.ToUpper())
                    {
                        case "SETSWITCH":
                            GE_Header.Note = "_NOTE_" + _Note;

                            // Add - Ben
                            GE_Header.InfoTxPort = "_" + _InfoTxPort;
                            GE_Header.InfoANTPort = "_" + _InfoANTPort;
                            GE_Header.InfoRxPort = "_" + _InfoRxPort;
                            break;
                        default:
                            MessageBox.Show("Test Parameter : " + _TestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }
                    break;
                #endregion
                case "NF":
                    #region NF Measurement
                    GE_Header.MeasType = "N";
                    switch (_TestParam.ToUpper())
                    {
                        case "NF_CA_NDIAG":
                            break;
                        case "NF_NONCA_NDIAG":
                            break;
                        case "NF_FIX_NMAX":
                            break;
                        case "RXPATH_CONTACT":
                            break;
                        case "NF_STEPSWEEP_NDIAG":
                            break;
                        case "PXI_NF_NONCA_NDIAG":
                            break;
                        case "PXI_NF_FIX_NMAX":
                            break;
                        case "PXI_RXPATH_CONTACT":
                            GE_Header.Param = "_Gain_RX";
                            GE_Header.Band = "_" + _RX1Band;
                            GE_Header.Pmode = "_" + _PowerMode;
                            GE_Header.Modulation = "_" + _Modulation;
                            GE_Header.PType = "_FixedPin";
                            GE_Header.Pwr = "_" + _Pin1 + "dBm";

                            // Add - Ben
                            GE_Header.InfoTxPort = "_" + _InfoTxPort;
                            GE_Header.InfoANTPort = "_" + _InfoANTPort;
                            GE_Header.InfoRxPort = "_" + _InfoRxPort;
                            break;
                        case "PXI_FIXED_POWERBLAST":
                            //GE_Header.Param = "_NF_Hot";
                            GE_Header.Band = "_" + _TX1Band;
                            GE_Header.Pmode = "_" + _PowerMode;
                            GE_Header.Modulation = "_" + _Modulation;
                            GE_Header.PType = "_FixedPout";
                            GE_Header.Pwr = "_" + _Pout1 + "dBm";
                            GE_Header.Freq1 = "_Tx-" + _StartTXFreq1 + "MHz";
                            break;
                        case "PXI_RAMP_POWERBLAST":
                            //GE_Header.Param = "_NF_Hot";
                            GE_Header.Band = "_" + _TX1Band;
                            GE_Header.Pmode = "_" + _PowerMode;
                            GE_Header.Modulation = "_" + _Modulation; ;
                            GE_Header.PType = "_FixedPout";
                            GE_Header.Pwr = "_" + _Pout1 + "dBm";
                            GE_Header.Freq1 = "_Tx-" + _StartTXFreq1 + "MHz";
                            break;
                        case "PXI_RXPATH_GAIN":
                        case "PXI_RXPATH_GAIN_NF":
                            GE_Header.Param = "_Gain_RX";
                            GE_Header.Band = "_" + _RX1Band;
                            GE_Header.Pmode = "_" + _PowerMode;
                            GE_Header.Modulation = "_" + _Modulation;
                            GE_Header.PType = "_FixedPin";
                            GE_Header.Pwr = "_" + _Pin1 + "dBm";

                            // Add - Ben
                            GE_Header.InfoTxPort = "_" + _InfoTxPort;
                            GE_Header.InfoANTPort = "_" + _InfoANTPort;
                            GE_Header.InfoRxPort = "_" + _InfoRxPort;
                            break;
                        case "PXI_NF_COLD":
                            GE_Header.Param = "_NF_Cold";
                            GE_Header.Band = "_" + _RX1Band;
                            GE_Header.Pmode = "_" + _PowerMode;
                            GE_Header.MeasInfo = "_" + _NF_BW + "MBW";
                            GE_Header.Freq1 = "_Rx-" + _StartRXFreq1 + "MHz";

                            // Add - Ben
                            GE_Header.InfoTxPort = "_" + _InfoTxPort;
                            GE_Header.InfoANTPort = "_" + _InfoANTPort;
                            GE_Header.InfoRxPort = "_" + _InfoRxPort;
                            break;
                        case "PXI_NF_HOT":
                            GE_Header.Param = "_NF_Hot";
                            GE_Header.Band = "_" + _TX1Band;
                            GE_Header.Pmode = "_" + _PowerMode;
                            GE_Header.Modulation = "_" + _Modulation;
                            GE_Header.PType = "_FixedPout";
                            GE_Header.Pwr = "_" + _Pout1 + "dBm";
                            GE_Header.MeasInfo = "_" + _NF_BW + "MBW";
                            GE_Header.Freq1 = "_Tx-" + _StartTXFreq1 + "MHz";
                            GE_Header.Freq2 = "_Tx-" + _StopTXFreq1 + "MHz";

                            // Add - Ben
                            GE_Header.InfoTxPort = "_" + _InfoTxPort;
                            GE_Header.InfoANTPort = "_" + _InfoANTPort;
                            GE_Header.InfoRxPort = "_" + _InfoRxPort;
                            if (_StartTXFreq1 == _StopTXFreq1)
                            {
                                GE_Header.Note = "_NOTE_NMAX_" + _RX1Band;
                            }
                            else
                            {
                                GE_Header.Note = "_NOTE_NDIAG_" + _RX1Band;
                            }
                            break;
                        case "PXI_NF_COLD_MIPI": // Ben, Add for MIPI NFR
                            GE_Header.Param = "_NF_Cold-MIPI"; // Need to modify
                            GE_Header.Band = "_" + _RX1Band;
                            GE_Header.Pmode = "_" + _PowerMode;
                            GE_Header.MeasInfo = "_" + _NF_BW + "MBW";
                            GE_Header.Freq1 = "_Rx-" + _StartRXFreq1 + "MHz";

                            // Add - Ben
                            GE_Header.InfoTxPort = "_" + _InfoTxPort;
                            GE_Header.InfoANTPort = "_" + _InfoANTPort;
                            GE_Header.InfoRxPort = "_" + _InfoRxPort;
                            break;
                        case "PXI_NF_MEAS":
                            GE_Header.MeasType = "NFG";
                            GE_Header.Param = "_NF";
                            GE_Header.Band = "_" + _RX1Band;
                            GE_Header.Pmode = "_" + _PowerMode;
                            GE_Header.PType = "_FixedPin";
                            GE_Header.Pwr = "_" + _Pin1 + "dBm";
                            GE_Header.MeasInfo = "_" + _NF_BW + "MBW";
                            GE_Header.Freq1 = "_Rx-" + _StartRXFreq1 + "MHz";
                            break;

                        case "PXI_NF_COLD_ALLINONE":
                            GE_Header.Param = "_NF_Cold-ALLINONE";
                            GE_Header.Band = "_" + _RX1Band;
                            GE_Header.Pmode = "_" + _PowerMode;
                            GE_Header.MeasInfo = "_" + _NF_BW + "MBW";
                            GE_Header.Freq1 = "_Rx-" + _StartRXFreq1 + "MHz";

                            // Add - Ben
                            GE_Header.InfoTxPort = "_" + _InfoTxPort;
                            GE_Header.InfoANTPort = "_" + _InfoANTPort;
                            GE_Header.InfoRxPort = "_" + _InfoRxPort;
                            break;

                        case "PXI_NF_COLD_MIPI_ALLINONE":
                            GE_Header.Param = "_NF_Cold-MIPI-ALLINONE";
                            GE_Header.Band = "_" + _RX1Band;
                            GE_Header.Pmode = "_" + _PowerMode;
                            GE_Header.MeasInfo = "_" + _NF_BW + "MBW";
                            GE_Header.Freq1 = "_Rx-" + _StartRXFreq1 + "MHz";

                            // Add - Ben
                            GE_Header.InfoTxPort = "_" + _InfoTxPort;
                            GE_Header.InfoANTPort = "_" + _InfoANTPort;
                            GE_Header.InfoRxPort = "_" + _InfoRxPort;

                            break;
                        default:
                            MessageBox.Show("NF Test Parameter : " + _TestParam.ToUpper() + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }
                    break;
                #endregion
                case "MKR_NF":
                    #region LXI Marker Function
                    switch (_TestParam.ToUpper())
                    {
                        case "NF_CA_NDIAG":
                            break;
                        case "NF_NONCA_NDIAG":
                            break;
                        case "NF_FIX_NMAX":
                            break;
                        default:
                            MessageBox.Show("NF Test Parameter : " + _TestParam.ToUpper() + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }
                    break;
                #endregion
                case "TESTBOARD":
                    #region Testboard ID & Temp
                    switch (_TestParam.ToUpper())
                    {
                        case "TEMPERATURE":
                            GE_Header.MeasType = "M";
                            GE_Header.Param = "_MIPI_TEMP";
                            GE_Header.Note = "_NOTE_" + _Note;
                            break;
                        default:
                            MessageBox.Show("Test Parameter : " + _TestParam + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                            break;
                    }
                    break;
                #endregion
                default:
                    MessageBox.Show("Test Mode " + _TestMode + " not supported at this moment.", "MyDUT", MessageBoxButtons.OK);
                    break;
            }
        }
        public void Construct_GE_Header(Dictionary<string, string> TestPara, s_GE_Header ge, Dictionary<string, string> DicTestLabel, string band, out string ge_HeaderStr, bool b_SmuHeader = false)
        {
            ge_HeaderStr = null;

            #region decode Dac Header

            string tmp_geHeader_DAC = null;

            for (int i = 0; i < ge.Dac.Length; i++)
            {
                tmp_geHeader_DAC = tmp_geHeader_DAC + ge.Dac[i];
            }

            #endregion

            #region decode SMU_Header
            string[] R_SMULabel_VCh;

            if (b_SmuHeader)
            {
                #region initialize data & result
                //Read TCF SMU Setting
                float[] _SMUVCh;
                _SMUVCh = new float[9];

                string _SMUSetCh = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUSetCh);
                string _SMUMeasCh = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUMeasCh);

                _SMUVCh[0] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh0));
                _SMUVCh[1] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh1));
                _SMUVCh[2] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh2));
                _SMUVCh[3] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh3));
                _SMUVCh[4] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh4));
                _SMUVCh[5] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh5));
                _SMUVCh[6] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh6));
                _SMUVCh[7] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh7));
                _SMUVCh[8] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh8));

                //to select which channel to set and measure - Format in TCF(DCSet_Channel) 1,4 -> means CH1 & CH4 to set/measure
                //string[] MeasSMU = _SMUMeasCh.Split(',');
                //R_SMULabel_VCh = new string[_SMUVCh.Length];
                //int ch_cnt = MeasSMU.Length;

                int _SMUTotal = Convert.ToInt32(SearchLocalSettingDictionary("SmuSetting", "TOTAL_SMUCHANNEL"));

                #endregion

                #region construct SMU header
                //for (int i = 0; i < MeasSMU.Count(); i++)
                for (int i = 0; i < _SMUTotal; i++)
                {
                    // pass out the test result label for every measurement channel
                    //int smuVChannel = Convert.ToInt16(MeasSMU[i]);
                    //string tempLabel = "SMUV_CH" + MeasSMU[i];
                    string tempLabel = "SMUV_CH" + i;
                    foreach (string key in DicTestLabel.Keys)
                    {
                        if (key == tempLabel)
                        {
                            if (DicTestLabel[key].ToString().ToUpper() == "VCC")
                            {
                                ge.Vcc = "_" + _SMUVCh[i] + "V";
                            }
                            if (DicTestLabel[key].ToString().ToUpper() == "VCC2") // Vcc_ET100

                            {
                                ge.Vcc = "_" + _SMUVCh[i] + "V";
                            }
                            if (DicTestLabel[key].ToString().ToUpper() == "VBATT")
                            {
                                ge.Vbat = "_" + _SMUVCh[i] + "Vbatt";
                            }
                            if (DicTestLabel[key].ToString().ToUpper() == "VDD")
                            {
                                ge.Vdd = "_" + _SMUVCh[i] + "Vdd";
                            }
                            break;
                        }
                    }
                }
                #endregion
            }

            #endregion

            //construct ge header string
            ge_HeaderStr = ge.MeasType + ge.Param + ge.Band + ge.Pmode + ge.Modulation + ge.Waveform
                + ge.PType + ge.Pwr + ge.MeasInfo + ge.Freq1 + ge.Freq2 + ge.InfoTxPort + ge.InfoANTPort + ge.InfoRxPort
                + ge.Vcc + ge.Vbat + ge.Vdd
                + tmp_geHeader_DAC + ge.Note;

        }
        public void Construct_GE_Header(Dictionary<string, string> TestPara, s_GE_Header ge, Dictionary<string, string> DicTestLabel, string band, double[] chSMU_I, out double[] smuRslt_I, out string[] ge_HeaderStr)
        {

            string tmp_geHeader_DAC = null;

            for (int i = 0; i < ge.Dac.Length; i++)
            {
                tmp_geHeader_DAC = tmp_geHeader_DAC + ge.Dac[i];
            }

            string[] R_SMULabel_VCh;
            smuRslt_I = new double[1]; //dummy data
            ge_HeaderStr = new string[1]; //dummy data

            bool _Test_SMU = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_SMU).ToUpper() == "V" ? true : false);
            bool _Test_NF1 = Convert.ToBoolean(myUtility.ReadTcfData(TestPara, TCF_Header.ConstPara_NF1).ToUpper() == "V" ? true : false);

            if (_Test_SMU)
            {
                #region initialize data & result
                //Read TCF SMU Setting
                float[] _SMUVCh;
                _SMUVCh = new float[9];

                string _SMUSetCh = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUSetCh);
                string _SMUMeasCh = myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUMeasCh);

                _SMUVCh[0] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh0));
                _SMUVCh[1] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh1));
                _SMUVCh[2] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh2));
                _SMUVCh[3] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh3));
                _SMUVCh[4] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh4));
                _SMUVCh[5] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh5));
                _SMUVCh[6] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh6));
                _SMUVCh[7] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh7));
                _SMUVCh[8] = Convert.ToSingle(myUtility.ReadTcfData(TestPara, TCF_Header.ConstSMUVCh8));

                //to select which channel to set and measure - Format in TCF(DCSet_Channel) 1,4 -> means CH1 & CH4 to set/measure
                string[] MeasSMU = _SMUMeasCh.Split(',');
                R_SMULabel_VCh = new string[_SMUVCh.Length];
                int ch_cnt = MeasSMU.Length;
                smuRslt_I = new double[ch_cnt];
                ge_HeaderStr = new string[ch_cnt];
                #endregion

                #region construct SMU header
                for (int i = 0; i < MeasSMU.Count(); i++)
                {
                    // pass out the test result label for every measurement channel
                    int smuVChannel = Convert.ToInt16(MeasSMU[i]);
                    string tempLabel = "SMUV_CH" + MeasSMU[i];
                    foreach (string key in DicTestLabel.Keys.Where(t => t.Contains("SMUV_CH")))
                    {
                        if (key == tempLabel)
                        {
                            if (DicTestLabel[key].ToString().ToUpper() == "VCC") // Vcc_ET40
                            {
                                ge.Vcc = "_" + _SMUVCh[smuVChannel] + "V";
                                smuRslt_I[i] = chSMU_I[smuVChannel];
                                if (_Test_NF1)
                                {
                                    ge.Param = "_Icc";
                                }
                                else
                                {
                                    ge.Param = "_Icc_Q";
                                }
                            }
                            if (DicTestLabel[key].ToString().ToUpper() == "VCC2") // Vcc_ET100
                            {
                                ge.Vcc = "_" + _SMUVCh[smuVChannel] + "V";
                                smuRslt_I[i] = chSMU_I[smuVChannel];
                                if (_Test_NF1)
                                {
                                    ge.Param = "_Icc2";
                                }
                                else
                                {
                                    ge.Param = "_Icc2_Q";
                                }
                            }
                            if (DicTestLabel[key].ToString().ToUpper() == "VBATT")
                            {
                                ge.Vbat = "_" + _SMUVCh[smuVChannel] + "Vbatt";
                                smuRslt_I[i] = chSMU_I[smuVChannel];
                                if (_Test_NF1)
                                {
                                    ge.Param = "_Ibatt";
                                }
                                else
                                {
                                    ge.Param = "_Ibatt_Q";
                                }
                            }
                            if (DicTestLabel[key].ToString().ToUpper() == "VDD")
                            {
                                ge.Vdd = "_" + _SMUVCh[smuVChannel] + "Vdd";
                                smuRslt_I[i] = chSMU_I[smuVChannel];
                                if (_Test_NF1)
                                {
                                    ge.Param = "_Idd";
                                }
                                else
                                {
                                    ge.Param = "_Idd_Q";
                                }
                            }
                            break;
                        }
                    }

                    //construct ge header string - partial
                    ge_HeaderStr[i] = ge.MeasType + ge.Param + ge.Band + ge.Pmode + ge.Modulation + ge.Waveform
                        + ge.PType + ge.Pwr + ge.MeasInfo + ge.Freq1 + ge.Freq2;
                }
                #endregion
            }

            for (int i = 0; i < ge_HeaderStr.Length; i++)
            {
                //construct ge header string
                ge_HeaderStr[i] = ge_HeaderStr[i] + ge.InfoTxPort + ge.InfoANTPort + ge.InfoRxPort
                    + ge.Vcc + ge.Vbat + ge.Vdd
                    + tmp_geHeader_DAC + ge.Note;
            }

        }

        //Temperally use for correlaton off set : To be obsolate...
        //private static void ReadGuCorrelationFile()
        //{
        //    bool GuInitSuccess = true;
        //    string correlationFilePath = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_CF_FULLPATH, "");

        //    // workaround for Clotho bug, suser pointing to TestPlans directory
        //    if (ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_VER, "") == "")  // is there a better way to know if I'm suser?
        //    {
        //        string correlationFileName = Path.GetFileName(ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_CF_FULLPATH, ""));
        //        correlationFilePath = @"C:\Avago.ATF.Common.x64\CorrelationFiles\Development\" + correlationFileName;
        //    }

        //    if (correlationFilePath == "" | correlationFilePath == null)
        //    {
        //        GuInitSuccess = false;
        //        return;
        //    }

        //    Dictionary<string, int> headerColumnLocaton = new Dictionary<string, int>();

        //    if (!File.Exists(correlationFilePath))
        //    {
        //        GuInitSuccess = false;
        //        return;
        //    }

        //    using (StreamReader calFile = new StreamReader(new FileStream(correlationFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
        //    {
        //        bool headerFound = false;

        //        while (!calFile.EndOfStream)
        //        {
        //            //string[] csvLine = calFile.ReadLine().Split(',');
        //            string[] csvLine = calFile.ReadLine().Split(',').TakeWhile(v => v != "").ToArray();

        //            //if (csvLine.TakeWhile(v => v != "").Count() < 7) continue;  // skip unimportant lines
        //            if (csvLine.Count() < 7) continue;  // skip unimportant lines

        //            string testName = csvLine[0];

        //            switch (testName)
        //            {
        //                case "ParameterName":   // the header row
        //                    headerFound = true;
        //                    for (int i = 0; i < csvLine.Length; i++)
        //                    {
        //                        headerColumnLocaton.Add(csvLine[i], i);
        //                    }
        //                    continue;

        //                default:   // calfactor data row
        //                    if (!headerFound)
        //                    {
        //                        GuInitSuccess = false;
        //                        return;
        //                    }

        //                    corrFileTestNameList.Add(testName);


        //                    float factorAdd = Convert.ToSingle(csvLine[headerColumnLocaton["Factor_Add"]]);
        //                    float factorMultiply = Convert.ToSingle(csvLine[headerColumnLocaton["Factor_Multiply"]]);

        //                    if (factorAdd != 0)
        //                    {
        //                        GuCalFactorsDict.Add(testName, factorAdd);   // read these in so we can potentially run GU cal on only 1 site later, and rewrite existing calfactors to other sites in calfactor file
        //                    }
        //                    else  // store calfactor multiply, even if it's zero
        //                    {
        //                        GuCalFactorsDict.Add(testName, factorMultiply);   // read these in so we can potentially run GU cal on only 1 site later, and rewrite existing calfactors to other sites in calfactor file
        //                    }


        //                    continue;
        //            }  // switch first cell in row
        //        }  // while (!calFile.EndOfStream)
        //    } // using streamreader

        //}

        void SetswitchPath(Dictionary<string, string> DicCalInfo, string LocSettingPath, string swHeader, string SwBand, int Setup_Delay)
        {
            List<System.Threading.Tasks.Task> m_tasks = new List<System.Threading.Tasks.Task>();

            R_Switch = 0;
            if (PreviousSWMode != SwBand.ToUpper())
            {
                R_Switch = 1;   //Status Switch = 1 , else not switching = 0
                if (bMultiSW)
                {
                    string[] str = myUtility.ReadTextFile(DicCalInfo[LocSettingPath], swHeader, SwBand.ToUpper()).Split('@');

                    m_tasks.Add(System.Threading.Tasks.Task.Factory.StartNew(Eq.Site[0]._EqSwitch.SetPath, str[0]));
                    m_tasks.Add(System.Threading.Tasks.Task.Factory.StartNew(Eq.Site[0]._EqSwitchSplit.SetPath, str[1]));
                }
                else
                {
                    Eq.Site[0]._EqSwitch.SetPath(myUtility.ReadTextFile(DicCalInfo[LocSettingPath], swHeader, SwBand.ToUpper()));

                }
                PreviousSWMode = SwBand.ToUpper();
                DelayMs(Setup_Delay);
                m_tasks.ForEach(t => t.Wait());
            }
        }
    }
}