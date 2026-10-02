using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;

namespace ConsolidadorLogs
{
    public partial class Form1 : Form
    {
        // declaração dos componentes da interface
        private TextBox textOrigem;
        private TextBox textDestino;
        private Button btnBrowseOrigem;
        private Button btnBrowseDestino;
        private Button btnProximo;
        private DateTimePicker dateInicio;
        private DateTimePicker dateFim;
        private CheckedListBox chekMaquinas;
        private CheckedListBox chekLogs;

        //Listas de dados pré-cadastrados
        private readonly string[] listMaquinas = new[]
        {
            "EP2091KS01", "EP2091KS02", "EP2091KS03", "EP2091KS04", "EP2091KS05", 
            "RC2092KS01", "RC2092KS02", "RC2092KS03", "RC2092KS04", "RC2092KS05", "RC2092KS06",
            "EP2020KS01", "EP2020KS02", "EP2020KS03",
            "RC2020KS01", "RC2020KS02", "RC2020KS03"
        };

        private readonly string[] listLog = new[]
        {
         "MACHINE_RECLAIMING_FLOW_BRIDGE",
         "MACHINE_CURRENT_OPERATION_MODE_BRIDGE",
         "MACHINE_RECLAIMING_FLOW_BRIDGE",
         "MACHINE_CURRENT_OPERATION_MODE_BRIDGE",
         "MACHINE_FLOW_SETPOINT_CLAMPED_BRIDGE",
         "MACHINE_OPERATION_BELT_WORKING_BRIDGE",
         "MACHINE_STATUS_BUCKETWHEEL_ON_BRIDGE"
        };

        //Construtor
        public Form1()
        {   //tudo o que colocar aqui dentro é executado na hora do "nascimento" da tela
            InitializeComponent();
            ConstruirInterface();
        }
        private void ConstruirInterface()
        {
            //Configurações básicas da janela
            this.Text = "Consolidador de Logs - Etapa 1";
            this.Size = new Size(600,500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // 1. Campo para Seleção da Pasta Origem
            Label lblOrigem = new Label
            {
                Text = "Pasta Origem dos Logs",
                Location = new Point(20,20),
                AutoSize = true
            };

            textOrigem = new TextBox
            {
                Location = new Point(20,42),
                Width = 430,
                ReadOnly = true
            };

            btnBrowseOrigem = new Button
            {
                Text = "Procurar...",
                Location = new Point(460,40),
                Width = 100
            };

            btnBrowseOrigem.Click += (s,e) => EscolherPasta(textOrigem);

            // 2. Campo para Seleção da Pasta Destino (padrão)
            Label lblDestino = new Label
            {
                Text = "Pasta Destino (Consolidado):",
                Location = new Point(20,89),
                AutoSize = true
            };

            string caminhoPadrao = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Medicao",
                "Consolidado");

            textDestino = new TextBox
            {
                Location = new Point(20,97),
                Width = 430,
                Text = caminhoPadrao

            };

            btnBrowseDestino = new Button
            {
                Text = "Procurar...",
                Location = new Point(460,100),
                Width = 100
            };

            Label lblDataInicio = new Label

            btnBrowseDestino.Click += (s,e) => EscolherPasta(textDestino);

            //Add todos os elementos na tela
            this.Controls.AddRange(new Control[]
            {
                lblOrigem, textOrigem, btnBrowseOrigem,
                lblDestino, textDestino, btnBrowseDestino
            });
        }

        private void EscolherPasta(TextBox campoTexto)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() == DialogResult.OK)
                {
                    campoTexto.Text = fbd.SelectedPath;
                }
            }
        }
    }
}