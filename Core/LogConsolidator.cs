using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConsolidadorLogs.Core
{
    public class LogConsolidator
    {
        /// <summary>
        /// Varre todas as subpastas de origem procurando exatamente por "NomeMaquina_NomeLog.txt",
        /// consolida o conteúdo dos arquivos encontrados e salva nas pastas das máquinas no destino.
        /// </summary>
        public static List<string> Executar(
            string pastaOrigem,
            string pastaDestino,
            DateTime dataInicio,
            DateTime dataFim,
            List<string> maquinasSelecionadas,
            List<string> logsSelecionados)
        {
            if (!Directory.Exists(pastaOrigem))
            {
                throw new DirectoryNotFoundException("A pasta de origem selecionada não foi encontrada.");
            }

            List<string> logsNaoEncontrados = new List<string>();

            // 1. Mapeia todos os nomes de arquivos EXATOS que o sistema deve procurar
            // Chave do Dicionário: "NomeMaquina_NomeLog.txt"
            // Valor: Estrutura [Maquina, Log, Encontrado (bool)]
            Dictionary<string, (string Maquina, string Log, bool Encontrado)> mapaArquivosEsperados = new();

            foreach (var maq in maquinasSelecionadas)
            {
                foreach (var log in logsSelecionados)
                {
                    string nomeArquivoExato = $"{maq}_{log}.txt";
                    mapaArquivosEsperados[nomeArquivoExato] = (maq, log, false);
                }
            }

            // 2. Busca todos os arquivos .txt na origem e em todas as suas subpastas
            string[] todosArquivos = Directory.GetFiles(pastaOrigem, "*.txt", SearchOption.AllDirectories);

            // 3. Estrutura de acúmulo: Chave = "NomeMaquina_NomeLog.txt" -> Valor = Conteúdo acumulado
            Dictionary<string, StringBuilder> logsAgrupados = new();

            foreach (string caminhoArquivo in todosArquivos)
            {
                string nomeArquivo = Path.GetFileName(caminhoArquivo);
                DateTime dataCriacao = File.GetLastWriteTime(caminhoArquivo);

                // Filtro por Data
                if (dataCriacao.Date < dataInicio.Date || dataCriacao.Date > dataFim.Date)
                {
                    continue;
                }

                // Verifica se o arquivo atual tem o nome EXATO "NomeMaquina_NomeLog.txt" da nossa lista esperada
                if (mapaArquivosEsperados.TryGetValue(nomeArquivo, out var info))
                {
                    // Marca que pelo menos um arquivo dessa combinação foi encontrado
                    mapaArquivosEsperados[nomeArquivo] = (info.Maquina, info.Log, true);

                    try
                    {
                        string conteudoDia = File.ReadAllText(caminhoArquivo);

                        if (!logsAgrupados.ContainsKey(nomeArquivo))
                        {
                            logsAgrupados[nomeArquivo] = new StringBuilder();
                        }

                        // Concatena os logs
                        // Remove quebras de linha no final para evitar linhas em branco extras
                        string textoLimpo = conteudoDia.TrimEnd('\r', '\n');
                        if (logsAgrupados[nomeArquivo].Length > 0) // Adiciona uma linha em branco entre os dias, se já houver conteúdo acumulado

                        {
                            logsAgrupados[nomeArquivo].AppendLine(); // Adiciona uma linha em branco entre os dias
                        }
                        logsAgrupados[nomeArquivo].Append(textoLimpo); 
                    }
                    catch
                    {
                        // Se falhar na leitura de um arquivo específico, ignora e continua
                        continue;
                    }
                }
            }

            // 4. Identifica quais arquivos da combinação "NomeMaquina_NomeLog.txt" nunca foram encontrados
            foreach (var item in mapaArquivosEsperados)
            {
                if (!item.Value.Encontrado)
                {
                    logsNaoEncontrados.Add($"Arquivo não encontrado em nenhum dia: {item.Key}");
                }
            }

            // 5. Salva os arquivos consolidados no destino
            GravarLogsConsolidados(pastaDestino, logsAgrupados, mapaArquivosEsperados);

            return logsNaoEncontrados;
        }

        private static void GravarLogsConsolidados(
            string pastaDestino,
            Dictionary<string, StringBuilder> logsAgrupados,
            Dictionary<string, (string Maquina, string Log, bool Encontrado)> mapaInfo)
        {
            foreach (var item in logsAgrupados)
            {
                string nomeArquivo = item.Key;
                string maquina = mapaInfo[nomeArquivo].Maquina;

                // Cria a pasta da máquina no destino (ex: Destino/RC2092KS01)
                string pastaMaquinaDestino = Path.Combine(pastaDestino, maquina);
                if (!Directory.Exists(pastaMaquinaDestino))
                {
                    Directory.CreateDirectory(pastaMaquinaDestino);
                }

                // Obtém o texto bruto totalmente unificado de todos os dias
                string textoConsolidadoBruto = item.Value.ToString();

                // Salva o arquivo final consolidado
                string caminhoFinal = Path.Combine(pastaMaquinaDestino, nomeArquivo);
                File.WriteAllText(caminhoFinal, textoConsolidadoBruto);
            }
        }
    }
}