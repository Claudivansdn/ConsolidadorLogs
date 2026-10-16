using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ConsolidadorLogs.Core; // Importa as classes da pasta Core

namespace ConsolidadorLogs
{
    public partial class MainScreen : Form
    {
        // declaração dos componentes da interface
        private TextBox textOrigem;
        private TextBox textDestino;
        private Button btnBrowseOrigem;
        private Button btnBrowseDestino;
        private Button btnExecutar;
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
         "MACHINE_FLOW_SETPOINT_CLAMPED_BRIDGE",
         "MACHINE_OPERATION_BELT_WORKING_BRIDGE",
         "MACHINE_STATUS_BUCKETWHEEL_ON_BRIDGE"
        };

        //Construtor
        public MainScreen()
        {   //tudo o que colocar aqui dentro é executado na hora do "nascimento" da tela
            InitializeComponent();
            ConstruirInterface();
        }

        //Método para construir a interface do usuário
        private void ConstruirInterface()
        {
            //Configurações básicas da janela
            this.Text = "Consolidador de Logs"; // Título da janela
            this.Size = new Size(800,620); // Tamanho da janela
            this.StartPosition = FormStartPosition.CenterScreen; // Centraliza a janela na tela
            this.FormBorderStyle = FormBorderStyle.FixedDialog; // Impede redimensionamento da janela
            this.MaximizeBox = false; // Remove o botão de maximizar

            // 1. Campo para Seleção da Pasta Origem
            Label lblOrigem = new Label
            {
                Text = "Pasta Origem dos Logs",
                Location = new Point(20,20), // Define a posição do rótulo na janela
                AutoSize = true // Ajusta automaticamente o tamanho do rótulo ao texto
            };

            // Campo de texto para exibir o caminho da pasta selecionada
            textOrigem = new TextBox
            {
                Location = new Point(20,42), // Define a posição do campo de texto na janela
                Width = 430, // Define a largura do campo de texto
                ReadOnly = true // Impede que o usuário edite o campo de texto diretamente
            };

            // Botão para abrir o diálogo de seleção de pasta
            btnBrowseOrigem = new Button
            {
                Text = "Procurar...",
                Location = new Point(460,40),
                Width = 100
            };

            // Adiciona o evento de clique ao botão para abrir o diálogo de seleção de pasta
            btnBrowseOrigem.Click += (s,e) => EscolherPasta(textOrigem);

            // 2. Campo para Seleção da Pasta Destino (padrão)
            Label lblDestino = new Label
            {
                Text = "Pasta Destino (Consolidado):",
                Location = new Point(20,79),
                AutoSize = true
            };
            
            // Define o caminho padrão para a pasta de destino
            string caminhoPadrao = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "Medicao",
                "Consolidado");

            // Cria o TextBox para exibir o caminho da pasta de destino
            textDestino = new TextBox
            {
                Location = new Point(20,97),
                Width = 430,
                Text = caminhoPadrao

            };

            // Cria o botão para abrir o diálogo de seleção de pasta para a pasta de destino
            btnBrowseDestino = new Button
            {
                Text = "Procurar...",
                Location = new Point(460,100),
                Width = 100
            };

            // Adiciona o evento de clique ao botão para abrir o diálogo de seleção de pasta
            btnBrowseDestino.Click += (s,e) => EscolherPasta(textDestino);

            // 3. Campos para Seleção do Período de Consolidação
            Label lblDataInicio = new Label
            {
                Text = "Data Inicial:",
                Location = new Point(20, 150),
                AutoSize = true
            };

            // Cria o DateTimePicker para a data inicial, definindo o valor padrão como um mês atrás
            dateInicio = new DateTimePicker
            {
                Location = new Point(20, 170),
                Format = DateTimePickerFormat.Short,
                Width = 130,
                Value = DateTime.Now.AddMonths(-1) // definie a data inicial como a data atual
            };

            // Cria o Label para a data final
            Label lblDataFim = new Label
            {
                Text = "Data Final:",
                Location = new Point(180, 150),
                AutoSize = true
            };

            // Cria o DateTimePicker para a data final, definindo o valor padrão como a data atual
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

            // Cria o CheckedListBox para as máquinas, permitindo seleção múltipla
            chekMaquinas = new CheckedListBox
            {
                Location = new Point(20, 225),
                Width = 220,
                Height = 280,
                CheckOnClick = true
            };

            // Adiciona as máquinas à lista e marca todas como selecionadas por padrão
            chekMaquinas.Items.AddRange(listMaquinas);
            for (int i = 0; i < chekMaquinas.Items.Count; i++)
            {
                chekMaquinas.SetItemChecked(i, true);
            } ;

            // Cria o Label para os logs
            Label lblLogs = new Label
            {
                Text = "Selecione os Logs:",
                Location = new Point(260, 200),
                AutoSize = true
            };

            // Cria o CheckedListBox para os logs, permitindo seleção múltipla
            chekLogs = new CheckedListBox
            {
                Location = new Point(260, 225),
                Width = 490,
                Height = 280,
                CheckOnClick = true
            };

            // Adiciona os logs à lista e marca todos como selecionados por padrão
            chekLogs.Items.AddRange(listLog);
            for (int i = 0; i < chekLogs.Items.Count; i++) 
            {
                chekLogs.SetItemChecked(i, true); // Deixa todos marcados
            } ;

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

            // Adiciona todos os componentes à janela
            this.Controls.AddRange(new Control[]
            {
                lblOrigem, textOrigem, btnBrowseOrigem,
                lblDestino, textDestino, btnBrowseDestino,
                lblDataInicio, dateInicio,
                lblDataFim, dateFim,
                lblMaquinas, chekMaquinas,
                lblLogs, chekLogs,
                btnExecutar
            });
        }

        // Método acionado ao clicar no botão "Consolidar Logs"
        private void BtnExecutar_Click(object? sender, EventArgs e)
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

                // Executa a consolidação chamando o backend na pasta Core
                List<string> naoEncontrados = LogConsolidator.Executar(
                    textOrigem.Text,
                    textDestino.Text,
                    dateInicio.Value,
                    dateFim.Value,
                    maquinasSelecionadas,
                    logsSelecionados
                );

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
        }

        // Método para abrir o diálogo de seleção de pasta e atualizar o TextBox correspondente
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