namespace Galileo.Models.ProGrX.Fondos
{
    public class FndNotificacionData
    {
        // rs!OPERADORA
        public string cod_operadora { get; set; } = string.Empty;

        // rs!PLAN
        public string cod_plan { get; set; } = string.Empty;

        public string? plan_descripcion { get; set; }

        // rs!DESCRIPCION
        public string? descripcion { get; set; } = string.Empty;

        // rs!COD_NOTIFICACION
        public int cod_notificacion { get; set; } = 0;

        // rs!TIPO_MOV_DESC
        public string tipo_mov_desc { get; set; } = string.Empty;

        // rs!RANGO (en VB se formatea "Standard")
        public required decimal rango { get; set; }  // si en BD es numérico con decimales, decimal es lo más seguro

        // rs!ACTIVO (0/1 -> bool)
        public required bool activo { get; set; }

        // rs!NOTIFICACION1 & ""
        public string? notificacion1 { get; set; } = string.Empty;

        // rs!NOTIFICACION2 & ""
        public string? notificacion2 { get; set; } = string.Empty;

        // rs!NOTIFICACION3 & ""
        public string? notificacion3 { get; set; } = string.Empty;

        // rs!Registro_fecha
        public DateTime? registro_fecha { get; set; }

        // rs!Registro_Usuario & ""
        public string? registro_usuario { get; set; } = string.Empty;

        // rs!Modifica_fecha
        public DateTime? modifica_fecha { get; set; }

        // rs!Modifica_Usuario & ""
        public string? modifica_usuario { get; set; } = string.Empty;

        public string? codigo { get; set; } = string.Empty;

        public object? tipo_mov_codigo { get; set; }
    }
}