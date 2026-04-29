using Dapper;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using System.Data;

namespace Galileo.DataBaseTier
{
    public class FrmCajasFndaportacionesDB
    {
        private readonly PortalDB _portalDb;

        public FrmCajasFndaportacionesDB(IConfiguration config)
        {
            _portalDb = new PortalDB(config ?? throw new ArgumentNullException(nameof(config)));
        }

        /// <summary>
        /// Obtener los tipos de documentos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Documentos_Obtener(int codEmpresa, string codCaja)
        {
            const string sql = @"
                    SELECT 
                        RTRIM(C.tipo_documento) AS item,
                        RTRIM(D.Descripcion)    AS descripcion
                    FROM SIF_DOCUMENTOS D
                    INNER JOIN CAJAS_DOCUMENTOS C 
                        ON D.TIPO_DOCUMENTO = C.TIPO_DOCUMENTO
                    WHERE C.cod_caja = @cod_caja
                      AND D.Tipo_Movimiento IN ('A', 'D')
                    ORDER BY C.tipo_documento;";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                _portalDb,
                codEmpresa,
                sql,
                new { cod_caja = codCaja });
        }

        /// <summary>
        /// Aplicar el aporte a la subcuenta
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Fondos_Aporte_Aplicar(int codEmpresa, FondosAporteAplicarDto request)
        {
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                // ?? 0. Obtener cod_oficina seg�n usuario y caja
                string sqlOficina = @"
                            SELECT TOP 1 C.cod_oficina
                            FROM CAJAS_USUARIOS Cu
                            INNER JOIN cajas_definicion C ON Cu.cod_caja = C.cod_caja
                            WHERE Cu.usuario = @Usuario AND Cu.Cod_Caja = @Caja;
                        ";

                var codOficina = connection.QueryFirstOrDefault<string>(sqlOficina, new { Usuario = request.usuario, Caja = request.caja }, transaction) ?? string.Empty;

                string? sqlCuenta = @"
                            SELECT cuenta_conta, cuenta_rendimiento
                            FROM fnd_planes
                            WHERE cod_operadora = @Operadora
                              AND cod_plan = @Plan;
                        ";

                var cuentas = connection.QueryFirstOrDefault<(string? cuenta_conta, string? cuenta_rendimiento)>(
                    sqlCuenta,
                    new { Operadora = 1, Plan = request.plan },
                    transaction);

                var cuentaConta = cuentas.cuenta_conta ?? string.Empty;

                var vTipoDoc = string.IsNullOrWhiteSpace(request.tipodoc)
                    ? throw new InvalidOperationException("El tipo de documento es requerido.")
                    : request.tipodoc;
                long vNumDoc = FxDocumentoConsecutivo(codEmpresa, vTipoDoc, 2);

                // ?? 1. Generar consecutivo de documento
                string concepto = "FND001";
                string fechaProceso = DateTime.Now.ToString("yyyyMM");

                // ?? 2. Insertar en SIF_TRANSACCIONES 
                string sqlTransaccion = @"
                    INSERT INTO SIF_TRANSACCIONES
                    (COD_TRANSACCION, TIPO_DOCUMENTO, REGISTRO_FECHA, REGISTRO_USUARIO, 
                     Cliente_IDENTIFICACION, CLIENTE_NOMBRE, cod_concepto, monto, estado, 
                     Referencia_01, Referencia_02, cod_oficina,
                     linea1,linea2,linea3,linea4,linea5,linea6,linea7,linea8,
                     detalle, documento, cod_caja, cod_apertura, id_sesion)
                    VALUES
                    (@NumDoc, @TipoDoc, GETDATE(), @Usuario,
                     @Cedula, @ClienteNombre, @Concepto, @Monto, 'P',
                     @Plan, @Contrato, @Oficina,
                     '', '', '', '', '', '', '', '',
                     '', '', @Caja, @Apertura, @SesionId);
                ";

                var paramTrans = new DynamicParameters();
                paramTrans.Add("@NumDoc", vNumDoc);
                paramTrans.Add("@TipoDoc", vTipoDoc);
                paramTrans.Add("@Usuario", request.usuario);
                paramTrans.Add("@Cedula", request.cedula);
                paramTrans.Add("@ClienteNombre", request.nombre);
                paramTrans.Add("@Concepto", concepto);
                paramTrans.Add("@Monto", request.aporte);
                paramTrans.Add("@Plan", request.plan);
                paramTrans.Add("@Contrato", request.contrato);
                paramTrans.Add("@Oficina", codOficina);
                paramTrans.Add("@Caja", request.caja);
                paramTrans.Add("@Apertura", request.apertura);
                paramTrans.Add("@SesionId", request.sesionid);

                connection.Execute(sqlTransaccion, paramTrans, transaction);

                // ?? 3. Insertar detalle de contrato y actualizar aportes
                string sqlDetalle = @"
                        INSERT INTO fnd_contratos_detalle
                            (Cod_operadora, Cod_plan, Cod_Contrato, Fecha, Monto, Fecha_Proceso,
                             Tcon, Ncon, cod_concepto, usuario, cod_Caja)
                        VALUES
                            (@Operadora, @Plan, @Contrato, GETDATE(), @Monto, @FechaProceso,
                             @TipoDoc, @NumDoc, @Concepto, @Usuario, @Caja);

                        UPDATE fnd_contratos
                        SET Aportes = Aportes + @Monto
                        WHERE Cod_operadora = @Operadora 
                          AND Cod_plan = @Plan 
                          AND Cod_Contrato = @Contrato;";

                var paramDetalle = new DynamicParameters();
                paramDetalle.Add("@Operadora", 1);
                paramDetalle.Add("@Plan", request.plan);
                paramDetalle.Add("@Contrato", request.contrato);
                paramDetalle.Add("@Monto", request.aporte);
                paramDetalle.Add("@FechaProceso", fechaProceso);
                paramDetalle.Add("@TipoDoc", vTipoDoc);
                paramDetalle.Add("@NumDoc", vNumDoc);
                paramDetalle.Add("@Concepto", concepto);
                paramDetalle.Add("@Usuario", request.usuario);
                paramDetalle.Add("@Caja", request.caja);

                connection.Execute(sqlDetalle, paramDetalle, transaction);

                // ?? 4. Ejecutar asiento (spSIFDocsAsiento)
                connection.Execute(
                    "exec spSIFDocsAsiento @Tipo, @Transaccion, @Monto, 'C', @Divisa, @TipoCambio, @Contabilidad, @Unidad, @CentroCosto, @Cuenta, @Referencia1, @Referencia2, @Referencia3",
                    new
                    {
                        Tipo = vTipoDoc,
                        Transaccion = vNumDoc,
                        Monto = request.aporte,
                        Divisa = request.cod_divisa,
                        TipoCambio = 1,
                        Contabilidad = 1,
                        Unidad = codOficina,
                        CentroCosto = "",
                        Cuenta = cuentaConta,
                        Referencia1 = request.plan,
                        Referencia2 = request.contrato,
                        Referencia3 = ""
                    },
                    transaction
                );

                // ?? 5. Ejecutar spCajas_DesglocePagosDocFinal
                connection.Execute(
                    "exec spCajas_DesglocePagosDocFinal @Caja, @Apertura, @Tiquete, @Usuario, @TipoDoc, @NumDoc, @Unidad, @Plan, @Contrato",
                    new
                    {
                        Caja = request.caja,
                        Apertura = request.apertura,
                        Tiquete = request.tiquete,
                        Usuario = request.usuario,
                        TipoDoc = vTipoDoc,
                        NumDoc = vNumDoc,
                        Unidad = codOficina,
                        Plan = request.plan,
                        Contrato = request.contrato
                    },
                    transaction
                );

                transaction.Commit();
                response.Description = $"{vNumDoc}";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                response.Code = -1;
                response.Description = $"Error al aplicar aporte: {ex.Message}";
            }

            return response;
        }



        /// <summary>
        /// Verifica si el aporte requiere autorizaci�n
        /// </summary>
        /// <param name="codempresa"></param>
        /// <param name="plan"></param>
        /// <param name="usuario"></param>
        /// <param name="aporte"></param>
        /// <returns></returns>
        public ErrorDto<FondosRequiereAutorizacionDto> Fondos_Aporte_RequiereAutorizacion(int codempresa, string plan, string usuario, decimal aporte)
        {
            var response = DbHelper.CreateOkResponse<FondosRequiereAutorizacionDto>(default!);

            try
            {
                var data = DbHelper.WithConn(
                    _portalDb,
                    codempresa,
                    connection => connection.QueryFirstOrDefault<(int autorizado, decimal monto)>(
                        "spFnd_Autoriza_Datos",
                        new { Plan = plan, Usuario = usuario, Accion = "A" },
                        commandType: CommandType.StoredProcedure));

                if (data.Code != 0)
                {
                    response.Code = -1;
                    response.Description = $"error al validar autorización: {data.Description}";
                    response.Result = null;
                    return response;
                }

                var montoMaximo = data.Result.monto;
                response.Result = new FondosRequiereAutorizacionDto
                {
                    requiere = aporte > montoMaximo,
                    montomaximo = montoMaximo
                };

                response.Description = response.Result.requiere
                    ? "el aporte excede el monto permitido. requiere autorización"
                    : "el aporte está dentro del rango permitido. no requiere autorización";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"error al validar autorización: {ex.Message}";
                response.Result = null;
            }

            return response;
        }

        /// <summary>
        /// Verifica el estado de la gesti�n
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="gestionId"></param>
        /// <returns></returns>
        public ErrorDto<GestionEstadoDto> Fondos_Gestion_Estado(int codEmpresa, int gestionId)
        {
            var response = DbHelper.CreateOkResponse<GestionEstadoDto>(default!);

            try
            {
                var connectionString = _portalDb.ObtenerDbConnStringEmpresa(codEmpresa);
                var result = DbHelper.ExecuteStoredProcedureSingle<GestionEstadoDto>(
                    connectionString,
                    "spFnd_Gestion_Estado",
                    default,
                    new { GestionId = gestionId });

                response.Code = result.Code;
                response.Description = result.Code == 0 ? "Ok" : $"Error al consultar estado de gestión: {result.Description}";
                response.Result = result.Result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al consultar estado de gestión: {ex.Message}";
                response.Result = null;
            }

            return response;
        }

        /// <summary>
        ///  Registra la gesti�n
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<FondosGestionRegistroDto> fondos_gestion_registro(int CodEmpresa, FondosGestionRegistroAddDto request)
        {
            var response = DbHelper.CreateOkResponse<FondosGestionRegistroDto>(default!);

            try
            {
                var connectionString = _portalDb.ObtenerDbConnStringEmpresa(CodEmpresa);
                var result = DbHelper.ExecuteStoredProcedureSingle<FondosGestionRegistroDto>(
                    connectionString,
                    "spFnd_Gestion_Registro",
                    default,
                    new
                    {
                        cedula = request.cedula,
                        tipo = request.tipo,
                        operadora = request.operadora,
                        plan = request.plan,
                        contrato = request.contrato,
                        montoautorizado = request.montoautorizado,
                        aporte = request.aporte,
                        usuario = request.usuario
                    });

                if (result.Code != 0)
                {
                    response.Code = -1;
                    response.Description = $"error en registro de gestión: {result.Description}";
                    response.Result = null;
                    return response;
                }

                if (result.Result == null)
                {
                    response.Code = -1;
                    response.Description = "no se pudo registrar la gestión";
                    response.Result = null;
                    return response;
                }

                response.Result = result.Result;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"error en registro de gestión: {ex.Message}";
                response.Result = null;
            }

            return response;
        }


        /// <summary>
        /// Obtuebe las sub cuentas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operadora"></param>
        /// <param name="plan"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<FndSubCuentasDto>> SubCuentas_Obtener(int CodEmpresa, string operadora, string plan, int contrato)
        {
            const string sql = @"SELECT IDx,
                         Cedula,
                         Nombre,
                         0 AS ValorFijo
                  FROM fnd_subCuentas
                  WHERE cod_operadora = @Operadora
                    AND cod_plan = @Plan
                    AND cod_contrato = @Contrato
                    AND estado = 'A';";

            return DbHelper.ExecuteListQuery<FndSubCuentasDto>(
                _portalDb,
                CodEmpresa,
                sql,
                new
                {
                    Operadora = operadora,
                    Plan = plan,
                    Contrato = contrato
                });
        }


        public long FxDocumentoConsecutivo(int codEmpresa, string vTipo, int sysDocVersion)
        {
            using var connection = DbHelper.OpenConnection(_portalDb, codEmpresa);
            connection.Open();

            try
            {
                if (sysDocVersion == 1)
                {
                    var consecutivoSql = ObtenerSqlConsecutivo(vTipo);

                    long consecutivo = connection.QueryFirstOrDefault<long>(consecutivoSql.SelectSql);
                    connection.Execute(consecutivoSql.UpdateSql);

                    return consecutivo;
                }

                return connection.QueryFirstOrDefault<long>(
                    "exec spSIFDocsConsecutivo @Tipo",
                    new { Tipo = vTipo },
                    commandType: CommandType.Text);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error al obtener consecutivo para tipo {vTipo}: {ex.Message}", ex);
            }
        }

        private static (string SelectSql, string UpdateSql) ObtenerSqlConsecutivo(string vTipo)
        {
            return vTipo.ToUpperInvariant() switch
            {
                "RE" => (
                    "SELECT CS_RECIBO AS Consecutivo FROM ase_consecutivos",
                    "UPDATE ase_consecutivos SET CS_RECIBO = CS_RECIBO + 1"),
                "DP" => (
                    "SELECT CS_DEPOSITO AS Consecutivo FROM ase_consecutivos",
                    "UPDATE ase_consecutivos SET CS_DEPOSITO = CS_DEPOSITO + 1"),
                "ND" => (
                    "SELECT CS_NOTA_DEBITO AS Consecutivo FROM ase_consecutivos",
                    "UPDATE ase_consecutivos SET CS_NOTA_DEBITO = CS_NOTA_DEBITO + 1"),
                "NC" => (
                    "SELECT CS_NOTA_CREDITO AS Consecutivo FROM ase_consecutivos",
                    "UPDATE ase_consecutivos SET CS_NOTA_CREDITO = CS_NOTA_CREDITO + 1"),
                _ => throw new InvalidOperationException($"Tipo de documento {vTipo} no válido")
            };
        }


    }


}