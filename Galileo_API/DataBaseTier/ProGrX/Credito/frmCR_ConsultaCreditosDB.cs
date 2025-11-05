using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using PgxAPI.BusinessLogic;
using PgxAPI.Models;
using PgxAPI.Models.ERROR;
using PgxAPI.Models.ProGrX.Cajas;
using PgxAPI.Models.ProGrX.Clientes;
using PgxAPI.Models.ProGrX.Credito;
using System.Data;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace PgxAPI.DataBaseTier.ProGrX.Credito
{
    public class frmCR_ConsultaCreditosDB
    {
        private readonly IConfiguration? _config;
        private readonly int vModulo = 3; // Modulo de Créditos
        private mProGrx_Main _mProGrx_Main;
        private mCredito _mCredito;
        private mProGrX_AuxiliarDB _AuxiliarDB;
        private readonly mSecurityMainDb _Security_MainDB;

        public frmCR_ConsultaCreditosDB(IConfiguration? config)
        {
            _config = config;
            _mProGrx_Main = new mProGrx_Main(_config);
            _mCredito = new mCredito(_config);
            _AuxiliarDB = new mProGrX_AuxiliarDB(_config);
            _Security_MainDB = new mSecurityMainDb(_config);
        }

        /// <summary>
        /// Consulta los tipos de garantía disponibles para el formulario en la tabla CRD_GARANTIA_TIPOS.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ConsultaCrdGarantiaTipo_Obtener(int CodEmpresa)
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
                    var query = $@"select GARANTIA as 'item', rtrim(DESCRIPCION) as 'descripcion'
                                    from CRD_GARANTIA_TIPOS
                                    where FORMULARIO = 'F01' order by Garantia";
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
        /// Consulta los socios disponibles para el formulario
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaCrdSociosData>> CR_ConsultaCrdSocios_Obtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CrConsultaCrdSociosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CrConsultaCrdSociosData>()
            };
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"Select cedula,cedular,nombre from SOCIOS";
                    response.Result = connection.Query<CrConsultaCrdSociosData>(query).ToList();
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
        /// Consulta los datos de la persona para el formulario de consulta integrada.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CrConsultaCrdData> CR_ConsultaCrdConsulta_Integrada_Obtener(int CodEmpresa, string cedula, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<CrConsultaCrdData>
            {
                Code = 0,
                Description = "Ok",
                Result = new CrConsultaCrdData()
            };
            try
            {
                
                var validaCadena = _mProGrx_Main.fxSIFValidaCadena(cedula.Trim());
                if (validaCadena.Code == -1)
                {
                    response.Code = validaCadena.Code;
                    response.Description = validaCadena.Description;
                    return response;
                }

                //'Valida Acceso a Expediente
                var vRA_Access = _mProGrx_Main.fxSys_RA_Consulta(CodEmpresa, cedula, usuario);
                if (!vRA_Access.Result)
                {
                    response.Code = -1;
                    response.Description = "Esta persona se encuentra con -> Expediente Restringido <- Requiere de Autorización para Consultar!";
                    return response;
                }

                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spSys_Consulta_Integrada @cedula";
                    response.Result = connection.Query<CrConsultaCrdData>(query, new { cedula = cedula }).FirstOrDefault();

                    response.Result.vMora = false;
                    DateTime vFechaIng = response.Result.fechaingreso == null ? DateTime.Now : (DateTime)response.Result.fechaingreso;


                    response.Result.membresiaCaption = "Membresía: NADA";
                    response.Result.membresiaToolTip = fxLiquidacion(CodEmpresa, cedula);

                    if (response.Result.estadoactual == "S")
                    {
                        response.Result.membresiaCaption = "Membresía: " + _mCredito.fxMembresia(vFechaIng);
                        response.Result.membresiaToolTip = "[Ing.:" + vFechaIng.ToString("g");

                        //renuncias
                        query = $@"exec spAFI_ConsultaRenunciaTransito @cedula";
                        var renuncias = connection.Query<CrConsultaCrdData>(query, new { cedula = cedula }).FirstOrDefault();
                        if (renuncias != null)
                        {
                            response.Result.membresiaCaption = $@"Renuncia: {renuncias.cod_Renuncia} ¦ {renuncias.registro_fecha} ¦ {renuncias.registro_user}";
                            response.Result.membresiaToolTip = $@"{renuncias.estado} ¦ {renuncias.tipo} ¦ {renuncias.descripcion}";
                        }
                        
                    }

                    //'Clasificación de la Persona
                    response.Result.clasificacionCaption = $@"Clasificación Crediticia : [{response.Result.clasificacion}]";

                    //response.Result.notas = $@"Usuario : {response.Result.nota_user} Fecha : {response.Result.nota_fecha} ";

                    if (response.Result.salario_traslada == 1)
                    {
                        response.Result.salarioTrasladaCaption = "Traslada Salario: Sí";
                    }
                    else
                    {
                        response.Result.salarioTrasladaCaption = "Sin Tramite (Traslado Salario)";
                    }

                    response.Result.patrimonio = response.Result.ahorro + response.Result.aporte + response.Result.custodia + response.Result.capitaliza;

                    response.Result.tarjetaCaption = $@"Tarjeta: {response.Result.tarjeta_numero}";
                    response.Result.ibanCaption = $@"IBAN: {response.Result.iban}";



                    if (response.Result.indmensajes == 0)
                    {
                        response.Result.estadoMensajesCaption = "Mensajes ?";
                    }
                    else
                    {
                        response.Result.estadoMensajesCaption = $@"Mensajes ({response.Result.indmensajes})";
                    }

                    //'Indicar de Gestiones de Cobros
                    if (response.Result.indcobro == 0)
                    {
                        response.Result.estadoCobrosCaption = "Sin Gestión de Cobro";
                    }
                    else
                    {
                        response.Result.estadoCobrosCaption = $@"Gestiones de Cobro ({response.Result.indcobro})";
                    }

                    //'Indicar de Advertencias
                    if (response.Result.indadvertencias == 0)
                    {
                        response.Result.estadoAdvertenciaCaption = "Sin Advertencias";
                    }
                    else
                    {
                        response.Result.estadoAdvertenciaCaption = $@"Advertencias ({response.Result.indadvertencias})";
                    }

                    //'Pregunta por el Consentimiento de Uso de la Información Personal para Contacto
                    if (response.Result.consentimiento_contacto_fecha != null)
                    {
                        //fecha con formato "g" = 9/1/2008 4:05 PM
                        string vFecha = response.Result.consentimiento_contacto_fecha == null ? "" : ((DateTime)response.Result.consentimiento_contacto_fecha).ToString("dd/MM/yyyy");
                        response.Result.estadoConsentimientoToolTip = $@"Fecha : {vFecha} | Usuario: {response.Result.consentimiento_contacto_usuario}";
                    }
                    else
                    {
                        response.Result.estadoConsentimientoToolTip = "";
                        response.Result.consentimiento_contacto_fecha = null;
                        response.Result.consentimiento_contacto_usuario = null;
                    }

                    if (response.Result.pat_advertencia.Length > 0)
                    {
                        response.Code = -2;
                        response.Description = "Advertencia de Aportes no cotizados: " + response.Result.pat_advertencia;
                    }

                    //'Indica el Estado de las Fianzas
                    if (response.Result.indfianzas == false) // en v6 su variable es vFianzas
                    {
                        response.Result.fianzasCaption = "Fianzas al Día";
                    }
                    else
                    {
                        response.Result.fianzasCaption = "Fianzas en Mora";
                    }

                    //Mensajes
                    query = $@"exec spSIFPersonaMensajes @cedula";
                    var mensajes = connection.Query<CrConsultaCrdData>(query, new { cedula = cedula }).FirstOrDefault();
                    if (mensajes != null)
                    {
                        response.Result.pendientes = mensajes.pendientes;
                        response.Result.advertencias = mensajes.advertencias;
                        response.Result.generales = mensajes.generales;
                        response.Result.morosidad = mensajes.morosidad;
                        response.Result.bloqueos = mensajes.bloqueos;
                    }

                    if (response.Result.pendientes > 0)
                    {
                        response.Result.pendientesCaption = $@"Pendientes ({response.Result.pendientes})";
                    }
                    else
                    {
                        response.Result.pendientesCaption = "Msj. Pendientes?";
                    }

                    if (response.Result.advertencias > 0)
                    {
                        response.Result.advertenciasCaption = $@"Advertencias ({response.Result.advertencias})";
                    }
                    else
                    {
                        response.Result.advertenciasCaption = "Msj Advertencias?";
                    }

                    if (response.Result.generales > 0)
                    {
                        response.Result.generalesCaption = $@"General ({response.Result.generales})";
                    }
                    else
                    {
                        response.Result.generalesCaption = "Msj Generales?";
                    }

                    if (response.Result.morosidad > 0)
                    {
                        response.Result.morosidadCaption = $@"Morosidad ({response.Result.morosidad})";
                    }
                    else
                    {
                        response.Result.morosidadCaption = "Msj Morosidad?";
                    }

                    if (response.Result.bloqueos > 0)
                    {
                        response.Result.bloqueosCaption = $@"Bloqueos ({response.Result.bloqueo})";
                    }
                    else
                    {
                        response.Result.bloqueosCaption = "Msj Bloqueos?";
                    }


                    //'Actualiza Disponible Garantia Sobre Ahorros
                    response.Result.pat_tipoSaldo = "Saldos en Garantía";
                    // originalmente llama cboPAT_TipoSaldo_Click en v6 pero es un trabajo desde el cliente

                    //'Actualiza el Detalle de Creditos
                    var listCredito = CR_ConsultaCrd_Creditos_Obtener(CodEmpresa, cedula, "C");
                    //reviso estatus
                    foreach(CrConsultaCrd_CreditosData credito in listCredito.Result)
                    {
                        switch (credito.procesoCod)
                        {
                            case "J":
                                response.Result.vMora = true;
                                response.Result.vMoraCaption = $@">> Cobro Judicial << 
                                | Fecha : {credito.fecha_enviaProceso} 
                                | Nota : {credito.observacion_proceso}";
                                continue;
                        }
                    }

                    //'Consulta Traslado de Salario
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
        /// Obtiene la causa de liquidación más reciente de un socio.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        private string fxLiquidacion(int CodEmpresa, string cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            string response = "";
            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"select C.descripcion
                                    from liquidacion L inner join Causas_Renuncias C on C.id_causa = L.id_causa
                                    where consec in(select max(consec) from liquidacion
                                    where cedula = @cedula)";
                    var detalle = connection.Query<string>(query, new { cedula = cedula }).FirstOrDefault();

                    if (detalle != null)
                    {
                        response = $@"[CAUSA: {detalle}]";
                    }


                }
            }
            catch (Exception ex)
            {
                response = "";
            }
            return response;
        }

        /// <summary>
        /// Metodo actualiza nota socio
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="nota"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto CR_Socios_RegistrarNota(int CodEmpresa, string cedula, string nota, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok",
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string query = @"
                UPDATE socios
                        SET notas = UPPER(LTRIM(RTRIM(@Nota))),
                            Nota_User = @Usuario,
                            Nota_Fecha = dbo.MyGetdate()
                        WHERE cedula = @Cedula;

                        INSERT INTO socios_mensajes (fecha, cedula, usuario, vencimiento, mensaje, tipo)
                        VALUES (dbo.MyGetdate(), @Cedula, @Usuario, '2100-01-01', @Nota, 'G');
                    ";

                    var parameters = new DynamicParameters();
                    parameters.Add("@Cedula", cedula);
                    parameters.Add("@Usuario", usuario);
                    parameters.Add("@Nota", nota);

                    connection.Execute(query, parameters);

                }
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto<decimal> fxCajas_SaldoaFavor(int CodEmpresa, string cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<decimal>
            {
                Code = 0,
                Description = "Ok",
                Result = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string query = @"select dbo.fxCajas_SaldoaFavor(@cedula) as 'Cajas_Saldo_Favor'";
                    response.Result = connection.Query<decimal>(query, new { cedula = cedula }).FirstOrDefault();
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

        #region Créditos

        /// <summary>
        /// Método para consultar Activos y Cancelados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaCrd_CreditosData>> CR_ConsultaCrd_Creditos_Obtener(int CodEmpresa, string cedula, string sheetName)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CrConsultaCrd_CreditosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CrConsultaCrd_CreditosData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spSys_Consulta_Integrada_Creditos @Cedula , @Estado ";
                    response.Result = connection.Query<CrConsultaCrd_CreditosData>(query,
                        new { Cedula = cedula, Estado = sheetName }).ToList();
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
        /// Consulta tramite credito
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaCrd_solicitudData>> CR_ConsultaCrd_Tramite_Obtener(int CodEmpresa, string cedula, string sheetName)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CrConsultaCrd_solicitudData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CrConsultaCrd_solicitudData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spSIFEstadoSolicitud @Cedula ";
                    response.Result = connection.Query<CrConsultaCrd_solicitudData>(query,
                        new { Cedula = cedula }).ToList();
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
        /// Consulta tramite credito
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaCreditosData>> CR_ConsultaCrd_Tramite_Obtener(int CodEmpresa, string cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CrConsultaCreditosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CrConsultaCreditosData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spSIFEstadoSolicitud @Cedula ";
                    response.Result = connection.Query<CrConsultaCreditosData>(query,
                        new { Cedula = cedula }).ToList();
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
        /// Obtiene creditos en PreAnalisis
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaCrd_preanalisisData>> CR_ConsultaCrd_PreAnalisis_Obtener(int CodEmpresa, string cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CrConsultaCrd_preanalisisData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CrConsultaCrd_preanalisisData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spSIFEstadoPreAnalisis  @Cedula ";
                    response.Result = connection.Query<CrConsultaCrd_preanalisisData>(query,
                        new { Cedula = cedula }).ToList();
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
        /// Obtiene creditos en Incobrable
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaCrd_incobrableData>> CR_ConsultaCrd_Incobrable_Obtener(int CodEmpresa, string cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<List<CrConsultaCrd_incobrableData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CrConsultaCrd_incobrableData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = $@"exec spSIFEstadoIncobrable  @Cedula ";
                    response.Result = connection.Query<CrConsultaCrd_incobrableData>(query,
                        new { Cedula = cedula }).ToList();
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

        #region Cobros

        /// <summary>
        /// Obtiene los cobros de una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaCobroDTO>> CR_ConsultaCobros_Obtener(int codEmpresa, string cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<List<CrConsultaCobroDTO>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CrConsultaCobroDTO>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"
                SELECT 
                    S.*,
                    ISNULL(G.descripcion, '') AS Gestion,
                    ISNULL(C.descripcion, '') AS Causa,
                    ISNULL(A.descripcion, '') AS Arreglo
                FROM CBR_Seguimiento S
                LEFT JOIN cbr_gestiones G 
                    ON S.cod_gestion = G.cod_gestion
                LEFT JOIN CBR_CAUSAS_MOROSIDAD C 
                    ON S.cod_causa = C.cod_causa
                LEFT JOIN CBR_TIPOS_ARREGLOS A 
                    ON S.cod_arreglo = A.cod_arreglo
                WHERE S.cedula = @Cedula
                ORDER BY S.cod_seg DESC;
            ";

                    response.Result = connection
                        .Query<CrConsultaCobroDTO>(query, new { Cedula = cedula })
                        .ToList();
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
        /// Consulta Asignacion de Oficina de Cobro
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaAsignacionCobroData>> CR_ConsultaAsignacion_Obtener(int codEmpresa, string cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<List<CrConsultaAsignacionCobroData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CrConsultaAsignacionCobroData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"
                SELECT 
                    usuario,
                    cedula,
                    fecha_asignacion,
                    mantener,
                    rebajo_doble,
                    aplica_mora
                FROM CBR_Asignacion_H
                WHERE cedula = @Cedula
                ORDER BY fecha_asignacion DESC;
            ";

                    response.Result = connection
                        .Query<CrConsultaAsignacionCobroData>(query, new { Cedula = cedula })
                        .ToList();
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

        #region Ahorros

        /// <summary>
        /// Consulta los movimientos de ahorro de una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaContratosData>> CR_ContratosConsulta_Obtener(int codEmpresa, string cedula, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<List<CrConsultaContratosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CrConsultaContratosData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "spFndContratosConsulta";
                    response.Result = connection
                        .Query<CrConsultaContratosData>(
                            query,
                            new { Cedula = cedula, Usuario = usuario },
                            commandType: CommandType.StoredProcedure
                        )
                        .ToList();
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
        /// Consulta los movimientos de ahorro de una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codOperadora"></param>
        /// <param name="codPlan"></param>
        /// <param name="codContrato"></param>
        /// <returns></returns>
        public ErrorDto<List<CrContratosMovimientosData>> CR_Contratos_Movimientos_Obtener(int codEmpresa, int codOperadora, string codPlan, long codContrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<List<CrContratosMovimientosData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CrContratosMovimientosData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"
                SELECT 
                    Det.fecha,
                    Det.Fecha_Proceso,
                    Det.Monto,
                    ISNULL(Doc.Descripcion, '') AS DocDesc,
                    Det.nCon,
                    ISNULL(Con.Descripcion, '') AS ConDesc,
                    Det.Usuario,
                    Det.Detalle_01
                FROM fnd_contratos_detalle AS Det
                LEFT JOIN SIF_Documentos AS Doc 
                    ON Det.Tcon = Doc.Tipo_Documento
                LEFT JOIN SIF_Conceptos AS Con 
                    ON Det.Cod_Concepto = Con.Cod_Concepto
                WHERE Det.cod_operadora = @CodOperadora
                  AND Det.cod_plan = @CodPlan
                  AND Det.cod_contrato = @CodContrato
                ORDER BY Det.Fecha DESC, Det.COD_fnd_detalle DESC;
            ";

                    response.Result = connection
                        .Query<CrContratosMovimientosData>(query, new
                        {
                            CodOperadora = codOperadora,
                            CodPlan = codPlan,
                            CodContrato = codContrato
                        })
                        .ToList();
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
        /// Consulta los cupones de un contrato de ahorro
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codOperadora"></param>
        /// <param name="codPlan"></param>
        /// <param name="codContrato"></param>
        /// <returns></returns>
        public ErrorDto<List<CrContratosCuponesData>> CR_Contratos_Cupones_Obtener(int codEmpresa, int codOperadora, string codPlan, long codContrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<List<CrContratosCuponesData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CrContratosCuponesData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"
                SELECT 
                    Cupon_Id,
                    Fecha_Vence,
                    Monto_Base,
                    Tasa_Aplicada,
                    Cupon_Monto,
                    Rendimiento,
                    Principal,
                    Dias,
                    Estado_Desc,
                    Consec,
                    ISR_PORC,
                    ISR_MNT_GRAVABLE,
                    ISR_MONTO,
                    TOTAL_GIRAR,
                    Tesoreria_Id,
                    Tes_Documento,
                    Bancos_Estado,
                    IBAN
                FROM vFnd_Contratos_Cupones
                WHERE cod_operadora = @CodOperadora
                  AND cod_plan = @CodPlan
                  AND cod_contrato = @CodContrato
                ORDER BY Fecha_Vence;
            ";

                    response.Result = connection
                        .Query<CrContratosCuponesData>(query, new
                        {
                            CodOperadora = codOperadora,
                            CodPlan = codPlan,
                            CodContrato = codContrato
                        })
                        .ToList();
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
        /// Consulta la bitacora de los contratos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codOperadora"></param>
        /// <param name="codPlan"></param>
        /// <param name="codContrato"></param>
        /// <returns></returns>
        public ErrorDto<List<CrContratosBitacoraData>> CR_Contratos_Bitacora_Obtener(int codEmpresa, int codOperadora, string codPlan, long codContrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<List<CrContratosBitacoraData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CrContratosBitacoraData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"
                SELECT 
                    C.ID_BITACORA,
                    C.COD_OPERADORA,
                    C.COD_PLAN,
                    C.COD_CONTRATO,
                    C.USUARIO,
                    C.FECHA,
                    C.MOVIMIENTO,
                    C.DETALLE,
                    C.REVISADO_USUARIO,
                    C.REVISADO_FECHA,
                    S.cedula,
                    S.nombre,
                    M.Descripcion AS MovimientoDesc,
                    CASE 
                        WHEN C.revisado_fecha IS NULL THEN 0 
                        ELSE 1 
                    END AS Revisado
                FROM fnd_contratos_cambios AS C
                INNER JOIN fnd_contratos AS X 
                    ON C.cod_operadora = X.cod_operadora
                   AND C.cod_plan = X.cod_plan
                   AND C.cod_contrato = X.cod_contrato
                INNER JOIN Socios AS S 
                    ON X.cedula = S.cedula
                INNER JOIN US_MOVIMIENTOS_BE AS M 
                    ON C.Movimiento = M.Movimiento
                   AND M.modulo = 18
                WHERE C.cod_operadora = @CodOperadora
                  AND C.cod_plan = @CodPlan
                  AND C.cod_contrato = @CodContrato
                ORDER BY C.fecha DESC;
            ";

                    response.Result = connection
                        .Query<CrContratosBitacoraData>(query, new
                        {
                            CodOperadora = codOperadora,
                            CodPlan = codPlan,
                            CodContrato = codContrato
                        })
                        .ToList();
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
        /// Consulta los cierres de contratos
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codOperadora"></param>
        /// <param name="codPlan"></param>
        /// <param name="codContrato"></param>
        /// <returns></returns>
        public ErrorDto<List<CrContratosCierresData>> CR_Contratos_Cierres_Obtener(int codEmpresa, int codOperadora, string codPlan, long codContrato)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<List<CrContratosCierresData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CrContratosCierresData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"
                SELECT TOP 36
                    A.Anio,
                    A.Mes,
                    A.Aportes,
                    A.Rendimientos,
                    (A.Aportes + A.Rendimientos) AS Total,
                    A.Monto_Transito,
                    A.Sobre_Giro,
                    A.Rend_Corte,
                    A.Ind_Deduccion,
                    A.Tipo_Deduc,
                    A.Porc_Deduc,
                    A.Monto,
                    A.Inversion,
                    A.Cashback_Pts_Corte,
                    A.Cashback_Pts_Otorgados,
                    A.Cashback_Pts_Redimidos,
                    A.Cod_Plan,
                    A.Cod_Contrato
                FROM FND_PER_CERRADOS AS A
                WHERE A.Cod_Operadora = @CodOperadora
                  AND A.Cod_Plan = @CodPlan
                  AND A.Cod_Contrato = @CodContrato
                ORDER BY A.Anio DESC, A.Mes DESC;
            ";

                    response.Result = connection
                        .Query<CrContratosCierresData>(query, new
                        {
                            CodOperadora = codOperadora,
                            CodPlan = codPlan,
                            CodContrato = codContrato
                        })
                        .ToList();
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
        /// Obtiene si la sesion esta activa o no
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="identificacion"></param>
        /// <returns></returns>
        public ErrorDto<CajasSesionDTO> Cajas_Sesion_ObtenerActiva(int codEmpresa, string usuario, string identificacion)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<CajasSesionDTO>
            {
                Code = 0,
                Description = "Ok",
                Result = null
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var sql = @"SELECT TOP 1 *
                        FROM CAJAS_SESION
                        WHERE cod_usuario = @Usuario
                          AND estado = 1
                          AND identificacion = @Identificacion";

                    var result = connection.QueryFirstOrDefault<CajasSesionDTO>(
                        sql,
                        new { Usuario = usuario, Identificacion = identificacion }
                    );

                    if (result != null)
                        response.Result = result;
                    else
                    {
                        response.Code = -2;
                        response.Description = "No se encontró sesión activa.";
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

        #region Patrimonio

        /// <summary>
        /// Obtiene el patrimonio de una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<CrPatrimonioData>> CR_Patrimonio_Obtener(int codEmpresa, string cedula, string tipo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<List<CrPatrimonioData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<CrPatrimonioData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"SELECT TOP 30
                                    Ah.*,
                                    ISNULL(Doc.Descripcion, '') AS DocDesc,
                                    ISNULL(Con.Descripcion, '') AS ConDesc,
                                    CASE Ah.Tipo
                                        WHEN 'O' THEN 'Obrero'
                                        WHEN 'P' THEN 'Patronal'
                                        WHEN 'X' THEN 'AP.Custodia'
                                        WHEN 'C' THEN 'Capitalización'
                                        ELSE Ah.Tipo
                                    END AS Tipo
                                FROM Ahorro_Detallado Ah
                                LEFT JOIN SIF_Documentos Doc 
                                       ON Ah.Tcon = Doc.Tipo_Documento
                                LEFT JOIN SIF_Conceptos Con 
                                       ON Ah.cod_Concepto = Con.cod_Concepto
                                  WHERE Ah.Cedula = @Cedula
                                    AND (
                                          @Tipo = 'T' 
                                          OR Ah.Tipo = @Tipo
                                        )
                                ORDER BY Ah.Fecha DESC;
                                ";

                    response.Result = connection
                        .Query<CrPatrimonioData>(query, new
                        {
                            Cedula = cedula,
                            Tipo = tipo
                        })
                        .ToList();
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
        /// Obtiene los periodos visibles para un socio
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<ExcPeriodosVisiblesData>> EXC_Periodos_Visibles_Obtener(int codEmpresa, string cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<List<ExcPeriodosVisiblesData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ExcPeriodosVisiblesData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "spEXC_Periodos_Visibles";

                    response.Result = connection
                        .Query<ExcPeriodosVisiblesData>(
                            query,
                            new { Cedula = cedula },
                            commandType: CommandType.StoredProcedure
                        )
                        .ToList();
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

        #region Beneficios

        /// <summary>
        /// Obtiene los beneficios de una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<AfiBeneficiosConsultaData>> AFI_Beneficios_Consulta(int codEmpresa, string cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<List<AfiBeneficiosConsultaData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfiBeneficiosConsultaData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "spAFI_Beneficios_Consulta";

                    response.Result = connection
                        .Query<AfiBeneficiosConsultaData>(
                            query,
                            new { Cedula = cedula },
                            commandType: CommandType.StoredProcedure
                        )
                        .ToList();
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

        #region Renuncias
        /// <summary>
        /// Obtiene las renuncias en tránsito de una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<AfiRenunciaTransitoData>> AFI_ConsultaRenunciaTransito(int codEmpresa, string cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<List<AfiRenunciaTransitoData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfiRenunciaTransitoData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "spAFI_ConsultaRenunciaTransito";

                    response.Result = connection
                        .Query<AfiRenunciaTransitoData>(
                            query,
                            new { Cedula = cedula },
                            commandType: CommandType.StoredProcedure
                        )
                        .ToList();
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
        /// Obtiene las renuncias de una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<AfiRenunciasConsultaData>> AFI_Renuncias_Consulta(int codEmpresa, string cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<List<AfiRenunciasConsultaData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfiRenunciasConsultaData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "spAFI_Renuncias_Consulta";

                    response.Result = connection
                        .Query<AfiRenunciasConsultaData>(
                            query,
                            new { Cedula = cedula },
                            commandType: CommandType.StoredProcedure
                        )
                        .ToList();
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

        #region Mensajes
        /// <summary>
        /// Obtiene los mensajes de una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<AfiSociosMensajesData>> AFI_Socios_Mensajes_Obtener(int codEmpresa, string cedula, string tipo)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<List<AfiSociosMensajesData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<AfiSociosMensajesData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"
                        SELECT *
                        FROM socios_mensajes
                        WHERE cedula = @Cedula
                          AND DATEDIFF(DAY, dbo.MyGetdate(), vencimiento) >= 0
                          AND Tipo = @Tipo
                          AND ISNULL(Resolucion, 'P') = 'P'
                        ORDER BY Fecha DESC;
                    ";

                    response.Result = connection
                        .Query<AfiSociosMensajesData>(query, new
                        {
                            Cedula = cedula,
                            Tipo = tipo
                        })
                        .ToList();
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

        public ErrorDto AFI_Socios_Mensajes_Guardar(int codEmpresa, AfiSociosMensajesData data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    //Valida si existe
                    var query = @"SELECT COUNT('X') FROM socios_mensajes where cedula = @cedula 
                         and vencimiento = @fecha 
                         and substring(mensaje,1,15) = substring(@mensaje,1,15) 
                         and usuario = @usuario 
                         and Tipo = 'G'
                         and resolucion = 'P'";
                    string vfecha = _AuxiliarDB.validaFechaGlobal(data.vencimiento);
                    string vfechaReg = _AuxiliarDB.validaFechaGlobal(data.fecha);

                    var existe = connection.Query<int>(query, new
                    {
                        cedula = data.cedula,
                        usuario = data.usuario,
                        fecha = vfecha,
                        mensaje = data.mensaje
                    }).FirstOrDefault();

                    if (existe > 0)
                    {
                       query = @"
                        update socios_mensajes set mensaje = @mensaje, vencimiento = @fecha_vence
                           where cedula = @cedula 
                             and fecha = @fecha 
                             and substring(mensaje,1,15) = substring(@ mensaje,1,15) 
                             and usuario = @usuario 
                             and Tipo = 'G'
                             and resolucion = 'P'";
                        connection.ExecuteAsync(query, new
                        {
                            cedula = data.cedula,
                            usuario = data.usuario,
                            fecha = vfechaReg,
                            fecha_vence = vfecha,
                            mensaje = data.mensaje
                        });
                    }
                    else
                    {
                        query = @"
                        insert socios_mensajes(fecha,cedula,usuario,vencimiento,mensaje,Tipo) 
                        values(dbo.MyGetdate(),@cedula,@usuario,@fecha_vence,@mensaje,'G')";
                        connection.ExecuteAsync(query, new
                        {
                            cedula = data.cedula,
                            usuario = data.usuario,
                            fecha_vence = vfecha,
                            mensaje = data.mensaje
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

        public ErrorDto AFI_Socios_Mensajes_Elimina(int codEmpresa, AfiSociosMensajesData data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"
                       delete from socios_mensajes 
                       where cedula = @cedula 
                         and vencimiento = @fecha 
                         and substring(mensaje,1,15) = substring(@mensaje,1,15) 
                         and usuario = @usuario 
                         and Tipo = 'G'
                         and resolucion = 'P'
                    ";

                    string vfecha = _AuxiliarDB.validaFechaGlobal(data.vencimiento);

                    connection.ExecuteAsync(query, new
                    {
                        cedula = data.cedula,
                        usuario = data.usuario,
                        fecha = vfecha,
                        mensaje = data.mensaje
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

        public ErrorDto AFI_Socios_Mensajes_Resolucion(int codEmpresa,string usuario, AfiSociosMensajesData data)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Ok"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = @"
                       update socios_mensajes set Resolucion = 'R', Resolucion_Fecha = dbo.MyGetdate()
                          , Resolucion_Usuario = @usuario
                           where cedula = @cedula 
                           and usuario = @userMsj
                           and vencimiento = @fecha_vence
                           and substring(mensaje,1,15) = substring(@mensaje,1,15)";

                    string vfecha = _AuxiliarDB.validaFechaGlobal(data.vencimiento);

                    connection.ExecuteAsync(query, new
                    {
                        cedula = data.cedula,
                        usuario =usuario,
                        userMsj = data.usuario,
                        fecha_vence = vfecha,
                        mensaje = data.mensaje
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

        #endregion

        #region Correo

        /// <summary>
        /// Obtiene los correos de una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<SysMailLoadData>> Sys_Mail_Load(int codEmpresa, string cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto<List<SysMailLoadData>>
            {
                Code = 0,
                Description = "Ok",
                Result = new List<SysMailLoadData>()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var query = "spSys_Mail_Load";

                    response.Result = connection
                        .Query<SysMailLoadData>(
                            query,
                            new { Cedula = cedula },
                            commandType: CommandType.StoredProcedure
                        )
                        .ToList();
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

        #region Info

        /// <summary>
        /// Obtiene la información general de una persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        public ErrorDto<CR_ConsultasInfoDTO> AF_Persona_Consulta_Obtener(int CodEmpresa, string cedula, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);

            var response = new ErrorDto<CR_ConsultasInfoDTO>
            {
                Code = 0,
                Description = "Consulta realizada correctamente",
                Result = new CR_ConsultasInfoDTO()
            };

            try
            {
                using var connection = new SqlConnection(stringConn); connection.Open();


                var parametros = new { Cedula = cedula, Usuario = usuario };

                using var multi = connection.QueryMultiple("spCR_InfoPersona_Consulta", param: parametros, commandType: CommandType.StoredProcedure);

                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                response.Result.Telefonos = multi.Read<AF_TelefonosDTO>().ToList();
                response.Result.CuentasBancarias = multi.Read<AF_CuentaBancariaDTO>().ToList();
                response.Result.Ingresos = multi.Read<AF_PersonaIngresoDTO>().ToList();
                response.Result.Liquidaciones = multi.Read<CR_PersonaLiquidacionDTO>().ToList();
                response.Result.Beneficiarios = multi.Read<AF_PersonaBeneficiarioDTO>().ToList();
                response.Result.Canales = multi.Read<AF_CanalesDTO>().ToList();
                response.Result.Bienes = multi.Read<AF_BienDTO>().ToList();
                response.Result.Escolaridad = multi.Read<AF_EscolaridadDTO>().ToList();
                response.Result.Contacto = multi.Read<AF_PersonaDetalleDto>().ToList();
                response.Result.EstadoLaboral = multi.Read<AF_PersonaEstadoLaboralDto>().ToList();
                response.Result.BenePolizas = multi.Read<AFPersonaBenePolizaDTO>().ToList();
                response.Result.Preferencias = multi.Read<CrPreferenciaDTO>().ToList();



            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto AF_Persona_Canales_Registra(int CodEmpresa, string req)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            AF_CanalesDTO request = JsonConvert.DeserializeObject<AF_CanalesDTO>(req) ?? new AF_CanalesDTO();
            var response = new ErrorDto
            {
                Code = 0,
                Description = "Operación realizada correctamente"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var p = new DynamicParameters();

                p.Add("@Cedula", request.cedula);
                p.Add("@Canal", request.canal_tipo.ToString("D2"));
                p.Add("@TipoMov", request.asignado ? "A" : "E");
                p.Add("@Usuario", request.registro_usuario);

                connection.Execute("dbo.spAFI_Persona_Canales_Registra", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Registra bienes de la persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Bienes_Registra(int CodEmpresa, string req)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            AF_PersonaBienesRegistraDTO request = JsonConvert.DeserializeObject<AF_PersonaBienesRegistraDTO>(req) ?? new AF_PersonaBienesRegistraDTO();

            var response = new ErrorDto
            {
                Code = 0,
                Description = "Operación realizada correctamente"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var p = new DynamicParameters();

                string codBien;
                if (request.CodBien.Contains("."))
                {
                    var partes = request.CodBien.Split('.');
                    var entero = partes[0].PadLeft(2, '0');
                    codBien = $"{entero}.{partes[1]}";
                }
                else
                {
                    codBien = request.CodBien.PadLeft(2, '0');
                }

                p.Add("@Cedula", request.Cedula);
                p.Add("@Codigo", codBien);
                p.Add("@TipoMov", request.Asignado ? "A" : "E");
                p.Add("@Usuario", request.Usuario);

                connection.Execute("dbo.spAFI_Persona_Bienes_Registra", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Registra escolaridad de la persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Escolaridad_Registra(int CodEmpresa, string request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            AF_PersonaEscolaridadRegistraDTO req = JsonConvert.DeserializeObject<AF_PersonaEscolaridadRegistraDTO>(request) ?? new AF_PersonaEscolaridadRegistraDTO();

            var response = new ErrorDto
            {
                Code = 0,
                Description = "Operación realizada correctamente"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var p = new DynamicParameters();

                string codEscolaridad;
                if (req.CodEscolaridad.Contains("."))
                {
                    var partes = req.CodEscolaridad.Split('.');
                    var entero = partes[0].PadLeft(2, '0');
                    codEscolaridad = $"{entero}.{partes[1]}";
                }
                else
                {
                    codEscolaridad = req.CodEscolaridad.PadLeft(2, '0');
                }

                p.Add("@Cedula", req.Cedula);
                p.Add("@Codigo", codEscolaridad);
                p.Add("@TipoMov", req.Asignado ? "A" : "E");
                p.Add("@Usuario", req.Usuario);

                connection.Execute("dbo.spAFI_Persona_Escolaridad_Registra", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        /// <summary>
        /// Registra la preferencia de una persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Preferencia_Registra(int CodEmpresa, string request)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            CR_PreferenciaDTO req = JsonConvert.DeserializeObject<CR_PreferenciaDTO>(request) ?? new CR_PreferenciaDTO();

            var response = new ErrorDto
            {
                Code = 0,
                Description = "Operación realizada correctamente"
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                var p = new DynamicParameters();

                string CodPreferencia;
                if (req.CodPreferencia.Contains("."))
                {
                    var partes = req.CodPreferencia.Split('.');
                    var entero = partes[0].PadLeft(2, '0');
                    CodPreferencia = $"{entero}.{partes[1]}";
                }
                else
                {
                    CodPreferencia = req.CodPreferencia.PadLeft(2, '0');
                }

                p.Add("@Cedula", req.Cedula);
                p.Add("@Codigo", CodPreferencia);
                p.Add("@TipoMov", req.Asignado ? "A" : "E");
                p.Add("@Usuario", req.Usuario);

                connection.Execute("dbo.spAFI_Persona_Preferencias_Registra", p, commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        #endregion

        #region Estado

        public ErrorDto<EmpresaEnlaceResultDTO> ConsultaVersionEmpresa(int codEmpresa)
        {
            var resp = new ErrorDto<EmpresaEnlaceResultDTO>
            {
                Code = 0,
                Description = "Ok",
                Result = new EmpresaEnlaceResultDTO()
            };

            resp.Result = EmpresaEnlaceObtener(codEmpresa)[0];

            return resp;
        }

        public List<EmpresaEnlaceResultDTO> EmpresaEnlaceObtener(int CodEmpresa)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            List<EmpresaEnlaceResultDTO> result = new List<EmpresaEnlaceResultDTO>();

            string strSQL = "";
            try
            {
                using (var connection = new SqlConnection(stringConn))
                {
                    strSQL = $@"select 
                                    cod_empresa_enlace,
                                    Nombre,
                                    SysCrdPlanPago,
                                    SysDocVersion,
                                    SysTesVersion, 
                                    SYS_CCSS_IND,
                                    ec_visible_patrimonio,
                                    ec_visible_fondos,
                                    ec_visible_creditos,
                                    ec_visible_fianzas,
                                    estadoCuenta
                              from dbo.sif_empresa";
                    result = connection.Query<EmpresaEnlaceResultDTO>(strSQL).ToList();
                }
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }

        #endregion

        #region @

        /// <summary>
        /// Método que obtiene el correo y los periodos de cierre disponibles para un socio
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<SocioCierresData> Email_SocioPeriodos_Obtener(int CodEmpresa, string cedula)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(CodEmpresa);
            var response = new ErrorDto<SocioCierresData>
            {
                Code = 0,
                Description = "Ok",
                Result = new SocioCierresData()
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    string query = @"select rtrim(isnull(AF_Email,'')) as 'Email' from socios where cedula = @cedula";
                    response.Result.email = connection.Query<string>(query, new { cedula = cedula }).FirstOrDefault();

                    query = "exec spSys_Periodos_Cierre_Consulta";
                    var periodosList = connection.Query<SociosPeriodoData>(query).ToList();
                    response.Result.periodos = periodosList
                        .Select(p => new DropDownListaGenericaModel
                        {
                            item = p.itmx.ToString(),
                            descripcion = p.idx.ToString()
                        })
                        .ToList();
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

        public ErrorDto Email_SocioEstadoCuenta_Enviar(int CodEmpresa, string usuario,string cedula, string email, string periodo, string tipo) 
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
                    if(tipo == "T")
                    {
                        string query = @"exec spuProGrX_MOBILE_CUENTAS_ENVIAESTADO @cedula";
                        var resp = connection.Query(query, new { cedula = cedula });

                        _Security_MainDB.Bitacora(new BitacoraInsertarDTO
                        {
                            EmpresaId = CodEmpresa,
                            Usuario = usuario,
                            DetalleMovimiento = $"Estado de Cuenta: [email]: {email}",
                            Movimiento = "Aplica - WEB",
                            Modulo = 10
                        });

                        response.Description = "Estado de Cuenta enviado al Correo Electrónico registrado de la persona!";
                    }
                    else
                    {
                        Nullable<DateTime> vCorte = null;
                        if (!string.IsNullOrEmpty(periodo))
                        {
                            vCorte = DateTime.Parse(periodo);
                        }
                       
                        response = _mProGrx_Main.sbEstadoCuenta_Email_Corte(CodEmpresa, usuario, cedula, email, vCorte);
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

        #region Aut/C.I

       /// <summary>
       /// Registra consentimiento de la persona
       /// </summary>
       /// <param name="codEmpresa"></param>
       /// <param name="cedula"></param>
       /// <param name="usuario"></param>
       /// <returns></returns>
        public ErrorDto CR_RegistraConsentimiento(int codEmpresa, string cedula, string usuario)
        {
            string stringConn = new PortalDB(_config).ObtenerDbConnStringEmpresa(codEmpresa);
            var response = new ErrorDto
            {
                Code = 0
            };

            try
            {
                using var connection = new SqlConnection(stringConn);
                {
                    var parameters = new DynamicParameters();
                    parameters.Add("@Cedula", cedula);
                    parameters.Add("@Indicador", 29);
                    parameters.Add("@Valor", 1);
                    parameters.Add("@Usuario", usuario);

                    connection.Execute("spAFI_Persona_Indicadores", parameters, commandType: CommandType.StoredProcedure);

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
