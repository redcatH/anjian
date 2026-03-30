using System.Drawing;
using System.Windows.Forms;

namespace Anjian;

public sealed partial class MainForm
{
    private TableLayoutPanel rootLayout = null!;
    private Panel headerPanel = null!;
    private Label lblHeaderTitle = null!;
    private Label lblHeaderDescription = null!;
    private TableLayoutPanel workspaceLayout = null!;
    private Panel leftCard = null!;
    private Panel centerCard = null!;
    private Panel rightCard = null!;
    private TableLayoutPanel leftStack = null!;
    private TableLayoutPanel centerStack = null!;
    private TableLayoutPanel rightStack = null!;
    private GroupBox grpHotKeys = null!;
    private Label lblHotKeys = null!;
    private GroupBox grpCapture = null!;
    private TableLayoutPanel captureLayout = null!;
    private Label lblCaptureLiveTitle = null!;
    private Label lblLivePosition = null!;
    private Label lblCaptureXTitle = null!;
    private TextBox txtX = null!;
    private Label lblCaptureYTitle = null!;
    private TextBox txtY = null!;
    private TableLayoutPanel captureButtonLayout = null!;
    private Button btnCapturePoint = null!;
    private Button btnCopyPoint = null!;
    private GroupBox grpMouseTools = null!;
    private TableLayoutPanel mouseToolLayout = null!;
    private Label lblDoubleClickIntervalTitle = null!;
    private TextBox txtDoubleClickInterval = null!;
    private Button btnMoveMouse = null!;
    private Button btnLeftClick = null!;
    private Button btnDoubleClick = null!;
    private Label lblMouseToolHint = null!;
    private GroupBox grpHistory = null!;
    private ListBox lstHistory = null!;
    private GroupBox grpToolEntrance = null!;
    private Button btnOpenImageMatch = null!;
    private Button btnOpenAmountOcr = null!;
    private GroupBox grpGuide = null!;
    private Label lblGuide = null!;
    private Label lblSnippetTitle = null!;
    private TableLayoutPanel snippetActionLayout = null!;
    private Button btnCopySnippet = null!;
    private Button btnClearSnippet = null!;
    private TextBox txtSnippet = null!;
    private Label lblSnippetHint = null!;
    private Panel statusPanel = null!;
    private Label lblStatus = null!;

    private void InitializeComponent()
    {
        rootLayout = new TableLayoutPanel();
        headerPanel = new Panel();
        lblHeaderDescription = new Label();
        lblHeaderTitle = new Label();
        workspaceLayout = new TableLayoutPanel();
        leftCard = new Panel();
        leftStack = new TableLayoutPanel();
        grpHotKeys = new GroupBox();
        lblHotKeys = new Label();
        grpCapture = new GroupBox();
        captureLayout = new TableLayoutPanel();
        lblCaptureLiveTitle = new Label();
        lblLivePosition = new Label();
        lblCaptureXTitle = new Label();
        txtX = new TextBox();
        lblCaptureYTitle = new Label();
        txtY = new TextBox();
        captureButtonLayout = new TableLayoutPanel();
        btnCapturePoint = new Button();
        btnCopyPoint = new Button();
        grpMouseTools = new GroupBox();
        mouseToolLayout = new TableLayoutPanel();
        lblDoubleClickIntervalTitle = new Label();
        txtDoubleClickInterval = new TextBox();
        btnMoveMouse = new Button();
        btnLeftClick = new Button();
        btnDoubleClick = new Button();
        lblMouseToolHint = new Label();
        grpHistory = new GroupBox();
        lstHistory = new ListBox();
        centerCard = new Panel();
        centerStack = new TableLayoutPanel();
        grpToolEntrance = new GroupBox();
        btnOpenAmountOcr = new Button();
        btnOpenImageMatch = new Button();
        grpGuide = new GroupBox();
        lblGuide = new Label();
        rightCard = new Panel();
        rightStack = new TableLayoutPanel();
        lblSnippetTitle = new Label();
        snippetActionLayout = new TableLayoutPanel();
        btnCopySnippet = new Button();
        btnClearSnippet = new Button();
        txtSnippet = new TextBox();
        lblSnippetHint = new Label();
        statusPanel = new Panel();
        lblStatus = new Label();
        rootLayout.SuspendLayout();
        headerPanel.SuspendLayout();
        workspaceLayout.SuspendLayout();
        leftCard.SuspendLayout();
        leftStack.SuspendLayout();
        grpHotKeys.SuspendLayout();
        grpCapture.SuspendLayout();
        captureLayout.SuspendLayout();
        captureButtonLayout.SuspendLayout();
        grpMouseTools.SuspendLayout();
        mouseToolLayout.SuspendLayout();
        grpHistory.SuspendLayout();
        centerCard.SuspendLayout();
        centerStack.SuspendLayout();
        grpToolEntrance.SuspendLayout();
        grpGuide.SuspendLayout();
        rightCard.SuspendLayout();
        rightStack.SuspendLayout();
        snippetActionLayout.SuspendLayout();
        statusPanel.SuspendLayout();
        SuspendLayout();
        // 
        // rootLayout
        // 
        rootLayout.BackColor = Color.FromArgb(245, 247, 250);
        rootLayout.ColumnCount = 1;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.Controls.Add(headerPanel, 0, 0);
        rootLayout.Controls.Add(workspaceLayout, 0, 1);
        rootLayout.Controls.Add(statusPanel, 0, 2);
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.Location = new Point(0, 0);
        rootLayout.Name = "rootLayout";
        rootLayout.Padding = new Padding(16, 14, 16, 16);
        rootLayout.RowCount = 3;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        rootLayout.Size = new Size(1484, 901);
        rootLayout.TabIndex = 0;
        // 
        // headerPanel
        // 
        headerPanel.BackColor = Color.White;
        headerPanel.Controls.Add(lblHeaderDescription);
        headerPanel.Controls.Add(lblHeaderTitle);
        headerPanel.Dock = DockStyle.Fill;
        headerPanel.Location = new Point(16, 14);
        headerPanel.Margin = new Padding(0, 0, 0, 12);
        headerPanel.Name = "headerPanel";
        headerPanel.Padding = new Padding(18, 16, 18, 12);
        headerPanel.Size = new Size(1452, 70);
        headerPanel.TabIndex = 0;
        // 
        // lblHeaderDescription
        // 
        lblHeaderDescription.Dock = DockStyle.Top;
        lblHeaderDescription.ForeColor = Color.DimGray;
        lblHeaderDescription.Location = new Point(18, 46);
        lblHeaderDescription.Name = "lblHeaderDescription";
        lblHeaderDescription.Size = new Size(1416, 24);
        lblHeaderDescription.TabIndex = 1;
        lblHeaderDescription.Text = "主页面负责坐标采集、鼠标动作调试和 C# 代码片段输出，专项参数请在测试窗口中完成调试。";
        // 
        // lblHeaderTitle
        // 
        lblHeaderTitle.Dock = DockStyle.Top;
        lblHeaderTitle.Font = new Font("Microsoft YaHei UI", 16F, FontStyle.Bold);
        lblHeaderTitle.Location = new Point(18, 16);
        lblHeaderTitle.Name = "lblHeaderTitle";
        lblHeaderTitle.Size = new Size(1416, 30);
        lblHeaderTitle.TabIndex = 0;
        lblHeaderTitle.Text = "C# 自动化工具台";
        // 
        // workspaceLayout
        // 
        workspaceLayout.ColumnCount = 3;
        workspaceLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 360F));
        workspaceLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320F));
        workspaceLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        workspaceLayout.Controls.Add(leftCard, 0, 0);
        workspaceLayout.Controls.Add(centerCard, 1, 0);
        workspaceLayout.Controls.Add(rightCard, 2, 0);
        workspaceLayout.Dock = DockStyle.Fill;
        workspaceLayout.Location = new Point(16, 96);
        workspaceLayout.Margin = new Padding(0);
        workspaceLayout.Name = "workspaceLayout";
        workspaceLayout.RowCount = 1;
        workspaceLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        workspaceLayout.Size = new Size(1452, 741);
        workspaceLayout.TabIndex = 1;
        // 
        // leftCard
        // 
        leftCard.BackColor = Color.White;
        leftCard.Controls.Add(leftStack);
        leftCard.Dock = DockStyle.Fill;
        leftCard.Location = new Point(0, 0);
        leftCard.Margin = new Padding(0, 0, 12, 0);
        leftCard.Name = "leftCard";
        leftCard.Padding = new Padding(14);
        leftCard.Size = new Size(348, 741);
        leftCard.TabIndex = 0;
        // 
        // leftStack
        // 
        leftStack.ColumnCount = 1;
        leftStack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        leftStack.Controls.Add(grpHotKeys, 0, 0);
        leftStack.Controls.Add(grpCapture, 0, 1);
        leftStack.Controls.Add(grpMouseTools, 0, 2);
        leftStack.Controls.Add(grpHistory, 0, 3);
        leftStack.Dock = DockStyle.Fill;
        leftStack.Location = new Point(14, 14);
        leftStack.Name = "leftStack";
        leftStack.RowCount = 4;
        leftStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 102F));
        leftStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 172F));
        leftStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 228F));
        leftStack.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        leftStack.Size = new Size(320, 713);
        leftStack.TabIndex = 0;
        // 
        // grpHotKeys
        // 
        grpHotKeys.Controls.Add(lblHotKeys);
        grpHotKeys.Dock = DockStyle.Fill;
        grpHotKeys.Location = new Point(0, 0);
        grpHotKeys.Margin = new Padding(0, 0, 0, 10);
        grpHotKeys.Name = "grpHotKeys";
        grpHotKeys.Padding = new Padding(10);
        grpHotKeys.Size = new Size(320, 92);
        grpHotKeys.TabIndex = 0;
        grpHotKeys.TabStop = false;
        grpHotKeys.Text = "全局热键";
        // 
        // lblHotKeys
        // 
        lblHotKeys.Dock = DockStyle.Fill;
        lblHotKeys.Location = new Point(10, 26);
        lblHotKeys.Name = "lblHotKeys";
        lblHotKeys.Size = new Size(300, 56);
        lblHotKeys.TabIndex = 0;
        lblHotKeys.Text = "Ctrl + Alt + 1：采集当前鼠标坐标\r\nCtrl + Alt + Shift + 1：采集并复制坐标";
        // 
        // grpCapture
        // 
        grpCapture.Controls.Add(captureLayout);
        grpCapture.Dock = DockStyle.Fill;
        grpCapture.Location = new Point(0, 102);
        grpCapture.Margin = new Padding(0, 0, 0, 10);
        grpCapture.Name = "grpCapture";
        grpCapture.Padding = new Padding(10);
        grpCapture.Size = new Size(320, 162);
        grpCapture.TabIndex = 1;
        grpCapture.TabStop = false;
        grpCapture.Text = "坐标采集";
        // 
        // captureLayout
        // 
        captureLayout.ColumnCount = 2;
        captureLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 72F));
        captureLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        captureLayout.Controls.Add(lblCaptureLiveTitle, 0, 0);
        captureLayout.Controls.Add(lblLivePosition, 1, 0);
        captureLayout.Controls.Add(lblCaptureXTitle, 0, 1);
        captureLayout.Controls.Add(txtX, 1, 1);
        captureLayout.Controls.Add(lblCaptureYTitle, 0, 2);
        captureLayout.Controls.Add(txtY, 1, 2);
        captureLayout.Controls.Add(captureButtonLayout, 1, 3);
        captureLayout.Dock = DockStyle.Fill;
        captureLayout.Location = new Point(10, 26);
        captureLayout.Name = "captureLayout";
        captureLayout.RowCount = 4;
        captureLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        captureLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        captureLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        captureLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        captureLayout.Size = new Size(300, 126);
        captureLayout.TabIndex = 0;
        // 
        // lblCaptureLiveTitle
        // 
        lblCaptureLiveTitle.Dock = DockStyle.Fill;
        lblCaptureLiveTitle.Location = new Point(3, 0);
        lblCaptureLiveTitle.Name = "lblCaptureLiveTitle";
        lblCaptureLiveTitle.Size = new Size(66, 30);
        lblCaptureLiveTitle.TabIndex = 0;
        lblCaptureLiveTitle.Text = "实时";
        lblCaptureLiveTitle.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblLivePosition
        // 
        lblLivePosition.Dock = DockStyle.Fill;
        lblLivePosition.Location = new Point(75, 0);
        lblLivePosition.Name = "lblLivePosition";
        lblLivePosition.Size = new Size(222, 30);
        lblLivePosition.TabIndex = 1;
        lblLivePosition.Text = "当前鼠标：X=0，Y=0";
        lblLivePosition.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lblCaptureXTitle
        // 
        lblCaptureXTitle.Dock = DockStyle.Fill;
        lblCaptureXTitle.Location = new Point(3, 30);
        lblCaptureXTitle.Name = "lblCaptureXTitle";
        lblCaptureXTitle.Size = new Size(66, 34);
        lblCaptureXTitle.TabIndex = 2;
        lblCaptureXTitle.Text = "X";
        lblCaptureXTitle.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // txtX
        // 
        txtX.Dock = DockStyle.Fill;
        txtX.Location = new Point(75, 33);
        txtX.Name = "txtX";
        txtX.ReadOnly = true;
        txtX.Size = new Size(222, 23);
        txtX.TabIndex = 3;
        // 
        // lblCaptureYTitle
        // 
        lblCaptureYTitle.Dock = DockStyle.Fill;
        lblCaptureYTitle.Location = new Point(3, 64);
        lblCaptureYTitle.Name = "lblCaptureYTitle";
        lblCaptureYTitle.Size = new Size(66, 34);
        lblCaptureYTitle.TabIndex = 4;
        lblCaptureYTitle.Text = "Y";
        lblCaptureYTitle.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // txtY
        // 
        txtY.Dock = DockStyle.Fill;
        txtY.Location = new Point(75, 67);
        txtY.Name = "txtY";
        txtY.ReadOnly = true;
        txtY.Size = new Size(222, 23);
        txtY.TabIndex = 5;
        // 
        // captureButtonLayout
        // 
        captureButtonLayout.ColumnCount = 2;
        captureButtonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        captureButtonLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        captureButtonLayout.Controls.Add(btnCapturePoint, 0, 0);
        captureButtonLayout.Controls.Add(btnCopyPoint, 1, 0);
        captureButtonLayout.Dock = DockStyle.Fill;
        captureButtonLayout.Location = new Point(72, 98);
        captureButtonLayout.Margin = new Padding(0);
        captureButtonLayout.Name = "captureButtonLayout";
        captureButtonLayout.RowCount = 1;
        captureButtonLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        captureButtonLayout.Size = new Size(228, 42);
        captureButtonLayout.TabIndex = 6;
        // 
        // btnCapturePoint
        // 
        btnCapturePoint.Dock = DockStyle.Fill;
        btnCapturePoint.Location = new Point(3, 3);
        btnCapturePoint.Name = "btnCapturePoint";
        btnCapturePoint.Size = new Size(108, 36);
        btnCapturePoint.TabIndex = 0;
        btnCapturePoint.Text = "采集坐标";
        btnCapturePoint.UseVisualStyleBackColor = true;
        btnCapturePoint.Click += btnCapturePoint_Click;
        // 
        // btnCopyPoint
        // 
        btnCopyPoint.Dock = DockStyle.Fill;
        btnCopyPoint.Location = new Point(117, 3);
        btnCopyPoint.Name = "btnCopyPoint";
        btnCopyPoint.Size = new Size(108, 36);
        btnCopyPoint.TabIndex = 1;
        btnCopyPoint.Text = "复制坐标";
        btnCopyPoint.UseVisualStyleBackColor = true;
        btnCopyPoint.Click += btnCopyPoint_Click;
        // 
        // grpMouseTools
        // 
        grpMouseTools.Controls.Add(mouseToolLayout);
        grpMouseTools.Dock = DockStyle.Fill;
        grpMouseTools.Location = new Point(0, 274);
        grpMouseTools.Margin = new Padding(0);
        grpMouseTools.Name = "grpMouseTools";
        grpMouseTools.Padding = new Padding(10);
        grpMouseTools.Size = new Size(320, 228);
        grpMouseTools.TabIndex = 2;
        grpMouseTools.TabStop = false;
        grpMouseTools.Text = "鼠标动作工具";
        // 
        // mouseToolLayout
        // 
        mouseToolLayout.ColumnCount = 2;
        mouseToolLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 92F));
        mouseToolLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        mouseToolLayout.Controls.Add(lblDoubleClickIntervalTitle, 0, 0);
        mouseToolLayout.Controls.Add(txtDoubleClickInterval, 1, 0);
        mouseToolLayout.Controls.Add(btnMoveMouse, 1, 1);
        mouseToolLayout.Controls.Add(btnLeftClick, 1, 2);
        mouseToolLayout.Controls.Add(btnDoubleClick, 1, 3);
        mouseToolLayout.Controls.Add(lblMouseToolHint, 1, 4);
        mouseToolLayout.Dock = DockStyle.Fill;
        mouseToolLayout.Location = new Point(10, 26);
        mouseToolLayout.Name = "mouseToolLayout";
        mouseToolLayout.RowCount = 5;
        mouseToolLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        mouseToolLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        mouseToolLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        mouseToolLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
        mouseToolLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        mouseToolLayout.Size = new Size(300, 192);
        mouseToolLayout.TabIndex = 0;
        // 
        // lblDoubleClickIntervalTitle
        // 
        lblDoubleClickIntervalTitle.Dock = DockStyle.Fill;
        lblDoubleClickIntervalTitle.Location = new Point(3, 0);
        lblDoubleClickIntervalTitle.Name = "lblDoubleClickIntervalTitle";
        lblDoubleClickIntervalTitle.Size = new Size(86, 34);
        lblDoubleClickIntervalTitle.TabIndex = 0;
        lblDoubleClickIntervalTitle.Text = "双击间隔";
        lblDoubleClickIntervalTitle.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // txtDoubleClickInterval
        // 
        txtDoubleClickInterval.Dock = DockStyle.Fill;
        txtDoubleClickInterval.Location = new Point(95, 3);
        txtDoubleClickInterval.Name = "txtDoubleClickInterval";
        txtDoubleClickInterval.Size = new Size(202, 23);
        txtDoubleClickInterval.TabIndex = 1;
        txtDoubleClickInterval.Text = "80";
        // 
        // btnMoveMouse
        // 
        btnMoveMouse.Dock = DockStyle.Fill;
        btnMoveMouse.Location = new Point(95, 37);
        btnMoveMouse.Name = "btnMoveMouse";
        btnMoveMouse.Size = new Size(202, 34);
        btnMoveMouse.TabIndex = 2;
        btnMoveMouse.Text = "测试移动 / 生成代码";
        btnMoveMouse.UseVisualStyleBackColor = true;
        btnMoveMouse.Click += btnMoveMouse_Click;
        // 
        // btnLeftClick
        // 
        btnLeftClick.Dock = DockStyle.Fill;
        btnLeftClick.Location = new Point(95, 77);
        btnLeftClick.Name = "btnLeftClick";
        btnLeftClick.Size = new Size(202, 34);
        btnLeftClick.TabIndex = 3;
        btnLeftClick.Text = "测试单击 / 生成代码";
        btnLeftClick.UseVisualStyleBackColor = true;
        btnLeftClick.Click += btnLeftClick_Click;
        // 
        // btnDoubleClick
        // 
        btnDoubleClick.Dock = DockStyle.Fill;
        btnDoubleClick.Location = new Point(95, 117);
        btnDoubleClick.Name = "btnDoubleClick";
        btnDoubleClick.Size = new Size(202, 34);
        btnDoubleClick.TabIndex = 4;
        btnDoubleClick.Text = "测试双击 / 生成代码";
        btnDoubleClick.UseVisualStyleBackColor = true;
        btnDoubleClick.Click += btnDoubleClick_Click;
        // 
        // lblMouseToolHint
        // 
        lblMouseToolHint.Dock = DockStyle.Fill;
        lblMouseToolHint.ForeColor = Color.DimGray;
        lblMouseToolHint.Location = new Point(95, 154);
        lblMouseToolHint.Name = "lblMouseToolHint";
        lblMouseToolHint.Size = new Size(202, 38);
        lblMouseToolHint.TabIndex = 5;
        lblMouseToolHint.Text = "先采集坐标，再测试动作或生成 C# 代码。";
        // 
        // grpHistory
        // 
        grpHistory.Controls.Add(lstHistory);
        grpHistory.Dock = DockStyle.Fill;
        grpHistory.Location = new Point(0, 512);
        grpHistory.Margin = new Padding(0, 10, 0, 0);
        grpHistory.Name = "grpHistory";
        grpHistory.Padding = new Padding(10);
        grpHistory.Size = new Size(320, 201);
        grpHistory.TabIndex = 3;
        grpHistory.TabStop = false;
        grpHistory.Text = "坐标历史";
        // 
        // lstHistory
        // 
        lstHistory.Dock = DockStyle.Fill;
        lstHistory.FormattingEnabled = true;
        lstHistory.HorizontalScrollbar = true;
        lstHistory.IntegralHeight = false;
        lstHistory.ItemHeight = 17;
        lstHistory.Location = new Point(10, 26);
        lstHistory.Name = "lstHistory";
        lstHistory.Size = new Size(300, 165);
        lstHistory.TabIndex = 0;
        lstHistory.DoubleClick += lstHistory_DoubleClick;
        // 
        // centerCard
        // 
        centerCard.BackColor = Color.White;
        centerCard.Controls.Add(centerStack);
        centerCard.Dock = DockStyle.Fill;
        centerCard.Location = new Point(360, 0);
        centerCard.Margin = new Padding(0, 0, 12, 0);
        centerCard.Name = "centerCard";
        centerCard.Padding = new Padding(14);
        centerCard.Size = new Size(308, 741);
        centerCard.TabIndex = 1;
        // 
        // centerStack
        // 
        centerStack.ColumnCount = 1;
        centerStack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        centerStack.Controls.Add(grpToolEntrance, 0, 0);
        centerStack.Controls.Add(grpGuide, 0, 1);
        centerStack.Dock = DockStyle.Fill;
        centerStack.Location = new Point(14, 14);
        centerStack.Name = "centerStack";
        centerStack.RowCount = 3;
        centerStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 156F));
        centerStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 184F));
        centerStack.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        centerStack.Size = new Size(280, 713);
        centerStack.TabIndex = 0;
        // 
        // grpToolEntrance
        // 
        grpToolEntrance.Controls.Add(btnOpenAmountOcr);
        grpToolEntrance.Controls.Add(btnOpenImageMatch);
        grpToolEntrance.Dock = DockStyle.Fill;
        grpToolEntrance.Location = new Point(0, 0);
        grpToolEntrance.Margin = new Padding(0);
        grpToolEntrance.Name = "grpToolEntrance";
        grpToolEntrance.Padding = new Padding(10);
        grpToolEntrance.Size = new Size(280, 156);
        grpToolEntrance.TabIndex = 0;
        grpToolEntrance.TabStop = false;
        grpToolEntrance.Text = "调试工具";
        // 
        // btnOpenAmountOcr
        // 
        btnOpenAmountOcr.Dock = DockStyle.Top;
        btnOpenAmountOcr.Location = new Point(10, 66);
        btnOpenAmountOcr.Name = "btnOpenAmountOcr";
        btnOpenAmountOcr.Size = new Size(260, 40);
        btnOpenAmountOcr.TabIndex = 1;
        btnOpenAmountOcr.Text = "金额识别测试";
        btnOpenAmountOcr.UseVisualStyleBackColor = true;
        btnOpenAmountOcr.Click += btnOpenAmountOcr_Click;
        // 
        // btnOpenImageMatch
        // 
        btnOpenImageMatch.Dock = DockStyle.Top;
        btnOpenImageMatch.Location = new Point(10, 26);
        btnOpenImageMatch.Margin = new Padding(0, 0, 0, 8);
        btnOpenImageMatch.Name = "btnOpenImageMatch";
        btnOpenImageMatch.Size = new Size(260, 40);
        btnOpenImageMatch.TabIndex = 0;
        btnOpenImageMatch.Text = "找图测试";
        btnOpenImageMatch.UseVisualStyleBackColor = true;
        btnOpenImageMatch.Click += btnOpenImageMatch_Click;
        // 
        // grpGuide
        // 
        grpGuide.Controls.Add(lblGuide);
        grpGuide.Dock = DockStyle.Fill;
        grpGuide.Location = new Point(0, 166);
        grpGuide.Margin = new Padding(0, 10, 0, 0);
        grpGuide.Name = "grpGuide";
        grpGuide.Padding = new Padding(10);
        grpGuide.Size = new Size(280, 174);
        grpGuide.TabIndex = 1;
        grpGuide.TabStop = false;
        grpGuide.Text = "使用说明";
        // 
        // lblGuide
        // 
        lblGuide.Dock = DockStyle.Fill;
        lblGuide.ForeColor = Color.DimGray;
        lblGuide.Location = new Point(10, 26);
        lblGuide.Name = "lblGuide";
        lblGuide.Size = new Size(260, 138);
        lblGuide.TabIndex = 0;
        lblGuide.Text = "1. 先用热键或按钮采集坐标。\r\n2. 在左侧测试鼠标移动、单击、双击。\r\n3. 在找图或金额窗口中调好参数。\r\n4. 复制右侧生成的 C# 调用代码，粘贴到你的工程里。";
        // 
        // rightCard
        // 
        rightCard.BackColor = Color.White;
        rightCard.Controls.Add(rightStack);
        rightCard.Dock = DockStyle.Fill;
        rightCard.Location = new Point(680, 0);
        rightCard.Margin = new Padding(0);
        rightCard.Name = "rightCard";
        rightCard.Padding = new Padding(14);
        rightCard.Size = new Size(772, 741);
        rightCard.TabIndex = 2;
        // 
        // rightStack
        // 
        rightStack.ColumnCount = 1;
        rightStack.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rightStack.Controls.Add(lblSnippetTitle, 0, 0);
        rightStack.Controls.Add(snippetActionLayout, 0, 1);
        rightStack.Controls.Add(txtSnippet, 0, 2);
        rightStack.Controls.Add(lblSnippetHint, 0, 3);
        rightStack.Dock = DockStyle.Fill;
        rightStack.Location = new Point(14, 14);
        rightStack.Name = "rightStack";
        rightStack.RowCount = 4;
        rightStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
        rightStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        rightStack.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rightStack.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        rightStack.Size = new Size(744, 713);
        rightStack.TabIndex = 0;
        // 
        // lblSnippetTitle
        // 
        lblSnippetTitle.Dock = DockStyle.Fill;
        lblSnippetTitle.Font = new Font("Microsoft YaHei UI", 10.5F, FontStyle.Bold);
        lblSnippetTitle.Location = new Point(0, 0);
        lblSnippetTitle.Margin = new Padding(0, 0, 0, 8);
        lblSnippetTitle.Name = "lblSnippetTitle";
        lblSnippetTitle.Size = new Size(744, 22);
        lblSnippetTitle.TabIndex = 0;
        lblSnippetTitle.Text = "当前代码片段";
        // 
        // snippetActionLayout
        // 
        snippetActionLayout.ColumnCount = 2;
        snippetActionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        snippetActionLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        snippetActionLayout.Controls.Add(btnCopySnippet, 0, 0);
        snippetActionLayout.Controls.Add(btnClearSnippet, 1, 0);
        snippetActionLayout.Dock = DockStyle.Fill;
        snippetActionLayout.Location = new Point(0, 30);
        snippetActionLayout.Margin = new Padding(0);
        snippetActionLayout.Name = "snippetActionLayout";
        snippetActionLayout.RowCount = 1;
        snippetActionLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        snippetActionLayout.Size = new Size(744, 48);
        snippetActionLayout.TabIndex = 1;
        // 
        // btnCopySnippet
        // 
        btnCopySnippet.Dock = DockStyle.Fill;
        btnCopySnippet.Location = new Point(3, 3);
        btnCopySnippet.Name = "btnCopySnippet";
        btnCopySnippet.Size = new Size(366, 42);
        btnCopySnippet.TabIndex = 0;
        btnCopySnippet.Text = "复制代码";
        btnCopySnippet.UseVisualStyleBackColor = true;
        btnCopySnippet.Click += btnCopySnippet_Click;
        // 
        // btnClearSnippet
        // 
        btnClearSnippet.Dock = DockStyle.Fill;
        btnClearSnippet.Location = new Point(375, 3);
        btnClearSnippet.Name = "btnClearSnippet";
        btnClearSnippet.Size = new Size(366, 42);
        btnClearSnippet.TabIndex = 1;
        btnClearSnippet.Text = "清空代码";
        btnClearSnippet.UseVisualStyleBackColor = true;
        btnClearSnippet.Click += btnClearSnippet_Click;
        // 
        // txtSnippet
        // 
        txtSnippet.AcceptsReturn = true;
        txtSnippet.AcceptsTab = true;
        txtSnippet.Dock = DockStyle.Fill;
        txtSnippet.Font = new Font("Consolas", 11F);
        txtSnippet.Location = new Point(0, 78);
        txtSnippet.Margin = new Padding(0, 0, 0, 10);
        txtSnippet.Multiline = true;
        txtSnippet.Name = "txtSnippet";
        txtSnippet.ScrollBars = ScrollBars.Both;
        txtSnippet.Size = new Size(744, 597);
        txtSnippet.TabIndex = 2;
        txtSnippet.WordWrap = false;
        // 
        // lblSnippetHint
        // 
        lblSnippetHint.AutoSize = true;
        lblSnippetHint.Dock = DockStyle.Fill;
        lblSnippetHint.ForeColor = Color.DimGray;
        lblSnippetHint.Location = new Point(0, 685);
        lblSnippetHint.Margin = new Padding(0);
        lblSnippetHint.Name = "lblSnippetHint";
        lblSnippetHint.Size = new Size(744, 28);
        lblSnippetHint.TabIndex = 3;
        lblSnippetHint.Text = "这里展示的是可直接复制到用户 C# 项目中的调用代码。";
        // 
        // statusPanel
        // 
        statusPanel.BackColor = Color.White;
        statusPanel.Controls.Add(lblStatus);
        statusPanel.Dock = DockStyle.Fill;
        statusPanel.Location = new Point(16, 849);
        statusPanel.Margin = new Padding(0, 12, 0, 0);
        statusPanel.Name = "statusPanel";
        statusPanel.Padding = new Padding(16, 8, 16, 8);
        statusPanel.Size = new Size(1452, 36);
        statusPanel.TabIndex = 2;
        // 
        // lblStatus
        // 
        lblStatus.Dock = DockStyle.Fill;
        lblStatus.ForeColor = Color.DimGray;
        lblStatus.Location = new Point(16, 8);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(1420, 20);
        lblStatus.TabIndex = 0;
        lblStatus.Text = "就绪";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 17F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.FromArgb(245, 247, 250);
        ClientSize = new Size(1484, 901);
        Controls.Add(rootLayout);
        MinimumSize = new Size(1320, 820);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "C# 自动化工具台";
        rootLayout.ResumeLayout(false);
        headerPanel.ResumeLayout(false);
        workspaceLayout.ResumeLayout(false);
        leftCard.ResumeLayout(false);
        leftStack.ResumeLayout(false);
        grpHotKeys.ResumeLayout(false);
        grpCapture.ResumeLayout(false);
        captureLayout.ResumeLayout(false);
        captureLayout.PerformLayout();
        captureButtonLayout.ResumeLayout(false);
        grpMouseTools.ResumeLayout(false);
        mouseToolLayout.ResumeLayout(false);
        mouseToolLayout.PerformLayout();
        grpHistory.ResumeLayout(false);
        centerCard.ResumeLayout(false);
        centerStack.ResumeLayout(false);
        grpToolEntrance.ResumeLayout(false);
        grpGuide.ResumeLayout(false);
        rightCard.ResumeLayout(false);
        rightStack.ResumeLayout(false);
        rightStack.PerformLayout();
        snippetActionLayout.ResumeLayout(false);
        statusPanel.ResumeLayout(false);
        ResumeLayout(false);
    }
}
