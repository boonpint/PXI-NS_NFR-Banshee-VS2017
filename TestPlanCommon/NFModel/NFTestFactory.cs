using Avago.ATF.StandardLibrary;
using ClothoLibAlgo;
using ClothoSharedItems;
using LibEqmtDriver;
using MPAD_TestTimer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
//using TestLib;
using TestPlanCommon.CommonModel;
//using static TestPlanCommon.CommonModel.MultiSiteTestRunner;


namespace TestPlanCommon.NFModel
{
    public class NFTestFactory
    {
        private NFTestConditionFactory testConFactory;
        private byte site;
        private List<string> AIDIPR;
        private ValidationDataObject m_validationDo;
        private string ClothoRootDir = "";
        public List<string> Pinsweep_Wvfm = new List<string>();
        //private CommonEquipmentInitializer EquipmentInitializer;
        public eTestType testType = eTestType.PATEST;
        public Dictionary<string, string> iTestRevIDs { get; private set; }

        public ValidationDataObject ValDataObject
        {
            get { return m_validationDo; }
        }

        public NFTestFactory(List<string> _AIDPR)
        {
            m_validationDo = new ValidationDataObject();
            ClothoRootDir = GetTestPlanPath();
            AIDIPR = _AIDPR;
            iTestRevIDs = new Dictionary<string, string>();
            ClothoDataObject.Instance.RunOptionLocked = true;
        }

        public NFTestFactory(byte site, NFTestConditionFactory paramFactory)
        {
            this.site = site;
            testConFactory = paramFactory;
            m_validationDo = new ValidationDataObject();
            ClothoRootDir = GetTestPlanPath();
        }

        private string GetTestPlanPath()
        {
            string basePath = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_FULLPATH, "");

            if (basePath == "")   // Lite Driver mode
            {
                string tcfPath = ATFCrossDomainWrapper.GetStringFromCache(PublishTags.PUBTAG_PACKAGE_TCF_FULLPATH, "");

                int pos1 = tcfPath.IndexOf("TestPlans") + "TestPlans".Length + 1;
                int pos2 = tcfPath.IndexOf('\\', pos1);

                basePath = tcfPath.Remove(pos2);
            }

            return basePath + "\\";
        }

    }
}
