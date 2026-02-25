namespace Galileo_API.Models.ProGrX_Polizas
{
    public class FrmPolizasPeConsultasModels
    {
        public class PolizasPeConsultasBuscarRequestDto
        {
            public bool SoloVencida { get; set; } = false;               // chkVencida
            public bool FiltrarVenceCobertura { get; set; } = false;      // chkVenceCobertura
            public DateTime? VenceInicio { get; set; }            // dtpVenceInicio
            public DateTime? VenceCorte { get; set; }             // dtpVenceCorte
            public int? PresentacionId { get; set; }              // cboPresentacion (null = TODOS)
            public string? CombustibleId { get; set; }               // cboCombustible
            public int? ModeloId { get; set; }                    // cboModelo
            public string? EstadoPersonaId { get; set; }          // cboEstadoPersona (en VB6 parece string)
            public int? Anio { get; set; }                        // txtAnio
            public int? PuertasNumero { get; set; }               // cboPuertas (null = No Aplica)
            public string? PesoUd { get; set; }                      // cboPeso.ItemData
            public decimal? PesoInicio { get; set; }              // txtPesoInicio
            public decimal? PesoCorte { get; set; }               // txtPesoCorte
            public string? CapacidadUd { get; set; }
            public decimal? CapacidadInicio { get; set; }
            public decimal? CapacidadCorte { get; set; }
            public string? CilindrajeUd { get; set; }
            public decimal? CilindrajeInicio { get; set; }
            public decimal? CilindrajeCorte { get; set; }
            public string? UserRegistra { get; set; }             // txtUserRegistra (LIKE)
            public string? UserActualiza { get; set; }            // txtUserActualiza (LIKE)
            public string? PersonaId { get; set; }                // txtPersonaId (Cedula LIKE)
            public string? Nombre { get; set; }                   // txtNombre (Nombre LIKE)
            public string? IdPrincipal { get; set; }              // txtIdPrincipal (LIKE)
            public string? IdProvisional { get; set; }            // txtIdSecundario (LIKE)
            public string? ChasisNumero { get; set; }             // txtChasisNo (LIKE)
            public string? VinMotor { get; set; }                 // txtVINMotor (LIKE)
            public string? Color { get; set; }                    // txtColor (LIKE)
            public string? Filtro { get; set; } //filtro del buscar en tablas o buscador
            public int? Pagina { get; set; } = 1;//pagina de la tabla
            public int? Paginacion { get; set; } = 30; //paginacion de la tabla
            public int? SortOrder { get; set; } = 0; //0: sin orden, 1: ascendente, 2: descendente
            public string? SortField { get; set; } //campo por el cual se ordena

        }

        public class PolizasPeConsultasDto
        { 
            public long? PrendaId { get; set; }
            public string? CodPreanalisis { get; set; }
            public string? IdSolicitud { get; set; }
            public string? Cedula { get; set; }
            public string? Nombre { get; set; }
            public string? TipoPrendaDesc { get; set; }
            public string? Descripcion { get; set; }

            public string? Cobertura { get; set; }
            public decimal? PorcCobertura { get; set; }
            public string? EstadoDesc { get; set; }
            public string? IdPrincipal { get; set; }
            public string? IdProvisional { get; set; }

            public decimal? Avaluo { get; set; }
            public decimal? ValorFiscal { get; set; }
            public decimal? ValorMercado { get; set; }

            public decimal? CreditoMonto { get; set; }
            public decimal? CreditoSaldo { get; set; }
            public string? CreditoDivisa { get; set; }

            public DateTime? RegistroFecha { get; set; }
            public string? RegistroUsuario { get; set; }
            public DateTime? ActualizaFecha { get; set; }
            public string? ActualizaUsuario { get; set; }

            public string? ComercializaDesc { get; set; }
            public string? MarcaDesc { get; set; }
            public string? ModeloDesc { get; set; }
            public int? Anio { get; set; }
            public string? PresentacionDesc { get; set; }

            public string? Serie { get; set; }
            public string? Color { get; set; }
            public string? ChasisNumero { get; set; }
            public string? VinMotor { get; set; }

            public int? PuertasNumero { get; set; }
            public decimal? Peso { get; set; }
            public decimal? Capacidad { get; set; }
            public decimal? Cilindraje { get; set; }

            public string? Tomo { get; set; }
            public string? Folio { get; set; }
            public string? Notario { get; set; }
            public DateTime? NotarioRegistroFecha { get; set; }

            public string? PolizaMntFormalizacion { get; set; }
            public string? PolizaRstPlan { get; set; }

            public string? PesoUdDesc { get; set; }
            public string? CapacidadUdDesc { get; set; }
            public string? CilindrajeUdDesc { get; set; }

            public string? PeActiva { get; set; }        // 'Sí'/'No' como VB6
            public string? PeNumero { get; set; }
            public DateTime? PeVence { get; set; }
            public decimal? PePrima { get; set; }
            public string? PeFrecuencia { get; set; }
            public string? PeVencida { get; set; }       // 'Sí'/'No'

            public string? PeCedula { get; set; }
            public string? PeNombre { get; set; }
            public string? PeCobertura { get; set; }

            public string? TitularTercero { get; set; }  // 'Sí'/'No'
            public string? TitularNombre { get; set; }
        }
        public sealed class PolizasPeConsultasBuscarResponseDto
        {
            public int? TotalRegistros { get; set; }
            public decimal? TotalValorMercado { get; set; }
            public List<PolizasPeConsultasDto> Items { get; set; } = [];
        }

        public sealed class UnidadRangoFilter
        {
            public string UnidadColumn { get; init; } = string.Empty;
            public string UnidadParam { get; init; } = string.Empty;
            public string? UnidadValue { get; init; }

            public string RangoColumn { get; init; } = string.Empty;
            public string RangoInicioParam { get; init; } = string.Empty;
            public string RangoCorteParam { get; init; } = string.Empty;

            public decimal? RangoInicio { get; init; }
            public decimal? RangoCorte { get; init; }
        }

    }
}
