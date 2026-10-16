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
        public static void ProcessarPasta(string pastaDestino)
        {
            if (!Directory.Exists(pastaDestino))
            {
                throw new DirectoryNotFoundException("A pasta informada para substituição de texto não foi encontrada.");
            }

            // Busca todos os arquivos .txt salvos na pasta destino
            string[] arquivos = Directory.GetFiles(pastaDestino, "*.txt", SearchOption.AllDirectories);

            foreach (string caminhoArquivo in arquivos)
            {
                try
                {
                    // Lê o conteúdo bruto do arquivo consolidado
                    string conteudoOriginal = File.ReadAllText(caminhoArquivo);

                    // Aplica as substituições
                    string conteudoTratado = ProcessarTexto(conteudoOriginal);

                    // Sobrescreve o arquivo no disco com os dados formatados
                    File.WriteAllText(caminhoArquivo, conteudoTratado);
                }
                catch
                {
                    // Se falhar a leitura/escrita de algum arquivo específico, ignora e continua os demais
                    continue;
                }
            }
        }
    }
}