using Dapper;
using Microsoft.Data.SqlClient;
using PdfSharp.Drawing;
using PgxAPI.Models;
using PgxAPI.Models.CxP;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.FSL;
using PgxAPI.Models.ProGrX.Fondos;
using PgxAPI.Models.Security;
using System;
using System.Diagnostics.Contracts;
using System.Numerics;

namespace PgxAPI.DataBaseTier.ProGrX.Fondos
{
    public class frmFNDContratosDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 18; // Modulo de Fondo de Inversion
        private readonly MSecurityMainDb _Security_MainDB;
        private readonly mFNDFuncionesDB _mFNDFunciones;
        private readonly mProGrx_Main _mProGrxMain;
        private readonly mProGrX_AuxiliarDB _AuxiliarDB;

        private string pCuponFrecuencia = "";
        private string pPlazoInversionId = "";
        private string pCuponPaga = "";
        private string pCuponFrecuenciaId = "";

        public frmFNDContratosDB(IConfiguration? config)
        {
            _config = config;
            _Security_MainDB = new MSecurityMainDb(_config);
            _mFNDFunciones = new mFNDFuncionesDB(_config);
            _mProGrxMain = new mProGrx_Main(_config);
            _AuxiliarDB = new mProGrX_AuxiliarDB(_config);
        }

        #region General
        /// <summary>
        /// Metodo para obtener las listas genericas del formulario de contratos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="lista"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Contratos_Listas_Obtener(int CodEmpresa, string lista)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "";
                    switch (lista)
                    {
                        case "cboOperadora":
                            query = $@"select rtrim(descripcion) as 'descripcion',cod_operadora as 'item' from FND_Operadoras";
                            break;
                        case "cboVendedor":
                            query = $@"select rtrim(nombre) as 'descripcion',cod_vendedor as 'item' from FND_vendedores";
                            break;
                        case "cboBanco":
                            query = $@"select B.id_Banco as 'item', rtrim(B.descripcion) as 'descripcion' 
                                       from Tes_Bancos B inner join FND_BANCOS_X X on B.id_Banco = X.Id_Banco 
                                       where B.Estado = 'A' and (X.Cheque = 1 or X.Transferencia = 1)";
                            break;
                        case "cboCuponFrecuencia":
                            query = $@"SELECT ID_FRECUENCIACUPON as 'item' , dbo.fxSys_Cadena_Capitaliza ( CUPON ) as 'descripcion' 
                                         FROM FND_CDP_FRECUENCIACUPONES Where Estado = 1 Order by FRECUENCIA_DIAS asc";
                            break;
                        case "cboPlazoInversion":
                            query = $@"select ID_PLAZO as 'item', dbo.fxSys_Cadena_Capitaliza ( PLAZO ) as 'descripcion' 
                                         From FND_CDP_PLAZOS  Where Estado = 1  Order by PLAZO_DIAS  asc ";
                            break;
                    }

                    response.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
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
        /// metodo para obtener los datos del contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operadora"></param>
        /// <param name="xCodigo"></param>
        /// <param name="vCedula"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<ContratosModels> Fnd_Contratos_Obtener(int CodEmpresa, int operadora, string cod_plan, int contrato, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<ContratosModels>
            {
                Code = 0,
                Description = "Ok",
                Result = new ContratosModels()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $"exec spFnd_Contrato_Consulta {operadora}, '{cod_plan}', {contrato}, '{usuario}'";
                    response.Result = connection.Query<ContratosModels>(query).FirstOrDefault();

                    response.Result.aplicaBeneficiarios = fxAplicaBeneficiarios(CodEmpresa, cod_plan, operadora).Result;
                    
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
        /// Metodo para obtener los planes de la operadora
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operadora"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Contratos_PlanLista_Obtener(int CodEmpresa, int operadora, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //codigo temporal
                    string where = "and dbo.fxFnd_Seguridad_Acceso_Planes(@usuario, cod_operadora, cod_plan) = 1";

                    if(usuario.ToUpper() == "PEDRO")
                    {
                        where = "";
                    }

                    var query = $@"select cod_plan as item,descripcion from fnd_planes WHERE Cod_operadora= @operadora 
                                    {where} ";
                    response.Result = connection.Query<DropDownListaGenericaModel>(query, new
                    {
                        operadora = operadora,
                        usuario = usuario
                    }).ToList();


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
        /// Metodo para obtener los datos del plan
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operadora"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        public ErrorDto<ContratosPlanModels> Fnd_Contratos_Plan_Obtener(int CodEmpresa, int operadora, string plan)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<ContratosPlanModels>
            {
                Code = 0,
                Description = "Ok",
                Result = new ContratosPlanModels()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select  Descripcion, Monto_Minimo, Plazo_Minimo, cuenta_maestra, Inversion_minimo
			                      , Tipo_CDP, WEB_VENCE, isnull(PERMITE_GIRO_TERCEROS,0) as 'PlanPermiteGT', cod_moneda
			                      , PAGO_CUPONES, TASA_MARGEN_NEGOCIACION, dbo.MyGetDate() as 'FechaServidor',
			                        TIPO_DEDUC, PORC_DEDUC,  DEDUCIR_PLANILLA , SubCuentasMax
                                            from fnd_Planes 
                                            where cod_operadora = @operadora and cod_plan= @cod_plan ";
                    var parametros = new
                    {
                        operadora = operadora,
                        cod_plan = plan,
                    };

                    response.Result = connection.Query<ContratosPlanModels>(query, parametros).FirstOrDefault();

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
        /// Metodo para obtener los plazos de inversion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="codigo"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Contratos_InversionPlazos_Obtener(int CodEmpresa, string codigo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spFnd_Inversion_Plazos '{codigo}' ";
                    var lista = connection.Query(query)
                            .Select(r => new { IdX = (int)r.IdX, ItmX = (string)r.ItmX })
                            .ToList();

                    response.Result = lista.Select(r => new DropDownListaGenericaModel
                    {
                        item = r.IdX.ToString(),
                        descripcion = r.ItmX
                    }).ToList();
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
        /// Metodo para buscar los contratos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operadora"></param>
        /// <param name="plan"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<FndContratosListaData> Fnd_Contratos_Buscar(int CodEmpresa, int operadora, string plan,FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<FndContratosListaData>
            {
                Code = 0,
                Description = "Ok",
                Result = new FndContratosListaData
                {
                    total = 0,
                    lineas = new List<FndContratosModels>()
                }
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

        
                    var query = $@"select COUNT(COD_CONTRATO) from fnd_Contratos WHERE
                                      cod_operadora= @operadora and cod_plan= @codigo";
                    response.Result.total = connection.Query<int>(query, new { operadora = operadora, codigo = plan }).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " AND ( COD_PLAN LIKE '%" + filtros.filtro + "%' " +
                            " OR COD_CONTRATO LIKE '%" + filtros.filtro + "%' " +
                             " OR CEDULA LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "COD_CONTRATO";
                    }

                    query = $@"select COD_PLAN,COD_CONTRATO,CEDULA,FECHA_INICIO from ( 
                                select COD_PLAN,COD_CONTRATO,CEDULA,FECHA_INICIO from fnd_Contratos WHERE
                                    cod_operadora= @operadora and cod_plan= @codigo  
                                        {filtros.filtro} ) t
                                     ORDER BY {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")} 
                                        OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";

                    response.Result.lineas = connection.Query<FndContratosModels>(query, new { operadora = operadora, codigo = plan }).ToList();

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
        /// Metodo para enviar el correo de solicitud de contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operadora"></param>
        /// <param name="codigo"></param>
        /// <param name="contrato"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<string> Fnd_Contratos_Email_Enviar(int CodEmpresa, int operadora, string codigo, int contrato, string usuario)
        {
            var response = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = string.Empty
            };
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spFnd_Contrato_Notifica_Email {operadora}, '{codigo}', {contrato}, {usuario} ";
                    var result = connection.QueryFirstOrDefault<dynamic>(query);
                    if (result != null)
                    {
                        if (result.Pass == 1)
                        {
                            response.Result = "Correo de Solicitud de Contrato enviado a la persona.";
                        }
                        else
                        {
                            response.Result = result.Mensaje ?? "No se pudo enviar el correo.";
                        }
                    }
                    else
                    {
                        response.Code = -1;
                        response.Description = "El procedimiento no devolvió resultados.";
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = string.Empty;
            }
            return response;
        }


        /// <summary>
        /// Metodo para guardar el contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contrato"></param>
        /// <param name="gDestinos"></param>
        /// <returns></returns>
        public ErrorDto Fnd_Contratos_Guardar(int CodEmpresa, string usuario, FndCambios vCambios , ContratosModels contrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            response = fxVerificaDatos(CodEmpresa, contrato);

            if (response.Code == -1)
            {
                return response;
            }
            else
            {
                if (contrato.inversion > 0)
                {
                    pPlazoInversionId = contrato.plazo_id;

                    if (contrato.pago_cuponescdp == true)
                    {
                        pCuponPaga = "1";
                        pCuponFrecuencia = contrato.cupon_frecuencia;
                        pCuponFrecuenciaId = contrato.idcupon_frecuencia;
                    }
                    else
                    {
                        pCuponPaga = "0";
                        pCuponFrecuencia = "N";
                        pCuponFrecuenciaId = "Null";
                    }
                }
                else
                {
                    pCuponFrecuencia = "N";
                    pCuponFrecuenciaId = "Null";
                    pPlazoInversionId = "Null";
                    pCuponPaga = "0";
                }

                if (!contrato.isNew)
                {
                    response = actualizarContrato(CodEmpresa, usuario, vCambios, contrato);
                }
                else
                {
                    response = insertarContrato(CodEmpresa, usuario, contrato);
                }

                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    if (response.Code == 0)
                    {
                        if (contrato.tipo_cdp)
                        {
                            query = $@"exec spFndCDPCupones {contrato.cod_operadora}, '{contrato.cod_plan}', {contrato.cod_contrato},'{usuario}'";
                            connection.Execute(query);
                        }
                    }

                    //Guarda Destinos/Objetivos
                    //Guardado desde la tabla con id de contrato creado

                }


            }

            return response;

        }


        /// <summary>
        /// Metodo para borrar el contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operadora"></param>
        /// <param name="codigo"></param>
        /// <param name="contrato"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto Fnd_Contratos_Borrar(int CodEmpresa, int operadora, string codigo, int contrato, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"delete FND_contratos where cod_contrato = @contrato
			                          and cod_operadora= @operadora
			                          and cod_plan= @cod_plan";
                    connection.Execute(query, new
                    {
                        contrato = contrato,
                        operadora = operadora,
                        cod_plan = codigo
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Contrato: {contrato}  Plan: {codigo}  Oper: {operadora}",
                        Movimiento = "Borra - WEB",
                        Modulo = vModulo
                    });
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
        /// Metodo para obtener la cantidad de meses que tiene una frecuencia de cupon
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="CuponFrecuencia"></param>
        /// <returns></returns>
        public ErrorDto<int> Fnd_Contratos_FrecuenciaMeses_Obtener(int CodEmpresa, string CuponFrecuencia)
        {
            var response = new ErrorDto<int>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spFnd_Cupon_Frecuencia_Meses {CuponFrecuencia} ";
                    response.Result = connection.QueryFirstOrDefault<int>(query);
                   
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = 0;
            }
            return response;
        }


        /// <summary>
        /// Metodo para obtener las frecuencias de cupones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="plazo_id"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Contratos_spFnd_Cupon_Frecuencia(int CodEmpresa, string plazo_id,string plan)
        {
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spFnd_Cupon_Frecuencia {plazo_id}, '{plan}' ";
                    var lista = connection.Query(query)
                            .Select(r => new { IdX = (string)r.IdX, ItmX = (string)r.ItmX })
                            .ToList();
                    var result = lista.Select(r => new DropDownListaGenericaModel   
                    {
                        item = r.IdX,
                        descripcion = r.ItmX
                    }).ToList();

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
        /// metodo para obtener los dias o meses de un plazo de inversion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="plazo_inversion"></param>
        /// <param name="cboPlazo"></param>
        /// <returns></returns>
        public ErrorDto<int> Fnd_Contratos_spFnd_Inversion_Plazos_Dias(int CodEmpresa, int plazo_inversion, string cboPlazo)
        {
            var response = new ErrorDto<int>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spFnd_Inversion_Plazos_Dias  {plazo_inversion}";
                    var result = connection.QueryFirstOrDefault<dynamic>(query);

                    if (result == null)
                    {
                        // Manejo de error si no hay datos
                        response.Result = -1;
                        response.Description = "No se encontró información para el plazo indicado.";
                    }
                    else
                    {
                        // Equivale a Mid(cboPlazo.Text, 1, 1)
                        var tipoPlazo = cboPlazo.ToUpper();

                        int plazo;
                        if (tipoPlazo == "D")
                            plazo = (int)result.PLAZO_DIAS;
                        else
                            plazo = (int)result.PLAZO_MESES;

                        response.Result = plazo;
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = 0;
            }
            return response;
        }

        /// <summary>
        /// Metodo para obtener la tasa plus
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="xPlazo"></param>
        /// <param name="xTipo"></param>
        /// <param name="xPlan"></param>
        /// <param name="xOperadora"></param>
        /// <returns></returns>
        public ErrorDto<decimal> Fnd_Contratos_fxTasaPtsAdd(int CodEmpresa, long xPlazo, string xTipo, string xPlan, string xOperadora)
        {
            var response = new ErrorDto<decimal>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    if(xTipo == "M")
                    {
                        xPlazo = xPlazo * 30;
                    }

                    var query = $@"select tasa_base,UTILIZA_TBP,TIPO_CDP
		                               ,dbo.fxFNDTasaPlus(cod_operadora,cod_plan, @plazo ) as PlusTasa
		                                from fnd_planes where cod_operadora = @operadora and cod_plan = @plan";
                    var result = connection.QueryFirstOrDefault<dynamic>(query, new
                    {
                        operadora = xOperadora,
                        plan = xPlan,
                        plazo = xPlazo
                    });

                    if(result != null)
                    {
                        response.Result = (decimal)result.PlusTasa;
                    }

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = 0;
            }
            return response;
        }

        /// <summary>
        /// Metodo para obtener los socios
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<FndSociosListaData> Fnd_ContratosSocios_Obtener(int CodEmpresa, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<FndSociosListaData>
            {
                Code = 0,
                Description = "Ok",
                Result = new FndSociosListaData
                {
                    total = 0,
                    socios = new List<DropDownListaGenericaModel>()
                }
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {


                    var query = $@"select COUNT(cedula) from socios";
                    response.Result.total = connection.Query<int>(query).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " AND ( cedula LIKE '%" + filtros.filtro + "%' " +
                             " OR nombre LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "cedula";
                    }

                    query = $@"select cedula as 'item',nombre as 'descripcion' from socios WHERE 1=1
                                        {filtros.filtro} 
                                     ORDER BY {filtros.sortField} {(filtros.sortOrder == 0 ? "DESC" : "ASC")} 
                                        OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";

                    response.Result.socios = connection.Query<DropDownListaGenericaModel>(query).ToList();

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

        #endregion

        #region Complementario
        /// <summary>
        /// Metodo para obtener las cuentas bancarias
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="cod_banco"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Fnd_Contratos_CuentasBancarias_Obtener(int CodEmpresa, string cedula, int cod_banco)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<DropDownListaGenericaModel>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spSys_Cuentas_Bancarias '{cedula}', {cod_banco}, 1 ";
                    var lista = connection.Query(query)
                            .Select(r => new { IdX = (string)r.IdX, ItmX = (string)r.ItmX })
                            .ToList();

                    response.Result = lista.Select(r => new DropDownListaGenericaModel
                    {
                        item = r.IdX,
                        descripcion = r.ItmX
                    }).ToList();
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

        #endregion

        #region Destinos

        /// <summary>
        /// Metodo para obtener los destinos del contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pOperadora"></param>
        /// <param name="pPlan"></param>
        /// <param name="pContrato"></param>
        /// <returns></returns>
        public ErrorDto<List<FndContratoDestinoData>> Fnd_Contratos_Destinos_Obtener(int CodEmpresa, int pOperadora, string pPlan, long pContrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<FndContratoDestinoData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<FndContratoDestinoData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select D.cod_destino,D.descripcion,A.cod_contrato 
	                                 from fnd_destinos D left join fnd_contratos_destinos A on D.cod_destino = A.cod_destino
	                                 and A.cod_operadora = @operadora
	                                 and A.cod_plan = @plan and A.cod_contrato = @contrato
	                                 Where D.cod_destino in(select cod_destino from fnd_planes_destinos where cod_plan = @plan )";
                    response.Result = connection.Query<FndContratoDestinoData>(query, new
                    {
                        operadora = pOperadora,
                        plan = pPlan,
                        contrato = pContrato
                    }).ToList();

                    if(response.Result == null || response.Result.Count == 0)
                    {
                        if(pContrato > 0)
                        {
                            query = $@"exec spFnd_Contrato_Destinos_List {pOperadora}, '{pPlan}', {pContrato}";
                        }
                        else
                        {
                            query = $@"exec spFnd_Contrato_Destinos_List {pOperadora}, '{pPlan}', 0";
                        }

                        response.Result = connection.Query<FndContratoDestinoData>(query).ToList();
                    }

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
        /// Metodo para guardar los destinos del contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="destino"></param>
        /// <returns></returns>
        public ErrorDto Fnd_Contratos_Destinos_Guardar(int CodEmpresa, FndContratoDestinoData destino)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {

                    //valida que existe id_registro
                    var query = $@"Select count(1) from FND_CONTRATOS_DESTINOS_AHORRO 
                                    where ID_REGISTRO = @ID_REGISTRO AND COD_PLAN = @COD_PLAN AND COD_CONTRATO = @COD_CONTRATO ";
                    var existe = connection.QueryFirstOrDefault<int>(query, new
                    {
                        ID_REGISTRO = destino.id_registro,
                        COD_PLAN = destino.cod_plan,
                        COD_CONTRATO = destino.cod_contrato
                    });

                    if(existe == 0)
                    {
                        query = $@"INSERT INTO FND_CONTRATOS_DESTINOS_AHORRO (
                                    ID_DESTINO,
                                    COD_PLAN,
                                    COD_CONTRATO,
                                    OBSERVACIONES,
                                    FEC_REGISTRO,
                                    USU_REGISTRO
                                )
                                VALUES (
                                    @ID_DESTINO,
                                    @COD_PLAN,
                                    @COD_CONTRATO,
                                    @OBSERVACIONES,
                                    dbo.MyGetdate(),
                                    @USU_REGISTRO
                                )";

                        connection.Execute(query, new
                        {
                            ID_DESTINO = destino.id_destino,
                            COD_PLAN = destino.cod_plan,
                            COD_CONTRATO = destino.cod_contrato,
                            OBSERVACIONES = destino.observaciones,
                            USU_REGISTRO = destino.usu_registro
                        });
                    }
                    else
                    {
                        query = $@"UPDATE FND_CONTRATOS_DESTINOS_AHORRO
                                SET 
                                    OBSERVACIONES = @OBSERVACIONES,
                                    FEC_MODIFICA = dbo.MyGetdate(),
                                    USU_MODIFICA = @USU_MODIFICA
                                WHERE ID_REGISTRO = @ID_REGISTRO";

                        connection.Execute(query, new
                            {
                            OBSERVACIONES = destino.observaciones,
                            USU_MODIFICA = destino.usu_registro,
                            ID_REGISTRO = destino.id_registro
                        });

                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        public ErrorDto Fnd_Contratos_DestinosLista_Guardar(int CodEmpresa, bool chkItem, FndContratoDestinoData destino)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "";
                    if (chkItem)
                    {
                        query = $@"INSERT INTO fnd_contratos_destinos (
                                    cod_plan,
                                    cod_operadora,
                                    cod_contrato,
                                    cod_destino,
                                    registro_usuario,
                                    registro_fecha
                                )
                                VALUES (
                                    @cod_plan,
                                    @cod_operadora,
                                    @cod_contrato,
                                    @cod_destino,
                                    @registro_usuario,
                                    @registro_fecha
                                )";

                        connection.Execute(query, new   
                            {
                            cod_plan = destino.cod_plan,
                            cod_operadora = destino.cod_operadora,
                            cod_contrato = destino.cod_contrato,
                            cod_destino = destino.id_destino,
                            registro_usuario = destino.usu_registro,
                            registro_fecha = DateTime.Now
                        });

                        _Security_MainDB.Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = destino.usu_modifica,
                            DetalleMovimiento = $"Asignación Destino: {destino.id_registro}  P.: {destino.cod_plan}  Cnt:  {destino.cod_contrato}",
                            Movimiento = "Aplica - WEB",
                            Modulo = vModulo
                        });
                    }
                    else
                    {
                        query = $@"DELETE FROM fnd_contratos_destinos 
                                    WHERE cod_plan = @cod_plan
                                    AND cod_operadora = @cod_operadora
                                    AND cod_contrato = @cod_contrato
                                    AND cod_destino = @cod_destino";
                        connection.Execute(query, new
                            {
                            cod_plan = destino.cod_plan,
                            cod_operadora = destino.cod_operadora,
                            cod_contrato = destino.cod_contrato,
                            cod_destino = destino.id_destino
                        });

                        _Security_MainDB.Bitacora(new BitacoraInsertarDto
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = destino.usu_modifica,
                            DetalleMovimiento = $"Asignación Destino: {destino.id_registro}  P.: {destino.cod_plan}  Cnt:  {destino.cod_contrato}",
                            Movimiento = "Elimina - WEB",
                            Modulo = vModulo
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        #endregion

        #region Beneficiarios

        /// <summary>
        /// Metodo para obtener los beneficiarios del contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pOperadora"></param>
        /// <param name="pPlan"></param>
        /// <param name="pContrato"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<FndContratoBeneficiariosData>> Fnd_Contratos_Beneficiarios_Obtener(int CodEmpresa, int pOperadora, string pPlan, long pContrato, string cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<FndContratoBeneficiariosData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<FndContratoBeneficiariosData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"Select CedulaBn,Nombre,Porcentaje,parentesco, p.Descripcion as parentesco_desc 
                                        From FND_CONTRATOS_BENEFICIARIOS left join sys_Parentescos p on p.cod_Parentesco = parentesco 
                                        where 
			                            Cedula = @cedula  and cod_contrato = @contrato
			                            and cod_operadora = @operadora
			                            and cod_plan= @plan
			                            and p.activo = 1";
                    response.Result = connection.Query<FndContratoBeneficiariosData>(query, new
                    {
                        operadora = pOperadora,
                        plan = pPlan,
                        contrato = pContrato,
                        cedula = cedula
                    }).ToList();

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

        #endregion

        #region SubCuentas

        /// <summary>
        /// Metodo para obtener las subcuentas del contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pOperadora"></param>
        /// <param name="pPlan"></param>
        /// <param name="pContrato"></param>
        /// <returns></returns>
        public ErrorDto<List<FndContratoSubCuentasData>> Fnd_Contratos_SubCuentas_Obtener(int CodEmpresa, int pOperadora, string pPlan, long pContrato, string cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<FndContratoSubCuentasData>>
            {
                Code = 0,
                Description = "OK",
                Result = new List<FndContratoSubCuentasData>()
            };
            try
            {
                //aportes+rendimiento as Acumulado,parentesco
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select idx,cedula,nombre,cuota,0
				                     from fnd_subCuentas where cod_operadora = @operadora
				                     and cod_plan = @plan and cod_contrato = @contrato";
                    response.Result = connection.Query<FndContratoSubCuentasData>(query, new
                    {
                        operadora = pOperadora,
                        plan = pPlan,
                        contrato = pContrato
                    }).ToList();

                    int consec = fxSubCuentaContrato(CodEmpresa, pOperadora, pPlan, pContrato).Result;

                    //insertar en la ultima linea una cuenta armada
                    response.Result.Add(new FndContratoSubCuentasData
                    {
                        idx = 0,
                        cedula = $"{cedula.Trim()}-{consec.ToString("00")}",
                        nombre = "0",
                        cuota = 0,
                    });

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
        /// Metodo para obtener el siguiente numero de subcuenta
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pOperadora"></param>
        /// <param name="pPlan"></param>
        /// <param name="pContrato"></param>
        /// <returns></returns>
        public ErrorDto<int> fxSubCuentaContrato(int CodEmpresa, int pOperadora, string pPlan, long pContrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<int>
            {
                Code = 0,
                Description = "OK",
                Result = 0
            };
            try
            {
                //aportes+rendimiento as Acumulado,parentesco
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select isnull(count(*),0) + 1 as Ultimo from fnd_subCuentas
                                        where cod_operadora = @operadora
                                        and cod_plan = @plan and cod_contrato = @contrato";
                    response.Result = connection.Query<int>(query, new
                    {
                        operadora = pOperadora,
                        plan = pPlan,
                        contrato = pContrato
                    }).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = 0;
            }
            return response;
        }

        /// <summary>
        /// Metodo para guardar la subcuenta
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="subCuenta"></param>
        /// <returns></returns>
        public ErrorDto Fnd_Contratos_SubCuentas_Guardar(int CodEmpresa, string usuario, FndContratoSubCuentasData subCuenta)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                if (subCuenta.isNew)
                {
                    response = Fnd_Contratos_SubCuentas_Insertar(CodEmpresa, usuario, subCuenta);
                }
                else
                {
                    response = Fnd_Contratos_SubCuentas_Actualizar(CodEmpresa, usuario, subCuenta);
                }
                if (response.Code == 0)
                {
                    response = sbActualizaCuotaContrato(CodEmpresa, usuario, subCuenta);
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
        /// Metodo para insertar la subcuenta
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="subCuenta"></param>
        /// <returns></returns>
        private ErrorDto Fnd_Contratos_SubCuentas_Insertar(int CodEmpresa, string usuario, FndContratoSubCuentasData subCuenta)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"INSERT INTO fnd_subCuentas (
                                        cod_operadora,
                                        cod_plan,
                                        cod_contrato,
                                        idX,
                                        cedula,
                                        nombre,
                                        cuota,
                                        estado,
                                        aportes,
                                        rendimiento,
                                        telefono1,
                                        telefono2,
                                        notas,
                                        email,
                                        apto_postal,
                                        direccion,
                                        parentesco,
                                        cod_beneficiario
                                    )
                                    VALUES (
                                        @cod_operadora,
                                        @cod_plan,
                                        @cod_contrato,
                                        @idX,
                                        @cedula,
                                        @nombre,
                                        @cuota,
                                        @estado,
                                        @aportes,
                                        @rendimiento,
                                        @telefono1,
                                        @telefono2,
                                        @notas,
                                        @email,
                                        @apto_postal,
                                        @direccion,
                                        @parentesco,
                                        @cod_beneficiario
                                    );";
                   connection.Execute(query, new
                    {
                        cod_operadora = subCuenta.cod_operadora,
                        cod_plan = subCuenta.cod_plan,
                        cod_contrato = subCuenta.cod_contrato,
                        idX = subCuenta.idx,
                        cedula = subCuenta.cedula,
                        nombre = subCuenta.nombre,
                        cuota = subCuenta.cuota,
                        estado = "A",
                        aportes = 0,
                        rendimiento = 0,
                        telefono1 = "",
                        telefono2 = "",
                        notas = "",
                        email = "",
                        apto_postal = "",
                        direccion = "",
                        parentesco = "",
                        cod_beneficiario = 0
                    });

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"SubCuenta: {subCuenta.cedula} Plan: {subCuenta.cod_plan} Contrato: {subCuenta.cod_contrato}",
                        Movimiento = "Registra - WEB",
                        Modulo = vModulo
                    });

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
        /// Metodo para actualizar la subcuenta
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="subCuenta"></param>
        /// <returns></returns>
        private ErrorDto Fnd_Contratos_SubCuentas_Actualizar(int CodEmpresa, string usuario, FndContratoSubCuentasData subCuenta)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "update fnd_subCuentas set cedula = @cedula, nombre = @nombre, cuota = @cuota " +
                        "where idx = @idx  and cod_operadora = @operadora  and cod_plan = @plan and cod_contrato = @contrato ";
                    connection.Execute(query, new
                        {
                        idx = subCuenta.idx,
                        cedula = subCuenta.cedula,
                        nombre = subCuenta.nombre,
                        cuota = subCuenta.cuota,
                        operadora = subCuenta.cod_operadora,
                        plan = subCuenta.cod_plan,
                        contrato = subCuenta.cod_contrato
                    });

                    sbGuardaCambios(CodEmpresa, subCuenta.cod_operadora, subCuenta.cod_plan, subCuenta.cod_contrato, usuario, 04, $@"Modifica a SubCuenta: {subCuenta.cedula}");

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"SubCuenta: {subCuenta.cedula} Plan: {subCuenta.cod_plan} Contrato: {subCuenta.cod_contrato}",
                        Movimiento = "Modifica - WEB",
                        Modulo = vModulo
                    });
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
        /// Metodo para actualizar la cuota del contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="subCuenta"></param>
        /// <returns></returns>
        private ErrorDto sbActualizaCuotaContrato(int CodEmpresa, string usuario, FndContratoSubCuentasData subCuenta)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "OK"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spFnd_SubCuentas_Maestro_Update {subCuenta.cod_operadora}, '{subCuenta.cod_plan}', {subCuenta.cod_contrato}, '{usuario}' ";
                    connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        #endregion

        #region Retiros

        /// <summary>
        /// Metodo para obtener los retiros / liquidaciones del contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operadora"></param>
        /// <param name="plan"></param>
        /// <param name="contrato"></param>
        /// <param name="filtros"></param>
        /// <returns></returns>
        public ErrorDto<FndContratosLiquidacionesListaData> Fnd_Contratos_Retiros_Obtener(int CodEmpresa, int operadora, string plan, int contrato, FiltrosLazyLoadData filtros)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<FndContratosLiquidacionesListaData>
            {
                Code = 0,
                Description = "Ok",
                Result = new FndContratosLiquidacionesListaData
                {
                    total = 0,
                    lineas = new List<FndContratosLiquidacionesModels>()
                }
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select count(consec)
			                        from fnd_liquidacion where cod_operadora = @codOperadora
			                        and cod_plan = @codPlan and cod_contrato = @codContrato ";
                    response.Result.total = connection.Query<int>(query, new {
                        codOperadora = operadora,
                        codPlan = plan,
                        codContrato = contrato
                    }).FirstOrDefault();

                    if (filtros.filtro != null)
                    {
                        filtros.filtro = " AND ( consec LIKE '%" + filtros.filtro + "%' " +
                            " OR usuario LIKE '%" + filtros.filtro + "%' " +
                             " OR fecha LIKE '%" + filtros.filtro + "%' ) ";
                    }

                    if (filtros.sortField == "" || filtros.sortField == null)
                    {
                        filtros.sortField = "consec";
                    }

                    query = $@"select consec,fecha,aportes_liq,rendi_liq,estado ,usuario
			                        from fnd_liquidacion  WHERE
                                     cod_operadora = @codOperadora
			                        and cod_plan = @codPlan and cod_contrato = @codContrato
                                        {filtros.filtro} 
                                     ORDER BY {filtros.sortField} {(filtros.sortOrder == 0 ? "ASC": "DESC")} 
                                        OFFSET {filtros.pagina} ROWS 
                                         FETCH NEXT {filtros.paginacion} ROWS ONLY ";

                    response.Result.lineas = connection.Query<FndContratosLiquidacionesModels>(query, new {
                        codOperadora = operadora,
                        codPlan = plan,
                        codContrato = contrato
                    }).ToList();

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

        #endregion

        #region Cupones
        /// <summary>
        /// Metodo para obtener los cupones del contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pOperadora"></param>
        /// <param name="pPlan"></param>
        /// <param name="pContrato"></param>
        /// <returns></returns>
        public ErrorDto<List<FndContratosCuponesData>> Fnd_Contratos_Cupones_Obtener(int CodEmpresa, int pOperadora, string pPlan, long pContrato)
        {
            return _mFNDFunciones.sbFnd_Contratos_Cupones(CodEmpresa, pOperadora, pPlan, pContrato);
        }

        #endregion

        #region Bitacora
        /// <summary>
        /// Metodo para obtener la bitacora de cambios de un contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="pOperadora"></param>
        /// <param name="pPlan"></param>
        /// <param name="pContrato"></param>
        /// <returns></returns>
        public ErrorDto<List<FndContratoBitacoraData>> Fnd_Contratos_Bitacora_Obtener(int CodEmpresa, int pOperadora, string pPlan, long pContrato)
        {
            return _mFNDFunciones.sbFnd_Contratos_Bitacora(CodEmpresa, pOperadora, pPlan, pContrato);
        }

        #endregion

        #region funciones privadas
        /// <summary>
        /// Metodo para verificar los datos antes de guardar
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contrato"></param>
        /// <param name="gDestinos"></param>
        /// <returns></returns>
        private ErrorDto fxVerificaDatos(int CodEmpresa, ContratosModels contrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                string mensaje = "";
                using var connection = new SqlConnection(stringConn);
                {
                    //Selecciona el numero de contraos activos por persona por plan
                    var query = $@"select num_contratos_activos from fnd_planes
                                        where cod_operadora = @operadora
                                        and cod_plan = @cod_plan";
                    int num_contratos_activos = connection.Query<int>(query, new { operadora = ((dynamic)contrato).cod_operadora, cod_plan = ((dynamic)contrato).cod_plan }).FirstOrDefault();

                    query = $@"select isnull(count(*),0) as existe from fnd_contratos
                                where cod_operadora = @operadora
                                and estado = 'A' and cedula = @cedula and cod_plan = @cod_plan";
                    int existe = connection.Query<int>(query, new { operadora = ((dynamic)contrato).cod_operadora, cod_plan = ((dynamic)contrato).cod_plan, cedula = ((dynamic)contrato).cedula }).FirstOrDefault();

                    if (existe >= num_contratos_activos)
                    {
                        mensaje += " - Esta persona ha superado el número máximo de contratos activos en este plan... \n";
                    }

                    query = $@"exec dbo.spFND_ValidaEstados '{((dynamic)contrato).cod_plan}', {((dynamic)contrato).cod_operadora}, '{((dynamic)contrato).cedula}'";
                    var encontrado = connection.Query<int>(query).FirstOrDefault();
                    if (encontrado == 0)
                    {
                        mensaje += " - El estado de esta persona no aplica para en este plan o el Plan se encuentra inactivo...\n";
                    }

                    query = $@"select dbo.fxFnd_Contrato_Valida_Plazo(@operadora ,@cod_plan, @plazo ) as 'Plazo_Valida'
                                   , dbo.fxFnd_Seguridad_Acceso_Planes(@usuario, @operadora, @cod_plan) as 'Acceso_Valida'
                                   , (select  count(*) From FND_PLANES_DESTINOS_AHORRO Where cod_Plan = @cod_plan and activo = 1) as 'Destinos'";
                    ValidaContratos valida = connection.Query<ValidaContratos>(query, new
                    {
                        operadora = ((dynamic)contrato).cod_operadora,
                        cod_plan = ((dynamic)contrato).cod_plan,
                        plazo = ((dynamic)contrato).plazo_inversion,
                        usuario = ((dynamic)contrato).usuario
                    }).FirstOrDefault();

                    if (valida.plazo_valida == 0)
                    {
                        mensaje += " - El Plazo se encuentra fuera del Rango Permitido por el Plan...\n";
                    }

                    if (valida.acceso_valida == 0)
                    {
                        mensaje += " - El usuario no tiene Autorización para gestionar este Plan...\n";
                    }

                    long pDestinoIndicados = 0;

                    //consulta destinos
                    List<FndContratoDestinoData> destinos = Fnd_Contratos_Destinos_Obtener(CodEmpresa, contrato.cod_operadora,contrato.cod_plan,contrato.cod_contrato).Result;

                    if (valida.destinos > 0)
                    {
                        pDestinoIndicados = 0;
                        foreach (FndContratoDestinoData item in destinos)
                        {
                            if (item.id_registro != null || item.id_registro > 0)
                            {
                                pDestinoIndicados = pDestinoIndicados + 1;
                            }
                        }

                        if (pDestinoIndicados == 0)
                        {
                            mensaje += " - No ha Indicado Ningún Destino/Objetivo para este Plan...\n";
                        }
                    }

                    if (string.IsNullOrEmpty(contrato.cod_plan))
                    {
                        mensaje += " - Indique el Plan...\n";
                    }

                    if (string.IsNullOrEmpty(contrato.cedula))
                    {
                        mensaje += " - Especifique la persona ...\n";
                    }

                    if (!contrato.porc_deduc.HasValue)
                    {
                        mensaje += " - El Porcentaje de deducción no es válido...\n";
                    }

                    if (!contrato.monto.HasValue)
                    {
                        mensaje += " - La cuota especificada no es válida...\n";
                    }

                    if (!contrato.inversion.HasValue)
                    {
                        mensaje += " - La inversión especificada no es válida...\n";
                    }


                    if (!contrato.plazo.HasValue)
                    {
                        mensaje += " - El plazo especificado no es válido...\n";
                    }


                    if (!contrato.inc_anual.HasValue)
                    {
                        mensaje += " - El % de Incremento anual no es válido...\n";
                    }

                    if (!contrato.capexc.HasValue)
                    {
                        mensaje += " - El % de Capitalización no es válido...\n";
                    }


                    if (contrato.tipo_deduc == "P" && mensaje.Length == 0)
                    {
                        if (contrato.porc_deduc > 100 || contrato.porc_deduc < 0)
                        {
                            mensaje += "El Porcentaje de Deducción no es válido!\n";
                        }
                    }

                    if(contrato.tasa_referencia == null)
                    {
                        contrato.tasa_referencia = 0;
                    }

                    if(contrato.mTipoDeduc == "P" && mensaje.Length == 0)
                    {
                        if(contrato.porcentaje > 100 || contrato.porcentaje < 0)
                        {
                            mensaje += "El Porcentaje de Deducción no es válido!\n";
                        }
                    }

                    //Verifica Montos Minimos
                    if (mensaje.Length == 0)
                    {
                        query = $@"select PLAZO_MINIMO * case when PLAZO_TIPO = 'M' then 30 else 1 end as 'Plazo_Minimo', MONTO_MINIMO, INVERSION_MINIMO
                                   from fnd_Planes where cod_operadora = @operadora
                                   and cod_plan = @cod_plan";
                        ValidaContratos valida2 = connection.Query<ValidaContratos>(query, new
                        {
                            operadora = contrato.cod_operadora,
                            cod_plan = contrato.cod_plan
                        }).FirstOrDefault();

                        if (contrato.plazo_id == "D")
                        {
                            if (contrato.plazo < valida2.plazo_minimo)
                            {
                                mensaje += $@" - El Plazo no cumple con el plazo mínimo permitido ({valida2.plazo_minimo})...\n";
                            }
                        }

                        if (contrato.plazo_id == "M")
                        {
                            if (contrato.plazo * 30 < valida2.plazo_minimo)
                            {
                                mensaje += $@" - El Plazo no cumple con el plazo mínimo permitido ({valida2.plazo_minimo})...\n";
                            }
                        }

                        if (contrato.tipo_deduc == "M")
                        {
                            if (contrato.monto < valida2.monto_minimo)
                            {
                                mensaje += $@" - El monto es menor al mínimo permitido...";
                            }
                            if (contrato.inversion < valida2.inversion_minimo)
                            {
                                mensaje += $@" - El monto de la INVERSIÓN es menor al mínimo permitido...";
                            }
                        }

                        if (mensaje.Length == 0)
                        {
                            response.Code = 0;
                            response.Description = "Ok";
                        }
                        else
                        {
                            response.Code = -1;
                            response.Description = mensaje;
                        }

                    }

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
        /// Metodo para insertar el contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        private ErrorDto insertarContrato(int CodEmpresa,string usuario, ContratosModels contrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    contrato.cod_contrato = fxConsecutivoContrato(CodEmpresa, contrato.cod_operadora, contrato.cod_plan).Result;

                    var query = $@"INSERT INTO FND_Contratos (
                                    Cod_operadora,
                                    Cod_plan,
                                    Cod_Contrato,
                                    Cedula,
                                    Cod_Vendedor,
                                    Tipo_Deduc,
                                    PORC_DEDUC,
                                    Estado,
                                    Fecha_Inicio,
                                    Plazo,
                                    Monto,
                                    Renueva,
                                    Inc_Anual,
                                    Inc_Tipo,
                                    Ind_comision,
                                    Cod_Banco,
                                    Cuenta_Ahorros,
                                    Tipo_Pago,
                                    CapExc,
                                    rend_corte,
                                    rend_saldo,
                                    fecha_corte,
                                    usuario,
                                    albacea_cedula,
                                    albacea_nombre,
                                    plazo_tipo,
                                    inversion,
                                    tasa_referencia,
                                    Tasa_Tipo,
                                    Tasa_PtsAdd,
                                    Cupon_Frecuencia,
                                    Cupon_Proximo,
                                    Cupon_Consec,
                                    ind_deduccion,
                                    PERMITE_GIRO_TERCEROS,
                                    IDCUPON_FRECUENCIA,
                                    PAGO_CUPONESCDP,
                                    ID_PER_TASA
                                )
                                VALUES (
                                    @cod_operadora,
                                    @cod_plan,
                                    @cod_contrato,
                                    @cedula,
                                    @cod_vendedor,
                                    @tipo_deduc,
                                    @porc_deduc,
                                    @estado,
                                    @fecha_inicio,
                                    @plazo,
                                    @monto,
                                    @renueva,
                                    @inc_anual,
                                    @inc_tipo,
                                    @ind_comision,
                                    @cod_banco,
                                    @cuenta_ahorros,
                                    @tipo_pago,
                                    @cap_exc,
                                    @rend_corte,
                                    @rend_saldo,
                                    @fecha_corte,
                                    @usuario,
                                    @albacea_cedula,
                                    @albacea_nombre,
                                    @plazo_tipo,
                                    @inversion,
                                    @tasa_referencia,
                                    @tasa_tipo,
                                    @tasa_ptsadd,
                                    @cupon_frecuencia,
                                    @cupon_proximo,
                                    @cupon_consec,
                                    @ind_deduccion,
                                    @permite_giro_terceros,
                                    @idcupon_frecuencia,
                                    @pago_cuponescdp,
                                    dbo.fxFnd_ReglaId_Tasa(@cod_plan, @fecha_inicio)
                                );";

                    var result = connection.Execute(query, new
                    {
                        cod_operadora = ((dynamic)contrato).cod_operadora,
                        cod_plan = ((dynamic)contrato).cod_plan,
                        cod_contrato = contrato.cod_contrato,
                        cedula = ((dynamic)contrato).cedula,
                        cod_vendedor = contrato.cod_vendedor,
                        tipo_deduc = contrato.tipo_deduc,
                        porc_deduc = contrato.porc_deduc,
                        estado = "A",
                        fecha_inicio = contrato.fecha_inicio,
                        plazo = contrato.plazo,
                        monto = contrato.monto,
                        renueva = contrato.renueva,
                        inc_anual = contrato.inc_anual,
                        inc_tipo = contrato.inc_tipo,
                        ind_comision = 0,
                        cod_banco = contrato.cod_banco,
                        cuenta_ahorros = contrato.cuenta_ahorros,
                        tipo_pago = contrato.tipo_pago,
                        cap_exc = contrato.capexc,
                        rend_corte = 0,
                        rend_saldo = 0,
                        fecha_corte = contrato.fecha_corte,
                        usuario = contrato.usuario,
                        albacea_cedula = contrato.albacea_cedula,
                        albacea_nombre = contrato.albacea_nombre,
                        plazo_tipo = contrato.plazo_id,
                        inversion = contrato.inversion,
                        tasa_referencia = contrato.tasa_referencia,
                        tasa_tipo = contrato.tasa_tipo,
                        tasa_ptsadd = contrato.tasa_ptsadd,
                        cupon_frecuencia = pCuponFrecuencia,
                        cupon_proximo = contrato.cupon_proximo,
                        cupon_consec = 0,
                        ind_deduccion = contrato.ind_deduccion,
                        permite_giro_terceros = contrato.permite_giro_terceros,
                        idcupon_frecuencia = pCuponFrecuenciaId,
                        pago_cuponescdp = pCuponPaga
                    });


                    if(result == 0)
                    {
                        response.Code = -1;
                        response.Description = "No se pudo insertar el contrato...";
                        return response;
                    }

                    //inserta los destinos
                    if(fxAplicaBeneficiarios(CodEmpresa,contrato.cod_plan, contrato.cod_operadora).Result)
                    {
                        if(fxBeneficiariosNoIncluidos(CodEmpresa, contrato.cod_plan,contrato.cod_operadora, contrato.cod_contrato))
                        {
                            response.Code = -1;
                            response.Description = "No estan incluidos los beneficiarios o el porcentaje es inferior al 100%...Por Favor Incluirlos";
                        }
                    }

                    //Bitacora
                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Contrato: {contrato.cod_contrato}  Plan: {contrato.cod_plan}  Oper: {contrato.cod_operadora}",
                        Movimiento = "Registra - WEB",
                        Modulo = vModulo
                    });

                    //Bitacora de cambios
                    _mProGrxMain.SbSIFRegistraTags( new SifRegistraTagsRequestDto
                            {
                                Codigo = contrato.cod_plan,
                                Tag = "S09",
                                Observacion = "Fondos",
                                Documento = contrato.cod_contrato.ToString(),
                                Modulo = "FND",
                                Llave_01 = contrato.cod_plan,
                                Llave_02 = contrato.cod_contrato.ToString(),
                                Llave_03 = contrato.cedula,
                                Usuario = usuario
                            }
                        );
                    sbGuardaCambios(CodEmpresa, contrato.cod_operadora, contrato.cod_plan, contrato.cod_contrato, usuario, 05, $@"Mensualidad: {contrato.monto} ¦ Inversión: {contrato.inversion}");


                    //insert trazabilidad.
                    string Consecutivo = $"{contrato.cod_contrato}-{contrato.cod_plan.Trim()}";
                    sbTrazabilidad_Inserta(CodEmpresa, "04", Consecutivo, contrato.cod_contrato.ToString(), usuario);
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
        /// Metodo para actualizar el contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        private ErrorDto actualizarContrato(int CodEmpresa, string usuario, FndCambios vCambios , ContratosModels contrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"UPDATE FND_Contratos SET
                                        cod_Vendedor         = @cod_vendedor,
                                        Plazo                = @plazo,
                                        fecha_corte          = @fecha_corte,
                                        Monto                = @monto,
                                        Renueva              = @renueva,
                                        Inc_Anual            = @inc_anual,
                                        Inc_Tipo             = @inc_tipo,
                                        Cod_Banco            = @cod_banco,
                                        Cuenta_Ahorros       = @cuenta_ahorros,
                                        tipo_Pago            = @tipo_pago,
                                        CapExc               = @cap_exc,
                                        albacea_Cedula       = @albacea_cedula,
                                        albacea_nombre       = @albacea_nombre,
                                        plazo_tipo           = @plazo_tipo,
                                        inversion            = @inversion,
                                        tasa_referencia      = @tasa_referencia,
                                        modifica_fecha       = dbo.MyGetdate(),
                                        modifica_usuario     = @modifica_usuario,
                                        cupon_frecuencia     = @cupon_frecuencia,
                                        cupon_proximo        = @cupon_proximo,
                                        ind_deduccion        = @ind_deduccion,
                                        PERMITE_GIRO_TERCEROS = @permite_giro_terceros,
                                        Tipo_Deduc           = @tipo_deduc,
                                        PORC_DEDUC           = @porc_deduc,
                                        IDCUPON_FRECUENCIA   = @idcupon_frecuencia,
                                        PAGO_CUPONESCDP      = @pago_cuponescdp,
                                        ID_PER_TASA          = dbo.fxFnd_ReglaId_Tasa(@cod_plan, @fecha_inicio)
                                    WHERE 
                                        cod_operadora        = @cod_operadora AND
                                        cod_plan             = @cod_plan AND
                                        cod_Contrato         = @cod_contrato;";
                    
                    var result = connection.Execute(query, new
                    {
                        cod_vendedor = contrato.cod_vendedor,
                        plazo = contrato.plazo,
                        fecha_corte = contrato.fecha_corte,
                        monto = contrato.monto,
                        renueva = contrato.renueva,
                        inc_anual = contrato.inc_anual,
                        inc_tipo = contrato.inc_tipo,
                        cod_banco = contrato.cod_banco,
                        cuenta_ahorros = contrato.cuenta_ahorros,
                        tipo_pago = contrato.tipo_pago,
                        cap_exc = contrato.capexc,
                        albacea_cedula = contrato.albacea_cedula,
                        albacea_nombre = contrato.albacea_nombre,
                        plazo_tipo = contrato.plazo_id,
                        inversion = contrato.inversion,
                        tasa_referencia = contrato.tasa_referencia,
                        modifica_usuario = contrato.usuario,
                        cupon_frecuencia = pCuponFrecuencia,
                        cupon_proximo = contrato.cupon_proximo,
                        ind_deduccion = contrato.ind_deduccion,
                        permite_giro_terceros = contrato.permite_giro_terceros,
                        tipo_deduc = contrato.tipo_deduc,
                        porc_deduc = contrato.porc_deduc,
                        idcupon_frecuencia = pCuponFrecuenciaId,
                        pago_cuponescdp = pCuponPaga,
                        cod_operadora = ((dynamic)contrato).cod_operadora,
                        cod_plan = ((dynamic)contrato).cod_plan,
                        cod_contrato = ((dynamic)contrato).cod_contrato,
                        fecha_inicio = contrato.fecha_inicio
                    });

                    //Bitacora
                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Contrato: {contrato.cod_contrato}  Plan: {contrato.cod_plan}  Oper: {contrato.cod_operadora}",
                        Movimiento = "Modifica - WEB",
                        Modulo = vModulo
                    });

                    //Bitacora de cambios
                    if(vCambios.vCuota != contrato.monto)
                    {
                        sbGuardaCambios(CodEmpresa, contrato.cod_operadora, contrato.cod_plan, contrato.cod_contrato, usuario, 01, $@"Anterior {vCambios.vCuota} - Nueva {contrato.monto} ");
                    }

                    if (vCambios.vPlazo != contrato.plazo)
                    {
                        sbGuardaCambios(CodEmpresa, contrato.cod_operadora, contrato.cod_plan, contrato.cod_contrato, usuario, 02, $@"Anterior {vCambios.vPlazo} - Nueva {contrato.plazo} ");
                    }

                    if (vCambios.vInversion != contrato.inversion)
                    {
                        sbGuardaCambios(CodEmpresa, contrato.cod_operadora, contrato.cod_plan, contrato.cod_contrato, usuario, 03, $@"Anterior {vCambios.vInversion} - Nueva {contrato.inversion} ");
                    }

                    if (vCambios.vDedPlanilla != contrato.ind_deduccion)
                    {
                        string anterior = vCambios.vDedPlanilla == true ? "SI" : "NO";
                        string nuevo = contrato.ind_deduccion == true ? "SI" : "NO";
                        sbGuardaCambios(CodEmpresa, contrato.cod_operadora, contrato.cod_plan, contrato.cod_contrato, usuario, 06, $@"Anterior {anterior} - Nueva {nuevo} ");
                    }


                    //insert trazabilidad.
                    string Consecutivo = $"{contrato.cod_contrato}-{contrato.cod_plan.Trim()}";
                    sbTrazabilidad_Inserta(CodEmpresa, "04", Consecutivo, contrato.cod_contrato.ToString(), usuario);
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
        /// Metodo para obtener el consecutivo del contrato
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operadora"></param>
        /// <param name="plan"></param>
        /// <returns></returns>
        private ErrorDto<int> fxConsecutivoContrato(int CodEmpresa, int operadora, string plan)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<int>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"Select isnull(Consecutivo,0) + 1 as Seq From fnd_planes where cod_operadora= @operadora  and cod_plan = @cod_plan";
                    var consc = connection.Query<int>(query, new { operadora = operadora, cod_plan = plan }).FirstOrDefault();

                    if (consc > 0)
                    {
                        string updateSql = @"
                                UPDATE fnd_planes
                                SET Consecutivo = @nuevoConsecutivo
                                WHERE cod_operadora = @codOperadora AND cod_plan = @codPlan";
                        connection.Execute(updateSql, new { nuevoConsecutivo = consc, codOperadora = operadora, codPlan = plan });

                        response.Result = consc;
                    }

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = 0;
            }
            return response;
        }
        /// <summary>
        /// Metodo para obtener los datos de la tasa preferencial
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operadora"></param>
        /// <param name="codigo"></param>
        /// <param name="contrato"></param>
        /// <param name="cedula"></param>
        /// <param name="tasaActual"></param>
        /// <param name="vTasaMargenNegociacion"></param>
        /// <returns></returns>
        private ErrorDto<dynamic> fxTasaPreferencial(
            int CodEmpresa,
            int operadora,
            string codigo,
            int contrato,
            string cedula,
            decimal tasaActual,
            decimal vTasaMargenNegociacion)
        {
            var response = new ErrorDto<dynamic>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                // Validación equivalente al "If txtContrato.Text = 0 Or txtContrato.Text = """"
                if (contrato == 0)
                {
                    response.Code = -1;
                    response.Description = "Registre el Contrato Primero, luego indique la solicitud de Tasa Preferencial!";
                    return response;
                }

                using var connection = new SqlConnection(stringConn);

                // Ejecución del SP
                var query = $@"exec spFnd_TP_Solicitud_Ultima {operadora}, '{codigo}', {contrato}, '{cedula}'";
                var result = connection.QueryFirstOrDefault<dynamic>(query);

                if (result != null)
                {
                    // Se devuelve un objeto anónimo con los valores de la tasa
                    response.Result = new
                    {
                        TasaCalculada = (decimal)result.TASA_CALCULADA,
                        MargenMaximo = (decimal)result.MARGEN_MAXIMO,
                        TasaSolicitada = (decimal)result.TASA_SOLICITADA,
                        IdTP = result.ID_TP != null ? result.ID_TP.ToString() : string.Empty,
                        EstadoDescripcion = result.ESTADO_DESC?.ToString() ?? string.Empty
                    };
                }
                else
                {
                    // Si no hay registros (EOF y BOF en VB6)
                    response.Result = new
                    {
                        TasaCalculada = tasaActual,
                        MargenMaximo = vTasaMargenNegociacion,
                        TasaSolicitada = tasaActual,
                        IdTP = string.Empty,
                        EstadoDescripcion = string.Empty
                    };
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
        /// Metodo para refrescar el estado de la gestion
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="gestionId"></param>
        /// <param name="contrato"></param>
        /// <returns></returns>
        private ErrorDto<dynamic> fxTP_Refresh(int CodEmpresa, string gestionId, int contrato)
        {
            var response = new ErrorDto<dynamic>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            try
            {
                // Validación equivalente a: If Not IsNumeric(txtGestionId.Text) Then Exit Sub
                if (!int.TryParse(gestionId, out int idGestion))
                {
                    response.Code = -1;
                    response.Description = "El ID de gestión no es válido.";
                    return response;
                }

                using var connection = new SqlConnection(stringConn);

                var query = $@"exec spFnd_TP_Estado {idGestion}";
                var result = connection.QueryFirstOrDefault<dynamic>(query);

                if (result != null)
                {
                    string estado = result.Gestion_Estado?.ToString() ?? string.Empty;

                    // Se retorna la información principal
                    response.Result = new
                    {
                        GestionId = result.Gestion_Id != null ? result.Gestion_Id.ToString() : gestionId,
                        GestionEstado = estado
                    };

                    // Si el estado no empieza con "P", simula el MsgBox + llamada a sbConsultaContrato
                    if (!string.IsNullOrEmpty(estado) && !estado.StartsWith("P", StringComparison.OrdinalIgnoreCase))
                    {
                        response.Code = 1;
                        response.Description = "La gestión ya fue resuelta. Se debe refrescar la información del contrato.";

                        // En el entorno real podrías aquí llamar a otro método, por ejemplo:
                        // var consulta = sbConsultaContrato(contrato);
                        // response.AdditionalData = consulta;
                    }
                }
                else
                {
                    response.Code = -1;
                    response.Description = "No se encontró información de la gestión.";
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        private ErrorDto<bool> fxAplicaBeneficiarios(int CodEmpresa, string plan, int operadora)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<bool>()
            {
                Code = 0,
                Description = "Ok",
                Result = false
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select REQUIERE_BENEFICIARIOS from fnd_planes 
                                    where cod_plan = @cod_plan 
                                    and cod_operadora = @operadora";
                    response.Result = connection.Query<bool>(query, new { operadora = operadora, cod_plan = plan }).FirstOrDefault();
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
        /// 
        /// </summary>
        /// <param name="xPlazo"></param>
        /// <param name="xTipo"></param>
        /// <param name="xPlan"></param>
        /// <param name="xOperadora"></param>
        /// <param name="chkCuponPaga"></param> PAGO_CUPONESCDP
        /// <param name="tipo_cdp"></param> txtInversion.Visible
        /// <param name="cuponFrecuencia"></param> cboCuponFrecuencia
        /// <param name="plazoInversion"></param> Plazo_Id
        /// <param name="txtTasa"></param> txtInversion.Visible
        /// <returns></returns>
        public ErrorDto<decimal> fxTasaRef(
            int CodEmpresa, 
            long xPlazo, 
            string xTipo,
            string xPlan, 
            int xOperadora, 
            bool chkCuponPaga, 
            int tipo_cdp, 
            string cuponFrecuencia,
            string plazoInversion,
            decimal txtTasa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<decimal>()
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "";
                    decimal fxTasaRef = 0;
                    if (!chkCuponPaga || tipo_cdp == 1)
                    {
                        query = $@"select dbo.fxFNDCalcularTasaRefContrato({xOperadora}, '{xPlan}', {xPlazo}, '{xTipo}', null, null, 0)";
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(cuponFrecuencia))
                        {
                            if(txtTasa > 0)
                            {
                                fxTasaRef = txtTasa;
                            }
                            else
                            {
                                fxTasaRef = 0;
                            }
                            response.Result = fxTasaRef;
                            return response;
                        }
                        query = $@"exec dbo.spFnd_Inversion_Tasas_Condiciones {xOperadora}, '{xPlan}', {plazoInversion}, {cuponFrecuencia})";
                    }

                       
                    response.Result = connection.Query<decimal>(query).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = 0;
            }
            return response;
        }


        private void sbGuardaCambios(int CodEmpresa,int operadora, string plan, long contrato, string usuario, int movimiento, string detalle)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"insert fnd_contratos_cambios(cod_operadora,cod_plan,cod_contrato,usuario,fecha,movimiento,detalle)
                                    values
                                    (@operadora,@cod_plan,@contrato ,@usuario,dbo.MyGetdate(), @vMovimiento,@detalle)";
                     connection.Execute(query , new
                     {
                         operadora = operadora,
                         cod_plan = plan,
                         contrato = contrato,
                         usuario = usuario,
                         vMovimiento = movimiento,
                         detalle = detalle
                     });
                }
            }
            catch (Exception ex)
            {
               _ = ex.Message;
            }
        }
        
        private void sbTrazabilidad_Inserta(int CodEmpresa, string CodDocumento,string Consecutivo,string CodBarras, string usuario, bool Nuevo = true)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            try
            {
                string IdSobre = "Null";
                long IdEstado = 1;
                int ConfirmaRecepcion = 2;


                var query = "";
                using var connection = new SqlConnection(stringConn);
                {
                    //solo para ASECCSS: por el momento
                    if (CodEmpresa != 61)
                    {
                        return;
                    }
                    else
                    {
                        if (CodDocumento == "04" && Nuevo == false)
                        {
                            query = $@"Select COUNT(*) + 1 AS CONSEC from TrdDocumentos where CodDocumento='04' and Consecutivo LIKE '%{Consecutivo}%'";
                            var consec = connection.Query<long>(query).FirstOrDefault();

                            Consecutivo = $"{Consecutivo}-{consec.ToString("00")}";
                        }
                    }

                    query = $@"exec spTrdDocumentosIns '{CodDocumento}', '{Consecutivo}', null, {IdEstado}, {ConfirmaRecepcion}, null, null, dbo.MyGetdate(), '{usuario}', '{CodBarras}', null ";
                    connection.Execute(query);
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
        }

        private bool fxBeneficiariosNoIncluidos(int CodEmpresa, string plan, int operadora, long contrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            bool response = false;
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select isnull(sum(porcentaje),0) as porcentaje from FND_CONTRATOS_BENEFICIARIOS 
                                       where cod_plan = @cod_plan  and cod_operadora = @operadora";
                    var porcentaje = connection.Query<decimal>(query, new { operadora = operadora, cod_plan = plan }).FirstOrDefault();
                    if(porcentaje != null)
                    {
                        if (porcentaje == 0 || porcentaje < 100)
                        {
                            response = false;
                        }
                        else
                        {
                            response = true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                   
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
                response = false;
            }
            return response;
        }

        #endregion

        #region TP

        /// <summary>
        /// Metodo para obtener los datos de la tasa preferencial
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="operadora"></param>
        /// <param name="plan"></param>
        /// <param name="contrato"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<FndContratoTasaPreferencial> Fnd_Contratos_TP_Obtener(int CodEmpresa,int operadora,string plan, int contrato, string cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<FndContratoTasaPreferencial>
            {
                Code = 0,
                Description = "Ok",
                Result = new FndContratoTasaPreferencial()
            };

            try
            {
                if(contrato == 0)
                {
                    response.Code = -1;
                    response.Description = "Registre el Contrato Primero, luego indique la solicitud de Tasa Preferencial!";
                    return response;
                }

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spFnd_TP_Solicitud_Ultima {operadora}, '{plan}', {contrato}, '{cedula}'";
                    response.Result = connection.Query<FndContratoTasaPreferencial>(query).FirstOrDefault();
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
        /// Metodo para solicitar la tasa preferencial
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="solicitud"></param>
        /// <returns></returns>
        public ErrorDto<FndSolicitudTpData> Fnd_Contratos_TP_Solicita(int CodEmpresa, FndContratoTasaPreferencial solicitud)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<FndSolicitudTpData>
            {
                Code = 0,
                Description = "Ok",
                Result = new FndSolicitudTpData()
            };

            try
            {
               
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spFnd_TP_Solicitud  {solicitud.operadora}, '{solicitud.cod_plan}', {solicitud.contrato}, '{solicitud.cedula}'
                                 , {solicitud.tasa_calculada}, {solicitud.margen_maximo}, {solicitud.tasa_solicitada}, 
                                 {solicitud.plazo}, {solicitud.frecuencia}, {solicitud.inversion}, '{solicitud.usuario}', '{solicitud.notas}'";
                    response.Result = connection.Query<FndSolicitudTpData>(query).FirstOrDefault();
                   
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
        /// Me
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="gestion_id"></param>
        /// <returns></returns>
        public ErrorDto<FndSolicitudTpData> Fnd_Contratos_TP_Estado(int CodEmpresa, int gestion_id)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<FndSolicitudTpData>
            {
                Code = 0,
                Description = "Ok",
                Result = new FndSolicitudTpData()
            };

            try
            {

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spFnd_TP_Estado {gestion_id}";
                    response.Result = connection.Query<FndSolicitudTpData>(query).FirstOrDefault();

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }
            return response;
        }

        #endregion

    }
}
