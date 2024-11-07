using System.Drawing;
using System.Windows.Forms;

namespace csAcq4
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        
        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.stopPlayingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deviceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.searchCamToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.calToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.measureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backgroundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loadIToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.currentImageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeBackgroundToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeAutofluorescenceToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageProcessingcToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageProcessingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fillColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fillColorToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.lengthCalToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.plotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.textToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rGBColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.histogramToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.loggingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.textToolBtn = new System.Windows.Forms.ToolStripButton();
            this.toolPointBtn = new System.Windows.Forms.ToolStripButton();
            this.lineDrawTool = new System.Windows.Forms.ToolStripButton();
            this.toolCircleBtn = new System.Windows.Forms.ToolStripButton();
            this.toolRectBtn = new System.Windows.Forms.ToolStripButton();
            this.singleArrowTool = new System.Windows.Forms.ToolStripButton();
            this.doubleArrowTool = new System.Windows.Forms.ToolStripButton();
            this.positionTool = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton10 = new System.Windows.Forms.ToolStripButton();
            this.toolCalBtn = new System.Windows.Forms.ToolStripButton();
            this.colorToolSelec = new System.Windows.Forms.ToolStripButton();
            this.applyAllTool = new System.Windows.Forms.ToolStripButton();
            this.cancelDrawTool = new System.Windows.Forms.ToolStripButton();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.pathLabel = new Sunny.UI.UILabel();
            this.uiLine1 = new Sunny.UI.UILine();
            this.savePathChoose = new Sunny.UI.UISymbolButton();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.preName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.saveGroup = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.delayVal = new System.Windows.Forms.NumericUpDown();
            this.shootStart = new Sunny.UI.UIButton();
            this.numVal = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.periodVal = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.saveImg = new Sunny.UI.UIButton();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.videoRecorder = new Sunny.UI.UIButton();
            this.funGroup = new System.Windows.Forms.GroupBox();
            this.enhanceBtn = new Sunny.UI.UIButton();
            this.uiLabel3 = new Sunny.UI.UILabel();
            this.exposureNum = new System.Windows.Forms.NumericUpDown();
            this.uiLabel2 = new Sunny.UI.UILabel();
            this.pushLive = new Sunny.UI.UIButton();
            this.pixelCombox = new System.Windows.Forms.ComboBox();
            this.uiLabel1 = new Sunny.UI.UILabel();
            this.devName = new System.Windows.Forms.ComboBox();
            this.leftPanel = new System.Windows.Forms.Panel();
            this.uiTitlePanel1 = new Sunny.UI.UITitlePanel();
            this.smallShow9 = new System.Windows.Forms.PictureBox();
            this.smallShow8 = new System.Windows.Forms.PictureBox();
            this.smallShow7 = new System.Windows.Forms.PictureBox();
            this.smallShow6 = new System.Windows.Forms.PictureBox();
            this.smallShow5 = new System.Windows.Forms.PictureBox();
            this.smallShow4 = new System.Windows.Forms.PictureBox();
            this.smallShow3 = new System.Windows.Forms.PictureBox();
            this.smallShow2 = new System.Windows.Forms.PictureBox();
            this.smallShow1 = new System.Windows.Forms.PictureBox();
            this.histogramPanel = new Sunny.UI.UITitlePanel();
            this.histSelect = new System.Windows.Forms.ComboBox();
            this.histLine = new Sunny.UI.UIBarChart();
            this.rightPanel = new System.Windows.Forms.Panel();
            this.middlePanel = new System.Windows.Forms.Panel();
            this.LiveBar = new Sunny.UI.UIScrollBar();
            this.RestoreBtn = new Sunny.UI.UISymbolButton();
            this.recoveryBtn = new Sunny.UI.UISymbolButton();
            this.narrowBtn = new Sunny.UI.UISymbolButton();
            this.enlargeBtn = new Sunny.UI.UISymbolButton();
            this.PicDisplay9 = new System.Windows.Forms.PictureBox();
            this.PicDisplay8 = new System.Windows.Forms.PictureBox();
            this.PicDisplay7 = new System.Windows.Forms.PictureBox();
            this.PicDisplay6 = new System.Windows.Forms.PictureBox();
            this.PicDisplay5 = new System.Windows.Forms.PictureBox();
            this.PicDisplay4 = new System.Windows.Forms.PictureBox();
            this.PicDisplay3 = new System.Windows.Forms.PictureBox();
            this.PicDisplay2 = new System.Windows.Forms.PictureBox();
            this.PicDisplay1 = new System.Windows.Forms.PictureBox();
            this.FullDisPlay = new System.Windows.Forms.PictureBox();
            this.menuStrip2 = new System.Windows.Forms.MenuStrip();
            this.najingNanoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.fullScreanMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.smallMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.bottomLabelLeft = new System.Windows.Forms.ToolStripStatusLabel();
            this.bottomLabelMiddle = new System.Windows.Forms.ToolStripStatusLabel();
            this.bottomLabelRight = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.saveGroup.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.delayVal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numVal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.periodVal)).BeginInit();
            this.groupBox4.SuspendLayout();
            this.funGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.exposureNum)).BeginInit();
            this.leftPanel.SuspendLayout();
            this.uiTitlePanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.smallShow9)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.smallShow8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.smallShow7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.smallShow6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.smallShow5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.smallShow4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.smallShow3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.smallShow2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.smallShow1)).BeginInit();
            this.histogramPanel.SuspendLayout();
            this.rightPanel.SuspendLayout();
            this.middlePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay9)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.FullDisPlay)).BeginInit();
            this.menuStrip2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.deviceToolStripMenuItem,
            this.measureToolStripMenuItem,
            this.toolsToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 38);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1920, 28);
            this.menuStrip1.TabIndex = 14;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form_MouseDown);
            this.menuStrip1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form_MouseMove);
            this.menuStrip1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form_MouseUp);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.stopPlayingToolStripMenuItem});
            this.fileToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.fileToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.fileToolStripMenuItem.ImageTransparentColor = System.Drawing.Color.IndianRed;
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(46, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.openToolStripMenuItem.Size = new System.Drawing.Size(163, 28);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(163, 28);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(163, 28);
            this.saveAsToolStripMenuItem.Text = "Save as";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // stopPlayingToolStripMenuItem
            // 
            this.stopPlayingToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.stopPlayingToolStripMenuItem.Name = "stopPlayingToolStripMenuItem";
            this.stopPlayingToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.stopPlayingToolStripMenuItem.Size = new System.Drawing.Size(163, 28);
            this.stopPlayingToolStripMenuItem.Text = "Stop Playing";
            this.stopPlayingToolStripMenuItem.Click += new System.EventHandler(this.stopPlayingToolStripMenuItem_Click);
            // 
            // deviceToolStripMenuItem
            // 
            this.deviceToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.searchCamToolStripMenuItem,
            this.calToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.deviceToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.deviceToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.deviceToolStripMenuItem.Name = "deviceToolStripMenuItem";
            this.deviceToolStripMenuItem.Size = new System.Drawing.Size(70, 24);
            this.deviceToolStripMenuItem.Text = "Device";
            // 
            // searchCamToolStripMenuItem
            // 
            this.searchCamToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.searchCamToolStripMenuItem.Name = "searchCamToolStripMenuItem";
            this.searchCamToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.searchCamToolStripMenuItem.Size = new System.Drawing.Size(157, 28);
            this.searchCamToolStripMenuItem.Text = "Search Cam";
            this.searchCamToolStripMenuItem.Click += new System.EventHandler(this.searchCamToolStripMenuItem_Click);
            // 
            // calToolStripMenuItem
            // 
            this.calToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aToolStripMenuItem,
            this.bToolStripMenuItem,
            this.cToolStripMenuItem});
            this.calToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.calToolStripMenuItem.Name = "calToolStripMenuItem";
            this.calToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.calToolStripMenuItem.Size = new System.Drawing.Size(157, 28);
            this.calToolStripMenuItem.Text = "Cal";
            // 
            // aToolStripMenuItem
            // 
            this.aToolStripMenuItem.Name = "aToolStripMenuItem";
            this.aToolStripMenuItem.Size = new System.Drawing.Size(87, 24);
            this.aToolStripMenuItem.Text = "a";
            // 
            // bToolStripMenuItem
            // 
            this.bToolStripMenuItem.Name = "bToolStripMenuItem";
            this.bToolStripMenuItem.Size = new System.Drawing.Size(87, 24);
            this.bToolStripMenuItem.Text = "b";
            // 
            // cToolStripMenuItem
            // 
            this.cToolStripMenuItem.Name = "cToolStripMenuItem";
            this.cToolStripMenuItem.Size = new System.Drawing.Size(87, 24);
            this.cToolStripMenuItem.Text = "c";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(157, 28);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // measureToolStripMenuItem
            // 
            this.measureToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.backgroundToolStripMenuItem,
            this.removeBackgroundToolStripMenuItem,
            this.removeAutofluorescenceToolStripMenuItem,
            this.imageProcessingcToolStripMenuItem,
            this.imageProcessingToolStripMenuItem,
            this.fillColorToolStripMenuItem});
            this.measureToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.measureToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.measureToolStripMenuItem.Name = "measureToolStripMenuItem";
            this.measureToolStripMenuItem.Size = new System.Drawing.Size(84, 24);
            this.measureToolStripMenuItem.Text = "Measure";
            // 
            // backgroundToolStripMenuItem
            // 
            this.backgroundToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadIToolStripMenuItem,
            this.currentImageToolStripMenuItem});
            this.backgroundToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.backgroundToolStripMenuItem.Name = "backgroundToolStripMenuItem";
            this.backgroundToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.backgroundToolStripMenuItem.Size = new System.Drawing.Size(254, 28);
            this.backgroundToolStripMenuItem.Text = "Background";
            this.backgroundToolStripMenuItem.Click += new System.EventHandler(this.backgroundToolStripMenuItem_Click);
            // 
            // loadIToolStripMenuItem
            // 
            this.loadIToolStripMenuItem.Name = "loadIToolStripMenuItem";
            this.loadIToolStripMenuItem.Size = new System.Drawing.Size(174, 24);
            this.loadIToolStripMenuItem.Text = "Load Image";
            this.loadIToolStripMenuItem.Click += new System.EventHandler(this.loadIToolStripMenuItem_Click);
            // 
            // currentImageToolStripMenuItem
            // 
            this.currentImageToolStripMenuItem.Name = "currentImageToolStripMenuItem";
            this.currentImageToolStripMenuItem.Size = new System.Drawing.Size(174, 24);
            this.currentImageToolStripMenuItem.Text = "Current Image";
            this.currentImageToolStripMenuItem.Click += new System.EventHandler(this.currentImageToolStripMenuItem_Click);
            // 
            // removeBackgroundToolStripMenuItem
            // 
            this.removeBackgroundToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.removeBackgroundToolStripMenuItem.Name = "removeBackgroundToolStripMenuItem";
            this.removeBackgroundToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.removeBackgroundToolStripMenuItem.Size = new System.Drawing.Size(254, 28);
            this.removeBackgroundToolStripMenuItem.Text = "Remove Background";
            this.removeBackgroundToolStripMenuItem.Click += new System.EventHandler(this.removeBackgroundToolStripMenuItem_Click);
            // 
            // removeAutofluorescenceToolStripMenuItem
            // 
            this.removeAutofluorescenceToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.removeAutofluorescenceToolStripMenuItem.Name = "removeAutofluorescenceToolStripMenuItem";
            this.removeAutofluorescenceToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.removeAutofluorescenceToolStripMenuItem.Size = new System.Drawing.Size(254, 28);
            this.removeAutofluorescenceToolStripMenuItem.Text = "Remove Autofluorescence";
            this.removeAutofluorescenceToolStripMenuItem.Click += new System.EventHandler(this.removeAutofluorescenceToolStripMenuItem_Click);
            // 
            // imageProcessingcToolStripMenuItem
            // 
            this.imageProcessingcToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.imageProcessingcToolStripMenuItem.Name = "imageProcessingcToolStripMenuItem";
            this.imageProcessingcToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.imageProcessingcToolStripMenuItem.Size = new System.Drawing.Size(254, 28);
            this.imageProcessingcToolStripMenuItem.Text = "Image Processingc";
            this.imageProcessingcToolStripMenuItem.Click += new System.EventHandler(this.imageProcessingcToolStripMenuItem_Click);
            // 
            // imageProcessingToolStripMenuItem
            // 
            this.imageProcessingToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.imageProcessingToolStripMenuItem.Name = "imageProcessingToolStripMenuItem";
            this.imageProcessingToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.imageProcessingToolStripMenuItem.Size = new System.Drawing.Size(254, 28);
            this.imageProcessingToolStripMenuItem.Text = "ROI";
            // 
            // fillColorToolStripMenuItem
            // 
            this.fillColorToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.fillColorToolStripMenuItem.Name = "fillColorToolStripMenuItem";
            this.fillColorToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.fillColorToolStripMenuItem.Size = new System.Drawing.Size(254, 28);
            this.fillColorToolStripMenuItem.Text = "Fill Color";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fillColorToolStripMenuItem1,
            this.lengthCalToolStripMenuItem,
            this.plotToolStripMenuItem,
            this.textToolStripMenuItem,
            this.rGBColorToolStripMenuItem});
            this.toolsToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.toolsToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(61, 24);
            this.toolsToolStripMenuItem.Text = "Tools";
            // 
            // fillColorToolStripMenuItem1
            // 
            this.fillColorToolStripMenuItem1.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.fillColorToolStripMenuItem1.Name = "fillColorToolStripMenuItem1";
            this.fillColorToolStripMenuItem1.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.fillColorToolStripMenuItem1.Size = new System.Drawing.Size(150, 28);
            this.fillColorToolStripMenuItem1.Text = "Fill Color";
            this.fillColorToolStripMenuItem1.Click += new System.EventHandler(this.fillColorToolStripMenuItem1_Click);
            // 
            // lengthCalToolStripMenuItem
            // 
            this.lengthCalToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.lengthCalToolStripMenuItem.Name = "lengthCalToolStripMenuItem";
            this.lengthCalToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.lengthCalToolStripMenuItem.Size = new System.Drawing.Size(150, 28);
            this.lengthCalToolStripMenuItem.Text = "Length Cal";
            this.lengthCalToolStripMenuItem.Click += new System.EventHandler(this.toolCalBtn_Click);
            // 
            // plotToolStripMenuItem
            // 
            this.plotToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.plotToolStripMenuItem.Name = "plotToolStripMenuItem";
            this.plotToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.plotToolStripMenuItem.Size = new System.Drawing.Size(150, 28);
            this.plotToolStripMenuItem.Text = "Plot";
            this.plotToolStripMenuItem.Click += new System.EventHandler(this.toolPointBtn_Click);
            // 
            // textToolStripMenuItem
            // 
            this.textToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.textToolStripMenuItem.Name = "textToolStripMenuItem";
            this.textToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.textToolStripMenuItem.Size = new System.Drawing.Size(150, 28);
            this.textToolStripMenuItem.Text = "Text";
            this.textToolStripMenuItem.Click += new System.EventHandler(this.textToolBtn_Click);
            // 
            // rGBColorToolStripMenuItem
            // 
            this.rGBColorToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.rGBColorToolStripMenuItem.Name = "rGBColorToolStripMenuItem";
            this.rGBColorToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.rGBColorToolStripMenuItem.Size = new System.Drawing.Size(150, 28);
            this.rGBColorToolStripMenuItem.Text = "RGB Color";
            this.rGBColorToolStripMenuItem.Click += new System.EventHandler(this.colorToolSelec_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.histogramToolStripMenuItem});
            this.viewToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.viewToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(56, 24);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // histogramToolStripMenuItem
            // 
            this.histogramToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.histogramToolStripMenuItem.Name = "histogramToolStripMenuItem";
            this.histogramToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.histogramToolStripMenuItem.Size = new System.Drawing.Size(148, 28);
            this.histogramToolStripMenuItem.Text = "Histogram";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loggingToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 11F);
            this.helpToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(56, 24);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // loggingToolStripMenuItem
            // 
            this.loggingToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.loggingToolStripMenuItem.Name = "loggingToolStripMenuItem";
            this.loggingToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.loggingToolStripMenuItem.Size = new System.Drawing.Size(134, 28);
            this.loggingToolStripMenuItem.Text = "Logging";
            this.loggingToolStripMenuItem.Click += new System.EventHandler(this.loggingToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Font = new System.Drawing.Font("Microsoft YaHei UI", 10F);
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(134, 28);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(71)))), ((int)(((byte)(71)))));
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.textToolBtn,
            this.toolPointBtn,
            this.lineDrawTool,
            this.toolCircleBtn,
            this.toolRectBtn,
            this.singleArrowTool,
            this.doubleArrowTool,
            this.positionTool,
            this.toolStripButton10,
            this.toolCalBtn,
            this.colorToolSelec,
            this.applyAllTool,
            this.cancelDrawTool});
            this.toolStrip1.Location = new System.Drawing.Point(0, 66);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0);
            this.toolStrip1.Size = new System.Drawing.Size(1920, 35);
            this.toolStrip1.TabIndex = 15;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // textToolBtn
            // 
            this.textToolBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.textToolBtn.Image = global::csAcq4.Properties.Resources.text;
            this.textToolBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.textToolBtn.Margin = new System.Windows.Forms.Padding(5, 1, 5, 2);
            this.textToolBtn.Name = "textToolBtn";
            this.textToolBtn.Padding = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.textToolBtn.Size = new System.Drawing.Size(44, 32);
            this.textToolBtn.Text = "Text Input";
            this.textToolBtn.Click += new System.EventHandler(this.textToolBtn_Click);
            // 
            // toolPointBtn
            // 
            this.toolPointBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolPointBtn.Image = global::csAcq4.Properties.Resources.point;
            this.toolPointBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolPointBtn.Margin = new System.Windows.Forms.Padding(5, 1, 5, 2);
            this.toolPointBtn.Name = "toolPointBtn";
            this.toolPointBtn.Size = new System.Drawing.Size(24, 32);
            this.toolPointBtn.Text = "Plot Point";
            this.toolPointBtn.Click += new System.EventHandler(this.toolPointBtn_Click);
            // 
            // lineDrawTool
            // 
            this.lineDrawTool.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.lineDrawTool.Image = global::csAcq4.Properties.Resources.drawLine;
            this.lineDrawTool.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.lineDrawTool.Margin = new System.Windows.Forms.Padding(5, 1, 5, 2);
            this.lineDrawTool.Name = "lineDrawTool";
            this.lineDrawTool.Size = new System.Drawing.Size(24, 32);
            this.lineDrawTool.Text = "Draw Line";
            this.lineDrawTool.Click += new System.EventHandler(this.lineDrawTool_Click);
            // 
            // toolCircleBtn
            // 
            this.toolCircleBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolCircleBtn.Image = global::csAcq4.Properties.Resources.drawCrcle;
            this.toolCircleBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolCircleBtn.Margin = new System.Windows.Forms.Padding(5, 1, 5, 2);
            this.toolCircleBtn.Name = "toolCircleBtn";
            this.toolCircleBtn.Size = new System.Drawing.Size(24, 32);
            this.toolCircleBtn.Text = "Draw Rectangle";
            this.toolCircleBtn.Click += new System.EventHandler(this.toolCircleBtn_Click);
            // 
            // toolRectBtn
            // 
            this.toolRectBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolRectBtn.Image = global::csAcq4.Properties.Resources.drawRect;
            this.toolRectBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolRectBtn.Margin = new System.Windows.Forms.Padding(5, 1, 5, 2);
            this.toolRectBtn.Name = "toolRectBtn";
            this.toolRectBtn.Size = new System.Drawing.Size(24, 32);
            this.toolRectBtn.Text = "Draw Circle";
            this.toolRectBtn.Click += new System.EventHandler(this.toolRectBtn_Click);
            // 
            // singleArrowTool
            // 
            this.singleArrowTool.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.singleArrowTool.Image = global::csAcq4.Properties.Resources.drawArraw1;
            this.singleArrowTool.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.singleArrowTool.Margin = new System.Windows.Forms.Padding(5, 1, 5, 2);
            this.singleArrowTool.Name = "singleArrowTool";
            this.singleArrowTool.Size = new System.Drawing.Size(24, 32);
            this.singleArrowTool.Text = "Draw Single Arrow Line";
            this.singleArrowTool.Click += new System.EventHandler(this.singleArrowTool_Click);
            // 
            // doubleArrowTool
            // 
            this.doubleArrowTool.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.doubleArrowTool.Image = global::csAcq4.Properties.Resources.drawDoubleArrow;
            this.doubleArrowTool.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.doubleArrowTool.Margin = new System.Windows.Forms.Padding(5, 1, 5, 2);
            this.doubleArrowTool.Name = "doubleArrowTool";
            this.doubleArrowTool.Size = new System.Drawing.Size(24, 32);
            this.doubleArrowTool.Text = "Draw Double Arrow Line";
            this.doubleArrowTool.Click += new System.EventHandler(this.doubleArrowTool_Click);
            // 
            // positionTool
            // 
            this.positionTool.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.positionTool.Image = global::csAcq4.Properties.Resources.loc;
            this.positionTool.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.positionTool.Margin = new System.Windows.Forms.Padding(5, 1, 5, 2);
            this.positionTool.Name = "positionTool";
            this.positionTool.Size = new System.Drawing.Size(24, 32);
            this.positionTool.Text = "Show Crosshair";
            this.positionTool.Click += new System.EventHandler(this.positionTool_Click);
            // 
            // toolStripButton10
            // 
            this.toolStripButton10.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton10.Image = global::csAcq4.Properties.Resources.HistLine;
            this.toolStripButton10.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton10.Margin = new System.Windows.Forms.Padding(5, 1, 5, 2);
            this.toolStripButton10.Name = "toolStripButton10";
            this.toolStripButton10.Size = new System.Drawing.Size(24, 32);
            this.toolStripButton10.Text = "Histogram";
            // 
            // toolCalBtn
            // 
            this.toolCalBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolCalBtn.Image = global::csAcq4.Properties.Resources.cal;
            this.toolCalBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolCalBtn.Margin = new System.Windows.Forms.Padding(5, 1, 5, 2);
            this.toolCalBtn.Name = "toolCalBtn";
            this.toolCalBtn.Size = new System.Drawing.Size(24, 32);
            this.toolCalBtn.Text = "Length Cal";
            this.toolCalBtn.Click += new System.EventHandler(this.toolCalBtn_Click);
            // 
            // colorToolSelec
            // 
            this.colorToolSelec.BackColor = System.Drawing.Color.Transparent;
            this.colorToolSelec.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.colorToolSelec.Image = global::csAcq4.Properties.Resources.colorSelect;
            this.colorToolSelec.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.colorToolSelec.Margin = new System.Windows.Forms.Padding(5, 1, 5, 2);
            this.colorToolSelec.Name = "colorToolSelec";
            this.colorToolSelec.Size = new System.Drawing.Size(24, 32);
            this.colorToolSelec.Text = "toolStripButton1";
            this.colorToolSelec.Click += new System.EventHandler(this.colorToolSelec_Click);
            // 
            // applyAllTool
            // 
            this.applyAllTool.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.applyAllTool.Image = global::csAcq4.Properties.Resources.tongbu;
            this.applyAllTool.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.applyAllTool.Margin = new System.Windows.Forms.Padding(5, 1, 5, 2);
            this.applyAllTool.Name = "applyAllTool";
            this.applyAllTool.Size = new System.Drawing.Size(24, 32);
            this.applyAllTool.Text = "Synchronous Display";
            this.applyAllTool.Click += new System.EventHandler(this.applyAllTool_Click);
            // 
            // cancelDrawTool
            // 
            this.cancelDrawTool.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.cancelDrawTool.Image = global::csAcq4.Properties.Resources.cancel;
            this.cancelDrawTool.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.cancelDrawTool.Margin = new System.Windows.Forms.Padding(5, 1, 5, 2);
            this.cancelDrawTool.Name = "cancelDrawTool";
            this.cancelDrawTool.Size = new System.Drawing.Size(24, 32);
            this.cancelDrawTool.Text = "Delete";
            this.cancelDrawTool.Click += new System.EventHandler(this.cancelDrawTool_Click);
            // 
            // groupBox6
            // 
            this.groupBox6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox6.Controls.Add(this.pathLabel);
            this.groupBox6.Controls.Add(this.uiLine1);
            this.groupBox6.Controls.Add(this.savePathChoose);
            this.groupBox6.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.groupBox6.ForeColor = System.Drawing.Color.White;
            this.groupBox6.Location = new System.Drawing.Point(6, 31);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(313, 128);
            this.groupBox6.TabIndex = 17;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Save Path";
            // 
            // pathLabel
            // 
            this.pathLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pathLabel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.pathLabel.ForeColor = System.Drawing.Color.White;
            this.pathLabel.Location = new System.Drawing.Point(6, 86);
            this.pathLabel.Name = "pathLabel";
            this.pathLabel.Size = new System.Drawing.Size(301, 23);
            this.pathLabel.TabIndex = 19;
            // 
            // uiLine1
            // 
            this.uiLine1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.uiLine1.BackColor = System.Drawing.Color.Transparent;
            this.uiLine1.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.uiLine1.ForeColor = System.Drawing.Color.Gray;
            this.uiLine1.LineColor = System.Drawing.Color.Black;
            this.uiLine1.LineColor2 = System.Drawing.Color.Black;
            this.uiLine1.Location = new System.Drawing.Point(1, 58);
            this.uiLine1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiLine1.Name = "uiLine1";
            this.uiLine1.Size = new System.Drawing.Size(312, 29);
            this.uiLine1.TabIndex = 20;
            this.uiLine1.Text = "Path Selected:";
            this.uiLine1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // savePathChoose
            // 
            this.savePathChoose.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.savePathChoose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(95)))), ((int)(((byte)(95)))), ((int)(((byte)(95)))));
            this.savePathChoose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.savePathChoose.FillColor = System.Drawing.Color.Gray;
            this.savePathChoose.FillColor2 = System.Drawing.Color.Gray;
            this.savePathChoose.FillDisableColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.savePathChoose.FillHoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.savePathChoose.FillPressColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.savePathChoose.FillSelectedColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.savePathChoose.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.savePathChoose.Location = new System.Drawing.Point(6, 25);
            this.savePathChoose.MinimumSize = new System.Drawing.Size(1, 1);
            this.savePathChoose.Name = "savePathChoose";
            this.savePathChoose.Radius = 2;
            this.savePathChoose.RectColor = System.Drawing.Color.Transparent;
            this.savePathChoose.RectDisableColor = System.Drawing.Color.Transparent;
            this.savePathChoose.RectHoverColor = System.Drawing.Color.Transparent;
            this.savePathChoose.RectPressColor = System.Drawing.Color.Transparent;
            this.savePathChoose.RectSelectedColor = System.Drawing.Color.Transparent;
            this.savePathChoose.Size = new System.Drawing.Size(301, 25);
            this.savePathChoose.Symbol = 61717;
            this.savePathChoose.SymbolColor = System.Drawing.Color.Black;
            this.savePathChoose.SymbolSize = 18;
            this.savePathChoose.TabIndex = 19;
            this.savePathChoose.Text = "Choose Folder";
            this.savePathChoose.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.savePathChoose.Click += new System.EventHandler(this.savePathChoose_Click);
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.preName);
            this.groupBox5.Controls.Add(this.label1);
            this.groupBox5.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.groupBox5.ForeColor = System.Drawing.Color.White;
            this.groupBox5.Location = new System.Drawing.Point(6, 177);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(313, 66);
            this.groupBox5.TabIndex = 18;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "File Name";
            // 
            // preName
            // 
            this.preName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.preName.BackColor = System.Drawing.Color.Silver;
            this.preName.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.preName.Location = new System.Drawing.Point(95, 26);
            this.preName.Name = "preName";
            this.preName.Size = new System.Drawing.Size(195, 25);
            this.preName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(13, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(45, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "Prefix:";
            // 
            // saveGroup
            // 
            this.saveGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.saveGroup.Controls.Add(this.groupBox3);
            this.saveGroup.Controls.Add(this.saveImg);
            this.saveGroup.Controls.Add(this.groupBox4);
            this.saveGroup.Controls.Add(this.groupBox5);
            this.saveGroup.Controls.Add(this.groupBox6);
            this.saveGroup.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.saveGroup.ForeColor = System.Drawing.Color.White;
            this.saveGroup.Location = new System.Drawing.Point(3, 280);
            this.saveGroup.Name = "saveGroup";
            this.saveGroup.Size = new System.Drawing.Size(338, 624);
            this.saveGroup.TabIndex = 18;
            this.saveGroup.TabStop = false;
            this.saveGroup.Text = "Save";
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.delayVal);
            this.groupBox3.Controls.Add(this.shootStart);
            this.groupBox3.Controls.Add(this.numVal);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.periodVal);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.groupBox3.ForeColor = System.Drawing.Color.White;
            this.groupBox3.Location = new System.Drawing.Point(6, 304);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(312, 220);
            this.groupBox3.TabIndex = 21;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Burst Settings";
            // 
            // delayVal
            // 
            this.delayVal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.delayVal.BackColor = System.Drawing.Color.Silver;
            this.delayVal.DecimalPlaces = 3;
            this.delayVal.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.delayVal.Location = new System.Drawing.Point(95, 35);
            this.delayVal.Name = "delayVal";
            this.delayVal.Size = new System.Drawing.Size(195, 25);
            this.delayVal.TabIndex = 19;
            this.delayVal.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // shootStart
            // 
            this.shootStart.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.shootStart.BackColor = System.Drawing.Color.Transparent;
            this.shootStart.Cursor = System.Windows.Forms.Cursors.Hand;
            this.shootStart.FillColor = System.Drawing.Color.Gray;
            this.shootStart.FillDisableColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.shootStart.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.shootStart.Location = new System.Drawing.Point(17, 166);
            this.shootStart.MinimumSize = new System.Drawing.Size(1, 1);
            this.shootStart.Name = "shootStart";
            this.shootStart.RectColor = System.Drawing.Color.Transparent;
            this.shootStart.RectDisableColor = System.Drawing.Color.Transparent;
            this.shootStart.RectHoverColor = System.Drawing.Color.Transparent;
            this.shootStart.RectPressColor = System.Drawing.Color.Transparent;
            this.shootStart.RectSelectedColor = System.Drawing.Color.Transparent;
            this.shootStart.Size = new System.Drawing.Size(273, 30);
            this.shootStart.TabIndex = 79;
            this.shootStart.Text = "Start";
            this.shootStart.TipsColor = System.Drawing.Color.Transparent;
            this.shootStart.TipsFont = new System.Drawing.Font("Segoe UI", 10F);
            this.shootStart.TipsForeColor = System.Drawing.Color.Transparent;
            this.shootStart.Click += new System.EventHandler(this.shootStart_Click);
            // 
            // numVal
            // 
            this.numVal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.numVal.BackColor = System.Drawing.Color.Silver;
            this.numVal.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.numVal.Location = new System.Drawing.Point(95, 125);
            this.numVal.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numVal.Name = "numVal";
            this.numVal.Size = new System.Drawing.Size(195, 25);
            this.numVal.TabIndex = 22;
            this.numVal.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(14, 127);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 19);
            this.label4.TabIndex = 21;
            this.label4.Text = "Number:";
            // 
            // periodVal
            // 
            this.periodVal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.periodVal.BackColor = System.Drawing.Color.Silver;
            this.periodVal.DecimalPlaces = 3;
            this.periodVal.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.periodVal.Location = new System.Drawing.Point(94, 81);
            this.periodVal.Name = "periodVal";
            this.periodVal.Size = new System.Drawing.Size(195, 25);
            this.periodVal.TabIndex = 20;
            this.periodVal.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(13, 83);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 19);
            this.label3.TabIndex = 2;
            this.label3.Text = "Period(s):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(14, 37);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(60, 19);
            this.label2.TabIndex = 1;
            this.label2.Text = "Delay(s):";
            // 
            // saveImg
            // 
            this.saveImg.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.saveImg.BackColor = System.Drawing.Color.Transparent;
            this.saveImg.Cursor = System.Windows.Forms.Cursors.Hand;
            this.saveImg.FillColor = System.Drawing.Color.Gray;
            this.saveImg.FillDisableColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.saveImg.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.saveImg.Location = new System.Drawing.Point(24, 259);
            this.saveImg.MinimumSize = new System.Drawing.Size(1, 1);
            this.saveImg.Name = "saveImg";
            this.saveImg.RectColor = System.Drawing.Color.Transparent;
            this.saveImg.RectDisableColor = System.Drawing.Color.Transparent;
            this.saveImg.RectHoverColor = System.Drawing.Color.Transparent;
            this.saveImg.RectPressColor = System.Drawing.Color.Transparent;
            this.saveImg.RectSelectedColor = System.Drawing.Color.Transparent;
            this.saveImg.Size = new System.Drawing.Size(272, 30);
            this.saveImg.TabIndex = 78;
            this.saveImg.Text = "Current Save";
            this.saveImg.TipsColor = System.Drawing.Color.Transparent;
            this.saveImg.TipsFont = new System.Drawing.Font("Segoe UI", 10F);
            this.saveImg.TipsForeColor = System.Drawing.Color.Transparent;
            this.saveImg.Click += new System.EventHandler(this.saveImg_Click);
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.videoRecorder);
            this.groupBox4.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.groupBox4.ForeColor = System.Drawing.Color.White;
            this.groupBox4.Location = new System.Drawing.Point(6, 530);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(313, 77);
            this.groupBox4.TabIndex = 19;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Video";
            // 
            // videoRecorder
            // 
            this.videoRecorder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.videoRecorder.BackColor = System.Drawing.Color.Transparent;
            this.videoRecorder.Cursor = System.Windows.Forms.Cursors.Hand;
            this.videoRecorder.FillColor = System.Drawing.Color.Gray;
            this.videoRecorder.FillDisableColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.videoRecorder.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.videoRecorder.Location = new System.Drawing.Point(17, 28);
            this.videoRecorder.MinimumSize = new System.Drawing.Size(1, 1);
            this.videoRecorder.Name = "videoRecorder";
            this.videoRecorder.RectColor = System.Drawing.Color.Transparent;
            this.videoRecorder.RectDisableColor = System.Drawing.Color.Transparent;
            this.videoRecorder.RectHoverColor = System.Drawing.Color.Transparent;
            this.videoRecorder.RectPressColor = System.Drawing.Color.Transparent;
            this.videoRecorder.RectSelectedColor = System.Drawing.Color.Transparent;
            this.videoRecorder.Size = new System.Drawing.Size(273, 35);
            this.videoRecorder.TabIndex = 80;
            this.videoRecorder.Text = "Start";
            this.videoRecorder.TipsColor = System.Drawing.Color.Transparent;
            this.videoRecorder.TipsFont = new System.Drawing.Font("Segoe UI", 10F);
            this.videoRecorder.TipsForeColor = System.Drawing.Color.Transparent;
            this.videoRecorder.Click += new System.EventHandler(this.videoRecorder_Click);
            // 
            // funGroup
            // 
            this.funGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.funGroup.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.funGroup.Controls.Add(this.enhanceBtn);
            this.funGroup.Controls.Add(this.uiLabel3);
            this.funGroup.Controls.Add(this.exposureNum);
            this.funGroup.Controls.Add(this.uiLabel2);
            this.funGroup.Controls.Add(this.pushLive);
            this.funGroup.Controls.Add(this.pixelCombox);
            this.funGroup.Controls.Add(this.uiLabel1);
            this.funGroup.Controls.Add(this.devName);
            this.funGroup.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.funGroup.ForeColor = System.Drawing.Color.White;
            this.funGroup.Location = new System.Drawing.Point(3, 8);
            this.funGroup.Name = "funGroup";
            this.funGroup.Size = new System.Drawing.Size(338, 266);
            this.funGroup.TabIndex = 16;
            this.funGroup.TabStop = false;
            this.funGroup.Text = "Function";
            // 
            // enhanceBtn
            // 
            this.enhanceBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.enhanceBtn.BackColor = System.Drawing.Color.Transparent;
            this.enhanceBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.enhanceBtn.FillColor = System.Drawing.Color.Gray;
            this.enhanceBtn.FillDisableColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.enhanceBtn.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.enhanceBtn.Location = new System.Drawing.Point(33, 212);
            this.enhanceBtn.MinimumSize = new System.Drawing.Size(1, 1);
            this.enhanceBtn.Name = "enhanceBtn";
            this.enhanceBtn.RectColor = System.Drawing.Color.Transparent;
            this.enhanceBtn.RectDisableColor = System.Drawing.Color.Transparent;
            this.enhanceBtn.RectHoverColor = System.Drawing.Color.Transparent;
            this.enhanceBtn.RectPressColor = System.Drawing.Color.Transparent;
            this.enhanceBtn.RectSelectedColor = System.Drawing.Color.Transparent;
            this.enhanceBtn.Size = new System.Drawing.Size(280, 35);
            this.enhanceBtn.TabIndex = 78;
            this.enhanceBtn.Text = "Enhance";
            this.enhanceBtn.TipsColor = System.Drawing.Color.Transparent;
            this.enhanceBtn.TipsFont = new System.Drawing.Font("Segoe UI", 10F);
            this.enhanceBtn.TipsForeColor = System.Drawing.Color.Transparent;
            this.enhanceBtn.Click += new System.EventHandler(this.enhanceBtn_Click);
            // 
            // uiLabel3
            // 
            this.uiLabel3.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.uiLabel3.ForeColor = System.Drawing.Color.White;
            this.uiLabel3.Location = new System.Drawing.Point(31, 168);
            this.uiLabel3.Name = "uiLabel3";
            this.uiLabel3.Size = new System.Drawing.Size(88, 23);
            this.uiLabel3.TabIndex = 23;
            this.uiLabel3.Text = "Exposure:";
            // 
            // exposureNum
            // 
            this.exposureNum.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.exposureNum.BackColor = System.Drawing.Color.Silver;
            this.exposureNum.DecimalPlaces = 3;
            this.exposureNum.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.exposureNum.Increment = new decimal(new int[] {
            1,
            0,
            0,
            131072});
            this.exposureNum.Location = new System.Drawing.Point(124, 169);
            this.exposureNum.Name = "exposureNum";
            this.exposureNum.Size = new System.Drawing.Size(189, 25);
            this.exposureNum.TabIndex = 18;
            // 
            // uiLabel2
            // 
            this.uiLabel2.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.uiLabel2.ForeColor = System.Drawing.Color.White;
            this.uiLabel2.Location = new System.Drawing.Point(30, 126);
            this.uiLabel2.Name = "uiLabel2";
            this.uiLabel2.Size = new System.Drawing.Size(88, 23);
            this.uiLabel2.TabIndex = 22;
            this.uiLabel2.Text = "Pixel:";
            // 
            // pushLive
            // 
            this.pushLive.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pushLive.BackColor = System.Drawing.Color.Transparent;
            this.pushLive.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pushLive.FillColor = System.Drawing.Color.Gray;
            this.pushLive.FillDisableColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.pushLive.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.pushLive.Location = new System.Drawing.Point(33, 72);
            this.pushLive.MinimumSize = new System.Drawing.Size(1, 1);
            this.pushLive.Name = "pushLive";
            this.pushLive.RectColor = System.Drawing.Color.Transparent;
            this.pushLive.RectDisableColor = System.Drawing.Color.Transparent;
            this.pushLive.RectHoverColor = System.Drawing.Color.Transparent;
            this.pushLive.RectPressColor = System.Drawing.Color.Transparent;
            this.pushLive.RectSelectedColor = System.Drawing.Color.Transparent;
            this.pushLive.Size = new System.Drawing.Size(280, 35);
            this.pushLive.TabIndex = 77;
            this.pushLive.Text = "Live";
            this.pushLive.TipsColor = System.Drawing.Color.Transparent;
            this.pushLive.TipsFont = new System.Drawing.Font("Segoe UI", 10F);
            this.pushLive.TipsForeColor = System.Drawing.Color.Transparent;
            this.pushLive.Click += new System.EventHandler(this.pushLive_Click);
            // 
            // pixelCombox
            // 
            this.pixelCombox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pixelCombox.BackColor = System.Drawing.Color.Silver;
            this.pixelCombox.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.pixelCombox.FormattingEnabled = true;
            this.pixelCombox.Items.AddRange(new object[] {
            "8",
            "12",
            "16"});
            this.pixelCombox.Location = new System.Drawing.Point(124, 124);
            this.pixelCombox.Name = "pixelCombox";
            this.pixelCombox.Size = new System.Drawing.Size(189, 25);
            this.pixelCombox.TabIndex = 16;
            // 
            // uiLabel1
            // 
            this.uiLabel1.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.uiLabel1.ForeColor = System.Drawing.Color.White;
            this.uiLabel1.Location = new System.Drawing.Point(30, 30);
            this.uiLabel1.Name = "uiLabel1";
            this.uiLabel1.Size = new System.Drawing.Size(88, 23);
            this.uiLabel1.TabIndex = 0;
            this.uiLabel1.Text = "Camera:";
            // 
            // devName
            // 
            this.devName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.devName.BackColor = System.Drawing.Color.Silver;
            this.devName.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.devName.FormattingEnabled = true;
            this.devName.Location = new System.Drawing.Point(124, 31);
            this.devName.Name = "devName";
            this.devName.Size = new System.Drawing.Size(189, 25);
            this.devName.TabIndex = 14;
            // 
            // leftPanel
            // 
            this.leftPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.leftPanel.Controls.Add(this.funGroup);
            this.leftPanel.Controls.Add(this.saveGroup);
            this.leftPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.leftPanel.Location = new System.Drawing.Point(0, 101);
            this.leftPanel.Name = "leftPanel";
            this.leftPanel.Size = new System.Drawing.Size(350, 917);
            this.leftPanel.TabIndex = 50;
            // 
            // uiTitlePanel1
            // 
            this.uiTitlePanel1.Controls.Add(this.smallShow9);
            this.uiTitlePanel1.Controls.Add(this.smallShow8);
            this.uiTitlePanel1.Controls.Add(this.smallShow7);
            this.uiTitlePanel1.Controls.Add(this.smallShow6);
            this.uiTitlePanel1.Controls.Add(this.smallShow5);
            this.uiTitlePanel1.Controls.Add(this.smallShow4);
            this.uiTitlePanel1.Controls.Add(this.smallShow3);
            this.uiTitlePanel1.Controls.Add(this.smallShow2);
            this.uiTitlePanel1.Controls.Add(this.smallShow1);
            this.uiTitlePanel1.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.uiTitlePanel1.FillColor2 = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.uiTitlePanel1.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.uiTitlePanel1.Location = new System.Drawing.Point(4, 5);
            this.uiTitlePanel1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.uiTitlePanel1.MinimumSize = new System.Drawing.Size(1, 1);
            this.uiTitlePanel1.Name = "uiTitlePanel1";
            this.uiTitlePanel1.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(71)))), ((int)(((byte)(71)))));
            this.uiTitlePanel1.ShowCollapse = true;
            this.uiTitlePanel1.ShowText = false;
            this.uiTitlePanel1.Size = new System.Drawing.Size(442, 459);
            this.uiTitlePanel1.TabIndex = 27;
            this.uiTitlePanel1.Text = "View Thumbnail";
            this.uiTitlePanel1.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.uiTitlePanel1.TitleColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(71)))), ((int)(((byte)(71)))));
            this.uiTitlePanel1.TitleHeight = 19;
            // 
            // smallShow9
            // 
            this.smallShow9.Location = new System.Drawing.Point(302, 315);
            this.smallShow9.Margin = new System.Windows.Forms.Padding(4);
            this.smallShow9.Name = "smallShow9";
            this.smallShow9.Size = new System.Drawing.Size(140, 140);
            this.smallShow9.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.smallShow9.TabIndex = 36;
            this.smallShow9.TabStop = false;
            // 
            // smallShow8
            // 
            this.smallShow8.Location = new System.Drawing.Point(154, 315);
            this.smallShow8.Margin = new System.Windows.Forms.Padding(4);
            this.smallShow8.Name = "smallShow8";
            this.smallShow8.Size = new System.Drawing.Size(140, 140);
            this.smallShow8.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.smallShow8.TabIndex = 35;
            this.smallShow8.TabStop = false;
            // 
            // smallShow7
            // 
            this.smallShow7.Location = new System.Drawing.Point(6, 315);
            this.smallShow7.Margin = new System.Windows.Forms.Padding(4);
            this.smallShow7.Name = "smallShow7";
            this.smallShow7.Size = new System.Drawing.Size(140, 140);
            this.smallShow7.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.smallShow7.TabIndex = 34;
            this.smallShow7.TabStop = false;
            // 
            // smallShow6
            // 
            this.smallShow6.Location = new System.Drawing.Point(302, 168);
            this.smallShow6.Margin = new System.Windows.Forms.Padding(4);
            this.smallShow6.Name = "smallShow6";
            this.smallShow6.Size = new System.Drawing.Size(140, 140);
            this.smallShow6.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.smallShow6.TabIndex = 33;
            this.smallShow6.TabStop = false;
            // 
            // smallShow5
            // 
            this.smallShow5.Location = new System.Drawing.Point(154, 168);
            this.smallShow5.Margin = new System.Windows.Forms.Padding(4);
            this.smallShow5.Name = "smallShow5";
            this.smallShow5.Size = new System.Drawing.Size(140, 140);
            this.smallShow5.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.smallShow5.TabIndex = 32;
            this.smallShow5.TabStop = false;
            // 
            // smallShow4
            // 
            this.smallShow4.Location = new System.Drawing.Point(6, 167);
            this.smallShow4.Margin = new System.Windows.Forms.Padding(4);
            this.smallShow4.Name = "smallShow4";
            this.smallShow4.Size = new System.Drawing.Size(140, 140);
            this.smallShow4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.smallShow4.TabIndex = 31;
            this.smallShow4.TabStop = false;
            // 
            // smallShow3
            // 
            this.smallShow3.Location = new System.Drawing.Point(302, 23);
            this.smallShow3.Margin = new System.Windows.Forms.Padding(4);
            this.smallShow3.Name = "smallShow3";
            this.smallShow3.Size = new System.Drawing.Size(140, 140);
            this.smallShow3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.smallShow3.TabIndex = 30;
            this.smallShow3.TabStop = false;
            // 
            // smallShow2
            // 
            this.smallShow2.Location = new System.Drawing.Point(154, 23);
            this.smallShow2.Margin = new System.Windows.Forms.Padding(4);
            this.smallShow2.Name = "smallShow2";
            this.smallShow2.Size = new System.Drawing.Size(140, 140);
            this.smallShow2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.smallShow2.TabIndex = 29;
            this.smallShow2.TabStop = false;
            // 
            // smallShow1
            // 
            this.smallShow1.Location = new System.Drawing.Point(6, 23);
            this.smallShow1.Margin = new System.Windows.Forms.Padding(4);
            this.smallShow1.Name = "smallShow1";
            this.smallShow1.Size = new System.Drawing.Size(140, 140);
            this.smallShow1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.smallShow1.TabIndex = 28;
            this.smallShow1.TabStop = false;
            // 
            // histogramPanel
            // 
            this.histogramPanel.Controls.Add(this.histSelect);
            this.histogramPanel.Controls.Add(this.histLine);
            this.histogramPanel.FillColor = System.Drawing.Color.Gray;
            this.histogramPanel.FillColor2 = System.Drawing.Color.Gray;
            this.histogramPanel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.histogramPanel.Location = new System.Drawing.Point(8, 473);
            this.histogramPanel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.histogramPanel.MinimumSize = new System.Drawing.Size(1, 1);
            this.histogramPanel.Name = "histogramPanel";
            this.histogramPanel.RectColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.histogramPanel.ShowCollapse = true;
            this.histogramPanel.ShowText = false;
            this.histogramPanel.Size = new System.Drawing.Size(446, 431);
            this.histogramPanel.TabIndex = 37;
            this.histogramPanel.Text = "Histogram";
            this.histogramPanel.TextAlignment = System.Drawing.ContentAlignment.MiddleLeft;
            this.histogramPanel.TitleColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(71)))), ((int)(((byte)(71)))));
            this.histogramPanel.TitleHeight = 19;
            // 
            // histSelect
            // 
            this.histSelect.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.histSelect.ForeColor = System.Drawing.Color.White;
            this.histSelect.FormattingEnabled = true;
            this.histSelect.Location = new System.Drawing.Point(164, 25);
            this.histSelect.Margin = new System.Windows.Forms.Padding(2);
            this.histSelect.Name = "histSelect";
            this.histSelect.Size = new System.Drawing.Size(155, 25);
            this.histSelect.TabIndex = 4;
            // 
            // histLine
            // 
            this.histLine.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.histLine.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.histLine.ForeColor = System.Drawing.Color.White;
            this.histLine.LegendFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.histLine.Location = new System.Drawing.Point(-38, 54);
            this.histLine.Margin = new System.Windows.Forms.Padding(2);
            this.histLine.MinimumSize = new System.Drawing.Size(1, 1);
            this.histLine.Name = "histLine";
            this.histLine.RectColor = System.Drawing.Color.Gray;
            this.histLine.Size = new System.Drawing.Size(533, 413);
            this.histLine.SubFont = new System.Drawing.Font("Segoe UI", 10F);
            this.histLine.TabIndex = 3;
            this.histLine.Text = "uiBarChart1";
            // 
            // rightPanel
            // 
            this.rightPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.rightPanel.Controls.Add(this.histogramPanel);
            this.rightPanel.Controls.Add(this.uiTitlePanel1);
            this.rightPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.rightPanel.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.rightPanel.Location = new System.Drawing.Point(1470, 101);
            this.rightPanel.Margin = new System.Windows.Forms.Padding(0);
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.Size = new System.Drawing.Size(450, 917);
            this.rightPanel.TabIndex = 47;
            // 
            // middlePanel
            // 
            this.middlePanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.middlePanel.Controls.Add(this.LiveBar);
            this.middlePanel.Controls.Add(this.RestoreBtn);
            this.middlePanel.Controls.Add(this.recoveryBtn);
            this.middlePanel.Controls.Add(this.narrowBtn);
            this.middlePanel.Controls.Add(this.enlargeBtn);
            this.middlePanel.Controls.Add(this.PicDisplay9);
            this.middlePanel.Controls.Add(this.PicDisplay8);
            this.middlePanel.Controls.Add(this.PicDisplay7);
            this.middlePanel.Controls.Add(this.PicDisplay6);
            this.middlePanel.Controls.Add(this.PicDisplay5);
            this.middlePanel.Controls.Add(this.PicDisplay4);
            this.middlePanel.Controls.Add(this.PicDisplay3);
            this.middlePanel.Controls.Add(this.PicDisplay2);
            this.middlePanel.Controls.Add(this.PicDisplay1);
            this.middlePanel.Controls.Add(this.FullDisPlay);
            this.middlePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.middlePanel.Location = new System.Drawing.Point(353, 101);
            this.middlePanel.Margin = new System.Windows.Forms.Padding(0);
            this.middlePanel.Name = "middlePanel";
            this.middlePanel.Size = new System.Drawing.Size(1114, 917);
            this.middlePanel.TabIndex = 53;
            // 
            // LiveBar
            // 
            this.LiveBar.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.LiveBar.FillWidth = 12;
            this.LiveBar.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LiveBar.Location = new System.Drawing.Point(854, 35);
            this.LiveBar.MinimumSize = new System.Drawing.Size(1, 1);
            this.LiveBar.Name = "LiveBar";
            this.LiveBar.Size = new System.Drawing.Size(18, 674);
            this.LiveBar.TabIndex = 77;
            this.LiveBar.Text = "uiScrollBar1";
            this.LiveBar.Visible = false;
            this.LiveBar.ValueChanged += new System.EventHandler(this.LiveBar_ValueChanged);
            // 
            // RestoreBtn
            // 
            this.RestoreBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.RestoreBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.RestoreBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.RestoreBtn.FillColor = System.Drawing.Color.Transparent;
            this.RestoreBtn.FillColor2 = System.Drawing.Color.Transparent;
            this.RestoreBtn.FillDisableColor = System.Drawing.Color.Transparent;
            this.RestoreBtn.FillHoverColor = System.Drawing.Color.Transparent;
            this.RestoreBtn.FillPressColor = System.Drawing.Color.Transparent;
            this.RestoreBtn.FillSelectedColor = System.Drawing.Color.Transparent;
            this.RestoreBtn.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RestoreBtn.Location = new System.Drawing.Point(990, 9);
            this.RestoreBtn.MinimumSize = new System.Drawing.Size(1, 1);
            this.RestoreBtn.Name = "RestoreBtn";
            this.RestoreBtn.RectColor = System.Drawing.Color.Transparent;
            this.RestoreBtn.RectDisableColor = System.Drawing.Color.Transparent;
            this.RestoreBtn.RectHoverColor = System.Drawing.Color.Transparent;
            this.RestoreBtn.RectPressColor = System.Drawing.Color.Transparent;
            this.RestoreBtn.RectSelectedColor = System.Drawing.Color.Transparent;
            this.RestoreBtn.Size = new System.Drawing.Size(25, 25);
            this.RestoreBtn.Symbol = 361470;
            this.RestoreBtn.SymbolColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(171)))), ((int)(((byte)(171)))));
            this.RestoreBtn.TabIndex = 76;
            this.RestoreBtn.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.RestoreBtn.Click += new System.EventHandler(this.RestoreBtn_Click);
            // 
            // recoveryBtn
            // 
            this.recoveryBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.recoveryBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.recoveryBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.recoveryBtn.FillColor = System.Drawing.Color.Transparent;
            this.recoveryBtn.FillColor2 = System.Drawing.Color.Transparent;
            this.recoveryBtn.FillDisableColor = System.Drawing.Color.Transparent;
            this.recoveryBtn.FillHoverColor = System.Drawing.Color.Transparent;
            this.recoveryBtn.FillPressColor = System.Drawing.Color.Transparent;
            this.recoveryBtn.FillSelectedColor = System.Drawing.Color.Transparent;
            this.recoveryBtn.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.recoveryBtn.Location = new System.Drawing.Point(1083, 9);
            this.recoveryBtn.MinimumSize = new System.Drawing.Size(1, 1);
            this.recoveryBtn.Name = "recoveryBtn";
            this.recoveryBtn.RectColor = System.Drawing.Color.Transparent;
            this.recoveryBtn.RectDisableColor = System.Drawing.Color.Transparent;
            this.recoveryBtn.RectHoverColor = System.Drawing.Color.Transparent;
            this.recoveryBtn.RectPressColor = System.Drawing.Color.Transparent;
            this.recoveryBtn.RectSelectedColor = System.Drawing.Color.Transparent;
            this.recoveryBtn.Size = new System.Drawing.Size(25, 25);
            this.recoveryBtn.Symbol = 361540;
            this.recoveryBtn.SymbolColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(171)))), ((int)(((byte)(171)))));
            this.recoveryBtn.TabIndex = 71;
            this.recoveryBtn.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.recoveryBtn.Click += new System.EventHandler(this.recoveryBtn_Click);
            // 
            // narrowBtn
            // 
            this.narrowBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.narrowBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.narrowBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.narrowBtn.FillColor = System.Drawing.Color.Transparent;
            this.narrowBtn.FillColor2 = System.Drawing.Color.Transparent;
            this.narrowBtn.FillDisableColor = System.Drawing.Color.Transparent;
            this.narrowBtn.FillHoverColor = System.Drawing.Color.Transparent;
            this.narrowBtn.FillPressColor = System.Drawing.Color.Transparent;
            this.narrowBtn.FillSelectedColor = System.Drawing.Color.Transparent;
            this.narrowBtn.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.narrowBtn.Location = new System.Drawing.Point(1052, 9);
            this.narrowBtn.MinimumSize = new System.Drawing.Size(1, 1);
            this.narrowBtn.Name = "narrowBtn";
            this.narrowBtn.RectColor = System.Drawing.Color.Transparent;
            this.narrowBtn.RectDisableColor = System.Drawing.Color.Transparent;
            this.narrowBtn.RectHoverColor = System.Drawing.Color.Transparent;
            this.narrowBtn.RectPressColor = System.Drawing.Color.Transparent;
            this.narrowBtn.RectSelectedColor = System.Drawing.Color.Transparent;
            this.narrowBtn.Size = new System.Drawing.Size(25, 25);
            this.narrowBtn.Symbol = 361456;
            this.narrowBtn.SymbolColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(171)))), ((int)(((byte)(171)))));
            this.narrowBtn.TabIndex = 70;
            this.narrowBtn.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.narrowBtn.Click += new System.EventHandler(this.narrowBtn_Click);
            // 
            // enlargeBtn
            // 
            this.enlargeBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.enlargeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.enlargeBtn.Cursor = System.Windows.Forms.Cursors.Hand;
            this.enlargeBtn.FillColor = System.Drawing.Color.Transparent;
            this.enlargeBtn.FillColor2 = System.Drawing.Color.Transparent;
            this.enlargeBtn.FillDisableColor = System.Drawing.Color.Transparent;
            this.enlargeBtn.FillHoverColor = System.Drawing.Color.Transparent;
            this.enlargeBtn.FillPressColor = System.Drawing.Color.Transparent;
            this.enlargeBtn.FillSelectedColor = System.Drawing.Color.Transparent;
            this.enlargeBtn.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.enlargeBtn.Location = new System.Drawing.Point(1021, 9);
            this.enlargeBtn.MinimumSize = new System.Drawing.Size(1, 1);
            this.enlargeBtn.Name = "enlargeBtn";
            this.enlargeBtn.RectColor = System.Drawing.Color.Transparent;
            this.enlargeBtn.RectDisableColor = System.Drawing.Color.Transparent;
            this.enlargeBtn.RectHoverColor = System.Drawing.Color.Transparent;
            this.enlargeBtn.RectPressColor = System.Drawing.Color.Transparent;
            this.enlargeBtn.RectSelectedColor = System.Drawing.Color.Transparent;
            this.enlargeBtn.Size = new System.Drawing.Size(25, 25);
            this.enlargeBtn.Symbol = 361454;
            this.enlargeBtn.SymbolColor = System.Drawing.Color.FromArgb(((int)(((byte)(171)))), ((int)(((byte)(171)))), ((int)(((byte)(171)))));
            this.enlargeBtn.TabIndex = 69;
            this.enlargeBtn.TipsFont = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.enlargeBtn.Click += new System.EventHandler(this.enlargeBtn_Click);
            // 
            // PicDisplay9
            // 
            this.PicDisplay9.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.PicDisplay9.Location = new System.Drawing.Point(627, 491);
            this.PicDisplay9.Margin = new System.Windows.Forms.Padding(4);
            this.PicDisplay9.Name = "PicDisplay9";
            this.PicDisplay9.Size = new System.Drawing.Size(220, 220);
            this.PicDisplay9.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.PicDisplay9.TabIndex = 58;
            this.PicDisplay9.TabStop = false;
            // 
            // PicDisplay8
            // 
            this.PicDisplay8.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.PicDisplay8.Location = new System.Drawing.Point(399, 491);
            this.PicDisplay8.Margin = new System.Windows.Forms.Padding(4);
            this.PicDisplay8.Name = "PicDisplay8";
            this.PicDisplay8.Size = new System.Drawing.Size(220, 220);
            this.PicDisplay8.TabIndex = 57;
            this.PicDisplay8.TabStop = false;
            // 
            // PicDisplay7
            // 
            this.PicDisplay7.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.PicDisplay7.Location = new System.Drawing.Point(171, 491);
            this.PicDisplay7.Margin = new System.Windows.Forms.Padding(4);
            this.PicDisplay7.Name = "PicDisplay7";
            this.PicDisplay7.Size = new System.Drawing.Size(220, 220);
            this.PicDisplay7.TabIndex = 56;
            this.PicDisplay7.TabStop = false;
            // 
            // PicDisplay6
            // 
            this.PicDisplay6.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.PicDisplay6.Location = new System.Drawing.Point(627, 263);
            this.PicDisplay6.Margin = new System.Windows.Forms.Padding(4);
            this.PicDisplay6.Name = "PicDisplay6";
            this.PicDisplay6.Size = new System.Drawing.Size(220, 220);
            this.PicDisplay6.TabIndex = 55;
            this.PicDisplay6.TabStop = false;
            // 
            // PicDisplay5
            // 
            this.PicDisplay5.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.PicDisplay5.Location = new System.Drawing.Point(399, 263);
            this.PicDisplay5.Margin = new System.Windows.Forms.Padding(4);
            this.PicDisplay5.Name = "PicDisplay5";
            this.PicDisplay5.Size = new System.Drawing.Size(220, 220);
            this.PicDisplay5.TabIndex = 54;
            this.PicDisplay5.TabStop = false;
            // 
            // PicDisplay4
            // 
            this.PicDisplay4.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.PicDisplay4.Location = new System.Drawing.Point(171, 263);
            this.PicDisplay4.Margin = new System.Windows.Forms.Padding(4);
            this.PicDisplay4.Name = "PicDisplay4";
            this.PicDisplay4.Size = new System.Drawing.Size(220, 220);
            this.PicDisplay4.TabIndex = 53;
            this.PicDisplay4.TabStop = false;
            // 
            // PicDisplay3
            // 
            this.PicDisplay3.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.PicDisplay3.Location = new System.Drawing.Point(627, 35);
            this.PicDisplay3.Margin = new System.Windows.Forms.Padding(4);
            this.PicDisplay3.Name = "PicDisplay3";
            this.PicDisplay3.Size = new System.Drawing.Size(220, 220);
            this.PicDisplay3.TabIndex = 52;
            this.PicDisplay3.TabStop = false;
            // 
            // PicDisplay2
            // 
            this.PicDisplay2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.PicDisplay2.Location = new System.Drawing.Point(399, 35);
            this.PicDisplay2.Margin = new System.Windows.Forms.Padding(4);
            this.PicDisplay2.Name = "PicDisplay2";
            this.PicDisplay2.Size = new System.Drawing.Size(220, 220);
            this.PicDisplay2.TabIndex = 51;
            this.PicDisplay2.TabStop = false;
            // 
            // PicDisplay1
            // 
            this.PicDisplay1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.PicDisplay1.Location = new System.Drawing.Point(171, 35);
            this.PicDisplay1.Margin = new System.Windows.Forms.Padding(4);
            this.PicDisplay1.Name = "PicDisplay1";
            this.PicDisplay1.Size = new System.Drawing.Size(220, 220);
            this.PicDisplay1.TabIndex = 50;
            this.PicDisplay1.TabStop = false;
            // 
            // FullDisPlay
            // 
            this.FullDisPlay.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.FullDisPlay.Location = new System.Drawing.Point(170, 34);
            this.FullDisPlay.Name = "FullDisPlay";
            this.FullDisPlay.Size = new System.Drawing.Size(677, 674);
            this.FullDisPlay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.FullDisPlay.TabIndex = 68;
            this.FullDisPlay.TabStop = false;
            // 
            // menuStrip2
            // 
            this.menuStrip2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.menuStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.najingNanoToolStripMenuItem,
            this.ExitMenuItem,
            this.fullScreanMenuItem,
            this.smallMenuItem});
            this.menuStrip2.Location = new System.Drawing.Point(0, 0);
            this.menuStrip2.Name = "menuStrip2";
            this.menuStrip2.Size = new System.Drawing.Size(1920, 38);
            this.menuStrip2.TabIndex = 54;
            this.menuStrip2.Text = "menuStrip2";
            this.menuStrip2.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Form_MouseDown);
            this.menuStrip2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Form_MouseMove);
            this.menuStrip2.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Form_MouseUp);
            // 
            // najingNanoToolStripMenuItem
            // 
            this.najingNanoToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 16F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))));
            this.najingNanoToolStripMenuItem.ForeColor = System.Drawing.Color.White;
            this.najingNanoToolStripMenuItem.Name = "najingNanoToolStripMenuItem";
            this.najingNanoToolStripMenuItem.Size = new System.Drawing.Size(73, 34);
            this.najingNanoToolStripMenuItem.Text = "Mira";
            // 
            // ExitMenuItem
            // 
            this.ExitMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.ExitMenuItem.Image = global::csAcq4.Properties.Resources.close;
            this.ExitMenuItem.Name = "ExitMenuItem";
            this.ExitMenuItem.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.ExitMenuItem.Size = new System.Drawing.Size(36, 34);
            this.ExitMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click_1);
            // 
            // fullScreanMenuItem
            // 
            this.fullScreanMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.fullScreanMenuItem.Image = global::csAcq4.Properties.Resources.恢复窗口;
            this.fullScreanMenuItem.Name = "fullScreanMenuItem";
            this.fullScreanMenuItem.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.fullScreanMenuItem.Size = new System.Drawing.Size(36, 34);
            this.fullScreanMenuItem.Click += new System.EventHandler(this.fullScreanMenuItem_Click);
            // 
            // smallMenuItem
            // 
            this.smallMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.smallMenuItem.Image = global::csAcq4.Properties.Resources.最小化;
            this.smallMenuItem.Name = "smallMenuItem";
            this.smallMenuItem.Padding = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.smallMenuItem.Size = new System.Drawing.Size(36, 34);
            this.smallMenuItem.Click += new System.EventHandler(this.smallMenuItem_Click);
            // 
            // splitter2
            // 
            this.splitter2.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter2.Location = new System.Drawing.Point(1467, 101);
            this.splitter2.Margin = new System.Windows.Forms.Padding(0);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(3, 917);
            this.splitter2.TabIndex = 52;
            this.splitter2.TabStop = false;
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(350, 101);
            this.splitter1.Margin = new System.Windows.Forms.Padding(0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 917);
            this.splitter1.TabIndex = 51;
            this.splitter1.TabStop = false;
            // 
            // bottomLabelLeft
            // 
            this.bottomLabelLeft.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(61)))), ((int)(((byte)(62)))), ((int)(((byte)(64)))));
            this.bottomLabelLeft.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.bottomLabelLeft.ForeColor = System.Drawing.Color.White;
            this.bottomLabelLeft.Margin = new System.Windows.Forms.Padding(0);
            this.bottomLabelLeft.Name = "bottomLabelLeft";
            this.bottomLabelLeft.Size = new System.Drawing.Size(73, 22);
            this.bottomLabelLeft.Text = "X:___;Y:___;";
            // 
            // bottomLabelMiddle
            // 
            this.bottomLabelMiddle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(61)))), ((int)(((byte)(62)))), ((int)(((byte)(64)))));
            this.bottomLabelMiddle.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.bottomLabelMiddle.ForeColor = System.Drawing.Color.White;
            this.bottomLabelMiddle.Margin = new System.Windows.Forms.Padding(0);
            this.bottomLabelMiddle.Name = "bottomLabelMiddle";
            this.bottomLabelMiddle.Size = new System.Drawing.Size(1753, 22);
            this.bottomLabelMiddle.Spring = true;
            this.bottomLabelMiddle.Text = "currentFrameRate:";
            this.bottomLabelMiddle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // bottomLabelRight
            // 
            this.bottomLabelRight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(61)))), ((int)(((byte)(62)))), ((int)(((byte)(64)))));
            this.bottomLabelRight.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.bottomLabelRight.ForeColor = System.Drawing.Color.White;
            this.bottomLabelRight.Margin = new System.Windows.Forms.Padding(0);
            this.bottomLabelRight.Name = "bottomLabelRight";
            this.bottomLabelRight.Size = new System.Drawing.Size(79, 22);
            this.bottomLabelRight.Text = "WaitBegin...";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.bottomLabelLeft,
            this.bottomLabelMiddle,
            this.bottomLabelRight});
            this.statusStrip1.Location = new System.Drawing.Point(0, 1018);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1920, 22);
            this.statusStrip1.TabIndex = 17;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // Form1
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.LightGray;
            this.ClientSize = new System.Drawing.Size(1920, 1040);
            this.Controls.Add(this.middlePanel);
            this.Controls.Add(this.splitter2);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.leftPanel);
            this.Controls.Add(this.rightPanel);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.menuStrip2);
            this.ForeColor = System.Drawing.Color.White;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "csAcq4";
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.saveGroup.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.delayVal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numVal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.periodVal)).EndInit();
            this.groupBox4.ResumeLayout(false);
            this.funGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.exposureNum)).EndInit();
            this.leftPanel.ResumeLayout(false);
            this.uiTitlePanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.smallShow9)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.smallShow8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.smallShow7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.smallShow6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.smallShow5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.smallShow4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.smallShow3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.smallShow2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.smallShow1)).EndInit();
            this.histogramPanel.ResumeLayout(false);
            this.rightPanel.ResumeLayout(false);
            this.middlePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay9)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PicDisplay1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.FullDisPlay)).EndInit();
            this.menuStrip2.ResumeLayout(false);
            this.menuStrip2.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deviceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem searchCamToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem calToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem measureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem backgroundToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeBackgroundToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeAutofluorescenceToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem imageProcessingcToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem imageProcessingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fillColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem fillColorToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem lengthCalToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem plotToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem textToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem rGBColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem histogramToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem loggingToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton textToolBtn;
        private System.Windows.Forms.ToolStripButton toolPointBtn;
        private System.Windows.Forms.ToolStripButton lineDrawTool;
        private System.Windows.Forms.ToolStripButton toolCircleBtn;
        private System.Windows.Forms.ToolStripButton toolRectBtn;
        private System.Windows.Forms.ToolStripButton singleArrowTool;
        private System.Windows.Forms.ToolStripButton doubleArrowTool;
        private System.Windows.Forms.ToolStripButton positionTool;
        private System.Windows.Forms.ToolStripButton toolStripButton10;
        private System.Windows.Forms.ToolStripButton toolCalBtn;
        private System.Windows.Forms.ToolStripButton applyAllTool;
        private System.Windows.Forms.ToolStripButton cancelDrawTool;
        private System.Windows.Forms.GroupBox groupBox6;
        private Sunny.UI.UISymbolButton savePathChoose;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.GroupBox saveGroup;
        private System.Windows.Forms.GroupBox funGroup;
        private System.Windows.Forms.NumericUpDown exposureNum;
        private System.Windows.Forms.ComboBox pixelCombox;
        private System.Windows.Forms.ComboBox devName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox preName;
        private Sunny.UI.UILine uiLine1;
        private Sunny.UI.UILabel pathLabel;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown delayVal;
        private System.Windows.Forms.NumericUpDown periodVal;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown numVal;
        private System.Windows.Forms.GroupBox groupBox4;
        private ToolStripMenuItem aToolStripMenuItem;
        private ToolStripMenuItem bToolStripMenuItem;
        private ToolStripMenuItem cToolStripMenuItem;
        private Sunny.UI.UILabel uiLabel1;
        private Sunny.UI.UILabel uiLabel2;
        private Sunny.UI.UILabel uiLabel3;
        private ToolStripMenuItem loadIToolStripMenuItem;
        private ToolStripMenuItem currentImageToolStripMenuItem;
        private ToolStripButton colorToolSelec;
        private ToolStripMenuItem stopPlayingToolStripMenuItem;
        private Panel leftPanel;
        private Sunny.UI.UITitlePanel uiTitlePanel1;
        public PictureBox smallShow9;
        public PictureBox smallShow8;
        public PictureBox smallShow7;
        public PictureBox smallShow6;
        public PictureBox smallShow5;
        public PictureBox smallShow4;
        public PictureBox smallShow3;
        public PictureBox smallShow2;
        private Sunny.UI.UITitlePanel histogramPanel;
        public ComboBox histSelect;
        private Sunny.UI.UIBarChart histLine;
        private Panel rightPanel;
        private Panel middlePanel;
        private Sunny.UI.UISymbolButton RestoreBtn;
        private Sunny.UI.UISymbolButton recoveryBtn;
        private Sunny.UI.UISymbolButton narrowBtn;
        private Sunny.UI.UISymbolButton enlargeBtn;
        public PictureBox PicDisplay9;
        public PictureBox PicDisplay8;
        public PictureBox PicDisplay7;
        public PictureBox PicDisplay6;
        public PictureBox PicDisplay5;
        public PictureBox PicDisplay4;
        public PictureBox PicDisplay3;
        public PictureBox PicDisplay2;
        public PictureBox PicDisplay1;
        private PictureBox FullDisPlay;
        private MenuStrip menuStrip2;
        private ToolStripMenuItem najingNanoToolStripMenuItem;
        private ToolStripMenuItem ExitMenuItem;
        private ToolStripMenuItem fullScreanMenuItem;
        private ToolStripMenuItem smallMenuItem;
        private Splitter splitter2;
        private Splitter splitter1;
        public PictureBox smallShow1;
        private Sunny.UI.UIButton pushLive;
        private Sunny.UI.UIButton videoRecorder;
        private Sunny.UI.UIButton shootStart;
        private Sunny.UI.UIButton saveImg;
        private Sunny.UI.UIButton enhanceBtn;
        private ToolStripStatusLabel bottomLabelLeft;
        private ToolStripStatusLabel bottomLabelMiddle;
        private ToolStripStatusLabel bottomLabelRight;
        private StatusStrip statusStrip1;
        private Sunny.UI.UIScrollBar LiveBar;
    }
}