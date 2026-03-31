using System.Drawing;
using System.Windows.Forms;

namespace Anjian;

public sealed partial class ImageMatchTestForm
{
    private TableLayoutPanel rootLayout = null!;
    private TableLayoutPanel leftPanel = null!;
    private TableLayoutPanel previewPanel = null!;
    private TableLayoutPanel rightPanel = null!;
    private GroupBox grpRegion = null!;
    private GroupBox grpMatcher = null!;
    private GroupBox grpTemplate = null!;
    private GroupBox grpResult = null!;
    private TableLayoutPanel regionLayout = null!;
    private TableLayoutPanel matcherLayout = null!;
    private TableLayoutPanel templateLayout = null!;
    private TableLayoutPanel actionPanel = null!;
    private TableLayoutPanel resultLayout = null!;
    private TableLayoutPanel snippetActions = null!;
    private Label lblRegionX = null!;
    private Label lblRegionY = null!;
    private Label lblRegionWidth = null!;
    private Label lblRegionHeight = null!;
    private Label lblThreshold = null!;
    private Label lblStep = null!;
    private Label lblMatchMode = null!;
    private Label lblScanDirection = null!;
    private Label lblTemplatePath = null!;
    private Label lblHint = null!;
    private Label lblSourceTitle = null!;
    private Label lblTemplateTitle = null!;
    private Label lblSnippetSection = null!;
    private Label lblLogSection = null!;
    private Label lblResultStatus = null!;
    private Label lblResultLocation = null!;
    private Label lblResultScore = null!;
    private Label lblResultElapsed = null!;
    private Label lblResultCount = null!;
    private TextBox txtRegionX = null!;
    private TextBox txtRegionY = null!;
    private TextBox txtRegionWidth = null!;
    private TextBox txtRegionHeight = null!;
    private TextBox txtThreshold = null!;
    private TextBox txtStep = null!;
    private TextBox txtTemplatePath = null!;
    private TextBox txtSnippet = null!;
    private TextBox txtLog = null!;
    private ComboBox cboMatchMode = null!;
    private ComboBox cboScanDirection = null!;
    private CheckBox chkGrayscale = null!;
    private PictureBox picSource = null!;
    private PictureBox picTemplate = null!;
    private Label lblStatusValue = null!;
    private Label lblLocationValue = null!;
    private Label lblScoreValue = null!;
    private Label lblElapsedValue = null!;
    private Label lblCountValue = null!;
    private Button btnSelectTemplate = null!;
    private Button btnCaptureRegion = null!;
    private Button btnPickRegion = null!;
    private Button btnExecuteMatch = null!;
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
        grpMatcher = new GroupBox();
        matcherLayout = new TableLayoutPanel();
        lblThreshold = new Label();
        txtThreshold = new TextBox();
        lblStep = new Label();
        txtStep = new TextBox();
        lblMatchMode = new Label();
        cboMatchMode = new ComboBox();
        lblScanDirection = new Label();
        cboScanDirection = new ComboBox();
        chkGrayscale = new CheckBox();
        grpTemplate = new GroupBox();
        templateLayout = new TableLayoutPanel();
        lblTemplatePath = new Label();
        txtTemplatePath = new TextBox();
        btnSelectTemplate = new Button();
        actionPanel = new TableLayoutPanel();
        btnPickRegion = new Button();
        btnCaptureRegion = new Button();
        btnExecuteMatch = new Button();
        btnGenerateSnippet = new Button();
        lblHint = new Label();
        previewPanel = new TableLayoutPanel();
        lblSourceTitle = new Label();
        picSource = new PictureBox();
        lblTemplateTitle = new Label();
        picTemplate = new PictureBox();
        rightPanel = new TableLayoutPanel();
        grpResult = new GroupBox();
        resultLayout = new TableLayoutPanel();
        lblResultStatus = new Label();
        lblStatusValue = new Label();
        lblResultLocation = new Label();
        lblLocationValue = new Label();
        lblResultScore = new Label();
        lblScoreValue = new Label();
        lblResultElapsed = new Label();
        lblElapsedValue = new Label();
        lblResultCount = new Label();
        lblCountValue = new Label();
        lblSnippetSection = new Label();
        snippetActions = new TableLayoutPanel();
        btnRegenerateSnippet = new Button();
        btnCopySnippet = new Button();
        txtSnippet = new TextBox();
        lblLogSection = new Label();
        txtLog = new TextBox();
        rootLayout.SuspendLayout();
        leftPanel.SuspendLayout();
        grpRegion.SuspendLayout();
        regionLayout.SuspendLayout();
        grpMatcher.SuspendLayout();
        matcherLayout.SuspendLayout();
        grpTemplate.SuspendLayout();
        templateLayout.SuspendLayout();
        actionPanel.SuspendLayout();
        previewPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picSource).BeginInit();
        ((System.ComponentModel.ISupportInitialize)picTemplate).BeginInit();
        rightPanel.SuspendLayout();
        grpResult.SuspendLayout();
        resultLayout.SuspendLayout();
        snippetActions.SuspendLayout();
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
        leftPanel.Controls.Add(grpMatcher, 0, 1);
        leftPanel.Controls.Add(grpTemplate, 0, 2);
        leftPanel.Controls.Add(actionPanel, 0, 3);
        leftPanel.Controls.Add(lblHint, 0, 4);
        leftPanel.Dock = DockStyle.Fill;
        leftPanel.Location = new Point(15, 15);
        leftPanel.Name = "leftPanel";
        leftPanel.RowCount = 5;
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 220F));
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 240F));
        leftPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 140F));
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
        grpRegion.Text = "搜索区域";
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
        txtRegionHeight.TabIndex = 7;
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
        // 
        // grpMatcher
        // 
        grpMatcher.Controls.Add(matcherLayout);
        grpMatcher.Dock = DockStyle.Fill;
        grpMatcher.Location = new Point(3, 183);
        grpMatcher.Name = "grpMatcher";
        grpMatcher.Padding = new Padding(10);
        grpMatcher.Size = new Size(318, 234);
        grpMatcher.TabIndex = 1;
        grpMatcher.TabStop = false;
        grpMatcher.Text = "匹配参数";
        // 
        // matcherLayout
        // 
        matcherLayout.ColumnCount = 2;
        matcherLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
        matcherLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        matcherLayout.Controls.Add(lblThreshold, 0, 0);
        matcherLayout.Controls.Add(txtThreshold, 1, 0);
        matcherLayout.Controls.Add(lblStep, 0, 1);
        matcherLayout.Controls.Add(txtStep, 1, 1);
        matcherLayout.Controls.Add(lblMatchMode, 0, 2);
        matcherLayout.Controls.Add(cboMatchMode, 1, 2);
        matcherLayout.Controls.Add(lblScanDirection, 0, 3);
        matcherLayout.Controls.Add(cboScanDirection, 1, 3);
        matcherLayout.Controls.Add(chkGrayscale, 1, 4);
        matcherLayout.Dock = DockStyle.Fill;
        matcherLayout.Location = new Point(10, 26);
        matcherLayout.Name = "matcherLayout";
        matcherLayout.RowCount = 5;
        matcherLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        matcherLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        matcherLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        matcherLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        matcherLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        matcherLayout.Size = new Size(298, 198);
        matcherLayout.TabIndex = 0;
        // 
        // lblThreshold
        // 
        lblThreshold.Dock = DockStyle.Fill;
        lblThreshold.Location = new Point(3, 0);
        lblThreshold.Name = "lblThreshold";
        lblThreshold.Size = new Size(90, 34);
        lblThreshold.TabIndex = 0;
        lblThreshold.Text = "Threshold";
        // 
        // txtThreshold
        // 
        txtThreshold.Dock = DockStyle.Fill;
        txtThreshold.Location = new Point(99, 3);
        txtThreshold.Name = "txtThreshold";
        txtThreshold.Size = new Size(196, 23);
        txtThreshold.TabIndex = 1;
        txtThreshold.Text = "0.90";
        // 
        // lblStep
        // 
        lblStep.Dock = DockStyle.Fill;
        lblStep.Location = new Point(3, 34);
        lblStep.Name = "lblStep";
        lblStep.Size = new Size(90, 34);
        lblStep.TabIndex = 2;
        lblStep.Text = "Step";
        // 
        // txtStep
        // 
        txtStep.Dock = DockStyle.Fill;
        txtStep.Location = new Point(99, 37);
        txtStep.Name = "txtStep";
        txtStep.Size = new Size(196, 23);
        txtStep.TabIndex = 3;
        txtStep.Text = "1";
        // 
        // lblMatchMode
        // 
        lblMatchMode.Dock = DockStyle.Fill;
        lblMatchMode.Location = new Point(3, 68);
        lblMatchMode.Name = "lblMatchMode";
        lblMatchMode.Size = new Size(90, 34);
        lblMatchMode.TabIndex = 4;
        lblMatchMode.Text = "匹配模式";
        // 
        // cboMatchMode
        // 
        cboMatchMode.Dock = DockStyle.Fill;
        cboMatchMode.DropDownStyle = ComboBoxStyle.DropDownList;
        cboMatchMode.Location = new Point(99, 71);
        cboMatchMode.Name = "cboMatchMode";
        cboMatchMode.Size = new Size(196, 25);
        cboMatchMode.TabIndex = 5;
        // 
        // lblScanDirection
        // 
        lblScanDirection.Dock = DockStyle.Fill;
        lblScanDirection.Location = new Point(3, 102);
        lblScanDirection.Name = "lblScanDirection";
        lblScanDirection.Size = new Size(90, 34);
        lblScanDirection.TabIndex = 6;
        lblScanDirection.Text = "扫描方向";
        // 
        // cboScanDirection
        // 
        cboScanDirection.Dock = DockStyle.Fill;
        cboScanDirection.DropDownStyle = ComboBoxStyle.DropDownList;
        cboScanDirection.Location = new Point(99, 105);
        cboScanDirection.Name = "cboScanDirection";
        cboScanDirection.Size = new Size(196, 25);
        cboScanDirection.TabIndex = 7;
        // 
        // chkGrayscale
        // 
        chkGrayscale.Checked = true;
        chkGrayscale.CheckState = CheckState.Checked;
        chkGrayscale.Dock = DockStyle.Fill;
        chkGrayscale.Location = new Point(99, 139);
        chkGrayscale.Name = "chkGrayscale";
        chkGrayscale.Size = new Size(196, 56);
        chkGrayscale.TabIndex = 8;
        chkGrayscale.Text = "启用灰度匹配";
        // 
        // grpTemplate
        // 
        grpTemplate.Controls.Add(templateLayout);
        grpTemplate.Dock = DockStyle.Fill;
        grpTemplate.Location = new Point(3, 423);
        grpTemplate.Name = "grpTemplate";
        grpTemplate.Padding = new Padding(10);
        grpTemplate.Size = new Size(318, 134);
        grpTemplate.TabIndex = 2;
        grpTemplate.TabStop = false;
        grpTemplate.Text = "模板图片";
        // 
        // templateLayout
        // 
        templateLayout.ColumnCount = 1;
        templateLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        templateLayout.Controls.Add(lblTemplatePath, 0, 0);
        templateLayout.Controls.Add(txtTemplatePath, 0, 1);
        templateLayout.Controls.Add(btnSelectTemplate, 0, 2);
        templateLayout.Dock = DockStyle.Fill;
        templateLayout.Location = new Point(10, 26);
        templateLayout.Name = "templateLayout";
        templateLayout.RowCount = 3;
        templateLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        templateLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        templateLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        templateLayout.Size = new Size(298, 98);
        templateLayout.TabIndex = 0;
        // 
        // lblTemplatePath
        // 
        lblTemplatePath.Dock = DockStyle.Fill;
        lblTemplatePath.Location = new Point(3, 0);
        lblTemplatePath.Name = "lblTemplatePath";
        lblTemplatePath.Size = new Size(292, 24);
        lblTemplatePath.TabIndex = 0;
        lblTemplatePath.Text = "本地图片路径";
        // 
        // txtTemplatePath
        // 
        txtTemplatePath.Dock = DockStyle.Fill;
        txtTemplatePath.Location = new Point(3, 27);
        txtTemplatePath.Name = "txtTemplatePath";
        txtTemplatePath.ReadOnly = true;
        txtTemplatePath.Size = new Size(292, 23);
        txtTemplatePath.TabIndex = 1;
        // 
        // btnSelectTemplate
        // 
        btnSelectTemplate.Dock = DockStyle.Fill;
        btnSelectTemplate.Location = new Point(3, 61);
        btnSelectTemplate.Name = "btnSelectTemplate";
        btnSelectTemplate.Size = new Size(292, 34);
        btnSelectTemplate.TabIndex = 2;
        btnSelectTemplate.Text = "选择模板";
        btnSelectTemplate.Click += btnSelectTemplate_Click;
        // 
        // actionPanel
        // 
        actionPanel.ColumnCount = 1;
        actionPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        actionPanel.Controls.Add(btnCaptureRegion, 0, 0);
        actionPanel.Controls.Add(btnExecuteMatch, 0, 1);
        actionPanel.Controls.Add(btnGenerateSnippet, 0, 2);
        actionPanel.Dock = DockStyle.Fill;
        actionPanel.Location = new Point(3, 563);
        actionPanel.Name = "actionPanel";
        actionPanel.RowCount = 3;
        actionPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        actionPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        actionPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 33.33F));
        actionPanel.Size = new Size(318, 124);
        actionPanel.TabIndex = 3;
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
        // btnExecuteMatch
        // 
        btnExecuteMatch.Dock = DockStyle.Fill;
        btnExecuteMatch.Location = new Point(3, 44);
        btnExecuteMatch.Name = "btnExecuteMatch";
        btnExecuteMatch.Size = new Size(312, 35);
        btnExecuteMatch.TabIndex = 1;
        btnExecuteMatch.Text = "执行匹配";
        btnExecuteMatch.Click += btnExecuteMatch_Click;
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
        lblHint.Location = new Point(3, 690);
        lblHint.Name = "lblHint";
        lblHint.Size = new Size(318, 200);
        lblHint.TabIndex = 4;
        lblHint.Text = "列表按钮通常适合“只找第一个 + 从上到下”或“找全部 + 按列扫描”。调好参数后可直接生成 C# 调用代码。";
        // 
        // previewPanel
        // 
        previewPanel.ColumnCount = 1;
        previewPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        previewPanel.Controls.Add(lblSourceTitle, 0, 0);
        previewPanel.Controls.Add(picSource, 0, 1);
        previewPanel.Controls.Add(lblTemplateTitle, 0, 2);
        previewPanel.Controls.Add(picTemplate, 0, 3);
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
        lblSourceTitle.Text = "区域截图";
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
        // lblTemplateTitle
        // 
        lblTemplateTitle.AutoSize = true;
        lblTemplateTitle.Dock = DockStyle.Top;
        lblTemplateTitle.Location = new Point(3, 445);
        lblTemplateTitle.Name = "lblTemplateTitle";
        lblTemplateTitle.Padding = new Padding(0, 0, 0, 6);
        lblTemplateTitle.Size = new Size(814, 23);
        lblTemplateTitle.TabIndex = 2;
        lblTemplateTitle.Text = "模板预览";
        // 
        // picTemplate
        // 
        picTemplate.BackColor = Color.White;
        picTemplate.BorderStyle = BorderStyle.FixedSingle;
        picTemplate.Dock = DockStyle.Fill;
        picTemplate.Location = new Point(3, 476);
        picTemplate.Name = "picTemplate";
        picTemplate.Size = new Size(814, 411);
        picTemplate.SizeMode = PictureBoxSizeMode.Zoom;
        picTemplate.TabIndex = 3;
        picTemplate.TabStop = false;
        // 
        // rightPanel
        // 
        rightPanel.ColumnCount = 1;
        rightPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rightPanel.Controls.Add(grpResult, 0, 0);
        rightPanel.Controls.Add(lblSnippetSection, 0, 1);
        rightPanel.Controls.Add(snippetActions, 0, 2);
        rightPanel.Controls.Add(txtSnippet, 0, 3);
        rightPanel.Controls.Add(lblLogSection, 0, 4);
        rightPanel.Controls.Add(txtLog, 0, 5);
        rightPanel.Dock = DockStyle.Fill;
        rightPanel.Location = new Point(1171, 15);
        rightPanel.Name = "rightPanel";
        rightPanel.RowCount = 6;
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 200F));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        rightPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
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
        grpResult.Size = new Size(348, 194);
        grpResult.TabIndex = 0;
        grpResult.TabStop = false;
        grpResult.Text = "匹配结果";
        // 
        // resultLayout
        // 
        resultLayout.ColumnCount = 2;
        resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 96F));
        resultLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        resultLayout.Controls.Add(lblResultStatus, 0, 0);
        resultLayout.Controls.Add(lblStatusValue, 1, 0);
        resultLayout.Controls.Add(lblResultLocation, 0, 1);
        resultLayout.Controls.Add(lblLocationValue, 1, 1);
        resultLayout.Controls.Add(lblResultScore, 0, 2);
        resultLayout.Controls.Add(lblScoreValue, 1, 2);
        resultLayout.Controls.Add(lblResultElapsed, 0, 3);
        resultLayout.Controls.Add(lblElapsedValue, 1, 3);
        resultLayout.Controls.Add(lblResultCount, 0, 4);
        resultLayout.Controls.Add(lblCountValue, 1, 4);
        resultLayout.Dock = DockStyle.Fill;
        resultLayout.Location = new Point(10, 26);
        resultLayout.Name = "resultLayout";
        resultLayout.RowCount = 5;
        resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        resultLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        resultLayout.Size = new Size(328, 158);
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
        // lblResultLocation
        // 
        lblResultLocation.Dock = DockStyle.Fill;
        lblResultLocation.Location = new Point(3, 32);
        lblResultLocation.Name = "lblResultLocation";
        lblResultLocation.Size = new Size(90, 32);
        lblResultLocation.TabIndex = 2;
        lblResultLocation.Text = "首个坐标";
        // 
        // lblLocationValue
        // 
        lblLocationValue.Dock = DockStyle.Fill;
        lblLocationValue.Location = new Point(99, 32);
        lblLocationValue.Name = "lblLocationValue";
        lblLocationValue.Size = new Size(226, 32);
        lblLocationValue.TabIndex = 3;
        lblLocationValue.Text = "-";
        // 
        // lblResultScore
        // 
        lblResultScore.Dock = DockStyle.Fill;
        lblResultScore.Location = new Point(3, 64);
        lblResultScore.Name = "lblResultScore";
        lblResultScore.Size = new Size(90, 32);
        lblResultScore.TabIndex = 4;
        lblResultScore.Text = "首个分数";
        // 
        // lblScoreValue
        // 
        lblScoreValue.Dock = DockStyle.Fill;
        lblScoreValue.Location = new Point(99, 64);
        lblScoreValue.Name = "lblScoreValue";
        lblScoreValue.Size = new Size(226, 32);
        lblScoreValue.TabIndex = 5;
        lblScoreValue.Text = "-";
        // 
        // lblResultElapsed
        // 
        lblResultElapsed.Dock = DockStyle.Fill;
        lblResultElapsed.Location = new Point(3, 96);
        lblResultElapsed.Name = "lblResultElapsed";
        lblResultElapsed.Size = new Size(90, 32);
        lblResultElapsed.TabIndex = 6;
        lblResultElapsed.Text = "耗时(ms)";
        // 
        // lblElapsedValue
        // 
        lblElapsedValue.Dock = DockStyle.Fill;
        lblElapsedValue.Location = new Point(99, 96);
        lblElapsedValue.Name = "lblElapsedValue";
        lblElapsedValue.Size = new Size(226, 32);
        lblElapsedValue.TabIndex = 7;
        lblElapsedValue.Text = "-";
        // 
        // lblResultCount
        // 
        lblResultCount.Dock = DockStyle.Fill;
        lblResultCount.Location = new Point(3, 128);
        lblResultCount.Name = "lblResultCount";
        lblResultCount.Size = new Size(90, 32);
        lblResultCount.TabIndex = 8;
        lblResultCount.Text = "命中数量";
        // 
        // lblCountValue
        // 
        lblCountValue.Dock = DockStyle.Fill;
        lblCountValue.Location = new Point(99, 128);
        lblCountValue.Name = "lblCountValue";
        lblCountValue.Size = new Size(226, 32);
        lblCountValue.TabIndex = 9;
        lblCountValue.Text = "-";
        // 
        // lblSnippetSection
        // 
        lblSnippetSection.AutoSize = true;
        lblSnippetSection.Dock = DockStyle.Top;
        lblSnippetSection.Location = new Point(3, 200);
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
        snippetActions.Location = new Point(3, 231);
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
        txtSnippet.Location = new Point(3, 271);
        txtSnippet.Multiline = true;
        txtSnippet.Name = "txtSnippet";
        txtSnippet.ReadOnly = true;
        txtSnippet.ScrollBars = ScrollBars.Both;
        txtSnippet.Size = new Size(348, 261);
        txtSnippet.TabIndex = 3;
        txtSnippet.WordWrap = false;
        // 
        // lblLogSection
        // 
        lblLogSection.AutoSize = true;
        lblLogSection.Dock = DockStyle.Top;
        lblLogSection.Location = new Point(3, 535);
        lblLogSection.Name = "lblLogSection";
        lblLogSection.Padding = new Padding(0, 0, 0, 6);
        lblLogSection.Size = new Size(348, 23);
        lblLogSection.TabIndex = 4;
        lblLogSection.Text = "调试日志";
        // 
        // txtLog
        // 
        txtLog.Dock = DockStyle.Fill;
        txtLog.Font = new Font("Consolas", 10.5F);
        txtLog.Location = new Point(3, 566);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Both;
        txtLog.Size = new Size(348, 321);
        txtLog.TabIndex = 5;
        txtLog.WordWrap = false;
        // 
        // ImageMatchTestForm
        // 
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1540, 920);
        Controls.Add(rootLayout);
        MinimumSize = new Size(1360, 820);
        Name = "ImageMatchTestForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "找图测试";
        rootLayout.ResumeLayout(false);
        leftPanel.ResumeLayout(false);
        leftPanel.PerformLayout();
        grpRegion.ResumeLayout(false);
        regionLayout.ResumeLayout(false);
        regionLayout.PerformLayout();
        grpMatcher.ResumeLayout(false);
        matcherLayout.ResumeLayout(false);
        matcherLayout.PerformLayout();
        grpTemplate.ResumeLayout(false);
        templateLayout.ResumeLayout(false);
        templateLayout.PerformLayout();
        actionPanel.ResumeLayout(false);
        previewPanel.ResumeLayout(false);
        previewPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)picSource).EndInit();
        ((System.ComponentModel.ISupportInitialize)picTemplate).EndInit();
        rightPanel.ResumeLayout(false);
        rightPanel.PerformLayout();
        grpResult.ResumeLayout(false);
        resultLayout.ResumeLayout(false);
        snippetActions.ResumeLayout(false);
        ResumeLayout(false);
    }
}
