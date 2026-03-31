using System.Drawing;
using System.Windows.Forms;

namespace Anjian;

public sealed partial class AmountOcrTestForm
{
    private TableLayoutPanel rootLayout = null!;
    private TableLayoutPanel leftPanel = null!;
    private TableLayoutPanel previewPanel = null!;
    private TableLayoutPanel rightPanel = null!;
    private GroupBox grpRegion = null!;
    private GroupBox grpPreprocess = null!;
    private GroupBox grpResult = null!;
    private GroupBox grpRaw = null!;
    private GroupBox grpNormalized = null!;
    private TableLayoutPanel regionLayout = null!;
    private TableLayoutPanel preprocessLayout = null!;
    private TableLayoutPanel actionPanel = null!;
    private TableLayoutPanel resultLayout = null!;
    private TableLayoutPanel snippetActions = null!;
    private Label lblRegionX = null!;
    private Label lblRegionY = null!;
    private Label lblRegionWidth = null!;
    private Label lblRegionHeight = null!;
    private Label lblThreshold = null!;
    private Label lblHint = null!;
    private Label lblSourceTitle = null!;
    private Label lblProcessedTitle = null!;
    private Label lblSnippetSection = null!;
    private Label lblLogSection = null!;
    private Label lblResultStatus = null!;
    private Label lblResultAmount = null!;
    private Label lblResultElapsed = null!;
    private TextBox txtRegionX = null!;
    private TextBox txtRegionY = null!;
    private TextBox txtRegionWidth = null!;
    private TextBox txtRegionHeight = null!;
    private TextBox txtThreshold = null!;
    private TextBox txtRaw = null!;
    private TextBox txtNormalized = null!;
    private TextBox txtSnippet = null!;
    private TextBox txtLog = null!;
    private CheckBox chkGrayscale = null!;
    private CheckBox chkBinarization = null!;
    private CheckBox chkScale2x = null!;
    private PictureBox picSource = null!;
    private PictureBox picProcessed = null!;
    private Label lblStatusValue = null!;
    private Label lblAmountValue = null!;
    private Label lblElapsedValue = null!;
    private Button btnCaptureRegion = null!;
    private Button btnPickRegion = null!;
    private Button btnExecuteOcr = null!;
    private Button btnGenerateSnippet = null!;
    private Button btnCopySnippet = null!;
    private Button btnRegenerateSnippet = null!;

    private void InitializeComponent()
    {
        rootLayout = new TableLayoutPanel();
        leftPanel = new TableLayoutPanel();
        grpRegion = new GroupBox();
        regionLayout = new TableLayoutPanel();
        lblRegionX = new Label();
        txtRegionX = new TextBox();
        lblRegionY = new Label();
        txtRegionY = new TextBox();
        lblRegionWidth = new Label();
        txtRegionWidth = new TextBox();
        lblRegionHeight = new Label();
        txtRegionHeight = new TextBox();
        grpPreprocess = new GroupBox();
        preprocessLayout = new TableLayoutPanel();
        chkGrayscale = new CheckBox();
        chkBinarization = new CheckBox();
        chkScale2x = new CheckBox();
        lblThreshold = new Label();
        txtThreshold = new TextBox();
        actionPanel = new TableLayoutPanel();
        btnPickRegion = new Button();
        btnCaptureRegion = new Button();
        btnExecuteOcr = new Button();
        btnGenerateSnippet = new Button();
        lblHint = new Label();
        previewPanel = new TableLayoutPanel();
        lblSourceTitle = new Label();
        picSource = new PictureBox();
        lblProcessedTitle = new Label();
        picProcessed = new PictureBox();
        rightPanel = new TableLayoutPanel();
        grpResult = new GroupBox();
        resultLayout = new TableLayoutPanel();
        lblResultStatus = new Label();
        lblStatusValue = new Label();
        lblResultAmount = new Label();
        lblAmountValue = new Label();
        lblResultElapsed = new Label();
        lblElapsedValue = new Label();
        lblSnippetSection = new Label();
        snippetActions = new TableLayoutPanel();
        btnRegenerateSnippet = new Button();
        btnCopySnippet = new Button();
        txtSnippet = new TextBox();
        grpRaw = new GroupBox();
        txtRaw = new TextBox();
        grpNormalized = new GroupBox();
        txtNormalized = new TextBox();
        lblLogSection = new Label();
        txtLog = new TextBox();
        rootLayout.SuspendLayout();
        leftPanel.SuspendLayout();
        grpRegion.SuspendLayout();
        regionLayout.SuspendLayout();
        grpPreprocess.SuspendLayout();
        preprocessLayout.SuspendLayout();
        actionPanel.SuspendLayout();
        previewPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picSource).BeginInit();
        ((System.ComponentModel.ISupportInitialize)picProcessed).BeginInit();
        rightPanel.SuspendLayout();
        grpResult.SuspendLayout();
        resultLayout.SuspendLayout();
        snippetActions.SuspendLayout();
        grpRaw.SuspendLayout();
        grpNormalized.SuspendLayout();
        SuspendLayout();
        // 
        // rootLayout
        // 
        rootLayout.ColumnCount = 3;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 330F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360F));
        rootLayout.Controls.Add(leftPanel, 0, 0);
        rootLayout.Controls.Add(previewPanel, 1, 0);
        rootLayout.Controls.Add(rightPanel, 2, 0);
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.Location = new Point(0, 0);
        rootLayout.Name = "rootLayout";
        rootLayout.Padding = new Padding(12);
        rootLayout.RowCount = 1;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.Size = new Size(1540, 920);
        rootLayout.TabIndex = 0;
        // 
        // leftPanel
        // 
        leftPanel.ColumnCount = 1;
        leftPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        leftPanel.Controls.Add(grpRegion, 0, 0);
        leftPanel.Controls.Add(grpPreprocess, 0, 1);
        leftPanel.Controls.Add(actionPanel, 0, 2);
        leftPanel.Controls.Add(lblHint, 0, 3);
        leftPanel.Dock = DockStyle.Fill;
        leftPanel.Location = new Point(15, 15);
        leftPanel.Name = "leftPanel";
        leftPanel.RowCount = 4;
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 220F));
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 200F));
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 130F));
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        leftPanel.Size = new Size(324, 890);
        leftPanel.TabIndex = 0;
        // 
        // grpRegion
        // 
        grpRegion.Controls.Add(regionLayout);
        grpRegion.Dock = DockStyle.Fill;
        grpRegion.Location = new Point(3, 3);
        grpRegion.Name = "grpRegion";
        grpRegion.Padding = new Padding(10);
        grpRegion.Size = new Size(318, 214);
        grpRegion.TabIndex = 0;
        grpRegion.TabStop = false;
        grpRegion.Text = "识别区域";
        // 
        // regionLayout
        // 
        regionLayout.ColumnCount = 2;
        regionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
        regionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        regionLayout.Controls.Add(lblRegionX, 0, 0);
        regionLayout.Controls.Add(txtRegionX, 1, 0);
        regionLayout.Controls.Add(lblRegionY, 0, 1);
        regionLayout.Controls.Add(txtRegionY, 1, 1);
        regionLayout.Controls.Add(lblRegionWidth, 0, 2);
        regionLayout.Controls.Add(txtRegionWidth, 1, 2);
        regionLayout.Controls.Add(lblRegionHeight, 0, 3);
        regionLayout.Controls.Add(txtRegionHeight, 1, 3);
        regionLayout.Controls.Add(btnPickRegion, 0, 4);
        regionLayout.Dock = DockStyle.Fill;
        regionLayout.Location = new Point(10, 26);
        regionLayout.Name = "regionLayout";
        regionLayout.RowCount = 5;
        regionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        regionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        regionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        regionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        regionLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        regionLayout.Size = new Size(298, 138);
        regionLayout.TabIndex = 0;
        // 
        // lblRegionX
        // 
        lblRegionX.Dock = DockStyle.Fill;
        lblRegionX.Location = new Point(3, 0);
        lblRegionX.Name = "lblRegionX";
        lblRegionX.Size = new Size(90, 34);
        lblRegionX.TabIndex = 0;
        lblRegionX.Text = "X";
        // 
        // txtRegionX
        // 
        txtRegionX.Dock = DockStyle.Fill;
        txtRegionX.Location = new Point(99, 3);
        txtRegionX.Name = "txtRegionX";
        txtRegionX.Size = new Size(196, 23);
        txtRegionX.TabIndex = 1;
        // 
        // lblRegionY
        // 
        lblRegionY.Dock = DockStyle.Fill;
        lblRegionY.Location = new Point(3, 34);
        lblRegionY.Name = "lblRegionY";
        lblRegionY.Size = new Size(90, 34);
        lblRegionY.TabIndex = 2;
        lblRegionY.Text = "Y";
        // 
        // txtRegionY
        // 
        txtRegionY.Dock = DockStyle.Fill;
        txtRegionY.Location = new Point(99, 37);
        txtRegionY.Name = "txtRegionY";
        txtRegionY.Size = new Size(196, 23);
        txtRegionY.TabIndex = 3;
        // 
        // lblRegionWidth
        // 
        lblRegionWidth.Dock = DockStyle.Fill;
        lblRegionWidth.Location = new Point(3, 68);
        lblRegionWidth.Name = "lblRegionWidth";
        lblRegionWidth.Size = new Size(90, 34);
        lblRegionWidth.TabIndex = 4;
        lblRegionWidth.Text = "Width";
        // 
        // txtRegionWidth
        // 
        txtRegionWidth.Dock = DockStyle.Fill;
        txtRegionWidth.Location = new Point(99, 71);
        txtRegionWidth.Name = "txtRegionWidth";
        txtRegionWidth.Size = new Size(196, 23);
        txtRegionWidth.TabIndex = 5;
        // 
        // lblRegionHeight
        // 
        lblRegionHeight.Dock = DockStyle.Fill;
        lblRegionHeight.Location = new Point(3, 102);
        lblRegionHeight.Name = "lblRegionHeight";
        lblRegionHeight.Size = new Size(90, 36);
        lblRegionHeight.TabIndex = 6;
        lblRegionHeight.Text = "Height";
        // 
        // txtRegionHeight
        // 
        txtRegionHeight.Dock = DockStyle.Fill;
        txtRegionHeight.Location = new Point(99, 105);
        txtRegionHeight.Name = "txtRegionHeight";
        txtRegionHeight.Size = new Size(196, 23);
        // 
        // btnPickRegion
        // 
        btnPickRegion.Dock = DockStyle.Fill;
        btnPickRegion.Location = new Point(3, 139);
        btnPickRegion.Name = "btnPickRegion";
        btnPickRegion.Size = new Size(292, 34);
        btnPickRegion.TabIndex = 8;
        btnPickRegion.Text = "选取区域";
        btnPickRegion.Click += btnPickRegion_Click;
        regionLayout.SetColumnSpan(btnPickRegion, 2);
        txtRegionHeight.TabIndex = 7;
        // 
        // grpPreprocess
        // 
        grpPreprocess.Controls.Add(preprocessLayout);
        grpPreprocess.Dock = DockStyle.Fill;
        grpPreprocess.Location = new Point(3, 183);
        grpPreprocess.Name = "grpPreprocess";
        grpPreprocess.Padding = new Padding(10);
        grpPreprocess.Size = new Size(318, 194);
        grpPreprocess.TabIndex = 1;
        grpPreprocess.TabStop = false;
        grpPreprocess.Text = "预处理参数";
        // 
        // preprocessLayout
        // 
        preprocessLayout.ColumnCount = 2;
        preprocessLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
        preprocessLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        preprocessLayout.Controls.Add(chkGrayscale, 1, 0);
        preprocessLayout.Controls.Add(chkBinarization, 1, 1);
        preprocessLayout.Controls.Add(chkScale2x, 1, 2);
        preprocessLayout.Controls.Add(lblThreshold, 0, 3);
        preprocessLayout.Controls.Add(txtThreshold, 1, 3);
        preprocessLayout.Dock = DockStyle.Fill;
        preprocessLayout.Location = new Point(10, 26);
        preprocessLayout.Name = "preprocessLayout";
        preprocessLayout.RowCount = 4;
        preprocessLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        preprocessLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        preprocessLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        preprocessLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        preprocessLayout.Size = new Size(298, 158);
        preprocessLayout.TabIndex = 0;
        // 
        // chkGrayscale
        // 
        chkGrayscale.Checked = true;
        chkGrayscale.CheckState = CheckState.Checked;
        chkGrayscale.Dock = DockStyle.Fill;
        chkGrayscale.Location = new Point(99, 3);
        chkGrayscale.Name = "chkGrayscale";
        chkGrayscale.Size = new Size(196, 28);
        chkGrayscale.TabIndex = 0;
        chkGrayscale.Text = "启用灰度";
        // 
        // chkBinarization
        // 
        chkBinarization.Dock = DockStyle.Fill;
        chkBinarization.Location = new Point(99, 37);
        chkBinarization.Name = "chkBinarization";
        chkBinarization.Size = new Size(196, 28);
        chkBinarization.TabIndex = 1;
        chkBinarization.Text = "启用二值化";
        // 
        // chkScale2x
        // 
        chkScale2x.Dock = DockStyle.Fill;
        chkScale2x.Location = new Point(99, 71);
        chkScale2x.Name = "chkScale2x";
        chkScale2x.Size = new Size(196, 28);
        chkScale2x.TabIndex = 2;
        chkScale2x.Text = "启用放大 2x";
        // 
        // lblThreshold
        // 
        lblThreshold.Dock = DockStyle.Fill;
        lblThreshold.Location = new Point(3, 102);
        lblThreshold.Name = "lblThreshold";
        lblThreshold.Size = new Size(90, 56);
        lblThreshold.TabIndex = 3;
        lblThreshold.Text = "二值阈值";
        // 
        // txtThreshold
        // 
        txtThreshold.Dock = DockStyle.Fill;
        txtThreshold.Location = new Point(99, 105);
        txtThreshold.Name = "txtThreshold";
        txtThreshold.Size = new Size(196, 23);
        txtThreshold.TabIndex = 4;
        txtThreshold.Text = "160";
        // 
        // actionPanel
        // 
        actionPanel.ColumnCount = 1;
        actionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        actionPanel.Controls.Add(btnCaptureRegion, 0, 0);
        actionPanel.Controls.Add(btnExecuteOcr, 0, 1);
        actionPanel.Controls.Add(btnGenerateSnippet, 0, 2);
        actionPanel.Dock = DockStyle.Fill;
        actionPanel.Location = new Point(3, 383);
        actionPanel.Name = "actionPanel";
        actionPanel.RowCount = 3;
        actionPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        actionPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        actionPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        actionPanel.Size = new Size(318, 124);
        actionPanel.TabIndex = 2;
        // 
        // btnCaptureRegion
        // 
        btnCaptureRegion.Dock = DockStyle.Fill;
        btnCaptureRegion.Location = new Point(3, 3);
        btnCaptureRegion.Name = "btnCaptureRegion";
        btnCaptureRegion.Size = new Size(312, 35);
        btnCaptureRegion.TabIndex = 0;
        btnCaptureRegion.Text = "截图测试区域";
        btnCaptureRegion.Click += btnCaptureRegion_Click;
        // 
        // btnExecuteOcr
        // 
        btnExecuteOcr.Dock = DockStyle.Fill;
        btnExecuteOcr.Location = new Point(3, 44);
        btnExecuteOcr.Name = "btnExecuteOcr";
        btnExecuteOcr.Size = new Size(312, 35);
        btnExecuteOcr.TabIndex = 1;
        btnExecuteOcr.Text = "执行金额识别";
        btnExecuteOcr.Click += btnExecuteOcr_Click;
        // 
        // btnGenerateSnippet
        // 
        btnGenerateSnippet.Dock = DockStyle.Fill;
        btnGenerateSnippet.Location = new Point(3, 85);
        btnGenerateSnippet.Name = "btnGenerateSnippet";
        btnGenerateSnippet.Size = new Size(312, 36);
        btnGenerateSnippet.TabIndex = 2;
        btnGenerateSnippet.Text = "生成代码";
        btnGenerateSnippet.Click += btnGenerateSnippet_Click;
        // 
        // lblHint
        // 
        lblHint.AutoSize = true;
        lblHint.Dock = DockStyle.Fill;
        lblHint.ForeColor = Color.DimGray;
        lblHint.Location = new Point(3, 510);
        lblHint.Name = "lblHint";
        lblHint.Size = new Size(318, 380);
        lblHint.TabIndex = 3;
        lblHint.Text = "金额识别优先用固定区域。默认建议灰度开启、二值化按需开启、2x 放大默认关闭。调好参数后可直接生成 C# 调用代码。";
        // 
        // previewPanel
        // 
        previewPanel.ColumnCount = 1;
        previewPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        previewPanel.Controls.Add(lblSourceTitle, 0, 0);
        previewPanel.Controls.Add(picSource, 0, 1);
        previewPanel.Controls.Add(lblProcessedTitle, 0, 2);
        previewPanel.Controls.Add(picProcessed, 0, 3);
        previewPanel.Dock = DockStyle.Fill;
        previewPanel.Location = new Point(345, 15);
        previewPanel.Name = "previewPanel";
        previewPanel.RowCount = 4;
        previewPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        previewPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        previewPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        previewPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        previewPanel.Size = new Size(820, 890);
        previewPanel.TabIndex = 1;
        // 
        // lblSourceTitle
        // 
        lblSourceTitle.AutoSize = true;
        lblSourceTitle.Dock = DockStyle.Top;
        lblSourceTitle.Location = new Point(3, 0);
        lblSourceTitle.Name = "lblSourceTitle";
        lblSourceTitle.Padding = new Padding(0, 0, 0, 6);
        lblSourceTitle.Size = new Size(814, 23);
        lblSourceTitle.TabIndex = 0;
        lblSourceTitle.Text = "原始截图";
        // 
        // picSource
        // 
        picSource.BackColor = Color.White;
        picSource.BorderStyle = BorderStyle.FixedSingle;
        picSource.Dock = DockStyle.Fill;
        picSource.Location = new Point(3, 31);
        picSource.Name = "picSource";
        picSource.Size = new Size(814, 411);
        picSource.SizeMode = PictureBoxSizeMode.Zoom;
        picSource.TabIndex = 1;
        picSource.TabStop = false;
        // 
        // lblProcessedTitle
        // 
        lblProcessedTitle.AutoSize = true;
        lblProcessedTitle.Dock = DockStyle.Top;
        lblProcessedTitle.Location = new Point(3, 445);
        lblProcessedTitle.Name = "lblProcessedTitle";
        lblProcessedTitle.Padding = new Padding(0, 0, 0, 6);
        lblProcessedTitle.Size = new Size(814, 23);
        lblProcessedTitle.TabIndex = 2;
        lblProcessedTitle.Text = "预处理结果";
        // 
        // picProcessed
        // 
        picProcessed.BackColor = Color.White;
        picProcessed.BorderStyle = BorderStyle.FixedSingle;
        picProcessed.Dock = DockStyle.Fill;
        picProcessed.Location = new Point(3, 476);
        picProcessed.Name = "picProcessed";
        picProcessed.Size = new Size(814, 411);
        picProcessed.SizeMode = PictureBoxSizeMode.Zoom;
        picProcessed.TabIndex = 3;
        picProcessed.TabStop = false;
        // 
        // rightPanel
        // 
        rightPanel.ColumnCount = 1;
        rightPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rightPanel.Controls.Add(grpResult, 0, 0);
        rightPanel.Controls.Add(lblSnippetSection, 0, 1);
        rightPanel.Controls.Add(snippetActions, 0, 2);
        rightPanel.Controls.Add(txtSnippet, 0, 3);
        rightPanel.Controls.Add(grpRaw, 0, 4);
        rightPanel.Controls.Add(grpNormalized, 0, 5);
        rightPanel.Controls.Add(lblLogSection, 0, 6);
        rightPanel.Controls.Add(txtLog, 0, 7);
        rightPanel.Dock = DockStyle.Fill;
        rightPanel.Location = new Point(1171, 15);
        rightPanel.Name = "rightPanel";
        rightPanel.RowCount = 8;
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 140F));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 22F));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 18F));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 18F));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
        rightPanel.Size = new Size(354, 890);
        rightPanel.TabIndex = 2;
        // 
        // grpResult
        // 
        grpResult.Controls.Add(resultLayout);
        grpResult.Dock = DockStyle.Fill;
        grpResult.Location = new Point(3, 3);
        grpResult.Name = "grpResult";
        grpResult.Padding = new Padding(10);
        grpResult.Size = new Size(348, 134);
        grpResult.TabIndex = 0;
        grpResult.TabStop = false;
        grpResult.Text = "识别结果";
        // 
        // resultLayout
        // 
        resultLayout.ColumnCount = 2;
        resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
        resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        resultLayout.Controls.Add(lblResultStatus, 0, 0);
        resultLayout.Controls.Add(lblStatusValue, 1, 0);
        resultLayout.Controls.Add(lblResultAmount, 0, 1);
        resultLayout.Controls.Add(lblAmountValue, 1, 1);
        resultLayout.Controls.Add(lblResultElapsed, 0, 2);
        resultLayout.Controls.Add(lblElapsedValue, 1, 2);
        resultLayout.Dock = DockStyle.Fill;
        resultLayout.Location = new Point(10, 26);
        resultLayout.Name = "resultLayout";
        resultLayout.RowCount = 3;
        resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        resultLayout.Size = new Size(328, 98);
        resultLayout.TabIndex = 0;
        // 
        // lblResultStatus
        // 
        lblResultStatus.Dock = DockStyle.Fill;
        lblResultStatus.Location = new Point(3, 0);
        lblResultStatus.Name = "lblResultStatus";
        lblResultStatus.Size = new Size(90, 32);
        lblResultStatus.TabIndex = 0;
        lblResultStatus.Text = "状态";
        // 
        // lblStatusValue
        // 
        lblStatusValue.Dock = DockStyle.Fill;
        lblStatusValue.Location = new Point(99, 0);
        lblStatusValue.Name = "lblStatusValue";
        lblStatusValue.Size = new Size(226, 32);
        lblStatusValue.TabIndex = 1;
        lblStatusValue.Text = "-";
        // 
        // lblResultAmount
        // 
        lblResultAmount.Dock = DockStyle.Fill;
        lblResultAmount.Location = new Point(3, 32);
        lblResultAmount.Name = "lblResultAmount";
        lblResultAmount.Size = new Size(90, 32);
        lblResultAmount.TabIndex = 2;
        lblResultAmount.Text = "最终金额";
        // 
        // lblAmountValue
        // 
        lblAmountValue.Dock = DockStyle.Fill;
        lblAmountValue.Location = new Point(99, 32);
        lblAmountValue.Name = "lblAmountValue";
        lblAmountValue.Size = new Size(226, 32);
        lblAmountValue.TabIndex = 3;
        lblAmountValue.Text = "-";
        // 
        // lblResultElapsed
        // 
        lblResultElapsed.Dock = DockStyle.Fill;
        lblResultElapsed.Location = new Point(3, 64);
        lblResultElapsed.Name = "lblResultElapsed";
        lblResultElapsed.Size = new Size(90, 34);
        lblResultElapsed.TabIndex = 4;
        lblResultElapsed.Text = "耗时(ms)";
        // 
        // lblElapsedValue
        // 
        lblElapsedValue.Dock = DockStyle.Fill;
        lblElapsedValue.Location = new Point(99, 64);
        lblElapsedValue.Name = "lblElapsedValue";
        lblElapsedValue.Size = new Size(226, 34);
        lblElapsedValue.TabIndex = 5;
        lblElapsedValue.Text = "-";
        // 
        // lblSnippetSection
        // 
        lblSnippetSection.AutoSize = true;
        lblSnippetSection.Dock = DockStyle.Top;
        lblSnippetSection.Location = new Point(3, 140);
        lblSnippetSection.Name = "lblSnippetSection";
        lblSnippetSection.Padding = new Padding(0, 0, 0, 6);
        lblSnippetSection.Size = new Size(348, 23);
        lblSnippetSection.TabIndex = 1;
        lblSnippetSection.Text = "C# 调用代码";
        // 
        // snippetActions
        // 
        snippetActions.ColumnCount = 2;
        snippetActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        snippetActions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        snippetActions.Controls.Add(btnRegenerateSnippet, 0, 0);
        snippetActions.Controls.Add(btnCopySnippet, 1, 0);
        snippetActions.Dock = DockStyle.Fill;
        snippetActions.Location = new Point(3, 171);
        snippetActions.Name = "snippetActions";
        snippetActions.RowStyles.Add(new RowStyle(SizeType.Absolute, 20F));
        snippetActions.Size = new Size(348, 34);
        snippetActions.TabIndex = 2;
        // 
        // btnRegenerateSnippet
        // 
        btnRegenerateSnippet.Dock = DockStyle.Fill;
        btnRegenerateSnippet.Location = new Point(3, 3);
        btnRegenerateSnippet.Name = "btnRegenerateSnippet";
        btnRegenerateSnippet.Size = new Size(168, 28);
        btnRegenerateSnippet.TabIndex = 0;
        btnRegenerateSnippet.Text = "重新生成";
        btnRegenerateSnippet.Click += btnGenerateSnippet_Click;
        // 
        // btnCopySnippet
        // 
        btnCopySnippet.Dock = DockStyle.Fill;
        btnCopySnippet.Location = new Point(177, 3);
        btnCopySnippet.Name = "btnCopySnippet";
        btnCopySnippet.Size = new Size(168, 28);
        btnCopySnippet.TabIndex = 1;
        btnCopySnippet.Text = "复制代码";
        btnCopySnippet.Click += btnCopySnippet_Click;
        // 
        // txtSnippet
        // 
        txtSnippet.Dock = DockStyle.Fill;
        txtSnippet.Font = new Font("Consolas", 10.5F);
        txtSnippet.Location = new Point(3, 211);
        txtSnippet.Multiline = true;
        txtSnippet.Name = "txtSnippet";
        txtSnippet.ReadOnly = true;
        txtSnippet.ScrollBars = ScrollBars.Both;
        txtSnippet.Size = new Size(348, 137);
        txtSnippet.TabIndex = 3;
        txtSnippet.WordWrap = false;
        // 
        // grpRaw
        // 
        grpRaw.Controls.Add(txtRaw);
        grpRaw.Dock = DockStyle.Fill;
        grpRaw.Location = new Point(3, 354);
        grpRaw.Name = "grpRaw";
        grpRaw.Padding = new Padding(10);
        grpRaw.Size = new Size(348, 111);
        grpRaw.TabIndex = 4;
        grpRaw.TabStop = false;
        grpRaw.Text = "OCR 原文";
        // 
        // txtRaw
        // 
        txtRaw.Dock = DockStyle.Fill;
        txtRaw.Font = new Font("Consolas", 10.5F);
        txtRaw.Location = new Point(10, 26);
        txtRaw.Multiline = true;
        txtRaw.Name = "txtRaw";
        txtRaw.ReadOnly = true;
        txtRaw.ScrollBars = ScrollBars.Both;
        txtRaw.Size = new Size(328, 75);
        txtRaw.TabIndex = 0;
        txtRaw.WordWrap = false;
        // 
        // grpNormalized
        // 
        grpNormalized.Controls.Add(txtNormalized);
        grpNormalized.Dock = DockStyle.Fill;
        grpNormalized.Location = new Point(3, 471);
        grpNormalized.Name = "grpNormalized";
        grpNormalized.Padding = new Padding(10);
        grpNormalized.Size = new Size(348, 111);
        grpNormalized.TabIndex = 5;
        grpNormalized.TabStop = false;
        grpNormalized.Text = "清洗后文本";
        // 
        // txtNormalized
        // 
        txtNormalized.Dock = DockStyle.Fill;
        txtNormalized.Font = new Font("Consolas", 10.5F);
        txtNormalized.Location = new Point(10, 26);
        txtNormalized.Multiline = true;
        txtNormalized.Name = "txtNormalized";
        txtNormalized.ReadOnly = true;
        txtNormalized.ScrollBars = ScrollBars.Both;
        txtNormalized.Size = new Size(328, 75);
        txtNormalized.TabIndex = 0;
        txtNormalized.WordWrap = false;
        // 
        // lblLogSection
        // 
        lblLogSection.AutoSize = true;
        lblLogSection.Dock = DockStyle.Top;
        lblLogSection.Location = new Point(3, 585);
        lblLogSection.Name = "lblLogSection";
        lblLogSection.Padding = new Padding(0, 0, 0, 6);
        lblLogSection.Size = new Size(348, 23);
        lblLogSection.TabIndex = 6;
        lblLogSection.Text = "调试日志";
        // 
        // txtLog
        // 
        txtLog.Dock = DockStyle.Fill;
        txtLog.Font = new Font("Consolas", 10.5F);
        txtLog.Location = new Point(3, 616);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Both;
        txtLog.Size = new Size(348, 271);
        txtLog.TabIndex = 7;
        txtLog.WordWrap = false;
        // 
        // AmountOcrTestForm
        // 
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1540, 920);
        Controls.Add(rootLayout);
        MinimumSize = new Size(1360, 820);
        Name = "AmountOcrTestForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "金额识别测试";
        rootLayout.ResumeLayout(false);
        leftPanel.ResumeLayout(false);
        leftPanel.PerformLayout();
        grpRegion.ResumeLayout(false);
        regionLayout.ResumeLayout(false);
        regionLayout.PerformLayout();
        grpPreprocess.ResumeLayout(false);
        preprocessLayout.ResumeLayout(false);
        preprocessLayout.PerformLayout();
        actionPanel.ResumeLayout(false);
        previewPanel.ResumeLayout(false);
        previewPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)picSource).EndInit();
        ((System.ComponentModel.ISupportInitialize)picProcessed).EndInit();
        rightPanel.ResumeLayout(false);
        rightPanel.PerformLayout();
        grpResult.ResumeLayout(false);
        resultLayout.ResumeLayout(false);
        snippetActions.ResumeLayout(false);
        grpRaw.ResumeLayout(false);
        grpRaw.PerformLayout();
        grpNormalized.ResumeLayout(false);
        grpNormalized.PerformLayout();
        ResumeLayout(false);
    }
}
