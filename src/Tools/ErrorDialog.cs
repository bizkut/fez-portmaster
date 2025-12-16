// Decompiled with JetBrains decompiler
// Type: FezGame.Tools.ErrorDialog
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

#nullable disable
namespace FezGame.Tools;

public class ErrorDialog : Form
{
  private IContainer components;
  private Label label1;
  private LinkLabel linkLabel1;
  private Label label2;
  private LinkLabel linkLabel2;
  private Button button1;

  public ErrorDialog() => this.InitializeComponent();

  private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
  {
    Process.Start("https://getsatisfaction.com/polytron/topics/support_for_intel_integrated_graphics_hardware");
  }

  private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
  {
    Process.Start("http://polytroncorporation.com/support/");
  }

  protected override void Dispose(bool disposing)
  {
    if (disposing && this.components != null)
      this.components.Dispose();
    base.Dispose(disposing);
  }

  private void InitializeComponent()
  {
    ComponentResourceManager componentResourceManager = new ComponentResourceManager(typeof (ErrorDialog));
    this.label1 = new Label();
    this.linkLabel1 = new LinkLabel();
    this.label2 = new Label();
    this.linkLabel2 = new LinkLabel();
    this.button1 = new Button();
    this.SuspendLayout();
    this.label1.Location = new Point(12, 9);
    this.label1.Name = "label1";
    this.label1.Size = new Size(512 /*0x0200*/, 189);
    this.label1.TabIndex = 0;
    this.label1.Text = componentResourceManager.GetString("label1.Text");
    this.label1.TextAlign = ContentAlignment.MiddleCenter;
    this.linkLabel1.AutoSize = true;
    this.linkLabel1.Location = new Point(159, 190);
    this.linkLabel1.Name = "linkLabel1";
    this.linkLabel1.Size = new Size(218, 13);
    this.linkLabel1.TabIndex = 2;
    this.linkLabel1.TabStop = true;
    this.linkLabel1.Text = "http://polytroncorporation.com/support/";
    this.linkLabel1.TextAlign = ContentAlignment.MiddleCenter;
    this.linkLabel1.LinkClicked += new LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
    this.label2.AutoSize = true;
    this.label2.Location = new Point(150, 149);
    this.label2.Name = "label2";
    this.label2.Size = new Size(0, 13);
    this.label2.TabIndex = 2;
    this.linkLabel2.AutoSize = true;
    this.linkLabel2.Location = new Point(28, 119);
    this.linkLabel2.Name = "linkLabel2";
    this.linkLabel2.Size = new Size(483, 13);
    this.linkLabel2.TabIndex = 3;
    this.linkLabel2.TabStop = true;
    this.linkLabel2.Text = "https://getsatisfaction.com/polytron/topics/support_for_intel_integrated_graphics_hardware";
    this.linkLabel2.LinkClicked += new LinkLabelLinkClickedEventHandler(this.linkLabel2_LinkClicked);
    this.button1.DialogResult = DialogResult.Cancel;
    this.button1.Location = new Point(231, 228);
    this.button1.Name = "button1";
    this.button1.Size = new Size(75, 23);
    this.button1.TabIndex = 1;
    this.button1.Text = "Close";
    this.button1.UseVisualStyleBackColor = true;
    this.AcceptButton = (IButtonControl) this.button1;
    this.AutoScaleDimensions = new SizeF(6f, 13f);
    this.AutoScaleMode = AutoScaleMode.Font;
    this.CancelButton = (IButtonControl) this.button1;
    this.ClientSize = new Size(536, 263);
    this.Controls.Add((Control) this.button1);
    this.Controls.Add((Control) this.linkLabel2);
    this.Controls.Add((Control) this.linkLabel1);
    this.Controls.Add((Control) this.label2);
    this.Controls.Add((Control) this.label1);
    this.Font = new Font("Segoe UI", 8.25f, FontStyle.Regular, GraphicsUnit.Point, (byte) 0);
    this.FormBorderStyle = FormBorderStyle.FixedDialog;
    this.Icon = (Icon) componentResourceManager.GetObject("$this.Icon");
    this.MaximizeBox = false;
    this.MinimizeBox = false;
    this.Name = nameof (ErrorDialog);
    this.StartPosition = FormStartPosition.CenterScreen;
    this.Text = "FEZ - Fatal Error";
    this.ResumeLayout(false);
    this.PerformLayout();
  }
}
