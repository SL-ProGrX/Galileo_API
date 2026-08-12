using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_EstudioCrd;
using System.Data;

namespace Galileo_API.DataBaseTier.ProGrX_EstudioCrd
{
    public partial class FrmPreaEstudiov2DB
    {
        private readonly PortalDB _portalDb;

        public FrmPreaEstudiov2DB(IConfiguration config)
        {
            _portalDb = new PortalDB(config);
        }

        /// <summary>
        /// Carga la información completa de un expediente de estudio de crédito.
        /// Obtiene estado, encabezado, crédito, salarios y catálogos en una sola llamada.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2CargaResponse> Prea_frmPreaEstudiov2_Cargar(
            int codEmpresa,
            FrmPreaEstudiov2CargaRequest request)
        {
            var result = new ErrorDto<FrmPreaEstudiov2CargaResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2CargaResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var parameters = new DynamicParameters();
                parameters.Add("@Expediente", request.cod_preanalisis?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@Usuario", request.usuario?.Trim() ?? string.Empty, DbType.String);

                using var multi = connection.QueryMultiple(
                    "spCRDPreaPREANALISIS",
                    parameters,
                    commandType: CommandType.StoredProcedure
                );

                var estado = multi.ReadFirstOrDefault<FrmPreaEstudiov2EstadoDto>();
                var encabezado = multi.ReadFirstOrDefault<FrmPreaEstudiov2EncabezadoDto>();
                var credito = multi.ReadFirstOrDefault<FrmPreaEstudiov2CreditoDto>();
                var salarios = multi.ReadFirstOrDefault<FrmPreaEstudiov2SalariosDto>();

                result.Result = new FrmPreaEstudiov2CargaResponse
                {
                    estado = estado ?? new FrmPreaEstudiov2EstadoDto(),
                    encabezado = encabezado ?? new FrmPreaEstudiov2EncabezadoDto(),
                    credito = credito ?? new FrmPreaEstudiov2CreditoDto(),
                    salarios = salarios ?? new FrmPreaEstudiov2SalariosDto(),
                    catalogos = CargarCatalogos(connection)
                };
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2CargaResponse();
            }

            return result;
        }

        /// <summary>
        /// Carga los catálogos (combos) necesarios para el formulario.
        /// </summary>
        private static FrmPreaEstudiov2CatalogosResponse CargarCatalogos(System.Data.IDbConnection connection)
        {
            var catalogos = new FrmPreaEstudiov2CatalogosResponse();

            try
            {
                var expedientes = connection.Query<FrmPreaEstudiov2DropdownDto>(
                    "SELECT cod_preanalisis AS item, cod_preanalisis AS descripcion FROM CRD_PREA_PREANALISIS WHERE estado <> 'B' ORDER BY cod_preanalisis"
                ).ToList();
                catalogos.expedientes = expedientes;
            }
            catch
            {
                catalogos.expedientes = [];
            }

            try
            {
                var lineas = connection.Query<FrmPreaEstudiov2DropdownDto>(
                    "SELECT cod_linea AS item, descripcion FROM CRD_LINEA_CREDITO WHERE estado = 'A' ORDER BY descripcion"
                ).ToList();
                catalogos.lineas = lineas;
            }
            catch
            {
                catalogos.lineas = [];
            }

            try
            {
                var destinos = connection.Query<FrmPreaEstudiov2DropdownDto>(
                    "SELECT cod_destino AS item, descripcion FROM CRD_DESTINO_CREDITO WHERE estado = 'A' ORDER BY descripcion"
                ).ToList();
                catalogos.destinos = destinos;
            }
            catch
            {
                catalogos.destinos = [];
            }

            try
            {
                var garantias = connection.Query<FrmPreaEstudiov2DropdownDto>(
                    "SELECT cod_garantia AS item, descripcion FROM CRD_GARANTIA_CREDITO WHERE estado = 'A' ORDER BY descripcion"
                ).ToList();
                catalogos.garantias = garantias;
            }
            catch
            {
                catalogos.garantias = [];
            }

            try
            {
                var tipos_salario = connection.Query<FrmPreaEstudiov2DropdownDto>(
                    "SELECT cod_tipo_salario AS item, descripcion FROM CRD_TIPO_SALARIO WHERE estado = 'A' ORDER BY descripcion"
                ).ToList();
                catalogos.tipos_salario = tipos_salario;
            }
            catch
            {
                catalogos.tipos_salario = [];
            }

            try
            {
                var componentes = connection.Query<FrmPreaEstudiov2DropdownDto>(
                    "SELECT cod_componente AS item, descripcion FROM CRD_COMPONENTE_ADICIONAL WHERE estado = 'A' ORDER BY descripcion"
                ).ToList();
                catalogos.componentes_adicionales = componentes;
            }
            catch
            {
                catalogos.componentes_adicionales = [];
            }

            try
            {
                var comites = connection.Query<FrmPreaEstudiov2DropdownDto>(
                    "SELECT id_comite AS item, descripcion FROM CRD_COMITE_RESOLUTIVO WHERE estado = 'A' ORDER BY descripcion"
                ).ToList();
                catalogos.comites = comites;
            }
            catch
            {
                catalogos.comites = [];
            }

            try
            {
                var bancos = connection.Query<FrmPreaEstudiov2DropdownDto>(
                    "exec spCrd_SGT_Bancos_Desembolso @Usuario = ''"
                ).ToList();
                catalogos.bancos = bancos;
            }
            catch
            {
                catalogos.bancos = [];
            }

            return catalogos;
        }

        /// <summary>
        /// Obtiene la información del tab Hipotecario del Estudio de Crédito v2.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2HipotecarioResponse> Prea_frmPreaEstudiov2_Hipotecario_Obtener(
            int codEmpresa,
            FrmPreaEstudiov2HipotecarioRequest request)
        {
            var result = new ErrorDto<FrmPreaEstudiov2HipotecarioResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2HipotecarioResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                var parameters = new DynamicParameters();
                parameters.Add("@cod_preanalisis", request.cod_preanalisis?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@id_solicitud", request.id_solicitud, DbType.Int64);
                parameters.Add("@usuario", request.usuario?.Trim() ?? string.Empty, DbType.String);

                result.Result = connection.QueryFirstOrDefault<FrmPreaEstudiov2HipotecarioResponse>(
                    "spPrea_frmPreaEstudiov2_Hipotecario_Obtener",
                    parameters,
                    commandType: CommandType.StoredProcedure
                ) ?? new FrmPreaEstudiov2HipotecarioResponse();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = new FrmPreaEstudiov2HipotecarioResponse();
            }

            return result;
        }

        /// <summary>
        /// Cambia el expediente a estado abandonado.
        /// </summary>
        public ErrorDto<FrmPreaEstudiov2AbandonarResponse> Prea_frmPreaEstudiov2_Abandonar(
            int codEmpresa,
            FrmPreaEstudiov2AbandonarRequest request)
        {
            var response = new ErrorDto<FrmPreaEstudiov2AbandonarResponse>
            {
                Code = 0,
                Description = "Ok",
                Result = new FrmPreaEstudiov2AbandonarResponse()
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                const string sql = @"EXEC spCrdPreaCambiaEstadoPreanalisis @cod_preanalisis, @estado";

                connection.Execute(
                    sql,
                    new
                    {
                        cod_preanalisis = request.cod_preanalisis.Trim(),
                        estado = "B"
                    },
                    commandType: CommandType.Text
                );

                response.Result = new FrmPreaEstudiov2AbandonarResponse
                {
                    cod_preanalisis = request.cod_preanalisis.Trim()
                };

                return response;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
                response.Result = new FrmPreaEstudiov2AbandonarResponse();
                return response;
            }
        }

        /// <summary>
        /// Consulta las causas de seguimiento del expediente.
        /// Obtiene la lista de causas registradas para un tipo específico (Denegados/Pendientes).
        /// </summary>
        public ErrorDto<List<FrmPreaEstudiov2CausaDto>> Prea_frmPreaEstudiov2_Causas_Consultar(
            int codEmpresa,
            string cod_preanalisis,
            string tipo)
        {
            var result = new ErrorDto<List<FrmPreaEstudiov2CausaDto>>
            {
                Code = 0,
                Description = "Ok",
                Result = []
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);

                const string sql = @"
                    SELECT Pa.COD_CAUSAS AS id_causa, 
                           Cg.DESCRIPCION AS descripcion, 
                           Pa.OBSERVACIONES AS observaciones,
                           Pa.REGISTRO_FECHA AS fecha,
                           Pa.TIPO AS tipo
                    FROM CRD_PREA_GESTION Pa
                    INNER JOIN OPERACION_CAUSAS Cg 
                        ON Pa.COD_CAUSAS = Cg.COD_CAUSAS AND Pa.TIPO = Cg.TIPO
                    WHERE Pa.COD_PREANALISIS = @cod_preanalisis 
                      AND Pa.TIPO = @tipo
                    ORDER BY Pa.REGISTRO_FECHA";

                var parameters = new DynamicParameters();
                parameters.Add("@cod_preanalisis", cod_preanalisis.Trim(), DbType.String);
                parameters.Add("@tipo", tipo.Trim(), DbType.String);

                result.Result = connection.Query<FrmPreaEstudiov2CausaDto>(
                    sql,
                    parameters,
                    commandType: CommandType.Text
                ).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = [];
            }

            return result;
        }

        /// <summary>
        /// Guarda observaciones de una causa del expediente.
        /// Actualiza las observaciones de una causa específica en la gestión del preanálisis.
        /// </summary>
        public ErrorDto<string> Prea_frmPreaEstudiov2_Causas_Guardar(
            int codEmpresa,
            FrmPreaEstudiov2CausasGuardarRequest request)
        {
            var result = new ErrorDto<string>
            {
                Code = 0,
                Description = "Ok",
                Result = string.Empty
            };

            try
            {
                using var connection = _portalDb.CreateConnection(codEmpresa);
                connection.Open();

                const string sql = @"
                    UPDATE CRD_PREA_GESTION 
                    SET OBSERVACIONES = @observaciones,
                        REGISTRO_USUARIO = @usuario,
                        REGISTRO_FECHA = GETDATE()
                    WHERE COD_PREANALISIS = @cod_preanalisis 
                      AND COD_CAUSAS = @id_causa 
                      AND TIPO = @tipo";

                var parameters = new DynamicParameters();
                parameters.Add("@cod_preanalisis", request.cod_preanalisis.Trim(), DbType.String);
                parameters.Add("@id_causa", request.id_causa, DbType.Int32);
                parameters.Add("@tipo", request.tipo.Trim(), DbType.String);
                parameters.Add("@observaciones", request.observaciones?.Trim() ?? string.Empty, DbType.String);
                parameters.Add("@usuario", request.usuario.Trim(), DbType.String);

                connection.Execute(
                    sql,
                    parameters,
                    commandType: CommandType.Text
                );

                result.Result = "Observaciones guardadas correctamente.";
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = string.Empty;
            }

            return result;
        }
    }
}
