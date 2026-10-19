using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using ConsolidadorLogs.Core; // Importa as classes da pasta Core

namespace ConsolidadorLogs
{
    public partial class MainScreen : Form
    {
        // Declaração dos componentes da interface
        private TextBox textOrigem;
        private TextBox textDestino;
        private Button btnBrowseOrigem;
        private Button btnBrowseDestino;
        private Button btnExecutar;
        private DateTimePicker dateInicio;
        private DateTimePicker dateFim;
        private CheckedListBox chekMaquinas;
        private CheckedListBox chekLogs;
        private ProgressBar progressBar;
        private Label lblStatus;

        // Listas de dados pré-cadastrados
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
            "MACHINE_FLOW_SETPOINT_CLAMPED_BRIDGE",
            "MACHINE_OPERATION_BELT_WORKING_BRIDGE",
            "MACHINE_STATUS_BUCKETWHEEL_ON_BRIDGE"
        };

        // Construtor
        public MainScreen()
        {
            InitializeComponent();
            ConstruirInterface();
        }

        // Método para construir a interface do usuário
        private void ConstruirInterface()
        {
            // Configurações básicas da janela
            this.Text = "Consolidador de Logs";
            this.Size = new Size(800, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // 1. Campo para Seleção da Pasta Origem
            Label lblOrigem = new Label
            {
                Text = "Pasta Origem dos Logs",
                Location = new Point(20, 20),
                AutoSize = true
            };

            textOrigem = new TextBox
            {
                Location = new Point(20, 42),
                Width = 430,
                ReadOnly = true
            };

            btnBrowseOrigem = new Button
            {
                Text = "Procurar...",
                Location = new Point(460, 40),
                Width = 100
            };

            btnBrowseOrigem.Click += (s, e) => EscolherPasta(textOrigem);

            // 2. Campo para Seleção da Pasta Destino
            Label lblDestino = new Label
            {
                Text = "Pasta Destino (Consolidado):",
                Location = new Point(20, 79),
                AutoSize = true
            };

            string caminhoPadrao = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Medicao",
                "Consolidado");

            textDestino = new TextBox
            {
                Location = new Point(20, 97),
                Width = 430,
                Text = caminhoPadrao
            };

            btnBrowseDestino = new Button
            {
                Text = "Procurar...",
                Location = new Point(460, 100),
                Width = 100
            };

            btnBrowseDestino.Click += (s, e) => EscolherPasta(textDestino);

            // 3. Campos para Seleção do Período de Consolidação
            Label lblDataInicio = new Label
            {
                Text = "Data Inicial:",
                Location = new Point(20, 150),
                AutoSize = true
            };

            dateInicio = new DateTimePicker
            {
                Location = new Point(20, 170),
                Format = DateTimePickerFormat.Short,
                Width = 130,
                Value = DateTime.Now.AddMonths(-1)
            };

            Label lblDataFim = new Label
            {
                Text = "Data Final:",
                Location = new Point(180, 150),
                AutoSize = true
            };

            dateFim = new DateTimePicker
            {
                Location = new Point(180, 170),
                Format = DateTimePickerFormat.Short,
                Width = 130,
                Value = DateTime.Now
            };

            // 4. Campos para Seleção das Máquinas e Logs
            Label lblMaquinas = new Label
            {
                Text = "Selecione as Máquinas:",
                Location = new Point(20, 200),
                AutoSize = true
            };

            chekMaquinas = new CheckedListBox
            {
                Location = new Point(20, 225),
                Width = 220,
                Height = 280,
                CheckOnClick = true
            };

            chekMaquinas.Items.AddRange(listMaquinas);
            for (int i = 0; i < chekMaquinas.Items.Count; i++)
            {
                chekMaquinas.SetItemChecked(i, true);
            }

            Label lblLogs = new Label
            {
                Text = "Selecione os Logs:",
                Location = new Point(260, 200),
                AutoSize = true
            };

            chekLogs = new CheckedListBox
            {
                Location = new Point(260, 225),
                Width = 490,
                Height = 280,
                CheckOnClick = true
            };

            chekLogs.Items.AddRange(listLog);
            for (int i = 0; i < chekLogs.Items.Count; i++)
            {
                chekLogs.SetItemChecked(i, true);
            }

            // 5. Botão Executar Consolidação
            btnExecutar = new Button
            {
                Text = "EXECUTAR",
                Location = new Point(580, 520),
                Width = 170,
                Height = 40,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                BackColor = Color.LightSkyBlue
            };

            btnExecutar.Click += BtnExecutar_Click;

            // 6. Label de Status e Barra de Progresso
            lblStatus = new Label
            {
                Text = "Status: Aguardando execução...",
                Location = new Point(20, 515),
                Width = 540,
                AutoSize = false
            };

            progressBar = new ProgressBar
            {
                Location = new Point(20, 535),
                Width = 540,
                Height = 25,
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };

            // Adiciona TODOS os componentes à janela (incluindo status e barra)
            this.Controls.AddRange(new Control[]
            {
                lblOrigem, textOrigem, btnBrowseOrigem,
                lblDestino, textDestino, btnBrowseDestino,
                lblDataInicio, dateInicio,
                lblDataFim, dateFim,
                lblMaquinas, chekMaquinas,
                lblLogs, chekLogs,
                lblStatus, progressBar,
                btnExecutar
            });
        }

        // Método para abrir o diálogo de seleção de pasta
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

        // Método acionado ao clicar no botão "EXECUTAR"
        private async void BtnExecutar_Click(object? sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textOrigem.Text))
                {
                    MessageBox.Show("Por favor, selecione a pasta de origem dos logs.", 
                                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Coleta máquinas selecionadas
                List<string> maquinasSelecionadas = new List<string>();
                foreach (var item in chekMaquinas.CheckedItems)
                {
                    maquinasSelecionadas.Add(item.ToString()!);
                }

                // Coleta logs selecionados
                List<string> logsSelecionados = new List<string>();
                foreach (var item in chekLogs.CheckedItems)
                {
                    logsSelecionados.Add(item.ToString()!);
                }

                if (maquinasSelecionadas.Count == 0 || logsSelecionados.Count == 0)
                {
                    MessageBox.Show("Selecione ao menos uma máquina e um tipo de log para prosseguir.", 
                                    "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Bloqueia o botão de execução para evitar múltiplos cliques
                btnExecutar.Enabled = false;
                progressBar.Value = 0;

                // Configura o canal de comunicação para receber atualizações de progresso
                var progressHandler = new Progress<ProgressReport>(report =>
                {
                    progressBar.Value = Math.Clamp(report.Percentual, 0, 100);
                    lblStatus.Text = $"Status: {report.Status} ({report.Percentual}%)";
                });

                List<string> naoEncontrados = new List<string>();

                // PROCESSO 1: Consolidação de arquivos (0% a 100%)
                await Task.Run(() =>
                {
                    naoEncontrados = LogConsolidator.Executar(
                        textOrigem.Text,
                        textDestino.Text,
                        dateInicio.Value,
                        dateFim.Value,
                        maquinasSelecionadas,
                        logsSelecionados,
                        progressHandler);
                });

                // Pequena transição visual antes da próxima etapa
                await Task.Delay(500);
                progressBar.Value = 0; // Reinicia a barra para o segundo processo

                // PROCESSO 2: Substituição de texto nos arquivos consolidados (0% a 100%)
                await Task.Run(() =>
                {
                    TextReplacer.ProcessarPasta(textDestino.Text, progressHandler);
                });

                // Monta mensagem final
                string mensagem = "Consolidação realizada com sucesso!";

                if (naoEncontrados.Count > 0)
                {
                    mensagem += "\n\nAtenção! Os seguintes arquivos não foram encontrados nas subpastas:\n";
                    mensagem += string.Join("\n", naoEncontrados);
                }

                MessageBox.Show(mensagem, "Resultado", MessageBoxButtons.OK, 
                                naoEncontrados.Count > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ocorreu um erro ao consolidar os logs: {ex.Message}", 
                                "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnExecutar.Enabled = true; // Reabilita o botão de execução
                progressBar.Value = 0; // Reseta a barra de progresso
                lblStatus.Text = "Status: Aguardando execução...";
            }
        }
    }
}