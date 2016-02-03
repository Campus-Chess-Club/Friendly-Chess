namespace Friendly_Chess
{
    partial class frmChess
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
            this.lblBoard = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.lblMoves = new System.Windows.Forms.Label();
            this.btnMove = new System.Windows.Forms.Button();
            this.lblPending = new System.Windows.Forms.Label();
            this.chkWhite = new System.Windows.Forms.CheckBox();
            this.chkBlack = new System.Windows.Forms.CheckBox();
            this.lblOnTurn = new System.Windows.Forms.Label();
            this.lblPly = new System.Windows.Forms.Label();
            this.btnMoves = new System.Windows.Forms.Button();
            this.chkRotate = new System.Windows.Forms.CheckBox();
            this.lblValue = new System.Windows.Forms.Label();
            this.tbrDepthWhite = new System.Windows.Forms.TrackBar();
            this.tbrDepthBlack = new System.Windows.Forms.TrackBar();
            this.btnTest = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.directoryEntry1 = new System.DirectoryServices.DirectoryEntry();
            this.btnLines = new System.Windows.Forms.Button();
            this.btnClear = new System.Windows.Forms.Button();
            this.btnPerft = new System.Windows.Forms.Button();
            this.btnPerftLines = new System.Windows.Forms.Button();
            this.btnClearLog = new System.Windows.Forms.Button();
            this.txtFen = new System.Windows.Forms.TextBox();
            this.btnImportFEN = new System.Windows.Forms.Button();
            this.btnExportFEN = new System.Windows.Forms.Button();
            this.txtMove = new System.Windows.Forms.TextBox();
            this.btnReverse = new System.Windows.Forms.Button();
            this.lblDepth = new System.Windows.Forms.Label();
            this.btnStopWatch = new System.Windows.Forms.Button();
            this.btnZobrist = new System.Windows.Forms.Button();
            this.btnQPerft = new System.Windows.Forms.Button();
            this.btnEstimate = new System.Windows.Forms.Button();
            this.btnTerminal = new System.Windows.Forms.Button();
            this.btnGame = new System.Windows.Forms.Button();
            this.chkPause = new System.Windows.Forms.CheckBox();
            this.btnPerftSuite = new System.Windows.Forms.Button();
            this.btnLine = new System.Windows.Forms.Button();
            this.optN = new System.Windows.Forms.RadioButton();
            this.optB = new System.Windows.Forms.RadioButton();
            this.optR = new System.Windows.Forms.RadioButton();
            this.optQ = new System.Windows.Forms.RadioButton();
            this.lblClock = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.tbrDepthWhite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbrDepthBlack)).BeginInit();
            this.SuspendLayout();
            // 
            // lblBoard
            // 
            this.lblBoard.AutoSize = true;
            this.lblBoard.Font = new System.Drawing.Font("Chess Merida Unicode", 24F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBoard.Location = new System.Drawing.Point(582, 239);
            this.lblBoard.Name = "lblBoard";
            this.lblBoard.Size = new System.Drawing.Size(483, 32);
            this.lblBoard.TabIndex = 0;
            this.lblBoard.Text = "\" ?♟♞♚♝♜♛?♙?♘♔♗♖♕\"";
            this.lblBoard.Visible = false;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(12, 20);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 1;
            this.btnStart.Text = "Start Game";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // lblMoves
            // 
            this.lblMoves.AutoSize = true;
            this.lblMoves.Font = new System.Drawing.Font("Miriam Fixed", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lblMoves.Location = new System.Drawing.Point(518, 18);
            this.lblMoves.Name = "lblMoves";
            this.lblMoves.Size = new System.Drawing.Size(39, 13);
            this.lblMoves.TabIndex = 2;
            this.lblMoves.Text = "a2a3";
            this.lblMoves.Click += new System.EventHandler(this.lblMoves_Click);
            this.lblMoves.DragDrop += new System.Windows.Forms.DragEventHandler(this.lblMoves_DragDrop);
            // 
            // btnMove
            // 
            this.btnMove.Location = new System.Drawing.Point(567, 399);
            this.btnMove.Name = "btnMove";
            this.btnMove.Size = new System.Drawing.Size(75, 23);
            this.btnMove.TabIndex = 3;
            this.btnMove.Text = "Submit Move";
            this.btnMove.UseVisualStyleBackColor = true;
            this.btnMove.Click += new System.EventHandler(this.btnMove_Click);
            // 
            // lblPending
            // 
            this.lblPending.AutoSize = true;
            this.lblPending.Location = new System.Drawing.Point(219, 20);
            this.lblPending.Name = "lblPending";
            this.lblPending.Size = new System.Drawing.Size(46, 13);
            this.lblPending.TabIndex = 4;
            this.lblPending.Text = "Pending";
            // 
            // chkWhite
            // 
            this.chkWhite.AutoSize = true;
            this.chkWhite.Location = new System.Drawing.Point(104, 28);
            this.chkWhite.Name = "chkWhite";
            this.chkWhite.Size = new System.Drawing.Size(102, 17);
            this.chkWhite.TabIndex = 5;
            this.chkWhite.Text = "White Computer";
            this.chkWhite.UseVisualStyleBackColor = true;
            this.chkWhite.CheckedChanged += new System.EventHandler(this.chkWhite_CheckedChanged);
            // 
            // chkBlack
            // 
            this.chkBlack.AutoSize = true;
            this.chkBlack.Location = new System.Drawing.Point(104, 48);
            this.chkBlack.Name = "chkBlack";
            this.chkBlack.Size = new System.Drawing.Size(101, 17);
            this.chkBlack.TabIndex = 6;
            this.chkBlack.Text = "Black Computer";
            this.chkBlack.UseVisualStyleBackColor = true;
            this.chkBlack.CheckedChanged += new System.EventHandler(this.chkBlack_CheckedChanged);
            // 
            // lblOnTurn
            // 
            this.lblOnTurn.AutoSize = true;
            this.lblOnTurn.Location = new System.Drawing.Point(219, 41);
            this.lblOnTurn.Name = "lblOnTurn";
            this.lblOnTurn.Size = new System.Drawing.Size(29, 13);
            this.lblOnTurn.TabIndex = 7;
            this.lblOnTurn.Text = "Turn";
            // 
            // lblPly
            // 
            this.lblPly.AutoSize = true;
            this.lblPly.Location = new System.Drawing.Point(219, 62);
            this.lblPly.Name = "lblPly";
            this.lblPly.Size = new System.Drawing.Size(21, 13);
            this.lblPly.TabIndex = 8;
            this.lblPly.Text = "Ply";
            // 
            // btnMoves
            // 
            this.btnMoves.Location = new System.Drawing.Point(311, 20);
            this.btnMoves.Name = "btnMoves";
            this.btnMoves.Size = new System.Drawing.Size(75, 23);
            this.btnMoves.TabIndex = 9;
            this.btnMoves.Text = "Test Moves";
            this.btnMoves.UseVisualStyleBackColor = true;
            this.btnMoves.Click += new System.EventHandler(this.btnMoves_Click);
            // 
            // chkRotate
            // 
            this.chkRotate.AutoSize = true;
            this.chkRotate.Location = new System.Drawing.Point(104, 71);
            this.chkRotate.Name = "chkRotate";
            this.chkRotate.Size = new System.Drawing.Size(89, 17);
            this.chkRotate.TabIndex = 10;
            this.chkRotate.Text = "Rotate Board";
            this.chkRotate.UseVisualStyleBackColor = true;
            this.chkRotate.CheckedChanged += new System.EventHandler(this.chkRotate_CheckedChanged);
            // 
            // lblValue
            // 
            this.lblValue.AutoSize = true;
            this.lblValue.Location = new System.Drawing.Point(308, 62);
            this.lblValue.Name = "lblValue";
            this.lblValue.Size = new System.Drawing.Size(13, 13);
            this.lblValue.TabIndex = 11;
            this.lblValue.Text = "0";
            // 
            // tbrDepthWhite
            // 
            this.tbrDepthWhite.Location = new System.Drawing.Point(411, 9);
            this.tbrDepthWhite.Name = "tbrDepthWhite";
            this.tbrDepthWhite.Size = new System.Drawing.Size(104, 45);
            this.tbrDepthWhite.TabIndex = 12;
            this.tbrDepthWhite.Value = 4;
            this.tbrDepthWhite.Scroll += new System.EventHandler(this.tbrDepthWhite_Scroll);
            // 
            // tbrDepthBlack
            // 
            this.tbrDepthBlack.Location = new System.Drawing.Point(411, 52);
            this.tbrDepthBlack.Name = "tbrDepthBlack";
            this.tbrDepthBlack.Size = new System.Drawing.Size(104, 45);
            this.tbrDepthBlack.TabIndex = 13;
            this.tbrDepthBlack.Value = 4;
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(567, 14);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 14;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // txtLog
            // 
            this.txtLog.Location = new System.Drawing.Point(743, 14);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(432, 521);
            this.txtLog.TabIndex = 15;
            this.txtLog.TextChanged += new System.EventHandler(this.txtLog_TextChanged);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(567, 44);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 16;
            this.button1.Text = "Debug";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // btnLines
            // 
            this.btnLines.Location = new System.Drawing.Point(567, 74);
            this.btnLines.Name = "btnLines";
            this.btnLines.Size = new System.Drawing.Size(75, 23);
            this.btnLines.TabIndex = 17;
            this.btnLines.Text = "Lines";
            this.btnLines.UseVisualStyleBackColor = true;
            this.btnLines.Click += new System.EventHandler(this.btnLines_Click);
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(567, 154);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(75, 23);
            this.btnClear.TabIndex = 18;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnPerft
            // 
            this.btnPerft.Location = new System.Drawing.Point(662, 52);
            this.btnPerft.Name = "btnPerft";
            this.btnPerft.Size = new System.Drawing.Size(75, 23);
            this.btnPerft.TabIndex = 19;
            this.btnPerft.Text = "Perft";
            this.btnPerft.UseVisualStyleBackColor = true;
            this.btnPerft.Click += new System.EventHandler(this.btnPerft_Click);
            // 
            // btnPerftLines
            // 
            this.btnPerftLines.Location = new System.Drawing.Point(662, 81);
            this.btnPerftLines.Name = "btnPerftLines";
            this.btnPerftLines.Size = new System.Drawing.Size(75, 23);
            this.btnPerftLines.TabIndex = 20;
            this.btnPerftLines.Text = "Perft Lines";
            this.btnPerftLines.UseVisualStyleBackColor = true;
            this.btnPerftLines.Click += new System.EventHandler(this.btnPerftLines_Click);
            // 
            // btnClearLog
            // 
            this.btnClearLog.Location = new System.Drawing.Point(567, 183);
            this.btnClearLog.Name = "btnClearLog";
            this.btnClearLog.Size = new System.Drawing.Size(75, 23);
            this.btnClearLog.TabIndex = 21;
            this.btnClearLog.Text = "Clear Log";
            this.btnClearLog.UseVisualStyleBackColor = true;
            this.btnClearLog.Click += new System.EventHandler(this.btnClearLog_Click);
            // 
            // txtFen
            // 
            this.txtFen.Location = new System.Drawing.Point(567, 541);
            this.txtFen.Name = "txtFen";
            this.txtFen.Size = new System.Drawing.Size(377, 20);
            this.txtFen.TabIndex = 22;
            // 
            // btnImportFEN
            // 
            this.btnImportFEN.Location = new System.Drawing.Point(567, 512);
            this.btnImportFEN.Name = "btnImportFEN";
            this.btnImportFEN.Size = new System.Drawing.Size(75, 23);
            this.btnImportFEN.TabIndex = 23;
            this.btnImportFEN.Text = "Import FEN";
            this.btnImportFEN.UseVisualStyleBackColor = true;
            this.btnImportFEN.Click += new System.EventHandler(this.btnImportFEN_Click);
            // 
            // btnExportFEN
            // 
            this.btnExportFEN.Location = new System.Drawing.Point(567, 483);
            this.btnExportFEN.Name = "btnExportFEN";
            this.btnExportFEN.Size = new System.Drawing.Size(75, 23);
            this.btnExportFEN.TabIndex = 24;
            this.btnExportFEN.Text = "Export FEN";
            this.btnExportFEN.UseVisualStyleBackColor = true;
            this.btnExportFEN.Click += new System.EventHandler(this.btnExportFEN_Click);
            // 
            // txtMove
            // 
            this.txtMove.Location = new System.Drawing.Point(567, 457);
            this.txtMove.Name = "txtMove";
            this.txtMove.Size = new System.Drawing.Size(130, 20);
            this.txtMove.TabIndex = 25;
            // 
            // btnReverse
            // 
            this.btnReverse.Location = new System.Drawing.Point(567, 428);
            this.btnReverse.Name = "btnReverse";
            this.btnReverse.Size = new System.Drawing.Size(75, 23);
            this.btnReverse.TabIndex = 26;
            this.btnReverse.Text = "Reverse";
            this.btnReverse.UseVisualStyleBackColor = true;
            this.btnReverse.Click += new System.EventHandler(this.btnReverse_Click);
            // 
            // lblDepth
            // 
            this.lblDepth.AutoSize = true;
            this.lblDepth.Location = new System.Drawing.Point(392, 13);
            this.lblDepth.Name = "lblDepth";
            this.lblDepth.Size = new System.Drawing.Size(13, 13);
            this.lblDepth.TabIndex = 27;
            this.lblDepth.Text = "4";
            // 
            // btnStopWatch
            // 
            this.btnStopWatch.Location = new System.Drawing.Point(12, 57);
            this.btnStopWatch.Name = "btnStopWatch";
            this.btnStopWatch.Size = new System.Drawing.Size(75, 23);
            this.btnStopWatch.TabIndex = 28;
            this.btnStopWatch.Text = "StopWatch";
            this.btnStopWatch.UseVisualStyleBackColor = true;
            this.btnStopWatch.Click += new System.EventHandler(this.btnStopWatch_Click);
            // 
            // btnZobrist
            // 
            this.btnZobrist.Location = new System.Drawing.Point(567, 274);
            this.btnZobrist.Name = "btnZobrist";
            this.btnZobrist.Size = new System.Drawing.Size(75, 23);
            this.btnZobrist.TabIndex = 29;
            this.btnZobrist.Text = "Zobrist";
            this.btnZobrist.UseVisualStyleBackColor = true;
            this.btnZobrist.Click += new System.EventHandler(this.btnZobrist_Click);
            // 
            // btnQPerft
            // 
            this.btnQPerft.Location = new System.Drawing.Point(662, 112);
            this.btnQPerft.Name = "btnQPerft";
            this.btnQPerft.Size = new System.Drawing.Size(75, 23);
            this.btnQPerft.TabIndex = 30;
            this.btnQPerft.Text = "QPerft";
            this.btnQPerft.UseVisualStyleBackColor = true;
            this.btnQPerft.Click += new System.EventHandler(this.btnQPerft_Click);
            // 
            // btnEstimate
            // 
            this.btnEstimate.Location = new System.Drawing.Point(662, 141);
            this.btnEstimate.Name = "btnEstimate";
            this.btnEstimate.Size = new System.Drawing.Size(75, 23);
            this.btnEstimate.TabIndex = 31;
            this.btnEstimate.Text = "Estimate";
            this.btnEstimate.UseVisualStyleBackColor = true;
            this.btnEstimate.Click += new System.EventHandler(this.btnEstimate_Click);
            // 
            // btnTerminal
            // 
            this.btnTerminal.Location = new System.Drawing.Point(567, 300);
            this.btnTerminal.Name = "btnTerminal";
            this.btnTerminal.Size = new System.Drawing.Size(75, 23);
            this.btnTerminal.TabIndex = 32;
            this.btnTerminal.Text = "Terminal Value";
            this.btnTerminal.UseVisualStyleBackColor = true;
            this.btnTerminal.Click += new System.EventHandler(this.btnTerminal_Click);
            // 
            // btnGame
            // 
            this.btnGame.Location = new System.Drawing.Point(567, 228);
            this.btnGame.Name = "btnGame";
            this.btnGame.Size = new System.Drawing.Size(75, 23);
            this.btnGame.TabIndex = 33;
            this.btnGame.Text = "Game";
            this.btnGame.UseVisualStyleBackColor = true;
            this.btnGame.Click += new System.EventHandler(this.btnGame_Click);
            // 
            // chkPause
            // 
            this.chkPause.AutoSize = true;
            this.chkPause.Location = new System.Drawing.Point(104, 5);
            this.chkPause.Name = "chkPause";
            this.chkPause.Size = new System.Drawing.Size(56, 17);
            this.chkPause.TabIndex = 34;
            this.chkPause.Text = "Pause";
            this.chkPause.UseVisualStyleBackColor = true;
            // 
            // btnPerftSuite
            // 
            this.btnPerftSuite.Location = new System.Drawing.Point(662, 12);
            this.btnPerftSuite.Name = "btnPerftSuite";
            this.btnPerftSuite.Size = new System.Drawing.Size(75, 23);
            this.btnPerftSuite.TabIndex = 35;
            this.btnPerftSuite.Text = "Perft Suite";
            this.btnPerftSuite.UseVisualStyleBackColor = true;
            this.btnPerftSuite.Click += new System.EventHandler(this.btnPerftSuite_Click);
            // 
            // btnLine
            // 
            this.btnLine.Location = new System.Drawing.Point(567, 103);
            this.btnLine.Name = "btnLine";
            this.btnLine.Size = new System.Drawing.Size(75, 23);
            this.btnLine.TabIndex = 36;
            this.btnLine.Text = "Line";
            this.btnLine.UseVisualStyleBackColor = true;
            this.btnLine.Click += new System.EventHandler(this.btnLine_Click);
            // 
            // optN
            // 
            this.optN.AutoSize = true;
            this.optN.Location = new System.Drawing.Point(271, 72);
            this.optN.Name = "optN";
            this.optN.Size = new System.Drawing.Size(33, 17);
            this.optN.TabIndex = 48;
            this.optN.Text = "N";
            this.optN.UseVisualStyleBackColor = true;
            // 
            // optB
            // 
            this.optB.AutoSize = true;
            this.optB.Location = new System.Drawing.Point(272, 49);
            this.optB.Name = "optB";
            this.optB.Size = new System.Drawing.Size(32, 17);
            this.optB.TabIndex = 47;
            this.optB.Text = "B";
            this.optB.UseVisualStyleBackColor = true;
            // 
            // optR
            // 
            this.optR.AutoSize = true;
            this.optR.Location = new System.Drawing.Point(271, 28);
            this.optR.Name = "optR";
            this.optR.Size = new System.Drawing.Size(33, 17);
            this.optR.TabIndex = 46;
            this.optR.Text = "R";
            this.optR.UseVisualStyleBackColor = true;
            // 
            // optQ
            // 
            this.optQ.AutoSize = true;
            this.optQ.Checked = true;
            this.optQ.Location = new System.Drawing.Point(272, 9);
            this.optQ.Name = "optQ";
            this.optQ.Size = new System.Drawing.Size(33, 17);
            this.optQ.TabIndex = 45;
            this.optQ.TabStop = true;
            this.optQ.Text = "Q";
            this.optQ.UseVisualStyleBackColor = true;
            // 
            // lblClock
            // 
            this.lblClock.AutoSize = true;
            this.lblClock.Font = new System.Drawing.Font("Miriam Fixed", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(177)));
            this.lblClock.Location = new System.Drawing.Point(662, 183);
            this.lblClock.Name = "lblClock";
            this.lblClock.Size = new System.Drawing.Size(74, 20);
            this.lblClock.TabIndex = 49;
            this.lblClock.Text = "00:00";
            // 
            // frmChess
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1291, 591);
            this.Controls.Add(this.lblClock);
            this.Controls.Add(this.optN);
            this.Controls.Add(this.optB);
            this.Controls.Add(this.optR);
            this.Controls.Add(this.optQ);
            this.Controls.Add(this.btnLine);
            this.Controls.Add(this.btnPerftSuite);
            this.Controls.Add(this.chkPause);
            this.Controls.Add(this.btnGame);
            this.Controls.Add(this.btnTerminal);
            this.Controls.Add(this.btnEstimate);
            this.Controls.Add(this.btnQPerft);
            this.Controls.Add(this.btnZobrist);
            this.Controls.Add(this.btnStopWatch);
            this.Controls.Add(this.lblDepth);
            this.Controls.Add(this.btnReverse);
            this.Controls.Add(this.txtMove);
            this.Controls.Add(this.btnExportFEN);
            this.Controls.Add(this.btnImportFEN);
            this.Controls.Add(this.txtFen);
            this.Controls.Add(this.btnClearLog);
            this.Controls.Add(this.btnPerftLines);
            this.Controls.Add(this.btnPerft);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.btnLines);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.tbrDepthBlack);
            this.Controls.Add(this.tbrDepthWhite);
            this.Controls.Add(this.lblValue);
            this.Controls.Add(this.chkRotate);
            this.Controls.Add(this.btnMoves);
            this.Controls.Add(this.lblPly);
            this.Controls.Add(this.lblOnTurn);
            this.Controls.Add(this.chkBlack);
            this.Controls.Add(this.chkWhite);
            this.Controls.Add(this.lblPending);
            this.Controls.Add(this.btnMove);
            this.Controls.Add(this.lblMoves);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.lblBoard);
            this.Name = "frmChess";
            this.Text = "Friendly Chess";
            this.Load += new System.EventHandler(this.frmChess_Load);
            ((System.ComponentModel.ISupportInitialize)(this.tbrDepthWhite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.tbrDepthBlack)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblBoard;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Label lblMoves;
        private System.Windows.Forms.Button btnMove;
        private System.Windows.Forms.Label lblPending;
        private System.Windows.Forms.CheckBox chkWhite;
        private System.Windows.Forms.CheckBox chkBlack;
        private System.Windows.Forms.Label lblOnTurn;
        private System.Windows.Forms.Label lblPly;
        private System.Windows.Forms.Button btnMoves;
        private System.Windows.Forms.CheckBox chkRotate;
        private System.Windows.Forms.Label lblValue;
        private System.Windows.Forms.TrackBar tbrDepthWhite;
        private System.Windows.Forms.TrackBar tbrDepthBlack;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Button button1;
        private System.DirectoryServices.DirectoryEntry directoryEntry1;
        private System.Windows.Forms.Button btnLines;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnPerft;
        private System.Windows.Forms.Button btnPerftLines;
        private System.Windows.Forms.Button btnClearLog;
        private System.Windows.Forms.TextBox txtFen;
        private System.Windows.Forms.Button btnImportFEN;
        private System.Windows.Forms.Button btnExportFEN;
        private System.Windows.Forms.TextBox txtMove;
        private System.Windows.Forms.Button btnReverse;
        private System.Windows.Forms.Label lblDepth;
        private System.Windows.Forms.Button btnStopWatch;
        private System.Windows.Forms.Button btnZobrist;
        private System.Windows.Forms.Button btnQPerft;
        private System.Windows.Forms.Button btnEstimate;
        private System.Windows.Forms.Button btnTerminal;
        private System.Windows.Forms.Button btnGame;
        private System.Windows.Forms.CheckBox chkPause;
        private System.Windows.Forms.Button btnPerftSuite;
        private System.Windows.Forms.Button btnLine;
        private System.Windows.Forms.RadioButton optN;
        private System.Windows.Forms.RadioButton optB;
        private System.Windows.Forms.RadioButton optR;
        private System.Windows.Forms.RadioButton optQ;
        private System.Windows.Forms.Label lblClock;
    }
}

