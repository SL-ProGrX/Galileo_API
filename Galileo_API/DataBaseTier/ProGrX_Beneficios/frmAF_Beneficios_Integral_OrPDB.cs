using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_Beneficios_Integral_OrPDB
    {
        private readonly IConfiguration _config;
        private readonly mBeneficiosDB _mBeneficiosDB;
        private readonly mProGrX_AuxiliarDB _AuxiliarDB;

        public frmAF_Beneficios_Integral_OrPDB(IConfiguration config)
        {
            _config = config;
            _mBeneficiosDB = new mBeneficiosDB(config);
            _AuxiliarDB = new mProGrX_AuxiliarDB(config);
        }

        /// <summary>
        /// Metodo para obtener los tipos de identificacion
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> TiposIdentificacion_Obtener(int CodCliente)
        {
            return _AuxiliarDB.TiposIdentificacion_Obtener(CodCliente);
        }

        /// <summary>
        /// Metodo para obtener la lista de divisas
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<AfBeneficioIntegralDropsLista>> DivisasLista_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfBeneficioIntegralDropsLista>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "select COD_DIVISA AS 'item', DESCRIPCION  From vSys_Divisas " +
                        " Where DIVISA_LOCAL = 1 ";
                    response.Result = connection.Query<AfBeneficioIntegralDropsLista>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Metodo para obtener la lista de bancos
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<AfBeneficioIntegralGenericLista>> BancosLista_Obtener(int CodCliente, string Usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfBeneficioIntegralGenericLista>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"exec spCrd_W_SGT_Bancos '{Usuario}'";
                    //var query = $@"exec spCrd_SGT_Bancos '{Usuario}'";
                    response.Result = connection.Query<AfBeneficioIntegralGenericLista>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Metodo para obtener la lista de cuentas bancarias
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="Cedula"></param>
        /// <param name="CodBanco"></param>
        /// <returns></returns>
        public ErrorDto<List<AfBeneIntegralCuentasLista>> CuentasBancariasLista_Obtener(int CodCliente, string? Cedula, int CodBanco)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfBeneIntegralCuentasLista>>();

            if(Cedula == null)
            {
                return response;
            }

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"exec spSys_W_Cuentas_Bancarias '{Cedula.Replace("-", "")}', {CodBanco}";
                    response.Result = connection.Query<AfBeneIntegralCuentasLista>(query).ToList();

                    if (response.Result.Count == 0)
                    {
                        query = $@"exec spSys_W_Cuentas_Bancarias '{Cedula}', {CodBanco}";
                        response.Result = connection.Query<AfBeneIntegralCuentasLista>(query).ToList();
                    }

                    //Eliminar cuentas duplicadas
                    response.Result = response.Result.GroupBy(x => x.itmx).Select(x => x.FirstOrDefault()).ToList();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Metodo para obtener la lista de productos
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <returns></returns>
        public ErrorDto<List<AfiBeneProductos>> ProductosLista_Obtener(int CodCliente)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfiBeneProductos>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"SELECT * FROM AFI_BENE_PRODUCTOS";
                    response.Result = connection.Query<AfiBeneProductos>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Metodo para obtener la lista de beneficios por socio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="Filtros"></param>
        /// <returns></returns>
        public ErrorDto<AfiBeneOtorgaData> AfiBeneOtorga_CedulaSocio_Obtener(int CodCliente, string Filtros)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            AfiBeneOtorgaFiltros filtro = JsonConvert.DeserializeObject<AfiBeneOtorgaFiltros>(Filtros);
            var response = new ErrorDto<AfiBeneOtorgaData>();
            try
            {

                string where = $@" WHERE O.CEDULA = '{filtro.cedula}'";
                if (filtro.categoria != null)
                {
                    where += $" AND B.COD_CATEGORIA = '{filtro.categoria}'";
                }
                if (filtro.consec != null)
                {
                    where += $" AND O.CONSEC = {filtro.consec}";
                }
                if (filtro.cod_beneficio != null)
                {
                    where += $" AND O.COD_BENEFICIO = '{filtro.cod_beneficio}'";
                }
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"Select O.*,B.Descripcion, B.PAGOS_MULTIPLES   
                        from afi_bene_otorga O inner join afi_beneficios B 
                        on O.cod_beneficio = B.cod_beneficio 
                        {where}";
                    response.Result = connection.Query<AfiBeneOtorgaData>(query).FirstOrDefault();
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
        /// Metodo para obtener la lista de pagos de beneficios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="Cedula"></param>
        /// <param name="Cod_Beneficio"></param>
        /// <param name="Consec"></param>
        /// <returns></returns>
        public ErrorDto<List<Afi_Bene_Integral_OrP>> AfiBeneficioPagosTabla_Obtener(int CodCliente, string Cedula, string Cod_Beneficio, int Consec)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<Afi_Bene_Integral_OrP>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"Select * from afi_bene_pago 
                        where cedula = '{Cedula}' and cod_beneficio = '{Cod_Beneficio}' and consec = {Consec}";
                    response.Result = connection.Query<Afi_Bene_Integral_OrP>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }
            return response;
        }

        /// <summary>
        /// Metodo para validar si existe un pago de beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="Cedula"></param>
        /// <param name="Cod_Beneficio"></param>
        /// <param name="Consec"></param>
        /// <returns></returns>
        public ErrorDto AfiBeneficioPagos_ValidaExiste(int CodCliente, string Cedula, string Cod_Beneficio, int Consec)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto resp = new ErrorDto();
            resp.Code = 0;

            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var query = $@"IF EXISTS (select 1 from afi_bene_pago where  cod_beneficio = '{Cod_Beneficio}' and cedula = '{Cedula}' and CONSEC = {Consec})
                        SELECT 1 AS existe;
                    ELSE
                        SELECT 0 AS existe;";
                    resp.Code = connection.Query<int>(query).FirstOrDefault();
                    if (resp.Code == 1)
                    {
                        query = $@"select COUNT(*) from afi_bene_pago where cod_beneficio = '{Cod_Beneficio}' and cedula = '{Cedula}' and CONSEC = {Consec} AND estado != 'S'";
                        int estadoValido = connection.Query<int>(query).FirstOrDefault();
                        if (estadoValido == 0)
                        {
                            resp.Description = "Ya existe una orden de pago para este expediente";
                        }
                        else
                        {
                            resp.Code = 2;
                            resp.Description = "La orden de pago para este expediente ya fue procesada";
                        }

                    }
                    else
                    {
                        resp.Description = "Ok";
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Code = -1;
                resp.Description = ex.Message;
            }
            return resp;
        }

        /// <summary>
        /// Metodo para insertar una orden de pago
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="beneficio"></param>
        /// <returns></returns>
        public ErrorDto AfiBeneficioIntegralOrdenPago_Agregar(int CodCliente, Afi_Bene_Integral_OrP beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            var response = new ErrorDto<BeneficioGeneralDatos>();

            response.Code = 0;
            //segundo filtro de validaciones
            response = _mBeneficiosDB.ValidaEstadoSocio(CodCliente, beneficio.cedula.Trim());
            if (response.Code == -1)
            {
                info.Code = response.Code;
                info.Description = response.Description;
                return info;
            }

            BeneficioGeneralDatos beneficioValida = new BeneficioGeneralDatos();
            beneficioValida.cedula = beneficio.cedula.Trim();
            beneficioValida.cod_beneficio.item = beneficio.cod_beneficio;
            beneficioValida.monto_aplicado =  beneficio.monto;
            beneficioValida.registra_user = beneficio.registro_usuario;
            beneficioValida.estado = new AfBeneficioIntegralDropsLista();
            beneficioValida.estado.item = beneficio.estado;
            beneficioValida.consec = beneficio.consec;
            beneficioValida.id_beneficio = (int)beneficio.id_beneficio;
           

            //tercer filtro de validaciones
            var errorBene = _mBeneficiosDB.ValidarBeneficioPagoDato(CodCliente, beneficioValida);
            if (errorBene.Code == -1)
            {
                info.Code = errorBene.Code;
                info.Description = errorBene.Description;
                return info;
            }

            

            try
            {
                int codBanco = beneficio.cod_banco ?? 0;
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "";
                    //busco valor justifica para el beneficio
                    query = $@"select requiere_justificacion from afi_bene_otorga where id_beneficio = '{beneficio.id_beneficio}'";
                    var justifica = connection.Query<bool>(query).FirstOrDefault();

                    var ErrJustifica = _mBeneficiosDB.ValidarBeneficioPagoJustificaDato(CodCliente, beneficioValida, justifica);
                    if (ErrJustifica.Code == -1)
                    {
                        info.Code = ErrJustifica.Code;
                        info.Description = ErrJustifica.Description;
                        return info;
                    }

                    //Insertar en pagos
                    query = $@"insert afi_bene_pago(
                                            cedula,
                                            consec,
                                            cod_beneficio,
                                            tipo,
                                            monto,
                                            cod_banco, 
                                            tipo_emision,
                                            cta_bancaria,
                                            estado,
                                            t_identificacion,
                                            t_beneficiario, 
                                            t_email,
                                            registro_fecha,
                                            registro_usuario,
                                            cod_producto
                    )values(
                                            '{beneficio.cedula.Trim()}',
                                            {beneficio.consec},
                                            '{beneficio.cod_beneficio}',
                                            '{beneficio.tipo}',
                                            {beneficio.monto},
                                            {codBanco},
                                            '{beneficio.tipo_emision}',
                                            '{beneficio.cta_bancaria}',
                                            'S',
                                            '{beneficio.t_identificacion}',
                                            '{beneficio.t_beneficiario}',
                                            '{beneficio.t_email}',
                                            Getdate(),
                                            '{beneficio.registro_usuario}',
                                            '{beneficio.cod_producto}'
                    )
                    ";

                    info.Code = connection.Execute(query);

                    if (info.Code > 0)
                    {
                        //Inserto Bitacora
                        query = $@"SELECT TOP 1 ID_PAGO FROM afi_bene_pago 
                                WHERE cedula = '{beneficio.cedula}' AND consec = {beneficio.consec} AND COD_BENEFICIO = '{beneficio.cod_beneficio}' 
                                ORDER BY ID_PAGO DESC";
                        var idPago = connection.Query<int>(query).FirstOrDefault();

                        _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                        {
                            EmpresaId = CodCliente,
                            cod_beneficio = beneficio.cod_beneficio,
                            consec = beneficio.consec,
                            movimiento = "Inserta",
                            detalle = $"Ingresa Orden de Pago COD: [" + idPago + "]",
                            registro_usuario = beneficio.registro_usuario.ToUpper(),
                        });

                        info.Code = 0;
                        info.Description = "Orden de pago cargada exitosamente";
                    }
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;
        }

        /// <summary>
        /// Metodo para actualizar una orden de pago
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="beneficio"></param>
        /// <returns></returns>
        public ErrorDto AfiBeneficioIntegralOrdenPago_Actualizar(int CodCliente, Afi_Bene_Integral_OrP beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                var response = new ErrorDto<BeneficioGeneralDatos>();

                response.Code = 0;
                //segundo filtro de validaciones
                response = _mBeneficiosDB.ValidaEstadoSocio(CodCliente, beneficio.cedula.Trim());
                if (response.Code == -1)
                {
                    info.Code = response.Code;
                    info.Description = response.Description;
                    return info;
                }

                BeneficioGeneralDatos beneficioValida = new BeneficioGeneralDatos();
                beneficioValida.cedula = beneficio.cedula.Trim();
                beneficioValida.cod_beneficio.item = beneficio.cod_beneficio;
                beneficioValida.monto_aplicado = beneficio.monto;
                beneficioValida.registra_user = beneficio.registro_usuario;
                beneficioValida.estado = new AfBeneficioIntegralDropsLista();
                beneficioValida.estado.item = beneficio.estado;
                beneficioValida.consec = beneficio.consec;

                //tercer filtro de validaciones
                var errorBene  = _mBeneficiosDB.ValidarBeneficioDato(CodCliente, beneficioValida);
                if (errorBene.Code == -1)
                {
                    info.Code = errorBene.Code;
                    info.Description = errorBene.Description;
                    return info;
                }

                using var connection = new SqlConnection(clienteConnString);
                {
                    string values = " ";
                    if (!string.IsNullOrEmpty(beneficio.cod_producto))
                    {
                        values += $@", cod_producto = '{beneficio.cod_producto}'";
                    }
                    if (!string.IsNullOrEmpty(beneficio.cta_bancaria))
                    {
                        values += $@", cta_bancaria = '{beneficio.cta_bancaria}'";
                    }
                    if (beneficio.cod_banco != null)
                    {
                        values += $@", cod_banco = {beneficio.cod_banco} ";
                    }

                    var query = $@"select estado from afi_bene_pago where cedula = '{beneficio.cedula}' AND consec = {beneficio.consec} 
                        AND cod_beneficio = '{beneficio.cod_beneficio}'";
                    string estadoValido = connection.Query<string>(query).FirstOrDefault();

                    if (estadoValido.Trim() == "S")
                    {
                        //Insertar en pagos
                        query = $@"update afi_bene_pago set 
                          tipo = '{beneficio.tipo}',
                          monto = {beneficio.monto},
                          tipo_emision = '{beneficio.tipo_emision}',
                          t_identificacion = '{beneficio.t_identificacion}',
                          t_beneficiario = '{beneficio.t_beneficiario}', 
                          t_email = '{beneficio.t_email}'
                          {values}
                        where 
                            cedula = '{beneficio.cedula}' AND consec = {beneficio.consec} 
                            AND cod_beneficio = '{beneficio.cod_beneficio}'
                        ";

                        info.Code = connection.Execute(query);

                        if (info.Code > 0)
                        {
                            //Inserto Bitacora

                            _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                            {
                                EmpresaId = CodCliente,
                                cod_beneficio = beneficio.cod_beneficio,
                                consec = beneficio.consec,
                                movimiento = "Actualiza",
                                detalle = $"Actualiza Orden de Pago COD: [" + beneficio.id_pago + "]",
                                registro_usuario = beneficio.registro_usuario.ToUpper(),
                            });


                            info.Description = "Orden de pago actualizada exitosamente";
                        }
                    }else
                    {
                        info.Code = -1;
                        info.Description = "No se permite modificar la orden de pago porque ya se encuentra procesada";
                    }
                }
            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;
        }

        /// <summary>
        /// Metodo para obtener la lista de proyecciones de pago
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="Cedula"></param>
        /// <param name="Cod_Beneficio"></param>
        /// <param name="Consec"></param>
        /// <returns></returns>
        public ErrorDto<List<AfiBenePagoProyecta>> AfiBeneficioIntegralProyeccionPago_Obtener(int CodCliente, string Cedula, string Cod_Beneficio, int Consec)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDto<List<AfiBenePagoProyecta>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select * from AFI_BENE_PAGO_PROYECTA where cedula = '{Cedula}' 
                        and cod_beneficio = '{Cod_Beneficio}' and consec = {Consec}
                        order by FECHA_VENCE asc";
                    response.Result = connection.Query<AfiBenePagoProyecta>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = null;
            }

            return response;
        }

        /// <summary>
        /// Metodo para validar si existe una proyeccion de pago
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="beneficio"></param>
        /// <returns></returns>
        public ErrorDto AfiBeneficioIntegralProyeccionPago_Insertar(int CodCliente, AfiBenePagoProyecta beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                int codBanco = beneficio.cod_banco ?? 0;
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"insert AFI_BENE_PAGO_PROYECTA(
                                            cedula,
                                            consec,
                                            cod_beneficio,
                                            tipo,
                                            fecha_vence,
                                            monto,
                                            cod_banco, 
                                            tipo_emision,
                                            cta_bancaria,
                                            estado,
                                            activa_usuario,
                                            activa_fecha,
                                            t_identificacion,
                                            t_beneficiario, 
                                            t_email,
                                            registro_fecha,
                                            registro_usuario,
                                            cod_producto
                            )values(
                                            '{beneficio.cedula.Trim().Replace("-","")}',
                                            {beneficio.consec},
                                            '{beneficio.cod_beneficio}',
                                            '{beneficio.tipo}',
                                            '{beneficio.fecha_vence}',
                                            {beneficio.monto},
                                            {codBanco},
                                            '{beneficio.tipo_emision}',
                                            '{beneficio.cta_bancaria}',
                                            'P',
                                            '{beneficio.activa_usuario}',
                                            Getdate(),
                                            '{beneficio.t_identificacion.Trim().Replace("-", "")}',
                                            '{beneficio.t_beneficiario}',
                                            '{beneficio.t_email}',
                                            Getdate(),
                                            '{beneficio.registro_usuario}',
                                            '{beneficio.cod_producto}'
                            )
                    ";

                    connection.Execute(query);


                    query = $@"SELECT TOP 1 PLAN_ID from AFI_BENE_PAGO_PROYECTA 
                                WHERE cedula = '{beneficio.cedula.Trim().Replace("-", "")}' AND consec = {beneficio.consec} AND COD_BENEFICIO = '{beneficio.cod_beneficio}' 
                                ORDER BY PLAN_ID DESC";
                    var idPlan = connection.Query<int>(query).FirstOrDefault();

                    _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                    {
                        EmpresaId = CodCliente,
                        cod_beneficio = beneficio.cod_beneficio,
                        consec = beneficio.consec,
                        movimiento = "Inserta",
                        detalle = $"Inserta Proyecci�n de Pago COD: [" + idPlan + "]",
                        registro_usuario = beneficio.registro_usuario.ToUpper(),
                    });

                    info.Description = "Proyecci�n de pago cargada exitosamente";
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;
        }

        /// <summary>
        /// Metodo para actualizar una proyeccion de pago
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="beneficio"></param>
        /// <returns></returns>
        public ErrorDto AfiBeneficioIntegralProyeccionPago_Actualizar(int CodCliente, AfiBenePagoProyecta beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"update AFI_BENE_PAGO_PROYECTA 
                        SET
                            fecha_vence = '{beneficio.fecha_vence}',
                            monto = {beneficio.monto},
                            cod_banco = {beneficio.cod_banco}, 
                            tipo_emision = '{beneficio.tipo_emision}',
                            cta_bancaria = '{beneficio.cta_bancaria}',
                            activa_usuario = '{beneficio.activa_usuario}',
                            activa_fecha = Getdate(),
                            t_identificacion = '{beneficio.t_identificacion}',
                            t_beneficiario = '{beneficio.t_beneficiario}',
                            t_email = '{beneficio.t_email}',
                            cod_producto = '{beneficio.cod_producto}',
                            tipo = '{beneficio.tipo}'
                            
                        where 
                            cedula = '{beneficio.cedula.Trim()}'
                            and cod_beneficio = '{beneficio.cod_beneficio}'
                            and plan_id = {beneficio.plan_id}
                            and estado = 'P'
                    ";

                    connection.Execute(query);


                    _mBeneficiosDB.BitacoraBeneficios(new BitacoraBeneInsertarDTO
                    {
                        EmpresaId = CodCliente,
                        cod_beneficio = beneficio.cod_beneficio,
                        consec = beneficio.consec,
                        movimiento = "Actualiza",
                        detalle = $"Actualiza Proyecci�n de Pago COD: [" + beneficio.plan_id + "]",
                        registro_usuario = beneficio.registro_usuario.ToUpper(),
                    });

                    info.Description = "Proyecci�n de pago actualizada exitosamente";
                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;
        }

        /// <summary>
        /// Metodo para eliminar una proyeccion de pago
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="Plan_Id"></param>
        /// <returns></returns>
        public ErrorDto AfiBeneficioIntegralProyeccionPago_Eliminar(int CodCliente, int Plan_Id)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDto info = new ErrorDto();
            info.Code = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"delete from AFI_BENE_PAGO_PROYECTA 
                        where Plan_Id = '{Plan_Id}' and estado = 'P'";
                    info.Code = connection.Execute(query);
                    if (info.Code > 0)
                    {
                        info.Description = "Proyecci�n de pago eliminada exitosamente";
                    }
                    else
                    {
                        info.Description = "No se encontraron resultados";
                    }

                }

            }
            catch (Exception ex)
            {
                info.Code = -1;
                info.Description = ex.Message;
            }

            return info;
        }
    }
}