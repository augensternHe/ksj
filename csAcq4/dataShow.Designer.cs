namespace csAcq4
{
    partial class dataShow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.imgList = new System.Windows.Forms.ImageList(this.components);
            this.roiCheck = new Sunny.UI.UICheckBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.picSaveTool = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageJPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showInImageJToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.chartSaveTool = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.execPy = new System.Windows.Forms.ToolStripMenuItem();
            this.roiLineChart = new Sunny.UI.UILineChart();
            this.picScroller = new Sunny.UI.UIScrollBar();
            this.smoothCheck = new Sunny.UI.UICheckBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.picShow = new System.Windows.Forms.PictureBox();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picShow)).BeginInit();
            this.SuspendLayout();
            // 
            // imgList
            // 
            this.imgList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imgList.ImageSize = new System.Drawing.Size(16, 16);
            this.imgList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // roiCheck
            // 
            this.roiCheck.Cursor = System.Windows.Forms.Cursors.Hand;
            this.roiCheck.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.roiCheck.ForeColor = System.Drawing.Color.White;
            this.roiCheck.Location = new System.Drawing.Point(64, 316);
            this.roiCheck.MinimumSize = new System.Drawing.Size(1, 1);
            this.roiCheck.Name = "roiCheck";
            this.roiCheck.Size = new System.Drawing.Size(80, 29);
            this.roiCheck.TabIndex = 4;
            this.roiCheck.Text = "ROI";
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(71)))), ((int)(((byte)(71)))), ((int)(((byte)(71)))));
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.picSaveTool,
            this.chartSaveTool,
            this.execPy});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(913, 28);
            this.menuStrip1.TabIndex = 6;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // picSaveTool
            // 
            this.picSaveTool.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.imageJPathToolStripMenuItem,
            this.showInImageJToolStripMenuItem});
            this.picSaveTool.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.picSaveTool.ForeColor = System.Drawing.Color.White;
            this.picSaveTool.Name = "picSaveTool";
            this.picSaveTool.Size = new System.Drawing.Size(66, 24);
            this.picSaveTool.Text = "Picture";
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(153, 28);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(153, 28);
            this.saveAsToolStripMenuItem.Text = "Save as";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // imageJPathToolStripMenuItem
            // 
            this.imageJPathToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.imageJPathToolStripMenuItem.Name = "imageJPathToolStripMenuItem";
            this.imageJPathToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.imageJPathToolStripMenuItem.Size = new System.Drawing.Size(153, 28);
            this.imageJPathToolStripMenuItem.Text = "ImageJ Path";
            this.imageJPathToolStripMenuItem.Click += new System.EventHandler(this.imageJPathToolStripMenuItem_Click);
            // 
            // showInImageJToolStripMenuItem
            // 
            this.showInImageJToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.showInImageJToolStripMenuItem.Name = "showInImageJToolStripMenuItem";
            this.showInImageJToolStripMenuItem.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.showInImageJToolStripMenuItem.Size = new System.Drawing.Size(153, 28);
            this.showInImageJToolStripMenuItem.Text = "ImageJ";
            this.showInImageJToolStripMenuItem.Click += new System.EventHandler(this.showInImageJToolStripMenuItem_Click);
            // 
            // chartSaveTool
            // 
            this.chartSaveTool.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem1,
            this.saveAsToolStripMenuItem1});
            this.chartSaveTool.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.chartSaveTool.ForeColor = System.Drawing.Color.White;
            this.chartSaveTool.Name = "chartSaveTool";
            this.chartSaveTool.Size = new System.Drawing.Size(56, 24);
            this.chartSaveTool.Text = "Chart";
            // 
            // saveToolStripMenuItem1
            // 
            this.saveToolStripMenuItem1.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.saveToolStripMenuItem1.Name = "saveToolStripMenuItem1";
            this.saveToolStripMenuItem1.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.saveToolStripMenuItem1.Size = new System.Drawing.Size(123, 28);
            this.saveToolStripMenuItem1.Text = "Save";
            this.saveToolStripMenuItem1.Click += new System.EventHandler(this.chartSaveToolClick);
            // 
            // saveAsToolStripMenuItem1
            // 
            this.saveAsToolStripMenuItem1.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.saveAsToolStripMenuItem1.Name = "saveAsToolStripMenuItem1";
            this.saveAsToolStripMenuItem1.Padding = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.saveAsToolStripMenuItem1.Size = new System.Drawing.Size(123, 28);
            this.saveAsToolStripMenuItem1.Text = "Save as";
            this.saveAsToolStripMenuItem1.Click += new System.EventHandler(this.saveAsToolStripMenuItem1_Click);
            // 
            // execPy
            // 
            this.execPy.Font = new System.Drawing.Font("Segoe UI", 11F);
            this.execPy.ForeColor = System.Drawing.Color.White;
            this.execPy.Name = "execPy";
            this.execPy.Size = new System.Drawing.Size(66, 24);
            this.execPy.Text = "Python";
            this.execPy.Click += new System.EventHandler(this.execPy_Click);
            // 
            // roiLineChart
            // 
            this.roiLineChart.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(45)))), ((int)(((byte)(45)))), ((int)(((byte)(45)))));
            this.roiLineChart.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.roiLineChart.ForeColor = System.Drawing.Color.White;
            this.roiLineChart.LegendFont = new System.Drawing.Font("Segoe UI", 9F);
            this.roiLineChart.Location = new System.Drawing.Point(347, 49);
            this.roiLineChart.MinimumSize = new System.Drawing.Size(1, 1);
            this.roiLineChart.MouseDownType = Sunny.UI.UILineChartMouseDownType.Zoom;
            this.roiLineChart.Name = "roiLineChart";
            this.roiLineChart.Size = new System.Drawing.Size(554, 436);
            this.roiLineChart.SubFont = new System.Drawing.Font("Segoe UI", 9F);
            this.roiLineChart.TabIndex = 7;
            this.roiLineChart.Text = "uiLineChart1";
            // 
            // picScroller
            // 
            this.picScroller.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.picScroller.FillColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.picScroller.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.picScroller.Location = new System.Drawing.Point(297, 49);
            this.picScroller.MinimumSize = new System.Drawing.Size(1, 1);
            this.picScroller.Name = "picScroller";
            this.picScroller.Size = new System.Drawing.Size(23, 245);
            this.picScroller.TabIndex = 8;
            this.picScroller.Text = "uiScrollBar1";
            // 
            // smoothCheck
            // 
            this.smoothCheck.Cursor = System.Windows.Forms.Cursors.Hand;
            this.smoothCheck.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.smoothCheck.ForeColor = System.Drawing.Color.White;
            this.smoothCheck.Location = new System.Drawing.Point(64, 361);
            this.smoothCheck.MinimumSize = new System.Drawing.Size(1, 1);
            this.smoothCheck.Name = "smoothCheck";
            this.smoothCheck.Size = new System.Drawing.Size(80, 29);
            this.smoothCheck.TabIndex = 9;
            this.smoothCheck.Text = "Smooth";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.numericUpDown1.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.numericUpDown1.ForeColor = System.Drawing.Color.White;
            this.numericUpDown1.Location = new System.Drawing.Point(150, 365);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(95, 25);
            this.numericUpDown1.TabIndex = 11;
            this.numericUpDown1.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // picShow
            // 
            this.picShow.Location = new System.Drawing.Point(40, 49);
            this.picShow.Name = "picShow";
            this.picShow.Size = new System.Drawing.Size(255, 245);
            this.picShow.TabIndex = 0;
            this.picShow.TabStop = false;
            // 
            // dataShow
            // 
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(40)))), ((int)(((byte)(40)))), ((int)(((byte)(40)))));
            this.ClientSize = new System.Drawing.Size(913, 516);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.smoothCheck);
            this.Controls.Add(this.picScroller);
            this.Controls.Add(this.roiLineChart);
            this.Controls.Add(this.roiCheck);
            this.Controls.Add(this.picShow);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "dataShow";
            this.Load += new System.EventHandler(this.dataShow_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picShow)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.PictureBox picShow;
        private System.Windows.Forms.ImageList imgList;
        private Sunny.UI.UICheckBox roiCheck;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem picSaveTool;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem chartSaveTool;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem1;
        private Sunny.UI.UILineChart roiLineChart;
        private Sunny.UI.UIScrollBar picScroller;
        private System.Windows.Forms.ToolStripMenuItem showInImageJToolStripMenuItem;
        private Sunny.UI.UICheckBox smoothCheck;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.ToolStripMenuItem imageJPathToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem execPy;
    }
}