using Ionic.CopyData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

#nullable disable
namespace UnitTestProgressMonitor;

public class Form1 : Form
{
  private Transceiver transceiver;
  private List<int> _maxFactor;
  private string[] _cmdLineArgs;
  private List<ProgressBar> pb;
  private Timer _Timer;
  private DateTime _startTime;
  private string _helpText = "This is a simple progress bar displayer.  It displays progress based\non messages sent via WM_COPYDATA. Useful for visually tracking the \nprogress of individual Visual Studio Unit Tests.\nOn startup, specify a channel with -channel <channelName> \n\nThis app then calls SetProp(channelName).  Subsequently, apps can do EnumWindows,\nlook for that Prop, and send the window these messages to affect the display: \n   test <test-name>\n   bars <N>\n   pb <N> max   <int64-hilimit>\n   pb <N> value <int64-value>\n   pb <N> step\n   status <txt>\n   stop\n";
  private IContainer components = (IContainer) null;
  private TextBox txtStatus;
  private Label lblTestName;
  private Label labelTime;

  public Form1(string[] args)
  {
    this.InitializeComponent();
    this._cmdLineArgs = args;
    this.pb = new List<ProgressBar>();
    this._maxFactor = new List<int>();
  }

  private void Form1_Load(object sender, EventArgs e)
  {
    this.transceiver = new Transceiver();
    ((NativeWindow) this.transceiver).AssignHandle(this.Handle);
    string str = (string) null;
    for (int index = 0; index < this._cmdLineArgs.Length; ++index)
    {
      if (this._cmdLineArgs[index] == "-channel")
      {
        ++index;
        if (this._cmdLineArgs.Length > index)
          str = this._cmdLineArgs[index];
      }
    }
    if (str == null)
    {
      int num = (int) MessageBox.Show(this._helpText, this.Text);
      Environment.Exit(1);
    }
    this.transceiver.Channel = str;
    this.transceiver.DataReceived += new EventHandler<DataReceivedEventArgs>(this.copyData_DataReceived);
    this._Timer = new Timer();
    this._Timer.Tick += new EventHandler(this.TimerTick);
    this._Timer.Interval = 1011;
    this._Timer.Start();
    this._startTime = DateTime.UtcNow;
  }

  private void TimerTick(object o, EventArgs e)
  {
    TimeSpan timeSpan = DateTime.UtcNow - this._startTime;
    this.labelTime.Text = $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
  }

  private void copyData_DataReceived(object sender, DataReceivedEventArgs e)
  {
    string[] strArray = ((string) e.Data).Split(' ');
    for (int index1 = 0; index1 < strArray.Length; ++index1)
    {
      switch (strArray[index1])
      {
        case "test":
          ++index1;
          if (strArray.Length > index1)
          {
            strArray[0] = "";
            this.SetTestName(string.Join(" ", strArray));
            break;
          }
          break;
        case "bars":
          ++index1;
          if (strArray.Length > index1)
          {
            try
            {
              int num = int.Parse(strArray[index1]);
              for (int index2 = 0; index2 < num; ++index2)
                this.AddProgressBar();
            }
            catch
            {
            }
            break;
          }
          break;
        case "pb":
          ++index1;
          if (strArray.Length > index1)
          {
            int ix = -1;
            try
            {
              ix = int.Parse(strArray[index1]);
            }
            catch
            {
            }
            if (ix < 0)
              return;
            ++index1;
            if (strArray.Length <= index1)
              return;
            switch (strArray[index1])
            {
              case "step":
                this.PbPerformStep(ix);
                break;
              case "max":
                ++index1;
                if (strArray.Length > index1)
                {
                  try
                  {
                    long maxValue = long.Parse(strArray[index1]);
                    this.PbSetLimit(ix, maxValue);
                  }
                  catch
                  {
                  }
                  break;
                }
                break;
              case "value":
                ++index1;
                if (strArray.Length > index1)
                {
                  try
                  {
                    long num = long.Parse(strArray[index1]);
                    this.PbSetValue(ix, num);
                  }
                  catch
                  {
                  }
                  break;
                }
                break;
            }
            break;
          }
          break;
        case "status":
          ++index1;
          if (strArray.Length > index1)
          {
            strArray[0] = "";
            this.SetStatus(string.Join(" ", strArray));
            break;
          }
          break;
        case "stop":
          this.Close();
          break;
      }
    }
  }

  private void AddProgressBar()
  {
    this.SuspendLayout();
    int num = 18;
    int count = this.pb.Count;
    this.MaximumSize = new Size(this.MaximumSize.Width, this.Height + num);
    this.Size = new Size(this.Width, this.Height + num);
    this.MinimumSize = new Size(this.MinimumSize.Width, this.Height);
    ProgressBar progressBar = new ProgressBar();
    this.pb.Add(progressBar);
    this._maxFactor.Add(0);
    progressBar.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
    progressBar.Name = $"progressBar{count}";
    progressBar.Size = new Size(this.txtStatus.Width, num - 6);
    progressBar.Style = count == 0 ? ProgressBarStyle.Continuous : ProgressBarStyle.Blocks;
    progressBar.Location = new Point(10, 38 + count * num);
    progressBar.TabIndex = (count + 1) * 10;
    this.Controls.Add((Control) progressBar);
    TextBox txtStatus = this.txtStatus;
    Point location = this.txtStatus.Location;
    int x = location.X;
    location = this.txtStatus.Location;
    int y = location.Y + num;
    Point point = new Point(x, y);
    txtStatus.Location = point;
    this.ResumeLayout(false);
    this.PerformLayout();
  }

  private void SetStatus(string s)
  {
    if (this.txtStatus.InvokeRequired)
    {
      this.txtStatus.Invoke((Delegate) new Form1.StringSet(this.SetStatus), (object) s);
    }
    else
    {
      this.txtStatus.Text = s;
      this.Update();
    }
  }

  private void SetTestName(string s)
  {
    if (this.lblTestName.InvokeRequired)
    {
      this.lblTestName.Invoke((Delegate) new Form1.StringSet(this.SetTestName), (object) s);
    }
    else
    {
      this.lblTestName.Text = s;
      this.Update();
    }
  }

  private void PbSetLimit(int ix, long maxValue)
  {
    if (this.txtStatus.InvokeRequired)
    {
      this.txtStatus.Invoke((Delegate) new Form1.PbSet(this.PbSetLimit), (object) ix, (object) maxValue);
    }
    else
    {
      lock (this.pb)
      {
        while (ix >= this.pb.Count)
          this.AddProgressBar();
      }
      long maxValue1 = (long) int.MaxValue;
      this._maxFactor[ix] = 0;
      if (maxValue < 0L)
        maxValue *= -1L;
      while (maxValue > maxValue1)
      {
        maxValue /= 2L;
        List<int> maxFactor;
        int index;
        (maxFactor = this._maxFactor)[index = ix] = maxFactor[index] + 1;
      }
      this.pb[ix].Minimum = 0;
      this.pb[ix].Maximum = (int) maxValue;
      this.pb[ix].Step = 1;
      this.pb[ix].Value = 0;
      if (ix < this.pb.Count - 1)
      {
        this.pb[ix + 1].Value = 0;
        this.pb[ix + 1].Maximum = 1;
      }
      this.Update();
    }
  }

  private void PbSetValue(int ix, long value)
  {
    if (ix >= this.pb.Count)
      return;
    if (this.pb[ix].InvokeRequired)
    {
      this.pb[ix].Invoke((Delegate) new Form1.PbSet(this.PbSetValue), (object) ix, (object) value);
    }
    else
    {
      int num = (int) (value >> this._maxFactor[ix]);
      this.pb[ix].Value = num >= this.pb[ix].Maximum ? this.pb[ix].Maximum : num;
      this.Update();
    }
  }

  private void PbPerformStep(int ix)
  {
    if (ix >= this.pb.Count)
      return;
    if (this.pb[ix].InvokeRequired)
    {
      this.pb[ix].Invoke((Delegate) new Form1.PbStep(this.PbPerformStep), (object) ix);
    }
    else
    {
      this.pb[ix].PerformStep();
      if (ix < this.pb.Count - 1)
        this.pb[ix + 1].Value = this.pb[ix + 1].Maximum = 1;
      this.Update();
    }
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    this.txtStatus = new TextBox();
    this.lblTestName = new Label();
    this.labelTime = new Label();
    this.SuspendLayout();
    this.txtStatus.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
    this.txtStatus.Location = new Point(10, 36);
    this.txtStatus.Name = "txtStatus";
    this.txtStatus.ReadOnly = true;
    this.txtStatus.Size = new Size(478, 20);
    this.txtStatus.TabIndex = 3;
    this.lblTestName.AutoSize = true;
    this.lblTestName.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
    this.lblTestName.Location = new Point(11, 9);
    this.lblTestName.Name = "lblTestName";
    this.lblTestName.Size = new Size(0, 20);
    this.lblTestName.TabIndex = 4;
    this.labelTime.Anchor = AnchorStyles.Top | AnchorStyles.Right;
    this.labelTime.AutoSize = true;
    this.labelTime.Location = new Point(438, 14);
    this.labelTime.Name = "labelTime";
    this.labelTime.Size = new Size(49, 13);
    this.labelTime.TabIndex = 5;
    this.labelTime.Text = "00:00:00";
    this.AutoScaleDimensions = new SizeF(6f, 13f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.ClientSize = new Size(499, 62);
    this.Controls.Add((Control) this.labelTime);
    this.Controls.Add((Control) this.lblTestName);
    this.Controls.Add((Control) this.txtStatus);
    this.MinimumSize = new Size(325, 62);
    this.MaximumSize = new Size(1024 /*0x0400*/, 102);
    this.Name = nameof (Form1);
    this.Text = "Ionic's Unit Test Progress Monitor";
    this.TopMost = true;
    this.Load += new EventHandler(this.Form1_Load);
    this.ResumeLayout(false);
    this.PerformLayout();
  }

  private delegate void PbStep(int ix);

  private delegate void PbSet(int ix, long value);

  private delegate void StringSet(string s);
}
