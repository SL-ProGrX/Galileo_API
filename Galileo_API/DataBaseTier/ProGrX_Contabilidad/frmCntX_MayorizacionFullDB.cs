using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXMayorizacionFullDb
    {
        private readonly PortalDB _portalDB;

        public FrmCntXMayorizacionFullDb(IConfiguration config)
            : this(new PortalDB(config))
        {
        }

        public FrmCntXMayorizacionFullDb(PortalDB portalDB)
        {
            _portalDB = portalDB;
        }

         /// <summary>
         /// Lista tipo de Asientos
         /// </summary>
         /// <param name="codEmpresa"></param>
         /// <param name="codContabilidad"></param>
         /// <returns></returns>
        public ErrorDto<List<CntxTipoAsientoDto>> CntX_TiposAsientos_Listar(int codEmpresa,int codContabilidad)
        {
            var response = new ErrorDto<List<CntxTipoAsientoDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
                    SELECT 
                        RTRIM(Tipo_Asiento) AS item,
                        RTRIM(descripcion) AS descripcion
                    FROM CntX_Tipos_Asientos
                    WHERE cod_contabilidad = @codContabilidad
                ";

                response.Result = cn.Query<CntxTipoAsientoDto>(
                    sql,
                    new { codContabilidad }
                ).ToList();
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }


        /// <summary>
        /// Procesa la mayorizacion
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto<bool> Procesar(int codEmpresa,int codContabilidad,CntxMayorizacionProcesarDto request)
        {
            var response = new ErrorDto<bool>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                string sp = request.tipo_filtro switch
                {
                    "PERIODO" => "spCntX_AsientosAplicacionLote_Todo",
                    "FECHAS" => "spCntX_AsientosAplicacionLote_Fechas",
                    "TIPO" => "spCntX_AsientosAplicacionLote_TipoAsiento",
                    "TIPO_FECHAS" => "spCntX_AsientosAplicacionLote_TipoAsientoFechas",
                    _ => throw new ArgumentException("Tipo filtro inválido", nameof(request))
                };

                var param = new
                {
                    codContabilidad,
                    request.anio,
                    request.mes,
                    tipo = request.tipo_aplicacion,
                    usuario = request.usuario,
                    tipoAsiento = request.tipo_asiento,
                    fechaInicio = request.fecha_inicio,
                    fechaFin = request.fecha_fin
                };

                cn.Execute(sp, param, commandType: System.Data.CommandType.StoredProcedure);

                response.Result = true;
            }
            catch (Exception ex)
            {
                response.Code = -1;
                response.Description = ex.Message;
            }

            return response;
        }
    }
}