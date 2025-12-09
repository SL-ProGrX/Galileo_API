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
        /// Obtiene listas genéricas de tipos de activos
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<List<DropDownListaGenericaModel>> Activos_Tipos_Obtener(int CodEmpresa)
        {
            const string sql = @"
                SELECT RTRIM(tipo_activo) AS item,
                       RTRIM(descripcion) AS descripcion
                FROM   Activos_tipo_activo
                ORDER BY tipo_activo";

            return DbHelper.ExecuteListQuery<DropDownListaGenericaModel>(_portalDB, CodEmpresa, sql);
        }


        /// <summary>
        /// Consulta los datos principales de un activo por su placa.
        /// </summary>
        /// <param name="CodEmpresa"></param>
        /// <param name="placa"></param>
        /// <returns></returns>
        public ErrorDto<ActivosPrincipalesData?> Activos_DatosActivo_Consultar(int CodEmpresa, string placa)
        {
            const string sql = @"
                SELECT  A.Num_Placa,
                        A.Nombre,
                        A.vida_util_en,
                        A.vida_util,
                        A.met_depreciacion,
                        A.tipo_activo,
                        T.descripcion AS Tipo_Activo_Desc
                FROM    Activos_Principal A
                        INNER JOIN Activos_tipo_activo T
                            ON A.tipo_activo = T.tipo_activo
                WHERE   A.num_placa = @placa";

            return DbHelper.ExecuteSingleQuery(
                _portalDB,
                CodEmpresa,
                sql,
                new ActivosPrincipalesData(),
                new { placa });
        }


       /// <summary>
       /// Obtiene la lista de activos.
       /// </summary>
       /// <param name="CodEmpresa"></param>
       /// <returns></returns>
        public ErrorDto<List<ActivosData>> Activos_Obtener(int CodEmpresa)
        {
            const string sql = @"
                SELECT num_placa,
                       Placa_Alterna,
                       Nombre
                FROM   Activos_Principal
                WHERE  estado = 'A'";

            return DbHelper.ExecuteListQuery<ActivosData>(_portalDB, CodEmpresa, sql);
        }
    }
}