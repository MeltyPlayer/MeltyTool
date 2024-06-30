using uni.ui.winforms.common;


namespace uni.ui.winforms.right_panel {
  partial class AnimationPlaybackPanel {
    /// <summary> 
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary> 
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      System.Windows.Forms.GroupBox PlaybackGroup;
      System.Windows.Forms.Label Label27;
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnimationPlaybackPanel));
      this.frameRateSelector_ = new System.Windows.Forms.NumericUpDown();
      this.elapsedFrames_ = new System.Windows.Forms.Label();
      this.elapsedSeconds_ = new System.Windows.Forms.Label();
      this.loopCheckBox_ = new System.Windows.Forms.CheckBox();
      this.pauseButton_ = new System.Windows.Forms.Button();
      this.playButton_ = new System.Windows.Forms.Button();
      this.frameTrackBar_ = new TransparentTrackBar();
      PlaybackGroup = new System.Windows.Forms.GroupBox();
      Label27 = new System.Windows.Forms.Label();
      PlaybackGroup.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.frameRateSelector_)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.frameTrackBar_)).BeginInit();
      this.SuspendLayout();
      // 
      // PlaybackGroup
      // 
      PlaybackGroup.Controls.Add(this.frameRateSelector_);
      PlaybackGroup.Controls.Add(Label27);
      PlaybackGroup.Controls.Add(this.elapsedFrames_);
      PlaybackGroup.Controls.Add(this.elapsedSeconds_);
      PlaybackGroup.Controls.Add(this.loopCheckBox_);
      PlaybackGroup.Controls.Add(this.pauseButton_);
      PlaybackGroup.Controls.Add(this.playButton_);
      PlaybackGroup.Controls.Add(this.frameTrackBar_);
      PlaybackGroup.Dock = System.Windows.Forms.DockStyle.Fill;
      PlaybackGroup.Location = new System.Drawing.Point(0, 0);
      PlaybackGroup.Name = "PlaybackGroup";
      PlaybackGroup.Size = new System.Drawing.Size(212, 117);
      PlaybackGroup.TabIndex = 17;
      PlaybackGroup.TabStop = false;
      PlaybackGroup.Text = "Playback";
      // 
      // frameRateSelector_
      // 
      this.frameRateSelector_.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.frameRateSelector_.Font = new System.Drawing.Font("Trebuchet MS", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.frameRateSelector_.Location = new System.Drawing.Point(134, 63);
      this.frameRateSelector_.Maximum = new decimal(new int[] {
            60,
            0,
            0,
            0});
      this.frameRateSelector_.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
      this.frameRateSelector_.Name = "frameRateSelector_";
      this.frameRateSelector_.Size = new System.Drawing.Size(40, 20);
      this.frameRateSelector_.TabIndex = 11;
      this.frameRateSelector_.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
      // 
      // Label27
      // 
      Label27.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      Label27.AutoSize = true;
      Label27.Location = new System.Drawing.Point(176, 65);
      Label27.Name = "Label27";
      Label27.Size = new System.Drawing.Size(27, 13);
      Label27.TabIndex = 12;
      Label27.Text = "FPS";
      // 
      // elapsedFrames_
      // 
      this.elapsedFrames_.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.elapsedFrames_.AutoSize = true;
      this.elapsedFrames_.Location = new System.Drawing.Point(160, 17);
      this.elapsedFrames_.Name = "elapsedFrames_";
      this.elapsedFrames_.Size = new System.Drawing.Size(24, 13);
      this.elapsedFrames_.TabIndex = 8;
      this.elapsedFrames_.Text = "0/0";
      this.elapsedFrames_.TextAlign = System.Drawing.ContentAlignment.TopRight;
      // 
      // elapsedSeconds_
      // 
      this.elapsedSeconds_.AutoSize = true;
      this.elapsedSeconds_.Location = new System.Drawing.Point(12, 17);
      this.elapsedSeconds_.Name = "elapsedSeconds_";
      this.elapsedSeconds_.Size = new System.Drawing.Size(27, 13);
      this.elapsedSeconds_.TabIndex = 7;
      this.elapsedSeconds_.Text = "0.0s";
      // 
      // loopCheckBox_
      // 
      this.loopCheckBox_.Image = ((System.Drawing.Image)(resources.GetObject("loopCheckBox_.Image")));
      this.loopCheckBox_.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.loopCheckBox_.Location = new System.Drawing.Point(16, 89);
      this.loopCheckBox_.Name = "loopCheckBox_";
      this.loopCheckBox_.Size = new System.Drawing.Size(70, 24);
      this.loopCheckBox_.TabIndex = 3;
      this.loopCheckBox_.Text = "Loop";
      this.loopCheckBox_.UseVisualStyleBackColor = true;
      // 
      // pauseButton_
      // 
      this.pauseButton_.Image = ((System.Drawing.Image)(resources.GetObject("pauseButton_.Image")));
      this.pauseButton_.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.pauseButton_.Location = new System.Drawing.Point(68, 59);
      this.pauseButton_.Name = "pauseButton_";
      this.pauseButton_.Size = new System.Drawing.Size(60, 28);
      this.pauseButton_.TabIndex = 2;
      this.pauseButton_.Text = "Pause";
      this.pauseButton_.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.pauseButton_.UseVisualStyleBackColor = true;
      // 
      // playButton_
      // 
      this.playButton_.Image = ((System.Drawing.Image)(resources.GetObject("playButton_.Image")));
      this.playButton_.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
      this.playButton_.Location = new System.Drawing.Point(15, 59);
      this.playButton_.Name = "playButton_";
      this.playButton_.Size = new System.Drawing.Size(51, 28);
      this.playButton_.TabIndex = 1;
      this.playButton_.Text = "Play";
      this.playButton_.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      this.playButton_.UseVisualStyleBackColor = true;
      // 
      // frameTrackBar_
      // 
      this.frameTrackBar_.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.frameTrackBar_.AutoSize = false;
      this.frameTrackBar_.BackColor = System.Drawing.SystemColors.Control;
      this.frameTrackBar_.LargeChange = 2;
      this.frameTrackBar_.Location = new System.Drawing.Point(8, 32);
      this.frameTrackBar_.Maximum = 1;
      this.frameTrackBar_.Name = "frameTrackBar_";
      this.frameTrackBar_.Size = new System.Drawing.Size(197, 24);
      this.frameTrackBar_.TabIndex = 14;
      // 
      // AnimationPlaybackPanel
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.Controls.Add(PlaybackGroup);
      this.Name = "AnimationPlaybackPanel";
      this.Size = new System.Drawing.Size(212, 117);
      PlaybackGroup.ResumeLayout(false);
      PlaybackGroup.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.frameRateSelector_)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.frameTrackBar_)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion
    internal System.Windows.Forms.Label elapsedFrames_;
    internal System.Windows.Forms.Label elapsedSeconds_;
    private System.Windows.Forms.NumericUpDown frameRateSelector_;
    private System.Windows.Forms.CheckBox loopCheckBox_;
    private System.Windows.Forms.Button pauseButton_;
    private System.Windows.Forms.Button playButton_;
    private TransparentTrackBar frameTrackBar_;
  }
}
