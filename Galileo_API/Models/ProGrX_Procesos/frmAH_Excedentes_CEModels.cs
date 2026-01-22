namespace Galileo_API.Models.ProGrX_Procesos
{
    public class ExcedentesPeriodoValidaResult
    {
        public int? Resultado { get; set; }
    }

    public class ExcedentesCasosEspecialesResult
    {
        public decimal Consec { get; set; }
        public int? Id_Periodo { get; set; }
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public string? Salida { get; set; }
        public string? Detalle { get; set; }
        public decimal? Porcentaje { get; set; }
        public byte[]? Doc_Adjunto { get; set; }
        public string? Registro_Usuario { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string? Modifica_Usuario { get; set; }
        public DateTime? Modifica_Fecha { get; set; }
        public byte[]? Doc_Ajunto { get; set; }
        public string? Socio_Nombre { get; set; }
    }

    public class ExcedentesCasosEspecialNuevoResult
    {
        public string? Cedula { get; set; }
        public string? Cedular { get; set; }
        public string? Nombre { get; set; }
    }

    public class ExcedentesCasosEspecialDetalleResult
    {
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }
        public string? Salida { get; set; }
        public string? Detalle { get; set; }
        public int? Adjunto { get; set; }
    }

    public class ExcedentesCasosEspecialSalidasCambioResult
    {
        public string? Cod_Salida { get; set; }
        public string? Descripcion { get; set; }
    }

    public class ExcedentesPeriodoEstadoResult
    {
        public string? Estado { get; set; }
    }

    // MODELOS UNIFICADOS
    public class OperacionCasoEspecialResult
    {
        public int? Caso_Id { get; set; }
        public int? Pass { get; set; }
    }

    public class CasoEspecialBaseParams
    {
        public int? Id { get; set; }
        public int? PeriodoId { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class ExcedentesCasoEspecialAddParams : CasoEspecialBaseParams
    {
        public string Detalle { get; set; } = string.Empty;
        public decimal? Porcentaje { get; set; }
        public string Salida { get; set; } = string.Empty;
    }

    public class ExcedentesCambioSalidaAddParams : CasoEspecialBaseParams
    {
        public string Detalle { get; set; } = string.Empty;
        public string Salida { get; set; } = string.Empty;
    }

    public class ExcedentesCambioSalidaDeleteParams : CasoEspecialBaseParams
    {
        public string Salida { get; set; } = string.Empty;
    }

    // MODELOS BASE PARA MASIVOS
    public class ExcedentesMassSubeBaseParams
    {
        public int? PeriodoId { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Salida { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public int? Primero { get; set; }
    }

    public class ExcedentesMassCESubeParams : ExcedentesMassSubeBaseParams
    {
        public decimal? Porcentaje { get; set; }
    }

    public class ExcedentesMassCSSubeParams : ExcedentesMassSubeBaseParams
    {
        public int? Autoriza_Ind { get; set; }
        public string Autoriza_Usuario { get; set; } = string.Empty;
    }

    public class ExcedentesMassValidaResult
    {
        public int? Total { get; set; }
        public int? Aplica { get; set; }
        public int? Inco { get; set; }
    }

    // MODELOS BASE PARA CONSULTA MASIVA
    public class ExcedentesMassConsultaBaseResult
    {
        public int? Id_Periodo { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Salida { get; set; } = string.Empty;
        public decimal? Porcentaje { get; set; }
        public string Detalle { get; set; } = string.Empty;
        public string Inconsistencia { get; set; } = string.Empty;
        public int? Aplica { get; set; }
        public int? Procesado { get; set; }
        public DateTime Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
    }

    public class ExcedentesMassCEConsultaResult : ExcedentesMassConsultaBaseResult
    {
        // No campos adicionales
    }

    public class ExcedentesMassCSConsultaResult : ExcedentesMassConsultaBaseResult
    {
        public int? Autoriza_Ind { get; set; }
        public string Autoriza_Usuario { get; set; } = string.Empty;
    }

    public class ExcedentesCasosEspecialesAplicadosParams
    {
        public int? PeriodoId { get; set; }
        public string Salida { get; set; } = string.Empty;
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
    }

    public class ExcedentesCasosEspecialesAplicadosResult
    {
        public decimal Consec { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int? Id_Periodo { get; set; }
        public string Salida { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public string Doc_Adjunto { get; set; } = string.Empty;
        public string Registro_Usuario { get; set; } = string.Empty;
        public DateTime? Registro_Fecha { get; set; }
        public string Modifica_Usuario { get; set; } = string.Empty;
        public DateTime? Modifica_Fecha { get; set; }
        public int? Consec_Apl { get; set; }
        public decimal? Porcentaje { get; set; }
    }

    public class ExcedentesCambioSalidaListaParams
    {
        public int? PeriodoId { get; set; }
        public string Filtro { get; set; } = string.Empty;
        public int? Autorizado { get; set; }
        public string Usuario { get; set; } = string.Empty;
    }

    public class ExcedentesCambioSalidaListaResult
    {
        public int? Consec { get; set; }
        public int? Id_Periodo { get; set; }
        public string Cedula { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
        public string Nueva_Salida { get; set; } = string.Empty;
        public int? Ind_Autorizado { get; set; }
        public DateTime? Registro_Fecha { get; set; }
        public string Registro_Usuario { get; set; } = string.Empty;
        public string? Modifica_Usuario { get; set; }
        public DateTime? Modifica_Fecha { get; set; }
        public DateTime? Autoriza_Fecha { get; set; }
        public string? Autoriza_Usuario { get; set; }
        public string Autorizado_Desc { get; set; } = string.Empty;
        public string Salida_Desc { get; set; } = string.Empty;
        public string Nombre_Desc { get; set; } = string.Empty;
    }
}
