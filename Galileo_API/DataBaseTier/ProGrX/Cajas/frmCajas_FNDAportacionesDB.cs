using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Cajas;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmCajas_FNDAportacionesDB
    {
        private readonly IConfiguration _config;
        private readonly mTesoreria _mtes;

        public frmCajas_FNDAportacionesDB(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Obtener los tipos de documentos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codCaja"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Cajas_Documentos_Obtener(int codEmpresa, string codCaja)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string sql = @"
                    SELECT 
                        RTRIM(C.tipo_documento) AS item,
                        RTRIM(D.Descripcion)    AS descripcion
                    FROM SIF_DOCUMENTOS D
                    INNER JOIN CAJAS_DOCUMENTOS C 
                        ON D.TIPO_DOCUMENTO = C.TIPO_DOCUMENTO
                    WHERE C.cod_caja = @cod_caja
                      AND D.Tipo_Movimiento IN ('A', 'D')
                    ORDER BY C.tipo_documento;";

                    var result = connection.Query<DropDownListaGenericaModel>(
                        sql,
                        new { cod_caja = codCaja },
                        commandType: CommandType.Text
                    ).AsList();

                    response.Result = result;
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Aplicar el aporte a la subcuenta
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto Fondos_Aporte_Aplicar(int codEmpresa, FondosAporteAplicarDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            using var connection = new SqlConnection(stringConn);
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

                string? codOficina = connection.QueryFirstOrDefault<string>(sqlOficina,new { Usuario = request.usuario, Caja = request.caja },transaction);

                string? sqlCuenta = @"
                            SELECT cuenta_conta, cuenta_rendimiento
                            FROM fnd_planes
                            WHERE cod_operadora = @Operadora
                              AND cod_plan = @Plan;
                        ";

                var cuentas = connection.QueryFirstOrDefault(sqlCuenta,new { Operadora = 1, Plan = request.plan },transaction);

                string? cuentaConta = cuentas.cuenta_conta;
                string? cuentaRendimiento = cuentas.cuenta_rendimiento;


                string vTipoDoc = request.tipodoc; 
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
                        Cuenta = cuentaConta, // validar
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
        public ErrorDto<fondosRequiereAutorizacionDTO> Fondos_Aporte_RequiereAutorizacion(int codempresa, string plan, string usuario, decimal aporte)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codempresa);
            var response = new ErrorDto<fondosRequiereAutorizacionDTO>
            {
                Code = 0,
                Description = "ok",
                Result = null
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var parametros = new DynamicParameters();
                parametros.Add("@Plan", plan);
                parametros.Add("@Usuario", usuario);

                var data = connection.QueryFirstOrDefault<(int autorizado, decimal monto)>(
                    "exec spFnd_Autoriza_Datos @Plan, 'A', @Usuario",
                    parametros
                );

                //if (data.autorizado == 0)
                //{
                //    response.Code = -1;
                //    response.Description = "el usuario no tiene nivel de autorizaci�n para este plan";
                //    return response;
                //}

                response.Result = new fondosRequiereAutorizacionDTO
                {
                    requiere = aporte > data.monto,
                    montomaximo = data.monto
                };

                response.Description = response.Result.requiere
                    ? "el aporte excede el monto permitido. requiere autorizaci�n"
                    : "el aporte est� dentro del rango permitido. no requiere autorizaci�n";
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"error al validar autorizaci�n: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        /// Verifica el estado de la gesti�n
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="gestionId"></param>
        /// <returns></returns>
        public ErrorDto<GestionEstadoDTO> Fondos_Gestion_Estado(int codEmpresa, int gestionId)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<GestionEstadoDTO>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var parametros = new DynamicParameters();
                parametros.Add("@GestionId", gestionId);

                var result = connection.QueryFirstOrDefault<GestionEstadoDTO>(
                    "exec spFnd_Gestion_Estado @GestionId",
                    parametros,
                    commandType: CommandType.Text
                );
                response.Result = result;


            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al consultar estado de gesti�n: {ex.Message}";
            }

            return response;
        }

        /// <summary>
        ///  Registra la gesti�n
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<fondosGestionRegistroDTO> fondos_gestion_registro(int CodEmpresa, fondosGestionRegistroAddDTO request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<fondosGestionRegistroDTO>
            {
                Code = 0,
                Description = "ok",
                Result = null
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var parametros = new DynamicParameters();
                parametros.Add("@cedula", request.cedula);
                parametros.Add("@tipo", request.tipo);
                parametros.Add("@operadora", request.operadora);
                parametros.Add("@plan", request.plan);
                parametros.Add("@contrato", request.contrato);
                parametros.Add("@montoautorizado", request.montoautorizado);
                parametros.Add("@aporte", request.aporte);
                parametros.Add("@usuario", request.usuario);

                var result = connection.QueryFirstOrDefault<fondosGestionRegistroDTO>(
                    "exec spFnd_Gestion_Registro @cedula, @tipo, @operadora, @plan, @contrato, @montoautorizado, @aporte, @usuario",
                    parametros,
                    commandType: CommandType.Text
                );

                if (result == null)
                {
                    response.Code = -1;
                    response.Description = "no se pudo registrar la gesti�n";
                }
                else
                {
                    response.Result = result;
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"error en registro de gesti�n: {ex.Message}";
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
        public ErrorDto<List<FndSubCuentasDTO>> SubCuentas_Obtener(int CodEmpresa, string operadora, string plan, int contrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<FndSubCuentasDTO>>
            {
                Code = 0,
                Description = "ok",
                Result = null
            };

            try
            {
                using var connection = new SqlConnection(stringConn);

                var parametros = new DynamicParameters();
                parametros.Add("@Operadora", operadora);
                parametros.Add("@Plan", plan);
                parametros.Add("@Contrato", contrato);

                response.Result = connection.Query<FndSubCuentasDTO>(
                    @"SELECT IDx,
                         Cedula,
                         Nombre,
                         0 AS ValorFijo
                  FROM fnd_subCuentas
                  WHERE cod_operadora = @Operadora
                    AND cod_plan = @Plan
                    AND cod_contrato = @Contrato
                    AND estado = 'A';",
                    parametros,
                    commandType: CommandType.Text
                ).ToList();

            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = $"Error al obtener subcuentas: {ex.Message}";
            }

            return response;
        }


        public long FxDocumentoConsecutivo(int codEmpresa, string vTipo, int sysDocVersion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);

            using var connection = new SqlConnection(stringConn);
            connection.Open();

            try
            {
                if (sysDocVersion == 1)
                {
                    // ?? Determinar el campo en ase_consecutivos
                    string strCampo = vTipo.ToUpper() switch
                    {
                        "RE" => "CS_RECIBO",
                        "DP" => "CS_DEPOSITO",
                        "ND" => "CS_NOTA_DEBITO",
                        "NC" => "CS_NOTA_CREDITO",
                        _ => throw new Exception($"Tipo de documento {vTipo} no v�lido")
                    };

                    // ?? Leer el consecutivo actual
                    var sqlSelect = $"SELECT {strCampo} AS Consecutivo FROM ase_consecutivos";
                    long consecutivo = connection.QueryFirstOrDefault<long>(sqlSelect);

                    // ?? Actualizar +1
                    var sqlUpdate = $"UPDATE ase_consecutivos SET {strCampo} = {strCampo} + 1";
                    connection.Execute(sqlUpdate);

                    return consecutivo;
                }
                else
                {
                    // ?? Control de documentos versi�n 2 (SP)
                    var consecutivo = connection.QueryFirstOrDefault<long>(
                        "exec spSIFDocsConsecutivo @Tipo",
                        new { Tipo = vTipo },
                        commandType: CommandType.Text
                    );

                    return consecutivo;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener consecutivo para tipo {vTipo}: {ex.Message}", ex);
            }
        }


    }


}