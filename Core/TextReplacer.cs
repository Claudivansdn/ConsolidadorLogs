using System;
using System.IO;

namespace ConsolidadorLogs.Core
{
    public static class TextReplacer
    {
        /// <summary>
        /// Processa o texto fornecido aplicando as substituições de formato:
        /// - Substitui 'true' e 'True' por '1'
        /// - Substitui 'false' e 'False' por '0'
        /// - Substitui ponto '.' por vírgula ','
        /// </summary>
        public static string ProcessarTexto(string conteudo)
        {
            if (string.IsNullOrEmpty(conteudo))
            {
                return string.Empty;
            }

            // 1. Substitui valores booleanos por 1 e 0
            conteudo = conteudo.Replace("true", "1")
                               .Replace("True", "1")
                               .Replace("false", "0")
                               .Replace("False", "0");

            // 2. Ajusta o separador decimal (. por ,)
            conteudo = conteudo.Replace(".", ",");

            return conteudo;
        }

        /// <summary>
        /// Varre todos os arquivos .txt na pasta informada (e suas subpastas)
        /// e aplica as substituições de texto sobrescrevendo os arquivos originais.
        /// </summary>
        public static void ProcessarPasta(string pastaDestino, IProgress<ProgressReport>? progress = null)
        {
            if (!Directory.Exists(pastaDestino)) 
            {
                throw new DirectoryNotFoundException("A pasta informada para substituição de texto não foi encontrada.");
            }

            progress?.Report(new ProgressReport(0, "Iniciando processamento de arquivos..."));

            // Busca todos os arquivos .txt salvos na pasta destino
            string[] arquivos = Directory.GetFiles(pastaDestino, "*.txt", SearchOption.AllDirectories);
            int total = arquivos.Length;

            if (total == 0)
            {
                progress?.Report(new ProgressReport(100, "Nenhum arquivo encontrado para formatação."));
                return;
            }

            for (int i = 0; i < total; i++)
            {
                int pct = (int)(((i + 1) / (double)total) * 100); // Calcula o percentual de progresso (0% a 100%)
                progress?.Report(new ProgressReport(pct, $"Formatando textos (pontos e booleanos) [{i + 1}/{total}]..."));
                
                try
                {
                    string conteudoOriginal = File.ReadAllText(arquivos[i]);
                    string conteudoTratado = ProcessarTexto(conteudoOriginal);
                    File.WriteAllText(arquivos[i], conteudoTratado);
                }
                catch
                {
                    // Se falhar a leitura/escrita de algum arquivo específico, ignora e continua os demais
                    continue;
                }
            }

            // Reporta 100% apenas ao FINAL de todos os arquivos
            progress?.Report(new ProgressReport(100, "Formatação de textos concluída com sucesso."));
        }
    }
}