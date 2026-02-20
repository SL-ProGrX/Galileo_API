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

        private static List<GridColumnDto> BuildColumnsByTipo(string tipo)
            {
                // Helper local para no repetir
                static GridColumnDto Col(string field, string title, int width, string align = "left", string? format = null)
                    => new() { field = field, title = title, width = width, align = align, format = format };

                return tipo switch
                {
                    // VB6: Case "PPC"
                    "PPC" => new List<GridColumnDto>
                {
                    Col("CEDULA", "Cédula", 2000),
                    Col("APELLIDO_1", "Apellido 1", 2000),
                    Col("APELLIDO_2", "Apellido 2", 2000),
                    Col("NOMBRE_1", "Nombre 1", 2000),
                    Col("NOMBRE_2", "Nombre 2", 2000),

                    Col("GENERO", "Genero", 1000, "center"),
                    Col("FECHA_NACIMIENTO", "Fecha Nac.", 2500, "center", "date:yyyy-MM-dd"),
                    Col("EDAD", "Edad", 1000, "center"),

                    Col("EMAIL", "Correo Electrónico", 2000),
                    Col("TELEFONO", "Teléfono", 2000, "center"),

                    Col("MONTO_ASEGURADO_01", "Monto Asegurado", 2000, "right", "n2"),
                    Col("FECHA_EMISION", "Fecha Emisión", 2000, "center", "date:yyyy-MM-dd"),
                    Col("MONEDA", "Moneda", 1500, "center"),
                    Col("MOVIMIENTO", "Movimiento", 2000, "center"),

                    Col("NOMBRE_COMPLETO", "Nombre Completo", 4000)
                },

                    // VB6: Case "PCG"
                    "PCG" => new List<GridColumnDto>
                {
                    Col("Identificacion", "Identificación", 2000),
                    Col("NombreCompleto", "Nombre Completo", 4000),
                    Col("FechaNacimiento", "Fecha Nacimiento", 2000, "center", "date:yyyy-MM-dd"),
                    Col("Genero", "Genero", 1000, "center"),
                    Col("Nacionalidad", "Nacionalidad", 2000),
                    Col("MOVIMIENTO", "Movimiento", 2000, "center")
                },

                    // VB6: Case "PDE"
                    "PDE" => new List<GridColumnDto>
                {
                    Col("Identificacion", "Identificación", 2000),
                    Col("NombreCompleto", "Nombre Completo", 4000),
                    Col("MontoAsegurado", "Monto Asegurado", 2000, "right", "n2"),
                    Col("FechaNacimiento", "Fecha Nacimiento", 2000, "center", "date:yyyy-MM-dd"),
                    Col("Genero", "Genero", 1000, "center"),
                    Col("Nacionalidad", "Nacionalidad", 2000),
                    Col("Movimiento", "Movimiento", 2000, "center")
                },

                    // VB6: Case "PINC", "PINCC"
                    "PINC" or "PINCC" => new List<GridColumnDto>
                {
                    Col("Corte", "Corte", 2000),

                    Col("CEDULA", "Identificación", 2000),
                    Col("APELLIDO_1", "Apellido 1", 2000),
                    Col("APELLIDO_2", "Apellido 2", 2000),
                    Col("NOMBRE_1", "Nombre 1", 2000),
                    Col("NOMBRE_2", "Nombre 2", 2000),

                    Col("GENERO", "Genero", 1000, "center"),
                    Col("FECHA_NACIMIENTO", "Fecha Nac.", 2000, "center", "date:yyyy-MM-dd"),

                    Col("TELEFONO", "Teléfono", 2000),
                    Col("EMAIL", "Correo Electrónico", 2000),

                    Col("Folio", "No. Folio", 2000),

                    Col("PROVINCIA_DESC", "Provincia", 2000),
                    Col("CANTON_DESC", "Cantón", 2000),
                    Col("DISTRITO_DESC", "Distrito", 2000),
                    Col("DIRECCION", "Dirección Completa", 3000),

                    Col("CREDITO_MONTO", "Monto del Crédito", 1800, "right", "n2"),
                    Col("ValorConstruccion", "Monto de Construcción", 1800, "right", "n2"),
                    Col("CREDITO_SALDO", "Saldo del Crédito", 1800, "right", "n2"),

                    Col("ID_SOLICITUD", "No. Operación", 2000),

                    Col("NumeroFinca", "No. Finca", 2000),
                    Col("AreaFinca", "Area Finca", 2000),
                    Col("NumPlanoCatastro", "Plano Castro", 2000),

                    Col("MOVIMIENTO", "Tipo", 2000),

                    Col("COD_POLIZA", "Cod.Póliza", 1100, "center"),
                    Col("CODIGO", "Cod.Retención", 1100, "center"),

                    Col("CREDITO_OPERACION", "CRD.Operación", 2000, "center"),
                    Col("CREDITO_CODIGO", "CRD.Código", 1500, "center"),
                    Col("CREDITO_MONTO", "CRD.Monto", 1500, "right", "n2"),
                    Col("CREDITO_SALDO", "CRD.Saldo", 1500, "right", "n2"),
                    Col("CREDITO_ESTADO", "CRD.Estado", 2000, "center"),
                    Col("VINCULADAS", "CRD.Ops", 2000, "center")
                },

                    // VB6: Case "HVID" (en tu VB6 la grilla “simple” era genérica)
                    "HVID" or "PVID" => new List<GridColumnDto>
                {
                        Col("Cedula", "Cédula", 2000),
                        Col("Apellido_1", "Apellido 1", 2000),
                        Col("Apellido_2", "Apellido 2", 2000),
                        Col("Nombre_1", "Nombre 1", 2000),
                        Col("Nombre_2", "Nombre 2", 2000),

                        Col("MONTO_ASEGURADO_01", "Monto Asegurado", 2000, "right", "n2"),

                        Col("Genero", "Genero", 1000, "center"),
                        Col("FECHA_NACIMIENTO", "Fecha Nac.", 2500, "center", "date:yyyy-MM-dd"),
                        Col("EDAD", "Edad", 1000, "center"),

                        Col("FECHA_EMISION", "Fecha Emisión", 2000, "center", "date:yyyy-MM-dd"),
                        Col("MONTO_ASEGURADO_01", "Suma Asegurada", 2000, "right", "n2"),

                        Col("Movimiento", "Descripción", 2000, "center"),
                        Col("MONEDA", "Moneda", 1500, "center"),

                        Col("Id_Solicitud", "Referencia", 1000),

                        Col("Email", "Correo Electrónico", 2000),
                        Col("TELEFONO", "Teléfono", 2000, "center"),

                        Col("Provincia_Desc", "Provincia", 2000),
                        Col("Canton_Desc", "Canton", 2000),
                        Col("Distrito_Desc", "Distrito", 2000),

                        Col("Nombre_Completo", "Nombre Completo", 4000),

                        Col("Id_Solicitud", "Pol.Operación", 2000, "center"),
                        Col("cod_poliza", "Pol.Código", 1500, "center"),
                        Col("Codigo", "Pol.Retención", 1500, "center"),

                        Col("Credito_Operacion", "CRD.Operación", 2000, "center"),
                        Col("Credito_Codigo", "CRD.Código", 1500, "center"),
                        Col("Credito_Monto", "CRD.Monto", 1500, "right", "n2"),
                        Col("Credito_Saldo", "CRD.Saldo", 1500, "right", "n2"),
                        Col("Credito_Estado", "CRD.Estado", 2000, "center"),
                        Col("Vinculadas", "CRD.Ops", 2000, "center")
                },

                    // VB6: Case "PREN"
                    "PREN" => new List<GridColumnDto>
                {
                    Col("CEDULA", "Cédula", 2000),
                    Col("NOMBRE_COMPLETO", "Asegurado", 4000),

                    Col("GENERO", "Genero", 1000, "center"),
                    Col("FECHA_NACIMIENTO", "Fecha Nac.", 2500, "center", "date:yyyy-MM-dd"),

                    Col("CREDITO_FECHA", "Fecha Emisión", 2000, "center", "date:yyyy-MM-dd"),
                    Col("EMAIL", "Correo Electrónico", 2000),
                    Col("TELEFONO", "Teléfono", 2000, "center"),

                    Col("USO", "Uso", 2000),
                    Col("PLACA", "Placa", 2000),
                    Col("ID_PROVISIONAL", "Id Provisional", 2000),
                    Col("MARCA_DESC", "Marca", 2000),
                    Col("MODELO_DESC", "Modelo", 2000),
                    Col("PRESENTACION_DESC", "Presentación", 2000),
                    Col("ANIO", "Año Vehículo", 1000, "center"),
                    Col("COLOR", "Color", 2000),

                    Col("MONTO", "Monto Asegurado", 2000, "right", "n2"),
                    Col("MOVIMIENTO", "Movimiento", 2000),

                    Col("ID_SOLICITUD", "Pol.Operación", 1100, "center"),
                    Col("COD_POLIZA", "Pol.Código", 1100, "center"),
                    Col("CODIGO", "Pol.Retención", 1100, "center"),

                    Col("CREDITO_OPERACION", "CRD.Operación", 2000, "center"),
                    Col("CREDITO_CODIGO", "CRD.Código", 1500, "center"),
                    Col("CREDITO_MONTO", "CRD.Monto", 1500, "right", "n2"),
                    Col("CREDITO_SALDO", "CRD.Saldo", 1500, "right", "n2"),
                    Col("CREDITO_ESTADO", "CRD.Estado", 2000, "center"),
                    Col("VINCULADAS", "CRD.Ops", 2000, "center")
                },

                    // VB6: Case "PVEH"
                    "PVEH" => new List<GridColumnDto>
                {
                    Col("CEDULA", "Cédula", 2000),
                    Col("NOMBRE_COMPLETO", "Asegurado", 4000),

                    Col("GENERO", "Genero", 1000, "center"),
                    Col("FECHA_NACIMIENTO", "Fecha Nac.", 2500, "center", "date:yyyy-MM-dd"),

                    Col("CREDITO_FECHA", "Fecha Emisión", 2000, "center", "date:yyyy-MM-dd"),
                    Col("EMAIL", "Correo Electrónico", 2000),
                    Col("TELEFONO", "Teléfono", 2000, "center"),

                    Col("USO", "Uso del Vehículo", 2000),
                    Col("PLACA", "Placa", 2000),
                    Col("ID_PROVISIONAL", "Id Provisional", 2000),
                    Col("MARCA_DESC", "Marca", 2000),
                    Col("MODELO_DESC", "Modelo", 2000),
                    Col("PRESENTACION_DESC", "Presentación", 2000),
                    Col("ANIO", "Año Vehículo", 1000, "center"),
                    Col("VIN_MOTOR", "Motor CC", 2000),
                    Col("CHASIS_NUMERO", "Chasis", 3000),
                    Col("COLOR", "Color", 2000),

                    Col("MONTO", "Monto Asegurado", 2000, "right", "n2"),
                    Col("MOVIMIENTO", "Movimiento", 2000),

                    Col("COD_POLIZA", "Pol.Código", 1100, "center"),
                    Col("CODIGO", "Pol.Retención", 1100, "center"),
                    Col("ID_SOLICITUD", "Pol.Operación", 1100, "center"),

                    Col("CREDITO_OPERACION", "CRD.Operación", 2000, "center"),
                    Col("CREDITO_CODIGO", "CRD.Código", 1500, "center"),
                    Col("CREDITO_MONTO", "CRD.Monto", 1500, "right", "n2"),
                    Col("CREDITO_SALDO", "CRD.Saldo", 1500, "right", "n2"),
                    Col("CREDITO_ESTADO", "CRD.Estado", 2000, "center"),
                    Col("VINCULADAS", "CRD.Ops", 2000, "center")
                },
                    _ => new List<GridColumnDto>
                {
                    Col("Info", "Tipo no soportado", 3000)
                }
                };
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
                new { cod_poliza = codPoliza });

             if (tipoRow == null)
                return DbHelper.CreateErrorResponse<CrdPolizaConsultaResponseDto>("No se pudo determinar el tipo.");

            string tipo = tipoRow.Trim().ToUpper();

            // 2️⃣ Determinar SP según tipo + análisis
            string sql = BuildSqlByTipo(tipo, req.analisis);

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
                rows = rows,
                total = rows.Count
            };

            return response;
        }


        private static string BuildSqlByTipo(string tipo, string analisis)
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

                _ => throw new Exception($"Tipo de póliza no soportado: {tipo}")
            };
        }

    }
}
