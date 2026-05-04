using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace Galileo_API.Models.ProGrX.Cobros
{
    public class FrmCoAplFndPrioridadesModels
    {
        public class COAplFndPrioridadesListaResult
        {
            public int total { get; set; }
            public List<COAplFndPrioridadData> lista { get; set; } = new();
        }

        public class COAplFndPrioridadData
        {
            public string codigo { get; set; } = string.Empty;
            public string descripcion { get; set; } = string.Empty;
            public int orden { get; set; } = 0;
            public bool activo { get; set; } = false;
            public string registro_fecha { get; set; } = string.Empty;
            public string registro_usuario { get; set; } = string.Empty;
            public string modifica_fecha { get; set; } = string.Empty;
            public string modifica_usuario { get; set; } = string.Empty;

            [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
            public bool isNew { get; set; }
        }

        public static class CoAplFndPrioridadConst
        {
            public const int vModulo = 4;

            public const string SF_CODIGO = "codigo";
            public const string SF_DESCRIPCION = "descripcion";
            public const string SF_ORDEN = "orden";
            public const string SF_ACTIVO = "activo";
            public const string SF_REGISTRO_FECHA = "registro_fecha";
            public const string SF_REGISTRO_USUARIO = "registro_usuario";
            public const string SF_MODIFICA_FECHA = "modifica_fecha";
            public const string SF_MODIFICA_USUARIO = "modifica_usuario";

            public const string SP_LISTA = @"EXEC spCBR_Pagos_Apl_Config_Prioridades_Lista;";
            public const string SP_ADD = @"EXEC spCBR_Pagos_Apl_Config_Prioridades_Add @Codigo, @Orden, @Activo, @Usuario;";
            public const string SP_UPD = @"EXEC spCBR_Pagos_Apl_Config_Prioridades_Add @Codigo, @Orden, @Activo, @Usuario;";
            public const string SP_DEL = @"EXEC spCBR_Pagos_Apl_Config_Prioridades_Del @Codigo, @Usuario;";
            public const string SP_PRIORIDAD_EJECUCION = @"EXEC spCBR_Pagos_Apl_Config_Prioridad_Ejecucion_Actualiza @Valor, @Usuario;";
        }
    }
}
