using Hamamatsu.DCAM4;
using System;

namespace csAcq4
{
    /// <summary>
    /// DCAM相机的类。主要包括：
    ///     1. 相机的handle的获取（这是后面所有步骤的基础）
    ///     2. 通过相机的handle（IntPtr hisHdcam）读写相机的各项参数和信息
    ///     3. 相机拍照过程的各种方法和属性
    ///     
    /// 注：关于DCAM相机的控制，当用户安装DCAMAPI（下载地址：dcam-api.com）之后，就能在system32文件夹下找到dcamapi.dll文件，其中涉及相机控制相关的函数/程序；
    /// 在dcamapi4.cs文件中，滨松将用C/C++编写的dcamapi.dll导入成了C#可以直接使用的函数/数据。在HisCamera这个类中，我们对其进行了进一步的封装，方便对相机的使用。
    /// </summary>
    public class HisCamera
    {
        //===========================================================
        //******* 1. DCAM相机的相关handle及其获取（构造函数） *******
        //===========================================================
        /// <summary>
        /// 本部分的目的是：
        ///     1.1. 获得相机的handle——HisCamera类所有操作的基础
        ///     1.1.1. 初始化DCAMAPI，初始化的过程中，DCAMAPI会自动寻找电脑上已经连接好的所有DCAM相机，并为其编号（cameraIndex）0，1，2等等    
        ///     1.1.2. 打开指定编号的相机，获得其handle 
        ///     
        ///     1.2. 与此相机相关的等待过程的handle。
        ///     为什么要有“等待过程”——在程序运行中，程序的线程和相机/DCAM的运作是并行的，两者需要在必要的时候进行同步操作；
        ///     否则就有可能出现程序第1行命令让相机拍照，相机刚开始拍，程序就直接到了第2行命令让相机关闭
        ///     如果有一个和相机状态（如相机拍照结束）挂钩的等待过程插在上述第1行和第2行命令之间，就能够让程序等待相机拍完了再运行命令让相机关闭
        ///     在本部分中，我们获得的是一个相机相关的等待过程的handle，具体等待相机的什么状态、等多久就不等了这些参数需要再行设置（参见第3.0部分）
        /// </summary>
        #region
        //相机的handle，所有和相机相关的操作都以此为基础（因为总得有一个相机才能操作）
        public IntPtr hisHdcam;

        //等待过程的handle，相关的内容参考第3.0步的说明
        private static IntPtr hisHwait;

        //构造函数，CameraIndex是相机的编号，一般就用0，也就是第一台相机
        public HisCamera(ushort cameraIndex)
        {
            //DCAMAPI初始化
            DCAMAPI_INIT paramInit = new DCAMAPI_INIT(0);
            hisLastError = dcamapi.init(ref paramInit);

            //打开相机
            DCAMDEV_OPEN paramDevOpen = new DCAMDEV_OPEN(cameraIndex);
            hisLastError = dcamdev.open(ref paramDevOpen);

            //获得相机的handle，赋值给hisHdcam
            hisHdcam = paramDevOpen.hdcam;

            Console.WriteLine($"hisHdcam:{hisHdcam}");
            //获得和相机相关的一个等待过程的handle，赋值给hisHwait
            DCAMWAIT_OPEN paramWaitOpen = new DCAMWAIT_OPEN(0);
            paramWaitOpen.hdcam = hisHdcam;
            hisLastError = dcamwait.open(ref paramWaitOpen);
            hisHwait = paramWaitOpen.hwait;
            Console.WriteLine($"hisLastError:{hisLastError}");
        }

        public void dev_exit()
        {

            StopVideoDisplay();

            // 关闭相机
            if (hisHdcam != null)
            {
                CloseCameraDevice();
            }
            if (!dcam_uninit())
            {
                return;                         // Fail: dcamapi_uninit()
            }

        }

        private void StopVideoDisplay()
        {
            if (hisHdcam != IntPtr.Zero)
            {
                cap_stop();
            }
            video_abort();

            if (hisHdcam != IntPtr.Zero)
            {
                buf_release();
            }

        }
        private void CloseCameraDevice()
        {
            if (hisHdcam == IntPtr.Zero)
            {
                return;                         // internal error
            }


            if (!dev_close())
            {
                return;                         // Fail: dcamdev_close()
            }

            // Success: dcamdev_close()
            hisHwait = IntPtr.Zero;
            hisHdcam = IntPtr.Zero;

        }

        public bool cap_stop()
        {
            if (hisHdcam == IntPtr.Zero)
            {
                hisLastError = DCAMERR.INVALIDHANDLE;
            }
            else
            {
                hisLastError = dcamcap.stop(hisHdcam);
            }

            return !hisLastError.failed();
        }
        public bool video_abort()
        {
            if (hisHwait == IntPtr.Zero)
            {
                hisLastError = DCAMERR.INVALIDWAITHANDLE;
            }
            else
            {
                hisLastError = dcamwait.abort(hisHwait);
            }

            return !hisLastError.failed();
        }
        public bool buf_release()
        {
            if (hisHdcam == IntPtr.Zero)
            {
                hisLastError = DCAMERR.INVALIDHANDLE;
            }
            else
            {
                hisLastError = dcambuf.release(hisHdcam, 0);
            }

            return !hisLastError.failed();
        }

        public static bool dcam_uninit()
        {
            hisLastError = dcamapidll.dcamapi_uninit();

            return !hisLastError.failed();
        }
        public bool dev_close()
        {
            if (hisHdcam == IntPtr.Zero)
                return true;    // already closed

            hisLastError = dcamdev.close(hisHdcam);
            if (!hisLastError.failed())
                hisHdcam = IntPtr.Zero;

            return !hisLastError.failed();
        }
        #endregion
        //===========================================================
        //******** 2. DCAM相机信息/参数的获取和参数的设定 ***********
        //===========================================================

        /// <summary>
        /// 对于不同型号的相机，其可以设置和读取的参数是不一样的。详细可以查找SDK中的dcamsdk4/doc/camera_properties/文件夹，其中有各个型号的参数列表文件
        /// </summary>

        #region

        //获得相机的基础信息
        public string GetModelSN(ref string activeCamInfo)
        {
            string cameraModel = "";
            string cameraSN = "";

            hisLastError = dcamdev.getstring(hisHdcam, DCAMIDSTR.MODEL, ref cameraModel);
            hisLastError = dcamdev.getstring(hisHdcam, DCAMIDSTR.CAMERAID, ref cameraSN);


            if (cameraModel != "")
            {
                activeCamInfo = cameraModel + " " + cameraSN;
            }

            return showError(hisLastError, "GetModelSN()");
        }

        /// <summary>
        /// 设置相机的参数
        /// </summary>
        /// <param name="camProperty"></param>
        /// <param name="specData"></param>
        /// <returns></returns>
        public string SetDCAMIDPROP(DCAMIDPROP camProperty, double specData)
        {
            if (hisHdcam == IntPtr.Zero)
            {
                hisLastError = DCAMERR.INVALIDHANDLE;
            }
            else
            {
                hisLastError = dcamprop.setgetvalue(hisHdcam, camProperty, ref specData, DCAMPROPOPTION.NONE);
            }

            return showError(hisLastError, "Set " + camProperty.ToString());
        }

        /// <summary>
        /// 用于获得相机的参数数值
        /// </summary>
        /// <param name="camProperty"></param>
        /// <returns></returns>
        public string GetDCAMIDPROP(DCAMIDPROP camProperty, ref double specData)
        {
            if (hisHdcam == IntPtr.Zero)
            {
                hisLastError = DCAMERR.INVALIDHANDLE;
            }
            else
            {
                hisLastError = dcamprop.getvalue(hisHdcam, camProperty, ref specData);
            }
            return showError(hisLastError, "Get " + camProperty.ToString());
        }

        /// <summary>
        /// 设置Property的参数
        /// </summary>
        /// <param name="camProperty"></param>
        /// <returns></returns>
        public string GetIDPROPAttribute(ref DCAMPROP_ATTR param)
        {
            if (hisHdcam == IntPtr.Zero)
            {
                hisLastError = DCAMERR.INVALIDHANDLE;
            }
            else
            {
                hisLastError = dcamprop.getattr(hisHdcam, ref param);
            }
            return showError(hisLastError, "Get Attribute of Properties.");
        }

        #endregion
        //===========================================================
        //******** 3. DCAM相机拍照的整个流程相关属性和方法 **********
        //===========================================================
        #region
        //******* 3.0. 与DCAM相机工作节点相关的程序等待设置 *********

        //int TimeOut给出等多长时间要是还没等到预定事件，就不等了;TimeOut的单位是ms;CapEvent如DCAMWAIT.CAPEVENT.FRAMEREADY
        public string DcamWait(int timeOut, int capEvent)
        {

            // 检查 hisHwait 是否已初始化
            if (hisHwait == IntPtr.Zero)
            {
                return "Error: hisHwait is not initialized. Cannot wait for camera capture event.";
            }

            DCAMWAIT_START paramWaitStart = new DCAMWAIT_START(capEvent);
            paramWaitStart.timeout = timeOut;
            hisLastError = dcamwait.start(hisHwait, ref paramWaitStart);   //由于相机拍照是需要时间的，为了防止相机还在拍照，程序就继续下面的命令行，此处就需要根据之前设定的参数进行等待

            return showError(hisLastError, "DcamWait()");
        }

        //** 3.1. 让DCAM开辟一块内存，用于临时储存相机采集到的数据 **


        //给相机分配内存，用户可以规定要留出多少张图片所对应的内存
        public string AllocBuffer(int picQuantity = 1)
        {
            hisLastError = dcambuf.alloc(hisHdcam, picQuantity);
            return showError(hisLastError, "AllocBuffer()");
        }

        //******** 3.2. 拍照模式的设定和开始拍照命令的下达 *********

        //开始拍照，只能是DCAMCAP_START.SNAP或者DCAMCAP_START.SEQUENCE
        public string StartCapture(DCAMCAP_START mode)
        {
            hisLastError = dcamcap.start(hisHdcam, mode);   //如DCAMCAP_START.SNAP
            return showError(hisLastError, "StartCapture()");
        }

        //******* 3.3. 从3.1步开辟的内存中取出原始图像数据 *********

        //从DCAMAPI开辟的内存中将数据取出来，放到应用程序开辟的内存中，并且将相关参数组合成一个HisPic的实例
        public string GetPicFromBuf(ref HisPic lockedFrame)
        {
            //这里512没什么意义，就是要有个赋值才能在下面用ref通过dcamprop.getvalue取出真实的长宽
            double camWidth = 512;
            double camHeight = 512;

            DCAMBUF_FRAME paramBufFrame = new DCAMBUF_FRAME(-1);
            hisLastError = dcambuf.lockframe(hisHdcam, ref paramBufFrame);

            dcamprop.getvalue(hisHdcam, DCAMIDPROP.IMAGE_WIDTH, ref camWidth);
            dcamprop.getvalue(hisHdcam, DCAMIDPROP.IMAGE_HEIGHT, ref camHeight);

            lockedFrame = new HisPic(paramBufFrame.buf, (int)camWidth, (int)camHeight, 2);

            return showError(hisLastError, "GetPicFromBuf()");
        }

        //****************** 3.4. 停止拍照（如需） *****************

        //在public string StartCapture(DCAMCAP_START mode)中，如果采用的mode是DCAMCAP_START.SNAP，那么相机把内存填满之后（内存能储存图片的数量取决于public string AllocBuffer(int picQuantity=1)中的picQuantity），相机就自动停止拍照了，无需StopSequenceCapture()这个函数；
        //如果采用的mode是DCAMCAP_START.SEQUENCE，那么就需要StopSequenceCapture()这个函数来停止拍照，否则相机新采集的数据会滚动覆盖内存中之前的数据

        public string StopSequenceCapture()
        {
            hisLastError = dcamcap.stop(hisHdcam);
            return showError(hisLastError, "StopSequenceCapture()");
        }

        //************* 3.5. 释放3.1步为DCAM开辟的内存 *************

        //给相机分配内存，用户可以规定要留出多少张图片所对应的内存
        public string ReleaseBuffer(int picQuantity = 1)
        {
            hisLastError = dcambuf.release(hisHdcam, 0);
            return showError(hisLastError, "AllocBuffer()");
        }
        #endregion

        //===========================================================
        //****************** 4.相机模式的设定 **********************
        //===========================================================
        #region
        //触发模式的设定（以C12300-321为例）

        public string SetTriggerMode(string triggerMode)
        {
            switch (triggerMode)
            {
                case "InternalTDIMode":
                    hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.SENSORMODE, DCAMPROP.SENSORMODE.TDI);
                    hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.TRIGGERSOURCE, DCAMPROP.TRIGGERSOURCE.INTERNAL);
                    hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.TRIGGERENABLE_ACTIVE, DCAMPROP.TRIGGERENABLE_ACTIVE.ALWAYS);
                    break;
                case "ExternalTriggerMode":
                    hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.SENSORMODE, DCAMPROP.SENSORMODE.TDI);
                    hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.TRIGGERSOURCE, DCAMPROP.TRIGGERSOURCE.EXTERNAL);
                    hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.TRIGGERENABLE_ACTIVE, DCAMPROP.TRIGGERENABLE_ACTIVE.ALWAYS);
                    break;
                case "TriggerEnableMode":
                    hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.SENSORMODE, DCAMPROP.SENSORMODE.TDI);
                    hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.TRIGGERSOURCE, DCAMPROP.TRIGGERSOURCE.INTERNAL);
                    hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.TRIGGERENABLE_ACTIVE, DCAMPROP.TRIGGERENABLE_ACTIVE.START);
                    break;
                case "InternalAreaMode":
                    hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.SENSORMODE, DCAMPROP.SENSORMODE.AREA);
                    hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.TRIGGERSOURCE, DCAMPROP.TRIGGERSOURCE.INTERNAL);
                    hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.TRIGGERENABLE_ACTIVE, DCAMPROP.TRIGGERENABLE_ACTIVE.ALWAYS);
                    break;
            }

            return showError(hisLastError, "SetTriggerMode()");



        }



        public string setBitDepth(int bitdepth)
        {

            hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.BITSPERCHANNEL, bitdepth);
            return showError(hisLastError, "setBitDepth()");

        }

        public string getBitDepth(ref double bitdepth)
        {

            hisLastError = dcamprop.getvalue(hisHdcam, DCAMIDPROP.BITSPERCHANNEL, ref bitdepth);
            return showError(hisLastError, "getBitDepth()");

        }


        public string getPixelType(ref double pixeltype)
        {

            hisLastError = dcamprop.getvalue(hisHdcam, DCAMIDPROP.IMAGE_PIXELTYPE, ref pixeltype);
            return showError(hisLastError, "getpixeltype()");

        }
        public string getFrameRate(ref double framerate)
        {

            hisLastError = dcamprop.getvalue(hisHdcam, DCAMIDPROP.INTERNALFRAMERATE, ref framerate);
            return showError(hisLastError, "getFrameRate()");

        }
        #endregion

        public string setPixelType(int pixelType)
        {

            hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.IMAGE_PIXELTYPE, pixelType);
            return showError(hisLastError, "setBitDepth()");

        }


        //===========================================================
        //****************** 5.相机的自带校正 **********************
        //===========================================================
        #region
        public string SetToNormalCaptureMode()
        {
            hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.CAPTUREMODE, DCAMPROP.CAPTUREMODE.NORMAL);

            return showError(hisLastError, "SetToNormalCapturnMode");
        }

        //获得暗场校正（dark calibration）的图片

        public string GetDarkCalibrationImage(int savePosition)
        {
            hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.CAPTUREMODE, DCAMPROP.CAPTUREMODE.DARKCALIB);

            this.AllocBuffer(1);   //分配能存图片的内存

            this.StartCapture(DCAMCAP_START.SNAP);  //开始拍照

            this.DcamWait(10000, DCAMWAIT.CAPEVENT.STOPPED);

            hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.STORESUBTRACTIMAGETOMEMORY, savePosition);

            this.ReleaseBuffer();

            return showError(hisLastError, "GetDarkCalibrationImage");
        }

        public string UseDarkCalibration(int savePosition)
        {
            hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.SUBTRACTIMAGEMEMORY, savePosition);
            hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.SUBTRACT, DCAMPROP.MODE.ON);


            return showError(hisLastError, "UseDarkCalibration");
        }

        public string DoNotUseDarkCalibration()
        {
            hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.SUBTRACT, DCAMPROP.MODE.OFF);
            return showError(hisLastError, "RemoveCalibration");
        }

        //Shading Correction

        public string GetShadingCalibrationImage(int savePosition)
        {
            //在采集shading calibration的图片时，必须要使用暗校正
            hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.SUBTRACT, DCAMPROP.MODE.ON);
            hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.SUBTRACTIMAGEMEMORY, 1);

            //采集shading calibration的图片，要求将相机的采集模式（DCAM_IDPROP_CAPTUREMODE)设置成DCAMPROP_CAPTUREMODE__SHADINGCALIB）
            hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.CAPTUREMODE, DCAMPROP.CAPTUREMODE.SHADINGCALIB);

            //shading calibration可以设置校正之后的灰度值，比如校正前像素在400-600之间浮动，中间亮两边暗；校正设置为16383（或1023，2047等等）之后，校正后像素灰度值就基本会在16383（或1023，2047等等）这个水平
            //但在一些测试中，如果实际亮度过低（如200-300），而DCAM_IDPROP_SHADINGCALIB_TARGET设置得过高（如16383），就会出现shading calibration校正失败的情况。原因待查。
            //在默认模式下，DCAM_IDPROP_SHADINGCALIB_METHOD是DCAMPROP_SHADINGCALIB_METHOD__MAXIMUM，如果有特别需求，再用这两句
            //hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.SHADINGCALIB_METHOD, DCAMPROP.SHADINGCALIB_METHOD.USETARGET);
            //hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.SHADINGCALIB_TARGET, 4095); //C12300-321是12bit的相机，所以这里最大值就是默认的4095

            //实际采集一张图
            this.AllocBuffer(1);   //分配能存图片的内存

            this.StartCapture(DCAMCAP_START.SNAP);  //开始拍照

            this.DcamWait(10000, DCAMWAIT.CAPEVENT.STOPPED);

            //将这张图储存到相机控制的一个内存位置中。注意，这个内存位置一般有十几个（查看DCAM_IDPROP_STORESHADINGCALIBDATATOMEMORY可以设定的数值即可知道）；每一个内存位置中可以同时存储暗校正、亮校正及其他校正的数据和图片。换句话说，下面的savePosition可以和暗校正的一样
            hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.STORESHADINGCALIBDATATOMEMORY, savePosition);

            this.ReleaseBuffer();

            return showError(hisLastError, "GetShadingCalibrationImage");
        }

        public string UseShadingCalibration(int savePosition)
        {


            hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.SHADINGCALIBDATAMEMORY, savePosition);
            hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.SHADINGCORRECTION, DCAMPROP.MODE.ON);

            return showError(hisLastError, "UseShadingCalibration");
        }

        public string DoNotUseShadingCalibration()
        {
            hisLastError = dcamprop.setvalue(hisHdcam, DCAMIDPROP.SHADINGCORRECTION, DCAMPROP.MODE.OFF);
            return showError(hisLastError, "RemoveCalibration");
        }
        #endregion
        //===========================================================
        //****************** -1.其他对象和方法 **********************
        //===========================================================
        #region
        //在dcamapi4.cs中，几乎所有相机的操作都会返回一个DCAMERR的值，不成功的话这个DCAMERR的值可以帮助了解出了什么问题;同样在dcamapi4.cs中，我们可以找到DCAMERR结构体的定义，便于将程序返回的数字和具体的error描述对应起来
        private static DCAMERR hisLastError;

        //每个针对相机的操作（即HisCamera里面的方法），都返回一个字符串，包含时间和结果（如ErrorCode），用于在Form1底部的TextBox中显示出来
        private string showError(DCAMERR hisLastError, string errorPosition)
        {
            string ErrorInfo = "";

            ErrorInfo = "时间：" + DateTime.Now.ToString() + ":" + DateTime.Now.Millisecond.ToString() + "；位置：" + errorPosition + "；信息： " + hisLastError.text() + "；编号： " + (Int16)hisLastError;

            return ErrorInfo;
        }
        #endregion
    }

    /// <summary>
    ///  helper function for DCAM properties
    /// </summary>
    public class MyDcamProp
    {
        public DCAMERR m_lasterr;
        public IntPtr m_hdcam;
        public DCAMIDPROP m_idProp;
        public DCAMPROP_ATTR m_attr;

        public MyDcamProp(HisCamera mydcam, DCAMIDPROP _iprop)
        {
            m_hdcam = mydcam.hisHdcam;
            m_idProp = _iprop;
            m_attr = new DCAMPROP_ATTR(m_idProp);
        }
        public MyDcamProp(IntPtr hdcam, DCAMIDPROP _iprop)
        {
            m_hdcam = hdcam;
            m_idProp = _iprop;
            m_attr = new DCAMPROP_ATTR(m_idProp);
        }
        public MyDcamProp Clone()
        {
            MyDcamProp ret = new MyDcamProp(m_hdcam, m_idProp);
            ret.m_attr = m_attr;
            return ret;
        }
        public bool update_attr()
        {
            m_attr.iProp = m_idProp;
            m_lasterr = dcamprop.getattr(m_hdcam, ref m_attr);
            return !m_lasterr.failed();
        }

        public bool getvalue(ref double value)
        {
            m_lasterr = dcamprop.getvalue(m_hdcam, m_idProp, ref value);
            return !m_lasterr.failed();
        }
        public bool setvalue(double value)
        {
            m_lasterr = dcamprop.setvalue(m_hdcam, m_idProp, value);
            return !m_lasterr.failed();
        }
        public bool setgetvalue(ref double value)
        {
            DCAMPROPOPTION _option = DCAMPROPOPTION.NONE;
            m_lasterr = dcamprop.setgetvalue(m_hdcam, m_idProp, ref value, _option);
            return !m_lasterr.failed();
        }
        public bool queryvalue(ref double value, DCAMPROPOPTION _option)
        {
            m_lasterr = dcamprop.queryvalue(m_hdcam, m_idProp, ref value, _option);
            return !m_lasterr.failed();
        }
        public bool queryvalue_next(ref double value)
        {
            m_lasterr = dcamprop.queryvalue(m_hdcam, m_idProp, ref value, DCAMPROPOPTION.NEXT);
            return !m_lasterr.failed();
        }
        public bool nextid()
        {
            m_lasterr = dcamprop.getnextid(m_hdcam, ref m_idProp, 0);
            return !m_lasterr.failed();
        }
        public string getname()
        {
            string name = "";
            m_lasterr = dcamprop.getname(m_hdcam, m_idProp, ref name);
            if (m_lasterr.failed())
            {
                name = "";
            }
            return name;
        }

        public string getvaluetext(double value)
        {
            string ret;
            ret = "";

            m_lasterr = dcamprop.getvaluetext(m_hdcam, m_idProp, value, ref ret);
            if (m_lasterr.failed())
            {
                ret = value.ToString();
            }
            return ret;
        }

        public bool is_attrtype_mode()
        {
            DCAMPROPATTRIBUTE attr = new DCAMPROPATTRIBUTE(m_attr.attribute);
            if (attr.is_type(DCAMPROPATTRIBUTE.TYPE_MODE))
                return true;

            return false;
        }
        public bool is_attr_readonly()
        {
            DCAMPROPATTRIBUTE attr = new DCAMPROPATTRIBUTE(m_attr.attribute);
            if (attr.has_attr(DCAMPROPATTRIBUTE.READABLE)
             && !attr.has_attr(DCAMPROPATTRIBUTE.WRITABLE))
                return true;
            return false;
        }
    }
}
