using Dapper;
using Microsoft.Data.SqlClient;
using PgxAPI.Models;
using PgxAPI.Models.AF;
using PgxAPI.Models.ERROR;
using System.Collections.Generic;
using System.Data;

namespace PgxAPI.DataBaseTier
{
    public class frmAF_BeneficioAsgDB
    {
        private readonly IConfiguration _config;
        private fxMontoModel datosBase = new fxMontoModel();
        mSecurityMainDb DBBitacora;
        mProGrx_Main mProGrx_Main;

        private bool bAplicaParcial = false;

        public frmAF_BeneficioAsgDB(IConfiguration config)
        {
            _config = config;
            DBBitacora = new mSecurityMainDb(_config);
            mProGrx_Main = new mProGrx_Main(_config);
        }

        public ErrorDTO Bitacora(BitacoraInsertarDTO data)
        {
            return DBBitacora.Bitacora(data);
        }

        public ErrorDTO SbSIFRegistraTags(SIFRegistraTagsRequestDTO data)
        {
            return mProGrx_Main.SbSIFRegistraTags(data);
        }

        /// <summary>
        /// Metodo para obtener la lista de beneficios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <param name="pagina"></param>
        /// <param name="paginacion"></param>
        /// <param name="filtro"></param>
        /// <returns></returns>
        public ErrorDTO<AfiBeneOtorgaAsgDataList> AfiBeneOtorga_Obtener(int CodCliente, string cedula, int? pagina, int? paginacion, string? filtro)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<AfiBeneOtorgaAsgDataList>();
            response.Result = new AfiBeneOtorgaAsgDataList();
            string paginaActual = " ", paginacionActual = " ";
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select COUNT(*) from afi_bene_otorga where cedula = '{cedula}'";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtro != null)
                    {
                        filtro = " AND (B.Descripcion LIKE '%" + filtro + "%' OR O.cod_beneficio LIKE '%" + filtro + "%' OR O.consec LIKE '%" + filtro + "%')";
                    }
                    if (pagina != null)
                    {
                        paginaActual = "ORDER BY O.cod_beneficio OFFSET " + pagina + " ROWS ";
                        paginacionActual = " FETCH NEXT " + paginacion + " ROWS ONLY ";
                    }

                    query = $@"Select O.*,B.Descripcion from afi_bene_otorga O  inner join afi_beneficios B 
                                      on O.cod_beneficio = B.cod_beneficio where O.cedula = '{cedula}'
                                {filtro} 
                                {paginaActual} {paginacionActual} ";
                    response.Result.beneficios = connection.Query<AfiBeneOtorgaData>(query).ToList();
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
        /// Metodo para obtener el detalle de un beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_beneficio"></param>
        /// <returns></returns>
        public ErrorDTO<List<Afi_BeneficiosDTO>> BeneficioDetalle_Obtener(int CodCliente, string cod_beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<Afi_BeneficiosDTO>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select rtrim(cod_Beneficio) as  cod_Beneficio, rtrim(descripcion) as descripcion,tipo,monto 
                                       ,modifica_diferencia,aplica_beneficiarios,aplica_parcial 
                                        from afi_beneficios where cod_beneficio  = '{cod_beneficio}'";
                    response.Result = connection.Query<Afi_BeneficiosDTO>(query).ToList();
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
        /// Metodo para obtener beneficios otorgados a un socio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="codBeneficio"></param>
        /// <param name="consec"></param>
        /// <returns></returns>
        public ErrorDTO<List<AfiBeneOtorgaData>> AfiBeneOtorgaSocio_Obtener(int CodCliente, string codBeneficio, int consec)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response  = new ErrorDTO<List<AfiBeneOtorgaData>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select A.*,O.descripcion, S.Nombre AS sNombre
                                    from afi_bene_otorga A
                                     inner join Socios S on A.cedula = S.cedula
                                      left join Sif_Oficinas O on A.cod_oficina = O.cod_Oficina
                                    where cod_beneficio = '{codBeneficio}' and consec = {consec} ";
                    response.Result = connection.Query<AfiBeneOtorgaData>(query).ToList();
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
        /// Metodo para obtener los pagos de un beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="codBeneficio"></param>
        /// <param name="consec"></param>
        /// <returns></returns>
        public ErrorDTO<List<AfiBeneficioPago>> AfiBeneficioPagos_Obtener(int CodCliente, string codBeneficio, int consec)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<AfiBeneficioPago>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select Bp.*, B.Descripcion as 'BancoDesc'
                                from afi_bene_pago Bp
                                 left join Tes_Bancos B on Bp.cod_Banco = B.id_Banco 
                                where Bp.consec = {consec} and Bp.cod_beneficio = '{codBeneficio}'";
                    response.Result = connection.Query<AfiBeneficioPago>(query).ToList();
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
        /// Metodo para obtener el nombre del beneficiario
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedulabn"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDTO Beneficiario_Obtener(int CodCliente, string cedulabn, string cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select Nombre from beneficiarios where cedulabn = '{cedulabn}' and cedula = '{cedula}' ";
                    info.Description = connection.Query<string>(query).FirstOrDefault();
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
        /// Metodo para obtener la lista de cuentas bancarias
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="Identificacion"></param>
        /// <param name="BancoId"></param>
        /// <param name="DivisaCheck"></param>
        /// <returns></returns>
        public ErrorDTO<List<CuentaListaData>> Cuentas_Obtener(int CodCliente, string Identificacion, int BancoId, int DivisaCheck)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<CuentaListaData>>();
            try
            {

                Identificacion = Identificacion.Replace("undefined", "").Replace(" ", "").Trim();

                using var connection = new SqlConnection(clienteConnString);
                {

                    var procedure = "[spSys_Cuentas_Bancarias]";
                    var values = new
                    {
                        Identificacion = Identificacion.Trim(),
                        BancoId = BancoId,
                        DivisaCheck = DivisaCheck

                    };
                    response.Code = 0;
                    response.Result = connection.Query<CuentaListaData>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
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
        /// Valida los montos de un beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public fxMontosResult fxMonto(int CodCliente, fxMontoModel datos)
        {
            datosBase = datos;
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            fxMontosResult info = new()
            {
                Code = 0
            };
            string query = "", menbrecia = "";
            float cMontoPagado = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    query = $@"select case when estadoactual = 'S' then datediff(d,fechaingreso,Getdate()) 
	                                     else 0 end as Membresia from socios where cedula = '{datos.cedula}'";
                    menbrecia = connection.Query<string>(query).FirstOrDefault();

                    if (menbrecia != null)
                    {
                        query = $@"select monto from afi_beneficio_montos where
                                cod_beneficio = '{datos.cod_beneficio}' and  {menbrecia}  between inicio and corte ";

                        float cMontoBene = connection.Query<float>(query).FirstOrDefault();
                        info.montoGira = cMontoBene;
                        if (datos.iBeneficiario == 0)
                        {
                            query = $@"select COALESCE(sum(monto), 0)  as  monto  from afi_bene_otorga 
                                        where cod_beneficio = '{datos.cod_beneficio}' and  cedula = '{datos.cedula}' ";
                        }
                        else
                        {
                            query = $@"select COALESCE(sum(monto), 0)  as  monto  from afi_bene_otorga 
                                    where cod_beneficio = '{datos.cod_beneficio}' and 
		                                    cedula = '{datos.cedula}' and solicita = '{datos.solicita}' ";
                        }

                        cMontoPagado = connection.Query<float>(query).FirstOrDefault();
                        string mensaje = "";
                        if (cMontoPagado >= datos.monto && datos.bConsulta == false && fxValida(CodCliente, ref mensaje).Result == false && datos.bNuevo == false)
                        {
                            info.Code = 1;
                            info.Description = "Ya le fue asignado el monto de la ayuda";
                            return info;
                        }

                        if (datos.monto <= 0)
                        {
                            //Estado Pendiente
                            info.Code = 2;
                            info.Description = "- No cumple con la membresía para este beneficio";
                            return info;
                        }
                        else if (datos.bNuevo == false)
                        {
                            //Estado Solicitado
                            fxValida(CodCliente, ref mensaje);
                            info.Description = mensaje;
                            info.Code = 3;
                            if (datos.iGrupo > 0)
                            {

                                if (datos.cMontoRealGrupo >= cMontoBene && datos.bAsignado == false)
                                {
                                    info.monto = cMontoBene;
                                    info.Description += mensaje;
                                }
                                else
                                {
                                    info.monto = datos.cMontoRealGrupo;
                                    info.montoGira = datos.monto;
                                    info.disponible = info.monto - info.montoGira;
                                    info.Description += mensaje;
                                }
                            }
                            else
                            {
                                info.monto = datos.monto;
                                info.Description += mensaje;
                            }
                        }
                        else
                        {
                            info.monto = 0;
                            info.Description += mensaje;
                        }

                    }
                    else
                    {
                        //Pendiente
                        info.Code = 4;
                        info.Description = "- No se encontró membresía para esta persona en este beneficio";
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
        /// Valida montos y asignaciones de beneficios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="mensaje"></param>
        /// <returns></returns>
        public ErrorDTO<bool> fxValida(int CodCliente, ref string mensaje)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<bool>();
            string query = "";
            int iMaxOtorga = 0, Cantidad;
            float cMontoGrupo = 0, iCantidaGrupo = 0, cMontoAsignado = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    //Valida Maximo de otorgamiento
                    query = $@"Select maximo_otorga from afi_beneficios where cod_beneficio = '{datosBase.cod_beneficio}' ";
                    iMaxOtorga = connection.Query<int>(query).FirstOrDefault();

                    //validacion por grupo
                    query = $@"select cod_grupo from afi_grupo_beneficio where cod_beneficio = '{datosBase.cod_beneficio}' ";
                    datosBase.iGrupo = connection.Query<int>(query).FirstOrDefault();

                    datosBase.bAsignado = false;

                    if (datosBase.iGrupo > 0)
                    {
                        query = $@"Select monto from afi_bene_grupos where cod_grupo  = '{datosBase.iGrupo}' ";
                        cMontoGrupo = connection.Query<float>(query).FirstOrDefault();

                        query = $@" Select count(*)as cantidad,isnull(sum(B.MONTO),0) as monto from afi_bene_otorga B inner join
                                     afi_grupo_beneficio G ON B.cod_beneficio = G.cod_beneficio
                                      where B.cedula ='{datosBase.cedula}' ";

                        AfiBeneMontoData dt = connection.Query<AfiBeneMontoData>(query).FirstOrDefault();
                        if (dt != null)
                        {
                            iCantidaGrupo = dt.cantidad;
                            cMontoAsignado = dt.monto;

                            datosBase.bAsignado = true;
                            if (cMontoAsignado >= cMontoGrupo)
                            {
                                mensaje += "\n - Sobrepasa el monto asignado al grupo de beneficios ";
                            }
                            else
                            {
                                datosBase.cMontoRealGrupo = cMontoGrupo - cMontoAsignado;
                            }

                        }


                    }

                    if (datosBase.bConsulta == false)
                    {
                        query = $@"select isnull(count(*),0) as  cantidad from afi_bene_otorga 
                                        where cod_beneficio = '{datosBase.cod_beneficio}' and 
			                                          cedula = '{datosBase.cedula}' ";
                        Cantidad = connection.Query<int>(query).FirstOrDefault();
                        if (Cantidad >= iMaxOtorga)
                        {
                            mensaje = " - Excede el numero de veces de Otorgamientos del Beneficio";
                        }
                    }

                    if (mensaje.Length > 0)
                    {
                        //Estado Pendiente
                        response.Result = false;
                    }
                    else
                    {
                        response.Result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = false;
            }

            return response;
        }

        /// <summary>
        /// Metodo para obtener los productos de un beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="codBeneficio"></param>
        /// <param name="consec"></param>
        /// <returns></returns>
        public ErrorDTO<List<AfiBeneficioPago>> AfiBeneficioProducto_Obtener(int CodCliente, string codBeneficio, int consec)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<AfiBeneficioPago>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select R.*, P.Descripcion as 'ProdDesc', P.costo_unidad as 'ProdCu' 
                       from afi_bene_prodasg R inner join afi_bene_productos P on R.cod_Producto = P.cod_Producto 
                       where R.consec = {consec} and R.cod_beneficio = '{codBeneficio.Trim()}' ";
                    response.Result = connection.Query<AfiBeneficioPago>(query).ToList();
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
        /// Carga la lista de oficinas de un usuario
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO<List<SIFOficinasUsuarioResultDTO>> CargaOficinas(int CodCliente, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<SIFOficinasUsuarioResultDTO>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {

                    var procedure = "[sbSIFOficinasUsuario]";
                    var values = new
                    {
                        Usuario = usuario,
                    };

                    response.Result = connection.Query<SIFOficinasUsuarioResultDTO>(procedure, values, commandType: CommandType.StoredProcedure).ToList();
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
        /// Consulta si el socio tiene membresia activa
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDTO Menbrecia_Consulta(int CodCliente, string? cedula)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new()
            {
                Code = 0
            };
            try
            {
                if(cedula == null)
                {
                    return info;
                }

                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select case when estadoactual = 'S' then datediff(d,fechaingreso,Getdate()) 
	                                     else 0 end as Membresia from socios where cedula = '{cedula}'";
                    string menbrecia = connection.Query<string>(query).FirstOrDefault();
                    if (menbrecia == null || menbrecia == "0")
                    {
                        info.Code = -1;
                        info.Description = "- No se encontro membresia para esta persona en este beneficio";
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
        /// Obtiene el monto de un beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_beneficio"></param>
        /// <param name="cedula"></param>
        /// <param name="solicita"></param>
        /// <returns></returns>
        public ErrorDTO Monto_Obtener(int CodCliente, string cod_beneficio, string cedula, string solicita)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new()
            {
                Code = 0
            };
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = "";
                    //validacion por grupo
                    query = $@"select cod_grupo from afi_grupo_beneficio where cod_beneficio = '{cod_beneficio}' ";
                    datosBase.iGrupo = connection.Query<int>(query).FirstOrDefault();

                    query = $@"Select monto from afi_bene_grupos where cod_grupo  = '{datosBase.iGrupo}' ";
                    info.Description = connection.Query<float>(query).FirstOrDefault().ToString();
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
        /// Obtiene la cuenta contable bancaria y la cuenta del beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_beneficio"></param>
        /// <param name="cedula"></param>
        /// <param name="consec"></param>
        /// <returns></returns>
        public ErrorDTO<AsientoContableData> AsientoContableData_Obtener(int CodCliente, string cod_beneficio, string cedula, int consec)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<AsientoContableData>();
            response.Result = new AsientoContableData();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select cod_banco from afi_bene_pago where  cedula = '{cedula}'
				             and cod_beneficio = '{cod_beneficio}' and consec = {consec}";

                    int bancos = connection.Query<int>(query).FirstOrDefault();

                    //Obtengo cuanta contable bancaria

                    CuentasBancariasModels ctacontable = new CuentasBancariasModels();
                    query = $@"select ctaconta as cuenta, descripcion from Tes_Bancos where id_banco = {bancos}";
                    ctacontable = connection.Query<CuentasBancariasModels>(query).FirstOrDefault();

                    //Obtengo cuantas  de la orden de compra
                    CuentasBancariasModels ctaBeneficio = new CuentasBancariasModels();
                    query = $@"select cod_cuenta as cuenta,descripcion from afi_beneficios where cod_beneficio = '{cod_beneficio}' ";
                    ctaBeneficio = connection.Query<CuentasBancariasModels>(query).FirstOrDefault();

                    //Obtengo monto de la boleta
                    query = $@"select monto from afi_bene_otorga where consec = '{consec}' AND cedula = '{cedula}' AND cod_beneficio = '{cod_beneficio}'";
                    float monto = connection.Query<float>(query).FirstOrDefault();

                    if (ctacontable != null)
                    {
                        response.Result.fxcuentabanco = ctacontable.cuenta;
                        response.Result.fxDescripcion = ctacontable.descripcion;
                    }
                    else
                    {
                        response.Result.fxcuentabanco = "ND";
                        response.Result.fxDescripcion = "ND";
                    }

                    if (ctaBeneficio != null)
                    {
                        response.Result.fxDescribe = ctaBeneficio.descripcion;
                        response.Result.fxcuenta = ctaBeneficio.cuenta;
                    }
                    else
                    {
                        response.Result.fxDescribe = "ND";
                        response.Result.fxcuenta = "ND";
                    }

                    response.Result.fxmonto = monto;
                    response.Result.fxmontobene = monto;
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

        //version 2 de guardado de informacion

        /// <summary>
        /// Carga la Lista de Tipos de Beneficios
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO<List<BeneficioData>> BeneficioUsuario_Obtener(int CodCliente, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<BeneficioData>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select rtrim(cod_Beneficio) as cod_beneficio,rtrim(descripcion) as descripcion from afi_beneficios 
                                 where estado = 'A' and cod_beneficio in (select cod_beneficio from AFI_BENE_GRUPOSB 
                                 where cod_grupo in(  select cod_grupo from AFI_BENE_USERG where usuario = '{usuario}'))";
                    response.Result = connection.Query<BeneficioData>(query).ToList();
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
        /// Busca la lista de Cuentas Bancarias de un Usuario
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDTO<List<CuentaListaData>> CuentasUsuario_Obtener(int CodCliente, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<List<CuentaListaData>>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select B.id_banco as 'IdX',rtrim(B.descripcion) as 'ItmX'
                                from tes_banco_asg T inner join Tes_Bancos B on T.id_banco = B.id_banco
                                where T.nombre = '{usuario}'";
                    response.Result = connection.Query<CuentaListaData>(query).ToList();
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
        /// Metodo para guardar un beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDTO AfBeneficioAsg_Guardar(int CodCliente, string usuario , AfiBeneficioAsgInsertar datos)
        {
            ErrorDTO info = new()
            {
               Code = 0
            };

             try
             {
                 if (datos.cedula == "" || datos.cedula == null)
                 {
                     info.Code = -1;
                     info.Description = "Cedula no puede ser nula";
                 }
                 else
                 {
                     AfiBeneficiosDTO afiBeneficios = AfiBeneficioDTO_Obtener(CodCliente, datos.cod_beneficio).Result;
                     bAplicaParcial = afiBeneficios.aplica_parcial == 1 ? true : false;

                     if (afiBeneficios.aplica_beneficiarios == 1)
                     {
                         if (datos.solicita == null || datos.solicita_nombre == null)
                         {
                             info.Code = -1;
                             info.Description = "Verifique los datos del Fallecido";
                             return info;
                         }
                     }

                     switch (datos.tipoBeneficio)
                     {
                         case "M":

                             if (bAplicaParcial)
                             {
                                 if (datos.disponible > 0)
                                 {
                                     datos.monto = datos.montoGira;
                                 }
                                 //llamo guardar beneficio
                                 info = Guardar_Beneficio(CodCliente, datos, "S", usuario);

                             }
                             else if (datos.disponible == 0 && datos.solicita != null)
                             {
                                 //llamo guardar beneficio
                                 info = Guardar_Beneficio(CodCliente, datos, "N", usuario);
                             }
                             else
                             {
                                 info.Code = -1;
                                 info.Description = "No ha distribuido el disponible";
                                 return info;
                             }
                             break;
                         case "P":
                             if (datos.productos.Count > 0)
                             {
                                 //llamo guardar beneficio producto
                                 info = Guarda_Productos(CodCliente, datos, "N", usuario);
                             }
                             else
                             {
                                 info.Code = -1;
                                 info.Description = "No se almacenó la información";
                                 return info;
                             }
                             break;
                         default:
                             break;
                     }

                 }

                 info.Description = "Información guardada Satisfactoriamente";
             }
             catch (Exception ex)
             {
                 info.Code = -1;
                 info.Description = ex.Message;
             }
             return info;
        }

        /// <summary>
        /// Obtiene el detalle de un beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="Cod_Beneficio"></param>
        /// <returns></returns>
        public ErrorDTO<AfiBeneficiosDTO> AfiBeneficioDTO_Obtener(int CodCliente, string Cod_Beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            var response = new ErrorDTO<AfiBeneficiosDTO>();
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"select * from afi_beneficios where Cod_Beneficio = '{Cod_Beneficio}'";
                    response.Result = connection.Query<AfiBeneficiosDTO>(query).FirstOrDefault();
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
        /// Metodo para guardar un beneficio 
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="datos"></param>
        /// <param name="modificaMonto"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private ErrorDTO Guardar_Beneficio(int CodCliente, AfiBeneficioAsgInsertar datos, string modificaMonto, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO<List<SIFOficinasUsuarioResultDTO>> empresa = CargaOficinas(CodCliente, usuario);

            ErrorDTO info = new()
            {
                Code = 0
            };

            string query = "";
            try
            {
                if (datos.txtBeneficioId == "" || datos.txtBeneficioId == null)
                {
                    long vBeneConsec = fxConsec(CodCliente, datos.cod_beneficio);

                    using var connection = new SqlConnection(clienteConnString);
                    {
                        query = $@"insert afi_bene_otorga (
                                            consec,
                                            cod_beneficio,
                                            cedula,
                                            monto,
                                            modifica_monto,
                                            registra_user,
                                            registra_fecha,
                                            estado,
                                            notas,
                                            Solicita,
                                            nombre,
                                            tipo,
                                            cod_oficina )values(
                                            {vBeneConsec},
                                            '{datos.cod_beneficio}',
                                            '{datos.cedula.Trim()}',
                                            {datos.monto},
                                            '{modificaMonto}',
                                            '{usuario.ToUpper()}',
                                            Getdate(),
                                            '{datos.estado}',
                                            '{datos.notas}',
                                            '{datos.solicita}',
                                            '{datos.solicita_nombre.ToUpper()}',
                                            '{datos.tipoBeneficio}',
                                            '{empresa.Result[0].Titular}' )";
                        int resp = connection.Execute(query);
                        if (resp > 0)
                        {

                            Bitacora(new BitacoraInsertarDTO
                            {
                                EmpresaId = CodCliente,
                                Usuario = usuario.ToUpper(),
                                DetalleMovimiento = "Registra, Beneficio:" + vBeneConsec + "-" + datos.cod_beneficio + ", Cedula [" + datos.cedula.Trim() + "]",
                                Movimiento = "REGISTRA - WEB",
                                Modulo = 7
                            });

                            query = $@"insert afi_bene_pago(
                                            cedula,
                                            consec,
                                            cod_beneficio,
                                            tipo,
                                            monto,
                                            cod_banco, 
                                            tipo_emision,
                                            cta_bancaria,
                                            estado
                                            )values(
                                            '{datos.solicita}',
                                            {vBeneConsec},
                                            '{datos.cod_beneficio}',
                                            '{datos.tipoBeneficio}',
                                            {datos.monto},
                                            {datos.cod_banco},
                                            '{datos.emitir}',
                                            '{datos.cod_cuenta}',
                                            '{datos.estado}'
                                            )
                        ";

                            resp = connection.Execute(query);

                            SbSIFRegistraTags(new SIFRegistraTagsRequestDTO
                            {
                                Codigo = vBeneConsec.ToString(),
                                Tag = "S.BEN.01",
                                Usuario = usuario.ToUpper(),
                                Observacion = "Reg. Ben",
                                Documento = datos.cod_beneficio,
                                Modulo = "BEN",
                            });

                            info.Description = "Informacion Guardada Satisfactoriamente";
                        }
                        else
                        {
                            info.Code = -1;
                            info.Description = "Error al insertar el registro";
                        }

                    }

                }
                else
                {
                    using var connection = new SqlConnection(clienteConnString);
                    {
                        query = $@"update afi_bene_otorga set 
                                        notas = '{datos.notas}',
                                        estado='{datos.estado}',
                                        modifica_monto = '{modificaMonto}',
                                        solicita = '{datos.solicita}',
                                        monto = {datos.monto},
                                        nombre = '{datos.solicita_nombre}',
                                        TIPO = '{datos.tipoBeneficio}' 
                                        where cod_beneficio = '{datos.cod_beneficio}' and cedula = '{datos.cedula.Trim()}' and consec = {datos.consec} ";

                        int resp = connection.Execute(query);
                        if (resp > 0)
                        {
                            Bitacora(new BitacoraInsertarDTO
                            {
                                EmpresaId = CodCliente,
                                Usuario = usuario.ToUpper(),
                                DetalleMovimiento = "Modifica, Beneficio:" + datos.consec + "-" + datos.cod_beneficio + ", Cedula [" + datos.cedula.Trim() + "]",
                                Movimiento = "MODIFICA - WEB",
                                Modulo = 7
                            });

                            query = $@"update afi_bene_pago set 
                                        monto = {datos.monto},
                                        tipo = '{datos.tipoBeneficio}',
                                        tipo_emision = '{datos.emitir}',
                                        cta_bancaria = '{datos.cod_cuenta}',
                                        cod_banco = {datos.cod_banco},
                                        estado = '{datos.estado}'
                                        where cod_beneficio = '{datos.cod_beneficio}' and cedula = '{datos.solicita.Trim()}' and consec = {datos.consec} ";

                            resp = connection.Execute(query);
                        }
                        else
                        {
                            info.Code = -1;
                            info.Description = "Error al actualizar el registro";
                        }
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
        /// Metodo para guardar los productos de un beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="datos"></param>
        /// <param name="modificaMonto"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private ErrorDTO Guarda_Productos(int CodCliente, AfiBeneficioAsgInsertar datos, string modificaMonto, string usuario)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            ErrorDTO info = new ErrorDTO
            {
                Code = 0
            };
            string query = "";
            try
            {
                if (datos.txtBeneficioId == "" || datos.txtBeneficioId == null)
                {
                    long vBeneConsec = fxConsec(CodCliente, datos.cod_beneficio);
                    using var connection = new SqlConnection(clienteConnString);
                    {
                        query = $@"insert afi_bene_otorga (
                                            consec,
                                            cod_beneficio,
                                            cedula,
                                            monto,
                                            modifica_monto,
                                            registra_user,
                                            registra_fecha,
                                            estado,
                                            notas,
                                            Solicita,
                                            nombre,
                                            tipo
                                            )values(
                                            {vBeneConsec},
                                            '{datos.cod_beneficio}',
                                            '{datos.cedula.Trim()}',
                                            {datos.monto},
                                            '{modificaMonto}',
                                            '{usuario}',
                                            Getdate(),
                                            '{datos.estado}',
                                            '{datos.notas}',
                                            '{datos.solicita}',
                                            '{datos.solicita_nombre.ToUpper()}',
                                            '{datos.tipoBeneficio}'
                                            )";

                        int resp = connection.Execute(query);
                        if (resp > 0)
                        {

                            Bitacora(new BitacoraInsertarDTO
                            {
                                EmpresaId = CodCliente,
                                Usuario = usuario.ToUpper(),
                                DetalleMovimiento = "Registra, Beneficio:" + vBeneConsec + "-" + datos.cod_beneficio + ", Cedula [" + datos.cedula.Trim() + "]",
                                Movimiento = "REGISTRA - WEB",
                                Modulo = 7
                            });

                            foreach (var prod in datos.productos)
                            {
                                query = $@"insert afi_bene_prodasg(consec,cod_beneficio,cod_producto,cantidad,costo_unidad)
                                    	values({vBeneConsec},'{datos.cod_beneficio}','{prod.cod_producto}',{prod.cantidad},{prod.costo_unidad})";

                                resp = connection.Execute(query);
                            }


                            info.Description = "Informacion Guardada Satisfactoriamente";
                        }
                        else
                        {
                            info.Code = -1;
                            info.Description = "Error al insertar el registro";
                        }

                    }
                }
                else
                {
                    using var connection = new SqlConnection(clienteConnString);
                    {
                        query = $@"update afi_bene_otorga set 
                                        notas = '{datos.notas}',
                                        estado='{datos.estado}',
                                        modifica_monto = '{modificaMonto}',
                                        solicita = '{datos.solicita}',
                                        monto = {datos.monto},
                                        nombre = '{datos.solicita_nombre}',
                                        TIPO = '{datos.tipoBeneficio}' 
                                        where cod_beneficio = '{datos.cod_beneficio}' and cedula = '{datos.cedula.Trim()}' and consec = {datos.consec} ";

                        int resp = connection.Execute(query);
                        if (resp > 0)
                        {
                            Bitacora(new BitacoraInsertarDTO
                            {
                                EmpresaId = CodCliente,
                                Usuario = usuario.ToUpper(),
                                DetalleMovimiento = "Modifica, Beneficio:" + datos.consec + "-" + datos.cod_beneficio + ", Cedula [" + datos.cedula.Trim() + "]",
                                Movimiento = "MODIFICA - WEB",
                                Modulo = 7
                            });

                            foreach (var prod in datos.productos)
                            {
                                query = $@"update afi_bene_prodasg set 
                                        cantidad = {prod.cantidad}
                                        where cod_beneficio = '{datos.cod_beneficio}' and consec = {datos.consec} and cod_producto = '{prod.cod_producto}' ";

                                resp = connection.Execute(query);
                            }
                        }
                        else
                        {
                            info.Code = -1;
                            info.Description = "Error al actualizar el registro";
                        }
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
        /// Obtiene el consecutivo de un beneficio
        /// </summary>
        /// <param name="CodCliente"></param>
        /// <param name="cod_beneficio"></param>
        /// <returns></returns>
        private long fxConsec(int CodCliente, string cod_beneficio)
        {
            var clienteConnString = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodCliente);
            long vBeneConsec = 0;
            try
            {
                using var connection = new SqlConnection(clienteConnString);
                {
                    var query = $@"Select isnull(Max(consec),0) as consecutivo from afi_bene_otorga where cod_beneficio = '{cod_beneficio}'";
                    vBeneConsec = connection.Query<long>(query).FirstOrDefault() + 1;
                }
            }
            catch (Exception)
            {
                vBeneConsec = 0;
            }
            return vBeneConsec;
        }
    }
}