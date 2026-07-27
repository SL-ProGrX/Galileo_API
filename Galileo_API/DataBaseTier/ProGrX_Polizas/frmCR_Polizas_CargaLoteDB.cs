using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Polizas;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Transactions;

namespace Galileo_API.DataBaseTier.ProGrX_Polizas
{
    public class FrmCrPolizasCargaLoteDB
    {
        private readonly PortalDB _portalDb;
        private readonly MProGrxMain mProGrxDll;

        public FrmCrPolizasCargaLoteDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
            mProGrxDll = new MProGrxMain(config); 
        }

        /// <summary>
        /// Llena combo de Cliente (VB6: cboCliente) desde CLIENTES.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CrdPolizasCargaLote_Cliente_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
                        SELECT
                            rtrim(codigo) as item,
                            rtrim(descripcion) + '  ['  + rtrim(codigo) + ']' as descripcion
                        FROM catalogo 
                        WHERE retencion = 'N' and activo = 1
                        and codigo not in(select codigo_ase from fnd_planes)
                        ORDER BY codigo";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Llena combo de Aseguradora (VB6: cboAseguradora) desde ASEGURADORAS.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CrdPolizasCargaLote_Aseguradora_Obtener(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"
                        SELECT
                            cod_Aseguradora AS item,
                            RTRIM(NOMBRE) AS descripcion
                        FROM CRD_POLIZAS_ASEGURADORAS where activo = 1 ";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Metodo para obtener bancos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CrdPolizasCargaLote_Banco_Obtener(int CodEmpresa, string usuario)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                const string query = @"EXEC spCrd_SGT_Bancos @Usuario";

                var param = new { Usuario = usuario };

                var result = conn.Query<dynamic>(query, param).ToList();
                //combierto lista a DropDownListaGenericaModel

                var lista = result.Select(x => new DropDownListaGenericaModel
                {
                    item = x.IdX,
                    descripcion = x.ItmX
                }).ToList();


                return lista;
            });
        }

        /// <summary>
        /// Llena combo de Cuenta (VB6: cboCuenta) según aseguradora y banco.
        /// Equivalente a cboBanco_Click en VB6.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CrdPolizasCargaLote_Cuenta_Obtener(
            int CodEmpresa,
            CrdPolizasCargaLoteCuentaCatalogoRequest request)
        {
            return DbHelper.WithConn(_portalDb, CodEmpresa, conn =>
            {
                // 1) Obtener CEDULA_Juridica de la aseguradora (VB6: mAseguradoraId)
                const string cedulaQuery = @"
                        SELECT CEDULA_Juridica
                        FROM CRD_POLIZAS_ASEGURADORAS
                        WHERE cod_Aseguradora = @CodAseguradora";

                var cedulaJuridica = conn.QueryFirstOrDefault<string>(
                    cedulaQuery,
                    new { request.CodAseguradora });

                if (string.IsNullOrWhiteSpace(cedulaJuridica))
                {
                    return new List<DropDownListaGenericaModel>();
                }

                // 2) Ejecutar SP para cuentas
                const string spQuery = @"EXEC spSys_Cuentas_Bancarias @Identificacion, @BancoId, @DivisaCheck";

                var rows = conn.Query<dynamic>(
                    spQuery,
                    new { Identificacion = cedulaJuridica, BancoId = request.IdBanco, DivisaCheck = 1 })
                    .ToList();

                // 3) Adaptar a modelo genérico del combo
                return rows
                    .Select(x => new DropDownListaGenericaModel
                    {
                        item = x.IdX,
                        descripcion = (x.ItmX ?? string.Empty).Trim()
                    })
                    .ToList();
            });
        }

        /// <summary>
        /// Helper C# para calcular siguiente periodo
        /// </summary>
        /// <param name="fechaYyyyMm"></param>
        /// <returns></returns>
        private static long FechaProcesoSiguiente(long fechaYyyyMm)
        {
            var anio = (int)(fechaYyyyMm / 100);
            var mes = (int)(fechaYyyyMm % 100);

            if (mes is < 1 or > 12)
            {
                return fechaYyyyMm; // defensivo (evita reventar por datos raros)
            }

            mes++;
            if (mes == 13)
            {
                mes = 1;
                anio++;
            }

            return (anio * 100L) + mes;
        }


        /// <summary>
        /// Llena combo de Periodo de Deducción (VB6: cboPrideduc).
        /// Usa GLOBALES.glngFechaCR y genera el periodo base + 6 siguientes.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CrdPolizasCargaLote_Prideduc_Obtener(int codEmpresa, string usuario, int codContabilidad)
        {
            // 1) Cargar parámetros globales (tu método existente)
            var globalesDto = mProGrxDll.sbSifParametrosInicializa(codEmpresa, usuario, codContabilidad);

            if (globalesDto.Result == null)
            {
                return DbHelper.CreateErrorResponse<List<DropDownListaGenericaModel>>(
                    globalesDto.Description ?? "No fue posible obtener parámetros iniciales.",
                    globalesDto.Code ?? -1,
                    new List<DropDownListaGenericaModel>());
            }

            var basePeriodo = globalesDto.Result.GlngFechaCR; // long yyyymm

            var lista = new List<DropDownListaGenericaModel>(capacity: 7);

            var periodo = basePeriodo;
            for (var i = 0; i < 7; i++)
            {
                lista.Add(new DropDownListaGenericaModel
                {
                    item = periodo,
                    descripcion = periodo.ToString()
                });

                periodo = FechaProcesoSiguiente((long)periodo);
            }

            return DbHelper.CreateOkResponse(lista);
        }

        public ErrorDto<CrdPolizasCargaLoteCargaResponse> CrdPolizasCargaLote_Cargar(
    int codEmpresa,
    string usuario,
    CrdPolizasCargaLoteCargaRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                if (request == null)
                {
                    return DbHelper.CreateErrorResponse(
                        "Request inválido. " + usuario,
                        -1,
                        new CrdPolizasCargaLoteCargaResponse());
                }

                if (string.IsNullOrWhiteSpace(request.CodigoCliente))
                {
                    return DbHelper.CreateErrorResponse(
                        "El cliente es requerido.",
                        -1,
                        new CrdPolizasCargaLoteCargaResponse());
                }

                if (string.IsNullOrWhiteSpace(request.CodAseguradora))
                {
                    return DbHelper.CreateErrorResponse(
                        "La aseguradora es requerida.",
                        -1,
                        new CrdPolizasCargaLoteCargaResponse());
                }

                if (request.Proceso <= 0)
                {
                    return DbHelper.CreateErrorResponse(
                        "El proceso es requerido.",
                        -1,
                        new CrdPolizasCargaLoteCargaResponse());
                }

                if (request.Items == null || request.Items.Count == 0)
                {
                    return DbHelper.CreateErrorResponse(
                        "No existen líneas para cargar.",
                        -1,
                        new CrdPolizasCargaLoteCargaResponse());
                }

                // 1) Delete previo (VB6)
                const string deleteSql = @"
            DELETE CRD_CREDITOS_CARGADO_H
            WHERE codigo = @Codigo
              AND PROCESO = @Proceso;";

                connection.Execute(deleteSql, new { Codigo = request.CodigoCliente, Proceso = request.Proceso });

                // 2) Bulk insert
                BulkInsertCreditosCargado(connection, request.CodigoCliente, request.CodAseguradora, request.Proceso, request.Items);

                // 3) Revisión (SP calcula cuota y demás)
                const string revisadoSp = @"EXEC spCrd_Creditos_Cargado_Revisado @ClienteId, @Aseguradora, @Proceso;";

                var revisado = connection.Query<CrdPolizasCargaLoteGridItem>(
                        revisadoSp,
                        new
                        {
                            ClienteId = request.CodigoCliente,
                            Aseguradora = request.CodAseguradora,
                            Proceso = request.Proceso
                        })
                    .ToList();

                // 4) Totales (como VB6)
                decimal totalMonto = 0m;
                decimal totalComision = 0m;

                foreach (var row in revisado)
                {
                    totalMonto += row.Monto;
                    totalComision += row.Comision;
                }

                var resp = new CrdPolizasCargaLoteCargaResponse
                {
                    Grid = revisado,
                    TotalMonto = totalMonto,
                    TotalComision = totalComision,
                    TotalNeto = totalMonto - totalComision,
                    LineasCargadas = request.Items.Count
                };

                return DbHelper.CreateOkResponse(resp);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse(
                    "Error al cargar la información del lote.",
                    -1,
                    new CrdPolizasCargaLoteCargaResponse());
            }
        }

        private static void BulkInsertCreditosCargado(
                IDbConnection connection,
                string codigoCliente,
                string codAseguradora,
                long proceso,
                List<CrdPolizasCargaLoteGridItem> items)
        {
            // Si no es SqlConnection, hacemos fallback con inserts.
            if (connection is not SqlConnection sqlConn)
            {
                FallbackInsertCreditosCargado(connection, codigoCliente, codAseguradora, proceso, items);
                return;
            }

            if (sqlConn.State != ConnectionState.Open)
            {
                sqlConn.Open();
            }

            using var table = new DataTable();
            table.Columns.Add("LINEA", typeof(int));
            table.Columns.Add("CODIGO", typeof(string));
            table.Columns.Add("COD_ASEGURADORA", typeof(string));
            table.Columns.Add("PROCESO", typeof(long));
            table.Columns.Add("CEDULA", typeof(string));
            table.Columns.Add("MONTO", typeof(decimal));
            table.Columns.Add("NOMBRE", typeof(string));
            table.Columns.Add("TIPO", typeof(string));
            table.Columns.Add("PLAZO", typeof(int));
            table.Columns.Add("TASA", typeof(decimal));
            table.Columns.Add("CUOTA", typeof(decimal));
            table.Columns.Add("COMISION", typeof(decimal));

            // VB6: LINEA incrementa por fila leída, pero ignora cédulas vacías.
            // Para evitar “huecos”, solo incrementamos cuando agregamos fila válida.
            var linea = 0;

            foreach (var it in items)
            {
                var cedula = (it.Cedula ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(cedula))
                {
                    continue;
                }

                linea++;

                var nombre = (it.Nombre ?? string.Empty).Trim();

                table.Rows.Add(
                    linea,
                    codigoCliente,
                    codAseguradora,
                    proceso,
                    cedula,
                    it.Monto,
                    nombre,
                    "D",
                    it.Plazo,
                    it.Tasa,
                    0m,          // VB6: cuota = 0 en carga
                    it.Comision);
            }

            using var bulk = new SqlBulkCopy(sqlConn, SqlBulkCopyOptions.Default, null)
            {
                DestinationTableName = "CRD_CREDITOS_CARGADO_H",
                BatchSize = 2000
            };

            bulk.ColumnMappings.Add("LINEA", "LINEA");
            bulk.ColumnMappings.Add("CODIGO", "CODIGO");
            bulk.ColumnMappings.Add("COD_ASEGURADORA", "COD_ASEGURADORA");
            bulk.ColumnMappings.Add("PROCESO", "PROCESO");
            bulk.ColumnMappings.Add("CEDULA", "CEDULA");
            bulk.ColumnMappings.Add("MONTO", "MONTO");
            bulk.ColumnMappings.Add("NOMBRE", "NOMBRE");
            bulk.ColumnMappings.Add("TIPO", "TIPO");
            bulk.ColumnMappings.Add("PLAZO", "PLAZO");
            bulk.ColumnMappings.Add("TASA", "TASA");
            bulk.ColumnMappings.Add("CUOTA", "CUOTA");
            bulk.ColumnMappings.Add("COMISION", "COMISION");

            bulk.WriteToServer(table);
        }

        private static void FallbackInsertCreditosCargado(
            IDbConnection connection,
            string codigoCliente,
            string codAseguradora,
            long proceso,
            List<CrdPolizasCargaLoteGridItem> items)
        {
            const string insertSql = @"
        INSERT CRD_CREDITOS_CARGADO_H
            (LINEA, CODIGO, cod_aseguradora, PROCESO, CEDULA, MONTO, NOMBRE, TIPO, PLAZO, TASA, CUOTA, COMISION)
        VALUES
            (@Linea, @Codigo, @CodAseguradora, @Proceso, @Cedula, @Monto, @Nombre, 'D', @Plazo, @Tasa, 0, @Comision);";

            var linea = 0;

            foreach (var it in items)
            {
                var cedula = (it.Cedula ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(cedula))
                {
                    continue;
                }

                linea++;

                connection.Execute(insertSql, new
                {
                    Linea = linea,
                    Codigo = codigoCliente,
                    CodAseguradora = codAseguradora,
                    Proceso = proceso,
                    Cedula = cedula,
                    Monto = it.Monto,
                    Nombre = (it.Nombre ?? string.Empty).Trim(),
                    Plazo = it.Plazo,
                    Tasa = it.Tasa,
                    Comision = it.Comision
                });
            }
        }

        public ErrorDto<long> CrdPolizasCargaLote_Procesar(
            int codEmpresa,
            string usuario,
            CrdPolizasCargaLoteProcesarRequest request)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    connection.Open();
                }

                if (!string.Equals(request.CodigoCliente?.Trim(), request.CodigoConfirma?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    return DbHelper.CreateErrorResponse<long>(
                        "La confirmación de la línea/cliente ha fallado, revise!");
                }

                if (request.MontoNeto <= 0m)
                {
                    return DbHelper.CreateErrorResponse<long>("El monto neto debe ser mayor a cero.");
                }

                // 1) Procesa lote
                const string procesaSp = @"
            EXEC spCrd_Creditos_Cargado_Procesa @Codigo, @Proceso, @Aseguradora, @Usuario;";

                connection.Execute(procesaSp, new
                {
                    Codigo = request.CodigoCliente,
                    Proceso = request.Proceso,
                    Aseguradora = request.CodAseguradora,
                    Usuario = usuario
                });

                // 2) Obtener CEDULA_Juridica (mAseguradoraId)
                const string cedulaQuery = @"
            SELECT CEDULA_Juridica
            FROM CRD_POLIZAS_ASEGURADORAS
            WHERE cod_Aseguradora = @CodAseguradora;";

                var cedulaJuridica = connection.QueryFirstOrDefault<string>(
                    cedulaQuery,
                    new { request.CodAseguradora });

                if (string.IsNullOrWhiteSpace(cedulaJuridica))
                {
                    return DbHelper.CreateErrorResponse<long>(
                        "No se encontró la cédula jurídica de la aseguradora.");
                }

                // 3) Insert tesorería maestro y obtener NSolicitud
                var tipo = request.TipoDocumentoUi;
                const string unidad = "OC";
                const string concepto = "CAR";

                var detalle1 = $"Ops:{request.Ops} Cp:{request.CodigoCliente}";
                var detalle2 = "Docs:";

                var nSolicitud = InsertTesTransaccionYObtenerNSolicitud(
                    new RegistrarDocumentoRequest
                    {
                        Conn = connection,
                        TipoDocumento = tipo,
                        IdBanco = request.IdBanco,
                        Monto = request.MontoNeto,
                        Codigo = cedulaJuridica.Trim(),
                        Beneficiario = request.AseguradoraNombre.Trim(),
                        CodigoCliente = request.CodigoCliente?.Trim() ?? string.Empty,
                        CtaAhorros = request.CuentaAhorros,
                        Detalle1 = detalle1,
                        Detalle2 = detalle2,
                        Fecha = DateTime.Now,
                        Unidad = unidad,
                        Concepto = concepto,
                        Usuario = usuario
                    }
                   );

                if (nSolicitud <= 0)
                {
                    return DbHelper.CreateErrorResponse<long>( "No fue posible generar la solicitud de tesorería.");
                }

                // 4) Asiento: H banco / D puente
                var ctaBanco = ObtenerCtaBanco(connection, request.IdBanco);
                if (string.IsNullOrWhiteSpace(ctaBanco))
                {
                    return DbHelper.CreateErrorResponse<long>("No se encontró la cuenta contable del banco.");
                }

                var ctaPuente = ObtenerCtaPuente(connection, request.CodigoCliente);
                if (string.IsNullOrWhiteSpace(ctaPuente))
                {
                    return DbHelper.CreateErrorResponse<long>("No se encontró la cuenta puente del cliente.");
                }

                InsertTesAsiento(connection, nSolicitud, ctaBanco, request.MontoNeto, "H", 1, unidad);
                InsertTesAsiento(connection, nSolicitud, ctaPuente, request.MontoNeto, "D", 2, unidad);

                return DbHelper.CreateOkResponse<long>(nSolicitud);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<long>("Error al procesar el lote y generar la solicitud en bancos.");
            }
        }

        private static long InsertTesTransaccionYObtenerNSolicitud(RegistrarDocumentoRequest request)
        {
            // Si NSolicitud NO es identity, esto debe ajustarse al método real de tu BD.
            const string insertSql = @"
        INSERT Tes_Transacciones
            (cod_concepto,cod_unidad,id_banco,tipo,tipo_Beneficiario,codigo,beneficiario,monto,fecha_solicitud,estado,estadoi,
             modulo,submodulo,cta_ahorros,detalle1,detalle2,referencia,op,genera,actualiza,user_solicita,autoriza,user_autoriza,fecha_autorizacion)
        VALUES
            (@Concepto,@Unidad,@IdBanco,@TipoDocumento,5,@Codigo,@Beneficiario,@Monto,@FechaSolicitud,'P','P',
             'Pol','C',@CtaAhorros,@Detalle1,@Detalle2,0,0,'S','S',@Usuario,
             @Autoriza,@UserAutoriza,@FechaAutoriza);

        SELECT CAST(SCOPE_IDENTITY() AS BIGINT) AS NSolicitud;";

            var autoriza = string.Equals(request.TipoDocumento, "CK", StringComparison.OrdinalIgnoreCase) ? "S" : "N";
            var userAutoriza = string.Equals(request.TipoDocumento, "CK", StringComparison.OrdinalIgnoreCase) ? request.Usuario : null;
            var fechaAutoriza = string.Equals(request.TipoDocumento, "CK", StringComparison.OrdinalIgnoreCase) ? (DateTime?)DateTime.Now : null;

            var nSolicitud = request.Conn.QueryFirstOrDefault<long>(insertSql, new
            {
                Concepto = request.Concepto,
                Unidad = request.Unidad,
                IdBanco = request.IdBanco,
                TipoDocumento = request.TipoDocumento,
                Codigo = request.Codigo,
                Beneficiario = request.Beneficiario,
                Monto = request.Monto,
                FechaSolicitud = request.Fecha,
                CtaAhorros = request.CtaAhorros,
                Detalle1 = request.Detalle1,
                Detalle2 = request.Detalle2,
                Usuario = request.Usuario,
                Autoriza = autoriza,
                UserAutoriza = userAutoriza,
                FechaAutoriza = fechaAutoriza
            });

            if (nSolicitud > 0)
            {
                return nSolicitud;
            }

            // Fallback VB6-like si SCOPE_IDENTITY no aplica: MAX por código.
            const string maxSql = @"SELECT MAX(nsolicitud) FROM Tes_Transacciones WHERE codigo = @Codigo;";
            return request.Conn.QueryFirstOrDefault<long>(maxSql, new { Codigo = request.CodigoCliente });
        }

        private static string ObtenerCtaBanco(IDbConnection conn, int idBanco)
        {
            const string sql = @"SELECT CTACONTA FROM Tes_Bancos WHERE id_banco = @IdBanco;";
            return conn.QueryFirstOrDefault<string>(sql, new { IdBanco = idBanco }) ?? string.Empty;
        }

        private static string ObtenerCtaPuente(IDbConnection conn, string codigoCliente)
        {
            const string sql = @"SELECT CtaPuente FROM Catalogo WHERE codigo = @Codigo;";
            return conn.QueryFirstOrDefault<string>(sql, new { Codigo = codigoCliente }) ?? string.Empty;
        }

        private static void InsertTesAsiento(
            IDbConnection conn,
            long nSolicitud,
            string cuentaContable,
            decimal monto,
            string debeHaber,
            int linea,
            string unidad)
        {
            const string sql = @"
        INSERT Tes_Trans_Asiento(nsolicitud,cuenta_contable,monto,debehaber,linea,cod_unidad)
        VALUES(@NSolicitud,@Cuenta,@Monto,@DH,@Linea,@Unidad);";

            conn.Execute(sql, new
            {
                NSolicitud = nSolicitud,
                Cuenta = cuentaContable.Trim(),
                Monto = monto,
                DH = debeHaber,
                Linea = linea,
                Unidad = unidad
            });
        }


        /// <summary>
        /// Llena combo de Cliente (VB6: cboCliente) desde CLIENTES.
        /// </summary>
        public ErrorDto CrdPolizasCargaLote_Obtener(int CodEmpresa, string cedula)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, CodEmpresa);

            const string query = @"SELECT COUNT('X') FROM SOCIOS S WHERE TRIM(S.CEDULA) = @cedula";

            int existe = conn.Query<int>(query, new { cedula = cedula.Trim() }).FirstOrDefault();

            if (existe == 0)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = "SOCIO NO ENCONTRADO"
                };
            }

            return new ErrorDto()
            {
                Code = 0,
                Description = "Ok"
            };
        }



    }
}
