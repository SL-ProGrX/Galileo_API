using Galileo.DataBaseTier;
using Galileo.Models.ERROR;
using Galileo_API.Models.ProGrX_Contabilidad;

namespace Galileo_API.DataBaseTier.ProGrX_Contabilidad
{
    public class FrmArfCierresDb
    {
        private readonly PortalDB _portalDb;
        private readonly MSecurityMainDb _mSecurityMain;
        private readonly int vModulo = 20;

        public FrmArfCierresDb(IConfiguration config)
            : this(
                  new PortalDB(config),
                  new MSecurityMainDb(config)
            )
        { }

        public FrmArfCierresDb(PortalDB portalDb, MSecurityMainDb mProGrxMain)
        {
            _portalDb = portalDb;
            _mSecurityMain = mProGrxMain;
        }

        /// <summary>
        /// Obtiene el corte actual 
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <returns></returns>
        public ErrorDto<ArfCierreData?> ARFCierres_CorteActual_Obtener(int codEmpresa)
        {
            const string sql = @"exec spARF_Cierre_Actual";

            return DbHelper.ExecuteSingleQuery<ArfCierreData?>(
                _portalDb,
                codEmpresa,
                sql,
                new ArfCierreData()
            );
        }

        /// <summary>
        /// Cierra el periodo seleccionado
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public ErrorDto ARFCierres_Cerrar(int codEmpresa, ArfCierreData request)
        {
            try
            {
                const string sql = @"exec spARF_Cierre @FechaCierre, @Usuario";

                var resp = DbHelper.ExecuteNonQuery(
                    _portalDb,
                    codEmpresa,
                    sql,
                    new
                    {
                        FechaCierre = request.corte.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                        Usuario = (request.usuario ?? string.Empty).Trim()
                    }
                );

                if (resp.Code < 0)
                    return resp;

                RegistrarBitacora(
                    codEmpresa,
                    (request.usuario ?? string.Empty).Trim(),
                    "Aplica",
                    $"Cierre de Arrendamientos: {request.corte:yyyy-MM-dd}"
                );

                return new ErrorDto
                {
                    Code = 0,
                    Description = "Mes Cerrado Satisfactoriamente..."
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

        /// <summary>
        /// Registra en bitacora
        /// </summary>
        /// <param name="codEmpresa"></param>
        /// <param name="usuario"></param>
        /// <param name="movimiento"></param>
        /// <param name="detalle"></param>
        private void RegistrarBitacora(int codEmpresa, string usuario, string movimiento, string detalle)
        {
            _mSecurityMain.Bitacora(new Galileo.Models.Security.BitacoraInsertarDto
            {
                EmpresaId = codEmpresa,
                Usuario = usuario,
                DetalleMovimiento = detalle,
                Movimiento = movimiento,
                Modulo = vModulo
            });
        }
    }
}
