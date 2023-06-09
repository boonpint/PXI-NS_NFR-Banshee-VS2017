﻿namespace ClothoSharedItems.Import.VISA
{
    public enum ViStatus
    {
        VI_ERROR_SYSTEM_ERROR = -1073807360, // -0x40010000
        VI_ERROR_INV_OBJECT = -1073807346, // -0x4000FFF2
        VI_ERROR_RSRC_LOCKED = -1073807345, // -0x4000FFF1
        VI_ERROR_INV_EXPR = -1073807344, // -0x4000FFF0
        VI_ERROR_RSRC_NFOUND = -1073807343, // -0x4000FFEF
        VI_ERROR_INV_RSRC_NAME = -1073807342, // -0x4000FFEE
        VI_ERROR_INV_ACC_MODE = -1073807341, // -0x4000FFED
        VI_ERROR_TMO = -1073807339, // -0x4000FFEB
        VI_ERROR_CLOSING_FAILED = -1073807338, // -0x4000FFEA
        VI_ERROR_INV_DEGREE = -1073807333, // -0x4000FFE5
        VI_ERROR_INV_JOB_ID = -1073807332, // -0x4000FFE4
        VI_ERROR_NSUP_ATTR = -1073807331, // -0x4000FFE3
        VI_ERROR_NSUP_ATTR_STATE = -1073807330, // -0x4000FFE2
        VI_ERROR_ATTR_READONLY = -1073807329, // -0x4000FFE1
        VI_ERROR_INV_LOCK_TYPE = -1073807328, // -0x4000FFE0
        VI_ERROR_INV_ACCESS_KEY = -1073807327, // -0x4000FFDF
        VI_ERROR_INV_EVENT = -1073807322, // -0x4000FFDA
        VI_ERROR_INV_MECH = -1073807321, // -0x4000FFD9
        VI_ERROR_HNDLR_NINSTALLED = -1073807320, // -0x4000FFD8
        VI_ERROR_INV_HNDLR_REF = -1073807319, // -0x4000FFD7
        VI_ERROR_INV_CONTEXT = -1073807318, // -0x4000FFD6
        VI_ERROR_NENABLED = -1073807313, // -0x4000FFD1
        VI_ERROR_ABORT = -1073807312, // -0x4000FFD0
        VI_ERROR_RAW_WR_PROT_VIOL = -1073807308, // -0x4000FFCC
        VI_ERROR_RAW_RD_PROT_VIOL = -1073807307, // -0x4000FFCB
        VI_ERROR_OUTP_PROT_VIOL = -1073807306, // -0x4000FFCA
        VI_ERROR_INP_PROT_VIOL = -1073807305, // -0x4000FFC9
        VI_ERROR_BERR = -1073807304, // -0x4000FFC8
        VI_ERROR_IN_PROGRESS = -1073807303, // -0x4000FFC7
        VI_ERROR_INV_SETUP = -1073807302, // -0x4000FFC6
        VI_ERROR_QUEUE_ERROR = -1073807301, // -0x4000FFC5
        VI_ERROR_ALLOC = -1073807300, // -0x4000FFC4
        VI_ERROR_INV_MASK = -1073807299, // -0x4000FFC3
        VI_ERROR_IO = -1073807298, // -0x4000FFC2
        VI_ERROR_INV_FMT = -1073807297, // -0x4000FFC1
        VI_ERROR_NSUP_FMT = -1073807295, // -0x4000FFBF
        VI_ERROR_LINE_IN_USE = -1073807294, // -0x4000FFBE
        VI_ERROR_NSUP_MODE = -1073807290, // -0x4000FFBA
        VI_ERROR_SRQ_NOCCURRED = -1073807286, // -0x4000FFB6
        VI_ERROR_INV_SPACE = -1073807282, // -0x4000FFB2
        VI_ERROR_INV_OFFSET = -1073807279, // -0x4000FFAF
        VI_ERROR_INV_WIDTH = -1073807278, // -0x4000FFAE
        VI_ERROR_NSUP_OFFSET = -1073807276, // -0x4000FFAC
        VI_ERROR_NSUP_VAR_WIDTH = -1073807275, // -0x4000FFAB
        VI_ERROR_WINDOW_NMAPPED = -1073807273, // -0x4000FFA9
        VI_ERROR_RESP_PENDING = -1073807271, // -0x4000FFA7
        VI_ERROR_NLISTENERS = -1073807265, // -0x4000FFA1
        VI_ERROR_NCIC = -1073807264, // -0x4000FFA0
        VI_ERROR_NSYS_CNTLR = -1073807263, // -0x4000FF9F
        VI_ERROR_NSUP_OPER = -1073807257, // -0x4000FF99
        VI_ERROR_INTR_PENDING = -1073807256, // -0x4000FF98
        VI_ERROR_ASRL_PARITY = -1073807254, // -0x4000FF96
        VI_ERROR_ASRL_FRAMING = -1073807253, // -0x4000FF95
        VI_ERROR_ASRL_OVERRUN = -1073807252, // -0x4000FF94
        VI_ERROR_TRIG_NMAPPED = -1073807250, // -0x4000FF92
        VI_ERROR_NSUP_ALIGN_OFFSET = -1073807248, // -0x4000FF90
        VI_ERROR_USER_BUF = -1073807247, // -0x4000FF8F
        VI_ERROR_RSRC_BUSY = -1073807246, // -0x4000FF8E
        VI_ERROR_NSUP_WIDTH = -1073807242, // -0x4000FF8A
        VI_ERROR_INV_PARAMETER = -1073807240, // -0x4000FF88
        VI_ERROR_INV_PROT = -1073807239, // -0x4000FF87
        VI_ERROR_INV_SIZE = -1073807237, // -0x4000FF85
        VI_ERROR_WINDOW_MAPPED = -1073807232, // -0x4000FF80
        VI_ERROR_NIMPL_OPER = -1073807231, // -0x4000FF7F
        VI_ERROR_INV_LENGTH = -1073807229, // -0x4000FF7D
        VI_ERROR_INV_MODE = -1073807215, // -0x4000FF6F
        VI_ERROR_SESN_NLOCKED = -1073807204, // -0x4000FF64
        VI_ERROR_MEM_NSHARED = -1073807203, // -0x4000FF63
        VI_ERROR_LIBRARY_NFOUND = -1073807202, // -0x4000FF62
        VI_ERROR_NSUP_INTR = -1073807201, // -0x4000FF61
        VI_ERROR_INV_LINE = -1073807200, // -0x4000FF60
        VI_ERROR_FILE_ACCESS = -1073807199, // -0x4000FF5F
        VI_ERROR_FILE_IO = -1073807198, // -0x4000FF5E
        VI_ERROR_NSUP_LINE = -1073807197, // -0x4000FF5D
        VI_ERROR_NSUP_MECH = -1073807196, // -0x4000FF5C
        VI_ERROR_INTF_NUM_NCONFIG = -1073807195, // -0x4000FF5B
        VI_ERROR_CONN_LOST = -1073807194, // -0x4000FF5A
        VI_SUCCESS = 0,
        VI_SUCCESS_EVENT_EN = 1073676290, // 0x3FFF0002
        VI_SUCCESS_EVENT_DIS = 1073676291, // 0x3FFF0003
        VI_SUCCESS_QUEUE_EMPTY = 1073676292, // 0x3FFF0004
        VI_SUCCESS_TERM_CHAR = 1073676293, // 0x3FFF0005
        VI_SUCCESS_MAX_CNT = 1073676294, // 0x3FFF0006
        VI_WARN_QUEUE_OVERFLOW = 1073676300, // 0x3FFF000C
        VI_WARN_CONFIG_NLOADED = 1073676407, // 0x3FFF0077
        VI_SUCCESS_DEV_NPRESENT = 1073676413, // 0x3FFF007D
        VI_SUCCESS_TRIG_MAPPED = 1073676414, // 0x3FFF007E
        VI_SUCCESS_QUEUE_NEMPTY = 1073676416, // 0x3FFF0080
        VI_WARN_NULL_OBJECT = 1073676418, // 0x3FFF0082
        VI_WARN_NSUP_ATTR_STATE = 1073676420, // 0x3FFF0084
        VI_WARN_UNKNOWN_STATUS = 1073676421, // 0x3FFF0085
        VI_WARN_NSUP_BUF = 1073676424, // 0x3FFF0088
        VI_SUCCESS_NCHAIN = 1073676440, // 0x3FFF0098
        VI_SUCCESS_NESTED_SHARED = 1073676441, // 0x3FFF0099
        VI_SUCCESS_NESTED_EXCLUSIVE = 1073676442, // 0x3FFF009A
        VI_SUCCESS_SYNC = 1073676443, // 0x3FFF009B
        VI_WARN_EXT_FUNC_NIMPL = 1073676457, // 0x3FFF00A9
    }
}