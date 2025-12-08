using Dapper;
using Galileo.Models.ERROR;
using Galileo.Models;
using Galileo.Models.ProGrX_Activos_Fijos;

namespace Galileo.DataBaseTier.ProGrX_Activos_Fijos
{
    public class FrmActivosCambioTipoDb
    {
        private readonly PortalDB _portalDB;

        public FrmActivosCambioTipoDb(IConfiguration config)
        {
            _portalDB = new PortalDB(config);
        }
        /// <summary>
        /// Obtiene la lista de tipos de activos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Tipos_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<DropDownListaGenericaModel>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<DropDownListaGenericaModel>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select rtrim(tipo_activo) as 'item',rtrim(descripcion) as 'descripcion' FROM  Activos_tipo_activo order by tipo_activo";
                result.Result = connection.Query<DropDownListaGenericaModel>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Consulta los datos principales de un activo por su placa.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<ActivosPrincipalesData> Activos_DatosActivo_Consultar(int CodEmpresa, string placa)
        {
            var result = new ErrorDto<ActivosPrincipalesData>()
            {
                Code = 0,
                Description = "Ok",
                Result = new ActivosPrincipalesData()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select A.Num_Placa, A.Nombre, A.vida_util_en, A.vida_util, A.met_depreciacion, A.tipo_activo,T.descripcion as 'Tipo_Activo_Desc'
                                    from Activos_Principal A
                                    inner join Activos_tipo_activo T on A.tipo_activo = T.tipo_activo
                                    where A.num_placa =@placa";
                result.Result = connection.Query<ActivosPrincipalesData>(query, new { placa }).FirstOrDefault();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }

        /// <summary>
        /// Obtiene la lista de activos.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<ActivosData>> Activos_Obtener(int CodEmpresa)
        {
            var result = new ErrorDto<List<ActivosData>>()
            {
                Code = 0,
                Description = "Ok",
                Result = new List<ActivosData>()
            };
            try
            {
                using var connection = _portalDB.CreateConnection(CodEmpresa);
                var query = $@"select num_placa, Placa_Alterna, Nombre from Activos_Principal where estado = 'A'";
                result.Result = connection.Query<ActivosData>(query).ToList();
            }
            catch (Exception ex)
            {
                result.Code = -1;
                result.Description = ex.Message;
                result.Result = null;
            }
            return result;
        }
    }
}
