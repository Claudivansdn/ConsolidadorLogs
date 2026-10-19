using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace ConsolidadorLogs.Core
{
    public static class ConfigManager
    {
        private static readonly string CaminhoPastaConfig = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ConsolidadorLogs");

        private static readonly string ArquivoMaquinas = Path.Combine(CaminhoPastaConfig, "maquinas.json");
        private static readonly string ArquivoLogs = Path.Combine(CaminhoPastaConfig, "logs.json");

        // Listas padrão iniciais (usadas na primeira vez que o app rodar)
        private static readonly List<string> MaquinasPadrao = new()
        {
            "EP2091KS01", "EP2091KS02", "EP2091KS03", "EP2091KS04", "EP2091KS05",
            "RC2092KS01", "RC2092KS02", "RC2092KS03", "RC2092KS04", "RC2092KS05", "RC2092KS06",
            "EP2020KS01", "EP2020KS02", "EP2020KS03",
            "RC2020KS01", "RC2020KS02", "RC2020KS03"
        };

        private static readonly List<string> LogsPadrao = new()
        {
            "MACHINE_RECLAIMING_FLOW_BRIDGE",
            "MACHINE_CURRENT_OPERATION_MODE_BRIDGE",
            "MACHINE_FLOW_SETPOINT_CLAMPED_BRIDGE",
            "MACHINE_OPERATION_BELT_WORKING_BRIDGE",
            "MACHINE_STATUS_BUCKETWHEEL_ON_BRIDGE"
        };

        private static void GarantePastaExiste()
        {
            if (!Directory.Exists(CaminhoPastaConfig))
            {
                Directory.CreateDirectory(CaminhoPastaConfig);
            }
        }

        // Carrega máquinas do JSON (ou cria com o padrão se não existir)
        public static List<string> CarregarMaquinas()
        {
            GarantePastaExiste();
            if (!File.Exists(ArquivoMaquinas))
            {
                SalvarMaquinas(MaquinasPadrao);
                return new List<string>(MaquinasPadrao);
            }

            try
            {
                string json = File.ReadAllText(ArquivoMaquinas);
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>(MaquinasPadrao);
            }
            catch
            {
                return new List<string>(MaquinasPadrao);
            }
        }

        // Salva a lista atualizada de máquinas
        public static void SalvarMaquinas(List<string> maquinas)
        {
            GarantePastaExiste();
            string json = JsonSerializer.Serialize(maquinas, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ArquivoMaquinas, json);
        }

        // Carrega logs do JSON (ou cria com o padrão se não existir)
        public static List<string> CarregarLogs()
        {
            GarantePastaExiste();
            if (!File.Exists(ArquivoLogs))
            {
                SalvarLogs(LogsPadrao);
                return new List<string>(LogsPadrao);
            }

            try
            {
                string json = File.ReadAllText(ArquivoLogs);
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>(LogsPadrao);
            }
            catch
            {
                return new List<string>(LogsPadrao);
            }
        }

        // Salva a lista atualizada de logs
        public static void SalvarLogs(List<string> logs)
        {
            GarantePastaExiste();
            string json = JsonSerializer.Serialize(logs, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ArquivoLogs, json);
        }

        // Restaura as máquinas padrão originais e sobrescreve o arquivo JSON
        public static List<string> RestaurarMaquinasPadrao()
        {
            SalvarMaquinas(MaquinasPadrao);
            return new List<string>(MaquinasPadrao);
        }

        // Restaura os logs padrão originais e sobrescreve o arquivo JSON
        public static List<string> RestaurarLogsPadrao()
        {
            SalvarLogs(LogsPadrao);
            return new List<string>(LogsPadrao);
        }        
    }
}