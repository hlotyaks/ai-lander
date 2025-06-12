namespace LanderGame;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Timer gameTimer;
    private System.Windows.Forms.ComboBox envComboBox;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.gameTimer = new System.Windows.Forms.Timer(this.components);
        this.envComboBox = new System.Windows.Forms.ComboBox();
        // 
        // envComboBox
        // 
        this.envComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.envComboBox.Items.AddRange(new object[] {
            "Moon (0.0005)",
            "Earth (0.001)",
            "Mars (0.00037)"});
        this.envComboBox.SelectedIndex = 1;
        this.envComboBox.Location = new System.Drawing.Point(12, 12);
        this.envComboBox.Name = "envComboBox";
        this.envComboBox.Size = new System.Drawing.Size(150, 23);
        this.envComboBox.TabIndex = 0;
        this.envComboBox.SelectedIndexChanged += new System.EventHandler(this.envComboBox_SelectedIndexChanged);
        // 
        // gameTimer
        // 
        this.gameTimer.Interval = 16;
        this.gameTimer.Tick += new System.EventHandler(this.gameTimer_Tick);
        // 
        // Form1
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 450);
        this.Controls.Add(this.envComboBox);
        this.KeyPreview = true;
        this.DoubleBuffered = true;
        this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
        this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
        this.Text = "Lunar Lander";
    }

    #endregion
}
