using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;
using Microsoft.Data.SqlClient;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXErConfiguracionDb
    {
        private readonly PortalDB _portalDB;

        public FrmCntXErConfiguracionDb(IConfiguration config): this(new PortalDB(config),new MSecurityMainDb(config))
        {
        }

        public FrmCntXErConfiguracionDb(PortalDB portalDB, MSecurityMainDb dbBitacora)
        {
            _portalDB = portalDB;

        }

        /// <summary>
        /// Cargar tipos de cuentas
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="tipo"></param>
        /// <returns></returns>
        public ErrorDto<List<CntxTipoCuentaERDto>> CargarTiposCuenta(int codEmpresa,int codContabilidad,string tipo)
        {
            var response = new ErrorDto<List<CntxTipoCuentaERDto>>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                var sql = @"
                SELECT 
                    RTRIM(tipo_cuenta) AS tipo_cuenta,
                    RTRIM(descripcion) AS descripcion,
                    ER
                FROM CntX_Tipos_Cuentas
                WHERE cod_contabilidad = @codContabilidad
            ";

                switch (tipo)
                {
                    case "OI":
                        sql += " AND clasificacion = 'I' AND (ER IS NULL OR ER = 'OI')";
                        break;

                    case "OG":
                        sql += " AND clasificacion = 'G' AND (ER IS NULL OR ER = 'OG')";
                        break;

                    case "VE":
                        sql += " AND clasificacion = 'I' AND (ER IS NULL OR ER = 'VE')";
                        break;

                    case "CV":
                        sql += " AND clasificacion = 'G' AND (ER IS NULL OR ER = 'CV')";
                        break;
                }

                sql += " ORDER BY tipo_cuenta";

                response.Result = cn.Query<CntxTipoCuentaERDto>(
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
        /// Guardar tipos de cuenta
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="codContabilidad"></param>
        /// <param name="usuario"></param>
        /// <param name="tipo"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public ErrorDto<bool> GuardarTiposCuenta(int codEmpresa,int codContabilidad,string usuario,string tipo,List<CntxTipoCuentaERDto> data)
        {
            var response = new ErrorDto<bool>();

            try
            {
                using var cn = new SqlConnection(
                    _portalDB.ObtenerDbConnStringEmpresa(codEmpresa));

                cn.Open();

                using var tx = cn.BeginTransaction();

                foreach (var item in data)
                {
                    var sql = @"
                    UPDATE CntX_Tipos_Cuentas
                    SET ER = @ER
                    WHERE cod_contabilidad = @codContabilidad
                    AND tipo_cuenta = @tipo_cuenta
                ";

                    cn.Execute(sql, new
                    {
                        codContabilidad,
                        tipo_cuenta = item.tipo_cuenta,
                        ER = item.er
                    }, tx);
                }

                tx.Commit();

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
