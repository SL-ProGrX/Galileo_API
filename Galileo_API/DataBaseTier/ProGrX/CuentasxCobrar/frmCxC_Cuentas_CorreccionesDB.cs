using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using static Galileo_API.Models.ProGrX.CuentasxCobrar.FrmCxCCuentasCorreccionesModels;

namespace Galileo_API.DataBaseTier.ProGrX.CuentasxCobrar
{
    public class FrmCxCCuentasCorreccionesDb
    {
        private readonly PortalDB _portalDB; 
        private readonly IConfiguration _config;  
        private const string consultaE = "Consulta realizada correctamente";
        public FrmCxCCuentasCorreccionesDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config); 
            _config = config;
        }

        /// <summary>
        ///  Consulta los bancos disponibles para emitir el pago, según la cédula del pagador y la empresa.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_CuentasCorreccionesBancos_Obtener(int CodEmpresa)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = consultaE,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                var query = $@"exec spCxC_Bancos_Autorizados ";
                var datos = conn.Query<BancoAutorizados>(query).ToList();
                foreach (var item in datos)
                {
                    string idx = item.IdX.ToString()!;
                    string itmx = item.ItmX!;

                    response.Result.Add(new DropDownListaGenericaModel { item = idx, descripcion = itmx });
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
        /// Consulta las cuentas bancarias disponibles para un cliente específico y un banco determinado.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="codBanco"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_CuentasCorreccionesCuentasBancarias_Obtener(int CodEmpresa, string cedula, string codBanco)
        {
            using var conn = DbHelper.OpenConnection(_portalDB, CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = consultaE,
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                var query = $@"exec spSys_Cuentas_Bancarias @cedula, @codBanco,1 ";
                var datos = conn.Query<CuentasBancarias>(query, new { cedula, codBanco }).ToList();
                foreach (var item in datos)
                {
                    string idx = item.IdX.ToString()!;
                    string itmx = item.ItmX!;

                    response.Result.Add(new DropDownListaGenericaModel { item = idx, descripcion = itmx });
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
        /// Consulta la información de la primer o ultimo  autorizado según la orden, para una cédula y empresa dada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="cedulaAutorizado"></param>
        /// <param name="orden"></param>
        /// <returns></returns>
        public ErrorDto<GeneralData> CxC_CuentasCorreccionesAutorizado_Consultar(int codEmpresa, string cedula, string cedulaAutorizado, int orden)
        {
            string query = $@"select Top 1 Per.Cedula as Item ,Per.nombre as Detalle from CxC_Personas Per
                                inner join CXC_PERSONAS_AUTORIZADOS Pa on Per.Cedula = Pa.Cedula_Autorizado
                                Where Pa.cedula = @cedula";

            if (orden == 1)
            {
                query += $" and Pa.Cedula_Autorizado > @cedulaAutorizado order by Pa.Cedula_Autorizado asc";
            }
            else
            {
                query += $" and Pa.Cedula_Autorizado < @cedulaAutorizado order by Cedula_Autorizado desc";
            }

            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                return conn.QueryFirstOrDefault<GeneralData>(query, new { cedula, cedulaAutorizado }) ?? new GeneralData();
            });
        }

        /// <summary>
        ///  Consulta la información de un contrato específico para una cédula y empresa dada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="orden"></param>
        /// <param name="cedula"></param>
        /// <param name="concepto"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        public ErrorDto<ContratoData> CxC_CuentasCorreccionesContrato_Consultar(int codEmpresa, int orden, string cedula, string concepto, string contrato)
        {

            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            var response = new ErrorDto<ContratoData>
            {
                Code = 0,
                Description = consultaE,
                Result = new ContratoData()
            };
            try
            {
                string query = $@"select Top 1 Cnt.Cod_Contrato 
                                from CxC_Conceptos_Contratos Cnt
                              inner join CxC_Contratos Cn on Cnt.Cod_Contrato = Cn.cod_Contrato
                              left join CxC_Personas_Contratos Pc on Cnt.cod_Contrato = Pc.cod_Contrato
                              and Cnt.Cod_Concepto = @concepto and Pc.Cedula = @cedula
                              Where Cn.Activo = 1 and Cnt.Cod_Concepto = @concepto
                              and (Pc.Cedula is not null or Cn.Suscripcion_Abierta = 1)";

                if (orden == 1)
                {
                    query += $" and Cn.cod_contrato > @contrato order by Cn.cod_contrato asc";
                }
                else
                {
                    query += $"and Cn.cod_contrato < @contrato  order by Cn.cod_contrato desc";
                }
                var respuesta = conn.Query<string>(query, new { cedula, concepto, contrato }).FirstOrDefault();

                if (respuesta != null && respuesta != "")
                {
                    response = CxC_ContratoDetalle_Consultar(codEmpresa, cedula, respuesta);

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
        ///  Consulta la información detallada de un contrato específico para una cédula y empresa dada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        public ErrorDto<ContratoData> CxC_ContratoDetalle_Consultar(int codEmpresa, string cedula, string contrato)
        {


            using var conn = DbHelper.OpenConnection(_portalDB, codEmpresa);
            var response = new ErrorDto<ContratoData>
            {
                Code = 0,
                Description = consultaE,
                Result = new ContratoData()
            };

            try
            {
                string query = $@" select Cnt.Cod_Contrato, Cnt.Descripcion, Cnt.PAGADORES_ABIERTO
                          , isnull(Per.Tasa_Corriente, Cnt.Tasa_Corriente) as 'Tasa_Corriente'
                          , ISNULL(Per.Tasa_Mora,Cnt.Tasa_Mora) as 'Tasa_Mora', isnull(Per.Plazo,Cnt.Plazo) as 'Plazo'
                          from CxC_Contratos Cnt
                          left join CxC_Personas_Contratos Per on  Cnt.Cod_Contrato = Per.cod_contrato
                          and Per.Activo = 1 and Per.Cedula =@cedula
                          Where Cnt.cod_Contrato = @contrato
                           and (Per.Cedula is not null or Cnt.Suscripcion_Abierta = 1)";

                response.Result = conn.Query<ContratoData>(query, new { cedula, contrato }).FirstOrDefault() ?? new ContratoData();


            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Consulta la información del primer o ultimo concepto  una empresa dada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="orden"></param>
        /// <param name="concepto"></param>
        /// <returns></returns>
        public ErrorDto<GeneralData> CxC_CuentasCorreccionesConceptos_Consultar(int codEmpresa, int orden, string concepto)
        {
            string query = $@"Select Top 1 cod_Concepto as Item ,Descripcion as Detalle from CxC_Conceptos 
                                where Activo = 1 ";

            if (orden == 1)
            {
                query += $"and cod_Concepto > @concepto order by cod_Concepto  asc";
            }
            else
            {
                query += $"and cod_Concepto  < @concepto order by cod_Concepto desc";
            }

            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                return conn.QueryFirstOrDefault<GeneralData>(query, new { concepto }) ?? new GeneralData();
            });
        }

        /// <summary>
        /// Consulta la información del primer o ultimo pagador de un contrato específico para una cédula y empresa dada.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="orden"></param>
        /// <param name="mCntPagadorAbierto"></param>
        /// <param name="cedula"></param>
        /// <param name="pagadorCedula"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        public ErrorDto<GeneralData> CxC_CuentasCorreccionesPagadores_Consultar(int codEmpresa, int orden, bool mCntPagadorAbierto, string cedula, string pagadorCedula, string contrato)
        {
           
            string query = mCntPagadorAbierto
                 ? $@"select Top 1 Cp.Cedula as Item,Cp.Nombre  as Detalle from CxC_Personas Cp 
                                     Where Cp.Rol_Pagador = 1 "
                 : $@"select Top 1 Cp.Cedula as Item,Per.nombre as Detalle
                                from CxC_Contratos_Pagadores Cp inner join  CxC_Contratos Cn on Cp.Cod_Contrato = Cn.Cod_Contrato
                                 inner join CxC_Personas Per on Cp.cedula = Per.cedula
                                 left join CxC_Personas_Contratos_Pagadores PcP on Cp.Cod_Contrato = PcP.cod_Contrato
                                 and Cp.Cedula = PcP.cedula_Pagador and PcP.cedula = @cedula
                                  Where Cn.Cod_Contrato =@contrato
                                 and (PcP.cedula is not null or Cn.Pagadores_Abierto = 1) ";

            if (orden == 1)
            {
                query += $" and Cp.Cedula > @pagadorCedula order by cod_Concepto  asc";
            }
            else
            {
                query += $" and Cp.Cedula < @pagadorCedula order by Cp.Cedula desc";
            }

            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                return conn.QueryFirstOrDefault<GeneralData>(query, new { cedula, contrato, pagadorCedula }) ?? new GeneralData();
            });
        }

        /// <summary>
        /// Consulta la información de una cuenta por cobrar específica para una empresa.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operacion"></param>
        /// <returns></returns>
        public ErrorDto<CuentaPorCobrarData> CxC_CuentasCorrecciones_Consultar(int CodEmpresa, int operacion)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select Cedula,Operacion, Nombre, Cod_Concepto, ConceptoDesc, Cod_Contrato, ContratoDesc, cedula_pagador, PagadorNom,
                                       Cedula_Autorizado,AutorizadoNom,BancoDesc,Emitir_Banco,Emitir_Tipo,Emitir_Cuenta,Emitir_Cuenta,CuentaDesc,Pagadores_Abierto
                                       from vCxC_Cuentas_Consulta
                                        where Operacion = @operacion ";

                return conn.Query<CuentaPorCobrarData>(query, new { operacion }).FirstOrDefault() ?? new CuentaPorCobrarData();
            });
        }

        /// <summary>
        /// Consulta el nombre de un cliente específico de una cédula.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<string> CxC_CuentasCorreccionesClientesNombre_Consultar(int CodEmpresa, string cedula)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select Nombre from cxc_personas where cedula =@cedula";
                return conn.Query<string>(query, new { cedula }).FirstOrDefault() ?? "";
            });
        }

        /// <summary>
        /// Consulta la lista de clientes disponibles.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_CuentasCorreccionesClientes_Listado(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select Cedula  as item ,Nombre as descripcion  from CxC_Personas";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }

        /// <summary>
        /// Consulta la lista de conceptos disponibles
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_CuentasCorreccionesConceptos_Listado(int CodEmpresa)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select cod_Concepto  as item ,Descripcion as descripcion  from CxC_Conceptos where Activo = 1";

                return conn.Query<DropDownListaGenericaModel>(query).ToList();
            });
        }
        /// <summary>
        ///  Consulta la lista de contratos disponibles
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="concepto"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_CuentasCorreccionesContratos_Listado(int CodEmpresa, string cedula, string concepto)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @" select  Cnt.cod_Contrato  as item ,Cnt.Descripcion as descripcion  
                             from CxC_Personas_Contratos Con 
                                inner join CxC_Contratos Cnt on Con.Cod_Contrato = Cnt.cod_contrato 
                             where Con.cedula = @cedula and 
                            Con.cod_contrato in(select cod_contrato from CxC_Conceptos_Contratos where cod_concepto =@concepto )
                            and Con.Activo = 1";

                return conn.Query<DropDownListaGenericaModel>(query, new { cedula, concepto }).ToList();
            });
        }

        /// <summary>
        /// Consulta la lista de pagadores disponibles para un contrato específico, según la cédula del cliente, el concepto y si el pagador está abierto o no.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="mCntPagadorAbierto"></param>
        /// <param name="cedula"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_CuentasCorreccionesPagadores_Listado(int CodEmpresa, bool mCntPagadorAbierto, string cedula, string contrato)
        {
            

            string query = mCntPagadorAbierto
                     ? @"select Cedula  as item ,Nombre as descripcion  
                                         from CxC_Personas where Rol_Pagador = 1"
                     : @"select PcP.Cedula_Pagador  as item ,Per.nombre as descripcion 
                                         from CxC_Personas_Contratos_Pagadores PcP
                                         inner join CxC_Personas Per on PcP.cedula_pagador = Per.cedula
                                         and PcP.cod_contrato =@contrato  and PcP.cedula =@cedula";

          
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {

                return conn.Query<DropDownListaGenericaModel>(query, new { contrato, cedula }).ToList();
            });
        }

        /// <summary>
        /// Consulta la lista de autorizados disponibles para un cliente específico, según la cédula del cliente.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CxC_CuentasCorreccionesAutorizados_Listado(int CodEmpresa, string cedula)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select PcP.CEDULA_AUTORIZADO  as item ,Per.nombre as descripcion  from CXC_PERSONAS_AUTORIZADOS PcP
                                  Inner join CxC_Personas Per on PcP.CEDULA_AUTORIZADO = Per.cedula
                                    and PcP.cedula =@cedula";

                return conn.Query<DropDownListaGenericaModel>(query, new { cedula }).ToList();
            });
        }

        /// <summary>
        ///  Consulta la información de un concepto específico.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="concepto"></param>
        /// <returns></returns>
        public ErrorDto<ConceptosData> CxC_CuentasCorreccionesConceptosDatos_Obtener(int CodEmpresa, string concepto)
        {
            return DbHelper.WithConn(_portalDB, CodEmpresa, conn =>
            {
                const string query = @"select C.Descripcion,C.Requiere_Contrato,Proceso_Descuento 
                                    from CxC_Conceptos C left join CxC_Personas P on C.PAGADOR_DEFAULT = P.cedula
                                     where C.cod_Concepto =@concepto";

                return conn.Query<ConceptosData>(query, new { concepto }).FirstOrDefault() ?? new ConceptosData();
            });
        }

        /// <summary>
        ///   Actualiza la cuenta contable.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="datos"></param>
        /// <returns></returns>
        public ErrorDto CxC_CuentasCorrecciones_Actualizar(int codEmpresa, string usuario, CuentaPorCobrarData datos)
        {

            const string sqlUpdate = @"     
               exec spCxC_Cuentas_Cambios @Operacion ,@usuario,@Notas,@Cedula,@Cedula_pagador,@Cedula_Autorizado,@Cod_Concepto,@Cod_Contrato
                        ,@Emitir_Banco,@Emitir_Cuenta,@Emitir_Tipo ";

            var result = DbHelper.ExecuteSingleQuery<ResultadoCuentaPorCobrarData>(
                _portalDB, codEmpresa, sqlUpdate, defaultValue: new ResultadoCuentaPorCobrarData(),
                parameters: new
                {
                    datos.Operacion,
                    usuario,
                    datos.Notas,
                    datos.Cedula,
                    datos.Cedula_Pagador,
                    datos.Cedula_Autorizado,
                    datos.Cod_Concepto,
                    datos.Cod_Contrato,
                    datos.Emitir_Banco,
                    datos.Emitir_Cuenta,
                    datos.Emitir_Tipo
                });

            if (result?.Result?.TipoDoc != "")
            {
                string TipoDoc = (result?.Result?.TipoDoc) ?? "";
                string NumDoc = (result?.Result?.NumDoc) ?? "";
                var resultado = CxC_CuentasCorrecciones_Reporte(codEmpresa, usuario, TipoDoc, NumDoc);
                return resultado.Code != -1
                  ? DbHelper.CreateOkResponse()
                  : DbHelper.ErrorResponse(resultado.Description ?? "Error al generar el reporte");

                 

            } 

            return DbHelper.CreateOkResponse();
        }

        /// <summary>
        /// Genera el reporte es requerido
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="tipoDocumento"></param>
        /// <param name="transaccion"></param>
        /// <returns></returns>
        public ErrorDto<object> CxC_CuentasCorrecciones_Reporte(int CodEmpresa, string usuario, string tipoDocumento="", string transaccion="")
        {
            var response = new ErrorDto<object>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };
            try
            {

                response = new MRecibos(_config).sbImprimeRecibo(CodEmpresa, transaccion, tipoDocumento, usuario);
                if (response.Code != -1)
                {
                    response.Description = "Proceso Aplicado Satisfactoriamente...";
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


    }
}