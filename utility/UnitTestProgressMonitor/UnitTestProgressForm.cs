using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Ionic.CopyData;
using UnitTestProgressMonitor.Properties;

namespace UnitTestProgressMonitor;

public class UnitTestProgressForm : Form
{
    private readonly string[] _cmdLineArgs;

    private readonly string _helpText = Resources.UnitTestProgressForm_HelpText;

    private readonly List<int> _maxFactor;
    private DateTime _startTime;
    private Timer _Timer;
    private readonly IContainer components = null;
    private Label labelTime;
    private Label lblTestName;
    private readonly List<ProgressBar> pb;
    private Transceiver transceiver;
    private TextBox txtStatus;

    public UnitTestProgressForm(string[] args)
    {
        InitializeComponent();
        Icon = new Icon(new MemoryStream(Resources.folder_archive_zip_22613));
        _cmdLineArgs = args;
        pb = new List<ProgressBar>();
        _maxFactor = new List<int>();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    private void AddProgressBar()
    {
        SuspendLayout();
        var num = 18;
        var count = pb.Count;
        MaximumSize = new Size(MaximumSize.Width, Height + num);
        Size = new Size(Width, Height + num);
        MinimumSize = new Size(MinimumSize.Width, Height);
        var progressBar = new ProgressBar();
        pb.Add(progressBar);
        _maxFactor.Add(0);
        progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        progressBar.Name = $"progressBar{count}";
        progressBar.Size = new Size(this.txtStatus.Width, num - 6);
        progressBar.Style = count == 0 ? ProgressBarStyle.Continuous : ProgressBarStyle.Blocks;
        progressBar.Location = new Point(10, 38 + count * num);
        progressBar.TabIndex = (count + 1) * 10;
        Controls.Add(progressBar);
        var txtStatus = this.txtStatus;
        var location = this.txtStatus.Location;
        var x = location.X;
        location = this.txtStatus.Location;
        var y = location.Y + num;
        var point = new Point(x, y);
        txtStatus.Location = point;
        ResumeLayout(false);
        PerformLayout();
    }

    private void copyData_DataReceived(object sender, DataReceivedEventArgs e)
    {
        var strArray = ((string)e.Data).Split(' ');
        for (var index1 = 0; index1 < strArray.Length; ++index1)
        {
            switch (strArray[index1])
            {
                case "test":
                    ++index1;
                    if (strArray.Length > index1)
                    {
                        strArray[0] = "";
                        SetTestName(string.Join(" ", strArray));
                    }

                    break;
                case "bars":
                    ++index1;
                    if (strArray.Length > index1)
                    {
                        try
                        {
                            var num = int.Parse(strArray[index1]);
                            for (var index2 = 0; index2 < num; ++index2)
                            {
                                AddProgressBar();
                            }
                        }
                        catch
                        {
                        }
                    }

                    break;
                case "pb":
                    ++index1;
                    if (strArray.Length > index1)
                    {
                        var ix = -1;
                        try
                        {
                            ix = int.Parse(strArray[index1]);
                        }
                        catch
                        {
                        }

                        if (ix < 0)
                        {
                            return;
                        }

                        ++index1;
                        if (strArray.Length <= index1)
                        {
                            return;
                        }

                        switch (strArray[index1])
                        {
                            case "step":
                                PbPerformStep(ix);
                                break;
                            case "max":
                                ++index1;
                                if (strArray.Length > index1)
                                {
                                    try
                                    {
                                        var maxValue = long.Parse(strArray[index1]);
                                        PbSetLimit(ix, maxValue);
                                    }
                                    catch
                                    {
                                    }
                                }

                                break;
                            case "value":
                                ++index1;
                                if (strArray.Length > index1)
                                {
                                    try
                                    {
                                        var num = long.Parse(strArray[index1]);
                                        PbSetValue(ix, num);
                                    }
                                    catch
                                    {
                                    }
                                }

                                break;
                        }
                    }

                    break;
                case "status":
                    ++index1;
                    if (strArray.Length > index1)
                    {
                        strArray[0] = "";
                        SetStatus(string.Join(" ", strArray));
                    }

                    break;
                case "stop":
                    Close();
                    break;
            }
        }
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        transceiver = new Transceiver();
        transceiver.AssignHandle(Handle);
        var str = (string)null;
        for (var index = 0; index < _cmdLineArgs.Length; ++index)
        {
            if (_cmdLineArgs[index] == "-channel")
            {
                ++index;
                if (_cmdLineArgs.Length > index)
                {
                    str = _cmdLineArgs[index];
                }
            }
        }

        if (str == null)
        {
            var num = (int)MessageBox.Show(_helpText, Text);
            Environment.Exit(1);
        }

        transceiver.Channel = str;
        transceiver.DataReceived += copyData_DataReceived;
        _Timer = new Timer();
        _Timer.Tick += TimerTick;
        _Timer.Interval = 1011;
        _Timer.Start();
        _startTime = DateTime.UtcNow;
    }

    private void InitializeComponent()
    {
        txtStatus = new TextBox();
        lblTestName = new Label();
        labelTime = new Label();
        SuspendLayout();
        txtStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        txtStatus.Location = new Point(10, 36);
        txtStatus.Name = "txtStatus";
        txtStatus.ReadOnly = true;
        txtStatus.Size = new Size(478, 20);
        txtStatus.TabIndex = 3;
        lblTestName.AutoSize = true;
        lblTestName.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, 0);
        lblTestName.Location = new Point(11, 9);
        lblTestName.Name = "lblTestName";
        lblTestName.Size = new Size(0, 20);
        lblTestName.TabIndex = 4;
        labelTime.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        labelTime.AutoSize = true;
        labelTime.Location = new Point(438, 14);
        labelTime.Name = "labelTime";
        labelTime.Size = new Size(49, 13);
        labelTime.TabIndex = 5;
        labelTime.Text = "00:00:00";
        AutoScaleDimensions = new SizeF(6f, 13f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(499, 62);
        Controls.Add(labelTime);
        Controls.Add(lblTestName);
        Controls.Add(txtStatus);
        MinimumSize = new Size(325, 62);
        MaximumSize = new Size(1024 /*0x0400*/, 102);
        Name = nameof(UnitTestProgressForm);
        Text = "Ionic's Unit Test Progress Monitor";
        TopMost = true;
        Load += Form1_Load;
        ResumeLayout(false);
        PerformLayout();
    }

    private void PbPerformStep(int ix)
    {
        if (ix >= pb.Count)
        {
            return;
        }

        if (pb[ix].InvokeRequired)
        {
            pb[ix].Invoke(new PbStep(PbPerformStep), ix);
        }
        else
        {
            pb[ix].PerformStep();
            if (ix < pb.Count - 1)
            {
                pb[ix + 1].Value = pb[ix + 1].Maximum = 1;
            }

            Update();
        }
    }

    private void PbSetLimit(int ix, long maxValue)
    {
        if (txtStatus.InvokeRequired)
        {
            txtStatus.Invoke(new PbSet(PbSetLimit), ix, maxValue);
        }
        else
        {
            lock (pb)
            {
                while (ix >= pb.Count)
                {
                    AddProgressBar();
                }
            }

            var maxValue1 = (long)int.MaxValue;
            _maxFactor[ix] = 0;
            if (maxValue < 0L)
            {
                maxValue *= -1L;
            }

            while (maxValue > maxValue1)
            {
                maxValue /= 2L;
                List<int> maxFactor;
                int index;
                (maxFactor = _maxFactor)[index = ix] = maxFactor[index] + 1;
            }

            pb[ix].Minimum = 0;
            pb[ix].Maximum = (int)maxValue;
            pb[ix].Step = 1;
            pb[ix].Value = 0;
            if (ix < pb.Count - 1)
            {
                pb[ix + 1].Value = 0;
                pb[ix + 1].Maximum = 1;
            }

            Update();
        }
    }

    private void PbSetValue(int ix, long value)
    {
        if (ix >= pb.Count)
        {
            return;
        }

        if (pb[ix].InvokeRequired)
        {
            pb[ix].Invoke(new PbSet(PbSetValue), ix, value);
        }
        else
        {
            var num = (int)(value >> _maxFactor[ix]);
            pb[ix].Value = num >= pb[ix].Maximum ? pb[ix].Maximum : num;
            Update();
        }
    }

    private void SetStatus(string s)
    {
        if (txtStatus.InvokeRequired)
        {
            txtStatus.Invoke(new StringSet(SetStatus), s);
        }
        else
        {
            txtStatus.Text = s;
            Update();
        }
    }

    private void SetTestName(string s)
    {
        if (lblTestName.InvokeRequired)
        {
            lblTestName.Invoke(new StringSet(SetTestName), s);
        }
        else
        {
            lblTestName.Text = s;
            Update();
        }
    }

    private void TimerTick(object o, EventArgs e)
    {
        var timeSpan = DateTime.UtcNow - _startTime;
        labelTime.Text = $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
    }

    private delegate void PbStep(int ix);

    private delegate void PbSet(int ix, long value);

    private delegate void StringSet(string s);
}