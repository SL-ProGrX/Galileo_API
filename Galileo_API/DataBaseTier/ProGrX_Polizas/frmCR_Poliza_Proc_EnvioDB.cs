using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmCrPolizaProcEnvioDb
    {
        private readonly PortalDB _portalDb;

        public FrmCrPolizaProcEnvioDb(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Obtiene listado de pólizas para cargar combo (cboPoliza en VB6).
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> Crd_PolizasProcEnvio_Catalogo_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
                        SELECT
                            COD_POLIZA     AS item,
                            DESCRIPCION    AS descripcion
                        FROM CRD_CATALOGO_POLIZAS
                        ORDER BY DESCRIPCION";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Metodo para obtener metadata de la grilla según el tipo de póliza, basado en el código de póliza seleccionado. En tu VB6 esto se hacía con un Select Case sobre el tipo obtenido desde la BD, y luego se armaba la grilla con los headers correspondientes.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto<CrdPolizaGridMetaResponseDto> Crd_PolizasProcEnvio_GridMeta_Obtener(int CodEmpresa,CrdPolizaGridMetaRequestDto req)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            var response = new ErrorDto<CrdPolizaGridMetaResponseDto>();

            var codPoliza = (req.cod_poliza ?? "").Trim();
            if (string.IsNullOrWhiteSpace(codPoliza))
            {
                return DbHelper.CreateErrorResponse<CrdPolizaGridMetaResponseDto>("Debe indicar el código de la póliza.");
            }
                        
                    var tipo = conn.ExecuteScalar<string>(
                        "exec spPolizas_Tipo_Aplicacion @cod_poliza",
                        new { cod_poliza = codPoliza }
                    ) ?? "";

                    tipo = tipo.Trim().ToUpperInvariant();
                    if (string.IsNullOrWhiteSpace(tipo))
                        return DbHelper.CreateErrorResponse<CrdPolizaGridMetaResponseDto>("No se pudo determinar el tipo de póliza.");

                    // 2) Armar columnas (VB6: Select Case rs!Tipo ... Add headers)
                    var columns = BuildColumnsByTipo(tipo);

                    var resp = new CrdPolizaGridMetaResponseDto
                    {
                        tipo = tipo,
                        columns = columns
                    };

            response.Result = resp;

            return response;
        }

        /// <summary>
        /// Metodo para consultar los datos de la póliza según el código y tipo, aplicando el análisis (Crédito o Retención) y fecha de corte. En tu VB6 esto se hacía con un Select Case sobre el tipo para determinar qué SP ejecutar, y luego se llenaba la grilla con los resultados.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto<CrdPolizaConsultaResponseDto> Crd_PolizasProcEnvio_Consultar(
            int CodEmpresa,
            CrdPolizaConsultaRequestDto req)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, CodEmpresa);
            var response = new ErrorDto<CrdPolizaConsultaResponseDto>();

            var codPoliza = (req.cod_poliza ?? "").Trim();

            if (string.IsNullOrWhiteSpace(codPoliza))
                return DbHelper.CreateErrorResponse<CrdPolizaConsultaResponseDto>("Debe indicar la póliza.");

            // 1️⃣ Obtener tipo
            string tipoRow = conn.QueryFirstOrDefault<string>(
                "exec spPolizas_Tipo_Aplicacion @cod_poliza",
                new { cod_poliza = codPoliza }) ?? string.Empty;

             if (tipoRow == null)
                return DbHelper.CreateErrorResponse<CrdPolizaConsultaResponseDto>("No se pudo determinar el tipo.");

            string tipo = tipoRow.Trim().ToUpper();

            // 2️⃣ Determinar SP según tipo + análisis
            string sql = BuildSqlByTipo(tipo, req.analisis, new Exception($"Tipo de póliza no soportado: {tipo}")
);

            // 3️⃣ Ejecutar SP principal
            var data = conn.Query(
                sql,
                new
                {
                    Poliza = codPoliza,
                    Corte = req.fecha_corte.ToString("yyyy-MM-dd")
                }).ToList();

            // 4️⃣ Convertir a Dictionary<string, object?>
            var rows = data
                .Select(r => (IDictionary<string, object>)r)
                .Select(dict => dict.ToDictionary(
                    k => k.Key.ToUpperInvariant(),   
                    v => v.Value,
                    StringComparer.OrdinalIgnoreCase
                ))
                .ToList();

            // 5️⃣ Obtener columnas (reutilizamos método anterior)
            var columns = BuildColumnsByTipo(tipo);

            response.Result = new CrdPolizaConsultaResponseDto
            {
                tipo = tipo,
                columns = columns,
                rows = rows!,
                total = rows.Count
            };

            return response;
        }


        private static string BuildSqlByTipo(string tipo, string analisis, Exception exception)
        {
            bool esCredito = analisis?.StartsWith("C", StringComparison.OrdinalIgnoreCase) == true;

            return tipo switch
            {
                "PPC" => "exec spPoliza_PPC_Cierre @Poliza, @Corte",

                "HVID" or "PVID" =>
                    esCredito
                        ? "exec spPoliza_Sicama @Poliza, @Corte"
                        : "exec spPoliza_Sicama_Retencion @Poliza, @Corte",

                "PREN" =>
                    esCredito
                        ? "exec spPoliza_Prendas_Cierre @Poliza, @Corte"
                        : "exec spPoliza_Prendas_Cierre_Retencion @Poliza, @Corte",

                "PVEH" =>
                    esCredito
                        ? "exec spPoliza_Prendas_Cierre @Poliza, @Corte"
                        : "exec spPoliza_Prendas_Cierre_Retencion @Poliza, @Corte",

                "PINC" or "PINCC" =>
                    "exec spPoliza_Incendio_Cierre_Retencion @Poliza, @Corte, 0, '', 'T'",

                _ => throw exception
            };
        }

        private static List<GridColumnDto> BuildColumnsByTipo(string tipo)
        {
            static GridColumnDto Col(string field, string title, int width, string align = GridConstants.Align.Left, string? format = null)
                => new() { field = field, title = title, width = width, align = align, format = format };

            static List<GridColumnDto> DedupByField(IEnumerable<GridColumnDto> cols)
                => cols
                    .GroupBy(c => c.field ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .Select(g => g.First())
                    .ToList();

            // ============================================================
            // BLOQUES (definidos una sola vez, para que Sonar no vea copy/paste)
            // ============================================================

            // Persona (CEDULA + nombres)
            static IEnumerable<GridColumnDto> PersonaCedula()
                => new[]
                {
            Col(GridConstants.Fields.Cedula, GridConstants.Titles.Cedula, 2000),
            Col("APELLIDO_1", "Apellido 1", 2000),
            Col("APELLIDO_2", "Apellido 2", 2000),
            Col("NOMBRE_1", "Nombre 1", 2000),
            Col("NOMBRE_2", "Nombre 2", 2000),
                };

            static IEnumerable<GridColumnDto> Demografia(int fechaWidth, bool incluyeEdad)
                => incluyeEdad
                    ? new[]
                    {
                Col(GridConstants.Fields.Genero, GridConstants.Titles.Genero, 1000, GridConstants.Align.Center),
                Col(GridConstants.Fields.FechaNacimiento, GridConstants.Titles.FechaNacimiento, fechaWidth, GridConstants.Align.Center, GridConstants.Formats.DateYMD),
                Col("EDAD", "Edad", 1000, GridConstants.Align.Center),
                    }
                    : new[]
                    {
                Col(GridConstants.Fields.Genero, GridConstants.Titles.Genero, 1000, GridConstants.Align.Center),
                Col(GridConstants.Fields.FechaNacimiento, GridConstants.Titles.FechaNacimiento, fechaWidth, GridConstants.Align.Center, GridConstants.Formats.DateYMD),
                    };

            static IEnumerable<GridColumnDto> Contacto()
                => new[]
                {
            Col(GridConstants.Fields.Email, GridConstants.Titles.Email, 2000),
            Col(GridConstants.Fields.Telefono, GridConstants.Titles.Telefono, 2000, GridConstants.Align.Center),
                };

            static IEnumerable<GridColumnDto> CreditoCRD()
                => new[]
                {
            Col("CREDITO_OPERACION", GridConstants.Titles.CrdOperacion, 2000, GridConstants.Align.Center),
            Col("CREDITO_CODIGO",    GridConstants.Titles.CrdCodigo,    1500, GridConstants.Align.Center),
            Col(GridConstants.Fields.CreditoMonto, GridConstants.Titles.CrdMonto, 1500, GridConstants.Align.Right, GridConstants.Formats.Numeric2),
            Col(GridConstants.Fields.CreditoSaldo, GridConstants.Titles.CrdSaldo, 1500, GridConstants.Align.Right, GridConstants.Formats.Numeric2),
            Col("CREDITO_ESTADO",    GridConstants.Titles.CrdEstado,    2000, GridConstants.Align.Center),
            Col("VINCULADAS",        GridConstants.Titles.CrdOps,       2000, GridConstants.Align.Center),
                };

            static IEnumerable<GridColumnDto> PolizaBase(int widthOperacion = 1100)
                => new[]
                {
            Col("ID_SOLICITUD", "Pol.Operación", widthOperacion, GridConstants.Align.Center),
            Col("COD_POLIZA", "Pol.Código", widthOperacion, GridConstants.Align.Center),
            Col("CODIGO", "Pol.Retención", widthOperacion, GridConstants.Align.Center),
                };

            // Vehículo: PREN vs PVEH (parámetro mata el copy/paste)
            static IEnumerable<GridColumnDto> Vehiculo(bool incluyeMotorYChasis)
            {
                var baseVeh = new List<GridColumnDto>
        {
            Col("USO", incluyeMotorYChasis ? "Uso del Vehículo" : "Uso", 2000),
            Col("PLACA", "Placa", 2000),
            Col("ID_PROVISIONAL", "Id Provisional", 2000),
            Col("MARCA_DESC", "Marca", 2000),
            Col("MODELO_DESC", "Modelo", 2000),
            Col("PRESENTACION_DESC", "Presentación", 2000),
            Col("ANIO", "Año Vehículo", 1000, GridConstants.Align.Center),
        };

                if (incluyeMotorYChasis)
                {
                    baseVeh.Add(Col("VIN_MOTOR", "Motor CC", 2000));
                    baseVeh.Add(Col("CHASIS_NUMERO", "Chasis", 3000));
                }

                baseVeh.Add(Col("COLOR", "Color", 2000));
                return baseVeh;
            }

            static IEnumerable<GridColumnDto> MontoYMovimiento()
                => new[]
                {
            Col("MONTO", GridConstants.Titles.MontoAsegurado, 2000, GridConstants.Align.Right, GridConstants.Formats.Numeric2),
            Col(GridConstants.Fields.Movimiento, GridConstants.Titles.Movimiento, 2000),
                };

            // PCG / PDE (Identificacion / NombreCompleto, etc.)
            static IEnumerable<GridColumnDto> PersonaIdentificacionBase()
                => new[]
                {
            Col("Identificacion", "Identificación", 2000),
            Col("NombreCompleto", "Nombre Completo", 4000),
            Col("FechaNacimiento", "Fecha Nacimiento", 2000, GridConstants.Align.Center, GridConstants.Formats.DateYMD),
            Col(GridConstants.Fields.Genero, GridConstants.Titles.Genero, 1000, GridConstants.Align.Center),
            Col(GridConstants.Fields.Nacionalidad, GridConstants.Titles.Nacionalidad, 2000),
                };

            // HVID / PVID (fields distintos)
            static IEnumerable<GridColumnDto> HvidBase()
                => new[]
                {
            Col("Cedula", "Cédula", 2000),
            Col("APELLIDO_1", "Apellido 1", 2000),
            Col("APELLIDO_2", "Apellido 2", 2000),
            Col("NOMBRE_1", "Nombre 1", 2000),
            Col("NOMBRE_2", "Nombre 2", 2000),

            Col("MONTO_ASEGURADO_01", GridConstants.Titles.MontoAsegurado, 2000, GridConstants.Align.Right, GridConstants.Formats.Numeric2),

            Col(GridConstants.Fields.Genero, GridConstants.Titles.Genero, 1000, GridConstants.Align.Center),
            Col(GridConstants.Fields.FechaNacimiento, GridConstants.Titles.FechaNacimiento, 2500, GridConstants.Align.Center, GridConstants.Formats.DateYMD),
            Col("EDAD", "Edad", 1000, GridConstants.Align.Center),

            Col("FECHA_EMISION", GridConstants.Titles.FechaEmision, 2000, GridConstants.Align.Center, GridConstants.Formats.DateYMD),

            Col("Movimiento", "Descripción", 2000, GridConstants.Align.Center),
            Col("MONEDA", "Moneda", 1500, GridConstants.Align.Center),

            Col("Id_Solicitud", "Referencia", 1000),

            Col("Email", GridConstants.Titles.Email, 2000),
            Col(GridConstants.Fields.Telefono, GridConstants.Titles.Telefono, 2000, GridConstants.Align.Center),

            Col("Provincia_Desc", "Provincia", 2000),
            Col("Canton_Desc", "Canton", 2000),
            Col("Distrito_Desc", "Distrito", 2000),

            Col("Nombre_Completo", GridConstants.Titles.NombreCompleto, 4000),

            Col("Id_Solicitud", "Pol.Operación", 2000, GridConstants.Align.Center),
            Col("cod_poliza", "Pol.Código", 1500, GridConstants.Align.Center),
            Col("Codigo", "Pol.Retención", 1500, GridConstants.Align.Center),

            Col("Credito_Operacion", GridConstants.Titles.CrdOperacion, 2000, GridConstants.Align.Center),
            Col("Credito_Codigo",    GridConstants.Titles.CrdCodigo,    1500, GridConstants.Align.Center),
            Col("Credito_Monto",     GridConstants.Titles.CrdMonto,     1500, GridConstants.Align.Right, GridConstants.Formats.Numeric2),
            Col("Credito_Saldo",     GridConstants.Titles.CrdSaldo,     1500, GridConstants.Align.Right, GridConstants.Formats.Numeric2),
            Col("Credito_Estado",    GridConstants.Titles.CrdEstado,    2000, GridConstants.Align.Center),
            Col("Vinculadas",        GridConstants.Titles.CrdOps,       2000, GridConstants.Align.Center),
                };

            // ============================================================
            // FACTORIES por tipo (cada uno sólo compone bloques)
            // ============================================================

            // Move Map declaration before its usage to fix CS0165
            Dictionary<string, Func<IEnumerable<GridColumnDto>>> Map = new(StringComparer.OrdinalIgnoreCase);

            Map["PPC"] = () =>
                PersonaCedula()
                    .Concat(Demografia(2500, incluyeEdad: true))
                    .Concat(Contacto())
                    .Concat(new[]
                    {
                Col("MONTO_ASEGURADO_01", GridConstants.Titles.MontoAsegurado, 2000, GridConstants.Align.Right, GridConstants.Formats.Numeric2),
                Col("FECHA_EMISION", GridConstants.Titles.FechaEmision, 2000, GridConstants.Align.Center, GridConstants.Formats.DateYMD),
                Col("MONEDA", "Moneda", 1500, GridConstants.Align.Center),
                Col(GridConstants.Fields.Movimiento, GridConstants.Titles.Movimiento, 2000, GridConstants.Align.Center),
                Col(GridConstants.Fields.NombreCompleto, GridConstants.Titles.NombreCompleto, 4000),
                    });

            Map["PCG"] = () =>
                PersonaIdentificacionBase()
                    .Concat(new[]
                    {
                Col(GridConstants.Fields.Movimiento, GridConstants.Titles.Movimiento, 2000, GridConstants.Align.Center)
                    });

            Map["PDE"] = () =>
                PersonaIdentificacionBase()
                    .Concat(new[]
                    {
                Col("MontoAsegurado", GridConstants.Titles.MontoAsegurado, 2000, GridConstants.Align.Right, GridConstants.Formats.Numeric2),
                Col("Movimiento", GridConstants.Titles.Movimiento, 2000, GridConstants.Align.Center)
                    });

            Map["PINC"] = () =>
                new[] { Col("Corte", "Corte", 2000) }
                    .Concat(PersonaCedula())
                    .Concat(Demografia(2000, incluyeEdad: false))
                    .Concat(new[]
                    {
                // OJO: aquí antes Telefono/Email estaban en orden contrario en algunos tipos.
                // Si quieres que el orden sea distinto, ajusta solo aquí.
                Col(GridConstants.Fields.Telefono, GridConstants.Titles.Telefono, 2000),
                Col(GridConstants.Fields.Email, GridConstants.Titles.Email, 2000),

                Col("Folio", "No. Folio", 2000),

                Col("PROVINCIA_DESC", "Provincia", 2000),
                Col("CANTON_DESC", "Cantón", 2000),
                Col("DISTRITO_DESC", "Distrito", 2000),
                Col("DIRECCION", "Dirección Completa", 3000),

                Col(GridConstants.Fields.CreditoMonto, "Monto del Crédito", 1800, GridConstants.Align.Right, GridConstants.Formats.Numeric2),
                Col("ValorConstruccion", "Monto de Construcción", 1800, GridConstants.Align.Right, GridConstants.Formats.Numeric2),
                Col(GridConstants.Fields.CreditoSaldo, "Saldo del Crédito", 1800, GridConstants.Align.Right, GridConstants.Formats.Numeric2),

                Col("ID_SOLICITUD", "No. Operación", 2000),

                Col("NumeroFinca", "No. Finca", 2000),
                Col("AreaFinca", "Area Finca", 2000),
                Col("NumPlanoCatastro", "Plano Castro", 2000),

                Col(GridConstants.Fields.Movimiento, "Tipo", 2000),

                Col("COD_POLIZA", "Cod.Póliza", 1100, GridConstants.Align.Center),
                Col("CODIGO", "Cod.Retención", 1100, GridConstants.Align.Center),
                    })
                    .Concat(CreditoCRD());

            Map["PINCC"] = () => Map["PINC"]();

            Map["HVID"] = () => HvidBase();
            Map["PVID"] = () => Map["HVID"]();

            // PREN y PVEH: misma composición, solo cambia Vehiculo(bool)
            Map["PREN"] = () =>
                new[]
                {
            Col(GridConstants.Fields.Cedula, GridConstants.Titles.Cedula, 2000),
            Col(GridConstants.Fields.NombreCompleto, "Asegurado", 4000),
                }
                .Concat(Demografia(2500, incluyeEdad: false))
                .Concat(new[]
                {
            Col("CREDITO_FECHA", GridConstants.Titles.FechaEmision, 2000, GridConstants.Align.Center, GridConstants.Formats.DateYMD),
                })
                .Concat(Contacto())
                .Concat(Vehiculo(incluyeMotorYChasis: false))
                .Concat(MontoYMovimiento())
                .Concat(PolizaBase())
                .Concat(CreditoCRD());

            Map["PVEH"] = () =>
                new[]
                {
            Col(GridConstants.Fields.Cedula, GridConstants.Titles.Cedula, 2000),
            Col(GridConstants.Fields.NombreCompleto, "Asegurado", 4000),
                }
                .Concat(Demografia(2500, incluyeEdad: false))
                .Concat(new[]
                {
            Col("CREDITO_FECHA", GridConstants.Titles.FechaEmision, 2000, GridConstants.Align.Center, GridConstants.Formats.DateYMD),
                })
                .Concat(Contacto())
                .Concat(Vehiculo(incluyeMotorYChasis: true))
                .Concat(MontoYMovimiento())
                .Concat(PolizaBase())
                .Concat(CreditoCRD());

            if (!Map.TryGetValue(tipo, out var factory))
                return new List<GridColumnDto> { Col("Info", "Tipo no soportado", 3000) };

            return DedupByField(factory());
        }

    }
}
