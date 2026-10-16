namespace Galileo_API.Models.ProGrX.Convenios
{
    /// <summary>
    /// Encabezado de la orden (sbConsultaConvenio): estado, disponible, flotante y rebajos vencidos.
    /// </summary>
    public class CrCargosCxPCabecera
    {
        public string cod_convenio { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
        public int cod_orden { get; set; }
        public string estado { get; set; } = string.Empty;
        public string estado_desc { get; set; } = string.Empty;

        /// <summary>TOTAL_PAGAR + TOTAL_REBAJOS_CXP de la orden.</summary>
        public decimal? disponible_bruto { get; set; }
        public decimal? cxp_flotante { get; set; }
        public decimal? rebajo_vencido { get; set; }

        /// <summary>vDisponible del VB6: disponible bruto menos el flotante de CxP.</summary>
        public decimal disponible { get; set; }
    }

    /// <summary>
    /// Rebajo registrado a la orden (spConvenios_Orden_Cargos_CxP).
    /// </summary>
    public class CrCargosCxPLinea
    {
        public int? num_linea { get; set; }
        public string? cod_cargo { get; set; }
        public string? descripcion { get; set; }
        public int? id_transac { get; set; }
        public int? id_fraccion { get; set; }
        public string? documento { get; set; }
        public string? detalle { get; set; }
        public decimal? monto { get; set; }
    }

    /// <summary>
    /// Rebajos registrados con su total y los saldos que muestra el formulario.
    /// </summary>
    public class CrCargosCxPConsulta
    {
        public List<CrCargosCxPLinea> lineas { get; set; } = new();
        public decimal total { get; set; }
        /// <summary>vDisponible - total de rebajos (txtDisponible).</summary>
        public decimal disponible { get; set; }
        public decimal rebajo_vencido { get; set; }
    }

    /// <summary>
    /// Cargo pendiente de cobro (spConvenios_Rebajos_Consulta_Pendientes).
    /// </summary>
    public class CrCargosCxPPendiente
    {
        public int? id_transac { get; set; }
        public int? fracciones_pendientes { get; set; }
        public decimal? cargo { get; set; }
        public decimal? saldo { get; set; }
        public string? cargodesc { get; set; }
        public string? documento { get; set; }
        public string? detalle { get; set; }
        public DateTime? cobro_inicio_fecha { get; set; }
        public string? cod_cargo { get; set; }
        public int? numero_pagos { get; set; }

        /// <summary>Numero_Pagos + 1: fracción que se cobra (txtNFraccion).</summary>
        public int no_fraccion => (numero_pagos ?? 0) + 1;
    }

    /// <summary>
    /// Cargo de CxP que el convenio permite rebajar (búsqueda F4).
    /// </summary>
    public class CrCargosCxPCargo
    {
        public string cod_cargo { get; set; } = string.Empty;
        public string descripcion { get; set; } = string.Empty;
    }

    /// <summary>
    /// Alta de un rebajo (btnGuardar_Click, acción 'A').
    /// </summary>
    public class CrCargosCxPRegistroRequest
    {
        public string cod_convenio { get; set; } = string.Empty;
        public int cod_orden { get; set; }
        /// <summary>Transacción del cargo pendiente; 0 si el rebajo no proviene de uno.</summary>
        public int id_transac { get; set; }
        public string cod_cargo { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string? documento { get; set; }
        public string? detalle { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    /// <summary>
    /// Edición de una línea desde la grilla (vGrid_KeyDown, acción 'E').
    /// </summary>
    public class CrCargosCxPEdicionRequest
    {
        public string cod_convenio { get; set; } = string.Empty;
        public int cod_orden { get; set; }
        public int num_linea { get; set; }
        public int id_transac { get; set; }
        public string cod_cargo { get; set; } = string.Empty;
        public decimal monto { get; set; }
        public string? documento { get; set; }
        public string? detalle { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    /// <summary>
    /// Borrado de una línea (vGrid_ButtonClicked, acción 'B').
    /// </summary>
    public class CrCargosCxPBorradoRequest
    {
        public string cod_convenio { get; set; } = string.Empty;
        public int cod_orden { get; set; }
        public int num_linea { get; set; }
        public string usuario { get; set; } = string.Empty;
    }

    /// <summary>
    /// Monto que devuelve el SP al editar una línea.
    /// </summary>
    public class CrCargosCxPEdicionResultado
    {
        public decimal? monto { get; set; }
    }
}
