using Dapper;
using Microsoft.Data.SqlClient;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo.Models.ProGrX_Personas;
using System.Data;

namespace Galileo.DataBaseTier.ProGrX_Personas
{
    public class FrmAfNoticaNoCotizantesDb
    {
        private readonly IConfiguration _config;

        public FrmAfNoticaNoCotizantesDb(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        /// <summary>
        /// Obtener lista de las instituciones
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_NoticaNoCotizantes_Instituciones_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                @"select cod_Institucion as item, Descripcion as descripcion
                  from Instituciones WHERE ACTIVA = 1");
        }

        /// <summary>
        /// Obtener lista de los rangos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> AF_NoticaNoCotizantes_Rangos_Obtener(int CodEmpresa)
        {
            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(
                CreatePortalDb(),
                CodEmpresa,
                @"select Linea_Id as item, Descripcion as descripcion 
                  From AFI_SOCIOS_SIN_APORTES_RANGOS Where Activo = 1 order by Dia_Desde");
        }

        /// <summary>
        /// Obtener lista de los asociados sin aportes
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Filtros"></param>
        /// <returns></returns>
        public ErrorDto<List<AfAsociadosSinAportesDto>> AF_NoticaNoCotizantes_Consulta_Obtener(int CodEmpresa, AfNoticaNoCotizantesFiltros Filtros)
        {
            if (Filtros is null)
            {
                return DbHelper.CreateErrorResponse("Los filtros de consulta son requeridos.", -2, new List<AfAsociadosSinAportesDto>());
            }

            if (Filtros.codInstitucion == 0)
            {
                Filtros.codInstitucion = null;
            }

            if (Filtros.informe != 1)
            {
                Filtros.rangoId = 0;
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
                connection.Query<AfAsociadosSinAportesDto>(
                    "spPAT_AsociadosSinAportes_Consulta",
                    new
                    {
                        Informe = Filtros.informe,
                        RangoId = Filtros.rangoId,
                        Institucion = Filtros.codInstitucion
                    },
                    commandType: CommandType.StoredProcedure,
                    commandTimeout: 60
                ).Select(x =>
                {
                    x.capitalizacion = x.capitalización;
                    return x;
                }).ToList());

            return result.Code == 0
                ? DbHelper.CreateOkResponse(result.Result ?? new List<AfAsociadosSinAportesDto>())
                : DbHelper.CreateErrorResponse(result.Description ?? "Error al consultar asociados sin aportes.", result.Code.GetValueOrDefault(-1), new List<AfAsociadosSinAportesDto>());
        }

        /// <summary>
        /// Actualizar estadística, actualiza fechas de pago válidas
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto AF_NoticaNoCotizantes_Estadistica_Actualizar(int CodEmpresa)
        {
            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                connection.Execute(
                    "spPAT_AsociadosSinAportes_RecalculaFechas",
                    commandType: CommandType.StoredProcedure);
                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al actualizar estadística de no cotizantes.", result.Code.GetValueOrDefault(-1));
        }

        /// <summary>
        /// Enviar notificaciones a los asociados seleccionados
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="Lista"></param>
        /// <param name="Aviso"></param>
        /// <param name="Usuario"></param>
        /// <returns></returns>
        public ErrorDto AF_NoticaNoCotizantes_Asociados_Notificar(int CodEmpresa, List<AfAsociadosSinAportesDto> Lista, int Aviso, string Usuario)
        {
            if (Lista is null)
            {
                return DbHelper.ErrorResponse("La lista de asociados es requerida.", -2);
            }

            var result = DbHelper.WithConn(CreatePortalDb(), CodEmpresa, connection =>
            {
                foreach (var item in Lista)
                {
                    connection.Execute(
                        "spPAT_AsociadosSinAportes_Notifica",
                        new
                        {
                            Cedula = item.cedula,
                            Aviso,
                            Usuario
                        },
                        commandType: CommandType.StoredProcedure);
                }

                return true;
            });

            return result.Code == 0
                ? DbHelper.OkResponse("Ok")
                : DbHelper.ErrorResponse(result.Description ?? "Error al notificar asociados sin aportes.", result.Code.GetValueOrDefault(-1));
        }

        private PortalDB CreatePortalDb() => new(_config);
    }
}
