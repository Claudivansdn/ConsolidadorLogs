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
        private CheckBox chekgnorarMaquina;
        private CheckedListBox chekLogs;
        private ProgressBar progressBar;
        private Label lblStatus;

        // Variáveis de backup para o recurso de Desfazer/Reverter
        private List<string>? maquinaBackup = null;
        private List<string>? logBackup = null;        // Listas de dados pré-cadastrados
        private List<string> listMaquinas = ConfigManager.CarregarMaquinas(); // Carrega máquinas do arquivo JSON ou usa padrão

        private List<string> listLog = ConfigManager.CarregarLogs(); // Carrega logs do arquivo JSON ou usa padrão

        private string? ExibirInputBox(string titulo, string mensagem)
        {
            Form prompt = new Form()
            {
                Width = 400,
                Height = 160,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = titulo,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };

            Label lblTexto = new Label() { Left = 20, Top = 15, Text = mensagem, AutoSize = true };
            TextBox txtInput = new TextBox() { Left = 20, Top = 40, Width = 340 };
            Button btnConfirmar = new Button() { Text = "OK", Left = 200, Width = 75, Top = 75, DialogResult = DialogResult.OK };
            Button btnCancelar = new Button() { Text = "Cancelar", Left = 285, Width = 75, Top = 75, DialogResult = DialogResult.Cancel };

            prompt.Controls.Add(lblTexto);
            prompt.Controls.Add(txtInput);
            prompt.Controls.Add(btnConfirmar);
            prompt.Controls.Add(btnCancelar);
            prompt.AcceptButton = btnConfirmar;

            return prompt.ShowDialog() == DialogResult.OK ? txtInput.Text.Trim() : null;
        }
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
            this.Size = new Size(840, 670);
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


            Button btnAddMaquina = new Button { Text = "+", Location = new Point(20, 510), Width = 30, Height = 25 };
            Button btnRemoveMaquina = new Button { Text = "-", Location = new Point(55, 510), Width = 30, Height = 25 };
            Button btnUndoMaquina = new Button { Text = "↺", Location = new Point(90, 510), Width = 30, Height = 25 }; 
            Button btnResetMaquina = new Button { Text = "Restaurar", Location = new Point(125, 510), Width = 65, Height = 25 };
            Button btnSalvarPadraoMaquina = new Button { Text = "Salvar Padrão", Location = new Point(190, 510), Width = 95, Height = 25 };

            btnAddMaquina.Click += (s, e) =>
            {
                string? novaMaquina = ExibirInputBox("Nova Máquina", "Digite o código/nome da máquina:");
                if (!string.IsNullOrWhiteSpace(novaMaquina))
                {
                    novaMaquina = novaMaquina.ToUpper();
                    if (!chekMaquinas.Items.Contains(novaMaquina))
                    {
                        // Guarda o estado atual antes de modificar
                        SalvarBackupMaquinas();

                        chekMaquinas.Items.Add(novaMaquina, true);
                        SalvarListaMaquinasAtualizada();
                    }
                }
            };

            // Evento para Remover Máquina (salva o backup antes de alterar)
            btnRemoveMaquina.Click += (s, e) =>
            {
                if (chekMaquinas.SelectedItem != null)
                {
                    // Guarda o estado atual antes de modificar
                    SalvarBackupMaquinas();

                    chekMaquinas.Items.Remove(chekMaquinas.SelectedItem);
                    SalvarListaMaquinasAtualizada();
                }
                else
                {
                    MessageBox.Show("Selecione uma máquina para remover.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            };

            // Evento para Reverter a última alteração das Máquinas
            btnUndoMaquina.Click += (s, e) =>
            {
                if (maquinaBackup != null)
                {
                    // Restaura a lista do backup
                    chekMaquinas.Items.Clear();
                    foreach (var item in maquinaBackup)
                    {
                        chekMaquinas.Items.Add(item, true);
                    }

                    // Salva o estado revertido no arquivo .json
                    SalvarListaMaquinasAtualizada();

                    // Limpa o backup para impedir múltiplos undos
                    maquinaBackup = null;

                    MessageBox.Show("Última alteração de máquinas revertida com sucesso!", "Reverter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Não há nenhuma alteração recente para reverter.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };    

            btnResetMaquina.Click += (s, e) =>
            {
                var resultado = MessageBox.Show(
                    "Tem certeza que deseja restaurar a lista padrão de máquinas?\nIsso substituirá as máquinas cadastradas atualmente.",
                    "Confirmar Restauração",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (resultado == DialogResult.Yes)
                {
                    // Salva o estado atual no backup caso o usuário queira desfazer depois
                    SalvarBackupMaquinas();

                    // Recarrega os padrões do ConfigManager
                    List<string> padrao = ConfigManager.RestaurarMaquinasPadrao();

                    // Atualiza a CheckBoxList na tela
                    chekMaquinas.Items.Clear();
                    foreach (var maquina in padrao)
                    {
                        chekMaquinas.Items.Add(maquina, true);
                    }

                    MessageBox.Show("Lista de máquinas restaurada para o padrão com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            btnSalvarPadraoMaquina.Click += (s, e) =>
            {
                var resultado = MessageBox.Show(
                    "Deseja definir as máquinas cadastradas atualmente como o novo padrão deste computador?",
                    "Salvar como Padrão",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (resultado == DialogResult.Yes)
                {
                    List<string> maquinasAtuais = new List<string>();
                    foreach (var item in chekMaquinas.Items)
                    {
                        maquinasAtuais.Add(item.ToString()!);
                    }

                    ConfigManager.DefinirMaquinasComoPadrao(maquinasAtuais);
                    MessageBox.Show("Lista atual de máquinas definida como o novo padrão com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };


            chekMaquinas.Items.AddRange(listMaquinas.ToArray());
            for (int i = 0; i < chekMaquinas.Items.Count; i++)
            {
                chekMaquinas.SetItemChecked(i, true);
            }

            chekgnorarMaquina = new CheckBox
            {
                Text = "Ignorar máquina",
                Location = new Point(160, 200),
                AutoSize = true
            };

            chekgnorarMaquina.CheckedChanged += (s, e) =>
            {
                bool usarMaquina = !chekgnorarMaquina.Checked;
                
                // Desabilita/habilita os controles de máquina
                chekMaquinas.Enabled = usarMaquina;
                btnAddMaquina.Enabled = usarMaquina;
                btnRemoveMaquina.Enabled = usarMaquina;
                btnUndoMaquina.Enabled = usarMaquina;
                btnResetMaquina.Enabled = usarMaquina;
                btnSalvarPadraoMaquina.Enabled = usarMaquina;
            };


            Label lblLogs = new Label
            {
                Text = "Selecione os Logs:",
                Location = new Point(300, 200),
                AutoSize = true
            };

            chekLogs = new CheckedListBox
            {
                Location = new Point(300, 225),
                Width = 490,
                Height = 280,
                CheckOnClick = true
            };

            chekLogs.Items.AddRange(listLog.ToArray());
            for (int i = 0; i < chekLogs.Items.Count; i++)
            {
                chekLogs.SetItemChecked(i, true);
            }

            Button btnAddLog = new Button { Text = "+", Location = new Point(300, 510), Width = 30, Height = 25 };
            Button btnRemoveLog = new Button { Text = "-", Location = new Point(335, 510), Width = 30, Height = 25 };
            Button btnUndoLog = new Button { Text = "↺", Location = new Point(370, 510), Width = 30, Height = 25 }; // Botão Reverter
            Button btnResetLog = new Button { Text = "Restaurar", Location = new Point(405, 510), Width = 65, Height = 25 };
            Button btnSalvarPadraoLog = new Button { Text = "Salvar Padrão", Location = new Point(470, 510), Width = 95, Height = 25 };
            
            btnAddLog.Click += (s, e) =>
            {
                string? novoLog = ExibirInputBox("Novo Log", "Digite o nome do log:");
                if (!string.IsNullOrWhiteSpace(novoLog))
                {
                    novoLog = novoLog.ToUpper();
                    if (!chekLogs.Items.Contains(novoLog))
                    {
                        SalvarBackupLogs(); // Guarda o estado anterior

                        chekLogs.Items.Add(novoLog, true);
                        SalvarListaLogsAtualizada();
                    }
                }
            };

            btnRemoveLog.Click += (s, e) =>
            {
                if (chekLogs.SelectedItem != null)
                {
                    SalvarBackupLogs(); // Guarda o estado anterior

                    chekLogs.Items.Remove(chekLogs.SelectedItem);
                    SalvarListaLogsAtualizada();
                }
            };

            btnUndoLog.Click += (s, e) =>
            {
                if (logBackup != null)
                {
                    chekLogs.Items.Clear();
                    foreach (var item in logBackup)
                    {
                        chekLogs.Items.Add(item, true);
                    }

                    SalvarListaLogsAtualizada();
                    logBackup = null;

                    MessageBox.Show("Última alteração de logs revertida com sucesso!", "Reverter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Não há nenhuma alteração recente para reverter.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            btnResetLog.Click += (s, e) =>
            {
                var resultado = MessageBox.Show(
                    "Tem certeza que deseja restaurar a lista padrão de logs?\nIsso substituirá os logs cadastrados atualmente.",
                    "Confirmar Restauração",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (resultado == DialogResult.Yes)
                {
                    // Salva o estado atual no backup caso queira desfazer
                    SalvarBackupLogs();

                    // Recarrega os padrões do ConfigManager
                    List<string> padrao = ConfigManager.RestaurarLogsPadrao();

                    // Atualiza a CheckBoxList na tela
                    chekLogs.Items.Clear();
                    foreach (var log in padrao)
                    {
                        chekLogs.Items.Add(log, true);
                    }

                    MessageBox.Show("Lista de logs restaurada para o padrão com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            btnSalvarPadraoLog.Click += (s, e) =>
            {
                var resultado = MessageBox.Show(
                    "Deseja definir os logs cadastrados atualmente como o novo padrão deste computador?",
                    "Salvar como Padrão",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (resultado == DialogResult.Yes)
                {
                    List<string> logsAtuais = new List<string>();
                    foreach (var item in chekLogs.Items)
                    {
                        logsAtuais.Add(item.ToString()!);
                    }

                    ConfigManager.DefinirLogsComoPadrao(logsAtuais);
                    MessageBox.Show("Lista atual de logs definida como o novo padrão com sucesso!", "Sucesso", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };
            // 5. Botão Executar Consolidação
            btnExecutar = new Button
            {
                Text = "EXECUTAR",
                Location = new Point(580, 555),
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
                Location = new Point(20, 550),
                Width = 540,
                AutoSize = false
            };

            progressBar = new ProgressBar
            {
                Location = new Point(20, 570),
                Width = 540,
                Height = 25,
                Minimum = 0,
                Maximum = 100,
                Value = 0
            };

            // Adiciona TODOS os componentes à janela
            this.Controls.AddRange(new Control[]
            {
                lblOrigem, textOrigem, btnBrowseOrigem,
                lblDestino, textDestino, btnBrowseDestino,
                lblDataInicio, dateInicio,
                lblDataFim, dateFim,
                lblMaquinas, chekMaquinas, btnAddMaquina, btnRemoveMaquina,
                lblLogs, chekLogs, btnAddLog, btnRemoveLog, btnUndoLog, btnUndoMaquina, 
                btnResetLog, btnResetMaquina, btnSalvarPadraoLog, btnSalvarPadraoMaquina,
                chekgnorarMaquina,
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
            // Congela a janela inteira (nenhum botão, texto ou clique funciona na janela)
            this.Enabled = false;
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
                        chekgnorarMaquina.Checked ? new List<string>() : maquinasSelecionadas,
                        logsSelecionados,
                        chekgnorarMaquina.Checked,
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
                this.Enabled = true; // Reabilita a janela
                btnExecutar.Enabled = true; // Reabilita o botão de execução
                progressBar.Value = 0; // Reseta a barra de progresso
                lblStatus.Text = "Status: Aguardando execução...";
            }
        }
        private void SalvarListaMaquinasAtualizada()
        {
            List<string> listaAtual = new List<string>();
            foreach (var item in chekMaquinas.Items)
            {
                listaAtual.Add(item.ToString()!);
            }
            ConfigManager.SalvarMaquinas(listaAtual);
        }

        private void SalvarListaLogsAtualizada()
        {
            List<string> listaAtual = new List<string>();
            foreach (var item in chekLogs.Items)
            {
                listaAtual.Add(item.ToString()!);
            }
            ConfigManager.SalvarLogs(listaAtual);
        }
        private void SalvarBackupMaquinas()
        {
            maquinaBackup = new List<string>();
            foreach (var item in chekMaquinas.Items)
            {
                maquinaBackup.Add(item.ToString()!);
            }
        }

        private void SalvarBackupLogs()
        {
            logBackup = new List<string>();
            foreach (var item in chekLogs.Items)
            {
                logBackup.Add(item.ToString()!);
            }
        }        
    }
}