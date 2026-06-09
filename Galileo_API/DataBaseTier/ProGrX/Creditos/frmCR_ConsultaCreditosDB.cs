using Dapper;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX.Cajas;
using Galileo.Models.ProGrX.Clientes;
using Galileo.Models.ProGrX.Credito;
using System.Data;
using System.Linq;
using Galileo.Models.Security;

namespace Galileo.DataBaseTier.ProGrX.Credito
{
    public class FrmCRConsultaCreditosDB
    {
        private readonly IConfiguration _config;
        private readonly MProGrxMain _mProGrx_Main;
        private readonly MSecurityMainDb _Security_MainDB;
        private const string FormatoFechaIso = "yyyy-MM-dd";
        private const string MensajeOperacionRealizadaCorrectamente = "Operación realizada correctamente";

        public FrmCRConsultaCreditosDB(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _mProGrx_Main = new MProGrxMain(_config);
            _Security_MainDB = new MSecurityMainDb(_config);
        }

        /// <summary>
        /// Consulta los tipos de garantía disponibles para el formulario en la tabla CRD_GARANTIA_TIPOS.
        /// </summary>
        public ErrorDto<List<DropDownListaGenericaModel>> CR_ConsultaCrdGarantiaTipo_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                @"select GARANTIA as item,
                         rtrim(DESCRIPCION) as descripcion
                  from CRD_GARANTIA_TIPOS
                  where FORMULARIO = 'F01'
                  order by Garantia");
        }

        /// <summary>
        /// Consulta los socios disponibles para el formulario.
        /// </summary>
        public ErrorDto<List<CrConsultaCrdSociosData>> CR_ConsultaCrdSocios_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<CrConsultaCrdSociosData>(
                CreatePortalDb(),
                CodEmpresa,
                "Select cedula, cedular, nombre from SOCIOS");
        }

        /// <summary>
        /// Consulta los datos de la persona para el formulario de consulta integrada.
        /// </summary>
        public ErrorDto<CrConsultaCrdData> CR_ConsultaCrdConsulta_Integrada_Obtener(int CodEmpresa, string cedula, string usuario)
        {
            var response = new ErrorDto<CrConsultaCrdData>
            {
                Code = 0,
                Description = "Ok",
                Result = new CrConsultaCrdData()
            };

            var cedulaNormalizada = (cedula ?? string.Empty).Trim();
            var usuarioNormalizado = usuario ?? string.Empty;

            var validaCadena = _mProGrx_Main.fxSIFValidaCadena(cedulaNormalizada);
            if (validaCadena.Code == -1)
            {
                response.Code = validaCadena.Code;
                response.Description = validaCadena.Description;
                return response;
            }

            var vRA_Access = _mProGrx_Main.fxSys_RA_Consulta(CodEmpresa, cedulaNormalizada, usuarioNormalizado);
            if (!vRA_Access.Result)
            {
                response.Code = -1;
                response.Description = "Esta persona se encuentra con -> Expediente Restringido <- Requiere de Autorización para Consultar!";
                return response;
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var persona = connection.QueryFirstOrDefault<CrConsultaCrdData>(
                    "spSys_Consulta_Integrada",
                    new { cedula = cedulaNormalizada },
                    commandType: CommandType.StoredProcedure);

                if (persona is null)
                {
                    return DbHelper.CreateErrorResponse("No se encontró información de la persona.", -1, new CrConsultaCrdData());
                }

                PrepararConsultaIntegrada(connection, CodEmpresa, cedulaNormalizada, persona);
                return DbHelper.CreateOkResponse(persona);
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al consultar información integrada.", result.Code.GetValueOrDefault(-1), new CrConsultaCrdData());
        }

        /// <summary>
        /// Obtiene la causa de liquidación más reciente de un socio.
        /// </summary>
        private string fxLiquidacion(int CodEmpresa, string cedula)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.QueryFirstOrDefault<string>(
                    @"select C.descripcion
                      from liquidacion L
                      inner join Causas_Renuncias C on C.id_causa = L.id_causa
                      where consec in(
                          select max(consec)
                          from liquidacion
                          where cedula = @cedula)",
                    new { cedula }));

            if (result.Code != 0 || string.IsNullOrWhiteSpace(result.Result))
            {
                return string.Empty;
            }

            return $"[CAUSA: {result.Result}]";
        }

        /// <summary>
        /// Método actualiza nota socio.
        /// </summary>
        public ErrorDto CR_Socios_RegistrarNota(int CodEmpresa, string cedula, string nota, string usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                const string query = @"
                UPDATE socios
                   SET notas = UPPER(LTRIM(RTRIM(@Nota))),
                       Nota_User = @Usuario,
                       Nota_Fecha = dbo.MyGetdate()
                 WHERE cedula = @Cedula;

                INSERT INTO socios_mensajes (fecha, cedula, usuario, vencimiento, mensaje, tipo)
                VALUES (dbo.MyGetdate(), @Cedula, @Usuario, '2100-01-01', @Nota, 'G');";

                connection.Execute(query, new
                {
                    Cedula = cedula,
                    Usuario = usuario,
                    Nota = nota
                });

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al registrar nota del socio.", result.Code.GetValueOrDefault(-1));
        }

        public ErrorDto<decimal> fxCajas_SaldoaFavor(int CodEmpresa, string cedula)
        {
            var result = DbHelper.ExecuteSingleQuery<decimal>(
                CreatePortalDb(),
                CodEmpresa,
                "select dbo.fxCajas_SaldoaFavor(@cedula) as Cajas_Saldo_Favor",
                0m,
                new { cedula });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result)
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al consultar saldo a favor.", result.Code.GetValueOrDefault(-1), 0m);
        }

        #region Créditos

        /// <summary>
        /// Método para consultar Activos y Cancelados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaCrdCreditosData>> CR_ConsultaCrd_Creditos_Obtener(int CodEmpresa, string cedula, string sheetName)
        {
            return EjecutarStoredProcedureList<CrConsultaCrdCreditosData>(
                CodEmpresa,
                "spSys_Consulta_Integrada_Creditos",
                new { Cedula = cedula, Estado = sheetName });
        }

        /// <summary>
        /// Consulta tramite credito
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaCrdSolicitudData>> CR_ConsultaCrd_Tramite_Obtener(int CodEmpresa, string cedula, string sheetName)
        {
            return EjecutarStoredProcedureList<CrConsultaCrdSolicitudData>(
                CodEmpresa,
                "spSIFEstadoSolicitud",
                new { Cedula = cedula });
        }

        /// <summary>
        /// Consulta tramite credito
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaCreditosData>> CR_ConsultaCrd_Tramite_Obtener(int CodEmpresa, string cedula)
        {
            return EjecutarStoredProcedureList<CrConsultaCreditosData>(
                CodEmpresa,
                "spSIFEstadoSolicitud",
                new { Cedula = cedula });
        }

        /// <summary>
        /// Obtiene creditos en PreAnalisis
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaCrdPreanalisisData>> CR_ConsultaCrd_PreAnalisis_Obtener(int CodEmpresa, string cedula)
        {
            return EjecutarStoredProcedureList<CrConsultaCrdPreanalisisData>(
                CodEmpresa,
                "spSIFEstadoPreAnalisis",
                new { Cedula = cedula });
        }

        /// <summary>
        /// Obtiene creditos en Incobrable
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaCrdIncobrableData>> CR_ConsultaCrd_Incobrable_Obtener(int CodEmpresa, string cedula)
        {
            return EjecutarStoredProcedureList<CrConsultaCrdIncobrableData>(
                CodEmpresa,
                "spSIFEstadoIncobrable",
                new { Cedula = cedula });
        }

        #endregion

        #region Cobros

        /// <summary>
        /// Obtiene los cobros de una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaCobroDto>> CR_ConsultaCobros_Obtener(int codEmpresa, string cedula)
        {
            return DbHelper.ExecuteListQuery<CrConsultaCobroDto>(
                CreatePortalDb(),
                codEmpresa,
                @"
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
                ORDER BY S.cod_seg DESC;",
                new { Cedula = cedula });
        }

        /// <summary>
        /// Consulta Asignacion de Oficina de Cobro
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<CrConsultaAsignacionCobroData>> CR_ConsultaAsignacion_Obtener(int codEmpresa, string cedula)
        {
            return DbHelper.ExecuteListQuery<CrConsultaAsignacionCobroData>(
                CreatePortalDb(),
                codEmpresa,
                @"
                SELECT 
                    usuario,
                    cedula,
                    fecha_asignacion,
                    mantener,
                    rebajo_doble,
                    aplica_mora
                FROM CBR_Asignacion_H
                WHERE cedula = @Cedula
                ORDER BY fecha_asignacion DESC;",
                new { Cedula = cedula });
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
            return EjecutarStoredProcedureList<CrConsultaContratosData>(
                codEmpresa,
                "spFndContratosConsulta",
                new { Cedula = cedula, Usuario = usuario });
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
            return DbHelper.ExecuteListQuery<CrContratosMovimientosData>(
                CreatePortalDb(),
                codEmpresa,
                @"
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
                ORDER BY Det.Fecha DESC, Det.COD_fnd_detalle DESC;",
                new
                {
                    CodOperadora = codOperadora,
                    CodPlan = codPlan,
                    CodContrato = codContrato
                });
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
            return DbHelper.ExecuteListQuery<CrContratosCuponesData>(
                CreatePortalDb(),
                codEmpresa,
                @"
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
                ORDER BY Fecha_Vence;",
                new
                {
                    CodOperadora = codOperadora,
                    CodPlan = codPlan,
                    CodContrato = codContrato
                });
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
            return DbHelper.ExecuteListQuery<CrContratosBitacoraData>(
                CreatePortalDb(),
                codEmpresa,
                @"
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
                ORDER BY C.fecha DESC;",
                new
                {
                    CodOperadora = codOperadora,
                    CodPlan = codPlan,
                    CodContrato = codContrato
                });
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
            return DbHelper.ExecuteListQuery<CrContratosCierresData>(
                CreatePortalDb(),
                codEmpresa,
                @"
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
                ORDER BY A.Anio DESC, A.Mes DESC;",
                new
                {
                    CodOperadora = codOperadora,
                    CodPlan = codPlan,
                    CodContrato = codContrato
                });
        }

        /// <summary>
        /// Obtiene si la sesion esta activa o no
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="identificacion"></param>
        /// <returns></returns>
        public ErrorDto<CajasSesionDto> Cajas_Sesion_ObtenerActiva(int codEmpresa, string usuario, string identificacion)
        {
            var result = DbHelper.ExecuteSingleQuery<CajasSesionDto>(
                CreatePortalDb(),
                codEmpresa,
                @"SELECT TOP 1 *
                  FROM CAJAS_SESION
                  WHERE cod_usuario = @Usuario
                    AND estado = 1
                    AND identificacion = @Identificacion",
                null,
                new { Usuario = usuario, Identificacion = identificacion });

            if (result.Code != 0)
            {
                return DbHelper.CreateErrorResponse<CajasSesionDto>(result.Description ?? "Error al consultar sesión activa.", result.Code.GetValueOrDefault(-1), null!);
            }

            return result.Result is not null
                ? DbHelper.CreateOkResponse(result.Result)
                : DbHelper.CreateErrorResponse<CajasSesionDto>("No se encontró sesión activa.", -2, null!);
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
            return DbHelper.ExecuteListQuery<CrPatrimonioData>(
                CreatePortalDb(),
                codEmpresa,
                @"
                SELECT TOP 30
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
                  AND (@Tipo = 'T' OR Ah.Tipo = @Tipo)
                ORDER BY Ah.Fecha DESC;",
                new { Cedula = cedula, Tipo = tipo });
        }

        /// <summary>
        /// Obtiene los periodos visibles para un socio
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<ExcPeriodosVisiblesData>> EXC_Periodos_Visibles_Obtener(int codEmpresa, string cedula)
        {
            return EjecutarStoredProcedureList<ExcPeriodosVisiblesData>(
                codEmpresa,
                "spEXC_Periodos_Visibles",
                new { Cedula = cedula });
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
            return EjecutarStoredProcedureList<AfiBeneficiosConsultaData>(
                codEmpresa,
                "spAFI_Beneficios_Consulta",
                new { Cedula = cedula });
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
            return EjecutarStoredProcedureList<AfiRenunciaTransitoData>(
                codEmpresa,
                "spAFI_ConsultaRenunciaTransito",
                new { Cedula = cedula });
        }

        /// <summary>
        /// Obtiene las renuncias de una persona
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="cedula"></param>
        /// <returns></returns>
        public ErrorDto<List<AfiRenunciasConsultaData>> AFI_Renuncias_Consulta(int codEmpresa, string cedula)
        {
            return EjecutarStoredProcedureList<AfiRenunciasConsultaData>(
                codEmpresa,
                "spAFI_Renuncias_Consulta",
                new { Cedula = cedula });
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
                //Valida si existe
                var query = @"SELECT COUNT('X') FROM socios_mensajes where cedula = @cedula 
                         and vencimiento = @fecha 
                         and substring(mensaje,1,15) = substring(@mensaje,1,15) 
                         and usuario = @usuario 
                         and Tipo = 'G'
                         and resolucion = 'P'";
                string vfecha = MProGrXAuxiliarDB.validaFechaGlobal(data.vencimiento, FormatoFechaIso) ?? string.Empty;
                string vfechaReg = MProGrXAuxiliarDB.validaFechaGlobal(data.fecha, FormatoFechaIso) ?? string.Empty;

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
                var query = @"
                       delete from socios_mensajes 
                       where cedula = @cedula 
                         and vencimiento = @fecha 
                         and substring(mensaje,1,15) = substring(@mensaje,1,15) 
                         and usuario = @usuario 
                         and Tipo = 'G'
                         and resolucion = 'P'
                    ";

                string vfecha = MProGrXAuxiliarDB.validaFechaGlobal(data.vencimiento, FormatoFechaIso) ?? string.Empty;

                connection.ExecuteAsync(query, new
                {
                    cedula = data.cedula,
                    usuario = data.usuario,
                    fecha = vfecha,
                    mensaje = data.mensaje
                });
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }

        public ErrorDto AFI_Socios_Mensajes_Resolucion(int codEmpresa, string usuario, AfiSociosMensajesData data)
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
                var query = @"
                       update socios_mensajes set Resolucion = 'R', Resolucion_Fecha = dbo.MyGetdate()
                          , Resolucion_Usuario = @usuario
                           where cedula = @cedula 
                           and usuario = @userMsj
                           and vencimiento = @fecha_vence
                           and substring(mensaje,1,15) = substring(@mensaje,1,15)";

                string vfecha = MProGrXAuxiliarDB.validaFechaGlobal(data.vencimiento, FormatoFechaIso) ?? string.Empty;

                connection.ExecuteAsync(query, new
                {
                    cedula = data.cedula,
                    usuario = usuario,
                    userMsj = data.usuario,
                    fecha_vence = vfecha,
                    mensaje = data.mensaje
                });
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
            return EjecutarStoredProcedureList<SysMailLoadData>(
                codEmpresa,
                "spSys_Mail_Load",
                new { Cedula = cedula });
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
        public ErrorDto<CRConsultaInfoDto> AF_Persona_Consulta_Obtener(int CodEmpresa, string cedula, string usuario)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var parametros = new { Cedula = cedula, Usuario = usuario };
                using var multi = connection.QueryMultiple(
                    "spCR_InfoPersona_Consulta",
                    param: parametros,
                    commandType: CommandType.StoredProcedure);

                Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

                return new CRConsultaInfoDto
                {
                    Telefonos = multi.Read<AfTelefonoDto>().ToList(),
                    CuentasBancarias = multi.Read<AfCuentaBancariaDto>().ToList(),
                    Ingresos = multi.Read<AfPersonaIngresoDto>().ToList(),
                    Liquidaciones = multi.Read<CRliquidacionDto>().ToList(),
                    Beneficiarios = multi.Read<AfPersonaBeneficiarioDto>().ToList(),
                    Canales = multi.Read<AfCanalesDto>().ToList(),
                    Bienes = multi.Read<AfBienDto>().ToList(),
                    Escolaridad = multi.Read<AfEscolaridadDto>().ToList(),
                    Contacto = multi.Read<AFPersonaDetalleDto>().ToList(),
                    EstadoLaboral = multi.Read<AFPersonaEstadoLaboralDto>().ToList(),
                    BenePolizas = multi.Read<AFPersonaBenePolizaDto>().ToList(),
                    Preferencias = multi.Read<CrPreferenciaDto>().ToList()
                };
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new CRConsultaInfoDto())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al consultar información de la persona.", result.Code.GetValueOrDefault(-1), new CRConsultaInfoDto());
        }

        public ErrorDto AF_Persona_Canales_Registra(int CodEmpresa, string req)
        {
            AfCanalesDto request = JsonConvert.DeserializeObject<AfCanalesDto>(req) ?? new AfCanalesDto();
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var p = new DynamicParameters();
                p.Add("@Cedula", request.cedula);
                p.Add("@Canal", request.canal_tipo.ToString("D2"));
                p.Add("@TipoMov", request.asignado ? "A" : "E");
                p.Add("@Usuario", request.registro_usuario);
                connection.Execute("dbo.spAFI_Persona_Canales_Registra", p, commandType: CommandType.StoredProcedure);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse(MensajeOperacionRealizadaCorrectamente)
                : DbHelper.ErrorResponse(result.Description ?? "Error al registrar canales de la persona.", result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Registra bienes de la persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Bienes_Registra(int CodEmpresa, string req)
        {
            AfPersonaBienesRegistraDto request = JsonConvert.DeserializeObject<AfPersonaBienesRegistraDto>(req) ?? new AfPersonaBienesRegistraDto();
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var p = new DynamicParameters();
                p.Add("@Cedula", request.Cedula);
                p.Add("@Codigo", FormatearCodigoCompuesto(request.CodBien));
                p.Add("@TipoMov", request.Asignado ? "A" : "E");
                p.Add("@Usuario", request.Usuario);
                connection.Execute("dbo.spAFI_Persona_Bienes_Registra", p, commandType: CommandType.StoredProcedure);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse(MensajeOperacionRealizadaCorrectamente)
                : DbHelper.ErrorResponse(result.Description ?? "Error al registrar bienes de la persona.", result.Code.GetValueOrDefault(-1));
        }


        /// <summary>
        /// Registra escolaridad de la persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Escolaridad_Registra(int CodEmpresa, string request)
        {
            AfPersonaEscolaridadRegistraDto req = JsonConvert.DeserializeObject<AfPersonaEscolaridadRegistraDto>(request) ?? new AfPersonaEscolaridadRegistraDto();
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var p = new DynamicParameters();
                p.Add("@Cedula", req.Cedula);
                p.Add("@Codigo", FormatearCodigoCompuesto(req.CodEscolaridad));
                p.Add("@TipoMov", req.Asignado ? "A" : "E");
                p.Add("@Usuario", req.Usuario);
                connection.Execute("dbo.spAFI_Persona_Escolaridad_Registra", p, commandType: CommandType.StoredProcedure);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse(MensajeOperacionRealizadaCorrectamente)
                : DbHelper.ErrorResponse(result.Description ?? "Error al registrar escolaridad de la persona.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Registra la preferencia de una persona
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto AF_Persona_Preferencia_Registra(int CodEmpresa, string request)
        {
            CrPreferenciaDto req = JsonConvert.DeserializeObject<CrPreferenciaDto>(request) ?? new CrPreferenciaDto();
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                var p = new DynamicParameters();
                p.Add("@Cedula", req.Cedula);
                p.Add("@Codigo", FormatearCodigoCompuesto(req.CodPreferencia.ToString()));
                p.Add("@TipoMov", req.asignado ? "A" : "E");
                p.Add("@Usuario", req.Usuario);
                connection.Execute("dbo.spAFI_Persona_Preferencias_Registra", p, commandType: CommandType.StoredProcedure);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse(MensajeOperacionRealizadaCorrectamente)
                : DbHelper.ErrorResponse(result.Description ?? "Error al registrar preferencia de la persona.", result.Code.GetValueOrDefault(-1));
        }

        #endregion

        #region Estado

        public ErrorDto<EmpresaEnlaceResultDto> ConsultaVersionEmpresa(int codEmpresa)
        {
            var lista = EmpresaEnlaceObtener(codEmpresa);
            return lista.Count > 0
                ? DbHelper.CreateOkResponse(lista[0])
                : DbHelper.CreateErrorResponse("No se encontró información de la empresa.", -1, new EmpresaEnlaceResultDto());
        }

        public List<EmpresaEnlaceResultDto> EmpresaEnlaceObtener(int CodEmpresa)
        {
            var result = DbHelper.ExecuteListQuery<EmpresaEnlaceResultDto>(
                CreatePortalDb(),
                CodEmpresa,
                @"select 
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
                  from dbo.sif_empresa");

            return result.Code == 0 ? result.Result ?? new List<EmpresaEnlaceResultDto>() : new List<EmpresaEnlaceResultDto>();
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
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                string email = connection.QueryFirstOrDefault<string>(
                    "select rtrim(isnull(AF_Email,'')) as Email from socios where cedula = @cedula",
                    new { cedula }) ?? string.Empty;

                var periodosList = connection.Query<SociosPeriodoData>(
                    "spSys_Periodos_Cierre_Consulta",
                    commandType: CommandType.StoredProcedure).ToList();

                return new SocioCierresData
                {
                    email = email,
                    periodos = periodosList
                        .Select(p => new DropDownListaGenericaModel
                        {
                            item = p?.itmx?.ToString() ?? string.Empty,
                            descripcion = p?.idx?.ToString() ?? string.Empty
                        })
                        .ToList()
                };
            });

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new SocioCierresData())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al consultar correo y periodos del socio.", result.Code.GetValueOrDefault(-1), new SocioCierresData());
        }

        public ErrorDto Email_SocioEstadoCuenta_Enviar(int CodEmpresa, string usuario, string cedula, string email, string periodo, string tipo)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                if (tipo == "T")
                {
                    connection.Query(
                        "spuProGrX_MOBILE_CUENTAS_ENVIAESTADO",
                        new { cedula },
                        commandType: CommandType.StoredProcedure);

                    _Security_MainDB.Bitacora(new BitacoraInsertarDto
                    {
                        EmpresaId = CodEmpresa,
                        Usuario = usuario,
                        DetalleMovimiento = $"Estado de Cuenta: [email]: {email}",
                        Movimiento = "Aplica - WEB",
                        Modulo = 10
                    });

                    return DbHelper.OkResponse("Estado de Cuenta enviado al Correo Electrónico registrado de la persona!");
                }

                DateTime? vCorte = string.IsNullOrEmpty(periodo) ? null : DateTime.Parse(periodo, System.Globalization.CultureInfo.InvariantCulture);
                return _mProGrx_Main.sbEstadoCuenta_Email_Corte(CodEmpresa, usuario, cedula, email, vCorte);
            });

            return result.Code == 0 && result.Result is not null
                ? result.Result
                : DbHelper.ErrorResponse(result.Description ?? "Error al enviar estado de cuenta.", result.Code.GetValueOrDefault(-1));
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
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Cedula", cedula);
                parameters.Add("@Indicador", 29);
                parameters.Add("@Valor", 1);
                parameters.Add("@Usuario", usuario);

                connection.Execute("spAFI_Persona_Indicadores", parameters, commandType: CommandType.StoredProcedure);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al registrar consentimiento.", result.Code.GetValueOrDefault(-1));
        }


        #endregion

        private void PrepararConsultaIntegrada(SqlConnection connection, int codEmpresa, string cedula, CrConsultaCrdData persona)
        {
            persona.vMora = false;
            DateTime vFechaIng = persona.fechaingreso ?? DateTime.Now;

            persona.membresiaCaption = "Membresía: NADA";
            persona.membresiaToolTip = fxLiquidacion(codEmpresa, cedula);

            if (persona.estadoactual == "S")
            {
                persona.membresiaCaption = "Membresía: " + MCredito.fxMembresia(vFechaIng);
                persona.membresiaToolTip = "[Ing.:" + vFechaIng.ToString("g");

                var renuncias = connection.QueryFirstOrDefault<CrConsultaCrdData>(
                    "spAFI_ConsultaRenunciaTransito",
                    new { cedula },
                    commandType: CommandType.StoredProcedure);

                if (renuncias != null)
                {
                    persona.membresiaCaption = $"Renuncia: {renuncias.cod_Renuncia} ¦ {renuncias.registro_fecha} ¦ {renuncias.registro_user}";
                    persona.membresiaToolTip = $"{renuncias.estado} ¦ {renuncias.tipo} ¦ {renuncias.descripcion}";
                }
            }

            persona.clasificacionCaption = $"Clasificación Crediticia : [{persona.clasificacion}]";
            persona.salarioTrasladaCaption = persona.salario_traslada == 1 ? "Traslada Salario: Sí" : "Sin Tramite (Traslado Salario)";
            persona.patrimonio = persona.ahorro + persona.aporte + persona.custodia + persona.capitaliza;
            persona.tarjetaCaption = $"Tarjeta: {persona.tarjeta_numero}";
            persona.ibanCaption = $"IBAN: {persona.iban}";
            persona.estadoMensajesCaption = persona.indmensajes == 0 ? "Mensajes ?" : $"Mensajes ({persona.indmensajes})";
            persona.estadoCobrosCaption = persona.indcobro == 0 ? "Sin Gestión de Cobro" : $"Gestiones de Cobro ({persona.indcobro})";
            persona.estadoAdvertenciaCaption = persona.indadvertencias == 0 ? "Sin Advertencias" : $"Advertencias ({persona.indadvertencias})";

            if (persona.consentimiento_contacto_fecha != null)
            {
                string vFecha = ((DateTime)persona.consentimiento_contacto_fecha).ToString("dd/MM/yyyy");
                persona.estadoConsentimientoToolTip = $"Fecha : {vFecha} | Usuario: {persona.consentimiento_contacto_usuario}";
            }
            else
            {
                persona.estadoConsentimientoToolTip = string.Empty;
                persona.consentimiento_contacto_usuario = null;
            }

            if (!string.IsNullOrWhiteSpace(persona.pat_advertencia))
            {
                persona.estadoAdvertenciaCaption = "Advertencia de Aportes no cotizados";
            }

            persona.fianzasCaption = persona.indfianzas == false ? "Fianzas al Día" : "Fianzas en Mora";
            CargarMensajesPersona(connection, cedula, persona);
            persona.pat_tipoSaldo = "Saldos en Garantía";

            var listCredito = CR_ConsultaCrd_Creditos_Obtener(codEmpresa, cedula, "C");
            foreach (CrConsultaCrdCreditosData credito in (listCredito.Result ?? new List<CrConsultaCrdCreditosData>()).Where(c => c.procesoCod == "J"))
            {
                persona.vMora = true;
                persona.vMoraCaption = $">> Cobro Judicial << | Fecha : {credito.fecha_enviaProceso} | Nota : {credito.observacion_proceso}";
            }
        }

        private static void CargarMensajesPersona(SqlConnection connection, string cedula, CrConsultaCrdData persona)
        {
            var mensajes = connection.QueryFirstOrDefault<CrConsultaCrdData>(
                "spSIFPersonaMensajes",
                new { cedula },
                commandType: CommandType.StoredProcedure);

            if (mensajes != null)
            {
                persona.pendientes = mensajes.pendientes;
                persona.advertencias = mensajes.advertencias;
                persona.generales = mensajes.generales;
                persona.morosidad = mensajes.morosidad;
                persona.bloqueos = mensajes.bloqueos;
            }

            persona.pendientesCaption = persona.pendientes > 0 ? $"Pendientes ({persona.pendientes})" : "Msj. Pendientes?";
            persona.advertenciasCaption = persona.advertencias > 0 ? $"Advertencias ({persona.advertencias})" : "Msj Advertencias?";
            persona.generalesCaption = persona.generales > 0 ? $"General ({persona.generales})" : "Msj Generales?";
            persona.morosidadCaption = persona.morosidad > 0 ? $"Morosidad ({persona.morosidad})" : "Msj Morosidad?";
            persona.bloqueosCaption = persona.bloqueos > 0 ? $"Bloqueos ({persona.bloqueo})" : "Msj Bloqueos?";
        }

        private ErrorDto<List<T>> EjecutarStoredProcedureList<T>(int codEmpresa, string storedProcedure, object parameters)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), codEmpresa, connection =>
                connection.Query<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<T>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al ejecutar procedimiento almacenado.", result.Code.GetValueOrDefault(-1), new List<T>());
        }

        private static string FormatearCodigoCompuesto(string? codigo)
        {
            var valor = codigo ?? string.Empty;
            if (valor.Contains('.'))
            {
                var partes = valor.Split('.');
                var entero = partes[0].PadLeft(2, '0');
                return $"{entero}.{partes[1]}";
            }

            return valor.PadLeft(2, '0');
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}