namespace Interfaz
{
    partial class Inicio
    {
        /// <summary>
        /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén utilizando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben eliminar; false en caso contrario, false.</param>
        protected override void Dispose(bool disposing)
            {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.administracionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.historialToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.restoreToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.facturaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pagarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.timbrarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cancelarToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.documentosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nuevoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.facturaToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.pagoDeHonorariosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.notaDeCreditoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generarFacturaToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lMsj = new System.Windows.Forms.TextBox();
            this.timer2 = new System.Windows.Forms.Timer(this.components);
            this.txtCodControl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtIdComprobante = new System.Windows.Forms.TextBox();
            this.txtCodDoc = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.regPdf = new System.Windows.Forms.Button();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // timer1
            // 
            this.timer1.Interval = 10000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.administracionToolStripMenuItem,
            this.facturaToolStripMenuItem,
            this.nuevoToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(351, 28);
            this.menuStrip1.TabIndex = 7;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // administracionToolStripMenuItem
            // 
            this.administracionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.historialToolStripMenuItem,
            this.backupToolStripMenuItem,
            this.restoreToolStripMenuItem});
            this.administracionToolStripMenuItem.Name = "administracionToolStripMenuItem";
            this.administracionToolStripMenuItem.Size = new System.Drawing.Size(123, 24);
            this.administracionToolStripMenuItem.Text = "Administracion";
            // 
            // historialToolStripMenuItem
            // 
            this.historialToolStripMenuItem.Name = "historialToolStripMenuItem";
            this.historialToolStripMenuItem.Size = new System.Drawing.Size(148, 26);
            this.historialToolStripMenuItem.Text = "Historial";
            // 
            // backupToolStripMenuItem
            // 
            this.backupToolStripMenuItem.Name = "backupToolStripMenuItem";
            this.backupToolStripMenuItem.Size = new System.Drawing.Size(148, 26);
            this.backupToolStripMenuItem.Text = "Backup ";
            // 
            // restoreToolStripMenuItem
            // 
            this.restoreToolStripMenuItem.Name = "restoreToolStripMenuItem";
            this.restoreToolStripMenuItem.Size = new System.Drawing.Size(148, 26);
            this.restoreToolStripMenuItem.Text = "Restore ";
            // 
            // facturaToolStripMenuItem
            // 
            this.facturaToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.pagarToolStripMenuItem,
            this.timbrarToolStripMenuItem,
            this.cancelarToolStripMenuItem,
            this.documentosToolStripMenuItem});
            this.facturaToolStripMenuItem.Name = "facturaToolStripMenuItem";
            this.facturaToolStripMenuItem.Size = new System.Drawing.Size(70, 24);
            this.facturaToolStripMenuItem.Text = "Factura";
            // 
            // pagarToolStripMenuItem
            // 
            this.pagarToolStripMenuItem.Name = "pagarToolStripMenuItem";
            this.pagarToolStripMenuItem.Size = new System.Drawing.Size(176, 26);
            this.pagarToolStripMenuItem.Text = "Pagar";
            // 
            // timbrarToolStripMenuItem
            // 
            this.timbrarToolStripMenuItem.Name = "timbrarToolStripMenuItem";
            this.timbrarToolStripMenuItem.Size = new System.Drawing.Size(176, 26);
            this.timbrarToolStripMenuItem.Text = "Timbrar";
            // 
            // cancelarToolStripMenuItem
            // 
            this.cancelarToolStripMenuItem.Name = "cancelarToolStripMenuItem";
            this.cancelarToolStripMenuItem.Size = new System.Drawing.Size(176, 26);
            this.cancelarToolStripMenuItem.Text = "Cancelar";
            // 
            // documentosToolStripMenuItem
            // 
            this.documentosToolStripMenuItem.Name = "documentosToolStripMenuItem";
            this.documentosToolStripMenuItem.Size = new System.Drawing.Size(176, 26);
            this.documentosToolStripMenuItem.Text = "Documentos";
            // 
            // nuevoToolStripMenuItem
            // 
            this.nuevoToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.facturaToolStripMenuItem1,
            this.pagoDeHonorariosToolStripMenuItem,
            this.notaDeCreditoToolStripMenuItem,
            this.generarFacturaToolStripMenuItem});
            this.nuevoToolStripMenuItem.Name = "nuevoToolStripMenuItem";
            this.nuevoToolStripMenuItem.Size = new System.Drawing.Size(66, 24);
            this.nuevoToolStripMenuItem.Text = "Nuevo";
            // 
            // facturaToolStripMenuItem1
            // 
            this.facturaToolStripMenuItem1.Name = "facturaToolStripMenuItem1";
            this.facturaToolStripMenuItem1.Size = new System.Drawing.Size(224, 26);
            this.facturaToolStripMenuItem1.Text = "Factura";
            this.facturaToolStripMenuItem1.Click += new System.EventHandler(this.facturaToolStripMenuItem1_Click);
            // 
            // pagoDeHonorariosToolStripMenuItem
            // 
            this.pagoDeHonorariosToolStripMenuItem.Name = "pagoDeHonorariosToolStripMenuItem";
            this.pagoDeHonorariosToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.pagoDeHonorariosToolStripMenuItem.Text = "Pago de Honorarios";
            // 
            // notaDeCreditoToolStripMenuItem
            // 
            this.notaDeCreditoToolStripMenuItem.Name = "notaDeCreditoToolStripMenuItem";
            this.notaDeCreditoToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.notaDeCreditoToolStripMenuItem.Text = "Nota de Credito";
            // 
            // generarFacturaToolStripMenuItem
            // 
            this.generarFacturaToolStripMenuItem.Name = "generarFacturaToolStripMenuItem";
            this.generarFacturaToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.generarFacturaToolStripMenuItem.Text = "Generar Factura";
            this.generarFacturaToolStripMenuItem.Click += new System.EventHandler(this.generarFacturaToolStripMenuItem_Click);
            // 
            // lMsj
            // 
            this.lMsj.Location = new System.Drawing.Point(16, 33);
            this.lMsj.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.lMsj.Multiline = true;
            this.lMsj.Name = "lMsj";
            this.lMsj.ReadOnly = true;
            this.lMsj.Size = new System.Drawing.Size(317, 149);
            this.lMsj.TabIndex = 8;
            // 
            // txtCodControl
            // 
            this.txtCodControl.Location = new System.Drawing.Point(27, 58);
            this.txtCodControl.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtCodControl.Name = "txtCodControl";
            this.txtCodControl.Size = new System.Drawing.Size(297, 22);
            this.txtCodControl.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 34);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 17);
            this.label1.TabIndex = 10;
            this.label1.Text = "Código Control";
            // 
            // txtIdComprobante
            // 
            this.txtIdComprobante.Location = new System.Drawing.Point(164, 86);
            this.txtIdComprobante.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtIdComprobante.Name = "txtIdComprobante";
            this.txtIdComprobante.Size = new System.Drawing.Size(160, 22);
            this.txtIdComprobante.TabIndex = 11;
            // 
            // txtCodDoc
            // 
            this.txtCodDoc.Location = new System.Drawing.Point(164, 114);
            this.txtCodDoc.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.txtCodDoc.Name = "txtCodDoc";
            this.txtCodDoc.Size = new System.Drawing.Size(160, 22);
            this.txtCodDoc.TabIndex = 12;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(55, 90);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(104, 17);
            this.label2.TabIndex = 13;
            this.label2.Text = "idComprobante";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(55, 118);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 17);
            this.label3.TabIndex = 14;
            this.label3.Text = "codDoc";
            // 
            // regPdf
            // 
            this.regPdf.Location = new System.Drawing.Point(40, 143);
            this.regPdf.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.regPdf.Name = "regPdf";
            this.regPdf.Size = new System.Drawing.Size(119, 28);
            this.regPdf.TabIndex = 15;
            this.regPdf.Text = "Recrear PDF";
            this.regPdf.UseVisualStyleBackColor = true;
            this.regPdf.Click += new System.EventHandler(this.regPdf_Click);
            // 
            // Inicio
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(351, 196);
            this.Controls.Add(this.regPdf);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtCodDoc);
            this.Controls.Add(this.txtIdComprobante);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtCodControl);
            this.Controls.Add(this.lMsj);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Inicio";
            this.Text = "XdService";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem facturaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem timbrarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pagarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cancelarToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem documentosToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem nuevoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem facturaToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem pagoDeHonorariosToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem notaDeCreditoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem administracionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem historialToolStripMenuItem;
        private System.Windows.Forms.TextBox lMsj;
        private System.Windows.Forms.ToolStripMenuItem generarFacturaToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem backupToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem restoreToolStripMenuItem;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.TextBox txtCodControl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtIdComprobante;
        private System.Windows.Forms.TextBox txtCodDoc;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button regPdf;
    }
}

