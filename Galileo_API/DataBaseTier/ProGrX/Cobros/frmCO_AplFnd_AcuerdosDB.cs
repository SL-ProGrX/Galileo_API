using Dapper; 
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo.Models.Security; 
using System.Data;
using static Galileo_API.Models.ProGrX.Cobros.FrmCoAplFndAcuerdosModels;


namespace Galileo_API.DataBaseTier.ProGrX.Cobros
{
    public class FrmCoAplFndAcuerdosDB
    {
        private readonly PortalDB _portalDB;
        private const string SpConsulta = "spCbr_Fondos_Apl_Acuerdos_Consulta";
        private const string SpListado = "spCbr_Fondos_Apl_Acuerdos_List";
        private const string SpGuardar = "spCbr_Fondos_Apl_Acuerdos_Add";
        private readonly MSecurityMainDb _Security_MainDB;

        // Módulo para bitácora
        private const int ModuloBitacora = 36;
        public FrmCoAplFndAcuerdosDB(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
            _Security_MainDB = new MSecurityMainDb(config);
        }

        private void RegistrarBitacora(int codEmpresa, string usuario, string detalle, string movimiento)
        {
            _Security_MainDB.Bitacora(new BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario ?? string.Empty,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = ModuloBitacora
            });
        }

        /// <summary>
        /// Mapeo de parametros para guardar un nuevo  caso con Acuerdos de Aplicación de Fondos a Mora
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static DynamicParameters BuildGuardarParameters(CoAplFndAcuerdosDetalleResponse request)
        {
            var parameters = new DynamicParameters();
            parameters.Add("@Acuerdo", request.Id_Acuerdo);
            parameters.Add("@Cedula", request.Cedula.Trim());
            parameters.Add("@Firma", request.Firma_Boleta.Date);
            parameters.Add("@Activo", request.Estado);
            parameters.Add("@ICreditos", request.Ind_Creditos);
            parameters.Add("@IObrero", request.Ind_Obrero);
            parameters.Add("@ISobres", request.Ind_Sobres);
            parameters.Add("@IAbono", request.Ind_Abono);
            parameters.Add("@Notas", request.Observaciones.Trim());
            parameters.Add("@Usuario", request.Usuario.Trim());
            return parameters;
        }
        /// <summary>
        ///  Mapeo de parametros para la carga de caso con Acuerdos de Aplicación de Fondos a Mora
        /// </summary>
        /// <param name="item"></param>
        /// <param name="usuario"></param>
        /// <returns></returns>
        private static CoAplFndAcuerdosDetalleResponse MapCargaItemRequest(CoAplFndAcuerdosCargaItemRequest item,string usuario)
        {
            return new CoAplFndAcuerdosDetalleResponse
            {
                Id_Acuerdo = 0,
                Cedula = item.Cedula,
                Firma_Boleta = item.Fecha_Firma,
                Estado = item.Activo,
                Ind_Creditos =item.Apl_Creditos,
                Ind_Obrero =item.Apl_Obrero,
                Ind_Sobres =item.Apl_Sobres,
                Ind_Abono = item.Apl_Abonos_ord,
                Observaciones = item.Notas,
                Usuario = usuario
            };
        }

        /// <summary>
        ///  Validaciones para el proceso de guardar
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private static string ValidarGuardarRequest(CoAplFndAcuerdosDetalleResponse request)
        {
            if (string.IsNullOrWhiteSpace(request.Cedula))
            {
                return "La cédula es requerida.";
            }

            if (string.IsNullOrWhiteSpace(request.Usuario))
            {
                return "El usuario es requerido.";
            }

            return string.Empty;
        }

        /// <summary>
        ///  Validaciones para el proceso de carga masiva
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static string ValidarCargaItem(CoAplFndAcuerdosCargaItemRequest item)
        {
            if (string.IsNullOrWhiteSpace(item.Cedula))
            {
                return "La cédula es requerida.";
            }

            return string.Empty;
        }

        /// <summary>
        ///  Consulta el detalle de un acuerdo de aplicación de fondos por Id.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="idAcuerdo"></param>
        /// <returns></returns>
        public ErrorDto<CoAplFndAcuerdosDetalleResponse> Co_AplFnd_Acuerdos_Consultar(int codEmpresa, int idAcuerdo)
        {
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Acuerdo", idAcuerdo);

                var result = conn.QueryFirstOrDefault<CoAplFndAcuerdosDetalleResponse>(
                    SpConsulta,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                return result ?? new CoAplFndAcuerdosDetalleResponse();
            });
        }

        /// <summary>
        /// Lista acuerdos de aplicación de fondos por filtro y estado.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<List<CoAplFndAcuerdosGridResponse>> Co_AplFnd_Acuerdos_Listar(int codEmpresa, CoAplFndAcuerdosFiltroRequest request)
        {
            
            return DbHelper.WithConn(_portalDB, codEmpresa, conn =>
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Filtro", request.Filtro?.Trim());
                parameters.Add("@Activo", request.Estado);


                return conn.Query<CoAplFndAcuerdosGridResponse>(
                    SpListado,
                    parameters,
                    commandType: CommandType.StoredProcedure
                ).ToList();
            });
        }

        /// <summary>
        /// Inserta o actualiza un acuerdo de aplicación de fondos.
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<CoAplFndAcuerdosGuardarResponse> Co_AplFnd_Acuerdos_Guardar(int codEmpresa, CoAplFndAcuerdosDetalleResponse request)
        {
            var validationMessage = ValidarGuardarRequest(request);
            if (!string.IsNullOrWhiteSpace(validationMessage))
            {
                return DbHelper.CreateErrorResponse<CoAplFndAcuerdosGuardarResponse>(
                    validationMessage,
                    -1,
                    new CoAplFndAcuerdosGuardarResponse());
            }

            using var connection = DbHelper.OpenConnection(_portalDB, codEmpresa);

            try
            {
                var parameters = BuildGuardarParameters(request);

                var result = connection.QueryFirstOrDefault<CoAplFndAcuerdosGuardarResponse>(
                    SpGuardar,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                if (result == null)
                {
                    return DbHelper.CreateErrorResponse<CoAplFndAcuerdosGuardarResponse>(
                        "No fue posible procesar el acuerdo.",
                        -1,
                        new CoAplFndAcuerdosGuardarResponse());
                }

                if (result.Pass == 0)
                {
                    return DbHelper.CreateErrorResponse<CoAplFndAcuerdosGuardarResponse>(
                        result.Mensaje,
                        -1,
                        result);
                }
                RegistrarBitacora(codEmpresa, request.Usuario, result.Mensaje ?? string.Empty, result.Movimiento);
                return DbHelper.CreateOkResponse(result);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<CoAplFndAcuerdosGuardarResponse>(
                    "Error al guardar el acuerdo de aplicación de fondos.",
                    -1,
                    new CoAplFndAcuerdosGuardarResponse());
            }
        }

        /// <summary>
        /// Procesa la carga masiva de acuerdos de aplicación de fondos.
        /// El cliente debe enviar la información ya leída del Excel.
        /// </summary>
        public ErrorDto<CoAplFndAcuerdosCargaMasivaResponse> Co_AplFnd_Acuerdos_CargaMasiva(
            int codEmpresa,  CoAplFndAcuerdosCargaMasivaRequest request)
        {
            if (request == null)
            {
                return DbHelper.CreateErrorResponse<CoAplFndAcuerdosCargaMasivaResponse>(
                    "La solicitud es requerida.",
                    -1,
                    new CoAplFndAcuerdosCargaMasivaResponse());
            }
            if (request.Items == null )
            {
                return DbHelper.CreateErrorResponse<CoAplFndAcuerdosCargaMasivaResponse>(
                    "No se recibieron registros para procesar.",
                    -1,
                    new CoAplFndAcuerdosCargaMasivaResponse());
            }
            if ( request.Items.Count == 0)
            {
                return DbHelper.CreateErrorResponse<CoAplFndAcuerdosCargaMasivaResponse>(
                    "No se recibieron registros para procesar.",
                    -1,
                    new CoAplFndAcuerdosCargaMasivaResponse());
            }

            if (string.IsNullOrWhiteSpace(request.Usuario))
            {
                return DbHelper.CreateErrorResponse<CoAplFndAcuerdosCargaMasivaResponse>(
                    "El usuario es requerido.",
                    -1,
                    new CoAplFndAcuerdosCargaMasivaResponse());
            }

            using var connection = DbHelper.OpenConnection(_portalDB, codEmpresa);

            var response = new CoAplFndAcuerdosCargaMasivaResponse
            {
                Procesados = request.Items.Count
            };

            try
            {
                foreach (var item in request.Items)
                {
                    var resultado = new CoAplFndAcuerdosGuardarResponse
                    {
                        Cedula = item.Cedula?.Trim() ?? string.Empty,
                        Nombre = item.Nombre?.Trim() ?? string.Empty
                    };

                    var validationMessage = ValidarCargaItem(item);
                    if (!string.IsNullOrWhiteSpace(validationMessage))
                    {
                        resultado.Procesado = false;
                        resultado.Mensaje = validationMessage;
                        response.ConError++;
                        response.Detalle.Add(resultado);
                        continue;
                    }


                    var spRequest = MapCargaItemRequest(item, request.Usuario);
                    var parameters = BuildGuardarParameters(spRequest);

                    var dbResult = connection.QueryFirstOrDefault<CoAplFndAcuerdosGuardarResponse>(
                        SpGuardar,
                        parameters,
                        commandType: CommandType.StoredProcedure);

                    if (dbResult == null)
                    {
                        resultado.Procesado = false;
                        resultado.Mensaje = "No fue posible procesar el registro.";
                        resultado.AcuerdoId = 0;
                        resultado.Movimiento = string.Empty;

                        response.ConError++;
                        response.Detalle.Add(resultado);
                        continue;
                    }

                    if (dbResult.Pass != 1)
                    {
                        resultado.Procesado = false;
                        resultado.Mensaje = dbResult.Mensaje;
                        resultado.AcuerdoId = dbResult.AcuerdoId;
                        resultado.Movimiento = dbResult.Movimiento;

                        response.ConError++;
                        response.Detalle.Add(resultado);
                        continue;
                    }

                    resultado.Procesado = true;
                    resultado.Mensaje = dbResult?.Mensaje ?? string.Empty;
                    resultado.AcuerdoId = dbResult.AcuerdoId ;
                    resultado.Movimiento = dbResult?.Movimiento ?? string.Empty;
                    response.Correctos++;
                    response.Detalle.Add(resultado);

                    RegistrarBitacora(codEmpresa, request.Usuario, resultado.Mensaje ?? string.Empty, resultado.Movimiento);
                }

                return DbHelper.CreateOkResponse(response);
            }
            catch (Exception)
            {
                return DbHelper.CreateErrorResponse<CoAplFndAcuerdosCargaMasivaResponse>(
                    "Error al procesar la carga masiva de acuerdos.",
                    -1,
                    response);
            }
        }

        /// <summary>
        /// Obtiene socios para búsqueda (F4).
        /// </summary>
        public ErrorDto<List<CoAplFndAcuerdosSocioResult>> Co_AplFnd_Socios_Obtener(int codEmpresa)
        {
            const string query = @"select Cedula, CedulaR, nombre from socios order by nombre";
            return DbHelper.ExecuteListQuery<CoAplFndAcuerdosSocioResult>(_portalDB, codEmpresa, query);
        }
    }
}