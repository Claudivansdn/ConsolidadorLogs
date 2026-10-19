using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConsolidadorLogs.Core
{
    public class LogConsolidator
    {
        /// <summary>
        /// Varre todas as subpastas de origem procurando pelos logs,
        /// consolida o conteúdo dos arquivos encontrados e salva no destino.
        /// </summary>
        public static List<string> Executar(
            string pastaOrigem,
            string pastaDestino,
            DateTime dataInicio,
            DateTime dataFim,
            List<string> maquinasSelecionadas,
            List<string> logsSelecionados,
            bool ignorarMaquina,
            IProgress<ProgressReport>? progress = null)
        {
            if (!Directory.Exists(pastaOrigem))
            {
                throw new DirectoryNotFoundException("A pasta de origem selecionada não foi encontrada.");
            }

            List<string> logsNaoEncontrados = new List<string>();

            // 0. Avisa a interface ANTES de iniciar a busca no disco
            progress?.Report(new ProgressReport(5, "Mapeando ficheiros e subpastas..."));

            // 1. Mapeia todos os nomes de arquivos EXATOS que o sistema deve procurar
            Dictionary<string, (string Maquina, string Log, bool Encontrado)> mapaArquivosEsperados = new();

            if (ignorarMaquina)
            {
                foreach (var log in logsSelecionados)
                {
                    string nomeArquivoExato = $"{log}.txt";
                    mapaArquivosEsperados[nomeArquivoExato] = ("", log, false);
                }
            }
            else
            {
                foreach (var maq in maquinasSelecionadas)
                {
                    foreach (var log in logsSelecionados)
                    {
                        string nomeArquivoExato = $"{maq}_{log}.txt";
                        mapaArquivosEsperados[nomeArquivoExato] = (maq, log, false);
                    }
                }
            }

            // 2. Busca todos os arquivos .txt na origem e em todas as suas subpastas
            string[] todosArquivos = Directory.GetFiles(pastaOrigem, "*.txt", SearchOption.AllDirectories);

            // 3. Estrutura de acúmulo
            Dictionary<string, StringBuilder> logsAgrupados = new();

            // ---------------------------------------------------------------------------------
            // LAÇO COM O PROGRESSO:
            // ---------------------------------------------------------------------------------
            int totalArquivos = todosArquivos.Length;

            for (int i = 0; i < totalArquivos; i++)
            {
                string caminhoArquivo = todosArquivos[i];
                string nomeArquivo = Path.GetFileName(caminhoArquivo);
                DateTime dataCriacao = File.GetLastWriteTime(caminhoArquivo);

                // Reporta progresso proporcional (10% a 70%)
                int pct = 10 + (int)(((i + 1) / (double)totalArquivos) * 60);
                progress?.Report(new ProgressReport(pct, $"Processando arquivo {i + 1} de {totalArquivos}: {nomeArquivo}"));

                // Filtro por Data
                if (dataCriacao.Date < dataInicio.Date || dataCriacao.Date > dataFim.Date)
                {
                    continue;
                }

                // Verifica se o arquivo atual bate com a lista esperada
                if (mapaArquivosEsperados.TryGetValue(nomeArquivo, out var info))
                {
                    mapaArquivosEsperados[nomeArquivo] = (info.Maquina, info.Log, true);

                    try
                    {
                        string conteudoDia = File.ReadAllText(caminhoArquivo);

                        // Usa a chave de agrupamento correta (se ignorar máquina, agrupa pelo nome do log)
                        string chaveAgrupamento = ignorarMaquina ? info.Log : nomeArquivo;

                        if (!logsAgrupados.ContainsKey(chaveAgrupamento))
                        {
                            logsAgrupados[chaveAgrupamento] = new StringBuilder();
                        }

                        string textoLimpo = conteudoDia.TrimEnd('\r', '\n');
                        if (logsAgrupados[chaveAgrupamento].Length > 0)
                        {
                            logsAgrupados[chaveAgrupamento].AppendLine();
                        }
                        logsAgrupados[chaveAgrupamento].Append(textoLimpo);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            // Reporta a checagem de arquivos não encontrados
            progress?.Report(new ProgressReport(70, "Verificando arquivos não encontrados..."));

            // 4. Identifica quais arquivos da combinação nunca foram encontrados
            foreach (var item in mapaArquivosEsperados)
            {
                if (!item.Value.Encontrado)
                {
                    logsNaoEncontrados.Add($"Arquivo não encontrado em nenhum dia: {item.Key}");
                }
            }

            // 5. Salva os arquivos consolidados no destino (80% a 100%)
            GravarLogsConsolidados(pastaDestino, logsAgrupados, mapaArquivosEsperados, ignorarMaquina, progress);

            return logsNaoEncontrados;
        }

        private static void GravarLogsConsolidados(
            string pastaDestino,
            Dictionary<string, StringBuilder> logsAgrupados,
            Dictionary<string, (string Maquina, string Log, bool Encontrado)> mapaInfo,
            bool ignorarMaquina,
            IProgress<ProgressReport>? progress = null)
        {
            int totalArquivos = logsAgrupados.Count;
            if (totalArquivos == 0)
            {
                progress?.Report(new ProgressReport(100, "Nenhum arquivo encontrado para consolidação."));
                return;
            }

            Directory.CreateDirectory(pastaDestino);
            int contador = 0;

            foreach (var item in logsAgrupados)
            {
                contador++;
                int pct = 80 + (int)((contador / (double)totalArquivos) * 20);
                string chave = item.Key;

                progress?.Report(new ProgressReport(pct, $"Gravando arquivo {contador} de {totalArquivos}: {chave}"));

                string caminhoFinal;

                if (ignorarMaquina)
                {
                    // Salva diretamente na pasta de destino sem criar subpastas
                    caminhoFinal = Path.Combine(pastaDestino, $"{chave}_Consolidado.txt");
                }
                else
                {
                    // Obtém a máquina a partir do mapa do arquivo
                    string maquina = mapaInfo[chave].Maquina;
                    string pastaMaquinaDestino = Path.Combine(pastaDestino, maquina);

                    if (!Directory.Exists(pastaMaquinaDestino))
                    {
                        Directory.CreateDirectory(pastaMaquinaDestino);
                    }

                    caminhoFinal = Path.Combine(pastaMaquinaDestino, chave);
                }

                string textoConsolidadoBruto = item.Value.ToString();
                File.WriteAllText(caminhoFinal, textoConsolidadoBruto);
            }

            // Reporta 100% apenas APÓS finalizar todos os arquivos da lista
            progress?.Report(new ProgressReport(100, "Consolidação finalizada com sucesso!"));
        }
    }
}