namespace Galileo.Models.ProGrX_Personas
{
    public class AfLiquidacionMasivaFiltros
    {
        public DateTime? Inicio { get; set; }
        public DateTime? Corte { get; set; }
        public string? Tipo { get; set; }
        public int? Institucion { get; set; }
        public int? Causa { get; set; }
        public string Cedula { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Ejecutivo { get; set; } = "";
        public string Usuario { get; set; } = "";
    }

    public class AfLiquidacionMasiva
    {
        public int Cod_Renuncia { get; set; }
        public string? Cedula { get; set; }              // Cédula
        public string? Nombre { get; set; }              // Nombre
        public string? Tipo_Desc { get; set; }           // Tipo
        public string? Causa_Desc { get; set; }          // Causa
        public string? Estado_Desc { get; set; }         // Estado
        public string? Resuelto_Fecha_Mask { get; set; } // Res.Fecha (formateada)
        public string? Resuelto_User { get; set; }       // Res.Usuario
        public string? Registro_Fecha_Mask { get; set; } // Reg.Fecha (formateada)
        public string? Registro_User { get; set; }       // Reg.Usuario
        public string? Promotor_Desc { get; set; }       // Ejecutivo
    }

    // ============================================================
    //  Proceso de liquidación masiva por lotes (reanudable)
    //  Encabezado + detalle: permite mostrar avance y reanudar si
    //  el usuario cierra el navegador. El procesamiento es
    //  secuencial (una renuncia a la vez) para no arriesgar los
    //  consecutivos/contabilidad del SP spAFI_Renuncia_Liquidacion_Procesa.
    // ============================================================

    /// <summary>Renuncia individual seleccionada para liquidar.</summary>
    public class AfLiqMasivaRenunciaItem
    {
        public int Cod_Renuncia { get; set; }
        public string? Cedula { get; set; }
        public short S06 { get; set; } = 1;
    }

    /// <summary>Petición para iniciar (o reanudar) un proceso de liquidación masiva.</summary>
    public class AfLiqMasivaIniciarRequest
    {
        public string Usuario { get; set; } = "";
        public List<AfLiqMasivaRenunciaItem> Renuncias { get; set; } = new();
    }

    /// <summary>Avance del proceso; lo consulta el front para mostrar el Swal y reanudar.</summary>
    public class AfLiqMasivaProgreso
    {
        public Guid Proceso_Id { get; set; }
        public string Estado { get; set; } = "";   // Procesando | Completado | Error
        public int Total { get; set; }
        public int Procesadas { get; set; }
        public int Exitosas { get; set; }
        public int Errores { get; set; }
        public string? Mensaje { get; set; }
        // true cuando Iniciar detectó un proceso activo previo y lo devolvió para reanudar.
        public bool Reanudado { get; set; }
    }

    /// <summary>Fila de detalle pendiente que consume el procesamiento por lotes.</summary>
    internal class AfLiqMasivaDetalleRow
    {
        public long Detalle_Id { get; set; }
        public int Cod_Renuncia { get; set; }
        public short S06 { get; set; }
    }
}