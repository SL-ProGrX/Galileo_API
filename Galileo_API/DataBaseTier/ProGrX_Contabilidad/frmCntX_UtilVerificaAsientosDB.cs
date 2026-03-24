using Dapper;
using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmCntXUtilVerificaAsientosDb
    {
        private readonly PortalDB _portalDb;

        public FrmCntXUtilVerificaAsientosDb(IConfiguration config)
            : this(new PortalDB(config)) { }

        public FrmCntXUtilVerificaAsientosDb(PortalDB portalDb)
        {
            _portalDb = portalDb;
        }

        /// <summary>
        /// Verifica los asientos contables segun la opcion seleccionada
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto CntXAsientos_Verificar(int codEmpresa, CntXAsientosVerificarRequest request)
        {
            using var conn = DbHelper.OpenConnection(_portalDb, codEmpresa);

            try
            {
                string sql = (request.opcion == 3 || request.opcion == 6)
                    ? @"exec spCntX_Asientos_Revision_Integral
                @Contabilidad, @Anio, @Mes, @Usuario, @Paso, @Fix;"
                    : @"exec spCntX_Asientos_Revision_Integral
                @Contabilidad, @Anio, @Mes, @Usuario, @Paso;";

                var param = new
                {
                    Contabilidad = request.cod_contabilidad,
                    Anio = request.anio,
                    Mes = request.mes,
                    Usuario = request.usuario,
                    Paso = request.opcion,
                    Fix = request.check ? 1 : 0
                };

                if (request.opcion == 1 || request.opcion == 2)
                {
                    return DbHelper.ExecuteNonQuery(_portalDb, codEmpresa, sql, param);
                }

                var mensajes = conn.Query<string>(sql, param)
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToList();

                return new ErrorDto
                {
                    Code = 0,
                    Description = mensajes.Count > 0
                        ? string.Join(Environment.NewLine, mensajes)
                        : ""
                };
            }
            catch (Exception ex)
            {
                return new ErrorDto
                {
                    Code = -1,
                    Description = ex.Message
                };
            }
        }
    }
}
